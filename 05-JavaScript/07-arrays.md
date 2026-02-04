# JavaScript Arrays - Comprehensive Guide

## What are Arrays?

**Arrays** are ordered collections of values that can store multiple items in a single variable. Arrays in JavaScript are:
- **Dynamic** - can grow or shrink in size
- **Zero-indexed** - first element is at index 0
- **Heterogeneous** - can contain mixed data types
- **Objects** - technically a special type of object

---

## 1. Array Creation and Initialization

### Array Literal (Most Common)

```javascript
// Empty array
const emptyArray = [];

// Array with values
const fruits = ['apple', 'banana', 'orange'];

// Mixed data types
const mixed = [1, 'hello', true, null, { name: 'John' }, [1, 2, 3]];

console.log(fruits); // ['apple', 'banana', 'orange']
console.log(mixed.length); // 6
```

### Array Constructor

```javascript
// Empty array
const arr1 = new Array();

// Array with length (creates empty slots)
const arr2 = new Array(5);
console.log(arr2); // [ <5 empty items> ]
console.log(arr2.length); // 5

// Array with elements (only if more than one argument)
const arr3 = new Array(1, 2, 3);
console.log(arr3); // [1, 2, 3]

// ⚠️ Pitfall: Single number creates array with that length
const arr4 = new Array(5); // Creates array with 5 empty slots
const arr5 = new Array('5'); // Creates array with one element: ['5']
```

### Array.of() (ES6)

Creates an array from arguments, solving the Array constructor ambiguity.

```javascript
// Creates array with single element
const arr1 = Array.of(5);
console.log(arr1); // [5]

// Multiple elements
const arr2 = Array.of(1, 2, 3);
console.log(arr2); // [1, 2, 3]

// Mixed types
const arr3 = Array.of('a', 2, true, { x: 1 });
console.log(arr3); // ['a', 2, true, { x: 1 }]
```

### Array.from() (ES6)

Creates an array from an iterable or array-like object.

```javascript
// From string
const str = Array.from('hello');
console.log(str); // ['h', 'e', 'l', 'l', 'o']

// From Set
const set = new Set([1, 2, 3, 3, 4]);
const arr = Array.from(set);
console.log(arr); // [1, 2, 3, 4]

// From NodeList (array-like)
const divs = document.querySelectorAll('div');
const divArray = Array.from(divs);

// With mapping function
const numbers = Array.from([1, 2, 3], x => x * 2);
console.log(numbers); // [2, 4, 6]

// Generate sequence
const range = Array.from({ length: 5 }, (_, i) => i + 1);
console.log(range); // [1, 2, 3, 4, 5]
```

---

## 2. Accessing Array Elements

### Index-Based Access

```javascript
const colors = ['red', 'green', 'blue', 'yellow'];

// Positive indices
console.log(colors[0]); // 'red' (first element)
console.log(colors[2]); // 'blue'
console.log(colors[colors.length - 1]); // 'yellow' (last element)

// Out of bounds
console.log(colors[10]); // undefined

// Setting values
colors[1] = 'purple';
console.log(colors); // ['red', 'purple', 'blue', 'yellow']

// Creating gaps (sparse arrays)
colors[10] = 'pink';
console.log(colors.length); // 11
console.log(colors); // ['red', 'purple', 'blue', 'yellow', <6 empty items>, 'pink']
```

### at() Method (ES2022)

Allows negative indices to access elements from the end.

```javascript
const arr = ['a', 'b', 'c', 'd', 'e'];

// Positive indices
console.log(arr.at(0)); // 'a'
console.log(arr.at(2)); // 'c'

// Negative indices (count from end)
console.log(arr.at(-1)); // 'e' (last element)
console.log(arr.at(-2)); // 'd' (second to last)
console.log(arr.at(-5)); // 'a' (first element)

// Out of bounds
console.log(arr.at(10)); // undefined
console.log(arr.at(-10)); // undefined

// Comparison with bracket notation
console.log(arr[arr.length - 1]); // 'e' (more verbose)
console.log(arr.at(-1)); // 'e' (cleaner)
```

---

## 3. Array Length Property

```javascript
const arr = [1, 2, 3, 4, 5];

// Getting length
console.log(arr.length); // 5

// Modifying length
arr.length = 3;
console.log(arr); // [1, 2, 3] (truncated)

arr.length = 5;
console.log(arr); // [1, 2, 3, <2 empty items>]

// Clearing array
arr.length = 0;
console.log(arr); // []

// Length with sparse arrays
const sparse = [1, , , 4];
console.log(sparse.length); // 4 (includes empty slots)
```

---

## 4. Mutating Methods (Modify Original Array)

### push() - Add to End

```javascript
const fruits = ['apple', 'banana'];

const newLength = fruits.push('orange');
console.log(fruits); // ['apple', 'banana', 'orange']
console.log(newLength); // 3

// Multiple elements
fruits.push('grape', 'mango');
console.log(fruits); // ['apple', 'banana', 'orange', 'grape', 'mango']

// Returns new length
console.log(fruits.push('kiwi')); // 6
```

### pop() - Remove from End

```javascript
const fruits = ['apple', 'banana', 'orange'];

const removed = fruits.pop();
console.log(removed); // 'orange'
console.log(fruits); // ['apple', 'banana']

// On empty array
const empty = [];
console.log(empty.pop()); // undefined
```

### unshift() - Add to Beginning

```javascript
const fruits = ['banana', 'orange'];

const newLength = fruits.unshift('apple');
console.log(fruits); // ['apple', 'banana', 'orange']
console.log(newLength); // 3

// Multiple elements
fruits.unshift('kiwi', 'grape');
console.log(fruits); // ['kiwi', 'grape', 'apple', 'banana', 'orange']
```

