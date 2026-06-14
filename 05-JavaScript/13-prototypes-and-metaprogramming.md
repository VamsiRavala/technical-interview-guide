# Prototypes and Metaprogramming

## Prototypes & Prototypal Inheritance

In JavaScript, **prototypes** are the mechanism by which objects inherit properties and methods from other objects. Every JavaScript object has a hidden internal property called `[[Prototype]]` (accessible via `__proto__` or `Object.getPrototypeOf()`).

### The Prototype Chain

When you try to access a property on an object:
1. JavaScript first looks for the property on the object itself.
2. If not found, it looks in the object's prototype.
3. If still not found, it looks in the prototype's prototype.
4. This continues until it reaches `null` (end of prototype chain).

```javascript
const animal = {
  eats: true,
  walk() {
    console.log('Animal walks');
  }
};

const dog = {
  barks: true
};

// Set animal as prototype of dog
dog.__proto__ = animal;

console.log(dog.eats);    // true (inherited from animal)
console.log(dog.barks);   // true (own property)
dog.walk();               // "Animal walks" (inherited method)

console.log(dog.hasOwnProperty('barks')); // true
console.log(dog.hasOwnProperty('eats'));  // false (inherited)
```

### Understanding `__proto__` vs `prototype`

`__proto__` (object property):
- Exists on **every object**.
- Points to the object's prototype.
- Used for lookup when accessing properties.

```javascript
const obj = {};
console.log(obj.__proto__ === Object.prototype); // true
```

`prototype` (function property):
- Exists on **functions only** (specifically constructor functions).
- Used when creating objects with `new`.
- Defines what `__proto__` will be for created instances.

```javascript
function Person(name) {
  this.name = name;
}

Person.prototype.greet = function() {
  console.log(`Hello, I'm ${this.name}`);
};

const alice = new Person('Alice');
console.log(alice.__proto__ === Person.prototype); // true
alice.greet(); // "Hello, I'm Alice"
```

### Constructor Functions and Prototypes

Basic constructor function:

```javascript
function Car(make, model) {
  this.make = make;
  this.model = model;
}

// Add method to prototype (shared by all instances)
Car.prototype.getInfo = function() {
  return `${this.make} ${this.model}`;
};

const car1 = new Car('Toyota', 'Camry');
const car2 = new Car('Honda', 'Civic');

console.log(car1.getInfo()); // "Toyota Camry"
console.log(car2.getInfo()); // "Honda Civic"

// Both instances share the same method
console.log(car1.getInfo === car2.getInfo); // true
```

Why use prototype for methods?

```javascript
// Bad: Method defined in constructor (duplicated for each instance)
function Person(name) {
  this.name = name;
  this.greet = function() {
    console.log(`Hello, I'm ${this.name}`);
  };
}

const p1 = new Person('Alice');
const p2 = new Person('Bob');
console.log(p1.greet === p2.greet); // false (different functions)

// Good: Method on prototype (shared by all instances)
function Person(name) {
  this.name = name;
}

Person.prototype.greet = function() {
  console.log(`Hello, I'm ${this.name}`);
};

const p3 = new Person('Charlie');
const p4 = new Person('David');
console.log(p3.greet === p4.greet); // true (same function)
```

Benefits:
- **Memory efficiency:** One method in memory, not duplicated per instance.
- **Dynamic updates:** Changing a prototype method affects all instances.

### Object.create()

Create an object with a specific prototype without using constructor functions.

```javascript
const animal = {
  type: 'Animal',
  speak() {
    console.log(`${this.name} makes a sound`);
  }
};

// Create object with animal as prototype
const dog = Object.create(animal);
dog.name = 'Buddy';
dog.breed = 'Golden Retriever';

console.log(dog.type);  // "Animal" (inherited)
dog.speak();            // "Buddy makes a sound"

console.log(dog.__proto__ === animal); // true
```

Object.create() with property descriptors:

```javascript
const person = {
  greet() {
    console.log(`Hello, I'm ${this.name}`);
  }
};

