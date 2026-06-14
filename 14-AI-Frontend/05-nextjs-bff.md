# Next.js App Router as a BFF

> In a Microsoft shop the real intelligence lives in **ASP.NET Core** (Semantic Kernel, orchestration, RAG, guardrails, the Azure OpenAI key). The Next.js App Router sits in front as a thin **backend-for-frontend**: it owns the user session, attaches the token, streams responses through unchanged, and never holds Azure OpenAI credentials. This file covers route handlers as a proxy, edge vs node runtime, and the RSC-vs-client split. Next.js routing basics are assumed (section **01-React-JS**).

---

## Why a BFF in front of ASP.NET Core

It looks like two hops — isn't that redundant? No: each hop has a distinct, intentional job.

```text
Browser ──► Next.js BFF (route handler) ──► ASP.NET Core API ──► Azure OpenAI
   user        session, CORS, token         orchestration,        key /
   token       attach, stream passthrough    RAG, guardrails,      managed
                                             content safety, key   identity
```

- The **.NET layer** owns orchestration, RAG, content safety, logging, rate limiting, and the Azure OpenAI key (or managed identity). It never deals with browser CORS or cookie sessions.
- The **Next BFF** owns the user session/token and same-origin requests, so the browser never learns the internal API URL and CORS disappears. It can also enrich prompts with server-only context and enforce per-tenant routing.

The browser never sees the Azure OpenAI endpoint or key — only its own origin.

## Route handler as a streaming proxy

The handler is a **dumb pipe**: acquire the confidential token, forward the request, and return the upstream `ReadableStream` body **unchanged**. Do not `await res.json()` or buffer — that defeats streaming and kills time-to-first-token.

```typescript
// app/api/chat/route.ts  — Node runtime, proxies to ASP.NET Core
import { NextRequest } from "next/server";
import { getServerToken } from "@/lib/auth";

export const runtime = "nodejs"; // need confidential token acquisition + full crypto

export async function POST(req: NextRequest) {
  const token = await getServerToken();
  const body = await req.text();

  const upstream = await fetch(`${process.env.API_BASE}/chat/stream`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      Accept: "text/event-stream",
    },
    body,
    // @ts-expect-error duplex is required by Node fetch when streaming a body
    duplex: "half",
  });

  // Pipe the SSE stream straight through; add auth, strip internal headers.
  return new Response(upstream.body, {
    headers: {
      "Content-Type": "text/event-stream",
      "Cache-Control": "no-cache, no-transform", // no-transform stops proxies buffering
      Connection: "keep-alive",
    },
  });
}
```

Add the auth check and per-user rate limiting **before** the upstream fetch. Strip internal headers from the response so service URLs never leak.

## Edge vs Node runtime

The decisive factor for AI BFFs is **confidential-client auth**. On-Behalf-Of and certificate-based auth need full Node crypto and the MSAL Node library, which the Edge runtime (Web Crypto only) can't fully provide.

| | Edge runtime | Node runtime |
|---|---|---|
| Cold start / latency | Very low, globally distributed | Higher |
| Crypto / MSAL confidential client | Limited (Web Crypto only) | Full |
| Azure SDK / longer timeouts | Limited | Yes |
| Streaming pass-through | Excellent | Excellent |
| Long-running OBO / cert auth | Avoid | Use this |
| Recommendation for AI BFF | Public/anon streaming proxies only | **Default for anything touching Entra tokens** |

Use **Node** for the chat proxy (OBO, confidential client, Azure SDK). Reserve **Edge** for stateless, public streaming endpoints or geo-routing where latency matters and no confidential auth is needed.

## RSC vs Client Components in an AI app

