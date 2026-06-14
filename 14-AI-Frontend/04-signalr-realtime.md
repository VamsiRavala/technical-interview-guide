# SignalR for Real-Time Agent Telemetry

> Agent runs are long-lived and asynchronous — a planning step, a tool call, a retrieval, a generation. Polling is wasteful and laggy. **SignalR** pushes status/trace events to the UI in real time. This file covers `@microsoft/signalr`, wrapping the connection in a hook, Entra-secured connections, reconnect, and when to use SignalR vs SSE. React hook/effect fundamentals are assumed (section **01-React-JS**).

---

## Why SignalR for agent runs

A single chat *response* is one-way server→client and maps perfectly to SSE (`01-streaming-responses.md`). But an agent *platform* needs more: long-running background orchestration, multiple event types ("planning", "retrieving", "executing tool 3 of 5"), multiple clients watching the same run, and the client subscribing/unsubscribing to specific runs. That is SignalR's sweet spot — transport negotiation (WebSockets → SSE → long-polling fallback), automatic reconnection, and groups (`Clients.Group(runId)`).

```text
ASP.NET Core agent orchestration
   │  emits structured events per step
   ▼
SignalR Hub (groups keyed by runId, [Authorize])
   │  WebSockets (token via query string on handshake)
   ▼
useAgentHub(runId) ──► node-state updates ──► dashboard / React Flow canvas
```

## The connection hook

Build the connection in an effect, register handlers, subscribe to the run group, and clean up on unmount. Supply the Entra token via `accessTokenFactory` so it is fetched fresh on every (re)connect.

```typescript
// hooks/useAgentHub.ts
import { useEffect, useRef, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  HttpTransportType,
  LogLevel,
} from "@microsoft/signalr";
import { msalInstance, apiRequest } from "@/auth/msalConfig";

export interface AgentEvent {
  runId: string;
  step: string;
  status: "started" | "running" | "tool_call" | "completed" | "failed";
  detail?: string;
  ts: string;
}

export function useAgentHub(runId: string) {
  const [events, setEvents] = useState<AgentEvent[]>([]);
  const connRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_BASE}/hubs/agent`, {
        // SignalR auth: provide the same scoped access token, fetched fresh each connect.
        accessTokenFactory: async () => {
          const account = msalInstance.getActiveAccount()!;
          const r = await msalInstance.acquireTokenSilent({ ...apiRequest, account });
          return r.accessToken;
        },
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Warning)
      .build();

    conn.on("AgentStatus", (e: AgentEvent) => {
      if (e.runId === runId) setEvents((prev) => [...prev, e]);
    });

    conn.start().then(() => conn.invoke("SubscribeToRun", runId)).catch(console.error);
    connRef.current = conn;

    return () => {
      conn.invoke("UnsubscribeFromRun", runId).catch(() => {});
      conn.stop();
    };
  }, [runId]);

  return { events, connection: connRef.current };
}
```

### Key gotchas

- **Auth is via `accessTokenFactory`, not an `Authorization` header.** The WebSocket handshake can't carry custom headers, so SignalR sends the token as a query-string parameter the server validates in `OnConnectedAsync`. Use HTTPS so it isn't exposed in transit.
- **Always wire `withAutomaticReconnect`** and re-join groups on the `reconnected` event — group membership is lost on a dropped connection.
- **Scope subscriptions to a `runId`** (groups) so a busy hub doesn't flood every client.
- **Clean up on unmount** (`conn.stop()`) or you leak connections.
- **Debounce / batch high-frequency events** so a chatty run doesn't cause render storms.

## Securing the hub (Entra ID)

Protect the hub with `[Authorize]`, validate the token's audience and scope, and map the authenticated user to groups. Never trust a client-supplied `runId` without checking the user actually owns/can-see that run.

```csharp
// AgentHub.cs
[Authorize]
public class AgentHub : Hub
{
    private readonly IRunAuthorizationService _authz;
    public AgentHub(IRunAuthorizationService authz) => _authz = authz;

    public async Task SubscribeToRun(string runId)
    {
        var userId = Context.UserIdentifier!;
        if (!await _authz.CanViewRunAsync(userId, runId))
            throw new HubException("Forbidden");           // verify ownership
        await Groups.AddToGroupAsync(Context.ConnectionId, runId);
    }

    public Task UnsubscribeFromRun(string runId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, runId);
}
```

```csharp
// Server pushes a step event to just the run's group
await _hub.Clients.Group(runId).SendAsync("AgentStatus", new {
    runId, step = "retrieve", status = "running", ts = DateTime.UtcNow
});
```

For production, use the **Azure SignalR Service** (offloads connection management, scales out across instances) with managed identity between your app and the service.

## Handling reconnection and token refresh

On a dropped connection, `withAutomaticReconnect` retries with the supplied backoff schedule, calls `accessTokenFactory` again (so an expired token is refreshed automatically), and fires `onreconnected` — where you must **re-subscribe to groups**.

```typescript
conn.onreconnecting(() => setStatus("reconnecting"));
conn.onreconnected(async () => {
  setStatus("connected");
  await conn.invoke("SubscribeToRun", runId); // groups were lost — rejoin
});
```

For offline/VPN flakiness, surface a "reconnecting" banner and queue any user actions; never show an optimistic success that lies about delivery.

## Driving UI state from events

The same `AgentEvent` stream feeds two surfaces: an event log/timeline (this file) and a React Flow node graph (`07-workflow-visualization.md`). Map an event onto node state:

```typescript
function applyRunEvent(nodes: Node[], e: AgentEvent): Node[] {
  return nodes.map((n) =>
    n.id === e.step
      ? { ...n, data: { ...n.data, status:
          e.status === "completed" ? "done" :
          e.status === "failed" ? "failed" : "running" } }
      : n
  );
}
```

## SSE vs SignalR — choosing per feature

| | SSE / `ReadableStream` | SignalR |
|---|---|---|
| Direction | One-way server→client | Bidirectional |
| Transport | Plain HTTP | WebSockets → SSE → long-poll fallback |
| Reconnect | You handle it | Built in (`withAutomaticReconnect`) |
| Groups / multiplexing | No | Yes (`Clients.Group`) |
| Auth headers | Yes (fetch + bearer) | Query-string token via `accessTokenFactory` |
| Best for | A single response token stream | Long-lived run telemetry, multi-client, notifications |

**In the same app, use both:** SSE for the chat token stream, SignalR for the run/trace dashboard and live workflow status. They are complementary, not competing.

## Interview angle

"SignalR vs SSE for agent status?" — SSE is one-way and rides plain HTTP, perfect for token streaming of a single response. SignalR is bidirectional with groups, reconnect, and transport fallback — the right tool for multiplexed, long-lived run telemetry where the client subscribes/unsubscribes and the server pushes to specific groups. SignalR auth uses `accessTokenFactory` because the WS handshake can't carry headers; always re-join groups on reconnect and verify the user is authorized for any `runId` they subscribe to.
