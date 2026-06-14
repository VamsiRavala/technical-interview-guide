> Reference document — Solution Architect interview track.

# Solution Architect Interview Preparation — 8-Week Daily Practice Plan

*Azure • React • Microservices • System Design • Design Patterns*

**Prepared for: Vamsi Raavala**

13+ years in .NET → Solution Architect track

2 hours / day • 6 days / week • 8 weeks

## How to Use This Plan

### The Solution Architect Bar

A Solution Architect is judged on five things: (1) translating business problems into systems, (2) making and defending technology trade-offs, (3) designing for non-functionals (scale, security, cost, reliability), (4) influencing across teams without owning them, and (5) delivering reference architectures that others can build from. Every day in this plan ladders up to one of those five.

### Strategy for 8 Weeks

- You have 13 years in .NET — strong foundation, but Solution Architect interviews lean heavily on Azure depth, distributed systems, and trade-off articulation. We spend ~50% of time there.
- React is a smaller portion of SA interviews (1 week), but you must defend frontend choices. We cover architect-level React, not deep hooks tutorials.
- Microservices and System Design get the most weight (3 weeks combined) because that's where SA interviews are won or lost.
- Last week is pure mock interviews. Treat them like the real thing — recorded, timed, ruthless self-review.

### Daily 2-Hour Block Template

- 0:00 – 0:10 Review yesterday's notes + flashcards from prior weeks.
- 0:10 – 1:10 Deep study: read the Master Training Doc section + one external source.
- 1:10 – 1:50 Hands-on: code, whiteboard a design, or write an ADR.
- 1:50 – 2:00 Journal: 1-paragraph summary + 3 flashcards + 1 architect insight.

### Weekly Cadence

- Mon-Fri: 5 daily topics.
- Saturday: Weekly lab — build or extend something runnable.
- Sunday: 45-minute mock interview, recorded. Self-grade, list 3 fixes.

### Tracking & Tools

- Journal: one Markdown file per week in Interview Prepare/Journal/Week-N.md.
- Flashcards: Anki or any spaced-repetition app. 3 cards/day = ~150 cards over 8 weeks.
- Diagrams: draw.io, Excalidraw, or pen + paper. Save in Interview Prepare/Diagrams/.
- Code: a single GitHub repo InterviewPrep with one folder per week.

### Honesty Rule

If at the end of any week you cannot teach that week's topic to a junior engineer in 10 minutes from memory, you don't know it. Repeat one day from that week before moving on. The plan is a guide, not a contract.

## 8-Week Roadmap at a Glance

| Week | Theme | Why It Matters for Solution Architect |
| --- | --- | --- |
| **1** | Architecture Foundations + Design Patterns Core | Build the architect mindset. Master SOLID, Clean/Hexagonal, ADR writing, and the GoF patterns most cited in design reviews. |
| **2** | Enterprise & Cloud Patterns + System Design Fundamentals | CQRS, Saga, Outbox, Strangler Fig + CAP, partitioning, replication, caching. The toolbox for every SA design discussion. |
| **3** | Microservices Architecture (Deep) | Bounded contexts, communication styles, API gateway, observability. Build a 3-service local lab. Your stated weak area — extra time. |
| **4** | Distributed Data, Messaging & Resilience | Sagas, idempotency, event sourcing, Service Bus, Polly. Where most SA interviews are won. |
| **5** | Azure Deep Dive Part 1: Compute, Data, Identity, Network | AKS, App Services, Functions, Azure SQL, Cosmos, Entra ID, Managed Identity, VNet, Private Endpoints. Closes your Azure gap. |
| **6** | Azure Deep Dive Part 2: Messaging, APIM, Observability, Cost | Service Bus, Event Grid, APIM, Front Door, Monitor, App Insights, Well-Architected Framework, FinOps. |
| **7** | React Architect View + Security & DevOps | Architect-grade React, OAuth2/OIDC, JWT, threat modeling, CI/CD, IaC. Closes React gap and rounds out cross-cutting topics. |
| **8** | Mock Interviews + Behavioral + Resume Stories | 5 system design mocks, 3 behavioral mocks, STAR story bank, salary negotiation, final readiness checklist. |

