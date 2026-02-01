# .NET OOP Topics

## Polymorphism
### Definition:
Polymorphism is the ability of different classes to be treated as instances of the same class through a common interface. It allows methods to do different things based on the object it is acting upon.
### Importance:
It provides flexibility and the ability to extend systems easily.
### When to Use:
When multiple classes share a common interface, but their implementation details differ.
### Example:
```csharp
public abstract class Animal {
    public abstract void Speak();
}

public class Dog : Animal {
    public override void Speak() {
        Console.WriteLine("Woof!");
    }
}

public class Cat : Animal {
    public override void Speak() {
        Console.WriteLine("Meow!");
    }
}

Animal myDog = new Dog();
myDog.Speak(); // Woof!
```

## Abstraction
### Definition:
Abstraction is the concept of hiding the complex implementation details and showing only the essential features of the object.
### Importance:
It reduces complexity and increases efficiency.
### When to Use:
When you want to expose only relevant functionality to the user.
### Example:
```csharp
public interface IDatabase
{
    void Connect();
    void Disconnect();
}

public class MySqlDatabase : IDatabase
{
    public void Connect() {
        // MySQL connection logic
    }

    public void Disconnect() {
        // MySQL disconnection logic
    }
}
```

## Interfaces
### Definition:
An interface defines a contract that classes must adhere to. It contains methods and properties, but no implementation.
### Importance:
They provide a way to achieve abstraction and multiple inheritance in C#.
### When to Use:
When you want to provide a contract for classes.
### Example:
```csharp
public interface IShape
{
    double Area();
}

public class Circle : IShape
{
    public double Radius { get; set; }
    public double Area() => Math.PI * Radius * Radius;
}
```

## Sealed Classes
### Definition:
A sealed class is one that cannot be inherited.
### Importance:
It can improve performance and prevent class inheritance which can be crucial in certain scenarios.
### When to Use:
When you want to restrict the class from being further derived.
### Example:
```csharp
public sealed class ConfigurationManager
{
    // Class members
}
```

# .NET Concepts

## Var/Dynamic/Object
### Definition:
- `var`: Implicitly typed local variable declaration.
- `dynamic`: Bypasses compile-time type checking.
- `object`: The base class for all data types.
### Importance:
Understanding these types helps in effective variable declaration and manipulation.
### When to Use:
- Use `var` when the type is clear from the right side.
- Use `dynamic` for late-binding scenarios.
- Use `object` for generically typed functions.
### Example:
```csharp
var number = 10;  // var
dynamic anything = "Text";  // dynamic
object obj = new object();  // object
```

## Value vs Reference Types
### Definition:
- Value types store the actual data.
- Reference types store a reference to the data.
### Importance:
It determines how data is stored, maintained, and passed in memory.
### When to Use:
- Use value types for small, simple data.
- Use reference types for larger complex data structures.
### Example:
```csharp
int a = 5;  // Value type
int[] arr = new int[] { 1, 2, 3 };  // Reference type
```

## ThreadPool
### Definition:
ThreadPool provides a pool of worker threads that can be used to execute tasks asynchronously.
### Importance:
It helps in better performance and resource management.
### When to Use:
Use when there is a need for concurrent processing.
### Example:
```csharp
ThreadPool.QueueUserWorkItem(state => {
    // Task execution code here
});
```

## Async/Await
### Definition:
Async/await is a pattern to write asynchronous code more easily.
### Importance:
It improves responsiveness of applications by preventing blocking calls.
### When to Use:
Use it for I/O-bound operations.
### Example:
```csharp
public async Task<string> FetchDataAsync()
{
    using (var client = new HttpClient())
    {
        return await client.GetStringAsync("http://example.com");
    }
}
```

# REST APIs

## Definition:
Representational State Transfer (REST) APIs allow interaction with web services over HTTP.
## Importance:
They are stateless and scalable, enabling integration between different services.
## When to Use:
When designing web services for CRUD operations.
## Example:
```csharp
[HttpGet]
public IActionResult GetItem(int id)
{
    var item = _repository.GetById(id);
    return Ok(item);
}
```

# Entity Framework

## DbContext
### Definition:
DbContext is the primary class responsible for interacting with a database in Entity Framework.
### Importance:
It manages database connections and handles CRUD operations.
### When to Use:
Use it as a bridge between the application and the database.
### Example:
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
}
```

## Migrations
### Definition:
Migrations allow you to keep your database schema in sync with the model classes.
### Importance:
It automates database schema updates.
### When to Use:
Use it for version control of your database models.
### Example:
```csharp
Add-Migration InitialCreate
Update-Database
```

## LINQ
### Definition:
LINQ (Language Integrated Query) provides a syntax for querying data in C#.
### Importance:
It allows for querying collections in a familiar and easy-to-read way.
### When to Use:
Use it for filtering and querying data.
### Example:
```csharp
var results = from item in items where item.Price > 10 select item;
```

## Lazy/Eager Loading
### Definition:
- Lazy Loading fetches related data on demand.
- Eager Loading fetches all related data up front.
### Importance:
Optimizes performance depending on the use case.
### When to Use:
Use lazy loading for high latency scenarios; use eager loading for complex data displays.
### Example:
```csharp
context.Blogs.Include(b => b.Posts); // Eager Loading
```

# Dependency Injection

## Definition:
A pattern that allows a class to receive its dependencies from external sources rather than creating them itself.
## Importance:
Improves code maintainability and testability.
## When to Use:
Use when classes need to be loosely coupled.
## Example:
```csharp
public class OrderService
{
    private readonly IEmailService _emailService;

