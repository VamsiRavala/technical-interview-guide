# Sealed Class in C#

A **sealed class** is a class marked with the `sealed` keyword.  
It **cannot be inherited** but can be instantiated and extended with extension methods.

---

## 🔹 Key Properties

### 1. Can a sealed class be instantiated?
✅ **Yes**  
A sealed class can be instantiated like any other class.

```csharp
public sealed class Logger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}

var log = new Logger();
log.Log("Hello");
```

---

### 2. Can a sealed class be inherited?
❌ **No**  
A sealed class **cannot** be inherited. Attempting to do so causes a compile-time error.

```csharp
public class FileLogger : Logger // ❌ Error: Logger is sealed
{
}
```

---

### 3. Can a sealed class have extension methods?
✅ **Yes**  
You can add extension methods to a sealed class.

```csharp
public static class LoggerExtensions
{
    public static void LogWarning(this Logger logger, string message)
    {
        logger.Log("WARNING: " + message);
    }
}

var log = new Logger();
log.LogWarning("Low disk space!");
```

Even though `Logger` is sealed, extension methods **work fine** since they’re just syntactic sugar for static methods.

---

## 🔹 Summary

| Feature                     | Sealed Class |
|------------------------------|--------------|
| Can be instantiated?         | ✅ Yes        |
| Can be inherited?            | ❌ No         |
| Can have extension methods?  | ✅ Yes        |

---

## ✅ When to Use Sealed Classes?
- **Security** → Prevents untrusted classes from inheriting.  
- **Design** → Restricts extensibility when a class should be final (e.g., `System.String`).  
- **Performance** → JIT compiler optimizations are possible when classes are sealed.

---
