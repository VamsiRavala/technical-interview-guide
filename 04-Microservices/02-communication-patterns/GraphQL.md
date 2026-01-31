# 🔍 GraphQL Explained

## 🔹 What is GraphQL?

**GraphQL** = **Query Language for APIs**, created by Facebook (2015).\
- Clients request **exactly the data they need**.\
- All queries go through **a single endpoint** (`/graphql`).\
- Responses are in **JSON**.

👉 Think of GraphQL like a **custom sandwich shop 🥪**:\
You choose the exact ingredients (fields), and you get only what you
ordered.

------------------------------------------------------------------------

## 🔹 Key Components of GraphQL

### 1. **Schema**

-   Defines the blueprint of the API.\
-   Includes **types, queries, mutations, subscriptions**.

``` graphql
type User {
  id: ID!
  name: String!
  email: String!
}

type Query {
  getUser(id: ID!): User
}
```

------------------------------------------------------------------------

### 2. **Queries**

-   For **reading data** (like GET in REST).

``` graphql
query {
  getUser(id: 1) {
    name
    email
  }
}
```

👉 Response:

``` json
{
  "getUser": {
    "name": "Alice",
    "email": "alice@mail.com"
  }
}
```

------------------------------------------------------------------------

### 3. **Mutations**

-   For **writing/updating data** (like POST/PUT/DELETE in REST).

``` graphql
mutation {
  createUser(name: "Alice", email: "alice@mail.com") {
    id
    name
  }
}
```

------------------------------------------------------------------------

### 4. **Subscriptions**

-   For **real-time updates** (via WebSockets).

``` graphql
subscription {
  newMessage {
    id
    text
  }
}
```

------------------------------------------------------------------------

### 5. **Resolvers**

-   Functions that **fetch the data** for queries/mutations.

``` js
const resolvers = {
  Query: {
    getUser: (_, { id }) => db.users.find(user => user.id === id),
  },
};
```

------------------------------------------------------------------------

### 6. **Types**

-   Strongly typed system.\
-   Built-in: `String`, `Int`, `Boolean`, `ID`, `Float`.\
-   Supports custom types.

------------------------------------------------------------------------

## 🔹 Tools in GraphQL Ecosystem

-   **Apollo Server/Client** → Most popular implementation.\
-   **GraphQL Playground / GraphiQL** → Interactive IDEs for queries.\
-   **Relay (Facebook)** → Advanced client library.\
-   **Hasura** → Auto-generates GraphQL API from a database.\
-   **Prisma** → ORM & database toolkit.\
-   **Subscriptions-Transport-WS** → Enables real-time GraphQL
    subscriptions.

------------------------------------------------------------------------

## 🔹 Best Practices for GraphQL

1.  **Design a strong schema** → clear, well-structured types.\
2.  **Use pagination** for large lists → `limit`, `offset`, cursors.\
3.  **Limit query depth/complexity** → prevent abuse with expensive
    queries.\
4.  **Enable caching** → persisted queries, Apollo caching.\
5.  **Secure your API** → Auth, disable introspection in production.\
6.  **Batch resolvers** → Use DataLoader to avoid N+1 queries.\
7.  **Return useful errors** → via `errors` field.\
8.  **Monitor & log queries** → Apollo Studio, tracing tools.

------------------------------------------------------------------------

## 🔹 Limitations of GraphQL

⚠️ More complex to implement server-side.\
⚠️ Caching is harder compared to REST.\
⚠️ Risk of expensive queries if unrestricted.\
⚠️ Overkill for simple CRUD APIs.

------------------------------------------------------------------------

## ✅ Summary

-   **GraphQL** = query language where **clients ask exactly what they
    need**.\
-   Components: **Schema, Queries, Mutations, Subscriptions, Resolvers,
    Types**.\
-   Tools: Apollo, GraphiQL, Relay, Hasura, Prisma.\
-   Best Practices: schema design, pagination, query limits, caching,
    security, error handling.\
-   Use GraphQL when clients need **flexible data** and you want to
    avoid over/under-fetching.


# 🚀 Implementing GraphQL in .NET Core with Hot Chocolate

This guide shows how to build a **GraphQL API** in .NET Core using the
**Hot Chocolate** library by ChilliCream.

------------------------------------------------------------------------

## 🧱 What We'll Build

A simple API with **Authors** and **Books**: - **Queries**: list
authors/books, with filtering, sorting, pagination - **Mutations**: add
a book - In-memory EF Core database (easy to run)

------------------------------------------------------------------------

## 1) Create the Project & Add Packages

``` bash
dotnet new web -n GraphQLDemo
cd GraphQLDemo

# Add GraphQL + EF Core packages
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data
dotnet add package HotChocolate.Data.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

------------------------------------------------------------------------

## 2) Add Models & DbContext

**Models.cs**

``` csharp
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

------------------------------------------------------------------------

## 3) Define GraphQL Types (Query & Mutation)

**GraphQL.cs**

``` csharp
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

------------------------------------------------------------------------

## 4) Wire Everything in Program.cs

**Program.cs**

``` csharp
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

``` bash
dotnet run
```

👉 Open **http://localhost:5000/graphql** → Banana Cake Pop IDE.

------------------------------------------------------------------------

## 5) Example Queries

### List Books with Author

``` graphql
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

### List Authors with Books

``` graphql
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

### Add a Book