### shift() - Remove from Beginning

```javascript
const fruits = ['apple', 'banana', 'orange'];

const removed = fruits.shift();
console.log(removed); // 'apple'
console.log(fruits); // ['banana', 'orange']

// On empty array
const empty = [];
console.log(empty.shift()); // undefined
```

### splice() - Add/Remove Elements Anywhere

```javascript
// Syntax: array.splice(start, deleteCount, item1, item2, ...)

const arr = ['a', 'b', 'c', 'd', 'e'];

// Remove elements
const removed = arr.splice(2, 2); // Start at index 2, remove 2 elements
console.log(removed); // ['c', 'd']
console.log(arr); // ['a', 'b', 'e']

// Add elements without removing
const arr2 = ['a', 'b', 'e'];
arr2.splice(2, 0, 'c', 'd'); // Start at 2, remove 0, add 'c' and 'd'
console.log(arr2); // ['a', 'b', 'c', 'd', 'e']

// Replace elements
const arr3 = ['a', 'b', 'c', 'd'];
arr3.splice(1, 2, 'x', 'y', 'z'); // Remove 2, add 3
console.log(arr3); // ['a', 'x', 'y', 'z', 'd']

// Negative index (from end)
const arr4 = ['a', 'b', 'c', 'd'];
arr4.splice(-2, 1); // Start at second-to-last, remove 1
console.log(arr4); // ['a', 'b', 'd']

// Remove all from index
const arr5 = ['a', 'b', 'c', 'd'];
arr5.splice(2); // Remove from index 2 to end
console.log(arr5); // ['a', 'b']
```

### reverse() - Reverse Array

```javascript
const arr = [1, 2, 3, 4, 5];

arr.reverse();
console.log(arr); // [5, 4, 3, 2, 1]

// Returns reference to same array
const result = arr.reverse();
console.log(result === arr); // true
```

### sort() - Sort Array

```javascript
// Default: sorts as strings
const arr1 = [3, 1, 4, 1, 5, 9];
arr1.sort();
console.log(arr1); // [1, 1, 3, 4, 5, 9]

// ⚠️ Pitfall: sorts numbers as strings
const arr2 = [10, 5, 40, 25, 1000];
arr2.sort();
console.log(arr2); // [10, 1000, 25, 40, 5] (wrong!)

// Correct numeric sort
arr2.sort((a, b) => a - b); // Ascending
console.log(arr2); // [5, 10, 25, 40, 1000]

arr2.sort((a, b) => b - a); // Descending
console.log(arr2); // [1000, 40, 25, 10, 5]

// String sort (case-insensitive)
const names = ['Charlie', 'alice', 'Bob'];
names.sort((a, b) => a.toLowerCase().localeCompare(b.toLowerCase()));
console.log(names); // ['alice', 'Bob', 'Charlie']

// Sort objects
const users = [
  { name: 'John', age: 30 },
  { name: 'Alice', age: 25 },
  { name: 'Bob', age: 35 }
];
users.sort((a, b) => a.age - b.age);
console.log(users); // Sorted by age: 25, 30, 35
```

### fill() - Fill with Static Value

```javascript
// Syntax: array.fill(value, start, end)

const arr1 = [1, 2, 3, 4, 5];
arr1.fill(0);
console.log(arr1); // [0, 0, 0, 0, 0]

// With start index
const arr2 = [1, 2, 3, 4, 5];
arr2.fill(0, 2);
console.log(arr2); // [1, 2, 0, 0, 0]

// With start and end
const arr3 = [1, 2, 3, 4, 5];
arr3.fill(0, 1, 4);
console.log(arr3); // [1, 0, 0, 0, 5]

// Initialize array
const arr4 = new Array(5).fill(1);
console.log(arr4); // [1, 1, 1, 1, 1]

// ⚠️ Pitfall: fills with reference (objects)
const arr5 = new Array(3).fill([]);
arr5[0].push(1);
console.log(arr5); // [[1], [1], [1]] (all references to same array!)
```

### copyWithin() - Copy Part of Array Within Itself

```javascript
// Syntax: array.copyWithin(target, start, end)

const arr1 = [1, 2, 3, 4, 5];
arr1.copyWithin(0, 3); // Copy from index 3 to 0
console.log(arr1); // [4, 5, 3, 4, 5]

const arr2 = [1, 2, 3, 4, 5];
arr2.copyWithin(0, 3, 4); // Copy from 3-4 to 0
console.log(arr2); // [4, 2, 3, 4, 5]

const arr3 = [1, 2, 3, 4, 5];
arr3.copyWithin(2, 0, 2); // Copy from 0-2 to 2
console.log(arr3); // [1, 2, 1, 2, 5]
```

---

## 5. Non-Mutating Methods (Return New Array)

### slice() - Extract Portion

```javascript
// Syntax: array.slice(start, end)

const arr = ['a', 'b', 'c', 'd', 'e'];

// From index to end
console.log(arr.slice(2)); // ['c', 'd', 'e']

// From start to end (end not included)
console.log(arr.slice(1, 4)); // ['b', 'c', 'd']

// Negative indices
console.log(arr.slice(-3)); // ['c', 'd', 'e']
console.log(arr.slice(-3, -1)); // ['c', 'd']

// Shallow copy
const copy = arr.slice();
console.log(copy); // ['a', 'b', 'c', 'd', 'e']
console.log(copy === arr); // false

// Original unchanged
console.log(arr); // ['a', 'b', 'c', 'd', 'e']
```

### concat() - Merge Arrays

