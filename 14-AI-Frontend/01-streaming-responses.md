# Streaming LLM Responses in React

> Consuming a token stream from an ASP.NET Core backend is the single most important AI-specific frontend skill. This file assumes you already know React, hooks, and TypeScript (see section **01-React-JS**) — it focuses only on the streaming delta: reading a `ReadableStream`, parsing SSE frames, rendering incrementally, and aborting cleanly.

---

## The mental model

A streaming chat response is just an HTTP response whose body arrives in chunks instead of all at once. The browser exposes that body as a `ReadableStream`. Your job is to read it chunk-by-chunk, decode bytes to text, parse the wire protocol (usually SSE frames), and append each token to React state so the UI grows live.

```text
ASP.NET Core (Azure OpenAI streaming)
        │  IAsyncEnumerable<string> → SSE frames
        ▼
  Next.js BFF route handler  (pipes body unchanged, adds auth)
        │  text/event-stream
        ▼
  Client Component
        │  fetch().body.getReader()
        ▼
  TextDecoder → split "\n\n" → JSON.parse(data:) → setState(s => s + delta)
```

Two things stay true everywhere: the browser never holds the Azure OpenAI key (the .NET backend owns it), and the component that renders tokens is always a **Client Component** because it owns mutable streaming state and an `AbortController`.

## Why `fetch` + `ReadableStream`, not `EventSource`

`EventSource` is the classic SSE client, but it is **GET-only** and **cannot set custom headers**. Chat prompts are large POST bodies and Entra ID auth needs an `Authorization: Bearer` header — both rule out `EventSource`. So we POST with `fetch`, set the bearer header, and read `response.body` ourselves. We still use the **SSE wire format** (`data: {...}\n\n`) on the backend because it is a clean, framed protocol that survives proxies and lets the server interleave token deltas, tool-call events, and citations.

| | `EventSource` | `fetch` + `ReadableStream` |
|---|---|---|
| HTTP method | GET only | Any (POST for prompts) |
| Custom headers (auth) | No | Yes (`Authorization`) |
| Auto-reconnect | Built in | You handle it |
| Abort | `close()` | `AbortController` (richer) |
| Verdict for chat | Avoid | **Use this** |

## The streaming hook

This is the canonical `useChatStream` hook. Note the three load-bearing details: the `AbortController` lives in a ref so it survives re-renders, the SSE buffer handles frames split across chunk boundaries, and the state update is **functional** (`s => ...`) so fast streams never drop tokens to a stale closure.

```tsx
// hooks/useChatStream.ts  ("use client")
import { useCallback, useRef, useState } from "react";

interface StreamState {
  text: string;
  isStreaming: boolean;
  error: string | null;
}

export function useChatStream() {
  const [state, setState] = useState<StreamState>({
    text: "",
    isStreaming: false,
    error: null,
  });
  const abortRef = useRef<AbortController | null>(null);

  const send = useCallback(async (prompt: string, conversationId: string) => {
    const controller = new AbortController();
    abortRef.current = controller;
    setState({ text: "", isStreaming: true, error: null });

    try {
      const res = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json", Accept: "text/event-stream" },
        body: JSON.stringify({ prompt, conversationId }),
        signal: controller.signal,
      });
      if (!res.ok || !res.body) throw new Error(`Stream failed: ${res.status}`);

      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });

        // SSE frames are separated by a blank line.
        const frames = buffer.split("\n\n");
        buffer = frames.pop() ?? ""; // keep the trailing partial frame
        for (const frame of frames) {
          const line = frame.replace(/^data:\s?/, "");
          if (line === "[DONE]") continue;
          const { delta } = JSON.parse(line) as { delta: string };
          // Functional update — never lose tokens to a stale closure.
          setState((s) => ({ ...s, text: s.text + delta }));
        }
      }
      setState((s) => ({ ...s, isStreaming: false }));
    } catch (e) {
      if ((e as Error).name === "AbortError") {
        setState((s) => ({ ...s, isStreaming: false })); // keep partial text
      } else {
        setState((s) => ({ ...s, isStreaming: false, error: (e as Error).message }));
      }
    }
  }, []);

  const stop = useCallback(() => abortRef.current?.abort(), []);

  return { ...state, send, stop };
}
```

### SSE frame parsing: the buffer is mandatory

A single `reader.read()` chunk does **not** align to SSE frame boundaries. A frame can be split across two reads, or several frames can arrive in one read. The fix is the rolling `buffer`: append each decoded chunk, split on `\n\n`, process every complete frame, and keep the last (possibly partial) fragment in the buffer for the next read. Skipping this is the most common streaming bug — it produces `JSON.parse` errors on half-frames under load.

