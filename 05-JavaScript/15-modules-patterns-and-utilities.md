# Modules, Patterns, and Utilities

This guide covers the JavaScript module systems (ES Modules and CommonJS), common utility techniques (memoization, debounce/throttle), regular expressions, the ES5 vs ES6 evolution, error handling, and a few additional fundamentals (constructor functions, DOM, BOM).

## ES Modules

**Modules** are reusable pieces of code that can be exported from one file and imported into another. They help organize code, prevent naming conflicts, and manage dependencies.

### ES6 Modules (ESM)

Modern JavaScript uses **ES6 module syntax** with `import` and `export`.

#### Named Exports

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

Importing named exports:

```javascript
// app.js
import { PI, add, subtract, Calculator } from './math.js';

console.log(PI);              // 3.14159
console.log(add(5, 3));       // 8
console.log(subtract(10, 4)); // 6

const calc = new Calculator();
console.log(calc.multiply(2, 3)); // 6
```

Import with aliases:

```javascript
import { add as sum, subtract as diff } from './math.js';

console.log(sum(5, 3));   // 8
console.log(diff(10, 4)); // 6
```

Import everything:

```javascript
import * as math from './math.js';

console.log(math.PI);
console.log(math.add(5, 3));
console.log(math.subtract(10, 4));
```

#### Default Exports

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

Importing a default export:

```javascript
// app.js
import User from './user.js';

const user = new User('Alice');
user.greet(); // "Hello, I'm Alice"
```

You can name it anything:

```javascript
import MyUser from './user.js'; // Different name
import Person from './user.js'; // Another name

const user = new MyUser('Bob');
```

#### Mixing Named and Default Exports

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

Importing:

```javascript
// app.js
import logger, { VERSION, formatDate } from './utils.js';

logger('Application started'); // [LOG]: Application started
console.log(VERSION);          // 1.0.0
console.log(formatDate(new Date()));
```

### Re-exporting

Export imports from other modules (barrel exports).

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

Usage:

```javascript
// app.js
import { Circle, Rectangle } from './shapes/index.js';

const circle = new Circle(5);
const rect = new Rectangle(10, 20);
```

### CommonJS (Node.js)

Traditional Node.js module system using `require` and `module.exports`.

Exporting:

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

Importing:

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

### ES6 vs CommonJS

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

ES6 modules example (imports are hoisted):

```javascript
// ES6: Imports are hoisted
import { user } from './user.js'; // Works even before this line runs
console.log(user);

export const data = 'some data';
```

CommonJS example (imports execute when reached):

```javascript
// CommonJS: Imports are executed when reached
console.log('Start');

const user = require('./user.js'); // Loaded here
console.log(user);

console.log('End');
```

### Dynamic Imports

ES6 dynamic import — load modules on demand (returns a Promise):

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

CommonJS dynamic require:

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

### Module Patterns (Pre-ES6)

Revealing Module Pattern:

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

Singleton Pattern:

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

### Module Best Practices

#### 1. One Module, One Purpose

```javascript
// Bad: Too many responsibilities
export function validateUser() {}
export function fetchData() {}
export function renderUI() {}
export function handleError() {}

// Good: Separate concerns
// validation.js
export function validateUser() {}

// api.js
export function fetchData() {}

// ui.js
export function renderUI() {}

// errors.js
export function handleError() {}
```

#### 2. Barrel Exports (Index Files)

```javascript
// components/index.js
export { Button } from './Button.js';
export { Input } from './Input.js';
export { Form } from './Form.js';

// Usage (cleaner imports)
import { Button, Input, Form } from './components';
```

#### 3. Avoid Circular Dependencies

```javascript
// Bad: Circular dependency
// a.js
import { b } from './b.js';
export const a = b + 1;

// b.js
import { a } from './a.js';
export const b = a + 1;

// Good: Refactor to avoid circular dependency
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

#### 4. Default Export for Main Functionality

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

### Real-World Examples

#### Example 1: API Module

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

#### Example 2: Utilities Module

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

### Node.js: Enabling ES6 Modules

Method 1: `.mjs` Extension

```javascript
// math.mjs
export function add(a, b) {
  return a + b;
}

// app.mjs
import { add } from './math.mjs';
console.log(add(2, 3));
```

Method 2: package.json

```json
{
  "type": "module",
  "name": "my-app"
}
```

Now all `.js` files are treated as ES6 modules.

Method 3: Using both systems

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

### Module Interview Questions

#### Q1: What's the difference between named and default exports?

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

#### Q2: What happens if you have circular dependencies?

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

#### Q3: How do you conditionally import a module?

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

### Module Summary

ES6 Modules:
- Modern standard
- Static analysis (tree shaking)
- Asynchronous loading
- Better for browsers
- Live bindings (exports are references)

CommonJS:
- Node.js default (traditionally)
- Synchronous loading
- Simple syntax
- Dynamic require
- Copies of exports

## Memoization

**Memoization** is an optimization technique where the result of a function is **cached** based on its input arguments. If the same inputs occur again, the cached result is returned instead of recalculating.

### How Memoization Works

When the function is called:
- Check if the result exists in cache for the given arguments.
- If yes → return cached result.
- If no → compute, store in cache, then return.

```javascript
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