const john = Object.create(person, {
  name: {
    value: 'John',
    writable: true,
    enumerable: true,
    configurable: true
  },
  age: {
    value: 30,
    writable: false
  }
});

john.greet(); // "Hello, I'm John"
console.log(john.name); // "John"
console.log(john.age);  // 30

john.age = 40; // No effect (writable: false)
console.log(john.age); // 30
```

### Prototypal Inheritance Patterns

#### Pattern 1: Constructor Function Inheritance

```javascript
// Parent constructor
function Animal(name) {
  this.name = name;
}

Animal.prototype.eat = function() {
  console.log(`${this.name} is eating`);
};

// Child constructor
function Dog(name, breed) {
  Animal.call(this, name); // Call parent constructor
  this.breed = breed;
}

// Set up inheritance
Dog.prototype = Object.create(Animal.prototype);
Dog.prototype.constructor = Dog;

// Add child-specific methods
Dog.prototype.bark = function() {
  console.log(`${this.name} barks!`);
};

const dog = new Dog('Max', 'Labrador');
dog.eat();  // "Max is eating" (inherited)
dog.bark(); // "Max barks!" (own method)

console.log(dog instanceof Dog);    // true
console.log(dog instanceof Animal); // true
```

#### Pattern 2: Object.create() Inheritance

```javascript
const animal = {
  init(name) {
    this.name = name;
    return this;
  },
  eat() {
    console.log(`${this.name} is eating`);
  }
};

const dog = Object.create(animal);
dog.bark = function() {
  console.log(`${this.name} barks!`);
};

const myDog = Object.create(dog).init('Buddy');
myDog.eat();  // "Buddy is eating"
myDog.bark(); // "Buddy barks!"
```

#### Pattern 3: ES6 Classes (Syntactic Sugar)

```javascript
class Animal {
  constructor(name) {
    this.name = name;
  }

  eat() {
    console.log(`${this.name} is eating`);
  }
}

class Dog extends Animal {
  constructor(name, breed) {
    super(name); // Call parent constructor
    this.breed = breed;
  }

  bark() {
    console.log(`${this.name} barks!`);
  }
}

const dog = new Dog('Rocky', 'Bulldog');
dog.eat();  // "Rocky is eating"
dog.bark(); // "Rocky barks!"

// Under the hood, still using prototypes
console.log(dog.__proto__ === Dog.prototype); // true
console.log(Dog.prototype.__proto__ === Animal.prototype); // true
```

### Prototype Methods and Properties

Checking prototypes:

```javascript
const obj = {};

// Check prototype
console.log(Object.getPrototypeOf(obj) === Object.prototype); // true

// Set prototype
const proto = { x: 10 };
Object.setPrototypeOf(obj, proto);
console.log(obj.x); // 10

// Check if object has own property (not inherited)
console.log(obj.hasOwnProperty('x')); // false
console.log(obj.hasOwnProperty('toString')); // false
```

`instanceof` operator:

```javascript
function Car() {}
const car = new Car();

console.log(car instanceof Car);    // true
console.log(car instanceof Object); // true
console.log(car instanceof Array);  // false

// instanceof checks prototype chain
console.log(Car.prototype.isPrototypeOf(car)); // true
console.log(Object.prototype.isPrototypeOf(car)); // true
```

`Object.hasOwnProperty()`:

```javascript
const parent = { a: 1 };
const child = Object.create(parent);
child.b = 2;

console.log(child.a); // 1 (inherited)
console.log(child.b); // 2 (own property)

console.log(child.hasOwnProperty('a')); // false
console.log(child.hasOwnProperty('b')); // true

// Safe way to use hasOwnProperty
console.log(Object.prototype.hasOwnProperty.call(child, 'a')); // false
```

### Prototype Chain Examples

#### Example 1: Complete Chain

```javascript
function Animal(name) {
  this.name = name;
}

Animal.prototype.eat = function() {
  console.log('eating');
};

function Dog(name, breed) {
  Animal.call(this, name);
  this.breed = breed;
}

Dog.prototype = Object.create(Animal.prototype);
Dog.prototype.constructor = Dog;

