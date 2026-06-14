# Closures, Scope, and the `this` Keyword

## Closures

A **closure** is created when a function "remembers" the variables from its **outer scope**, even after that outer function has finished executing.

```javascript
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

### Why Are Closures Required?

#### 1. Data Privacy / Encapsulation

Closures let you create **private variables** that cannot be directly accessed from outside.

```javascript
function secret() {
  let key = "mySecret";
  return function() {
    return key;
  };
}
const getKey = secret();
console.log(getKey()); // "mySecret"
```

#### 2. Maintaining State

They allow functions to "remember" values across multiple invocations.

```javascript
function makeCounter() {
  let count = 0;
  return () => ++count;
}
const counter = makeCounter();
console.log(counter()); // 1
console.log(counter()); // 2
```

#### 3. Callbacks & Event Handlers

When you pass functions around (like in event listeners, `setTimeout`, promises), closures help those functions access the context they were created in.

#### 4. Functional Programming Patterns

Closures are the foundation for currying, partial application, etc.

### Real-World Use Cases

#### 1. React Hooks (`useState`)

```javascript
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

#### 2. Event Handlers

```javascript
function attachHandler(buttonId) {
  let clicks = 0;

  document.getElementById(buttonId).addEventListener("click", () => {
    clicks++;
    console.log(`Button ${buttonId} clicked ${clicks} times`);
  });
}
```

#### 3. Memoization

```javascript
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

#### 4. Module Pattern (Private Variables)

```javascript
function BankAccount(initialBalance) {
  let balance = initialBalance;

  return {
    deposit(amount) { balance += amount; },
    withdraw(amount) { balance -= amount; },
    getBalance() { return balance; }
  };
}
```

#### 5. setTimeout Loops

```javascript
for (let i = 1; i <= 3; i++) {
  setTimeout(function() {
    console.log(i);
  }, i * 1000);
}
// Outputs: 1, 2, 3
```

### Summary

Closures power React hooks, event listeners, memoization, private variables, and async patterns. They are a key feature that makes JavaScript **flexible** and **powerful**.

---

## Temporal Dead Zone

The **Temporal Dead Zone (TDZ)** is a behaviour that occurs with variables declared using `let` and `const`. It is the period between when a variable is **hoisted** and when it is **initialized**, during which accessing the variable will throw a **ReferenceError**.

A variable exists in memory but **cannot be accessed** until its declaration line is executed.

### Example with `let`

```javascript
console.log(a); // ReferenceError (TDZ)
let a = 10;
console.log(a); // 10
```

- `let a` is hoisted.
- But it is in the **TDZ** until the declaration line is reached.

### Example with `const`

```javascript
console.log(b); // ReferenceError (TDZ)
const b = 20;
console.log(b); // 20
```

- `const` behaves like `let`, but it must also be **initialized at declaration**.

### Why Not with `var`?

```javascript
console.log(c); // undefined (no TDZ)
var c = 30;
console.log(c); // 30
```

- `var` is hoisted and **initialized with `undefined`**.
- No TDZ exists for `var`.

### TDZ with Functions

```javascript
{
  console.log(typeof func); // "function"
  console.log(typeof x);    // ReferenceError (TDZ)

  function func() {}
  let x = 5;
}
```

- Function declarations are fully hoisted.
- `let`/`const` variables remain in TDZ until initialized.

### Real-Life Analogy

Imagine a **reserved seat in a theater**:
- The seat (variable) exists from the start (hoisting).
- But you **cannot sit in it** (use it) until the ticket holder arrives (declaration line).
- Trying early results in an error.

### Summary

- **TDZ = Time between hoisting and initialization.**
- Affects **`let`** and **`const`**, not `var`.
- Accessing a variable in TDZ throws a **ReferenceError**.
- Prevents using variables before proper initialization, leading to safer, more predictable code.

---

## The `this` Keyword

The `this` keyword in JavaScript is a special identifier that refers to the context in which a function is executed. Its value is determined by **how a function is called**, not where it's defined.

### `this` in Different Contexts

#### 1. Global Context

In the global execution context (outside any function), `this` refers to the global object.

```javascript
// In browsers
console.log(this); // Window object

