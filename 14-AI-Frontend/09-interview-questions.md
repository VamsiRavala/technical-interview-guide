# Interview Questions: React + AI (50)

> Senior-level interview preparation for AI Solutions Developer/Architect roles in a Microsoft-centric stack (React, .NET, Azure OpenAI, Entra ID, SignalR, Power BI). These 50 questions cover the AI-specific React layer; React/TypeScript fundamentals are in section **01-React-JS**. Answers are crisp and production-oriented.

---

**Q1.** How do you render streaming LLM tokens in React using `fetch` and `ReadableStream`?

Call your backend with `fetch`, then read `response.body.getReader()` and decode chunks with `TextDecoder`. Append each decoded delta to React state so the UI grows token-by-token. Use a functional state update to avoid stale closures, and batch with `requestAnimationFrame` or a small buffer to avoid re-rendering on every byte.

```tsx
async function streamChat(prompt: string, onDelta: (t: string) => void, signal: AbortSignal) {
  const res = await fetch("/api/chat", {
    method: "POST",
    body: JSON.stringify({ prompt }),
    headers: { "Content-Type": "application/json" },
    signal,
  });
  const reader = res.body!.getReader();
  const decoder = new TextDecoder();
  while (true) {
    const { value, done } = await reader.read();
    if (done) break;
    onDelta(decoder.decode(value, { stream: true }));
  }
}
```

This keeps perceived latency low because the user sees output as soon as the first token arrives, instead of waiting for the full completion.

**Q2.** When should you use Server-Sent Events (SSE) versus raw `ReadableStream` for chat streaming?

SSE (`text/event-stream`) gives you a structured, named-event protocol with automatic reconnection in `EventSource`, which is ideal when the server emits multiple event types (token deltas, tool-call events, citations, done markers). Raw `ReadableStream` over `fetch` is lower-level but supports POST bodies (EventSource only does GET) and works cleanly with abort. For LLM chat I prefer `fetch` + ReadableStream because prompts are large POST payloads and I need `AbortController`. If I want server-push reconnection semantics and GET-friendly endpoints, SSE via EventSource is simpler. With ASP.NET Core both map to `IAsyncEnumerable`. Note EventSource lacks custom headers (auth tokens), so SSE often needs cookies or query-string tokens; ReadableStream lets me set `Authorization` headers directly, which fits Entra ID bearer tokens better.

**Q3.** How do you implement stop/abort for an in-flight streaming response?

Create an `AbortController` per request, pass `controller.signal` to `fetch`, and call `controller.abort()` when the user clicks Stop. Store the controller in a ref so it survives re-renders. On abort, the reader loop throws an `AbortError`, which you catch and ignore. Crucially, also notify the backend to cancel the upstream Azure OpenAI call so you stop paying for tokens — the aborted HTTP request triggers `HttpContext.RequestAborted`, which you forward as the `CancellationToken` to `GetStreamingChatMessageContentsAsync`.

```tsx
const controllerRef = useRef<AbortController | null>(null);
function stop() { controllerRef.current?.abort(); }
async function send(prompt: string) {
  controllerRef.current = new AbortController();
  try { await streamChat(prompt, append, controllerRef.current.signal); }
  catch (e) { if ((e as Error).name !== "AbortError") throw e; }
}
```

**Q4.** What are the core UI patterns for a production chat interface?

A message list (alternating user/assistant roles), a sticky composer with auto-growing textarea, a streaming indicator, and stop/regenerate controls. Keep messages as an array of typed objects (`{ id, role, content, status, toolCalls?, citations? }`). Render an in-progress assistant message separately so streaming deltas don't churn the whole list. Support keyboard send (Enter) vs newline (Shift+Enter). Auto-scroll to bottom only when the user is already near the bottom — otherwise respect their scroll position so they can read history. Add per-message actions (copy, regenerate, thumbs up/down for eval feedback). Persist conversation state to the backend so refresh restores history. Show token/cost or model badges for transparency in enterprise tools.

**Q5.** How do you implement optimistic UI in a chat app?

Immediately append the user's message to local state with a temporary client-generated id and a `pending` status, before the network round-trip confirms it. Then start streaming the assistant reply. If the send fails, mark the message with an `error` status and offer retry rather than silently dropping it. Optimistic updates make the UI feel instant. The key is reconciliation: when the server returns the canonical message id, swap the temp id. Use a stable `id` as the React `key` so the swap doesn't remount the component. Avoid optimistic rendering for the assistant message itself — you don't know its content until tokens arrive — but the user bubble and the "assistant is typing" placeholder should appear instantly.