### Where Memoization is Used

#### 1. Performance Optimization

For expensive calculations (e.g., Fibonacci, factorial).

```javascript
function fibonacci(n, memo = {}) {
  if (n <= 1) return n;
  if (memo[n]) return memo[n];
  return memo[n] = fibonacci(n - 1, memo) + fibonacci(n - 2, memo);
}
```

#### 2. React Hooks (`useMemo`, `useCallback`)

```javascript
import React, { useMemo } from "react";

function App({ numbers }) {
  const total = useMemo(() => {
    console.log("Calculating sum...");
    return numbers.reduce((a, b) => a + b, 0);
  }, [numbers]);

  return <div>Total: {total}</div>;
}
```

#### 3. Data Fetching / API Caching

```javascript
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

#### 4. Functional Utilities

Lodash provides `_.memoize`.

```javascript
const _ = require("lodash");

const add = (a, b) => {
  console.log("Calculating...");
  return a + b;
};

const memoizedAdd = _.memoize(add);
```

### Benefits & Drawbacks

Benefits:
- Faster performance by avoiding recomputation
- Useful in recursion and repeated operations
- Optimizes UI rendering (React, Vue)

Drawbacks:
- Extra memory usage for cache
- Cache invalidation is tricky
- Works best with **pure functions**

## Debounce & Throttle

**Debouncing** and **throttling** are performance optimization techniques used to control how many times a function executes over time, especially for events that fire rapidly (scrolling, resizing, typing, etc.).

### Debouncing

**Debouncing** ensures a function is only executed **after a certain amount of time has passed** since it was last called. If the function is called again before the delay expires, the timer resets.

**Use Case:** Wait for user to stop typing before making an API call.

How it works:

```text
User types: a → b → c → d → e (rapid succession)
Debounced function: Called only ONCE after user stops typing (after delay)
```

Basic implementation:

```javascript
function debounce(func, delay) {
  let timeoutId;

  return function(...args) {
    // Clear previous timer
    clearTimeout(timeoutId);

    // Set new timer
    timeoutId = setTimeout(() => {
      func.apply(this, args);
    }, delay);
  };
}
```

Example: Search input:

```javascript
const searchInput = document.getElementById('search');

// Without debounce - API called on every keystroke
searchInput.addEventListener('input', (e) => {
  searchAPI(e.target.value); // Called too many times!
});

// With debounce - API called only after user stops typing
const debouncedSearch = debounce((query) => {
  searchAPI(query);
}, 500);

searchInput.addEventListener('input', (e) => {
  debouncedSearch(e.target.value);
});

function searchAPI(query) {
  console.log('Searching for:', query);
  // fetch(`/api/search?q=${query}`)...
}
```

Benefits:
- User types "JavaScript" (10 keystrokes)
- Without debounce: 10 API calls
- With debounce (500ms): 1 API call (after 500ms of no typing)

Advanced debounce with immediate option:

```javascript
function debounce(func, delay, immediate = false) {
  let timeoutId;

  return function(...args) {
    const callNow = immediate && !timeoutId;

    clearTimeout(timeoutId);

    timeoutId = setTimeout(() => {
      timeoutId = null;
      if (!immediate) {
        func.apply(this, args);
      }
    }, delay);

    if (callNow) {
      func.apply(this, args);
    }
  };
}

// Execute immediately on first call, then debounce
const handleClick = debounce(() => {
  console.log('Button clicked');
}, 1000, true);
```

### Throttling

**Throttling** ensures a function is executed at most **once in a specified time period**, no matter how many times it's called.

**Use Case:** Limit how often a scroll event handler fires.

How it works:

```text
User scrolls continuously for 5 seconds
Throttled function (1000ms): Called every 1 second (5 times total)
```

Basic implementation:

```javascript
function throttle(func, limit) {
  let inThrottle;

  return function(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;

      setTimeout(() => {
        inThrottle = false;
      }, limit);
    }
  };
}
```

Example: Scroll event:

```javascript
// Without throttle - fires hundreds of times per second
window.addEventListener('scroll', () => {
  console.log('Scroll position:', window.scrollY);
  updateProgressBar(); // Heavy operation
});

// With throttle - fires at most once per 200ms
const throttledScroll = throttle(() => {
  console.log('Scroll position:', window.scrollY);
  updateProgressBar();
}, 200);

window.addEventListener('scroll', throttledScroll);