Dog.prototype.bark = function() {
  console.log('barking');
};

const dog = new Dog('Max', 'Labrador');

// Prototype chain: dog -> Dog.prototype -> Animal.prototype -> Object.prototype -> null

console.log(dog.name);                    // 'Max' (own property)
console.log(dog.bark);                    // function (Dog.prototype)
console.log(dog.eat);                     // function (Animal.prototype)
console.log(dog.toString);                // function (Object.prototype)
console.log(Object.getPrototypeOf(dog));  // Dog.prototype
```

#### Example 2: Multi-Level Inheritance

```javascript
// Level 1
function LivingBeing() {
  this.alive = true;
}

LivingBeing.prototype.breathe = function() {
  console.log('Breathing');
};

// Level 2
function Animal(name) {
  LivingBeing.call(this);
  this.name = name;
}

Animal.prototype = Object.create(LivingBeing.prototype);
Animal.prototype.constructor = Animal;

Animal.prototype.move = function() {
  console.log('Moving');
};

// Level 3
function Dog(name, breed) {
  Animal.call(this, name);
  this.breed = breed;
}

Dog.prototype = Object.create(Animal.prototype);
Dog.prototype.constructor = Dog;

Dog.prototype.bark = function() {
  console.log('Barking');
};

const dog = new Dog('Buddy', 'Beagle');

// Can access properties/methods from all levels
console.log(dog.alive);  // true (from LivingBeing)
dog.breathe();           // "Breathing" (from LivingBeing)
dog.move();              // "Moving" (from Animal)
dog.bark();              // "Barking" (from Dog)
```

### Common Patterns and Best Practices

#### 1. Method Sharing via Prototype

```javascript
function User(name, email) {
  this.name = name;
  this.email = email;
}

// Shared methods on prototype
User.prototype.getInfo = function() {
  return `${this.name} (${this.email})`;
};

User.prototype.updateEmail = function(newEmail) {
  this.email = newEmail;
};

const users = [
  new User('Alice', 'alice@example.com'),
  new User('Bob', 'bob@example.com')
];

// All users share the same methods
console.log(users[0].getInfo === users[1].getInfo); // true
```

#### 2. Modifying Built-in Prototypes (Anti-pattern)

```javascript
// Don't do this (can break existing code)
Array.prototype.shuffle = function() {
  // shuffle implementation
};

// Better: Use utility functions
function shuffleArray(arr) {
  // shuffle implementation
}
```

#### 3. Creating Objects Without Prototype

```javascript
// Object with no prototype
const obj = Object.create(null);

console.log(obj.toString); // undefined
console.log(obj.__proto__); // undefined

// Useful for dictionaries/hash maps
obj.key = 'value';
console.log(obj.key); // 'value'
```

### Prototype Interview Questions

#### Q1: What's the output?

```javascript
function Person(name) {
  this.name = name;
}

Person.prototype.age = 30;

const person1 = new Person('Alice');
const person2 = new Person('Bob');

person1.age = 25;

console.log(person1.age); // ?
console.log(person2.age); // ?

delete person1.age;

console.log(person1.age); // ?
```

**Answer:**

```javascript
console.log(person1.age); // 25 (own property)
console.log(person2.age); // 30 (from prototype)

delete person1.age;

console.log(person1.age); // 30 (now reads from prototype)
```

#### Q2: Fix the inheritance

```javascript
function Animal(name) {
  this.name = name;
}

Animal.prototype.speak = function() {
  console.log(`${this.name} makes a sound`);
};

function Dog(name, breed) {
  this.name = name;
  this.breed = breed;
}

Dog.prototype = Animal.prototype; // Wrong!

Dog.prototype.bark = function() {
  console.log('Woof!');
};

const animal = new Animal('Generic');
animal.bark(); // Should not work!
```

**Fixed:**

```javascript
function Dog(name, breed) {
  Animal.call(this, name);
  this.breed = breed;
}

// Correct: Create new object with Animal.prototype as prototype
Dog.prototype = Object.create(Animal.prototype);
Dog.prototype.constructor = Dog;

