# JavaScript Functions - Comprehensive Guide

## What is a Function?

A **function** is a reusable block of code designed to perform a particular task. Functions are first-class objects in JavaScript, meaning they can be:
- Assigned to variables
- Passed as arguments to other functions
- Returned from other functions
- Stored in data structures

```javascript
// Basic function
function greet(name) {
  return `Hello, ${name}!`;
}

console.log(greet('Alice')); // "Hello, Alice!"
```

**Key Characteristics:**
- All functions are objects (inherit from `Function.prototype`)
- Functions can have properties and methods
- Functions have their own `this` context
- Functions can be invoked in multiple ways

---

## Function Declaration vs Function Expression

### Function Declaration

A function declaration defines a named function and is **hoisted** to the top of its scope.

```javascript
// Function Declaration
function add(a, b) {
  return a + b;
}

console.log(add(5, 3)); // 8

// Can be called before declaration (due to hoisting)
console.log(multiply(4, 2)); // 8

function multiply(a, b) {
  return a * b;
}
```

**Characteristics:**
- Hoisted (available before declaration)
- Must have a name
- Creates a function-scoped variable
- Ideal for functions used throughout the scope

### Function Expression

A function expression defines a function as part of an expression and is **not hoisted**.

```javascript
// Anonymous function expression
const subtract = function(a, b) {
  return a - b;
};

console.log(subtract(10, 3)); // 7

// Named function expression (useful for debugging)
const divide = function divideNumbers(a, b) {
  if (b === 0) {
    throw new Error('Division by zero');
  }
  return a / b;
};

console.log(divide(10, 2)); // 5
```

**Characteristics:**
- Not hoisted (cannot be called before definition)
- Can be anonymous or named
- Can be assigned to variables or passed as arguments
- Evaluated when code execution reaches it

### Comparison Table

| Feature | Function Declaration | Function Expression |
|---------|---------------------|---------------------|
| Syntax | `function name() {}` | `const name = function() {}` |
| Hoisting | ✅ Yes | ❌ No |
| Name Required | ✅ Yes | ❌ No (can be anonymous) |
| Use Before Definition | ✅ Yes | ❌ No |
| Best For | Top-level functions | Callbacks, conditional creation |

### Example: Conditional Function Creation

```javascript
// ❌ Function declaration - both branches define the function
if (true) {
  function test() {
    return 'A';
  }
} else {
  function test() {
    return 'B';
  }
}
console.log(test()); // Behavior varies by engine (avoid this)

// ✅ Function expression - works as expected
let test2;
if (true) {
  test2 = function() {
    return 'A';
  };
} else {
  test2 = function() {
    return 'B';
  };
}
console.log(test2()); // 'A'
```

---

## Function Parameters

### Required Parameters

```javascript
function greet(firstName, lastName) {
  return `Hello, ${firstName} ${lastName}!`;
}

console.log(greet('John', 'Doe')); // "Hello, John Doe!"
console.log(greet('Jane')); // "Hello, Jane undefined!"
```

### Default Parameters (ES6)

```javascript
// Traditional way (pre-ES6)
function greetOld(name) {
  name = name || 'Guest';
  return `Hello, ${name}!`;
}

// Modern way (ES6+)
function greetNew(name = 'Guest') {
  return `Hello, ${name}!`;
}

console.log(greetNew()); // "Hello, Guest!"
console.log(greetNew('Alice')); // "Hello, Alice!"

// Default parameters can reference earlier parameters
function createUser(name, role = 'user', id = name.toLowerCase()) {
  return { name, role, id };
}

console.log(createUser('Alice'));
// { name: 'Alice', role: 'user', id: 'alice' }

// Default parameters can be expressions
function calculatePrice(price, tax = price * 0.1) {
  return price + tax;
}

console.log(calculatePrice(100)); // 110
```

### Rest Parameters (...)

Rest parameters allow functions to accept an indefinite number of arguments as an array.

```javascript
// Basic rest parameters
function sum(...numbers) {
  return numbers.reduce((total, num) => total + num, 0);
}

console.log(sum(1, 2, 3)); // 6
console.log(sum(1, 2, 3, 4, 5)); // 15

// Rest parameters with other parameters
function multiply(multiplier, ...numbers) {
  return numbers.map(num => num * multiplier);
}

console.log(multiply(2, 1, 2, 3)); // [2, 4, 6]

// Gathering remaining arguments
function createUser(name, email, ...roles) {
  return {
    name,
    email,
    roles // Array of all remaining arguments
  };
}

console.log(createUser('Alice', 'alice@example.com', 'admin', 'editor'));
// { name: 'Alice', email: 'alice@example.com', roles: ['admin', 'editor'] }
```

