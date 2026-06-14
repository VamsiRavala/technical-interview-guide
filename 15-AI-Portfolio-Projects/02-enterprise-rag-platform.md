# Enterprise RAG Platform

A full Retrieval-Augmented Generation platform — Project 2 of the portfolio: upload documents, ingest them through a chunking/embedding pipeline, search with hybrid + semantic reranking, and answer questions with **inline citations**. This is the project most directly tied to the AI Solutions Architect job description. **Difficulty: ⭐⭐ Intermediate.**

> **Build-order note:** Builds on Project 1's Bicep, auth, and SSE streaming. Project 2's RAG pipeline is later promoted into a standalone, reusable RAG microservice consumed by every agent in Projects 4 and 6.

### Business Problem

A 2,000-person manufacturer has thousands of internal PDFs and Office docs — SOPs, safety manuals, equipment specs, supplier contracts — scattered across SharePoint and file shares. Frontline engineers and support staff waste hours hunting for the right paragraph, and answers are often outdated or wrong. **Who pays:** the VP of Operations and the Knowledge Management office, justified by reduced equipment downtime (faster access to correct SOPs) and lower onboarding time for new engineers. The hard requirement is **trustworthy answers with citations** — every claim must link back to the exact source document and page, because a wrong torque spec or safety procedure has real-world consequences. The platform must respect document-level access controls (not everyone can see contracts).

### Architecture Diagram

```text
                         Entra ID
                            |
 ┌──────────┐  upload/ask  v   ┌────────────────────────────────────────┐
 │ Browser  │──────────────────│ React + TS SPA (Container App / SWA)     │
 └──────────┘<──── answer +    └───────────────┬────────────────────────┘
                  citations                     | Bearer (Rag.Read / Rag.Write)
                                                v
 ┌───────────────────────────────────────────────────────────────────────┐
 │ ASP.NET Core API (Container App)                                        │
 │  ┌──────────┐ ┌──────────────┐ ┌─────────────┐ ┌────────────────────┐  │
 │  │ Upload   │ │ Ingestion    │ │ RAG Query   │ │ Semantic Kernel    │  │
 │  │ endpoint │ │ orchestrator │ │ service     │ │ (prompt + cite)    │  │
 │  └────┬─────┘ └──────┬───────┘ └──────┬──────┘ └─────────┬──────────┘  │
 └───────┼──────────────┼────────────────┼──────────────────┼─────────────┘
         │ blob         │ MI             │ MI               │ MI
         v              v                v                  v
 ┌────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐
 │ Blob       │  │ Doc          │  │ Azure AI     │  │ Azure OpenAI      │
 │ Storage    │  │ Intelligence │  │ Search       │  │ gpt-4o +          │
 │ (raw docs) │  │ (layout/OCR) │  │ (hybrid +    │  │ embedding-3-large │
 └─────┬──────┘  └──────────────┘  │  semantic    │  └──────────────────┘
       │  blob-trigger             │  ranker,     │
       v                           │  vector idx) │
 ┌──────────────┐                  └──────────────┘
 │ Container    │  chunk → embed → push to Search
 │ Apps Job /   │  (ingestion worker)
 │ background   │
 └──────────────┘
       │ metadata
       v
 ┌──────────────┐         ┌─────────────────────┐
 │ Azure SQL    │         │ App Insights        │
 │ (doc meta,   │         │ (ingest + query     │
 │  ACLs, jobs) │         │  telemetry)         │
 └──────────────┘         └─────────────────────┘
```

### Sequence Diagram