Dog.prototype.bark = function() {
  console.log('Woof!');
};
```

#### Q3: Implement instanceof

```javascript
function myInstanceOf(obj, constructor) {
  let proto = Object.getPrototypeOf(obj);

  while (proto !== null) {
    if (proto === constructor.prototype) {
      return true;
    }
    proto = Object.getPrototypeOf(proto);
  }

  return false;
}

// Test
function Car() {}
const car = new Car();

console.log(myInstanceOf(car, Car));    // true
console.log(myInstanceOf(car, Object)); // true
console.log(myInstanceOf(car, Array));  // false
```

### Prototypes Summary

Key concepts:
1. **Prototypes** enable object inheritance in JavaScript.
2. **Prototype chain** allows objects to inherit properties and methods.
3. **`__proto__`** points to an object's prototype (for lookup).
4. **`prototype`** is a property of constructor functions (defines inheritance).
5. **Object.create()** creates objects with specific prototypes.

Best practices:
- Use prototypes for shared methods (memory efficiency).
- Use `Object.create()` for clean inheritance.
- Use ES6 classes for syntactic clarity.
- Avoid modifying built-in prototypes.
- Avoid `__proto__` (use `Object.getPrototypeOf()`).

Prototype chain:

```text
instance -> Constructor.prototype -> Parent.prototype -> Object.prototype -> null
```

---

## Prototype Design Pattern

### Conceptual Difference: Class vs Prototype Pattern

**Class (OOP style):**
- A **blueprint** for creating objects.
- Properties and methods are defined inside the class.
- Instances are created with `new`, which links them to the class's prototype.

**Prototype Pattern:**
- Uses an **existing object** as a base (prototype).
- New objects are created by **cloning** this prototype.
- No rigid blueprint needed, more flexible.

### Example in JavaScript

Using a class:

```javascript
class Car {
  constructor(make, model) {
    this.make = make;
    this.model = model;
  }
  drive() {
    console.log(`Driving a ${this.make} ${this.model}`);
  }
}

const car1 = new Car("Tesla", "Model 3");
const car2 = new Car("BMW", "X5");
car1.drive(); // Driving a Tesla Model 3
```

Using the prototype pattern:

```javascript
const carPrototype = {
  drive() {
    console.log(`Driving a ${this.make} ${this.model}`);
  }
};

const car1 = Object.create(carPrototype);
car1.make = "Tesla";
car1.model = "Model 3";

const car2 = Object.create(carPrototype);
car2.make = "BMW";
car2.model = "X5";

car1.drive(); // Driving a Tesla Model 3
```

### Key Differences

| Aspect | Class | Prototype Pattern |
|--------|-------|-------------------|
| **Definition** | Defined using `class` keyword (blueprint) | Starts with an existing object |
| **Creation** | `new ClassName()` | `Object.create(prototype)` |
| **Inheritance** | `extends` keyword | Prototype chaining |
| **Flexibility** | Structured, OOP-like | Dynamic, flexible |
| **Readability** | Familiar to OOP devs | Native JavaScript style |
| **Best Use Case** | Entities, models, clear structure | Lightweight cloning, config objects |

### Real-Life Analogy

- **Class** is like a **blueprint of a house**: define layout, rooms, and design, then build many houses from the same plan.
- **Prototype Pattern** is like **copying an existing house**: make slight modifications (paint, furniture, extensions).

Both give you a house, but the **approach differs**.

### Summary

- Use **Classes** for structured, blueprint-driven designs (great for teams and OOP developers).
- Use the **Prototype Pattern** when you want flexibility and lightweight cloning (great for dynamic or config-driven systems).

---

## Symbols

**Symbol** is a primitive data type introduced in ES6. Each Symbol value is **unique** and **immutable**, making it perfect for creating unique property keys that won't conflict with other properties.

```javascript
const sym1 = Symbol();
const sym2 = Symbol();