**Important Notes:**
- Rest parameter must be the last parameter
- Only one rest parameter is allowed
- Rest parameters are real arrays (unlike `arguments` object)

```javascript
// ❌ Invalid - rest parameter not last
// function invalid(...args, last) {}

// ✅ Valid
function valid(first, ...rest) {
  console.log(first); // First argument
  console.log(rest);  // Array of remaining arguments
}
```

---

## Return Statements

### Basic Return

```javascript
function add(a, b) {
  return a + b;
}

const result = add(5, 3);
console.log(result); // 8
```

### Implicit Return (Undefined)

Functions without explicit `return` statement return `undefined`.

```javascript
function noReturn() {
  console.log('This function has no return');
}

const result = noReturn(); // Logs: "This function has no return"
console.log(result); // undefined
```

### Early Return (Guard Clauses)

```javascript
function divide(a, b) {
  // Guard clause - early return
  if (b === 0) {
    return 'Cannot divide by zero';
  }
  
  return a / b;
}

console.log(divide(10, 2)); // 5
console.log(divide(10, 0)); // "Cannot divide by zero"

// Multiple guard clauses
function processUser(user) {
  if (!user) {
    return { error: 'User is required' };
  }
  
  if (!user.email) {
    return { error: 'Email is required' };
  }
  
  if (user.age < 18) {
    return { error: 'Must be 18 or older' };
  }
  
  // Main logic
  return { success: true, user };
}
```

### Returning Objects and Arrays

```javascript
// Returning object
function createPerson(name, age) {
  return {
    name: name,
    age: age,
    greet: function() {
      return `Hi, I'm ${this.name}`;
    }
  };
}

// Returning array
function getCoordinates() {
  return [40.7128, -74.0060];
}

const [lat, lng] = getCoordinates();
console.log(lat, lng); // 40.7128 -74.006
```

### Returning Functions (Closures)

```javascript
function multiplier(factor) {
  return function(number) {
    return number * factor;
  };
}

const double = multiplier(2);
const triple = multiplier(3);

console.log(double(5)); // 10
console.log(triple(5)); // 15
```

---

## IIFE (Immediately Invoked Function Expression)

An **IIFE** is a function that runs immediately after it's defined.

### Basic IIFE Syntax

```javascript
// Anonymous IIFE
(function() {
  console.log('This runs immediately!');
})();

// With parameters
(function(name) {
  console.log(`Hello, ${name}!`);
})('Alice');

// Arrow function IIFE
(() => {
  console.log('Arrow IIFE');
})();

// Alternative syntax (same behavior)
!function() {
  console.log('Another IIFE syntax');
}();
```

### Use Cases for IIFE

#### 1. Creating Private Scope

```javascript
// Avoid polluting global scope
(function() {
  const privateVar = 'I am private';
  
  function privateFunction() {
    console.log(privateVar);
  }
  
  privateFunction(); // Works
})();

// console.log(privateVar); // ReferenceError
```

#### 2. Module Pattern

```javascript
const counter = (function() {
  let count = 0; // Private variable
  
  return {
    increment: function() {
      count++;
      return count;
    },
    decrement: function() {
      count--;
      return count;
    },
    getCount: function() {
      return count;
    }
  };
})();

console.log(counter.increment()); // 1
console.log(counter.increment()); // 2
console.log(counter.getCount());  // 2
// console.log(counter.count); // undefined (private)
```

#### 3. Avoiding Variable Collision

```javascript
// Before IIFE (ES5)
for (var i = 0; i < 3; i++) {
  setTimeout(function() {
    console.log(i); // 3, 3, 3 (var is function-scoped)
  }, 100);
}

// Using IIFE to create closure
for (var i = 0; i < 3; i++) {
  (function(index) {
    setTimeout(function() {
      console.log(index); // 0, 1, 2
    }, 100);
  })(i);
}

// Modern solution (ES6)
for (let i = 0; i < 3; i++) {
  setTimeout(function() {
    console.log(i); // 0, 1, 2 (let is block-scoped)
  }, 100);
}
```

---

## Higher-Order Functions

A **higher-order function** is a function that:
1. Takes one or more functions as arguments, OR
2. Returns a function as its result

### Functions as Arguments

```javascript
// Higher-order function
function repeat(n, action) {
  for (let i = 0; i < n; i++) {
    action(i);
  }
}

// Usage
repeat(3, console.log);
// Output: 0, 1, 2

repeat(3, (i) => {
  console.log(`Iteration ${i}`);
});
// Output:
// Iteration 0
// Iteration 1
// Iteration 2
```

### Functions Returning Functions

```javascript
function createMultiplier(multiplier) {
  return function(number) {
    return number * multiplier;
  };
}

const double = createMultiplier(2);
const triple = createMultiplier(3);