**Q6.** Why and how do you virtualize a long message list?

Long conversations (hundreds of messages, each with rendered markdown and code blocks) destroy scroll performance because the DOM grows unbounded. Virtualization renders only the visible window plus a small overscan. Use `@tanstack/react-virtual` with dynamic measurement since chat messages have variable heights. The tricky parts are: measuring variable heights (use `measureElement`), keeping scroll anchored to the bottom during streaming, and handling images/code that change height after load. For streaming, I often exempt the last (in-progress) message from virtualization and render it normally so its rapidly changing height doesn't fight the virtualizer. Combine with `content-visibility: auto` as a cheaper fallback for moderate lists.

**Q7.** How do you render citations returned by a RAG backend?

Return citations as structured data alongside the answer: `[{ id, title, url, snippet, chunkId }]`, and have the model emit inline markers like `[1]`. In React, parse the markers and render them as superscript buttons that open a popover or scroll to a sources panel. Don't trust the model to format HTML — keep citation rendering deterministic on the client from the structured array. Highlight the cited snippet and link to the source document (with deep-linking to the page or chunk when possible). For auditability in enterprise scenarios, show a "Sources" section listing every retrieved chunk with relevance scores. This builds trust and lets users verify grounding, which is essential for compliance-sensitive Microsoft customers.

**Q8.** How do you render tool/function calls in the chat UI?

Tool calls are a distinct message type, not free text. The backend streams events like `{ type: "tool_call", name, args }` and `{ type: "tool_result", output }`. Render these as collapsible cards ("Searching knowledge base…", "Calling weather API") with status (pending, succeeded, failed) and the arguments/results in a JSON viewer or formatted summary. This gives transparency into agent reasoning. Keep them visually subordinate to the final assistant answer. For multi-step agents, render a timeline of steps. Allow expanding to see raw arguments for debugging, but show a human-readable label by default. This pattern maps directly to Semantic Kernel function-calling and the Microsoft Agent Framework, where each step emits structured events.

**Q9.** When would you use SignalR instead of SSE for AI features?

Use SignalR for bidirectional, multi-event scenarios: long-running agent jobs where you push status updates ("planning", "retrieving", "executing tool 3 of 5"), collaborative sessions, or when multiple clients watch the same job. SignalR handles transport negotiation (WebSockets, falling back to SSE/long-polling), automatic reconnection, and grouping (push to a specific user or conversation group). For a single request/response token stream, plain SSE/ReadableStream is simpler. But for an agent platform with background orchestration, SignalR's hub model and `Clients.Group(conversationId)` semantics are a much better fit. In React, use `@microsoft/signalr` with `HubConnectionBuilder`, `withAutomaticReconnect`, and `withUrl` configured for Entra ID access-token retrieval.

**Q10.** How do you wire SignalR into a React component for agent status?

Build the connection in an effect, register handlers, and clean up on unmount. Use a factory for the access token so it refreshes.

```tsx
useEffect(() => {
  const conn = new HubConnectionBuilder()
    .withUrl("/hubs/agent", { accessTokenFactory: () => getToken() })
    .withAutomaticReconnect()
    .build();
  conn.on("status", (s: AgentStatus) => setStatus(s));
  conn.on("token", (t: string) => append(t));
  conn.start().then(() => conn.invoke("Join", conversationId));
  return () => { conn.stop(); };
}, [conversationId]);
```

Keep the connection per conversation, join a group keyed by conversation id, and debounce status updates to avoid render storms.

**Q11.** How do you implement Entra ID auth in a React SPA with MSAL?

Use `@azure/msal-react` with `MsalProvider`, `PublicClientApplication`, and `MsalAuthenticationTemplate`. Configure `authority` to your tenant and `redirectUri`. Acquire tokens silently with `acquireTokenSilent`, falling back to `acquireTokenPopup`/`Redirect` on `InteractionRequiredAuthError`. Request scopes for your protected API (e.g. `api://<app-id>/access_as_user`), not Graph directly, so your backend receives an audience-correct token. Attach the token as a bearer header on API calls. Never store tokens in `localStorage` for sensitive apps — MSAL's default session storage plus its in-memory token cache is safer. Use `useMsalAuthentication` or `useIsAuthenticated` hooks to gate routes. The SPA only ever sees a token scoped to your own API; it must not hold Azure OpenAI keys.

