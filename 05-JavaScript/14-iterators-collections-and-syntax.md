# Iterators, Collections, and Modern Syntax

This guide covers iterators and iterables, the built-in collection types (Map, Set, WeakMap, WeakSet), the `for...of` loop, common array methods for slicing data, destructuring, the rest/spread syntax, and arrow functions.

## Iterators & Iterables

### What are Iterables?

An **iterable** is an object that implements the **iterable protocol** by having a method with the key `Symbol.iterator` that returns an iterator.

Common iterables:
- Arrays
- Strings
- Maps
- Sets
- NodeList
- Arguments object

### What are Iterators?

An **iterator** is an object that implements the **iterator protocol** by having a `next()` method that returns:
- `value`: The next value in the sequence
- `done`: Boolean indicating if iteration is complete

### The Iterable Protocol (Built-in Iterables)

```javascript
// Arrays are iterable
const arr = [1, 2, 3];
const iterator = arr[Symbol.iterator]();

console.log(iterator.next()); // { value: 1, done: false }
console.log(iterator.next()); // { value: 2, done: false }
console.log(iterator.next()); // { value: 3, done: false }
console.log(iterator.next()); // { value: undefined, done: true }

// Strings are iterable
const str = 'hello';
const strIterator = str[Symbol.iterator]();

console.log(strIterator.next()); // { value: 'h', done: false }
console.log(strIterator.next()); // { value: 'e', done: false }
```

### Creating Custom Iterables

#### Example 1: Simple Range

```javascript
const range = {
  start: 1,
  end: 5,

  [Symbol.iterator]() {
    let current = this.start;
    const end = this.end;

    return {
      next() {
        if (current <= end) {
          return { value: current++, done: false };
        } else {
          return { value: undefined, done: true };
        }
      }
    };
  }
};

for (const num of range) {
  console.log(num); // 1, 2, 3, 4, 5
}

// Convert to array
console.log([...range]); // [1, 2, 3, 4, 5]
```

#### Example 2: Fibonacci Sequence

```javascript
const fibonacci = {
  [Symbol.iterator]() {
    let prev = 0;
    let curr = 1;
    let count = 0;
    const maxCount = 10;

    return {
      next() {
        if (count++ < maxCount) {
          const value = prev;
          [prev, curr] = [curr, prev + curr];
          return { value, done: false };
        }
        return { value: undefined, done: true };
      }
    };
  }
};

console.log([...fibonacci]); // [0, 1, 1, 2, 3, 5, 8, 13, 21, 34]
```

#### Example 3: Custom Collection Class

```javascript
class LinkedList {
  constructor() {
    this.head = null;
    this.size = 0;
  }

  add(value) {
    const node = { value, next: null };

    if (!this.head) {
      this.head = node;
    } else {
      let current = this.head;
      while (current.next) {
        current = current.next;
      }
      current.next = node;
    }

    this.size++;
  }

  [Symbol.iterator]() {
    let current = this.head;

    return {
      next() {
        if (current) {
          const value = current.value;
          current = current.next;
          return { value, done: false };
        }
        return { value: undefined, done: true };
      }
    };
  }
}

const list = new LinkedList();
list.add(1);
list.add(2);
list.add(3);

for (const value of list) {
  console.log(value); // 1, 2, 3
}

console.log([...list]); // [1, 2, 3]
```

### Generator Functions (Easy Iterables)

Generators provide an easier way to create iterables using `function*` and `yield`.

```javascript
function* numberGenerator() {
  yield 1;
  yield 2;
  yield 3;
}

const gen = numberGenerator();

console.log(gen.next()); // { value: 1, done: false }
console.log(gen.next()); // { value: 2, done: false }
console.log(gen.next()); // { value: 3, done: false }
console.log(gen.next()); // { value: undefined, done: true }

// Use in for...of
for (const num of numberGenerator()) {
  console.log(num); // 1, 2, 3
}
```

#### Range Generator

```javascript
function* range(start, end) {
  for (let i = start; i <= end; i++) {
    yield i;
  }
}

console.log([...range(1, 5)]); // [1, 2, 3, 4, 5]

for (const num of range(10, 15)) {
  console.log(num); // 10, 11, 12, 13, 14, 15
}
```

#### Fibonacci Generator

```javascript
function* fibonacci(count) {
  let prev = 0;
  let curr = 1;

  for (let i = 0; i < count; i++) {
    yield prev;
    [prev, curr] = [curr, prev + curr];
  }
}

console.log([...fibonacci(10)]); // [0, 1, 1, 2, 3, 5, 8, 13, 21, 34]
```

