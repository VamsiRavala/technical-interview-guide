# Asynchronous Programming?
- Asynchronous programming allows a program to perform other operations while waiting for long-running tasks (like I/O, network calls, or database queries) to complete — without blocking the main thread.
- In C#, asynchronous programming is implemented using the async and await keywords, introduced in .NET Framework 4.5.
- This model is called the Task-based Asynchronous Pattern (TAP) — it’s the modern standard for async code in .NET.

# Core Concepts
## 1. Tasks:
  - A Task represents an operation that runs asynchronously and may return a value.
  - Task → represents a void-returning async operation.
  - Task<T> → represents an async operation that returns a value of type T.

## 2. Async and Await
  - async marks a method as asynchronous.
  - await pauses execution until the awaited task completes, without blocking the thread.
  - Example:
  ```
    public async Task<string> GetDataAsync()
    {
      HttpClient client = new HttpClient();
      string result = await client.GetStringAsync("https://api.example.com/data");
      return result;
    }
  ```
  - Here:

  - The method is declared async.

  - await suspends the method until the HTTP request completes.

  - The method returns Task<string> instead of string.

## How It Works Under the Hood
  - When an async method encounters an await, the control is returned to the caller.
  - When the awaited task completes, the method resumes from where it left off.
  - This avoids thread blocking — essential for UI responsiveness and scalability in web apps.

## Example: Parallel Async Operations:
 - You can run multiple asynchronous tasks simultaneously using Task.WhenAll:
```csharp
   public async Task LoadMultipleDataAsync()
    {
        var task1 = GetDataAsync("https://api.example.com/users");
        var task2 = GetDataAsync("https://api.example.com/orders");
    
        var results = await Task.WhenAll(task1, task2);
        Console.WriteLine($"Users: {results[0]}");
        Console.WriteLine($"Orders: {results[1]}");
    }
  ```

   - This way, both tasks run concurrently, improving efficiency.

## Best Practices
- Use async/await all the way down — don’t mix with blocking calls (.Result, .Wait()).
- Avoid async void (except in event handlers).
   (Use Task or Task<T> instead for error handling and chaining.)
- Use ConfigureAwait(false) in library code to avoid deadlocks.
- Parallelize when possible using Task.WhenAll for independent tasks.
- Handle exceptions with try-catch around awaited calls:
  ```csharp
    try
    {
    await SomeOperationAsync();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  ```

  ## Asynchronous Patterns

|	Pattern |	Description	 |	Example |
|-------|-----------|----------| 
|	TAP (Task-based Asynchronous Pattern)	|	Modern approach using Task and async/await	|	✅ Preferred|	
|	APM (Asynchronous Programming Model)	|	Legacy model using Begin/End methods	|	❌ Outdated|	
|	EAP (Event-based Asynchronous Pattern)	|	Uses events (e.g., DownloadFileAsync)	|	⚠️ Legacy Windows Forms/WPF|	

## ConfigureAwait:
- What happens with await
- Starts SomeAsyncMethod() — this runs in the background (maybe it’s downloading data or waiting for I/O).
- Pauses your method and remembers where to continue once it’s done.
- Then, when the async work finishes, the code resumes.
- By default, await tries to resume on the same place it started — the same context (thread or environment).

 - In a UI app (like WPF or WinForms), that’s the UI thread.

- In a web app (like old ASP.NET), that’s the request thread.

- In ASP.NET Core or console apps, it’s usually a thread pool (no special context).
- Your library code (something reusable) does await SomethingAsync().

- The caller (maybe a UI app) calls your code and blocks the thread:

- ## The solution — ConfigureAwait(false)
- When you write:
  ```
  await SomethingAsync().ConfigureAwait(false);
  ```

- you are saying "Don’t go back to the old thread/context.Just continue on any available thread." That means your continuation does not depend on the caller’s environment —
so even if the caller is blocking or has a special thread, you’re safe.
- No deadlocks. No surprises.


## Handle exceptions effectively:

| Concept | Meaning |
| --------| -------------| 
|await	|Automatically rethrows exceptions from async methods on your current thread|
|Task.Exception	| Holds the exception if a task fails|
|TaskCompletionSource	| Lets you manually propagate exceptions between threads
| try/catch	|Works normally with async/await — just wrap your awaits
| async void	| Exceptions can’t be awaited or caught properly (avoid it!) |

```csharp
var tasks = new[] { Task1(), Task2(), Task3() };

try
{
    await Task.WhenAll(tasks);
}
catch
{
    foreach (var t in tasks.Where(t => t.IsFaulted))
        Console.WriteLine(t.Exception?.InnerException.Message);
}
```

```csharp
// Repository Layer
public class UserRepository
{
    public async Task<User> GetUserFromDbAsync(int id)
    {
        // Simulating DB failure
        throw new Exception("Database connection failed");
    }
}

// Service Layer
public class UserService
{
    private readonly UserRepository _repo = new UserRepository();

    public async Task<User> GetUserAsync(int id)
    {
        var user = await _repo.GetUserFromDbAsync(id); // may throw
        return user;
    }
}

// Controller (or top layer)
public class UserController
{
    private readonly UserService _service = new UserService();

    public async Task GetUserControllerAsync()
    {
        try
        {
            var user = await _service.GetUserAsync(1);
            Console.WriteLine($"User: {user.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Handled in controller: {ex.Message}");
        }
    }
}
 ```
  