**Q12.** Explain the On-Behalf-Of (OBO) flow and where React fits.

OBO lets your API call a downstream API (e.g., Microsoft Graph or another protected service) as the signed-in user, not as the app. The React SPA acquires a token for your API (`api://your-api/.default`). Your API receives that token, then exchanges it via the OBO grant for a new token scoped to the downstream resource, preserving the user's identity and permissions. React's only job is to obtain and send the correctly scoped token for your API — it never touches downstream tokens. This is important for RAG over SharePoint/OneDrive where retrieval must respect per-user permissions (security trimming). The token exchange and downstream calls stay server-side, keeping the chain auditable and keys out of the browser.

**Q13.** How do you use Next.js App Router as a BFF/proxy in front of Azure OpenAI?

Put a Route Handler (`app/api/chat/route.ts`) that runs server-side, holds the Azure OpenAI credentials (or uses managed identity), validates the user's session/token, applies rate limiting, and streams the response back. The browser only ever talks to your own origin, never to Azure OpenAI directly — so keys are never exposed and you can add logging, quotas, and content filtering centrally. Return a `ReadableStream` from the handler to forward tokens. The BFF pattern also lets you enrich prompts with server-only context, enforce per-tenant model routing, and strip PII before it reaches the model. Use `export const runtime = "nodejs"` (not edge) if you need the Azure SDK or longer timeouts.

**Q14.** Show a Next.js Route Handler that streams from a backend.

```tsx
export const runtime = "nodejs";
export async function POST(req: Request) {
  const { prompt } = await req.json();
  const upstream = await fetch(process.env.AI_API!, {
    method: "POST",
    headers: { Authorization: `Bearer ${await getMIToken()}` },
    body: JSON.stringify({ prompt }),
  });
  return new Response(upstream.body, {
    headers: {
      "Content-Type": "text/event-stream",
      "Cache-Control": "no-cache",
      Connection: "keep-alive",
    },
  });
}
```

Forward the upstream `ReadableStream` directly so you don't buffer the whole completion in memory. Add auth checks and per-user rate limiting before the fetch.

**Q15.** RSC vs client components — how do you split a chat app?

React Server Components render on the server and ship no JS for static, data-heavy parts: conversation history lists, document/source panels, settings pages, and initial message hydration. Mark interactive pieces (`"use client"`) for the composer, streaming message renderer, stop button, and anything using hooks, refs, or event handlers. The streaming chat surface itself must be a client component because it manages `AbortController`, local state, and the reader loop. Fetch initial conversation data in a server component and pass it as props to the client island. This minimizes client bundle size and leverages the server for auth and data access while keeping the realtime parts on the client.

**Q16.** What state-management approach fits a chat application?

For most chat apps, colocated React state plus a reducer is enough: `useReducer` for the message list with actions like `ADD_USER_MSG`, `APPEND_DELTA`, `SET_ERROR`. For cross-component state (multiple panels, sidebar conversation list, global model selection), use Zustand — it's minimal, avoids context re-render storms, and supports selectors so streaming deltas only re-render the active message. Use TanStack Query for server state: fetching conversation lists, persisting messages, caching documents. Avoid Redux's boilerplate unless the org standardizes on it. The key insight is separating high-frequency streaming state (kept local/ref-based to avoid re-rendering the whole tree) from low-frequency server state (cached via Query).

**Q17.** How do you design error, retry, and rate-limit UX for AI calls?

Distinguish error classes: transient (429, 503, network) vs permanent (400 bad request, 401 auth, content filtered). For transient, auto-retry with exponential backoff and show a subtle "retrying…" indicator; surface a manual Retry button after attempts are exhausted. For 429, read the `Retry-After` header and show a countdown rather than hammering the API. For content-filter rejections, show a clear, non-technical message. Always keep the user's typed prompt recoverable so a failed send doesn't lose their text. Never show raw stack traces. Log correlation ids so support can trace issues. Degrade gracefully — if the primary model is down, the BFF can fall back to a secondary model and the UI just shows a "using backup model" badge.

