// =============================================================================
// Promotions Management — Azure infrastructure (Bicep)
//
// Covers the "Core Concepts" (01) and "Regions & Availability Zones" (02)
// topics, and provisions every service used by the solution:
//   Cosmos DB (10), Service Bus (05), Event Grid (07), Event Hubs (06),
//   Storage Account + Queue (08, 09), Notification Hubs (11), and the
//   Function App + App Service that host the code.
//
// High availability: zone-redundant SKUs are selected where the service and
// region support Availability Zones, so a single datacentre (zone) failure
// does not take the workload down.
// Security: a user-assigned managed identity is created and granted data-plane
// roles, so app code authenticates with DefaultAzureCredential — no secrets.
// =============================================================================

targetScope = 'resourceGroup'

@description('Primary Azure region. Must be an Availability-Zone-enabled region for zone redundancy (e.g. eastus, westeurope).')
param location string = resourceGroup().location

@description('Short name prefix for all resources, e.g. "promo".')
@minLength(3)
@maxLength(10)
param namePrefix string = 'promo'

@description('Environment moniker used in names and tags.')
@allowed(['dev', 'test', 'prod'])
param environment string = 'dev'

@description('Enable zone redundancy. Turn off for regions/SKUs without AZ support.')
param zoneRedundant bool = true

var suffix = uniqueString(resourceGroup().id)
var tags = {
  workload: 'promotions-management'
  environment: environment
  managedBy: 'bicep'
}

// -----------------------------------------------------------------------------
// Identity & secrets (security best practices)
// -----------------------------------------------------------------------------
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${namePrefix}-id-${environment}'
  location: location
  tags: tags
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${namePrefix}kv${suffix}'
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
  }
}

// -----------------------------------------------------------------------------
// Cosmos DB (10) — globally distributed, Session consistency, zone redundant
// -----------------------------------------------------------------------------
resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: '${namePrefix}-cosmos-${suffix}'
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    defaultConsistencyLevel: 'Session'
    consistencyPolicy: { defaultConsistencyLevel: 'Session' }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: zoneRedundant
      }
    ]
    capabilities: []
    enableAutomaticFailover: true
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmos
  name: 'promotions'
  properties: {
    resource: { id: 'promotions' }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: cosmosDb
  name: 'promotions'
  properties: {
    resource: {
      id: 'promotions'
      // Partition by region — co-locates a campaign's reads/writes (see Promotion.cs).
      partitionKey: { paths: ['/region'], kind: 'Hash' }
      indexingPolicy: { indexingMode: 'consistent', automatic: true }
    }
    options: {
      autoscaleSettings: { maxThroughput: 4000 }
    }
  }
}

// -----------------------------------------------------------------------------
// Service Bus (05) — Premium is zone-redundant by default; queue with sessions
// -----------------------------------------------------------------------------
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${namePrefix}-sb-${suffix}'
  location: location
  tags: tags
  sku: {
    name: zoneRedundant ? 'Premium' : 'Standard'
    tier: zoneRedundant ? 'Premium' : 'Standard'
    capacity: zoneRedundant ? 1 : 0
  }
  properties: {
    zoneRedundant: zoneRedundant
  }
}

resource promotionEventsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'promotion-events'
  properties: {
    requiresSession: true            // ordered per-promotion processing (FIFO)
    requiresDuplicateDetection: true // dedupe on MessageId
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

// -----------------------------------------------------------------------------
// Event Grid (07) — custom topic for reactive fan-out
// -----------------------------------------------------------------------------
resource eventGridTopic 'Microsoft.EventGrid/topics@2023-12-15-preview' = {
  name: '${namePrefix}-grid-${suffix}'
  location: location
  tags: tags
  properties: {
    inputSchema: 'EventGridSchema'
  }
}

// -----------------------------------------------------------------------------
// Event Hubs (06) — redemption telemetry stream, partitioned, zone redundant
// -----------------------------------------------------------------------------
resource eventHubsNs 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: '${namePrefix}-eh-${suffix}'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    zoneRedundant: zoneRedundant
  }
}

resource redemptionsHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = {
  parent: eventHubsNs
  name: 'redemptions'
  properties: {
    partitionCount: 8     // unit of parallelism for consumers
    messageRetentionInDays: 7
  }
}

resource redemptionsConsumerGroup 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = {
  parent: redemptionsHub
  name: 'analytics'
}

