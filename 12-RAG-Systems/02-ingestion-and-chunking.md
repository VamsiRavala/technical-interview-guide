# Ingestion and Chunking

Chunking is where most RAG quality is won or lost. Before a single embedding is computed, the decisions you make about loading, parsing, splitting, and tagging documents determine the ceiling on every downstream stage. This file covers the indexing-side pipeline: loaders, parsing complex documents, chunk size and overlap, fixed vs semantic chunking, the parent-child pattern, metadata, and deduplication.

---

## 📥 Ingestion: Loaders and Parsing

Enterprise corpora are heterogeneous — PDFs, Office docs, HTML, SharePoint pages, Confluence, database rows, scanned images. The ingestion job is to pull each source, extract clean text, and preserve enough structure that chunking can produce *meaningful* units.

### The hard part: complex documents

Naive text extraction destroys meaning. Tables flatten into word salad, reading order breaks across columns, and images carry information that vanishes entirely. The Azure answer is **Azure AI Document Intelligence** (Layout / Read models):

- **Tables** are extracted as structured rows, not flattened text. Preserve them as Markdown or HTML inside the chunk so the model can actually read them.
- **Headings and reading order** are recovered, enabling structure-aware chunking.
- **Key-value pairs** (forms) are extracted.
- Output as **Markdown** so a structure-aware splitter can keep tables and sections intact.

```text
  Raw PDF ──▶ Doc Intelligence (Layout) ──▶ Markdown (headings, tables, order)
                                                    │
                                                    ▼
                                            structure-aware chunker
```

For **images/diagrams**: generate a text description via a vision model (gpt-4o) and embed that, OR use multi-modal embeddings, OR store the image and pass it to a vision-capable model at answer time. For **scanned documents**: OCR first. The principle is **extract faithfully, preserve structure, represent non-text content as something retrievable**.

### Ingestion as an async pipeline

Ingestion is bursty and batch; queries are interactive. **Decouple them.** A typical Azure pattern: source change → **Event Grid** → **Service Bus** queue (backpressure) → **Container Apps** worker that extracts, chunks, embeds, and pushes to the index — idempotent and retryable.

---

## ✂️ Chunking Strategies

Chunking splits documents into retrievable passages. The fundamental tension:

- **Too small** → precise retrieval and tight citations, but context is fragmented and you manage far more pieces.
- **Too large** → more context per chunk, but the single embedding vector summarizes too much (diluted relevance) and you waste prompt tokens.

| Strategy | How it works | Best for |
|---|---|---|
| **Fixed-size + overlap** | Split by token/char count (e.g., 512 tokens) with 10–20% overlap. | A robust default; uniform prose. |
| **Sentence / paragraph** | Split on natural language boundaries. | More coherent chunks than fixed-size. |
| **Recursive** | Try progressively finer separators (sections → paragraphs → sentences). | Keeping chunks semantically whole. |
| **Structure-aware** | Split on document layout (Markdown headings, HTML tags, PDF sections). | Dense technical docs; enables heading metadata. |
| **Semantic** | Group sentences by embedding similarity so each chunk is topically cohesive. | Mixed-topic documents where boundaries aren't structural. |

**Default to semantic / structure-aware chunking** (split on headings/sentences, ~300–800 tokens with 10–15% overlap), *not* naive fixed-size. Choose based on content: dense technical docs benefit from structure-aware/semantic chunking; uniform prose is fine with fixed-size + overlap.

---

## 📏 Chunk Size and Overlap

There is **no universal value** — it depends on content and the embedding/LLM. A common, effective default:

> **~300–600 tokens per chunk with 10–20% overlap** (e.g., 512 tokens, 50–100 token overlap).

| Choice | Effect |
|---|---|
| Small chunks (128–256 tokens) | More precise retrieval, better citation granularity, but fragment context and explode the chunk count. |
| Large chunks (800–1000+ tokens) | Preserve narrative context, but dilute the embedding and waste prompt tokens/cost. |

**Why overlap?** It repeats a slice of text (e.g., the last 100 tokens) at the start of the next chunk so a fact straddling a boundary isn't severed and incomplete in both chunks. The trade-off is duplication — more overlap means more stored vectors, higher index cost, and possible duplicate retrieval results. Typical overlap is 10–20% of chunk size.

**Tune empirically.** Build an eval set and measure `recall@k` and groundedness across chunk sizes. Microsoft's "integrated vectorization" defaults often land around 512 tokens with overlap. Also align chunk size to the embedding model's recommended input length so chunks aren't truncated.

