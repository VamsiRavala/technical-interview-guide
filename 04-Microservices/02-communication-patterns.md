# Communication Patterns Overview

How microservices talk to each other is one of the most important design decisions in a distributed system. This is the overview file covering the main communication styles — synchronous request-response, streaming, publish-subscribe, and asynchronous messaging — plus guidance on how to choose. REST, gRPC, and GraphQL each get their own dedicated topic files.

## API Communication Patterns

APIs provide different ways for systems to talk to each other. Here are the most common communication patterns.

### 1. Request-Response (Synchronous APIs)

- **Style:** Client sends a request, server responds.
- **Examples:** REST, GraphQL, gRPC (unary calls).
- **Best for:** CRUD operations, fetching resources, client-server apps.

Analogy: Ordering food at a restaurant and waiting for it to arrive.

### 2. Streaming (Continuous Data)

- **Style:** Instead of one response, data streams over time.
- **Examples:** gRPC streaming, WebSockets, Server-Sent Events (SSE).
- **Best for:** Real-time apps (chat, stock tickers, dashboards).

Analogy: Watching Netflix — you don't download the whole movie, you stream it.

### 3. Publish-Subscribe (Event-Driven APIs)

- **Style:** Clients subscribe to topics; server publishes updates when events happen.
- **Examples:** MQTT, Kafka, Firebase Realtime DB, WebSockets (pub/sub).
- **Best for:** Notifications, IoT, event-driven microservices.

Analogy: Subscribing to a YouTube channel — you get notified when new videos arrive.

### 4. Batch & Bulk APIs

- **Style:** Client sends one big request with multiple operations, server processes in bulk.
- **Examples:** Bulk REST endpoints, GraphQL batch queries.
- **Best for:** Data imports/exports, analytics, syncing large datasets.

Analogy: Shopping wholesale — instead of buying one item at a time, you buy in bulk.

### 5. Webhooks (Server-to-Client Callbacks)

- **Style:** Server calls back a client URL when something happens.
- **Examples:** Stripe (payment success webhooks), GitHub (push events).
- **Best for:** Event notifications without polling.

Analogy: Giving your phone number — they call you when there's news.

### 6. Asynchronous Messaging (Queue-Based)

- **Style:** Client sends a message to a queue, server processes it later.
- **Examples:** RabbitMQ, AWS SQS, Azure Service Bus.
- **Best for:** Background jobs, long-running tasks, decoupled systems.

Analogy: Posting a letter — you don't wait at the mailbox; the recipient handles it later.

## Cheat Sheet Comparison

| Pattern | Examples | When to Use |
|---|---|---|
| **Request-Response** | REST, GraphQL, gRPC | Standard client-server APIs |
| **Streaming** | gRPC streaming, WebSockets, SSE | Real-time apps |
| **Pub-Sub** | MQTT, Kafka, Firebase | Event-driven, notifications |
| **Batch/Bulk** | Bulk REST, GraphQL batch | Data imports/exports |
| **Webhooks** | Stripe, GitHub | Event callbacks |
| **Async Messaging** | RabbitMQ, SQS | Background jobs, decoupled microservices |

## Synchronous vs Asynchronous Communication

| Aspect | Synchronous | Asynchronous |
|---|---|---|
| **Flow** | Client waits for the response | Client sends and continues; processed later |
| **Coupling** | Tighter (caller depends on callee being up) | Looser (decoupled via a broker) |
| **Examples** | REST, gRPC unary, GraphQL queries | Message queues, event streams, pub/sub |
| **Best for** | Direct request/response, CRUD | Background jobs, long-running tasks, fan-out |
| **Risk** | Cascading failures if a downstream service is slow | Eventual consistency, harder debugging |

## Comparing the Common Technologies

A practical comparison of the transports you will most often choose between in a microservices or device ecosystem.

### REST (HTTP/1.1, JSON/XML)

- **Style:** Request/Response (stateless).
- **Best for:** CRUD operations, public APIs.
- **Pros:** Simple, human-readable, widely supported.
- **Cons:** Verbose (JSON/XML), no streaming, higher latency.

**Example:**
- Mobile app to dealer system: *"Get my last 5 service appointments"*.
- Dealer CRM to OEM backend: *"Fetch warranty details by VIN"*.

### gRPC (HTTP/2, Protobuf)

- **Style:** Strongly-typed RPC, supports streaming.
- **Best for:** Microservices, inter-service APIs.
- **Pros:** Fast (binary Protobuf), streaming support, cross-language.
- **Cons:** Not human-readable, browser clients need gRPC-Web.

**Example:**
- Infotainment to navigation service: *fetch traffic-aware routes*.
- Backend services to diagnostics, ticketing, vector DB.
- Autonomous stack: Perception to Planning modules.

### WebSockets

- **Style:** Persistent, full-duplex channel.
- **Best for:** Real-time push updates.
- **Pros:** Low latency, event-driven.
- **Cons:** Connection management overhead.

**Example:**
- Dealer dashboard: live complaint status updates.
- EV charging: live kWh delivered & cost updates.
- Car to mobile app: *"Battery at 80%"*.

### MQTT (IoT Protocol)

- **Style:** Publish/Subscribe, lightweight.
- **Best for:** IoT telemetry, unstable networks.
- **Pros:** Very lightweight, reliable.
- **Cons:** Not optimized for heavy backend processing.

**Example:**
- Car telemetry to cloud (speed, GPS, tire pressure).
- EV charging station to backend usage updates.

### Kafka / Event Streams

- **Style:** Distributed log + event streaming.
- **Best for:** Large-scale data pipelines.
- **Pros:** Scalable, fault-tolerant, event replay.
- **Cons:** Heavy infra, not for direct device communication.

**Example:**
- Fleet analytics: thousands of cars streaming data.
- Predictive maintenance: analyzing telemetry in real time.

### When to Use What

| Use Case | Best Tech | Why |
|---|---|---|
| CRUD ops (client to backend) | REST | Simple, standard |
| Microservices (inter-service) | gRPC | Strong typing, fast |
| Real-time updates (dashboards) | WebSocket | Persistent channel |
| Telemetry (device to cloud) | MQTT | Lightweight, reliable |
| Fleet/large-scale analytics | Kafka | Event streaming & replay |

### Ecosystem Diagram

```text
[Device] --Telemetry--> (MQTT Broker) --> [Cloud Backend]
[Device] --Sensor Data--> [gRPC Microservices] --> [Cloud Backend]
[Cloud Backend] --CRUD Ops--> [REST APIs - Dealer/CRM] --> [Dashboard]
[Cloud Backend] --Streaming Data--> (Kafka Cluster)
[Cloud Backend] --Live Updates--> [Mobile App via WebSocket] --> [Dashboard]
```

## How to Choose

- **Request-Response** → Most common (REST, GraphQL, gRPC).
- **Streaming** → For real-time, continuous data.
- **Pub-Sub** → For broadcasting events to many consumers.
- **Batch/Bulk** → For large data processing.
- **Webhooks** → Server calls you when something happens.
- **Async Messaging** → Decouple systems with queues.

Quick rules of thumb:

- **REST** → CRUD + external APIs.
- **gRPC** → High-performance inter-service comms.
- **WebSocket** → Real-time push.
- **MQTT** → Lightweight IoT telemetry.
- **Kafka** → Fleet-scale event analytics.