console.log(double(10)); // 20
console.log(triple(10)); // 30
```

### Built-in Higher-Order Functions

```javascript
const numbers = [1, 2, 3, 4, 5];

// map - transforms each element
const doubled = numbers.map(n => n * 2);
console.log(doubled); // [2, 4, 6, 8, 10]

// filter - selects elements
const evens = numbers.filter(n => n % 2 === 0);
console.log(evens); // [2, 4]

// reduce - accumulates values
const sum = numbers.reduce((acc, n) => acc + n, 0);
console.log(sum); // 15

// forEach - executes for each element
numbers.forEach(n => console.log(n * 2));

// sort - sorts with comparator
const sorted = [3, 1, 4, 1, 5].sort((a, b) => a - b);
console.log(sorted); // [1, 1, 3, 4, 5]
```

### Practical Example: Function Composition

```javascript
// Helper higher-order functions
const compose = (...fns) => (x) => fns.reduceRight((v, f) => f(v), x);
const pipe = (...fns) => (x) => fns.reduce((v, f) => f(v), x);

// Simple functions
const addOne = (x) => x + 1;
const double = (x) => x * 2;
const square = (x) => x * x;

// Compose (right to left)
const composedFn = compose(square, double, addOne);
console.log(composedFn(3)); // square(double(addOne(3))) = square(double(4)) = square(8) = 64

// Pipe (left to right)
const pipedFn = pipe(addOne, double, square);
console.log(pipedFn(3)); // square(double(addOne(3))) = square(double(4)) = square(8) = 64
```

---

## Callbacks

A **callback** is a function passed as an argument to another function. Callbacks are covered in detail in [callback-functions.md](./09-callback-functions.md).

### Quick Example

```javascript
// Synchronous callback
function processArray(arr, callback) {
  const result = [];
  for (let item of arr) {
    result.push(callback(item));
  }
  return result;
}

const numbers = [1, 2, 3];
const squared = processArray(numbers, n => n * n);
console.log(squared); // [1, 4, 9]

// Asynchronous callback
function fetchData(callback) {
  setTimeout(() => {
    callback({ data: 'Some data' });
  }, 1000);
}

fetchData((result) => {
  console.log(result); // { data: 'Some data' }
});
```

For more details on callbacks, see [09-callback-functions.md](./09-callback-functions.md).

---

## Function Scope and Closures

### Function Scope

Each function creates its own scope. Variables declared inside a function are not accessible outside.

```javascript
function outer() {
  const outerVar = 'I am outer';
  
  function inner() {
    const innerVar = 'I am inner';
    console.log(outerVar); // ✅ Can access outer scope
    console.log(innerVar); // ✅ Can access own scope
  }
  
  inner();
  // console.log(innerVar); // ❌ ReferenceError
}

outer();
// console.log(outerVar); // ❌ ReferenceError
```

### Closures (Brief Introduction)

A **closure** is a function that has access to variables in its outer (enclosing) scope, even after the outer function has returned.

```javascript
function createCounter() {
  let count = 0; // Private variable
  
  return {
    increment() {
      count++;
      return count;
    },
    decrement() {
      count--;
      return count;
    },
    getCount() {
      return count;
    }
  };
}

const counter = createCounter();
console.log(counter.increment()); // 1
console.log(counter.increment()); // 2
console.log(counter.getCount());  // 2
```

**For comprehensive closure examples and interview questions, see:** [11-interview/Closure.md](./11-interview/Closure.md)

---

## Recursion

**Recursion** is when a function calls itself. Every recursive function needs:
1. **Base case** - condition to stop recursion
2. **Recursive case** - function calls itself with modified arguments

### Basic Recursion Examples

#### Example 1: Factorial

```javascript
function factorial(n) {
  // Base case
  if (n <= 1) {
    return 1;
  }
  
  // Recursive case
  return n * factorial(n - 1);
}

console.log(factorial(5)); // 120
// Execution: 5 * 4 * 3 * 2 * 1 = 120
```

#### Example 2: Fibonacci

```javascript
function fibonacci(n) {
  // Base cases
  if (n <= 0) return 0;
  if (n === 1) return 1;
  
  // Recursive case
  return fibonacci(n - 1) + fibonacci(n - 2);
}

console.log(fibonacci(6)); // 8
// Sequence: 0, 1, 1, 2, 3, 5, 8
```

#### Example 3: Sum of Array

```javascript
function sumArray(arr) {
  // Base case
  if (arr.length === 0) {
    return 0;
  }
  
  // Recursive case
  return arr[0] + sumArray(arr.slice(1));
}

