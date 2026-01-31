# 🌐 API Communication Patterns

APIs provide different ways for systems to talk to each other. Here are
the most common communication patterns.

------------------------------------------------------------------------

## 🔹 1. Request--Response (Synchronous APIs)

-   **Style:** Client sends a request → Server responds.\
-   **Examples:** REST, GraphQL, gRPC (unary calls).\
-   **Best for:** CRUD operations, fetching resources, client-server
    apps.

👉 Analogy: Ordering food at a restaurant and waiting for it to arrive
🍽️.

------------------------------------------------------------------------

## 🔹 2. Streaming (Continuous Data)

-   **Style:** Instead of one response, data streams over time.\
-   **Examples:** gRPC streaming, WebSockets, Server-Sent Events (SSE).\
-   **Best for:** Real-time apps (chat, stock tickers, dashboards).

👉 Analogy: Watching Netflix 🎥 --- you don't download the whole movie,
you stream it.

------------------------------------------------------------------------

## 🔹 3. Publish--Subscribe (Event-Driven APIs)

-   **Style:** Clients subscribe to topics; server publishes updates
    when events happen.\
-   **Examples:** MQTT, Kafka, Firebase Realtime DB, WebSockets
    (pub/sub).\
-   **Best for:** Notifications, IoT, event-driven microservices.

👉 Analogy: Subscribing to a YouTube channel 📺 --- you get notified
when new videos arrive.

------------------------------------------------------------------------

## 🔹 4. Batch & Bulk APIs

-   **Style:** Client sends one big request with multiple operations,
    server processes in bulk.\
-   **Examples:** Bulk REST endpoints, GraphQL batch queries.\
-   **Best for:** Data imports/exports, analytics, syncing large
    datasets.

👉 Analogy: Shopping wholesale 🛒 --- instead of buying one item at a
time, you buy in bulk.

------------------------------------------------------------------------

## 🔹 5. Webhooks (Server-to-Client Callbacks)

-   **Style:** Server calls back a client URL when something happens.\
-   **Examples:** Stripe (payment success webhooks), GitHub (push
    events).\
-   **Best for:** Event notifications without polling.

👉 Analogy: Giving your phone number 📱 --- they call you when there's
news.

------------------------------------------------------------------------

## 🔹 6. Asynchronous Messaging (Queue-Based)

-   **Style:** Client sends a message to a queue, server processes it
    later.\
-   **Examples:** RabbitMQ, AWS SQS, Azure Service Bus.\
-   **Best for:** Background jobs, long-running tasks, decoupled
    systems.

👉 Analogy: Posting a letter 📬 --- you don't wait at the mailbox;
recipient handles it later.

------------------------------------------------------------------------

## 🔹 Cheat Sheet Comparison

  -----------------------------------------------------------------------
  Pattern                           Examples         When to Use
  --------------------------------- ---------------- --------------------
  **Request--Response**             REST, GraphQL,   Standard
                                    gRPC             client-server APIs

  **Streaming**                     gRPC streaming,  Real-time apps
                                    WebSockets, SSE  

  **Pub--Sub**                      MQTT, Kafka,     Event-driven,
                                    Firebase         notifications

  **Batch/Bulk**                    Bulk REST,       Data imports/exports
                                    GraphQL batch    

  **Webhooks**                      Stripe, GitHub   Event callbacks

  **Async Messaging**               RabbitMQ, SQS    Background jobs,
                                                     decoupled
                                                     microservices
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## ✅ Summary

-   **Request--Response** → Most common (REST, GraphQL, gRPC).\
-   **Streaming** → For real-time, continuous data.\
-   **Pub--Sub** → For broadcasting events to many consumers.\
-   **Batch/Bulk** → For large data processing.\
-   **Webhooks** → Server calls you when something happens.\
-   **Async Messaging** → Decouple systems with queues.
