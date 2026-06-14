# Chat UI Patterns

> A production chat surface is far more than a `<div>` of messages. This file covers the AI-specific UX: virtualizing long conversations, optimistic turns, rendering citations and tool calls, sanitizing untrusted model output, error/retry, and accessibility. React fundamentals (component design, hooks, refs) are assumed — see section **01-React-JS**.

---

## The message model is the source of truth

Everything below depends on messages being a typed array, not DOM-derived state. A stable `id` per message is what makes optimistic reconciliation, virtualization keys, regenerate, and branching all work.

```typescript
type Role = "user" | "assistant" | "system";
type MessageStatus = "pending" | "streaming" | "complete" | "stopped" | "error";

interface ToolCall { id: string; name: string; args: unknown; output?: unknown; status: "pending" | "succeeded" | "failed"; }
interface Citation { id: string; title: string; url?: string; snippet: string; chunkId: string; }

interface ChatMessage {
  id: string;            // stable; temp client id swapped for server id on reconcile
  role: Role;
  content: string;
  status: MessageStatus;
  toolCalls?: ToolCall[];
  citations?: Citation[];
  model?: string;
  tokens?: { prompt: number; completion: number };
}
```

## Message-list virtualization

Long conversations (hundreds of turns, each rendering markdown and code) destroy scroll performance because the DOM grows unbounded. Virtualize with `@tanstack/react-virtual`, rendering only the visible window plus a small overscan. The hard parts are **variable row heights** (markdown measures differently per message) and **stick-to-bottom while streaming** without yanking a user who has scrolled up to read history.

```tsx
// components/MessageList.tsx  ("use client")
import { useVirtualizer } from "@tanstack/react-virtual";
import { useRef, useEffect, useState } from "react";

export function MessageList({ messages, isStreaming }: {
  messages: ChatMessage[];
  isStreaming: boolean;
}) {
  const parentRef = useRef<HTMLDivElement>(null);
  const [pinnedToBottom, setPinnedToBottom] = useState(true);

  const v = useVirtualizer({
    count: messages.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 120,
    measureElement: (el) => el.getBoundingClientRect().height, // dynamic heights
    overscan: 6,
  });

  // Only auto-scroll while streaming AND only if the user hasn't scrolled away.
  useEffect(() => {
    if (isStreaming && pinnedToBottom) {
      v.scrollToIndex(messages.length - 1, { align: "end" });
    }
  }, [messages.length, isStreaming, pinnedToBottom]);

  return (
    <div
      ref={parentRef}
      role="log"
      aria-live="polite"
      className="msg-scroll"
      onScroll={(e) => {
        const el = e.currentTarget;
        setPinnedToBottom(el.scrollHeight - el.scrollTop - el.clientHeight < 80);
      }}
    >
      <div style={{ height: v.getTotalSize(), position: "relative" }}>
        {v.getVirtualItems().map((item) => (
          <div
            key={messages[item.index].id}
            data-index={item.index}
            ref={v.measureElement}
            style={{ position: "absolute", top: 0, transform: `translateY(${item.start}px)`, width: "100%" }}
          >
            <MessageBubble message={messages[item.index]} />
          </div>
        ))}
      </div>
    </div>
  );
}
```

A pragmatic refinement: **exempt the last in-progress message from virtualization** and render it normally, so its rapidly changing height during streaming doesn't fight the virtualizer's measurement.

## Optimistic UI

Append the user's message to local state with a temporary client-generated id and `pending` status **before** the network confirms it, then show an empty assistant bubble and fill it from the stream. The key is reconciliation: when the server returns the canonical message id, swap the temp id — using a stable React `key` so the bubble doesn't remount.

```tsx
// inside a useReducer for the live turn
function reducer(state: ChatMessage[], action: Action): ChatMessage[] {
  switch (action.type) {
    case "ADD_USER_MSG":
      return [...state, { id: action.tempId, role: "user", content: action.text, status: "pending" }];
    case "ADD_ASSISTANT_PLACEHOLDER":
      return [...state, { id: action.tempId, role: "assistant", content: "", status: "streaming" }];
    case "APPEND_DELTA":
      return state.map((m) => m.id === action.id ? { ...m, content: m.content + action.delta } : m);
    case "RECONCILE_ID": // swap temp id for server id on completion
      return state.map((m) => m.id === action.tempId ? { ...m, id: action.serverId, status: "complete" } : m);
    case "SET_ERROR":
      return state.map((m) => m.id === action.id ? { ...m, status: "error" } : m);
    default:
      return state;
  }
}
```

Do **not** optimistically render assistant *content* — you don't know it until tokens arrive. Only the user bubble and the typing placeholder appear instantly.

## Rendering citations (RAG)

RAG answers carry structured source references. Return them as a typed array and have the model emit inline markers like `[1]`; render the markers deterministically on the client from the array — never trust the model to emit citation HTML. Citations often arrive in a **separate SSE frame after the token stream**, so buffer and apply them when they land.

```tsx
function Citations({ citations }: { citations: Citation[] }) {
  return (
    <ul className="citations">
      {citations.map((c, i) => (
        <li key={c.id}>
          <button aria-label={`Source ${i + 1}: ${c.title}`} onClick={() => openSource(c)}>
            [{i + 1}] {c.title}
          </button>
        </li>
      ))}
    </ul>
  );
}
```

