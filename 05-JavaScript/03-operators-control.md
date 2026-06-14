# JavaScript Operators and Control Flow

A comprehensive guide covering all JavaScript operators and control flow structures essential for technical interviews.

---

## Table of Contents

1. [Operators](#operators)
   - [Arithmetic Operators](#1-arithmetic-operators)
   - [Assignment Operators](#2-assignment-operators)
   - [Comparison Operators](#3-comparison-operators)
   - [Logical Operators](#4-logical-operators)
   - [Bitwise Operators](#5-bitwise-operators)
   - [Unary Operators](#6-unary-operators)
   - [Ternary Operator](#7-ternary-operator)
   - [Nullish Coalescing Operator](#8-nullish-coalescing-operator)
   - [Optional Chaining Operator](#9-optional-chaining-operator)
   - [Comma Operator](#10-comma-operator)
2. [Truthy and Falsy Values](#truthy-and-falsy-values)
3. [Control Flow Structures](#control-flow-structures)
4. [Interview Questions](#interview-questions)
5. [Best Practices](#best-practices)
6. [Summary](#summary)

---

## Operators

### 1. Arithmetic Operators

Perform mathematical calculations.

```javascript
let a = 10, b = 3;

// Addition
console.log(a + b);  // 13

// Subtraction
console.log(a - b);  // 7

// Multiplication
console.log(a * b);  // 30

// Division
console.log(a / b);  // 3.3333...

// Modulus (remainder)
console.log(a % b);  // 1

// Exponentiation (ES2016)
console.log(a ** b); // 1000 (10^3)

// Increment
let x = 5;
console.log(++x);    // 6 (pre-increment)
console.log(x++);    // 6 (post-increment, x becomes 7)

// Decrement
let y = 5;
console.log(--y);    // 4 (pre-decrement)
console.log(y--);    // 4 (post-decrement, y becomes 3)
```

**String Concatenation with +:**

```javascript
console.log('Hello' + ' ' + 'World');  // 'Hello World'
console.log('5' + 3);                   // '53' (string)
console.log(5 + '3');                   // '53' (string)
console.log('5' + 3 + 2);               // '532'
console.log(5 + 3 + '2');               // '82'
```

---

### 2. Assignment Operators

Assign values to variables.

```javascript
let x = 10;

// Basic assignment
x = 5;           // x = 5

// Addition assignment
x += 3;          // x = x + 3  → x = 8

// Subtraction assignment
x -= 2;          // x = x - 2  → x = 6

// Multiplication assignment
x *= 4;          // x = x * 4  → x = 24

// Division assignment
x /= 3;          // x = x / 3  → x = 8

// Modulus assignment
x %= 5;          // x = x % 5  → x = 3

// Exponentiation assignment
x **= 2;         // x = x ** 2 → x = 9

// Logical AND assignment (ES2021)
let a = true;
a &&= false;     // a = a && false → a = false

// Logical OR assignment (ES2021)
let b = false;
b ||= true;      // b = b || true → b = true

// Nullish coalescing assignment (ES2021)
let c = null;
c ??= 'default'; // c = c ?? 'default' → c = 'default'
```

---

### 3. Comparison Operators

Compare values and return a boolean.

#### == vs === (Equality vs Strict Equality)

**`==` (Abstract Equality):** Performs type coercion before comparison.

**`===` (Strict Equality):** No type coercion; checks both value and type.

```javascript
// == (loose equality)
console.log(5 == '5');        // true (type coercion)
console.log(null == undefined); // true
console.log(0 == false);      // true
console.log('' == false);     // true
console.log([] == false);     // true

// === (strict equality)
console.log(5 === '5');       // false (different types)
console.log(null === undefined); // false
console.log(0 === false);     // false
console.log('' === false);    // false
console.log([] === false);    // false

// Object comparison
const obj1 = { name: 'Alice' };
const obj2 = { name: 'Alice' };
const obj3 = obj1;

console.log(obj1 == obj2);    // false (different references)
console.log(obj1 === obj2);   // false (different references)
console.log(obj1 === obj3);   // true (same reference)
```

**All Comparison Operators:**

```javascript
let a = 10, b = 5;

// Equal to
console.log(a == b);   // false

// Strict equal to
console.log(a === b);  // false

// Not equal to
console.log(a != b);   // true

// Strict not equal to
console.log(a !== b);  // true

// Greater than
console.log(a > b);    // true

// Less than
console.log(a < b);    // false

// Greater than or equal to
console.log(a >= b);   // true

// Less than or equal to
console.log(a <= b);   // false
```

**Comparison Table:**

| Operator | Description | Type Coercion | Example |
|----------|-------------|---------------|---------|
| `==` | Equal to | Yes | `5 == '5'` → `true` |
| `===` | Strict equal to | No | `5 === '5'` → `false` |
| `!=` | Not equal to | Yes | `5 != '5'` → `false` |
| `!==` | Strict not equal to | No | `5 !== '5'` → `true` |
| `>` | Greater than | Yes | `10 > 5` → `true` |
| `<` | Less than | Yes | `10 < 5` → `false` |
| `>=` | Greater than or equal | Yes | `10 >= 10` → `true` |
| `<=` | Less than or equal | Yes | `5 <= 10` → `true` |

---

### 4. Logical Operators

Combine or invert boolean values.

```javascript
// AND (&&) - returns true if both operands are true
console.log(true && true);   // true
console.log(true && false);  // false
console.log(false && true);  // false

// OR (||) - returns true if at least one operand is true
console.log(true || false);  // true
console.log(false || false); // false

// NOT (!) - inverts boolean value
console.log(!true);          // false
console.log(!false);         // true
console.log(!!1);            // true (double negation converts to boolean)
```

**Short-Circuit Evaluation:**

```javascript
// && returns first falsy value or last value
console.log(0 && 'hello');        // 0
console.log('hello' && 'world');  // 'world'
console.log(null && 'hello');     // null

// || returns first truthy value or last value
console.log(0 || 'hello');        // 'hello'
console.log('hello' || 'world');  // 'hello'
console.log(null || undefined);   // undefined

// Practical use cases
function greet(name) {
  name = name || 'Guest';  // Default value (old way)
  console.log(`Hello, ${name}!`);
}

greet('Alice');  // Hello, Alice!
greet();         // Hello, Guest!

// Guard clause
const user = { name: 'Alice' };
user && console.log(user.name);  // Alice

// Chaining conditions
const isAdmin = true;
const hasPermission = true;
isAdmin && hasPermission && console.log('Access granted');
```

---

### 5. Bitwise Operators

Operate on binary representations of numbers.

```javascript
let a = 5;  // Binary: 0101
let b = 3;  // Binary: 0011

// AND (&)
console.log(a & b);  // 1 (0001)

// OR (|)
console.log(a | b);  // 7 (0111)

// XOR (^)
console.log(a ^ b);  // 6 (0110)

// NOT (~)
console.log(~a);     // -6 (inverts all bits)

// Left shift (<<)
console.log(a << 1); // 10 (1010) - multiply by 2

// Right shift (>>)
console.log(a >> 1); // 2 (0010) - divide by 2

// Zero-fill right shift (>>>)
console.log(-5 >>> 1); // 2147483645 (unsigned)
```

**Practical Use Cases:**

```javascript
// Check if number is even
function isEven(n) {
  return (n & 1) === 0;
}

console.log(isEven(4));  // true
console.log(isEven(5));  // false

// Toggle boolean (0 or 1)
let flag = 1;
flag ^= 1;  // Toggle: 0
flag ^= 1;  // Toggle: 1

// Quick multiplication/division by powers of 2
console.log(5 << 2);  // 20 (5 * 4)
console.log(20 >> 2); // 5 (20 / 4)
```

---

### 6. Unary Operators

Operators with a single operand.

```javascript
// typeof - returns the type of a variable
console.log(typeof 42);          // 'number'
console.log(typeof 'hello');     // 'string'
console.log(typeof true);        // 'boolean'
console.log(typeof undefined);   // 'undefined'
console.log(typeof null);        // 'object' (JavaScript quirk)
console.log(typeof {});          // 'object'
console.log(typeof []);          // 'object'
console.log(typeof function(){}); // 'function'

// delete - removes a property from an object
const obj = { name: 'Alice', age: 25 };
delete obj.age;
console.log(obj);  // { name: 'Alice' }

// void - evaluates an expression and returns undefined
console.log(void 0);           // undefined
console.log(void (2 + 3));     // undefined

// Unary plus (+) - converts to number
console.log(+'42');      // 42
console.log(+'3.14');    // 3.14
console.log(+true);      // 1
console.log(+false);     // 0
console.log(+'hello');   // NaN

// Unary negation (-) - converts to number and negates
console.log(-'42');      // -42
console.log(-true);      // -1

// Logical NOT (!)
console.log(!true);      // false
console.log(!'hello');   // false
console.log(!0);         // true
```

---

### 7. Ternary Operator

Conditional operator with three operands (condition ? trueValue : falseValue).

```javascript
// Basic syntax
const age = 18;
const status = age >= 18 ? 'Adult' : 'Minor';
console.log(status);  // 'Adult'

// With function calls
const isLoggedIn = true;
isLoggedIn ? showDashboard() : showLogin();

// Nested ternary (use sparingly)
const score = 75;
const grade = score >= 90 ? 'A' : 
              score >= 80 ? 'B' : 
              score >= 70 ? 'C' : 
              score >= 60 ? 'D' : 'F';
console.log(grade);  // 'C'

// Assigning default values
const userName = inputName ? inputName : 'Guest';

// Multiple conditions
const num = 5;
const result = num > 0 ? 'positive' : num < 0 ? 'negative' : 'zero';
console.log(result);  // 'positive'

// Inline JSX in React (common use case)
// return <div>{isLoading ? <Spinner /> : <Content />}</div>;
```

---

### 8. Nullish Coalescing Operator (??)

Returns the right-hand operand when the left-hand operand is `null` or `undefined`.

```javascript
// Basic usage
const value1 = null ?? 'default';
console.log(value1);  // 'default'

const value2 = undefined ?? 'default';
console.log(value2);  // 'default'

const value3 = 'hello' ?? 'default';
console.log(value3);  // 'hello'

// Key difference from || operator
console.log(0 || 'default');     // 'default' (0 is falsy)
console.log(0 ?? 'default');     // 0 (0 is not null/undefined)

console.log('' || 'default');    // 'default' ('' is falsy)
console.log('' ?? 'default');    // '' ('' is not null/undefined)

console.log(false || 'default'); // 'default' (false is falsy)
console.log(false ?? 'default'); // false (false is not null/undefined)

// Practical example
function getConfig(options) {
  const timeout = options.timeout ?? 5000;  // Default to 5000
  const retries = options.retries ?? 3;     // Default to 3
  return { timeout, retries };
}

console.log(getConfig({ timeout: 0 }));  
// { timeout: 0, retries: 3 } - 0 is preserved!

console.log(getConfig({}));  
// { timeout: 5000, retries: 3 }
```

**Comparison Table:**

| Expression | `\|\|` Result | `??` Result |
|------------|-------------|-------------|
| `null \|\| 'default'` | `'default'` | `'default'` |
| `undefined \|\| 'default'` | `'default'` | `'default'` |
| `0 \|\| 'default'` | `'default'` | `0` |
| `'' \|\| 'default'` | `'default'` | `''` |
| `false \|\| 'default'` | `'default'` | `false` |
| `NaN \|\| 'default'` | `'default'` | `NaN` |

---

### 9. Optional Chaining Operator (?.)

Safely access nested object properties without explicit null/undefined checks.

```javascript
// Without optional chaining
const user = {
  name: 'Alice',
  address: {
    street: '123 Main St',
    city: 'New York'
  }
};

// Verbose null checking
const zipCode = user && user.address && user.address.zipCode;
console.log(zipCode);  // undefined

// With optional chaining
const zipCode2 = user?.address?.zipCode;
console.log(zipCode2);  // undefined (no error)

// If user is null/undefined
const user2 = null;
console.log(user2?.address?.city);  // undefined (no error)

// Optional chaining with methods
const obj = {
  method: function() {
    return 'Hello';
  }
};

console.log(obj.method?.());      // 'Hello'
console.log(obj.nonExistent?.()); // undefined (no error)

// Optional chaining with arrays
const users = null;
console.log(users?.[0]?.name);  // undefined (no error)

const arr = ['Alice', 'Bob'];
console.log(arr?.[0]);          // 'Alice'
console.log(arr?.[5]);          // undefined

// Combining with nullish coalescing
const displayName = user?.profile?.displayName ?? user?.name ?? 'Anonymous';
console.log(displayName);  // 'Alice'

// Real-world example
function getUserCity(user) {
  return user?.address?.city ?? 'Unknown';
}

console.log(getUserCity({ address: { city: 'NYC' } }));  // 'NYC'
console.log(getUserCity(null));                          // 'Unknown'
```

---

### 10. Comma Operator

Evaluates multiple expressions and returns the last one.

```javascript
// Basic usage
let a = (1, 2, 3);
console.log(a);  // 3

// In for loops
for (let i = 0, j = 10; i < 5; i++, j--) {
  console.log(`i: ${i}, j: ${j}`);
}
// i: 0, j: 10
// i: 1, j: 9
// i: 2, j: 8
// i: 3, j: 7
// i: 4, j: 6

// Multiple assignments
let x, y, z;
x = (y = 5, z = 10, y + z);
console.log(x);  // 15
console.log(y);  // 5
console.log(z);  // 10
```

---

## Truthy and Falsy Values

JavaScript coerces values to boolean in conditional contexts.

### Falsy Values (8 total)

Only these values are falsy:

```javascript
console.log(Boolean(false));      // false
console.log(Boolean(0));          // false
console.log(Boolean(-0));         // false
console.log(Boolean(0n));         // false (BigInt zero)
console.log(Boolean(''));         // false (empty string)
console.log(Boolean(null));       // false
console.log(Boolean(undefined));  // false
console.log(Boolean(NaN));        // false
```

### Truthy Values

Everything else is truthy, including:

```javascript
console.log(Boolean(true));           // true
console.log(Boolean(1));              // true
console.log(Boolean(-1));             // true
console.log(Boolean('0'));            // true (non-empty string)
console.log(Boolean('false'));        // true (non-empty string)
console.log(Boolean([]));             // true (empty array)
console.log(Boolean({}));             // true (empty object)
console.log(Boolean(function() {}));  // true
console.log(Boolean(Infinity));       // true
console.log(Boolean(-Infinity));      // true
console.log(Boolean(new Date()));     // true
```

### Practical Examples

```javascript
// Conditional checks
if ('hello') {
  console.log('Truthy!');  // This runs
}

if (0) {
  console.log('Will not run');
}

// Default values with ||
function greet(name) {
  name = name || 'Guest';  // If name is falsy, use 'Guest'
  return `Hello, ${name}`;
}

console.log(greet('Alice'));  // Hello, Alice
console.log(greet(''));       // Hello, Guest
console.log(greet(0));        // Hello, Guest (careful with 0!)

// Better with nullish coalescing
function greet2(name) {
  name = name ?? 'Guest';  // Only null/undefined trigger default
  return `Hello, ${name}`;
}

console.log(greet2(''));  // Hello,  (empty string preserved)
console.log(greet2(0));   // Hello, 0 (0 preserved)
```

---

## Control Flow Structures

### 1. if...else Statement

```javascript
// Basic if
const age = 18;
if (age >= 18) {
  console.log('Adult');
}

// if...else
const score = 75;
if (score >= 60) {
  console.log('Pass');
} else {
  console.log('Fail');
}

// if...else if...else
const grade = 85;
if (grade >= 90) {
  console.log('A');
} else if (grade >= 80) {
  console.log('B');
} else if (grade >= 70) {
  console.log('C');
} else if (grade >= 60) {
  console.log('D');
} else {
  console.log('F');
}

// One-liner (no braces for single statement)
if (age >= 18) console.log('Adult');

// Nested if
const isLoggedIn = true;
const isAdmin = true;

if (isLoggedIn) {
  if (isAdmin) {
    console.log('Admin access');
  } else {
    console.log('User access');
  }
}
```

---

### 2. switch Statement

Best for multiple specific value checks.

```javascript
// Basic switch
const day = 3;

switch (day) {
  case 1:
    console.log('Monday');
    break;
  case 2:
    console.log('Tuesday');
    break;
  case 3:
    console.log('Wednesday');
    break;
  case 4:
    console.log('Thursday');
    break;
  case 5:
    console.log('Friday');
    break;
  case 6:
  case 7:
    console.log('Weekend');
    break;
  default:
    console.log('Invalid day');
}

// Fall-through behavior (without break)
const month = 2;
let days;

switch (month) {
  case 1: case 3: case 5: case 7: case 8: case 10: case 12:
    days = 31;
    break;
  case 4: case 6: case 9: case 11:
    days = 30;
    break;
  case 2:
    days = 28;
    break;
  default:
    days = 0;
}

console.log(days);  // 28

// Switch with expressions
const operation = 'add';
const a = 10, b = 5;
let result;

switch (operation) {
  case 'add':
    result = a + b;
    break;
  case 'subtract':
    result = a - b;
    break;
  case 'multiply':
    result = a * b;
    break;
  case 'divide':
    result = a / b;
    break;
  default:
    result = 'Invalid operation';
}

console.log(result);  // 15
```

---

### 3. for Loop

Traditional loop with counter.

```javascript
// Basic for loop
for (let i = 0; i < 5; i++) {
  console.log(i);  // 0, 1, 2, 3, 4
}

// Reverse loop
for (let i = 5; i > 0; i--) {
  console.log(i);  // 5, 4, 3, 2, 1
}

// Step by 2
for (let i = 0; i < 10; i += 2) {
  console.log(i);  // 0, 2, 4, 6, 8
}

// Multiple variables
for (let i = 0, j = 10; i < 5; i++, j--) {
  console.log(`i: ${i}, j: ${j}`);
}

// Nested loops
for (let i = 1; i <= 3; i++) {
  for (let j = 1; j <= 3; j++) {
    console.log(`${i} x ${j} = ${i * j}`);
  }
}

// Infinite loop (use with caution)
// for (;;) {
//   console.log('Forever');
//   break;  // Must have exit condition
// }
```

---

### 4. while Loop

Executes while condition is true.

```javascript
// Basic while loop
let count = 0;
while (count < 5) {
  console.log(count);  // 0, 1, 2, 3, 4
  count++;
}

// User input simulation
let password = '';
let attempts = 0;
const correctPassword = 'secret';

while (password !== correctPassword && attempts < 3) {
  // password = prompt('Enter password:');
  password = 'secret';  // Simulation
  attempts++;
}

// Loop until condition
let num = 1;
while (num < 100) {
  num *= 2;
}
console.log(num);  // 128

// Infinite loop with break
let i = 0;
while (true) {
  if (i >= 5) break;
  console.log(i);
  i++;
}
```

---

### 5. do...while Loop

Executes at least once, then checks condition.

```javascript
// Basic do...while
let count = 0;
do {
  console.log(count);  // 0, 1, 2, 3, 4
  count++;
} while (count < 5);

// Executes at least once even if condition is false
let num = 10;
do {
  console.log(num);  // 10 (runs once)
  num++;
} while (num < 5);

// Menu system example
let choice;
do {
  // choice = prompt('Enter 1-3 or 0 to exit:');
  choice = 0;  // Simulation
  console.log(`You selected: ${choice}`);
} while (choice !== 0);
```

---

### 6. for...in Loop

Iterates over enumerable properties of an object.

```javascript
// Object iteration
const person = {
  name: 'Alice',
  age: 25,
  city: 'New York'
};

for (let key in person) {
  console.log(`${key}: ${person[key]}`);
}
// name: Alice
// age: 25
// city: New York

// Array iteration (not recommended - use for...of)
const arr = ['a', 'b', 'c'];
for (let index in arr) {
  console.log(index, arr[index]);  // 0 a, 1 b, 2 c
}

// Inherited properties
const parent = { inherited: true };
const child = Object.create(parent);
child.own = 'value';

for (let key in child) {
  console.log(key);  // 'own', 'inherited'
}

// Filter own properties
for (let key in child) {
  if (child.hasOwnProperty(key)) {
    console.log(key);  // 'own' only
  }
}
```

---

### 7. for...of Loop (ES6)

Iterates over iterable objects (arrays, strings, Maps, Sets, etc.).

```javascript
// Array iteration
const fruits = ['apple', 'banana', 'cherry'];
for (let fruit of fruits) {
  console.log(fruit);  // apple, banana, cherry
}

// String iteration
const text = 'Hello';
for (let char of text) {
  console.log(char);  // H, e, l, l, o
}

// Array with index
const colors = ['red', 'green', 'blue'];
for (let [index, color] of colors.entries()) {
  console.log(`${index}: ${color}`);
}
// 0: red
// 1: green
// 2: blue

// Set iteration
const numbers = new Set([1, 2, 3, 4, 5]);
for (let num of numbers) {
  console.log(num);  // 1, 2, 3, 4, 5
}

// Map iteration
const map = new Map([
  ['name', 'Alice'],
  ['age', 25]
]);

for (let [key, value] of map) {
  console.log(`${key}: ${value}`);
}
// name: Alice
// age: 25

// NodeList iteration (DOM)
// const elements = document.querySelectorAll('div');
// for (let element of elements) {
//   console.log(element);
// }
```

**for...in vs for...of:**

| Feature | for...in | for...of |
|---------|----------|----------|
| Iterates over | Enumerable properties (keys) | Iterable values |
| Use with | Objects | Arrays, Strings, Maps, Sets |
| Array iteration | Index (string) | Value |
| Inherited properties | Yes | No |
| Best for | Objects | Arrays and iterables |

---

### 8. break Statement

Exits a loop or switch statement immediately.

```javascript
// Break in for loop
for (let i = 0; i < 10; i++) {
  if (i === 5) break;
  console.log(i);  // 0, 1, 2, 3, 4
}

// Break in while loop
let count = 0;
while (true) {
  if (count >= 5) break;
  console.log(count);
  count++;
}

// Break with label (nested loops)
outerLoop: for (let i = 0; i < 3; i++) {
  for (let j = 0; j < 3; j++) {
    if (i === 1 && j === 1) {
      break outerLoop;  // Breaks outer loop
    }
    console.log(`i: ${i}, j: ${j}`);
  }
}
// Output:
// i: 0, j: 0
// i: 0, j: 1
// i: 0, j: 2
// i: 1, j: 0

// Search and break
const numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
const target = 7;
let found = false;

for (let num of numbers) {
  if (num === target) {
    found = true;
    break;
  }
}

console.log(found ? 'Found!' : 'Not found');  // Found!
```

---

### 9. continue Statement

Skips the current iteration and continues to the next.

```javascript
// Skip even numbers
for (let i = 0; i < 10; i++) {
  if (i % 2 === 0) continue;
  console.log(i);  // 1, 3, 5, 7, 9
}

// Skip empty strings
const words = ['hello', '', 'world', '', 'foo'];
for (let word of words) {
  if (!word) continue;
  console.log(word);  // hello, world, foo
}

// Continue with label
outer: for (let i = 0; i < 3; i++) {
  for (let j = 0; j < 3; j++) {
    if (j === 1) continue outer;  // Skip to next outer iteration
    console.log(`i: ${i}, j: ${j}`);
  }
}
// Output:
// i: 0, j: 0
// i: 1, j: 0
// i: 2, j: 0

// Filter valid values
const values = [1, null, 2, undefined, 3, NaN, 4];
for (let value of values) {
  if (value == null || Number.isNaN(value)) continue;
  console.log(value);  // 1, 2, 3, 4
}
```

---

## Interview Questions

### Question 1: What is the difference between `==` and `===`?

**Answer:**
- `==` (loose equality) performs type coercion before comparison
- `===` (strict equality) compares both value and type without coercion
- Always prefer `===` unless you specifically need type coercion

```javascript
console.log(5 == '5');   // true (type coercion)
console.log(5 === '5');  // false (different types)
console.log(null == undefined);  // true
console.log(null === undefined); // false
```

---

### Question 2: What are falsy values in JavaScript?

**Answer:**
There are exactly 8 falsy values:
1. `false`
2. `0`
3. `-0`
4. `0n` (BigInt zero)
5. `''` (empty string)
6. `null`
7. `undefined`
8. `NaN`

Everything else is truthy, including `'0'`, `'false'`, `[]`, `{}`, and `function() {}`.

---

### Question 3: What is the difference between `||` and `??`?

**Answer:**
- `||` returns the first truthy value or the last value
- `??` returns the first non-null/undefined value

```javascript
console.log(0 || 'default');     // 'default' (0 is falsy)
console.log(0 ?? 'default');     // 0 (0 is not null/undefined)

console.log('' || 'default');    // 'default'
console.log('' ?? 'default');    // ''

// Use ?? when 0, '', or false are valid values
```

---

### Question 4: Explain optional chaining and when to use it.

**Answer:**
Optional chaining (`?.`) safely accesses nested properties without explicit null checks. It returns `undefined` if any part of the chain is `null` or `undefined`.

```javascript
const user = {
  name: 'Alice',
  address: null
};

// Without optional chaining
const city = user && user.address && user.address.city;

// With optional chaining
const city2 = user?.address?.city;  // undefined (no error)

// Combining with nullish coalescing
const displayCity = user?.address?.city ?? 'Unknown';
```

---

### Question 5: What is the output of this code?

```javascript
for (var i = 0; i < 3; i++) {
  setTimeout(() => console.log(i), 100);
}

for (let j = 0; j < 3; j++) {
  setTimeout(() => console.log(j), 100);
}
```

**Answer:**
```
3
3
3
0
1
2
```

**Explanation:**
- `var` is function-scoped, so all timeouts share the same `i`, which is 3 after the loop
- `let` is block-scoped, creating a new `j` for each iteration
- Solution: Use `let` instead of `var` in loops

---

### Question 6: How do you break out of nested loops?

**Answer:**
Use labeled statements:

```javascript
outer: for (let i = 0; i < 3; i++) {
  for (let j = 0; j < 3; j++) {
    if (i === 1 && j === 1) {
      break outer;  // Breaks the outer loop
    }
    console.log(`i: ${i}, j: ${j}`);
  }
}
```

---

### Question 7: What is the difference between `for...in` and `for...of`?

**Answer:**

| Feature | for...in | for...of |
|---------|----------|----------|
| Iterates over | Keys/properties | Values |
| Use with | Objects | Arrays, Strings, Maps, Sets |
| Array iteration | Returns index (string) | Returns value |
| Inherited properties | Yes | No |

```javascript
const arr = ['a', 'b', 'c'];

for (let key in arr) {
  console.log(key);  // '0', '1', '2' (indices)
}

for (let value of arr) {
  console.log(value);  // 'a', 'b', 'c' (values)
}
```

---

### Question 8: Explain the ternary operator and when to use it.

**Answer:**
The ternary operator is a concise way to write simple if-else statements:

```javascript
// Syntax: condition ? trueValue : falseValue

const age = 18;
const status = age >= 18 ? 'Adult' : 'Minor';

// Use when:
// 1. Assigning conditional values
// 2. Inline JSX in React
// 3. Simple conditions

// Avoid when:
// 1. Complex conditions (use if-else)
// 2. Multiple nested ternaries (hard to read)
```

---

### Question 9: What is short-circuit evaluation?

**Answer:**
Logical operators (`&&`, `||`) stop evaluating as soon as the result is determined:

```javascript
// && returns first falsy value or last value
console.log(false && expensiveOperation());  // false (doesn't call function)
console.log(true && 'hello');                 // 'hello'

// || returns first truthy value or last value
console.log(true || expensiveOperation());   // true (doesn't call function)
console.log(false || 'default');             // 'default'

// Practical uses:
// Conditional execution
isLoggedIn && redirectToDashboard();

// Default values
const name = userName || 'Guest';
```

---

### Question 10: What is the comma operator and when would you use it?

**Answer:**
The comma operator evaluates multiple expressions and returns the last one:

```javascript
let a = (1, 2, 3);  // a = 3

// Common use in for loops
for (let i = 0, j = 10; i < 5; i++, j--) {
  console.log(i, j);
}

// Generally avoided elsewhere for readability
```

---

## Best Practices

### 1. Always Use `===` and `!==`

```javascript
// ❌ Avoid
if (value == null) { }

// ✅ Good
if (value === null) { }

// Exception: checking for null or undefined
if (value == null) { }  // Equivalent to value === null || value === undefined
```

---

### 2. Prefer `const` and `let` over `var`

```javascript
// ❌ Avoid
var count = 0;

// ✅ Good
const MAX_COUNT = 100;
let count = 0;
```

---

### 3. Use Optional Chaining and Nullish Coalescing

```javascript
// ❌ Avoid
const city = user && user.address && user.address.city || 'Unknown';

// ✅ Good
const city = user?.address?.city ?? 'Unknown';
```

---

### 4. Keep Ternary Operators Simple

```javascript
// ❌ Avoid (nested ternary)
const result = condition1 ? value1 : condition2 ? value2 : condition3 ? value3 : value4;

// ✅ Good (use if-else for complex logic)
let result;
if (condition1) {
  result = value1;
} else if (condition2) {
  result = value2;
} else if (condition3) {
  result = value3;
} else {
  result = value4;
}
```

---

### 5. Use `for...of` for Arrays, `for...in` for Objects

```javascript
// ❌ Avoid
const arr = [1, 2, 3];
for (let i in arr) {
  console.log(arr[i]);
}

// ✅ Good
for (let value of arr) {
  console.log(value);
}

// For objects
const obj = { a: 1, b: 2 };
for (let key in obj) {
  if (obj.hasOwnProperty(key)) {
    console.log(key, obj[key]);
  }
}
```

---

### 6. Be Explicit with Truthy/Falsy Checks

```javascript
// ❌ Ambiguous
if (value) { }

// ✅ Explicit
if (value !== null && value !== undefined) { }
if (value.length > 0) { }
if (value !== 0) { }
```

---

### 7. Use Early Returns

```javascript
// ❌ Avoid
function process(data) {
  if (data) {
    if (data.isValid) {
      // lots of code
      return result;
    }
  }
}

// ✅ Good
function process(data) {
  if (!data) return null;
  if (!data.isValid) return null;
  
  // lots of code
  return result;
}
```

---

### 8. Avoid Deep Nesting

```javascript
// ❌ Avoid
if (condition1) {
  if (condition2) {
    if (condition3) {
      // code
    }
  }
}

// ✅ Good
if (!condition1) return;
if (!condition2) return;
if (!condition3) return;
// code
```

---

## Summary

### Key Takeaways

1. **Operators:**
   - Use `===` and `!==` for comparisons (strict equality)
   - `??` is better than `||` for default values when 0, '', or false are valid
   - `?.` (optional chaining) simplifies nested property access
   - Logical operators use short-circuit evaluation

2. **Truthy/Falsy:**
   - Only 8 falsy values: `false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN`
   - Everything else is truthy, including `[]`, `{}`, `'0'`, `'false'`

3. **Control Flow:**
   - Use `for...of` for arrays/iterables, `for...in` for objects
   - `break` exits loops, `continue` skips iterations
   - Labels allow breaking/continuing outer loops
   - Ternary operator is concise but avoid nesting

4. **Best Practices:**
   - Prefer `const` and `let` over `var`
   - Use optional chaining and nullish coalescing
   - Be explicit with boolean checks
   - Use early returns to reduce nesting
   - Keep code readable and maintainable

5. **ES6+ Features:**
   - `??` (nullish coalescing) - ES2020
   - `?.` (optional chaining) - ES2020
   - `**` (exponentiation) - ES2016
   - `for...of` loop - ES2015
   - Logical assignment operators (`&&=`, `||=`, `??=`) - ES2021

---

### Quick Reference

```javascript
// Operators
5 === '5'         // false (strict equality)
5 == '5'          // true (loose equality)
null ?? 'default' // 'default' (nullish coalescing)
obj?.prop?.nested // undefined (optional chaining)
condition ? 'yes' : 'no'  // ternary

// Truthy/Falsy
if (value) { }    // Check truthy
if (!value) { }   // Check falsy

// Loops
for (let i = 0; i < n; i++) { }        // Traditional
for (let key in obj) { }               // Object keys
for (let value of arr) { }             // Array values
while (condition) { }                  // While
do { } while (condition);              // Do-while

// Control
break;            // Exit loop
continue;         // Skip iteration
return;           // Exit function
```

---

**Remember:** Write clear, maintainable code. Optimize for readability first, then performance if needed!