function updateProgressBar() {
  const scrolled = (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100;
  document.getElementById('progress').style.width = scrolled + '%';
}
```

Benefits:
- User scrolls for 2 seconds
- Without throttle: ~200+ calls
- With throttle (200ms): ~10 calls

Advanced throttle with leading and trailing options:

```javascript
function throttle(func, limit, options = {}) {
  let timeout;
  let previous = 0;

  const { leading = true, trailing = true } = options;

  return function(...args) {
    const now = Date.now();

    if (!previous && !leading) {
      previous = now;
    }

    const remaining = limit - (now - previous);

    if (remaining <= 0 || remaining > limit) {
      if (timeout) {
        clearTimeout(timeout);
        timeout = null;
      }
      previous = now;
      func.apply(this, args);
    } else if (!timeout && trailing) {
      timeout = setTimeout(() => {
        previous = leading ? Date.now() : 0;
        timeout = null;
        func.apply(this, args);
      }, remaining);
    }
  };
}

// Execute on leading edge only
const handleScroll = throttle(() => {
  console.log('Scrolling');
}, 1000, { leading: true, trailing: false });

// Execute on trailing edge only
const handleResize = throttle(() => {
  console.log('Resizing');
}, 1000, { leading: false, trailing: true });
```

### Debounce vs Throttle

| Aspect | Debounce | Throttle |
|--------|----------|----------|
| **Execution** | After inactivity period | At regular intervals |
| **First call** | Delayed (or immediate with option) | Immediate (with leading) |
| **Subsequent calls** | Reset timer | Ignored until interval passes |
| **Best for** | Waiting for user to finish action | Limiting execution rate |
| **Example** | Search input, form validation | Scroll, resize, mouse move |

Visual comparison:

```text
User Action: |||||||||||||||||||||||||| (continuous events)

Debounce:    .......................... ✓ (executes once at end)

Throttle:    ✓...✓...✓...✓...✓...✓     (executes at intervals)
```

### Real-World Use Cases

#### 1. Search/Autocomplete (Debounce)

```javascript
const searchBox = document.getElementById('search');

const debouncedSearch = debounce(async (query) => {
  if (query.length < 3) return;

  try {
    const response = await fetch(`/api/autocomplete?q=${query}`);
    const suggestions = await response.json();
    displaySuggestions(suggestions);
  } catch (error) {
    console.error('Search failed:', error);
  }
}, 300);

searchBox.addEventListener('input', (e) => {
  debouncedSearch(e.target.value);
});

function displaySuggestions(suggestions) {
  const list = document.getElementById('suggestions');
  list.innerHTML = suggestions
    .map(s => `<li>${s}</li>`)
    .join('');
}
```

#### 2. Window Resize (Debounce)

```javascript
const debouncedResize = debounce(() => {
  console.log('Window resized to:', window.innerWidth, window.innerHeight);

  // Heavy layout calculations
  recalculateLayout();
  adjustChartDimensions();
}, 250);

window.addEventListener('resize', debouncedResize);
```

#### 3. Infinite Scroll (Throttle)

```javascript
const throttledScroll = throttle(() => {
  const scrollPosition = window.innerHeight + window.scrollY;
  const documentHeight = document.documentElement.scrollHeight;

  // Load more when user is 200px from bottom
  if (documentHeight - scrollPosition < 200) {
    loadMoreContent();
  }
}, 200);

window.addEventListener('scroll', throttledScroll);

async function loadMoreContent() {
  const content = await fetch('/api/more-content');
  appendContent(content);
}
```

#### 4. Button Click Prevention (Debounce)

```javascript
const saveButton = document.getElementById('save');

// Prevent multiple submissions
const debouncedSave = debounce(async () => {
  saveButton.disabled = true;
  saveButton.textContent = 'Saving...';

  try {
    await saveData();
    showSuccess('Data saved!');
  } catch (error) {
    showError('Save failed');
  } finally {
    saveButton.disabled = false;
    saveButton.textContent = 'Save';
  }
}, 1000, true); // immediate = true for first click

saveButton.addEventListener('click', debouncedSave);
```

#### 5. Mouse Move Tracking (Throttle)

```javascript
const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');

const throttledDraw = throttle((x, y) => {
  ctx.fillStyle = 'blue';
  ctx.fillRect(x - 2, y - 2, 4, 4);
}, 50);

canvas.addEventListener('mousemove', (e) => {
  const rect = canvas.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const y = e.clientY - rect.top;

  throttledDraw(x, y);
});
```

#### 6. API Rate Limiting (Throttle)

```javascript
class APIClient {
  constructor() {
    // Allow 10 requests per second
    this.makeRequest = throttle(this._makeRequest.bind(this), 100);
  }

  async _makeRequest(endpoint, options) {
    const response = await fetch(endpoint, options);
    return response.json();
  }
}

const client = new APIClient();

// Multiple rapid calls will be throttled
for (let i = 0; i < 50; i++) {
  client.makeRequest('/api/data');
}
```

### Modern Alternatives

Using Lodash:

```javascript
import { debounce, throttle } from 'lodash';

// Debounce
const debouncedFn = debounce((value) => {
  console.log(value);
}, 500);

// Throttle
const throttledFn = throttle((value) => {
  console.log(value);
}, 1000);

// Cancel debounce
debouncedFn.cancel();

// Flush debounce (execute immediately)
debouncedFn.flush();
```

Using Intersection Observer (for scroll) instead of throttling scroll events:

```javascript
const observer = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      loadMoreContent();
    }
  });
}, {
  rootMargin: '200px' // Trigger 200px before element is visible
});