console.log(sumArray([1, 2, 3, 4, 5])); // 15
```

### Advanced Recursion Examples

#### Example 4: Flatten Nested Array

```javascript
function flatten(arr) {
  let result = [];
  
  for (let item of arr) {
    if (Array.isArray(item)) {
      // Recursive case - flatten nested array
      result = result.concat(flatten(item));
    } else {
      // Base case - add non-array item
      result.push(item);
    }
  }
  
  return result;
}

console.log(flatten([1, [2, [3, 4], 5], 6]));
// [1, 2, 3, 4, 5, 6]
```

#### Example 5: Deep Clone Object

```javascript
function deepClone(obj) {
  // Base cases
  if (obj === null || typeof obj !== 'object') {
    return obj;
  }
  
  if (Array.isArray(obj)) {
    return obj.map(item => deepClone(item));
  }
  
  // Recursive case - clone object properties
  const cloned = {};
  for (let key in obj) {
    if (obj.hasOwnProperty(key)) {
      cloned[key] = deepClone(obj[key]);
    }
  }
  
  return cloned;
}

const original = {
  name: 'Alice',
  address: {
    city: 'NYC',
    coords: { lat: 40, lng: -74 }
  },
  hobbies: ['reading', 'coding']
};

const copy = deepClone(original);
copy.address.city = 'LA';
console.log(original.address.city); // 'NYC' (not affected)
console.log(copy.address.city);     // 'LA'
```

#### Example 6: Binary Tree Traversal

```javascript
class TreeNode {
  constructor(value) {
    this.value = value;
    this.left = null;
    this.right = null;
  }
}

function inOrderTraversal(node, result = []) {
  if (node === null) {
    return result;
  }
  
  // Left -> Root -> Right
  inOrderTraversal(node.left, result);
  result.push(node.value);
  inOrderTraversal(node.right, result);
  
  return result;
}

// Create tree:     4
//                /   \
//               2     6
//              / \   / \
//             1   3 5   7
const root = new TreeNode(4);
root.left = new TreeNode(2);
root.right = new TreeNode(6);
root.left.left = new TreeNode(1);
root.left.right = new TreeNode(3);
root.right.left = new TreeNode(5);
root.right.right = new TreeNode(7);

console.log(inOrderTraversal(root)); // [1, 2, 3, 4, 5, 6, 7]
```

### Recursion Best Practices

```javascript
// ❌ Bad - No base case (infinite recursion)
function badRecursion(n) {
  return n + badRecursion(n - 1); // Stack overflow!
}

// ✅ Good - Has base case
function goodRecursion(n) {
  if (n <= 0) return 0; // Base case
  return n + goodRecursion(n - 1);
}

// ✅ Better - Tail recursion (optimizable by some engines)
function tailRecursion(n, acc = 0) {
  if (n <= 0) return acc;
  return tailRecursion(n - 1, acc + n);
}

// ✅ Best for large data - Iterative solution (avoids stack overflow)
function iterative(n) {
  let result = 0;
  for (let i = n; i > 0; i--) {
    result += i;
  }
  return result;
}
```

---

## Pure Functions vs Impure Functions

### Pure Functions

A **pure function**:
1. Always returns the same output for the same input
2. Has no side effects (doesn't modify external state)

```javascript
// ✅ Pure function
function add(a, b) {
  return a + b;
}

console.log(add(2, 3)); // Always 5
console.log(add(2, 3)); // Always 5

// ✅ Pure function - array methods that return new arrays
function double(numbers) {
  return numbers.map(n => n * 2);
}

const nums = [1, 2, 3];
console.log(double(nums)); // [2, 4, 6]
console.log(nums);         // [1, 2, 3] (not modified)

// ✅ Pure function with objects
function updateUser(user, newEmail) {
  return {
    ...user,
    email: newEmail
  };
}

const user = { name: 'Alice', email: 'alice@old.com' };
const updated = updateUser(user, 'alice@new.com');
console.log(user.email);    // 'alice@old.com' (not modified)
console.log(updated.email); // 'alice@new.com'
```

### Impure Functions

An **impure function** has side effects or depends on external state.

```javascript
let count = 0;

// ❌ Impure - modifies external state
function incrementCount() {
  count++;
  return count;
}

console.log(incrementCount()); // 1
console.log(incrementCount()); // 2 (different output)

// ❌ Impure - modifies input
function addItem(arr, item) {
  arr.push(item);
  return arr;
}

const items = [1, 2];
addItem(items, 3);
console.log(items); // [1, 2, 3] (modified)

// ❌ Impure - depends on external state (Date)
function getCurrentTime() {
  return new Date().toISOString();
}

// ❌ Impure - I/O operations
function logMessage(message) {
  console.log(message); // Side effect
}