```text
Browser    SPA      API        Blob   IngestJob   DocIntel   OpenAI(embed)  AISearch   SQL
  ask "torque spec for pump X?"
  |-------->|        |           |        |           |           |            |        |
  |         | POST /api/query    |        |           |           |            |        |
  |         |------->|  (Rag.Read)|        |           |           |            |        |
  |         |        | embed query text   |           |           |            |        |
  |         |        |--------------------------------------------->|           |        |
  |         |        |<------------------------------- query vector-|           |        |
  |         |        | hybrid search (BM25 + vector) + semantic rank |          |        |
  |         |        |--------------------------------------------------------->|        |
  |         |        |<------------ top-k chunks + scores + metadata-----------|        |
  |         |        | filter chunks by user's doc ACLs                         |------->|
  |         |        |<------------------------------------------- allowed docs-|        |
  |         |        | build grounded prompt (chunks + citation markers)        |        |
  |         |        | chat completion (stream)                                 |        |
  |         |        |------------------------------------------>|              |        |
  |  delta+ |  SSE   |<----------- answer tokens ----------------|              |        |
  |  cites  |<-------| data:{delta} ... data:{citations:[...]}                  |        |
  |<--------|        |                                                          |        |

  (separate) upload flow:
  upload PDF -> API stores to Blob -> blob trigger -> IngestJob:
    DocIntel(layout) -> chunk -> embed(batch) -> push docs to AISearch -> update SQL job=done
```

### Folder Structure

```text
enterprise-ai-portfolio/
└─ projects/
   └─ 02-rag-platform/
      ├─ src/
      │  ├─ Rag.Api/                      # query + upload endpoints
      │  │  ├─ Endpoints/(QueryEndpoints.cs, DocumentsEndpoints.cs)
      │  │  ├─ Services/(RagQueryService.cs, CitationBuilder.cs)
      │  │  └─ Program.cs
      │  ├─ Rag.Ingestion/                # Container Apps Job / worker
      │  │  ├─ Pipeline/
      │  │  │  ├─ LayoutExtractor.cs      # Doc Intelligence
      │  │  │  ├─ Chunker.cs              # semantic/token chunking
      │  │  │  ├─ Embedder.cs             # batch embeddings
      │  │  │  └─ SearchIndexer.cs        # push to AI Search
      │  │  └─ Program.cs
      │  ├─ Rag.Search/                   # AI Search index schema + client
      │  │  ├─ IndexDefinition.cs
      │  │  └─ HybridSearchClient.cs
      │  ├─ Rag.Domain/
      │  └─ Rag.Infrastructure/           # EF Core, Blob, DI
      ├─ web/
      │  ├─ src/
      │  │  ├─ components/(SearchBox, AnswerPanel, CitationCard, UploadDropzone, DocLibrary)
      │  │  ├─ hooks/(useRagQuery.ts, useUpload.ts)
      │  │  └─ App.tsx
      │  └─ vite.config.ts
      ├─ infra/
      │  ├─ main.bicep
      │  └─ modules/(search.bicep, storage.bicep, docintel.bicep,
      │              openai.bicep, sql.bicep, containerapp.bicep, job.bicep)
      ├─ tests/(Rag.Api.Tests, Rag.Ingestion.Tests, web/)
      └─ README.md
```

### Database Design

Two stores, by design:

**1) Azure AI Search** — the vector + keyword store (index `documents-chunks`). This *is* the vector database; do not roll your own.

| Field | Type | Attributes | Purpose |
|---|---|---|---|
| `id` | `Edm.String` | key | `{docId}-{chunkIndex}` |
| `content` | `Edm.String` | searchable | chunk text (BM25) |
| `contentVector` | `Collection(Edm.Single)` | searchable, dims=3072, HNSW | embedding (`text-embedding-3-large`) |
| `docId` | `Edm.String` | filterable | parent document |
| `title` | `Edm.String` | searchable, retrievable | document title |
| `page` | `Edm.Int32` | retrievable | source page for citation |
| `section` | `Edm.String` | retrievable | heading/section |
| `aclGroups` | `Collection(Edm.String)` | filterable | Entra group ids allowed to see chunk |
| `sourceUrl` | `Edm.String` | retrievable | blob/SharePoint link |

Configured with a **vector profile (HNSW)** + **semantic configuration** (for the L2 semantic reranker). Queries use hybrid (vector + BM25) with `semanticConfiguration` reranking.

**2) Azure SQL Database** — operational metadata, not retrieval.

**Table: `Documents`**

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `FileName` | `nvarchar(260)` | original name |
| `BlobPath` | `nvarchar(400)` | container/blob |
| `ContentType` | `nvarchar(100)` | pdf/docx/... |
| `UploadedBy` | `nvarchar(64)` | Entra oid |
| `Status` | `nvarchar(20)` | `Uploaded`/`Processing`/`Indexed`/`Failed` |
| `ChunkCount` | `int` | populated after ingest |
| `AclGroups` | `nvarchar(max)` | JSON array of Entra group ids |
| `CreatedAt` / `IndexedAt` | `datetime2` | timestamps |