console.log(sym1 === sym2); // false (each Symbol is unique)
console.log(typeof sym1);   // "symbol"
```

### Creating Symbols

Basic symbol creation:

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

`Symbol.for()` - global symbol registry. Creates or retrieves a symbol from a global registry.

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

### Using Symbols as Property Keys

Unique property keys:

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

Preventing property name collisions:

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

### Well-Known Symbols

JavaScript has built-in symbols that customize object behavior.

`Symbol.iterator` - make objects iterable:

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

`Symbol.toStringTag` - customize object type string:

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

`Symbol.hasInstance` - customize `instanceof` behavior:

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

`Symbol.toPrimitive` - control type conversion:

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

`Symbol.isConcatSpreadable` - control array spreading in concat:

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

### Common Well-Known Symbols

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

### Use Cases

#### 1. Private-Like Properties

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

#### 2. Enum-Like Values

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

#### 3. Metadata Keys

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

#### 4. Observable Pattern

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

### Symbol Properties in Classes

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

### Symbols vs Strings as Keys

| Feature | String Keys | Symbol Keys |
|---------|-------------|-------------|
| **Uniqueness** | Can collide | Always unique |
| **Visibility** | Object.keys(), JSON | Hidden from these |
| **Access** | Dot notation OK | Bracket notation only |
| **Global** | N/A | Symbol.for() creates global |
| **Use case** | Public API | Internal/private properties |

### Symbol Interview Questions

#### Q1: What's the output?

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

#### Q2: How do you get all property keys including symbols?

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

#### Q3: Global vs Local Symbols

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

### Symbols Summary

Key concepts:
1. **Symbols are unique primitives** - Each `Symbol()` is unique.
2. **Symbol.for()** creates/retrieves global symbols.
3. **Hidden from enumeration** - Don't show in `Object.keys()`, JSON.
4. **Well-known symbols** customize object behavior.
5. **Use bracket notation** to access symbol properties.

Symbols are the 7th primitive type (string, number, boolean, null, undefined, bigint, symbol). Common use cases: unique property keys, preventing collisions, private-like properties, custom object behavior (iterators), and metadata that shouldn't be serialized.

---

## Proxy & Reflect

A **Proxy** is an object that wraps another object and intercepts operations performed on it (reading, writing, function calls, etc.). It allows you to customize fundamental operations on objects.

```javascript
const target = {
  message: 'Hello'
};

const handler = {
  get(target, property) {
    console.log(`Getting ${property}`);
    return target[property];
  }
};

const proxy = new Proxy(target, handler);

console.log(proxy.message);
// Logs: "Getting message"
// Returns: "Hello"
```

### Proxy Syntax

```javascript
const proxy = new Proxy(target, handler);
```

- **target**: The original object to wrap.
- **handler**: An object with "traps" (methods that intercept operations).

### Common Proxy Traps

#### 1. `get` Trap - Intercept Property Access

```javascript
const user = {
  name: 'Alice',
  age: 30
};

const handler = {
  get(target, property) {
    if (property in target) {
      return target[property];
    }
    return `Property "${property}" doesn't exist`;
  }
};

const proxy = new Proxy(user, handler);

console.log(proxy.name);    // "Alice"
console.log(proxy.age);     // 30
console.log(proxy.email);   // "Property "email" doesn't exist"
```

#### 2. `set` Trap - Intercept Property Assignment

```javascript
const user = {
  name: 'Bob',
  age: 25
};

const handler = {
  set(target, property, value) {
    if (property === 'age') {
      if (typeof value !== 'number' || value < 0) {
        throw new TypeError('Age must be a positive number');
      }
    }
    target[property] = value;
    return true; // Indicates success
  }
};

const proxy = new Proxy(user, handler);

proxy.age = 30;          // OK
console.log(proxy.age);  // 30

proxy.age = -5;          // TypeError: Age must be a positive number
proxy.age = 'thirty';    // TypeError: Age must be a positive number
```

#### 3. `has` Trap - Intercept `in` Operator

```javascript
const secrets = {
  password: 'secret123',
  apiKey: 'abc-def-ghi'
};

const handler = {
  has(target, property) {
    if (property.startsWith('_')) {
      return false; // Hide private properties
    }
    return property in target;
  }
};