```javascript
const arr1 = [1, 2, 3];
const arr2 = [4, 5, 6];

// Merge arrays
const merged = arr1.concat(arr2);
console.log(merged); // [1, 2, 3, 4, 5, 6]

// Multiple arrays
const arr3 = [7, 8];
const result = arr1.concat(arr2, arr3);
console.log(result); // [1, 2, 3, 4, 5, 6, 7, 8]

// Mix arrays and values
const mixed = arr1.concat(4, arr2, 7);
console.log(mixed); // [1, 2, 3, 4, 4, 5, 6, 7]

// Original unchanged
console.log(arr1); // [1, 2, 3]
```

### join() - Convert to String

```javascript
const arr = ['apple', 'banana', 'orange'];

// Default separator (comma)
console.log(arr.join()); // 'apple,banana,orange'

// Custom separator
console.log(arr.join(' - ')); // 'apple - banana - orange'
console.log(arr.join('')); // 'applebananaorange'
console.log(arr.join(' ')); // 'apple banana orange'

// Numbers
const nums = [1, 2, 3];
console.log(nums.join('-')); // '1-2-3'
```

### flat() - Flatten Nested Arrays (ES2019)

```javascript
// Syntax: array.flat(depth)

// One level deep
const arr1 = [1, 2, [3, 4]];
console.log(arr1.flat()); // [1, 2, 3, 4]

// Two levels deep
const arr2 = [1, [2, [3, 4]]];
console.log(arr2.flat()); // [1, 2, [3, 4]] (default depth = 1)
console.log(arr2.flat(2)); // [1, 2, 3, 4]

// Infinite depth
const arr3 = [1, [2, [3, [4, [5]]]]];
console.log(arr3.flat(Infinity)); // [1, 2, 3, 4, 5]

// Removes empty slots
const arr4 = [1, 2, , 4, 5];
console.log(arr4.flat()); // [1, 2, 4, 5]
```

### flatMap() - Map then Flatten (ES2019)

```javascript
// Equivalent to map().flat() but more efficient

const arr = [1, 2, 3];

// Regular map (creates nested arrays)
const mapped = arr.map(x => [x, x * 2]);
console.log(mapped); // [[1, 2], [2, 4], [3, 6]]

// flatMap (maps and flattens)
const flatMapped = arr.flatMap(x => [x, x * 2]);
console.log(flatMapped); // [1, 2, 2, 4, 3, 6]

// Practical example: split sentences into words
const sentences = ['Hello world', 'How are you'];
const words = sentences.flatMap(s => s.split(' '));
console.log(words); // ['Hello', 'world', 'How', 'are', 'you']

// Filter and map in one operation
const numbers = [1, 2, 3, 4, 5];
const doubled = numbers.flatMap(x => x % 2 === 0 ? [x * 2] : []);
console.log(doubled); // [4, 8] (only even numbers doubled)
```

---

## 6. Iteration Methods

### forEach() - Execute Function for Each Element

```javascript
const arr = ['a', 'b', 'c'];

// Basic usage
arr.forEach(item => {
  console.log(item);
});
// Output: 'a', 'b', 'c'

// With index and array
arr.forEach((item, index, array) => {
  console.log(`${index}: ${item}`);
});
// Output:
// 0: a
// 1: b
// 2: c

// ⚠️ forEach cannot be broken with break or continue
// ⚠️ forEach returns undefined (cannot be chained)

// Modifying array elements (reference types)
const objects = [{ x: 1 }, { x: 2 }];
objects.forEach(obj => {
  obj.x *= 2;
});
console.log(objects); // [{ x: 2 }, { x: 4 }]
```

### map() - Transform Each Element

```javascript
const numbers = [1, 2, 3, 4];

// Double each number
const doubled = numbers.map(x => x * 2);
console.log(doubled); // [2, 4, 6, 8]

// With index
const indexed = numbers.map((x, i) => `${i}: ${x}`);
console.log(indexed); // ['0: 1', '1: 2', '2: 3', '3: 4']

// Object transformation
const users = [
  { firstName: 'John', lastName: 'Doe' },
  { firstName: 'Jane', lastName: 'Smith' }
];
const fullNames = users.map(u => `${u.firstName} ${u.lastName}`);
console.log(fullNames); // ['John Doe', 'Jane Smith']

// Chaining
const result = numbers
  .map(x => x * 2)
  .map(x => x + 1);
console.log(result); // [3, 5, 7, 9]
```

### filter() - Filter Elements

```javascript
const numbers = [1, 2, 3, 4, 5, 6];

// Filter even numbers
const even = numbers.filter(x => x % 2 === 0);
console.log(even); // [2, 4, 6]

// Filter with multiple conditions
const filtered = numbers.filter(x => x > 2 && x < 6);
console.log(filtered); // [3, 4, 5]

// Filter objects
const users = [
  { name: 'John', age: 30 },
  { name: 'Jane', age: 25 },
  { name: 'Bob', age: 35 }
];
const adults = users.filter(u => u.age >= 30);
console.log(adults); // [{ name: 'John', age: 30 }, { name: 'Bob', age: 35 }]

// Remove falsy values
const mixed = [0, 1, false, 2, '', 3, null, undefined, 4];
const truthy = mixed.filter(Boolean);
console.log(truthy); // [1, 2, 3, 4]

// Chaining map and filter
const result = numbers
  .map(x => x * 2)
  .filter(x => x > 5);
console.log(result); // [6, 8, 10, 12]
```

### reduce() - Reduce to Single Value