**Table: `IngestionJobs`**

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `DocumentId` | `uniqueidentifier` | FK → Documents |
| `Stage` | `nvarchar(30)` | `Extract`/`Chunk`/`Embed`/`Index` |
| `Status` | `nvarchar(20)` | `Running`/`Succeeded`/`Failed` |
| `Error` | `nvarchar(max)` | nullable |
| `StartedAt` / `CompletedAt` | `datetime2` | |

### API Contracts

| Method | Path | Scope | Success | Errors |
|---|---|---|---|---|
| `POST` | `/api/documents` (multipart) | `Rag.Write` | `202` | `400`, `401`, `413` |
| `GET` | `/api/documents` | `Rag.Read` | `200` | `401` |
| `GET` | `/api/documents/{id}/status` | `Rag.Read` | `200` | `401`, `404` |
| `POST` | `/api/query` | `Rag.Read` | `200` (SSE) | `400`, `401`, `429` |
| `DELETE` | `/api/documents/{id}` | `Rag.Write` | `204` | `401`, `403`, `404` |

**Upload**

```json
// POST /api/documents (multipart/form-data: file + metadata) -> 202
{
  "documentId": "9a1c...",
  "status": "Processing",
  "statusUrl": "/api/documents/9a1c.../status"
}
```

**Query (streamed answer + citations)**

```json
// POST /api/query
{ "question": "What is the torque spec for the X200 coolant pump?", "topK": 8 }
```

```text
// 200 text/event-stream
data: {"type":"delta","content":"The X200 coolant pump bolts are torqued to "}
data: {"type":"delta","content":"35 Nm [1]."}
data: {"type":"citations","items":[
  {"marker":"1","docId":"9a1c...","title":"X200 Maintenance SOP","page":42,
   "sourceUrl":"https://.../X200-SOP.pdf#page=42","score":0.91}]}
data: [DONE]
```

The hybrid + reranked search call:

```csharp
var results = await searchClient.SearchAsync<ChunkDoc>(
    searchText: question,                       // BM25 leg
    new SearchOptions {
        VectorSearch = new() { Queries = {
            new VectorizedQuery(queryEmbedding) { KNearestNeighborsCount = 50,
                                                  Fields = { "contentVector" } } } },
        QueryType = SearchQueryType.Semantic,    // L2 semantic reranker
        SemanticSearch = new() { SemanticConfigurationName = "default" },
        Filter = aclFilter,                       // aclGroups/any(g: search.in(g,'...'))
        Size = topK
    });
```

### Security Design

- **AuthN/Z:** Entra ID; scopes `Rag.Read` and `Rag.Write`. Upload/delete require `Rag.Write` (e.g., the "KM editor" app role).
- **Document-level ACLs (security trimming):** each chunk carries `aclGroups`. The query filter restricts results to the caller's Entra group memberships (`groups` claim or Graph lookup), so a user never sees content from documents they can't access — enforced at the **search layer**, not just the UI.
- **Managed identity everywhere:** API + ingestion job use managed identities with least-privilege roles: `Search Index Data Contributor` (ingest) / `Search Index Data Reader` (query), `Storage Blob Data Contributor`, `Cognitive Services OpenAI User`, and `Cognitive Services User` for Document Intelligence. No keys.
- **Key Vault:** any residual secrets; prefer MI auth to all data planes.
- **Prompt injection / grounding safety:** retrieved chunks are clearly delimited and the system prompt instructs the model to (a) answer only from provided context, (b) say "I don't know" when context is insufficient, and (c) ignore instructions inside documents. Add **Azure AI Content Safety groundedness detection** to flag ungrounded ("hallucinated") sentences before returning. Enforce that every factual sentence carries a citation marker; reject/append a warning otherwise.
- **PII/over-sharing:** optionally run Content Safety + PII detection on ingested chunks; redact before indexing.

### Deployment Architecture