const sentinel = document.getElementById('sentinel');
observer.observe(sentinel);
```

Using RequestAnimationFrame (for visual updates):

```javascript
let ticking = false;

function onScroll() {
  if (!ticking) {
    requestAnimationFrame(() => {
      updateProgressBar();
      ticking = false;
    });
    ticking = true;
  }
}

window.addEventListener('scroll', onScroll);
```

### Debounce & Throttle Interview Questions

#### Q1: Implement a basic debounce function

```javascript
function debounce(func, delay) {
  let timeoutId;

  return function(...args) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
      func.apply(this, args);
    }, delay);
  };
}

// Test
const log = debounce(() => console.log('Debounced!'), 1000);
log(); log(); log(); // Only last call executes after 1s
```

#### Q2: Implement a basic throttle function

```javascript
function throttle(func, limit) {
  let inThrottle;

  return function(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => inThrottle = false, limit);
    }
  };
}

// Test
const log = throttle(() => console.log('Throttled!'), 1000);
log(); log(); log(); // First call executes immediately, others ignored for 1s
```

#### Q3: When to use debounce vs throttle?

**Debounce:**
- Search inputs (wait for user to finish typing)
- Form validation (validate after user stops typing)
- Button clicks (prevent double submission)
- Window resize (recalculate layout after resize stops)

**Throttle:**
- Scroll events (update progress bar)
- Mouse move events (drawing, tracking)
- API rate limiting (limit requests per second)
- Game loop (limit frame updates)

### Performance Comparison

Without optimization:

```javascript
let count = 0;
window.addEventListener('scroll', () => {
  count++;
});

// After 10 seconds of scrolling: count could be 1000+
```

With debounce:

```javascript
let count = 0;
const debouncedHandler = debounce(() => {
  count++;
}, 500);
window.addEventListener('scroll', debouncedHandler);

// After 10 seconds of scrolling: count = 1-2
```

With throttle:

```javascript
let count = 0;
const throttledHandler = throttle(() => {
  count++;
}, 500);
window.addEventListener('scroll', throttledHandler);

// After 10 seconds of scrolling: count = ~20
```

Key takeaways:
1. Both improve performance by reducing function calls
2. Debounce waits for calm, throttle ensures regular pace
3. Choose based on use case: "wait until done" vs "limit rate"
4. Modern alternatives: Intersection Observer, requestAnimationFrame
5. Libraries like Lodash provide robust implementations

## Regular Expressions

**Regular Expressions** (regex or regexp) are patterns used to match character combinations in strings. They're powerful tools for searching text, validating input, extracting data, replacing text, and parsing strings.

### Creating Regular Expressions

```javascript
// 1. Literal notation (preferred for static patterns)
const regex1 = /pattern/flags;

// 2. Constructor (useful for dynamic patterns)
const regex2 = new RegExp('pattern', 'flags');

// Examples
const literal = /hello/i;
const constructor = new RegExp('hello', 'i');
```

### Regex Flags

| Flag | Name | Description |
|------|------|-------------|
| `g` | Global | Find all matches (not just first) |
| `i` | Ignore case | Case-insensitive matching |
| `m` | Multiline | `^` and `$` match line starts/ends |
| `s` | Dot all | `.` matches newlines too |
| `u` | Unicode | Treat pattern as Unicode |
| `y` | Sticky | Match from `lastIndex` position |

```javascript
const text = 'Hello World, hello universe';

// No flags: first match only, case-sensitive
console.log(text.match(/hello/));    // ["hello"]

// g flag: all matches
console.log(text.match(/hello/g));   // ["hello"]

// i flag: case-insensitive
console.log(text.match(/hello/i));   // ["Hello"]

// gi flags: all matches, case-insensitive
console.log(text.match(/hello/gi));  // ["Hello", "hello"]
```

### Basic Patterns

Literal characters:

```javascript
const regex = /cat/;

