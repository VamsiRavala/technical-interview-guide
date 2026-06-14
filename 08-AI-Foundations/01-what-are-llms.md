# What Are LLMs?

A large language model (LLM) is a neural network trained to predict the next token of text. That is the whole job. Everything that feels like reasoning, knowledge, or creativity is an emergent property of doing next-token prediction extremely well at scale. As an engineer, you don't need the heavy math — you need a *usable* mental model so you can reason about cost, latency, quality, and failure modes. This page builds that model.

---

## The One-Sentence Model

> An LLM reads a sequence of tokens and outputs a probability distribution over the next token, one token at a time, appending each choice back to the input and repeating.

That autoregressive loop is why generation is **sequential**, why **output length dominates latency**, and why the model has **no memory beyond its context window**.

---

## Transformers & Attention (at a usable level)

Modern LLMs (GPT, Claude, Llama, Phi, Mistral) are **decoder-only transformers**: a tall stack of identical blocks. Each block has two main parts:

1. **Multi-head self-attention** — lets every token look at (attend to) every earlier token.
2. **A feed-forward network** — processes each position independently.

Each block also has residual connections and layer normalization for stable training. After the final block, the model projects the last position's hidden state into **logits** (one score per vocabulary token), a **softmax** turns those into probabilities, and a **sampler** picks the next token.

### Self-attention without the math

Each token produces three vectors:

```text
Query (Q) = "what am I looking for?"
Key   (K) = "what do I offer?"
Value (V) = "the content I carry"
```

Attention scores each token's Query against every other token's Key (a dot product), softmaxes those into weights, and returns a weighted sum of Values. Intuitively, the word "it" can dynamically pull meaning from the noun it refers to, regardless of distance. **Multi-head** just means doing this several times in parallel so different heads capture different relationships (syntax, coreference, position).

This is why transformers beat the older RNN/LSTM models: there is a direct, learnable path between any two tokens, so long-range dependencies are easy.

### Why attention is expensive

Self-attention compares every token to every other token, so it builds an **n × n** score matrix. Compute and memory grow **quadratically — O(n²)** — with sequence length.

```text
Double the context  →  ~4x the attention cost
```

Practical consequences for you:
- Long prompts cost more and add latency.
- You pay for the **full context on every call**.
- **Trimming irrelevant context is one of the highest-leverage cost optimizations available.**

Optimizations exist (FlashAttention reduces memory traffic; sliding-window/sparse attention limits how far each token looks), but the one you must know is the **KV cache** (below).

---

## Tokens — the Unit That Actually Matters

A **token** is a sub-word chunk produced by a tokenizer (typically byte-pair encoding). Common English words are often one token; rare words, code, GUIDs, and non-Latin scripts shatter into many.

```text
Rough rule (English):  1 token ≈ 4 characters ≈ 0.75 words
```

Why engineers must care: **billing, rate limits, and context windows are all measured in tokens**, not characters. A JSON blob or a wall of code can be far more token-dense than it looks.

| Text | Approx tokens |
|---|---|
| "The cat sat on the mat." | ~7 |
| A GUID `550e8400-e29b-41d4-...` | ~15+ |
| 1 page of prose (~500 words) | ~650 |
| Typical RAG chunk | ~200–500 |

```python
# Count before you send (OpenAI tokenizer)
import tiktoken
enc = tiktoken.encoding_for_model("gpt-4o")
print(len(enc.encode("The cat sat on the mat.")))  # -> 7
```

**Gotcha:** output tokens are usually billed at a *higher* rate than input tokens, and `max_tokens` caps the output but TPM quotas count *both*.

---

## The Context Window

The context window is the maximum number of tokens — **prompt + completion combined** — the model can handle in one call. Treat it as scarce, billed scratch space, not free space.

| Model class (illustrative) | Context window |
|---|---|
| GPT-4o-class | ~128K tokens |
| GPT-4.1-class / long-context | hundreds of K to ~1M |
| Older / small models | 4K–32K |

Things that bite people:
- **Exceeding it** throws an error.
- **Nearing it** degrades quality and raises cost/latency.
- **"Lost in the middle":** models attend most to the **start and end** of a long context and can overlook the middle. Put the most important instructions and most relevant retrieved chunks at the edges.

Management strategies (covered in [04-context-engineering](04-context-engineering.md)): trim/summarize history, retrieve only relevant chunks instead of whole documents, and always reserve budget for the completion.

---

## Decoding: How the Next Token Is Chosen

After logits → softmax, a sampler picks. The knobs you'll actually touch:

### Temperature

Scales the logits before softmax.

```text
temp 0.0–0.3  →  sharp distribution, picks likely tokens  →  factual, deterministic
temp 0.7–1.0  →  flatter distribution, more variety        →  creative, diverse
```

### Top-p (nucleus sampling)

