# 📌 C# Variables Cheat Sheet

| Concept              | Syntax / Example | Notes |
|----------------------|------------------|-------|
| **Declaration**      | `int age;` | Declares a variable without assigning a value. |
| **Initialization**   | `int age = 25;` | Declares and assigns value. |
| **Assignment**       | `age = 30;` | Updates value of variable. |
| **Naming Rules**     | `string userName;` | Start with letter/_ ; case-sensitive; no spaces/symbols. |
| **Value Types**      | `int x = 10;` <br> `double pi = 3.14;` <br> `char grade = 'A';` <br> `bool isReady = true;` | Stored directly in memory. |
| **Reference Types**  | `string name = "Tom";` <br> `object obj = 5;` | Store reference (address). |
| **var**              | `var msg = "Hello";` | Type inferred at compile time. |
| **dynamic**          | `dynamic val = 10;` | Type checked at runtime, flexible but less safe. |
| **object**           | `object num = 100;` | Base type for all data types. Requires casting. |
| **const**            | `const double Pi = 3.14159;` | Compile-time constant (must assign at declaration). |
| **readonly**         | `readonly int year;` <br> `year = 2025; // in constructor` | Assign once at declaration or in constructor. |
| **Scope: Local**     | Inside method: <br> `void Test(){ int x=10; }` | Exists only inside method/block. |
| **Scope: Field**     | `private int count;` | Declared at class level, accessible in class. |
| **Scope: Static**    | `static int total;` | Shared across all objects of a class. |

---

✅ **Tips**  
- Prefer `var` for readability when the type is obvious.  
- Use `const` for fixed values, `readonly` for runtime-assigned constants.  
- Avoid `dynamic` unless necessary.  
- Always use meaningful variable names.
