# GraphQL

## What is GraphQL?

**GraphQL** = Query Language for APIs, created by Facebook (2015).

- Clients request exactly the data they need.
- All queries go through a single endpoint (`/graphql`).
- Responses are in JSON.
- Strongly typed (type checking on every field).

Think of GraphQL like a custom sandwich shop: you choose the exact ingredients (fields), and you get only what you ordered. Originally built at Facebook to handle deeply nested data (e.g., friends-of-friends four or five levels deep) without over-fetching.

Example query and mutation:

```graphql
query {
  getProduct(id: "f543f9a284603ba2c7fe") {
    price
    name
    description
    stores {
      store
    }
  }
}
```

```graphql
mutation {
  createProduct(input: {
    name: "Widget2",
    description: "another widget2",
    price: 33.99,
    soldout: false,
    stores: [
      { store: "NY" }, { store: "CA" }
    ]
  }) {
    price
    name
    description
    id
  }
}
```

## Key Components of GraphQL

### 1. Schema

Defines the blueprint of the API — includes types, queries, mutations, and subscriptions.

```graphql
type User {
  id: ID!
  name: String!
  email: String!
}

type Query {
  getUser(id: ID!): User
}
```

### 2. Queries

For reading data (like GET in REST).

```graphql
query {
  getUser(id: 1) {
    name
    email
  }
}
```

Response:

```json
{
  "getUser": {
    "name": "Alice",
    "email": "alice@mail.com"
  }
}
```

### 3. Mutations

For writing/updating data (like POST/PUT/DELETE in REST).

```graphql
mutation {
  createUser(name: "Alice", email: "alice@mail.com") {
    id
    name
  }
}
```

### 4. Subscriptions

For real-time updates (via WebSockets).

```graphql
subscription {
  newMessage {
    id
    text
  }
}
```

### 5. Resolvers

Functions that fetch the data for queries/mutations.

```graphql
# Example resolver (Node.js)
# Query.getUser: (_, { id }) => db.users.find(user => user.id === id)
```

### 6. Types

Strongly typed system. Built-in: `String`, `Int`, `Boolean`, `ID`, `Float`. Supports custom types.

## Aliases

Aliases let you run multiple queries (or the same field with different arguments) in a single request by giving each result a custom name. This is useful when you need, for example, the same field fetched with two different IDs side by side in one response.

```graphql
query {
  first: getProduct(id: "1") { name price }
  second: getProduct(id: "2") { name price }
}
```

## Fragments

Fragments let you define a reusable set of fields and include them across multiple queries, keeping requests DRY and consistent.

```graphql
fragment ProductFields on Product {
  id
  name
  price
}

query {
  getProduct(id: "1") {
    ...ProductFields
    description
  }
}
```

## Implementing GraphQL in .NET Core with Hot Chocolate

This builds a GraphQL API in .NET Core using the **Hot Chocolate** library by ChilliCream. The sample exposes **Authors** and **Books** with queries (filtering, sorting, pagination), a mutation to add a book, and an in-memory EF Core database.

### 1. Create the Project & Add Packages

```bash
dotnet new web -n GraphQLDemo
cd GraphQLDemo

# Add GraphQL + EF Core packages
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data
dotnet add package HotChocolate.Data.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 2. Add Models & DbContext

```csharp
using Microsoft.EntityFrameworkCore;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = default!;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
}
```

### 3. Define GraphQL Types (Query & Mutation)

```csharp
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

public class Query
{
    [UseDbContext(typeof(AppDbContext))]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors([ScopedService] AppDbContext db)
        => db.Authors;

    [UseDbContext(typeof(AppDbContext))]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Book> GetBooks([ScopedService] AppDbContext db)
        => db.Books;
}

public record AddBookInput(string Title, int AuthorId);
public record AddBookPayload(Book Book);

public class Mutation
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<AddBookPayload> AddBookAsync(
        AddBookInput input,
        [ScopedService] AppDbContext db,
        CancellationToken ct)
    {
        var book = new Book { Title = input.Title, AuthorId = input.AuthorId };
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);
        await db.Entry(book).Reference(b => b.Author).LoadAsync(ct);
        return new AddBookPayload(book);
    }
}

public class AuthorType : ObjectType<Author>
{
    protected override void Configure(IObjectTypeDescriptor<Author> d)
    {
        d.Field(a => a.Id);
        d.Field(a => a.Name);
        d.Field(a => a.Books)
         .UsePaging()
         .UseFiltering()
         .UseSorting();
    }
}

public class BookType : ObjectType<Book>
{
    protected override void Configure(IObjectTypeDescriptor<Book> d)
    {
        d.Field(b => b.Id);
        d.Field(b => b.Title);
        d.Field(b => b.Author)
         .ResolveWith<Resolvers>(r => r.GetAuthor(default!, default!))
         .UseProjection();
    }

