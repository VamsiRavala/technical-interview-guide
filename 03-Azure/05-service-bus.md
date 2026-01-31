# Azure Service Bus:
- Azure Service Bus is a fully managed enterprise messaging service provided by Microsoft Azure.
- It’s designed to enable reliable communication between applications and services—even when they’re running independently, at different times, or in different environments.

# Core Concept:
 - Azure Service Bus is a message broker that decouples applications by using queues and topics to manage communication between components.
 - It’s useful for asynchronous messaging, load leveling, and reliable event-driven systems.

# Messaging Patterns:
   ## Queue (Point-to-Point):
  - Messages are sent to a queue and processed by a single consumer.
  - Each message is consumed only once.
  - Ideal for background processing, job queues, or task scheduling.

    ## Topic and Subscription (Publish/Subscribe):
  - A message is published to a topic and can be received by multiple subscriptions.
  - Each subscription acts like its own queue.
  - Ideal for event distribution to multiple services.

# Key Features:

| Feature	| Description| 
|----------|------------|
| Reliable Messaging | Guarantees message delivery even if receivers are temporarily offline.|
| Message Ordering | Supports FIFO (First In, First Out) with sessions.|
| Dead-lettering	| Stores messages that can’t be processed for later inspection.| 
| Duplicate Detection	| Prevents processing the same message more than once.| 
| Transactions	| Supports atomic send/receive operations across multiple entities.| 
| Auto-forwarding	| Chains queues/topics together to build complex message flows.| 
| Scheduled Delivery	| Allows delayed or time-based message delivery.| 

# Security:
- Uses Azure Active Directory (AAD) or Shared Access Signatures (SAS) for authentication.
- Supports role-based access control (RBAC).
- All data is encrypted in transit and at rest.

# Typical Use Cases
- Decoupling microservices.
- Processing orders or background jobs asynchronously.
- Integrating legacy systems with cloud applications.
- Event distribution across multiple subscribers.

# Integration Examples
- Azure Functions or Logic Apps can trigger from Service Bus messages.
- .NET, Java, Python, and Node.js SDKs are available.
- Can be combined with Event Grid or Event Hubs for advanced event-driven architectures.

# Duplicate Detection:
```bash
az servicebus queue create \
  --resource-group myResourceGroup \
  --namespace-name myNamespace \
  --name myQueue \
  --enable-duplicate-detection true \
  --duplicate-detection-history-time-window PT10M
```
- enable-duplicate-detection true turns it on.
- PT10M sets the detection window to 10 minutes (ISO 8601 duration format).

 - **Detection Window Details **
  
|Parameter|	Description
|-----------|------------|
|Default	|Disabled|
|Min Window	|20 seconds|
|Max Window	|7 days|
|How it works	|The MessageId is cached in memory for the specified window. After that, it’s purged and the message ID can be reused.|

# who is responsible for message id:
- and a very important one when working with Azure Service Bus duplicate detection
- The sender (your application or service) is responsible for assigning the MessageId property when sending messages to Azure Service Bus.
- Azure does not generate this ID for you in any meaningful way for duplicate detection — unless you explicitly set it, each message automatically gets a random GUID assigned by the SDK.
- If you don’t set it yourself → every message has a different random ID → no duplicates will ever be detected.
- If you want duplicate detection to work → you must assign a consistent ID for logically identical messages.

# MessageId Rules
|Rule	|Description|
|-----------|------------|
|Type	|Must be a string (max 128 characters).|
|Required	|Optional, but critical for duplicate detection.|
|Scope	|Unique per queue/topic within the duplicate detection window.|
|Common Strategy	|Use business keys or deterministic IDs — e.g., order number, payment ID, request UUID.|

# Best practices for duplicate detection:
- Always set MessageId manually in your producer code.
- Use a deterministic, idempotent ID (e.g., same ID if the same logical event is retried).
- If your system retries failed sends automatically, make sure the retry logic reuses the same MessageId.
- Avoid using timestamps or random values for MessageId unless duplicates are impossible.

# Message Settlement Methods in C#
- When you receive a message from Azure Service Bus, it’s not automatically removed from the queue.
- You need to explicitly tell Service Bus whether the message was:
- ✅ Successfully processed, or
- ⚠️ Failed (and should be retried or dead-lettered).

# Message Settlement Methods in C#

|Method	| Meaning	| Result |
|-----------|------------|------------|
| CompleteMessageAsync(message)	| ✅ Success	 | Removes message from the queue | 
| AbandonMessageAsync(message)	| ⚠️ Temporary failure | 	Makes message available for reprocessing | 
||DeadLetterMessageAsync(message, reason, description) |	❌ Unrecoverable failure	Moves message to Dead-Letter Queue | 
| DeferMessageAsync(message)	| ⏸️ Delay processing	 | Keeps message in queue but marked as deferred | 

