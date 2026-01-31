# What Is Azure Storage Queue?
- Azure Storage Queues are part of Azure Storage Account, designed for simple, lightweight message queuing — usually for basic decoupling between app components.
- They’re ideal when you need a reliable, simple, inexpensive queue, not complex broker features.

# Core Characteristics
| Property	| Description| 
| -----------|-------------|
| Service Type	| Part of Azure Storage Account (uses REST over HTTPS)| 
| Message Type	| Text or Base64-encoded messages| 
| Message Size | Limit	64 KB per message| 
| Queue Size Limit	| Up to 500 TB total, limited by your storage account capacity| 
| Message Lifetime (TTL)	| Up to 7 days| 
| Protocol	| HTTPS REST (simple CRUD operations)| 
| Concurrency Model	| Client-managed (no broker lock management)| 
| Access	| Shared Access Signatures (SAS) or Account Keys| 

# When to Use Azure Storage Queues?

- Simplicity & Cost-Effectiveness
  - No need for advanced messaging (no topics, sessions, or transactions).
  - Works well for internal background jobs, batch tasks, or worker roles

- Decoupled Components in Simple Apps
    - Example: Frontend places a message, backend worker picks it up.
    - Suitable for serverless functions (Azure Function QueueTrigger).
 
- Massive Scale and Simplicity
  - You just need a large, simple, reliable queue — not a message bus.

- Integration with Azure Functions
  - Functions can automatically trigger from Storage Queue messages with minimal setup.
 
# Limitation
| **Capability** | **Azure Storage Queue** | **Service Bus Queue / Topic** |
|----------------|--------------------------|-------------------------------|
| **Message Size** | 64 KB | 256 KB (Standard) / 100 MB (Premium) |
| **Topics / Pub-Sub** | ❌ No | ✅ Yes (Topics & Subscriptions) |
| **Filters & Rules** | ❌ No | ✅ Yes (SQL & Correlation Filters) |
| **Sessions / FIFO** | ❌ No native FIFO | ✅ Supported (via Sessions) |
| **Duplicate Detection** | ❌ No | ✅ Yes |
| **Transactions** | ❌ No | ✅ Yes |
| **Dead-Letter Queue (DLQ)** | ✅ Optional, simple version | ✅ Advanced DLQ |
| **PeekLock / Delivery Semantics** | Basic visibility timeout | Advanced peek-lock, retry, defer |
| **Protocols** | REST/HTTPS only | AMQP 1.0, HTTPS |
| **Security / RBAC** | Storage keys, SAS | Azure AD, RBAC, SAS |
| **Ordering** | Not guaranteed | FIFO (with sessions) |
| **Batch Processing** | Limited | Full batch support |
| **Geo-disaster Recovery** | Limited | Supported in Standard/Premium |