**Q18.** How do you safely render markdown and code from LLM output?

Use `react-markdown` with `remark-gfm`, and never use `dangerouslySetInnerHTML` on raw model output. Sanitize with `rehype-sanitize` (or DOMPurify if you must inject HTML) to strip scripts, iframes, and event handlers — LLM output is untrusted input and can carry prompt-injected XSS payloads. Render code blocks with a syntax highlighter (`react-syntax-highlighter` or Shiki) and a copy button. Disable raw HTML passthrough in the markdown parser. Constrain link rendering to add `rel="noopener noreferrer"` and consider blocking `javascript:` URLs. Treating model output as hostile is essential: a RAG document or tool result could contain malicious content that the model faithfully reproduces.

**Q19.** How do you handle streaming markdown that's syntactically incomplete?

While tokens stream, the markdown is often half-formed (an unclosed code fence, a dangling `**`). Re-parsing the full buffer on every delta is both expensive and produces flickering as elements open and close. Buffer deltas and re-render at a throttled cadence (e.g., 30–60ms via `requestAnimationFrame`). Some teams use incremental markdown parsers that tolerate incomplete syntax. A pragmatic approach: render the streaming text as preformatted/plain during streaming, then do a final full markdown parse when the stream completes (`done`). For code blocks specifically, detect an open fence and render the partial content in a `<pre>` so it looks right even before the closing fence arrives.

**Q20.** What accessibility concerns apply to AI chat UIs?

Announce streaming responses to screen readers using an `aria-live="polite"` region, but throttle updates so it doesn't read every token — announce on completion or in sentence chunks. Ensure the composer is a labeled `textarea`, Stop/Send buttons have accessible names, and focus management returns to the composer after send. Make tool-call cards and citations keyboard-navigable and expose state via `aria-expanded`. Maintain sufficient color contrast for code blocks and role indicators; don't rely on color alone to distinguish user vs assistant. Support reduced motion for typing animations. Provide skip links past long histories. For status indicators use `role="status"`. These matter doubly for enterprise/government Microsoft customers with strict WCAG 2.1 AA requirements.

**Q21.** How do you embed Power BI reports in a React app securely?

Use `powerbi-client-react` (`PowerBIEmbed`). Never embed with a master/Pro user token directly in the browser. Instead, your backend generates an embed token via the Power BI REST API (`GenerateToken`) using a service principal or managed identity, applying row-level security (RLS) for the signed-in user. The React component receives only the short-lived embed token, embed URL, and report id. Configure `tokenType: models.TokenType.Embed`, set `permissions`, and handle the `loaded`/`error` events. Refresh the token before expiry. This "embed for your customers" pattern keeps capacity credentials server-side and enforces per-user data security through RLS, which is critical when surfacing AI-generated insights alongside governed BI dashboards.

**Q22.** How do you visualize agent/RAG workflows with React Flow?

React Flow (`@xyflow/react`) models nodes and edges declaratively. Represent each agent step or pipeline stage (retrieve, rerank, generate, tool-call) as a custom node, and connections as edges showing data flow. Drive node status (idle/running/done/error) from live SignalR updates so the diagram animates as the agent executes. Use custom node components for rich content (showing the prompt, tokens used, latency). Keep layout deterministic with a layout library (Dagre/ELK) for auto-positioning of DAGs. This is excellent for explainability dashboards in multi-agent platforms — stakeholders see exactly which tools fired and where time was spent. Make it read-only for monitoring, or editable for a low-code workflow builder.

**Q23.** How do you communicate cost and latency to users without alarming them?

Show time-to-first-token by streaming early; perceived speed matters more than total time. Optionally display a subtle model badge (e.g., "GPT-4o") and, for internal/enterprise tools, token counts and estimated cost per message for transparency and budgeting. Use skeleton states and a typing indicator so the wait feels active. For long agent runs, show step-by-step progress rather than an indeterminate spinner. Avoid surfacing raw latency numbers to end users in consumer apps; surface them in admin/observability views. If using cost-aware routing, you can hint "fast mode" vs "smart mode" as a user choice. The goal: make latency legible and give power users control without cluttering the casual experience.

**Q24.** What are the top frontend security rules for an AI app?

