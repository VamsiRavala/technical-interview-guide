# IEnumerable, IQueryable, and List<T> in C#

## 1. IEnumerable<T>
- **Definition**: The most basic collection interface. It represents a sequence of elements that can be enumerated (looped over).
- **Namespace**: `System.Collections.Generic`
- **Execution**: **Deferred execution** (especially with LINQ). It pulls data in-memory one item at a time.
- **Operations**:
  - Supports **forward-only iteration** (can’t go backward, can’t use indexes).
  - Works best with **in-memory collections**.
- **Example**:
  ```csharp
  IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
  foreach (var num in numbers)
  {
      Console.WriteLine(num);
  }
  ```
- **Use Case**: When you just need to **iterate** through a sequence of data, without worrying about querying optimization or database execution.

---

## 2. IQueryable<T>
- **Definition**: Extends `IEnumerable<T>` to support **LINQ-to-SQL**, **LINQ-to-Entities**, or other providers.
- **Namespace**: `System.Linq`
- **Execution**: **Deferred execution**, but the query is translated into a **data source–specific query language** (like SQL).
- **Operations**:
  - Builds an **expression tree** instead of running in-memory.
  - Useful when working with **remote data sources** (e.g., databases, APIs).
- **Example**:
  ```csharp
  IQueryable<Employee> employees = dbContext.Employees
                                            .Where(e => e.Age > 30);
  // This gets converted to SQL: SELECT * FROM Employees WHERE Age > 30
  ```
- **Use Case**: When you need to **query a database or external source** and let the provider optimize execution.

---

## 3. List<T>
- **Definition**: A **concrete class** that implements `IEnumerable<T>`, `ICollection<T>`, and `IList<T>`.
- **Namespace**: `System.Collections.Generic`
- **Execution**: **Immediate execution** (data is already in memory).
- **Operations**:
  - Supports **index-based access** (`list[0]`).
  - Allows adding/removing items dynamically.
  - More **flexible and powerful** than arrays.
- **Example**:
  ```csharp
  List<string> names = new List<string> { "Alice", "Bob" };
  names.Add("Charlie");
  Console.WriteLine(names[1]); // Bob
  ```
- **Use Case**: When you want a **mutable collection** in memory with fast index-based access.

---

## 🔑 Key Differences

| Feature                  | IEnumerable<T> | IQueryable<T> | List<T> |
|---------------------------|----------------|---------------|---------|
| **Type**                  | Interface      | Interface     | Class   |
| **Execution**             | Deferred       | Deferred (remote translation) | Immediate |
| **Data Source**           | In-memory      | Database / Remote | In-memory |
| **Indexing**              | ❌ No           | ❌ No          | ✅ Yes |
| **LINQ Support**          | ✅ In-memory LINQ | ✅ Translates to SQL (etc.) | ✅ In-memory LINQ |
| **Performance**           | Good for small/simple iteration | Optimized for remote queries | Fast in-memory access |

---

# Real-World Example: Filtering Employees

### Scenario
We have a database table `Employees` with thousands of rows.  
We want to get employees whose **Age > 30**, and then select just their **Name**.

---

### 1. Using IEnumerable
```csharp
IEnumerable<Employee> employees = dbContext.Employees.ToList(); 
// .ToList() brings ALL rows into memory!

var result = employees
                .Where(e => e.Age > 30)  // Filtering happens in MEMORY
                .Select(e => e.Name);

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
**Behavior**:
- Fetches all rows into memory first.
- Filtering happens locally in C#.
- **Inefficient** for large datasets.

---

### 2. Using IQueryable
```csharp
IQueryable<Employee> employees = dbContext.Employees; 
// Query is not executed yet

var result = employees
                .Where(e => e.Age > 30)   // Builds expression tree
                .Select(e => e.Name);     // Still not executed

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
**Behavior**:
- Translates to SQL:
  ```sql
  SELECT Name FROM Employees WHERE Age > 30;
  ```
- Fetches only filtered results from the database.
- **Efficient** for large datasets.

---

### 3. Using List<T>
```csharp
List<Employee> employees = dbContext.Employees.ToList(); 
// Everything is in memory immediately

var result = employees
                .Where(e => e.Age > 30)
                .Select(e => e.Name);

foreach (var name in result)
{
    Console.WriteLine(name);
}
```
**Behavior**:
- Like IEnumerable, all rows are loaded first.
- Supports index access (`employees[0]`).
- Still inefficient for large DB queries.

---

# ⚡ Key Takeaway
- `IEnumerable` / `List<T>` → query executes **in memory** after fetching all data.  
- `IQueryable` → query executes at the **database/source** level.  

👉 Always prefer **IQueryable** for database queries, and **List<T>** or **IEnumerable<T>** for in-memory work.