// -----------------------------------------------------------------------------
// Storage Account (08) + Queue (09) — ZRS for zone redundancy
// -----------------------------------------------------------------------------
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: '${namePrefix}st${suffix}'
  location: location
  tags: tags
  sku: {
    name: zoneRedundant ? 'Standard_ZRS' : 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource catalogContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'promotion-catalogs'
  properties: { publicAccess: 'None' }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource jobQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = {
  parent: queueService
  name: 'promotion-jobs'
}

// -----------------------------------------------------------------------------
// Notification Hubs (11)
// -----------------------------------------------------------------------------
resource notificationHubNs 'Microsoft.NotificationHubs/namespaces@2023-09-01' = {
  name: '${namePrefix}-ntf-${suffix}'
  location: location
  tags: tags
  sku: { name: 'Standard' }
}

resource notificationHub 'Microsoft.NotificationHubs/namespaces/notificationHubs@2023-09-01' = {
  parent: notificationHubNs
  name: 'promotions-hub'
  location: location
}

// -----------------------------------------------------------------------------
// Observability + hosting (Function App on an Elastic Premium plan)
// -----------------------------------------------------------------------------
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-law-${environment}'
  location: location
  tags: tags
  properties: { sku: { name: 'PerGB2018' } }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-ai-${environment}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource functionPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${namePrefix}-func-plan-${environment}'
  location: location
  tags: tags
  sku: {
    name: 'EP1'   // Elastic Premium — supports zone redundancy + VNET
    tier: 'ElasticPremium'
  }
  properties: {
    maximumElasticWorkerCount: 10
    zoneRedundant: zoneRedundant
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${namePrefix}-func-${suffix}'
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      appSettings: [
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'AzureWebJobsStorage__accountName', value: storage.name }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'Azure__Cosmos__AccountEndpoint', value: cosmos.properties.documentEndpoint }
        { name: 'Azure__ServiceBus__FullyQualifiedNamespace', value: '${serviceBus.name}.servicebus.windows.net' }
        { name: 'Azure__EventHubs__FullyQualifiedNamespace', value: '${eventHubsNs.name}.servicebus.windows.net' }
        { name: 'Azure__EventGrid__TopicEndpoint', value: eventGridTopic.properties.endpoint }
        { name: 'Azure__Storage__BlobServiceUri', value: storage.properties.primaryEndpoints.blob }
        { name: 'Azure__Storage__QueueServiceUri', value: storage.properties.primaryEndpoints.queue }
      ]
    }
  }
}

// -----------------------------------------------------------------------------
// Data-plane RBAC — grant the managed identity least-privilege access
// -----------------------------------------------------------------------------
var roleIds = {
  // Built-in role definition GUIDs (stable across tenants)
  serviceBusDataOwner: '090c5cfd-751d-490a-894a-3ce6f1109419'
  eventHubsDataOwner: 'f526a384-b230-433a-b45c-95f59c4a2dec'
  storageBlobDataContributor: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  storageQueueDataContributor: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
  eventGridDataSender: 'd5a91429-5739-47e2-a06b-3470a27159e7'
}

resource sbRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, identity.id, roleIds.serviceBusDataOwner)
  scope: serviceBus
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.serviceBusDataOwner)
  }
}

resource ehRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventHubsNs.id, identity.id, roleIds.eventHubsDataOwner)
  scope: eventHubsNs
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.eventHubsDataOwner)
  }
}

resource blobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, identity.id, roleIds.storageBlobDataContributor)
  scope: storage
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.storageBlobDataContributor)
  }
}

resource queueRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, identity.id, roleIds.storageQueueDataContributor)
  scope: storage
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.storageQueueDataContributor)
  }
}

resource gridRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventGridTopic.id, identity.id, roleIds.eventGridDataSender)
  scope: eventGridTopic
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.eventGridDataSender)
  }
}

// -----------------------------------------------------------------------------
// Outputs — wire these into app configuration
// -----------------------------------------------------------------------------
output cosmosEndpoint string = cosmos.properties.documentEndpoint
output serviceBusNamespace string = '${serviceBus.name}.servicebus.windows.net'
output eventHubsNamespace string = '${eventHubsNs.name}.servicebus.windows.net'
output eventGridEndpoint string = eventGridTopic.properties.endpoint
output storageAccountName string = storage.name
output functionAppName string = functionApp.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output managedIdentityClientId string = identity.properties.clientId
