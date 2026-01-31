# 🔄 Generator Functions in JavaScript

## 🔹 What is a Generator Function?

A **generator function** is a special type of function that can **pause
its execution** and later **resume from where it left off**.\
They are defined using the `function*` syntax.

👉 Analogy: Like watching a **TV series** 🎬 --- you can **pause after
an episode** and **resume later**, unlike a movie (normal function) that
runs all at once.

------------------------------------------------------------------------

## 🔹 Syntax

``` js
function* generatorFunction() {
  yield 1;
  yield 2;
  yield 3;
}
```

-   `yield` → pauses the function and returns a value.\
-   When called, a generator returns an **iterator object**.

------------------------------------------------------------------------

## 🔹 Using a Generator

``` js
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

-   Each `next()` resumes execution until the next `yield`.\
-   When done, `done: true` is returned.

------------------------------------------------------------------------

## 🔹 Real-World Uses of Generators

### 1. **Lazy Evaluation**

Generate values only when needed.

``` js
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

------------------------------------------------------------------------

### 2. **Infinite Sequences**

``` js
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

------------------------------------------------------------------------

### 3. **Asynchronous Programming (before async/await)**

Generators were once used with libraries like `co` for async tasks.

``` js
function* asyncTask() {
  const result = yield fetch("https://jsonplaceholder.typicode.com/posts/1");
  console.log(result);
}
```

------------------------------------------------------------------------

## 🔹 Difference from Normal Functions

  -----------------------------------------------------------------------
  Normal Function                 Generator Function
  ------------------------------- ---------------------------------------
  Runs start to finish            Can pause (`yield`) and resume

  Returns a single value          Can return multiple values (via
                                  `yield`)

  Called once → runs fully        Called once → returns iterator, run
                                  with `next()`

  Not iterable                    Iterable (usable in `for...of`)
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## ✅ Summary

-   A **generator function** (`function*`) can **pause and resume**
    execution using `yield`.\
-   They are useful for **iterators, lazy evaluation, infinite
    sequences, and async-like patterns**.\
-   Think of it as a **TV series you can pause/resume**, while normal
    functions are a **movie that runs straight through**.
