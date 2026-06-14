> Long-term senior/staff/principal interview reference (.NET, Azure, React, distributed systems).

# The Architect's Interview Reference

**Senior / Staff / Principal / Tech Lead**

*.NET, C#, Azure, React, SQL, Distributed Systems*

*A reference you keep forever, refresh before any senior interview.*

**GENERAL REFERENCE — Not Tied to Any Specific Interview**

## Section 1 — How to Use This Reference

This is your long-term technical interview reference, not a one-time prep guide. Use it three ways:

### Before Any Senior Interview (1-2 weeks ahead)

Skim full document. Identify weakest sections. Deep-read those. Practice the coding problems actively.

### The Day Before Any Interview (90 minutes)

Re-read the cornerstone sections:

- [Section 5 — Complete Request Lifecycle](#section-5--complete-request-lifecycle-the-most-asked-question) (most asked senior question)
- [Section 9 — Memory & GC](#section-9--memory--garbage-collection-architect-depth) (architect signal)
- [Section 11 — SQL Deep](#section-11--sql-deep-architect-depth) (every level tests this)
- [Section 14 — Behavioral STAR stories](#section-14--behavioral-preparation--star-stories)

### As a Working Reference

Bookmark sections relevant to your current work. When refactoring something, check the patterns section. When tuning performance, check memory/GC and SQL sections.

### Document Map

| Section | Use it for |
| ------------------------------ | ------------------------------------------------------------- |
| [1. How to Use](#section-1--how-to-use-this-reference) | Reading plan |
| [2. Reading the Role](#section-2--reading-the-role-youre-interviewing-for) | Senior vs Staff vs Principal vs Architect distinctions |
| [3. Interview Mechanics](#section-3--interview-mechanics-what-happens-in-each-phase) | How to handle every interview phase |
| [4. Tech Lead Mindset](#section-4--how-a-tech-lead--architect-thinks) | How architects think, phrase decisions, run incidents |
| [5. Complete Request Lifecycle](#section-5--complete-request-lifecycle-the-most-asked-question) | The most-asked senior question — 18 steps cold |
| [6. Top 100 Technical Q&A](#section-6--top-100-technical-qa-most-likely-asked) | C#, .NET, EF, SQL, Azure, microservices, React, DevOps |
| [7. Live Coding Questions](#section-7--live-coding-interview-questions) | Algorithms + code review bugs |
| [8. Design Patterns](#section-8--design-patterns-cheat-sheet) | 15 patterns with triggers and code |
| [9. Memory & GC (Architect)](#section-9--memory--garbage-collection-architect-depth) | Stack/heap, GC internals, allocation-free patterns |
| [10. Data Structures in C#](#section-10--data-structures-in-c-architect-depth) | When to use what, Big-O, internals |
| [11. SQL Deep (Architect)](#section-11--sql-deep-architect-depth) | Engine internals, indexes, locking, transactions |
| [12. CS Fundamentals](#section-12--cs-fundamentals-architect-depth) | Threading, networking, distributed systems, security, scaling |
| [13. System Design Framework](#section-13--system-design-framework) | How to attack any system design question |
| [14. Behavioral + STAR Stories](#section-14--behavioral-preparation--star-stories) | Universal stories that adapt to many questions |
| [15. Question-Answer Playbook](#section-15--question-answer-playbook) | Common questions categorized |
| [16. Closing Strong](#section-16--closing-the-interview-strong) | Final phrases, what to never say, recovery moves |

## Section 2 — Reading the Role You're Interviewing For

Senior tech titles vary by company. Same title means different things at Microsoft vs Google vs a startup. Before any interview, read the job description carefully and figure out which archetype the role wants.

### The Five Common Archetypes

#### Senior Engineer (L4-L5)

Highly hands-on. Owns features end-to-end. Mentors juniors informally. Reviews PRs.

- Interview emphasis: coding, design of single service, technical depth in one stack.
- What they want to see: clean code, testable design, can ship independently.
- What they don't expect: cross-team architecture, organizational influence.

#### Staff Engineer / Tech Lead (L6)

Senior IC level. Leads team's technical decisions but doesn't manage people. Hands-on still — writes code, reviews everything, owns design quality.

- Interview emphasis: design at service-cluster level, trade-off discussion, mentoring stories.
- What they want to see: judgment, prioritization, ability to say no.
- What they don't expect: full system architecture across the company.

#### Principal Engineer (L7-L8)

Cross-team or org-wide impact. Solves problems no single team can. Writes ADRs that affect many teams. Often less hands-on coding, more design + influence.

- Interview emphasis: cross-cutting concerns, organizational design, ambiguity navigation.
- What they want to see: think in 1-3 year horizons, scale thinking, build vs buy.
- What they don't expect: heavy day-to-day coding.

#### Architect (Solution / Enterprise)

Decisions affect technical direction broadly. Often customer-facing. Builds reference architectures. Less hands-on code, more decision-making.

- Interview emphasis: greenfield design, full-stack architecture, cloud platform expertise.
- What they want to see: pattern fluency, breadth, governance, frameworks.
- What they don't expect: writing production code daily.

#### Engineering Manager / Tech Lead Manager

People + technical. Mixed hands-on and management.

- Interview emphasis: leadership stories, 1:1s, hiring, performance conversations.
- What they want to see: people development, conflict resolution, business alignment.
- What they don't expect: live coding.

### How to Decode a Job Description

Look for these signals to figure out which archetype the role really is:

| Signal in JD | What it means |
| ------------------------------------------------- | ----------------------------------------- |
| 'You'll write code daily' | Senior or Staff IC — coding-heavy |
| 'You'll lead architectural decisions' | Staff/Principal — design emphasis |
| 'You'll mentor and grow other engineers' | Tech Lead or Staff — people skill matters |
| 'You'll set technical strategy across teams' | Principal — org-level influence |
| 'You'll work with customers / executives' | Architect — communication + business |
| 'You'll hire and grow the team' | Manager — people management |
| Heavy keyword list (Kafka, Redis, K8s, etc.) | Hands-on IC — they want practitioners |
| Light tech list, heavy on 'influence', 'strategy' | Staff+/Principal — judgment-driven |

### Align Your Examples to the Role

The same story can be told differently depending on what the role wants:

**For Senior role:** 'I implemented the migration. Here's what I built, the patterns I used, how I tested it.'

**For Staff role:** 'I led the technical design. I chose the Strangler Fig pattern after evaluating alternatives. I split work across 3 engineers and mentored them.'

**For Principal/Architect role:** 'I established the migration playbook that 5 teams across the org now use. The decisions affected our cloud cost model and skills hiring plan.'

Same project. Different framing. The interviewer is matching you to a level.

## Section 3 — Interview Mechanics: What Happens in Each Phase

Senior interviews have predictable phases. Knowing what each is testing helps you give the right answer.

### The Recruiter Screen

Goal of recruiter: confirm you're real, motivated, and within range on compensation.

What to do:

- Lead with the 90-second elevator pitch (Section 14).
- Be honest but precise about compensation expectations.
- Ask: 'What's the interview loop look like?' — get the sequence and topics.
- Ask: 'What's the team and role's biggest challenge right now?' — useful intel for later rounds.

### The Hiring Manager Screen

Goal: confirm fit with the role and team. Less technical, more behavioral.

What to do:

- Connect their needs (from JD + recruiter intel) to your background explicitly.
- Have 2-3 STAR stories ready for: leadership, technical decision, conflict.
- Ask: 'What does success look like in 90 days? In a year?'
- Ask about team composition, current focus, biggest gap they're hiring for.

### The Technical / Coding Round

Goal: confirm you can code at level. See Section 7 for problems.

Universal coding interview script:

**1. Clarify (1-2 min):** Input, output, edge cases, constraints, scale.

**2. Examples (1 min):** Walk through 1-2 manually. Confirms understanding.

**3. Approach (2-3 min):** Plain English plan. State complexity. Get agreement before coding.

**4. Code (10-15 min):** Talk through as you write. Clean, named variables, no shortcuts.

**5. Test (3-5 min):** Walk through with original example. Check edge cases.

> **Say it like this:** "Before I start writing code, can I confirm a few things about the input format and edge cases?" That sentence alone signals senior.

### The System Design Round

Goal: confirm you can think at scale and articulate trade-offs. See Section 13 for framework.

This round filters most strongly by level:

- Senior: design ONE service well.
- Staff: design a set of services that interact.
- Principal/Architect: design a system with cross-cutting concerns, multi-team ownership.

### The Behavioral / Leadership Round

Goal: confirm you can work with humans, handle pressure, grow others.

This round is often what makes or breaks Staff+ candidates. Technical chops alone aren't enough.

Universal preparation:

- Have 5-6 STAR stories that cover the common categories (Section 14).
- Each story should be 2-3 minutes when told.
- Quantify outcomes. 'We reduced p99 latency by 60%' beats 'we made it faster.'
- Include a reflection — what would you do differently?

### The Bar Raiser / Cross-Team Round

Some companies (Amazon, Meta, Microsoft) include an interviewer from outside the team. Their job: check that you're above the company's average bar.

How to handle: treat as a senior peer. Be confident but not arrogant. Show genuine intellectual engagement.

## Section 4 — How a Tech Lead / Architect Thinks

### 4.1 — Seven Mental Shifts From Senior Engineer to Tech Lead

| Senior thinks | Tech Lead / Architect thinks |
| ------------------- | ------------------------------------------------------------------------- |
| What tool do I use? | What's the trade-off and what business need does it serve? |
| Make it work | Make it work, fail safely, observe, recover. |
| Local optimization | System-wide optimization — cost, ops, security, performance, reliability. |
| Add features | Manage technical debt and complexity over time. |
| Write code | Decide what NOT to build. Buy vs build vs reuse. |
| My team | Multiple teams across the org. |
| Now | 5 years from now — how does this evolve? |

### 4.2 — Twelve Phrases That Land as Senior+

- 'The trade-off was X vs Y; we accepted Z because of [requirement]'
- 'We documented this in an ADR (Architecture Decision Record)'
- 'My approach is two-phase: immediate workaround, then permanent fix'
- 'I'd validate that with a small spike before committing'
- 'I time-box research at one day before deciding'
- 'I treat AI tools as force multipliers, not replacements'
- 'Functional vs non-functional requirements first'
- 'Effectively-once via at-least-once delivery + idempotent receivers'
- 'Strangler Fig migration with an Anti-Corruption Layer'
- 'Walking the Well-Architected pillars: cost, ops, reliability, performance, security'
- 'Reversible decisions get made fast; irreversible ones get more deliberation'
- 'I optimize for team throughput, not individual velocity'

### 4.3 — Things to NEVER Say

- 'Best practice' — without specifying for what context.
- 'Industry standard' — vague, defensive.
- 'Cutting-edge,' 'world-class,' 'enterprise-grade' — empty marketing language.
- 'I think we...' — uncertain about your own work. Either you did it or didn't.
- 'They wouldn't let me...' — passive, blames others.
- Faking knowledge of something you haven't done.
- 'It's the right way to do it' — instead of 'It was the right call given X.'

### 4.4 — Code Review as a Tech Lead

Code review is your highest-leverage activity. Use these principles:

- Be timely. PRs sitting >24 hours kill team velocity.
- Review WHY before HOW. Is this the right approach? Then check syntax.
- Distinguish blocking comments from suggestions. Use prefixes: 'BLOCKER:', 'NIT:', 'QUESTION:'.
- Praise good code, not just criticism. Public praise builds team.
- Don't bikeshed. Save energy for design, security, performance.
- Pair on hard reviews. 30-min call beats 20 review comments.
- Explain WHY, link to docs — don't just say 'do this.'

### 4.5 — Production Incident Handling — Two-Phase Approach

Memorize this sequence. The most common tech lead interview question.

**1. ACKNOWLEDGE:** In the channel: 'I'm on it.' Sets expectations.

**2. STOP THE BLEEDING:** Immediate workaround. Rollback, config change, circuit breaker, scale-up.

**3. COMMUNICATE:** Status updates every 15 minutes. Even if no progress: 'still investigating.'

**4. DIAGNOSE:** Only after stopping the bleed. App Insights, logs, traces.

**5. CONFIRM RESOLUTION:** Don't declare 'all clear' from one OK metric. Watch 15+ minutes.

**6. WRITE-UP:** Blameless RCA within 24-48 hours.

**7. PREVENT RECURRENCE:** Action items with owners and dates.

### 4.6 — RCA Pattern (5 Whys)

> Symptom: customer checkout returned 500 errors for 15 minutes.
>
> Why? Order service was timing out on database calls.
>
> Why? Database connection pool was exhausted.
>
> Why? Connections were being created per-request after refactor.
>
> Why? Refactor removed connection pool config; tests didn't catch it.
>
> Why? No connection-pool integration test.
>
> Root cause: missing test coverage for connection pool configuration.
>
> Fix: add the test. Add load test step to CI.
>
> Always ask: 'What alert would have caught this earlier?'

### 4.7 — Decision Frameworks That Signal Senior

#### Reversible vs Irreversible (Bezos's One-Way vs Two-Way Doors)

Most decisions are reversible (two-way door). Make them fast with less analysis. Some are hard to undo (one-way door) — invest more in those.

- Two-way: framework choice within a service, naming, code organization.
- One-way: data model in a shared database, API contracts published externally, cloud provider lock-in.

#### Build vs Buy vs Reuse

Default order: REUSE > BUY > BUILD.

Build only if:

- It's core differentiation for your business.
- Existing solutions don't meet specific functional or non-functional needs.
- Total Cost of Ownership of building (including maintenance forever) is genuinely lower.

Most senior engineers underestimate maintenance cost of self-built systems by 5-10x.

#### YAGNI

Don't build flexibility for needs you don't have. Real cost: complexity now for hypothetical gain later.

BUT: don't ship architecturally unsound code for needs you DO have (e.g., real multi-tenancy from day one).

Senior judgment: distinguish 'flexibility I'll need' from 'flexibility I'm imagining.'

## Section 5 — Complete Request Lifecycle (THE Most Asked Question)

When an interviewer asks 'walk me through what happens when a user clicks Submit,' this is your script. 18 steps. Memorize the sequence.

### Scenario

A user opens a React web app, types a search query, and clicks Search. Stack: React frontend + .NET API + Azure SQL + Service Bus + App Insights, behind Front Door and APIM.

### The 18-Step Journey

> STEP 1: Browser DNS resolution
>
> STEP 2: TLS handshake to Azure Front Door
>
> STEP 3: Front Door WAF inspection
>
> STEP 4: Front Door routes to APIM
>
> STEP 5: APIM JWT validation
>
> STEP 6: APIM rate limiting
>
> STEP 7: APIM forwards to App Service backend
>
> STEP 8: ASP.NET Core middleware pipeline
>
> STEP 9: Model binding and validation
>
> STEP 10: Controller invokes service layer
>
> STEP 11: Service queries Redis cache
>
> STEP 12: Cache miss -> EF Core to Azure SQL
>
> STEP 13: SQL execution with connection pool
>
> STEP 14: Result hydrated into DTOs
>
> STEP 15: Service publishes audit event to Service Bus
>
> STEP 16: Response serialized to JSON
>
> STEP 17: Response flows back through APIM + Front Door
>
> STEP 18: React receives, updates state, renders

### Network & Identity Layer (Steps 1-7)

#### Step 1-2: DNS + TLS

Browser resolves the app URL to Front Door anycast IP (closest edge). TLS 1.3 handshake. Modern apps enforce TLS 1.2+ minimum.

#### Step 3: Front Door WAF

Web Application Firewall inspects request:

- OWASP Core Rule Set (SQL injection, XSS, path traversal).
- Bot Manager rules.
- Custom rules: IP, geo, header patterns.
- DDoS protection at network layer.

#### Step 4: Front Door Routing

Routes /api/* to APIM, static content from CDN cache. Health probes every 30s remove unhealthy backends. Anycast routes to closest healthy region.

#### Step 5: APIM JWT Validation

```xml
<validate-jwt header-name="Authorization"
    failed-validation-httpcode="401">
    <openid-config url="https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration" />
    <audiences>
        <audience>api://my-api</audience>
    </audiences>
    <required-claims>
        <claim name="scp" match="any">
            <value>orders.read</value>
        </claim>
    </required-claims>
</validate-jwt>
```

#### Step 6: APIM Rate Limiting

```xml
<rate-limit-by-key calls="100" renewal-period="60"
    counter-key="@(context.User.Id ?? context.Request.IpAddress)" />
<quota-by-key calls="10000" renewal-period="86400"
    counter-key="@(context.Subscription?.Key)" />
```

#### Step 7: APIM Forwards to Backend

APIM uses Managed Identity to authenticate to App Service. Custom X-Correlation-ID added for distributed tracing.

### Application Layer (Steps 8-10)

#### Step 8: ASP.NET Core Middleware Pipeline

```csharp
var app = builder.Build();
app.UseExceptionHandler("/error");
app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLogging();
app.MapControllers();
app.Run();
```

#### Step 9: Model Binding + Validation

```csharp
public class SearchRequest
{
    [Required, StringLength(200)]
    public string Query { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
}

[HttpGet("search")]
public async Task<IActionResult> Search(
    [FromQuery] SearchRequest request,
    CancellationToken ct)
{
    var results = await service.SearchAsync(
        request.Query, request.Page, request.PageSize, ct);
    return Ok(results);
}
```

### Data Layer (Steps 11-14)

#### Step 11: Redis Cache Check

```csharp
var cacheKey = $"search:{query}:{page}:{pageSize}";
var cachedJson = await cache.GetStringAsync(cacheKey, ct);
if (cachedJson is not null)
{
    logger.LogInformation("Cache HIT for {Key}", cacheKey);
    return JsonSerializer.Deserialize<SearchResult>(cachedJson);
}
```

Cache hit returns in <5ms. No database query.

#### Step 12: Cache Miss → EF Core to SQL

```csharp
var query = db.Items
    .AsNoTracking()
    .Where(i => i.Name.Contains(searchTerm))
    .Where(i => i.Status == ItemStatus.Active)
    .OrderByDescending(i => i.CreatedAt);

var total = await query.CountAsync(ct);
var items = await query
    .Skip((page - 1) * pageSize).Take(pageSize)
    .Select(i => new ItemDto { Id = i.Id, Name = i.Name })
    .ToListAsync(ct);
```

#### Step 13: SQL Execution + Connection Pool

EF Core uses connection from pool. Managed Identity auth — no password in code.

#### Step 14: Hydration

Result rows materialize into DTO objects. Microseconds.

### Async + Response (Steps 15-18)

#### Step 15: Audit Event to Service Bus

```csharp
await cache.SetStringAsync(cacheKey,
    JsonSerializer.Serialize(result),
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    }, ct);

var sender = busClient.CreateSender("audit-events");
await sender.SendMessageAsync(new ServiceBusMessage(
    JsonSerializer.Serialize(auditEvent))
{
    MessageId = Guid.NewGuid().ToString(),
    ContentType = "application/json"
}, ct);
```

#### Step 16-17: Response Back

JSON serialized via System.Text.Json. HTTP 200 + camelCase. Response flows: App Service → APIM → Front Door (compression) → Browser. App Insights captures full trace.

#### Step 18: React Renders

```jsx
function SearchView() {
    const [query, setQuery] = useState('');
    const { data, isLoading } = useQuery({
        queryKey: ['search', query],
        queryFn: () => fetch(`/api/search?q=${query}`,
            { headers: { Authorization: `Bearer ${token}` } }
        ).then(r => r.json()),
        enabled: query.length > 0
    });
    if (isLoading) return <Spinner />;
    return <ResultList items={data?.items ?? []} />;
}
```

### Performance Budget

| Step | Cache HIT | Cache MISS |
| ---------------------- | ------------- | -------------- |
| DNS + TLS (first time) | 30ms | 30ms |
| DNS + TLS (cached) | 0ms | 0ms |
| Front Door + WAF | 5ms | 5ms |
| APIM JWT + rate limit | 8ms | 8ms |
| ASP.NET pipeline | 2ms | 2ms |
| Redis lookup | 3ms | 3ms |
| Azure SQL query | 0ms | 15ms |
| EF hydration + DTO | 0ms | 2ms |
| Service Bus publish | 0ms | 0ms (async) |
| JSON serialization | 1ms | 1ms |
| Response transit | 5ms | 5ms |
| React render | 10ms | 10ms |
| TOTAL (warm path) | ~35ms | ~50ms |

### Failure Modes & Mitigations

| Failure | Mitigation |
| ---------------------------- | -------------------------------------------------------- |
| DNS slow / failed | Multiple DNS providers, DNS over HTTPS |
| Front Door outage | Multi-region Front Door with health probes |
| WAF false positive | Detection mode in pre-prod, tune before prevention |
| JWT signing key rotation | JWKS endpoint cached, refresh on key change |
| App Service instance failure | Auto-scale + load balancer remove unhealthy |
| Connection pool exhaustion | Pool monitoring, alerts before limit |
| Azure SQL slow query | Query Store baseline, plan regression alerts |
| Redis cache outage | Cache wrapped in try/catch, fail open to DB |
| Service Bus down | Outbox pattern — event in DB, replayed when bus recovers |

### The Script — How to Tell This Story

> **Say it like this:** "Let me walk through what happens when a user clicks Search. Eighteen steps, but I'll group them. First, network and identity: DNS, TLS, Front Door with WAF, then APIM where we validate the JWT and enforce rate limiting. Second, into the application: ASP.NET middleware, model binding with validation, controller to service. Third, data: service checks Redis first — sub-5ms if hit. On miss, EF Core to SQL with parameterized queries via Managed Identity. Fourth, observability: service publishes an audit event asynchronously. Finally, response: JSON back through APIM, Front Door applies compression, React updates state. End-to-end on the warm path: around 35ms. The architectural choices that matter: caching at every meaningful layer, async I/O all the way down, observability built in, and security at every gate — not relying on any single layer."

## Section 6 — Top 100 Technical Q&A (Most Likely Asked)

Curated from your 540 Q&A reference. These are the questions most likely to come up. Organized by topic, answers tightened for interview delivery.

### 6.1 — C# Core (Q1-15)

**Q1. What's the difference between value types and reference types?**

**A.** Value types (struct, int, bool, DateTime): stored on stack or inline. Copied by value. Fast, no GC. Reference types (class, string, arrays): stored on heap, variables hold reference, GC-managed. Boxing converts value to reference type and allocates — avoid in hot paths.

**Q2. What's the difference between IEnumerable, IQueryable, and List?**

**A.** IEnumerable: in-memory iteration, LINQ to Objects, pull-based. IQueryable: query expression not yet executed, pushes filters to database. List: concrete in-memory collection with random access. Critical: AsEnumerable() too early forces ALL rows to load — kills performance.

**Q3. Explain async/await.**

**A.** Compiler rewrites async method into a state machine. await suspends without blocking the thread — thread returns to pool to handle other work. When awaited task completes, execution resumes. Enables high concurrency with few threads.

**Q4. What's the common async pitfall?**

**A.** Calling .Result or .Wait() on async. In ASP.NET classic with sync context, this deadlocks. In ASP.NET Core no deadlock but still blocks a thread. Rule: async all the way down.

**Q5. Task vs Thread?**

**A.** Task: lightweight, thousands at once, scheduled on ThreadPool. Thread: ~1MB stack each, dedicated OS thread. Use Task almost always. Thread only for: long-running work that can't be Task-based, or specific thread requirements.

**Q6. Task.Run vs Task.Factory.StartNew?**

**A.** Task.Run is the modern way. Use for CPU-bound work to offload from request thread. DON'T wrap async I/O in Task.Run — wastes threads. Only Task.Factory.StartNew if you need specific options it provides.

**Q7. What's ValueTask?**

**A.** Avoids allocation when async method completes synchronously (cache hit, fast path). Use when: method often completes sync, in hot paths where allocations matter, won't await same ValueTask multiple times.

**Q8. What's CancellationToken and why use it?**

**A.** Lets long-running async operations be cancelled cooperatively. ASP.NET Core provides one per HTTP request — fires when client disconnects. Always accept and propagate. Without it, work continues after user disconnects, wasting CPU.

**Q9. What's a record in modern C#?**

**A.** Immutable-by-default reference type with value-based equality. Compiler generates Equals, GetHashCode, ToString, copy constructor, 'with' expression. Use for DTOs, value objects, messages.

**Q10. What's pattern matching in C#?**

**A.** switch expressions and 'is' patterns. Cleaner than if-else chains. Example: `result switch { > 0 => 'positive', < 0 => 'negative', _ => 'zero' }`. Useful for type checking, deconstruction, exhaustive matching.

**Q11. What's the using statement?**

**A.** Calls Dispose() on IDisposable when scope exits. Modern syntax: 'using var stream = new FileStream(...)' — disposed at end of method. Prevents resource leaks (file handles, DB connections).

**Q12. What's an extension method?**

**A.** Static method that appears as instance method on existing type. Declared in static class with 'this' keyword: 'public static int WordCount(this string s) => s.Split().Length'. LINQ uses extension methods extensively.

**Q13. What are nullable reference types?**

**A.** C# 8+ feature. Reference types are non-null by default. 'string' means must have value; 'string?' means may be null. Compiler warns on potential null dereferences. Enable with #nullable in projects.

**Q14. What's the difference between IDisposable and IAsyncDisposable?**

**A.** IDisposable: synchronous cleanup via Dispose(). IAsyncDisposable: async cleanup via DisposeAsync(). Use IAsyncDisposable for resources with async cleanup (HttpClient, DbContext in EF Core, streams). Syntax: 'await using var x = ...'.

**Q15. What's a Span\<T\>?**

**A.** Stack-allocated view over contiguous memory. Zero-allocation slicing. Use for: high-performance parsing, buffer manipulation. Limits: stack-only, no async, no fields in classes. For async, use Memory\<T\>.

### 6.2 — .NET / ASP.NET Core (Q16-30)

**Q16. What's Dependency Injection in .NET?**

**A.** Built-in container in Microsoft.Extensions.DependencyInjection. Three lifetimes: Singleton (app lifetime, must be thread-safe), Scoped (per HTTP request, most common for services), Transient (new instance every time). DbContext is always Scoped.

**Q17. What's middleware in ASP.NET Core?**

**A.** Components that handle requests in a pipeline. Each can short-circuit or pass to next via await _next(). Order matters: Authentication before Authorization, CORS before MapControllers. This is the Chain of Responsibility pattern.

**Q18. What's the difference between Controllers and Minimal APIs?**

**A.** Controllers: full features (filters, model binding, action results), more boilerplate. Minimal APIs: less code, faster startup, best for small services. Both viable in .NET 8+. Use Controllers for large APIs, Minimal for microservices.

**Q19. How do you version a REST API?**

**A.** Three options: URL (/api/v1/orders), header (api-version), query string (?version=1). URL most explicit. Use Microsoft.AspNetCore.Mvc.Versioning library. Always have a deprecation policy.

**Q20. What's ProblemDetails (RFC 7807)?**

**A.** Standard format for HTTP error responses. Has status, title, detail, instance, type fields. Use in ASP.NET Core for consistent errors across endpoints. Built-in support via app.UseExceptionHandler().

**Q21. How do you handle configuration in .NET?**

**A.** Layered: appsettings.json -> appsettings.{env}.json -> environment variables -> Azure App Configuration -> Key Vault. Higher priority overrides lower. Use IOptions\<T\> pattern to inject strongly-typed config.

**Q22. What's IOptions vs IOptionsSnapshot vs IOptionsMonitor?**

**A.** IOptions: singleton, doesn't reload. IOptionsSnapshot: per-request scoped, reloads each request. IOptionsMonitor: singleton with change notifications. Use IOptionsMonitor when config can change at runtime.

**Q23. How do you do health checks?**

**A.** Built-in AddHealthChecks(). Endpoints: /health/live (is app alive?), /health/ready (can it serve traffic?). Used by K8s liveness/readiness probes. Check DB, Service Bus, Redis connectivity.

**Q24. What's a hosted service / background service?**

**A.** IHostedService or BackgroundService. Long-running task that starts with app, stops on shutdown. Use for: outbox publishers, scheduled jobs, queue consumers, cache warmers.

**Q25. How do you log in ASP.NET Core?**

**A.** ILogger\<T\> injected via DI. Structured logging: _logger.LogInformation('Order {OrderId} placed for {Amount}', id, amount). NEVER string interpolate — breaks structured logging. Log levels: Trace, Debug, Info, Warning, Error, Critical.

**Q26. What's CORS and how do you configure it?**

**A.** Cross-Origin Resource Sharing. Browser blocks JS calls to different origin unless server allows. Configure in Program.cs with AddCors() + UseCors(). NEVER use AllowAnyOrigin with AllowCredentials — security risk.

**Q27. How do you validate input?**

**A.** Data annotations on DTOs: [Required], [StringLength], [Range], [EmailAddress]. ASP.NET auto-validates with [ApiController]. For complex rules: FluentValidation library.

**Q28. What's the difference between IActionResult and Task\<IActionResult\>?**

**A.** Same thing, async wrapper. Task\<IActionResult\> for async controller actions (most cases). IActionResult for sync. Modern code: ActionResult\<T\> for type-safe responses with OpenAPI generation.

**Q29. What's HttpClientFactory?**

**A.** Creates and manages HttpClient instances. Avoids socket exhaustion from too many HttpClient creation. Adds: handler lifetime management, Polly integration, named/typed clients. Register: services.AddHttpClient\<IMyClient, MyClient\>().

**Q30. What's a Typed Client vs Named Client?**

**A.** Named: AddHttpClient('github'). Inject IHttpClientFactory, call CreateClient('github'). Typed: AddHttpClient\<IGitHubClient, GitHubClient\>(). Inject IGitHubClient directly. Typed is cleaner; preferred.

### 6.3 — Entity Framework Core (Q31-45)

**Q31. Lazy Loading vs Eager Loading vs Explicit Loading?**

**A.** Lazy: loads navigation properties on access — N+1 risk. Eager: .Include() loads in same query. Explicit: manual via context.Entry().Reference().LoadAsync(). Use Eager almost always.

**Q32. What's the N+1 query problem?**

**A.** 1 query to list items + N queries for each item's relations. Fix with .Include() to JOIN in one query. Or project to DTO with .Select() — only loads needed fields. Use AsSplitQuery() if Include causes Cartesian explosion.

**Q33. AsNoTracking vs default tracking?**

**A.** Default tracks every entity to detect changes. AsNoTracking skips tracking — faster, less memory. Use for read-only queries (GETs, reports). Required if you'll modify and save: tracking on.

**Q34. How does EF Core handle transactions?**

**A.** SaveChangesAsync wraps all pending changes in implicit transaction. For multiple SaveChanges in one transaction: BeginTransactionAsync() / CommitAsync(). For distributed transactions across services: use Saga or Outbox, not 2PC.

**Q35. How do you handle EF migrations in production?**

**A.** Don't apply at app startup (race conditions). Generate SQL script: 'dotnet ef migrations script'. Apply via CI/CD or DBA. Rule: backward-compatible migrations only. Add columns nullable first, deploy code, backfill, then NOT NULL.

**Q36. How do you avoid SQL injection with EF Core?**

**A.** LINQ is always safe — translates to parameterized SQL. FromSqlInterpolated() with $"..." is safe — args become parameters. FromSqlRaw() with string concatenation is UNSAFE. Always parameterize.

**Q37. What's the difference between Add, Update, Attach?**

**A.** Add: marks entity as new, inserts on save. Update: marks as modified, updates all properties. Attach: tracks as Unchanged, no DB op until you modify properties. Use Attach for known-existing entities to avoid SELECT before UPDATE.

**Q38. How do you handle concurrency conflicts?**

**A.** Optimistic concurrency. Add [Timestamp] byte[] RowVersion property. EF checks WHERE RowVersion = @oldVersion in UPDATE. If 0 rows affected: DbUpdateConcurrencyException. Handle by reload + retry, or user choice.

**Q39. What's a global query filter?**

**A.** Filter applied to every query automatically. Common for soft delete (WHERE IsDeleted = 0) or multi-tenancy (WHERE TenantId = @currentTenant). Configure in OnModelCreating with HasQueryFilter().

**Q40. How do you bulk insert in EF Core?**

**A.** EF Core has AddRange + SaveChanges (batched but still one INSERT per row). For real bulk: use EFCore.BulkExtensions library (BulkInsert, BulkUpdate, BulkDelete). Or fall back to raw SQL bulk copy for very large data.

**Q41. What's a DbContext pooling?**

**A.** Reuses DbContext instances across requests instead of creating new each time. Configure: AddDbContextPool\<\>(). Speeds up high-traffic apps. Trade-off: instances must be safely reset between uses; some scenarios incompatible.

**Q42. How do you log SQL EF generates?**

**A.** Add logger to context: options.LogTo(Console.WriteLine, LogLevel.Information). Or use EnableSensitiveDataLogging() to see parameter values (dev only). In production, use Application Insights dependency tracking.

**Q43. What's an owned type?**

**A.** Entity that lives inside another entity's table. Example: Address owned by Customer. ModelBuilder: builder.Entity\<Customer\>().OwnsOne(c => c.Address). Use for value objects, embedded structures.

**Q44. What's compiled queries?**

**A.** Pre-compile a query for reuse. EF Core 5+: EF.CompileAsyncQuery((Context ctx, int id) => ctx.Orders.First(o => o.Id == id)). Skips query translation on each call. Use for hot paths.

**Q45. When NOT to use EF Core?**

**A.** Heavy reporting / analytics — use Dapper or raw SQL. Very large bulk operations — use bulk copy. Stored procedure-heavy systems. Real-time aggregations. EF is great for OLTP CRUD, less so for OLAP.

### 6.4 — SQL Performance (Q46-55)

**Q46. What's an index and how does it work?**

**A.** B-tree data structure that speeds up reads. Without index, query scans full table. With index, database walks tree to matching rows. Trade-off: reads faster, writes slower (must update index), uses storage.

**Q47. Clustered vs non-clustered index?**

**A.** Clustered: data IS the index. One per table (usually primary key). Non-clustered: separate structure pointing to rows. Many per table. Covering index: includes all columns query needs — no row lookup.

**Q48. How do you optimize a slow query?**

**A.** Get execution plan (SET STATISTICS IO ON or graphical). Look for: table scans, key lookups, missing indexes, expensive sorts. Check WHERE is sargable (function on column prevents index). Project only needed columns. Validate with plan after fixes.

**Q49. What's a sargable query?**

**A.** 'Search ARGument-able' — can use an index. BAD: WHERE YEAR(OrderDate) = 2026 (function on column). GOOD: WHERE OrderDate >= '2026-01-01' AND OrderDate < '2027-01-01'. Functions on columns break index usage.

**Q50. What are SQL isolation levels?**

**A.** Read Uncommitted (dirty reads OK, fastest). Read Committed (default, no dirty reads). Repeatable Read (no phantoms in same tx). Serializable (full isolation, slowest). Snapshot/RCSI: MVCC — readers don't block writers. Enable RCSI in SQL Server for best concurrency.

**Q51. What's a deadlock?**

**A.** Two transactions hold locks the other needs. SQL detects, kills the cheaper one (1205 error). Fix: access tables in same order across all transactions. Keep transactions short. Lower isolation when safe. App retries on 1205.

**Q52. What's READ_COMMITTED_SNAPSHOT (RCSI)?**

**A.** MVCC mode in SQL Server. Readers see committed snapshot — don't block, aren't blocked by writers. Massive concurrency win. Enable: ALTER DATABASE SET READ_COMMITTED_SNAPSHOT ON. Trade-off: uses tempdb for versions.

**Q53. OLTP vs OLAP?**

**A.** OLTP: transactional, many small writes/reads, normalized schema. Examples: order entry, banking. OLAP: analytical, large scans/aggregations, denormalized star/snowflake. Examples: BI, reports. Don't run OLAP on OLTP database — kills user-facing queries.

**Q54. What's a covering index?**

**A.** Index that includes all columns the query needs. Database serves the query from the index alone — no row lookup. Use INCLUDE clause: CREATE INDEX IX_Orders_Customer INCLUDE (Total, Status). Big perf win for hot queries.

**Q55. Stored procedures vs LINQ — when each?**

**A.** LINQ default for most cases — readable, type-safe, refactorable. Stored procs for: complex performance-critical operations, batch ETL, security boundary (grant EXEC without table access). Don't use SPs just because — they split logic between code and DB.

### 6.5 — Azure for Developers (Q56-70)

**Q56. What Azure services have you used as a developer?**

**A.** App Service, Azure Functions, Service Bus, Cosmos DB, Azure SQL, Key Vault, Managed Identity, App Insights, APIM, Front Door, Azure DevOps/GitHub Actions. AKS from developer side (worked alongside platform team for cluster setup).

**Q57. How do you deploy an ASP.NET Core app to App Service?**

**A.** Production: via CI/CD (Azure DevOps Pipelines or GitHub Actions). Process: build -> test -> publish artifact -> deploy to staging slot -> validate -> swap to production. Zero-downtime swap. Visual Studio Publish only for dev/test.

**Q58. How does Managed Identity work with Azure SQL?**

**A.** Connection string: 'Authentication=Active Directory Default'. App's MI must have user mapping in SQL: CREATE USER [appservice-name] FROM EXTERNAL PROVIDER. No password in connection string. DefaultAzureCredential picks up MI in Azure, IDE login locally.

**Q59. How do you publish to Service Bus from .NET?**

```csharp
var sender = busClient.CreateSender("orders");
var msg = new ServiceBusMessage(JsonSerializer.Serialize(order))
{
    MessageId = order.Id.ToString(), // for dedup
    ContentType = "application/json",
    Subject = nameof(OrderPlaced)
};
await sender.SendMessageAsync(msg);
```

**Q60. What's PeekLock vs ReceiveAndDelete in Service Bus?**

**A.** PeekLock (default): receive + lock, must Complete to remove, or Abandon to retry. At-least-once delivery. ReceiveAndDelete: atomic, faster, but message lost if consumer crashes. At-most-once. Always use PeekLock in production.

**Q61. What's a Dead-Letter Queue (DLQ)?**

**A.** Sub-queue for unprocessable messages. Triggers: exceeded max retries, expired, filter failed, explicitly DLQ'd. Alert on DLQ depth. Triage and reprocess after fixing root cause. Never blindly drain.

**Q62. Cosmos DB partition key — why critical?**

**A.** Determines physical distribution. CANNOT change after container created. Wrong choice = hot partitions = throttling. Pick: high cardinality, even access, used in queries. Bad picks: single tenantId when one tenant is huge, dateOnly when all writes go to today.

**Q63. What's a hot partition and how do you fix it?**

**A.** Partition getting disproportionate traffic — Cosmos throttles even if container has spare RU. Fixes: better partition key (best, requires migration), add randomness for hot writes (suffix 0-9), aggressive caching for hot reads, move VIP tenants to dedicated containers.

**Q64. What's the difference between Azure SQL and Cosmos DB?**

**A.** Azure SQL: relational, ACID, joins, complex queries, single-region. Cosmos: NoSQL, global distribution, schemaless, sub-10ms reads. Pick Cosmos for: global apps, massive scale. Pick SQL for: rich queries, joins, regulatory familiarity.

**Q65. What are Cosmos consistency levels?**

**A.** Strong (linearizable, highest cost). Bounded Staleness (lag bounded by K versions or T seconds). Session (default, see your own writes). Consistent Prefix (no out-of-order). Eventual (cheapest). Most apps use Session.

**Q66. What's Azure Functions cold start?**

**A.** On Consumption plan, idle functions de-provisioned. First request after idle takes 1-10s for .NET. Mitigations: Premium plan or Flex Consumption (pre-warmed), keep-alive timer triggers, AOT compilation, smaller deployments.

**Q67. When use Durable Functions?**

**A.** Stateful workflows. State persists, orchestrator survives crashes. Patterns: function chaining, fan-out/fan-in, async HTTP API, monitoring, human interaction. Use for sagas, multi-step processes, approval workflows.

**Q68. What's Application Insights and how do you instrument it?**

**A.** App-level telemetry. AddApplicationInsightsTelemetry() auto-tracks requests, dependencies, exceptions, distributed traces via W3C Trace Context. Custom: telemetry.TrackEvent(), TrackMetric(). Query with KQL in Log Analytics.

**Q69. What's an SLI, SLO, SLA?**

**A.** SLI: the metric you measure ('% of requests <500ms'). SLO: your target (99.9% over 30 days). SLA: contractual promise with customer (with penalties). SLO is tighter than SLA — gives buffer.

**Q70. What's an error budget?**

**A.** If SLO is 99.9%, budget is 0.1% — ~43 min/month allowed downtime. Budget remaining: ship faster, take risk. Budget exhausted: freeze risky changes, focus on reliability. Forces quantitative conversation between product and platform.

### 6.6 — Microservices (Q71-85)

**Q71. What's a microservice?**

**A.** Small, independently deployable service owning a bounded business capability. Three properties: independently deployable, owns its data, loosely coupled communication. Size doesn't define it — independence does.

**Q72. When NOT to use microservices?**

**A.** Small team (<10 engineers). Immature CI/CD or observability. Domain not yet clear. Cross-service transactions common. No on-call culture. Solves people problems, not technical. Start with monolith, split when team independence demands it.

**Q73. What's a distributed monolith?**

**A.** Services that LOOK independent but require coordinated deploys, share databases, or have synchronous chains. Worst of both worlds. Smells: lockstep deploys, shared DBs, long sync chains, coordinated releases.

**Q74. Sync vs async communication between services?**

**A.** Sync (REST, gRPC): caller waits. Use when caller needs the answer to proceed. Async (queues, events): fire-and-forget or poll later. Use for background work, decoupling, multiple consumers. Default to async — sync only with specific reason.

**Q75. Why is sync risky in microservices?**

**A.** Three reasons: 1) Cascading failures — slow downstream ties up caller threads. 2) Latency compounds — 5 hops × 50ms = 250ms minimum. 3) Availability multiplies — 0.999^5 = 99.5% chain reliability.

**Q76. What's REST vs gRPC?**

**A.** REST: HTTP + JSON, loose contract (OpenAPI), human-readable, slower. gRPC: HTTP/2 + Protobuf, strong contract (.proto), faster, native streaming. Pick REST for public/browser APIs. Pick gRPC for internal service-to-service.

**Q77. What's the Saga pattern?**

**A.** Long-running distributed transaction via local transactions and compensating actions. Two flavors: Orchestration (central coordinator) or Choreography (services emit/react to events). Use for multi-service workflows that can't use 2PC.

**Q78. What's the Outbox pattern?**

**A.** Solves dual-write problem. Write business state AND outbox message in same DB transaction. Background poller reads outbox, publishes to bus, marks sent. Inbox table on consumer side for dedup. Guaranteed reliable event publishing.

**Q79. How do you make a handler idempotent?**

**A.** Three options: 1) Dedup table — atomic insert of processed messageId with business work. 2) Conditional updates — UPDATE WHERE status = expected_old. 3) Set semantics — operation is no-op when state already there.

**Q80. Why is exactly-once delivery a myth?**

**A.** Wire-level: ack can drop, you don't know if message arrived. Solution: at-least-once delivery + idempotent consumers = 'effectively-once' processing. Same effect as exactly-once for users.

**Q81. What's a circuit breaker?**

**A.** State machine: Closed (calls flow), Open (fail fast after N failures), Half-Open (probe after cooldown). Prevents cascade when downstream struggles. Polly library: standard .NET implementation.

**Q82. What's exponential backoff with jitter?**

**A.** Retry wait increases exponentially: 100ms, 200ms, 400ms, 800ms. Jitter (random variance) prevents thundering herd — synchronized retries hitting downstream in waves. Always pair backoff with jitter.

**Q83. What's a bounded context?**

**A.** Boundary inside which a model is internally consistent. Same word means different things in different contexts. 'Customer' in Sales (name, payment) vs Support (tickets) vs Billing (invoices). Service boundaries should follow bounded contexts.

**Q84. How do you decide service boundaries?**

**A.** Three heuristics: 1) Bounded contexts — each is a service candidate. 2) Change rate — things that change together belong together. 3) Data ownership — each service owns its data. Avoid splitting by technical layer (UI/business/data).

**Q85. What's an API Gateway?**

**A.** Single entry point for client traffic. Provides cross-cutting concerns: auth, rate limiting, transformation, versioning, caching, observability. Azure: APIM. Eliminates duplicating these in every service.

### 6.7 — React (Q86-95)

**Q86. What's React in one sentence?**

**A.** JavaScript library for building UIs using composable components and a virtual DOM that efficiently updates the real DOM when state changes.

**Q87. Most common React hooks?**

**A.** useState (local state), useEffect (side effects), useRef (mutable ref persisting across renders), useMemo (memoize expensive calc), useCallback (memoize functions), useContext (access React Context).

**Q88. useState vs useReducer?**

**A.** useState: simple, primitive values or small objects. useReducer: complex state with multiple sub-values, or when next state depends on previous. Same pattern as Redux.

**Q89. What's the most common useEffect mistake?**

**A.** Missing dependencies in dep array. Causes stale closures over state. Always include all values from outer scope. Use eslint-plugin-react-hooks 'exhaustive-deps' rule to catch automatically.

**Q90. Context vs Redux?**

**A.** Context: built-in, best for app-wide read-mostly data (theme, user, locale). Redux: external library, for complex client state with many updates. In 2026 many apps don't need Redux — TanStack Query for server state + Context for app state.

**Q91. What's TanStack Query?**

**A.** Library for managing SERVER state in React. Handles caching, refetch, loading/error states, optimistic updates. Replaces ad-hoc useEffect+fetch patterns. Biggest 2026 React mistake: putting server data in Redux instead of TanStack Query.

**Q92. How do you optimize React performance?**

**A.** Code splitting with React.lazy. React.memo for components receiving same props but parent re-renders frequently. useMemo for expensive calc. useCallback for callbacks passed to memo children. Virtualize large lists with react-window. Profile with DevTools.

**Q93. What are React Server Components?**

**A.** Components running on server, output HTML directly, don't ship JS for those parts. Benefits: smaller bundles, direct DB access, faster first paint. Limits: no useState/useEffect/browser APIs. Need framework support (Next.js).

**Q94. How do you handle auth in a React app?**

**A.** OAuth/OIDC with Entra ID via MSAL.js (Microsoft official). Access token in memory (NOT localStorage — XSS risk). Refresh token in HttpOnly cookie. Bearer header on API calls. Don't roll your own OAuth.

**Q95. Controlled vs uncontrolled components?**

**A.** Controlled: form value comes from state, updates via onChange. Uncontrolled: input manages own value, read via ref. Use controlled for forms with validation. Uncontrolled for simple file uploads, integration with non-React code.

### 6.8 — DevOps & Quality (Q96-100)

**Q96. What's CI/CD?**

**A.** Continuous Integration: every commit builds, runs tests, validates. CD: Continuous Delivery (ready to deploy anytime) or Deployment (auto-deploys to production). Goal: small, frequent, low-risk releases.

**Q97. What are DORA metrics?**

**A.** Four metrics measuring delivery performance: Deployment Frequency (how often), Lead Time (commit to production), Change Failure Rate (% deploys causing incidents), MTTR (Mean Time To Restore). Elite teams: deploy multiple times per day, lead time hours, <15% failure, MTTR <1 hour.

**Q98. What's blue-green vs canary deployment?**

**A.** Blue-Green: two identical environments, switch traffic instantly. Fast rollback. Canary: route small % to new version, monitor, gradually increase. Risk-limited. Use Front Door / APIM weighted routing for both.

**Q99. What's the test pyramid?**

**A.** Many fast unit tests, fewer integration tests, very few end-to-end. Unit: milliseconds, hundreds-thousands. Integration: seconds, dozens-hundreds. E2E: minutes, only critical paths. Anti-pattern: ice-cream cone (lots of E2E, few unit) — slow and fragile.

**Q100. What's FIRST in testing?**

**A.** Fast (milliseconds), Independent (no shared state, order doesn't matter), Repeatable (same result every time), Self-validating (clear pass/fail), Timely (written close to the code). FIRST principles for unit tests.

## Section 7 — Live Coding Interview Questions

Top 20 coding problems you might face. Each with approach + clean C# solution. Practice actively — type them yourself.

### How to Approach Any Coding Question

**1. Clarify (1-2 min):** Input format, output, edge cases, constraints.

**2. Examples:** Walk through 1-2 manually.

**3. Approach:** Plain English. Discuss complexity.

**4. Code (10-15 min):** Clean, readable. Talk through it.

**5. Test:** Walk through with example. Check edge cases.

> **Say it like this:** "Before I write code, can I confirm the input format and edge cases?" — One sentence that signals senior.

### Problem 1: Reverse a String

```csharp
// Best for production
string Reverse(string s) => new string(s.Reverse().ToArray());

// Manual — show you know the algorithm
string ReverseManual(string s) {
    var chars = s.ToCharArray();
    int left = 0, right = chars.Length - 1;
    while (left < right) {
        (chars[left], chars[right]) = (chars[right], chars[left]);
        left++; right--;
    }
    return new string(chars);
}
// Time: O(n), Space: O(1) extra
```

### Problem 2: Palindrome Check

```csharp
bool IsPalindrome(string s) {
    int left = 0, right = s.Length - 1;
    while (left < right) {
        // Skip non-alphanumerics, case-insensitive
        while (left < right && !char.IsLetterOrDigit(s[left])) left++;
        while (left < right && !char.IsLetterOrDigit(s[right])) right--;
        if (char.ToLower(s[left]) != char.ToLower(s[right])) return false;
        left++; right--;
    }
    return true;
}
```

### Problem 3: Two Sum

Find indices of two numbers that sum to target.

```csharp
int[] TwoSum(int[] nums, int target) {
    var seen = new Dictionary<int, int>(); // value -> index
    for (int i = 0; i < nums.Length; i++) {
        int complement = target - nums[i];
        if (seen.TryGetValue(complement, out int j))
            return new[] { j, i };
        seen[nums[i]] = i;
    }
    return Array.Empty<int>();
}
// O(n) time, O(n) space
```

### Problem 4: First Non-Repeating Character

```csharp
char? FirstNonRepeating(string s) {
    var counts = new Dictionary<char, int>();
    foreach (var ch in s)
        counts[ch] = counts.GetValueOrDefault(ch, 0) + 1;
    foreach (var ch in s)
        if (counts[ch] == 1) return ch;
    return null;
}
```

### Problem 5: Find Duplicates

```csharp
List<int> FindDuplicates(int[] nums) {
    var seen = new HashSet<int>();
    var dupes = new List<int>();
    foreach (var n in nums)
        if (!seen.Add(n)) dupes.Add(n);
    return dupes;
}

// LINQ version
var dupes = nums.GroupBy(x => x)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key).ToList();
```

### Problem 6: Reverse Linked List

```csharp
ListNode? Reverse(ListNode? head) {
    ListNode? prev = null;
    ListNode? curr = head;
    while (curr != null) {
        var next = curr.Next;
        curr.Next = prev;
        prev = curr;
        curr = next;
    }
    return prev;
}
```

### Problem 7: Find Middle of Linked List

```csharp
ListNode? FindMiddle(ListNode? head) {
    var slow = head;
    var fast = head;
    while (fast?.Next != null) {
        slow = slow!.Next;
        fast = fast.Next.Next;
    }
    return slow;
}
// Floyd's tortoise-hare. Same pattern detects cycles.
```

### Problem 8: TTL Cache

```csharp
public class TtlCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (TValue Value, DateTime Expiry)> _store = new();
    private readonly TimeSpan _ttl;
    public TtlCache(TimeSpan ttl) => _ttl = ttl;

    public void Set(TKey key, TValue value) =>
        _store[key] = (value, DateTime.UtcNow.Add(_ttl));

    public bool TryGet(TKey key, out TValue? value)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.Expiry > DateTime.UtcNow)
            {
                value = entry.Value;
                return true;
            }
            _store.TryRemove(key, out _);
        }
        value = default;
        return false;
    }
}
```

### Problem 9: Rate Limiter (Sliding Window)

```csharp
public class RateLimiter
{
    private readonly int _maxCalls;
    private readonly TimeSpan _window;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _calls = new();

    public RateLimiter(int maxCalls, TimeSpan window)
    {
        _maxCalls = maxCalls;
        _window = window;
    }

    public bool TryAcquire(string userId)
    {
        var queue = _calls.GetOrAdd(userId, _ => new ConcurrentQueue<DateTime>());
        var now = DateTime.UtcNow;
        var cutoff = now - _window;
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);
        if (queue.Count >= _maxCalls) return false;
        queue.Enqueue(now);
        return true;
    }
}
```

### Problem 10: Retry with Exponential Backoff

```csharp
public static async Task<T> RetryAsync<T>(
    Func<Task<T>> operation, int maxAttempts = 3,
    TimeSpan? initialDelay = null, CancellationToken ct = default)
{
    var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
    var random = new Random();
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try { return await operation(); }
        catch (Exception ex) when (
            ex is HttpRequestException || ex is TimeoutException)
        {
            if (attempt == maxAttempts) throw;
            var jitter = random.Next(0, 100);
            await Task.Delay(delay + TimeSpan.FromMilliseconds(jitter), ct);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
        }
    }
    throw new InvalidOperationException("Unreachable");
}
// Production: use Polly library
```

### Problem 11: Thread-Safe Singleton

```csharp
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;
    private Logger() { }
}
// Lazy<T> is thread-safe by default. Modern alt: DI singleton.
```

### Problem 12: Longest Substring Without Repeating

```csharp
int LongestUnique(string s)
{
    var seen = new Dictionary<char, int>();
    int start = 0, maxLen = 0;
    for (int i = 0; i < s.Length; i++)
    {
        if (seen.TryGetValue(s[i], out int prev) && prev >= start)
            start = prev + 1;
        seen[s[i]] = i;
        maxLen = Math.Max(maxLen, i - start + 1);
    }
    return maxLen;
}
// Sliding window pattern
```

### Problem 13: Moving Average

```csharp
public class MovingAverage
{
    private readonly Queue<double> _values = new();
    private readonly int _windowSize;
    private double _sum;

    public MovingAverage(int windowSize) => _windowSize = windowSize;

    public double Add(double value)
    {
        _values.Enqueue(value);
        _sum += value;
        if (_values.Count > _windowSize)
            _sum -= _values.Dequeue();
        return _sum / _values.Count;
    }
}
// O(1) per update — for real-time telemetry
```

### Problem 14: Group Anagrams

```csharp
List<List<string>> GroupAnagrams(string[] words)
{
    return words
        .GroupBy(w => new string(w.OrderBy(c => c).ToArray()))
        .Select(g => g.ToList())
        .ToList();
}
```

### Problem 15: State Machine for Order Workflow

```csharp
public enum OrderState { Pending, Paid, Shipped, Delivered, Cancelled }

public class OrderStateMachine
{
    private static readonly Dictionary<OrderState, HashSet<OrderState>> Transitions = new()
    {
        [OrderState.Pending] = new() { OrderState.Paid, OrderState.Cancelled },
        [OrderState.Paid] = new() { OrderState.Shipped, OrderState.Cancelled },
        [OrderState.Shipped] = new() { OrderState.Delivered },
        [OrderState.Delivered] = new() { },
        [OrderState.Cancelled] = new() { }
    };

    public bool CanTransition(OrderState from, OrderState to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public OrderState Transition(OrderState from, OrderState to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Cannot transition {from} -> {to}");
        return to;
    }
}
```

### Code Review Bugs to Spot

#### Bug 1: Async Deadlock

```csharp
// BAD
public string GetUserName(int id) {
    var user = _userService.GetAsync(id).Result; // .Result blocks
    return user.Name;
}

// FIX: async all the way
public async Task<string> GetUserNameAsync(int id) {
    var user = await _userService.GetAsync(id);
    return user.Name;
}
```

#### Bug 2: AsEnumerable Trap

```csharp
// BAD - loads ALL orders into memory FIRST
var orders = _db.Orders.AsEnumerable()
    .Where(o => o.Total > 100).ToList();

// FIX - filter pushed to SQL
var orders = _db.Orders
    .Where(o => o.Total > 100).ToList();
```

#### Bug 3: DbContext as Singleton

```csharp
// BAD - DbContext is NOT thread-safe
services.AddSingleton<AppDbContext>();

// FIX - always Scoped
services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(connStr));
```

#### Bug 4: Race Condition in Counter

```csharp
// BAD - _count++ is not atomic
public void Increment() => _count++;

// FIX - atomic operation
public void Increment() => Interlocked.Increment(ref _count);
```

#### Bug 5: SQL Injection

```csharp
// BAD - string interpolation in SQL
var sql = $"SELECT * FROM Orders WHERE Name = '{name}'";
return _db.Orders.FromSqlRaw(sql).ToList();

// FIX - parameterized
return _db.Orders
    .FromSqlInterpolated($"SELECT * FROM Orders WHERE Name = {name}")
    .ToList();
```

#### Bug 6: Cache Stampede

```csharp
// BAD - 1000 concurrent requests all miss + hit DB
if (_cache.TryGetValue(id, out User cached)) return cached;
var user = await _db.Users.FindAsync(id);
_cache.Set(id, user);

// FIX - GetOrCreateAsync = single-flight
return await _cache.GetOrCreateAsync($"user:{id}", async entry => {
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await _db.Users.FindAsync(id);
});
```

#### Bug 7: Disposable Leak

```csharp
// BAD - never disposed
var stream = new FileStream(path, FileMode.Open);
var reader = new StreamReader(stream);
return reader.ReadToEnd();

// FIX - using declarations
using var stream = new FileStream(path, FileMode.Open);
using var reader = new StreamReader(stream);
return reader.ReadToEnd();
```

## Section 8 — Design Patterns Cheat Sheet

Top 15 patterns. For each: trigger sentence (when to use) + key code. Tech leads must know these by name and use case.

### How to Talk About Patterns

- Name the pattern explicitly: 'I'd model this as a Strategy pattern.'
- State the trigger: 'Strategy is for when multiple algorithms exist for the same task, selectable at runtime.'
- Mention the trade-off: Every pattern has a cost — complexity, indirection.

### Pattern 1: Singleton

Trigger: 'Exactly one of this thing in the process, global access acceptable.' Cases: config, cache, logger.

```csharp
public sealed class ConnectionPool
{
    private static readonly Lazy<ConnectionPool> _instance = new(() => new ConnectionPool());
    public static ConnectionPool Instance => _instance.Value;
    private ConnectionPool() { }
}
// Modern alternative: services.AddSingleton<ConnectionPool>(); — more testable
```

### Pattern 2: Factory

Trigger: 'I need to create objects of varying types, choice depends on input.'

```csharp
public interface IPaymentProcessor { Task<bool> ChargeAsync(decimal amount); }
public class StripeProcessor : IPaymentProcessor { /* ... */ }
public class PayPalProcessor : IPaymentProcessor { /* ... */ }

public class PaymentProcessorFactory
{
    private readonly IServiceProvider _services;
    public PaymentProcessorFactory(IServiceProvider services) => _services = services;

    public IPaymentProcessor Create(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => _services.GetRequiredService<StripeProcessor>(),
        PaymentMethod.PayPal => _services.GetRequiredService<PayPalProcessor>(),
        _ => throw new NotSupportedException()
    };
}
```

### Pattern 3: Builder

Trigger: 'Constructing object with many optional parts; want readable, immutable result.'

```csharp
var request = new HttpRequestBuilder()
    .Method(HttpMethod.Post)
    .Url("https://api.example.com/orders")
    .Header("Authorization", $"Bearer {token}")
    .JsonBody(new { customerId = 123 })
    .Build();
```

### Pattern 4: Adapter

Trigger: 'Class with wrong interface for my consumer.' Wrap incompatible API in clean interface.

```csharp
public class LegacyComAdapter : IPromotionService
{
    private readonly LegacyComPromotionApi _legacy;
    public LegacyComAdapter(LegacyComPromotionApi legacy) => _legacy = legacy;

    public Promotion Get(Guid id)
    {
        var legacy = _legacy.GetPromotion(id.ToString());
        return new Promotion {
            Id = id,
            ApplicableItems = legacy.ItemList.Split('|').ToList(),
            Discount = legacy.DiscountPct / 100m
        };
    }
}
// Same pattern as your VB6 -> .NET migration
```

### Pattern 5: Decorator

Trigger: 'Add cross-cutting behavior (logging, caching, retry) without modifying core.'

```csharp
public class CachedOrderRepository : IOrderRepository
{
    private readonly IOrderRepository _inner;
    private readonly IMemoryCache _cache;
    public CachedOrderRepository(IOrderRepository inner, IMemoryCache cache) {
        _inner = inner; _cache = cache;
    }

    public async Task<Order?> GetAsync(Guid id) =>
        await _cache.GetOrCreateAsync($"order:{id}", async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.GetAsync(id);
        });

    public Task SaveAsync(Order order) {
        _cache.Remove($"order:{order.Id}");
        return _inner.SaveAsync(order);
    }
}
// Stack: Logged -> Cached -> Retry -> SqlRepo
// ASP.NET middleware works exactly like this
```

### Pattern 6: Facade

Trigger: 'Expose simpler API over complex subsystem.'

```csharp
public class OrderFacade {
    private readonly InventoryService _inventory;
    private readonly PaymentService _payment;
    private readonly ShippingService _shipping;

    public async Task<OrderResult> PlaceOrderAsync(OrderRequest request) {
        // Hide all subsystem orchestration
        var reservation = await _inventory.ReserveAsync(request.Items);
        var charge = await _payment.ChargeAsync(request.Card, request.Total);
        await _shipping.ScheduleAsync(request.Address);
        return new OrderResult { Success = true };
    }
}
```

### Pattern 7: Strategy

Trigger: 'Many algorithms for the same task, selectable at runtime.' Most-cited pattern in design reviews.

```csharp
public interface IDiscountStrategy {
    string Name { get; }
    decimal Apply(Cart cart);
}

public class PercentageDiscount : IDiscountStrategy {
    public string Name => "Percentage";
    public decimal Apply(Cart cart) => cart.Subtotal * 0.10m;
}

public class DiscountSelector {
    private readonly IDictionary<string, IDiscountStrategy> _strategies;
    public DiscountSelector(IEnumerable<IDiscountStrategy> strategies) {
        _strategies = strategies.ToDictionary(s => s.Name);
    }
    public IDiscountStrategy Get(string name) => _strategies[name];
}

services.AddScoped<IDiscountStrategy, PercentageDiscount>();
services.AddScoped<IDiscountStrategy, BogoDiscount>();
```

### Pattern 8: Observer (Events)

Trigger: 'When this changes, multiple unrelated things need to react.'

```csharp
public class OrderService {
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public async Task<Guid> PlaceOrderAsync(OrderRequest request) {
        var order = await SaveOrderAsync(request);
        OrderPlaced?.Invoke(this, new OrderPlacedEventArgs { OrderId = order.Id });
        return order.Id;
    }
}
// Modern alt: MediatR domain events
// Cross-service: Service Bus topic + subscriptions
```

### Pattern 9: Mediator (MediatR)

Trigger: 'N components should talk through one hub instead of N×N.'

```csharp
public record CreateOrderCommand(string CustomerId, List<OrderItem> Items) : IRequest<Guid>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid> {
    private readonly AppDbContext _db;
    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct) {
        var order = new Order(cmd.CustomerId, cmd.Items);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order.Id;
    }
}

services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// In controller
var orderId = await _mediator.Send(new CreateOrderCommand(customerId, items));
```

### Pattern 10: Chain of Responsibility (Middleware)

ASP.NET Core middleware IS this pattern. Each handler decides to handle or pass to next.

```csharp
app.UseAuthentication(); // each is a handler
app.UseRateLimit();      // calls next() or short-circuits
app.UseAuthorization();
app.MapControllers();
```

### Pattern 11: Repository

Trigger: 'I want my domain to think in collections, not SQL.'

```csharp
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Order>> FindByStatusAsync(OrderStatus status);
    Task AddAsync(Order order);
}
// EF Core DbContext is already Repository + Unit of Work.
// Wrap only if you'll swap data stores or need clean DDD boundaries.
```

### Pattern 12: CQRS

Trigger: 'Read shape differs significantly from write shape.' Separate write model from read model.

```csharp
// WRITE side - normalized domain
public class Order { /* full entity with rules */ }

// READ side - denormalized for queries
public class OrderListView {
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } // denormalized
    public decimal Total { get; set; }
    public int LineCount { get; set; } // pre-computed
}
// Projection handler keeps read in sync with writes via events
```

### Pattern 13: Outbox

Trigger: 'Reliably publish events when business state changes.' Solves dual-write.

```csharp
// Same DB transaction writes BOTH order AND outbox
await using var tx = await _db.Database.BeginTransactionAsync();
_db.Orders.Add(order);
_db.OutboxMessages.Add(new OutboxMessage {
    Id = Guid.NewGuid(),
    MessageType = nameof(OrderPlacedEvent),
    Payload = JsonSerializer.Serialize(new OrderPlacedEvent(order.Id)),
    CreatedAt = DateTime.UtcNow
});
await _db.SaveChangesAsync();
await tx.CommitAsync();
// Background service polls outbox and publishes to Service Bus
```

### Pattern 14: Saga

Trigger: 'Long-running business transaction across multiple services.'

```csharp
public async Task<bool> ExecuteOrderSagaAsync(OrderContext ctx) {
    Guid? paymentId = null;
    Guid? reservationId = null;
    try {
        paymentId = await _payment.ChargeAsync(ctx.Total);
        reservationId = await _inventory.ReserveAsync(ctx.Items);
        await _shipping.ScheduleAsync(ctx.Address);
        return true;
    } catch {
        // Compensate in REVERSE order
        if (reservationId.HasValue) await _inventory.ReleaseAsync(reservationId.Value);
        if (paymentId.HasValue) await _payment.RefundAsync(paymentId.Value);
        return false;
    }
}
```

### Pattern 15: Strangler Fig (THE Migration Pattern)

Trigger: 'Migrate legacy system without big-bang risk.' The dominant migration pattern in modern enterprises.

> Phase 1: Facade in front of legacy. No functional change.
>
> Phase 2: New UI alongside, routing through facade.
>
> Phase 3: Migrate ONE bounded context to new service.
>
> Phase 4-N: Migrate more contexts incrementally.
>
> Phase Final: Legacy decommissioned.
>
> Key: each migration is small, reversible, validated in prod.

### SOLID Principles Reminder

**S — SRP:** Single Responsibility. One reason to change.

**O — OCP:** Open for extension, closed for modification. New behavior via polymorphism.

**L — LSP:** Subtypes must be substitutable for base types. Square shouldn't extend Rectangle.

**I — ISP:** Interface Segregation. Many small interfaces beat one god interface.

**D — DIP:** Depend on abstractions, not concretes. The most important — enables DI, testing, swappable infrastructure.

## Section 9 — Memory & Garbage Collection (Architect Depth)

Architects must understand how .NET allocates and reclaims memory. This separates senior engineers from architects. Every interview at this level touches on GC, stack vs heap, allocation patterns, and how to design for low memory pressure.

### 9.1 — Process Memory Model

**Q1. What's in a .NET process's memory space?**

**A.** A .NET process has several distinct memory regions:

| Region | What lives there |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------- |
| Stack | Method call frames, local variables, value types. Grows/shrinks as methods enter/exit. ~1 MB per thread by default. |
| Small Object Heap (SOH) | Reference types and boxed values under 85 KB. Generations 0, 1, 2. |
| Large Object Heap (LOH) | Objects 85 KB and larger. Allocated directly in Gen 2. |
| Pinned Object Heap (POH) | .NET 5+. Objects pinned for native interop. Avoids heap fragmentation. |
| Code (CLR managed code) | JIT-compiled methods, R2R native code. |
| Native heap | Used by Win32, COM, third-party native libraries via P/Invoke. |

**Q2. What's the difference between stack and heap?**

**A.** Two fundamentally different allocators:

| Aspect | Stack vs Heap |
| -------------- | -------------------------------------------------------------------------------- |
| Allocation | Stack: bump pointer, O(1). Heap: allocator finds free slot, may trigger GC. |
| Deallocation | Stack: pointer rewinds on return. Heap: GC reclaims when no references. |
| Size | Stack: ~1MB per thread (fixed). Heap: gigabytes (grows on demand). |
| What goes here | Stack: locals, value types, method frames. Heap: reference types, large objects. |
| Lifetime | Stack: lexical scope. Heap: until GC reclaims. |
| Thread safety | Stack: per-thread (no contention). Heap: shared (contention possible). |
| Performance | Stack: nanoseconds. Heap: depends — usually fast, sometimes triggers GC. |

**Q3. Walk me through what happens when I declare `int x = 5`.**

**A.** Depends WHERE it's declared:

```csharp
// 1. Local variable in method - ON STACK
void Method() {
    int x = 5; // 4 bytes pushed to current stack frame
    // When method returns, stack pointer rewinds — x gone instantly
}

// 2. Field in a reference type - ON HEAP (inside the object)
class MyClass {
    int x = 5; // 4 bytes inside the heap-allocated MyClass instance
}

// 3. Field in a value type - depends on where the struct lives
struct MyStruct {
    int x; // Lives wherever MyStruct lives (stack or heap)
}

// 4. Boxed - on HEAP (with extra header overhead)
object boxed = 5; // Heap allocation: 4 bytes int + 16 bytes object header on x64
```

Key insight: 'value type' doesn't mean 'on stack' — it means 'value semantics.' A value type field inside a class lives on the heap with that class.

**Q4. What's an object header in .NET?**

**A.** Every reference-typed object has overhead beyond its fields:

| Field | Size (x64) |
| ------------------------- | ------------------------------------------------ |
| Sync block index | 8 bytes — used for locks, GetHashCode, etc. |
| Method table pointer (MT) | 8 bytes — points to type metadata, methods, etc. |
| Object fields | Sum of field sizes (aligned) |
| Padding | 0-7 bytes for 8-byte alignment |
| Total minimum | 24 bytes (header + minimal padding) |

Implication: an `object` with NO fields still costs 24 bytes. Creating millions of small objects adds up. This is WHY value types matter for hot paths.

**Q5. What's boxing and why is it expensive?**

**A.** Converting a value type to System.Object (or interface) requires heap allocation:

```csharp
// Boxing - hidden heap allocation
int x = 5;
object obj = x; // BOXING: allocates 24+ bytes on heap, copies value

// Unboxing - cast back
int y = (int)obj; // UNBOXING: copies value off heap

// Hidden boxing - ArrayList.Add(int)
var list = new ArrayList();
list.Add(5); // Boxes 5 into object. Use List<int> instead — no boxing.

// Hidden boxing - interface implementation on struct
interface IFormatter { string Format(); }
struct MyStruct : IFormatter {
    public string Format() => "...";
}
void DoWork(IFormatter f) { f.Format(); }
DoWork(new MyStruct()); // BOXES MyStruct to IFormatter

// FIX: generic constraint avoids boxing
void DoWork<T>(T f) where T : IFormatter { f.Format(); }
DoWork(new MyStruct()); // No boxing — T is constrained to struct
```

Performance impact: boxing in a tight loop can produce millions of Gen 0 allocations, killing throughput.

### 9.2 — Garbage Collection Deep

**Q6. How does the .NET GC work at a high level?**

**A.** Tracing generational GC. Steps:

**1. Mark:** Start from roots (statics, local variables on stack, registers, GC handles). Walk the object graph. Mark every reachable object.

**2. Sweep / Compact:** Unmarked objects are dead. Reclaim their memory. SOH gets compacted (objects moved together) to eliminate fragmentation.

**3. Update references:** After compaction, references that pointed to moved objects get updated.

The CLR pauses execution during certain GC phases (Stop The World). Workstation GC pauses more; Server GC parallelizes across cores.

**Q7. Explain the generational hypothesis.**

**A.** Empirical observation that drives the GC design: most objects die young. Few survive long. Therefore, focus collection effort on the young objects.

| Generation | Behavior |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Gen 0 | Newest. Smallest (a few MB). Collected most often. Cheapest collection (microseconds). Most objects die here. |
| Gen 1 | Survived one Gen 0 collection. Buffer between short-lived and long-lived. |
| Gen 2 | Survived Gen 1. Long-lived objects (caches, statics, singletons). Most expensive collection (milliseconds-seconds for large heap). |
| LOH | Large Object Heap. Objects >= 85KB. Collected ONLY with Gen 2. NOT compacted by default (fragmentation risk). |
| POH | Pinned Object Heap. .NET 5+. For pinned objects. Avoids fragmenting SOH. |

**Q8. What triggers a GC?**

**A.** Three primary triggers:

- Gen 0 allocation budget exceeded. Most common trigger. Allocates ~256KB-1MB, then collects.
- System memory pressure. OS signals low memory.
- Explicit GC.Collect() call. Almost never use this. Profilers and tests, not production.
- AppDomain unload or process exit.

After Gen 0 collection, if too many objects survived, Gen 1 is collected. If Gen 1 is full, Gen 2 is collected. This is the 'cascade' — collecting a higher generation always collects all lower ones.

**Q9. Workstation GC vs Server GC — what's the difference?**

**A.** Two GC modes optimized for different scenarios:

| Aspect | Workstation vs Server |
| ------------- | ----------------------------------------------------------------------------------- |
| Threading | Workstation: GC on same thread as user code. Server: dedicated GC threads per core. |
| Heap layout | Workstation: one heap. Server: one heap per processor (parallelized). |
| Pause time | Workstation: shorter pauses. Server: shorter pauses on multi-core. |
| Throughput | Workstation: lower. Server: much higher. |
| Memory use | Workstation: lower. Server: higher (multiple heaps). |
| When to use | Workstation: desktop apps, single-user. Server: ASP.NET, multi-core servers. |
| Configuration | `<gcServer enabled="true"/>` in app config. Or env var: DOTNET_gcServer=1. |

ASP.NET Core defaults to Server GC on multi-core machines. App Service + AKS use Server GC automatically.

**Q10. What's Background GC?**

**A.** Concurrent variant of GC. Most Gen 2 collection happens in the background while user code continues. Only brief pauses to mark roots and finalize.

Both Workstation and Server GC support Background GC. It's on by default since .NET 4.5+. Reduces pause times significantly. Trade-off: slightly higher CPU.

**Q11. What's a GC root?**

**A.** Starting point for GC's reachability analysis. Objects reachable from any root are 'live.' Types of roots:

- Static fields (class statics).
- Local variables in any thread's stack frame.
- CPU registers holding references.
- GC handles (objects pinned via GCHandle, weak references).
- Finalization queue (objects awaiting Finalize).

Anything NOT reachable from a root is garbage. The Mark phase walks the graph FROM roots.

**Q12. How can I check what's keeping an object alive?**

**A.** Tools that show the path from root to object:

- Visual Studio: Memory Usage in Diagnostic Tools.
- dotMemory (JetBrains): excellent GC analysis.
- PerfView (Microsoft): free, deep ETW traces.
- WinDbg + SOS: low-level. !gcroot command shows path from roots.
- dotnet-dump + dotnet-gcdump: capture and analyze offline.

**Q13. What's the difference between Mark-Sweep and Mark-Sweep-Compact?**

**A.** Mark-Sweep just frees memory without moving objects. Causes fragmentation — holes between live objects. Mark-Sweep-Compact moves live objects together, eliminating fragmentation.

.NET SOH is Mark-Sweep-Compact. LOH is Mark-Sweep (compaction is optional via GCSettings.LargeObjectHeapCompactionMode). Compaction is expensive — only triggered when fragmentation is severe.

**Q14. What's a card table?**

**A.** Optimization for generational GC. To collect Gen 0/1, GC needs to find roots in Gen 2 that point to Gen 0/1 objects.

Naive approach: scan entire Gen 2 heap (slow). Card table approach: mark 'cards' (chunks of Gen 2) that contain references to younger generations. On Gen 0/1 collection, only scan dirty cards.

When you write a reference to a Gen 0/1 object into a Gen 2 object's field, the CLR marks the corresponding card dirty. This 'write barrier' is why object writes are slightly slower than primitive writes.

**Q15. What's the LOH and what are its quirks?**

**A.** Large Object Heap. Objects 85,000 bytes and larger.

- Allocated directly in Gen 2 — collected only with full GC.
- NOT compacted by default — risk of fragmentation.
- Common LOH objects: large arrays, big strings, large buffers.
- LOH fragmentation causes OutOfMemoryException even with free memory.
- Force LOH compaction once if needed: GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce.

Best practice: avoid frequent large allocations. Pool large buffers (ArrayPool\<T\>). Use Span\<T\> to slice rather than copy.

**Q16. What is a finalizer and when should I use one?**

**A.** Finalizer (~MyClass()) runs before object is reclaimed. Used to release unmanaged resources (file handles, native memory).

```csharp
public class FileReader : IDisposable
{
    private IntPtr _handle; // native handle
    private bool _disposed;

    public FileReader(string path) { _handle = NativeOpen(path); }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // tell GC: don't run finalizer
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) { /* dispose managed resources */ }
        if (_handle != IntPtr.Zero) NativeClose(_handle);
        _disposed = true;
    }

    ~FileReader() => Dispose(false); // finalizer
}
```

Finalizers are expensive — they push objects to Gen 2 (Finalization Queue then F-Reachable Queue). Avoid unless absolutely needed for unmanaged cleanup. Always pair with IDisposable + SuppressFinalize for performance.

**Q17. What's the 'managed memory leak' pattern?**

**A.** In .NET, 'leak' means holding references you no longer need, preventing GC from reclaiming objects. Common causes:

- Static collections that grow unbounded (caches without TTL or size limit).
- Event handlers not unsubscribed. Publisher holds subscriber alive — common in WPF/WinForms.
- Captured variables in lambdas. The lambda holds the entire enclosing scope alive.
- Long-lived DbContext. Tracks all entities loaded.
- Singleton holding short-lived data.
- Async/await + cancellation: cancelled tasks holding state.

```csharp
// CLASSIC LEAK: event handler
publisher.SomethingHappened += handler;
// publisher now holds reference to whatever 'handler' captured
// If you forget to unsubscribe and publisher is long-lived, leak.

// FIX: always unsubscribe, or use weak event pattern
publisher.SomethingHappened -= handler;
```

**Q18. How do you tune GC for a high-throughput server?**

**A.** Configuration choices that impact GC behavior:

```xml
<!-- In runtimeconfig.json or csproj -->
<PropertyGroup>
    <ServerGarbageCollection>true</ServerGarbageCollection>
    <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
    <RetainVMGarbageCollection>true</RetainVMGarbageCollection>
    <TieredCompilation>true</TieredCompilation>
</PropertyGroup>
```

```text
// Or environment variables (Docker / Kubernetes friendly)
DOTNET_gcServer=1
DOTNET_gcConcurrent=1
DOTNET_GCHeapCount=8        // # of heaps for Server GC
DOTNET_GCHeapHardLimit=...  // hard memory cap
```

**Q19. What's allocation-free code and why does it matter?**

**A.** Code that produces zero or near-zero heap allocations in its hot path. Critical for: serializers, web frameworks, game loops, anything called millions of times per second.

Techniques:

- Use Span\<T\> / Memory\<T\> for slicing without allocation.
- Pool objects with ArrayPool\<T\> or ObjectPool\<T\>.
- Use struct for small short-lived data (avoid boxing).
- Use ValueTask instead of Task when result is often available synchronously.
- Avoid LINQ in hot paths (LINQ creates iterators).
- Avoid string concatenation in loops — use StringBuilder or string.Create.

Modern .NET (System.Text.Json, ASP.NET Core, Kestrel) heavily uses these techniques.

**Q20. What is ArrayPool\<T\> and when do you use it?**

**A.** Pool of reusable arrays. Reduces GC pressure in scenarios that allocate temporary buffers.

```csharp
// Without pooling - allocates a 1MB array every call
public async Task<int> ReadStreamAsync(Stream s) {
    var buffer = new byte[1024 * 1024]; // GC pressure
    return await s.ReadAsync(buffer);
}

// With pooling - reuses buffers
public async Task<int> ReadStreamAsync(Stream s) {
    byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
    try {
        return await s.ReadAsync(buffer);
    } finally {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
// Caveat: buffer returned may be LARGER than requested
// Always pass the actual count to readers/writers
// Caveat: returned buffers may contain old data — clear if sensitive
```

**Q21. How do I measure GC behavior in production?**

**A.** Metrics that matter:

- GC Gen 0/1/2 collections per minute.
- % time in GC — should be under 5% for healthy apps.
- Gen 2 collection frequency — high means long-lived objects, possible leak.
- Allocation rate (MB/sec).
- LOH allocation rate.
- Average pause time.

Tools: dotnet-counters (real-time), Application Insights performance counters, Prometheus + OpenTelemetry runtime metrics.

**Q22. What's the difference between Tier 0 and Tier 1 JIT?**

**A.** Tiered Compilation, on by default since .NET Core 3.0:

**Tier 0:** Quick JIT. Generates unoptimized code fast. Used on first invocation to reduce startup time.

**Tier 1:** Optimizing JIT. Replaces hot methods (after N invocations) with optimized code. Slower compilation, faster runtime.

Why it matters: cold startup is faster, but first few invocations of a method are slower than steady-state. Affects benchmarks (warmup loops matter).

**Q23. What is AOT compilation?**

**A.** Ahead-Of-Time compilation. Generates native machine code at build time, instead of JIT at runtime.

- Native AOT (.NET 7+): full AOT, single self-contained native binary. No JIT, faster startup, smaller memory, lower steady-state perf.
- ReadyToRun (R2R): partial AOT. Pre-compiles to native, JIT can still recompile hot paths.
- Trade-offs: AOT has size limits, can't use Reflection.Emit, some features (dynamic) blocked.
- Use cases: serverless (cold start), CLI tools, containers, embedded.

**Q24. How would you debug an out-of-memory exception in production?**

**A.** Step-by-step approach:

- Capture dump: dotnet-dump collect -p \<pid\>.
- Analyze with dotnet-dump analyze or Visual Studio.
- Run !dumpheap -stat to see object types by count and total size.
- Sort by total size, find the biggest contributors.
- For the top type, !dumpheap -mt \<method-table\> to see instances.
- For a sample instance, !gcroot \<addr\> shows what's keeping it alive.
- Look for: large collections (Dictionary, List with many entries), strings, byte arrays.
- Common culprits: unbounded caches, event handler leaks, holding DbContext too long.

### 9.3 — Common Architect-Level Memory Decisions

**Q25. When should I use struct vs class?**

**A.** Decision matrix:

| Use struct when | Use class when |
| ----------------------------------------- | -------------------------------- |
| Small (< 16-24 bytes) | Larger sizes |
| Value semantics make sense (Point, Money) | Identity semantics (User, Order) |
| Immutable | Mutable state |
| Short-lived, in hot paths | Long-lived |
| Want to avoid heap allocation | Need inheritance |
| Examples: int, DateTime, Guid, Span\<T\> | Examples: most domain entities |

Rule of thumb: default to class. Use struct only when you have a specific performance reason and the type is small + immutable.

**Q26. When does struct allocation NOT go on the stack?**

**A.** Common misconception. Structs allocate WHEREVER they're declared:

- Local variable in method: STACK.
- Field of a class: HEAP (inside the class instance).
- Element of array: HEAP (inside the array).
- Captured by lambda or async state machine: HEAP (in the closure object).
- Boxed (assigned to object or interface): HEAP.

Struct is about VALUE SEMANTICS, not location.

**Q27. What's a ref struct and when do I use one?**

**A.** Special struct that MUST live on the stack. Cannot be heap-allocated. Cannot be boxed, captured by lambda, or used as a class field.

Use cases: Span\<T\>, ReadOnlySpan\<T\>. These wrap stack memory or pinned heap memory and would be unsafe if they outlived the stack frame.

```csharp
ref struct MyBuffer { /* must be stack-only */ }

// CAN be local
void Method() { MyBuffer b = ...; }

// CANNOT be a class field
class Bad { MyBuffer _field; } // compile error

// CANNOT be captured
void Method() {
    MyBuffer b = ...;
    Action a = () => Use(b); // compile error
}
```

**Q28. What's the cost of an empty class in .NET?**

**A.** On 64-bit:

- 8 bytes sync block index.
- 8 bytes method table pointer.
- Minimum object size: 24 bytes (after padding).

Add fields, each takes its size (4 for int, 8 for long/reference, etc.), aligned to 8 bytes.

Implication: a List\<string\> with 1M items uses ~8MB just for the references (8 bytes each), PLUS the string objects themselves. Memory adds up fast at scale.

**Q29. What's the difference between heap fragmentation and external fragmentation?**

**A.** Both reduce usable memory:

**Heap fragmentation (internal):** Free space exists but in small, non-contiguous chunks. Can't allocate larger objects even with total free memory available.

**External fragmentation:** Virtual address space fragmentation. Can run out of contiguous address space.

SOH compaction prevents most fragmentation. LOH doesn't compact by default — long-running servers with mixed-size allocations can fragment LOH. Symptom: OutOfMemory despite GC showing free memory.

**Q30. How do I architect a system to minimize GC pressure?**

**A.** Patterns and principles:

- Pool large buffers (ArrayPool\<T\>, ObjectPool\<T\>).
- Reuse buffers across requests — don't allocate per-request.
- Avoid LINQ in hot paths — use explicit loops or for-loops.
- Use Span\<T\> / Memory\<T\> instead of substring/Slice copies.
- Use struct for small short-lived data.
- Cache reflection results (Type.GetMethods is expensive).
- Minimize boxing — use generics, avoid object.
- Use ValueTask for hot async paths with frequent sync completion.
- Bound your caches — unbounded growth = Gen 2 nightmare.
- Profile, don't guess. Measure allocations with BenchmarkDotNet.

## Section 10 — Data Structures in C# (Architect Depth)

Architects must know which data structure to pick for which workload, what's happening under the hood, and the memory and time costs.

### 10.1 — Big-O Refresher

**Q1. What's Big-O notation and what should I memorize?**

**A.** Big-O describes how an algorithm's resource usage grows with input size. Memorize these:

| Complexity | Meaning / Example |
| -------------- | ------------------------------------------------------------------------- |
| O(1) | Constant. Hash table lookup, array index access. |
| O(log n) | Logarithmic. Binary search, balanced tree operations. |
| O(n) | Linear. Iterating a list, linear search. |
| O(n log n) | Log-linear. Best comparison-based sorting (mergesort, heapsort). |
| O(n²) | Quadratic. Nested loops over n. Bubble sort. Acceptable for small n only. |
| O(2^n) | Exponential. Recursive Fibonacci without memoization. Avoid. |
| O(n!) | Factorial. Permutations. Only for very small n. |

Architect rule: know the dominant operation in your workload, pick the data structure that gives O(1) or O(log n) for it.

### 10.2 — Arrays and Spans

**Q2. How does an array work in memory?**

**A.** Contiguous block of memory. The fastest data structure when you know what you need.

```csharp
int[] nums = new int[1000]; // 4000 bytes (plus array header overhead)
// Array header on x64 (32 bytes total before data):
// - Sync block index (8)
// - Method table pointer (8)
// - Length (8)
// - Bounds info (8) for multi-dim
// Element access: O(1)
// Memory address = base + (index * elementSize)
// Cache-friendly: contiguous memory hits CPU cache prefetch
// 64-byte cache line holds 16 ints
```

**Q3. Array vs List\<T\>?**

**A.** List\<T\> is a resizable wrapper around an array (T[]). Internal array doubles when full.

| Operation | Array vs List\<T\> |
| --------------- | ---------------------------------------------------------------------- |
| Indexing | Array: O(1) direct. List: O(1) through indexer. |
| Add to end | Array: not supported (fixed). List: O(1) amortized. |
| Insert middle | Array: N/A. List: O(n) — shifts elements. |
| Memory overhead | Array: minimal. List: capacity may exceed count (up to 2x). |
| When to use | Array: known fixed size, max perf. List: dynamic size, default choice. |

**Q4. What's List\<T\>.Capacity vs Count?**

**A.** Count: number of items in the list. Capacity: size of the internal array (may be larger).

```csharp
var list = new List<int>(); // Count=0, Capacity=0
list.Add(1); // Count=1, Capacity=4
list.Add(2); // Count=2, Capacity=4
// ... Adding 5th item:
list.Add(5); // Count=5, Capacity=8 (doubled)

// Avoid unnecessary resizes - allocate with known capacity:
var list = new List<int>(1000); // Capacity=1000, Count=0

// Or shrink after population:
list.TrimExcess(); // Cap = Count, returns memory
```

**Q5. When should I use Span\<T\> vs Array?**

**A.** Span\<T\> is a view over memory, not its own copy. Use for slicing/parsing without allocation.

```csharp
// Old way - allocates substring
string s = "Hello, World";
string hello = s.Substring(0, 5); // new allocation

// Span way - zero allocation
ReadOnlySpan<char> hello = s.AsSpan(0, 5);

// Span on stack-allocated memory
Span<int> buffer = stackalloc int[100];
// No heap allocation, freed on method return

// Span on array
int[] arr = new int[1000];
Span<int> slice = arr.AsSpan(100, 200); // view, no copy
```

Limitations: Span\<T\> is ref struct — can't be field, can't be in async method, can't be captured by lambda. For async, use Memory\<T\> (heap-backed).

### 10.3 — Dictionary and HashSet

**Q6. How does Dictionary\<TKey, TValue\> work internally?**

**A.** Hash table with separate chaining. Key insights:

- Internal array of buckets. Index = hash(key) % bucketCount.
- Multiple keys can hash to same bucket — chained via linked list of entries.
- Resize when load factor exceeds threshold (~1.0). Doubles bucket count, rehashes everything.
- Lookup O(1) average, O(n) worst case (all keys collide).

Modern .NET Dictionary is highly optimized — minimal overhead, careful prime-sized bucket counts for good distribution.

**Q7. Why must my custom keys override Equals AND GetHashCode?**

**A.** Dictionary uses both:

- GetHashCode determines which bucket to check.
- Equals determines if the entry in that bucket matches.

Rules:

- Equal objects MUST have equal hash codes.
- Hash code SHOULD be reasonably distributed (avoid all-zero or all-same).
- Hash code MUST be stable for the object's lifetime in the collection.

```csharp
public class CustomerId : IEquatable<CustomerId>
{
    public string Region { get; }
    public int Number { get; }

    public CustomerId(string region, int number)
    {
        Region = region;
        Number = number;
    }

    public bool Equals(CustomerId? other) =>
        other is not null && Region == other.Region && Number == other.Number;

    public override bool Equals(object? obj) => obj is CustomerId c && Equals(c);

    // C# 7+: use HashCode.Combine
    public override int GetHashCode() => HashCode.Combine(Region, Number);
}
```

**Q8. Dictionary vs HashSet?**

**A.** HashSet\<T\> is a set — collection of unique items, no values. Dictionary\<TKey,TValue\> is a map — keys to values.

Both have O(1) average add/contains/remove. Same hash table internals.

Use HashSet when you only need 'is this present' or set operations (Union, Intersect, Except).

**Q9. What's the load factor and why does it matter?**

**A.** Ratio of entries to buckets. Higher = more collisions = slower.

.NET Dictionary resizes (doubles buckets) when load factor reaches ~1.0. Trade-off: lower load = less collision but more memory.

Pre-size dictionaries when you know approximate capacity:

```csharp
var dict = new Dictionary<string, int>(capacity: 10000);
// Avoids multiple resize+rehash operations during fill
```

**Q10. When should I use ConcurrentDictionary?**

**A.** When multiple threads read AND write the same dictionary. Standard Dictionary is NOT thread-safe — concurrent modifications can corrupt state.

ConcurrentDictionary uses fine-grained locking (per bucket) for high concurrency. Slower than Dictionary in single-threaded scenarios. Use when actually concurrent.

```csharp
private readonly ConcurrentDictionary<string, int> _counts = new();

// Atomic operations
_counts.AddOrUpdate(key, 1, (_, current) => current + 1);
var connection = _connections.GetOrAdd(name, _ => new Connection(name));
_counts.TryRemove(key, out var oldValue);

// Iteration is safe but reads a snapshot — may include or miss
// concurrent changes. Don't depend on consistency during iteration.
```

### 10.4 — Queue, Stack, LinkedList

**Q11. Queue\<T\> vs Stack\<T\>?**

**A.** Queue: FIFO (first in, first out). Stack: LIFO (last in, first out).

Both internally use arrays with circular buffer (Queue) or simple list (Stack). O(1) for primary operations (Enqueue/Dequeue, Push/Pop).

```csharp
// Queue - process in order
var queue = new Queue<Order>();
queue.Enqueue(order1);
queue.Enqueue(order2);
var first = queue.Dequeue(); // order1

// Stack - reverse order processing
var stack = new Stack<int>();
stack.Push(1); stack.Push(2);
var top = stack.Pop(); // 2 (last in)
```

**Q12. When use LinkedList\<T\> instead of List\<T\>?**

**A.** Almost never. LinkedList has higher overhead per element (each node is a heap allocation with prev/next pointers), worse cache locality, and the O(n) traversal still beats most use cases.

LinkedList shines ONLY when you have a reference to a node and want O(1) insert/remove at that position. In practice, that's rare.

| Operation | List\<T\> vs LinkedList\<T\> |
| ----------------------------- | ---------------------------------------------------------- |
| Index access | List: O(1). LinkedList: O(n) — must walk. |
| Add to end | List: O(1) amortized. LinkedList: O(1). |
| Insert middle (with node ref) | List: O(n). LinkedList: O(1). |
| Memory per element | List: T size only. LinkedList: T + 24 bytes node overhead. |
| Cache locality | List: excellent. LinkedList: poor. |

**Q13. What's PriorityQueue\<TElement, TPriority\>?**

**A.** Added in .NET 6. Heap-based priority queue. Items dequeue in priority order.

```csharp
var pq = new PriorityQueue<string, int>();
pq.Enqueue("high", 1); // 1 = highest priority
pq.Enqueue("low", 5);
pq.Enqueue("medium", 3);
while (pq.Count > 0) {
    Console.WriteLine(pq.Dequeue());
}
// Output: high, medium, low
// O(log n) for Enqueue and Dequeue
// Use for: top-K problems, task scheduling, Dijkstra's algorithm
```

### 10.5 — Sorted Collections

**Q14. What's SortedDictionary vs SortedList?**

**A.** Both maintain sorted order by key. Different internals:

| Aspect | SortedDictionary vs SortedList |
| ------------------ | -------------------------------------------------------------------------------- |
| Internal structure | SortedDictionary: red-black tree. SortedList: parallel arrays of keys + values. |
| Lookup | Both O(log n). |
| Insert/Remove | SortedDictionary: O(log n). SortedList: O(n) — shifts. |
| Memory | SortedDictionary: higher (tree nodes). SortedList: lower (arrays). |
| When to use | SortedDictionary: frequent inserts. SortedList: mostly read, infrequent inserts. |

**Q15. What's SortedSet\<T\>?**

**A.** Set with elements in sorted order. Red-black tree internally. O(log n) for add/contains/remove.

Use when you need: ordered iteration, range queries (GetViewBetween), set operations with order preserved.

### 10.6 — Concurrent Collections

**Q16. Walk through the concurrent collections.**

**A.** Thread-safe alternatives to standard collections:

| Collection | Use case |
| --------------------------- | ---------------------------------------------------------- |
| ConcurrentDictionary\<K,V\> | Thread-safe dictionary. Most-used concurrent collection. |
| ConcurrentQueue\<T\> | Lock-free FIFO. Producer-consumer scenarios. |
| ConcurrentStack\<T\> | Lock-free LIFO. |
| ConcurrentBag\<T\> | Unordered. Optimized for same-thread add+remove. |
| BlockingCollection\<T\> | Bounded. Producer blocks when full. Legacy — use Channels. |
| Channel\<T\> | Modern producer-consumer with async support. Use this. |

**Q17. Channel\<T\> vs BlockingCollection\<T\>?**

**A.** Channel\<T\> is the modern choice (since .NET Core 3). Async-friendly, more flexible.

```csharp
// Bounded channel - producer waits when buffer full
var channel = Channel.CreateBounded<Order>(new BoundedChannelOptions(1000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = false,
    SingleWriter = false
});

// Producer
_ = Task.Run(async () => {
    foreach (var order in orderSource)
        await channel.Writer.WriteAsync(order);
    channel.Writer.Complete();
});

// Consumer
await foreach (var order in channel.Reader.ReadAllAsync(ct))
    await ProcessAsync(order);
```

### 10.7 — Trees and Graphs

**Q18. Does .NET have built-in tree data structures?**

**A.** Not as first-class types. Use:

- SortedDictionary / SortedSet — internally red-black tree, but exposes dictionary interface.
- Implement your own for specific algorithms (B-tree, trie, etc.).
- For XML/JSON trees: System.Xml.Linq, System.Text.Json.JsonDocument.

Architect insight: most production data 'trees' are actually graphs stored in databases (parent_id columns, graph DBs). Don't reinvent.

**Q19. How would you represent a graph in C#?**

**A.** Two main representations:

**Adjacency list (common):** Dictionary\<Node, List\<Node\>\>. Memory O(V+E). Good for sparse graphs.

**Adjacency matrix:** 2D array bool[V,V]. Memory O(V²). Good for dense graphs, O(1) edge check.

```csharp
// Adjacency list
var graph = new Dictionary<string, List<string>>
{
    ["A"] = new() { "B", "C" },
    ["B"] = new() { "D" },
    ["C"] = new() { "D", "E" }
};

// BFS traversal
var visited = new HashSet<string>();
var queue = new Queue<string>();
queue.Enqueue("A");
while (queue.Count > 0)
{
    var node = queue.Dequeue();
    if (!visited.Add(node)) continue;
    Console.WriteLine(node);
    foreach (var neighbor in graph[node])
        if (!visited.Contains(neighbor))
            queue.Enqueue(neighbor);
}
```

### 10.8 — Specialized Structures

**Q20. What's an ImmutableList\<T\> and when do I use one?**

**A.** Immutable collection — operations return new collection instead of mutating. Thread-safe by definition.

```csharp
var list = ImmutableList<int>.Empty;
var list2 = list.Add(1); // new list, original unchanged
var list3 = list2.Add(2);
// Internal: tree-based structure. O(log n) add/remove.
// Each operation shares unchanged parts with parent (structural sharing).
// Use for:
// - Functional/event-sourcing code
// - Sharing state across threads safely
// - Snapshot scenarios
```

Trade-off: O(log n) vs O(1) for mutable list. Memory may be higher. Use when immutability gives clarity (concurrent code, audit trails).

**Q21. What's FrozenDictionary / FrozenSet (.NET 8+)?**

**A.** Read-optimized immutable collections. Slower to build, but faster lookups than Dictionary/HashSet.

Use when you build a lookup once at startup and read millions of times after. Static reference data, lookup tables, routing maps.

```csharp
// Build once
var statusCodes = new Dictionary<int, string>
{
    [200] = "OK", [404] = "Not Found", [500] = "Server Error"
    // ... many more
}.ToFrozenDictionary();

// Read many times - faster than Dictionary
var msg = statusCodes[404];
```

**Q22. What's MemoryCache vs IDistributedCache?**

**A.** Two cache abstractions in .NET:

| Aspect | MemoryCache vs IDistributedCache |
| ----------- | ---------------------------------------------------------------------------------------------- |
| Location | MemoryCache: in-process. Distributed: external (Redis, SQL). |
| Speed | MemoryCache: nanoseconds. Distributed: 1-5ms (network). |
| Scope | MemoryCache: per app instance. Distributed: shared across instances. |
| Persistence | MemoryCache: lost on restart. Distributed: persists. |
| When to use | MemoryCache: hot reads with per-instance scope OK. Distributed: shared state, session storage. |
| Capacity | MemoryCache: bound by RAM. Distributed: bound by external store. |

Common pattern: two-tier cache. MemoryCache (L1) wraps IDistributedCache (L2). Check L1 first, fall to L2, fall to DB.

**Q23. What's a Bloom Filter and when would I use one?**

**A.** Probabilistic data structure that tests set membership. Says 'definitely not in set' or 'probably in set' — false positives possible, never false negatives.

Memory-efficient: maybe 10 bits per element regardless of element size. Hash-based.

Use cases: 'before expensive lookup, is this even worth checking?' Examples:

- Cache: 'is this URL cached?' — check Bloom first, only hit Redis on probable hit.
- Database: 'is this username taken?' — check Bloom in memory before DB query.
- Web crawling: 'have we seen this URL?' — Bloom prevents duplicate crawls.

Trade-off: false positives mean some wasted lookups. Tune size based on acceptable rate.

### 10.9 — Choosing the Right Structure

**Q24. How do I pick the right collection?**

**A.** Decision flowchart:

```text
Need fast lookup by key?
  -> Dictionary<TKey, TValue>
     -> Concurrent? ConcurrentDictionary
     -> Read-only after build? FrozenDictionary
     -> Sorted iteration? SortedDictionary

Need ordered sequence?
  -> List<T> (default — dynamic array)
  -> Array (known size, max perf)
  -> Need front/back ops? LinkedList (rare)

Need FIFO?
  -> Queue<T>
     -> Concurrent? ConcurrentQueue<T>
     -> Async producer/consumer? Channel<T>

Need LIFO?
  -> Stack<T>

Need uniqueness only?
  -> HashSet<T>
     -> Sorted iteration? SortedSet<T>

Need priority order?
  -> PriorityQueue<TElement, TPriority>

Need thread-safe with copy semantics?
  -> ImmutableList<T> et al
```

**Q25. What's an architect-level take on data structure choice in microservices?**

**A.** Most service-level data lives in databases — the data structure choice is at a different layer:

- SQL: relational tables. Choose indexes wisely (B-tree usually).
- Cosmos: document with partition. Index choice matters less but partition key is everything.
- Redis: native types (string, hash, list, set, sorted set). Pick by access pattern.
- Search: inverted index (Azure AI Search, Elasticsearch).

In-memory C# collections are for per-request work, caching layers, and orchestration. The 'main' data structure choice is the database one.

## Section 11 — SQL Deep (Architect Depth)

Architects must understand HOW the database engine executes queries, not just SQL syntax. Indexes, locking, execution plans, isolation, transaction internals — these are interview gold and production essentials.

### 11.1 — How a Query Actually Runs

**Q1. Walk me through how SQL Server processes a query.**

**A.** Eight steps from SQL text to result:

```text
1. PARSE: SQL text -> syntax tree. Catches syntax errors.
2. BIND: Resolves names (tables, columns) against catalog. Type checking.
3. OPTIMIZE: Cost-based optimizer evaluates many plans, picks cheapest.
4. CACHE: Compiled plan stored in plan cache, keyed by query hash.
5. EXECUTE: Executor reads pages from buffer pool or disk.
6. LOCKING: Acquires locks per isolation level.
7. RESULT: Rows streamed back to client.
8. CLEANUP: Locks released at transaction end.
```

**Q2. What's the buffer pool?**

**A.** SQL Server's in-memory cache of database pages (8KB units). Reads check buffer pool first; only go to disk on miss.

Hit ratio matters massively:

- 99%+ hit ratio: excellent. Memory-bound workload.
- 80-95%: tolerable. Some I/O.
- Below 80%: investigate. May need more RAM or query tuning.

SQL Server uses ALL available memory by default. This is correct behavior — unused memory is wasted memory.

**Q3. What's a page in SQL Server?**

**A.** 8 KB unit of storage. All reads and writes are page-aligned.

Implications:

- Reading one row reads the whole page (typically 8-100 rows depending on row size).
- Smaller rows = more rows per page = fewer page reads = faster.
- Wide tables with rarely-used columns waste page space. Consider splitting.
- Row size MUST fit in a page (8060 bytes max for inline data). Large text/blob go off-page.

**Q4. What's a page split and why is it bad?**

**A.** When inserting into a full page, SQL splits it: half stays, half moves to new page. Costly operation:

- Doubles I/O for the split.
- Fragments the index — non-contiguous pages.
- Increases page count and storage.

Mitigations:

- Use ever-increasing clustered index (identity, sequential GUID, timestamp). Inserts go to end.
- Avoid random clustered index (random GUID, hash). Splits everywhere.
- Set FILLFACTOR < 100 on volatile indexes — leaves space for inserts.

**Q5. What's the WAL (Write-Ahead Log) and why does it exist?**

**A.** Write-Ahead Logging principle: log records hit disk BEFORE data pages. SQL Server's transaction log:

- Every modification logged before the data change is durable.
- Commit returns only after log records on disk.
- Crash recovery: redo committed transactions, undo uncommitted ones from log.
- This is the 'D' (Durability) in ACID.

Log file performance is critical — keep it on fast storage, separate from data files.

### 11.2 — Indexes Deep

**Q6. How does a B-tree index work?**

**A.** Balanced tree structure. Three levels typical:

```text
[Root]  <- 1 page, in memory always
 / | \
[I] [I] [I]  <- Intermediate pages
 |   |   |
[L][L] [L] [L]  <- Leaf pages (sorted data or pointers)

Lookup: walks root -> intermediate -> leaf. O(log n).
For 1B rows with 100 keys per page: 5 levels (log_100(1B) ≈ 4.5).
5 page reads to find any row. Fast even at huge scale.
```

**Q7. Clustered vs Non-Clustered index — what's REALLY different?**

**A.** Clustered: leaf pages ARE the table data. Order of leaf pages = physical row order. One per table.

Non-clustered: leaf pages contain index key + pointer (RID or clustered key) to actual row. Many per table.

| Aspect | Clustered vs Non-Clustered |
| ----------------- | -------------------------------------------------------------------------------------- |
| Storage | Clustered: data IS the index. Non-clustered: separate from data. |
| Per table | Clustered: ONE max. Non-clustered: up to 999. |
| Range queries | Clustered: fast (data physically sorted). Non-clustered: needs lookup unless covering. |
| Insert cost | Clustered: moderate (may split pages). Non-clustered: every index updated on insert. |
| Pick clustered on | Most-queried key, narrow, immutable, ever-increasing. |
| Common choice | Identity or sequential ID, NOT a random GUID. |

**Q8. What's a covering index?**

**A.** Index that contains ALL columns a query needs. Engine serves from index alone — no row lookup.

```sql
-- Query
SELECT CustomerId, Total, Status FROM Orders WHERE CustomerId = @id;

-- Index that covers this query:
CREATE NONCLUSTERED INDEX IX_Orders_Customer
ON Orders (CustomerId) -- key (used in WHERE)
INCLUDE (Total, Status); -- additional cols in leaf

-- Result: no key lookup. All data from index leaf pages.
```

Massive perf win for hot queries. Trade-off: index storage size increases.

**Q9. What's a key lookup and why is it bad?**

**A.** When a non-clustered index doesn't cover the query, engine must lookup each matching row in the clustered index/heap to get other columns. One lookup per row matched.

Example: index on (CustomerId), query needs Total too. Index finds CustomerId=42, but Total isn't there. Engine looks up clustered row for each match. For 1000 matches = 1000 random I/O operations.

Fix: include the missing columns in the index (covering index), OR query only what's in the index.

**Q10. What makes a query sargable?**

**A.** 'Search ARGument-able' — can use an index. Non-sargable queries force scans even with indexes.

```sql
-- NON-SARGABLE (function on column)
WHERE YEAR(OrderDate) = 2026
WHERE UPPER(Name) = 'JOHN'
WHERE Name LIKE '%john%' -- leading wildcard
WHERE Total + Tax > 100 -- arithmetic on column
WHERE ISNULL(Email, '') = ''
WHERE CAST(OrderId AS varchar) = '42'

-- SARGABLE equivalents
WHERE OrderDate >= '2026-01-01' AND OrderDate < '2027-01-01'
WHERE Name = 'John' COLLATE Latin1_General_CI_AS
WHERE Name LIKE 'john%' -- trailing wildcard OK
WHERE Total > 100 - @tax
WHERE Email IS NULL OR Email = ''
WHERE OrderId = 42
```

**Q11. What's a filtered index?**

**A.** Index with a WHERE clause. Only includes rows matching the filter. Smaller, faster, more targeted.

```sql
-- Index only active orders (99% of queries are on active)
CREATE NONCLUSTERED INDEX IX_Orders_ActiveCustomer
ON Orders (CustomerId)
WHERE Status = 'Active';

-- Queries with matching WHERE benefit
SELECT * FROM Orders WHERE Status = 'Active' AND CustomerId = @id;
-- Index is smaller, scan less data, less storage
```

**Q12. When should I add an index?**

**A.** Signs you need one:

- Execution plan shows Index Scan or Table Scan on large table.
- High logical reads for small result set (>1000:1 ratio).
- Same WHERE pattern in multiple slow queries.
- Missing Index warnings in SSMS execution plan (take with skepticism — often suggests too-wide indexes).

Signs you have TOO MANY indexes:

- Insert/update performance degraded.
- sys.dm_db_index_usage_stats shows indexes with zero seeks/scans but many updates.
- Storage growing faster than data.

### 11.3 — Locking and Isolation

**Q13. What lock types does SQL Server use?**

**A.** Common locks:

| Lock | Compatibility |
| --------------------------- | ----------------------------------------------------------------------------------------------------------- |
| S (Shared) | Read lock. Multiple S allowed. Blocks X. |
| X (Exclusive) | Write lock. Blocks all others. |
| U (Update) | Update intent. Held while reading row to be updated. Converts to X on write. |
| IS, IX, IU (Intent) | Higher-level locks indicating intent on lower. Object level says: 'something inside is being read/written.' |
| Sch-S (Schema Stability) | Held during reads to prevent DDL. |
| Sch-M (Schema Modification) | Held during DDL. Blocks everything. |

**Q14. What lock granularity does SQL choose?**

**A.** Engine picks granularity to balance concurrency vs overhead:

- Row: fine grain. Best concurrency. Most overhead per lock.
- Page: page-level (8KB).
- Object: whole table.
- Database: rare. ALTER DATABASE etc.

SQL escalates locks when row/page count exceeds threshold (~5000). To avoid escalation: filter to fewer rows, or use hints.

**Q15. Walk me through the 4 standard isolation levels.**

**A.** From weakest to strongest:

| Level | Prevents / Allows |
| ------------------------ | --------------------------------------------------------------------------------------------- |
| Read Uncommitted | Allows dirty reads (uncommitted data). Fastest. Almost never used. |
| Read Committed (default) | Prevents dirty reads. Allows non-repeatable reads (same query, different results in same tx). |
| Repeatable Read | Prevents dirty + non-repeatable. Allows phantoms (new rows appearing). |
| Serializable | Prevents all anomalies. Strongest. Slowest. Most contention. |

**Q16. What's Read Committed Snapshot Isolation (RCSI)?**

**A.** Optimistic concurrency mode. Readers see a snapshot at statement start. Writers don't block readers, readers don't block writers.

Enable: ALTER DATABASE [name] SET READ_COMMITTED_SNAPSHOT ON;

Benefits: massive concurrency improvement. Trade-off: uses tempdb for version store. Most production OLTP databases should have RCSI enabled.

**Q17. Explain a deadlock and how to handle it.**

**A.** Two (or more) transactions hold locks the other needs. Neither can proceed.

```text
Time   Transaction A          Transaction B
----   -----------------      -----------------
T1     Lock Row 1             Lock Row 2
T2     Wait for Row 2  --->   Wait for Row 1
       (deadlock detected)
       Victim chosen (cheaper to roll back)
       Error 1205 returned
```

Avoidance:

- Always access tables/rows in same order across all code paths.
- Keep transactions short — less time holding locks.
- Use lower isolation when safe (RCSI helps).
- Add indexes — faster queries hold locks less.
- App retry on 1205: catch SqlException, check Number == 1205, retry with backoff.

**Q18. What's an SCH-M (Schema Modification) lock and why does it bite people?**

**A.** Held when running DDL (ALTER TABLE, CREATE INDEX). Blocks ALL other operations on the object — even SELECTs.

In production: even an 'ALTER TABLE ADD COLUMN' can cause a full app outage if the lock blocks reads. Best practices:

- Use ONLINE = ON for index operations (Enterprise edition).
- Schedule DDL during low-traffic windows.
- Use migration patterns: add column nullable -> deploy code -> backfill -> add NOT NULL.
- Never run a long ALTER TABLE in business hours.

### 11.4 — Transactions & ACID Deep

**Q19. Explain ACID with what each property actually means at the engine level.**

**A.** Four guarantees from the storage engine:

**Atomicity:** All or nothing. Implemented via transaction log + rollback. If failure mid-transaction, log is used to undo changes.

**Consistency:** Database moves between valid states. Constraints, foreign keys, triggers enforce this. Not the storage engine alone — schema + app logic together.

**Isolation:** Concurrent transactions don't see each other's intermediate state. Implemented via locks (pessimistic) or versions (optimistic/MVCC).

**Durability:** Committed transactions survive crashes. Implemented via WAL — log records on disk before commit returns.

**Q20. What's the transaction log and how should I think about it?**

**A.** Sequential write-ahead log file. Every modification is logged BEFORE the data page is modified on disk.

Operationally:

- Log file grows during transactions, truncated on backup (Full/Bulk-Logged) or checkpoint (Simple).
- Long-running transactions hold log records indefinitely — log can grow huge.
- Always run regular log backups in Full recovery mode.
- Place log on fast storage. Log writes are the bottleneck for write-heavy systems.
- Don't shrink log files casually — they'll just grow again, and shrink fragments.

**Q21. What's optimistic vs pessimistic concurrency?**

**A.** Two strategies for concurrent updates:

**Pessimistic:** Lock the row when you read it (Repeatable Read or Serializable). Other transactions wait. Safe but reduces concurrency.

**Optimistic:** Read without lock. Before update, check if row was modified since you read it. If yes, conflict — retry or error.

```sql
-- Optimistic via rowversion column
CREATE TABLE Orders (
    Id int PRIMARY KEY,
    Status varchar(20),
    RowVersion rowversion -- auto-increments on update
);

-- Read
SELECT Id, Status, RowVersion FROM Orders WHERE Id = 42;
-- Got: (42, 'pending', 0x0000000000001A2B)

-- Update with check
UPDATE Orders SET Status = 'shipped'
WHERE Id = 42 AND RowVersion = 0x0000000000001A2B;

-- If 0 rows affected: someone else updated. Concurrency conflict.
IF @@ROWCOUNT = 0
    THROW 50001, 'Concurrency conflict', 1;
```

**Q22. What's the difference between READ COMMITTED and SNAPSHOT isolation?**

**A.** Both pessimistic vs optimistic at statement level:

| Aspect | Read Committed vs Snapshot |
| ------------------ | ---------------------------------------------------------------------------------- |
| Read behavior | RC: blocks on locked rows. Snapshot: reads pre-write version. |
| Lock holding | RC: holds shared lock during read. Snapshot: no shared locks for reads. |
| Statement boundary | RC: snapshot per statement. Snapshot: per transaction (sees same data throughout). |
| Storage cost | RC: minimal. Snapshot: row versions stored in tempdb. |
| Use Snapshot for | Long transactions where consistent view matters (reports). |

### 11.5 — Query Optimization

**Q23. How do I read an execution plan?**

**A.** Read RIGHT to LEFT, BOTTOM to TOP. Each node:

- Cost % shows relative expense in the plan.
- Arrow thickness = number of rows.
- Look for: Index Scans (read whole index), Table Scans (read whole table), Key Lookups, Hash/Sort warnings (yellow triangle).

Common red flags:

- Table scan on large table = missing index.
- Estimated rows >> actual rows = bad statistics.
- Spool operators = engine had to materialize intermediate results.
- Key Lookups = non-covering index.

**Q24. What are statistics and why do they matter?**

**A.** Histograms describing data distribution. Optimizer uses them to estimate row counts and choose plans.

Stale statistics = wrong row estimates = wrong plan choice. Common cause of 'query was fast yesterday, slow today.'

Update statistics: UPDATE STATISTICS TableName WITH FULLSCAN. SQL Server auto-updates when ~20% of rows change, but for large tables that's too rare.

Auto-create and auto-update should be ON by default. Architects sometimes turn off auto-update for very large tables and run nightly.

**Q25. What's parameter sniffing?**

**A.** Optimizer compiles a plan based on the FIRST set of parameters. Subsequent executions reuse the plan even if optimal plan differs for different parameters.

```sql
-- Stored proc:
CREATE PROC GetOrdersByCustomer @CustomerId int AS
    SELECT * FROM Orders WHERE CustomerId = @CustomerId;

-- First call with @CustomerId = 42 (1 row matches)
-- Plan: Index Seek (great for 1 row)
-- Cached plan reused for @CustomerId = 7 (100000 rows match)
-- Same Index Seek + 100000 Key Lookups = disaster

-- Fixes:
-- 1. OPTION (RECOMPILE) per query
-- 2. WITH RECOMPILE on procedure
-- 3. Parameter mask: @custLocal = @CustomerId, use @custLocal
-- 4. Plan guides for specific scenarios
-- 5. Query Store (SQL 2016+) to force good plans
```

**Q26. What's the Query Store?**

**A.** SQL Server feature (2016+) that tracks query plans, execution stats, and lets you force specific plans. Game-changer for production tuning.

Use cases:

- Find regressed queries: same query, plan changed, now slow.
- Force a known-good plan when optimizer picks badly.
- Top resource consumers report.
- Baseline performance over time.

**Q27. What's a clustered index design pattern for a write-heavy table?**

**A.** Hot insert tables (orders, audit logs, events). Goals:

- Minimize page splits — use ever-increasing key.
- Avoid index contention — distribute writes.

Options:

- Identity column (int/bigint): all inserts at end. Simple. Page contention possible at very high throughput.
- Sequential GUID (NEWSEQUENTIALID): like identity but GUID.
- DateTime + sequence: natural for time-series.
- Avoid: random GUID (NEWID()) for clustered key — fragmentation nightmare.

### 11.6 — Partitioning and Scale

**Q28. What's table partitioning?**

**A.** Splitting a logical table into multiple physical partitions, typically by date range or hash of a key. Queries that filter on the partition column can skip irrelevant partitions ('partition elimination').

Use cases:

- Time-series data: partition by month/year. Old data archived/dropped easily.
- Multi-tenant: partition by tenant. Isolation + sometimes performance.
- Large fact tables in data warehouse.

**Q29. When should I shard horizontally instead of scaling up?**

**A.** Sharding (splitting data across multiple physical databases) is heavy. Avoid until necessary:

- Single-server has limits: ~64 cores, ~6TB RAM. Plenty for most workloads.
- Scale up first (bigger Azure SQL tier).
- Add read replicas next (cheap reads scaling).
- Optimize queries (often 10x perf available without scale-up).
- Cache aggressively (Redis offloads read traffic).
- Shard ONLY if you've exhausted the above. Cross-shard queries and transactions are painful.

**Q30. What's read replication?**

**A.** Replicate database to read-only secondary. Apps route reads to replicas, writes to primary.

Azure SQL Hyperscale gives 1-4 read replicas. Active Geo-Replication is multi-region.

Trade-off: replicas have some lag. Don't read your own writes from a replica — stale data. Use replicas for reports, dashboards, search.

### 11.7 — Architect-Level SQL Decisions

**Q31. When use stored procedures vs application queries?**

**A.** Modern preference: application queries (Entity Framework, Dapper) with stored procs as exception:

**Application queries:** Versioned with code. Type-safe with EF. Easy to refactor. Single source of truth.

**Stored procedures for:** Complex performance-critical operations. Pre-compiled plan. Security boundary (grant EXEC without table grants). Batch ETL.

Don't write stored procs by default just because it's 'how we did it.' Splits logic between code and DB, harder to test, harder to version.

**Q32. When use NoSQL instead of relational?**

**A.** Decision matrix:

| Need | Pick |
| ------------------------------------------------ | -------------------------------------------------------- |
| ACID transactions across entities | Relational (SQL Server, Postgres) |
| Rich JOIN queries, complex reporting | Relational |
| Strong schema, integrity constraints | Relational |
| Global distribution, low-latency reads worldwide | Cosmos, DynamoDB |
| Massive write scale (100K+/sec) | Cosmos, Cassandra |
| Schema-flexible documents | Cosmos, MongoDB |
| Time-series at scale | Time-series DB (Timescale, InfluxDB) |
| Graph relationships | Graph DB (Neo4j, Cosmos Gremlin) |
| Full-text search | Azure AI Search, Elasticsearch |
| Most enterprise apps | Start with relational. Add specialized stores as needed. |

**Q33. What's the lambda architecture for analytics?**

**A.** Two parallel data pipelines:

**Batch layer:** Process all historical data. Accurate, slow. Synapse, Spark.

**Speed layer:** Process recent data in real-time. Approximate, fast. Stream Analytics, Spark Streaming.

**Serving layer:** Merges batch and speed for queries.

Modern alternative: Kappa architecture — only stream processing, replay history through same engine. Simpler but harder to reach same accuracy.

**Q34. What's a star schema?**

**A.** Data warehouse design. One central fact table surrounded by dimension tables:

- Fact table: measurements/events (sales, telemetry events). Many rows, narrow.
- Dimension tables: descriptive (customer, product, date). Fewer rows, wide.

Queries join fact to dimensions. Optimized for read/aggregation, not for writes. Use in: Synapse, Fabric, BI databases.

Snowflake schema: dimensions normalized further (denormalized dimensions split into sub-tables). More complex, less common modern.

**Q35. How would you handle 1 billion rows in a single table?**

**A.** Architect-level question. Approaches:

- Partition by date. Old partitions go to cheaper storage. Drop entire partitions for retention.
- Columnstore index for analytical workloads. 10-100x compression, fast aggregation.
- Filtered indexes on hot subsets (e.g., active records).
- Archive cold data to separate table or storage (Blob).
- Read replicas for reports.
- Consider Hyperscale tier (Azure SQL) — separates compute from storage, scales storage independently.
- For >10B rows or write rates >100K/sec, consider Cosmos or specialized stores.

## Section 12 — CS Fundamentals (Architect Depth)

The deeper layer that separates great architects from those who only know frameworks. Threading, networking, distributed systems, security, scaling theory. Read once carefully; this is your foundation for any tech interview at any level.

### 12.1 — Threading & Concurrency Fundamentals

**Q1. What's a thread vs a process?**

**A.** Process: an instance of a program. Has its own address space, file handles, security context. Heavyweight.

Thread: an execution path within a process. Shares process's memory and resources. Lightweight.

Threads in same process can share data directly. Across processes requires IPC (pipes, sockets, shared memory).

**Q2. What's a context switch and why is it expensive?**

**A.** When OS scheduler switches from one thread to another, it must:

- Save current thread's CPU registers, stack pointer, instruction pointer.
- Load next thread's state.
- If different process: also switch virtual memory mappings (TLB flush).

Cost: ~1-5 microseconds. Sounds small, but at 10K+ switches/second, becomes significant. Why too many threads = bad performance.

**Q3. How does async/await avoid thread blocking?**

**A.** async/await reuses threads. While thread waits on I/O, it can do other work:

```csharp
// Synchronous: thread blocks during I/O wait
var data = httpClient.GetString(url); // thread sits idle ~50ms

// Async: thread freed during I/O wait
var data = await httpClient.GetStringAsync(url);
// |
// v
// 1. State machine captures local state
// 2. I/O initiated to OS
// 3. Method 'returns' (continuation registered with task)
// 4. Thread released to pool — handles other requests
// 5. I/O completes -> OS notifies
// 6. Continuation scheduled on available thread
// 7. State restored, execution resumes after await
// Result: 1000 concurrent async requests can run on ~10 threads
```

**Q4. What's the ThreadPool and how does it work?**

**A.** .NET maintains a pool of reusable threads. Tasks are queued; threads pick up work.

Tunable:

- Min threads: pool size before considering creating new. Default ~processor count.
- Max threads: cap. Default ~32K.
- ThreadPool grows slowly under sudden load — adds threads at ~1-2 per second.

Pool starvation: if all threads blocked (e.g., sync waits in async code), no thread available. Symptom: requests time out, CPU low. Fix: async all the way, increase min threads (rarely the right answer).

**Q5. What's the difference between cooperative and preemptive multitasking?**

**A.** Preemptive (OS threads): kernel scheduler can interrupt any thread at any time. Allocates time slices. Modern OSes.

Cooperative (async/await, fibers): tasks voluntarily yield control. async/await is cooperative — only yields at await points. If your async method has no awaits and runs CPU-bound, it blocks the thread the entire time.

**Q6. What's race condition vs deadlock vs livelock?**

**A.** Three concurrency problems:

**Race condition:** Behavior depends on timing. Two threads doing x++ — final value unpredictable.

**Deadlock:** Two+ threads each waiting for resource the other holds. System frozen.

**Livelock:** Threads keep changing state in response to each other but make no progress. Like two people sidestepping into each other repeatedly.

**Starvation:** Thread never gets the resource (low priority threads behind high).

**Q7. What synchronization primitives are most important?**

**A.** From cheapest to most expensive:

| Primitive | Use case |
| --------------------------------- | ------------------------------------------------------------------ |
| Interlocked | Atomic ops on int/long. Cheapest. Increment, Add, CompareExchange. |
| Volatile | Memory barrier without lock. Rarely correct to use directly. |
| lock (C# keyword) | Critical sections. Reentrant. Most common. |
| Monitor.Enter/Exit | Same as lock with extra options (TryEnter timeout). |
| SemaphoreSlim | Limit concurrency to N. Async-friendly. |
| ReaderWriterLockSlim | Many readers, one writer. Use when reads vastly outnumber. |
| Mutex | Cross-process. Slower. |
| AutoResetEvent / ManualResetEvent | Signal between threads. |

**Q8. What's a memory barrier?**

**A.** CPU optimizations reorder instructions for speed. Memory barriers tell CPU: 'don't reorder past this point.'

In .NET, locks include implicit barriers. Explicit barriers (Volatile.Read/Write, Interlocked) are rarely needed in app code.

Architect insight: relevant when writing your own primitives or low-level libraries. App developers don't usually touch this.

**Q9. What's Amdahl's Law?**

**A.** Theoretical speedup of parallelization is limited by the serial portion of work.

```text
Speedup = 1 / (S + (1 - S) / N)
where S = serial fraction, N = number of processors

If 10% of work is serial:
  4 cores:  ~3.1x speedup
  16 cores: ~6.4x speedup
  ∞ cores:  10x max (never more)

Implication: throwing more cores at problems with serial bottlenecks
has diminishing returns.
```

Architect application: optimize the serial parts first. Look for bottleneck (DB, single slow service) before scaling out.

### 12.2 — Networking Fundamentals

**Q10. Walk through the OSI model.**

**A.** 7 layers. You need 4 in practice:

| Layer | Purpose / Examples |
| ---------------- | --------------------------------- |
| 7 — Application | HTTP, gRPC, SMTP, DNS |
| 6 — Presentation | TLS encryption, compression |
| 5 — Session | Connection state |
| 4 — Transport | TCP (reliable), UDP (best-effort) |
| 3 — Network | IP routing |
| 2 — Data Link | Ethernet, WiFi MAC |
| 1 — Physical | Cables, radio |

Architects focus on: L7 (your APIs), L4 (TCP/UDP semantics), L3 (IP/routing for VNets, ExpressRoute).

**Q11. TCP vs UDP — when to use each?**

**A.** Two transport protocols, different guarantees:

| Aspect | TCP vs UDP |
| ----------- | -------------------------------------------------------------------------------- |
| Connection | TCP: connection-oriented. UDP: connectionless. |
| Reliability | TCP: guaranteed delivery + ordering. UDP: no guarantees. |
| Performance | TCP: handshake + ACKs cost. UDP: minimal overhead. |
| Use cases | TCP: HTTP, databases, file transfer. UDP: DNS, video streaming, real-time games. |
| Modern | HTTP/3 uses QUIC over UDP — fast like UDP, reliable like TCP. |

**Q12. Walk me through a TCP handshake.**

**A.** Three-way handshake to establish connection:

```text
Client                                  Server
  |                                       |
  | -------- SYN (seq=x) --------------> |
  |                                       |
  | <----- SYN-ACK (seq=y, ack=x+1) ---- |
  |                                       |
  | -------- ACK (ack=y+1) ------------> |
  |                                       |
  |          connection established       |
  | <======= data flowing both ways ====> |
```

Cost: 1.5 RTTs before any data flows. Big penalty for short-lived connections (HTTP/1.1). HTTP/2 multiplexes many requests over one connection.

**Q13. What's HTTPS / TLS?**

**A.** HTTP over TLS (Transport Layer Security). Provides:

- Confidentiality: data encrypted, eavesdroppers see ciphertext.
- Integrity: tampering detected via MAC.
- Authentication: server proves identity via certificate.

TLS 1.3 handshake: 1 RTT (down from 2 in TLS 1.2). Session resumption: 0 RTT for repeat clients.

HIPAA requires TLS 1.2+. Modern apps use TLS 1.3 where available.

**Q14. What's HTTP/2 vs HTTP/1.1?**

**A.** Performance improvements:

**HTTP/1.1:** One request per connection (with keepalive, sequential). Head-of-line blocking — slow request blocks next.

**HTTP/2:** Binary protocol. Multiplexed streams over one connection. Header compression (HPACK). Server push (mostly unused).

**HTTP/3:** Uses QUIC (over UDP). Fixes HTTP/2 TCP head-of-line blocking. Faster connection establishment. 0-RTT resumption.

App impact: HTTP/2+ allows browsers to load many resources without connection limits.

**Q15. What's a CDN and how does it work?**

**A.** Content Delivery Network. Caches content at 'edge' locations close to users.

- User requests example.com/image.jpg.
- DNS resolves to nearest edge IP (anycast).
- Edge serves from cache if present.
- Cache miss: edge fetches from origin, caches, serves.

Why it matters: reduces latency (closer = faster), offloads origin (most requests never hit your server), provides DDoS absorption (huge edge capacity).

**Q16. What's DNS and why should architects care?**

**A.** Domain Name System. Hierarchical lookup: name -> IP address. Cached at many levels.

Architect concerns:

- DNS TTL determines how quickly clients see changes. Low TTL = fast failover but more DNS load.
- DNS lookups add latency on first connection (cached afterward).
- DNS resolution failures cause total app outages — multi-provider DNS is best practice.
- Private DNS Zones in Azure: required when using Private Endpoints. Without them, FQDNs resolve to public IPs.

**Q17. What's a load balancer and what types exist?**

**A.** Distributes incoming requests across multiple backend servers.

| Type | Layer / Use |
| ---------------- | ----------------------------------------------------------------------------- |
| L4 (Transport) | Routes by IP + port. Fast, no inspection. Azure Load Balancer. |
| L7 (Application) | Routes by URL, headers, cookies. Slower but smarter. App Gateway, Front Door. |
| DNS-based | Returns different IPs based on policy. Traffic Manager. Slower failover. |
| Global | Routes across regions. Anycast or DNS. Front Door. |

**Q18. What's idempotency in HTTP?**

**A.** Operation produces same effect whether called once or N times. HTTP verbs:

| Verb | Idempotent? |
| -------- | --------------------------------------------- |
| GET | Yes — safe (no side effects) and idempotent |
| PUT | Yes — set value to specific state |
| DELETE | Yes — deleted is deleted, no further effect |
| POST | No — typically creates new resource each time |
| PATCH | Often, but not guaranteed |

Architect implication: retries are safe for idempotent operations. POST retries can create duplicates — use idempotency keys.

### 12.3 — Distributed Systems Theory

**Q19. What's the CAP theorem?**

**A.** Distributed system can have at most 2 of: Consistency, Availability, Partition tolerance. Network partitions are inevitable, so practical choice is C vs A:

**CP (Consistency + Partition):** Reject requests that can't be consistent. Sacrifice availability. Banking, financial.

**AP (Availability + Partition):** Continue serving with eventual consistency. Sacrifice immediate consistency. Social media, shopping carts.

Real systems aren't always pure CP or AP — they make different choices per operation.

**Q20. What's the PACELC theorem?**

**A.** Extension of CAP. If there's a Partition (P), choose between Availability (A) and Consistency (C). Else (E), choose between Latency (L) and Consistency (C).

Most NoSQL is PA/EL: available during partitions, low latency normally, eventual consistency.

Traditional RDBMS: PC/EC. Strong consistency always, sacrificing availability/latency.

**Q21. What's the 8 fallacies of distributed computing?**

**A.** Listed by Peter Deutsch / James Gosling. Each is a real production failure mode:

- The network is reliable.
- Latency is zero.
- Bandwidth is infinite.
- The network is secure.
- Topology doesn't change.
- There is one administrator.
- Transport cost is zero.
- The network is homogeneous.

Architects internalize these — every distributed system design must account for all 8 being false.

**Q22. What's consensus and why is it hard?**

**A.** Multiple nodes agree on a value despite failures, network delays, partitions.

Algorithms: Paxos (theoretical), Raft (practical, used in etcd, Consul), Zab (ZooKeeper). All have similar essence: elect leader, replicate log, achieve majority quorum.

Why hard: FLP theorem — no deterministic algorithm guarantees consensus with even one faulty process in asynchronous network. In practice we use timeouts and accept liveness trade-offs.

**Q23. What's eventual consistency?**

**A.** Updates propagate over time. All replicas eventually converge to same value. No guarantees on timing or order.

Use when: massive scale, geo-distribution, can tolerate brief inconsistency. Examples: DNS, S3, Cosmos eventual mode.

App developer must handle: 'I just wrote X, but read returns old value.' Solutions: read-your-writes session token, read from same replica that handled write.

**Q24. What's two-phase commit (2PC) and why isn't it used in microservices?**

**A.** Distributed transaction protocol:

- Phase 1: coordinator asks all participants 'can you commit?'
- If all say yes, Phase 2: 'commit.'
- If any say no, Phase 2: 'rollback.'

Problems in modern systems:

- Blocking: participants hold locks during PREPARE phase.
- Coordinator failure: participants stuck in doubt.
- Performance: extra roundtrips.
- Cloud incompatible: HTTP, queues, microservices don't fit.

Replaced by: Saga pattern (compensating transactions), Outbox pattern (eventual consistency).

**Q25. What's leader election?**

**A.** In a cluster, one node is designated 'leader' for some operation (writes, coordination). Others are followers.

Used in: SQL Server Always On (primary), Cosmos (write region), Redis Sentinel, Kafka brokers.

If leader fails, election picks new leader. Brief unavailability during election.

**Q26. What's a quorum?**

**A.** Minimum number of replicas that must respond for an operation to succeed.

Common pattern: N replicas, R read quorum, W write quorum. For strong consistency: R + W > N (read sees all writes).

Example: 5 replicas, write to 3, read from 3. Any read overlaps with any write — sees latest data. Trade-off: latency increases.

### 12.4 — Security Fundamentals

**Q27. What's symmetric vs asymmetric encryption?**

**A.** Two approaches:

| Aspect | Symmetric vs Asymmetric |
| --------------- | ----------------------------------------------------------------------------------------- |
| Keys | Symmetric: same key for encrypt + decrypt. Asymmetric: public encrypts, private decrypts. |
| Performance | Symmetric: fast (AES). Asymmetric: slow (RSA, ECC). |
| Key exchange | Symmetric: secure channel required. Asymmetric: no secret channel needed. |
| Use cases | Symmetric: bulk data. Asymmetric: key exchange, signatures. |
| Typical pattern | TLS uses asymmetric for handshake, symmetric for data. |

**Q28. What's a hash function vs encryption?**

**A.** Hash: one-way function. Same input -> same hash. Cannot reverse to get input.

Encryption: reversible with key. Encrypt produces ciphertext, decrypt restores plaintext.

Hashes for: passwords (with salt), data integrity checks, digital signatures, blockchain.

NEVER hash with: MD5, SHA-1 (broken). Use: SHA-256, SHA-3 for integrity. For passwords: bcrypt, Argon2, scrypt (slow on purpose to resist brute force).

**Q29. What's a salt and why use it for passwords?**

**A.** Random value added to password before hashing. Prevents:

- Rainbow tables (pre-computed hash lookups).
- Two users with same password having same hash.

```csharp
// BAD
var hash = SHA256(password);

// GOOD - use proper password hashing
var salt = RandomNumberGenerator.GetBytes(16);
var hash = KeyDerivation.Pbkdf2(
    password, salt,
    KeyDerivationPrf.HMACSHA256,
    iterationCount: 100000,
    numBytesRequested: 32);

// Store salt + hash. Verify: hash incoming password with same salt, compare.
// Better: use ASP.NET Core PasswordHasher<TUser> or Argon2id.
```

**Q30. What's OWASP Top 10 (architect awareness)?**

**A.** Top web app vulnerabilities. Architects must design against these:

- Broken Access Control: missing authorization checks.
- Cryptographic Failures: weak crypto, exposed secrets.
- Injection: SQL injection, command injection.
- Insecure Design: missing threat modeling.
- Security Misconfiguration: default passwords, verbose errors.
- Vulnerable Components: outdated libraries.
- Identification + Authentication Failures: weak passwords, missing MFA.
- Software + Data Integrity Failures: unverified updates.
- Security Logging + Monitoring Failures: missing detection.
- Server-Side Request Forgery (SSRF): unfiltered URL fetching.

**Q31. What's defense in depth?**

**A.** Multiple layers of security. No single layer is trusted to be perfect.

Example for an enterprise web app:

- Network: VNet, NSG, Private Endpoints, WAF.
- Identity: Entra ID, MFA, Conditional Access, PIM.
- API: JWT validation, rate limiting, input validation.
- Code: parameterized SQL, output encoding, principle of least privilege.
- Data: encryption at rest (CMK), encryption in transit (TLS).
- Audit: logging, monitoring, immutable trail.
- Operations: PIM, secret rotation, vuln scanning.

**Q32. What's zero trust architecture?**

**A.** 'Never trust, always verify.' Replaces perimeter security ('inside good, outside bad') with explicit verification at every access:

- Verify identity for every request, not just at login.
- Least privilege access — minimum needed, time-bounded.
- Assume breach — design for compromise scenarios.

Implementation: Entra Conditional Access, Managed Identity (no shared secrets), Private Endpoints, segmentation.

### 12.5 — Scaling & Performance Theory

**Q33. Vertical vs horizontal scaling?**

**A.** Two approaches:

| Aspect | Vertical (scale up) vs Horizontal (scale out) |
| ----------------- | ---------------------------------------------------------------------------------------------- |
| Method | Vertical: bigger machine. Horizontal: more machines. |
| Limit | Vertical: hardware ceiling. Horizontal: nearly unlimited. |
| Failure model | Vertical: single point of failure. Horizontal: redundancy. |
| Cost curve | Vertical: exponential. Horizontal: linear. |
| Complexity | Vertical: simple — no code changes. Horizontal: requires statelessness, sharding, replication. |
| Default for cloud | Horizontal. Cloud is built for it. |

**Q34. What makes a service horizontally scalable?**

**A.** Statelessness. The service doesn't keep client state in memory between requests.

Where to put state:

- Database (persistent).
- Distributed cache (Redis) for session-like state.
- Client (JWT for auth state, cookies for session ID).

Stateless service can run on N instances. Any instance handles any request. Load balancer distributes. Scale by adding/removing instances.

**Q35. What's caching strategy and what levels exist?**

**A.** Cache at every level where it helps. Multi-tier:

| Tier | Latency / Scope |
| ------------------------ | ------------------------------------------- |
| L1: CPU cache | Nanoseconds. CPU-managed. |
| L2: Process memory | <1ms. Per app instance (IMemoryCache). |
| L3: Distributed cache | 1-5ms. Shared across instances (Redis). |
| L4: CDN | 10-50ms. Edge cache for users (Front Door). |
| L5: Database query cache | Variable. Query plan + result cache. |

**Q36. What's cache invalidation and why is it hard?**

**A.** 'There are only two hard things in CS: cache invalidation and naming things.' — Phil Karlton

Strategies:

**TTL:** Time-based expiry. Simple, can serve stale data briefly.

**Event-based:** On data change, invalidate matching keys. Requires pub/sub for distributed.

**Write-through:** Update cache + DB atomically. Cache always fresh.

**Cache-aside:** App manages cache. On write, invalidate. On read, populate.

**Write-behind:** Update cache first, DB async. Risky — data loss on crash.

Real architecture often mixes: TTL fallback + event invalidation. Tiered TTLs (longer for stable data).

**Q37. What's a thundering herd?**

**A.** Many clients hit a system simultaneously after some trigger. Common scenarios:

- Cache expiry: all clients miss simultaneously, all hit DB.
- Service restart: connections reconnect at once.
- Synchronized retries (no jitter): clients retry at same intervals.

Mitigations:

- Single-flight: only one cache rebuild allowed, others wait (GetOrCreateAsync).
- Jittered TTL: cache entries expire at slightly different times.
- Exponential backoff with jitter on retries.
- Probabilistic early refresh: refresh hot keys before expiry.

**Q38. What's the difference between throughput and latency?**

**A.** Two different performance metrics:

**Latency:** Time per request. Measured in ms. 'How fast for one user?'

**Throughput:** Requests per second. 'How many users can we serve?'

Not the same. A system can have low latency (each request fast) but low throughput (limited concurrency). Or vice versa.

Little's Law: Concurrency = Throughput × Latency. To serve 1000 req/sec at 100ms each, you need ~100 in-flight requests.

**Q39. What's percentile latency and why use it?**

**A.** Average latency hides tail latency. Percentiles tell the truth:

- p50 (median): half of requests are faster.
- p95: 95% of requests faster than this. 5% are slower.
- p99: 99% faster. The slow 1% — your worst experiences.
- p99.9: even tighter tail. Critical for compound services.

Architect rule: optimize for p95 or p99, not average. Average is misleading when distribution is skewed.

**Q40. How do you design for 10x scale?**

**A.** Common architect interview question. Step-through:

- Identify current bottleneck (CPU? memory? I/O? DB? specific service?).
- Apply Amdahl's Law — scaling won't help if bottleneck is serial.
- Stateless services: just add instances + load balancer.
- Database: read replicas, partition by hot column, denormalize for reads.
- Add caching layer (Redis) to absorb read traffic.
- Move sync calls to async (queues) — smooth bursts.
- Move hot data to specialized stores (Cosmos for global, Search for full-text).
- Edge caching (CDN) for static + cacheable dynamic content.
- Monitor everything — find the new bottleneck.

Architecture isn't about supporting 10x today — it's about adding capacity without rewriting.

### 12.6 — AI Architecture for Tech Leads

**Q41. What's an LLM in one sentence?**

**A.** Neural network trained on huge text corpora that predicts the next token given previous tokens. Generates text by predicting one token at a time.

**Q42. What's RAG (Retrieval-Augmented Generation)?**

**A.** Pattern for grounding LLMs in YOUR data without retraining:

- Index your documents into vector DB. Each chunk gets an embedding.
- User asks question.
- Convert question to embedding.
- Query vector DB for most-similar chunks.
- Pass chunks + question to LLM as context.
- LLM generates answer grounded in YOUR docs.

Dominant enterprise AI pattern in 2026. Don't fine-tune unless RAG insufficient.

**Q43. What's prompt engineering?**

**A.** Crafting input to LLM for reliable output. Components:

- System prompt: role, rules, format.
- Few-shot examples: 2-3 input/output pairs.
- User prompt: the actual question.
- Output format directive: JSON, structured, length limits.

Cheapest, fastest way to apply LLMs. Solves 70%+ of use cases without training.

**Q44. What's hallucination and how to prevent it?**

**A.** LLM generates confident-sounding text that's factually wrong. The model is fundamentally a 'next token predictor' — no built-in truth check.

Mitigations:

- RAG with strong retrieval — ground in real documents, instruct model to cite.
- System prompt rules: 'If you don't know, say so. Never invent.'
- Output validation: parse structured output, reject malformed.
- Confidence thresholds: only show answers when retrieval scores high.
- Human review for high-stakes domains (medical, legal).
- Evaluation harness: golden dataset, automated grading.

**Q45. How would you architect an AI feature for a healthcare company?**

**A.** Reference architecture:

- Use case identified — start with low-stakes internal pilot.
- Azure OpenAI Service (BAA-covered, HIPAA-eligible). Not public OpenAI.
- RAG over your data — Azure AI Search with vector index.
- PHI handling: redact before sending to LLM where possible. Audit every LLM call.
- Cost control: token budgets, rate limits per user/tenant.
- Eval harness: track accuracy, hallucination rate.
- Human-in-the-loop for critical decisions.
- Monitoring: latency, cost per request, error rates.

## Section 13 — System Design Framework

Every senior+ interview has a system design round. The questions vary — design a URL shortener, design a chat app, design an order processing system — but the framework is universal. Memorize this framework. Use it on every question.

### 13.1 — The 8-Step Framework

Use this script. Memorize the order. Talk through each step out loud — interviewer needs to see your reasoning.

**Step 1 — Clarify (3-5 min):** Functional + non-functional requirements. Scale, users, latency, availability, compliance.

**Step 2 — Estimate (3 min):** Back-of-envelope. Users × actions = requests/sec. Storage/year. Read:write ratio.

**Step 3 — APIs (3 min):** 3-5 main endpoints with request/response shape.

**Step 4 — Data Model (5 min):** Pick stores. Why. Schema.

**Step 5 — High-level diagram (5 min):** Client → gateway → services → data.

**Step 6 — Deep dive (10 min):** Pick 1-2 risky parts. Talk through.

**Step 7 — Failure modes (5 min):** What breaks. Retries, fallbacks, monitoring.

**Step 8 — Trade-offs (3 min):** What you did NOT pick, and why.

> **Say it like this:** "Before I start designing, I want to understand the requirements. Can I ask a few questions about scale, users, and constraints?" That sentence alone signals senior.

### 13.2 — Step 1: Clarifying Questions to Always Ask

#### Functional

- What are the core user flows?
- Read-heavy or write-heavy?
- Real-time or batch?
- Authenticated or public?
- Multi-tenant or single?
- Multi-region or single?

#### Non-Functional

- How many users? Concurrent peak?
- Latency target — p50, p95, p99?
- Availability target — 99.9? 99.99?
- Data volume now, in 1 year, in 5 years?
- Compliance requirements? (HIPAA, PCI, GDPR, FedRAMP)
- Cost constraints?
- Existing infrastructure? (already on AWS? Azure? on-prem?)

Resist the urge to dive in. The clarification phase is where you signal seniority. Junior candidates start designing immediately.

### 13.3 — Step 2: Estimation Templates

Build the habit of always doing back-of-envelope. Numbers ground the conversation.

#### Common Numbers (Memorize These)

| Quantity | Approximate |
| ---------------------------------- | ---------------- |
| Seconds per day | ~86,400 (≈10^5) |
| L1 cache reference | 0.5 ns |
| Main memory reference | 100 ns |
| SSD random read | 150 µs |
| Disk seek (HDD) | 10 ms |
| Network round-trip in DC | 0.5 ms |
| Network round-trip cross-coast | 150 ms |
| Network round-trip cross-continent | 200-300 ms |
| 1 KB | 10³ bytes |
| 1 MB | 10⁶ bytes |
| 1 GB | 10⁹ bytes |
| 1 TB | 10¹² bytes |

#### Sample Estimation: 'How much storage in 1 year?'

```text
Users: 10M
Active daily: 1M (10%)
Avg writes per active user per day: 10
Avg size per write: 1 KB
Per day: 1M × 10 × 1KB = 10 GB/day
Per year: 10 GB × 365 ≈ 3.6 TB
With 3x replication: ~11 TB
5 years: ~55 TB
```

#### Sample Estimation: 'How many QPS?'

```text
10M daily active users
Avg 100 requests per user per day
Total: 10M × 100 = 1B requests/day
Average: 1B / 86,400 ≈ 12K QPS
Peak (3x avg): ~35K QPS
```

### 13.4 — Step 3: API Design

Always include a few key endpoints. Shows you think about contract before implementation.

Format consistently:

```text
POST /api/v1/orders
Auth: Bearer token
Request: { customerId, items: [...], shippingAddress }
Response: 201 { orderId, status, total }
Errors: 400 (validation), 401 (auth), 409 (duplicate)

GET /api/v1/orders/{orderId}
Auth: Bearer token
Response: 200 { orderId, status, items, shipping }
Errors: 404, 403

GET /api/v1/orders?customerId=X&page=1&pageSize=20
Response: 200 { items: [...], total, page }
```

If asked to design a real-time feature:

```text
WS /api/v1/live?token=X
Server pushes events as JSON:
{ type: 'OrderUpdated', payload: {...} }
```

### 13.5 — Step 4: Data Model

Pick stores based on access patterns:

| Access pattern | Store choice |
| ---------------------------------------------- | ------------------------------------------ |
| Transactional CRUD, ACID, joins | Relational (SQL Server, Postgres, MySQL) |
| Global distribution, schemaless, massive scale | Cosmos DB, DynamoDB |
| Time-series at scale | Timescale, InfluxDB, Cosmos with TTL |
| Full-text search | Azure AI Search, Elasticsearch, OpenSearch |
| Graph traversal | Neo4j, Cosmos Gremlin |
| Caching, session storage | Redis |
| Blobs, files, images | Blob Storage, S3 |
| Event log / streaming | Event Hubs, Kafka, Kinesis |
| Analytics / OLAP | Synapse, BigQuery, Snowflake, Fabric |

### 13.6 — Step 5: High-Level Diagram

Every system has these layers. Draw them in order:

```text
[Client] (web, mobile, IoT)
   |
   v
[CDN + DNS] (Front Door, CloudFront)
   |
   v
[API Gateway] (APIM, Kong, Apigee — auth, rate limit, routing)
   |
   v
[Services] (microservices or modular monolith)
   |
   v
[Data Layer] (caches, DBs, search, blob, queues)

Cross-cutting:
[Auth: Entra ID / Auth0]
[Observability: App Insights + Log Analytics]
[Secrets: Key Vault]
[Async: Service Bus / Event Hubs]
```

### 13.7 — Step 6: Deep Dive Pattern

Interviewer will pick a part to go deep on. Common deep-dive targets:

- 'How does the cache stay consistent with the database?'
- 'What happens if a service goes down mid-transaction?'
- 'How do you scale this to 10x users?'
- 'How do you handle hot partitions?'
- 'What about multi-region failover?'

For each, have a default answer ready. Cache consistency: TTL + event-based invalidation. Failed transaction: Saga or outbox. Scale 10x: identify bottleneck first.

### 13.8 — Step 7: Failure Modes Table

Walk through your design and ask 'what if X fails?' for every component:

| Component | What if it fails? |
| --------------------- | ----------------------------------------------------------------------- |
| CDN | Edge cache miss penalty. Origin handles. Health probes route to backup. |
| API Gateway | Multi-region failover. Health-checked routing. |
| Service instance | Load balancer removes from rotation. Other instances absorb. |
| Cache | Fall back to database. Slower but functional. |
| Database primary | Failover to standby (Always On / Hyperscale). |
| Database read replica | Reads route to primary. |
| Queue | Producer retries. Local outbox buffers. Consumer resumes. |
| Whole region | DR plan: traffic to other region. RTO/RPO defined. |

### 13.9 — Step 8: Articulating Trade-offs

Senior signal: name what you DIDN'T pick and why.

> **Say it like this:** "I picked Cosmos DB for the hot store. Alternative was SQL with sharding. Cosmos gave us per-tenant partition key isolation naturally and auto-scale. SQL would have required manual sharding logic and made multi-region harder. Trade-off: lost JOINs and complex queries. For our query patterns, that's fine. If reporting needs grow, we add Synapse downstream."

### 13.10 — Worked Example: URL Shortener

Classic interview question. Walk through the framework:

#### Clarify

- Functional: shorten long URLs, redirect on visit, analytics on clicks.
- Scale: 100M new URLs/month, 10B click-throughs/month.
- Latency: redirect <100ms p95.
- Custom aliases? Yes. Expiration? Yes, default 1 year.

#### Estimate

- 100M URLs/month = ~40 writes/sec average, 200 peak.
- 10B redirects/month = ~3,800 reads/sec average, 12K peak.
- Storage: 500 bytes/record × 100M × 60 months = ~3 TB.

#### APIs

```text
POST /api/v1/shorten
Request: { longUrl, customAlias?, expiresAt? }
Response: 201 { shortCode, shortUrl, expiresAt }

GET /{shortCode}
Redirect: 302 to longUrl
Async: record click event
```

#### Data Model

Primary store: key-value, optimized for read-by-shortCode. Pick Cosmos DB with shortCode as partition key, or Redis with persistence, or DynamoDB.

Analytics: events to Event Hubs → Stream Analytics → Synapse for click data.

#### Architecture

```text
[Browser] -> [Front Door + CDN]
   |
   long-tail traffic to:
   v
[APIM] (auth, rate limit)
   |
   +--- POST /shorten -> [Shortener API] -> [Cosmos]
   +--- GET /{code}    -> [Redirect API]  -> [Redis cache]
   |                                            |
   |                                            v
   |                                         [Cosmos]
   v
[Event Hubs] -> [Stream Analytics]
   |
   v
[Synapse]
```

#### Deep Dive: Short Code Generation

Options:

- Random base62 (a-zA-Z0-9, 6 chars = 56B combos). Risk: collision needs retry.
- Counter + base62 encoding. Sequential, predictable, no collision. Risk: enumerable.
- Snowflake-style ID: timestamp + machine ID + counter. Distributed, no central counter.

Pick: Snowflake. Distributed write without central counter, no collisions, unpredictable enough.

#### Deep Dive: Redirect at Scale

12K reads/sec sustained, much higher peaks.

- Redis caches the long URL by short code. 99%+ cache hit.
- Aggressive TTL — 24h+ since URLs rarely change.
- CDN caches the 302 response at edge for popular URLs.
- Cosmos as origin — only ~120 reads/sec hit it (1% miss rate).

#### Failure Modes

| Failure | Mitigation |
| ------------------------- | ----------------------------------------------------- |
| Cosmos throttled | Auto-scale RU. Cache absorbs read spike. |
| Hot partition (viral URL) | Cache layer absorbs. Edge CDN also. |
| Click event loss | Event Hubs buffers. Best-effort, OK for analytics. |
| Cache outage | Reads fall to Cosmos directly. Slower but functional. |

#### Trade-offs Discussion

Cosmos vs SQL: picked Cosmos for global distribution and partition by short code. SQL would have required manual sharding.

Sync vs async click events: chose async (Event Hubs). Redirect should never wait for analytics write. Trade-off: 5-10s delay before analytics visible.

### 13.11 — Common System Design Questions to Practice

Practice each by walking through the 8-step framework:

- Design a URL shortener (bit.ly)
- Design a chat/messaging app (WhatsApp basic)
- Design a ride-sharing service
- Design a video streaming service (YouTube basic)
- Design a social media feed (Twitter timeline)
- Design a distributed cache (Redis-like)
- Design a notification system (email + SMS + push)
- Design a rate limiter
- Design a search autocomplete service
- Design a payment processing system
- Design an e-commerce checkout flow
- Design a multi-tenant SaaS platform
- Design a real-time analytics dashboard
- Design a job scheduler (Cron-as-a-service)
- Design a webhook delivery system
- Design a feature flag service
- Design a logging/observability pipeline
- Design a content delivery network
- Design a recommendation system
- Design an A/B testing platform

## Section 14 — Behavioral Preparation + STAR Stories

Architects must communicate, lead, and own. This section gives you templates and stories that work across many roles and many questions.

### 14.1 — The STAR Format

STAR = Situation, Task, Action, Result. Every behavioral story should fit this structure.

**Situation:** Set the scene. What was the context? Brief — 1-2 sentences.

**Task:** What was YOUR specific responsibility? Not 'we' — what did YOU own?

**Action:** What did YOU specifically do? Multiple actions OK. This is the bulk of the story.

**Result:** What changed because of your work? Quantify where possible.

Add a 'Reflection' at the end of each: what would you do differently? Senior+ candidates always include this.

### 14.2 — The Six Universal Stories You Need

Develop 6 STAR stories that cover the most-asked categories. Each story might serve multiple questions.

#### Story Bucket 1: A Significant Technical Decision

Use for: 'biggest technical decision', 'architectural choice', 'trade-off you made', 'how you make decisions'.

Template:

- S: A real architectural fork in your past work.
- T: You owned the decision.
- A: Options considered, criteria, who you consulted, how you documented (ADR).
- R: Outcome with measurable impact.
- Reflection: What you'd revisit.

#### Story Bucket 2: Leading a Team Through Change

Use for: 'leadership', 'change management', 'difficult migration', 'project you led'.

Template:

- S: A migration, redesign, or major refactor that affected multiple engineers.
- T: You led the technical effort.
- A: How you broke down the work, mitigated risk, communicated to stakeholders.
- R: Delivery + team growth outcome.

#### Story Bucket 3: A Production Incident You Owned

Use for: 'on-call experience', 'firefighting', 'crisis management', 'root cause analysis'.

Template:

- S: Outage, severity, customer impact.
- T: You were on-call or escalated to.
- A: Two-phase response — stop the bleeding, then RCA. Specific actions.
- R: Time to resolution. Systemic changes made afterward.
- Reflection: What alert would have caught it earlier.

#### Story Bucket 4: Mentoring or Growing an Engineer

Use for: 'mentoring', 'influence without authority', 'developing others', 'leadership'.

Template:

- S: An engineer who needed growth in some specific area.
- T: You took on the mentoring (formally or informally).
- A: Specific techniques (Socratic questioning, pairing, ADR practice, code reviews).
- R: Their concrete growth — promotion, new responsibilities, behavior change.

#### Story Bucket 5: A Mistake You Made and Recovered From

Use for: 'biggest mistake', 'a failure', 'something you'd do differently', 'humility'.

Template:

- S: Real mistake. Don't pick a humblebrag ('I worked too hard').
- T: You owned it.
- A: How you recognized it, what you did to mitigate, what you fixed long-term.
- R: Outcome. What changed in your judgment because of this.

#### Story Bucket 6: Disagreement With a Stakeholder

Use for: 'disagreement with manager', 'pushing back', 'saying no', 'difficult conversation'.

Template:

- S: A real disagreement with a manager, product person, or peer.
- T: You needed to advocate for a position.
- A: How you presented your case. Listened to theirs. Found common ground or held firm with reasons.
- R: Outcome — your position carried, their position carried (and you committed), or compromise.

### 14.3 — Story Construction Tips

Quality bar for STAR stories:

- Length: 2-3 minutes told. Practice with a stopwatch.
- Specificity: real names of products, real numbers, real outcomes. Vague stories sound made up.
- Quantification: 'reduced p99 from 2s to 200ms' beats 'made it faster.'
- Your role: 'I' for your actions, 'we' for team outcomes. Don't claim sole credit for team wins or deflect responsibility for failures.
- Show learning: always end with reflection. Senior signal.

### 14.4 — Common Behavioral Questions & How to Answer

**QQ1. Tell me about yourself.**

**A.** 90-second elevator pitch. Past, present, future:

Past: brief background (1-2 sentences).

Present: current role + key recent work.

Future: what you want next (tied to this role).

End with: 'happy to dive deeper into any of that.'

**QQ2. Why are you looking to leave / make a change?**

**A.** Never bash current employer. Frame around growth:

> **Say it like this:** "I've been doing [next-level work] informally. Looking for a role where the responsibility matches the work — explicitly leading X, owning Y."

**QQ3. Why this company?**

**A.** 3 specific reasons. Show you've done homework:

- Domain or product angle (why this industry/problem).
- Technology angle (their stack matches your strength or aspiration).
- Team angle (something specific about the team, mission, or recent news).

**QQ4. What's your biggest weakness?**

**A.** Real weakness + active mitigation. Not a humblebrag.

> **Say it like this:** "I tend to go deep on technical research before committing to a decision. Produces good outcomes but can slow the team. I've been working on timeboxing — setting explicit decision deadlines and being okay with 'good enough' over 'perfect.'"

**QQ5. Where do you see yourself in 5 years?**

**A.** Match the role's trajectory. Be honest:

If interviewing for Staff/Tech Lead: 'Continuing as a tech lead with broader scope. Beyond title, what matters: making decisions affecting many engineers, mentoring next generation, being accountable for systems running for years.'

**QQ6. How do you handle ambiguous requirements?**

> **Say it like this:** "First move is to make ambiguity explicit. I write what I think the requirement is and share back: 'Here's what I think you mean — confirm, correct, or fill gaps.' This surfaces disagreements. Then I prioritize: assumptions I can make safely, must-confirm gaps. Must-confirm goes to a stakeholder call; rest, document and proceed."

**QQ7. How do you handle disagreement with your manager?**

> **Say it like this:** "Disagree privately, commit publicly. I raise concerns directly in 1:1 with evidence and alternatives. If after that they still want the original direction, I commit fully — no passive-aggressive complaining. Worst thing is to half-commit to a direction the team needs to execute."

**QQ8. Tell me about a time you said no to a stakeholder.**

> **Say it like this:** "Saying just 'no' is junior. The senior move is 'yes, with this safer/cheaper/better path on a comparable timeline.' I usually frame trade-offs explicitly: 'Here's what you're asking, here's the risk I see, here's an alternative that gets you the outcome with that risk addressed.'"

**QQ9. How do you stay current with technology?**

**A.** 3 sources, specific:

- Official docs for the cloud you work with.
- Specific people you follow (mention 1-2 by name).
- Hands-on POCs. 'Best learning is building. Last quarter I built X to internalize Y.'

**QQ10. Why should we hire you?**

**A.** 3 reasons tied to their needs:

- Background that maps directly.
- A specific experience that's hard to fake (migration, regulated environment, scale).
- Something current that differentiates (AI tooling, specific domain, etc.).

## Section 15 — Question-Answer Playbook

Common questions you'll face in any senior interview, categorized by what they're testing. Have a default answer ready for each.

### 15.1 — Questions About Your Technical Approach

**Q. How do you approach a new codebase?**

**A.**

- Read the README, build, run tests.
- Find the entry point. Trace one request end-to-end.
- Read the deployment/CI config — tells me what the team actually values.
- Look at recent PRs to see patterns and team rhythm.
- Find the longest-tenured engineer and ask: 'What's the part you'd most warn me about?'
- Don't refactor anything for the first 30 days.

**Q. How do you debug a problem you've never seen?**

**A.**

- Reproduce reliably. If I can't, that's step 1.
- Look at recent changes. Git bisect if necessary.
- Read the logs. Real logs, not just error messages.
- Form a hypothesis. Test it cheaply.
- If stuck after 30 min: explain to someone (rubber duck or human).
- Document the fix and root cause for the next person.

**Q. How do you write tests?**

**A.**

- Test behavior, not implementation.
- Test pyramid: many unit, fewer integration, very few E2E.
- FIRST: Fast, Independent, Repeatable, Self-validating, Timely.
- Use AAA: Arrange, Act, Assert.
- Don't mock what you don't own — fragile tests.
- Contract tests at service boundaries.

**Q. How do you decide what to refactor?**

**A.**

- Boy Scout rule: leave the code slightly better than you found it.
- Refactor when you have a reason (adding feature, fixing bug touching that code).
- Don't refactor without tests in place.
- Don't refactor and add features in the same commit.
- Strangler Fig for big refactors, never big-bang.

**Q. How do you balance technical debt vs new features?**

**A.**

Two answers:

- Tactical: budget ~20% of each sprint for debt. Make it visible. Track.
- Strategic: classify debt. Some is deliberate (intentional shortcut for delivery). Some is accidental (didn't know better). Some is decay (was fine, now isn't). Treat differently.

### 15.2 — Questions About Working With Others

**Q. How do you handle code review pushback?**

**A.**

- Distinguish blocker from preference. Be explicit which is which.
- If preference: state once, move on if they disagree. Not worth the friction.
- If blocker: explain why, link evidence, suggest alternative.
- If still stuck: pair on it. 30-min call beats 20 review comments.

**Q. How do you onboard a new team member?**

**A.**

- Week 1: setup, run-the-app, read key docs. No production access yet.
- Week 2-3: small bug fixes. Get them shipping. Confidence > breadth at this stage.
- Month 2-3: real features. Pair on first design.
- Continuous: 1:1s, watch their PRs, give specific praise + corrections early.
- 3-month checkin: have we adjusted the role to fit their strengths?

**Q. How do you give negative feedback?**

**A.**

- Specific, recent, behavioral. 'In yesterday's review you said X' beats 'You always...'
- Impact-focused. 'When that happens, here's the effect on the team.'
- In private. Never in front of others.
- Forward-looking. 'Going forward, here's what would work better.'
- Don't sandwich (compliment-criticism-compliment) — feels manipulative.

### 15.3 — Questions About Architectural Judgment

**Q. When would you NOT use microservices?**

**A.**

- Small team (<10 engineers) — operational overhead dominates.
- Immature CI/CD or observability — you'll cascade failures.
- Domain not yet clear — boundaries will be wrong.
- Cross-service transactions common — fight the distribution at every step.
- No on-call culture — distributed systems need it.

Microservices solve people problems (independent team deploys), not technical ones. Start with a modular monolith.

**Q. When would you NOT use NoSQL?**

**A.**

- ACID transactions across entities matter.
- Rich JOIN queries / reporting / ad-hoc queries.
- Strong schema integrity required (regulated data).
- Team has deep SQL knowledge.
- Volume isn't massive (<TB).

Start relational. Add specialized stores as access patterns demand.

**Q. When would you NOT use the cloud?**

**A.**

- Data sovereignty requires on-prem (some defense, healthcare).
- Extreme cost sensitivity at huge sustained scale (Dropbox famously moved off AWS).
- Specialized hardware unavailable in cloud (HFT).

99% of enterprises should be cloud. The exceptions are real but rare.

**Q. How would you design for 10x scale?**

**A.**

- Identify current bottleneck first.
- Apply Amdahl's Law — scaling doesn't help if bottleneck is serial.
- Stateless services: just add instances.
- Database: replicas for reads, partition by hot column for writes.
- Caching layer (Redis) for read traffic.
- Async (queues) for spike absorption.
- Edge caching (CDN) for cacheable content.
- Monitor, find new bottleneck, repeat.

### 15.4 — Questions About Process and Culture

**Q. How do you run a design review?**

**A.**

- Pre-read: doc sent 24h ahead. Meeting is for discussion, not reading.
- Author presents context + decision + alternatives — not 'here's my design.'
- Reviewers focus on: what's the failure mode? What does this make hard later?
- Outcomes: approve / approve with edits / needs another round.
- Always end with action items and owner.

**Q. How do you handle a 'we need this NOW' request?**

**A.**

- Acknowledge urgency. Don't dismiss.
- Ask: what's the actual deadline driver? Often softer than presented.
- Frame trade-offs: 'I can ship X by Y but it skips Z. Z would take W more time.'
- Document the shortcut as tech debt with a follow-up ticket.
- Don't repeatedly accept 'this one time' — it becomes the norm.

**Q. How do you scale yourself as a tech lead?**

**A.**

- Document decisions (ADRs) so context survives.
- Code review patterns: build team's pattern language, not just fix individual PRs.
- Mentor explicitly. Pick 1-2 engineers per quarter.
- Delegate decisions, not just tasks. Let them make calls within boundaries.
- Make yourself replaceable. If team can't function without you, you're a bottleneck.

## Section 16 — Closing the Interview Strong

How you close matters. The last impression is what they remember.

### 16.1 — Questions YOU Should Ask

Always have 3-4 ready. Not asking questions is a red flag.

#### Questions About the Role

- What does success look like in 30/60/90 days?
- What's the biggest technical challenge the team is working through?
- Why is this role open — backfill, growth, new initiative?
- How are architecture decisions made and documented here?

#### Questions About the Team

- How big is the team? Composition by level?
- What's the on-call rotation like?
- How does the team handle disagreements on technical direction?
- What's something the team is proud of? Something it's still working on?

#### Questions About Culture

- How does the team think about technical debt vs new features?
- How does the team adopt new tools — top down or bottom up?
- How does the team handle a production incident?

#### Questions About Growth

- What does career growth look like from this role?
- Where do strong people in this role go next?
- How does the team handle learning — books, courses, conferences?

#### Questions About Your Interviewer

- What's been the highlight of your time at this company?
- What's something you'd change if you could?
- What's the part of your job that energizes you most?

DON'T ask: salary or benefits (recruiter handles), things easily Googled, anything that sounds like you didn't read the JD.

### 16.2 — Closing the Conversation

End every interview with a confident close:

> **Say it like this:** "This conversation has been useful. Your background tells me you've solved exactly the kinds of problems I want to work on. Is there anything in my background you'd want me to clarify or strengthen before next steps?"

This move is confident, not desperate. Explicitly invites feedback — rare and senior. Most candidates leave feedback on the table.

### 16.3 — After the Interview

- Send thank-you within 24 hours. Short, specific. Reference one technical topic discussed.
- Don't follow up multiple times. One thank-you is enough.
- If you bombed a question, you can mention in the thank-you: 'On the [X] question, I realize I should have said Y.' Recovers some signal.
- Note questions you struggled with — that's your study list.

### 16.4 — Recovery Phrases When Things Go Sideways

Things will go wrong in some interviews. These phrases buy time and signal maturity:

- 'Let me think about that for a moment.' (Then actually think. Silence is OK.)
- 'That's a good question — I want to break it into two parts.'
- 'Can I clarify what you're looking for — the technical mechanism or design rationale?'
- 'I'd answer differently depending on constraints. Can I assume X and Y?'
- 'I'd want to validate that with a small spike before committing in real code.'
- 'I haven't worked with that specifically. My understanding is X. The closest thing I've shipped is Y.'

### 16.5 — If You Don't Know Something

Don't bluff. Senior interviewers will smell it. Use this template:

> **Say it like this:** "I haven't worked with [X] directly. My understanding is [one-sentence positioning]. The closest thing I've shipped is [Y]. The pattern transfers because [reason]. I'd want to do a small spike before claiming production knowledge."

This response signals: honest about gaps, has related transferable experience, knows how to learn fast. All senior signals.

### 16.6 — Universal Closing Statement

If they ask 'do you have any final thoughts?' use this:

> **Say it like this:** "Three things I'd want to leave you with. First, [your strongest skill that maps to the role]. Second, [a specific example that proves it]. Third, [why you want this role specifically — tie to something they said earlier]. Happy to answer anything else."

### Final Note

*This document grows with you. Add notes after each interview. Track what worked, what didn't, which questions surprised you.*

*The best senior engineers I know treat every interview as a chance to refine their craft of articulating what they know. The technical knowledge is necessary but not sufficient. The way you talk about it is what gets you the offer.*

***Be honest about gaps. Lean into strengths. Quantify everything. Show the trade-offs you considered. End with confidence.***
