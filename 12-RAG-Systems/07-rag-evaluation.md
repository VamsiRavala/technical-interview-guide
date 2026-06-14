# RAG Evaluation

You cannot improve what you can't measure, and RAG tuning by "vibes" is the most common path to a brittle system. This file covers the two halves of evaluation — retrieval metrics (recall@k, MRR, nDCG) and generation metrics (groundedness, answer relevance) — the RAG triad and RAGAS framework, Azure AI Foundry's built-in evaluators, A/B comparison of configurations, and regression testing in CI.

---

## 🧭 The Core Principle: Evaluate the Two Halves Separately and End-to-End

RAG has two failure points, and conflating them hides bugs:

```text
  ┌──────────────┐        ┌──────────────┐
  │  RETRIEVAL   │  ───▶  │  GENERATION  │
  │ did we fetch │        │ did the model│
  │ the right    │        │ answer well  │
  │ evidence?    │        │ from it?     │
  └──────────────┘        └──────────────┘
   recall@k, MRR,          groundedness,
   precision@k, nDCG       answer relevance
```

> The crucial diagnostic insight: **"the LLM is wrong" is frequently a retrieval (recall) failure, not a generation one.** Measure retrieval first.

Everything below depends on a **golden dataset** — a curated, versioned set of representative queries, each mapped to the relevant chunk(s)/document(s) and/or a reference answer. Build it from real user queries, domain-expert input, and edge cases; **add every production failure as a new case**; version it alongside your code; keep it large enough to be representative but small enough to run in CI.

---

## 📊 Retrieval Metrics

These require a labeled set of `query → relevant document(s)` pairs.

| Metric | Definition | What it tells you |
|---|---|---|
| **Recall@k** | Fraction of queries for which a relevant doc appears in the top-k retrieved. | **The dominant RAG metric** — the LLM can only use what's retrieved. High recall@k is necessary. |
| **Precision@k** | Fraction of the top-k that are relevant. | Noise in the context. Reranking improves this. |
| **MRR (Mean Reciprocal Rank)** | Average of `1/rank` of the *first* relevant result. | Rewards putting a relevant item high — earlier chunks influence the LLM more. |
| **nDCG** | Normalized Discounted Cumulative Gain. | Accounts for **graded** relevance and position, not just binary hit/miss. |

**How to use them:** compute across chunk sizes, embedding models, and retrieval modes (vector vs hybrid vs hybrid+semantic) to choose configurations *empirically*. High `recall@k` is necessary; **reranking** then improves MRR/precision@k. Build this harness early — retrieval is the foundation.

A practical tuning loop: increase first-stage k until `recall@k` plateaus, then minimize the final k passed to the LLM while maintaining groundedness.

---

## 🎯 Generation Metrics and the RAG Triad

Beyond retrieval, evaluate the generated answer. Key dimensions:

| Dimension | Question it answers |
|---|---|
| **Groundedness / Faithfulness** | Are all claims supported by the retrieved context (no hallucination)? **The key anti-hallucination metric.** |
| **Answer relevance** | Does the answer actually address the question? |
| **Context relevance / precision** | Were the retrieved chunks relevant? |
| **Context recall** | Did retrieval capture all info needed for the ground-truth answer? |
| **Completeness / correctness** | vs. a reference answer. |

