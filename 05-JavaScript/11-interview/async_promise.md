# 🔮 Promises in JavaScript

## 🔹 What is a Promise?

A **Promise** in JavaScript is an **object that represents the eventual
completion (or failure) of an asynchronous operation**.\
It acts like a **placeholder** for a value that will be available **in
the future**.

👉 Think of it as a **food delivery order**:\
- You place an order (async task starts).\
- You get a receipt (Promise object).\
- Later, the food is either delivered (**fulfilled**) or the restaurant
says sorry (**rejected**).

------------------------------------------------------------------------

## 🔹 States of a Promise

1.  **Pending** → Initial state, waiting.\
2.  **Fulfilled** → Operation completed successfully (`resolve`).\
3.  **Rejected** → Operation failed (`reject`).

------------------------------------------------------------------------

## 🔹 Creating a Promise

``` js
const myPromise = new Promise((resolve, reject) => {
  let success = true;

  setTimeout(() => {
    if (success) {
      resolve("✅ Task completed!");
    } else {
      reject("❌ Something went wrong.");
    }
  }, 2000);
});
```

------------------------------------------------------------------------

## 🔹 Consuming a Promise

### Using `.then()`, `.catch()`, `.finally()`

``` js
myPromise
  .then(result => console.log(result))    // if resolved
  .catch(error => console.error(error))   // if rejected
  .finally(() => console.log("Done"));    // always runs
```

------------------------------------------------------------------------

## 🔹 Async/Await (Modern Way)

``` js
async function runTask() {
  try {
    const result = await myPromise;
    console.log(result); // ✅ Task completed!
  } catch (error) {
    console.error(error);
  } finally {
    console.log("Done");
  }
}
runTask();
```

------------------------------------------------------------------------

## 🔹 Real-World Example (Fetching Data)

### With Promises

``` js
fetch("https://jsonplaceholder.typicode.com/posts/1")
  .then(response => response.json())
  .then(data => console.log("Post:", data))
  .catch(err => console.error("Error fetching:", err));
```

### With Async/Await

``` js
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

------------------------------------------------------------------------

## 🔹 Why Promises Are Important

1.  Avoids **callback hell** (messy nested callbacks).\
2.  Makes async code look **cleaner**.\
3.  Powers modern features like **fetch API**, **async/await**, **React
    data fetching**, etc.

------------------------------------------------------------------------

## ✅ Summary

A **Promise** is a way to handle asynchronous tasks in JavaScript.\
- It has **3 states**: pending, fulfilled, rejected.\
- You consume it with `.then()`, `.catch()`, `.finally()` or
`async/await`.\
- Promises make async code easier, cleaner, and more reliable.
