# JavaScript Modules

## What are Modules?

**Modules** are reusable pieces of code that can be exported from one file and imported into another. They help organize code, prevent naming conflicts, and manage dependencies.

---

## ES6 Modules (ESM)

Modern JavaScript uses **ES6 module syntax** with `import` and `export`.

### Named Exports

Export multiple values from a module.

```javascript
// math.js
export const PI = 3.14159;

export function add(a, b) {
  return a + b;
}

export function subtract(a, b) {
  return a - b;
}

export class Calculator {
  multiply(a, b) {
    return a * b;
  }
}
```

**Importing named exports:**

```javascript
// app.js
import { PI, add, subtract, Calculator } from './math.js';

console.log(PI);              // 3.14159
console.log(add(5, 3));       // 8
console.log(subtract(10, 4)); // 6

const calc = new Calculator();
console.log(calc.multiply(2, 3)); // 6
```

**Import with aliases:**

```javascript
import { add as sum, subtract as diff } from './math.js';

console.log(sum(5, 3));  // 8
console.log(diff(10, 4)); // 6
```

**Import everything:**

```javascript
import * as math from './math.js';

console.log(math.PI);
console.log(math.add(5, 3));
console.log(math.subtract(10, 4));
```

### Default Exports

Export a single main value from a module.

```javascript
// user.js
export default class User {
  constructor(name) {
    this.name = name;
  }
  
  greet() {
    console.log(`Hello, I'm ${this.name}`);
  }
}
```

**Importing default export:**

```javascript
// app.js
import User from './user.js';

const user = new User('Alice');
user.greet(); // "Hello, I'm Alice"
```

**You can name it anything:**

```javascript
import MyUser from './user.js'; // Different name
import Person from './user.js'; // Another name

const user = new MyUser('Bob');
```

### Mixing Named and Default Exports

```javascript
// utils.js
export default function logger(message) {
  console.log(`[LOG]: ${message}`);
}

export const VERSION = '1.0.0';

export function formatDate(date) {
  return date.toISOString();
}
```

**Importing:**

```javascript
// app.js
import logger, { VERSION, formatDate } from './utils.js';

logger('Application started'); // [LOG]: Application started
console.log(VERSION);          // 1.0.0
console.log(formatDate(new Date()));
```

---

## Re-exporting

Export imports from other modules.

```javascript
// shapes/circle.js
export class Circle {
  constructor(radius) {
    this.radius = radius;
  }
}

// shapes/rectangle.js
export class Rectangle {
  constructor(width, height) {
    this.width = width;
    this.height = height;
  }
}

// shapes/index.js (barrel export)
export { Circle } from './circle.js';
export { Rectangle } from './rectangle.js';

// Or re-export everything
export * from './circle.js';
export * from './rectangle.js';
```

**Usage:**

```javascript
// app.js
import { Circle, Rectangle } from './shapes/index.js';

const circle = new Circle(5);
const rect = new Rectangle(10, 20);
```

---

## CommonJS (Node.js)

Traditional Node.js module system using `require` and `module.exports`.

### Exporting

```javascript
// math.js
const PI = 3.14159;

function add(a, b) {
  return a + b;
}

function subtract(a, b) {
  return a - b;
}

// Export multiple values
module.exports = {
  PI,
  add,
  subtract
};

// Or export individually
exports.PI = PI;
exports.add = add;
exports.subtract = subtract;

// Export single value
module.exports = add;
```

### Importing

```javascript
// app.js
const math = require('./math.js');

console.log(math.PI);
console.log(math.add(5, 3));
console.log(math.subtract(10, 4));

// Or destructure
const { PI, add, subtract } = require('./math.js');

// Single export
const add = require('./add.js');
```

---

## ES6 vs CommonJS

| Feature | ES6 Modules | CommonJS |
|---------|-------------|----------|
| **Syntax** | `import/export` | `require/module.exports` |
| **Loading** | Asynchronous | Synchronous |
| **Static analysis** | Yes (tree shaking) | No |
| **Browser support** | Modern browsers | Requires bundler |
| **Node.js** | Supported (`.mjs` or `"type": "module"`) | Default |
| **Top-level await** | Yes | No |
| **Dynamic imports** | `import()` | `require()` |
| **When loaded** | Compile time (hoisted) | Runtime |

### ES6 Modules Example

```javascript
// ES6: Imports are hoisted
import { user } from './user.js'; // Works even before this line runs
console.log(user);