```javascript
// Syntax: array.reduce(callback, initialValue)

const numbers = [1, 2, 3, 4, 5];

// Sum
const sum = numbers.reduce((acc, curr) => acc + curr, 0);
console.log(sum); // 15

// Product
const product = numbers.reduce((acc, curr) => acc * curr, 1);
console.log(product); // 120

// Max value
const max = numbers.reduce((acc, curr) => Math.max(acc, curr));
console.log(max); // 5

// Count occurrences
const fruits = ['apple', 'banana', 'apple', 'orange', 'banana', 'apple'];
const count = fruits.reduce((acc, fruit) => {
  acc[fruit] = (acc[fruit] || 0) + 1;
  return acc;
}, {});
console.log(count); // { apple: 3, banana: 2, orange: 1 }

// Flatten array
const nested = [[1, 2], [3, 4], [5, 6]];
const flat = nested.reduce((acc, arr) => acc.concat(arr), []);
console.log(flat); // [1, 2, 3, 4, 5, 6]

// Group by property
const users = [
  { name: 'John', role: 'admin' },
  { name: 'Jane', role: 'user' },
  { name: 'Bob', role: 'admin' }
];
const grouped = users.reduce((acc, user) => {
  const { role } = user;
  if (!acc[role]) acc[role] = [];
  acc[role].push(user);
  return acc;
}, {});
console.log(grouped);
// {
//   admin: [{ name: 'John', role: 'admin' }, { name: 'Bob', role: 'admin' }],
//   user: [{ name: 'Jane', role: 'user' }]
// }

// ⚠️ Without initial value, first element is used as accumulator
const arr = [1, 2, 3];
console.log(arr.reduce((a, b) => a + b)); // 6 (starts with 1)
console.log([].reduce((a, b) => a + b)); // TypeError (empty array needs initial value)
```

### reduceRight() - Reduce from Right to Left

```javascript
const arr = ['a', 'b', 'c', 'd'];

const result = arr.reduceRight((acc, curr) => acc + curr);
console.log(result); // 'dcba'

// Useful for operations where order matters
const numbers = [2, 3, 4];
const power = numbers.reduceRight((acc, num) => Math.pow(num, acc));
console.log(power); // 2^(3^4) = 2^81
```

### find() - Find First Matching Element

```javascript
const numbers = [1, 5, 10, 15, 20];

// Find first element > 10
const found = numbers.find(x => x > 10);
console.log(found); // 15

// Not found
const notFound = numbers.find(x => x > 100);
console.log(notFound); // undefined

// Find object
const users = [
  { id: 1, name: 'John' },
  { id: 2, name: 'Jane' },
  { id: 3, name: 'Bob' }
];
const user = users.find(u => u.id === 2);
console.log(user); // { id: 2, name: 'Jane' }
```

### findIndex() - Find Index of First Match

```javascript
const numbers = [1, 5, 10, 15, 20];

// Find index
const index = numbers.findIndex(x => x > 10);
console.log(index); // 3

// Not found
const notFound = numbers.findIndex(x => x > 100);
console.log(notFound); // -1

// Compare with indexOf
console.log(numbers.indexOf(10)); // 2 (exact match only)
console.log(numbers.findIndex(x => x === 10)); // 2 (with condition)
```

### findLast() and findLastIndex() (ES2023)

```javascript
const numbers = [1, 5, 10, 15, 20, 15, 10];

// Find last element > 10
const last = numbers.findLast(x => x > 10);
console.log(last); // 15 (last occurrence)

// Find last index
const lastIndex = numbers.findLastIndex(x => x > 10);
console.log(lastIndex); // 5

// Compare with find
console.log(numbers.find(x => x > 10)); // 15 (first)
console.log(numbers.findLast(x => x > 10)); // 15 (last)
console.log(numbers.findIndex(x => x > 10)); // 3 (first index)
console.log(numbers.findLastIndex(x => x > 10)); // 5 (last index)
```

### some() - Test if Any Element Passes

```javascript
const numbers = [1, 2, 3, 4, 5];

// Check if any element is even
console.log(numbers.some(x => x % 2 === 0)); // true

// Check if any element > 10
console.log(numbers.some(x => x > 10)); // false

// Empty array
console.log([].some(x => true)); // false

// Practical: check if user has permission
const users = [
  { name: 'John', role: 'user' },
  { name: 'Jane', role: 'admin' }
];
const hasAdmin = users.some(u => u.role === 'admin');
console.log(hasAdmin); // true
```

### every() - Test if All Elements Pass

```javascript
const numbers = [2, 4, 6, 8];

// Check if all are even
console.log(numbers.every(x => x % 2 === 0)); // true

// Check if all > 5
console.log(numbers.every(x => x > 5)); // false

// Empty array (vacuous truth)
console.log([].every(x => false)); // true

// Practical: validate all fields filled
const formData = [
  { field: 'name', value: 'John' },
  { field: 'email', value: 'john@example.com' },
  { field: 'age', value: '' }
];
const allFilled = formData.every(f => f.value.trim() !== '');
console.log(allFilled); // false
```

---

## 7. Array Destructuring (ES6)

```javascript
// Basic destructuring
const arr = [1, 2, 3, 4, 5];
const [first, second, third] = arr;
console.log(first); // 1
console.log(second); // 2
console.log(third); // 3

// Skip elements
const [a, , c] = arr;
console.log(a); // 1
console.log(c); // 3

// Rest pattern
const [x, y, ...rest] = arr;
console.log(x); // 1
console.log(y); // 2
console.log(rest); // [3, 4, 5]

// Default values
const [p = 0, q = 0, r = 0] = [1, 2];
console.log(p); // 1
console.log(q); // 2
console.log(r); // 0 (default)

// Nested arrays
const nested = [1, [2, 3], 4];
const [first2, [second2, third2]] = nested;
console.log(first2); // 1
console.log(second2); // 2
console.log(third2); // 3

// Swapping variables
let var1 = 1, var2 = 2;
[var1, var2] = [var2, var1];
console.log(var1); // 2
console.log(var2); // 1

// Function return values
function getCoordinates() {
  return [10, 20];
}
const [x2, y2] = getCoordinates();
console.log(x2); // 10
console.log(y2); // 20
```