    private sealed class Resolvers
    {
        [UseDbContext(typeof(AppDbContext))]
        public Author GetAuthor([Parent] Book book, [ScopedService] AppDbContext db)
            => db.Authors.First(a => a.Id == book.AuthorId);
    }
}
```

### 4. Wire Everything in Program.cs

```csharp
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPooledDbContextFactory<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("AppDb"));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<AuthorType>()
    .AddType<BookType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddInMemorySubscriptions();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();

    if (!db.Authors.Any())
    {
        var a1 = new Author { Name = "Ursula K. Le Guin" };
        var a2 = new Author { Name = "Douglas Adams" };
        db.Authors.AddRange(a1, a2);
        db.Books.AddRange(
            new Book { Title = "The Left Hand of Darkness", Author = a1 },
            new Book { Title = "The Dispossessed", Author = a1 },
            new Book { Title = "The Hitchhiker's Guide to the Galaxy", Author = a2 }
        );
        db.SaveChanges();
    }
}

app.MapGraphQL("/graphql");

app.Run();
```

Run the app:

```bash
dotnet run
```

Open `http://localhost:5000/graphql` to use the Banana Cake Pop IDE.

### 5. Example Queries

List books with author:

```graphql
query {
  books(first: 10, where: { title: { contains: "the" } }, order: { title: ASC }) {
    nodes {
      id
      title
      author { id name }
    }
  }
}
```

List authors with books:

```graphql
query {
  authors(first: 5) {
    nodes {
      id
      name
      books(first: 2, order: { title: ASC }) {
        nodes { id title }
      }
    }
  }
}
```

Add a book:

```graphql
mutation {
  addBook(input: { title: "A Wizard of Earthsea", authorId: 1 }) {
    book {
      id
      title
      author { id name }
    }
  }
}
```

### Why Hot Chocolate?

- Minimal boilerplate.
- Built-in filtering, sorting, paging.
- Automatic projection for EF Core.
- Easy to extend with auth, subscriptions, validation.

Next steps: add `[Authorize]` auth, validations with FluentValidation, subscriptions for real-time updates, and swap the InMemory DB with SQL Server/Postgres.

## Is GraphQL Stateless?

Yes — GraphQL is stateless for queries and mutations, but subscriptions are stateful because they use persistent connections (WebSockets).

### Why GraphQL is Stateless

- Built on top of HTTP, which is stateless by default.
- Each request is independent; the server does not remember past queries or sessions.
- The client must send all info (auth, variables, query) with every request.

Benefits: scalable (any server can handle requests), simpler architecture, more secure (no hidden server-side state).

### Where Confusion Comes From

1. **Variables** — sent with each request, not stored on the server.
2. **Authentication** — done via tokens (e.g., JWT in headers), included on every request.
3. **Subscriptions** — use WebSockets (or SSE); the connection stays open and the server pushes updates, which makes them stateful.

| Feature | Stateless? | Why |
|---|---|---|
| GraphQL Queries | Yes | Each request is independent |
| GraphQL Mutations | Yes | Separate calls per mutation |
| GraphQL Subscriptions | No | Requires persistent WebSocket connection |

So: GraphQL is mostly stateless, except subscriptions.

## Authentication & Authorization in GraphQL

Unlike REST (multiple endpoints), GraphQL has a single endpoint (`/graphql`). So auth must be handled at the request, resolver, or schema level.

### Authentication

Authentication = verifying who the user is.

- GraphQL requests use `POST /graphql`.
- Tokens (JWT, OAuth, API keys) are sent in HTTP headers.
- Middleware verifies the token and attaches user info to `context`.

```http
POST /graphql
Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Body:
{
  "query": "{ me { id name email } }"
}
```

Example (Apollo Server, Node.js):

```text
const server = new ApolloServer({
  typeDefs,
  resolvers,
  context: ({ req }) => {
    const token = req.headers.authorization || "";
    const user = verifyToken(token); // decode JWT
    return { user };
  },
});
```

### Authorization

Authorization = deciding what actions/resources a user can access. Since GraphQL is one endpoint, authorization is applied at the resolver or schema level.

Schema directives:

```graphql
type Query {
  users: [User] @auth(role: "admin")
  me: User @auth
}
```

Middleware in .NET (Hot Chocolate):

```csharp
public class Query {
    [Authorize(Roles = new[] { "Admin" })]
    public IQueryable<User> GetUsers([Service] AppDbContext db) => db.Users;
}
```

### Why Context Matters

- Context is created per request.
- Holds `user`, `roles`, and request metadata.
- Resolvers check `context` for permissions.

### Best Practices

- Authentication: verify the token in middleware before executing GraphQL.
- Authorization: apply at resolver/schema level.
- Granularity: restrict at query/mutation level (e.g., `deleteUser` only for admins) and field level (e.g., `email` visible only to the owner).
- Prevent leaks: don't expose unauthorized fields in the schema.
- Disable introspection in production to stop attackers from exploring the schema.

REST secures by endpoint; GraphQL secures by resolver/field.

## Tools in the GraphQL Ecosystem

