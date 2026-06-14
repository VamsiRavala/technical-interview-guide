# AI Frontend — The Thin, Secure, Streaming-Aware React Layer Over ASP.NET Core

> This section covers **only the AI-specific React layer** for enterprise AI apps in a Microsoft stack: streaming LLM responses, Entra ID auth + On-Behalf-Of to Azure OpenAI, SignalR agent telemetry, chat UX, agent dashboards, workflow graphs, and Power BI embedding. It does **not** re-teach React, TypeScript, hooks, or component design — for those fundamentals see section **[01-React-JS](../01-React-JS/README.md)**. Accurate to the library and Azure surface as of **mid-2026**.

The opinion running through this section: **the frontend is a thin, secure, streaming-aware client over an ASP.NET Core backend.** Never put Azure OpenAI keys, Power BI master credentials, or RAG vector stores in the browser. The browser holds a narrowly-scoped *user token*; the backend holds the *secrets* and does the privileged calls. Everything below follows from that.

---

## 📚 Table of Contents

### Realtime & Streaming

1. **[Streaming Responses](01-streaming-responses.md)** - Consuming SSE / `fetch` `ReadableStream` token streaming from a .NET backend, SSE frame buffering, incremental memoized render, and clean stop/abort that cancels Azure OpenAI billing
4. **[SignalR Real-Time](04-signalr-realtime.md)** - `@microsoft/signalr` in a hook, Entra-secured connections, agent status/run updates, reconnect + group re-join, and SSE vs SignalR per feature

### Chat Experience

2. **[Chat UI Patterns](02-chat-ui-patterns.md)** - Message-list virtualization, optimistic UI + reconciliation, citations, tool-call rendering, markdown/code sanitization, error/retry, IME-safe composer, and accessibility

### Identity & Architecture

3. **[Entra ID Auth](03-entra-id-auth.md)** - MSAL.js / `@azure/msal-react`, silent token acquisition, calling a protected ASP.NET Core API, and On-Behalf-Of to Azure OpenAI — never exposing keys
5. **[Next.js BFF](05-nextjs-bff.md)** - App Router route handlers as a BFF/proxy to ASP.NET Core/Azure OpenAI, edge vs node runtime, and the RSC-vs-client split

### Operations & Insight

6. **[AI Dashboards](06-ai-dashboards.md)** - Agent monitoring: runs/traces, token & cost charts with Recharts/visx, and merging live SignalR updates with historical TanStack Query data
7. **[Workflow Visualization](07-workflow-visualization.md)** - React Flow for agent/workflow graphs, driving node/edge state from SignalR events, auto-layout, and designer vs monitor modes
8. **[Power BI Embedding](08-powerbi-embedding.md)** - `powerbi-client-react`, embed tokens from the backend, Row-Level Security (RLS), and token refresh

### Interview Preparation

9. **[Interview Questions](09-interview-questions.md)** - 50 detailed React + AI Q&A covering streaming, auth/OBO, SignalR, chat UX, security, performance, and production bugs

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Read **Streaming Responses** and build a `useChatStream` hook that reads a `ReadableStream`, buffers SSE frames, and renders tokens incrementally with a functional state update
2. Work through **Chat UI Patterns** — a typed message model, optimistic user bubble, sanitized markdown, and an IME-safe composer
3. Skim **Entra ID Auth** and wire `MsalProvider` + `acquireTokenSilent` to call a protected API with a bearer token

### Intermediate (Week 3-4)
1. Master **Entra ID Auth** end-to-end — request only `access_as_user`, and explain the On-Behalf-Of exchange to Azure OpenAI
2. Build the **Next.js BFF** — a node-runtime route handler that pipes the SSE body unchanged, and split a chat page into RSC + client island
3. Add **SignalR Real-Time** — a `useAgentHub` hook with `accessTokenFactory`, automatic reconnect, and group re-join

### Advanced (Week 5-6)
1. Build **AI Dashboards** — merge live SignalR runs with historical TanStack Query data without flicker, and chart backend-computed token cost
2. Add **Workflow Visualization** with React Flow (designer + monitor modes) and **Power BI Embedding** with RLS-scoped embed tokens
3. Drill the 50 **Interview Questions** and rehearse "SSE vs SignalR", "why never call Azure OpenAI from the browser", and "the OBO flow at the frontend boundary" out loud

---

## 🔑 Where Each Concern Lives

| Concern | Server Component / Route Handler / .NET API | Client Component |
|---|---|---|
| Calling Azure OpenAI / .NET API with secrets | Yes | Never |
| Acquiring confidential tokens / reading session | Yes | No |
| Rendering a streaming token feed | No | Yes (`useState`/`useReducer`) |
| Chat input, stop button, scroll-to-bottom | No | Yes |
| Initial conversation history (read-only) | Yes (RSC, ships no JS) | No |
| React Flow canvas, Power BI embed, charts | No (need browser APIs) | Yes |
| System prompts / guardrail instructions | Yes (versioned store) | Never |

## 🔑 Tool Choices at a Glance

| Job | Use | Not |
|---|---|---|
| Single-response token stream | `fetch` + `ReadableStream` (SSE wire format) | `EventSource` (GET-only, no headers) |
| Long-lived run telemetry, multi-client | SignalR (groups, reconnect, fallback) | Polling |
| Server state (lists, history, cost, metrics) | TanStack Query | Redux |
| High-frequency streaming state | local `useReducer` / refs | global store |
| Cross-cutting client UI (model, theme, flags) | Zustand / Redux | per-token state |
| Markdown render | `react-markdown` + `remark-gfm` + `rehype-sanitize` | `dangerouslySetInnerHTML` |
| Charts | Recharts (fast) / visx (control) | hand-rolled SVG |
| Graphs | React Flow (`@xyflow/react`) + Dagre/ELK | manual positioning |
| BFF runtime touching Entra tokens | Node | Edge (Web Crypto only) |

## 🔑 Key Decisions

| Question | Short answer |
|---|---|
| **Browser → Azure OpenAI directly?** | Never. The browser holds a token scoped only to your API; the .NET backend does OBO to Azure OpenAI. |
| **Stop a stream cleanly?** | `AbortController.abort()`, swallow `AbortError`, keep partial text, and forward `RequestAborted` to cancel Azure OpenAI billing. |
| **Most common streaming bug?** | Stale-closure append dropping tokens — always use the functional updater or a ref. |
| **SSE or SignalR?** | SSE for the chat token stream; SignalR for multiplexed, long-lived run telemetry. Use both. |
| **Secure Power BI embed?** | Backend (service principal) generates a short-lived RLS-scoped embed token via `EffectiveIdentity`; the client cannot broaden it. |
| **Token expiry mid-stream?** | Acquire fresh via MSAL `acquireTokenSilent` per request; `accessTokenFactory` for SignalR; offload long jobs to background + push. |

> **Where the rest lives:** React/TypeScript/hooks fundamentals are section **01-React-JS** (this section deliberately does not duplicate them). The ASP.NET Core backend, Semantic Kernel orchestration, RAG, and Azure OpenAI infrastructure that this frontend talks to are covered in sections **09-Azure-AI**, **10-Semantic-Kernel**, **11-AI-Agents**, and **12-RAG-Systems**. This section keeps the AI-specific frontend surface front and center and points to those for backend depth.
