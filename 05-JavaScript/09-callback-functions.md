# Callback Functions in JavaScript

## What is a Callback Function?

A **callback function** is a function passed as an argument to another function, which is then executed (called back) at a later time or after a specific event occurs.

```javascript
function greet(name, callback) {
  console.log(`Hello, ${name}!`);
  callback();
}

function sayGoodbye() {
  console.log('Goodbye!');
}

greet('Alice', sayGoodbye);
// Output:
// Hello, Alice!
// Goodbye!
```

---

## Why Use Callbacks?

1. **Asynchronous Operations**: Handle operations that take time (API calls, file reading)
2. **Event Handling**: Respond to user interactions
3. **Code Reusability**: Pass different behaviors to the same function
4. **Higher-Order Functions**: Enable functional programming patterns

---

## Synchronous Callbacks

Callbacks that execute immediately during the function call.

### Example 1: Array Methods

```javascript
const numbers = [1, 2, 3, 4, 5];

// forEach callback
numbers.forEach(function(num) {
  console.log(num * 2);
});
// Output: 2, 4, 6, 8, 10

// map callback
const doubled = numbers.map(function(num) {
  return num * 2;
});
console.log(doubled); // [2, 4, 6, 8, 10]

// filter callback
const evens = numbers.filter(function(num) {
  return num % 2 === 0;
});
console.log(evens); // [2, 4]

// reduce callback
const sum = numbers.reduce(function(acc, num) {
  return acc + num;
}, 0);
console.log(sum); // 15
```

### Example 2: Custom Function with Callback

```javascript
function processArray(arr, callback) {
  const result = [];
  for (let i = 0; i < arr.length; i++) {
    result.push(callback(arr[i], i));
  }
  return result;
}

const numbers = [1, 2, 3];

// Use with different callbacks
const squared = processArray(numbers, num => num * num);
console.log(squared); // [1, 4, 9]

const withIndex = processArray(numbers, (num, idx) => `${idx}: ${num}`);
console.log(withIndex); // ['0: 1', '1: 2', '2: 3']
```

---

## Asynchronous Callbacks

Callbacks that execute after an asynchronous operation completes.

### Example 1: setTimeout

```javascript
console.log('Start');

setTimeout(function() {
  console.log('This runs after 2 seconds');
}, 2000);

console.log('End');

// Output:
// Start
// End
// This runs after 2 seconds
```

### Example 2: setInterval

```javascript
let count = 0;

const intervalId = setInterval(function() {
  count++;
  console.log(`Count: ${count}`);
  
  if (count === 5) {
    clearInterval(intervalId);
    console.log('Stopped!');
  }
}, 1000);

// Output (every 1 second):
// Count: 1
// Count: 2
// Count: 3
// Count: 4
// Count: 5
// Stopped!
```

### Example 3: Event Listeners

```javascript
// Button click event
const button = document.getElementById('myButton');

button.addEventListener('click', function() {
  console.log('Button clicked!');
});

// Or with arrow function
button.addEventListener('click', () => {
  console.log('Button clicked!');
});

// Mouse move event
document.addEventListener('mousemove', function(event) {
  console.log(`Mouse position: ${event.clientX}, ${event.clientY}`);
});
```

---

## Error-First Callbacks (Node.js Convention)

A common pattern where the first parameter of a callback is an error object.

```javascript
function readFileAsync(filename, callback) {
  // Simulate file reading
  setTimeout(() => {
    if (!filename) {
      callback(new Error('Filename is required'), null);
    } else {
      callback(null, 'File contents here');
    }
  }, 1000);
}

// Usage
readFileAsync('data.txt', function(error, data) {
  if (error) {
    console.error('Error:', error.message);
    return;
  }
  console.log('Data:', data);
});

// With arrow function
readFileAsync('data.txt', (err, data) => {
  if (err) {
    console.error('Error:', err.message);
    return;
  }
  console.log('Data:', data);
});
```

---

## Callback Hell (Pyramid of Doom)

When callbacks are nested too deeply, code becomes hard to read and maintain.

### The Problem

```javascript
// ❌ Callback hell
getData(function(a) {
  getMoreData(a, function(b) {
    getEvenMoreData(b, function(c) {
      getYetMoreData(c, function(d) {
        getFinalData(d, function(e) {
          console.log('Finally done!', e);
        });
      });
    });
  });
});
```

