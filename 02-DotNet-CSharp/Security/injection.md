# Injection Prevention — SQL / NoSQL / OS / LDAP (ASP.NET Core)

A practical, production-focused guide with copy‑paste C# snippets.

---

## Core principles (apply to all)
- **Treat all input as data, never code.** Don’t concatenate user input into commands/queries.
- **Prefer framework APIs that parameterize by default** (LINQ/EF Core, Mongo Builders, etc.).
- **Whitelist & normalize** inputs where you can (IDs, enums, filenames, booleans).
- **Escape only when you must**, and only with the correct, context‑specific escaper.
- **Log attempts, test negative paths,** and add rate limits to slow brute‑force guessing.

---

## 1) SQL Injection (SQL Server/Postgres/MySQL)
### ✅ Safe patterns
**Entity Framework Core (LINQ auto-parameterizes):**
```csharp
// LINQ => parameterized by EF Core
var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
```

**Raw SQL with parameters (never concat):**
```csharp
// Interpolated: parameters auto-generated
var rows = await db.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE Users SET LastLogin = {DateTime.UtcNow} WHERE Id = {userId}");

// Or Raw + parameters array
await db.Database.ExecuteSqlRawAsync(
    "UPDATE Users SET LastLogin = @p0 WHERE Id = @p1",
    parameters: new object[] { DateTime.UtcNow, userId });
```

**Dapper (parameters by name):**
```csharp
using var conn = new SqlConnection(cs);
var user = await conn.QuerySingleAsync<User>(
    "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
```

**ADO.NET (SqlCommand with parameters):**
```csharp
using var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @email", conn);
cmd.Parameters.AddWithValue("@email", email);
using var rdr = await cmd.ExecuteReaderAsync();
```

### 🚫 Dangerous
```csharp
// ❌ Vulnerable: string concatenation/interpolation
var sql = $"SELECT * FROM Users WHERE Email = '{email}'";
```

### Extra tips
- For `LIKE`, add wildcards to **the parameter**, not the SQL string:
```csharp
var pattern = $"%{term}%";
await db.Products.FromSqlInterpolated($"SELECT * FROM Products WHERE Name LIKE {pattern}").ToListAsync();
```
- Avoid building dynamic ORDER BY / column names from user input. If needed, use a **whitelist**:
```csharp
var orderBy = sort switch {
  "name" => "Name",
  "price" => "Price",
  _ => "CreatedAt"
};
var sql = $"SELECT * FROM Products ORDER BY {orderBy} OFFSET @o ROWS FETCH NEXT @n ROWS ONLY";
```

---

## 2) NoSQL Injection (MongoDB / Elasticsearch / Others)
### MongoDB
**Use typed builders; never accept raw operator JSON (`$ne`, `$where`, …) from clients.**
```csharp
var filter = Builders<User>.Filter.Eq(u => u.Email, email); // parameterized
var user = await users.Find(filter).SingleOrDefaultAsync();
```

**Whitelist fields for filters/sorts:**
```csharp
string field = sort switch { "email" => "Email", "created" => "CreatedAt", _ => "CreatedAt" };
var sortDef = Builders<User>.Sort.Ascending(field);
```

**Disable risky features** (e.g., server-side `$where` JavaScript).

### Elasticsearch
Build queries with **object models/clients** instead of concatenating JSON strings. If you must accept user text, **map it to a single “query_string” clause** you control, not raw JSON.

---

## 3) OS Command Injection
**Never invoke a shell with concatenated strings. Prefer direct executables with `ArgumentList`.**
```csharp
using System.Diagnostics;

var psi = new ProcessStartInfo
{
    FileName = "/usr/bin/grep",        // or full path to your tool
    UseShellExecute = false,
    RedirectStandardOutput = true
};
psi.ArgumentList.Add("-R");
psi.ArgumentList.Add(pattern);         // user input as an argument (no shell parsing)
psi.ArgumentList.Add(directory);       // validate/whitelist directory!
using var p = Process.Start(psi);
string output = await p.StandardOutput.ReadToEndAsync();
```

**Avoid:**
```csharp
// ❌ Vulnerable: sends to shell for parsing
Process.Start(new ProcessStartInfo("bash", $"-lc "grep -R {pattern} {directory}""));
Process.Start("cmd.exe", $"/c dir {userInput}"); // Windows example
```

**Hardening:**
- Use **allowlists** for commands and flags; reject everything else.
- Validate file paths with `Path.GetFullPath` + ensure they reside under an allowed base directory.
- Run commands with least privilege; prefer sandboxed services over shelling out.

---

## 4) LDAP Injection
When creating LDAP **filters** (RFC 4515), escape user values: `\` `*` `(` `)` and NUL must be encoded.

```csharp
static string EscapeLdapFilter(string value) => value
    .Replace("\", "\5c")
    .Replace("*", "\2a")
    .Replace("(", "\28")
    .Replace(")", "\29")
    .Replace("