If retrieval found nothing, render a clear "no relevant sources" state — never fabricate a citation. For compliance-sensitive Microsoft customers, also expose a "Sources" panel listing every retrieved chunk with relevance scores so users can verify grounding.

## Rendering tool / function calls

Tool calls are a distinct message type, not free text. The backend streams `{ type: "tool_call", name, args }` and `{ type: "tool_result", output }` events (this maps directly to Semantic Kernel function-calling and the Microsoft Agent Framework). Render them as collapsible cards with a human-readable label by default and raw args on expand, visually subordinate to the final answer.

```tsx
function MessageBubble({ message }: { message: ChatMessage }) {
  return (
    <article className={`bubble bubble--${message.role}`}>
      <StreamedMarkdown text={message.content} isStreaming={message.status === "streaming"} />

      {message.toolCalls?.map((t) => (
        <details key={t.id} className={`tool-call tool-call--${t.status}`}>
          <summary>
            {t.status === "pending" ? "Calling" : "Called"} <code>{t.name}</code>
          </summary>
          <pre>{JSON.stringify(t.args, null, 2)}</pre>
          {t.output != null && <pre className="tool-output">{JSON.stringify(t.output, null, 2)}</pre>}
        </details>
      ))}

      {message.citations?.length ? <Citations citations={message.citations} /> : null}
    </article>
  );
}
```

For multi-step agents, render the tool calls as a step timeline ("Searching… → Analyzing… → Generating") rather than a flat list.

## Sanitizing markdown and code (untrusted output)

**Treat all model output as hostile.** A RAG document, tool result, or prompt-injected payload can carry XSS. Use `react-markdown` with `remark-gfm`, disable raw-HTML passthrough, and sanitize with `rehype-sanitize` (or DOMPurify if you must inject HTML). Never use `dangerouslySetInnerHTML` on raw output. Constrain links to `rel="noopener noreferrer"` and block `javascript:`/`data:` URIs.

```tsx
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeSanitize from "rehype-sanitize";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";

<ReactMarkdown
  remarkPlugins={[remarkGfm]}
  rehypePlugins={[rehypeSanitize]} // strips scripts, iframes, on* handlers
  components={{
    code({ inline, className, children }) {
      const lang = /language-(\w+)/.exec(className ?? "")?.[1];
      return inline
        ? <code className={className}>{children}</code>
        : <SyntaxHighlighter language={lang}>{String(children)}</SyntaxHighlighter>;
    },
    a: ({ href, children }) => <a href={href} rel="noopener noreferrer" target="_blank">{children}</a>,
  }}
>
  {content}
</ReactMarkdown>
```

### Incomplete markdown during streaming

Streaming markdown is often half-formed (an unclosed code fence, a dangling `**`). Re-parsing the full buffer on every delta is expensive and flickers as elements open and close. Pragmatic approach: render streaming text as plain/preformatted (detect an open fence and put partial content in a `<pre>`), then do one full markdown parse on `done`. Throttle re-parses to ~30-60ms regardless.

## Error, retry, and rate-limit UX

Distinguish error classes and never lose the user's typed text:

| Class | Examples | UX |
|---|---|---|
| Transient | 429, 503, network | Auto-retry with backoff; subtle "retrying…"; manual Retry after exhaustion |
| Rate limit | 429 + `Retry-After` | Read the header, show a countdown, don't hammer |
| Content filter | Azure Content Safety reject | Clear non-technical message, keep prompt recoverable |
| Auth | 401 | Re-acquire token (MSAL silent), retry once |
| Bad request | 400 | Surface a fixable message; don't auto-retry |

Keep the typed prompt recoverable so a failed send never discards it. Never show raw stack traces; log a correlation id for support. For resilience, the BFF can fall back to a secondary model and the UI shows a "using backup model" badge.

## Accessibility (WCAG 2.1 AA matters for Microsoft enterprise/gov customers)

- Announce streaming responses via an `aria-live="polite"` region, but **throttle** announcements — read on completion or sentence chunks, not every token.
- Composer is a labeled `<textarea>`; Send/Stop buttons have accessible names; focus returns to the composer after send.
- Tool-call cards and citations are keyboard-navigable and expose `aria-expanded`.
- Don't rely on color alone for user vs assistant; maintain code-block contrast.
- Respect `prefers-reduced-motion` for typing/caret animations; use `role="status"` for status indicators; provide skip links past long histories.

## The composer (auto-resize + IME)

An auto-growing `textarea` that grows to a max then scrolls. Handle Enter-to-send / Shift+Enter-for-newline, but **respect IME composition**: check `event.nativeEvent.isComposing` (or `keyCode === 229`) so confirming a Japanese/Chinese/Korean candidate with Enter doesn't accidentally send. Mishandling IME is a classic bug that makes the app unusable for CJK input. Disable Send when empty or while streaming; keep focus in the composer after send.

```tsx
function onKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
  if (e.key === "Enter" && !e.shiftKey && !e.nativeEvent.isComposing) {
    e.preventDefault();
    send();
  }
}
```

## Regenerate and edit-and-resend

Store messages with stable ids and parent links. **Regenerate** re-sends the prior user turn plus context to produce a new assistant message (optionally a sibling node for version history). **Edit and resend** replaces a user message and truncates everything after it (downstream turns are now invalid context), then re-streams. For comparison/branching, model the conversation as a tree with a `currentPath`; for simplicity, linear-with-truncation is acceptable. The message array — not the DOM — is always the source of truth.