// ❌ Impure - API calls
function fetchUserData(userId) {
  return fetch(`/api/users/${userId}`);
}
```

### Comparison Table

| Aspect | Pure Function | Impure Function |
|--------|--------------|-----------------|
| Same input → Same output | ✅ Always | ❌ May vary |
| Side effects | ❌ None | ✅ Yes |
| External state | ❌ Doesn't depend on it | ✅ May depend on it |
| Testability | ✅ Easy to test | ⚠️ Harder to test |
| Predictability | ✅ Highly predictable | ⚠️ Less predictable |
| Caching | ✅ Can be memoized | ❌ Cannot be memoized |

### Converting Impure to Pure

```javascript
// ❌ Impure
let total = 0;
function addToTotal(value) {
  total += value;
  return total;
}

// ✅ Pure - pass state as parameter
function addToTotal(total, value) {
  return total + value;
}

let total = 0;
total = addToTotal(total, 5);
total = addToTotal(total, 10);
console.log(total); // 15
```

---

## Function Hoisting

**Hoisting** is JavaScript's behavior of moving declarations to the top of their scope during the compilation phase.

### Function Declaration Hoisting

```javascript
// ✅ This works - function declarations are hoisted
console.log(greet('Alice')); // "Hello, Alice!"

function greet(name) {
  return `Hello, ${name}!`;
}

// Behind the scenes (conceptually):
// function greet(name) {
//   return `Hello, ${name}!`;
// }
// console.log(greet('Alice'));
```

### Function Expression NOT Hoisted

```javascript
// ❌ This doesn't work - function expressions are not hoisted
console.log(greet('Alice')); // TypeError: greet is not a function

var greet = function(name) {
  return `Hello, ${name}!`;
};

// Behind the scenes (conceptually):
// var greet; // Declaration hoisted (undefined)
// console.log(greet('Alice')); // greet is undefined
// greet = function(name) { ... }; // Assignment stays in place
```

### Hoisting with let/const

```javascript
// ❌ ReferenceError - temporal dead zone
console.log(greet('Alice')); // ReferenceError: Cannot access 'greet' before initialization

const greet = function(name) {
  return `Hello, ${name}!`;
};
```

### Hoisting Examples

```javascript
// Example 1: Function overwriting
console.log(test()); // "Second"

function test() {
  return "First";
}

function test() {
  return "Second";
}

// Both declarations are hoisted, second overwrites first

// Example 2: Variable vs Function hoisting
console.log(typeof foo); // "function"

var foo = 'variable';

function foo() {
  return 'function';
}

console.log(typeof foo); // "string"

// Function declaration hoisted first, then variable assignment happens
```

---

## Arguments Object

The `arguments` object is an array-like object available inside all (non-arrow) functions containing the values of arguments passed.

### Basic Usage

```javascript
function sum() {
  console.log(arguments); // [Arguments] { '0': 1, '1': 2, '2': 3 }
  console.log(arguments.length); // 3
  console.log(arguments[0]); // 1
  
  let total = 0;
  for (let i = 0; i < arguments.length; i++) {
    total += arguments[i];
  }
  return total;
}

console.log(sum(1, 2, 3)); // 6
console.log(sum(1, 2, 3, 4, 5)); // 15
```

### Arguments Object Characteristics

```javascript
function demo(a, b) {
  console.log(arguments.length);     // Number of arguments passed
  console.log(arguments[0]);         // First argument
  console.log(arguments[1]);         // Second argument
  console.log(Array.isArray(arguments)); // false (array-like, not array)
  
  // ❌ Arguments is not a real array
  // arguments.forEach(x => console.log(x)); // TypeError
  
  // ✅ Convert to array
  const argsArray = Array.from(arguments);
  argsArray.forEach(x => console.log(x)); // Works
  
  // ✅ Or use spread operator
  const argsArray2 = [...arguments];
  console.log(argsArray2); // [1, 2, 3]
}

demo(1, 2, 3);
```

### Arguments vs Rest Parameters

```javascript
// Old way - using arguments object
function sumOld() {
  let total = 0;
  for (let i = 0; i < arguments.length; i++) {
    total += arguments[i];
  }
  return total;
}

// Modern way - using rest parameters (preferred)
function sumNew(...numbers) {
  return numbers.reduce((total, num) => total + num, 0);
}

console.log(sumOld(1, 2, 3)); // 6
console.log(sumNew(1, 2, 3)); // 6
```

**Comparison:**

| Feature | arguments | Rest Parameters |
|---------|-----------|-----------------|
| Array methods | ❌ No | ✅ Yes |
| Arrow functions | ❌ Not available | ✅ Available |
| Named parameter | ❌ No | ✅ Yes |
| Modern | ❌ Legacy | ✅ Modern |
| Best practice | ⚠️ Avoid | ✅ Prefer |

### arguments.callee (Deprecated)

```javascript
// ❌ Deprecated - don't use
function factorial(n) {
  if (n <= 1) return 1;
  return n * arguments.callee(n - 1); // Refers to the function itself
}