#### Infinite Generator

```javascript
function* infiniteSequence() {
  let i = 0;
  while (true) {
    yield i++;
  }
}

const gen = infiniteSequence();

console.log(gen.next().value); // 0
console.log(gen.next().value); // 1
console.log(gen.next().value); // 2

// Take first 5
const firstFive = [];
const generator = infiniteSequence();
for (let i = 0; i < 5; i++) {
  firstFive.push(generator.next().value);
}
console.log(firstFive); // [0, 1, 2, 3, 4]
```

### Iterable Operations

#### Spread Operator

```javascript
const range = {
  start: 1,
  end: 3,
  [Symbol.iterator]() {
    let current = this.start;
    return {
      next: () => {
        if (current <= this.end) {
          return { value: current++, done: false };
        }
        return { value: undefined, done: true };
      }
    };
  }
};

const arr = [...range];
console.log(arr); // [1, 2, 3]

const set = new Set(range);
console.log(set); // Set { 1, 2, 3 }
```

#### Destructuring

```javascript
function* colors() {
  yield 'red';
  yield 'green';
  yield 'blue';
}

const [first, second, ...rest] = colors();
console.log(first);  // 'red'
console.log(second); // 'green'
console.log(rest);   // ['blue']
```

#### Array.from()

```javascript
function* numbers() {
  yield 1;
  yield 2;
  yield 3;
}

const arr = Array.from(numbers());
console.log(arr); // [1, 2, 3]
```

### Real-World Examples

#### Example 1: Pagination Iterator

```javascript
class PaginatedAPI {
  constructor(data, pageSize = 10) {
    this.data = data;
    this.pageSize = pageSize;
  }

  [Symbol.iterator]() {
    let index = 0;
    const data = this.data;
    const pageSize = this.pageSize;

    return {
      next() {
        if (index < data.length) {
          const page = data.slice(index, index + pageSize);
          index += pageSize;
          return { value: page, done: false };
        }
        return { value: undefined, done: true };
      }
    };
  }
}

const data = Array.from({ length: 35 }, (_, i) => i + 1);
const api = new PaginatedAPI(data, 10);

for (const page of api) {
  console.log('Page:', page);
}
// Page: [1, 2, ..., 10]
// Page: [11, 12, ..., 20]
// Page: [21, 22, ..., 30]
// Page: [31, 32, 33, 34, 35]
```

#### Example 2: Async Iterator (Node.js Streams)

```javascript
// Async iterator
async function* asyncNumbers() {
  for (let i = 1; i <= 5; i++) {
    await new Promise(resolve => setTimeout(resolve, 1000));
    yield i;
  }
}

// Use with for await...of
async function main() {
  for await (const num of asyncNumbers()) {
    console.log(num); // Logs 1, 2, 3, 4, 5 (one per second)
  }
}

main();
```

#### Example 3: Tree Traversal

```javascript
class TreeNode {
  constructor(value) {
    this.value = value;
    this.children = [];
  }

  addChild(node) {
    this.children.push(node);
  }

  *[Symbol.iterator]() {
    // Depth-first traversal
    yield this.value;
    for (const child of this.children) {
      yield* child; // Delegate to child's iterator
    }
  }
}

const root = new TreeNode(1);
const child1 = new TreeNode(2);
const child2 = new TreeNode(3);
const grandchild1 = new TreeNode(4);
const grandchild2 = new TreeNode(5);

root.addChild(child1);
root.addChild(child2);
child1.addChild(grandchild1);
child1.addChild(grandchild2);

console.log([...root]); // [1, 2, 4, 5, 3]
```

#### Example 4: Custom String Iterator

```javascript
class CustomString {
  constructor(str) {
    this.str = str;
  }

  *[Symbol.iterator]() {
    for (let i = 0; i < this.str.length; i += 2) {
      yield this.str.slice(i, i + 2);
    }
  }
}

const str = new CustomString('hello world');
console.log([...str]); // ['he', 'll', 'o ', 'wo', 'rl', 'd']
```

### Iterator Helpers (Proposal)

Future JavaScript may include iterator helper methods (proposed features, not yet fully standardized):

