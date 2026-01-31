# Azure Event Hubs,
- which is a core building block for real-time data streaming and event ingestion on Azure 🚀
  - If Service Bus is about reliable messaging and workflows,
  - then Event Hub is about high-throughput event streaming and analytics.
# What Is Azure Event Hubs?
 - Azure Event Hubs is a big data streaming platform and event ingestion service — part of the Azure Messaging family.
 - It can collect, buffer, and deliver millions of events per second from devices, apps, or services in real time.
 - Think of it as Azure’s version of Kafka-as-a-service — a high-scale event ingestion pipeline for telemetry, logs, IoT data, and analytics.

# Core Concept:
| Concept	| Description| 
| -------|-------------|
| Event Hub Namespace	| A container for one or more Event Hubs (like a Kafka cluster).| 
| Event Hub | 	A specific stream (like a Kafka topic).| 
| Partition	| A unit of parallelism — events are distributed across partitions for scaling.| 
| Consumer Group	| A view of the event stream — each consumer group reads independently.| 
| Event Producer	| Sends (publishes) events to the Event Hub.| 
| Event Consumer	| Reads (subscribes) to events from the Event Hub.| 

# When to Use Event Hubs
| Use Case	| Example|
| -------|-------------|
| Telemetry ingestion | Collecting logs from thousands of devices or microservices|
| Real-time analytics | Streaming events to Azure Stream Analytics or Databricks|
| Event-driven processing| Triggers in Functions or Spark jobs|
| IoT streaming | Sensor and telemetry data from IoT devices|
| Integration with Kafka | Replacing or extending on-prem Kafka clusters| 

# Event Hubs Architecture Overview
```css
[ Produc​ers ] 
     ↓
[ Event Hub ] → [ Partitions (n) ] → [ Storage Retention ]
     ↓
[ Consumer Group A ] → Stream Processor A
[ Consumer Group B ] → Stream Processor B
```
# Key Features

| Feature	| Description| 
| -------|-------------|
|Massive throughput	| Millions of events per second|
|Low latency	| Near real-time ingestion|
|Partitioned consumers	| Parallel reads via partitions|
|Durable storage	| Events stored up to 90 days |
|Capture	| Auto-archive to Azure Blob Storage or Data Lake |
|Kafka-compatible endpoint	| Can use Kafka APIs directly|
|Auto-scaling	Automatically | adjusts throughput units|
|Geo-disaster recovery	| Namespace-level replication for resilience |


# Supported Protocols

- Azure Event Hubs supports multiple protocols for producers and consumers.

| Protocol	| Description	Used For| 
| -------|-------------|
| AMQP 1.0	| Default protocol for Azure SDKs	Most common, reliable| 
| HTTPS REST API	| Lightweight API	Low-frequency ingestion or management| 
| Kafka 1.0 API	| Kafka-compatible endpoint	Allows Kafka clients to connect natively| 
| Apache Avro	Event | serialization format	Efficient, compact, schema-friendly| 


# Overview: High-Level Analogy
| Concept	| Service Bus	| Event Hubs| 
| -------|-------------|-------------|
| Publisher / Producer	| Sends messages to a Queue or Topic	| Sends events to an Event Hub| 
| Subscriber	| A Subscription under a Topic	| ❌ Not applicable| 
| Consumer	| The app that reads from a Queue or Subscription | 	The app that reads from a Partition| 
| Consumer Group	❌|  Not applicable	| A view of the event stream for one or more consumers| 

# IF consumer A reads, consumer B in the same group receive same message or not?
- No — in Azure Event Hubs, if Consumer A reads a message, Consumer B in the same consumer group will not receive the same message.
- Only one consumer per partition per consumer group reads a given event.


Let’s say your Event Hub has 4 partitions, and your consumer group = “AnalyticsGroup”.

You deploy two consumers within that group:

Partition	Assigned Consumer
Partition 0	Consumer A
Partition 1	Consumer A
Partition 2	Consumer B
Partition 3	Consumer B

```css
Event Hub: TelemetryStream
 ├── Partition 0
 ├── Partition 1
 ├── Partition 2
 └── Partition 3

Consumer Group A: Analytics
 ├── Consumer A1 → P0, P1
 └── Consumer A2 → P2, P3

Consumer Group B: Monitoring
 ├── Consumer B1 → P0, P1
 └── Consumer B2 → P2, P3

```

