# JavaScript Syntax

## What is JavaScript?

JavaScript is a high-level, interpreted programming language primarily used for web development. It makes web pages interactive and dynamic.

---

## ECMAScript Versions

JavaScript follows the **ECMAScript (ES)** specification standard.

### Version History Timeline

| Version | Year | Key Features |
|---------|------|--------------|
| ES5 | 2009 | Strict mode, JSON support, array methods |
| ES6 (ES2015) | 2015 | let/const, arrow functions, classes, modules, promises |
| ES2016 | 2016 | Array.includes(), exponentiation operator |
| ES2017 | 2017 | async/await, Object.entries/values |
| ES2018 | 2018 | Rest/spread for objects, async iteration |
| ES2019 | 2019 | Array.flat(), Object.fromEntries() |
| ES2020 | 2020 | Optional chaining (?.), nullish coalescing (??) |
| ES2021 | 2021 | Logical assignment operators, Promise.any() |
| ES2022 | 2022 | Top-level await, class fields |
| ES2023+ | 2023+ | Ongoing development |

**Modern JavaScript** typically refers to ES6 (ES2015) and later.

---

## Comments

Comments are used to explain code and are ignored by the JavaScript engine.

### Single-Line Comments

```javascript
// This is a single-line comment
let name = 'Alice'; // Comment after code
```

### Multi-Line Comments

```javascript
/*
  This is a multi-line comment
  It can span multiple lines
  Useful for longer explanations
*/

function greet() {
  /* This also works */
  console.log('Hello');
}
```

### Documentation Comments (JSDoc)

```javascript
/**
 * Calculates the sum of two numbers
 * @param {number} a - First number
 * @param {number} b - Second number
 * @returns {number} The sum of a and b
 */
function add(a, b) {
  return a + b;
}
```

---

## Statements vs Expressions

### Statements

A **statement** performs an action but doesn't produce a value.

```javascript
// Variable declaration statement
let x = 5;

// If statement
if (x > 0) {
  console.log('Positive');
}

// Loop statement
for (let i = 0; i < 5; i++) {
  console.log(i);
}

// Function declaration statement
function greet() {
  console.log('Hello');
}
```

### Expressions

An **expression** produces a value.

```javascript
// Arithmetic expression
5 + 3          // Produces 8

// String expression
'Hello' + ' World'  // Produces 'Hello World'

// Function expression
const add = function(a, b) {
  return a + b;
};

// Ternary expression
x > 0 ? 'positive' : 'negative'

// Assignment is an expression
let y = (x = 10);  // Both x and y are 10
```

**Key Difference:**
- **Statement**: Does something (performs an action)
- **Expression**: Is something (produces a value)

```javascript
// Expression (can be used where a value is expected)
console.log(2 + 2);  // 2 + 2 is an expression

// Statement (cannot be used where a value is expected)
let x = if (true) { 5 };  // ❌ Syntax Error
let x = true ? 5 : 0;     // ✅ Ternary is an expression
```

---

## Semicolons

### Automatic Semicolon Insertion (ASI)

JavaScript has **Automatic Semicolon Insertion (ASI)**, which automatically adds semicolons at line breaks in certain situations.

```javascript
// With semicolons (explicit)
let a = 5;
let b = 10;
console.log(a + b);

// Without semicolons (ASI adds them)
let a = 5
let b = 10
console.log(a + b)  // Works, but not recommended
```

### When ASI Can Cause Problems

```javascript
// Problem 1: Return statement
function getValue() {
  return
    { value: 42 }  // ❌ Returns undefined (ASI inserts ; after return)
}

// Fix
function getValue() {
  return { value: 42 };  // ✅ Works correctly
}

// Problem 2: Array access
const arr = [1, 2, 3]
[0, 1].forEach(x => console.log(x))  // ❌ Error!

// Fix
const arr = [1, 2, 3];
[0, 1].forEach(x => console.log(x));  // ✅ Works
```

**Best Practice**: Always use semicolons explicitly to avoid unexpected behavior.

---

## Code Blocks

A **code block** is a group of statements enclosed in curly braces `{}`.

```javascript
// Block with if statement
if (true) {
  let x = 10;
  console.log(x);  // 10
}
console.log(x);    // ❌ ReferenceError (x is block-scoped)

// Block with function
function test() {
  let y = 20;
  console.log(y);  // 20
}

// Block with loop
for (let i = 0; i < 3; i++) {
  console.log(i);
}

// Standalone block (rarely used)
{
  let z = 30;
  console.log(z);  // 30
}
console.log(z);    // ❌ ReferenceError
```

**Block Scope**: Variables declared with `let` and `const` are scoped to the block they're in.

---

## Strict Mode

**Strict mode** makes JavaScript more secure and helps catch common coding mistakes.

### Enabling Strict Mode

```javascript
// Entire script
'use strict';

let x = 10;
delete x;  // ❌ Error in strict mode

// Function only
function strictFunction() {
  'use strict';
  // Strict mode applies here
}
```

### What Strict Mode Does

1. **Prevents undeclared variables**
```javascript
'use strict';
x = 10;  // ❌ ReferenceError (x is not declared)
```

2. **Prevents deleting variables**
```javascript
'use strict';
let x = 10;
delete x;  // ❌ SyntaxError
```

