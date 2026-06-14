# Asynchronous JavaScript and the Event Loop

## The Event Loop

The **Event Loop** is the mechanism that allows JavaScript to perform non-blocking operations despite being single-threaded. It handles the execution of multiple chunks of your program over time, coordinating between the call stack, web APIs, and callback queues.

### JavaScript Runtime Components

#### 1. Call Stack

The **call stack** is where JavaScript keeps track of function calls. It operates on a LIFO (Last In, First Out) basis.

```javascript
function first() {
  console.log('First');
}

function second() {
  first();
  console.log('Second');
}

second();

// Call Stack Execution:
// 1. second() pushed
// 2. first() pushed
// 3. console.log('First') executed, first() popped
// 4. console.log('Second') executed, second() popped

// Output:
// First
// Second
```

#### 2. Web APIs (Browser APIs)

These are provided by the browser (or Node.js) and include:
- `setTimeout` / `setInterval`
- `fetch` / AJAX
- DOM events
- Promise callbacks

These APIs handle asynchronous operations outside the main JavaScript thread.

#### 3. Callback Queue (Task Queue / Macrotask Queue)

When async operations complete (like `setTimeout`), their callbacks are placed in the **callback queue**.

#### 4. Microtask Queue (Job Queue)

Higher priority queue for:
- Promise callbacks (`.then()`, `.catch()`, `.finally()`)
- `queueMicrotask()`
- `MutationObserver`

**Microtasks are processed before the next macrotask.**

### How the Event Loop Works

```text
┌───────────────────────────┐
│   Call Stack              │
│   (Execute sync code)     │
└───────────┬───────────────┘
            │
            ▼
    ┌──────────────┐
    │ Stack Empty? │──No──> Continue execution
    └───────┬──────┘
            │Yes
            ▼
┌───────────────────────────┐
│   Microtask Queue         │◄──── Process ALL microtasks
│   (Promises, etc.)        │
└───────────┬───────────────┘
            │
            ▼
┌───────────────────────────┐
│   Callback Queue          │◄──── Process ONE macrotask
│   (setTimeout, etc.)      │
└───────────┬───────────────┘
            │
            ▼
       Render (if needed)
            │
            └──> Back to top (Event Loop)
```

**Event Loop Process:**
1. Execute all synchronous code (call stack).
2. Check if call stack is empty.
3. Process **ALL** microtasks (promises, etc.).
4. Process **ONE** macrotask (setTimeout, setInterval).
5. Render updates (if necessary).
6. Repeat.

### Examples

#### Example 1: Basic Event Loop

```javascript
console.log('Start');

setTimeout(() => {
  console.log('Timeout');
}, 0);

console.log('End');

// Output:
// Start
// End
// Timeout
```

**Explanation:**
1. `console.log('Start')` executes immediately (call stack).
2. `setTimeout` is registered with Web API, callback goes to callback queue after 0ms.
3. `console.log('End')` executes immediately (call stack).
4. Call stack empty, event loop checks callback queue, `console.log('Timeout')` executes.

#### Example 2: Promises vs setTimeout

```javascript
console.log('Start');

setTimeout(() => {
  console.log('Timeout');
}, 0);

Promise.resolve().then(() => {
  console.log('Promise');
});

console.log('End');

// Output:
// Start
// End
// Promise
// Timeout
```

**Explanation:**
1. `console.log('Start')` - Call stack.
2. `setTimeout` - Callback goes to **callback queue** (macrotask).
3. `Promise.resolve().then()` - Callback goes to **microtask queue**.
4. `console.log('End')` - Call stack.
5. Call stack empty, process **all microtasks** first, `Promise` logs.
6. Then process macrotask, `Timeout` logs.

**Key Point:** Microtasks (Promises) have higher priority than macrotasks (setTimeout).

#### Example 3: Multiple Promises and setTimeout

```javascript
console.log('1');

setTimeout(() => console.log('2'), 0);

Promise.resolve()
  .then(() => console.log('3'))
  .then(() => console.log('4'));

Promise.resolve().then(() => {
  console.log('5');
  setTimeout(() => console.log('6'), 0);
});

console.log('7');

// Output:
// 1
// 7
// 3
// 5
// 4
// 2
// 6
```