*Note: Weeks 3-6 are the highest-leverage. If you fall behind, sacrifice Week 1 (you partly know it) before sacrificing Weeks 3-6.*

## Week 1 — Architecture Foundations + Design Patterns Core

### Theme

Establish the architect mental model. Stop thinking like a developer; start thinking in trade-offs and non-functional requirements. Build a working vocabulary of the design patterns SAs cite most.

### Learning Outcomes

- Articulate the difference between developer, lead, and architect mindsets in your own words.
- Explain SOLID with examples drawn from your Kroger and Kraft Heinz codebases.
- Diagram Clean / Onion / Hexagonal architecture from memory and explain when each fits.
- Write a real Architecture Decision Record (ADR) for a past decision.
- Recall and apply the 8 most-asked GoF patterns: Singleton, Factory, Builder, Strategy, Decorator, Observer, Adapter, Repository.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | Architect mindset & roles | Master Doc §1.1. Watch Mark Richards 'Software Architecture Foundations' intro. Note 5 differences between Senior Eng and Solution Architect. | Journal: 'My architect role in one paragraph.' |
| **Tue** | SOLID — SRP, OCP, LSP | Master Doc §1.2. Take a real Kroger class and refactor for SRP. | Code: split a 200-line service via SRP. Push to repo. |
| **Wed** | SOLID — ISP, DIP + Clean Architecture | Master Doc §1.3 + §1.4. Why DI containers exist (DIP). Clean Arch layers. | Diagram: redraw Sales Promotion as Clean Architecture. |
| **Thu** | Hexagonal & Onion + ADR writing | Master Doc §1.5 + §1.6. Read 3 sample ADRs from GitHub adr/madr. | Write ADR-001: 'Why we chose AKS over App Services.' |
| **Fri** | GoF essentials: Singleton, Factory, Builder, Strategy | Master Doc §2.1-2.4. Code each in C#. | Code 4 patterns; write 1 anti-pattern (Singleton abuse). |
| **Sat** | GoF essentials: Decorator, Observer, Adapter, Repository | Master Doc §2.5-2.8. Decorator chain for logging+caching+retry. | Code: DecoratorChain\<T\> reusable wrapper. |
| **Sun** | Review + Mock | Mock: 'Walk me through your project's architecture and one trade-off.' Record yourself. | Self-grade. List 3 weakest answers. |

### Weekly Lab

- Repo: CleanArchPromotions on GitHub.
- Stack: .NET 8, EF Core, xUnit.
- Layers: WebApi → Application → Domain → Infrastructure.
- One feature: CRUD on Promotion entity. One integration test.
- README: architecture diagram, ADR-001, layer responsibility table.

### Mock Interview Questions

- Tell me about an architectural decision you made and the alternatives you rejected.
- Walk me through SOLID. Now tell me where you've seen each violated and what it cost.
- When would you NOT use Clean Architecture?

## Week 2 — Enterprise & Cloud Patterns + System Design Fundamentals

### Theme

Move past GoF into the patterns that matter for distributed systems on Azure. Then build the system design vocabulary every SA interview demands.

### Learning Outcomes

- Implement Repository + Unit of Work and explain when EF Core makes them redundant.
- Explain CQRS, Saga (orchestration vs choreography), Outbox, Strangler Fig with real examples.
- Articulate CAP and PACELC with concrete database examples.
- Choose a partitioning strategy and defend it under hot-key scenarios.
- Pick the right caching strategy for a given workload.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | Repository, UoW, CQRS, Mediator | Master Doc §3.1-3.2. MediatR in .NET. When CQRS is overkill. | Code: Promotion read model with MediatR query + command. |
| **Tue** | Saga + Outbox | Master Doc §3.3-3.4. Order saga across 3 services. Dual-write problem. | Diagram: choreography saga. Code: Outbox table + publisher. |
| **Wed** | Strangler Fig, ACL, BFF, Sidecar | Master Doc §3.5-3.8. The VB6 → .NET migration is a Strangler Fig. | Write: '5 cloud patterns I used at Kroger' (1 paragraph each). |
| **Thu** | CAP, PACELC, consistency models | Master Doc §4.1. Strong vs eventual vs causal. Real DB examples. | Whiteboard: explain CAP to a junior in 5 minutes. |
| **Fri** | Partitioning, sharding, replication, quorums | Master Doc §4.2-4.3. Hash vs range vs geo. Quorum math. | Sketch: shard 100M users; discuss rebalancing. |
| **Sat** | Caching, load balancing, queues | Master Doc §4.4-4.6. Cache-aside in .NET with Redis. L4 vs L7. | Code: Redis cache-aside with stale-while-revalidate. |
| **Sun** | Review + Mock | Mock: 'How would you migrate a monolith to microservices safely?' | Refine VB6 → React/.NET STAR story. |