### Solutions

#### 1. Named Functions

```javascript
function processA(a) {
  getMoreData(a, processB);
}

function processB(b) {
  getEvenMoreData(b, processC);
}

function processC(c) {
  getYetMoreData(c, processD);
}

function processD(d) {
  getFinalData(d, processFinal);
}

function processFinal(e) {
  console.log('Finally done!', e);
}

getData(processA);
```

#### 2. Promises (Modern Solution)

```javascript
getData()
  .then(a => getMoreData(a))
  .then(b => getEvenMoreData(b))
  .then(c => getYetMoreData(c))
  .then(d => getFinalData(d))
  .then(e => console.log('Finally done!', e))
  .catch(error => console.error('Error:', error));
```

#### 3. Async/Await (Best Solution)

```javascript
async function processData() {
  try {
    const a = await getData();
    const b = await getMoreData(a);
    const c = await getEvenMoreData(b);
    const d = await getYetMoreData(c);
    const e = await getFinalData(d);
    console.log('Finally done!', e);
  } catch (error) {
    console.error('Error:', error);
  }
}

processData();
```

---

## Common Callback Patterns

### Pattern 1: Custom Event Emitter

```javascript
class EventEmitter {
  constructor() {
    this.events = {};
  }
  
  on(event, callback) {
    if (!this.events[event]) {
      this.events[event] = [];
    }
    this.events[event].push(callback);
  }
  
  emit(event, data) {
    if (this.events[event]) {
      this.events[event].forEach(callback => callback(data));
    }
  }
}

const emitter = new EventEmitter();

// Register callbacks
emitter.on('data', (data) => {
  console.log('Data received:', data);
});

emitter.on('data', (data) => {
  console.log('Another handler:', data);
});

// Trigger callbacks
emitter.emit('data', { value: 42 });
// Output:
// Data received: { value: 42 }
// Another handler: { value: 42 }
```

### Pattern 2: Once Callback

```javascript
function once(callback) {
  let called = false;
  
  return function(...args) {
    if (!called) {
      called = true;
      callback.apply(this, args);
    }
  };
}

const initialize = once(() => {
  console.log('Initialized!');
});

initialize(); // "Initialized!"
initialize(); // Nothing happens
initialize(); // Nothing happens
```

### Pattern 3: Retry with Callback

```javascript
function retry(fn, maxAttempts, callback) {
  let attempts = 0;
  
  function attempt() {
    attempts++;
    
    fn((error, result) => {
      if (error && attempts < maxAttempts) {
        console.log(`Attempt ${attempts} failed, retrying...`);
        attempt();
      } else if (error) {
        callback(error, null);
      } else {
        callback(null, result);
      }
    });
  }
  
  attempt();
}

// Usage
function unreliableOperation(callback) {
  const random = Math.random();
  setTimeout(() => {
    if (random < 0.7) {
      callback(new Error('Failed'), null);
    } else {
      callback(null, 'Success!');
    }
  }, 1000);
}

retry(unreliableOperation, 3, (error, result) => {
  if (error) {
    console.error('All attempts failed');
  } else {
    console.log('Result:', result);
  }
});
```

---

## Real-World Examples

### Example 1: API Request

```javascript
function fetchUser(userId, callback) {
  fetch(`https://api.example.com/users/${userId}`)
    .then(response => response.json())
    .then(data => callback(null, data))
    .catch(error => callback(error, null));
}

// Usage
fetchUser(123, (error, user) => {
  if (error) {
    console.error('Failed to fetch user:', error);
    return;
  }
  console.log('User:', user);
});
```

### Example 2: Form Validation

```javascript
function validateForm(formData, validators, callback) {
  const errors = [];
  
  validators.forEach(validator => {
    const error = validator(formData);
    if (error) {
      errors.push(error);
    }
  });
  
  if (errors.length > 0) {
    callback(errors, null);
  } else {
    callback(null, formData);
  }
}

// Validators
const validators = [
  (data) => !data.email ? 'Email is required' : null,
  (data) => data.password.length < 8 ? 'Password too short' : null,
  (data) => !data.username ? 'Username is required' : null
];