(1) Never put API keys (Azure OpenAI, search) in client code or env vars prefixed for the browser — all model calls go through your authenticated BFF. (2) Treat all model output as untrusted: sanitize markdown/HTML to prevent XSS. (3) Enforce auth on every backend route; the SPA token is scoped only to your API. (4) Apply Content Security Policy to limit script/connect sources. (5) Rate-limit per user server-side, not just client-side. (6) Don't log full prompts/responses with PII to client telemetry. (7) Guard against prompt injection by not letting model output trigger privileged client actions automatically — require user confirmation for destructive tool calls. (8) Validate/escape any model-suggested URLs or commands before acting on them.

**Q25.** How do you implement regenerate and edit-and-resend?

Store messages with stable ids and parent links. "Regenerate" re-sends the prior user turn (and prior context) to produce a new assistant message, optionally keeping a version history so users can compare. "Edit and resend" replaces a user message and truncates the conversation after it — discard downstream assistant/user turns since they're now invalid context — then re-streams. Model this as a tree if you want branching ("alternate responses"), or linearly with truncation for simplicity. On the backend, pass the reconstructed message array up to the edited point. Optimistically show the edit, then stream the new reply. Keep an undo path. This requires the message array to be the single source of truth, not derived DOM state.

**Q26.** How do you persist and restore chat sessions?

Persist server-side keyed by conversation id and user id; the client fetches the conversation list and lazy-loads messages on selection (paginated, newest first). Use TanStack Query to cache and revalidate. On send, write the user message and the final assistant message to the store (the BFF does this after streaming completes). For resilience, you can also checkpoint partial streams. Store metadata: model used, token counts, timestamps, feedback. For offline/refresh resilience, hydrate the open conversation from the server on mount. Avoid storing full history only in `localStorage` for enterprise apps — server persistence enables cross-device continuity, audit, and admin review. Encrypt sensitive conversation data at rest (Azure Storage/Cosmos with CMK).

**Q27.** How do you throttle re-renders during high-frequency token streaming?

Appending to state on every token can fire hundreds of renders per second. Buffer incoming deltas into a ref and flush to state on a `requestAnimationFrame` or a ~30ms interval. Alternatively, keep the streaming text outside React (mutate a DOM node directly via ref) and only commit to React state when the stream finishes — this bypasses reconciliation entirely for the hot path. Use `React.memo` on message components keyed by id so only the in-progress message re-renders, not the whole list. Zustand selectors or `useSyncExternalStore` can scope subscriptions. Measure with the React Profiler; the streaming message is the only thing that should be re-rendering during a response.

**Q28.** How do you handle multi-modal (image/file) inputs in the chat UI?

Add a file picker and drag-drop zone to the composer. Validate type/size client-side, then upload to a backend endpoint (which stores in Azure Blob and returns a reference), rather than embedding base64 in the prompt. Show thumbnail previews and upload progress. For vision models, send the blob URL/SAS or the bytes to the BFF, which constructs the multimodal message for Azure OpenAI. Render image inputs inline in the user bubble. For documents, show a chip with filename and let the backend handle extraction/RAG. Handle paste-from-clipboard for screenshots. Enforce content scanning (Azure AI Content Safety) on uploads. Keep large payloads off the main prompt path to control cost and latency.

**Q29.** How would you implement "thinking"/reasoning display for reasoning models?

Reasoning models can emit separate reasoning vs final-answer channels. Render the reasoning trace in a collapsible "Thinking…" disclosure that's expanded while streaming and auto-collapses when the final answer begins, so users can peek but aren't overwhelmed. Visually de-emphasize it (muted color, smaller text). Stream reasoning tokens into the disclosure and answer tokens into the main bubble, distinguished by event type from the backend. Persist whether the user prefers to see reasoning. Be careful not to expose chain-of-thought in contexts where it shouldn't be shown (some providers/policies restrict surfacing raw reasoning). For agent steps, the "thinking" view doubles as a step timeline.

**Q30.** How do you test a streaming React chat component?

Use Vitest/Jest + React Testing Library. Mock `fetch` to return a `Response` whose `body` is a `ReadableStream` you control, enqueuing chunks to simulate streaming and asserting the UI updates incrementally. Test abort by asserting the reader's cancel is called when Stop is clicked. Use fake timers to test throttling/buffering logic. For SignalR, abstract the connection behind an interface and inject a fake. Test error paths (429 with Retry-After, content filter, network failure) and optimistic-then-reconcile flows. Add Playwright end-to-end tests against a stubbed backend for real streaming, abort, and scroll-anchoring behavior. Snapshot the markdown sanitization output to catch XSS regressions.

