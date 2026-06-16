# Promotions Management — .NET 9 + Azure Reference Solution

A runnable, end-to-end .NET solution that demonstrates **every topic in the
[03-Azure](../) guide** inside one coherent domain: **promotions management**
(create → approve → activate → distribute → redeem → expire).

It is built so each Azure service appears exactly where it makes sense in a real
retail/POS promotions platform — not as isolated snippets. The whole solution
**builds clean** with the .NET 9 SDK (`dotnet build`).

```
src/
├─ Promotions.Domain/          Pure domain: models, events, abstractions, app service
├─ Promotions.Infrastructure/  One adapter per Azure service (Cosmos, SB, EG, EH, Blob, Queue, NH)
├─ Promotions.Api/             ASP.NET Core minimal API (REST surface + Swagger)
├─ Promotions.Functions/       Isolated-worker Functions: every trigger + Durable workflow
└─ infra/                      Bicep IaC: provisions all services, zone-redundant
```

---

## 🗺️ Topic → code map

| # | Topic | Where it lives | What it shows |
|---|-------|----------------|---------------|
| 01 | **Core Concepts** | [`infra/main.bicep`](infra/main.bicep), [`DependencyInjection.cs`](Promotions.Infrastructure/DependencyInjection.cs) | Resource groups, naming, tags, managed identity + RBAC, `DefaultAzureCredential` |
| 02 | **Regions & Availability Zones** | [`infra/main.bicep`](infra/main.bicep) | `location` param, `zoneRedundant` flag → ZRS storage, zone-redundant Service Bus/Event Hubs/Cosmos/Functions plan |
| 03 | **Azure Functions** | [`Triggers/HttpTriggers.cs`](Promotions.Functions/Triggers/HttpTriggers.cs), [`Triggers/TimerTriggers.cs`](Promotions.Functions/Triggers/TimerTriggers.cs) | HTTP trigger + scheduled timer (CRON expiry scan), DI in isolated worker |
| 04 | **Durable Functions** | [`Durable/`](Promotions.Functions/Durable) | One approval orchestration: **chaining + human interaction + monitor/timeout + fan-out/fan-in**, with retry policies & async-HTTP status API |
| 05 | **Service Bus** | [`Messaging/ServiceBusEventPublisher.cs`](Promotions.Infrastructure/Messaging/ServiceBusEventPublisher.cs), [`Triggers/ServiceBusTriggers.cs`](Promotions.Functions/Triggers/ServiceBusTriggers.cs) | Sessions (FIFO), duplicate detection, subject-based routing, DLQ via maxDeliveryCount |
| 06 | **Event Hubs** | [`Events/EventHubsRedemptionStream.cs`](Promotions.Infrastructure/Events/EventHubsRedemptionStream.cs), [`Triggers/EventHubTriggers.cs`](Promotions.Functions/Triggers/EventHubTriggers.cs) | High-volume redemption telemetry: partition keys, batch consumption, consumer groups |
| 07 | **Event Grid** | [`Events/EventGridPublisher.cs`](Promotions.Infrastructure/Events/EventGridPublisher.cs), [`Triggers/EventGridTriggers.cs`](Promotions.Functions/Triggers/EventGridTriggers.cs) | Reactive fan-out, eventType/subject filtering, push delivery to a Function |
| 08 | **Storage Account** | [`Storage/BlobStorageService.cs`](Promotions.Infrastructure/Storage/BlobStorageService.cs), [`Triggers/BlobTriggers.cs`](Promotions.Functions/Triggers/BlobTriggers.cs) | Blob catalog export + blob trigger reacting to uploads |
| 09 | **Storage Queue** | [`Messaging/StorageQueueClientAdapter.cs`](Promotions.Infrastructure/Messaging/StorageQueueClientAdapter.cs), [`Triggers/QueueTriggers.cs`](Promotions.Functions/Triggers/QueueTriggers.cs) | Cheap background jobs (catalog rebuild), Base64 queue messages, queue trigger |
| 10 | **Cosmos DB** | [`Cosmos/CosmosPromotionRepository.cs`](Promotions.Infrastructure/Cosmos/CosmosPromotionRepository.cs), [`Models/Promotion.cs`](Promotions.Domain/Models/Promotion.cs) | Partition-key point reads, parameterized + cross-partition queries, ETag optimistic concurrency, Session consistency |
| 11 | **Notification Hubs** | [`Notifications/NotificationHubService.cs`](Promotions.Infrastructure/Notifications/NotificationHubService.cs) | Cross-platform device registration (FCM/APNS/WNS) + tag-targeted template push |

