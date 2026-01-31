Temporal Dead Zone is a behaviour that occurs with variables declared using let and const keywords. It is a behaviour where we try to access a variable before it is initialized. Examples of temporal dead zone:

# ⏳ Temporal Dead Zone (TDZ) in JavaScript

## 🔹 What is TDZ?

The **Temporal Dead Zone (TDZ)** is the period between when a variable
is **hoisted** and when it is **initialized**, during which accessing
the variable will throw a **ReferenceError**.

👉 A variable exists in memory but **cannot be accessed** until its
declaration line is executed.

------------------------------------------------------------------------

## 🔹 Example with `let`

``` js
console.log(a); // ❌ ReferenceError (TDZ)
let a = 10;
console.log(a); // ✅ 10
```

-   `let a` is hoisted.\
-   But it's in the **TDZ** until the declaration line is reached.

------------------------------------------------------------------------

## 🔹 Example with `const`

``` js
console.log(b); // ❌ ReferenceError (TDZ)
const b = 20;
console.log(b); // ✅ 20
```

-   `const` behaves like `let`, but it must also be **initialized at
    declaration**.

------------------------------------------------------------------------

## 🔹 Why Not with `var`?

``` js
console.log(c); // ✅ undefined (no TDZ)
var c = 30;
console.log(c); // ✅ 30
```

-   `var` is hoisted and **initialized with `undefined`**.\
-   No TDZ exists for `var`.

------------------------------------------------------------------------

## 🔹 TDZ with Functions

``` js
{
  console.log(typeof func); // ✅ "function"
  console.log(typeof x);    // ❌ ReferenceError (TDZ)

  function func() {}
  let x = 5;
}
```

-   Function declarations are fully hoisted.\
-   `let`/`const` variables remain in TDZ until initialized.

------------------------------------------------------------------------

## 🔹 Real-Life Analogy

Imagine a **reserved seat in a theater** 🎭:\
- The seat (variable) exists from the start (hoisting).\
- But you **cannot sit in it** (use it) until the ticket holder arrives
(declaration line).\
- Trying early → ❌ error.

------------------------------------------------------------------------

## ✅ Summary

-   **TDZ = Time between hoisting and initialization.**\
-   Affects **`let`** and **`const`**, not `var`.\
-   Accessing a variable in TDZ → **ReferenceError**.\
-   Prevents using variables before proper initialization → safer, more
    predictable code.