---

## 8. Spread Operator with Arrays (ES6)

```javascript
const arr1 = [1, 2, 3];
const arr2 = [4, 5, 6];

// Combine arrays
const combined = [...arr1, ...arr2];
console.log(combined); // [1, 2, 3, 4, 5, 6]

// Insert elements
const inserted = [...arr1, 99, ...arr2];
console.log(inserted); // [1, 2, 3, 99, 4, 5, 6]

// Shallow copy
const copy = [...arr1];
console.log(copy); // [1, 2, 3]
console.log(copy === arr1); // false

// Convert string to array
const str = 'hello';
const chars = [...str];
console.log(chars); // ['h', 'e', 'l', 'l', 'o']

// Convert Set to array
const set = new Set([1, 2, 3, 3, 4]);
const arr = [...set];
console.log(arr); // [1, 2, 3, 4]

// Function arguments
function sum(a, b, c) {
  return a + b + c;
}
const numbers = [1, 2, 3];
console.log(sum(...numbers)); // 6

// Math operations
const nums = [5, 1, 9, 3, 7];
console.log(Math.max(...nums)); // 9
console.log(Math.min(...nums)); // 1

// Push multiple elements
const arr3 = [1, 2];
arr3.push(...[3, 4, 5]);
console.log(arr3); // [1, 2, 3, 4, 5]

// Clone nested arrays (shallow copy warning)
const nested = [[1], [2]];
const cloned = [...nested];
cloned[0].push(99);
console.log(nested); // [[1, 99], [2]] (nested arrays still reference same objects!)
```

---

## 9. Multi-Dimensional Arrays

```javascript
// 2D array (matrix)
const matrix = [
  [1, 2, 3],
  [4, 5, 6],
  [7, 8, 9]
];

// Accessing elements
console.log(matrix[0][0]); // 1
console.log(matrix[1][2]); // 6
console.log(matrix[2][1]); // 8

// Iterating 2D array
matrix.forEach((row, i) => {
  row.forEach((cell, j) => {
    console.log(`[${i}][${j}] = ${cell}`);
  });
});

// Creating 2D array dynamically
const rows = 3, cols = 4;
const grid = Array.from({ length: rows }, () => 
  Array.from({ length: cols }, () => 0)
);
console.log(grid);
// [
//   [0, 0, 0, 0],
//   [0, 0, 0, 0],
//   [0, 0, 0, 0]
// ]

// ⚠️ Wrong way (all rows reference same array)
const wrong = Array(3).fill(Array(4).fill(0));
wrong[0][0] = 1;
console.log(wrong); // All rows affected!
// [
//   [1, 0, 0, 0],
//   [1, 0, 0, 0],
//   [1, 0, 0, 0]
// ]

// Flatten 2D array
const arr2D = [[1, 2], [3, 4], [5, 6]];
const flattened = arr2D.flat();
console.log(flattened); // [1, 2, 3, 4, 5, 6]

// 3D array
const cube = [
  [[1, 2], [3, 4]],
  [[5, 6], [7, 8]]
];
console.log(cube[0][1][0]); // 3
console.log(cube.flat(2)); // [1, 2, 3, 4, 5, 6, 7, 8]
```

---

## 10. Array-Like Objects and Conversions

### Array-Like Objects

```javascript
// Array-like: has length and indexed elements, but no array methods
const arrayLike = {
  0: 'a',
  1: 'b',
  2: 'c',
  length: 3
};

console.log(arrayLike[0]); // 'a'
console.log(arrayLike.length); // 3
// arrayLike.push('d'); // TypeError: not a function

// Examples of array-like objects:
// - arguments object (in non-arrow functions)
// - NodeList (from querySelectorAll)
// - HTMLCollection
// - String
```

### Converting to Arrays

```javascript
const arrayLike = {
  0: 'a',
  1: 'b',
  2: 'c',
  length: 3
};

// 1. Array.from()
const arr1 = Array.from(arrayLike);
console.log(arr1); // ['a', 'b', 'c']
console.log(Array.isArray(arr1)); // true

// 2. Spread operator (if iterable)
const str = 'hello';
const arr2 = [...str];
console.log(arr2); // ['h', 'e', 'l', 'l', 'o']

// 3. Array.prototype.slice.call()
const arr3 = Array.prototype.slice.call(arrayLike);
console.log(arr3); // ['a', 'b', 'c']

// 4. [...] spread with Array.from for non-iterables
const arr4 = [...Array.from(arrayLike)];
console.log(arr4); // ['a', 'b', 'c']

// Real-world example: NodeList to Array
// const divs = document.querySelectorAll('div'); // NodeList
// const divArray = Array.from(divs); // Now can use map, filter, etc.
// divArray.forEach(div => console.log(div));

// arguments object
function example() {
  console.log(arguments); // [Arguments] { '0': 1, '1': 2, '2': 3 }
  console.log(Array.isArray(arguments)); // false
  
  const args = Array.from(arguments);
  console.log(Array.isArray(args)); // true
  
  // Or with spread
  const args2 = [...arguments];
  console.log(args2); // [1, 2, 3]
}
example(1, 2, 3);
```

---

## 11. Comparison of Array Methods

### Mutating vs Non-Mutating Methods

