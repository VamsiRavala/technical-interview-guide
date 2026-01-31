# Azure — Complete Cloud Services Guide

> Comprehensive guide to Microsoft Azure services, concepts, and interview preparation.

---

## 📚 Table of Contents

1. **[Core Concepts](01-core-concepts.md)** - Azure fundamentals and core services
2. **[Regions & Availability Zones](02-regions-availability-zones.md)** - Geographic distribution and high availability
3. **[Azure Functions](03-azure-functions.md)** - Serverless compute platform
4. **[Durable Functions](04-durable-functions.md)** - Stateful serverless orchestrations
5. **[Service Bus](05-service-bus.md)** - Enterprise messaging service
6. **[Event Hubs](06-event-hubs.md)** - Big data streaming platform
7. **[Event Grid](07-event-grid.md)** - Event-driven architecture
8. **[Storage Account](08-storage-account.md)** - Cloud storage solutions
9. **[Storage Queue](09-storage-queue.md)** - Message queue service
10. **[Cosmos DB](10-cosmos-db.md)** - Globally distributed database
11. **[Notification Hubs](11-notification-hubs.md)** - Push notification service

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. Start with **Core Concepts** to understand Azure basics
2. Learn about **Regions & Availability Zones**
3. Understand **Storage Account** fundamentals
4. Introduction to **Azure Functions**

### Intermediate (Week 3-4)
1. Deep dive into **Service Bus** messaging patterns
2. Study **Event Hubs** for streaming scenarios
3. Learn **Cosmos DB** for NoSQL solutions
4. Explore **Event Grid** for event-driven architecture

### Advanced (Week 5-6)
1. Master **Durable Functions** orchestrations
2. Implement complex messaging patterns
3. Design globally distributed solutions
4. Optimize cost and performance

---

## 🔑 Key Service Categories

### Compute Services
- **Azure Functions**: Serverless, event-driven compute
- **Durable Functions**: Stateful function orchestrations
- App Service, Container Instances, Kubernetes Service

### Storage Services
- **Storage Account**: Blobs, Files, Tables, Queues
- **Cosmos DB**: Globally distributed NoSQL database
- SQL Database, PostgreSQL, MySQL

### Messaging Services
- **Service Bus**: Enterprise messaging with queues and topics
- **Event Hubs**: Big data ingestion and streaming
- **Event Grid**: Event-driven reactive programming
- **Storage Queue**: Simple message queue

### Notification Services
- **Notification Hubs**: Cross-platform push notifications
- SignalR Service for real-time communication

---

## 💡 Service Comparison

### Messaging Services Comparison

| Feature | Service Bus | Event Hubs | Event Grid | Storage Queue |
|---------|------------|------------|------------|---------------|
| **Use Case** | Enterprise messaging | Big data streaming | Event routing | Simple queuing |
| **Message Size** | 256 KB - 1 MB | 1 MB | 1 MB | 64 KB |
| **Ordering** | Yes (sessions) | Yes (partition) | No | No guarantee |
| **Transactions** | Yes | No | No | No |
| **Throughput** | Thousands/sec | Millions/sec | Millions/sec | Thousands/sec |
| **Cost** | Higher | Moderate | Low | Very Low |

### When to Use What?

| Scenario | Recommended Service |
|----------|-------------------|
| Order processing | Service Bus with sessions |
| IoT telemetry | Event Hubs |
| Microservices events | Event Grid |
| Background jobs | Storage Queue |
| Workflow orchestration | Durable Functions |
| File uploads | Blob Storage + Event Grid |

---

## 🎓 Common Interview Topics

### 1. Compute & Serverless
- Azure Functions triggers and bindings
- Durable Functions patterns (chaining, fan-out/fan-in, monitoring)
- Scaling and performance considerations
- Cold start mitigation

### 2. Storage
- Blob storage tiers (Hot, Cool, Archive)
- Storage redundancy options (LRS, GRS, ZRS, GZRS)
- Cosmos DB consistency levels
- Partitioning strategies

### 3. Messaging
- Service Bus vs Event Hubs vs Event Grid
- At-least-once vs exactly-once delivery
- Dead letter queues
- Message sessions and ordering

### 4. High Availability
- Availability Zones vs Availability Sets
- Geo-redundancy and failover
- SLA calculations
- Disaster recovery planning

### 5. Security
- Managed identities
- Azure Key Vault
- Virtual Networks and private endpoints
- Role-based access control (RBAC)

---

## 🚀 Getting Started