---

## 🪆 The Parent-Child / Small-to-Big Pattern

This pattern resolves the precision-vs-context tension by **decoupling the chunk you retrieve on from the chunk you pass to the LLM**.

```text
  INDEX small chunks (precise, embed well)
        │ each stores parent_id
        ▼
  RETRIEVE via small chunks  ──▶  EXPAND to the parent section  ──▶  PASS parent to LLM
  (precision)                     (full context)
```

- Index **small** chunks (focused passages that match queries accurately).
- Link each to a **larger parent** (the surrounding section or document) via a `parent_id`.
- At query time, retrieve on the small chunks for precision, then expand to and pass the **parent** for fuller context.

**Variants:**
- **Sentence-window retrieval** — match on a sentence, return the surrounding sentences.
- **Auto-merging** — if many child chunks of the same parent are retrieved, merge them back into the parent.

The result: accurate matching *plus* sufficient grounding context, often improving both retrieval metrics and answer quality without simply enlarging every chunk.

---

## 🏷️ Metadata: The Difference Between a Toy and Production RAG

Store metadata that supports **filtering, citation, freshness, and security**. This is frequently what separates a demo from a dependable system, because it lets you constrain and contextualize retrieval rather than relying on embeddings alone.

| Field | Purpose |
|---|---|
| Source ID, file name, URL, page, section/heading | Citations and navigation |
| Title | Context for the model and the user |
| Timestamps (created / modified / ingested) | Freshness filtering and recency boosting |
| Author / owner | Attribution, filtering |
| Content type / category / tags | Filtered retrieval (e.g., restrict to "policy" docs) |
| Language | Multilingual filtering |
| **ACL / group IDs** | **Security trimming** (see `04`) |
| Version | Handling document revisions and deduplication |
| Parent document ID | Parent-child expansion, grouping chunks |

In Azure AI Search you declare these as fields with the right attributes — `filterable`, `sortable`, `retrievable`, `searchable`. **Design the index schema up front** with the filter fields you'll need; retrofitting metadata after ingestion is painful and usually means a full re-index.

---

## 🧹 Deduplication and Near-Duplicate Chunks

Duplicate or near-duplicate content (boilerplate, repeated headers/footers, multiple document versions) wastes context, biases retrieval, and crowds out diverse evidence.

**At indexing time:**
- Deduplicate exact/near-exact chunks (hash content, or detect high embedding similarity).
- Strip repetitive boilerplate (headers, footers, nav) before chunking.
- Use **stable keys** (e.g., `{docId}-{chunkIndex}`) so re-indexing a changed document *replaces* rather than duplicates its chunks, and remove old versions on update.

**At retrieval / post-processing time:**
- Apply **Maximal Marginal Relevance (MMR)** to balance relevance against novelty, so you don't pass five near-identical passages.
- Collapse chunks from the same parent.
- For versioned documents, store a `version` field and filter to the current version (or boost it via a scoring profile) so stale duplicates aren't retrieved.

The goal is a context window of **distinct, relevant** evidence: dedup at index time prevents bloat, MMR/diversity at query time ensures the few chunks you pass cover different facets rather than repeating one.

---

## 📄 Handling Very Large Documents

You can't pass a 200-page manual into the prompt. Chunk and retrieve, but with care:

- **Structure-aware chunking** so chunks are meaningful units; attach hierarchical metadata (document → section → chunk).
- **Parent-child** to get precise matching plus contextual passing.
- For **whole-document** questions ("does this contract contain X anywhere?", "summarize this report"), single-shot retrieval is insufficient — use **map-reduce summarization** (summarize chunks, then summarize the summaries), **iterative/agentic** passes, or **GraphRAG** (see `08`).
- Index **per-section** with good metadata so filtering can scope into the relevant parts of huge documents.

---

## 🗝️ Key Takeaways

- Chunking is the highest-leverage stage. Default to **semantic / structure-aware** chunking at ~300–800 tokens with 10–15% overlap, not naive fixed-size.
- Use **Azure AI Document Intelligence** for complex PDFs/tables/scans; output Markdown to preserve structure.
- **Overlap** prevents boundary loss; tune size and overlap on an eval set (recall@k, groundedness).
- The **parent-child** pattern gives retrieval precision *and* answer context simultaneously.
- **Rich metadata** (source, page, timestamps, ACL, version, parent_id) is what makes RAG filterable, citable, secure, and fresh — design the schema before you ingest.
- **Dedup** with stable keys at index time and MMR/version filtering at query time to keep context distinct.