// ✅ Use named function instead
function factorial(n) {
  if (n <= 1) return 1;
  return n * factorial(n - 1);
}
```

### Arrow Functions and arguments

```javascript
// ❌ Arrow functions don't have arguments object
const sum = () => {
  console.log(arguments); // ReferenceError in strict mode
};

// ✅ Use rest parameters with arrow functions
const sumArrow = (...numbers) => {
  return numbers.reduce((total, num) => total + num, 0);
};

console.log(sumArrow(1, 2, 3)); // 6
```

---

## Method Definitions

Methods are functions defined as properties of objects.

### Object Methods - Traditional Syntax

```javascript
const person = {
  name: 'Alice',
  age: 30,
  
  // Method using function expression
  greet: function() {
    return `Hi, I'm ${this.name}`;
  },
  
  // Method accessing other properties
  introduce: function() {
    return `${this.name} is ${this.age} years old`;
  }
};

console.log(person.greet());      // "Hi, I'm Alice"
console.log(person.introduce());  // "Alice is 30 years old"
```

### Object Methods - ES6 Shorthand

```javascript
const person = {
  name: 'Bob',
  age: 25,
  
  // ES6 method shorthand (preferred)
  greet() {
    return `Hi, I'm ${this.name}`;
  },
  
  celebrateBirthday() {
    this.age++;
    return `Happy birthday! Now ${this.age}`;
  }
};

console.log(person.greet());              // "Hi, I'm Bob"
console.log(person.celebrateBirthday());  // "Happy birthday! Now 26"
```

### Getter and Setter Methods

```javascript
const user = {
  firstName: 'John',
  lastName: 'Doe',
  
  // Getter - accessed like a property
  get fullName() {
    return `${this.firstName} ${this.lastName}`;
  },
  
  // Setter - assigned like a property
  set fullName(value) {
    const parts = value.split(' ');
    this.firstName = parts[0];
    this.lastName = parts[1];
  }
};

console.log(user.fullName); // "John Doe" (no parentheses)
user.fullName = 'Jane Smith'; // Using setter
console.log(user.firstName); // "Jane"
console.log(user.lastName);  // "Smith"
```

### Class Methods

```javascript
class Calculator {
  constructor() {
    this.value = 0;
  }
  
  // Instance method
  add(n) {
    this.value += n;
    return this;
  }
  
  subtract(n) {
    this.value -= n;
    return this;
  }
  
  // Static method (called on class, not instance)
  static multiply(a, b) {
    return a * b;
  }
  
  // Getter
  get result() {
    return this.value;
  }
}

const calc = new Calculator();
calc.add(5).subtract(2);
console.log(calc.result); // 3

console.log(Calculator.multiply(3, 4)); // 12 (static method)
```

### Method Chaining

```javascript
const calculator = {
  value: 0,
  
  add(n) {
    this.value += n;
    return this; // Return this for chaining
  },
  
  subtract(n) {
    this.value -= n;
    return this;
  },
  
  multiply(n) {
    this.value *= n;
    return this;
  },
  
  getResult() {
    return this.value;
  }
};

const result = calculator
  .add(10)
  .subtract(5)
  .multiply(2)
  .getResult();

console.log(result); // 10
```

---

## Interview Questions

### Q1: What's the output?

```javascript
console.log(foo());
console.log(bar());

function foo() {
  return 'foo';
}

var bar = function() {
  return 'bar';
};
```

**Answer:**
```
"foo"
TypeError: bar is not a function
```
**Explanation:** Function declarations are hoisted, but function expressions are not.

---

### Q2: What does this print?

```javascript
for (var i = 0; i < 3; i++) {
  setTimeout(function() {
    console.log(i);
  }, 1000);
}
```

**Answer:** `3, 3, 3` (all print 3)

**Explanation:** `var` is function-scoped. By the time the callbacks run, the loop has finished and `i` is 3.

**Solution 1 - IIFE:**
```javascript
for (var i = 0; i < 3; i++) {
  (function(index) {
    setTimeout(function() {
      console.log(index);
    }, 1000);
  })(i);
}
// Prints: 0, 1, 2
```

**Solution 2 - let:**
```javascript
for (let i = 0; i < 3; i++) {
  setTimeout(function() {
    console.log(i);
  }, 1000);
}
// Prints: 0, 1, 2
```

---

### Q3: Implement a function that can be called like this

```javascript
add(2)(3)(4) // Returns 9
```

**Answer:**

```javascript
function add(a) {
  return function(b) {
    return function(c) {
      return a + b + c;
    };
  };
}

// Or with arrow functions
const add = a => b => c => a + b + c;