    public OrderService(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

# Azure Services

## Functions
### Definition:
Serverless compute service that enables you to run event-driven code without having to manage infrastructure.
### Importance:
Cost-effective and efficient scaling on demand.
### When to Use:
When you need to run background tasks in response to events.
### Example:
```csharp
[FunctionName("MyFunction")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
    ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    return new OkResult();
}
```

## Durable Functions
### Definition:
An extension of Azure Functions that lets you write stateful functions in a serverless environment.
### Importance:
Allows for orchestrating multiple function calls in a durable way.
### When to Use:
When workflows need to maintain state.
### Example:
```csharp
[FunctionName("OrchestratorFunction")]
public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
{
    // orchestration logic
}
```

## Service Bus
### Definition:
A messaging service for connecting decoupled applications.
### Importance:
Ensures reliable communication between services.
### When to Use:
Use for asynchronous messaging between microservices.
### Example:
```csharp
var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);
```

## Event Hubs & Event Grid
### Definition:
Event Hubs allows high-throughput data streaming; Event Grid manages events and routing.
### Importance:
Enable real-time data processing and event-driven architecture.
### When to Use:
When dealing with large streams of data or events in varying workloads.
### Example:
```csharp
var producer = eventHubProducerClient.CreateProducer();
await producer.SendAsync(new EventData(Encoding.UTF8.GetBytes("Demo Event")));
```

## Cosmos DB
### Definition:
Globally distributed, multi-model database service for any scale.
### Importance:
Provides reliability, scalability, and a diverse set of APIs.
### When to Use:
When building globally distributed applications.
### Example:
```csharp
var client = new CosmosClient(endpointUrl, authorizationKey);
var container = client.GetContainer(databaseId, containerId);
```

## Storage
### Definition:
Azure Storage provides scalable cloud storage for data.
### Importance:
Handles unstructured data with ease and reliability.
### When to Use:
Use for storing files and unstructured data in the cloud.
### Example:
```csharp
CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
var blobClient = storageAccount.CreateCloudBlobClient();
```

# Microservices

## Communication Patterns
### Definition:
Various ways microservices communicate: synchronous, asynchronous.
### Importance:
Choosing the right pattern affects system performance and reliability.
### When to Use:
Use synchronicity for immediate responses; use asynchronicity for decoupling.
### Example:
- Synchronous: REST API calls.
- Asynchronous: Message queues.

## gRPC
### Definition:
A high-performance RPC framework that uses HTTP/2.
### Importance:
Offers fast communication and efficient protocol usage.
### When to Use:
When low-latency communication is crucial.
### Example:
```csharp
var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
var client = new Greeter.GreeterClient(channel);
```

## API Gateway
### Definition:
A single entry point for managing client requests to multiple microservices.
### Importance:
Centralizes routing, authentication, and monitoring.
### When to Use:
Use when you need to aggregate multiple services. 
### Example:
```csharp
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
```

## Circuit Breaker
### Definition:
A pattern that prevents application from trying to execute an action that is likely to fail.
### Importance:
It helps maintain the stability of a system during failures.
### When to Use:
Use during API calls and external service dependencies.
### Example:
```csharp
var breaker = new CircuitBreaker(3, TimeSpan.FromSeconds(5));
if (await breaker.ExecuteAsync(() => CallExternalService()) == default)
{
    // Handle alternative action
}
```

## CQRS
### Definition:
Command Query Responsibility Segregation separates reading and writing operations.
### Importance:
Enables scaling and optimization of reads and writes independently.
### When to Use:
When system demands scale and performance.
### Example:
```csharp
public interface ICommand {}    // Command interface
public interface IQuery<T> {}  // Query interface
```

## Saga
### Definition:
A design pattern managing distributed transactions across microservices.
### Importance:
It helps maintain data consistency in complex systems.
### When to Use:
When actions span multiple services and require coordination.
### Example:
```csharp
public async Task HandleOrder(order) {
    await processPayment(order);
    // Further actions
}
```

# JavaScript Topics

## Variables
### Definition:
Containers for storing data values.
### Importance:
Understanding scope and data type is crucial.
### When to Use:
When declaring values that will be used later.
### Example:
```javascript
let number = 5;
const name = "John";
```

## Functions
### Definition:
A reusable block of code designed to perform a particular task.
### Importance:
They help in code reusability and organization.
### When to Use:
Any time you want to encapsulate a piece of logic.
### Example:
```javascript
function greet(name) {
    return `Hello, ${name}!`;
}
```

## Objects
### Definition:
Collections of related data and functionality.
### Importance:
They represent entities with properties and methods.
### When to Use:
When modeling complex data structures.
### Example:
```javascript
let person = {
    firstName: "John",
    lastName: "Doe",
    fullName: function() { return `${this.firstName} ${this.lastName}`; }
};
```

## Arrays
### Definition:
A list-like object used to store multiple values.
### Importance:
Facilitates collection and manipulation of multiple items.
### When to Use:
When needing to store a list of items.
### Example:
```javascript
let fruits = ["Apple", "Banana", "Cherry"];
```

## Callbacks
### Definition:
Functions passed as arguments to other functions.
### Importance:
They facilitate asynchronous programming.
### When to Use:
When waiting for a response from a function.
### Example:
```javascript
function fetchData(callback) {
    setTimeout(() => {
        callback('Data');
    }, 1000);
}
```

## Promises
### Definition:
An object representing the eventual completion or failure of an asynchronous operation.
### Importance:
They add robustness to asynchronous code.
### When to Use:
When working with operations that take time and can fail.
### Example:
```javascript
let myPromise = new Promise((resolve, reject) => {
    let success = true;
    if(success) {
        resolve('Success');
    } else {
        reject('Failed');
    }
});
```

## Async/Await
### Definition:
A syntax for working with promises more easily.
### Importance:
It makes asynchronous code easier to read and maintain.
### When to Use:
When dealing with asynchronous operations.
### Example:
```javascript
async function fetchData() {
    let data = await myPromise;
    console.log(data);
}
```

## Closures
### Definition:
A function that remembers its lexical scope even when the function is executed outside that scope.
### Importance:
They allow for data encapsulation and privacy.
### When to Use:
When you need to maintain state in functions.
### Example:
```javascript
function outer() {
    let outerVariable = 'I am outside!';
    return function inner() {
        console.log(outerVariable);
    };
}
```

## Destructuring
### Definition:
A syntax that unpacks values from arrays or properties from objects into distinct variables.
### Importance:
Allows for cleaner and more concise code.
### When to Use:
When extracting multiple properties at once.
### Example:
```javascript
const array = [1, 2, 3];
const [a, b] = array;
```

# SQL Topics

## SELECT
### Definition:
SQL command to select data from a database.
### Importance:
It retrieves data based on specified criteria.
### When to Use:
When querying a database.
### Example:
```sql
SELECT * FROM Users WHERE Age > 18;
```

## JOINs
### Definition:
Combining rows from two or more tables based on a related column.
### Importance:
Enables data retrieval from multiple tables in a single query.
### When to Use:
When working with related data across tables.
### Example:
```sql
SELECT Users.Name, Orders.OrderID
FROM Users
JOIN Orders ON Users.UserID = Orders.UserID;
```

## INSERT
### Definition:
SQL command for inserting new records into a table.
### Importance:
Allows for adding new data to the database.
### When to Use:
When adding new records.
### Example:
```sql
INSERT INTO Users (Name, Age) VALUES ('John Doe', 30);
```

## UPDATE
### Definition:
SQL command to update existing records in a table.
### Importance:
It modifies data already present in the database.
### When to Use:
When you need to change existing records.
### Example:
```sql
UPDATE Users SET Age = 31 WHERE Name = 'John Doe';
```

## DELETE
### Definition:
SQL command to delete records from a table.
### Importance:
It removes data from the database.
### When to Use:
When you need to remove data.
### Example:
```sql
DELETE FROM Users WHERE Name = 'John Doe';
```

## Indexes
### Definition:
A database object that improves the speed of data retrieval.
### Importance:
Enhances performance by minimizing the amount of data the database engine needs to scan.
### When to Use:
When querying large data sets.
### Example:
```sql
CREATE INDEX idx_name ON Users(Name);
```

## Stored Procedures
### Definition:
A prepared SQL code that can be saved and reused.
### Importance:
Increases security and helps with code maintenance.
### When to Use:
When you need consistent reuse of complex queries.
### Example:
```sql
CREATE PROCEDURE GetUserByName @Name NVARCHAR(50)
AS
BEGIN
    SELECT * FROM Users WHERE Name = @Name;
END;
```

## Transactions
### Definition:
A sequence of database operations performed as a single logical unit of work.
### Importance:
Ensures data integrity by ensuring that either all operations complete or none at all.
### When to Use:
When you need to perform multiple data operations atomically.
### Example:
```sql
BEGIN TRANSACTION;
UPDATE Users SET Age = 30;
DELETE FROM Orders WHERE OrderID = 1;
COMMIT;
```

## Views
### Definition:
A virtual table based on the result set of an SQL statement.
### Importance:
Provides a way to simplify data access.
### When to Use:
When you want to aggregate data and make it simpler to access.
### Example:
```sql
CREATE VIEW UserOrders AS
SELECT Users.Name, Orders.OrderID
FROM Users
JOIN Orders ON Users.UserID = Orders.UserID;
```

