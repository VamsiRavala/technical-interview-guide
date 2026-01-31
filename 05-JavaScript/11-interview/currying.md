# 🌟 Currying in JavaScript: Why It's Important

## 🔹 What is Currying?

Currying transforms a function that takes multiple arguments into a
sequence of functions, each taking **one argument at a time**.

``` js
// Normal function
function add(a, b) {
  return a + b;
}

// Curried version
function curriedAdd(a) {
  return function(b) {
    return a + b;
  };
}

console.log(curriedAdd(2)(3)); // 5
```

------------------------------------------------------------------------

## 🔹 Why Currying is Important

### 1. **Reusability & Function Specialization**

You can create specialized functions by partially applying arguments.

``` js
function multiply(a) {
  return function(b) {
    return a * b;
  };
}

const double = multiply(2);
console.log(double(5)); // 10
```

------------------------------------------------------------------------

### 2. **Improved Readability & Modularity**

Curried functions make pipelines cleaner.

``` js
const add = a => b => a + b;
const increment = add(1);

console.log(increment(10)); // 11
```

------------------------------------------------------------------------

### 3. **Avoids Repetition**

No need to pass the same arguments repeatedly.

``` js
function greet(greeting) {
  return function(name) {
    return `${greeting}, ${name}`;
  };
}

const sayHello = greet("Hello");
console.log(sayHello("Alice")); // Hello, Alice
console.log(sayHello("Bob"));   // Hello, Bob
```

------------------------------------------------------------------------

### 4. **Works Well with Higher-Order Functions**

Currying fits perfectly with `map`, `filter`, and `reduce`.

``` js
const isGreaterThan = a => b => b > a;
const greaterThan10 = isGreaterThan(10);

console.log([5, 15, 20].filter(greaterThan10)); // [15, 20]
```

------------------------------------------------------------------------

### 5. **Encourages Functional Programming**

Currying supports **function composition**.

``` js
const add = a => b => a + b;
const multiply = a => b => a * b;

const add5ThenDouble = x => multiply(2)(add(5)(x));
console.log(add5ThenDouble(10)); // (10+5)*2 = 30
```

------------------------------------------------------------------------

## 🔹 Real-World Use Cases

### 1. **Event Handling**

``` js
const handleEvent = type => msg => console.log(`${type}: ${msg}`);

const errorHandler = handleEvent("Error");
errorHandler("Something went wrong!"); // Error: Something went wrong!
```

### 2. **API Requests**

``` js
const apiRequest = baseUrl => endpoint => params => 
  fetch(`${baseUrl}/${endpoint}?${new URLSearchParams(params)}`);

const githubAPI = apiRequest("https://api.github.com");
const getUsers = githubAPI("users");

getUsers({ since: 1000 });
```

### 3. **React Example**

``` js
const withClass = className => Component => props =>
  <div className={className}><Component {...props} /></div>;

// Usage
const RedButton = withClass("red")(props => <button>{props.label}</button>);
```

------------------------------------------------------------------------

## ✅ Summary

Currying is important because it: - Enables **function
specialization** - Improves **reusability & modularity** - Reduces
**argument repetition** - Integrates with **functional programming** -
Simplifies **real-world tasks** (event handling, API requests, React
patterns)