The **"RAG triad"** — the three you should always track — is **context relevance, groundedness, and answer relevance.** Note these are independent: an answer can be **grounded but irrelevant** (faithful to sources that don't address the question), or **relevant but ungrounded** (addresses the question but invents facts). You need both axes.

### Groundedness in detail

A groundedness evaluator **decomposes the answer into claims** and checks each against the retrieved chunks, scoring the fraction supported. A low score means the model is inventing or extrapolating beyond its sources. Track it on *every* RAG change. Azure also offers **real-time groundedness detection** via Content Safety to flag ungrounded responses in production before they reach users.

---

## 🧪 RAGAS and LLM-as-Judge

The **RAGAS** framework formalizes RAG generation metrics — **faithfulness, answer relevancy, context precision, context recall** — typically scored with an **LLM-as-judge** (a strong model scoring outputs against a rubric).

**LLM-as-judge pitfalls** (and mitigations):
- **Position bias** (favoring the first option) → randomize/swap order in pairwise tests.
- **Verbosity bias** (preferring longer answers) → explicit rubric criteria.
- **Self-preference** (favoring its own family's outputs) → use a different/strong judge model.
- **Inconsistency** → require the judge to give *reasoning before the score*; calibrate against a human-labeled subset.

Treat judge scores as a **useful signal, not ground truth** — validate the judge against human labels and audit periodically.

---

## ☁️ Azure AI Foundry Evaluation

**Azure AI Foundry evaluation** provides built-in evaluators you can run on datasets and wire into CI/CD:

- **RAG-specific:** groundedness, relevance, retrieval, plus context evaluators.
- **Quality:** coherence, fluency, similarity (to a reference).
- **Safety:** content safety / protected-material evaluators, plus red-teaming.

```python
# Sketch: run Foundry evaluators over a golden dataset
from azure.ai.evaluation import evaluate, GroundednessEvaluator, RelevanceEvaluator

results = evaluate(
    data="golden_set.jsonl",                  # query, context, response, ground_truth
    evaluators={
        "groundedness": GroundednessEvaluator(model_config),
        "relevance": RelevanceEvaluator(model_config),
    },
)
# Gate the build on aggregate thresholds; track scores over time.
```

Run these on a fixed eval set across changes to catch regressions when you tweak chunking, embeddings, prompts, or models. The Foundry SDK integrates into pipelines.

---

## ⚗️ A/B Testing RAG Configurations

Treat every RAG knob — chunk size/overlap, embedding model/dimensions, retrieval mode, top-k, reranking, prompt — as an **experiment evaluated on a fixed labeled dataset.**

**Offline (in CI):**
- For each configuration, run the eval harness computing retrieval metrics (recall@k, MRR, precision@k) and generation metrics (groundedness, relevance/correctness).
- **Change one variable at a time** to attribute effects.
- Report metric deltas with the dataset held constant.

**Online (production validation):**
- Split live traffic between configurations.
- Compare user signals (thumbs up/down, follow-up rate, deflection, task success) and operational metrics (latency, cost).

The discipline of an evaluation dataset + controlled, one-variable-at-a-time comparison is **what turns RAG tuning from guesswork into engineering.** Foundry's evaluation tooling supports running evaluators over datasets and comparing runs.

---

## 🔁 Regression Testing and Production Monitoring

### Regression suite in CI

- Assemble a **golden set** covering common cases, edge cases, and *past failures* (every production bug becomes a test).
- Define evaluators: exact/semantic match for deterministic outputs, schema validation for structured output, LLM-as-judge with rubrics for open-ended ones.
- Run on every prompt/model/code change; set **score thresholds as gates**; alert on regressions.
- **Pin model versions** so you isolate your changes from model drift.
- Because LLMs are nondeterministic, run multiple samples and look at **aggregate pass rates**, not single runs.

### Online groundedness as an SLO

In production you can't rely only on offline eval. Instrument the pipeline to log per request: the query, retrieved chunks (with scores), the final context, the answer, and citations. Then:

- Run an **online groundedness/faithfulness check** (Azure groundedness detection or LLM-as-judge) on a sample to estimate the hallucination rate, and **alert** when it rises.
- Capture **user feedback** (thumbs down, corrections) and flag low-confidence cases (low top retrieval scores) for review.
- **Feedback loop:** failures reveal missing/poor content to add or re-chunk, queries that need rewriting, or thresholds to tune.

Track **groundedness as a first-class production SLO** alongside latency and cost. The combination — continuous groundedness measurement + retrieval improvement + refusal logic — keeps hallucination bounded over time.

---

## 🎯 Target Metrics (Enterprise RAG Reference)

From the enterprise RAG reference architecture, reasonable production targets:

| Metric | Target |
|---|---|
| Retrieval recall@5 | > 0.9 on golden set |
| Groundedness | > 0.95 (answers supported by retrieved context) |
| Answer latency | < 3 s P95 (streaming first-token < 1 s) |
| Freshness | new/changed docs searchable within ~15 min |

---

## 🗝️ Key Takeaways

- Evaluate **retrieval and generation separately** — most "wrong answers" are retrieval (recall) failures. Build the harness early.
- **Retrieval:** recall@k (dominant), precision@k, MRR, nDCG — need labeled query→doc pairs.
- **Generation / RAG triad:** context relevance, **groundedness/faithfulness** (anti-hallucination), answer relevance. RAGAS formalizes these via LLM-as-judge.
- **LLM-as-judge** scales eval but has biases — require reasoning, randomize positions, use a strong independent judge, calibrate against humans.
- **Azure AI Foundry** ships built-in groundedness/relevance/retrieval/safety evaluators for CI; **pin model versions** and gate on thresholds.
- Compare configs **one variable at a time** on a fixed golden set; validate online with A/B tests on user + operational signals.
- Maintain a **golden dataset** (every production failure becomes a test) and treat **groundedness as a production SLO**.
