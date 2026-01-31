# 🌐 REST vs GraphQL vs gRPC

## 🔹 What Are They?

Three major **API communication styles**:\
- **REST** → Resource-based, uses HTTP verbs.\
- **GraphQL** → Query language, client specifies exactly what data they
want.\
- **gRPC** → Remote Procedure Call, binary protocol for high
performance.

------------------------------------------------------------------------

## 🔹 Flowchart: How They Work

``` mermaid
flowchart TD
    A[Client] -->|HTTP GET/POST/PUT/DELETE| B[REST Server]
    A -->|GraphQL Query | C[GraphQL Server]
    A -->|RPC Method Call binary| D[gRPC Server]

    B -->|JSON/XML Response| A
    C -->|JSON exact fields| A
    D -->|Protobuf binary| A
```

------------------------------------------------------------------------

## 🔹 REST (Representational State Transfer)

### Example

    GET /users/1 → { "id": 1, "name": "Alice" }

### ✅ Pros

-   Simple, widely used.\
-   Works with HTTP tools (Postman, browser).\
-   Easy caching.

### ❌ Cons

-   Over-fetching / under-fetching.\
-   Multiple requests for complex data.

------------------------------------------------------------------------

## 🔹 GraphQL

### Example Query

``` graphql
query {
  user(id: 1) {
    name
    email
    posts {
      title
    }
  }
}
```

👉 Response:

``` json
{
  "user": {
    "name": "Alice",
    "email": "alice@mail.com",
    "posts": [{ "title": "GraphQL vs REST" }]
  }
}
```

### ✅ Pros

-   No over/under-fetching.\
-   Single endpoint.\
-   Strongly typed schema.

### ❌ Cons

-   Complex to implement.\
-   Caching is harder.

------------------------------------------------------------------------

## 🔹 gRPC (Google Remote Procedure Call)

### Example `.proto`

``` proto
service UserService {
  rpc GetUser (UserRequest) returns (UserResponse);
}

message UserRequest {
  int32 id = 1;
}

message UserResponse {
  string name = 1;
  string email = 2;
}
```

### ✅ Pros

-   Extremely fast (binary over HTTP/2).\
-   Supports streaming.\
-   Strongly typed, auto-generated code.

### ❌ Cons

-   Harder debugging (binary not human-readable).\
-   Browser support limited (needs gRPC-web).

------------------------------------------------------------------------

## 🔹 Comparison Table

  ----------------------------------------------------------------------------------
  Feature                      REST 🌐          GraphQL 🔍        gRPC ⚡
  ---------------------------- ---------------- ----------------- ------------------
  **Style**                    Resource-based   Query language    RPC (functions)

  **Transport**                HTTP/1.1         HTTP/1.1 or       HTTP/2
                                                HTTP/2            

  **Data Format**              JSON, XML        JSON              Protocol Buffers
                                                                  (binary)

  **Schema**                   None             Strongly typed    Strongly typed

  **Over/Under Fetching**      Yes              No                No

  **Performance**              Medium           Good              Very High

  **Streaming**                ❌ No            ❌ Limited        ✅ Yes

  **Ease of Use**              Easy             Medium            Harder

  **Best For**                 Public APIs,     Flexible data     High-performance
                               CRUD apps        needs             microservices
  ----------------------------------------------------------------------------------

------------------------------------------------------------------------

## 🔹 Real-Life Analogy

-   **REST** → Restaurant menu 📖 (choose from fixed dishes).\
-   **GraphQL** → Custom sandwich shop 🥪 (choose exact ingredients).\
-   **gRPC** → Personal chef 👨‍🍳 (call direct methods).

------------------------------------------------------------------------

## ✅ Summary

-   **REST** → Simple, universal, but can cause over/under-fetching.\
-   **GraphQL** → Flexible, efficient, but more complex.\
-   **gRPC** → Super fast, streaming-ready, but complex and less
    human-friendly.


# 🤔 When to Use REST, GraphQL, or gRPC

Choosing the right API communication style depends on your **use case,
performance needs, and client requirements**.

------------------------------------------------------------------------

## 🔹 When to Use REST

✅ Use REST when:\
- Building **public APIs** (GitHub, Twitter, Stripe).\
- Clients are **browsers, mobile apps** with simple data needs.\
- You want **simplicity, caching, and compatibility**.\
- Data fits CRUD patterns.

⚠️ Avoid if:\
- You often fetch **nested/related data**.\
- You need **real-time streaming**.

👉 Example: E-commerce API → `/products`, `/orders`, `/users`.

------------------------------------------------------------------------

## 🔹 When to Use GraphQL

✅ Use GraphQL when:\
- Clients need **different data views** (mobile vs web).\
- You want to **avoid over/under-fetching**.\
- You have **complex relationships** (users → posts → comments).\
- API is consumed by multiple teams.

⚠️ Avoid if:\
- API is **simple CRUD**.\
- Caching is critical.\
- Setup complexity is a concern.

👉 Example: Social media app → query user, posts, likes, comments in one
request.

------------------------------------------------------------------------

## 🔹 When to Use gRPC

✅ Use gRPC when:\
- You need **high performance** & **low latency**.\
- Services are **microservices** inside a system.\
- You need **real-time streaming** (chat, IoT, video).\
- You want **strongly typed APIs** with auto-generated clients.

⚠️ Avoid if:\
- API is **public/external** (debugging harder).\
- Consumers are **developers** testing in browsers/Postman.

👉 Example: Microservices at Uber/Google, real-time trading, video
streaming.

------------------------------------------------------------------------

## 🔹 Decision Matrix

  Scenario                                    Best Choice
  ------------------------------------------- -----------------------
 -  **Simple CRUD API**                         REST 🌐
-   **Public API for developers**               REST 🌐
-   **Complex queries, avoid over-fetching**    GraphQL 🔍
-   **One query, multiple related resources**   GraphQL 🔍
-   **High-performance microservices**          gRPC ⚡
-   **Streaming (chat, IoT, gaming, video)**    gRPC ⚡
-   **Different clients (web, mobile, IoT)**    GraphQL 🔍 or gRPC ⚡
 -  **When in doubt (safe default)**            REST 🌐

------------------------------------------------------------------------

## 🔹 Real-Life Analogy

-   **REST** → McDonald's 🍔: fixed menu, everyone gets the same set.\
-   **GraphQL** → Subway 🥪: build your sandwich with exact
    ingredients.\
-   **gRPC** → Private chef 👨‍🍳: efficient, high-performance, tailored
    service.

------------------------------------------------------------------------

## ✅ Summary

-   **REST** → Simplicity & compatibility.\
-   **GraphQL** → Flexibility & client-driven queries.\
-   **gRPC** → High-performance, streaming, and internal microservices.