## Rendering incrementally without re-parsing the whole document

Re-parsing the entire accumulated markdown on every token is a real performance trap at long responses. Memoize the rendered output so a token append only re-renders the active bubble, and sanitize because model output is untrusted (see `02-chat-ui-patterns.md`).

```tsx
// components/StreamedMarkdown.tsx  ("use client")
import { memo } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeSanitize from "rehype-sanitize";

export const StreamedMarkdown = memo(function StreamedMarkdown({
  text,
  isStreaming,
}: {
  text: string;
  isStreaming: boolean;
}) {
  return (
    <div className="prose" aria-live="polite" aria-busy={isStreaming}>
      <ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
        {text}
      </ReactMarkdown>
      {isStreaming && <span className="caret" aria-hidden>▋</span>}
    </div>
  );
});
```

### Throttling re-renders on fast streams

Appending to React state on every token can fire hundreds of renders per second. Two production techniques:

- **Buffer + flush:** accumulate deltas in a ref and flush to state on a `requestAnimationFrame` or a ~30ms interval, so you render at frame cadence rather than per-token.
- **Bypass the reconciler for the hot path:** write streaming text directly to a DOM node via ref during streaming, and commit to React state only on `done`. This is the fastest option but loses React's declarative model mid-stream — reserve it for the one in-progress message.

Combine either with `React.memo` keyed by message id so only the streaming message re-renders, never the whole list.

## Stop / abort cleanly

`AbortController.abort()` cancels the fetch; the reader loop throws `AbortError`, which you swallow while keeping the partial text already rendered. Crucially, the abort must also reach the **backend** so Azure OpenAI stops generating (and billing) tokens. In ASP.NET Core the aborted HTTP request surfaces as `HttpContext.RequestAborted`, which you forward as the `CancellationToken` to `GetStreamingChatMessageContentsAsync`.

```csharp
// ASP.NET Core minimal API: stream SSE and honor client abort
app.MapPost("/chat/stream", async (HttpContext ctx, ChatRequest req,
    IChatCompletionService chat) =>
{
    ctx.Response.Headers.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache, no-transform";

    var history = new ChatHistory();
    history.AddUserMessage(req.Prompt);

    // RequestAborted fires when the browser aborts the fetch — stops Azure OpenAI billing.
    await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(
        history, cancellationToken: ctx.RequestAborted))
    {
        var frame = JsonSerializer.Serialize(new { delta = chunk.Content });
        await ctx.Response.WriteAsync($"data: {frame}\n\n", ctx.RequestAborted);
        await ctx.Response.Body.FlushAsync(ctx.RequestAborted); // flush per token, no buffering
    }
    await ctx.Response.WriteAsync("data: [DONE]\n\n");
});
```

The `FlushAsync` per frame is essential — without it the server (or a proxy) buffers the whole completion and you lose streaming entirely, tanking time-to-first-token.

## Stop-and-keep vs stop-and-discard

When the user stops, you have already rendered partial tokens. Model two behaviors via a message `status`:

- **Stop and keep** (sensible default): finalize the partial assistant message with status `stopped`, persist it, and offer a **Continue** action that resumes generation. Users usually stop because they have enough.
- **Stop and discard:** remove the in-progress message entirely.

Either way, propagate cancellation to the backend to save tokens. This requires a status model with `stopped` distinct from `complete` and `error`.

## Token-expiry mid-stream

A long stream can outlive a short-lived Entra access token. Acquire a fresh token immediately **before** initiating each request via MSAL `acquireTokenSilent` (see `03-entra-id-auth.md`). Keep individual streaming requests bounded; for truly long jobs, offload to background processing and push status over SignalR (`04-signalr-realtime.md`), which reconnects with a fresh token via `accessTokenFactory`.

## Common bugs and fixes

| Bug | Symptom | Fix |
|---|---|---|
| Stale closure on append | Tokens randomly dropped under fast streams | Functional updater `setState(s => ...)` or a ref |
| No SSE buffer | `JSON.parse` errors on partial frames | Rolling buffer, split on `\n\n`, keep trailing fragment |
| Re-parsing full markdown per token | UI jank on long responses | `React.memo` + throttled flush |
| Backend buffers response | No streaming; long time-to-first-token | `FlushAsync` per frame; `no-transform`; nodejs runtime on BFF |
| Abort doesn't cancel backend | Azure OpenAI keeps billing after stop | Forward `RequestAborted` as `CancellationToken` |
| Leaked `AbortController` | Aborts fire after unmount | Clean up in the effect / store in ref |