### Prerequisites
```bash
# Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login to Azure
az login

# Set default subscription
az account set --subscription "Your Subscription Name"
```

### Common Azure CLI Commands
```bash
# Resource Groups
az group create --name myResourceGroup --location eastus
az group list
az group delete --name myResourceGroup

# Storage Account
az storage account create --name mystorageaccount --resource-group myResourceGroup
az storage container create --name mycontainer --account-name mystorageaccount

# Azure Functions
az functionapp create --name myFunctionApp --resource-group myResourceGroup --consumption-plan-location eastus
az functionapp deployment source config-zip --name myFunctionApp --resource-group myResourceGroup --src app.zip

# Cosmos DB
az cosmosdb create --name mycosmosdb --resource-group myResourceGroup
az cosmosdb database create --name mycosmosdb --db-name mydatabase

# Service Bus
az servicebus namespace create --name myservicebus --resource-group myResourceGroup
az servicebus queue create --name myqueue --namespace-name myservicebus
```

---

## 📖 Architecture Patterns

### Event-Driven Architecture
```
[Event Source] → [Event Grid] → [Azure Functions]
                              → [Logic Apps]
                              → [Event Hubs]
```

### Microservices Communication
```
[Service A] → [Service Bus] → [Service B]
                           → [Service C]
```

### Data Ingestion Pipeline
```
[IoT Devices] → [Event Hubs] → [Stream Analytics] → [Cosmos DB]
                                                  → [Blob Storage]
```

### Serverless Processing
```
[HTTP Request] → [Function App] → [Service Bus] → [Durable Function]
[Blob Upload]  → [Event Grid]   → [Function]   → [Cosmos DB]
```

---

## 💰 Cost Optimization Tips

1. **Compute**
   - Use consumption plan for Functions when possible
   - Implement auto-scaling
   - Schedule resources (stop/start VMs when not needed)

2. **Storage**
   - Use appropriate storage tiers
   - Implement lifecycle management policies
   - Enable compression

3. **Messaging**
   - Right-size message brokers
   - Use batching where applicable
   - Clean up unused resources

4. **Monitoring**
   - Set up cost alerts
   - Use Azure Advisor recommendations
   - Review Cost Management regularly

---

## 🔒 Security Best Practices

1. **Identity & Access**
   - Use managed identities
   - Implement least privilege access
   - Enable MFA for all users

2. **Network Security**
   - Use virtual networks and subnets
   - Implement private endpoints
   - Configure NSGs properly

3. **Data Protection**
   - Enable encryption at rest and in transit
   - Use Azure Key Vault for secrets
   - Implement backup and disaster recovery

4. **Monitoring & Compliance**
   - Enable Azure Security Center
   - Configure diagnostic logs
   - Implement compliance policies

---

## 📚 Additional Resources

### Official Documentation
- [Azure Documentation](https://docs.microsoft.com/azure)
- [Azure Architecture Center](https://docs.microsoft.com/azure/architecture)
- [Azure Well-Architected Framework](https://docs.microsoft.com/azure/architecture/framework)

### Learning Paths
- [Microsoft Learn](https://docs.microsoft.com/learn/azure)
- Azure Certifications (AZ-900, AZ-104, AZ-204, AZ-305)

### Tools
- Azure Portal
- Azure CLI
- Azure PowerShell
- Visual Studio Code with Azure extensions

---

## 🎯 Interview Preparation Strategy

### Hands-On Practice
- Create sample applications using Azure services
- Implement common patterns (event-driven, microservices)
- Practice troubleshooting scenarios
- Understand pricing and cost optimization

### Key Topics to Master
- Service selection criteria
- Scaling and performance patterns
- High availability and disaster recovery
- Security and compliance
- Monitoring and diagnostics

### Sample Questions
- When would you use Service Bus vs Event Grid?
- How do you ensure high availability in Azure?
- Explain Durable Functions patterns
- How do you secure Azure Functions?
- What are Cosmos DB consistency levels?
- How do you optimize Azure costs?

---

## 🔗 Related Topics

- **DevOps**: Azure DevOps, CI/CD, Infrastructure as Code
- **Containers**: AKS, Container Instances, Container Registry
- **AI/ML**: Cognitive Services, Machine Learning, Bot Service
- **Monitoring**: Application Insights, Log Analytics, Azure Monitor

---

## 🤝 Contributing

Contributions are welcome! Please ensure:
- Content is accurate and up-to-date
- Examples are tested
- Formatting is consistent
- Include relevant Azure CLI commands

---

*Master Azure and build cloud-native applications!* ☁️
