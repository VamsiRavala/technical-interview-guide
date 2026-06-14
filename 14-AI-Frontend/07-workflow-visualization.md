# Workflow Visualization with React Flow

> Agent orchestrations and RAG pipelines are graphs: nodes (LLM call, tool, retriever, branch) and edges (data/control flow). **React Flow** (`@xyflow/react`) renders interactive, pannable graphs with custom nodes, and node *state* is driven by live SignalR run events. This file covers the canvas, custom nodes, mapping events onto nodes, and the editor-vs-monitor split. React component basics are assumed (section **01-React-JS**).

---

## Why a graph view

For explainability dashboards in multi-agent platforms, stakeholders need to see exactly which tools fired and where time went. A graph makes the orchestration legible: each agent step or pipeline stage (retrieve → rerank → generate → tool-call) is a node, and connections are edges showing data flow. The same component serves two modes — a read-only **monitor** that animates as the agent executes, and an editable **designer** for a low-code workflow builder.

```text
Backend workflow definition (JSON graph)  ──► nodes[] / edges[]  ──► React Flow canvas
                                                                         ▲
SignalR AgentStatus events  ──► applyRunEvent()  ──► node.data.status ───┘  (live animation)
```

## The canvas

```tsx
// components/WorkflowCanvas.tsx  ("use client")  — needs browser APIs, so a Client Component
import { ReactFlow, Background, Controls, type Node, type Edge } from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { AgentNode } from "./AgentNode";

const nodeTypes = { agent: AgentNode };

export function WorkflowCanvas({
  nodes,
  edges,
  mode = "monitor",
}: {
  nodes: Node[];
  edges: Edge[];
  mode?: "monitor" | "designer";
}) {
  const editable = mode === "designer";
  return (
    <div style={{ height: "70vh" }}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        nodesDraggable={editable}
        nodesConnectable={editable}
        elementsSelectable={editable}
        fitView
      >
        <Background />
        <Controls />
      </ReactFlow>
    </div>
  );
}
```

## Custom node reflecting live status

A custom node component renders rich content — label, kind, status, optionally tokens/latency — and `Handle`s define where edges connect.

```tsx
// components/AgentNode.tsx — custom node reflecting live run state
import { Handle, Position, type NodeProps } from "@xyflow/react";

type Status = "idle" | "running" | "done" | "failed";

export function AgentNode({
  data,
}: NodeProps<{ label: string; status: Status; kind: string }>) {
  return (
    <div className={`agent-node agent-node--${data.status}`}>
      <Handle type="target" position={Position.Left} />
      <strong>{data.label}</strong>
      <span className="kind">{data.kind}</span>
      {data.status === "running" && <span className="spinner" aria-label="running" />}
      <Handle type="source" position={Position.Right} />
    </div>
  );
}
```

Define node kinds (`LlmNode`, `ToolNode`, `RetrieverNode`, `BranchNode`) for richer monitoring, each showing its own metadata.

## Driving node state from SignalR

Node `data.status` is updated from the same `AgentEvent` stream the dashboard uses (`04-signalr-realtime.md`). Map an event keyed by node id onto the matching node.

```typescript
// Mapping SignalR events onto node state
function applyRunEvent(nodes: Node[], e: AgentEvent): Node[] {
  return nodes.map((n) =>
    n.id === e.step
      ? {
          ...n,
          data: {
            ...n.data,
            status:
              e.status === "completed" ? "done" :
              e.status === "failed" ? "failed" : "running",
          },
        }
      : n
  );
}
```

```tsx
// wire the hub events into React Flow state
const [nodes, setNodes] = useNodesState(initialNodes);
const { events } = useAgentHub(runId);

useEffect(() => {
  const last = events.at(-1);
  if (last) setNodes((ns) => applyRunEvent(ns, last));
}, [events]);
```

For correct node-event matching, the backend's `step` identifier must equal the node `id` in the graph definition — design that contract up front.

## Auto-layout

For DAGs, keep layout deterministic with a layout library (Dagre or ELK) rather than hand-positioning nodes. Run layout once on load (and after structural edits in designer mode), then let React Flow handle pan/zoom.

```typescript
import dagre from "@dagrejs/dagre";

function layout(nodes: Node[], edges: Edge[]): Node[] {
  const g = new dagre.graphlib.Graph().setGraph({ rankdir: "LR" });
  g.setDefaultEdgeLabel(() => ({}));
  nodes.forEach((n) => g.setNode(n.id, { width: 180, height: 64 }));
  edges.forEach((e) => g.setEdge(e.source, e.target));
  dagre.layout(g);
  return nodes.map((n) => {
    const { x, y } = g.node(n.id);
    return { ...n, position: { x: x - 90, y: y - 32 } };
  });
}
```

## Designer mode: editing and serialization

In designer mode, enable `onConnect`, `onNodesChange`, and `onEdgesChange` (via React Flow's `useNodesState`/`useEdgesState`), then serialize the graph to a workflow definition the backend can execute (e.g., a Semantic Kernel step sequence).

```typescript
function serialize(nodes: Node[], edges: Edge[]): WorkflowDefinition {
  return {
    nodes: nodes.map((n) => ({ id: n.id, kind: n.data.kind, config: n.data.config })),
    edges: edges.map((e) => ({ from: e.source, to: e.target })),
  };
}
```

Validate the graph **before** running: no disallowed cycles, all required inputs connected, node-level config valid against typed schemas. Offer a dry-run that streams a sample execution without calling Azure OpenAI for real.

## Editor vs monitor — one component

Keep a single `nodeTypes` registry and toggle interactivity by mode. In **monitor** mode set `nodesDraggable={false}` and feed live status into node `data`; in **designer** mode enable connection/change handlers and serialize. The node schema must map cleanly to the backend orchestration so the same definition both renders and executes.

## Interview angle

"Designer (editable) vs monitor (read-only) — same component?" — Mostly yes. React Flow supports both: in monitor mode you disable dragging/connecting and feed live SignalR status into node `data`; in designer mode you enable `onConnect`/`onNodesChange` and serialize the graph to an executable workflow definition. Keep one `nodeTypes` registry, toggle interactivity by mode, and make the node `id` match the backend `step` id so live events land on the right node.