| Method | Mutates? | Returns | Purpose |
|--------|----------|---------|---------|
| `push()` | ✅ Yes | New length | Add to end |
| `pop()` | ✅ Yes | Removed element | Remove from end |
| `shift()` | ✅ Yes | Removed element | Remove from beginning |
| `unshift()` | ✅ Yes | New length | Add to beginning |
| `splice()` | ✅ Yes | Removed elements | Add/remove anywhere |
| `reverse()` | ✅ Yes | Reversed array | Reverse order |
| `sort()` | ✅ Yes | Sorted array | Sort elements |
| `fill()` | ✅ Yes | Modified array | Fill with value |
| `copyWithin()` | ✅ Yes | Modified array | Copy within array |
| `slice()` | ❌ No | New array | Extract portion |
| `concat()` | ❌ No | New array | Merge arrays |
| `flat()` | ❌ No | New array | Flatten nested |
| `flatMap()` | ❌ No | New array | Map + flatten |
| `map()` | ❌ No | New array | Transform elements |
| `filter()` | ❌ No | New array | Filter elements |
| `reduce()` | ❌ No | Single value | Reduce to value |
| `join()` | ❌ No | String | Convert to string |

### Search/Test Methods Comparison

| Method | Returns | Purpose | Example |
|--------|---------|---------|---------|
| `indexOf()` | Index or -1 | Find exact match | `arr.indexOf(5)` |
| `lastIndexOf()` | Index or -1 | Find exact match (from end) | `arr.lastIndexOf(5)` |
| `includes()` | Boolean | Check if contains | `arr.includes(5)` |
| `find()` | Element or undefined | Find first match | `arr.find(x => x > 5)` |
| `findIndex()` | Index or -1 | Find first match index | `arr.findIndex(x => x > 5)` |
| `findLast()` | Element or undefined | Find last match | `arr.findLast(x => x > 5)` |
| `findLastIndex()` | Index or -1 | Find last match index | `arr.findLastIndex(x => x > 5)` |
| `some()` | Boolean | Test if any passes | `arr.some(x => x > 5)` |
| `every()` | Boolean | Test if all pass | `arr.every(x => x > 5)` |

---

## 12. Interview Questions and Examples

### Question 1: Remove Duplicates from Array

```javascript
// Method 1: Using Set
function removeDuplicates1(arr) {
  return [...new Set(arr)];
}

// Method 2: Using filter
function removeDuplicates2(arr) {
  return arr.filter((item, index) => arr.indexOf(item) === index);
}

// Method 3: Using reduce
function removeDuplicates3(arr) {
  return arr.reduce((acc, item) => {
    return acc.includes(item) ? acc : [...acc, item];
  }, []);
}

const arr = [1, 2, 2, 3, 4, 4, 5];
console.log(removeDuplicates1(arr)); // [1, 2, 3, 4, 5]
console.log(removeDuplicates2(arr)); // [1, 2, 3, 4, 5]
console.log(removeDuplicates3(arr)); // [1, 2, 3, 4, 5]
```

### Question 2: Flatten Nested Array

```javascript
// Method 1: Using flat() with Infinity
function flatten1(arr) {
  return arr.flat(Infinity);
}

// Method 2: Recursive
function flatten2(arr) {
  return arr.reduce((acc, item) => {
    return acc.concat(Array.isArray(item) ? flatten2(item) : item);
  }, []);
}

// Method 3: Iterative with stack
function flatten3(arr) {
  const stack = [...arr];
  const result = [];
  
  while (stack.length) {
    const item = stack.pop();
    if (Array.isArray(item)) {
      stack.push(...item);
    } else {
      result.unshift(item);
    }
  }
  
  return result;
}

const nested = [1, [2, [3, [4, 5]]]];
console.log(flatten1(nested)); // [1, 2, 3, 4, 5]
console.log(flatten2(nested)); // [1, 2, 3, 4, 5]
console.log(flatten3(nested)); // [1, 2, 3, 4, 5]
```

### Question 3: Find Missing Number in Sequence

```javascript
// Given array of numbers 1 to n with one missing, find missing number

// Method 1: Using sum formula
function findMissing1(arr) {
  const n = arr.length + 1;
  const expectedSum = (n * (n + 1)) / 2;
  const actualSum = arr.reduce((a, b) => a + b, 0);
  return expectedSum - actualSum;
}

// Method 2: Using XOR
function findMissing2(arr) {
  const n = arr.length + 1;
  let xor1 = 0, xor2 = 0;
  
  for (let i = 1; i <= n; i++) {
    xor1 ^= i;
  }
  
  for (let num of arr) {
    xor2 ^= num;
  }
  
  return xor1 ^ xor2;
}

const arr = [1, 2, 3, 5, 6, 7, 8];
console.log(findMissing1(arr)); // 4
console.log(findMissing2(arr)); // 4
```

### Question 4: Rotate Array

```javascript
// Rotate array by k positions

// Method 1: Using slice and concat
function rotate1(arr, k) {
  k = k % arr.length; // Handle k > length
  return arr.slice(-k).concat(arr.slice(0, -k));
}

// Method 2: Using splice (mutates original)
function rotate2(arr, k) {
  k = k % arr.length;
  arr.unshift(...arr.splice(-k));
  return arr;
}

// Method 3: Using reverse (mutates original)
function rotate3(arr, k) {
  k = k % arr.length;
  reverse(arr, 0, arr.length - 1);
  reverse(arr, 0, k - 1);
  reverse(arr, k, arr.length - 1);
  return arr;
}

function reverse(arr, start, end) {
  while (start < end) {
    [arr[start], arr[end]] = [arr[end], arr[start]];
    start++;
    end--;
  }
}

const arr = [1, 2, 3, 4, 5, 6, 7];
console.log(rotate1([...arr], 3)); // [5, 6, 7, 1, 2, 3, 4]
console.log(rotate2([...arr], 3)); // [5, 6, 7, 1, 2, 3, 4]
console.log(rotate3([...arr], 3)); // [5, 6, 7, 1, 2, 3, 4]
```