console.log(regex.test('cat'));        // true
console.log(regex.test('catch'));      // true
console.log(regex.test('dog'));        // false
```

Special characters need escaping: `. * + ? ^ $ { } [ ] ( ) | \`

```javascript
// Escape with backslash
const dotRegex = /\./;
console.log(dotRegex.test('file.txt')); // true

const dollarRegex = /\$100/;
console.log(dollarRegex.test('$100'));  // true
```

### Character Classes

```javascript
// [abc] - Match any character in brackets
const vowels = /[aeiou]/;
console.log(vowels.test('hello')); // true

// [^abc] - Match any character NOT in brackets
const consonants = /[^aeiou]/i;
console.log(consonants.test('hello')); // true (h, l, l)

// [a-z] - Match range
const lowercase = /[a-z]/;
console.log(lowercase.test('Hello')); // true (e, l, l, o)

// [A-Za-z0-9] - Multiple ranges
const alphanumeric = /[A-Za-z0-9]/;
console.log(alphanumeric.test('test123')); // true
```

Predefined character classes:

| Pattern | Equivalent | Description |
|---------|------------|-------------|
| `\d` | `[0-9]` | Any digit |
| `\D` | `[^0-9]` | Any non-digit |
| `\w` | `[A-Za-z0-9_]` | Word character |
| `\W` | `[^A-Za-z0-9_]` | Non-word character |
| `\s` | `[ \t\n\r\f\v]` | Whitespace |
| `\S` | `[^ \t\n\r\f\v]` | Non-whitespace |
| `.` | Any character | Except newline (unless `s` flag) |

```javascript
// \d - digits
const hasDigit = /\d/;
console.log(hasDigit.test('abc123')); // true

// \w - word characters
const word = /\w+/;
console.log(word.test('hello_world')); // true

// \s - whitespace
const hasSpace = /\s/;
console.log(hasSpace.test('hello world')); // true

// . - any character
const anyChar = /.at/;
console.log(anyChar.test('cat')); // true
console.log(anyChar.test('bat')); // true
console.log(anyChar.test('mat')); // true
```

### Quantifiers

| Quantifier | Description |
|------------|-------------|
| `*` | 0 or more |
| `+` | 1 or more |
| `?` | 0 or 1 (optional) |
| `{n}` | Exactly n |
| `{n,}` | n or more |
| `{n,m}` | Between n and m |

```javascript
// * - 0 or more
const colors = /colou*r/;
console.log(colors.test('color'));   // true
console.log(colors.test('colour'));  // true
console.log(colors.test('colouur')); // true

// + - 1 or more
const digits = /\d+/;
console.log(digits.test('123'));     // true
console.log(digits.test('abc'));     // false

// ? - optional
const optional = /colou?r/;
console.log(optional.test('color'));  // true
console.log(optional.test('colour')); // true

// {n} - exactly n
const threeDigits = /\d{3}/;
console.log(threeDigits.test('12'));  // false
console.log(threeDigits.test('123')); // true

// {n,m} - range
const zipCode = /\d{5,9}/;
console.log(zipCode.test('12345'));     // true
console.log(zipCode.test('123456789')); // true
```

Greedy vs lazy quantifiers:

```javascript
const text = '<div>Hello</div><div>World</div>';

// Greedy (default): matches as much as possible
const greedy = /<div>.*<\/div>/;
console.log(text.match(greedy));
// ["<div>Hello</div><div>World</div>"]

// Lazy (add ?): matches as little as possible
const lazy = /<div>.*?<\/div>/;
console.log(text.match(lazy));
// ["<div>Hello</div>"]

// With global flag
console.log(text.match(/<div>.*?<\/div>/g));
// ["<div>Hello</div>", "<div>World</div>"]
```

### Anchors

| Anchor | Description |
|--------|-------------|
| `^` | Start of string (or line with `m` flag) |
| `$` | End of string (or line with `m` flag) |
| `\b` | Word boundary |
| `\B` | Not a word boundary |

```javascript
// ^ - start
const startsWithHello = /^Hello/;
console.log(startsWithHello.test('Hello World')); // true
console.log(startsWithHello.test('Say Hello'));   // false

// $ - end
const endsWithWorld = /World$/;
console.log(endsWithWorld.test('Hello World')); // true
console.log(endsWithWorld.test('World Hello')); // false

// \b - word boundary
const exactWord = /\bcat\b/;
console.log(exactWord.test('cat'));     // true
console.log(exactWord.test('catch'));   // false
console.log(exactWord.test('the cat')); // true

// Exact match (start and end)
const exactEmail = /^[a-z]+@[a-z]+\.[a-z]+$/;
console.log(exactEmail.test('user@example.com'));       // true
console.log(exactEmail.test('user@example.com extra')); // false
```

### Groups and Capturing

Capturing groups `()`:

```javascript
const dateRegex = /(\d{4})-(\d{2})-(\d{2})/;
const match = '2024-03-15'.match(dateRegex);