**Explanation:**
1. `1` - Call stack.
2. `setTimeout` for `2` - Macrotask queue.
3. First promise chain (`3`, `4`) - Microtask queue.
4. Second promise (`5`) - Microtask queue.
5. `7` - Call stack.
6. Call stack empty, process all microtasks: `3`, `5`, `4`.
   - Inside `5`, `setTimeout` for `6` is registered (new macrotask).
7. Process macrotasks: `2`, then `6`.

#### Example 4: Nested setTimeout

```javascript
console.log('Start');

setTimeout(() => {
  console.log('Timeout 1');

  Promise.resolve().then(() => {
    console.log('Promise inside Timeout 1');
  });

  setTimeout(() => {
    console.log('Timeout 2');
  }, 0);
}, 0);

Promise.resolve().then(() => {
  console.log('Promise 1');
});

console.log('End');

// Output:
// Start
// End
// Promise 1
// Timeout 1
// Promise inside Timeout 1
// Timeout 2
```

**Explanation:**
1. Synchronous code: `Start`, `End`.
2. Microtasks: `Promise 1`.
3. First macrotask: `Timeout 1`.
   - Registers new promise (microtask): `Promise inside Timeout 1`.
   - Registers new setTimeout (macrotask): `Timeout 2`.
4. Process microtasks before next macrotask: `Promise inside Timeout 1`.
5. Next macrotask: `Timeout 2`.

#### Example 5: Async/Await

```javascript
console.log('Start');

async function asyncFunc() {
  console.log('Async start');
  await Promise.resolve();
  console.log('Async end');
}

asyncFunc();

Promise.resolve().then(() => {
  console.log('Promise');
});

console.log('End');

// Output:
// Start
// Async start
// End
// Async end
// Promise
```

**Explanation:**
1. `Start` - Synchronous.
2. `asyncFunc()` called, `Async start` logs.
3. `await Promise.resolve()` - Code after await goes to microtask queue.
4. `End` - Synchronous.
5. Microtasks: `Async end`, then `Promise`.

### Macrotasks vs Microtasks

**Macrotasks (Task Queue):**
- `setTimeout`
- `setInterval`
- `setImmediate` (Node.js)
- I/O operations
- UI rendering

**Microtasks (Job Queue):**
- `Promise.then/catch/finally`
- `async/await` (after await)
- `queueMicrotask()`
- `MutationObserver`
- `process.nextTick()` (Node.js - even higher priority)

**Priority:** Microtasks > Macrotasks

### Event Loop Interview Questions

#### Q1: What will be the output?

```javascript
console.log('1');

setTimeout(() => console.log('2'), 0);

Promise.resolve().then(() => console.log('3'));

console.log('4');
```

**Answer:**

```text
1
4
3
2
```

#### Q2: What will be the output?

```javascript
setTimeout(() => console.log('A'), 0);

Promise.resolve()
  .then(() => console.log('B'))
  .then(() => console.log('C'));

setTimeout(() => console.log('D'), 0);

console.log('E');
```

**Answer:**

```text
E
B
C
A
D
```

#### Q3: Explain the output

```javascript
async function async1() {
  console.log('async1 start');
  await async2();
  console.log('async1 end');
}

async function async2() {
  console.log('async2');
}

console.log('script start');

setTimeout(() => console.log('setTimeout'), 0);

async1();

new Promise((resolve) => {
  console.log('promise1');
  resolve();
}).then(() => {
  console.log('promise2');
});

console.log('script end');
```

**Answer:**

```text
script start
async1 start
async2
promise1
script end
async1 end
promise2
setTimeout
```

**Explanation:**
1. Synchronous: `script start`, `async1 start`, `async2`, `promise1`, `script end`.
2. Microtasks: `async1 end` (from await), `promise2`.
3. Macrotasks: `setTimeout`.

### Real-World Applications

#### 1. Debouncing

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

// Usage: Search input
const search = debounce((query) => {
  console.log('Searching for:', query);
}, 500);

input.addEventListener('input', (e) => search(e.target.value));
```

#### 2. API Rate Limiting

```javascript
class RateLimiter {
  constructor(maxRequests, timeWindow) {
    this.maxRequests = maxRequests;
    this.timeWindow = timeWindow;
    this.requests = [];
  }