Restricts choices to the smallest set of tokens whose cumulative probability exceeds `p` (e.g., 0.9), adapting the candidate pool dynamically.

> **Best practice: tune one, not both — usually temperature.** Setting both fights itself.

| Task | Suggested setting |
|---|---|
| Extraction, classification, SQL/code, RAG answers | temp 0–0.2 |
| Summarization | temp 0.2–0.5 |
| Brainstorming, marketing copy | temp 0.7–1.0 |

**Reality check:** even at temperature 0, outputs are *not* guaranteed bit-for-bit identical — floating-point and backend nondeterminism creep in. Use a `seed` where supported and assert on *semantic* equivalence in tests, not exact strings.

### Decoding algorithms (know the names)

| Method | Behavior | Use |
|---|---|---|
| **Greedy** | Always pick the top token | Fast, deterministic, can be repetitive |
| **Sampling** (temp/top-p) | Draw probabilistically | The default for chat/creative |
| **Beam search** | Keep top-k partial sequences | Common in translation, rarely in chat (bland, slow) |

For API work you rarely choose the algorithm explicitly — you tune temperature and top-p, which sit on the sampling spectrum.

---

## The KV Cache — Why Streaming Is Feasible

During generation, each new token must attend to all *prior* tokens' Keys and Values. Recomputing them every step would be brutal. The **KV cache** stores those K/V tensors so they're reused, turning per-token cost from quadratic to roughly **linear**.

```text
Without KV cache:  recompute all past K/V every token  →  O(n²) total
With KV cache:     reuse cached K/V                     →  O(n) generation
```

Implications:
- The cache grows with sequence length and eats GPU memory — a reason providers cap context and long chats get pricier.
- **Prompt caching** (Azure OpenAI / Anthropic) extends this idea across requests: a repeated, stable prompt *prefix* can be reused, cutting latency and input cost. This is a strong reason to keep large, stable system prompts at the **very start** of your messages.

---

## Model Families: Decoder-only vs Encoder-only vs Encoder-Decoder

| Architecture | Examples | Best at | You'll use it for |
|---|---|---|---|
| **Decoder-only** | GPT, Claude, Llama, Phi | Autoregressive generation, chat | Almost all generative/chat work |
| **Encoder-only** | BERT | Bidirectional understanding | Classification, NER, **embeddings** |
| **Encoder-decoder** | T5, original transformer | Seq-to-seq | Translation, some summarization |

Practical takeaway: use a **decoder-only** model for generation and a **separate embedding model** (encoder-style) for retrieval. Don't reach for a chat model to produce embeddings.

You'll also hear about **reasoning models** (o-series and similar) that do chain-of-thought *internally* before answering — great for hard multi-step problems, but slower and pricier, so don't use them for simple lookups.

---

## Why LLMs Hallucinate

LLMs predict the most *probable* continuation — not verified facts. There's no built-in database to check against, so when the training signal favored fluent, plausible text, the model produces confident output even when it doesn't know.

Hallucinations worsen with:
- Out-of-distribution or ambiguous queries
- Requests for specifics absent from training (dates, citations, IDs)
- High temperature

You **manage** it (ground with RAG, instruct "say I don't know," lower temperature, require + verify citations, validate programmatically) — you don't eliminate it. Hallucination is architectural; the realistic goal is an acceptably low, monitored rate. See [07-responsible-ai](07-responsible-ai.md).

---

## Fine-tuning, LoRA & PEFT (concept level)

**Fine-tuning** further trains a base model on your labeled examples to bake in a behavior, format, or tone. Use it to *enforce consistent behavior/format* or to *shrink long few-shot prompts* — **not** to inject fresh knowledge (that's RAG's job; fine-tunes go stale and can't cite).

**PEFT / LoRA:** Full fine-tuning updates all billions of parameters (costly). **LoRA** (Low-Rank Adaptation) freezes the base and learns tiny low-rank adapter matrices — often <1% of parameters — approaching full quality far more cheaply. **QLoRA** adds 4-bit quantization to fit training on one GPU. For an Azure-centric engineer this is mostly conceptual (you consume *managed* fine-tuning), but it explains why fine-tuning got cheap and modular.

---

## Brush-up Cheat-Sheet

```text
LLM            = next-token predictor, autoregressive, sequential
Attention      = Q·K → softmax → weighted V; O(n²) in context length
Token          = ~4 chars EN; the unit for billing/limits/context
Context window = prompt + completion budget; "lost in the middle" is real
Temperature    = low=factual, high=creative; tune ONE knob (usually temp)
KV cache       = reuse past K/V → linear generation; basis of streaming + prompt caching
Decoder-only   = generation; Encoder = embeddings/understanding
Hallucination  = inherent; ground + verify, don't expect zero
Fine-tune      = behavior/format, NOT knowledge; LoRA makes it cheap
```

**Next:** [02-embeddings-and-vectors](02-embeddings-and-vectors.md) — how meaning becomes math.