```javascript
function* numbers() {
  yield 1;
  yield 2;
  yield 3;
  yield 4;
  yield 5;
}

// map
const doubled = numbers().map(x => x * 2);
console.log([...doubled]); // [2, 4, 6, 8, 10]

// filter
const evens = numbers().filter(x => x % 2 === 0);
console.log([...evens]); // [2, 4]

// take
const firstThree = numbers().take(3);
console.log([...firstThree]); // [1, 2, 3]

// drop
const skipTwo = numbers().drop(2);
console.log([...skipTwo]); // [3, 4, 5]
```

**Current workaround:**

```javascript
function* map(iterable, fn) {
  for (const value of iterable) {
    yield fn(value);
  }
}

function* filter(iterable, predicate) {
  for (const value of iterable) {
    if (predicate(value)) {
      yield value;
    }
  }
}

const numbers = [1, 2, 3, 4, 5];
const doubled = map(numbers, x => x * 2);
const evens = filter(doubled, x => x > 4);

console.log([...evens]); // [6, 8, 10]
```

### Interview Questions

#### Q1: Make this object iterable

```javascript
const myObject = {
  data: [1, 2, 3, 4, 5]
};

// Add iterator (using generator)
myObject[Symbol.iterator] = function* () {
  for (const item of this.data) {
    yield item;
  }
};

// Or without generator
myObject[Symbol.iterator] = function() {
  let index = 0;
  const data = this.data;

  return {
    next() {
      if (index < data.length) {
        return { value: data[index++], done: false };
      }
      return { value: undefined, done: true };
    }
  };
};

console.log([...myObject]); // [1, 2, 3, 4, 5]
```

#### Q2: What's the output?

```javascript
function* gen() {
  yield 1;
  yield 2;
  return 3;
}

const arr = [...gen()];
console.log(arr); // ?
```

**Answer:** `[1, 2]` — The `return` value is not included in spread/for...of.

#### Q3: Implement a `take` function

```javascript
function* take(iterable, n) {
  let count = 0;
  for (const value of iterable) {
    if (count++ >= n) break;
    yield value;
  }
}

// Test
function* numbers() {
  let i = 0;
  while (true) yield i++;
}

console.log([...take(numbers(), 5)]); // [0, 1, 2, 3, 4]
```

### Key Concepts & Tips

1. **Iterable**: Object with `[Symbol.iterator]()` method
2. **Iterator**: Object with `next()` method returning `{ value, done }`
3. **for...of**: Works with any iterable
4. **Generators**: Easy way to create iterators using `function*` and `yield`
5. **Spread operator**: Works with iterables

Best practices:
- Use generators for complex iteration logic
- Implement `[Symbol.iterator]` for custom collections
- Use `for...of` instead of manual iterator calls
- Remember: `return` value in generators is not iterated
- Use async iterators for async operations

Interview tips:
- Understand the difference between iterable and iterator
- Know how to implement custom iterables
- Be familiar with generator functions
- Understand when to use iterables vs arrays
- Know that `for...of` only works with iterables (not plain objects)

## Map/Set vs WeakMap/WeakSet

JavaScript provides powerful collection types beyond arrays and objects.

### Set

- Stores **unique values** (no duplicates).
- Values can be any type (primitives or objects).
- Insertion order is preserved.

```javascript
const mySet = new Set([1, 2, 3, 3, 4]);
console.log(mySet); // Set(4) { 1, 2, 3, 4 }

mySet.add(5);
console.log(mySet.has(3)); // true
mySet.delete(2);

for (let val of mySet) console.log(val); // 1, 3, 4, 5
```

**Best for:** Removing duplicates, storing unique values.

### Map

- Stores **key–value pairs**.
- Keys can be **any type** (even objects or functions).
- Preserves insertion order.

```javascript
const myMap = new Map();
myMap.set("name", "Alice");
myMap.set(1, "number one");
myMap.set({ id: 1 }, "object key");

console.log(myMap.get("name")); // Alice
console.log(myMap.has(1));      // true
myMap.delete("name");
```

**Best for:** Key–value storage when keys aren't limited to strings.

### WeakSet

- Like **Set**, but only stores **objects**.
- References are **weak** → objects can be garbage-collected if no other reference exists.
- Not iterable (no `.forEach`, no `size`).

```javascript
let obj1 = { name: "Alice" };
let obj2 = { name: "Bob" };

const weakSet = new WeakSet([obj1, obj2]);
console.log(weakSet.has(obj1)); // true

obj1 = null; // obj1 removed automatically from weakSet
```

**Best for:** Temporary object references without memory leaks.

### WeakMap

- Like **Map**, but keys must be **objects**.
- References are **weak** → if the key object is garbage-collected, entry is removed.
- Not iterable (no `.forEach`, no `size`).