  async makeRequest(apiCall) {
    // Clean old requests
    const now = Date.now();
    this.requests = this.requests.filter(
      time => now - time < this.timeWindow
    );

    if (this.requests.length >= this.maxRequests) {
      const oldestRequest = this.requests[0];
      const waitTime = this.timeWindow - (now - oldestRequest);

      await new Promise(resolve => setTimeout(resolve, waitTime));
      return this.makeRequest(apiCall);
    }

    this.requests.push(now);
    return apiCall();
  }
}
```

#### 3. Task Scheduling

```javascript
class TaskScheduler {
  constructor() {
    this.tasks = [];
  }

  addTask(task, priority = 0) {
    if (priority === 'high') {
      // Use microtask for high priority
      queueMicrotask(task);
    } else {
      // Use macrotask for normal priority
      setTimeout(task, 0);
    }
  }
}

const scheduler = new TaskScheduler();

scheduler.addTask(() => console.log('Low priority'), 'low');
scheduler.addTask(() => console.log('High priority'), 'high');

// Output: High priority, Low priority
```

### Common Pitfalls

#### 1. Infinite Loop with Promises

```javascript
// This will block the browser
Promise.resolve().then(function loop() {
  console.log('Looping');
  return Promise.resolve().then(loop);
});

// Microtasks keep getting added, macrotasks never execute
```

#### 2. setTimeout Delays

```javascript
// Not guaranteed to run after exactly 1000ms
setTimeout(() => console.log('1 second'), 1000);

// Heavy synchronous work
for (let i = 0; i < 1000000000; i++) {
  // Blocks event loop
}

// setTimeout callback waits until call stack is clear
```

#### 3. Promise vs setTimeout Order

```javascript
// Common mistake: expecting setTimeout(0) to run before Promise
setTimeout(() => console.log('Timeout'), 0);
Promise.resolve().then(() => console.log('Promise'));

// Output: Promise, Timeout (not Timeout, Promise)
```

### Event Loop Summary

Key concepts:
1. **JavaScript is single-threaded** but can handle async operations via the event loop.
2. **Call stack** executes synchronous code.
3. **Web APIs** handle async operations (setTimeout, fetch, etc.).
4. **Microtasks** (Promises) have **higher priority** than **macrotasks** (setTimeout).
5. Event loop continuously checks: Call Stack → Microtask Queue → Callback Queue.

Event loop order:

```text
1. Execute all synchronous code (call stack)
2. Process ALL microtasks (Promise callbacks)
3. Process ONE macrotask (setTimeout callback)
4. Render (if needed)
5. Repeat
```

Interview tips:
- Always remember: **Microtasks before Macrotasks**.
- `await` puts code after it in the microtask queue.
- `setTimeout(fn, 0)` doesn't mean immediate execution.
- Heavy synchronous code blocks the event loop.

---

## Promises & Async/Await

A **Promise** in JavaScript is an **object that represents the eventual completion (or failure) of an asynchronous operation**. It acts like a **placeholder** for a value that will be available **in the future**.

Think of it as a **food delivery order**:
- You place an order (async task starts).
- You get a receipt (Promise object).
- Later, the food is either delivered (**fulfilled**) or the restaurant says sorry (**rejected**).

### States of a Promise

1. **Pending** - Initial state, waiting.
2. **Fulfilled** - Operation completed successfully (`resolve`).
3. **Rejected** - Operation failed (`reject`).

### Creating a Promise

```javascript
const myPromise = new Promise((resolve, reject) => {
  let success = true;

  setTimeout(() => {
    if (success) {
      resolve("Task completed!");
    } else {
      reject("Something went wrong.");
    }
  }, 2000);
});
```

### Consuming a Promise

Using `.then()`, `.catch()`, `.finally()`:

```javascript
myPromise
  .then(result => console.log(result))    // if resolved
  .catch(error => console.error(error))   // if rejected
  .finally(() => console.log("Done"));    // always runs