export const data = 'some data';
```

### CommonJS Example

```javascript
// CommonJS: Imports are executed when reached
console.log('Start');

const user = require('./user.js'); // Loaded here
console.log(user);

console.log('End');
```

---

## Dynamic Imports

### ES6 Dynamic Import

Load modules on demand (returns a Promise).

```javascript
// Load module conditionally
if (condition) {
  import('./module.js')
    .then(module => {
      module.doSomething();
    })
    .catch(err => {
      console.error('Failed to load module', err);
    });
}

// With async/await
async function loadModule() {
  try {
    const module = await import('./module.js');
    module.doSomething();
  } catch (err) {
    console.error('Failed to load module', err);
  }
}

// Code splitting in webpack
button.addEventListener('click', async () => {
  const { feature } = await import('./feature.js');
  feature();
});
```

### CommonJS Dynamic Require

```javascript
// Load module conditionally
if (condition) {
  const module = require('./module.js');
  module.doSomething();
}

// Load based on variable
const moduleName = './math.js';
const math = require(moduleName);
```

---

## Module Patterns (Pre-ES6)

### Revealing Module Pattern

```javascript
const calculator = (function() {
  // Private variables
  let result = 0;
  
  // Private function
  function log(message) {
    console.log(`[Calculator]: ${message}`);
  }
  
  // Public API
  return {
    add(value) {
      result += value;
      log(`Added ${value}, result: ${result}`);
      return this;
    },
    
    subtract(value) {
      result -= value;
      log(`Subtracted ${value}, result: ${result}`);
      return this;
    },
    
    getResult() {
      return result;
    },
    
    reset() {
      result = 0;
      log('Reset');
      return this;
    }
  };
})();

calculator.add(5).subtract(2).add(10);
console.log(calculator.getResult()); // 13
```

### Singleton Pattern

```javascript
const database = (function() {
  let instance;
  
  function createInstance() {
    return {
      connect() {
        console.log('Connected to database');
      },
      disconnect() {
        console.log('Disconnected from database');
      }
    };
  }
  
  return {
    getInstance() {
      if (!instance) {
        instance = createInstance();
      }
      return instance;
    }
  };
})();

const db1 = database.getInstance();
const db2 = database.getInstance();

console.log(db1 === db2); // true (same instance)
```

---

## Module Best Practices

### 1. One Module, One Purpose

```javascript
// ❌ Bad: Too many responsibilities
export function validateUser() {}
export function fetchData() {}
export function renderUI() {}
export function handleError() {}

// ✅ Good: Separate concerns
// validation.js
export function validateUser() {}

// api.js
export function fetchData() {}

// ui.js
export function renderUI() {}

// errors.js
export function handleError() {}
```

### 2. Barrel Exports (Index Files)

```javascript
// components/index.js
export { Button } from './Button.js';
export { Input } from './Input.js';
export { Form } from './Form.js';

// Usage (cleaner imports)
import { Button, Input, Form } from './components';
```

### 3. Avoid Circular Dependencies

```javascript
// ❌ Bad: Circular dependency
// a.js
import { b } from './b.js';
export const a = b + 1;

// b.js
import { a } from './a.js';
export const b = a + 1;

// ✅ Good: Refactor to avoid circular dependency
// Extract shared logic to a third module
// shared.js
export const base = 0;

// a.js
import { base } from './shared.js';
export const a = base + 1;

// b.js
import { base } from './shared.js';
export const b = base + 2;
```

### 4. Default Export for Main Functionality

```javascript
// User.js - class is the main export
export default class User {
  constructor(name) {
    this.name = name;
  }
}

// constants.js - multiple related exports
export const MAX_USERS = 100;
export const MIN_AGE = 18;
export const ROLES = ['admin', 'user'];
```

---

## Real-World Examples

### Example 1: API Module

```javascript
// api/config.js
export const API_BASE_URL = 'https://api.example.com';
export const TIMEOUT = 5000;