// In Node.js
console.log(this); // global object (or {} in modules)
```

#### 2. Function Context (Regular Functions)

In regular functions, `this` depends on how the function is called.

Simple function call:

```javascript
function showThis() {
  console.log(this);
}

showThis(); // Window (in non-strict mode) or undefined (in strict mode)
```

Strict mode:

```javascript
'use strict';
function showThis() {
  console.log(this);
}

showThis(); // undefined
```

#### 3. Object Method Context

When a function is called as a method of an object, `this` refers to the object.

```javascript
const person = {
  name: 'Alice',
  greet: function() {
    console.log(`Hello, I'm ${this.name}`);
  }
};

person.greet(); // "Hello, I'm Alice"
```

Important: If you extract the method, `this` is lost:

```javascript
const greetFunc = person.greet;
greetFunc(); // "Hello, I'm undefined" (or error in strict mode)
```

#### 4. Constructor Functions

When using `new` with a function, `this` refers to the newly created object.

```javascript
function Person(name) {
  this.name = name;
  this.greet = function() {
    console.log(`Hello, I'm ${this.name}`);
  };
}

const alice = new Person('Alice');
alice.greet(); // "Hello, I'm Alice"
```

#### 5. Arrow Functions

Arrow functions **do not have their own `this`**. They inherit `this` from the enclosing lexical context.

```javascript
const person = {
  name: 'Bob',
  regularFunc: function() {
    console.log(this.name); // 'Bob'
  },
  arrowFunc: () => {
    console.log(this.name); // undefined (inherits from global scope)
  }
};

person.regularFunc(); // 'Bob'
person.arrowFunc();   // undefined
```

Arrow functions in callbacks:

```javascript
const person = {
  name: 'Charlie',
  hobbies: ['reading', 'coding'],
  showHobbies: function() {
    this.hobbies.forEach(hobby => {
      // Arrow function inherits 'this' from showHobbies
      console.log(`${this.name} likes ${hobby}`);
    });
  }
};

person.showHobbies();
// "Charlie likes reading"
// "Charlie likes coding"
```

Compare with a regular function:

```javascript
const person = {
  name: 'Charlie',
  hobbies: ['reading', 'coding'],
  showHobbies: function() {
    this.hobbies.forEach(function(hobby) {
      // Regular function has its own 'this' (undefined or Window)
      console.log(`${this.name} likes ${hobby}`);
    });
  }
};

person.showHobbies();
// "undefined likes reading"
// "undefined likes coding"
```

#### 6. Event Handlers

In DOM event handlers, `this` refers to the element that received the event.

```javascript
const button = document.querySelector('button');

button.addEventListener('click', function() {
  console.log(this); // The button element
  this.textContent = 'Clicked!';
});

// With arrow function
button.addEventListener('click', () => {
  console.log(this); // Window (inherits from outer scope)
});
```

### Explicitly Setting `this`: call, apply, bind

JavaScript provides three methods to explicitly set the value of `this`.

#### 1. `call()`

Calls a function with a given `this` value and arguments provided individually.

```javascript
function greet(greeting, punctuation) {
  console.log(`${greeting}, I'm ${this.name}${punctuation}`);
}

const person = { name: 'David' };

greet.call(person, 'Hello', '!'); // "Hello, I'm David!"
```

#### 2. `apply()`

Similar to `call()`, but arguments are provided as an array.

```javascript
function greet(greeting, punctuation) {
  console.log(`${greeting}, I'm ${this.name}${punctuation}`);
}

const person = { name: 'Emma' };

greet.apply(person, ['Hi', '.']); // "Hi, I'm Emma."
```

#### 3. `bind()`

Returns a new function with a permanently bound `this` value.

```javascript
function greet() {
  console.log(`Hello, I'm ${this.name}`);
}

const person = { name: 'Frank' };
const boundGreet = greet.bind(person);

