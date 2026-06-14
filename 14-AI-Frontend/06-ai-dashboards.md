# AI Dashboards: Agent Monitoring

> Operators need to see **runs, traces, token usage, and cost**. This file covers the data shape, charting token/cost with Recharts (or visx), recursive trace-tree rendering, and merging live SignalR updates with historical query data. Chart/component basics are assumed (section **01-React-JS**).

---

## The data shape

A monitoring view is built from a few read-only server resources plus a live event stream:

```text
/runs?from=&to=&status=     paginated historical runs (table)
/runs/{id}/trace            span tree (recursive: name, ms, tokens, children)
/metrics/cost?bucket=day    cost time series (USD precomputed on the backend)
/hubs/agent  (SignalR)      live AgentStatus / RunCompleted events
```

Each run has: status, latency, token counts (prompt + completion), a dollar cost, and a trace tree of spans (a planning step contains a retrieval span which contains a tool-call span, etc.).

## State strategy

The defining challenge is **two data sources on one view**: historical aggregates and live updates.

| Data | Source | Tool |
|---|---|---|
| Historical runs, cost/day, status breakdown | request/response | **TanStack Query** (cache, pagination, invalidation) |
| Live run rows + span updates | server push | **SignalR** (`useAgentHub`, see `04-signalr-realtime.md`) |
| Cross-panel filters (date range, model, status) | client-global | **Redux/Zustand** (only this) |

On a `RunCompleted` SignalR event, invalidate the relevant TanStack queries so historical aggregates refresh — don't try to manually patch every cached list.

```typescript
const qc = useQueryClient();
conn.on("RunCompleted", (e: { runId: string }) => {
  qc.invalidateQueries({ queryKey: ["runs"] });
  qc.invalidateQueries({ queryKey: ["metrics", "cost"] });
});
```

## Token and cost charts (Recharts)

Charts are read-only server state. Recharts is fast to build (use visx when you need more control). Compute **cost on the backend** — rates live there and change per model — and send pre-computed USD to the client.

```tsx
// components/TokenCostChart.tsx  ("use client")
import { ResponsiveContainer, AreaChart, Area, XAxis, YAxis, Tooltip, CartesianGrid } from "recharts";

interface CostPoint { date: string; promptTokens: number; completionTokens: number; usd: number; }

export function TokenCostChart({ data }: { data: CostPoint[] }) {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <AreaChart data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis yAxisId="tokens" />
        <YAxis yAxisId="usd" orientation="right" tickFormatter={(v) => `$${v}`} />
        <Tooltip formatter={(val, name) => (name === "usd" ? `$${val}` : val)} />
        <Area yAxisId="tokens" type="monotone" dataKey="promptTokens" stackId="t" />
        <Area yAxisId="tokens" type="monotone" dataKey="completionTokens" stackId="t" />
        <Area yAxisId="usd" type="monotone" dataKey="usd" fillOpacity={0.15} />
      </AreaChart>
    </ResponsiveContainer>
  );
}
```

Cost formula (computed server-side, charted client-side):

```text
cost = prompt_tokens * promptRate + completion_tokens * completionRate   (per model)
```

Chart cumulative spend plus a per-model breakdown. Pair with a `LatencyHistogram` and a runs/status bar.

## Trace span tree

An agent run is a tree of spans. Render it recursively with indentation reflecting depth.

```tsx
// Trace span tree — recursive render of an agent run's spans
interface Span { id: string; name: string; ms: number; tokens?: number; children: Span[]; }

function SpanRow({ span, depth = 0 }: { span: Span; depth?: number }) {
  return (
    <>
      <tr>
        <td style={{ paddingLeft: depth * 16 }}>{span.name}</td>
        <td>{span.ms} ms</td>
        <td>{span.tokens ?? "—"}</td>
      </tr>
      {span.children.map((c) => (
        <SpanRow key={c.id} span={c} depth={depth + 1} />
      ))}
    </>
  );
}
```

For deep trees, virtualize the flattened rows or lazy-expand children to keep rendering cheap.

## Merging live and historical data without flicker

The classic dashboard bug: a `RunCompleted` event adds a live row, then the next query refetch returns the same run, producing a duplicate or a flash. Mitigations:

- **Key by `runId`** and de-duplicate when merging live rows into the cached list.
- **Batch** rapid SignalR events (a burst of `AgentStatus`) into a single state commit per frame to avoid render storms under backpressure.
- Treat the query as the source of truth for *completed* runs and SignalR as the source for *in-flight* runs; on completion, let the query refetch take over and drop the live entry.

## Live runs table

```tsx
// merge in-flight (SignalR) runs above completed (TanStack Query) runs, deduped by id
function useRunsView() {
  const { data: historical = [] } = useQuery({ queryKey: ["runs"], queryFn: fetchRuns });
  const { events } = useAgentHub("*"); // subscribe to all (operator scope)

  return useMemo(() => {
    const live = new Map<string, RunRow>();
    for (const e of events) {
      if (e.status !== "completed") live.set(e.runId, toRunRow(e));
      else live.delete(e.runId); // completed — query owns it now
    }
    const byId = new Map(historical.map((r) => [r.id, r]));
    for (const [id, r] of live) byId.set(id, r);
    return [...byId.values()].sort((a, b) => b.startedAt.localeCompare(a.startedAt));
  }, [historical, events]);
}
```

## Communicating cost and latency to users

- Show **time-to-first-token** by streaming early — perceived speed beats total time.
- Surface raw latency numbers in **admin/observability** views, not consumer chat.
- For long agent runs show **step-by-step progress** (driven by SignalR), not an indeterminate spinner.
- For enterprise tools, show per-message token counts / estimated cost and a model badge for transparency and budgeting; offer "fast mode" vs "smart mode" if you do cost-aware routing.

## Stretch capabilities

- **Anomaly highlighting:** flag runs exceeding a cost/latency threshold.
- **Replay trace:** a timeline scrubber that re-animates a run's spans.
- **Feedback loop:** thumbs up/down per message POSTing `{ messageId, rating, model, promptVersion, sources, latency }` to feed an evaluation pipeline (e.g., Azure AI Foundry evaluations) for regression detection.

## Interview angle

"How do you compute and display cost?" — Cost is `prompt_tokens * promptRate + completion_tokens * completionRate` per model, summed per run. Compute it on the **backend** (rates live there and change), send pre-computed USD to the client, and chart cumulative + per-model breakdown. Live runs update via SignalR; historical aggregates via TanStack Query that you **invalidate** on `RunCompleted`. The tricky part is merging the two sources without flicker — key by `runId`, dedupe, batch live events, and let the query own completed runs.