// api/client.js
import { API_BASE_URL, TIMEOUT } from './config.js';

export default class APIClient {
  async request(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const config = {
      timeout: TIMEOUT,
      ...options
    };
    
    const response = await fetch(url, config);
    return response.json();
  }
  
  get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  }
  
  post(endpoint, data) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(data),
      headers: { 'Content-Type': 'application/json' }
    });
  }
}

// api/users.js
import APIClient from './client.js';

const client = new APIClient();

export async function getUsers() {
  return client.get('/users');
}

export async function getUser(id) {
  return client.get(`/users/${id}`);
}

export async function createUser(userData) {
  return client.post('/users', userData);
}

// api/index.js
export * from './users.js';
export { default as APIClient } from './client.js';
export * from './config.js';

// app.js
import { getUsers, createUser } from './api';

async function main() {
  const users = await getUsers();
  console.log(users);
  
  await createUser({ name: 'Alice', email: 'alice@example.com' });
}
```

### Example 2: Utilities Module

```javascript
// utils/string.js
export function capitalize(str) {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

export function truncate(str, length) {
  return str.length > length ? str.slice(0, length) + '...' : str;
}

// utils/array.js
export function unique(arr) {
  return [...new Set(arr)];
}

export function chunk(arr, size) {
  return Array.from({ length: Math.ceil(arr.length / size) }, (_, i) =>
    arr.slice(i * size, i * size + size)
  );
}

// utils/index.js
export * from './string.js';
export * from './array.js';

// app.js
import { capitalize, unique, chunk } from './utils';
```

---

## Node.js: Enabling ES6 Modules

### Method 1: `.mjs` Extension

```javascript
// math.mjs
export function add(a, b) {
  return a + b;
}

// app.mjs
import { add } from './math.mjs';
console.log(add(2, 3));
```

### Method 2: package.json

```json
{
  "type": "module",
  "name": "my-app"
}
```

Now all `.js` files are treated as ES6 modules.

### Method 3: Using Both Systems

```json
{
  "type": "module"
}
```

```javascript
// ES6 module (default with "type": "module")
// app.js
import fs from 'fs';

// CommonJS (use .cjs extension)
// legacy.cjs
const fs = require('fs');
```

---

## Interview Questions

### Q1: What's the difference between named and default exports?

**Named exports:**
- Multiple exports per module
- Must use exact name when importing
- Use `{ }` for imports

**Default exports:**
- One default per module
- Can rename on import
- No `{ }` needed

```javascript
// Named
export function foo() {}
import { foo } from './module.js';

// Default
export default function() {}
import anyName from './module.js';
```

### Q2: What happens if you have circular dependencies?

```javascript
// a.js
import { b } from './b.js';
export const a = 'a';
console.log(b); // undefined (b hasn't been evaluated yet)

// b.js
import { a } from './a.js';
export const b = 'b';
console.log(a); // 'a' (works because a is exported first)
```

ES6 modules handle circular dependencies better than CommonJS due to live bindings, but it's best to avoid them.

### Q3: How do you conditionally import a module?

```javascript
// ES6: Dynamic import
if (condition) {
  const module = await import('./module.js');
  module.doSomething();
}

// CommonJS: require anywhere
if (condition) {
  const module = require('./module.js');
  module.doSomething();
}
```

---

## Summary

### ES6 Modules
- ✅ Modern standard
- ✅ Static analysis (tree shaking)
- ✅ Asynchronous loading
- ✅ Better for browsers
- ✅ Live bindings (exports are references)

### CommonJS
- ✅ Node.js default (traditionally)
- ✅ Synchronous loading
- ✅ Simple syntax
- ✅ Dynamic require
- ✅ Copies of exports

### Best Practices
1. Use ES6 modules for new projects
2. One module = one responsibility
3. Use barrel exports (index files)
4. Avoid circular dependencies
5. Use default exports for main functionality
6. Use named exports for utilities

### Key Takeaways
- Modules help organize and reuse code
- ES6 modules are the future (use them!)
- CommonJS still relevant for Node.js
- Dynamic imports enable code splitting
- Understanding both systems is important for interviews

Master modules to write maintainable and scalable JavaScript! 🚀