console.log(match[0]); // "2024-03-15" (full match)
console.log(match[1]); // "2024" (first group)
console.log(match[2]); // "03" (second group)
console.log(match[3]); // "15" (third group)
```

Non-capturing groups `(?:)`:

```javascript
// Capture
const withCapture = /(hello) (world)/;
console.log('hello world'.match(withCapture));
// ["hello world", "hello", "world"]

// Non-capture (for grouping without capturing)
const noCapture = /(?:hello) (world)/;
console.log('hello world'.match(noCapture));
// ["hello world", "world"]
```

Named capturing groups `(?<name>)`:

```javascript
const dateRegex = /(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})/;
const match = '2024-03-15'.match(dateRegex);

console.log(match.groups.year);  // "2024"
console.log(match.groups.month); // "03"
console.log(match.groups.day);   // "15"
```

### Alternation

OR operator `|`:

```javascript
const fileExt = /\.(jpg|png|gif)$/;

console.log(fileExt.test('photo.jpg')); // true
console.log(fileExt.test('image.png')); // true
console.log(fileExt.test('file.txt'));  // false
```

### Lookahead and Lookbehind

Positive lookahead `(?=)` — match if followed by pattern (doesn't include in match):

```javascript
// Match number followed by "px"
const pxValue = /\d+(?=px)/;

console.log('10px'.match(pxValue));  // ["10"]
console.log('10em'.match(pxValue));  // null
```

Negative lookahead `(?!)` — match if NOT followed by pattern:

```javascript
// Match number NOT followed by "px"
const notPx = /\d+(?!px)/;

console.log('10em'.match(notPx));  // ["10"]
console.log('10px'.match(notPx));  // null
```

Positive lookbehind `(?<=)` — match if preceded by pattern:

```javascript
// Match number preceded by "$"
const price = /(?<=\$)\d+/;

console.log('$100'.match(price));  // ["100"]
console.log('100'.match(price));   // null
```

Negative lookbehind `(?<!)` — match if NOT preceded by pattern:

```javascript
// Match number NOT preceded by "$"
const notPrice = /(?<!\$)\d+/;

console.log('100'.match(notPrice));  // ["100"]
console.log('$100'.match(notPrice)); // null
```

### String Methods with Regex

`test()` — boolean check:

```javascript
const regex = /hello/i;

console.log(regex.test('Hello World')); // true
console.log(regex.test('Goodbye'));     // false
```

`match()` — find matches:

```javascript
const text = 'Contact: john@email.com and jane@email.com';
const emailRegex = /\w+@\w+\.\w+/g;

const matches = text.match(emailRegex);
console.log(matches);
// ["john@email.com", "jane@email.com"]
```

`matchAll()` — get all match details:

```javascript
const text = 'test1 test2 test3';
const regex = /test(\d)/g;

const matches = [...text.matchAll(regex)];
matches.forEach(match => {
  console.log(`Full: ${match[0]}, Group: ${match[1]}`);
});
// Full: test1, Group: 1
// Full: test2, Group: 2
// Full: test3, Group: 3
```

`search()` — find index:

```javascript
const text = 'Hello World';

console.log(text.search(/World/));   // 6
console.log(text.search(/Goodbye/)); // -1
```

`replace()` — replace matches:

```javascript
const text = 'Hello World';

// Simple replacement
console.log(text.replace(/World/, 'Universe'));
// "Hello Universe"

// With function
console.log(text.replace(/\w+/g, match => match.toUpperCase()));
// "HELLO WORLD"

// With groups
const date = '2024-03-15';
const formatted = date.replace(/(\d{4})-(\d{2})-(\d{2})/, '$2/$3/$1');
console.log(formatted); // "03/15/2024"
```

`replaceAll()` — replace all matches:

```javascript
const text = 'cat cat cat';

console.log(text.replace(/cat/, 'dog'));     // "dog cat cat"
console.log(text.replaceAll(/cat/g, 'dog')); // "dog dog dog"
```

`split()` — split by pattern:

```javascript
const text = 'one,two;three:four';

console.log(text.split(/[,;:]/));
// ["one", "two", "three", "four"]

const words = 'hello   world    test';
console.log(words.split(/\s+/));
// ["hello", "world", "test"]
```

### Common Patterns

Email validation:

```javascript
const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

console.log(emailRegex.test('user@example.com')); // true
console.log(emailRegex.test('invalid.email'));    // false
```

Phone number:

```javascript
// US phone: (123) 456-7890 or 123-456-7890
const phoneRegex = /^\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/;

console.log(phoneRegex.test('(123) 456-7890')); // true
console.log(phoneRegex.test('123-456-7890'));   // true
console.log(phoneRegex.test('1234567890'));     // true
```

URL:

```javascript
const urlRegex = /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b/;

