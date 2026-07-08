# Senior .NET Lead/Architect — Behavioral Cheat-Sheet (1-Page)

> Glance-before-you-walk-in version. Each item = **headline** (say this first) + 3 anchor bullets. Full answers: [senior-dotnet-lead-behavioral-questions.md](senior-dotnet-lead-behavioral-questions.md). Insert your real numbers where you see `[#]`.

**1 · Evolution over 14+ years** — *"From writing all the code to designing systems and raising a team's bar — and from 'it runs' to reliable, secure, observable, affordable."*
- Framework/monoliths → **.NET Core/8, microservices, DDD, event-driven** on AKS.
- Manual deploys → **CI/CD, Docker, IaC**; non-functionals became design inputs.
- Now: **AI-assisted dev** (Copilot/Claude) + **building AI in** (ML.NET → Azure OpenAI/RAG/agents).

**2 · Design scalable/secure/high-perf apps** — *"Principles applied consistently, validated with data."*
- **Design:** DDD boundaries, Clean Architecture, REST + async messaging (Service Bus).
- **Scale/perf:** stateless + HPA, Redis cache, EF tuning/indexing, async all the way.
- **Secure:** OAuth2/JWT, managed identity, Key Vault, OWASP, least-privilege.

**3 · Most complex project (STAR)** — *"Kroger ESP→PPM: VB6/DB2 monolith → React + ASP.NET Core microservices on AKS."*
- **Strategy:** strangler-fig, incremental, parallel-run, zero-downtime cutover.
- **Challenges:** domain decomposition, DB2 data migration, no data loss.
- **Result:** [#] faster/safer releases, less manual effort, better maintainability.

**4 · Enforce quality (arch/SOLID/security/perf)** — *"Quality is a system, not heroics."*
- Clean Arch guardrails + SOLID vocabulary in every review.
- Patterns with intent (CQRS/Mediator, **Polly** resilience); secure coding + Key Vault.
- Quality gates: analyzers, SonarQube, critical-path tests; **profile before optimizing**.

**5 · Perf/memory/scale under load** — *"Measure → diagnose → fix → prevent. Never guess."*
- **Measure:** App Insights (p95/p99), dotnet-counters, memory dumps.
- **Culprits:** thread-pool starvation (`.Result`), N+1 EF, LOH, pool exhaustion, no cache.
- **Prevent:** async, pooling, caching, stateless+HPA, load tests + SLO alerts.

**6 · Balance trade-offs** — *"Explicit, risk-based trade-offs — made visible, not silent."*
- Security/data integrity = non-negotiable; cut scope before cutting those.
- **Tech-debt register**; reserve ~15–20%/sprint; spend rigor where business risk is.
- Communicate trade-offs in business terms; quality baked into Definition of Done.

**7 · Modern tools → outcomes** — *"Chosen for outcome, not novelty."*
- **AKS/Docker + CI/CD** → repeatable, [#] faster, safer releases.
- **Redis + async messaging** → lower latency, resilience under spikes.
- **App Insights** → faster MTTR; **Copilot/Claude** → more time on design.

**8 · Cross-functional delivery (STAR)** — *"Most big-project failures are misalignment, not code."*
- Architects (design reviews/boundaries), POs/BAs (turn rules into clear requirements).
- QA **shift-left**; DevOps co-built pipelines/AKS strategy.
- Regular **demos of working software** → high trust, few surprises.

**9 · KPIs & metrics** — *"A balanced scorecard that drives decisions, not decoration."*
- **DORA:** deploy frequency, lead time, change-failure rate, MTTR.
- Quality: critical-path coverage, complexity, SonarQube, defect-escape rate.
- Perf/reliability: p95/p99, error rate, **SLOs + error budgets**; security: SAST/DAST.

**10 · First 100 days / 1-year success** — *"Leave the team and system stronger, faster, more reliable than I found them."*
- **100 days:** understand (domain/code/pipeline/people) → quick wins + baselines → standards + roadmap.
- Emphasis shifts by role: Lead/Architect = direction & de-risking; EM = team health & predictability.
- **1 year:** lead time/CFR/MTTR down [#], fewer P1s, team growth, roadmap delivered, an AI-adoption story.

---
*Delivery: headline first, then support. STAR for #3/#5/#8; principle→example for #2/#4/#6/#9. Always land on a measurable/business outcome.*