console.log(add(2)(3)(4)); // 9
```

---

### Q4: What's the difference between these?

```javascript
// Version 1
const obj1 = {
  method: function() {
    setTimeout(function() {
      console.log(this);
    }, 100);
  }
};

// Version 2
const obj2 = {
  method: function() {
    setTimeout(() => {
      console.log(this);
    }, 100);
  }
};

obj1.method();
obj2.method();
```

**Answer:**
- Version 1: `this` refers to global object (or `undefined` in strict mode)
- Version 2: `this` refers to `obj2` (arrow functions inherit `this`)

---

### Q5: Write a function to memoize expensive calculations

```javascript
function memoize(fn) {
  const cache = {};
  
  return function(...args) {
    const key = JSON.stringify(args);
    
    if (key in cache) {
      console.log('Returning cached result');
      return cache[key];
    }
    
    console.log('Calculating result');
    const result = fn(...args);
    cache[key] = result;
    return result;
  };
}

// Usage
function expensiveOperation(n) {
  let result = 0;
  for (let i = 0; i < n * 1000000; i++) {
    result += i;
  }
  return result;
}

const memoizedOperation = memoize(expensiveOperation);

console.log(memoizedOperation(1)); // Calculating... (slow)
console.log(memoizedOperation(1)); // Cached (fast)
console.log(memoizedOperation(2)); // Calculating... (slow)
console.log(memoizedOperation(2)); // Cached (fast)
```

---

### Q6: What's wrong with this code?

```javascript
const obj = {
  name: 'Alice',
  greet: () => {
    console.log(`Hello, ${this.name}`);
  }
};

obj.greet();
```

**Answer:** Prints `"Hello, undefined"`

**Explanation:** Arrow functions don't have their own `this`. They inherit `this` from the enclosing scope (likely global/window).

**Fix:**
```javascript
const obj = {
  name: 'Alice',
  greet() {
    console.log(`Hello, ${this.name}`);
  }
};

obj.greet(); // "Hello, Alice"
```

---

### Q7: Implement a curry function

```javascript
function curry(fn) {
  return function curried(...args) {
    if (args.length >= fn.length) {
      return fn.apply(this, args);
    } else {
      return function(...nextArgs) {
        return curried.apply(this, args.concat(nextArgs));
      };
    }
  };
}

// Usage
function add(a, b, c) {
  return a + b + c;
}

const curriedAdd = curry(add);

console.log(curriedAdd(1)(2)(3));    // 6
console.log(curriedAdd(1, 2)(3));    // 6
console.log(curriedAdd(1)(2, 3));    // 6
console.log(curriedAdd(1, 2, 3));    // 6
```

---

### Q8: What will this log?

```javascript
function test() {
  console.log(a);
  console.log(foo());
  
  var a = 1;
  function foo() {
    return 2;
  }
}

test();
```

**Answer:**
```
undefined
2
```

**Explanation:** 
- `var a` declaration is hoisted (but not assignment), so it's `undefined`
- `function foo()` is fully hoisted, so it works

---

## Best Practices

### 1. Use Descriptive Names

```javascript
// ❌ Bad
function calc(x, y) {
  return x * y + x;
}

// ✅ Good
function calculatePriceWithTax(price, taxRate) {
  return price * taxRate + price;
}
```

### 2. Keep Functions Small and Focused

```javascript
// ❌ Bad - function does too much
function processUser(user) {
  validateEmail(user.email);
  hashPassword(user.password);
  saveToDatabase(user);
  sendWelcomeEmail(user);
  logActivity(user);
}

// ✅ Good - separate concerns
function validateUser(user) {
  validateEmail(user.email);
  validatePassword(user.password);
}

function saveUser(user) {
  const hashedPassword = hashPassword(user.password);
  return saveToDatabase({ ...user, password: hashedPassword });
}

function notifyUser(user) {
  sendWelcomeEmail(user);
  logActivity(user);
}
```

### 3. Prefer Pure Functions When Possible

```javascript
// ❌ Impure
let total = 0;
function addToTotal(value) {
  total += value;
}

// ✅ Pure
function add(a, b) {
  return a + b;
}

let total = 0;
total = add(total, 5);
```

### 4. Use Default Parameters

```javascript
// ❌ Old way
function greet(name) {
  name = name || 'Guest';
  return `Hello, ${name}`;
}

// ✅ Modern way
function greet(name = 'Guest') {
  return `Hello, ${name}`;
}
```

### 5. Use Rest Parameters Over arguments

```javascript
// ❌ Avoid arguments object
function sum() {
  let total = 0;
  for (let i = 0; i < arguments.length; i++) {
    total += arguments[i];
  }
  return total;
}

