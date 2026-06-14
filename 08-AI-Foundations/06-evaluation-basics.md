# Evaluation Basics

Evaluation is the skill that separates "I built a demo" from "I shipped AI to production." Because LLMs are **nondeterministic**, you cannot unit-test them to green — you test them the way you test any probabilistic system: with datasets, metrics, thresholds, and continuous monitoring. In 2026, hiring managers screen *hardest* on eval, because it's where AI projects fail. This page covers offline vs online eval, LLM-as-judge, groundedness, golden sets, regression testing, and A/B.

---

## The Mental Shift

> An LLM is an unreliable, nondeterministic dependency you must **constrain and evaluate**, not unit-test to green.

Practical consequences:
- Run **multiple samples** and look at **aggregate pass rates**, not single runs.
- Assert on **semantic equivalence** or **rubric scores**, not exact strings.
- **Pin model versions** so your changes — not silent provider drift — explain score moves.

---

## Offline vs Online Evaluation

You need both. They answer different questions.

| | **Offline** | **Online** |
|---|---|---|
| Runs against | Curated, labeled dataset | Real production traffic |
| Speed / cost | Fast, cheap, repeatable | Slower, real users |
| Answers | "Did I break anything? Which option scores higher?" | "Does it actually work for users?" |
| Use | Regression gate in CI; compare variants | A/B tests, live metrics, sampled review |

```text
change → OFFLINE eval (gate in CI) → deploy → ONLINE eval (A/B + live metrics + sampled judge)
                          ▲                                              │
                          └────── feed interesting prod cases back ──────┘
```

Mature teams continuously feed interesting production cases back into the offline set.

---

## The Golden Dataset

A **golden dataset** is a curated, **versioned** set of representative inputs paired with expected outputs or rubric criteria — your stable benchmark for every change.

Build it from:
- Real user queries
- Domain-expert input
- **Edge cases** — and **every production bug becomes a new test case**

Maintain it actively:
- Keep it **balanced** across scenarios.
- **Refresh** it as usage shifts.
- **Version it alongside code.**
- Keep it large enough to be representative, small enough to run quickly in CI.
- **Separate it from training/fine-tune data** to avoid leakage.

> Without a golden set, prompt/model changes are blind. Curating it is ongoing, high-value engineering work — not a one-time task.

---

## Choosing Metrics by Task Type

### Classification / extraction (objective ground truth)

Treat it like any ML classifier — use **hard metrics**, not LLM-as-judge.

| Metric | What it tells you |
|---|---|
| Precision / Recall / F1 | Per-class correctness; prefer per-class F1 + macro/weighted for imbalanced classes |
| Confusion matrix | Which labels get confused |
| Field-level precision/recall (extraction) | Did it pull the right entities with correct values? |
| Schema-validity / format-failure rate | Fraction of outputs that parse |

Target thresholds tied to the **business cost of errors**.

### Open-ended generation / summarization (no single right answer)

```text
Reference metrics (ROUGE/BLEU/BERTScore)  → weak proxies, increasingly deprecated for LLMs
Rubric-based LLM-as-judge                  → score faithfulness, coverage, coherence, relevance
Pairwise preference (A vs B)               → humans & judges are better at relative than absolute
```

For **summaries** specifically, **faithfulness/groundedness** against the source is critical. The key shift vs classic NLP: **rubric-driven LLM judging + targeted human review beats n-gram overlap.**

---

## LLM-as-Judge

Use a strong model to score outputs against a **rubric** (e.g., relevance/groundedness/helpfulness on 1–5, or a pairwise A/B preference). It scales evaluation far beyond manual review and correlates reasonably with humans **when well-designed**.

**Pitfalls and mitigations:**

| Pitfall | Mitigation |
|---|---|
| **Position bias** (favors first option) | Randomize/swap positions in pairwise tests |
| **Verbosity bias** (prefers longer answers) | Rubric explicitly penalizes padding |
| **Self-preference** (favors its own family) | Use a different/strong judge model |
| **Inconsistency** | Require reasoning *before* the score; clear rubric with examples |
| **Treating scores as truth** | Calibrate against a human-labeled subset; audit periodically |

> Treat judge scores as a **useful signal, not ground truth.**

---

## Groundedness / Faithfulness

The **key anti-hallucination metric for RAG**: does each claim in the answer actually appear in the provided source context?

```text
answer → decompose into claims → check each claim against retrieved chunks → % supported
```

- A **low** score means the model is inventing or extrapolating beyond its sources.
- **Distinguish from answer relevance:** an answer can be grounded but irrelevant, or relevant but ungrounded.
- Track groundedness on **every** RAG change.

Azure AI Foundry provides a built-in **groundedness evaluator**; Azure AI Content Safety offers **real-time groundedness detection** to flag ungrounded responses in production before users see them. (RAG eval in depth — including the **RAG triad** of context relevance, groundedness, answer relevance, plus retrieval metrics like recall@k and NDCG — is section **12**.)

---

## Building a Regression Suite

```text
1. Assemble the golden set (common + edge + past failures)
2. Pick evaluators per case:
     - exact/semantic match  → deterministic outputs
     - schema validation     → structured output
     - LLM-as-judge + rubric → open-ended
3. Run in CI on every prompt/model/code change
4. Set score thresholds as GATES; alert on regression
5. Pin model versions; run multiple samples; compare aggregate pass rates
```

Azure AI Foundry's **evaluation SDK** runs this programmatically and plugs into pipelines (section **09**). The discipline mirrors integration testing — it's quality engineering for probabilistic systems.

---

## A/B Testing in Production

The gold standard for proving offline gains translate to real outcomes.

- **Primary metric** tied to user value: task completion, thumbs-up rate, conversion, deflection rate.
- **Guardrail metrics:** latency, **cost per request**, escalation rate.
- Randomly split traffic; **consistent assignment per user**.
- Run to **statistical significance**; watch for novelty effects and segment differences.

> For LLMs specifically, always watch **cost and latency deltas** — a quality win that doubles cost may not be worth it.

---

## Human Evaluation & Drift

**Human eval** is the ground truth for subjective quality and the **calibration anchor for LLM-as-judge.** Make it efficient: focus humans on a **sampled, stratified subset** (edge cases, low-confidence outputs, judge disagreements); use **pairwise comparisons**; provide clear rubrics; measure inter-annotator agreement. Embed lightweight in-product feedback (thumbs up/down + reason).

**Drift** has two sources: provider model updates and shifting user inputs. Defend with: pinned model versions (test new ones side-by-side), continuous quality monitoring (sampled judge scores, feedback, refusal/error rates, output-length/format anomalies), and periodic re-runs of the golden set against live config. **Versioning + monitoring + a fixed baseline** is the core toolkit.

---

## Brush-up Cheat-Sheet

```text
Mindset      = evaluate, don't unit-test; aggregate over samples; pin versions
Offline      = golden set, CI gate, compare variants (fast/cheap)
Online       = A/B + live metrics + sampled judge (real users)
Golden set   = versioned, balanced, grows with every prod bug, no training leakage
Metrics      = classification → P/R/F1; open-ended → rubric LLM-judge + pairwise
LLM-as-judge = scales eval; beware position/verbosity/self bias; calibrate to humans
Groundedness = % of claims supported by sources; THE RAG anti-hallucination metric
A/B          = primary value metric + cost/latency guardrails; significance + consistent assignment
Drift        = pin versions + monitor signals + re-run baseline
```

**Next:** [07-responsible-ai](07-responsible-ai.md) — keeping it safe, fair, and private.
