# ⚡ Memoization in JavaScript

## 🔹 What is Memoization?

**Memoization** is an optimization technique where the result of a
function is **cached** based on its input arguments.\
If the same inputs occur again, the cached result is returned instead of
recalculating.

------------------------------------------------------------------------

## 🔹 How Memoization Works

1.  When the function is called:
    -   Check if the result exists in cache for the given arguments.
    -   If yes → return cached result.
    -   If no → compute, store in cache, then return.

``` js
function memoize(fn) {
  const cache = {};

  return function(...args) {
    const key = JSON.stringify(args);
    if (cache[key]) {
      console.log("Fetching from cache:", key);
      return cache[key];
    }
    console.log("Calculating result for:", key);
    const result = fn(...args);
    cache[key] = result;
    return result;
  };
}
```

------------------------------------------------------------------------

## 🔹 Where Memoization is Used

### 1. **Performance Optimization**

For expensive calculations (e.g., Fibonacci, factorial).

``` js
function fibonacci(n, memo = {}) {
  if (n <= 1) return n;
  if (memo[n]) return memo[n];
  return memo[n] = fibonacci(n-1, memo) + fibonacci(n-2, memo);
}
```

------------------------------------------------------------------------

### 2. **React Hooks (`useMemo`, `useCallback`)**

``` js
import React, { useMemo } from "react";

function App({ numbers }) {
  const total = useMemo(() => {
    console.log("Calculating sum...");
    return numbers.reduce((a, b) => a + b, 0);
  }, [numbers]);

  return <div>Total: {total}</div>;
}
```

------------------------------------------------------------------------

### 3. **Data Fetching / API Caching**

``` js
const fetchWithCache = (() => {
  const cache = {};
  return async function(url) {
    if (cache[url]) return cache[url];
    const response = await fetch(url);
    const data = await response.json();
    cache[url] = data;
    return data;
  };
})();
```

------------------------------------------------------------------------

### 4. **Functional Utilities**

Lodash provides `_.memoize`.

``` js
const _ = require("lodash");

const add = (a, b) => {
  console.log("Calculating...");
  return a + b;
};

const memoizedAdd = _.memoize(add);
```

------------------------------------------------------------------------

## 🔹 Benefits

-   ✅ Faster performance by avoiding recomputation\
-   ✅ Useful in recursion and repeated operations\
-   ✅ Optimizes UI rendering (React, Vue)

------------------------------------------------------------------------

## 🔹 Drawbacks

-   ⚠️ Extra memory usage for cache\
-   ⚠️ Cache invalidation is tricky\
-   ⚠️ Works best with **pure functions**

------------------------------------------------------------------------

## ✅ Summary

Memoization = **caching function results**.\
It's widely used for **algorithms, React optimizations, API caching, and
functional utilities**.\
A must-know concept for writing **efficient modern JavaScript**.