boundGreet(); // "Hello, I'm Frank"

// Even if called as a method of another object
const anotherObj = { name: 'Grace', greet: boundGreet };
anotherObj.greet(); // Still "Hello, I'm Frank"
```

Practical example: fixing lost context.

```javascript
const person = {
  name: 'Helen',
  greet: function() {
    console.log(`Hello, I'm ${this.name}`);
  }
};

// Problem: Lost context
setTimeout(person.greet, 1000); // "Hello, I'm undefined"

// Solution 1: bind
setTimeout(person.greet.bind(person), 1000); // "Hello, I'm Helen"

// Solution 2: Arrow function
setTimeout(() => person.greet(), 1000); // "Hello, I'm Helen"
```

### Common Pitfalls

#### 1. Lost Context in Callbacks

```javascript
class Counter {
  constructor() {
    this.count = 0;
  }

  increment() {
    this.count++;
  }

  start() {
    // Wrong: 'this' is lost
    setInterval(this.increment, 1000);

    // Fix 1: bind
    setInterval(this.increment.bind(this), 1000);

    // Fix 2: arrow function
    setInterval(() => this.increment(), 1000);
  }
}
```

#### 2. Method Extraction

```javascript
const obj = {
  value: 42,
  getValue: function() {
    return this.value;
  }
};

const getValue = obj.getValue;
console.log(getValue()); // undefined (lost context)

// Fix
const getValue = obj.getValue.bind(obj);
console.log(getValue()); // 42
```

#### 3. Nested Functions

```javascript
const obj = {
  value: 42,
  method: function() {
    function nested() {
      console.log(this.value); // undefined (this is Window or undefined)
    }
    nested();
  }
};

obj.method();

// Fix 1: Save 'this' reference
const obj = {
  value: 42,
  method: function() {
    const self = this; // Save reference
    function nested() {
      console.log(self.value); // 42
    }
    nested();
  }
};

// Fix 2: Use arrow function
const obj = {
  value: 42,
  method: function() {
    const nested = () => {
      console.log(this.value); // 42 (inherits 'this')
    };
    nested();
  }
};
```

### Interview Questions

#### Q1: What will be logged?

```javascript
const obj = {
  name: 'John',
  greet: function() {
    console.log(this.name);
  }
};

const greet = obj.greet;
greet();
```

**Answer:** `undefined` (in non-strict mode) or error (in strict mode). The context is lost when the method is extracted.

#### Q2: What about this code?

```javascript
const obj = {
  name: 'Jane',
  greet: () => {
    console.log(this.name);
  }
};

obj.greet();
```

**Answer:** `undefined`. Arrow functions don't have their own `this`; they inherit from the lexical scope (global scope in this case).

#### Q3: Fix the following code

```javascript
class Timer {
  constructor() {
    this.seconds = 0;
  }

  start() {
    setInterval(function() {
      this.seconds++;
      console.log(this.seconds);
    }, 1000);
  }
}

const timer = new Timer();
timer.start(); // NaN, NaN, NaN...
```

**Answers:**

```javascript
// Solution 1: Arrow function
start() {
  setInterval(() => {
    this.seconds++;
    console.log(this.seconds);
  }, 1000);
}

// Solution 2: bind
start() {
  setInterval(function() {
    this.seconds++;
    console.log(this.seconds);
  }.bind(this), 1000);
}

// Solution 3: Save 'this' reference
start() {
  const self = this;
  setInterval(function() {
    self.seconds++;
    console.log(self.seconds);
  }, 1000);
}
```

### `this` Context Summary

| Context | `this` Refers To |
|---------|------------------|
| Global | Window (browser) or global (Node.js) |
| Regular function (simple call) | Window (non-strict) or undefined (strict) |
| Object method | The object |
| Constructor (with `new`) | The new instance |
| Arrow function | Inherited from lexical scope |
| Event handler | The element that received the event |
| `call` / `apply` / `bind` | Explicitly set value |

### Key Takeaways

1. **`this` is determined by how a function is called**, not where it's defined.
2. **Arrow functions inherit `this`** from their surrounding scope.
3. **Use `bind()`, `call()`, or `apply()`** to explicitly set `this`.
4. **Common pitfall:** Lost context when extracting methods or using callbacks.
5. **Best practice:** Use arrow functions in callbacks to preserve context.

### Real-World Examples

React class components:

```javascript
class MyComponent extends React.Component {
  constructor(props) {
    super(props);
    this.state = { count: 0 };

    // Need to bind 'this' for event handlers
    this.handleClick = this.handleClick.bind(this);
  }

