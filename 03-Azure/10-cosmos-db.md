# Azure Cosmos DB Overview

Azure **Cosmos DB** is Microsoft’s fully managed, **NoSQL database service** designed for **globally distributed**, **highly scalable**, and **low-latency** applications. It supports multiple data models and APIs, making it a versatile choice for modern app development.

---

## 🧩 Key Features

1. **Global Distribution**
   - Replicate your data across any number of Azure regions.
   - Enables multi-region writes and automatic failover for high availability.

2. **Multiple APIs / Data Models**
   Cosmos DB supports different APIs to suit various development needs:
   - **Core (SQL)** – For document (JSON) data using SQL-like syntax.
   - **MongoDB API** – For apps using MongoDB drivers.
   - **Cassandra API** – For column-family data.
   - **Gremlin API** – For graph data.
   - **Table API** – For key-value workloads.

3. **Guaranteed Performance (SLAs)**
   - 99.999% availability (multi-region).  
   - Single-digit millisecond latency.  
   - Throughput and latency guarantees under SLA.

4. **Automatic Scaling (Autoscale)**
   - Automatically adjusts RU/s (Request Units per second) based on usage.

5. **Multi-Master Writes**
   - Allows write operations in multiple regions simultaneously.

6. **Consistency Levels**
   Offers five tunable consistency models:
   - Strong  
   - Bounded staleness  
   - Session  
   - Consistent prefix  
   - Eventual

---

## ⚙️ Architecture Basics

- **Containers**: Logical units of storage for items (documents, key-value pairs, etc.).
- **Databases**: Collections of containers.
- **Partitions**: Data is automatically partitioned using a partition key.
- **Request Units (RUs)**: Abstract measure of performance cost for database operations.

---

## 📊 Use Cases

- IoT and telemetry data storage.
- Real-time analytics.
- Retail personalization systems.
- Social media applications.
- Gaming leaderboards and telemetry.

---

## 🔐 Security and Compliance

- Data encryption at rest and in transit.
- Managed identities for Azure services.
- Role-based access control (RBAC).
- Compliance with ISO, HIPAA, SOC, GDPR, etc.

---

## 🧠 Example Query (SQL API)

```sql
SELECT c.id, c.name, c.city
FROM c
WHERE c.city = "Berlin"
```

This retrieves all documents where the city field equals “Berlin”.

---

## 🚀 Integration with Other Azure Services

- **Azure Functions** – Event-driven apps using Cosmos DB triggers.
- **Azure Synapse Link** – Real-time analytics without ETL.
- **Power BI** – Visualization and dashboards.
- **Azure Data Factory** – ETL workflows.

---

# 💻 Using Azure Cosmos DB with .NET

Below is a basic example demonstrating how to create a Cosmos DB database and container, and insert/query data using the **Azure Cosmos DB .NET SDK**.

## Step 1: Install NuGet Package

```bash
dotnet add package Microsoft.Azure.Cosmos
```

## Step 2: Initialize Cosmos Client

```csharp
using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

class Program
{
    private static readonly string connectionString = "<YOUR_COSMOS_DB_CONNECTION_STRING>";
    private static readonly string databaseId = "SampleDB";
    private static readonly string containerId = "Items";

    static async Task Main(string[] args)
    {
        CosmosClient client = new CosmosClient(connectionString);
        Database database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
        Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");

        Console.WriteLine("Database and container created successfully!");

        // Insert a document
        var item = new { id = "1", name = "John Doe", city = "Berlin" };
        await container.CreateItemAsync(item, new PartitionKey(item.id));

        Console.WriteLine("Item inserted successfully!");

        // Query the document
        var query = new QueryDefinition("SELECT * FROM c WHERE c.city = @city").WithParameter("@city", "Berlin");
        using FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(query);
        while (resultSet.HasMoreResults)
        {
            foreach (var doc in await resultSet.ReadNextAsync())
            {
                Console.WriteLine($"Found item: {doc}");
            }
        }
    }
}
```

## Step 3: Run the Application

```bash
dotnet run
```

This program connects to Azure Cosmos DB, creates a database and container (if they don't exist), inserts a document, and queries it.

---

✅ **Tip:** Use environment variables or Azure Key Vault for secure management of your connection string.



<img width="1907" height="895" alt="image" src="https://github.com/user-attachments/assets/3d54540e-51ce-4c77-a30b-fde1147a527d" />

<img width="1436" height="838" alt="image" src="https://github.com/user-attachments/assets/4f368e11-e502-42ec-823c-5181e9067223" /> 

<img width="773" height="553" alt="image" src="https://github.com/user-attachments/assets/7b60aa30-dc48-49fc-b35a-f4c27cdfa910" />

# ⚖️ Azure Cosmos DB Consistency Levels

Cosmos DB provides **five tunable consistency levels**, allowing developers to balance **data accuracy (consistency)** and **performance (latency + availability)** according to application needs.

Think of them as a **spectrum** between **strong consistency** (most accurate) and **eventual consistency** (most performant).

---

## 1. Strong Consistency
- **Guarantee:** Reads always return the most recent committed version of an item.  
- **Scenario:** Banking systems or real-time financial transactions where correctness is critical.
- **Cons:** Higher latency (as data must replicate across all regions).Lower availability during network partitions (per CAP theorem).

```csharp
CosmosClientOptions options = new CosmosClientOptions
{
    ConsistencyLevel = ConsistencyLevel.Strong
};
```

---

## 2. Bounded Staleness
- **Guarantee:** Reads lag behind writes by a specific time or operation count.  
- **Scenario:** Stock market tickers or live dashboards that tolerate minimal delay.
- **Cons:** Still higher latency than session or eventual., More complex to configure and manage.
```csharp
CosmosClientOptions options = new CosmosClientOptions
{
    ConsistencyLevel = ConsistencyLevel.BoundedStaleness
};
```

---

## 3. Session Consistency (Default)
- **Guarantee:** Within a user session, reads reflect their own writes immediately.  
- **Scenario:** Social media posts, e-commerce carts, or user settings.
- **Cons:** Different users may see slightly outdated data temporarily..
```csharp
CosmosClientOptions options = new CosmosClientOptions
{
    ConsistencyLevel = ConsistencyLevel.Session
};
```

---

## 4. Consistent Prefix
- **Guarantee:** Reads never see data out of order, but may be slightly stale.  
- **Scenario:** IoT telemetry where the order of readings must be preserved.
- **cons:** You might not see the latest data immediately.
  
```csharp
CosmosClientOptions options = new CosmosClientOptions
{
    ConsistencyLevel = ConsistencyLevel.ConsistentPrefix
};
```

---

## 5. Eventual Consistency
- **Guarantee:** Data replicas converge eventually, no ordering or staleness guarantees.  
- **Scenario:** News feeds, recommendation engines, or analytics dashboards.

```csharp
CosmosClientOptions options = new CosmosClientOptions
{
    ConsistencyLevel = ConsistencyLevel.Eventual
};
```

---

## 🧭 Summary Table

| Consistency Level  | Data Freshness | Latency | Order Guarantee | Best For |
|--------------------|----------------|----------|----------------|-----------|
| **Strong** | Always latest | Highest | ✅ | Banking, Finance |
| **Bounded Staleness** | Slight delay | High | ✅ | Real-time analytics |
| **Session** | Latest in session | Medium | ✅ | Social media, e-commerce |
| **Consistent Prefix** | Ordered but stale | Low | ✅ | IoT telemetry |
| **Eventual** | Eventually consistent | Lowest | ❌ | News feeds, recommendations |

---

✅ **Tip:** Choose the consistency level based on your app’s data accuracy vs performance tradeoff.