const proxy = new Proxy(secrets, handler);

console.log('password' in proxy);  // true
console.log('_hidden' in proxy);   // false (even if it exists)
```

#### 4. `deleteProperty` Trap - Intercept `delete`

```javascript
const user = {
  name: 'Charlie',
  age: 28,
  _id: 123
};

const handler = {
  deleteProperty(target, property) {
    if (property.startsWith('_')) {
      throw new Error('Cannot delete private properties');
    }
    delete target[property];
    return true;
  }
};

const proxy = new Proxy(user, handler);

delete proxy.age;    // OK
delete proxy._id;    // Error: Cannot delete private properties
```

#### 5. `apply` Trap - Intercept Function Calls

```javascript
function sum(a, b) {
  return a + b;
}

const handler = {
  apply(target, thisArg, args) {
    console.log(`Calling function with args: ${args}`);
    return target.apply(thisArg, args);
  }
};

const proxy = new Proxy(sum, handler);

console.log(proxy(2, 3));
// Logs: "Calling function with args: 2,3"
// Returns: 5
```

#### 6. `construct` Trap - Intercept `new` Operator

```javascript
class User {
  constructor(name) {
    this.name = name;
  }
}

const handler = {
  construct(target, args) {
    console.log(`Creating new instance with: ${args}`);
    return new target(...args);
  }
};

const ProxyUser = new Proxy(User, handler);

const user = new ProxyUser('David');
// Logs: "Creating new instance with: David"
console.log(user.name); // "David"
```

### All Available Traps

| Trap | Intercepts |
|------|------------|
| `get(target, prop, receiver)` | Reading property |
| `set(target, prop, value, receiver)` | Writing property |
| `has(target, prop)` | `in` operator |
| `deleteProperty(target, prop)` | `delete` operator |
| `apply(target, thisArg, args)` | Function call |
| `construct(target, args, newTarget)` | `new` operator |
| `getPrototypeOf(target)` | `Object.getPrototypeOf()` |
| `setPrototypeOf(target, proto)` | `Object.setPrototypeOf()` |
| `isExtensible(target)` | `Object.isExtensible()` |
| `preventExtensions(target)` | `Object.preventExtensions()` |
| `getOwnPropertyDescriptor(target, prop)` | `Object.getOwnPropertyDescriptor()` |
| `defineProperty(target, prop, descriptor)` | `Object.defineProperty()` |
| `ownKeys(target)` | `Object.keys()`, `for...in` |

### What is Reflect?

**Reflect** is a built-in object that provides methods for interceptable JavaScript operations. It mirrors the Proxy traps and provides default behavior.

Why use Reflect?
1. **Default behavior**: Call the default operation inside traps.
2. **Better error handling**: Returns boolean instead of throwing.
3. **Cleaner code**: More functional approach.

### Reflect Methods

Basic examples:

```javascript
const obj = {
  name: 'Eve',
  age: 32
};

// Reflect.get (like obj[prop])
console.log(Reflect.get(obj, 'name')); // "Eve"

// Reflect.set (like obj[prop] = value)
Reflect.set(obj, 'age', 33);
console.log(obj.age); // 33

// Reflect.has (like prop in obj)
console.log(Reflect.has(obj, 'name')); // true
console.log(Reflect.has(obj, 'email')); // false

// Reflect.deleteProperty (like delete obj[prop])
Reflect.deleteProperty(obj, 'age');
console.log(obj.age); // undefined

// Reflect.ownKeys (like Object.keys + symbols)
const sym = Symbol('id');
obj[sym] = 123;
console.log(Reflect.ownKeys(obj)); // ['name', Symbol(id)]
```

Reflect vs regular operations:

```javascript
const obj = {};

// Regular way
obj.prop = 'value';
delete obj.prop;

// Reflect way (returns boolean)
const setSuccess = Reflect.set(obj, 'prop', 'value');
const deleteSuccess = Reflect.deleteProperty(obj, 'prop');