### Question 5: Group Array Elements

```javascript
// Group array elements by a property or condition

// Method 1: Using reduce
function groupBy(arr, key) {
  return arr.reduce((acc, item) => {
    const group = typeof key === 'function' ? key(item) : item[key];
    if (!acc[group]) acc[group] = [];
    acc[group].push(item);
    return acc;
  }, {});
}

const users = [
  { name: 'John', age: 30, role: 'admin' },
  { name: 'Jane', age: 25, role: 'user' },
  { name: 'Bob', age: 30, role: 'admin' },
  { name: 'Alice', age: 25, role: 'user' }
];

console.log(groupBy(users, 'role'));
// {
//   admin: [{ name: 'John', ... }, { name: 'Bob', ... }],
//   user: [{ name: 'Jane', ... }, { name: 'Alice', ... }]
// }

console.log(groupBy(users, user => user.age));
// {
//   30: [{ name: 'John', ... }, { name: 'Bob', ... }],
//   25: [{ name: 'Jane', ... }, { name: 'Alice', ... }]
// }

// Note: ES2024 introduces Object.groupBy()
// const grouped = Object.groupBy(users, user => user.role);
```

### Question 6: Chunk Array

```javascript
// Split array into chunks of specified size

function chunk(arr, size) {
  const chunks = [];
  for (let i = 0; i < arr.length; i += size) {
    chunks.push(arr.slice(i, i + size));
  }
  return chunks;
}

// Alternative with reduce
function chunk2(arr, size) {
  return arr.reduce((acc, item, i) => {
    const chunkIndex = Math.floor(i / size);
    if (!acc[chunkIndex]) acc[chunkIndex] = [];
    acc[chunkIndex].push(item);
    return acc;
  }, []);
}

const arr = [1, 2, 3, 4, 5, 6, 7, 8];
console.log(chunk(arr, 3)); // [[1, 2, 3], [4, 5, 6], [7, 8]]
console.log(chunk2(arr, 3)); // [[1, 2, 3], [4, 5, 6], [7, 8]]
```

### Question 7: Intersection and Union of Arrays

```javascript
// Intersection: elements common to both arrays
function intersection(arr1, arr2) {
  const set2 = new Set(arr2);
  return [...new Set(arr1.filter(x => set2.has(x)))];
}

// Union: all unique elements from both arrays
function union(arr1, arr2) {
  return [...new Set([...arr1, ...arr2])];
}

// Difference: elements in arr1 but not in arr2
function difference(arr1, arr2) {
  const set2 = new Set(arr2);
  return arr1.filter(x => !set2.has(x));
}

const a = [1, 2, 3, 4, 5];
const b = [3, 4, 5, 6, 7];

console.log(intersection(a, b)); // [3, 4, 5]
console.log(union(a, b)); // [1, 2, 3, 4, 5, 6, 7]
console.log(difference(a, b)); // [1, 2]
console.log(difference(b, a)); // [6, 7]
```

### Question 8: Sort Array of Objects by Multiple Properties

```javascript
function multiSort(arr, props) {
  return arr.sort((a, b) => {
    for (let prop of props) {
      const order = prop.order || 'asc';
      const key = prop.key;
      
      if (a[key] < b[key]) return order === 'asc' ? -1 : 1;
      if (a[key] > b[key]) return order === 'asc' ? 1 : -1;
    }
    return 0;
  });
}

const users = [
  { name: 'John', age: 30, score: 85 },
  { name: 'Jane', age: 25, score: 95 },
  { name: 'Bob', age: 30, score: 75 },
  { name: 'Alice', age: 25, score: 85 }
];

// Sort by age (asc), then by score (desc)
console.log(multiSort([...users], [
  { key: 'age', order: 'asc' },
  { key: 'score', order: 'desc' }
]));
// Results ordered by age first, then score within same age
```

---

## 13. Best Practices and Common Pitfalls

### Best Practices

```javascript
// ✅ Use const for arrays (prevents reassignment)
const arr = [1, 2, 3];
arr.push(4); // OK (mutating content)
// arr = [5, 6]; // Error

// ✅ Use array methods over loops when possible
const numbers = [1, 2, 3, 4, 5];

// Instead of:
const doubled1 = [];
for (let i = 0; i < numbers.length; i++) {
  doubled1.push(numbers[i] * 2);
}

// Use:
const doubled2 = numbers.map(x => x * 2);

// ✅ Chain methods for readability
const result = numbers
  .filter(x => x % 2 === 0)
  .map(x => x * 2)
  .reduce((a, b) => a + b, 0);

// ✅ Use spread for copying (shallow)
const original = [1, 2, 3];
const copy = [...original];

// ✅ Use destructuring for clarity
const [first, second, ...rest] = arr;

// ✅ Check if array before using array methods
function processArray(data) {
  if (!Array.isArray(data)) {
    throw new TypeError('Expected array');
  }
  return data.map(x => x * 2);
}
```

### Common Pitfalls