```javascript
let obj = { id: 1 };
const weakMap = new WeakMap();
weakMap.set(obj, "secret data");

console.log(weakMap.get(obj)); // secret data
obj = null; // entry removed automatically
```

**Best for:** Storing private metadata tied to object lifecycle.

### Key Differences (Cheat Sheet)

| Feature | Set | Map | WeakSet | WeakMap |
|---------|-----|-----|---------|---------|
| **Stores** | Unique values | Key–value pairs | Objects only | Key–value pairs (keys = objects only) |
| **Key types** | N/A | Any type | Objects only | Objects only |
| **Garbage collection** | No | No | Yes | Yes |
| **Iterable** | Yes | Yes | No | No |
| **Duplicates** | No | No (keys unique) | No | No |

### Real-Life Analogy

- **Set** → A guest list (no duplicate names).
- **Map** → A phone book (name → phone number).
- **WeakSet** → A backstage pass (only valid while the person exists).
- **WeakMap** → A locker room (key = person, value = locker; if the person leaves, the locker is removed).

## for...of

The **`for...of` loop** lets you **iterate directly over iterable objects** like arrays, strings, maps, sets, etc. It gives you **values**, not indexes (unlike `for` or `for...in`).

### Example with Arrays

```javascript
const numbers = [10, 20, 30];

for (let num of numbers) {
  console.log(num);
}
// 10
// 20
// 30
```

### Example with Strings

```javascript
const str = "Hi";

for (let ch of str) {
  console.log(ch);
}
// H
// i
```

### Example with Set

```javascript
const mySet = new Set([1, 2, 3]);

for (let val of mySet) {
  console.log(val);
}
// 1
// 2
// 3
```

### Example with Map

```javascript
const myMap = new Map([
  ["name", "Alice"],
  ["age", 25]
]);

for (let [key, value] of myMap) {
  console.log(key, value);
}
// name Alice
// age 25
```

### `for...in` vs `for...of`

- **`for...in`** → iterates **keys/indexes** (good for objects).
- **`for...of`** → iterates **values** (good for arrays, strings, iterables).

```javascript
const arr = ["a", "b", "c"];

for (let i in arr) {
  console.log(i); // 0, 1, 2 (indexes)
}

for (let val of arr) {
  console.log(val); // a, b, c (values)
}
```

### Arrays (including array of objects)

An **array is iterable**, so `for...of` is the best choice when you want **values**.

```javascript
const users = [
  { name: "Alice", age: 25 },
  { name: "Bob", age: 30 }
];

for (let user of users) {
  console.log(user.name);
}
// Alice
// Bob
```

### When to Use `for...in`

`for...in` is designed for **objects**, not arrays. It iterates over **keys (property names)**, not values.

```javascript
const user = { name: "Alice", age: 25 };

for (let key in user) {
  console.log(key, user[key]);
}
// name Alice
// age 25
```

If you use `for...in` on an array, you'll get **indexes**:

```javascript
const arr = ["a", "b", "c"];

for (let i in arr) {
  console.log(i);       // 0, 1, 2
  console.log(arr[i]);  // a, b, c
}
```

But this is **not recommended**, because:
- It may include inherited properties (if someone extends `Array.prototype`).
- The order is not guaranteed (though it usually works like indexes).

### Golden Rule

- Use **`for...of`** → When you need **values** (arrays, strings, maps, sets, array of objects).
- Use **`for...in`** → When you need **keys/properties** of a plain object.
- Avoid `for...in` with arrays (use `for...of` instead).

### Real-Life Analogy

- **`for...in`** = Walking through a **dictionary's index** (keys).
- **`for...of`** = Reading the **actual words/definitions** (values).

## Array filter/map/reduce

These higher-order array methods are central to functional-style data transformation. Below are common usages, including ways to take the **first two elements** of an array.

### Using `slice()`

```javascript
const arr = [10, 20, 30, 40, 50];
const firstTwo = arr.slice(0, 2);

console.log(firstTwo); // [10, 20]
```

`slice(start, end)` extracts elements from `start` index up to (but not including) `end`.

### Using Array Destructuring

```javascript
const arr = [10, 20, 30, 40, 50];
const [first, second] = arr;

console.log(first, second); // 10 20
```

If you want them as a **new array**:

```javascript
const arr = [10, 20, 30, 40, 50];
const [first, second] = arr;
const firstTwo = [first, second];

console.log(firstTwo); // [10, 20]
```