  handleClick() {
    this.setState({ count: this.state.count + 1 });
  }

  render() {
    return <button onClick={this.handleClick}>Click</button>;
  }
}

// Or use arrow function (class field)
class MyComponent extends React.Component {
  state = { count: 0 };

  handleClick = () => {
    this.setState({ count: this.state.count + 1 });
  }

  render() {
    return <button onClick={this.handleClick}>Click</button>;
  }
}
```

API calls:

```javascript
class UserService {
  constructor(apiUrl) {
    this.apiUrl = apiUrl;
  }

  async getUser(id) {
    const response = await fetch(`${this.apiUrl}/users/${id}`);
    return response.json();
  }

  async getAllUsers() {
    // Using arrow function to preserve 'this'
    const promises = [1, 2, 3].map(id => this.getUser(id));
    return Promise.all(promises);
  }
}
```

---

## Currying

Currying transforms a function that takes multiple arguments into a sequence of functions, each taking **one argument at a time**.

```javascript
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

### Why Currying Is Important

#### 1. Reusability & Function Specialization

You can create specialized functions by partially applying arguments.

```javascript
function multiply(a) {
  return function(b) {
    return a * b;
  };
}

const double = multiply(2);
console.log(double(5)); // 10
```

#### 2. Improved Readability & Modularity

Curried functions make pipelines cleaner.

```javascript
const add = a => b => a + b;
const increment = add(1);

console.log(increment(10)); // 11
```

#### 3. Avoids Repetition

No need to pass the same arguments repeatedly.

```javascript
function greet(greeting) {
  return function(name) {
    return `${greeting}, ${name}`;
  };
}

const sayHello = greet("Hello");
console.log(sayHello("Alice")); // Hello, Alice
console.log(sayHello("Bob"));   // Hello, Bob
```

#### 4. Works Well with Higher-Order Functions

Currying fits perfectly with `map`, `filter`, and `reduce`.

```javascript
const isGreaterThan = a => b => b > a;
const greaterThan10 = isGreaterThan(10);

console.log([5, 15, 20].filter(greaterThan10)); // [15, 20]
```

#### 5. Encourages Functional Programming

Currying supports **function composition**.

```javascript
const add = a => b => a + b;
const multiply = a => b => a * b;

const add5ThenDouble = x => multiply(2)(add(5)(x));
console.log(add5ThenDouble(10)); // (10+5)*2 = 30
```

### Real-World Use Cases

#### 1. Event Handling

```javascript
const handleEvent = type => msg => console.log(`${type}: ${msg}`);

const errorHandler = handleEvent("Error");
errorHandler("Something went wrong!"); // Error: Something went wrong!
```

#### 2. API Requests

```javascript
const apiRequest = baseUrl => endpoint => params =>
  fetch(`${baseUrl}/${endpoint}?${new URLSearchParams(params)}`);

const githubAPI = apiRequest("https://api.github.com");
const getUsers = githubAPI("users");

getUsers({ since: 1000 });
```

#### 3. React Example

```javascript
const withClass = className => Component => props =>
  <div className={className}><Component {...props} /></div>;

// Usage
const RedButton = withClass("red")(props => <button>{props.label}</button>);
```

### Summary

Currying is important because it:
- Enables **function specialization**.
- Improves **reusability & modularity**.
- Reduces **argument repetition**.
- Integrates with **functional programming**.
- Simplifies **real-world tasks** (event handling, API requests, React patterns).
