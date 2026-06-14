# REST and API Design

REST (Representational State Transfer) is an architectural style for designing networked applications. REST APIs use HTTP methods to perform CRUD operations (Create, Read, Update, Delete) on resources, which are typically represented as URLs.

Think of it like a restaurant menu:
- **Menu items** = resources (users, products, orders).
- **Waiter** = HTTP methods (GET, POST, PUT, DELETE).
- **Kitchen** = server.

You order something, the kitchen prepares it, and the waiter brings it back.

## REST vs RESTful

- **REST** → the concept (the rules/architecture).
- **RESTful API** → an API that follows REST rules properly.

Example:
- `GET /getUsersList` → not RESTful (RPC-like).
- `GET /users` → RESTful (resource-based).

So: **REST** = theory, **RESTful** = applying it correctly.

## Core Principles of REST API Design

### 1. Resources and URIs

- Everything in a REST API is a resource, represented by a URI (Uniform Resource Identifier).
- Use nouns, not verbs.
- Use plural nouns for collections (`/users`, not `/userList`).

```text
GET    /users
GET    /users/123
POST   /users
PUT    /users/123
DELETE /users/123
```

Avoid: `/getUsers`, `/createUser`, `/getUserById`.

### 2. HTTP Methods

Use the appropriate HTTP verbs for operations:

| HTTP Method | Operation | Description |
|---|---|---|
| GET | Read | Retrieve data |
| POST | Create | Add a new resource |
| PUT | Update | Replace an existing resource |
| PATCH | Partial Update | Update part of a resource |
| DELETE | Delete | Remove a resource |

### 3. Status Codes

Always return proper HTTP status codes:

| Code | Meaning | Example |
|---|---|---|
| 200 | OK (successful read) | GET completed |
| 201 | Created | POST successful |
| 204 | No Content | DELETE successful |
| 400 | Bad Request | Invalid input |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Not allowed |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server failure |

### 4. Request and Response Format

- Prefer JSON for data exchange.
- Use consistent field naming (camelCase or snake_case).

```json
{
  "id": 123,
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

### 5. Versioning

Always version your API to avoid breaking clients when updating. Approaches:

- **Path-based** → `/api/v1/users` (most common)
- **Header-based** → `Accept: application/vnd.myapi.v1+json`
- **Query param** → `/users?version=1`

```text
/api/v1/users
/api/v2/users
```

### 6. Filtering, Sorting, and Pagination

Support query parameters for efficiency:

```text
GET /users?role=admin&sort=createdAt&page=2&limit=20
GET /products?category=shoes&page=2&sort=price
GET /users?offset=100&limit=20
```

Pagination prevents huge responses and improves performance.

### 7. Authentication and Security

- Use OAuth2 or JWT (JSON Web Token) for authentication.
- Always use HTTPS.
- Avoid sending sensitive data in URLs.

### 8. Error Handling

Return clear and structured error responses with status codes:

```json
{
  "error": {
    "code": 400,
    "message": "Email is required"
  }
}
```

| Code | Meaning |
|---|---|
| 400 | Bad request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not found |
| 500 | Server error |

### 9. HATEOAS (Optional for Discoverability)

Include links to related resources so clients can discover what they can do next:

```json
{
  "id": 1,
  "name": "John",
  "links": {
    "self": "/users/1",
    "orders": "/users/1/orders"
  }
}
```

### 10. Documentation

- Use OpenAPI (Swagger) to document endpoints.
- Clearly describe methods, parameters, and example responses.

## PATCH vs PUT

### PUT (Replace)

- **PUT** updates or replaces an entire resource.
- If the resource exists, it is replaced. If it doesn't, sometimes it is created.

Think of PUT like replacing an old document with a new one.

```http
PUT /users/1
{
  "id": 1,
  "name": "Alice",
  "email": "alice@example.com"
}
```

- Replaces the whole user object.
- If `email` is missing, it might get erased.

### PATCH (Partial Update)

- **PATCH** updates only part of a resource, leaving other fields unchanged.

Think of PATCH like editing a few lines in the document.

```http
PATCH /users/1
{
  "email": "alice_new@example.com"
}
```

- Only the email field is updated; `id` and `name` stay the same.

### When to Use Which

| Feature | PUT (Replace) | PATCH (Partial) |
|---|---|---|
| Purpose | Replace entire resource | Update part of a resource |
| Requires all fields? | Yes (usually) | No (only what changes) |
| Risk | Missing fields may get erased | Safer for partial updates |
| Analogy | Replace whole document | Edit just one line |

Rule of thumb: **PUT** for full replacement, **PATCH** for partial update.

## Advanced REST Concepts

Beyond the basics, these principles make APIs robust, scalable, and interview-ready.

### Statelessness

- REST is stateless: each request must include all required information.
- The server does not remember previous requests.

Example: `GET /orders` must include your auth token every time.

- Advantage: Easy to scale across servers.
- Downside: Extra data may need to be sent repeatedly.

### Idempotency

Idempotent = same effect if repeated many times.

| HTTP Method | Idempotent? | Notes |
|---|---|---|
| GET | Yes | Always safe |
| DELETE | Yes | Repeated delete = still gone |
| PUT | Yes | Replaces resource with same data |
| POST | No | Creates new resource each time |

Example:
- `DELETE /users/1` → same result once or 10 times.
- `POST /users` → each call creates a new user.

### Caching

- REST works well with HTTP caching.
- Use headers: `Cache-Control`, `ETag`, `Last-Modified`.
- Improves performance, reduces server load.

Example: Browser caches `GET /products` for 60s.

### Rate Limiting & Throttling

- Prevents abuse of public APIs (e.g., 100 requests per minute per client).
- Implement via API gateways (Kong, NGINX, Apigee).

### Limitations of REST

- **Over-fetching** → Get more data than needed (e.g., `GET /users/1` returns name, email, phone, address when you only need the name).
- **Under-fetching** → Need multiple requests (e.g., `GET /users/1` then `GET /users/1/posts`).
- **No real-time updates** → Only request/response, no live push.
- **Statelessness = extra data** → Each request must include all info (like tokens).

### Alternatives to REST

- **GraphQL** → Avoids over/under-fetching.
- **gRPC** → Faster, supports streaming.
- **WebSockets** → Real-time bidirectional communication.

## Summary

- REST is a stateless, scalable, and simple API design concept; a RESTful API is a proper implementation of REST rules.
- Best practices: use nouns, correct HTTP methods, status codes, versioning, security, pagination, and clear error handling.
- Know the idempotency rules for HTTP methods.
- REST is great, but GraphQL, gRPC, or WebSockets may be better in some cases.
