# 🚀 Closures in JavaScript: Why and Real-World Examples

## 🔹 What is a Closure?

A **closure** is created when a function "remembers" the variables from
its **outer scope**, even after that outer function has finished
executing.

``` js
function outer() {
  let count = 0;
  return function inner() {
    count++;
    console.log(count);
  };
}

const counter = outer();
counter(); // 1
counter(); // 2
counter(); // 3
```

------------------------------------------------------------------------

## 🔹 Why are Closures Required?

### 1. **Data Privacy / Encapsulation**

Closures let you create **private variables** that cannot be directly
accessed from outside.

``` js
function secret() {
  let key = "mySecret";
  return function() {
    return key;
  };
}
const getKey = secret();
console.log(getKey()); // "mySecret"
```

### 2. **Maintaining State**

They allow functions to "remember" values across multiple invocations.

``` js
function makeCounter() {
  let count = 0;
  return () => ++count;
}
const counter = makeCounter();
console.log(counter()); // 1
console.log(counter()); // 2
```

### 3. **Callbacks & Event Handlers**

When you pass functions around (like in event listeners, setTimeout,
promises), closures help those functions access the context they were
created in.

### 4. **Functional Programming Patterns**

Closures are the foundation for currying, partial application, etc.

------------------------------------------------------------------------

## 🔹 Real-World Use Cases

### 1. **React Hooks (`useState`)**

``` js
function Counter() {
  const [count, setCount] = React.useState(0);

  function increment() {
    setCount(prev => prev + 1);
  }

  return (
    <div>
      <p>{count}</p>
      <button onClick={increment}>+</button>
    </div>
  );
}
```

### 2. **Event Handlers**

``` js
function attachHandler(buttonId) {
  let clicks = 0;

  document.getElementById(buttonId).addEventListener("click", () => {
    clicks++;
    console.log(`Button ${buttonId} clicked ${clicks} times`);
  });
}
```

### 3. **Memoization**

``` js
function memoizedAdd() {
  let cache = {};
  return function(x, y) {
    let key = `${x},${y}`;
    if (cache[key]) return cache[key];
    let result = x + y;
    cache[key] = result;
    return result;
  };
}
```

### 4. **Module Pattern (Private Variables)**

``` js
function BankAccount(initialBalance) {
  let balance = initialBalance;

  return {
    deposit(amount) { balance += amount; },
    withdraw(amount) { balance -= amount; },
    getBalance() { return balance; }
  };
}
```

### 5. **setTimeout Loops**

``` js
for (let i = 1; i <= 3; i++) {
  setTimeout(function() {
    console.log(i);
  }, i * 1000);
}
// Outputs: 1, 2, 3
```

------------------------------------------------------------------------

✅ **Summary:**\
Closures power React hooks, event listeners, memoization, private
variables, and async patterns.\
They're a key feature that makes JavaScript **flexible** and
**powerful**.