# Example 1: Auto-Complete Disabled (Best Practice):
```csharp
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

class Program
{
    static string connectionString = "<YOUR_SERVICE_BUS_CONNECTION_STRING>";
    static string queueName = "orders";

    static async Task Main()
    {
        await using var client = new ServiceBusClient(connectionString);
        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock  // Default
        });

        Console.WriteLine("Listening for messages...");

        while (true)
        {
            ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

            if (message != null)
            {
                try
                {
                    string body = message.Body.ToString();
                    Console.WriteLine($"Received message: {body}, ID: {message.MessageId}");

                    // Process your message logic here
                    ProcessOrder(body);

                    // ✅ Mark as successfully processed
                    await receiver.CompleteMessageAsync(message);
                    Console.WriteLine($"Message {message.MessageId} completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Processing failed: {ex.Message}");
                    
                    // ⚠️ Option 1: Retry later
                    await receiver.AbandonMessageAsync(message);

                    // ❌ Option 2 (uncomment if needed): Move to DLQ
                    // await receiver.DeadLetterMessageAsync(message, "ProcessingError", ex.Message);
                }
            }
        }
    }

    static void ProcessOrder(string body)
    {
        // Simulate order processing
        if (body.Contains("FAIL")) throw new Exception("Simulated failure");
    }
}
```

# Example 2: Auto-Complete Enabled (Simpler, Less Control)
```csharp
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

class Program
{
    static string connectionString = "<YOUR_CONNECTION_STRING>";
    static string queueName = "orders";

    static async Task Main()
    {
        await using var client = new ServiceBusClient(connectionString);

        var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false  // Important: Manual control
        });

        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync();

        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        await processor.StopProcessingAsync();
    }

    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        Console.WriteLine($"Received message: {body}, ID: {args.Message.MessageId}");

        try
        {
            // Process business logic
            ProcessOrder(body);

            // ✅ Mark as processed
            await args.CompleteMessageAsync(args.Message);
            Console.WriteLine($"Completed message: {args.Message.MessageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    static void ProcessOrder(string body)
    {
        if (body.Contains("FAIL"))
            throw new Exception("Simulated failure");
    }
}
```

- when autocomplete who will mark the message as completed or failed one? what basis

  |Setting	|Who Completes	|When	| On Exception |
  |-----------|------------|------------|------------|
  |AutoCompleteMessages = true (default)	|SDK automatically	|After handler finishes successfully	|SDK calls Abandon → message retried|
  |AutoCompleteMessages = false	|You manually |	After explicit CompleteMessageAsync()	|You decide (Abandon, DLQ, Defer)|
<img width="547" height="856" alt="image" src="https://github.com/user-attachments/assets/c9c9f39f-cc0d-4e52-bf71-d7bcd5dbbe35" />

# Topics:
  ## What Are Topics and Subscriptions?
  - A Topic in Azure Service Bus is like a queue that supports multiple independent subscribers.
  - Each Subscription acts like a virtual queue attached to the Topic.
  - When a message is sent to a Topic, all active subscriptions receive a copy (subject to filters, if any)

    ```scss
    Publisher → [Topic: "OrderEvents"]
                     ├── SubscriptionA (Sends email)
                     ├── SubscriptionB (Updates database)
                     └── SubscriptionC (Triggers analytics)
    ```

- you can forward message to different queue as well


<img width="1362" height="867" alt="image" src="https://github.com/user-attachments/assets/b6b70d8e-d258-44c8-a573-fb7e42884a12" />

**Filtering**
<img width="1877" height="872" alt="image" src="https://github.com/user-attachments/assets/e20510a6-d673-4b3a-a763-f5ae4c2eb38e" />


## Service bus limitation:

| Category	| Key Points	| 
|-----------|------------|
| Message | Size	Up to 100 MB (Premium), 256 KB (Standard/Basic)| 
| TTL	| Up to 14 days| 
| Duplicate | Detection Window	20 s – 7 days| 
| Subscriptions | per Topic	Up to 2,000| 
| Protocol | Support	AMQP 1.0 (default), HTTPS REST API| 
| Recommended Tier	| Premium for enterprise-grade throughput, VNET, and reliability| 

# supported protocol
| Protocol	| Description	| Use Case	Notes| 
|-----------|------------|------------|
| AMQP 1.0 (Advanced Message Queuing Protocol)	| Binary, efficient, open standard messaging protocol	Default for all Azure SDKs (.NET, Python, Java, Node.js, etc.) | 	✅ Recommended for all client apps | 
| HTTPS (REST API)	| HTTP-based API following AtomPub conventions	Management tasks or low-throughput systems	| ✅ Good for admin scripts or integrations| 
| SBMP (Service Bus Messaging Protocol)	| Legacy proprietary Microsoft protocol	Older .NET SDKs (pre-2017)	| ⚠️ Deprecated — not supported in new SDKs| 
  
