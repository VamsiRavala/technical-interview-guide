# Azure Event Grid
 - 👉 Azure Event Grid, the event routing and reactive automation service.

- If you think of the three Azure messaging options —

| Purpose	| Service | 
| ----------|--------|
| Reliable messaging / workflow	| Service Bus| 
| High-throughput data streaming| 	Event Hubs| 
| Reactive event distribution	| Event Grid| 

- then Event Grid is what connects events from anywhere to anything — in near real-time.

- Azure Event Grid is a fully managed event routing service built on the publish–subscribe model.
- It enables event-driven architectures (EDA) by allowing you to react to events (e.g., file uploaded, VM deleted, IoT alert, custom business events) in real time.
- It’s optimized for routing, not message storage or long-term queuing — think of it as a scalable event notification system.

# When to Use Event Grid
- Use Event Grid when you need

| **Scenario** | **Example** |
|---------------|-------------|
| **Reactive, event-driven workflows** | Trigger Azure Functions when a blob is created |
| **Serverless integration** | Automatically notify services of state changes |
| **Fan-out event delivery** | Send one event to multiple handlers (e.g., webhook, Function, Logic App) |
| **Custom business events** | Publish your own events (like “OrderCreated”, “PaymentSucceeded”) |
| **Lightweight pub/sub** | Use as a backbone between multiple apps or microservices |
| **Cloud automation** | Automatically respond to Azure resource events (e.g., “VM Created”) |


# Architecture Overview
```css
Event Source  ──►  Event Grid Topic  ──►  Event Subscription  ──►  Event Handler

```
- An event source (e.g., Blob Storage, Event Hub, or custom app) publishes an event.

- The event goes into an Event Grid Topic.

- One or more Event Subscriptions define which handlers should receive which events.

- Event Handlers (e.g., Function, Logic App, Webhook, Service Bus) process the event.

  # Event Sources (Producers)
  - Azure Blob Storage Blob created/deleted
  - Azure Blob Storage	Blob created/deleted
  - Azure Event Hubs	Capture or processing completed
  - Azure IoT Hub	Device connected/disconnected
  - Azure Resource Manager (ARM)	Resource group created/deleted
  - Azure Media Services	Job finished
  - Custom applications	Custom business events
 
  # Event Handlers (Consumers)
  - Azure Function	Run code in response to events
  - Azure Logic App	Orchestrate workflows visually
  - Azure Automation	Runbooks for cloud automation
  - Service Bus Queue/Topic	Route events to reliable messaging
  - Event Hubs	Forward events for analytics or streaming
  - WebHook / API Endpoint	Notify external systems or apps
 
  # Filtering & Routing (Important Topic)
  - Event Grid lets you define subscriptions with filters so that only relevant events go to specific handlers.
  - Filtering Types:
       - Filter Type	Description
       - Subject filtering	Match event subject path (e.g. /images/)
       - Event type filtering	e.g., BlobCreated, BlobDeleted
       - Advanced filtering	Match event data fields (e.g., data.size > 1000)
       - Prefix/Suffix filtering	Match event subjects by filename or path
   
  ```json
  {
  "subjectBeginsWith": "/images/",
  "eventType": ["BlobCreated"],
  "advancedFilters": [
    {
      "operatorType": "NumberGreaterThan",
      "key": "data.contentLength",
      "value": 1000
    }
  ]
}
```

Delivery & Reliability
Mechanism	Description
Push delivery	Event Grid pushes events to subscriber endpoints via HTTPS POST
Retry policy	Retries for up to 24 hours with exponential backoff
Dead-letter destination	Failed deliveries (after retries) go to Azure Storage Blob container
Event order	No guaranteed global ordering (per subscription basis only)
Durability	Guaranteed delivery (at least once) within retry window
