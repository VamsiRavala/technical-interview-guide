# Symbols in JavaScript

## What are Symbols?

**Symbol** is a primitive data type introduced in ES6. Each Symbol value is **unique** and **immutable**, making it perfect for creating unique property keys that won't conflict with other properties.

```javascript
const sym1 = Symbol();
const sym2 = Symbol();

console.log(sym1 === sym2); // false (each Symbol is unique)
console.log(typeof sym1);   // "symbol"
```

---

## Creating Symbols

### Basic Symbol Creation

```javascript
// Without description
const sym1 = Symbol();

// With description (for debugging)
const sym2 = Symbol('mySymbol');
const sym3 = Symbol('mySymbol');

console.log(sym2 === sym3); // false (still unique)
console.log(sym2.toString()); // "Symbol(mySymbol)"
console.log(sym2.description); // "mySymbol"
```

###Symbol.for() - Global Symbol Registry

Creates or retrieves a symbol from a global registry.

```javascript
// Create/get from global registry
const sym1 = Symbol.for('app.id');
const sym2 = Symbol.for('app.id');

console.log(sym1 === sym2); // true (same symbol from registry)

// Get key for a symbol
console.log(Symbol.keyFor(sym1)); // "app.id"

// Regular symbols don't have keys
const localSym = Symbol('local');
console.log(Symbol.keyFor(localSym)); // undefined
```

---

## Using Symbols as Property Keys

### Unique Property Keys

```javascript
const id = Symbol('id');
const user = {
  name: 'Alice',
  [id]: 12345 // Symbol as property key
};

console.log(user.name);  // "Alice"
console.log(user[id]);   // 12345
console.log(user.id);    // undefined (symbol properties don't work with dot notation)

// Symbols don't appear in Object.keys()
console.log(Object.keys(user));        // ["name"]
console.log(Object.getOwnPropertyNames(user)); // ["name"]

// But can be accessed with Object.getOwnPropertySymbols()
console.log(Object.getOwnPropertySymbols(user)); // [Symbol(id)]

// JSON.stringify ignores symbol properties
console.log(JSON.stringify(user)); // {"name":"Alice"}
```

### Preventing Property Name Collisions

```javascript
// Library A
const SIZE = Symbol('size');

class BoxA {
  constructor() {
    this[SIZE] = 100;
  }
  
  getSize() {
    return this[SIZE];
  }
}

// Library B (different code, same property name)
const SIZE_B = Symbol('size');

class BoxB {
  constructor() {
    this[SIZE_B] = 200;
  }
  
  getSize() {
    return this[SIZE_B];
  }
}

// No conflict!
const box = { ...new BoxA(), ...new BoxB() };
console.log(box[SIZE]);   // 100
console.log(box[SIZE_B]); // 200
```

---

## Well-Known Symbols

JavaScript has built-in symbols that customize object behavior.

### Symbol.iterator

Make objects iterable.

```javascript
const myIterable = {
  data: [1, 2, 3],
  [Symbol.iterator]() {
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
  }
};

for (const value of myIterable) {
  console.log(value); // 1, 2, 3
}

console.log([...myIterable]); // [1, 2, 3]
```

### Symbol.toStringTag

Customize object type string.

```javascript
class MyClass {
  get [Symbol.toStringTag]() {
    return 'MyClass';
  }
}

const obj = new MyClass();
console.log(Object.prototype.toString.call(obj)); // "[object MyClass]"

// Default behavior
const plainObj = {};
console.log(Object.prototype.toString.call(plainObj)); // "[object Object]"
```

### Symbol.hasInstance

Customize `instanceof` behavior.

```javascript
class MyArray {
  static [Symbol.hasInstance](instance) {
    return Array.isArray(instance);
  }
}

console.log([] instanceof MyArray);       // true
console.log([1, 2] instanceof MyArray);   // true
console.log({} instanceof MyArray);       // false
```

### Symbol.toPrimitive

Control type conversion.

