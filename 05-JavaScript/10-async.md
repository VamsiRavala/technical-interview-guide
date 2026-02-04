# Asynchronous JavaScript - Complete Guide

## Table of Contents
1. [Synchronous vs Asynchronous Programming](#synchronous-vs-asynchronous-programming)
2. [setTimeout and setInterval](#settimeout-and-setinterval)
3. [Callbacks](#callbacks)
4. [Promises](#promises)
5. [Promise Chaining](#promise-chaining)
6. [Promise Static Methods](#promise-static-methods)
7. [Async/Await](#asyncawait)
8. [Error Handling](#error-handling)
9. [Async Iteration](#async-iteration)
10. [Fetch API](#fetch-api)
11. [Parallel vs Sequential Execution](#parallel-vs-sequential-execution)
12. [Common Patterns and Best Practices](#common-patterns-and-best-practices)
13. [Common Pitfalls](#common-pitfalls)
14. [Interview Questions](#interview-questions)
15. [Summary](#summary)

---

## Synchronous vs Asynchronous Programming

### Synchronous Programming

Code executes line by line, blocking execution until each operation completes.

```javascript
console.log('Start');

// Blocking operation (simulated)
function slowOperation() {
  const end = Date.now() + 2000;
  while (Date.now() < end) {
    // Blocks for 2 seconds
  }
  return 'Done';
}

console.log('Processing...');
const result = slowOperation(); // Blocks here
console.log(result);
console.log('End');

// Output:
// Start
// Processing...
// (2 second pause)
// Done
// End
```

### Asynchronous Programming

Operations run in the background, allowing other code to execute without waiting.

```javascript
console.log('Start');

// Non-blocking operation
setTimeout(() => {
  console.log('Async operation completed');
}, 2000);

console.log('End');

// Output:
// Start
// End
// (2 second pause)
// Async operation completed
```

### Why Asynchronous?

| Aspect | Synchronous | Asynchronous |
|--------|-------------|--------------|
| **Execution** | Blocking | Non-blocking |
| **Performance** | Slower for I/O | Faster for I/O |
| **User Experience** | Can freeze UI | Responsive UI |
| **Use Case** | Simple calculations | API calls, file I/O, timers |
| **Complexity** | Simpler to reason about | More complex (callbacks, promises) |

**Common Async Operations:**
- Network requests (fetch, AJAX)
- File system operations
- Database queries
- Timers (setTimeout, setInterval)
- User events

> **Note:** For more details on how asynchronous code is executed, see [11-interview/event_loop.md](./11-interview/event_loop.md)

---

## setTimeout and setInterval

### setTimeout

Executes a function after a specified delay (in milliseconds).

```javascript
// Basic usage
setTimeout(() => {
  console.log('Executed after 2 seconds');
}, 2000);

// With parameters
setTimeout((name, age) => {
  console.log(`${name} is ${age} years old`);
}, 1000, 'Alice', 30);
// Output (after 1 second): Alice is 30 years old

// Storing timeout ID
const timeoutId = setTimeout(() => {
  console.log('This will not execute');
}, 3000);

// Cancel the timeout
clearTimeout(timeoutId);
```

### setInterval

Repeatedly executes a function at specified intervals.

```javascript
// Basic usage
let count = 0;
const intervalId = setInterval(() => {
  count++;
  console.log(`Count: ${count}`);
  
  if (count >= 5) {
    clearInterval(intervalId);
    console.log('Interval cleared');
  }
}, 1000);

// Output (every second):
// Count: 1
// Count: 2
// Count: 3
// Count: 4
// Count: 5
// Interval cleared
```

### clearTimeout and clearInterval

```javascript
// clearTimeout example
const timeout1 = setTimeout(() => {
  console.log('Will not execute');
}, 5000);

const timeout2 = setTimeout(() => {
  console.log('Will execute');
}, 1000);

clearTimeout(timeout1); // Cancel timeout1

// clearInterval example
let counter = 0;
const interval = setInterval(() => {
  counter++;
  console.log(counter);
}, 1000);

// Clear after 5 seconds
setTimeout(() => {
  clearInterval(interval);
  console.log('Stopped');
}, 5500);
```

### Important Notes

```javascript
// setTimeout with 0ms still runs asynchronously
console.log('1');
setTimeout(() => console.log('2'), 0);
console.log('3');

// Output:
// 1
// 3
// 2

// Reason: setTimeout callback goes to task queue, 
// executes after current call stack is empty
```

---

## Callbacks

A callback is a function passed as an argument to another function, executed after an operation completes.

> **For detailed callback information, see [09-callback-functions.md](./09-callback-functions.md)**

### Basic Callback

```javascript
function fetchData(callback) {
  setTimeout(() => {
    const data = { id: 1, name: 'John' };
    callback(data);
  }, 1000);
}

fetchData((data) => {
  console.log('Data received:', data);
});
// Output (after 1 second): Data received: { id: 1, name: 'John' }
```

### Callback Hell (Pyramid of Doom)

Multiple nested callbacks become hard to read and maintain.

```javascript
// ❌ Bad: Callback Hell
getData((a) => {
  getMoreData(a, (b) => {
    getEvenMoreData(b, (c) => {
      getYetMoreData(c, (d) => {
        getFinalData(d, (e) => {
          console.log('Finally done!');
        });
      });
    });
  });
});

// Problems:
// 1. Hard to read (pyramid shape)
// 2. Difficult to handle errors
// 3. Hard to maintain
// 4. Difficult to reason about
```

**Solution:** Use Promises or async/await (covered below).

---

## Promises

A Promise is an object representing the eventual completion or failure of an asynchronous operation.

### Promise States

| State | Description |
|-------|-------------|
| **Pending** | Initial state, neither fulfilled nor rejected |
| **Fulfilled** | Operation completed successfully |
| **Rejected** | Operation failed |
| **Settled** | Either fulfilled or rejected (no longer pending) |

### Creating Promises

```javascript
// Basic Promise creation
const myPromise = new Promise((resolve, reject) => {
  const success = true;
  
  setTimeout(() => {
    if (success) {
      resolve('Operation successful!'); // Fulfilled
    } else {
      reject('Operation failed!'); // Rejected
    }
  }, 1000);
});

// Using the promise
myPromise
  .then((result) => {
    console.log(result); // Operation successful!
  })
  .catch((error) => {
    console.error(error);
  });
```

### Promise Methods: then, catch, finally

```javascript
// then: handles fulfilled state
promise.then((value) => {
  console.log('Success:', value);
});

// catch: handles rejected state
promise.catch((error) => {
  console.error('Error:', error);
});

// finally: executes regardless of outcome
promise.finally(() => {
  console.log('Cleanup or final operations');
});

// Complete example
function fetchUser(id) {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      if (id > 0) {
        resolve({ id, name: 'Alice', email: 'alice@example.com' });
      } else {
        reject('Invalid user ID');
      }
    }, 1000);
  });
}

fetchUser(1)
  .then((user) => {
    console.log('User:', user);
    return user.id;
  })
  .then((id) => {
    console.log('User ID:', id);
  })
  .catch((error) => {
    console.error('Error:', error);
  })
  .finally(() => {
    console.log('Request completed');
  });

// Output:
// User: { id: 1, name: 'Alice', email: 'alice@example.com' }
// User ID: 1
// Request completed
```

### Resolving and Rejecting Immediately

```javascript
// Immediately resolved
const resolvedPromise = Promise.resolve('Immediate success');
resolvedPromise.then(console.log); // Immediate success

// Immediately rejected
const rejectedPromise = Promise.reject('Immediate failure');
rejectedPromise.catch(console.error); // Immediate failure

// Wrapping a value in a Promise
const value = 42;
Promise.resolve(value).then(console.log); // 42
```

---

## Promise Chaining

Chain multiple asynchronous operations in sequence.

### Basic Chaining

```javascript
function getUser(userId) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve({ id: userId, name: 'John' });
    }, 1000);
  });
}

function getPosts(userId) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve([
        { id: 1, title: 'Post 1' },
        { id: 2, title: 'Post 2' }
      ]);
    }, 1000);
  });
}

function getComments(postId) {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve([
        { id: 1, text: 'Great post!' },
        { id: 2, text: 'Thanks for sharing' }
      ]);
    }, 1000);
  });
}

// ✅ Good: Promise Chaining
getUser(1)
  .then((user) => {
    console.log('User:', user.name);
    return getPosts(user.id);
  })
  .then((posts) => {
    console.log('Posts:', posts.length);
    return getComments(posts[0].id);
  })
  .then((comments) => {
    console.log('Comments:', comments.length);
  })
  .catch((error) => {
    console.error('Error:', error);
  });

// Output:
// User: John
// Posts: 2
// Comments: 2
```

### Returning Values vs Promises

```javascript
Promise.resolve(5)
  .then((value) => {
    console.log(value); // 5
    return value * 2; // Return plain value
  })
  .then((value) => {
    console.log(value); // 10
    return Promise.resolve(value * 2); // Return promise
  })
  .then((value) => {
    console.log(value); // 20
  });

// Both work! Returning a value auto-wraps it in a Promise
```

### Error Propagation in Chains

```javascript
Promise.resolve(1)
  .then((value) => {
    console.log('Step 1:', value);
    return value + 1;
  })
  .then((value) => {
    console.log('Step 2:', value);
    throw new Error('Something went wrong!');
  })
  .then((value) => {
    console.log('Step 3:', value); // Won't execute
  })
  .catch((error) => {
    console.error('Caught:', error.message); // Catches the error
    return 'recovered';
  })
  .then((value) => {
    console.log('Step 4:', value); // Continues after catch
  });

// Output:
// Step 1: 1
// Step 2: 2
// Caught: Something went wrong!
// Step 4: recovered
```

---

## Promise Static Methods

### Comparison Table

| Method | Description | Resolves When | Rejects When | Use Case |
|--------|-------------|---------------|--------------|----------|
| `Promise.all()` | Waits for all promises | All resolve | Any rejects | All must succeed |
| `Promise.race()` | Waits for first settled | First resolves | First rejects | Need fastest result |
| `Promise.allSettled()` | Waits for all to settle | All settle | Never rejects | Want all results |
| `Promise.any()` | Waits for first fulfilled | First resolves | All reject | Need any success |

### Promise.all()

Waits for all promises to resolve. Rejects if any promise rejects.

```javascript
const promise1 = Promise.resolve(3);
const promise2 = new Promise((resolve) => 
  setTimeout(() => resolve('foo'), 1000)
);
const promise3 = fetch('https://jsonplaceholder.typicode.com/users/1')
  .then(res => res.json());

Promise.all([promise1, promise2, promise3])
  .then((values) => {
    console.log('All resolved:', values);
    // [3, 'foo', { user object }]
  })
  .catch((error) => {
    console.error('One failed:', error);
  });

// Practical example: Fetch multiple users
const userIds = [1, 2, 3];
const promises = userIds.map(id => 
  fetch(`https://jsonplaceholder.typicode.com/users/${id}`)
    .then(res => res.json())
);

Promise.all(promises)
  .then((users) => {
    console.log('All users:', users);
  })
  .catch((error) => {
    console.error('Failed to fetch users:', error);
  });
```

### Promise.race()

Returns the first promise that settles (either resolves or rejects).

```javascript
const slow = new Promise((resolve) => 
  setTimeout(() => resolve('Slow'), 3000)
);

const fast = new Promise((resolve) => 
  setTimeout(() => resolve('Fast'), 1000)
);

Promise.race([slow, fast])
  .then((result) => {
    console.log(result); // 'Fast' (after 1 second)
  });

// Practical: Timeout pattern
function fetchWithTimeout(url, timeout = 5000) {
  const fetchPromise = fetch(url);
  const timeoutPromise = new Promise((_, reject) => 
    setTimeout(() => reject(new Error('Request timeout')), timeout)
  );
  
  return Promise.race([fetchPromise, timeoutPromise]);
}

fetchWithTimeout('https://jsonplaceholder.typicode.com/users/1', 3000)
  .then(res => res.json())
  .then(data => console.log(data))
  .catch(error => console.error(error.message));
```

### Promise.allSettled()

Waits for all promises to settle (resolve or reject). Never rejects.

```javascript
const promises = [
  Promise.resolve(1),
  Promise.reject('Error!'),
  Promise.resolve(3),
  Promise.reject('Another error!')
];

Promise.allSettled(promises)
  .then((results) => {
    console.log(results);
    /*
    [
      { status: 'fulfilled', value: 1 },
      { status: 'rejected', reason: 'Error!' },
      { status: 'fulfilled', value: 3 },
      { status: 'rejected', reason: 'Another error!' }
    ]
    */
    
    // Filter successful results
    const successful = results
      .filter(r => r.status === 'fulfilled')
      .map(r => r.value);
    console.log('Successful:', successful); // [1, 3]
    
    // Filter failed results
    const failed = results
      .filter(r => r.status === 'rejected')
      .map(r => r.reason);
    console.log('Failed:', failed); // ['Error!', 'Another error!']
  });

// Practical: Batch operations with mixed results
async function batchUpdateUsers(users) {
  const promises = users.map(user => updateUser(user));
  const results = await Promise.allSettled(promises);
  
  const succeeded = results.filter(r => r.status === 'fulfilled').length;
  const failed = results.filter(r => r.status === 'rejected').length;
  
  console.log(`Updated: ${succeeded}, Failed: ${failed}`);
  return results;
}
```

### Promise.any()

Resolves with the first fulfilled promise. Rejects only if all promises reject.

```javascript
const promise1 = Promise.reject('Error 1');
const promise2 = new Promise((resolve) => 
  setTimeout(() => resolve('Success 2'), 1000)
);
const promise3 = new Promise((resolve) => 
  setTimeout(() => resolve('Success 3'), 2000)
);

Promise.any([promise1, promise2, promise3])
  .then((result) => {
    console.log(result); // 'Success 2' (first to resolve)
  })
  .catch((error) => {
    console.error('All failed:', error);
  });

// All reject example
const allReject = [
  Promise.reject('Error 1'),
  Promise.reject('Error 2'),
  Promise.reject('Error 3')
];

Promise.any(allReject)
  .catch((error) => {
    console.error(error); // AggregateError: All promises were rejected
    console.error(error.errors); // ['Error 1', 'Error 2', 'Error 3']
  });

// Practical: Multiple API endpoints (fallback)
async function fetchFromMultipleServers(path) {
  const servers = [
    'https://api1.example.com',
    'https://api2.example.com',
    'https://api3.example.com'
  ];
  
  const promises = servers.map(server => 
    fetch(`${server}${path}`).then(res => res.json())
  );
  
  try {
    const data = await Promise.any(promises);
    return data;
  } catch (error) {
    throw new Error('All servers failed');
  }
}
```

---

## Async/Await

Syntactic sugar over Promises, making asynchronous code look synchronous.

### Basic Syntax

```javascript
// Promise-based approach
function fetchUserPromise(id) {
  return fetch(`https://jsonplaceholder.typicode.com/users/${id}`)
    .then(response => response.json())
    .then(user => {
      console.log(user.name);
      return user;
    })
    .catch(error => {
      console.error('Error:', error);
    });
}

// Async/Await approach
async function fetchUserAsync(id) {
  try {
    const response = await fetch(`https://jsonplaceholder.typicode.com/users/${id}`);
    const user = await response.json();
    console.log(user.name);
    return user;
  } catch (error) {
    console.error('Error:', error);
  }
}

// An async function always returns a Promise
const result = fetchUserAsync(1); // Returns Promise
result.then(user => console.log(user));
```

### Multiple Awaits

```javascript
async function getUserData(userId) {
  try {
    // Sequential execution
    const user = await fetchUser(userId);
    console.log('User fetched:', user.name);
    
    const posts = await fetchPosts(user.id);
    console.log('Posts fetched:', posts.length);
    
    const comments = await fetchComments(posts[0].id);
    console.log('Comments fetched:', comments.length);
    
    return { user, posts, comments };
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Call async function
getUserData(1)
  .then(data => console.log('Complete data:', data))
  .catch(error => console.error('Failed:', error));
```

### Async Functions Always Return Promises

```javascript
async function returnValue() {
  return 42;
}

// Equivalent to:
function returnValuePromise() {
  return Promise.resolve(42);
}

returnValue().then(console.log); // 42

// Throwing in async function = rejected promise
async function throwError() {
  throw new Error('Oops!');
}

// Equivalent to:
function throwErrorPromise() {
  return Promise.reject(new Error('Oops!'));
}

throwError().catch(error => console.error(error.message)); // Oops!
```

### Await with Non-Promise Values

```javascript
async function example() {
  const value1 = await 42; // Works fine
  const value2 = await Promise.resolve(100);
  
  console.log(value1); // 42
  console.log(value2); // 100
}

// await converts non-promise values to resolved promises
```

### Top-Level Await (ES2022)

```javascript
// In modules (.mjs or type: "module" in package.json)
// You can use await at the top level

// Before ES2022: Need wrapper function
(async () => {
  const data = await fetchData();
  console.log(data);
})();

// ES2022+: Top-level await
const data = await fetchData();
console.log(data);

// Practical example
const config = await fetch('/api/config').then(r => r.json());
const app = createApp(config);
app.start();
```

---

## Error Handling

### Try/Catch with Async/Await

```javascript
async function fetchData(url) {
  try {
    const response = await fetch(url);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Fetch error:', error.message);
    throw error; // Re-throw if needed
  }
}

// Multiple try/catch blocks
async function complexOperation() {
  let user;
  
  try {
    user = await fetchUser(1);
  } catch (error) {
    console.error('Failed to fetch user:', error);
    user = { id: 1, name: 'Default User' }; // Fallback
  }
  
  try {
    const posts = await fetchPosts(user.id);
    return posts;
  } catch (error) {
    console.error('Failed to fetch posts:', error);
    return []; // Return empty array
  }
}
```

### Error Handling Best Practices

```javascript
// ❌ Bad: Unhandled rejection
async function bad() {
  await Promise.reject('Error');
} // Unhandled promise rejection!

// ✅ Good: Always handle errors
async function good() {
  try {
    await Promise.reject('Error');
  } catch (error) {
    console.error('Handled:', error);
  }
}

// ✅ Good: Let caller handle
async function goodAlternative() {
  await Promise.reject('Error');
}

goodAlternative().catch(error => console.error('Handled by caller:', error));

// Centralized error handler
async function withErrorHandler(asyncFn) {
  try {
    return await asyncFn();
  } catch (error) {
    console.error('Global error handler:', error);
    // Log to error tracking service
    // logToService(error);
    throw error;
  }
}

// Usage
withErrorHandler(async () => {
  const data = await fetchData();
  return data;
});
```

### Finally Block

```javascript
async function fetchWithCleanup(url) {
  let connection;
  
  try {
    connection = await openConnection();
    const data = await fetch(url);
    return await data.json();
  } catch (error) {
    console.error('Error:', error);
    throw error;
  } finally {
    // Always executes (success or failure)
    if (connection) {
      await connection.close();
      console.log('Connection closed');
    }
  }
}

// Real-world example: Loading indicator
async function loadData() {
  showLoadingSpinner();
  
  try {
    const data = await fetch('/api/data').then(r => r.json());
    displayData(data);
  } catch (error) {
    showError(error.message);
  } finally {
    hideLoadingSpinner(); // Always hide spinner
  }
}
```

---

## Async Iteration

### for await...of

Iterate over async iterables (streams, async generators, etc.).

```javascript
// Async generator function
async function* numberGenerator() {
  for (let i = 1; i <= 5; i++) {
    await new Promise(resolve => setTimeout(resolve, 1000));
    yield i;
  }
}

// Using for await...of
async function iterateNumbers() {
  for await (const num of numberGenerator()) {
    console.log(num); // 1, 2, 3, 4, 5 (one per second)
  }
  console.log('Done');
}

iterateNumbers();
```

### Practical Example: Fetching Pages

```javascript
async function* fetchPages(userId, maxPages = 5) {
  for (let page = 1; page <= maxPages; page++) {
    const response = await fetch(
      `https://api.example.com/users/${userId}/posts?page=${page}`
    );
    const data = await response.json();
    yield data;
  }
}

async function getAllPosts(userId) {
  const allPosts = [];
  
  for await (const page of fetchPages(userId)) {
    allPosts.push(...page);
    console.log(`Fetched page with ${page.length} posts`);
  }
  
  return allPosts;
}
```

### Async Iteration with Arrays

```javascript
// Processing array of promises sequentially
async function processSequentially(urls) {
  const results = [];
  
  for (const url of urls) {
    const response = await fetch(url);
    const data = await response.json();
    results.push(data);
    console.log(`Processed: ${url}`);
  }
  
  return results;
}

// With for await...of (if working with async iterables)
async function* fetchUrls(urls) {
  for (const url of urls) {
    const response = await fetch(url);
    yield response.json();
  }
}

async function processWithAsyncIteration(urls) {
  for await (const data of fetchUrls(urls)) {
    console.log('Data:', data);
  }
}
```

---

## Fetch API

Modern API for making HTTP requests, returns Promises.

### Basic GET Request

```javascript
// Simple GET
fetch('https://jsonplaceholder.typicode.com/users/1')
  .then(response => response.json())
  .then(data => console.log(data))
  .catch(error => console.error('Error:', error));

// With async/await
async function getUser(id) {
  try {
    const response = await fetch(`https://jsonplaceholder.typicode.com/users/${id}`);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const user = await response.json();
    return user;
  } catch (error) {
    console.error('Error fetching user:', error);
    throw error;
  }
}
```

### POST Request

```javascript
async function createPost(post) {
  try {
    const response = await fetch('https://jsonplaceholder.typicode.com/posts', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(post)
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    console.log('Created:', data);
    return data;
  } catch (error) {
    console.error('Error creating post:', error);
    throw error;
  }
}

// Usage
createPost({
  title: 'My Post',
  body: 'This is the content',
  userId: 1
});
```

### PUT and DELETE Requests

```javascript
// PUT - Update
async function updatePost(id, updates) {
  const response = await fetch(`https://jsonplaceholder.typicode.com/posts/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(updates)
  });
  
  return response.json();
}

// PATCH - Partial update
async function patchPost(id, updates) {
  const response = await fetch(`https://jsonplaceholder.typicode.com/posts/${id}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(updates)
  });
  
  return response.json();
}

// DELETE
async function deletePost(id) {
  const response = await fetch(`https://jsonplaceholder.typicode.com/posts/${id}`, {
    method: 'DELETE'
  });
  
  if (response.ok) {
    console.log('Deleted successfully');
  }
}
```

### Headers and Authentication

```javascript
async function fetchWithAuth(url, token) {
  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  });
  
  return response.json();
}

// Reading response headers
async function getHeaders(url) {
  const response = await fetch(url);
  
  console.log('Content-Type:', response.headers.get('Content-Type'));
  console.log('Date:', response.headers.get('Date'));
  
  // Iterate all headers
  response.headers.forEach((value, key) => {
    console.log(`${key}: ${value}`);
  });
}
```

### Handling Different Response Types

```javascript
async function fetchText(url) {
  const response = await fetch(url);
  const text = await response.text(); // Get as text
  return text;
}

async function fetchBlob(url) {
  const response = await fetch(url);
  const blob = await response.blob(); // Get as blob (for files/images)
  return blob;
}

async function fetchFormData(url) {
  const response = await fetch(url);
  const formData = await response.formData(); // Get as FormData
  return formData;
}

// Downloading an image
async function downloadImage(url) {
  const response = await fetch(url);
  const blob = await response.blob();
  const imageUrl = URL.createObjectURL(blob);
  
  const img = document.createElement('img');
  img.src = imageUrl;
  document.body.appendChild(img);
}
```

### Fetch with Timeout

```javascript
async function fetchWithTimeout(url, timeout = 5000) {
  const controller = new AbortController();
  const signal = controller.signal;
  
  const timeoutId = setTimeout(() => controller.abort(), timeout);
  
  try {
    const response = await fetch(url, { signal });
    clearTimeout(timeoutId);
    return response.json();
  } catch (error) {
    clearTimeout(timeoutId);
    
    if (error.name === 'AbortError') {
      throw new Error('Request timeout');
    }
    throw error;
  }
}

// Usage
fetchWithTimeout('https://jsonplaceholder.typicode.com/users/1', 3000)
  .then(data => console.log(data))
  .catch(error => console.error(error.message));
```

---

## Parallel vs Sequential Execution

### Sequential Execution

Operations run one after another.

```javascript
// Sequential - Takes 3 seconds total
async function sequential() {
  console.time('Sequential');
  
  const user = await fetch('https://jsonplaceholder.typicode.com/users/1')
    .then(r => r.json());
  // 1 second
  
  const posts = await fetch('https://jsonplaceholder.typicode.com/posts?userId=1')
    .then(r => r.json());
  // 1 second
  
  const todos = await fetch('https://jsonplaceholder.typicode.com/todos?userId=1')
    .then(r => r.json());
  // 1 second
  
  console.timeEnd('Sequential'); // ~3000ms
  return { user, posts, todos };
}
```

### Parallel Execution

Independent operations run simultaneously.

```javascript
// Parallel - Takes 1 second total (fastest request)
async function parallel() {
  console.time('Parallel');
  
  const [user, posts, todos] = await Promise.all([
    fetch('https://jsonplaceholder.typicode.com/users/1').then(r => r.json()),
    fetch('https://jsonplaceholder.typicode.com/posts?userId=1').then(r => r.json()),
    fetch('https://jsonplaceholder.typicode.com/todos?userId=1').then(r => r.json())
  ]);
  
  console.timeEnd('Parallel'); // ~1000ms
  return { user, posts, todos };
}
```

### Comparison Example

```javascript
async function compareExecution() {
  // Sequential execution
  console.log('Starting sequential...');
  const start1 = Date.now();
  await sequential();
  console.log(`Sequential took: ${Date.now() - start1}ms`);
  
  // Parallel execution
  console.log('\nStarting parallel...');
  const start2 = Date.now();
  await parallel();
  console.log(`Parallel took: ${Date.now() - start2}ms`);
}

// Output:
// Starting sequential...
// Sequential took: 3245ms
//
// Starting parallel...
// Parallel took: 1089ms
```

### When to Use Each

```javascript
// ✅ Use Sequential when operations depend on each other
async function dependentOperations(userId) {
  // Must get user first
  const user = await fetchUser(userId);
  
  // Need user data to fetch posts
  const posts = await fetchPosts(user.id);
  
  // Need posts data to fetch comments
  const comments = await fetchComments(posts[0].id);
  
  return { user, posts, comments };
}

// ✅ Use Parallel when operations are independent
async function independentOperations(userId) {
  // All can run at the same time
  const [user, settings, notifications] = await Promise.all([
    fetchUser(userId),
    fetchSettings(userId),
    fetchNotifications(userId)
  ]);
  
  return { user, settings, notifications };
}

// ✅ Hybrid approach
async function hybridApproach(userId) {
  // Parallel: Independent initial fetches
  const [user, settings] = await Promise.all([
    fetchUser(userId),
    fetchSettings(userId)
  ]);
  
  // Sequential: Depends on user data
  const posts = await fetchPosts(user.id);
  
  // Parallel: Independent follow-up fetches
  const [comments, likes] = await Promise.all([
    fetchComments(posts[0].id),
    fetchLikes(posts[0].id)
  ]);
  
  return { user, settings, posts, comments, likes };
}
```

### Parallel with Limit

```javascript
// Process array in parallel batches
async function processBatch(items, batchSize = 3) {
  const results = [];
  
  for (let i = 0; i < items.length; i += batchSize) {
    const batch = items.slice(i, i + batchSize);
    const batchResults = await Promise.all(
      batch.map(item => processItem(item))
    );
    results.push(...batchResults);
    console.log(`Processed batch ${Math.floor(i / batchSize) + 1}`);
  }
  
  return results;
}

// Usage: Process 10 items in batches of 3
const items = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
processBatch(items, 3);
// Batch 1: [1, 2, 3] - parallel
// Batch 2: [4, 5, 6] - parallel
// Batch 3: [7, 8, 9] - parallel
// Batch 4: [10] - single item
```

---

## Common Patterns and Best Practices

### 1. Retry Logic

```javascript
async function fetchWithRetry(url, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = await fetch(url);
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }
      
      return await response.json();
    } catch (error) {
      const isLastAttempt = i === maxRetries - 1;
      
      if (isLastAttempt) {
        throw error;
      }
      
      console.log(`Attempt ${i + 1} failed, retrying...`);
      await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1))); // Exponential backoff
    }
  }
}

// Usage
fetchWithRetry('https://api.example.com/data')
  .then(data => console.log('Success:', data))
  .catch(error => console.error('All retries failed:', error));
```

### 2. Debounced API Calls

```javascript
function debounce(fn, delay) {
  let timeoutId;
  return function(...args) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => fn(...args), delay);
  };
}

// Debounced search
const debouncedSearch = debounce(async (query) => {
  if (!query) return;
  
  try {
    const response = await fetch(`/api/search?q=${query}`);
    const results = await response.json();
    displayResults(results);
  } catch (error) {
    console.error('Search failed:', error);
  }
}, 300);

// Usage in input handler
document.getElementById('searchInput').addEventListener('input', (e) => {
  debouncedSearch(e.target.value);
});
```

### 3. Caching Results

```javascript
class AsyncCache {
  constructor() {
    this.cache = new Map();
  }
  
  async get(key, fetchFn, ttl = 60000) {
    const cached = this.cache.get(key);
    
    if (cached && Date.now() - cached.timestamp < ttl) {
      console.log('Cache hit:', key);
      return cached.data;
    }
    
    console.log('Cache miss:', key);
    const data = await fetchFn();
    
    this.cache.set(key, {
      data,
      timestamp: Date.now()
    });
    
    return data;
  }
  
  clear() {
    this.cache.clear();
  }
}

// Usage
const cache = new AsyncCache();

async function getUser(id) {
  return cache.get(
    `user_${id}`,
    () => fetch(`/api/users/${id}`).then(r => r.json()),
    30000 // 30 second TTL
  );
}

// First call: fetches from API
await getUser(1);

// Second call within 30s: returns from cache
await getUser(1);
```

### 4. Queue Processing

```javascript
class AsyncQueue {
  constructor(concurrency = 1) {
    this.concurrency = concurrency;
    this.running = 0;
    this.queue = [];
  }
  
  async add(asyncFn) {
    return new Promise((resolve, reject) => {
      this.queue.push({ asyncFn, resolve, reject });
      this.process();
    });
  }
  
  async process() {
    if (this.running >= this.concurrency || this.queue.length === 0) {
      return;
    }
    
    this.running++;
    const { asyncFn, resolve, reject } = this.queue.shift();
    
    try {
      const result = await asyncFn();
      resolve(result);
    } catch (error) {
      reject(error);
    } finally {
      this.running--;
      this.process();
    }
  }
}

// Usage
const queue = new AsyncQueue(2); // Process 2 at a time

const tasks = [1, 2, 3, 4, 5].map(i => 
  queue.add(() => {
    console.log(`Processing task ${i}`);
    return new Promise(resolve => 
      setTimeout(() => resolve(i * 2), 1000)
    );
  })
);

Promise.all(tasks).then(results => {
  console.log('All tasks completed:', results);
});
```

### 5. Promise Memoization

```javascript
function memoizeAsync(fn) {
  const cache = new Map();
  
  return async function(...args) {
    const key = JSON.stringify(args);
    
    if (cache.has(key)) {
      return cache.get(key);
    }
    
    const promise = fn(...args);
    cache.set(key, promise);
    
    try {
      const result = await promise;
      return result;
    } catch (error) {
      cache.delete(key); // Remove failed promise from cache
      throw error;
    }
  };
}

// Usage
const fetchUser = memoizeAsync(async (id) => {
  console.log(`Fetching user ${id}`);
  const response = await fetch(`/api/users/${id}`);
  return response.json();
});

// First call: fetches from API
await fetchUser(1); // Logs: "Fetching user 1"

// Second call: returns cached promise
await fetchUser(1); // No log, returns immediately
```

### 6. Graceful Degradation

```javascript
async function fetchWithFallback(primaryUrl, fallbackUrl) {
  try {
    const response = await fetch(primaryUrl);
    if (!response.ok) throw new Error('Primary failed');
    return await response.json();
  } catch (error) {
    console.warn('Primary source failed, trying fallback');
    
    try {
      const response = await fetch(fallbackUrl);
      if (!response.ok) throw new Error('Fallback failed');
      return await response.json();
    } catch (fallbackError) {
      console.error('Both sources failed');
      return getDefaultData(); // Return default/cached data
    }
  }
}

function getDefaultData() {
  return {
    message: 'Using default data',
    data: []
  };
}
```

---

## Common Pitfalls

### 1. Forgetting to Return in Promise Chains

```javascript
// ❌ Bad: Forgetting to return
function bad() {
  fetchUser(1)
    .then(user => {
      fetchPosts(user.id); // Missing return!
    })
    .then(posts => {
      console.log(posts); // undefined!
    });
}

// ✅ Good: Returning promise
function good() {
  fetchUser(1)
    .then(user => {
      return fetchPosts(user.id); // Return the promise
    })
    .then(posts => {
      console.log(posts); // Works correctly
    });
}
```

### 2. Not Handling Errors

```javascript
// ❌ Bad: Unhandled promise rejection
async function bad() {
  const data = await fetch('/api/data').then(r => r.json());
  // If fetch fails, unhandled rejection!
}

// ✅ Good: Proper error handling
async function good() {
  try {
    const data = await fetch('/api/data').then(r => r.json());
    return data;
  } catch (error) {
    console.error('Error:', error);
    return null;
  }
}

// ✅ Alternative: Let caller handle
async function goodAlternative() {
  const data = await fetch('/api/data').then(r => r.json());
  return data;
}

goodAlternative().catch(error => console.error('Handled:', error));
```

### 3. Sequential Instead of Parallel

```javascript
// ❌ Bad: Unnecessarily sequential (slow)
async function bad() {
  const user = await fetchUser(1);      // 1 second
  const posts = await fetchPosts(1);    // 1 second
  const comments = await fetchComments(1); // 1 second
  // Total: 3 seconds
}

// ✅ Good: Parallel execution (fast)
async function good() {
  const [user, posts, comments] = await Promise.all([
    fetchUser(1),
    fetchPosts(1),
    fetchComments(1)
  ]);
  // Total: ~1 second
}
```

### 4. Using async in Array Methods Incorrectly

```javascript
// ❌ Bad: forEach doesn't wait for async
async function bad(ids) {
  ids.forEach(async (id) => {
    const user = await fetchUser(id);
    console.log(user);
  });
  console.log('Done'); // Logs before users are fetched!
}

// ✅ Good: Use for...of
async function good(ids) {
  for (const id of ids) {
    const user = await fetchUser(id);
    console.log(user);
  }
  console.log('Done'); // Logs after all users are fetched
}

// ✅ Good: Parallel with Promise.all
async function goodParallel(ids) {
  const users = await Promise.all(
    ids.map(id => fetchUser(id))
  );
  users.forEach(user => console.log(user));
  console.log('Done');
}
```

### 5. Floating Promises

```javascript
// ❌ Bad: Floating promise (not awaited or handled)
async function bad() {
  updateDatabase(); // Returns promise but not awaited!
  console.log('Database updated'); // Might log before update completes
}

// ✅ Good: Await the promise
async function good() {
  await updateDatabase();
  console.log('Database updated'); // Logs after update completes
}

// ✅ Good: Handle with .catch() if not awaiting
async function goodAlternative() {
  updateDatabase().catch(error => console.error('Update failed:', error));
  console.log('Update initiated');
}
```

### 6. Not Checking Response.ok with Fetch

```javascript
// ❌ Bad: Fetch doesn't reject on HTTP errors
async function bad(url) {
  try {
    const response = await fetch(url);
    return await response.json(); // Might fail if status is 404, 500, etc.
  } catch (error) {
    console.error(error); // Won't catch HTTP errors!
  }
}

// ✅ Good: Check response.ok
async function good(url) {
  try {
    const response = await fetch(url);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

### 7. Race Conditions

```javascript
// ❌ Bad: Race condition
let latestRequestId = 0;

async function bad(query) {
  const results = await search(query);
  displayResults(results); // Might display old results!
}

// User types: "hello"
// Request 1: search("hel")
// Request 2: search("hello")
// If Request 1 finishes after Request 2, shows wrong results!

// ✅ Good: Use request ID to prevent race conditions
let latestRequestId = 0;

async function good(query) {
  const requestId = ++latestRequestId;
  const results = await search(query);
  
  // Only display if this is still the latest request
  if (requestId === latestRequestId) {
    displayResults(results);
  }
}
```

### 8. Promise Constructor Anti-pattern

```javascript
// ❌ Bad: Wrapping promise in promise (unnecessary)
async function bad() {
  return new Promise(async (resolve, reject) => {
    try {
      const data = await fetchData();
      resolve(data);
    } catch (error) {
      reject(error);
    }
  });
}

// ✅ Good: Just return the promise
async function good() {
  return await fetchData();
}

// ✅ Even better: Don't await if just returning
async function better() {
  return fetchData();
}
```

---

## Interview Questions

### Question 1: What is the difference between synchronous and asynchronous code?

**Answer:**
- **Synchronous**: Code executes line by line, blocking until each operation completes. If an operation takes time, everything waits.
- **Asynchronous**: Operations run in the background, allowing other code to execute without waiting. Useful for I/O operations, network requests, timers.

```javascript
// Synchronous - blocks for 2 seconds
const result = slowFunction(); // Waits here
console.log(result);

// Asynchronous - doesn't block
setTimeout(() => {
  console.log('Done');
}, 2000); // Continues immediately
console.log('Waiting...');
```

### Question 2: Explain the Promise states.

**Answer:**
A Promise has three states:
1. **Pending**: Initial state, operation hasn't completed yet
2. **Fulfilled**: Operation completed successfully (resolved)
3. **Rejected**: Operation failed

Once a promise is fulfilled or rejected, it's **settled** and cannot change state.

```javascript
const promise = new Promise((resolve, reject) => {
  // Pending state
  setTimeout(() => {
    resolve('Success'); // Now fulfilled
    // or
    reject('Error'); // Now rejected
  }, 1000);
});
```

### Question 3: What's the difference between Promise.all() and Promise.allSettled()?

**Answer:**

| Feature | Promise.all() | Promise.allSettled() |
|---------|---------------|----------------------|
| **Waits for** | All promises to resolve | All promises to settle |
| **Rejects when** | Any promise rejects | Never rejects |
| **Returns** | Array of values | Array of result objects |
| **Use case** | All must succeed | Want all results regardless |

```javascript
// Promise.all - rejects if any fails
Promise.all([Promise.resolve(1), Promise.reject('Error')])
  .catch(error => console.log('Rejected:', error));

// Promise.allSettled - never rejects
Promise.allSettled([Promise.resolve(1), Promise.reject('Error')])
  .then(results => console.log(results));
// [
//   { status: 'fulfilled', value: 1 },
//   { status: 'rejected', reason: 'Error' }
// ]
```

### Question 4: How does async/await differ from Promises?

**Answer:**
- **async/await** is syntactic sugar over Promises
- Makes asynchronous code look synchronous
- Easier to read and reason about
- Better error handling with try/catch
- An async function always returns a Promise

```javascript
// With Promises
function fetchUserPromise() {
  return fetch('/api/user')
    .then(res => res.json())
    .then(user => user.name)
    .catch(error => console.error(error));
}

// With async/await
async function fetchUserAsync() {
  try {
    const res = await fetch('/api/user');
    const user = await res.json();
    return user.name;
  } catch (error) {
    console.error(error);
  }
}
```

### Question 5: What happens if you don't await a Promise in an async function?

**Answer:**
The function continues without waiting for the Promise to resolve. This creates a "floating promise" that may cause:
1. Execution order issues
2. Unhandled promise rejections
3. Race conditions

```javascript
// ❌ Problem: Not awaiting
async function problem() {
  saveToDatabase(); // Returns promise but not awaited
  console.log('Saved'); // May log before save completes
}

// ✅ Solution: Await the promise
async function solution() {
  await saveToDatabase();
  console.log('Saved'); // Logs after save completes
}
```

### Question 6: How do you handle errors in async/await?

**Answer:**
Use try/catch blocks:

```javascript
async function fetchData() {
  try {
    const response = await fetch('/api/data');
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Fetch failed:', error);
    throw error; // Re-throw or handle
  } finally {
    // Cleanup code (always runs)
    console.log('Request completed');
  }
}

// Or let caller handle
fetchData().catch(error => console.error('Handled by caller:', error));
```

### Question 7: When should you use Promise.all() vs sequential await?

**Answer:**

**Use Promise.all() (parallel)** when:
- Operations are independent
- Want faster execution
- Don't need results from one to start another

**Use sequential await** when:
- Operations depend on previous results
- Need to control execution order
- Want to limit concurrent operations

```javascript
// Parallel (faster) - independent operations
async function parallel() {
  const [users, posts, comments] = await Promise.all([
    fetchUsers(),
    fetchPosts(),
    fetchComments()
  ]);
}

// Sequential (necessary) - dependent operations
async function sequential(userId) {
  const user = await fetchUser(userId);
  const posts = await fetchPosts(user.id); // Needs user.id
  const comments = await fetchComments(posts[0].id); // Needs post id
}
```

### Question 8: What is the event loop and how does it relate to async code?

**Answer:**
The event loop is JavaScript's mechanism for handling asynchronous operations:

1. **Call Stack**: Executes synchronous code
2. **Web APIs**: Handle async operations (setTimeout, fetch, etc.)
3. **Callback Queue**: Stores completed async callbacks
4. **Microtask Queue**: Stores promise callbacks (higher priority)
5. **Event Loop**: Moves callbacks from queue to call stack when stack is empty

```javascript
console.log('1'); // Call stack

setTimeout(() => console.log('2'), 0); // Task queue

Promise.resolve().then(() => console.log('3')); // Microtask queue

console.log('4'); // Call stack

// Output: 1, 4, 3, 2
// Microtasks run before tasks
```

> **For more details, see [11-interview/event_loop.md](./11-interview/event_loop.md)**

### Question 9: What are common pitfalls with async/await?

**Answer:**

1. **Forgetting await**: Creates floating promises
2. **Sequential when parallel would work**: Slower execution
3. **Not handling errors**: Unhandled rejections
4. **Using async in forEach**: Doesn't wait for promises
5. **Not checking response.ok with fetch**: Doesn't catch HTTP errors

```javascript
// ❌ Pitfalls
async function pitfalls(ids) {
  // Pitfall 1: Not awaiting
  fetchData(); // Continues without waiting
  
  // Pitfall 2: Sequential when parallel works
  const u1 = await fetchUser(1); // Wait
  const u2 = await fetchUser(2); // Wait
  
  // Pitfall 3: async in forEach
  ids.forEach(async (id) => {
    await processId(id); // forEach doesn't wait
  });
}

// ✅ Solutions
async function solutions(ids) {
  // Solution 1: Await promises
  await fetchData();
  
  // Solution 2: Parallel execution
  const [u1, u2] = await Promise.all([
    fetchUser(1),
    fetchUser(2)
  ]);
  
  // Solution 3: Use for...of
  for (const id of ids) {
    await processId(id);
  }
}
```

### Question 10: How do you implement timeout for async operations?

**Answer:**

```javascript
// Method 1: Promise.race with timeout promise
async function withTimeout(promise, ms) {
  const timeout = new Promise((_, reject) =>
    setTimeout(() => reject(new Error('Timeout')), ms)
  );
  
  return Promise.race([promise, timeout]);
}

// Usage
try {
  const data = await withTimeout(fetch('/api/data'), 5000);
} catch (error) {
  console.error('Request timeout');
}

// Method 2: AbortController (for fetch)
async function fetchWithTimeout(url, ms) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), ms);
  
  try {
    const response = await fetch(url, { signal: controller.signal });
    clearTimeout(timeout);
    return response.json();
  } catch (error) {
    clearTimeout(timeout);
    if (error.name === 'AbortError') {
      throw new Error('Request timeout');
    }
    throw error;
  }
}
```

---

## Summary

### Key Concepts

1. **Asynchronous Programming**: Non-blocking operations that allow other code to run
2. **Callbacks**: Functions passed as arguments, basis of async (see [09-callback-functions.md](./09-callback-functions.md))
3. **Promises**: Objects representing eventual completion/failure of async operations
4. **Async/Await**: Syntactic sugar making async code look synchronous
5. **Event Loop**: JavaScript's mechanism for handling async operations (see [11-interview/event_loop.md](./11-interview/event_loop.md))

### Promise States
- **Pending** → **Fulfilled** or **Rejected**
- Once settled, state cannot change

### Promise Methods

```javascript
// Instance methods
.then()     // Handle fulfillment
.catch()    // Handle rejection
.finally()  // Always runs

// Static methods
Promise.all()         // All must resolve
Promise.race()        // First to settle
Promise.allSettled()  // All settle, never rejects
Promise.any()         // First to resolve
Promise.resolve()     // Create resolved promise
Promise.reject()      // Create rejected promise
```

### Best Practices

1. ✅ **Always handle errors** (try/catch or .catch())
2. ✅ **Use parallel execution** when operations are independent
3. ✅ **Check response.ok** with fetch API
4. ✅ **Return promises** in promise chains
5. ✅ **Use async/await** for cleaner code
6. ✅ **Implement retry logic** for network requests
7. ✅ **Add timeouts** to prevent hanging requests
8. ✅ **Use for...of** instead of forEach with async

### Common Patterns

```javascript
// Retry logic
fetchWithRetry(url, maxRetries)

// Timeout
Promise.race([fetch(url), timeoutPromise])

// Batch processing
processBatch(items, batchSize)

// Caching
cache.get(key, fetchFn, ttl)

// Queue processing
queue.add(asyncFn)
```

### When to Use What

| Scenario | Use |
|----------|-----|
| Single async operation | async/await |
| Multiple independent operations | Promise.all() |
| Need first result | Promise.race() |
| Want all results (mixed success/failure) | Promise.allSettled() |
| Need any success | Promise.any() |
| Sequential dependent operations | Sequential await |
| Error handling | try/catch with async/await |

### Performance Tips

1. **Parallel > Sequential** for independent operations
2. **Batch operations** instead of one-by-one
3. **Cache results** to avoid redundant requests
4. **Use debouncing** for frequent operations
5. **Implement retries** with exponential backoff

---

## Additional Resources

- **Related Files:**
  - [09-callback-functions.md](./09-callback-functions.md) - Detailed callback information
  - [11-interview/event_loop.md](./11-interview/event_loop.md) - Event loop deep dive
  - [11-interview/callback.md](./11-interview/callback.md) - Callback interview questions

- **MDN Documentation:**
  - [Promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise)
  - [async function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function)
  - [await](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await)
  - [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)

---

**End of Asynchronous JavaScript Guide**
