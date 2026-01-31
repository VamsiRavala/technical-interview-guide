# 🔄 For...of Loop and For...in vs For...of in JavaScript

## 🔹 What is `for...of`?

The **`for...of` loop** lets you **iterate directly over iterable
objects** like arrays, strings, maps, sets, etc.

👉 It gives you **values**, not indexes (unlike `for` or `for...in`).

### Example with Arrays

``` js
const numbers = [10, 20, 30];

for (let num of numbers) {
  console.log(num);
}
// 10
// 20
// 30
```

### Example with Strings

``` js
const str = "Hi";

for (let ch of str) {
  console.log(ch);
}
// H
// i
```

### Example with Set

``` js
const mySet = new Set([1, 2, 3]);

for (let val of mySet) {
  console.log(val);
}
// 1
// 2
// 3
```

### Example with Map

``` js
const myMap = new Map([
  ["name", "Alice"],
  ["age", 25]
]);

for (let [key, value] of myMap) {
  console.log(key, value);
}
// name Alice
// age 25
```

------------------------------------------------------------------------

## 🔹 `for...in` vs `for...of`

### Difference

-   **`for...in`** → iterates **keys/indexes** (good for objects).\
-   **`for...of`** → iterates **values** (good for arrays, strings,
    iterables).

``` js
const arr = ["a", "b", "c"];

for (let i in arr) {
  console.log(i); // 0, 1, 2 (indexes)
}

for (let val of arr) {
  console.log(val); // a, b, c (values)
}
```

------------------------------------------------------------------------

## 🔹 Arrays (including array of objects)

An **array is iterable**, so `for...of` is the best choice when you want
**values**.

``` js
const users = [
  { name: "Alice", age: 25 },
  { name: "Bob", age: 30 }
];

for (let user of users) {
  console.log(user.name);
}
// Alice
// Bob
```

✔️ `for...of` → clean and readable.

------------------------------------------------------------------------

## 🔹 When to Use `for...in`

`for...in` is designed for **objects**, not arrays.\
It iterates over **keys (property names)**, not values.

Example with an object:

``` js
const user = { name: "Alice", age: 25 };

for (let key in user) {
  console.log(key, user[key]);
}
// name Alice
// age 25
```

⚠️ If you use `for...in` on an array, you'll get **indexes**:

``` js
const arr = ["a", "b", "c"];

for (let i in arr) {
  console.log(i);       // 0, 1, 2
  console.log(arr[i]);  // a, b, c
}
```

But this is **not recommended**, because: - It may include inherited
properties (if someone extends `Array.prototype`). - The order is not
guaranteed (though usually works like indexes).

------------------------------------------------------------------------

## 🔹 Golden Rule

-   ✅ Use **`for...of`** → When you need **values** (arrays, strings,
    maps, sets, array of objects).\
-   ✅ Use **`for...in`** → When you need **keys/properties** of a plain
    object.\
-   ❌ Avoid `for...in` with arrays (use `for...of` instead).

------------------------------------------------------------------------

## 🔹 Real-Life Analogy

-   **`for...in`** = Walking through a **dictionary's index** 📖
    (keys).\
-   **`for...of`** = Reading the **actual words/definitions** 📚
    (values).

------------------------------------------------------------------------

## ✅ Summary

-   **`for...of`** → For values of iterables (arrays, strings, sets,
    maps, array of objects).\
-   **`for...in`** → For keys/properties of plain objects.