``` graphql
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

------------------------------------------------------------------------

## ✅ Why Hot Chocolate?

-   Minimal boilerplate.\
-   Built-in **filtering, sorting, paging**.\
-   Automatic **projection** for EF Core.\
-   Easy to extend with **auth, subscriptions, validation**.

------------------------------------------------------------------------

## 🔧 Next Steps

-   Add **auth** (`[Authorize]`)\
-   Add **validations** with FluentValidation\
-   Add **subscriptions** for real-time updates\
-   Swap **InMemory DB** with SQL Server/Postgres

# 🔍 Is GraphQL Stateless?

## 🔹 Short Answer

Yes ✅ --- **GraphQL is stateless** for queries and mutations, but
**subscriptions are stateful** because they use persistent connections
(WebSockets).

------------------------------------------------------------------------

## 🔹 Why GraphQL is Stateless

-   Built on top of **HTTP** → which is stateless by default.\
-   Each request is **independent**.\
-   Server does not remember past queries or sessions.\
-   Client must send all info (auth, variables, query) with **every
    request**.

👉 Benefits:\
- Scalable (any server can handle requests).\
- Simpler architecture.\
- More secure (no hidden server-side state).

------------------------------------------------------------------------

## 🔹 Where Confusion Comes From

1.  **Variables**\
    Sent with each request → not stored on server.

2.  **Authentication**\
    Done via tokens (e.g., JWT in headers). Must be included on **every
    request**.

3.  **Subscriptions**

    -   Use **WebSockets** (or SSE).\
    -   Connection stays open → server pushes updates.\
    -   This makes **subscriptions stateful**.

------------------------------------------------------------------------

## 🔹 Flowchart: Stateless vs Stateful

``` mermaid
flowchart TD
    A[Client] -->|GraphQL Query/Mutation| B[GraphQL Server]
    B -->|JSON Response| A

    C[Client] -.->|GraphQL Subscription WebSocket| D[GraphQL Server]
    D -.->|Real-time Data Stream| C
```

-   Queries & Mutations → Independent request/response (stateless).\
-   Subscriptions → Persistent connection (stateful).

------------------------------------------------------------------------

## 🔹 Comparison

  ----------------------------------------------------------------------------
  Feature                                   Stateless?           Why
  ----------------------------------------- -------------------- -------------
  **GraphQL Queries**                       ✅ Yes               Each request
                                                                 is
                                                                 independent

  **GraphQL Mutations**                     ✅ Yes               Separate
                                                                 calls per
                                                                 mutation

  **GraphQL Subscriptions**                 ⚠️ No                Requires
                                                                 persistent
                                                                 WebSocket
                                                                 connection
  ----------------------------------------------------------------------------

------------------------------------------------------------------------

## ✅ Summary

-   **GraphQL Queries & Mutations** → Stateless (like REST).\
-   **GraphQL Subscriptions** → Stateful (because WebSockets keep a live
    connection).

👉 So: **GraphQL is mostly stateless, except subscriptions.**

# 🔐 Authentication & Authorization in GraphQL

Unlike REST (multiple endpoints), GraphQL has a **single endpoint
(`/graphql`)**.\
So, **Auth must be handled at the request, resolver, or schema level.**

------------------------------------------------------------------------

## 🔹 1. Authentication in GraphQL

**Authentication** = verifying *who* the user is.

-   GraphQL requests use `POST /graphql`.\
-   Tokens (JWT, OAuth, API keys) are sent in **HTTP headers**.\
-   Middleware verifies the token and attaches user info to `context`.

👉 Example Request

``` http
POST /graphql
Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Body:
{
  "query": "{ me { id name email } }"
}
```

👉 Example (Apollo Server, Node.js)

``` js
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

------------------------------------------------------------------------

## 🔹 2. Authorization in GraphQL

**Authorization** = deciding *what actions/resources* a user can access.

Since GraphQL is one endpoint, authorization is applied at **resolver or
schema level**.

### ✅ Schema Directives

``` graphql
type Query {
  users: [User] @auth(role: "admin")
  me: User @auth
}
```

### ✅ Resolver-Level Checks

``` js
const resolvers = {
  Query: {
    users: (parent, args, context) => {
      if (context.user.role !== "admin") {
        throw new Error("Unauthorized");
      }
      return db.users.findAll();
    }
  }
};
```

### ✅ Middleware in .NET (Hot Chocolate)

``` csharp
public class Query {
    [Authorize(Roles = new[] { "Admin" })]
    public IQueryable<User> GetUsers([Service] AppDbContext db) => db.Users;
}
```

------------------------------------------------------------------------

## 🔹 3. Why Context Matters

-   **Context** is created per request.\
-   Holds `user`, `roles`, and request metadata.\
-   Resolvers check `context` for permissions.

------------------------------------------------------------------------

## 🔹 4. Best Practices

1.  **Authentication** → Verify token in middleware before executing
    GraphQL.\
2.  **Authorization** → Apply at resolver/schema level.\
3.  **Granularity** → Restrict at:
    -   Query/mutation level (e.g., `deleteUser` only for admins).\
    -   Field level (e.g., `email` visible only to the owner).\
4.  **Prevent leaks** → Don't expose unauthorized fields in schema.\
5.  **Disable introspection in production** → Stop attackers from
    exploring schema.

------------------------------------------------------------------------

## ✅ Summary

-   **Authentication** → Headers + middleware (verify token).\
-   **Authorization** → Resolver or schema level (role/field checks).\
-   Use **context** to pass user info.\
-   REST secures by endpoint → GraphQL secures by resolver/field.