console.log(setSuccess);    // true
console.log(deleteSuccess); // true
```

### Proxy + Reflect: Best Practice

Use Reflect inside proxy traps to call default behavior.

```javascript
const user = {
  name: 'Frank',
  age: 40
};

const handler = {
  get(target, property, receiver) {
    console.log(`Getting: ${property}`);
    return Reflect.get(target, property, receiver); // Default behavior
  },

  set(target, property, value, receiver) {
    console.log(`Setting: ${property} = ${value}`);
    return Reflect.set(target, property, value, receiver); // Default behavior
  }
};

const proxy = new Proxy(user, handler);

proxy.name = 'Grace';
// Logs: "Setting: name = Grace"

console.log(proxy.name);
// Logs: "Getting: name"
// Returns: "Grace"
```

### Real-World Use Cases

#### 1. Validation

```javascript
function createValidator(target, validations) {
  return new Proxy(target, {
    set(target, property, value) {
      const validation = validations[property];

      if (validation && !validation(value)) {
        throw new Error(`Invalid value for ${property}`);
      }

      return Reflect.set(target, property, value);
    }
  });
}

const user = createValidator({}, {
  age: value => typeof value === 'number' && value >= 0,
  email: value => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
  name: value => typeof value === 'string' && value.length > 0
});

user.name = 'Alice';    // OK
user.age = 25;          // OK
user.email = 'alice@example.com'; // OK

user.age = -5;          // Error: Invalid value for age
user.email = 'invalid'; // Error: Invalid value for email
```

#### 2. Logging/Debugging

```javascript
function createLoggingProxy(target, name) {
  return new Proxy(target, {
    get(target, property) {
      const value = Reflect.get(target, property);
      console.log(`[${name}] GET ${property} = ${value}`);
      return value;
    },

    set(target, property, value) {
      console.log(`[${name}] SET ${property} = ${value}`);
      return Reflect.set(target, property, value);
    }
  });
}

const user = createLoggingProxy({ name: 'Bob' }, 'User');

user.name;        // [User] GET name = Bob
user.age = 30;    // [User] SET age = 30
```

#### 3. Negative Array Indices (Python-like)

```javascript
function createNegativeIndexArray(arr) {
  return new Proxy(arr, {
    get(target, property) {
      let index = parseInt(property);

      if (!isNaN(index) && index < 0) {
        index = target.length + index;
      }

      return Reflect.get(target, index);
    }
  });
}

const arr = createNegativeIndexArray([1, 2, 3, 4, 5]);

console.log(arr[-1]);  // 5 (last element)
console.log(arr[-2]);  // 4 (second to last)
console.log(arr[0]);   // 1 (normal index)
```

#### 4. Observable Objects (Reactive Data)

```javascript
function createObservable(target, onChange) {
  return new Proxy(target, {
    set(target, property, value) {
      const oldValue = target[property];
      const result = Reflect.set(target, property, value);

      if (oldValue !== value) {
        onChange(property, oldValue, value);
      }

      return result;
    }
  });
}

const user = createObservable({ name: 'Alice', age: 25 }, (prop, oldVal, newVal) => {
  console.log(`${prop} changed from ${oldVal} to ${newVal}`);
});

user.age = 26;  // "age changed from 25 to 26"
user.name = 'Alice'; // No log (value unchanged)
```

#### 5. Private Properties

```javascript
function createPrivateProxy(target) {
  return new Proxy(target, {
    get(target, property) {
      if (property.startsWith('_')) {
        throw new Error(`Cannot access private property: ${property}`);
      }
      return Reflect.get(target, property);
    },

    set(target, property, value) {
      if (property.startsWith('_')) {
        throw new Error(`Cannot modify private property: ${property}`);
      }
      return Reflect.set(target, property, value);
    },

    has(target, property) {
      if (property.startsWith('_')) {
        return false;
      }
      return Reflect.has(target, property);
    },

    ownKeys(target) {
      return Reflect.ownKeys(target).filter(key => !key.startsWith('_'));
    }
  });
}

const obj = createPrivateProxy({
  public: 'visible',
  _private: 'hidden'
});

