# ⚡ Errors and Error Handling in JavaScript

## 🔹 What is an Error?

An **error** occurs when something unexpected happens in your program
that prevents it from running normally.\
JavaScript has different built-in error types, and also allows creating
custom errors.

------------------------------------------------------------------------

## 🔹 Types of Errors

### 1. **Syntax Error**

Occurs when code is written incorrectly and cannot be parsed.

``` js
console.log("Hello";
// SyntaxError: missing ) after argument list
```

### 2. **Reference Error**

Accessing a variable that does not exist.

``` js
console.log(myVar);
// ReferenceError: myVar is not defined
```

### 3. **Type Error**

Using a value in the wrong way.

``` js
let num = 5;
num.toUpperCase();
// TypeError: num.toUpperCase is not a function
```

### 4. **Range Error**

When a value is not in the valid range.

``` js
let arr = new Array(-1);
// RangeError: Invalid array length
```

### 5. **URI Error**

When URI handling functions are misused.

``` js
decodeURIComponent("%");
// URIError: URI malformed
```

### 6. **Eval Error**

Legacy error related to `eval()` (rare today).

------------------------------------------------------------------------

## 🔹 Error Handling

### 1. **try...catch**

``` js
try {
  let result = riskyFunction();
  console.log(result);
} catch (error) {
  console.error("Something went wrong:", error.message);
}
```

### 2. **finally**

``` js
try {
  console.log("Trying...");
  throw new Error("Oops!");
} catch (error) {
  console.log("Caught error:", error.message);
} finally {
  console.log("Always runs!");
}
```

### 3. **Custom Errors**

``` js
function divide(a, b) {
  if (b === 0) {
    throw new Error("Cannot divide by zero");
  }
  return a / b;
}

try {
  divide(10, 0);
} catch (e) {
  console.error(e.message); // Cannot divide by zero
}
```

### 4. **Error Object Properties**

``` js
try {
  throw new TypeError("Wrong type!");
} catch (err) {
  console.log(err.name);    // TypeError
  console.log(err.message); // Wrong type!
  console.log(err.stack);   // Stack trace
}
```

### 5. **Error Handling in Async Code**

#### Using Promises

``` js
fetch("invalid-url")
  .then(res => res.json())
  .catch(err => console.error("Fetch error:", err));
```

#### Using async/await

``` js
async function getData() {
  try {
    let response = await fetch("invalid-url");
    let data = await response.json();
    console.log(data);
  } catch (err) {
    console.error("Async error:", err);
  }
}
getData();
```

------------------------------------------------------------------------

## ✅ Best Practices

1.  Catch errors gracefully.\
2.  Provide meaningful error messages.\
3.  Use custom error classes when needed.\
4.  Handle async errors with `.catch()` or `try...catch`.\
5.  Avoid overusing try...catch unnecessarily.\
6.  Log errors for debugging and monitoring.

------------------------------------------------------------------------

⚡ **Summary**: Errors are inevitable, but handling them properly makes
your app **robust, user-friendly, and secure**.