console.log(urlRegex.test('https://example.com'));    // true
console.log(urlRegex.test('http://www.example.com')); // true
```

Password strength:

```javascript
// At least 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char
const strongPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

console.log(strongPassword.test('Password123!')); // true
console.log(strongPassword.test('weak'));         // false
```

Credit card:

```javascript
// Basic pattern (spaces optional)
const ccRegex = /^\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}$/;

console.log(ccRegex.test('1234 5678 9012 3456')); // true
console.log(ccRegex.test('1234-5678-9012-3456')); // true
```

Hex color:

```javascript
const hexColor = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;

console.log(hexColor.test('#FFF'));    // true
console.log(hexColor.test('#FF5733')); // true
console.log(hexColor.test('#GGG'));    // false
```

IPv4 address:

```javascript
const ipv4 = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

console.log(ipv4.test('192.168.1.1')); // true
console.log(ipv4.test('256.1.1.1'));   // false
```

### Performance Tips

1. Compile once, use many times:

```javascript
// Bad: Compiles regex every iteration
for (let i = 0; i < 1000; i++) {
  if (/test/.test(strings[i])) {
    // ...
  }
}

// Good: Compile once
const regex = /test/;
for (let i = 0; i < 1000; i++) {
  if (regex.test(strings[i])) {
    // ...
  }
}
```

2. Use specific patterns:

```javascript
// Slower: Too general
const slow = /.*@.*/;

// Faster: More specific
const fast = /\w+@\w+\.\w+/;
```

3. Avoid catastrophic backtracking:

```javascript
// Dangerous: Can hang on long strings
const dangerous = /(a+)+b/;

// Better: More efficient
const safe = /a+b/;
```

### Regex Interview Questions

#### Q1: Extract all numbers from a string

```javascript
function extractNumbers(str) {
  return str.match(/\d+/g) || [];
}

console.log(extractNumbers('Price: $123.45, Qty: 10'));
// ["123", "45", "10"]
```

#### Q2: Validate username (alphanumeric, 3-16 chars, underscore allowed)

```javascript
const usernameRegex = /^[a-zA-Z0-9_]{3,16}$/;

console.log(usernameRegex.test('user_123'));  // true
console.log(usernameRegex.test('ab'));        // false (too short)
console.log(usernameRegex.test('user-name')); // false (dash not allowed)
```

#### Q3: Replace multiple spaces with single space

```javascript
const text = 'hello    world     test';
console.log(text.replace(/\s+/g, ' '));
// "hello world test"
```

#### Q4: Check if string contains only digits

```javascript
const onlyDigits = /^\d+$/;

console.log(onlyDigits.test('12345'));  // true
console.log(onlyDigits.test('123a45')); // false
```

#### Q5: Extract domain from email

```javascript
function getDomain(email) {
  const match = email.match(/@(.+)$/);
  return match ? match[1] : null;
}

console.log(getDomain('user@example.com')); // "example.com"
```

### Regex Best Practices & Tips

- Escape special characters
- Use specific patterns
- Compile regex once
- Test edge cases
- Avoid catastrophic backtracking
- Don't use regex for complex parsing (HTML, JSON)
- Know common patterns (email, phone, URL)
- Understand greedy vs lazy matching
- Be familiar with lookahead/lookbehind

## ES5 vs ES6

- **ES5 (ECMAScript 5)** → Released in **2009**, supported in all browsers.
- **ES6 (ECMAScript 2015)** → Released in **2015**, introduced many modern features.

### 1. Variable Declaration

```javascript
// ES5
var x = 10;

// ES6
let y = 20;
const z = 30;
```

### 2. Functions

```javascript
// ES5
var add = function(a, b) {
  return a + b;
};

// ES6
const add = (a, b) => a + b;
```

### 3. Template Literals

```javascript
// ES5
var name = "Alice";
console.log("Hello " + name + "!");

// ES6
const name = "Alice";
console.log(`Hello ${name}!`);
```

### 4. Default Parameters

```javascript
// ES5
function greet(name) {
  name = name || "Guest";
  console.log("Hello " + name);
}

// ES6
function greet(name = "Guest") {
  console.log(`Hello ${name}`);
}
```

### 5. Destructuring (ES6 only)

```javascript
const arr = [1, 2, 3];
const [a, b] = arr; // a=1, b=2

const obj = { name: "Alice", age: 25 };
const { name, age } = obj;
```

### 6. Modules

```javascript
// ES5 → No native support (used CommonJS/RequireJS)

// ES6
export const pi = 3.14;
import { pi } from "./math.js";
```

### 7. Classes

```javascript
// ES5
function Person(name) {
  this.name = name;
}
Person.prototype.greet = function() {
  console.log("Hello " + this.name);
};