```javascript
const obj = {
  value: 42,
  [Symbol.toPrimitive](hint) {
    if (hint === 'number') {
      return this.value;
    }
    if (hint === 'string') {
      return `Value: ${this.value}`;
    }
    return this.value;
  }
};

console.log(+obj);        // 42 (number)
console.log(`${obj}`);    // "Value: 42" (string)
console.log(obj + 10);    // 52 (default)
```

### Symbol.isConcatSpreadable

Control array spreading in concat.

```javascript
const arr1 = [1, 2];
const arr2 = [3, 4];

console.log(arr1.concat(arr2)); // [1, 2, 3, 4]

// Prevent spreading
arr2[Symbol.isConcatSpreadable] = false;
console.log(arr1.concat(arr2)); // [1, 2, [3, 4]]

// Make non-array spreadable
const arrayLike = {
  0: 'a',
  1: 'b',
  length: 2,
  [Symbol.isConcatSpreadable]: true
};

console.log(['x'].concat(arrayLike)); // ['x', 'a', 'b']
```

---

## Common Well-Known Symbols

| Symbol | Purpose |
|--------|---------|
| `Symbol.iterator` | Make object iterable (for...of) |
| `Symbol.asyncIterator` | Make object async iterable |
| `Symbol.toStringTag` | Customize Object.prototype.toString() |
| `Symbol.toPrimitive` | Control type coercion |
| `Symbol.hasInstance` | Customize instanceof |
| `Symbol.species` | Control constructor for derived objects |
| `Symbol.isConcatSpreadable` | Control array concat behavior |
| `Symbol.match` | Customize String.match() |
| `Symbol.replace` | Customize String.replace() |
| `Symbol.search` | Customize String.search() |
| `Symbol.split` | Customize String.split() |

---

## Use Cases

### 1. Private-Like Properties

```javascript
const _privateValue = Symbol('privateValue');

class Counter {
  constructor() {
    this[_privateValue] = 0;
  }
  
  increment() {
    this[_privateValue]++;
  }
  
  getValue() {
    return this[_privateValue];
  }
}

const counter = new Counter();
counter.increment();
console.log(counter.getValue()); // 1

// Can't access directly without the symbol
console.log(counter.privateValue); // undefined
console.log(Object.keys(counter)); // []

// But it's not truly private (can still access if you have the symbol)
console.log(counter[_privateValue]); // 1
```

### 2. Enum-Like Values

```javascript
const Colors = {
  RED: Symbol('red'),
  GREEN: Symbol('green'),
  BLUE: Symbol('blue')
};

function setColor(color) {
  switch (color) {
    case Colors.RED:
      console.log('Color set to red');
      break;
    case Colors.GREEN:
      console.log('Color set to green');
      break;
    case Colors.BLUE:
      console.log('Color set to blue');
      break;
    default:
      console.log('Unknown color');
  }
}

setColor(Colors.RED);   // "Color set to red"
setColor('red');        // "Unknown color" (string doesn't match symbol)
```

### 3. Metadata Keys

```javascript
const metadata = Symbol('metadata');

class User {
  constructor(name) {
    this.name = name;
    this[metadata] = {
      createdAt: new Date(),
      version: '1.0'
    };
  }
  
  getMetadata() {
    return this[metadata];
  }
}

const user = new User('Alice');
console.log(user.name);            // "Alice"
console.log(user.getMetadata());   // { createdAt: ..., version: '1.0' }

// Metadata doesn't show up in regular enumeration
console.log(Object.keys(user));    // ["name"]
console.log(JSON.stringify(user)); // {"name":"Alice"}
```

### 4. Observable Pattern

```javascript
const observers = Symbol('observers');

class Observable {
  constructor() {
    this[observers] = [];
  }
  
  subscribe(observer) {
    this[observers].push(observer);
  }
  
  unsubscribe(observer) {
    const index = this[observers].indexOf(observer);
    if (index > -1) {
      this[observers].splice(index, 1);
    }
  }
  
  notify(data) {
    this[observers].forEach(observer => observer(data));
  }
}

const observable = new Observable();

observable.subscribe(data => console.log('Observer 1:', data));
observable.subscribe(data => console.log('Observer 2:', data));

observable.notify('Hello'); // Both observers notified
```

