# Iterators and Iterables in JavaScript

## What are Iterables?

An **iterable** is an object that implements the **iterable protocol** by having a method with the key `Symbol.iterator` that returns an iterator.

Common iterables:
- Arrays
- Strings
- Maps
- Sets
- NodeList
- Arguments object

---

## What are Iterators?

An **iterator** is an object that implements the **iterator protocol** by having a `next()` method that returns:
- `value`: The next value in the sequence
- `done`: Boolean indicating if iteration is complete

---

## The Iterable Protocol

### Built-in Iterables

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

### for...of Loop

The `for...of` loop works with any iterable.

```javascript
// Arrays
for (const num of [1, 2, 3]) {
  console.log(num);
}

// Strings
for (const char of 'hello') {
  console.log(char);
}

// Maps
const map = new Map([
  ['a', 1],
  ['b', 2]
]);

for (const [key, value] of map) {
  console.log(key, value);
}

// Sets
const set = new Set([1, 2, 3]);
for (const num of set) {
  console.log(num);
}
```

---

## Creating Custom Iterables

### Example 1: Simple Range

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

### Example 2: Fibonacci Sequence

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

### Example 3: Custom Collection Class

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

---

## Generator Functions (Easy Iterables)

Generators provide an easier way to create iterables using `function*` and `yield`.

### Basic Generator

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

### Range Generator

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

### Fibonacci Generator

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

### Infinite Generator

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

---

## Iterable Operations

### Spread Operator

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

### Destructuring

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

### Array.from()

```javascript
function* numbers() {
  yield 1;
  yield 2;
  yield 3;
}

const arr = Array.from(numbers());
console.log(arr); // [1, 2, 3]
```

---

## Real-World Examples

### Example 1: Pagination Iterator

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

### Example 2: Async Iterator (Node.js Streams)

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

### Example 3: Tree Traversal

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

### Example 4: Custom String Iterator

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

---

## Iterator Helpers (Proposal)

Future JavaScript may include iterator helper methods:

```javascript
// Note: These are proposed features, not yet standardized

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

---

## Interview Questions

### Q1: Make this object iterable

```javascript
const myObject = {
  data: [1, 2, 3, 4, 5]
};

// Add iterator
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

### Q2: What's the output?

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

### Q3: Implement a `take` function

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

---

## Summary

### Key Concepts

1. **Iterable**: Object with `[Symbol.iterator]()` method
2. **Iterator**: Object with `next()` method returning `{ value, done }`
3. **for...of**: Works with any iterable
4. **Generators**: Easy way to create iterators using `function*` and `yield`
5. **Spread operator**: Works with iterables

### Common Iterables

- Arrays: `[1, 2, 3]`
- Strings: `'hello'`
- Maps: `new Map()`
- Sets: `new Set()`
- Arguments: `arguments`

### Best Practices

- ✅ Use generators for complex iteration logic
- ✅ Implement `[Symbol.iterator]` for custom collections
- ✅ Use `for...of` instead of manual iterator calls
- ✅ Remember: `return` value in generators is not iterated
- ✅ Async iterators for async operations

### Interview Tips

- Understand the difference between iterable and iterator
- Know how to implement custom iterables
- Be familiar with generator functions
- Understand when to use iterables vs arrays
- Know that `for...of` only works with iterables (not plain objects)

Master iterators to create powerful, memory-efficient data structures! 🚀