- **Hosting:** API on **Container Apps**; ingestion as a **Container Apps Job** (event-driven, triggered by a Blob/Storage Queue message on upload) so heavy embedding work scales independently and doesn't block the API.
- **Environments:** `dev`/`prod`; AI Search `dev` on Basic tier, `prod` on Standard (semantic ranker requires Standard+). Flag in README: semantic ranker has per-query billing.
- **CI/CD:** GitHub Actions matrix — build API + ingestion images, run integration tests against an ephemeral AI Search index, deploy via Bicep. Index schema is created/updated idempotently on deploy (`SearchIndexClient.CreateOrUpdateIndex`).
- **IaC:** Bicep modules for Search, Storage, Doc Intelligence, OpenAI, SQL, Container Apps + Job.

```bicep
// modules/search.bicep (excerpt)
resource search 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: searchName
  location: location
  sku: { name: 'standard' }          // semantic ranker needs standard+
  properties: {
    semanticSearch: 'standard'
    authOptions: { aadOrApiKey: { aadAuthFailureMode: 'http401WithBearerChallenge' } }
  }
  identity: { type: 'SystemAssigned' }
}
```

### Azure Services Used

- **Azure AI Search** — hybrid (vector + keyword) retrieval, HNSW vector index, L2 **semantic reranker**, security trimming via filters. (GA; vector + semantic ranker GA.)
- **Azure OpenAI** — `text-embedding-3-large` (3072-dim embeddings), `gpt-4o` grounded generation. (GA.)
- **Azure AI Document Intelligence** — layout/OCR extraction from PDFs/Office for high-quality chunking. (GA.)
- **Azure Blob Storage** — raw document storage + event trigger source. (GA.)
- **Azure Container Apps + Jobs** — API hosting + event-driven ingestion. (GA.)
- **Azure SQL Database** — document metadata, ACLs, job tracking. (GA.)
- **Azure AI Content Safety** — groundedness detection, PII, prompt shields. (GA; groundedness detection GA.)
- **Entra ID, Key Vault, ACR, App Insights** — identity, secrets, registry, observability. (GA.)

### Resume Bullet Points

- Architected an enterprise **RAG platform** over **2,000+ internal documents** using **Azure AI Search** (hybrid vector + BM25 retrieval with **semantic reranking**) and **Azure OpenAI**, delivering **inline-cited** answers with a measured **>90% citation accuracy** in evaluation.
- Built an event-driven **ingestion pipeline** (Azure Document Intelligence → semantic chunking → batched `text-embedding-3-large` → AI Search) as a **Container Apps Job**, processing documents at **~X pages/min** with automatic retry and per-stage job tracking.
- Implemented **document-level security trimming** at the search layer using Entra **group-based ACL filters**, guaranteeing users only retrieve content they are authorized to see — validated against a security audit.
- Reduced hallucinations by combining strict grounded prompting with **Azure AI Content Safety groundedness detection**, flagging unsupported sentences before they reach the user.
- Wired **end-to-end managed-identity** access to Search, Storage, Document Intelligence, and OpenAI with least-privilege RBAC, removing all API keys from the runtime.
- Automated infra and index provisioning with **Bicep + GitHub Actions**, including idempotent AI Search index schema deployment across `dev`/`prod`.

### GitHub Portfolio Presentation

- **README outline:** pitch ("Trustworthy enterprise Q&A with citations") → GIF of upload → processing status → cited answer with clickable source → "RAG architecture" diagram → "The ingestion pipeline" (chunking strategy, why semantic chunking, embedding model choice) → "Hybrid search + reranking" (the SearchOptions snippet, with a short explanation of why hybrid beats pure vector) → "Security trimming" → "Evaluate it" (link to an eval harness scoring citation accuracy / groundedness) → deploy.
- **Demo:** include a small sample doc set (public-domain manuals) so reviewers can `azd up` and immediately ask questions and see citations resolve to the right page.
- **Highlight to reviewers:** the chunking strategy and the hybrid+semantic query (this is where architects look), the citation builder that maps chunks → page-anchored URLs, and security trimming. Include a one-page "evaluation" section — showing you *measure* RAG quality is a strong differentiator.