### Using `filter()` (less common for slicing)

```javascript
const arr = [10, 20, 30, 40, 50];
const firstTwo = arr.filter((_, index) => index < 2);

console.log(firstTwo); // [10, 20]
```

Works fine, but not as efficient as `slice()`.

### `map()` and `reduce()`

`map()` transforms each element into a new value (returning a new array of the same length); `reduce()` folds the array into a single accumulated value.

```javascript
const nums = [1, 2, 3, 4, 5];

// map: transform each element
const doubled = nums.map(n => n * 2);
console.log(doubled); // [2, 4, 6, 8, 10]

// filter: keep elements that pass a predicate
const evens = nums.filter(n => n % 2 === 0);
console.log(evens); // [2, 4]

// reduce: accumulate into a single value
const sum = nums.reduce((acc, n) => acc + n, 0);
console.log(sum); // 15
```

### Best Practices

- Use **`slice(0, 2)`** → when you need a sub-array.
- Use **destructuring** → when you just want the first two elements as separate variables.
- Use **`map`** for one-to-one transforms, **`filter`** for selecting a subset, and **`reduce`** for aggregation.

## Destructuring

Destructuring lets you unpack values from arrays or properties from objects into distinct variables.

### Object Destructuring (with renaming)

```javascript
const classDetails = {
  strength: 78,
  benches: 39,
  blackBoard: 1
};

const { strength: classStrength, benches: classBenches, blackBoard: classBlackBoard } = classDetails;

console.log(classStrength);     // Outputs 78
console.log(classBenches);      // Outputs 39
console.log(classBlackBoard);   // Outputs 1
```

### Array Destructuring

Before the ES6 version:

```javascript
const arr = [1, 2, 3, 4];
const first = arr[0];
const second = arr[1];
const third = arr[2];
const fourth = arr[3];
```

Now (ES6):

```javascript
const arr = [1, 2, 3, 4];
const [first, second, third, fourth] = arr;

console.log(first);  // Outputs 1
console.log(second); // Outputs 2
console.log(third);  // Outputs 3
console.log(fourth); // Outputs 4
```

## Rest vs Spread

Both use the `...` syntax, but they serve **different purposes**.

### Rest Parameter (`...`)

Used in **function definitions** to collect multiple arguments into an array. Think of it as: "**pack** arguments into a box."

```javascript
function sum(...numbers) {
  return numbers.reduce((a, b) => a + b, 0);
}

console.log(sum(1, 2, 3, 4)); // 10
```

With multiple params:

```javascript
function introduce(firstName, ...hobbies) {
  console.log(firstName, "likes", hobbies);
}

introduce("Alice", "reading", "coding", "music");
// Alice likes [ 'reading', 'coding', 'music' ]
```

The rest parameter only works as the **last parameter** in a function.

### Spread Operator (`...`)

Used to **unpack** elements from an array (or object) into individual items. Think of it as: "**unpack** the box."

In arrays:

```javascript
const arr = [1, 2, 3];
const newArr = [...arr, 4, 5];
console.log(newArr); // [1, 2, 3, 4, 5]
```

In objects:

```javascript
const obj = { name: "Alice", age: 25 };
const newObj = { ...obj, city: "Paris" };
console.log(newObj); // { name: 'Alice', age: 25, city: 'Paris' }
```

In function calls:

```javascript
function greet(a, b, c) {
  console.log(a, b, c);
}

const values = ["Hi", "there", "!"];
greet(...values); // Hi there !
```

### Key Differences

| Feature | Rest Parameter | Spread Operator |
|---------|----------------|-----------------|
| **Usage** | In function **parameters** | In arrays, objects, or function calls |
| **Action** | **Collects (packs)** multiple values into an array | **Expands (unpacks)** an array/object into individual values |
| **Syntax location** | Function definition | Function call, object/array literals |
| **Example** | `function f(...args) {}` | `f(...arr)` |

### Real-Life Analogy

- **Rest**: Packing clothes into a suitcase (collecting items).
- **Spread**: Taking clothes out of a suitcase and laying them out (expanding items).

## Arrow Functions

Arrow functions were introduced in the ES6 version of JavaScript. They provide a new and shorter syntax for declaring functions. Arrow functions can only be used as a function expression.

```javascript
// Traditional function expression
var multiplyBy2 = function(num){
  return num * 2;
}

// Arrow function expression
var arrowMultiplyBy2 = num => num * 2;
```