Cross-cutting: **Security** (managed identity + Key Vault + RBAC, no secrets in code)
and **Monitoring** (Application Insights wired in both hosts) appear throughout.

---

## 🔄 How the domain ties the services together

```
            ┌──────────── Promotions.Api (REST) ─────────────┐
 marketer → │  POST /promotions  POST /{id}/activate          │
            └───────┬───────────────────┬─────────────────────┘
                    │ persist            │ publish
                    ▼                    ▼
              Cosmos DB (10)   Service Bus (05) ─▶ Functions: ProcessPromotionEvent
                    │          Event Grid (07) ──▶ Functions: OnPromotionEvent ─▶ Notification Hubs (11)
                    │
   approval flow ───┼─▶ Durable Functions (04): validate → wait-for-approver(timeout) → fan-out to stores → activate
                    │
   activate ────────┴─▶ Storage Queue (09): RebuildCatalog ─▶ Blob (08) ─▶ blob trigger ─▶ POS distribution
                    │
   timer (03) ──────┴─▶ ExpirePromotions every 15 min ─▶ Promotion.Expired

   POS terminals ──▶ Event Hubs (06): redemption stream ─▶ Functions: AggregateRedemptions (batch analytics)
```

The `PromotionService` ([`Domain/Services/PromotionService.cs`](Promotions.Domain/Services/PromotionService.cs))
is the write-side coordinator and depends only on **abstractions**, so the API
and the Functions host reuse it unchanged.

---

## ▶️ Build & run

### Build everything
```bash
cd src
dotnet build
```

### Run the API locally
Point `appsettings.json` at the [Cosmos DB Emulator](https://learn.microsoft.com/azure/cosmos-db/local-emulator)
and [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
(the defaults already do), then:
```bash
dotnet run --project Promotions.Api
# browse https://localhost:7xxx/swagger
```
Use [`Promotions.Api.http`](Promotions.Api/Promotions.Api.http) to exercise the endpoints.

### Run the Functions host locally
Requires the [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local):
```bash
cd Promotions.Functions
func start
```

### Provision Azure
```powershell
cd infra
./deploy.ps1 -ResourceGroup rg-promotions-dev -Location eastus
```
`zoneRedundant=true` selects AZ-capable SKUs; set it to `false` for regions/SKUs
without Availability Zone support.

---

## 🎤 Interview talking points baked into the code

- **Service Bus vs Event Grid vs Event Hubs vs Storage Queue** — all four are
  used for the job each is best at: SB for ordered workflow commands, EG for
  reactive fan-out, EH for high-volume telemetry, Storage Queue for cheap jobs.
- **Why partition Cosmos by `region`** — co-locates a campaign's reads/writes,
  keeps point reads at ~1 RU, and the expiry scan shows the cross-partition cost.
- **Durable Functions patterns** — a single orchestration intentionally combines
  chaining, human-in-the-loop approval, a timeout monitor, and fan-out/fan-in.
- **Idempotency & ordering** — `MessageId` duplicate detection + `SessionId`
  FIFO on Service Bus; partition keys on Event Hubs.
- **Security posture** — managed identity + RBAC via Bicep; connection strings
  are only a local-dev fallback in `DependencyInjection.cs`.

> Bicep is authored to current API versions but was not compiled here (no Bicep
> CLI in the build environment); validate with `az bicep build -f infra/main.bicep`
> before deploying.