- **Hot Chocolate** → .NET GraphQL server (used above).
- **Apollo Server/Client** → most popular implementation in the JS ecosystem.
- **GraphQL Playground / GraphiQL** → interactive IDEs for queries.
- **Relay (Facebook)** → advanced client library.
- **Hasura** → auto-generates a GraphQL API from a database.
- **Prisma** → ORM & database toolkit.
- **Subscriptions-Transport-WS** → enables real-time GraphQL subscriptions.

## Best Practices for GraphQL

1. Design a strong schema with clear, well-structured types.
2. Use pagination for large lists (`limit`, `offset`, cursors).
3. Limit query depth/complexity to prevent abuse with expensive queries.
4. Enable caching (persisted queries, Apollo caching).
5. Secure your API (auth, disable introspection in production).
6. Batch resolvers — use DataLoader to avoid N+1 queries.
7. Return useful errors via the `errors` field.
8. Monitor & log queries (Apollo Studio, tracing tools).

## Limitations of GraphQL

- More complex to implement server-side.
- Caching is harder compared to REST.
- Risk of expensive queries if unrestricted.
- Overkill for simple CRUD APIs.

## GraphQL vs OData

### OData

- **What it is:** Open Data Protocol, a REST-based standard by Microsoft.
- **Supports:** Querying (`$filter`, `$select`, `$expand`, `$orderby`, `$top`) and full CRUD (`POST`, `PUT`, `PATCH`, `DELETE`).
- **Style:** URL-driven query language (`/Products?$filter=Price gt 100&$orderby=Name`).
- **Strengths:** Great for relational/data-service-heavy backends (SQL, SAP, Dynamics); works with existing REST tools; strong metadata exposure (`$metadata` endpoint).

### GraphQL (in comparison)

- **What it is:** Query language for APIs created by Facebook.
- **Supports:** Queries (read), mutations (create/update/delete), subscriptions (real-time updates).
- **Style:** JSON-like request body (`{ products(filter: { price_gt: 100 }) { name } }`).
- **Strengths:** Client decides exactly what data it wants (no over/under-fetching); flexible, strongly typed schema; works well for aggregating multiple backends.

### Key Differences

| Feature | OData | GraphQL |
|---|---|---|
| Transport | REST (HTTP + URL params) | Single endpoint, JSON body |
| Queries | Strong support, URL syntax | Strong support, JSON-like syntax |
| Mutations (CUD) | Supported via HTTP verbs (`POST`, `PATCH`) | First-class `mutation` concept |
| Subscriptions / Realtime | Not native | Built-in (`subscription`) |
| Over/under-fetching | Possible (fixed response schema) | Avoided (client controls fields) |
| Tooling / Ecosystem | Strong in Microsoft/enterprise ecosystems | Huge ecosystem (Apollo, Relay, Hasura, etc.) |
| Learning Curve | Easier if familiar with REST | Steeper, but more flexible |

### Analogy: Restaurant Menu vs. À la Carte

- **OData = Restaurant Menu (fixed combos):** you can filter (no pickles, large fries) and choose a combo (endpoint + `$filter`, `$select`), but you're still bound by the menu structure.
- **GraphQL = À la Carte Ordering:** *"I want just a burger patty, no bun. Add sweet potato fries, and give me the drink name but not its price."* You pick exactly what you want, in what shape, all in one request.

### Real-World Use Cases

**When to Use OData:**
1. Enterprise/ERP systems (Microsoft Dynamics, SAP, Azure expose data as OData) — e.g., `/Employees?$filter=Department eq 'HR'`.
2. Data services / reporting APIs (Power BI, Tableau connect directly to OData feeds) — e.g., `/Sales?$filter=Year eq 2024&$select=Product,Revenue`.
3. CRUD on data entities — e.g., `POST /Products`, `PATCH /Products(123)`, `DELETE /Products(123)`.

**When to Use GraphQL:**
1. Modern frontend apps (React, Angular, mobile) — e.g., `{ product { id, name, price, reviews { rating } } }`.
2. API gateway / aggregation — e.g., a travel site fetching from Flights + Hotels + Car Rentals APIs in one query.
3. Real-time applications — e.g., `subscription { messages }` for chat or dashboards.
4. Mobile devices (limited bandwidth) — e.g., `{ product { name, thumbnail } }`.

### Quick Decision Guide

- If your backend is enterprise/data-heavy, standardized, and tool-driven → OData.
- If your frontend is dynamic, client-driven, or needs aggregation/flexibility → GraphQL.

```text
API Need
  -> Enterprise / Relational Data? -- Yes --> Use OData
                                  -- No  --> Frontend Flexibility Needed?
                                               -- Yes --> Use GraphQL
                                               -- No  --> Consider Simple REST
```

## Summary

- GraphQL is a query language where clients ask for exactly what they need through a single endpoint.
- Core components: schema, queries, mutations, subscriptions, resolvers, types; plus aliases and fragments for flexible, reusable requests.
- In .NET, Hot Chocolate provides a low-boilerplate server with built-in filtering, sorting, paging, and EF Core projection.
- Best practices: strong schema design, pagination, query limits, caching, security, and error handling.
- Use GraphQL when clients need flexible data and you want to avoid over/under-fetching; consider OData when the backend is enterprise/relational and tool-driven.
