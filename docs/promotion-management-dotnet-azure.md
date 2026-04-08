# Complete Study Material: Promotions Management System
## High-Level Architecture, Design & Concepts Focus

---

# TABLE OF CONTENTS

1. [System Architecture Overview](#1-system-architecture-overview)
2. [Microservices Design Patterns](#2-microservices-design-patterns)
3. [Azure Services - When & Why](#3-azure-services)
4. [API Design & Management](#4-api-design--management)
5. [Database Strategy](#5-database-strategy)
6. [Event-Driven Architecture](#6-event-driven-architecture)
7. [DevOps & CI/CD Strategy](#7-devops--cicd-strategy)
8. [Docker & Kubernetes](#8-docker--kubernetes)
9. [Security Architecture](#9-security-architecture)
10. [React Frontend Architecture](#10-react-frontend-architecture)
11. [Retail Domain / POS Knowledge](#11-retail-domain--pos)
12. [Monitoring & Observability](#12-monitoring--observability)
13. [Interview Q&A - 100+ Questions](#13-interview-qa)
14. [System Design Scenarios](#14-system-design-scenarios)

---

# 1. SYSTEM ARCHITECTURE OVERVIEW

## 1.1 Complete System Flow

**Promotions Management System - High Level Data Flow:**

• User creates promotion in React UI
• Request goes through Azure CDN/WAF for security
• Azure API Management (APIM) acts as single entry point
• APIM handles authentication, rate limiting, request validation
• Request routed to appropriate microservice based on endpoint
• Microservice processes business logic
• Data persisted in appropriate database (SQL or Cosmos DB)
• Domain event published to Azure Service Bus
• Event triggers downstream services asynchronously
• Analytics service updates read models
• Notification service sends alerts
• Store service syncs with POS terminals
• React UI updates in real-time via SignalR or polling

## 1.2 Microservice Decomposition

**Core Microservices in Promotions System:**

• **Promotion Service**: CRUD operations, lifecycle management (Draft → Active → Expired), business rule validation, promotion templates, campaign management

• **Pricing Engine**: Real-time price calculation, discount application, conflict resolution when multiple promotions apply, price history tracking, margin validation

• **Store Service**: Store master data, store grouping, POS terminal mapping, store-specific rules, regional configurations, store hierarchy

• **Identity Service**: User authentication (OAuth 2.0/OIDC), role-based access control (RBAC), permission management, audit logging, multi-tenancy support

• **Analytics Service**: Redemption tracking, ROI calculation, performance metrics, trend analysis, reporting, dashboards, data warehousing

• **Notification Service**: Email notifications, SMS alerts, push notifications, in-app messages, POS alerts, escalation rules

• **Integration Service**: External system integration, POS sync, ERP connectivity, legacy system bridges, webhook management, third-party APIs

• **Approval Workflow Service**: Multi-level approval chains, escalation rules, SLA management, approval history, delegation support

## 1.3 Data Flow - Promotion Lifecycle

**Promotion State Transitions:**

• **Draft** → Created by user, editable, not yet submitted
• **Pending Approval** → Submitted for review, awaiting approver action
• **Approved** → Passed all approvals, ready to activate
• **Active** → Currently running, customers can redeem
• **Paused** → Temporarily stopped, can be resumed
• **Expired** → End date passed, no longer available
• **Cancelled** → Manually cancelled by admin, no redemptions allowed
• **Rejected** → Approval denied, can be edited and resubmitted

**Events Published at Each Transition:**

• Draft → Pending: PromotionSubmittedEvent (triggers approval workflow)
• Pending → Approved: PromotionApprovedEvent (updates cache, notifies team)
• Approved → Active: PromotionActivatedEvent (syncs to POS, updates pricing)
• Active → Expired: PromotionExpiredEvent (cleanup, archive data)
• Active → Paused: PromotionPausedEvent (removes from POS, updates pricing)
• Any → Cancelled: PromotionCancelledEvent (refund logic, audit trail)

## 1.4 Technology Stack Summary

**Frontend Layer:**
• React 18+ with TypeScript
• Redux/Context API for state management
• Material-UI or Ant Design for components
• Axios/Fetch for API calls
• SignalR for real-time updates

**API Gateway Layer:**
• Azure API Management (APIM)
• Request/response transformation
• Rate limiting and throttling
• JWT token validation
• API versioning and routing

**Backend Services:**
• .NET Core 8 (C#)
• RESTful APIs with minimal APIs
• gRPC for service-to-service communication
• Entity Framework Core for ORM

**Data Layer:**
• Azure SQL Database (transactional data)
• Cosmos DB (NoSQL for high-read scenarios)
• Azure Blob Storage (documents, images)
• Azure Redis Cache (caching layer)

**Messaging & Events:**
• Azure Service Bus (queues and topics)
• Apache Kafka (optional, for high-throughput scenarios)
• Event Grid (for event routing)

**Serverless:**
• Azure Functions (event processing, scheduled tasks)
• Azure Logic Apps (approval workflows)

**Security:**
• Azure Key Vault (secrets management)
• Azure AD / Azure AD B2C (authentication)
• Managed Identity (service-to-service auth)

**Containers & Orchestration:**
• Docker (containerization)
• Azure Kubernetes Service (AKS) for orchestration
• Azure Container Registry (image storage)

**CI/CD:**
• Azure DevOps Pipelines or GitHub Actions
• Infrastructure as Code (Terraform/ARM templates)
• GitOps for deployment

**Monitoring & Logging:**
• Application Insights (APM)
• Azure Monitor
• Log Analytics
• ELK Stack (optional)

---

# 2. MICROSERVICES DESIGN PATTERNS

## 2.1 Key Patterns Used

**Communication Patterns:**
• **Synchronous (Request-Response)**: REST APIs for immediate responses, gRPC for inter-service calls requiring low latency
• **Asynchronous (Event-Driven)**: Service Bus for decoupled communication, eventual consistency
• **API Gateway Pattern**: APIM as single entry point, hides service complexity, handles cross-cutting concerns
• **Service Discovery**: Kubernetes DNS for service discovery in AKS, health checks for availability

**Data Management Patterns:**
• **Database per Service**: Each microservice owns its database, no shared databases
• **CQRS (Command Query Responsibility Segregation)**: Separate read and write models, optimize each independently
• **Event Sourcing**: Store all changes as immutable events, rebuild state from events
• **Saga Pattern**: Distributed transactions across services, compensating transactions for rollback
• **Outbox Pattern**: Guarantee event publishing with database transaction, background worker publishes to broker

**Reliability Patterns:**
• **Circuit Breaker**: Fail fast when service unavailable, prevent cascading failures (Polly library)
• **Retry with Exponential Backoff**: Retry failed requests with increasing delays
• **Bulkhead Isolation**: Isolate resources to prevent one failure affecting others
• **Health Checks**: Kubernetes liveness and readiness probes
• **Fallback**: Provide degraded service when dependencies fail

**Deployment Patterns:**
• **Blue-Green Deployment**: Two identical environments, switch traffic instantly
• **Canary Deployment**: Route small percentage of traffic to new version, monitor metrics
• **Rolling Updates**: Gradually replace old instances with new ones
• **Feature Flags**: Toggle features without redeployment

## 2.2 API Gateway Pattern (Azure APIM)

**Why API Gateway:**

• Single entry point for all clients (web, mobile, third-party)
• Centralized authentication and authorization
• Rate limiting and throttling at gateway level
• Request/response transformation and validation
• API versioning and routing
• SSL/TLS termination
• Caching of responses
• Logging and monitoring
• API documentation and developer portal

**APIM Responsibilities:**

• **Authentication**: Validate JWT tokens, OAuth 2.0 flows
• **Authorization**: Check user permissions before routing
• **Rate Limiting**: Throttle requests per user/API key
• **Transformation**: Convert request/response formats
• **Caching**: Cache GET responses to reduce backend load
• **Routing**: Route requests to correct microservice
• **Versioning**: Support multiple API versions simultaneously
• **Monitoring**: Track API usage, performance metrics
• **Security**: WAF integration, IP whitelisting, SSL enforcement

**Request Flow Through APIM:**

• Client sends request to APIM endpoint
• APIM validates request format and size
• APIM checks rate limits for client
• APIM validates JWT token in Authorization header
• APIM checks cache for GET requests
• APIM transforms request if needed
• APIM routes to backend service
• Backend service processes request
• APIM caches response (if applicable)
• APIM transforms response
• APIM returns to client

## 2.3 CQRS Pattern (Command Query Responsibility Segregation)

**Why CQRS:**

• Write model optimized for transactional consistency
• Read model optimized for query performance
• Scale read and write independently
• Different data structures for different use cases
• Easier to understand business operations

**Write Side (Commands):**

• CreatePromotionCommand: Validate, persist, publish event
• UpdatePromotionCommand: Modify existing promotion
• ActivatePromotionCommand: Change status, trigger sync
• CancelPromotionCommand: Soft delete, compensate

**Read Side (Queries):**

• GetPromotionQuery: Fetch single promotion
• ListPromotionsQuery: Paginated list with filters
• SearchPromotionsQuery: Full-text search
• GetAnalyticsQuery: Aggregated metrics

**Data Synchronization:**

• Write to Azure SQL (normalized, transactional)
• Publish event to Service Bus
• Event handler updates Cosmos DB (denormalized, optimized for reads)
• Read queries hit Cosmos DB for performance
• Eventual consistency between write and read models

**Handling Eventual Consistency:**

• Version numbers on read model
• Timestamp on data for freshness
• Notification to client when read model updated
• Accept stale data for non-critical queries
• Use write model for critical operations

## 2.4 Saga Pattern (Distributed Transactions)

**Problem Being Solved:**

• Promotion activation requires multiple services
• Pricing engine needs to update prices
• Store service needs to sync to POS
• Notification service needs to alert
• If any step fails, need to rollback all

**Choreography Saga (Event-Driven):**

• Promotion Service publishes PromotionActivatingEvent
• Pricing Engine listens, updates prices, publishes PricesUpdatedEvent
• Store Service listens, syncs to POS, publishes PromotionDistributedEvent
• Notification Service listens, sends alerts, publishes NotificationsSentEvent
• Promotion Service listens, marks as ACTIVE

**Compensation on Failure:**

• If Pricing Engine fails: publish PricesUpdateFailedEvent
• Promotion Service listens, reverts status to APPROVED
• Store Service listens, removes from POS
• Notification Service listens, sends failure notification

**Orchestration Saga (Centralized):**

• Promotion Service initiates workflow
• Saga Orchestrator (Azure Logic App) coordinates steps
• Orchestrator calls Pricing Engine API
• Orchestrator calls Store Service API
• Orchestrator calls Notification Service API
• If any fails, orchestrator triggers compensation

**When to Use:**

• Choreography: Simple workflows, few services, loose coupling preferred
• Orchestration: Complex workflows, many steps, need clear control flow

## 2.5 Circuit Breaker Pattern

**States:**

• **Closed**: Normal operation, requests pass through
• **Open**: Service unavailable, requests fail immediately
• **Half-Open**: Testing if service recovered, limited requests allowed

**Implementation in .NET:**

• Use Polly library for resilience policies
• Configure failure threshold (e.g., 5 consecutive failures)
• Configure timeout duration (e.g., 30 seconds)
• Configure half-open test requests (e.g., 3 requests)

**Example Scenario:**

• Pricing Engine calls Product Service
• Product Service returns 500 errors
• After 5 failures, circuit opens
• Subsequent requests fail immediately (fast fail)
• After 30 seconds, circuit goes to half-open
• Send test request to Product Service
• If successful, circuit closes
• If fails, circuit opens again

**Benefits:**

• Prevents cascading failures
• Allows failing service time to recover
• Provides fast feedback to client
• Enables fallback strategies

## 2.6 Outbox Pattern (Reliable Event Publishing)

**Problem:**

• Update database AND publish event must be atomic
• If publish fails after DB update, event lost
• If DB update fails after publish, inconsistency

**Solution:**

• Single database transaction for both
• Write domain event to OutboxMessages table
• Commit transaction
• Background worker polls OutboxMessages
• Publishes to Service Bus
• Marks as processed
• Handles idempotency (same event published twice is safe)

**Implementation Steps:**

• When creating promotion, insert into Promotions table
• In same transaction, insert into OutboxMessages table
• Commit transaction
• Background job (Azure Function) polls OutboxMessages every 5 seconds
• For unprocessed messages, publish to Service Bus
• Update Processed = true
• If publish fails, retry with exponential backoff
• Dead-letter queue for messages that fail after max retries

**Guarantees:**

• Event is ALWAYS published if database write succeeds
• No event loss due to publish failures
• Handles service restarts gracefully
• Idempotent processing (safe to process same event twice)

---

# 3. AZURE SERVICES - WHEN & WHY

## 3.1 Azure SQL Database

**When to Use:**

• Transactional data requiring ACID guarantees
• Complex queries with joins and aggregations
• Data with fixed schema
• Relational data with foreign keys
• Compliance requirements (audit trails, data retention)

**Promotions System Usage:**

• Promotions table (core data)
• Campaigns table (grouping promotions)
0• PromotionRules table (business rules)
• Stores table (store master)
• Users table (user management)
• AuditLog table (compliance)
• OutboxMessages table (event publishing)

**Key Features:**

• Automatic backups and point-in-time restore
• Transparent Data Encryption (TDE)
• Row-level security (RLS)
• Dynamic data masking
• Threat detection
• Elastic pools for cost optimization
• Geo-replication for disaster recovery

**Pricing Models:**

• DTU (Database Transaction Unit): Bundled compute, memory, storage
• vCore: Pay per virtual core, more flexible
• Serverless: Auto-pause when idle, pay per second

## 3.2 Cosmos DB

**When to Use:**

• High read throughput scenarios
• Flexible/evolving schema
• Global distribution required
• Sub-millisecond latency needed
• High write throughput
• Document/JSON data
• Real-time analytics

**Promotions System Usage:**

• Product catalog (millions of products, high reads)
• Promotion read model (CQRS read side)
• Shopping cart/session data (TTL expiration)
• Event store (event sourcing)
• Analytics events (high volume writes)
• User preferences (flexible schema)

**Key Features:**

• Multi-region replication
• Automatic indexing
• TTL (Time to Live) for auto-expiration
• Change Feed for event processing
• Transactions within partition
• Partition key selection critical for performance

**Cosmos DB vs SQL Decision Matrix:**

• Fixed schema → SQL
• Flexible schema → Cosmos DB
• Complex queries → SQL
• Simple queries, high volume → Cosmos DB
• Strong consistency required → SQL
• Eventual consistency acceptable → Cosmos DB
• Vertical scaling → SQL
• Horizontal scaling → Cosmos DB
• Transactions across records → SQL
• Transactions within partition → Cosmos DB

## 3.3 Azure Service Bus

**When to Use:**

• Asynchronous communication between services
• Decoupling services (loose coupling)
• Guaranteed message delivery
• Message ordering required
• Dead-letter queue for failed messages
• Scheduled message delivery
• Duplicate detection needed

**Promotions System Usage:**

• Promotion events (PromotionCreatedEvent, PromotionActivatedEvent)
• POS sync queue (distribute promotions to stores)
• Notification queue (email, SMS, push)
• Analytics events queue
• Approval workflow events

**Queue vs Topic:**

• **Queue**: Point-to-point, single consumer, FIFO ordering
• **Topic**: Pub/Sub, multiple subscribers, filter by subscription

**Service Bus Features:**

• Dead Letter Queue (DLQ) for failed messages
• Sessions for ordered processing
• Scheduled delivery (send at specific time)
• Duplicate detection (prevent duplicate processing)
• Auto-forwarding (chain queues)
• Message TTL (expire old messages)
• Correlation ID for tracing

## 3.4 Azure Functions

**When to Use:**

• Event-driven processing
• Scheduled tasks
• Lightweight computations
• Serverless architecture (no infrastructure management)
• Pay per execution
• Auto-scaling needed

**Promotions System Usage:**

• **Timer Trigger**: Check for expired promotions every 5 minutes
• **Service Bus Trigger**: Process promotion events
• **HTTP Trigger**: On-demand report generation
• **Blob Trigger**: Process uploaded promotion images
• **Queue Trigger**: Process POS sync requests

**Pricing Models:**

• Consumption Plan: Pay per execution, auto-scale to 0
• Premium Plan: Reserved capacity, faster cold starts
• Dedicated Plan: App Service plan, guaranteed capacity

**Cold Start Considerations:**

• First invocation takes 1-2 seconds (Consumption Plan)
• Mitigate with Premium Plan or keep-alive pings
• Use in-process functions (.NET) for faster startup
• Pre-warm functions during off-peak hours

## 3.5 Azure Logic Apps

**When to Use:**

• Approval workflows
• Integration workflows
• Scheduled tasks
• Conditional logic
• Multi-step processes
• No-code/low-code solutions

**Promotions System Usage:**

• Promotion approval workflow (multi-level approvals)
• Escalation workflow (if approval not completed in time)
• Notification workflow (send emails, Teams messages)
• Integration workflow (sync with ERP)

**Advantages:**

• Visual workflow designer
• 400+ connectors (Office 365, Teams, SAP, etc.)
• Built-in retry and error handling
• Audit trail
• No code required
• Scheduled triggers

**Disadvantages:**

• Less flexible than code
• Higher cost for complex workflows
• Limited debugging capabilities
• Vendor lock-in

## 3.6 Azure Key Vault

**When to Use:**

• Store secrets (API keys, connection strings)
• Store certificates (SSL/TLS)
• Store keys (encryption keys)
• Centralized secret management
• Audit trail for secret access
• Rotation policies

**Promotions System Usage:**

• Database connection strings
• Service Bus connection strings
• API keys for external services
• JWT signing keys
• SSL certificates
• Storage account keys
• Third-party integration credentials

**Best Practices:**

• Use Managed Identity (no credentials in code)
• Enable soft-delete and purge protection
• Use Key Vault references in App Service config
• Rotate secrets regularly
• Separate Key Vaults per environment
• Use access policies or RBAC
• Enable logging and monitoring

## 3.7 Azure API Management (APIM)

**When to Use:**

• Expose APIs to internal/external consumers
• Centralized API governance
• Rate limiting and throttling
• API versioning
• Request/response transformation
• Developer portal needed

**Promotions System Usage:**

• Single entry point for all clients
• Authentication and authorization
• Rate limiting per customer
• Request validation
• Response caching
• API documentation
• Usage analytics

**Tiers:**

• Developer: Development/testing, limited throughput
• Standard: Production workloads, auto-scaling
• Premium: Multi-region deployment, high availability

## 3.8 Azure Kubernetes Service (AKS)

**When to Use:**

• Container orchestration
• Microservices deployment
• Auto-scaling based on load
• Rolling updates and rollbacks
• Service discovery
• Load balancing
• Multi-region deployment

**Promotions System Usage:**

• Deploy each microservice as container
• Auto-scale services based on CPU/memory
• Rolling updates for zero-downtime deployments
• Service mesh for inter-service communication
• Network policies for security

**Key Concepts:**

• **Pod**: Smallest deployable unit, contains one or more containers
• **Deployment**: Manages pods, ensures desired state
• **Service**: Exposes pods internally or externally
• **Ingress**: Routes external traffic to services
• **ConfigMap**: Store configuration data
• **Secret**: Store sensitive data
• **StatefulSet**: For stateful applications
• **DaemonSet**: Run pod on every node

## 3.9 Azure Data Factory (Good-to-Have)

**When to Use:**

• ETL/ELT pipelines
• Data integration from multiple sources
• Scheduled data movement
• Data transformation
• Data warehouse loading

**Promotions System Usage:**

• Load product catalog from ERP to Azure SQL
• Load store master data from legacy system
• Daily aggregation of promotion performance data
• Load analytics data to Synapse for reporting
• Sync customer data from CRM

**Key Concepts:**

• **Pipeline**: Workflow of activities
• **Activity**: Processing step (Copy, Transform, etc.)
• **Dataset**: Reference to data source/sink
• **Linked Service**: Connection to data store
• **Trigger**: When to run (schedule, event, tumbling window)
• **Integration Runtime**: Compute for execution

## 3.10 Azure Cache for Redis

**When to Use:**

• Cache frequently accessed data
• Reduce database load
• Improve response times
• Session storage
• Real-time leaderboards
• Rate limiting counters

**Promotions System Usage:**

• Cache active promotions (1-hour TTL)
• Cache product prices (5-minute TTL)
• Cache store information
• Cache user permissions
• Store shopping cart sessions
• Rate limiting counters per API key

**Caching Strategy:**

• **Cache-Aside**: Application checks cache, if miss, fetch from DB and populate cache
• **Write-Through**: Write to cache and DB simultaneously
• **Write-Behind**: Write to cache, asynchronously write to DB

**Eviction Policies:**

• LRU (Least Recently Used): Remove least recently used items
• LFU (Least Frequently Used): Remove least frequently used items
• TTL (Time to Live): Auto-expire after duration

---

# 4. API DESIGN & MANAGEMENT

## 4.1 REST API Design Principles

**Resource Naming Conventions:**

• Use nouns, not verbs: /api/v1/promotions (not /api/v1/getPromotions)
• Use plural for collections: /api/v1/promotions (not /api/v1/promotion)
• Use hierarchical structure for relationships: /api/v1/promotions/{id}/rules
• Use lowercase and hyphens: /api/v1/store-groups (not /api/v1/StoreGroups)
• Avoid file extensions: /api/v1/promotions (not /api/v1/promotions.json)

**HTTP Methods:**

• **GET**: Retrieve resource(s), idempotent, safe (no side effects)
• **POST**: Create new resource, not idempotent (multiple calls create multiple resources)
• **PUT**: Replace entire resource, idempotent
• **PATCH**: Partial update, idempotent
• **DELETE**: Remove resource, idempotent

**Status Codes:**

• **2xx Success**:
  - 200 OK: Successful GET/PUT/PATCH
  - 201 Created: Successful POST
  - 204 No Content: Successful DELETE
  - 206 Partial Content: Partial response (pagination)

• **4xx Client Error**:
  - 400 Bad Request: Invalid request format/validation error
  - 401 Unauthorized: Missing/invalid authentication
  - 403 Forbidden: Authenticated but insufficient permissions
  - 404 Not Found: Resource doesn't exist
  - 409 Conflict: Duplicate resource or state conflict
  - 422 Unprocessable Entity: Business rule violation
  - 429 Too Many Requests: Rate limited

• **5xx Server Error**:
  - 500 Internal Server Error: Unexpected error
  - 503 Service Unavailable: Service temporarily down

## 4.2 API Endpoints for Promotions System

**Promotions CRUD:**

• GET /api/v1/promotions - List all promotions (paginated)
• GET /api/v1/promotions/{id} - Get specific promotion
• POST /api/v1/promotions - Create new promotion
• PUT /api/v1/promotions/{id} - Full update
• PATCH /api/v1/promotions/{id} - Partial update
• DELETE /api/v1/promotions/{id} - Soft delete

**Lifecycle Operations:**

• POST /api/v1/promotions/{id}/submit - Submit for approval
• POST /api/v1/promotions/{id}/approve - Approve promotion
• POST /api/v1/promotions/{id}/reject - Reject promotion
• POST /api/v1/promotions/{id}/activate - Activate promotion
• POST /api/v1/promotions/{id}/pause - Pause promotion
• POST /api/v1/promotions/{id}/resume - Resume promotion
• POST /api/v1/promotions/{id}/cancel - Cancel promotion

**Rules Management:**

• GET /api/v1/promotions/{id}/rules - List promotion rules
• POST /api/v1/promotions/{id}/rules - Add rule
• DELETE /api/v1/promotions/{id}/rules/{ruleId} - Remove rule

**Search & Filter:**

• GET /api/v1/promotions?status=active&type=percentage&storeId=S001&page=1&pageSize=20
• GET /api/v1/promotions/search?q=summer&category=seasonal

**Pricing Engine:**

• POST /api/v1/pricing/calculate - Calculate final price with promotions
• POST /api/v1/pricing/validate-cart - Validate cart with promotions
• GET /api/v1/pricing/applicable/{productId} - Get applicable promotions

**Analytics:**

• GET /api/v1/analytics/promotions/{id}/performance - Promotion performance metrics
• GET /api/v1/analytics/dashboard - Dashboard metrics
• GET /api/v1/analytics/redemptions - Redemption data
• GET /api/v1/analytics/reports/{reportId} - Download report

## 4.3 API Versioning Strategy

**URL Path Versioning (Recommended):**

• /api/v1/promotions
• /api/v2/promotions
• Clear in URL, easy to route in APIM
• Allows running multiple versions simultaneously
• Clients explicitly choose version

**Header Versioning:**

• X-API-Version: 1.0
• Cleaner URLs but less discoverable
• Requires documentation

**Query String Versioning:**

• /api/promotions?api-version=1.0
• Least preferred, not RESTful

**Deprecation Strategy:**

• Support v1 for 6 months after v2 release
• Send Sunset header: Sunset: Sat, 01 Jun 2025 00:00:00 GMT
• Send Deprecation header: Deprecation: true
• Provide migration guide
• Monitor usage of old version

## 4.4 API Response Standards

**Success Response (Single Resource):**

```json
{
  "data": {
    "id": "promo-123",
    "name": "Summer Sale 2024",
    "type": "PercentageDiscount",
    "status": "Active",
    "discountValue": 20.0,
    "validFrom": "2024-06-01T00:00:00Z",
    "validTo": "2024-08-31T23:59:59Z"
  },
  "meta": {
    "timestamp": "2024-06-15T10:30:00Z",
    "correlationId": "abc-123-def"
  }
}
```

**Success Response (List with Pagination):**

```json
{
  "data": [
    { "id": "promo-1", "name": "Promo 1" },
    { "id": "promo-2", "name": "Promo 2" }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNext": true,
    "hasPrevious": false
  },
  "meta": {
    "timestamp": "2024-06-15T10:30:00Z",
    "correlationId": "abc-123-def"
  }
}
```

**Error Response:**

```json
{
  "error": {
    "code": "PROMOTION_OVERLAP",
    "message": "Overlapping promotion exists",
    "details": [
      {
        "field": "validFrom",
        "message": "Conflicts with promo-456"
      }
    ],
    "traceId": "abc-123-def",
    "timestamp": "2024-06-15T10:30:00Z"
  }
}
```

## 4.5 Request/Response Handling

**Request Validation:**

• Validate input format (JSON schema)
• Validate required fields
• Validate field types and lengths
• Validate business rules (discount <= 100%)
• Return 400 Bad Request with details if validation fails

**Response Transformation:**

• Transform domain objects to DTOs
• Include only necessary fields
• Mask sensitive data
• Include hypermedia links (HATEOAS optional)

**Pagination:**

• Default page size: 20, max: 100
• Support offset/limit or page/pageSize
• Return total count for UI
• Include hasNext/hasPrevious for navigation

**Filtering:**

• Support multiple filters: ?status=active&type=percentage
• Support operators: ?discount[gte]=20&discount[lte]=50
• Support sorting: ?sortBy=createdAt&sortOrder=desc
• Support search: ?q=summer

## 4.6 API Security

**Authentication:**

• JWT tokens in Authorization header
• OAuth 2.0 for third-party integrations
• API keys for service-to-service
• Rotate tokens regularly

**Authorization:**

• Role-based access control (RBAC)
• Scope-based permissions
• Resource-level permissions
• Audit who accessed what

**Rate Limiting:**

• Per user: 1000 requests/hour
• Per API key: 10000 requests/hour
• Per IP: 100 requests/minute
• Return 429 Too Many Requests when exceeded

**HTTPS/TLS:**

• All APIs over HTTPS
• TLS 1.2 minimum
• Certificate pinning for mobile apps

**CORS (Cross-Origin Resource Sharing):**

• Allow only trusted origins
• Specify allowed methods (GET, POST, etc.)
• Specify allowed headers
• Handle preflight requests

---

# 5. DATABASE STRATEGY

## 5.1 Database Per Service Pattern

**Why Database Per Service:**

• Each service owns its data
• No shared databases (prevents tight coupling)
• Different services can use different database types
• Scale databases independently
• Easier to change database technology per service

**Promotions System Database Allocation:**

• **Promotion Service**: Azure SQL (promotions, campaigns, rules)
• **Pricing Engine**: Cosmos DB (prices, price history)
• **Store Service**: Azure SQL (stores, store groups, POS mappings)
• **Identity Service**: Azure AD (users, roles)
• **Analytics Service**: Cosmos DB (events, aggregations)
• **Notification Service**: Azure SQL (notification templates, history)

**Data Sharing Between Services:**

• Never access another service's database directly
• Use APIs for synchronous calls
• Use events for asynchronous updates
• Cache shared data locally
• Duplicate data if needed for performance

## 5.2 Azure SQL Database Schema

**Core Tables:**

• **Promotions**: id, name, description, type, status, discountValue, discountType, startDate, endDate, maxRedemptions, currentRedemptions, createdBy, createdAt, modifiedBy, modifiedAt

• **Campaigns**: id, name, description, promotionIds (array), startDate, endDate, budget, status, createdAt

• **PromotionRules**: id, promotionId, ruleType, condition, value, priority, createdAt

• **PromotionStores**: promotionId, storeId, createdAt

• **Stores**: id, name, storeCode, address, city, state, country, posTerminalIds, createdAt

• **Users**: id, email, firstName, lastName, role, permissions, lastLogin, createdAt

• **AuditLog**: id, entityType, entityId, action, oldValues, newValues, userId, timestamp

• **OutboxMessages**: id, eventType, payload, processed, createdAt, processedAt

**Indexing Strategy:**

• Primary key on id
• Unique index on storeCode
• Index on status for filtering
• Index on createdAt for sorting
• Composite index on (storeId, status) for common queries
• Index on promotionId in PromotionRules

**Partitioning (for large tables):**

• Partition Promotions by year (startDate)
• Partition AuditLog by month
• Partition OutboxMessages by date

## 5.3 Cosmos DB Schema

**Product Catalog Container:**

• Partition Key: /categoryId
• Document: { id, productId, name, category, price, stock, attributes }
• TTL: None (permanent data)
• Throughput: 10,000 RU/s (high reads)

**Promotion Read Model Container:**

• Partition Key: /storeId
• Document: { id, promotionId, storeId, name, discount, applicable, active }
• TTL: None
• Throughput: 5,000 RU/s

**Shopping Cart Container:**

• Partition Key: /customerId
• Document: { id, customerId, items, total, createdAt, lastModified }
• TTL: 86400 (24 hours, auto-expire)
• Throughput: 20,000 RU/s (high writes during sales)

**Event Store Container:**

• Partition Key: /aggregateId
• Document: { id, aggregateId, eventType, payload, timestamp, version }
• TTL: None (permanent audit trail)
• Throughput: 15,000 RU/s

**Analytics Events Container:**

• Partition Key: /eventDate
• Document: { id, eventType, promotionId, storeId, customerId, amount, timestamp }
• TTL: 7776000 (90 days)
• Throughput: 50,000 RU/s (high volume)

## 5.4 Data Consistency Strategies

**Strong Consistency (Azure SQL):**

• Used for transactional data
• Promotions, campaigns, rules
• ACID guarantees
• Immediate consistency

**Eventual Consistency (Cosmos DB):**

• Used for read models
• Analytics data
• Acceptable delay of seconds/minutes
• Higher throughput and availability

**Handling Eventual Consistency:**

• Include version number in read model
• Include timestamp of last update
• Notify client when data updated
• Accept stale data for non-critical queries
• Use write model for critical operations

## 5.5 Data Backup & Recovery

**Azure SQL Backup:**

• Automatic full backups weekly
• Automatic differential backups daily
• Automatic transaction log backups every 5-10 minutes
• Point-in-time restore for 35 days
• Geo-redundant backup storage

**Cosmos DB Backup:**

• Automatic backup every 4 hours
• Retention for 30 days
• Geo-redundant backup
• Restore to new account

**Disaster Recovery:**

• Geo-replication for critical databases
• Read replicas in different regions
• Failover to replica on primary failure
• RTO (Recovery Time Objective): < 5 minutes
• RPO (Recovery Point Objective): < 1 minute

---

# 6. EVENT-DRIVEN ARCHITECTURE

## 6.1 Event-Driven System Design

**Benefits of Event-Driven Architecture:**

• Loose coupling between services
• Asynchronous processing
• Scalability (process events at own pace)
• Auditability (all events recorded)
• Replay events for recovery
• Real-time updates to multiple consumers

**Event Types:**

• **Domain Events**: Business-meaningful events (PromotionActivated, PromotionExpired)
• **Integration Events**: Events crossing service boundaries
• **System Events**: Technical events (ServiceStarted, ServiceStopped)

## 6.2 Event Publishing & Consumption

**Event Publishing:**

• Promotion Service publishes PromotionCreatedEvent when promotion created
• Event contains: promotionId, name, type, discount, storeIds, timestamp
• Published to Service Bus Topic: "promotion-events"
• Includes metadata: correlationId, causationId, userId

**Event Consumption:**

• Analytics Service subscribes to "promotion-events" topic
• Subscription filter: EventType = 'PromotionCreated' OR 'PromotionActivated'
• Processes event, updates analytics read model
• Publishes AnalyticsUpdatedEvent

**Event Ordering:**

• Single partition for same promotion (by promotionId)
• Ensures events processed in order
• Service Bus Sessions maintain order

**Idempotency:**

• Generate unique eventId for each event
• Consumer checks if eventId already processed
• Skip if already processed (safe to process twice)
• Store processed eventIds in database

## 6.3 Event Schema

**Standard Event Format:**

```json
{
  "eventId": "evt-123",
  "eventType": "PromotionActivated",
  "aggregateId": "promo-456",
  "aggregateType": "Promotion",
  "timestamp": "2024-06-15T10:30:00Z",
  "version": 1,
  "correlationId": "corr-789",
  "causationId": "cmd-012",
  "userId": "user-345",
  "data": {
    "promotionId": "promo-456",
    "name": "Summer Sale",
    "storeIds": ["S001", "S002"],
    "activatedAt": "2024-06-15T10:30:00Z"
  }
}
```

**Event Versioning:**

• Version field indicates schema version
• Handle multiple versions in consumer
• Migrate old events to new schema
• Maintain backward compatibility

## 6.4 Event Sourcing

**What is Event Sourcing:**

• Store all changes as immutable events
• Current state derived from events
• Rebuild state by replaying events
• Complete audit trail

**Implementation:**

• Event Store table: id, aggregateId, eventType, payload, timestamp, version
• When creating promotion: insert CreatePromotionEvent
• When activating: insert PromotionActivatedEvent
• To get current state: replay all events for that promotion

**Benefits:**

• Complete audit trail
• Can see state at any point in time
• Can replay events to debug issues
• Temporal queries (what was state on date X)

**Challenges:**

• Increased storage (all events stored)
• Eventual consistency
• Complex queries (need projections)
• Event schema evolution

## 6.5 Dead Letter Queue (DLQ) Handling

**When Messages Go to DLQ:**

• Message processing fails after max retries
• Message expires (TTL exceeded)
• Message size exceeds limit
• Poison message (causes repeated failures)

**DLQ Monitoring:**

• Monitor DLQ length (should be 0)
• Alert if messages in DLQ
• Investigate cause of failure
• Fix issue and reprocess

**Reprocessing from DLQ:**

• Manual review of failed message
• Fix underlying issue
• Move message back to main queue
• Reprocess

**Common Failures:**

• Service unavailable (retry with backoff)
• Invalid message format (fix and reprocess)
• Missing dependency (wait for dependency)
• Business rule violation (manual review)

---

# 7. DEVOPS & CI/CD STRATEGY

## 7.1 CI/CD Pipeline Overview

**Continuous Integration (CI):**

• Developer commits code to Git
• Automated build triggered
• Run unit tests
• Run code quality checks (SonarQube)
• Build Docker image
• Push to container registry
• Run security scanning

**Continuous Deployment (CD):**

• Deploy to dev environment automatically
• Run integration tests
• Deploy to staging environment
• Run smoke tests
• Manual approval for production
• Deploy to production
• Run health checks
• Monitor for errors

## 7.2 Azure DevOps Pipelines

**Pipeline Stages:**

• **Build Stage**: Compile code, run tests, build artifacts
• **Test Stage**: Run integration tests, security tests
• **Dev Deploy**: Deploy to development environment
• **Staging Deploy**: Deploy to staging environment
• **Prod Deploy**: Manual approval, deploy to production

**Build Pipeline YAML:**

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dockerRegistryServiceConnection: 'acr-connection'
  imageRepository: 'promotions-service'
  containerRegistry: 'myregistry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/Dockerfile'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    steps:
    - task: UseDotNet@2
      inputs:
        version: '8.0.x'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        arguments: '--configuration $(buildConfiguration) --no-build'
    
    - task: Docker@2
      inputs:
        command: 'build'
        Dockerfile: $(dockerfilePath)
        tags: |
          $(containerRegistry)/$(imageRepository):$(tag)
          $(containerRegistry)/$(imageRepository):latest
    
    - task: Docker@2
      inputs:
        command: 'push'
        containerRegistry: $(dockerRegistryServiceConnection)
        repository: $(imageRepository)
        tags: |
          $(tag)
          latest

- stage: DeployDev
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: DeployToDev
    environment: 'dev'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: KubernetesManifest@0
            inputs:
              action: 'deploy'
              kubernetesServiceConnection: 'aks-dev'
              namespace: 'default'
              manifests: |
                $(Pipeline.Workspace)/manifests/deployment.yml
              images: |
                $(containerRegistry)/$(imageRepository):$(tag)

- stage: DeployProd
  dependsOn: DeployDev
  condition: succeeded()
  jobs:
  - deployment: DeployToProd
    environment: 'prod'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: KubernetesManifest@0
            inputs:
              action: 'deploy'
              kubernetesServiceConnection: 'aks-prod'
              namespace: 'default'
              manifests: |
                $(Pipeline.Workspace)/manifests/deployment.yml
              images: |
                $(containerRegistry)/$(imageRepository):$(tag)
```

## 7.3 GitHub Actions Alternative

**GitHub Actions Workflow:**

• Trigger on push to main branch
• Checkout code
• Setup .NET environment
• Restore dependencies
• Build solution
• Run tests
• Build Docker image
• Push to registry
• Deploy to AKS

## 7.4 Infrastructure as Code (IaC)

**Terraform for Azure Resources:**

• Define all Azure resources in code
• Version control infrastructure
• Reproducible deployments
• Easy to create dev/staging/prod environments

**Key Resources:**

• Azure SQL Database
• Cosmos DB
• Service Bus
• Key Vault
• App Service / AKS
• Storage Account
• Application Insights

**Benefits:**

• Infrastructure documented in code
• Easy to replicate environments
• Track changes in Git
• Rollback infrastructure changes
• Automated provisioning

## 7.5 Deployment Strategies

**Blue-Green Deployment:**

• Two identical production environments (Blue and Green)
• Currently serving traffic: Blue
• Deploy new version to Green
• Test Green environment
• Switch traffic from Blue to Green
• Keep Blue as rollback option

**Advantages:**

• Zero-downtime deployment
• Easy rollback
• Test in production environment

**Disadvantages:**

• Double infrastructure cost
• Database migration complexity

**Canary Deployment:**

• Deploy new version to small percentage of servers (5%)
• Monitor metrics (error rate, latency)
• Gradually increase percentage (10%, 25%, 50%, 100%)
• Rollback if issues detected

**Advantages:**

• Low risk
• Detect issues early
• Gradual rollout

**Disadvantages:**

• Slower deployment
• Complex monitoring

**Rolling Deployment:**

• Gradually replace old instances with new ones
• Update one pod at a time
• Kubernetes default strategy
• Zero-downtime

**Advantages:**

• Simple
• No double infrastructure cost
• Kubernetes native

**Disadvantages:**

• Slower deployment
• Mixed versions during rollout

## 7.6 Monitoring Deployments

**Health Checks:**

• Liveness probe: Is service running?
• Readiness probe: Is service ready to accept traffic?
• Startup probe: Has service started?

**Metrics to Monitor:**

• Error rate (should be near 0%)
• Response time (should be within SLA)
• CPU/Memory usage
• Database connections
• Service Bus queue length

**Rollback Criteria:**

• Error rate > 1%
• Response time > 2 seconds
• Service unavailable
• Database connection failures

---

# 8. DOCKER & KUBERNETES

## 8.1 Docker Fundamentals

**What is Docker:**

• Containerization platform
• Package application with dependencies
• Consistent environment (dev, staging, prod)
• Lightweight compared to VMs
• Fast startup time

**Docker Components:**

• **Image**: Blueprint for container (immutable)
• **Container**: Running instance of image
• **Registry**: Repository for images (Docker Hub, ACR)
• **Dockerfile**: Instructions to build image

**Dockerfile for .NET Core Service:**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Promotion.API/Promotion.API.csproj", "Promotion.API/"]
COPY ["Promotion.Application/Promotion.Application.csproj", "Promotion.Application/"]
COPY ["Promotion.Domain/Promotion.Domain.csproj", "Promotion.Domain/"]
COPY ["Promotion.Infrastructure/Promotion.Infrastructure.csproj", "Promotion.Infrastructure/"]

RUN dotnet restore "Promotion.API/Promotion.API.csproj"

COPY . .
RUN dotnet build "Promotion.API/Promotion.API.csproj" -c Release -o /app/build

RUN dotnet publish "Promotion.API/Promotion.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Promotion.API.dll"]
```

**Docker Best Practices:**

• Use multi-stage builds (separate build and runtime)
• Use specific base image versions (not latest)
• Minimize image size (remove unnecessary files)
• Use .dockerignore to exclude files
• Run as non-root user
• Use health checks
• Keep layers small (each RUN command is a layer)

## 8.2 Azure Container Registry (ACR)

**Why ACR:**

• Private container registry
• Integrated with Azure services
• Geo-replication for global distribution
• Image scanning for vulnerabilities
• Webhook for CI/CD integration

**ACR Tiers:**

• **Basic**: Development/testing, limited storage
• **Standard**: Production workloads, more storage
• **Premium**: High throughput, geo-replication

**Push Image to ACR:**

```bash
# Login to ACR
az acr login --name myregistry

# Tag image
docker tag promotions-service:latest myregistry.azurecr.io/promotions-service:latest

# Push to ACR
docker push myregistry.azurecr.io/promotions-service:latest
```

## 8.3 Kubernetes Basics

**What is Kubernetes:**

• Container orchestration platform
• Automate deployment, scaling, management
• Self-healing (restart failed containers)
• Load balancing
• Rolling updates
• Resource management

**Kubernetes Objects:**

• **Pod**: Smallest unit, contains one or more containers
• **Deployment**: Manages pods, ensures desired state
• **Service**: Exposes pods (internal or external)
• **Ingress**: Routes external traffic
• **ConfigMap**: Configuration data
• **Secret**: Sensitive data
• **PersistentVolume**: Storage
• **StatefulSet**: Stateful applications

**Deployment YAML:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: promotions-service
  labels:
    app: promotions-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: promotions-service
  template:
    metadata:
      labels:
        app: promotions-service
    spec:
      containers:
      - name: promotions-service
        image: myregistry.azurecr.io/promotions-service:latest
        ports:
        - containerPort: 80
        - containerPort: 443
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: promotions-service
spec:
  selector:
    app: promotions-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: promotions-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: promotions-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## 8.4 Azure Kubernetes Service (AKS)

**AKS Features:**

• Managed Kubernetes cluster
• Azure handles control plane
• Auto-scaling of nodes
• Integration with Azure services
• Network policies
• RBAC
• Monitoring with Application Insights

**Creating AKS Cluster:**

```bash
# Create resource group
az group create --name myResourceGroup --location eastus

# Create AKS cluster
az aks create \
  --resource-group myResourceGroup \
  --name myAKSCluster \
  --node-count 3 \
  --vm-set-type VirtualMachineScaleSets \
  --load-balancer-sku standard \
  --enable-managed-identity \
  --network-plugin azure \
  --docker-bridge-address 172.17.0.1/16 \
  --service-cidr 10.0.0.0/16 \
  --dns-service-ip 10.0.0.10 \
  --enable-cluster-autoscaling \
  --min-count 3 \
  --max-count 10

# Get credentials
az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
```

**Deploying to AKS:**

```bash
# Apply deployment
kubectl apply -f deployment.yaml

# Check deployment status
kubectl get deployments

# Check pods
kubectl get pods

# Check services
kubectl get services

# View logs
kubectl logs <pod-name>

# Port forward for testing
kubectl port-forward svc/promotions-service 8080:80
```

## 8.5 Service Mesh (Optional)

**What is Service Mesh:**

• Infrastructure layer for service-to-service communication
• Handles traffic management, security, observability
• Sidecar proxy in each pod (Istio, Linkerd)
• Decouples communication from application code

**Benefits:**

• Traffic management (canary, circuit breaker)
• Security (mTLS, authorization policies)
• Observability (distributed tracing, metrics)
• Resilience (retries, timeouts)

**Istio Example:**

```yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: promotions-service
spec:
  hosts:
  - promotions-service
  http:
  - match:
    - headers:
        user-type:
          exact: canary
    route:
    - destination:
        host: promotions-service
        subset: v2
      weight: 100
  - route:
    - destination:
        host: promotions-service
        subset: v1
      weight: 90
    - destination:
        host: promotions-service
        subset: v2
      weight: 10
---
apiVersion: networking.istio.io/v1beta1
kind: DestinationRule
metadata:
  name: promotions-service
spec:
  host: promotions-service
  trafficPolicy:
    connectionPool:
      tcp:
        maxConnections: 100
      http:
        http1MaxPendingRequests: 100
        http2MaxRequests: 1000
    outlierDetection:
      consecutive5xxErrors: 5
      interval: 30s
      baseEjectionTime: 30s
  subsets:
  - name: v1
    labels:
      version: v1
  - name: v2
    labels:
      version: v2
```

---

# 9. SECURITY ARCHITECTURE

## 9.1 Authentication & Authorization

**Authentication (Who are you):**

• OAuth 2.0 / OpenID Connect (OIDC)
• JWT tokens
• Azure AD B2C for customer authentication
• Azure AD for employee authentication
• Multi-factor authentication (MFA)

**Authorization (What can you do):**

• Role-Based Access Control (RBAC)
• Scope-based permissions
• Resource-level permissions
• Claim-based authorization

**JWT Token Structure:**

```
Header.Payload.Signature

Header: { "alg": "HS256", "typ": "JWT" }
Payload: { "sub": "user-123", "role": "admin", "scope": "promotions:write", "exp": 1234567890 }
Signature: HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)
```

**Authorization in .NET:**

```csharp
[Authorize(Roles = "Admin")]
[HttpPost("/api/v1/promotions/{id}/approve")]
public async Task<IActionResult> ApprovePromotion(string id)
{
    // Only admins can approve
}

[Authorize(Policy = "CanEditPromotion")]
[HttpPut("/api/v1/promotions/{id}")]
public async Task<IActionResult> UpdatePromotion(string id)
{
    // Custom policy
}
```

## 9.2 API Security

**API Key Management:**

• Generate unique API keys per client
• Store in Key Vault
• Rotate keys regularly
• Revoke compromised keys
• Track API key usage

**Rate Limiting:**

• Per user: 1000 requests/hour
• Per API key: 10000 requests/hour
• Per IP: 100 requests/minute
• Implement in APIM

**CORS (Cross-Origin Resource Sharing):**

• Allow only trusted origins
• Specify allowed methods
• Specify allowed headers
• Handle preflight requests

**HTTPS/TLS:**

• All APIs over HTTPS
• TLS 1.2 minimum
• Certificate pinning for mobile apps
• HSTS header to force HTTPS

## 9.3 Data Security

**Encryption at Rest:**

• Azure SQL: Transparent Data Encryption (TDE)
• Cosmos DB: Encryption enabled by default
• Blob Storage: Encryption enabled
• Use customer-managed keys (CMK) for sensitive data

**Encryption in Transit:**

• HTTPS for all APIs
• TLS 1.2 minimum
• mTLS for service-to-service communication
• Encrypt sensitive data in messages

**Data Masking:**

• Mask PII in logs
• Mask credit card numbers
• Mask email addresses
• Use Azure SQL Dynamic Data Masking

**Data Retention:**

• Delete old data per policy
• Archive old data to cold storage
• Comply with GDPR (right to be forgotten)
• Audit trail retention (7 years)

## 9.4 Secret Management

**Azure Key Vault:**

• Store database connection strings
• Store API keys
• Store certificates
• Store encryption keys
• Managed Identity for access

**Rotation:**

• Rotate secrets every 90 days
• Rotate certificates before expiration
• Automated rotation policies
• No downtime during rotation

**Access Control:**

• Use RBAC for Key Vault access
• Audit who accessed secrets
• Restrict access to production secrets
• Use Managed Identity (no credentials in code)

## 9.5 Network Security

**Network Policies:**

• Restrict traffic between pods
• Allow only necessary communication
• Deny by default, allow explicitly

**Firewall Rules:**

• Whitelist IP addresses
• Block suspicious traffic
• DDoS protection
• Web Application Firewall (WAF)

**VNet Integration:**

• Deploy AKS in VNet
• Private endpoints for databases
• Service endpoints for Azure services
• Network segmentation

**Example Network Policy:**

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: promotions-service-netpol
spec:
  podSelector:
    matchLabels:
      app: promotions-service
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 80
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: pricing-engine
    ports:
    - protocol: TCP
      port: 80
  - to:
    - namespaceSelector: {}
      podSelector:
        matchLabels:
          k8s-app: kube-dns
    ports:
    - protocol: UDP
      port: 53
```

---

# 10. REACT FRONTEND ARCHITECTURE

## 10.1 React Application Structure

**Project Layout:**

• src/
  - components/ (Reusable UI components)
  - pages/ (Page components)
  - services/ (API calls)
  - hooks/ (Custom hooks)
  - context/ (Context API for state)
  - utils/ (Helper functions)
  - types/ (TypeScript types)
  - styles/ (CSS/SCSS)
  - App.tsx (Root component)
  - index.tsx (Entry point)

## 10.2 State Management

**Redux vs Context API:**

• **Redux**: Large applications, complex state, time-travel debugging
• **Context API**: Small applications, simple state, less boilerplate

**Redux Store Structure:**

• promotions slice (list, create, update, delete)
• campaigns slice (manage campaigns)
• filters slice (search, sort, pagination)
• ui slice (loading, error, modal states)
• auth slice (user, permissions)

## 10.3 API Integration

**Axios Instance:**

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
  timeout: 10000,
});

// Add JWT token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

**API Service:**

```typescript
import api from './api';

export const promotionService = {
  getPromotions: (page: number, pageSize: number) =>
    api.get('/promotions', { params: { page, pageSize } }),
  
  getPromotion: (id: string) =>
    api.get(`/promotions/${id}`),
  
  createPromotion: (data: CreatePromotionRequest) =>
    api.post('/promotions', data),
  
  updatePromotion: (id: string, data: UpdatePromotionRequest) =>
    api.patch(`/promotions/${id}`, data),
  
  activatePromotion: (id: string) =>
    api.post(`/promotions/${id}/activate`),
  
  cancelPromotion: (id: string) =>
    api.post(`/promotions/${id}/cancel`),
};
```

## 10.4 Component Examples

**Promotion List Component:**

```typescript
import React, { useEffect, useState } from 'react';
import { promotionService } from '../services/promotionService';
import PromotionCard from './PromotionCard';

interface Promotion {
  id: string;
  name: string;
  type: string;
  status: string;
  discount: number;
}

const PromotionList: React.FC = () => {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  useEffect(() => {
    fetchPromotions();
  }, [page, pageSize]);

  const fetchPromotions = async () => {
    try {
      setLoading(true);
      const response = await promotionService.getPromotions(page, pageSize);
      setPromotions(response.data.data);
    } catch (err) {
      setError('Failed to load promotions');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="promotion-list">
      <h1>Promotions</h1>
      <div className="grid">
        {promotions.map((promo) => (
          <PromotionCard key={promo.id} promotion={promo} />
        ))}
      </div>
      <div className="pagination">
        <button onClick={() => setPage(page - 1)} disabled={page === 1}>
          Previous
        </button>
        <span>Page {page}</span>
        <button onClick={() => setPage(page + 1)}>Next</button>
      </div>
    </div>
  );
};

export default PromotionList;
```

**Create Promotion Form:**

```typescript
import React, { useState } from 'react';
import { promotionService } from '../services/promotionService';

interface CreatePromotionRequest {
  name: string;
  description: string;
  type: string;
  discountValue: number;
  startDate: string;
  endDate: string;
  maxRedemptions?: number;
}

const CreatePromotionForm: React.FC = () => {
  const [formData, setFormData] = useState<CreatePromotionRequest>({
    name: '',
    description: '',
    type: 'PercentageDiscount',
    discountValue: 0,
    startDate: '',
    endDate: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: name === 'discountValue' ? parseFloat(value) : value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);
      await promotionService.createPromotion(formData);
      setSuccess(true);
      setFormData({
        name: '',
        description: '',
        type: 'PercentageDiscount',
        discountValue: 0,
        startDate: '',
        endDate: '',
      });
    } catch (err: any) {
      setError(err.response?.data?.error?.message || 'Failed to create promotion');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="form">
      <h2>Create Promotion</h2>
      
      {error && <div className="error">{error}</div>}
      {success && <div className="success">Promotion created successfully!</div>}

      <div className="form-group">
        <label htmlFor="name">Name</label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="description">Description</label>
        <input
          type="text"
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label htmlFor="type">Type</label>
        <select id="type" name="type" value={formData.type} onChange={handleChange}>
          <option value="PercentageDiscount">Percentage Discount</option>
          <option value="FlatDiscount">Flat Discount</option>
          <option value="BuyOneGetOne">Buy One Get One</option>
        </select>
      </div>

      <div className="form-group">
        <label htmlFor="discountValue">Discount Value</label>
        <input
          type="number"
          id="discountValue"
          name="discountValue"
          value={formData.discountValue}
          onChange={handleChange}
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="startDate">Start Date</label>
        <input
          type="datetime-local"
          id="startDate"
          name="startDate"
          value={formData.startDate}
          onChange={handleChange}
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="endDate">End Date</label>
        <input
          type="datetime-local"
          id="endDate"
          name="endDate"
          value={formData.endDate}
          onChange={handleChange}
          required
        />
      </div>

      <button type="submit" disabled={loading}>
        {loading ? 'Creating...' : 'Create Promotion'}
      </button>
    </form>
  );
};

export default CreatePromotionForm;
```

## 10.5 Real-Time Updates with SignalR

**SignalR Connection:**

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_SIGNALR_URL)
  .withAutomaticReconnect()
  .build();

connection.start().catch(err => console.error(err));

// Listen for promotion updates
connection.on('PromotionUpdated', (promotion) => {
  // Update UI with new promotion data
  console.log('Promotion updated:', promotion);
});

export default connection;
```

**Using SignalR in Component:**

```typescript
useEffect(() => {
  connection.on('PromotionActivated', (promotionId) => {
    // Refresh promotion list
    fetchPromotions();
  });

  return () => {
    connection.off('PromotionActivated');
  };
}, []);
```

---

# 11. RETAIL DOMAIN / POS

## 11.1 POS (Point of Sale) System Basics

**What is POS:**

• System at checkout counter
• Scans items, calculates total
• Processes payments
• Prints receipts
• Manages inventory
• Tracks sales

**POS Hardware:**

• Cash register/terminal
• Barcode scanner
• Receipt printer
• Payment terminal
• Customer display
• Scale (for produce)

## 11.2 Promotion Integration with POS

**How Promotions Reach POS:**

• Promotion created and activated in cloud system
• Event published to Service Bus
• Store Service listens to event
• Transforms promotion to POS format
• Pushes to POS terminal via API/file
• POS applies discount at checkout

**POS Promotion Data:**

• Promotion ID (unique identifier)
• Promotion name/description
• Discount type (percentage, flat, BOGO)
• Discount value
• Applicable product codes
• Applicable store codes
• Valid date range
• Max redemptions
• Exclusions (products, customers)

**POS Sync Process:**

• Promotion Service publishes PromotionActivatedEvent
• Store Service receives event
• Queries applicable stores
• Transforms to POS format (XML/JSON)
• Calls POS API for each store
• POS confirms receipt
• Store Service publishes PromotionSyncedEvent
• Analytics Service tracks sync success

## 11.3 Retail Domain Concepts

**SKU (Stock Keeping Unit):**

• Unique identifier for product
• Barcode number
• Example: 1234567890123

**Product Hierarchy:**

• Category (e.g., Grocery)
• Subcategory (e.g., Beverages)
• Product (e.g., Coca Cola)
• Variant (e.g., 2L Bottle)

**Store Hierarchy:**

• Region (e.g., North America)
• Zone (e.g., Northeast)
• District (e.g., New York)
• Store (e.g., Times Square)

**Promotion Types in Retail:**

• **Percentage Discount**: 20% off
• **Flat Discount**: $5 off
• **Buy One Get One (BOGO)**: Buy 1, get 1 free
• **Buy X Get Y**: Buy 2, get 3rd at 50%
• **Bundle Deal**: 3 items for $10
• **Loyalty Points**: Earn 2x points
• **Free Shipping**: Free shipping on order
• **Tiered Discount**: $5 off $25, $10 off $50

## 11.4 Redemption Tracking

**Redemption Flow:**

• Customer purchases item at POS
• POS applies promotion
• Records redemption in local database
• Syncs to cloud at end of day
• Analytics Service processes redemption
• Updates promotion redemption count
• Checks if max redemptions reached

**Redemption Data:**

• Redemption ID
• Promotion ID
• Store ID
• Product ID
• Customer ID (optional)
• Discount amount
• Original price
• Final price
• Timestamp

**Analytics Metrics:**

• Total redemptions
• Redemption rate (redemptions / impressions)
• Average discount per redemption
• Revenue impact
• ROI (Return on Investment)
• Margin impact

## 11.5 Inventory Management

**Inventory Sync:**

• POS maintains local inventory
• Syncs to cloud periodically
• Cloud system has master inventory
• Promotions consider inventory levels
• Don't promote out-of-stock items

**Stock Levels:**

• On-hand: Physical stock in store
• Available: On-hand minus reserved
• Reserved: Items for pending orders
• Damaged: Unsellable items

**Inventory Alerts:**

• Alert when stock below threshold
• Alert when out of stock
• Alert when overstock
• Trigger replenishment orders

---

# 12. MONITORING & OBSERVABILITY

## 12.1 Application Insights

**What is Application Insights:**

• Application Performance Monitoring (APM)
• Collects telemetry from applications
• Tracks requests, exceptions, dependencies
• Provides dashboards and analytics
• Integrates with Azure services

**Key Metrics:**

• **Request Rate**: Requests per second
• **Response Time**: Average latency
• **Error Rate**: Percentage of failed requests
• **Dependency Duration**: Time spent in external calls
• **Exception Rate**: Unhandled exceptions
• **Availability**: Uptime percentage

**Instrumenting .NET Core:**

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Inject ILogger
public class PromotionService
{
    private readonly ILogger<PromotionService> _logger;
    
    public PromotionService(ILogger<PromotionService> logger)
    {
        _logger = logger;
    }
    
    public async Task CreatePromotion(CreatePromotionRequest request)
    {
        _logger.LogInformation("Creating promotion: {Name}", request.Name);
        
        try
        {
            // Business logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating promotion");
            throw;
        }
    }
}
```

## 12.2 Logging Strategy

**Logging Levels:**

• **Trace**: Detailed diagnostic information
• **Debug**: Debugging information
• **Information**: General information
• **Warning**: Warning messages
• **Error**: Error messages
• **Critical**: Critical errors

**Structured Logging:**

• Use structured logging (JSON format)
• Include correlation ID for tracing
• Include user ID for auditing
• Include request/response data
• Use consistent field names

**Log Aggregation:**

• Centralize logs in Log Analytics
• Query logs using KQL (Kusto Query Language)
• Create alerts based on log patterns
• Retention: 30 days (configurable)

**Example KQL Query:**

```kusto
requests
| where name == "POST /api/v1/promotions"
| where duration > 1000
| summarize count() by bin(timestamp, 1m)
| render timechart
```

## 12.3 Distributed Tracing

**Correlation ID:**

• Unique ID for request across services
• Passed in X-Correlation-ID header
• Logged in all services
• Enables end-to-end tracing

**Trace Flow:**

• Client sends request with Correlation-ID
• API Gateway adds if missing
• Promotion Service logs with Correlation-ID
• Calls Pricing Engine with Correlation-ID
• Pricing Engine logs with Correlation-ID
• All logs linked by Correlation-ID

**Application Insights Tracing:**

• Automatically tracks dependencies
• Shows service-to-service calls
• Shows database queries
• Shows external API calls
• Visualizes in Application Map

## 12.4 Alerting & Monitoring

**Alert Rules:**

• Error rate > 1% for 5 minutes
• Response time > 2 seconds for 10 minutes
• Service Bus queue length > 1000
• Database CPU > 80% for 10 minutes
• Disk space < 10% available

**Alert Actions:**

• Send email to team
• Send SMS to on-call engineer
• Create incident in PagerDuty
• Post to Slack
• Trigger runbook

**Dashboard:**

• Real-time metrics
• Error rate chart
• Response time chart
• Service health status
• Active alerts
• Recent deployments

## 12.5 Health Checks

**Liveness Probe:**

• Is service running?
• Endpoint: /health/live
• Returns 200 if running, 503 if not
• Kubernetes restarts pod if fails

**Readiness Probe:**

• Is service ready for traffic?
• Endpoint: /health/ready
• Checks dependencies (database, cache)
• Returns 200 if ready, 503 if not
• Kubernetes removes from load balancer if fails

**Health Check Implementation:**

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PromotionDbContext>()
    .AddAzureServiceBusQueue(serviceBusConnectionString, "promotion-queue")
    .AddRedis(redisConnectionString);

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

# 13. INTERVIEW Q&A

## 13.1 Architecture & Design Questions

**Q1: Explain the microservices architecture for the Promotions Management System.**

A: The system is decomposed into independent microservices:
• Promotion Service: CRUD and lifecycle management
• Pricing Engine: Real-time price calculation
• Store Service: Store data and POS integration
• Analytics Service: Metrics and reporting
• Notification Service: Customer and internal notifications
• Identity Service: Authentication and authorization

Each service has its own database (database per service pattern), communicates via REST APIs and async events through Service Bus. APIM acts as single entry point providing authentication, rate limiting, and routing.

**Q2: What is CQRS and why use it in this system?**

A: CQRS separates read and write models:
• Write side: Normalized Azure SQL, optimized for consistency
• Read side: Denormalized Cosmos DB, optimized for query performance
• Events sync data from write to read model

Benefits:
• Scale read and write independently
• Different data structures for different use cases
• Better performance for read-heavy scenarios
• Easier to understand business operations

**Q3: Explain the Saga pattern for distributed transactions.**

A: Saga coordinates transactions across multiple services:
• Choreography: Services listen to events and trigger next step
• Orchestration: Central orchestrator (Logic App) coordinates steps

Example: Activating promotion
• Promotion Service publishes PromotionActivatingEvent
• Pricing Engine updates prices, publishes PricesUpdatedEvent
• Store Service syncs to POS, publishes PromotionDistributedEvent
• If any step fails, compensation events trigger rollback

**Q4: How do you ensure reliable event publishing?**

A: Use Outbox Pattern:
• Write to database and OutboxMessages table in single transaction
• Background worker polls OutboxMessages
• Publishes to Service Bus
• Marks as processed
• Guarantees event published if DB write succeeds

**Q5: What is the difference between synchronous and asynchronous communication?**

A: 
• Synchronous (REST): Caller waits for response, tight coupling, simpler debugging
• Asynchronous (Events): Caller doesn't wait, loose coupling, better scalability

Use synchronous for immediate responses (GET requests), asynchronous for side effects (creation events).

## 13.2 Azure Services Questions

**Q6: When would you use Cosmos DB vs Azure SQL?**

A: 
• Azure SQL: Fixed schema, complex queries, ACID transactions, relational data
• Cosmos DB: Flexible schema, high read throughput, global distribution, eventual consistency

In Promotions System:
• SQL: Promotions, campaigns, rules (transactional)
• Cosmos DB: Product catalog, read models, analytics (high volume)

**Q7: Explain Azure API Management (APIM) and its role.**

A: APIM is API gateway that:
• Provides single entry point for all APIs
• Authenticates requests (JWT validation)
• Rate limits per user/API key
• Caches responses
• Transforms requests/responses
• Routes to backend services
• Provides developer portal
• Tracks usage and metrics

**Q8: What are Azure Functions and when to use them?**

A: Serverless compute service:
• Event-driven (Service Bus, Timer, HTTP)
• Pay per execution
• Auto-scale to 0
• No infrastructure management

Use for:
• Promotion expiry checker (Timer trigger)
• Event processing (Service Bus trigger)
• Report generation (HTTP trigger)
• Image processing (Blob trigger)

**Q9: How would you use Azure Logic Apps in approval workflow?**

A: Logic App coordinates approval process:
• Triggered by PromotionSubmittedEvent
• Determines approval chain based on discount
• Sends approval email with Approve/Reject buttons
• Waits for response (with timeout)
• Calls Promotion API to approve/reject
• Sends notification of decision
• Escalates if timeout

**Q10: Explain Key Vault and secret management.**

A: Key Vault stores secrets securely:
• Connection strings
• API keys
• Certificates
• Encryption keys

Access via Managed Identity (no credentials in code). Rotate every 90 days. Audit access. Separate vaults per environment.

## 13.3 Database & Data Questions

**Q11: Design the database schema for Promotions table.**

A: 
• Id (GUID, PK)
• Name (string, max 200)
• Type (enum: Percentage, Flat, BOGO)
• Status (enum: Draft, Pending, Approved, Active, Expired)
• DiscountValue (decimal)
• DiscountType (enum)
• StartDate (datetime)
• EndDate (datetime)
• MaxRedemptions (int, nullable)
• CurrentRedemptions (int)
• CreatedBy (string)
• CreatedAt (datetime)
• ModifiedBy (string)
• ModifiedAt (datetime)

Indexes: Status, CreatedAt, (StoreId, Status)

**Q12: How would you handle eventual consistency in CQRS?**

A:
• Accept stale data for non-critical queries
• Include version/timestamp in read model
• Notify client when data updated
• Use write model for critical operations
• Implement retry logic for consistency checks

**Q13: Explain partitioning strategy for Cosmos DB.**

A: Choose partition key carefully:
• Product Catalog: /categoryId (even distribution)
• Promotion Read Model: /storeId (query by store)
• Analytics Events: /eventDate (time-based, auto-expire)
• Event Store: /aggregateId (all events for promotion together)

Avoid hot partitions (one partition gets all traffic).

**Q14: How would you backup and recover data?**

A:
• Azure SQL: Auto backups, point-in-time restore (35 days)
• Cosmos DB: Auto backup every 4 hours (30 days)
• Geo-replication for disaster recovery
• RTO < 5 minutes, RPO < 1 minute

**Q15: Design a data warehouse for analytics.**

A: Use Azure Synapse:
• Fact table: Redemptions (promotionId, storeId, customerId, amount, date)
• Dimension tables: Promotions, Stores, Customers, Dates
• Aggregate tables: Daily sales by promotion, ROI by promotion
• Refresh daily via Data Factory
• Query with SQL or Power BI

## 13.4 API & Integration Questions

**Q16: Design the REST API for creating a promotion.**

A:
```
POST /api/v1/promotions
Content-Type: application/json

{
  "name": "Summer Sale",
  "description": "20% off all items",
  "type": "PercentageDiscount",
  "discountValue": 20.0,
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-08-31T23:59:59Z",
  "maxRedemptions": 10000,
  "storeIds": ["S001", "S002"]
}

Response: 201 Created
{
  "data": {
    "id": "promo-123",
    "status": "Draft",
    "createdAt": "2024-06-15T10:30:00Z"
  }
}
```

**Q17: How would you implement pagination in API?**

A:
```
GET /api/v1/promotions?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc

Response:
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNext": true
  }
}
```

**Q18: Design error handling for APIs.**

A:
• Validation errors: 400 Bad Request with field details
• Authentication errors: 401 Unauthorized
• Authorization errors: 403 Forbidden
• Not found: 404 Not Found
• Business rule violations: 422 Unprocessable Entity
• Rate limited: 429 Too Many Requests
• Server errors: 500 Internal Server Error

Include error code, message, details, and traceId.

**Q19: How would you version APIs?**

A: URL path versioning:
• /api/v1/promotions (current version)
• /api/v2/promotions (new version)

Support v1 for 6 months after v2 release. Send Sunset header indicating deprecation date. Provide migration guide.

**Q20: Explain API security best practices.**

A:
• Use HTTPS/TLS
• JWT tokens in Authorization header
• Validate all inputs
• Rate limiting per user/API key
• CORS for trusted origins only
• Secrets in Key Vault
• Audit logging
• SQL injection prevention (parameterized queries)

## 13.5 DevOps & Deployment Questions

**Q21: Design a CI/CD pipeline for this system.**

A:
• Trigger: Push to main branch
• Build: Compile, run tests, build Docker image
• Push: Push image to ACR
• Deploy Dev: Deploy to dev AKS cluster
• Test: Run integration tests
• Deploy Staging: Deploy to staging
• Approval: Manual approval for prod
• Deploy Prod: Blue-green deployment
• Monitor: Health checks, rollback if needed

**Q22: Explain blue-green deployment.**

A:
• Two identical production environments (Blue, Green)
• Currently serving: Blue
• Deploy new version to Green
• Test Green thoroughly
• Switch traffic from Blue to Green
• Keep Blue as rollback option
• Benefits: Zero downtime, easy rollback
• Disadvantage: Double infrastructure cost

**Q23: How would you implement canary deployment?**

A:
• Deploy new version to 5% of servers
• Monitor error rate, latency
• Gradually increase to 10%, 25%, 50%, 100%
• Rollback if error rate > 1%
• Benefits: Low risk, detect issues early
• Disadvantage: Slower deployment

**Q24: Explain Kubernetes deployment manifest.**

A:
• Deployment: Manages pods, ensures desired state
• Service: Exposes pods (ClusterIP, NodePort, LoadBalancer)
• Ingress: Routes external traffic
• ConfigMap: Configuration data
• Secret: Sensitive data
• HPA: Auto-scaling based on metrics
• NetworkPolicy: Restrict traffic

**Q25: How would you monitor a Kubernetes cluster?**

A:
• Prometheus: Metrics collection
• Grafana: Visualization
• Application Insights: APM
• Liveness/Readiness probes: Pod health
• Resource requests/limits: Prevent resource starvation
• Alerts: Error rate, latency, CPU, memory

## 13.6 Security Questions

**Q26: Explain OAuth 2.0 flow for authentication.**

A:
• User clicks "Login with Azure AD"
• Redirected to Azure AD login
• User enters credentials
• Azure AD returns authorization code
• Application exchanges code for token
• Token used in Authorization header for API calls
• Token expires, refresh token gets new token

**Q27: How would you implement role-based access control (RBAC)?**

A:
• Define roles: Admin, Manager, Viewer
• Assign permissions to roles
• Assign users to roles
• Check role in API: [Authorize(Roles = "Admin")]
• Check permission in code: User.HasClaim("permission", "promotions:approve")

**Q28: Explain mTLS (mutual TLS) for service-to-service communication.**

A:
• Both client and server have certificates
• Client presents certificate to server
• Server verifies client certificate
• Server presents certificate to client
• Client verifies server certificate
• Encrypted communication established
• Prevents unauthorized services from calling API

**Q29: How would you prevent SQL injection?**

A:
• Use parameterized queries (Entity Framework Core)
• Never concatenate user input into SQL
• Validate and sanitize input
• Use stored procedures with parameters
• Principle of least privilege (limited DB permissions)

**Q30: Design a secrets rotation strategy.**

A:
• Rotate every 90 days
• Use Key Vault rotation policies
• No downtime during rotation
• Old and new secrets valid during transition
• Audit who accessed secrets
• Alert on failed rotation

## 13.7 Retail Domain Questions

**Q31: Explain how promotions reach POS terminals.**

A:
• Promotion activated in cloud
• Event published to Service Bus
• Store Service listens
• Transforms to POS format
• Calls POS API for each store
• POS applies discount at checkout
• Redemption recorded and synced back

**Q32: How would you track promotion redemptions?**

A:
• POS records redemption locally
• Syncs to cloud at end of day
• Analytics Service processes
• Updates redemption count
• Checks if max reached
• Calculates ROI
• Generates reports

**Q33: Design a promotion conflict resolution strategy.**

A:
• Prevent overlapping promotions on same product
• If multiple apply, use highest discount
• Exclude certain products from promotions
• Limit promotions per customer
• Validate in Pricing Engine

**Q34: Explain inventory considerations for promotions.**

A:
• Don't promote out-of-stock items
• Check stock before applying promotion
• Reserve inventory for high-demand promotions
• Alert when stock low
• Trigger replenishment orders

**Q35: How would you calculate promotion ROI?**

A:
• Revenue = Redemptions × Discount Amount
• Cost = Discount Amount × Redemptions
• Profit = (Revenue - Cost) - Promotion Setup Cost
• ROI = (Profit / Cost) × 100%
• Track margin impact

## 13.8 Performance & Optimization Questions

**Q36: How would you optimize API response time?**

A:
• Implement caching (Redis)
• Use CQRS (optimize read model)
• Pagination (don't return all data)
• Database indexing
• Async operations
• CDN for static content
• Connection pooling
• Query optimization

**Q37: Explain caching strategy for promotions.**

A:
• Cache active promotions (1-hour TTL)
• Cache product prices (5-minute TTL)
• Cache store information
• Invalidate on update
• Use cache-aside pattern
• Monitor cache hit rate

**Q38: How would you handle high-volume promotion events?**

A:
• Use Service Bus with partitions
• Increase throughput (RU/s)
• Use Cosmos DB for high writes
• Batch processing
• Async processing
• Auto-scaling

**Q39: Design a load testing strategy.**

A:
• Load test with 10x expected traffic
• Identify bottlenecks
• Test failure scenarios
• Test auto-scaling
• Test database limits
• Monitor metrics during test

**Q40: How would you optimize database queries?**

A:
• Add indexes on frequently queried columns
• Avoid SELECT * (specify columns)
• Use joins efficiently
• Partition large tables
• Archive old data
• Use query execution plans
• Monitor slow queries

## 13.9 Troubleshooting Questions

**Q41: A promotion is not appearing in POS. How would you debug?**

A:
• Check if promotion is Active
• Check if within valid date range
• Check Service Bus messages (published?)
• Check Store Service logs
• Check POS sync status
• Check network connectivity to POS
• Verify POS API response
• Check POS terminal logs

**Q42: API response time is slow. How would you investigate?**

A:
• Check Application Insights metrics
• Check database query performance
• Check external API calls
• Check cache hit rate
• Check CPU/memory usage
• Check network latency
• Load test to identify bottleneck
• Profile code

**Q43: Promotion redemption count is incorrect. How would you fix?**

A:
• Check POS sync logs
• Verify redemption data in database
• Check for duplicate processing
• Verify idempotency implementation
• Audit trail of changes
• Recalculate from raw data
• Notify affected parties

**Q44: Service Bus messages stuck in DLQ. How would you resolve?**

A:
• Investigate cause of failure
• Fix underlying issue
• Manually review message
• Move back to main queue
• Reprocess
• Monitor for recurrence

**Q45: Database connection pool exhausted. How would you fix?**

A:
• Increase max connections
• Reduce connection timeout
• Close connections properly
• Use connection pooling
• Reduce concurrent requests
• Scale database
• Optimize queries

## 13.10 System Design Questions

**Q46: Design a high-availability system for Promotions.**

A:
• Multi-region deployment (active-active)
• Database geo-replication
• Service Bus geo-replication
• Auto-failover
• Load balancer across regions
• RTO < 5 minutes, RPO < 1 minute
• Health checks and monitoring
• Automated rollback

**Q47: Design a disaster recovery plan.**

A:
• RTO: 4 hours (acceptable downtime)
• RPO: 1 hour (data loss acceptable)
• Backup to secondary region
• Test recovery quarterly
• Document procedures
• Assign responsibilities
• Communication plan
• Runbooks for common failures

**Q48: How would you scale the system for 10x traffic?**

A:
• Horizontal scaling (more pods/instances)
• Database scaling (more RU/s, read replicas)
• Caching layer
• CDN for static content
• Async processing
• Message batching
• Load testing to identify bottlenecks

**Q49: Design a multi-tenant system for multiple retailers.**

A:
• Separate databases per tenant
• Tenant ID in all queries
• Row-level security (RLS)
• Separate Kubernetes namespaces
• Separate API keys per tenant
• Billing per tenant
• Audit trail per tenant

**Q50: How would you migrate from monolith to microservices?**

A:
• Strangler pattern (gradually replace)
• Identify service boundaries
• Extract services one by one
• Implement APIs for communication
• Migrate data
• Run both in parallel
• Switch traffic gradually
• Decommission monolith

## 13.11 Advanced Questions

**Q51: Explain event sourcing vs event streaming.**

A:
• Event Sourcing: Store all changes as events, rebuild state
• Event Streaming: Stream events in real-time to consumers
• Can use both: Event sourcing for audit, event streaming for real-time

**Q52: How would you implement CQRS with event sourcing?**

A:
• Write side: Store events in event store
• Read side: Project events to read model
• Separate databases
• Eventual consistency
• Replay events to rebuild state

**Q53: Design a feature flag system.**

A:
• Store flags in configuration
• Check flag before executing feature
• Toggle without deployment
• Gradual rollout
• A/B testing
• Rollback without deployment

**Q54: How would you implement distributed tracing?**

A:
• Generate correlation ID per request
• Pass through all services
• Log with correlation ID
• Aggregate logs by correlation ID
• Visualize in Application Insights
• Identify slow services

**Q55: Explain circuit breaker vs bulkhead pattern.**

A:
• Circuit Breaker: Fail fast when service unavailable
• Bulkhead: Isolate resources to prevent cascade
• Use together: Circuit breaker detects failure, bulkhead isolates impact

**Q56: How would you implement idempotency?**

A:
• Generate unique request ID
• Store processed request IDs
• Check if already processed
• Return cached response if duplicate
• Safe to retry without side effects

**Q57: Design a rate limiting strategy.**

A:
• Per user: 1000 requests/hour
• Per API key: 10000 requests/hour
• Per IP: 100 requests/minute
• Implement in APIM
• Return 429 Too Many Requests
• Retry-After header

**Q58: How would you implement request validation?**

A:
• Validate format (JSON schema)
• Validate required fields
• Validate field types
• Validate business rules
• Return 400 with details
• Use FluentValidation in .NET

**Q59: Explain the difference between authentication and authorization.**

A:
• Authentication: Who are you? (login)
• Authorization: What can you do? (permissions)
• Example: Login with password (auth), check if admin (authz)

**Q60: How would you implement audit logging?**

A:
• Log all changes (create, update, delete)
• Include user ID, timestamp, old values, new values
• Store in immutable table
• Retention: 7 years
• Query for compliance
• Alert on suspicious activity

## 13.12 Scenario-Based Questions

**Q61: A promotion is accidentally activated for wrong stores. How would you handle?**

A:
• Immediately pause promotion
• Identify affected stores
• Notify POS terminals to remove
• Compensate affected customers
• Update audit log
• Investigate root cause
• Implement preventive measures

**Q62: Service Bus is down. How would you handle?**

A:
• Fallback to synchronous API calls
• Queue messages locally
• Retry when Service Bus recovers
• Alert operations team
• Check Service Bus status
• Failover to backup region
• Investigate root cause

**Q63: Database is running out of space. How would you handle?**

A:
• Archive old data
• Delete unnecessary data
• Increase storage
• Optimize indexes
• Partition tables
• Scale database
• Monitor growth

**Q64: Promotion approval is taking too long. How would you optimize?**

A:
• Reduce approval levels
• Parallel approvals instead of sequential
• Auto-approve low-risk promotions
• Set SLA for approvals
• Escalation if timeout
• Notification reminders

**Q65: Pricing calculation is incorrect. How would you debug?**

A:
• Check promotion rules
• Check applicable products
• Check discount calculation
• Check tax calculation
• Check rounding
• Unit test pricing logic
• Integration test with real data

## 13.13 Code & Implementation Questions

**Q66: Write a simple promotion validation function.**

A:
```csharp
public class PromotionValidator
{
    public Result Validate(Promotion promotion)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(promotion.Name))
            errors.Add("Name is required");
        
        if (promotion.DiscountValue <= 0)
            errors.Add("Discount must be positive");
        
        if (promotion.DiscountType == DiscountType.Percentage && 
            promotion.DiscountValue > 100)
            errors.Add("Percentage cannot exceed 100");
        
        if (promotion.EndDate <= promotion.StartDate)
            errors.Add("End date must be after start date");
        
        return errors.Any() 
            ? Result.Failure(errors) 
            : Result.Success();
    }
}
```

**Q67: Write a circuit breaker implementation.**

A:
```csharp
public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    private const int FailureThreshold = 5;
    private const int TimeoutSeconds = 30;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > TimeSpan.FromSeconds(TimeoutSeconds))
                _state = CircuitState.HalfOpen;
            else
                throw new CircuitBreakerOpenException();
        }
        
        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure();
            throw;
        }
    }
    
    private void OnSuccess()
    {
        _failureCount = 0;
        _state = CircuitState.Closed;
    }
    
    private void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
        if (_failureCount >= FailureThreshold)
            _state = CircuitState.Open;
    }
}
```

**Q68: Write a caching decorator.**

A:
```csharp
public class CachedPromotionRepository : IPromotionRepository
{
    private readonly IPromotionRepository _inner;
    private readonly IDistributedCache _cache;
    
    public async Task<Promotion> GetByIdAsync(string id)
    {
        var cacheKey = $"promotion:{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (cached != null)
            return JsonSerializer.Deserialize<Promotion>(cached);
        
        var promotion = await _inner.GetByIdAsync(id);
        
        if (promotion != null)
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(promotion),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        
        return promotion;
    }
}
```

**Q69: Write a request correlation middleware.**

A:
```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        using (logger.BeginScope(new { CorrelationId = correlationId }))
        {
            await _next(context);
        }
    }
}
```

**Q70: Write a retry policy with exponential backoff.**

A:
```csharp
public class RetryPolicy
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        int retryCount = 0;
        
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                await Task.Delay(delay);
            }
        }
    }
}
```

## 13.14 Communication & Soft Skills

**Q71: How would you explain microservices to a non-technical stakeholder?**

A: "Instead of one big application, we break it into smaller, independent services. Each service handles one responsibility (like managing promotions, calculating prices, etc.). They communicate with each other like departments in a company. This makes it easier to change one service without affecting others, and we can scale services independently based on demand."

**Q72: How would you communicate a critical production issue to management?**

A: "We have an issue affecting X% of users. The root cause is [technical explanation]. Impact: [business impact]. We're taking [immediate action]. ETA to resolution: [time]. We'll provide updates every [interval]."

**Q73: How would you handle disagreement with a team member on architecture?**

A: "I understand your perspective. Let's discuss the trade-offs: your approach has benefits [benefits] but drawbacks [drawbacks]. My approach has benefits [benefits] but drawbacks [drawbacks]. Let's evaluate against our requirements and constraints. If we still disagree, let's involve the tech lead."

**Q74: How would you document a complex system?**

A: "Create architecture diagrams, document each service's responsibility, API documentation, deployment procedures, runbooks for common issues, decision logs explaining why we chose certain technologies."

**Q75: How would you mentor a junior developer?**

A: "Pair programming, code reviews, explain design decisions, encourage questions, assign progressively complex tasks, provide feedback, help with problem-solving."

---

# 14. SYSTEM DESIGN SCENARIOS

## 14.1 Scenario 1: Black Friday Promotion Surge

**Problem:**

• Expected 100x traffic increase on Black Friday
• Promotions must be applied correctly at checkout
• System must handle millions of redemptions
• No downtime allowed

**Solution:**

• **Pre-scaling**: Scale AKS nodes to 50+ before event
• **Database**: Increase Cosmos DB RU/s to 100,000+
• **Caching**: Pre-load active promotions in Redis
• **Async Processing**: Queue redemptions, process asynchronously
• **Circuit Breaker**: Fail gracefully if dependencies slow
• **Monitoring**: Real-time dashboards, alert thresholds
• **Load Testing**: Test with 10x expected traffic beforehand
• **Rollback Plan**: Quick rollback if issues detected

**Key Metrics:**

• P99 latency < 500ms
• Error rate < 0.1%
• Throughput: 100,000 requests/second

## 14.2 Scenario 2: Promotion Overlap Conflict

**Problem:**

• Two promotions on same product, conflicting rules
• Need to determine which discount applies
• Customer expects maximum discount

**Solution:**

• **Conflict Detection**: Check for overlaps before activation
• **Resolution Rules**: Define priority (highest discount wins)
• **Exclusions**: Explicitly exclude products from promotions
• **Validation**: Pricing Engine validates before applying
• **Compensation**: If wrong discount applied, refund difference

**Implementation:**

• Query database for overlapping promotions
• Compare discount amounts
• Apply highest discount
• Log decision for audit
• Alert if unexpected conflicts

## 14.3 Scenario 3: POS Out of Sync

**Problem:**

• POS terminal offline for 2 hours
• Promotions not synced
• Customers can't get discounts

**Solution:**

• **Retry Logic**: Automatically retry sync every 5 minutes
• **Queue**: Store sync requests in queue
• **Fallback**: POS uses cached promotions
• **Reconciliation**: When POS comes back online, sync all pending
• **Notification**: Alert store manager of sync failure

**Implementation:**

• Store Service publishes PromotionSyncEvent
• If POS unreachable, add to retry queue
• Background job retries with exponential backoff
• When POS comes online, process queue
• Verify sync success

## 14.4 Scenario 4: Database Failover

**Problem:**

• Primary database fails
• Need to failover to secondary region
• Minimize data loss and downtime

**Solution:**

• **Geo-Replication**: Secondary database in different region
• **Automatic Failover**: Failover automatically on primary failure
• **RTO < 5 minutes**: Acceptable downtime
• **RPO < 1 minute**: Acceptable data loss
• **Connection String**: Update to secondary region
• **Testing**: Quarterly failover drills

**Implementation:**

• Azure SQL geo-replication enabled
• Automatic failover group configured
• Application uses failover group endpoint
• Monitor replication lag
• Alert on failover

## 14.5 Scenario 5: Malicious Promotion Redemption

**Problem:**

• Customer exploits promotion (redeems multiple times)
• Fraudulent activity detected
• Need to prevent and investigate

**Solution:**

• **Validation**: Check max redemptions per customer
• **Detection**: Monitor unusual patterns
• **Prevention**: Require login for redemption
• **Investigation**: Audit trail of all redemptions
• **Action**: Reverse fraudulent transactions, ban customer

**Implementation:**

• Store redemption per customer
• Check limit before applying discount
• Log all redemptions with customer ID
• Alert on suspicious patterns
• Manual review for high-value fraud

## 14.6 Scenario 6: Service Dependency Chain Failure

**Problem:**

• Pricing Engine depends on Product Service
• Product Service is down
• Pricing Engine can't calculate prices

**Solution:**

• **Circuit Breaker**: Detect Product Service failure
• **Fallback**: Use cached product data
• **Degraded Mode**: Apply promotion without product details
• **Retry**: Retry when service recovers
• **Notification**: Alert team of dependency failure

**Implementation:**

• Implement circuit breaker for Product Service calls
• Cache product data locally
• If circuit open, use cache
• Retry with exponential backoff
• Alert on circuit open

## 14.7 Scenario 7: Promotion Approval Bottleneck

**Problem:**

• Approval queue has 1000+ pending promotions
• Approvers can't keep up
• Promotions delayed

**Solution:**

• **Auto-Approval**: Auto-approve low-risk promotions
• **Parallel Approvals**: Multiple approvers simultaneously
• **SLA**: Set deadline for approvals
• **Escalation**: Escalate if timeout
• **Notification**: Remind approvers of pending

**Implementation:**

• Define low-risk criteria (discount < 10%, < 5 stores)
• Auto-approve if criteria met
• Send parallel approval requests
• Set 24-hour SLA
• Escalate to manager if timeout
• Daily reminder email

## 14.8 Scenario 8: Real-Time Analytics Update

**Problem:**

• Dashboard shows stale data
• Customers want real-time redemption metrics
• Current batch processing too slow

**Solution:**

• **Event Streaming**: Stream redemption events in real-time
• **Materialized Views**: Update views as events arrive
• **WebSocket**: Push updates to client
• **Caching**: Cache aggregates in Redis
• **Batch + Real-time**: Combine for accuracy

**Implementation:**

• Publish redemption events to Service Bus
• Analytics Service subscribes
• Update Cosmos DB read model in real-time
• Aggregate in Redis (1-minute windows)
• Push updates via SignalR to dashboard

## 14.9 Scenario 9: Multi-Region Deployment

**Problem:**

• Need to support customers in multiple regions
• Latency requirements vary by region
• Compliance requirements differ

**Solution:**

• **Active-Active**: Deploy in multiple regions
• **Data Replication**: Replicate data globally
• **Traffic Routing**: Route to nearest region
• **Compliance**: Separate data per region if needed
• **Failover**: Automatic failover between regions

**Implementation:**

• Deploy AKS clusters in US East, Europe, Asia
• Cosmos DB multi-region replication
• Azure Front Door for traffic routing
• Separate databases per region if compliance requires
• Automated failover

## 14.10 Scenario 10: Performance Degradation Investigation

**Problem:**

• API response time increased from 100ms to 500ms
• No code changes
• Need to identify root cause

**Solution:**

• **Monitoring**: Check Application Insights metrics
• **Database**: Check query performance, slow queries
• **Dependencies**: Check external API latency
• **Resources**: Check CPU, memory, disk
• **Network**: Check latency, bandwidth
• **Load**: Check request volume

**Investigation Steps:**

• Check Application Insights dashboard
• Query slow requests
• Check database query execution plans
• Monitor external API response times
• Check resource utilization
• Load test to reproduce
• Profile code if needed
• Check recent deployments
• Rollback if necessary

---

# 15. FINAL PREPARATION TIPS

## Study Approach

• **Week 1**: Learn architecture patterns and Azure services
• **Week 2**: Study microservices design and API design
• **Week 3**: Learn DevOps, Docker, Kubernetes
• **Week 4**: Practice system design scenarios
• **Week 5**: Mock interviews and Q&A
• **Week 6**: Retail domain and final review

## Interview Preparation

• Understand trade-offs (why choose A over B)
• Be able to explain decisions to non-technical people
• Practice drawing architecture diagrams
• Prepare examples from real projects
• Understand failure scenarios and recovery
• Know monitoring and observability approaches
• Be familiar with cost considerations

## Key Concepts to Master

• Microservices decomposition
• CQRS and event sourcing
• Distributed transactions (Saga pattern)
• API design and versioning
• Database strategy (SQL vs NoSQL)
• Event-driven architecture
• CI/CD pipelines
• Kubernetes basics
• Security (authentication, authorization, encryption)
• Monitoring and observability
• Retail domain knowledge

## Common Mistakes to Avoid

• Over-engineering (don't use microservices for simple problems)
• Ignoring scalability from the start
• Not considering security early
• Poor error handling
• Inadequate monitoring
• Not planning for failures
• Ignoring operational concerns
• Not documenting decisions

## Resources

• Microsoft Azure documentation
• Azure Architecture Center
• Microservices patterns (Chris Richardson)
• Domain-Driven Design (Eric Evans)
• Building Microservices (Sam Newman)
• Release It! (Michael Nygard)
• System Design Interview (Alex Xu)

---

**END OF DOCUMENT**

This comprehensive study material covers all aspects needed for the Promotions Management System role. Focus on understanding concepts deeply rather than memorizing, and practice explaining your understanding to others.
