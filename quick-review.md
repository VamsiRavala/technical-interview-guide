# Quick Review

## React JS
- **Fundamentals**: React is a JavaScript library for building user interfaces.
- **Hooks: useState**: Helps to add state to functional components.
  ```javascript
  const [count, setCount] = useState(0);
  ```
- **Hooks: useEffect**: Allows side effects in functional components.
  ```javascript
  useEffect(() => {
      // Do something
  }, [dependencies]);
  ```
- **Virtual DOM**: A lightweight copy of the real DOM used for efficient updates.
- **Reconciliation**: The process React uses to update the DOM.
- **JSX**: A syntax extension for JavaScript that looks similar to XML.
  ```javascript
  const element = <h1>Hello, world!</h1>;
  ```
- **Props**: Short for properties, used to pass data to components.
- **Components**: Independent, reusable pieces of UI.
- **Error Boundaries**: A React component that catches JavaScript errors.
- **Portals**: Allow rendering children into a DOM node that exists outside the DOM hierarchy.
- **Context API**: A way to share values between components without passing props through every level.

## .NET Web APIs and C# (OOP concepts)
- **Encapsulation**: BankAccount class with private balance.
  ```csharp
  public class BankAccount {
      private decimal balance;
      public void Deposit(decimal amount) { balance += amount; }
      public decimal GetBalance() { return balance; }
  }
  ```
- **Inheritance**: Animal and Dog classes.
  ```csharp
  public class Animal { }
  public class Dog : Animal { }
  ```
- **Polymorphism**: Using virtual MakeSound method.
  ```csharp
  public class Animal { public virtual void MakeSound() { } }
  public class Dog : Animal { public override void MakeSound() { } }
  ```
- **Abstraction**: Abstract Shape class.
  ```csharp
  public abstract class Shape { public abstract double Area(); }
  public class Circle : Shape { public override double Area() { return Math.PI * radius * radius; } }
  ```
- **Interface**: ILogger and FileLogger implementation.
  ```csharp
  public interface ILogger { void Log(string message); }
  public class FileLogger : ILogger { public void Log(string message) { } }
  ```
- **Sealed class**: Cannot be inherited.
- **var/dynamic/object**: Implicit typing.
- **Value vs Reference types**: Understanding their behavior.
- **ThreadPool, async/await**: Managing asynchronous programming.
- **REST API**: Example
  ```csharp
  [HttpPost]
  public IActionResult Create([FromBody] Item item) { }
  ```
- **Entity Framework**: DbContext, Migrations, LINQ Query.
- **Lazy/Eager Loading**: Virtual keyword and Include method.
- **Dependency Injection**: Managing dependencies effectively.

## Azure
- **Functions**: HTTP via Function.
  ```javascript
  public static async Task<IActionResult> Run(HttpRequest req, ILogger log) { }
  ```
- **Durable Functions orchestration**: Managing state across function executions.
- **Service Bus queue**: Messaging between services.
- **Event Hubs, Event Grid**: Event-driven architecture.
- **Cosmos DB**: Creating and querying databases.
- **Storage Blob upload**: Interacting with Azure Blob Storage.
- **Storage Queue**: Reliable message queuing.

## Microservices
- **REST communication**: Building microservices with REST.
- **gRPC**: High-performance RPC framework.
- **API Gateway**: Centralized management.
- **Circuit Breaker**: Using Polly for fault tolerance.
- **CQRS**: Command-query responsibility segregation.
- **Saga**: Managing distributed transactions.
- **Database per Service**: Each service manages its own data.

## JavaScript
- **var/let/const**: Variable declarations.
- **Functions**: Defining reusable code.
- **Arrow functions**: Concise syntax.
  ```javascript
  const add = (a, b) => a + b;
  ```
- **Objects, Arrays, Map, Set**: Data structures in JS.
- **Callbacks, Promises**: Handling asynchronous operations.
- **Async/Await**: Simplified syntax for promises.
- **Closures, Destructuring, Spread operator**: Advanced concepts.

## SQL
- **SELECT, INNER JOIN, LEFT JOIN**: Querying data.
  ```sql
  SELECT * FROM Users INNER JOIN Orders ON Users.Id = Orders.UserId;
  ```
- **INSERT, UPDATE, DELETE**: Modifying data.
  ```sql
  INSERT INTO Users (Name) VALUES ('John');
  ```
- **Indexes, Stored Procedures**: Performance optimization.
- **Transactions, Views**: Data integrity and abstraction.