**Q31.** How do you implement infinite scroll for conversation history?

Load the most recent N messages, and when the user scrolls near the top, fetch older messages with cursor-based pagination (keyed on message timestamp/id). Preserve scroll position when prepending older content by measuring `scrollHeight` before and after insert and adjusting `scrollTop` — otherwise the view jumps. Use TanStack Query's `useInfiniteQuery` with `getPreviousPageParam`. Combine with virtualization for very long histories. Distinguish "load older" (scroll up) from streaming new messages (scroll down). Show a small loading spinner at the top during fetch. Cursor pagination beats offset pagination because messages are appended continuously and offsets shift.

**Q32.** How do you manage prompt templates and system prompts on the client?

Keep system prompts and sensitive instructions server-side in the BFF — never ship them to the browser where they can be inspected or tampered with. The client sends only the user input and a template id or selected "mode." The server composes the final prompt from a versioned template store (e.g., in config or a prompt registry). For user-facing templates (saved prompts, slash commands), store those per-user and render a picker. This protects proprietary prompt engineering, lets you A/B test prompts without redeploying the client, and prevents users from extracting or overriding guardrail instructions. Expose only the variables the user is allowed to fill.

**Q33.** How do you implement a typing/streaming indicator that feels natural?

Show a three-dot pulsing indicator immediately on send (before first token) to signal the request is alive. Replace it with the streaming text as soon as the first delta arrives. Avoid artificial per-character typewriter delays on already-received text — that adds latency for no benefit; just render tokens as they arrive, which already feels like typing. Use `aria-live` for accessibility but throttle announcements. For tool-using agents, swap the indicator label to reflect the current step ("Searching…", "Analyzing…"). Respect `prefers-reduced-motion`. The combination of instant indicator + real token streaming gives the most responsive feel.

**Q34.** How do you handle very large model responses without freezing the UI?

Stream incrementally so you never hold a giant string before rendering. Virtualize the message list and consider collapsing very long assistant messages with a "show more." Render code blocks lazily and avoid re-highlighting on every delta (highlight once on completion). Offload heavy markdown parsing to a throttled flush. If a single message is huge, paginate or chunk its rendering. Keep the streaming write path off React's reconciler (direct DOM mutation via ref) to avoid layout thrash, committing to state at the end. Monitor with the Performance panel for long tasks and break them up. Cap message size server-side as a safety valve.

**Q35.** How do you build a feedback (thumbs up/down) loop for eval?

Add per-message feedback controls that POST `{ messageId, rating, optionalReason, conversationContext }` to your backend, which stores it for offline evaluation and prompt/model tuning. Capture the model, prompt version, retrieved sources, and latency alongside the rating so feedback is actionable. Optionally prompt for a free-text reason on thumbs-down. Use this data to build eval datasets and detect regressions. Show subtle confirmation, and let users change their rating. Keep it privacy-aware (consent, PII handling). On the platform side, this feeds an evaluation pipeline (Azure AI Foundry evaluations) comparing model/prompt variants. The frontend's job is low-friction capture with enough context attached to be useful.

**Q36.** How do you implement client-side rate limiting and debouncing?

Disable the Send button while a request is in flight to prevent duplicate sends, and debounce rapid retries. For autocomplete/suggestion features that call the model on keystroke, debounce (e.g., 300–500ms) and cancel stale requests with `AbortController`. Track a client-side request budget and show a cooldown when exceeded — but treat this as UX only, never as the security boundary. The authoritative rate limit must be enforced server-side per user/tenant (the client can be bypassed). Client-side limiting reduces unnecessary load and accidental spamming, improving cost and responsiveness, while the BFF enforces hard quotas with 429 + Retry-After.

**Q37.** How do you support conversation branching/alternate responses?

Model the conversation as a tree where each message node can have multiple children. "Regenerate" creates a sibling assistant node; the UI shows "2 / 3" navigation arrows to switch active branches, and the active path determines context for subsequent turns. Persist the full tree server-side with parent ids. Rendering walks the currently selected path. This mirrors ChatGPT's branching. The complexity is in state management — keep a `currentPath` and a node map. Truncation on edit creates a new branch rather than destroying history. Provide clear affordances for navigating versions. For simpler apps, linear with truncation is acceptable, but branching preserves user work and supports comparison.