// Usage
const formData = {
  email: 'user@example.com',
  password: 'pass',
  username: 'john'
};

validateForm(formData, validators, (errors, data) => {
  if (errors) {
    console.error('Validation errors:', errors);
  } else {
    console.log('Valid data:', data);
  }
});
```

### Example 3: Database Query (Node.js Style)

```javascript
function queryDatabase(sql, params, callback) {
  // Simulate database query
  setTimeout(() => {
    if (!sql) {
      callback(new Error('SQL query is required'), null);
      return;
    }
    
    // Simulate results
    const results = [
      { id: 1, name: 'Alice' },
      { id: 2, name: 'Bob' }
    ];
    
    callback(null, results);
  }, 1000);
}

// Usage
queryDatabase('SELECT * FROM users', [], (err, results) => {
  if (err) {
    console.error('Database error:', err);
    return;
  }
  
  console.log('Query results:', results);
  
  // Process results
  results.forEach(row => {
    console.log(`User: ${row.name}`);
  });
});
```

---

## Callback vs Promise vs Async/Await

### Callback

```javascript
function getUser(id, callback) {
  setTimeout(() => {
    callback(null, { id, name: 'Alice' });
  }, 1000);
}

getUser(1, (err, user) => {
  if (err) {
    console.error(err);
  } else {
    console.log(user);
  }
});
```

### Promise

```javascript
function getUser(id) {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      resolve({ id, name: 'Alice' });
    }, 1000);
  });
}

getUser(1)
  .then(user => console.log(user))
  .catch(err => console.error(err));
```

### Async/Await

```javascript
function getUser(id) {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      resolve({ id, name: 'Alice' });
    }, 1000);
  });
}

async function main() {
  try {
    const user = await getUser(1);
    console.log(user);
  } catch (err) {
    console.error(err);
  }
}

main();
```

---

## Interview Questions

### Q1: What will be logged?

```javascript
console.log('A');

setTimeout(() => console.log('B'), 0);

console.log('C');
```

**Answer:** A, C, B (callbacks in setTimeout are async, even with 0 delay)

### Q2: Implement a `map` function using callbacks

```javascript
function myMap(array, callback) {
  const result = [];
  for (let i = 0; i < array.length; i++) {
    result.push(callback(array[i], i, array));
  }
  return result;
}

// Test
const doubled = myMap([1, 2, 3], x => x * 2);
console.log(doubled); // [2, 4, 6]
```

### Q3: Fix this callback hell

```javascript
// ❌ Nested callbacks
getData(function(a) {
  getMoreData(a, function(b) {
    console.log(b);
  });
});

// ✅ Using Promises
getData()
  .then(a => getMoreData(a))
  .then(b => console.log(b))
  .catch(err => console.error(err));

// ✅ Using async/await
async function process() {
  try {
    const a = await getData();
    const b = await getMoreData(a);
    console.log(b);
  } catch (err) {
    console.error(err);
  }
}
```

---

## Summary

### Key Concepts

1. **Callback**: Function passed as argument to another function
2. **Synchronous Callbacks**: Execute immediately (array methods)
3. **Asynchronous Callbacks**: Execute later (setTimeout, events)
4. **Error-First Callbacks**: Common Node.js pattern (error, data)
5. **Callback Hell**: Deeply nested callbacks (hard to maintain)

### When to Use Callbacks

- ✅ Simple async operations
- ✅ Event handlers
- ✅ Array methods (forEach, map, filter, reduce)
- ✅ Custom higher-order functions

### When NOT to Use Callbacks

- ❌ Complex async flows (use Promises/async-await)
- ❌ Multiple sequential operations (callback hell)
- ❌ Error handling across multiple operations

### Modern Alternatives

1. **Promises**: Better error handling, chainable
2. **Async/Await**: Synchronous-looking async code
3. **Observables** (RxJS): For streams of data

### Best Practices

- ✅ Use error-first callback pattern
- ✅ Handle errors properly
- ✅ Avoid deep nesting
- ✅ Use Promises or async/await for complex flows
- ✅ Keep callbacks focused (single responsibility)

Master callbacks to understand JavaScript's asynchronous nature! 🚀