```

### Async/Await (Modern Way)

```javascript
async function runTask() {
  try {
    const result = await myPromise;
    console.log(result); // Task completed!
  } catch (error) {
    console.error(error);
  } finally {
    console.log("Done");
  }
}
runTask();
```

### Real-World Example (Fetching Data)

With Promises:

```javascript
fetch("https://jsonplaceholder.typicode.com/posts/1")
  .then(response => response.json())
  .then(data => console.log("Post:", data))
  .catch(err => console.error("Error fetching:", err));
```

With Async/Await:

```javascript
async function getPost() {
  try {
    const response = await fetch("https://jsonplaceholder.typicode.com/posts/1");
    const data = await response.json();
    console.log("Post:", data);
  } catch (err) {
    console.error("Error fetching:", err);
  }
}
getPost();
```

### Why Promises Are Important

1. Avoids **callback hell** (messy nested callbacks).
2. Makes async code look **cleaner**.
3. Powers modern features like the **fetch API**, **async/await**, **React data fetching**, etc.

### Summary

A **Promise** is a way to handle asynchronous tasks in JavaScript.
- It has **3 states**: pending, fulfilled, rejected.
- You consume it with `.then()`, `.catch()`, `.finally()` or `async/await`.
- Promises make async code easier, cleaner, and more reliable.

---

## Callbacks

A **callback** is a function that will be executed after another function gets executed. In JavaScript, functions are treated as first-class citizens: they can be used as an argument of another function, can be returned by another function, and can be used as a property of an object.

```javascript
function divideByHalf(sum){
  console.log(Math.floor(sum / 2));
}

function multiplyBy2(sum){
  console.log(sum * 2);
}

function operationOnSum(num1, num2, operation){
  var sum = num1 + num2;
  operation(sum);
}

operationOnSum(3, 3, divideByHalf); // Outputs 3

operationOnSum(5, 5, multiplyBy2); // Outputs 20
```

---

## Generator Functions

A **generator function** is a special type of function that can **pause its execution** and later **resume from where it left off**. They are defined using the `function*` syntax.

Analogy: Like watching a **TV series** - you can **pause after an episode** and **resume later**, unlike a movie (normal function) that runs all at once.

### Syntax

```javascript
function* generatorFunction() {
  yield 1;
  yield 2;
  yield 3;
}
```

- `yield` pauses the function and returns a value.
- When called, a generator returns an **iterator object**.

### Using a Generator

```javascript
function* numbers() {
  yield 10;
  yield 20;
  yield 30;
}

const gen = numbers();

console.log(gen.next()); // { value: 10, done: false }
console.log(gen.next()); // { value: 20, done: false }
console.log(gen.next()); // { value: 30, done: false }
console.log(gen.next()); // { value: undefined, done: true }
```

- Each `next()` resumes execution until the next `yield`.
- When done, `done: true` is returned.

### Real-World Uses of Generators

#### 1. Lazy Evaluation

Generate values only when needed.

```javascript
function* countUpTo(limit) {
  let i = 1;
  while (i <= limit) {
    yield i++;
  }
}

for (let num of countUpTo(5)) {
  console.log(num);
}
// 1, 2, 3, 4, 5
```

#### 2. Infinite Sequences

```javascript
function* infiniteCounter() {
  let i = 0;
  while (true) {
    yield i++;
  }
}

const counter = infiniteCounter();
console.log(counter.next().value); // 0
console.log(counter.next().value); // 1
console.log(counter.next().value); // 2
```

#### 3. Asynchronous Programming (before async/await)

Generators were once used with libraries like `co` for async tasks.

```javascript
function* asyncTask() {
  const result = yield fetch("https://jsonplaceholder.typicode.com/posts/1");
  console.log(result);
}
```

### Difference from Normal Functions

| Normal Function | Generator Function |
|-----------------|--------------------|
| Runs start to finish | Can pause (`yield`) and resume |
| Returns a single value | Can return multiple values (via `yield`) |
| Called once, runs fully | Called once, returns iterator, run with `next()` |
| Not iterable | Iterable (usable in `for...of`) |

### Summary

- A **generator function** (`function*`) can **pause and resume** execution using `yield`.
- They are useful for **iterators, lazy evaluation, infinite sequences, and async-like patterns**.
- Think of it as a **TV series you can pause/resume**, while normal functions are a **movie that runs straight through**.