// ✅ Use rest parameters
function sum(...numbers) {
  return numbers.reduce((total, num) => total + num, 0);
}
```

### 6. Use Arrow Functions for Callbacks

```javascript
// ❌ Verbose
numbers.map(function(n) {
  return n * 2;
});

// ✅ Concise
numbers.map(n => n * 2);
```

### 7. Use Function Expressions for Conditional Creation

```javascript
// ❌ Unpredictable with function declarations
if (condition) {
  function test() { return 'A'; }
} else {
  function test() { return 'B'; }
}

// ✅ Predictable with function expressions
let test;
if (condition) {
  test = function() { return 'A'; };
} else {
  test = function() { return 'B'; };
}
```

---

## Common Pitfalls

### 1. Forgetting to Return

```javascript
// ❌ Forgot return
function add(a, b) {
  a + b; // Returns undefined
}

// ✅ Fixed
function add(a, b) {
  return a + b;
}
```

### 2. Modifying Arguments

```javascript
// ❌ Modifies original array
function addItem(arr, item) {
  arr.push(item);
  return arr;
}

// ✅ Returns new array
function addItem(arr, item) {
  return [...arr, item];
}
```

### 3. Confusing Function Declaration vs Expression

```javascript
// ❌ This might not work as expected
console.log(foo()); // May throw error

if (true) {
  function foo() { return 'test'; }
}

// ✅ Use function expression for conditional logic
let foo;
if (true) {
  foo = function() { return 'test'; };
}
console.log(foo());
```

### 4. Not Handling Missing Arguments

```javascript
// ❌ Doesn't handle missing arguments
function divide(a, b) {
  return a / b; // What if b is undefined?
}

// ✅ Use default parameters or validation
function divide(a, b = 1) {
  if (b === 0) {
    throw new Error('Cannot divide by zero');
  }
  return a / b;
}
```

### 5. Infinite Recursion

```javascript
// ❌ Missing base case
function countdown(n) {
  console.log(n);
  countdown(n - 1); // Stack overflow!
}

// ✅ Has base case
function countdown(n) {
  if (n <= 0) return; // Base case
  console.log(n);
  countdown(n - 1);
}
```

### 6. this Context Loss

```javascript
const obj = {
  name: 'Alice',
  greet() {
    console.log(`Hello, ${this.name}`);
  }
};

// ❌ Loses context
const greet = obj.greet;
greet(); // "Hello, undefined"

// ✅ Bind context
const boundGreet = obj.greet.bind(obj);
boundGreet(); // "Hello, Alice"

// ✅ Or use arrow function
setTimeout(() => obj.greet(), 1000);
```

---

## Summary

### Key Concepts

1. **Function Types:**
   - Function declarations (hoisted)
   - Function expressions (not hoisted)
   - Arrow functions (covered in [05-arrow-functions.md](./05-arrow-functions.md))
   - IIFE (immediately invoked)

2. **Parameters:**
   - Required parameters
   - Default parameters (ES6)
   - Rest parameters (...args)
   - Arguments object (legacy)

3. **Function Features:**
   - Higher-order functions (take/return functions)
   - Callbacks (detailed in [09-callback-functions.md](./09-callback-functions.md))
   - Closures (detailed in [11-interview/Closure.md](./11-interview/Closure.md))
   - Recursion (function calls itself)

4. **Function Characteristics:**
   - Pure vs impure functions
   - Function hoisting behavior
   - Method definitions
   - Function scope

### When to Use What

| Use Case | Recommended Approach |
|----------|---------------------|
| Top-level utility function | Function declaration |
| Callback or conditional function | Function expression or arrow function |
| Object method | Method shorthand |
| One-time initialization | IIFE |
| Variable number of arguments | Rest parameters |
| Function composition | Higher-order functions |
| Private scope/variables | Closures or IIFE |
| Same function, different contexts | Pure functions |

### Best Practices Checklist

- ✅ Use descriptive function names
- ✅ Keep functions small and focused (single responsibility)
- ✅ Prefer pure functions when possible
- ✅ Use default parameters instead of `||` operator
- ✅ Use rest parameters instead of `arguments` object
- ✅ Always include base case in recursive functions
- ✅ Return new values instead of modifying inputs
- ✅ Use arrow functions for simple callbacks
- ✅ Use function expressions for conditional creation
- ✅ Handle edge cases (null, undefined, empty arrays)

### Related Topics

- **Arrow Functions:** [05-arrow-functions.md](./05-arrow-functions.md)
- **Callback Functions:** [09-callback-functions.md](./09-callback-functions.md)
- **Closures:** [11-interview/Closure.md](./11-interview/Closure.md)
- **Async Functions:** [10-async.md](./10-async.md)

Master these function concepts to write clean, efficient, and maintainable JavaScript code! 🚀
