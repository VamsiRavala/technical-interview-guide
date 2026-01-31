# 🔢 Taking the First Two Elements of an Array in JavaScript

There are multiple ways to get the **first two elements** of an array.

------------------------------------------------------------------------

## 🔹 1. Using `slice()`

``` js
const arr = [10, 20, 30, 40, 50];
const firstTwo = arr.slice(0, 2);

console.log(firstTwo); // [10, 20]
```

-   `slice(start, end)` → extracts elements from `start` index up to
    (but not including) `end`.

------------------------------------------------------------------------

## 🔹 2. Using Array Destructuring

``` js
const arr = [10, 20, 30, 40, 50];
const [first, second] = arr;

console.log(first, second); // 10 20
```

If you want them as a **new array**:

``` js
const arr = [10, 20, 30, 40, 50];
const [first, second] = arr;
const firstTwo = [first, second];

console.log(firstTwo); // [10, 20]
```

------------------------------------------------------------------------

## 🔹 3. Using `filter()` (less common)

``` js
const arr = [10, 20, 30, 40, 50];
const firstTwo = arr.filter((_, index) => index < 2);

console.log(firstTwo); // [10, 20]
```

-   Works fine, but not as efficient as `slice()`.

------------------------------------------------------------------------

## ✅ Best Practices

-   Use **`slice(0, 2)`** → when you need a sub-array.\
-   Use **destructuring** → when you just want the first two elements as
    separate variables.