### Weekly Lab

- Extend CleanArchPromotions: add Outbox table, background processor, MediatR commands/queries.
- Add a Redis cache-aside on the read side.
- ADR-002: 'Why we chose CQRS for the read path.'

### Mock Interview Questions

- You said you migrated a VB6 monolith. Walk me through the strangler fig approach and how you handled data consistency during the cutover.
- Design a system that needs strong consistency in one part and eventual in another. How do you partition responsibility?
- Your read traffic is 100x your write traffic. Walk me through how you'd architect for that.

## Week 3 — Microservices Architecture (Deep)

### Theme

Your stated weak spot. Build a defensible mental model and one full hands-on implementation. By Sunday you'll have 3 services running locally talking via REST and Service Bus.

### Learning Outcomes

- Explain when microservices are wrong (distributed monolith, Conway's Law, fallacies of distributed computing).
- Identify bounded contexts using DDD and draw a context map.
- Choose between sync (REST/gRPC) and async (messaging) for each call and defend it.
- Place an API Gateway, BFF, or Service Mesh correctly.
- Run 3 microservices locally with Docker Compose, including health checks and observability.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | When microservices, when not | Master Doc §5.1. Conway's Law, distributed monolith, 8 fallacies of distributed computing. | Write: '5 cases where I would NOT recommend microservices.' |
| **Tue** | Bounded Contexts & DDD basics | Master Doc §5.2. Aggregates, entities, value objects, ubiquitous language. | Map: 4 bounded contexts in Kroger Sales Promotion. |
| **Wed** | Sync vs async communication | Master Doc §5.3. REST vs gRPC vs messaging. Failure modes per style. | Decide: which 3 calls in your design should be async? |
| **Thu** | API Gateway, BFF, Service Mesh | Master Doc §5.4. APIM, Envoy basics, Istio concepts. | Diagram: APIM in front of 5 services with auth + rate limit + logging. |
| **Fri** | Service discovery, config, secrets, 12-factor | Master Doc §5.5-5.6. K8s service discovery, App Configuration, Key Vault. | Sketch: 12-factor compliance audit for one service. |
| **Sat** | Lab: 3 services on Docker Compose | Build PromotionsAPI, PricingAPI, NotificationAPI. REST sync + RabbitMQ. Health endpoints. | Repo: micro-promotions runs with docker compose up. |
| **Sun** | Review + Mock | Mock: 'Design a microservices architecture for e-commerce checkout.' | STAR refinement: your microservices migration story. |

### Weekly Lab

- Repo: micro-promotions.
- 3 services in .NET 8, each with own DB (SQLite for simplicity).
- PromotionsAPI publishes PromotionCreated to RabbitMQ; NotificationAPI consumes.
- PromotionsAPI calls PricingAPI sync via REST.
- Compose file with all 3 services + RabbitMQ + Seq for centralized logs.
- README with architecture diagram and ADR-003 explaining the sync/async split.

### Mock Interview Questions

- Walk me through a microservices design for e-commerce checkout. How do services communicate? How do you handle failure of the payment service?
- How do you decide where one service ends and another begins?
- Your monolith is 'working fine.' Why would you ever break it up?

## Week 4 — Distributed Data, Messaging & Resilience

### Theme

The hardest area in SA interviews. If you can speak fluently about idempotency, sagas, and Service Bus, you sound senior immediately. If you can't, no amount of Azure knowledge saves you.

### Learning Outcomes

- Explain why two-phase commit is avoided at scale and what replaces it.
- Implement an orchestration saga with compensating transactions in .NET.
- Design idempotent message handlers with dedup keys.
- Choose between Service Bus, Event Grid, Event Hubs, and Storage Queue with confident reasons.
- Apply retry, circuit breaker, timeout, and bulkhead patterns using Polly.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | Why no 2PC at scale + Saga orchestration | Master Doc §6.1-6.2. State machines. Compensating transactions. | Code: simple saga orchestrator in .NET. |
| **Tue** | Event sourcing & CQRS together | Master Doc §6.3. Event store, projections, replay, snapshots. | Diagram: ES+CQRS for an Order aggregate. |
| **Wed** | Idempotency & exactly-once myth | Master Doc §6.4. Effectively-once via dedup keys, idempotent receivers. | Code: idempotent handler with dedup table. |
| **Thu** | Azure Service Bus deep dive | Master Doc §6.5. Queues vs topics, sessions, dead-letter, peek-lock. | Sketch: ordered processing per customer using sessions. |
| **Fri** | Event Grid, Event Hubs, Kafka concepts | Master Doc §6.6. Use-case matrix. | Cheat sheet: Service Bus vs Event Grid vs Event Hubs vs Kafka. |
| **Sat** | Resilience: Retry, Circuit Breaker, Timeout, Bulkhead | Master Doc §6.7. Polly in .NET 8. | Code: HttpClient with Polly chain. |
| **Sun** | Review + Mock | Mock: 'Design a payment system that never double-charges.' | Flashcards: 10 messaging trade-off questions. |

### Weekly Lab

- Extend micro-promotions: replace RabbitMQ with Azurite + Service Bus emulator.
- Add idempotent message handler with dedup table.
- Add Polly chain (retry + circuit breaker) on cross-service HTTP calls.
- ADR-004: 'Why we picked Service Bus over Event Grid for this workflow.'

### Mock Interview Questions

- How do you guarantee a customer is never double-charged in a distributed payment workflow?
- A downstream service is failing intermittently. Walk me through your resilience strategy without making it worse.
- When would you choose Event Grid over Service Bus, and vice versa?

## Week 5 — Azure Deep Dive Part 1: Compute, Data, Identity, Network

### Theme

Direct closure of your Azure gap. Spend Saturday hands-on in the Azure portal — you cannot fake this in a real interview.

### Learning Outcomes

- Pick the right compute (VM, App Service, Functions, AKS, Container Apps) and defend it.
- Explain AKS internals: pods, deployments, services, ingress, HPA, node pools.
- Choose between Azure SQL, Managed Instance, Cosmos DB, and storage tables for a given dataset.
- Use Managed Identity end-to-end (zero connection strings).
- Design a hub-spoke network with private endpoints and NSGs.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | Azure compute landscape | Master Doc §7.1. VMs vs App Services vs Functions vs AKS vs Container Apps. | Build: a 1-page decision flowchart for picking compute. |
| **Tue** | AKS deep dive | Master Doc §7.2. Pods, deployments, services, ingress, HPA, node pools, networking modes. | Lab: deploy your micro-promotions to a kind cluster locally. |
| **Wed** | Azure SQL, Cosmos, Storage | Master Doc §7.3. SQL DB vs MI vs Cosmos vs Tables. Cosmos partition key choice. | Pick: best store for promotions catalog. Justify. |
| **Thu** | Identity: Entra ID, Managed Identity, Key Vault | Master Doc §7.4. Why Managed Identity beats connection strings. | Sketch: secure secret flow for an AKS-hosted .NET service. |
| **Fri** | Networking: VNet, Private Endpoints, NSG | Master Doc §7.5. Hub-spoke topology basics. Public vs Private DNS. | Diagram: a 3-tier app with private endpoints + hub-spoke. |
| **Sat** | Hands-on: deploy to Azure | Lab: deploy a .NET API to App Service via Azure CLI. Wire to Azure SQL with Managed Identity. | Working deployment + screenshot in journal. |
| **Sun** | Review + Mock | Mock: 'Design a secure Azure architecture for a fintech B2B API.' | Notes: 10 Azure-specific gotchas to memorize. |

### Weekly Lab

- Real Azure deployment: one .NET API on App Service, talking to Azure SQL via Managed Identity.
- Set up Key Vault for any non-DB secret.
- Document the steps in a README so you can reproduce under interview pressure.

### Mock Interview Questions

- Design a secure, scalable Azure architecture for a B2B API serving 10K req/sec. Use Managed Identity end-to-end.
- Walk me through your AKS production setup. What's in your node pools? How do you handle secrets?
- When would you NOT pick Cosmos DB?

## Week 6 — Azure Deep Dive Part 2: Messaging, APIM, Observability, Cost

### Learning Outcomes

- Build a Durable Function fan-out/fan-in workflow.
- Write APIM policies for JWT validation, rate limiting, and request transforms.
- Choose between Front Door, Application Gateway, and Traffic Manager.
- Write KQL queries for errors, slow requests, and dependency failures.
- Apply the Azure Well-Architected Framework's 5 pillars.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | Azure Functions & Durable Functions | Master Doc §8.1. Triggers, bindings, fan-out/in, cold starts. | Code: Durable Function with fan-out/fan-in pattern. |
| **Tue** | API Management (APIM) | Master Doc §8.2. Policies, products, subscriptions, versioning. | Write: APIM policy doing JWT validation + rate limit + transform. |
| **Wed** | Front Door, App Gateway, Traffic Manager | Master Doc §8.3. Global vs regional, L7 vs DNS-based. | Decide: which for active-active multi-region. |
| **Thu** | Azure Monitor, App Insights, KQL | Master Doc §8.4. KQL fundamentals. Distributed tracing across services. | Write 5 KQL queries (errors, slow, dep failures, custom). |
| **Fri** | WAF, DDoS, Defender for Cloud | Master Doc §8.5. Threat protection at the edge. | Audit: where would you place WAF rules in your architecture? |
| **Sat** | Well-Architected Framework + FinOps | Master Doc §8.6. The 5 pillars. Cost-optimization patterns. | Cost-optimize Week-5 architecture; calculate monthly TCO. |
| **Sun** | Review + Mock | Mock: 'Design a global, multi-region SaaS on Azure with \<100ms p99.' | Cheat sheet: Azure service-by-use-case (1 page). |

### Weekly Lab

- Add APIM in front of your Week-5 App Service. Write 1 real policy file.
- Wire App Insights into the API. Generate traffic, write 5 KQL queries to find issues.
- Run a Well-Architected self-assessment on your architecture.

### Mock Interview Questions

- Design a multi-region active-active SaaS on Azure targeting 99.99%. Walk me through compute, data replication, traffic routing, and observability.
- Your monthly Azure bill doubled. Walk me through your approach to find why and reduce it.
- How do you trace a single user request across 5 microservices?

## Week 7 — React Architect View + Security & DevOps

### Theme

The React you need for SA interviews is architectural, not syntactic. Plus tighten security, identity, CI/CD, and IaC — the cross-cutting topics that show up everywhere.

### Learning Outcomes

- Explain state management trade-offs (Context vs Redux Toolkit vs Zustand vs React Query) and pick correctly.
- Architect a large React frontend for 50 engineers (folder structure, state, build, deploy).
- Explain when SSR / Next.js / micro-frontends help and when they hurt.
- Diagram OAuth2 + OIDC for SPA + API + mobile combo.
- Build a CI/CD pipeline with environment gates.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | React architect view: state, performance, design systems | Master Doc §9.1-9.3. Hooks gotchas. Profiler. Atomic design + headless UI. | Decide: what state lives where for a promotions dashboard? |
| **Tue** | SSR / Next.js / Micro-frontends | Master Doc §9.4. When to choose each. Module Federation basics. | Diagram: micro-frontend split for an enterprise SaaS with 50 devs. |
| **Wed** | OAuth2 & OIDC flows | Master Doc §10.1. Auth Code + PKCE, Client Credentials, Device Code. | Diagram: auth flow for SPA + API + mobile combo. |
| **Thu** | JWT + threat modeling (STRIDE) | Master Doc §10.2-10.3. Signing vs encryption, key rotation. STRIDE walk. | STRIDE-model your micro-promotions architecture. |
| **Fri** | CI/CD: Azure DevOps + GitHub Actions | Master Doc §10.4. Pipelines, environments, gates, blue/green, canary. | Pipeline YAML: build → test → containerize → deploy to AKS staging. |
| **Sat** | IaC: Bicep + GitOps | Master Doc §10.5. Bicep modules, state, drift. GitOps with Flux/ArgoCD. | Write a Bicep file for AKS + ACR + Key Vault. |
| **Sun** | Review + Mock | Mock: 'Design a secure CI/CD that meets SOC2 controls.' | Flashcards: 10 security gotchas. |

### Weekly Lab

- Build a tiny React app (Vite + TS) that calls your Azure API with JWT auth.
- Create a GitHub Actions pipeline that builds, tests, and deploys it to Azure Static Web Apps.
- Write Bicep to provision the static web app + storage account.

### Mock Interview Questions

- You're hiring 50 engineers to work on a single React app. How do you architect it so they don't step on each other?
- A new compliance requirement says no production secret can ever sit on disk. Walk me through how you'd refactor CI/CD and runtime.
- Walk me through OAuth2 Authorization Code with PKCE and tell me what each step protects against.

## Week 8 — Mock Interviews + Behavioral + Resume Stories

### Theme

Polish. No new material. Translate everything you've built and learned into stories an interview panel will remember. Run real, recorded mocks. Do not skip recording — you will be shocked at what you find.

### Learning Outcomes

- Have 10 STAR stories ready, each tunable to 90 seconds or 4 minutes.
- Score \>= 4/5 on the System Design Rubric in 3 different design problems.
- Have a 60-90 second 'tell me about yourself' rehearsed and natural.
- Have salary range, leveling research, and 5 strong questions for interviewers ready.

### Daily Plan

| Day | Topic | Daily Focus (2 hrs) | Deliverable / Practice |
| --- | --- | --- | --- |
| **Mon** | STAR — Leadership (5 stories) | Mentoring, conflict, leading change, ownership of outage, influencing without authority. | Doc: 5 STAR stories in Master Doc Appendix A. |
| **Tue** | STAR — Technical (5 stories) | VB6 migration, microservices design, performance win, hard tech debate, architectural mistake. | Doc: 5 more STAR stories. |
| **Wed** | Resume polish + 2-min pitch | Read resume aloud. Tighten claims. Rehearse 'tell me about yourself.' | Updated resume v2 + 2-min pitch script. |
| **Thu** | System Design Mock 1 (60 min, recorded) | URL shortener OR Twitter timeline. Whiteboard. Speak out loud the whole time. | Recording + diagram + retro using rubric. |
| **Fri** | System Design Mock 2 (60 min, recorded) | Kroger-style promotions/pricing engine. Your home-turf scenario. | Recording + diagram + retro. |
| **Sat** | System Design Mock 3 + Behavioral Mock (75 min) | SD3: multi-tenant SaaS on Azure. Behavioral: 5 random questions from bank. | Recording + retro on both. |
| **Sun** | Final readiness + salary + questions | Levels.fyi research, negotiation script, 5 questions for interviewers, final checklist. | All assets organized in Interview Prepare folder. |

### System Design Self-Grading Rubric

After each mock, score yourself 1-5 on each:

- Requirements clarification (functional + non-functional).
- Capacity estimation (back-of-envelope numbers).
- API design (clear contracts, versioning, idempotency).
- Data model + storage choice with justification.
- Component diagram covering compute, data, cache, queue, gateway.
- Bottleneck analysis + scaling strategy.
- Failure modes, retries, fallbacks, observability.
- Security: AuthN/Z, secrets, network.
- Cost awareness.
- Trade-off articulation (alternatives considered, why rejected).

### Final Readiness Checklist

- I can explain SOLID with examples from MY codebase.
- I can name 10 design patterns and give a 1-sentence trigger for each.
- I can whiteboard CAP, partitioning, replication, and caching from memory.
- I can design a microservices architecture and defend service boundaries.
- I can pick the right Azure service for compute / data / messaging without hesitation.
- I have 10 STAR stories, each tunable to 90s or 4 minutes.
- I have a 60-second 'tell me about yourself' rehearsed.
- I have salary band + 3 leveling data points researched.
- I have 5 strong questions for interviewers.
- Portfolio: 2 GitHub repos (Clean Arch + micro-promotions) + 1 deployed Azure app.

## Appendix A — System Design Problem Bank

Rotate these from Week 4 onward. Each takes 60 minutes.

- Design a URL shortener (TinyURL).
- Design Twitter / Threads timeline.
- Design Uber / ride-hailing dispatch.
- Design Netflix-like video streaming.
- Design WhatsApp / chat with read receipts.
- Design Dropbox / file sync.
- Design a rate limiter as a service.
- Design a distributed cache (Redis-like).
- Design a payment gateway with idempotency guarantees.
- Design Kroger's promotions & pricing engine (your home turf).
- Design a real-time bidding (RTB) ad system.
- Design Google Docs collaborative editing.
- Design a notification service (email / SMS / push).
- Design a multi-tenant SaaS billing system.
- Design a global feature flag service.
- Design a leaderboard for a mobile game.
- Design a typeahead / autocomplete service.
- Design a log aggregation pipeline.
- Design an e-commerce checkout (cart → payment → order).
- Design an inventory system with eventual consistency.

## Appendix B — Behavioral Question Bank

- Tell me about yourself (60-90 seconds).
- Walk me through your most impactful project.
- Describe a time you disagreed with a senior engineer or architect.
- Tell me about a production incident you owned.
- Describe a time you said no to a stakeholder.
- Tell me about mentoring a junior who was struggling.
- Describe a time you had to learn a new technology fast.
- Tell me about a technical debt decision you made.
- Describe a time you missed a deadline.
- Tell me about a time you influenced without authority.
- Walk me through an architectural decision you regret.
- Tell me about a time you simplified a complex system.
- Describe how you handle ambiguous requirements.
- Tell me about a time you delivered bad news to leadership.
- Walk me through a code review you led that changed direction.
- Tell me about a time you owned an outage from start to root cause.
- Describe a time you had to choose between fast and right.
- Tell me about a time a stakeholder pushed an unrealistic timeline.

## Appendix C — Daily Journal Template

Copy this into Interview Prepare/Journal/Week-N.md and fill in each day.

```text
# Week N — Day M (YYYY-MM-DD)

**Topic:** ...

**What I learned (3 bullets):**

**What I built / practiced:**

**One architect-level insight:**

**3 flashcards (Q --> A):**

**What confused me / open questions:**

**Self-rating on today's topic (1-5):**
```

## Appendix D — STAR Story Template

Use this for every story. Aim for one 90-second version and one 4-minute version of each.

- Situation (15s): Where, when, what was at stake. One sentence on context.
- Task (10s): Your specific responsibility — not the team's, yours.
- Action (45-60s): What YOU did. Decisions, trade-offs, alternatives rejected. Use 'I' not 'we' for architect-level moves.
- Result (15s): Quantified outcome. Latency dropped X%, deploys went from N to M.
- Reflection (10s, optional but powerful): What you'd do differently now.

## Appendix E — Resources to Actually Read/Watch

### Books (skim chapters as topics arise)

- Designing Data-Intensive Applications — Martin Kleppmann (the SA bible).
- Fundamentals of Software Architecture — Mark Richards & Neal Ford.
- Building Microservices — Sam Newman.
- Domain-Driven Design Distilled — Vaughn Vernon.
- Cloud Native Patterns — Cornelia Davis.

### Free Online

- Microsoft Learn — AZ-305 Solutions Architect path (free, official).
- Azure Architecture Center — official reference architectures.
- System Design Primer (donnemartin/system-design-primer on GitHub).
- Refactoring.guru — design patterns explained visually.
- ByteByteGo YouTube — system design walkthroughs.

### Practice

- Pramp / Interviewing.io — free peer mocks.
- LeetCode — light coding practice (mediums only, 30 min/day in Weeks 6-8 if time permits).