```javascript
// ❌ Modifying array during iteration
const arr = [1, 2, 3, 4, 5];
arr.forEach((item, index) => {
  if (item % 2 === 0) {
    arr.splice(index, 1); // DON'T DO THIS!
  }
});
console.log(arr); // Unexpected results

// ✅ Use filter instead
const arr2 = [1, 2, 3, 4, 5];
const filtered = arr2.filter(item => item % 2 !== 0);
console.log(filtered); // [1, 3, 5]

// ❌ Forgetting that sort() mutates
const nums = [3, 1, 4, 1, 5];
const sorted = nums.sort(); // Mutates original!
console.log(nums); // [1, 1, 3, 4, 5] - CHANGED!

// ✅ Create copy first
const nums2 = [3, 1, 4, 1, 5];
const sorted2 = [...nums2].sort();
console.log(nums2); // [3, 1, 4, 1, 5] - unchanged
console.log(sorted2); // [1, 1, 3, 4, 5]

// ❌ Default sort with numbers
const numbers = [10, 5, 40, 25, 1000];
numbers.sort();
console.log(numbers); // [10, 1000, 25, 40, 5] - WRONG!

// ✅ Use comparator
numbers.sort((a, b) => a - b);
console.log(numbers); // [5, 10, 25, 40, 1000]

// ❌ Shallow copy issues with nested objects
const nested = [{ x: 1 }, { x: 2 }];
const copy = [...nested];
copy[0].x = 99;
console.log(nested[0].x); // 99 - CHANGED!

// ✅ Deep copy for nested structures
const deepCopy = JSON.parse(JSON.stringify(nested));
// Or use structuredClone() (modern browsers)
const deepCopy2 = structuredClone(nested);

// ❌ Array constructor with single number
const arr3 = new Array(5); // [<5 empty items>] - NOT [5]

// ✅ Use Array.of()
const arr4 = Array.of(5); // [5]

// ❌ Forgetting reduce initial value
const arr5 = [];
// arr5.reduce((a, b) => a + b); // TypeError
const sum = arr5.reduce((a, b) => a + b, 0); // ✅ 0

// ❌ Using delete on arrays
const arr6 = [1, 2, 3, 4, 5];
delete arr6[2];
console.log(arr6); // [1, 2, <1 empty item>, 4, 5]
console.log(arr6.length); // 5 (length unchanged!)

// ✅ Use splice instead
arr6.splice(2, 1);
console.log(arr6); // [1, 2, 4, 5]
```

---

## 14. Performance Considerations

### Time Complexity

| Operation | Time Complexity |
|-----------|----------------|
| Access by index | O(1) |
| push() | O(1) |
| pop() | O(1) |
| shift() | O(n) |
| unshift() | O(n) |
| splice() | O(n) |
| slice() | O(n) |
| concat() | O(n + m) |
| indexOf() | O(n) |
| includes() | O(n) |
| find() | O(n) |
| forEach() | O(n) |
| map() | O(n) |
| filter() | O(n) |
| reduce() | O(n) |
| sort() | O(n log n) |

### Performance Tips

```javascript
// ✅ Use appropriate method for adding elements
// Beginning: unshift() O(n) - slower
// End: push() O(1) - faster

// ✅ For large datasets, avoid nested loops
const arr1 = Array(1000).fill(0);
const arr2 = Array(1000).fill(0);

// ❌ O(n²)
arr1.forEach(item1 => {
  arr2.forEach(item2 => {
    // process
  });
});

// ✅ Use Set for lookups O(n)
const set2 = new Set(arr2);
arr1.forEach(item1 => {
  if (set2.has(item1)) {
    // process
  }
});

// ✅ Cache length in loops (negligible in modern JS)
// But still good practice
for (let i = 0, len = arr.length; i < len; i++) {
  // process
}

// ✅ Use method chaining carefully
// Each method creates new array - memory overhead
const result1 = arr
  .map(x => x * 2)      // New array
  .filter(x => x > 10)  // New array
  .map(x => x + 1);     // New array

// ✅ Combine operations when possible
const result2 = arr.reduce((acc, x) => {
  const doubled = x * 2;
  if (doubled > 10) {
    acc.push(doubled + 1);
  }
  return acc;
}, []); // Single pass, one new array

// ✅ Use for...of for simple iterations (faster than forEach)
for (const item of arr) {
  console.log(item);
}

// ✅ Avoid creating large arrays in loops
// ❌ Bad
let result = [];
for (let i = 0; i < 1000000; i++) {
  result = result.concat([i]); // Creates new array each time!
}

// ✅ Good
const result = [];
for (let i = 0; i < 1000000; i++) {
  result.push(i); // Modifies existing array
}
```

---

## Summary

### Key Takeaways

1. **Arrays are versatile**: Can store any data type, are dynamic, and zero-indexed
2. **Creation methods**: Literal `[]`, constructor `new Array()`, `Array.of()`, `Array.from()`
3. **Mutating methods** modify original: `push`, `pop`, `shift`, `unshift`, `splice`, `sort`, `reverse`, `fill`
4. **Non-mutating methods** return new array: `slice`, `concat`, `map`, `filter`, `flat`, `flatMap`
5. **Iteration methods**: `forEach`, `map`, `filter`, `reduce`, `find`, `some`, `every`
6. **Modern features**: Destructuring, spread operator, `at()`, `flat()`, `flatMap()`
7. **Performance**: Consider time complexity, use appropriate methods, avoid nested loops
8. **Common pitfalls**: Default sort behavior, shallow copying, modifying during iteration

### When to Use Which Method

**Adding/Removing:**
- End: `push()`, `pop()` (fastest - O(1))
- Beginning: `unshift()`, `shift()` (slower - O(n))
- Anywhere: `splice()` (O(n))

**Searching:**
- Exact match: `indexOf()`, `includes()`
- With condition: `find()`, `findIndex()`, `some()`, `every()`

**Transforming:**
- Transform all: `map()`
- Filter subset: `filter()`
- Reduce to value: `reduce()`
- Flatten: `flat()`, `flatMap()`

**Copying:**
- Shallow: `slice()`, spread `[...]`, `Array.from()`
- Deep: `JSON.parse(JSON.stringify())`, `structuredClone()`

Arrays are fundamental to JavaScript programming and mastering them is essential for technical interviews and real-world development. Practice these concepts with the provided examples and you'll be well-prepared!
