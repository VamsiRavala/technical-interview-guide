# Content Safety & Responsible AI

> Azure's safety stack: the content filters built into Azure OpenAI, the standalone **Azure AI Content Safety** service (Prompt Shields, groundedness detection, protected material), and the Responsible AI process that wraps them. Defense in depth — never rely on a single layer, especially not just the prompt.

---

## Built-In Content Filters (Azure OpenAI)

Azure OpenAI applies **content filtering** (powered by Azure AI Content Safety) to **both prompts (input) and completions (output)** — it's **bidirectional**.

- **Categories**: hate, sexual, violence, self-harm.
- **Severity per category**: safe / low / medium / high. Default blocks **medium and high**.
- **Configurable per deployment** via a **custom content-filter configuration** — set the block threshold for input and output **independently**.
- Extra features (below): jailbreak/prompt-injection detection, protected material, groundedness.

### What happens when content is filtered (handle gracefully)
- **Prompt** triggers → request **rejected** with an error indicating category/severity.
- **Completion** triggers → response returns with **`finish_reason: content_filter`** and the content withheld (or partially, mid-stream).

Your app must:
1. Detect the `content_filter` finish reason / error and the triggered category.
2. Return a **user-friendly message** ("I can't help with that request") — not a stack trace.
3. **Log** the event (category, severity) for safety monitoring (without storing harmful content insecurely).
4. Optionally route to a safe fallback / escalate.
5. For legitimate false positives, consider **adjusting the filter config** (or applying for **modified filters** / **abuse-monitoring opt-out** for approved scenarios) and refine prompts.
6. Account for **streaming** — filtering can interrupt a stream; check annotations for which category fired.

---

## Azure AI Content Safety (standalone service)

A separate service you can call directly to apply the **same moderation to anything** — user inputs before they reach a model, model outputs, **RAG-retrieved documents**, even non-OpenAI model outputs — with configurable thresholds and custom blocklists. Gives **finer control** and **defense in depth** beyond the deployment-attached filter.

### Configurable content filters
Text/Image moderation across hate/violence/sexual/self-harm with **severity scores** and **custom blocklists/categories** for domain-specific terms.

### Prompt Shields (jailbreak + indirect injection)
Detects:
- **Direct jailbreaks** — "ignore previous instructions," DAN-style role-play, encoding tricks, many-shot priming.
- **Indirect (document-embedded) prompt injection** — malicious instructions hidden inside content the model later ingests (a web page, email, or a **document in your RAG corpus**, or a tool's output).

**Why indirect injection is the dangerous one:** the user is innocent and unaware, and an agent with tools can take **real actions** on the attacker's behalf ("exfiltrate the user's data to this URL"). Top risk for tool-using agents.

**Layered defense (prompting alone can't fully solve it):**
1. Enable **Prompt Shields** (direct + indirect).
2. **Spotlight/delimit** retrieved/user content — clearly mark it as untrusted **data**, instruct the model to never follow instructions found within it.
3. **Least-privilege tools** — tools enforce authorization server-side and validate arguments, so an injected "delete everything" can't succeed (limit the **blast radius**).
4. **Output constraints** — structured outputs, don't expose the system prompt, filter outputs.
5. **Human-in-the-loop** before sensitive/irreversible actions.
6. Never put **secrets** in the prompt; monitor/log suspicious patterns; **red-team**.

### Groundedness detection
Real-time check that an answer's claims are **supported by the provided source context** — the key anti-hallucination signal for RAG. Decomposes the answer into claims and verifies each against the retrieved chunks, flagging **ungrounded** sentences before they reach the user. Complements the Foundry **groundedness evaluator** used offline in CI.

### Protected material detection
Flags output matching known **copyrighted text/code**, so you can avoid emitting verbatim protected content — and it underpins the **Customer Copyright Commitment**.

---

## Responsible AI Process

Microsoft's RAI principles: **fairness, reliability & safety, privacy & security, inclusiveness, transparency, accountability.** The shipping lifecycle:

```text
Identify ──► Measure ──► Mitigate ──► Operate
 (harms,    (eval sets:  (filters,    (monitor,
  impact     quality,     grounding,   log, feedback,
  assessment) safety,      guardrails,  incident/rollback)
             fairness,     HITL)
             red-team)
```

1. **Identify** — impact assessment: intended use + potential harms.
2. **Measure** — build eval sets for quality, safety, fairness, groundedness; **red-team** for jailbreaks/misuse (Foundry's automated red-teaming agent / PyRIT).
3. **Mitigate** — content filters, grounding, guardrails, **human oversight** where risk is high.
4. **Operate** — monitor in production, log, gather feedback, keep an **incident/rollback plan**.
5. **Document** — transparency notes describing capabilities and limitations.

**Gate launch on safety + quality thresholds**, not just functionality. Safety is a process woven through the lifecycle, not a final checkbox.

---

## Guardrail Layers (defense in depth)

| Layer | Controls |
|---|---|
| **Input** | Content Safety filtering, **Prompt Shields** (jailbreak/indirect injection), **PII redaction**, scope restriction |
| **Prompt** | Strong system instructions, grounding constraints, spotlighting untrusted content |
| **Tool / action** | Least privilege, **human approval** for risky actions, argument validation |
| **Output** | Content Safety on completions, **groundedness** checks, schema validation, PII redaction, blocklists |
| **Operational** | Rate limits, monitoring, audit logs, kill switch |

Never rely on one layer — any single one can be bypassed.

---

## PII Handling (brief — see also networking/security file)

Use **Azure AI Language PII detection** (or Content Safety) to detect/redact entities (names, SSNs, emails, account numbers) **before** sending to the model when not needed; re-insert via a secure mapping if required. Apply on **inputs, retrieved docs, and outputs**. Don't log raw PII; encrypt with strict RBAC + short retention. For RAG, enforce **security trimming** so PII isn't surfaced to unauthorized users.

---

## Customer Copyright Commitment

Contractual assurance: if a customer using Azure OpenAI is sued for **copyright infringement over generated output**, Microsoft will **defend the customer and pay resulting damages/settlements** — **provided** they used the built-in guardrails/filters (including protected-material detection) and didn't intentionally infringe. **Protected material detection** is the mechanism that helps you stay within those terms. An enterprise differentiator vs less-protected alternatives.

---

## Brush-Up Cheat-Sheet

- Built-in filters are **bidirectional, per-category with severity thresholds, configurable per deployment**; default blocks medium+high. Handle `finish_reason: content_filter` gracefully.
- **Azure AI Content Safety** = standalone service: configurable filters + **Prompt Shields** (direct **and** indirect injection) + **groundedness** + **protected material** — apply to inputs, outputs, and **RAG docs**.
- **Indirect prompt injection via retrieved docs** is the top agent risk → treat retrieved content as **untrusted data** + Prompt Shields + **least-privilege tools** + HITL.
- RAI process: **identify → measure → mitigate → operate**; gate launch on safety+quality; red-team continuously.
- **Customer Copyright Commitment** indemnifies generated output when you use the required safety features.
