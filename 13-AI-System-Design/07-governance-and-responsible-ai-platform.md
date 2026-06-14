# Governance & Responsible-AI Platform

Regulated enterprises must prove **Responsible AI**: every model in use is approved, every risky output is checked, sensitive data isn't leaked, decisions are auditable, and policies are **enforced — not just documented**. This is the layer that lets Legal/Risk say yes. The architect's stance: a governance board that only reviews documents, without runtime enforcement, is theater. RAI succeeds only when the *safe path is the easy path*, baked into the platform SDK and gateway defaults.

## (a) Requirements

**Functional**
- **Policy enforcement** at the gateway (which tenant may use which model, for what data class).
- **Content safety** on inputs and outputs (harm categories, jailbreak/prompt-injection detection).
- **PII/DLP**: detect, classify, redact, and prevent exfiltration of sensitive data.
- **Model registry** with approval gates (no unapproved model reaches prod).
- **Audit trail** of every AI interaction for compliance/forensics.
- **Approval gates** in the deployment pipeline (prompt/model changes reviewed).

**Non-functional**

| NFR | Target |
|---|---|
| Policy coverage | 100% of AI calls pass through policy + safety checks |
| Audit completeness | Immutable record of every prompt/response + decision |
| PII leakage | Zero unredacted PII in logs or to unapproved models |
| Approval | No model/prompt to prod without registry + sign-off |

## (b) Architecture diagram

```text
  Request ─▶ AI GATEWAY (APIM)  ── policy decision point (PDP) ──────────────────┐
                │  1) POLICY check: tenant × model × data-class allowed?         │
                │  2) INPUT safety: Azure AI Content Safety (jailbreak/prompt-   │
                │     injection shield) + Purview PII detection/redaction        │
                ▼                                                                │
        Azure OpenAI (only APPROVED deployments)                                 │
                │                                                                │
                ▼                                                                │
                │  3) OUTPUT safety: Content Safety (harm categories,            │
                │     groundedness/protected-material) + DLP egress check        │
                ▼                                                                │
            Response ──▶ user (or blocked/redacted)                              │
                                                                                 ▼
  CONTROL PLANE                                            AUDIT (immutable)
  ┌───────────────────────────────┐        ┌────────────────────────────────────┐
  │ MODEL REGISTRY (Azure AI       │        │ Every call: who, tenant, model,     │
  │ Foundry / Azure ML registry):  │        │ data-class, safety verdict, cost,   │
  │  approved models, versions,    │        │ redactions ──▶ Log Analytics +      │
  │  RAI eval results, owners      │        │ immutable Blob (WORM) / Purview     │
  │  ──▶ APPROVAL GATES in pipeline│        └────────────────────────────────────┘
  │  (DevOps: RAI checklist, sign- │        Microsoft Purview: classification,
  │   off before prod deploy)      │        lineage, DLP, compliance reporting
  └───────────────────────────────┘
```

## (c) Component-by-component walkthrough

- **Gateway as Policy Decision/Enforcement Point**: APIM evaluates **policy** (is this tenant allowed to send this data class to this model?) before the call, and enforces input/output safety inline. Policy rules live in Cosmos DB / config as **policy-as-code**, versioned and reviewed.
- **Azure AI Content Safety** on **both** input and output: harm categories (hate, sexual, violence, self-harm), **Prompt Shields** (jailbreak/indirect prompt-injection detection), **groundedness detection**, and **protected-material** checks. Block, redact, or annotate per policy.
- **Microsoft Purview** for **data governance**: classify/label data, **DLP** to stop sensitive data egressing to models, **data lineage** (which documents fed which answer), and compliance reporting. Pairs with PII detection/redaction (Azure AI Language) before the prompt leaves your boundary.
- **Model registry = Azure AI Foundry / Azure ML model registry**: the system of record for **approved** models/versions, their **Responsible AI eval results**, and owners. CI/CD **approval gates** ensure no model or prompt reaches prod without an RAI checklist + human sign-off. Pin versions explicitly (never "latest"); track retirement dates so you migrate ahead of forced upgrades.
- **Immutable audit**: every interaction (who, tenant, model, data class, safety verdict, redactions, cost) to Log Analytics + **WORM immutable Blob storage** for forensics/regulators. For regulated decisions, enforce HITL and record the human's action so accountability is clear.
- **Responsible AI** wraps it all: an Impact Assessment per use case classifying risk and required controls, mapped to Microsoft's RAI Standard (fairness, reliability/safety, privacy/security, inclusiveness, transparency, accountability) and, where relevant, **EU AI Act** risk tiers. Bias and harm are *managed and monitored* down to a documented level, not eliminated — pretending otherwise is a red flag.

## (d) Scalability strategies

- **Policy/safety checks inline at the gateway** scale with the gateway (stateless, cached policy lookups).
- **Async deep-audit**: lightweight inline verdict + full audit record written asynchronously to avoid hot-path latency.
- **Cache safety verdicts** for identical content; tier audit storage (hot → WORM cold).
- **Tier controls by risk** so a low-risk internal summarizer doesn't carry the ceremony of a customer-facing decisioning system.

## (e) Security patterns

- **Policy-as-code** in source control, versioned and reviewed; the gateway is the single enforcement point (no bypass — block direct Azure OpenAI access via network + RBAC).
- **Defense in depth**: input safety → approved-model-only → output safety → DLP egress — multiple independent gates.
- **Separation of duties**: model approval (Risk/Legal) separate from deployment (Eng); enforced by pipeline gates.
- **Immutable, RBAC-protected audit**; PII redaction so the audit itself doesn't become a breach vector.

## (f) Trade-offs

| Decision | Option A | Option B | Guidance |
|---|---|---|---|
| **Inline (blocking) vs async safety** | Inline: unsafe content never reaches the user, adds latency | Async/audit-only: zero latency, content already delivered | Inline for harm/PII on output (must block); async for non-blocking analytics. Tune which checks are blocking by risk. |
| **Centralized governance vs team autonomy** | Central gateway enforcement: consistent, auditable, can bottleneck | Per-team self-governance: agile, inconsistent, risky | Centralize *enforcement*, decentralize *policy authoring* (teams propose, central approves). The whole platform exists to make governance non-optional. |
| **Strict guardrails vs usability** | Aggressive blocking: safe, more false positives, user friction | Permissive: smooth UX, higher risk of bad output | Calibrate thresholds with eval data; offer human-review/appeal for blocks. Over-blocking erodes trust as much as under-blocking, and can itself be a fairness issue if it disproportionately blocks certain dialects. |
| **Log depth vs privacy** | Comprehensive prompt/output logging: full audit, forensics | Minimal logging: private, harder to debug | Reconcile "log everything for audit" with "minimize personal data" via CMK, retention limits, RBAC, and redaction. Capture enough context (model version, full prompt, retrieved chunks) to *explain* an output even when you can't bit-for-bit reproduce it. |

> **Phrase that lands:** "Responsible AI is architecture, not a slide — Content Safety on every call, Purview DLP and PII redaction before the prompt leaves my boundary, an approved-model registry with RAI eval gates, and an immutable audit, all enforced at a gateway no team can bypass."