The single most important boundary: **where the live token stream is consumed.** A streaming response is interactive and stateful, so the component rendering tokens is a Client Component. But the *fetch* (with the user's token, request shaping, RAG assembly) can live server-side, keeping secrets and latency off the client.

| Concern | Server Component / Route Handler | Client Component |
|---|---|---|
| Calling Azure OpenAI / .NET API with secrets | Yes | Never |
| Acquiring confidential tokens / reading session | Yes | No |
| Rendering a streaming token feed | No | Yes (`useState`/`useReducer`) |
| Chat input, stop button, scroll-to-bottom | No | Yes |
| Initial conversation history (read-only) | Yes (RSC, ships no JS) | No |
| React Flow canvas, Power BI embed, charts | No (need browser APIs) | Yes |

```tsx
// app/conversations/[id]/page.tsx  — Server Component
// Fetches history server-side (no client JS, token never reaches the browser),
// then hands an interactive Client Component the initial messages.
import { ChatPanel } from "./ChatPanel";
import { getServerToken } from "@/lib/auth";

export default async function ConversationPage({ params }: { params: { id: string } }) {
  const token = await getServerToken(); // confidential, server only
  const res = await fetch(
    `${process.env.API_BASE}/conversations/${params.id}/messages`,
    { headers: { Authorization: `Bearer ${token}` }, cache: "no-store" }
  );
  const initialMessages: ChatMessage[] = await res.json();

  // ChatPanel is "use client" — it owns the live streaming state and AbortController.
  return <ChatPanel conversationId={params.id} initialMessages={initialMessages} />;
}
```

The rule of thumb: secrets and one-shot reads go server-side (history, token acquisition, RAG assembly); anything that mutates per-token or needs browser APIs (the live stream, React Flow, Power BI) is a Client Component. The boundary is "does this need the live stream or browser APIs?" — not "is this interactive in general?"

## Two streams that are easy to confuse

| Stream | Mechanism | Where |
|---|---|---|
| **Suspense / RSC streaming** | HTML chunks over the SSR connection | Server → browser, renders the shell instantly |
| **LLM token streaming** | SSE / `ReadableStream` | Consumed in a Client Component |

They are orthogonal. Use Suspense to stream the chat *shell* and history HTML so the user never sees a spinner-on-blank-page; use SSE for the live *response* tokens.

```tsx
import { Suspense } from "react";

export default function Page() {
  return (
    <main>
      <ChatHeader />
      <Suspense fallback={<MessageSkeleton count={3} />}>
        {/* async RSC; HTML streams in when ready */}
        <ConversationHistory id="abc" />
      </Suspense>
      <ChatComposer />
    </main>
  );
}
```

## Data fetching around the stream

Streaming is hand-rolled, but everything *around* the chat (conversation list, agent runs, cost history, model catalog) is classic request/response. Use **TanStack Query** for server state — caching, retries, invalidation for free. Reserve Redux/Zustand for genuinely client-global UI state (selected model, theme, feature flags).

```typescript
// hooks/useConversations.ts
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/apiClient";

export function useConversations() {
  return useQuery({
    queryKey: ["conversations"],
    queryFn: () => apiFetch<Conversation[]>("/conversations"),
    staleTime: 30_000,
  });
}
```

## System prompts stay server-side

Never ship system prompts or guardrail instructions to the browser — they can be inspected or tampered with. The client sends only user input and a template id or "mode"; the BFF (or .NET API) composes the final prompt from a versioned template store. This protects proprietary prompt engineering, enables A/B testing prompts without a client deploy, and prevents users from extracting or overriding guardrails.

## Interview angle

"Why proxy Azure OpenAI through Next.js *and* ASP.NET Core — isn't that two hops?" — Because the .NET layer owns orchestration, RAG, content safety, logging, and the Azure OpenAI key; the Next BFF owns the user session and CORS. Each hop is intentional. The browser never sees the OpenAI endpoint or key, and the .NET API never deals with browser CORS or cookie sessions. Use the **node** runtime for the proxy because OBO/confidential-client auth needs full crypto, and forward the `ReadableStream` body unchanged with `no-transform` so nothing buffers.