// ES6
class Person {
  constructor(name) {
    this.name = name;
  }
  greet() {
    console.log(`Hello ${this.name}`);
  }
}
```

### 8. Promises

```javascript
// ES5 (callback hell)
doSomething(function(result) {
  doSomethingElse(result, function(finalResult) {
    console.log(finalResult);
  });
});

// ES6 (promises)
doSomething()
  .then(result => doSomethingElse(result))
  .then(finalResult => console.log(finalResult));
```

### 9. Other ES6 Features

- Rest & Spread operators (`...`)
- For-of loop
- Symbols (new primitive type)
- Map, Set, WeakMap, WeakSet
- Generators (`function*`)

### Cheat Sheet

| Feature | ES5 | ES6 |
|---------|-----|-----|
| Variable declaration | `var` only | `let`, `const` |
| Functions | Regular | Arrow functions |
| Strings | Concatenation | Template literals |
| Params | Manual default (`||`) | Default parameters |
| Destructuring | No | Yes |
| Modules | No native support | `import` / `export` |
| Classes | Prototype-based | `class` keyword |
| Async | Callbacks | Promises (later async/await in ES2017) |
| Collections | Objects, Arrays | Map, Set, WeakMap, WeakSet |

Conclusion:
- **ES5** = Old JavaScript (still everywhere).
- **ES6** = Modern JavaScript → cleaner syntax, modules, better async, new data structures.

## Error Handling

An **error** occurs when something unexpected happens in your program that prevents it from running normally. JavaScript has different built-in error types, and also allows creating custom errors.

### Types of Errors

#### 1. Syntax Error

Occurs when code is written incorrectly and cannot be parsed.

```javascript
console.log("Hello";
// SyntaxError: missing ) after argument list
```

#### 2. Reference Error

Accessing a variable that does not exist.

```javascript
console.log(myVar);
// ReferenceError: myVar is not defined
```

#### 3. Type Error

Using a value in the wrong way.

```javascript
let num = 5;
num.toUpperCase();
// TypeError: num.toUpperCase is not a function
```

#### 4. Range Error

When a value is not in the valid range.

```javascript
let arr = new Array(-1);
// RangeError: Invalid array length
```

#### 5. URI Error

When URI handling functions are misused.

```javascript
decodeURIComponent("%");
// URIError: URI malformed
```

#### 6. Eval Error

Legacy error related to `eval()` (rare today).

### Error Handling Techniques

#### 1. try...catch

```javascript
try {
  let result = riskyFunction();
  console.log(result);
} catch (error) {
  console.error("Something went wrong:", error.message);
}
```

#### 2. finally

```javascript
try {
  console.log("Trying...");
  throw new Error("Oops!");
} catch (error) {
  console.log("Caught error:", error.message);
} finally {
  console.log("Always runs!");
}
```

#### 3. Custom Errors

```javascript
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

#### 4. Error Object Properties

```javascript
try {
  throw new TypeError("Wrong type!");
} catch (err) {
  console.log(err.name);    // TypeError
  console.log(err.message); // Wrong type!
  console.log(err.stack);   // Stack trace
}
```

#### 5. Error Handling in Async Code

Using Promises:

```javascript
fetch("invalid-url")
  .then(res => res.json())
  .catch(err => console.error("Fetch error:", err));
```

Using async/await:

```javascript
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

### Error Handling Best Practices

1. Catch errors gracefully.
2. Provide meaningful error messages.
3. Use custom error classes when needed.
4. Handle async errors with `.catch()` or `try...catch`.
5. Avoid overusing try...catch unnecessarily.
6. Log errors for debugging and monitoring.

Errors are inevitable, but handling them properly makes your app **robust, user-friendly, and secure**.

## Additional Topics

### Constructor Function

Constructor functions are used to create objects in JavaScript. Use them when you want to create multiple objects having similar properties and methods.

```javascript
function Person(name, age, gender) {
  this.name = name;
  this.age = age;
  this.gender = gender;
}

var person1 = new Person("Vivek", 76, "male");
console.log(person1);

var person2 = new Person("Courtney", 34, "female");
console.log(person2);
```

### What is the DOM?

- DOM stands for **Document Object Model**. The DOM is a programming interface for HTML and XML documents.
- When the browser tries to render an HTML document, it creates an object based on the HTML document called the DOM. Using this DOM, we can manipulate or change various elements inside the HTML document.

### Retrieving a Character at a Certain Index

The `charAt()` function of the JavaScript string finds a char element at the supplied index. The index number begins at 0 and continues up to n-1, where n is the string length. The index value must be positive, higher than, or the same as the string length.

### What is the BOM?

**Browser Object Model** is known as the BOM. It allows users to interact with the browser. A browser's initial object is a `window`. As a result, you may call all of the window's functions directly or by referencing the window. The `document`, `history`, `screen`, `navigator`, `location`, and other attributes are available in the window object.
