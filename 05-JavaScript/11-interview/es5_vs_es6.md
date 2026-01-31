# ⚡ ES5 vs ES6 in JavaScript

## 🔹 What is ES5 and ES6?

-   **ES5 (ECMAScript 5)** → Released in **2009**, supported in all
    browsers.\
-   **ES6 (ECMAScript 2015)** → Released in **2015**, introduced many
    modern features.

------------------------------------------------------------------------

## 🔹 Key Differences

### 1. Variable Declaration

``` js
// ES5
var x = 10;

// ES6
let y = 20;
const z = 30;
```

------------------------------------------------------------------------

### 2. Functions

``` js
// ES5
var add = function(a, b) {
  return a + b;
};

// ES6
const add = (a, b) => a + b;
```

------------------------------------------------------------------------

### 3. Template Literals

``` js
// ES5
var name = "Alice";
console.log("Hello " + name + "!");

// ES6
const name = "Alice";
console.log(`Hello ${name}!`);
```

------------------------------------------------------------------------

### 4. Default Parameters

``` js
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

------------------------------------------------------------------------

### 5. Destructuring (ES6 only)

``` js
const arr = [1, 2, 3];
const [a, b] = arr; // a=1, b=2

const obj = { name: "Alice", age: 25 };
const { name, age } = obj;
```

------------------------------------------------------------------------

### 6. Modules

``` js
// ES5 → No native support (used CommonJS/RequireJS)

// ES6
export const pi = 3.14;
import { pi } from "./math.js";
```

------------------------------------------------------------------------

### 7. Classes

``` js
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

------------------------------------------------------------------------

### 8. Promises

``` js
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

------------------------------------------------------------------------

### 9. Other ES6 Features

-   Rest & Spread operators (`...`)\
-   For-of loop\
-   Symbols (new primitive type)\
-   Map, Set, WeakMap, WeakSet\
-   Generators (`function*`)

------------------------------------------------------------------------

## 🔹 Summary (Cheat Sheet)

  --------------------------------------------------------------------------------
  Feature                                          ES5               ES6
  ------------------------------------------------ ----------------- -------------
  Variable declaration                             `var` only        `let`,
                                                                     `const`

  Functions                                        Regular           Arrow
                                                                     functions

  Strings                                          Concatenation     Template
                                                                     literals

  Params                                           Manual default    Default
                                                   (`||`)            parameters

  Destructuring                                    ❌ No             ✅ Yes

  Modules                                          No native support `import` /
                                                                     `export`

  Classes                                          Prototype-based   `class`
                                                                     keyword

  Async                                            Callbacks         Promises
                                                                     (later
                                                                     async/await
                                                                     in ES2017)

  Collections                                      Objects, Arrays   Map, Set,
                                                                     WeakMap,
                                                                     WeakSet
  --------------------------------------------------------------------------------

------------------------------------------------------------------------

## ✅ Conclusion

-   **ES5** = Old JavaScript (still everywhere).\
-   **ES6** = Modern JavaScript → cleaner syntax, modules, better async,
    new data structures.