console.log(obj.public);    // "visible"
console.log(obj._private);  // Error: Cannot access private property
console.log(Object.keys(obj)); // ["public"]
```

#### 6. Default Values

```javascript
function createDefaultProxy(defaults) {
  return new Proxy({}, {
    get(target, property) {
      if (property in target) {
        return target[property];
      }
      return defaults[property] || undefined;
    }
  });
}

const config = createDefaultProxy({
  host: 'localhost',
  port: 3000,
  debug: false
});

console.log(config.host);   // "localhost" (default)
console.log(config.debug);  // false (default)

config.host = '0.0.0.0';
console.log(config.host);   // "0.0.0.0" (custom)
```

#### 7. Method Call Counter

```javascript
function createCounterProxy(target) {
  const counts = {};

  return new Proxy(target, {
    get(target, property) {
      if (typeof target[property] === 'function') {
        return new Proxy(target[property], {
          apply(fn, thisArg, args) {
            counts[property] = (counts[property] || 0) + 1;
            console.log(`${property} called ${counts[property]} times`);
            return Reflect.apply(fn, thisArg, args);
          }
        });
      }
      return Reflect.get(target, property);
    }
  });
}

const calculator = createCounterProxy({
  add(a, b) { return a + b; },
  multiply(a, b) { return a * b; }
});

calculator.add(2, 3);      // "add called 1 times"
calculator.add(5, 7);      // "add called 2 times"
calculator.multiply(2, 4); // "multiply called 1 times"
```

### Performance Considerations

Proxies have performance overhead:
- Slower than direct property access.
- Not optimized by JavaScript engines.
- Best for development tools, not hot paths.

```javascript
// Don't use proxies in performance-critical loops
for (let i = 0; i < 1000000; i++) {
  proxy.value = i; // Slow
}

// Better: Direct access
for (let i = 0; i < 1000000; i++) {
  obj.value = i; // Fast
}
```

### Proxy & Reflect Interview Questions

#### Q1: Implement a read-only proxy

```javascript
function createReadOnly(target) {
  return new Proxy(target, {
    set() {
      throw new Error('Object is read-only');
    },
    deleteProperty() {
      throw new Error('Object is read-only');
    }
  });
}

const user = createReadOnly({ name: 'Alice' });
user.name = 'Bob'; // Error: Object is read-only
```

#### Q2: Implement a range validator

```javascript
function createRangeProxy(min, max) {
  return new Proxy({}, {
    set(target, property, value) {
      if (value < min || value > max) {
        throw new RangeError(`Value must be between ${min} and ${max}`);
      }
      return Reflect.set(target, property, value);
    }
  });
}

const temperature = createRangeProxy(-50, 50);
temperature.current = 25; // OK
temperature.current = 100; // RangeError
```

#### Q3: What's the output?

```javascript
const target = { value: 10 };
const handler = {
  get(target, prop) {
    return target[prop] * 2;
  }
};

const proxy = new Proxy(target, handler);

console.log(proxy.value);
target.value = 20;
console.log(proxy.value);
```

**Answer:**

```text
20  // 10 * 2
40  // 20 * 2
```

### Proxy & Reflect Summary

Proxy:
- **Wraps objects** to intercept operations.
- **Traps** define custom behavior.
- **Use cases**: validation, logging, reactivity, access control.
- **Performance**: Slower than direct access.

Reflect:
- **Mirrors Proxy traps** with default operations.
- **Better error handling** (returns boolean).
- **Use with Proxy** for cleaner code.
- **Functional approach** to object operations.

Best practices:
- Use Reflect inside proxy traps.
- Return proper values from traps (boolean for set/deleteProperty).
- Handle edge cases (symbols, non-existent properties).
- Avoid in performance-critical code.
- Don't create deep proxy trees (performance).

Common patterns:
1. **Validation** - Enforce rules on property assignment.
2. **Logging** - Track property access/modification.
3. **Reactivity** - Notify on changes (Vue.js 3 uses Proxy).
4. **Access control** - Hide/protect properties.
5. **Default values** - Provide fallbacks.