---

## Symbol Properties in Classes

```javascript
const secret = Symbol('secret');

class SecureData {
  constructor(publicData, privateData) {
    this.public = publicData;
    this[secret] = privateData;
  }
  
  static [Symbol.hasInstance](instance) {
    return instance && typeof instance[secret] !== 'undefined';
  }
  
  [Symbol.toStringTag]() {
    return 'SecureData';
  }
  
  *[Symbol.iterator]() {
    yield ['public', this.public];
    yield ['secret', '***hidden***'];
  }
}

const data = new SecureData('visible', 'hidden');

console.log(data.public);       // "visible"
console.log(data[secret]);      // "hidden"
console.log(data.secret);       // undefined

// Custom instanceof
console.log(data instanceof SecureData); // true

// Custom toString
console.log(Object.prototype.toString.call(data)); // "[object SecureData]"

// Iterable
for (const [key, value] of data) {
  console.log(key, value);
}
// "public" "visible"
// "secret" "***hidden***"
```

---

## Symbols vs Strings as Keys

| Feature | String Keys | Symbol Keys |
|---------|-------------|-------------|
| **Uniqueness** | Can collide | Always unique |
| **Visibility** | Object.keys(), JSON | Hidden from these |
| **Access** | Dot notation OK | Bracket notation only |
| **Global** | N/A | Symbol.for() creates global |
| **Use case** | Public API | Internal/private properties |

---

## Interview Questions

### Q1: What's the output?

```javascript
const sym1 = Symbol('test');
const sym2 = Symbol('test');

console.log(sym1 === sym2);
console.log(sym1 == sym2);
```

**Answer:**
```javascript
false // Symbols are always unique
false // Even with ==
```

### Q2: How do you get all property keys including symbols?

```javascript
const obj = {
  name: 'Alice',
  [Symbol('id')]: 123,
  [Symbol('role')]: 'admin'
};

// Get string keys
console.log(Object.keys(obj)); // ["name"]

// Get symbol keys
console.log(Object.getOwnPropertySymbols(obj)); // [Symbol(id), Symbol(role)]

// Get all keys
console.log(Reflect.ownKeys(obj)); // ["name", Symbol(id), Symbol(role)]
```

### Q3: Global vs Local Symbols

```javascript
// Local symbols (always unique)
const sym1 = Symbol('test');
const sym2 = Symbol('test');
console.log(sym1 === sym2); // false

// Global symbols (shared)
const sym3 = Symbol.for('test');
const sym4 = Symbol.for('test');
console.log(sym3 === sym4); // true
```

---

## Summary

### Key Concepts

1. **Symbols are unique primitives** - Each Symbol() is unique
2. **Symbol.for()** creates/retrieves global symbols
3. **Hidden from enumeration** - Don't show in Object.keys(), JSON
4. **Well-known symbols** customize object behavior
5. **Use bracket notation** to access symbol properties

### Common Use Cases

- ✅ Unique property keys
- ✅ Preventing property name collisions
- ✅ Private-like properties
- ✅ Custom object behavior (iterators, etc.)
- ✅ Metadata that shouldn't be serialized

### Well-Known Symbols

- `Symbol.iterator` - Make iterable
- `Symbol.toStringTag` - Custom type string
- `Symbol.toPrimitive` - Control type conversion
- `Symbol.hasInstance` - Custom instanceof
- And many more...

### Interview Tips

- Symbols are primitives (7th type: string, number, boolean, null, undefined, bigint, symbol)
- Each Symbol() call creates a unique symbol
- Symbol.for() creates/retrieves from global registry
- Symbols are hidden from Object.keys() and JSON.stringify()
- Used for "private" properties and custom behavior

Master symbols to write more robust and collision-free JavaScript! 🚀