**Q38.** How do you handle authentication token expiry during a long stream?

Long streams can outlive a short-lived access token. Acquire a fresh token before initiating each request via MSAL `acquireTokenSilent`. For very long-running connections (SignalR), use `accessTokenFactory` so the library fetches a current token on (re)connect. If a mid-stream call fails with 401, abort and re-acquire the token, then retry from a checkpoint if your backend supports resumable streams. Prefer keeping individual streaming requests bounded; offload truly long jobs to background processing with SignalR status push, which reconnects with a fresh token. Never refresh tokens by exposing refresh tokens to JS — rely on MSAL's silent renewal via hidden iframe/refresh token in secure storage.

**Q39.** How do you render structured/JSON model output in the UI?

When using structured outputs (JSON schema / function calling), parse the JSON server-side, validate against the schema, and send typed objects to the client — don't make React parse half-streamed JSON. Render purpose-built components: a table for tabular results, a form for extracted fields, cards for entities. If you must stream partial JSON, use a tolerant streaming JSON parser and render progressively, but it's cleaner to stream a human-readable summary while the structured payload arrives at the end. Validate with Zod on the client as a defense layer. Structured output unlocks rich, deterministic UIs (charts, forms, action buttons) instead of free text, which is far more useful in enterprise tools.

**Q40.** How do you implement "stop and keep partial" vs "stop and discard"?

On abort, you've already rendered partial tokens. "Stop and keep" finalizes the partial assistant message (mark status `stopped`) and persists what was generated, so the user keeps useful partial output and can regenerate or continue. "Stop and discard" removes the in-progress message entirely. Make the default "keep partial" since users usually stop because they have enough. Persist the partial to the backend on stop so refresh preserves it. Inform the backend to cancel the upstream call to save tokens regardless. Offer a "Continue" action that resumes generation from the partial. This requires the message status model to include a `stopped` state distinct from `complete` and `error`.

**Q41.** How do you secure the SignalR connection with Entra ID?

