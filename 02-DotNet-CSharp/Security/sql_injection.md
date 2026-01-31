# 🛡️ SQL Injection & EF Mitigation (Nutshell)

## ⚠️ SQL Injection
- Attack where user input is executed as **SQL code**.
- Happens when queries are built with **string concatenation**.
- Input is treated as code instead of data.

---

## ✅ How EF Mitigates
- **LINQ → parameterized SQL**  
  - Translates to queries with `@p0`, `@p1` → values kept separate.
- **Safe raw SQL APIs**  
  - `FromSqlInterpolated`, `ExecuteSqlInterpolated` → parameters applied.
  - `FromSqlRaw("... {0}", param)` → safe.
- **Restrictions**  
  - EF Core enforces entity shape, limits multiple statements.

---

## ⚠️ Edge Cases
- ❌ String interpolation/concatenation in `FromSqlRaw` / `ExecuteSqlRaw`.
- ❌ Dynamic **table/column names, ORDER BY** → not parameterized.
- ❌ Stored procs with **dynamic SQL inside**.
- ❌ **Second-order injection** (malicious data reused later).
- ⚠️ `LIKE` patterns: safe from injection, but can cause abuse/perf issues.
- ⚠️ Dynamic LINQ / string APIs (`OrderBy("...")`, `Include("...")`) → validate.

---

## 📝 Best Practices
- Prefer **LINQ** over raw SQL.
- Use **interpolated/raw with parameters** – never concatenation.
- **Whitelist** schema identifiers (columns, tables, sort keys).
- Keep stored procs **parameterized internally**.
- Apply **least-privilege DB accounts**.

---

👉 **In short:**  
EF **protects by default** with parameterization.  
Risk comes back if you build **raw SQL strings or dynamic identifiers** without validation.
