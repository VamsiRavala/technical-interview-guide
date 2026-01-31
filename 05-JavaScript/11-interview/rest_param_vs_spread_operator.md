# ✨ Rest Parameter vs Spread Operator in JavaScript

Both use the `...` syntax, but they serve **different purposes**.

------------------------------------------------------------------------

## 🔹 1. Rest Parameter (`...`)

Used in **function definitions** to collect multiple arguments into an
array.\
👉 Think of it as: "**pack** arguments into a box."

``` js
function sum(...numbers) {
  return numbers.reduce((a, b) => a + b, 0);
}

console.log(sum(1, 2, 3, 4)); // 10
```

### Example with multiple params:

``` js
function introduce(firstName, ...hobbies) {
  console.log(firstName, "likes", hobbies);
}

introduce("Alice", "reading", "coding", "music");
// Alice likes [ 'reading', 'coding', 'music' ]
```

✔️ Only works as the **last parameter** in a function.

------------------------------------------------------------------------

## 🔹 2. Spread Operator (`...`)

Used to **unpack** elements from an array (or object) into individual
items.\
👉 Think of it as: "**unpack** the box."

### In Arrays

``` js
const arr = [1, 2, 3];
const newArr = [...arr, 4, 5];
console.log(newArr); // [1, 2, 3, 4, 5]
```

### In Objects

``` js
const obj = { name: "Alice", age: 25 };
const newObj = { ...obj, city: "Paris" };
console.log(newObj); // { name: 'Alice', age: 25, city: 'Paris' }
```

### Function Calls

``` js
function greet(a, b, c) {
  console.log(a, b, c);
}

const values = ["Hi", "there", "!"];
greet(...values); // Hi there !
```

------------------------------------------------------------------------

## 🔹 3. Key Differences

  -----------------------------------------------------------------------------
  Feature                      Rest Parameter             Spread Operator
  ---------------------------- -------------------------- ---------------------
  **Usage**                    In function **parameters** In arrays, objects,
                                                          or function calls

  **Action**                   **Collects (packs)**       **Expands (unpacks)**
                               multiple values into an    an array/object into
                               array                      individual values

  **Syntax Location**          Function definition        Function call,
                                                          object/array literals

  **Example**                  `function f(...args) {}`   `f(...arr)`
  -----------------------------------------------------------------------------

------------------------------------------------------------------------

## 🔹 4. Real-Life Analogy

-   **Rest**: Packing clothes into a suitcase 🧳 (collecting items).\
-   **Spread**: Taking clothes out of a suitcase and laying them out
    👕👖 (expanding items).

------------------------------------------------------------------------

## ✅ Summary

-   **Rest parameter** → Collects arguments into an array (used in
    function definitions).\
-   **Spread operator** → Expands arrays/objects into individual values
    (used in function calls, arrays, objects).
