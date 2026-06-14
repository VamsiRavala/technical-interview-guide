# .NET & C# — Interview Preparation Guide

> A comprehensive resource for .NET and C# interview preparation covering fundamentals, architecture, security, and best practices.

---

## 📚 Table of Contents

### Modern .NET & C# (2026)

1. **[C# 13 Features](01-csharp-13-features.md)**
2. **[C# 12 Features](02-csharp-12-features.md)**
3. **[.NET 9 Features](03-dotnet-9-features.md)**
4. **[ASP.NET Core 9](04-aspnet-core-9.md)**
5. **[EF Core 9](05-ef-core-9.md)**
6. **[Native AOT](06-native-aot.md)**
7. **[Minimal APIs (Advanced)](07-minimal-apis-advanced.md)**
8. **[Authentication 2026](08-auth-2026.md)**
9. **[Performance Optimization](09-performance-optimization.md)**
10. **[Testing in .NET 9](10-testing-dotnet-9.md)**

### Architecture & Design Patterns

11. **[Clean Architecture](11-clean-architecture.md)**
12. **[SOLID Principles](12-solid-principles.md)**
13. **[Dependency Injection](13-dependency-injection.md)**
14. **[Web API Best Practices](14-web-api-best-practices.md)**
15. **[Repository Pattern](15-repository-pattern.md)** 🆕
16. **[CQRS Pattern](16-cqrs-pattern.md)** 🆕
17. **[Domain-Driven Design](17-domain-driven-design.md)** 🆕

### Language, Runtime & Pipeline (Deep Dives)

18. **[Authentication & Authorization](18-authentication.md)** — Basic, Bearer, JWT (+ refresh), OAuth2/OIDC, auth server
19. **[C# Language Fundamentals](19-csharp-language-fundamentals.md)** — OOP, types, equality, collections/LINQ interfaces, threading basics
20. **[.NET Runtime & Internals](20-runtime-and-internals.md)** — CLR, JIT/tiered compilation, Kestrel, configuration, static files
21. **[ASP.NET Core Request Pipeline](21-aspnet-core-pipeline.md)** — middleware, Map/Run/Use, service lifetimes, ControllerBase vs Controller
22. **[Async, Await & Parallel Programming](22-async-and-parallel.md)** — fundamentals, deep dive, edge cases & pitfalls
23. **[Garbage Collection & Memory](23-garbage-collection-and-memory.md)** — generations, finalization, IDisposable/using, allocations

### Data, Web, Security & Practice

24. **[Caching](24-caching.md)** — in-memory/distributed, Redis, response vs output caching
25. **[REST & Content Negotiation](25-rest-and-content-negotiation.md)** — REST vs RESTful, return types, content negotiation
26. **[Application Security & OWASP Top 10](26-security-owasp.md)** — injection, XSS, CSRF, access control, crypto, misconfig
27. **[Entity Framework Core — Deep Dive](27-entity-framework-deep-dive.md)** — change tracking, loading strategies, concurrency
28. **[Data Structures & Algorithms (C#)](28-data-structures-and-algorithms.md)** — arrays, strings, hashing
29. **[ASP.NET & EF Core — Interview Notes](29-dotnet-interview-notes.md)** — compact quick-reference handbook

> Related: [C# Collections reference](../docs/csharp-collections.md) · [End-to-end .NET + Azure case study](../docs/promotion-management-dotnet-azure.md)

---

## 📚 Planned Content Structure

### 1. C# Fundamentals
Topics to be covered:
- Variables and data types
- Sealed classes
- var, dynamic, and object keywords
- OOP pillars (Encapsulation, Inheritance, Polymorphism, Abstraction)
- Abstract classes vs interfaces
- Equals vs == operator
- Value types vs reference types
- Shallow copy vs deep copy
- ThreadPool and threading

### 2. .NET Architecture
Topics to be covered:
- .NET architecture overview
- JIT compilation
- JIT modes (Tiered compilation)
- Memory management and garbage collection
- Major components of .NET
- Kestrel web server
- appsettings.json configuration
- wwwroot folder and static files

### 3. Authentication & Authorization
Topics to be covered:
- Authentication overview
- Basic authentication
- Bearer token authentication
- JWT (JSON Web Tokens)
- JWT vs Bearer token
- JWT refresh token flow
- OAuth 2.0 and OpenID Connect
- Authentication server setup
- Practical implementation guide

### 4. Entity Framework
Topics to be covered:
- EF Core overview
- Code-first vs database-first
- Migrations and seeding
- Concurrency handling
- Lazy loading, eager loading, explicit loading
- Query optimization
- Change tracking

### 5. Web API
Topics to be covered:
- REST principles
- REST vs RESTful
- API best practices
- Action return types
- Content negotiation
- Model binding
- Versioning strategies
- API documentation

### 6. Caching
Topics to be covered:
- Caching strategies
- In-memory caching
- Distributed caching
- Redis cache
- Response caching vs output caching
- Cache invalidation
- Performance considerations

### 7. Security
Topics to be covered:
- OWASP Top 10
- XSS (Cross-Site Scripting)
- CSRF/XSRF protection
- Injection attacks
- SQL injection prevention
- Broken access control
- Cryptographic failures
- Insecure design
- Security misconfiguration
- Vulnerable and outdated components

### 8. Additional Topics
- Scaffolding and code generation
- Dependency injection
- Middleware pipeline
- Logging and monitoring
- Testing strategies

---

## 🎯 Learning Path

### Beginner
1. C# Fundamentals
2. .NET Architecture Basics
3. Basic Web API Development
4. Simple Authentication

### Intermediate
1. Advanced C# Concepts
2. Entity Framework Deep Dive
3. Authentication & Authorization Patterns
4. Caching Strategies

### Advanced
1. Performance Optimization
2. Security Best Practices
3. Microservices Architecture
4. Cloud Deployment (Azure)

---

## 🔑 Key Concepts to Master

### C# Core
- **Value Types vs Reference Types**: Understanding memory allocation
- **Async/Await**: Asynchronous programming patterns
- **LINQ**: Language Integrated Query
- **Delegates and Events**: Event-driven programming
- **Generics**: Type-safe collections and methods

### ASP.NET Core
- **Middleware Pipeline**: Request processing
- **Dependency Injection**: Built-in IoC container
- **Configuration**: appsettings and environment variables
- **Routing**: Attribute and conventional routing
- **Model Validation**: Data annotations and custom validators

### Entity Framework Core
- **DbContext**: Database context and configuration
- **Relationships**: One-to-one, one-to-many, many-to-many
- **Transactions**: ACID properties
- **Performance**: Query optimization and tracking

---

## 💡 Quick Reference

### Common Interview Topics

| Topic | Key Points |
|-------|-----------|
| **SOLID Principles** | Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion |
| **Async/Await** | Task-based asynchronous pattern, ConfigureAwait, cancellation tokens |
| **Memory Management** | Stack vs Heap, GC generations, finalizers, IDisposable |
| **Authentication** | JWT, OAuth 2.0, OIDC, claims-based identity |
| **API Design** | REST, HTTP methods, status codes, versioning |
| **Performance** | Caching, async operations, connection pooling, query optimization |

---

## 🚀 Getting Started

### Prerequisites
```bash
# Install .NET SDK (latest LTS)
# Download from: https://dotnet.microsoft.com/download

# Verify installation
dotnet --version
```

### Create a New Web API Project
```bash
# Create new Web API
dotnet new webapi -n MyApi

# Navigate to project
cd MyApi

# Run the application
dotnet run

# Create solution
dotnet new sln -n MySolution
dotnet sln add MyApi/MyApi.csproj
```

### Essential Packages
```bash
# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Caching
dotnet add package Microsof