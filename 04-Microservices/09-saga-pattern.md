# Saga Pattern (Distributed Transactions)

## The Problem

- In monoliths, a single database transaction can span multiple tables with ACID guarantees.
- In microservices, each service has its own database (Database per Service).
- A single business process (e.g., placing an order) may touch multiple services/databases.
- Global transactions across services are hard due to: different DB technologies, network failures, and scalability concerns.

The Saga Pattern solves this.

## What is the Saga Pattern?

A **Saga** is a sequence of local transactions across services.

- Each local transaction updates its own database and then publishes an event.
- The next service listens for that event and executes its own local transaction.
- If one transaction fails, the saga triggers compensating transactions to undo previous changes.

This guarantees eventual consistency instead of strong ACID.

## Saga Execution Models

### 1. Choreography (Event-driven)

- Each service listens to events and reacts.
- No central coordinator.

Example:
1. `Order Service` creates the order, emits `OrderCreated`.
2. `Payment Service` processes payment, emits `PaymentProcessed`.
3. `Inventory Service` reserves stock, emits `StockReserved`.
4. `Shipping Service` ships the item.

If payment fails, `Order Service` receives `PaymentFailed` and cancels the order.

- Simple, decentralized.
- Harder to manage/debug complex flows.

### 2. Orchestration (Coordinator)

- A central **Saga Orchestrator** coordinates the workflow.
- The orchestrator tells each service what to do next.

Example:
1. Orchestrator tells `Payment Service` to process payment.
2. If success, tells `Inventory Service` to reserve stock.
3. If success, tells `Shipping Service` to ship.
4. If failure, the orchestrator issues compensating actions.

- Centralized control, easier to manage.
- The orchestrator can become a bottleneck.

## Example: E-commerce Order

### Choreography Flow

```text
Customer -> Order Service: Place Order
Order Service -> Payment Service: OrderCreated Event
Payment Service -> Order Service: PaymentSucceeded Event
Payment Service -> Inventory Service: PaymentSucceeded Event
Inventory Service -> Shipping Service: StockReserved Event
Shipping Service -> Customer: Order Shipped
```

### Orchestration Flow

```text
Customer -> Orchestrator: Place Order
Orchestrator -> Payment Service: Process Payment
Payment Service -> Orchestrator: Payment Result
Orchestrator -> Inventory Service: Reserve Stock
Inventory Service -> Orchestrator: Inventory Result
Orchestrator -> Shipping Service: Ship Order
Shipping Service -> Orchestrator: Shipping Result
Orchestrator -> Customer: Order Completed
```

## Benefits

- Works across different services/databases.
- No need for distributed ACID transactions.
- Enables eventual consistency.
- Resilient — supports compensation/rollback.

## Challenges

- More complex than traditional transactions.
- Requires careful design of compensating actions.
- Debugging sagas can be difficult.
- Eventual consistency may not suit all use cases.

## Saga Pattern and Messaging Queues

### Does Saga Require a Message Queue?

- No, it's not mandatory — but it's very common.
- Saga is a pattern, not a technology. It only defines how distributed transactions are managed (choreography or orchestration).
- Implementation options:
  - **Messaging-based** (Kafka, RabbitMQ, AWS SQS/SNS, Azure Service Bus).
  - **HTTP/REST** calls.
  - **gRPC** or other RPC protocols.

### Why Messaging Queues Are Common in Sagas

1. **Asynchronous communication** → services don't block.
2. **Reliability** → messages retried if delivery fails.
3. **Decoupling** → services don't need to know each other's addresses.
4. **Ordering guarantees** → Kafka ensures ordered streams.
5. **Event replay** → useful for debugging and rebuilding state.

### Choreography with Messaging Queue

- Services publish events to a topic/queue; other services subscribe and react.

Example:
- `OrderService` publishes `OrderCreated`.
- `PaymentService` consumes and publishes `PaymentProcessed`.
- `InventoryService` consumes and publishes `StockReserved`.
- `ShippingService` consumes and publishes `OrderShipped`.

### Orchestration with Messaging Queue

- The orchestrator controls the workflow but uses queue messages.

Example:
- Orchestrator sends `ProcessPayment` to the queue.
- `PaymentService` consumes it and sends back `PaymentResult`.
- Orchestrator then continues the workflow.

### Without Messaging Queue

- Possible with HTTP/gRPC calls.
- Simpler for small flows, but leads to:
  - Tight coupling.
  - Higher risk of cascading failures.
  - No built-in retries/durability.

### Flow Diagram with Messaging Queue

```text
Order Service -> Message Queue: Publish OrderCreated
Message Queue -> Payment Service: Consume OrderCreated
Payment Service -> Message Queue: Publish PaymentProcessed
Message Queue -> Inventory Service: Consume PaymentProcessed
Inventory Service -> Message Queue: Publish StockReserved
Message Queue -> Shipping Service: Consume StockReserved
Shipping Service -> Message Queue: Publish OrderShipped
```

## Summary

- **Saga Pattern** = a distributed transaction solution for microservices.
- Two styles: Choreography (event-driven) or Orchestration (coordinator).
- Ensures eventual consistency with compensating transactions.
- Ideal for e-commerce orders, IoT workflows, and booking systems.
- Saga does not require a message queue, but messaging (Kafka, RabbitMQ, Azure Service Bus) is the most natural fit — with a queue you get reliability, decoupling, and scalability; without one, flows tend to be tightly coupled and brittle.