3. **Prevents duplicate parameter names**
```javascript
'use strict';
function test(a, a, b) {  // ❌ SyntaxError
  return a + b;
}
```

4. **Makes `this` undefined in functions**
```javascript
'use strict';
function test() {
  console.log(this);  // undefined (not Window)
}
test();
```

5. **Prevents octal syntax**
```javascript
'use strict';
let x = 010;  // ❌ SyntaxError (octal literals not allowed)
```

**Best Practice**: Always use strict mode in modern JavaScript.

---

## JavaScript Syntax Rules

### 1. Case Sensitivity

JavaScript is **case-sensitive**.

```javascript
let name = 'Alice';
let Name = 'Bob';
let NAME = 'Charlie';

// These are three different variables
console.log(name);  // 'Alice'
console.log(Name);  // 'Bob'
console.log(NAME);  // 'Charlie'
```

### 2. Identifiers (Variable Names)

Rules for naming variables:
- Must start with a letter, `$`, or `_`
- Can contain letters, digits, `$`, or `_`
- Cannot be a reserved keyword
- Convention: use camelCase

```javascript
// ✅ Valid identifiers
let userName = 'Alice';
let _private = 'secret';
let $element = document.querySelector('div');
let age2 = 25;

// ❌ Invalid identifiers
let 2name = 'Bob';      // Starts with digit
let user-name = 'Charlie'; // Contains hyphen
let class = 'ES6';      // Reserved keyword
```

### 3. Reserved Keywords

Cannot be used as identifiers:

```
break, case, catch, class, const, continue, debugger, default, delete,
do, else, export, extends, finally, for, function, if, import, in,
instanceof, let, new, return, super, switch, this, throw, try, typeof,
var, void, while, with, yield
```

### 4. Whitespace

JavaScript ignores extra whitespace (except in strings).

```javascript
// These are equivalent
let x=5;
let x = 5;
let   x   =   5   ;

// Whitespace in strings is preserved
let str1 = 'hello world';     // 'hello world'
let str2 = 'hello    world';  // 'hello    world'
```

---

## Code Structure Best Practices

### 1. Indentation

```javascript
// ✅ Good: Consistent indentation
function greet(name) {
  if (name) {
    console.log(`Hello, ${name}!`);
  } else {
    console.log('Hello!');
  }
}

// ❌ Bad: Inconsistent indentation
function greet(name) {
if (name) {
      console.log(`Hello, ${name}!`);
    } else {
console.log('Hello!');
}
}
```

### 2. Line Length

Keep lines under 80-100 characters for readability.

```javascript
// ✅ Good: Break long lines
const message = 'This is a very long message that should be ' +
                'broken into multiple lines for better readability';

// ❌ Bad: Very long line
const message = 'This is a very long message that should be broken into multiple lines for better readability';
```

### 3. Naming Conventions

```javascript
// Variables and functions: camelCase
let userName = 'Alice';
function getUserData() {}

// Constants: UPPER_CASE
const MAX_SIZE = 100;
const API_URL = 'https://api.example.com';

// Classes: PascalCase
class UserProfile {}
class HttpRequest {}

// Private properties: prefix with _
class User {
  constructor() {
    this._privateData = 'secret';
  }
}
```

---

## Console Operations

### Console Methods

```javascript
// Basic logging
console.log('Hello World');
console.log('Value:', 42);

// Warning
console.warn('This is a warning');

// Error
console.error('This is an error');

// Info
console.info('Information message');

// Table (for objects/arrays)
console.table([
  { name: 'Alice', age: 25 },
  { name: 'Bob', age: 30 }
]);

// Group logs
console.group('User Details');
console.log('Name: Alice');
console.log('Age: 25');
console.groupEnd();

// Timing
console.time('Operation');
// ... some code ...
console.timeEnd('Operation');  // Logs time elapsed

// Clear console
console.clear();
```

---

## Running JavaScript

### 1. In Browser Console

Open DevTools (F12) and use the Console tab.

```javascript
console.log('Hello from console!');
```

### 2. In HTML File

```html
<!DOCTYPE html>
<html>
<head>
  <title>JavaScript Example</title>
</head>
<body>
  <!-- Inline JavaScript -->
  <script>
    console.log('Inline JavaScript');
  </script>
  
  <!-- External JavaScript file -->
  <script src="script.js"></script>
</body>
</html>
```

### 3. With Node.js

```javascript
// Save as script.js
console.log('Hello from Node.js!');

// Run in terminal
// node script.js
```

---

## Summary

### Key Points

1. **Comments**: `//` for single-line, `/* */` for multi-line
2. **Statements vs Expressions**: Statements do things, expressions produce values
3. **Semicolons**: Use them explicitly to avoid ASI issues
4. **Code Blocks**: Group statements with `{}`
5. **Strict Mode**: Use `'use strict';` for safer code
6. **Case Sensitive**: `name`, `Name`, and `NAME` are different
7. **Naming**: Use camelCase for variables, PascalCase for classes
8. **ES Versions**: Modern JavaScript is ES6+ (2015 onwards)

### Best Practices

- ✅ Always use strict mode
- ✅ Use semicolons explicitly
- ✅ Follow naming conventions
- ✅ Comment your code when necessary
- ✅ Use consistent indentation
- ✅ Keep code readable and organized

Master JavaScript syntax to write clean, maintainable code! 🚀