Protect the hub with `[Authorize]`, and have the client supply a bearer token via `accessTokenFactory` (SignalR sends it as a query string for WebSockets since headers aren't available on the WS handshake — ensure HTTPS and validate the token in `OnConnectedAsync`). Map the authenticated user's id to groups so you only push to authorized connections (`Clients.User(userId)` or group-per-conversation with authorization checks on join). Validate the token's audience and scopes. Handle reconnection token refresh via the factory. Don't trust client-supplied conversation ids without verifying the user has access. For Azure SignalR Service, configure upstream auth and use managed identity between your app and the service.

**Q42.** How do you implement slash commands and prompt shortcuts?

Detect a leading `/` in the composer and show an autocomplete menu of commands (e.g., `/summarize`, `/translate`, `/image`). Each command maps to a backend behavior or prompt template (resolved server-side). Render the menu with keyboard navigation (arrow keys, Enter to select, Esc to dismiss) and filter as the user types. On selection, either insert template text or set a hidden mode flag sent with the request. Keep the command registry server-driven so you can add commands without a client deploy. For arguments, support inline parameters or a guided form. This pattern improves discoverability and steers users toward supported, well-tested workflows rather than free-form prompting.

**Q43.** How do you measure and improve frontend AI latency?

Instrument time-to-first-token (TTFT) and inter-token latency on the client, plus total time and time-to-interactive. Send these to your telemetry (Application Insights via the JS SDK) tagged with model, route, and tenant. TTFT dominates perceived speed, so optimize the path to first token: avoid buffering at the BFF, use `flush`/no-buffering on proxies, choose nodejs runtime over edge if the SDK adds latency, and keep the prompt assembly fast. Reduce bundle size and hydrate the chat island early. Prefetch the conversation. Use HTTP/2 and keep-alive to the BFF. Correlate client TTFT with backend Azure OpenAI latency to find where time goes (network, queueing, model).

**Q44.** How do you guard against prompt injection affecting the UI?

Assume retrieved documents and tool outputs may contain instructions attempting to hijack behavior or inject markup. Never auto-execute model-suggested actions (navigation, API calls, downloads) without explicit user confirmation, especially destructive ones. Sanitize all rendered output (no raw HTML/scripts). Don't let model output set sensitive UI state directly. For agents with tools, keep a human-in-the-loop confirmation for high-risk tools, and render tool calls transparently so users can spot anomalies. Constrain links and never render `javascript:`/`data:` URIs. On the backend, separate trusted system instructions from untrusted content and use spotlighting/delimiters. The frontend's role is to refuse to treat model output as privileged commands.

**Q45.** How do you implement export/share of a conversation?

Provide export to markdown, PDF, or a shareable link. For markdown, serialize the message array (roles, content, citations) into a clean document. For share links, create a server-side immutable snapshot with its own id and access policy (public, org-only, or specific users) rather than exposing the live conversation. Redact or warn about sensitive content before sharing. Generate PDFs server-side for fidelity and security. Respect data governance — enterprise tenants may disable external sharing. Include metadata (model, date) for provenance. Make the share token revocable. The key is that sharing creates a controlled copy with explicit access control, not a leak of the underlying private conversation store.

**Q46.** How do you keep the composer responsive with auto-resize and IME?

Use an auto-growing `textarea` that adjusts height to content up to a max, then scrolls. Handle Enter-to-send / Shift+Enter-for-newline, but respect IME composition: check `event.isComposing` (or `keyCode === 229`) so pressing Enter to confirm a Japanese/Chinese/Korean candidate doesn't accidentally send the message. Debounce height recalculation. Preserve cursor position on programmatic edits. Support paste of images/files. Disable send when empty or while streaming. Keep focus in the composer after send. These details matter for global enterprise users; mishandling IME is a common bug that makes the app unusable for CJK input.

**Q47.** How do you implement model/temperature selection in the UI?

Offer a constrained set of options (model, "creative/balanced/precise" presets mapping to temperature/top-p) rather than raw numeric knobs for most users; expose advanced controls behind a developer toggle. Send the selection to the BFF, which validates it against allowed values per tenant/role — never trust the client to pick arbitrary models, since model access has cost and governance implications. Persist the user's preference. Show clear labels and tooltips explaining trade-offs (speed vs quality vs cost). For cost-aware routing, the "auto" option lets the backend choose. Validating server-side prevents users from selecting expensive or unapproved models and supports per-tenant model allowlists.

**Q48.** How do you handle offline and connection-loss scenarios?

Detect offline via `navigator.onLine` and connection errors, and show a clear banner. Queue the user's typed message locally so it isn't lost, and retry when connectivity returns. For SignalR, rely on `withAutomaticReconnect` and show reconnecting status, re-joining groups on reconnect. For an in-flight stream that drops, mark the message as interrupted and offer resume/regenerate. Cache the conversation list and recently viewed messages so the app is readable offline (with TanStack Query persistence or a service worker for PWAs). Avoid optimistic success states that lie about delivery. Clearly distinguish "saved locally, will retry" from "delivered." Graceful degradation maintains trust during flaky enterprise networks/VPNs.

**Q49.** How do you architect a reusable chat component library?

Expose a headless core (hooks like `useChatStream`, `useAbortableRequest`) decoupled from presentation, so teams can style their own UI. Provide composable primitives: `<MessageList>`, `<Message>`, `<Composer>`, `<ToolCallCard>`, `<Citations>`. Accept a transport adapter interface so the same UI works over fetch/SSE/SignalR. Keep markdown/sanitization centralized and configurable. Support render props/slots for custom message types. Ship TypeScript types for the message model. Make theming token-based. Document accessibility behavior. This lets multiple Microsoft-internal apps share battle-tested streaming/abort/sanitization logic while customizing look and integrations, avoiding each team re-implementing fragile streaming code.

**Q50.** What are the most common production bugs in React AI chat apps and their fixes?

(1) Stale closures appending to old state during streaming — fix with functional updates or refs. (2) Scroll jumping when prepending history or during streaming — fix with scroll anchoring and exempting the in-progress message. (3) Memory/perf from un-virtualized long lists — add virtualization. (4) Leaked `AbortController`/connections on unmount — clean up in effects. (5) XSS from unsanitized markdown — sanitize. (6) Token expiry mid-stream — refresh proactively. (7) Re-render storms from per-token state — buffer/throttle. (8) Duplicate sends — disable button in flight. (9) IME Enter sending early. (10) Keys leaking to the client — always proxy through the BFF. Each maps to a concrete, testable fix.