# 🧩 Short Summary (Diffence b/w Azure Service bus vs Event Hub)
|Feature	|Event Hubs	|Service Bus|
| -------|-------------|-------------|
| Auto-delete messages	| ✅ (via retention window)	| ✅ (via TTL / auto-delete settings)| 
| Dead-letter queue (DLQ)	| ❌ No DLQ concept	| ✅ Yes, per queue or subscription| 
| Acknowledgement / Complete	| ❌ No manual ack; checkpointing instead	| ✅ Yes (CompleteMessageAsync, etc.)| 
| Message retry & delivery guarantee| 🔁 At-least-once (consumer-managed)	| ✅ Guaranteed delivery (broker-managed)| 


# Auto-Delete / Message Expiration
- Event Hubs automatically deletes old events based on the retention period you configure.
  - Default: 1 day
  - Premium (Dedicated tier): up to 90 days
 
# Code Sample:
```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;

namespace EventHubConsumerDemo
{
    class Program
    {
        // ✅ Replace with your actual values
        private const string eventHubsConnectionString = "<EVENT_HUBS_CONNECTION_STRING>";
        private const string eventHubName = "<EVENT_HUB_NAME>";
        private const string blobStorageConnectionString = "<BLOB_STORAGE_CONNECTION_STRING>";
        private const string blobContainerName = "<BLOB_CONTAINER_NAME>";

        // Consumer group — each app can have its own (default = "$Default")
        private const string consumerGroup = "$Default";

        static async Task Main()
        {
            Console.WriteLine("🚀 Starting Event Hub consumer...");

            // 1️⃣ Create a Blob container client (for storing checkpoints)
            var storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
            await storageClient.CreateIfNotExistsAsync();

            // 2️⃣ Create the Event Processor Client
            var processor = new EventProcessorClient(
                storageClient,
                consumerGroup,
                eventHubsConnectionString,
                eventHubName);

            // 3️⃣ Define message handler
            processor.ProcessEventAsync += ProcessEventHandler;

            // 4️⃣ Define error handler
            processor.ProcessErrorAsync += ProcessErrorHandler;

            // 5️⃣ Start the processor
            await processor.StartProcessingAsync();

            Console.WriteLine("Listening for events. Press any key to stop...");
            Console.ReadKey();

            // 6️⃣ Stop the processor cleanly
            await processor.StopProcessingAsync();

            Console.WriteLine("🛑 Event Hub consumer stopped.");
        }

        // ✅ Handler to process each event
        static async Task ProcessEventHandler(ProcessEventArgs args)
        {
            try
            {
                string partitionId = args.Partition.PartitionId;
                string data = Encoding.UTF8.GetString(args.Data.Body.ToArray());

                Console.WriteLine($"📩 Partition {partitionId}: {data}");

                // Process your business logic here
                await ProcessEventDataAsync(data);

                // ✅ Update checkpoint only after successful processing
                await args.UpdateCheckpointAsync(args.CancellationToken);
                Console.WriteLine($"✔️ Checkpoint updated for partition {partitionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error while processing event: {ex.Message}");
                // You could log or send this event to a custom DLQ/Event Hub here
            }
        }

        // ⚙️ Error handling for Event Hub or Blob Storage operations
        static Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"💥 Error in partition {args.PartitionId ?? "N/A"}: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        // 🧠 Example of business logic function
        static Task ProcessEventDataAsync(string data)
        {
            // Simulate work (e.g., parsing JSON, saving to DB, etc.)
            Console.WriteLine($"Processing event data: {data}");
            return Task.CompletedTask;
        }
    }
}

```

<img width="1578" height="893" alt="image" src="https://github.com/user-attachments/assets/31f2e870-6ccd-45d1-aca4-8c167265da06" />

<img width="1902" height="777" alt="image" src="https://github.com/user-attachments/assets/ce254309-81bf-4ad0-9e00-5a33563d484d" />

<img width="1901" height="641" alt="image" src="https://github.com/user-attachments/assets/6eae329d-0df6-4ac8-9e59-6a503e80c81c" />

<img width="1883" height="921" alt="image" src="https://github.com/user-attachments/assets/24c74d26-f9b7-4cce-a37f-0725bfbc2dac" />




