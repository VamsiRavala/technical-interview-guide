# Prototypes and Prototypal Inheritance in JavaScript

## What are Prototypes?

In JavaScript, **prototypes** are the mechanism by which objects inherit properties and methods from other objects. Every JavaScript object has a hidden internal property called `[[Prototype]]` (accessible via `__proto__` or `Object.getPrototypeOf()`).

---

## The Prototype Chain

When you try to access a property on an object:
1. JavaScript first looks for the property on the object itself
2. If not found, it looks in the object's prototype
3. If still not found, it looks in the prototype's prototype
4. This continues until it reaches `null` (end of prototype chain)

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

---

## Understanding `__proto__` vs `prototype`

### `__proto__` (Object Property)

- Exists on **every object**
- Points to the object's prototype
- Used for lookup when accessing properties

```javascript
const obj = {};
console.log(obj.__proto__ === Object.prototype); // true
```

### `prototype` (Function Property)

- Exists on **functions only** (specifically constructor functions)
- Used when creating objects with `new`
- Defines what `__proto__` will be for created instances

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

---

## Constructor Functions and Prototypes

### Basic Constructor Function

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

### Why Use Prototype for Methods?

```javascript
// ❌ Bad: Method defined in constructor (duplicated for each instance)
function Person(name) {
  this.name = name;
  this.greet = function() {
    console.log(`Hello, I'm ${this.name}`);
  };
}

const p1 = new Person('Alice');
const p2 = new Person('Bob');
console.log(p1.greet === p2.greet); // false (different functions)

// ✅ Good: Method on prototype (shared by all instances)
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

**Benefits:**
- **Memory efficiency:** One method in memory, not duplicated per instance
- **Dynamic updates:** Change prototype method affects all instances

---

## Object.create()

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

### Object.create() with Property Descriptors

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

---

## Prototypal Inheritance Patterns

### Pattern 1: Constructor Function Inheritance

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

### Pattern 2: Object.create() Inheritance

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

### Pattern 3: ES6 Classes (Syntactic Sugar)

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

---

## Prototype Methods and Properties

### Checking Prototypes

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

### instanceof Operator

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

### Object.hasOwnProperty()

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

---

## Prototype Chain Examples

### Example 1: Complete Chain

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

### Example 2: Multi-Level Inheritance

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

---

## Common Patterns and Best Practices

### 1. Method Sharing via Prototype

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

### 2. Modifying Built-in Prototypes (Anti-pattern)

```javascript
// ❌ Don't do this (can break existing code)
Array.prototype.shuffle = function() {
  // shuffle implementation
};

// ✅ Better: Use utility functions
function shuffleArray(arr) {
  // shuffle implementation
}
```

### 3. Creating Objects Without Prototype

```javascript
// Object with no prototype
const obj = Object.create(null);

console.log(obj.toString); // undefined
console.log(obj.__proto__); // undefined

// Useful for dictionaries/hash maps
obj.key = 'value';
console.log(obj.key); // 'value'
```

---

## Interview Questions

### Q1: What's the output?

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

### Q2: Fix the inheritance

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

Dog.prototype = Animal.prototype; // ❌ Wrong!

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

// ✅ Correct: Create new object with Animal.prototype as prototype
Dog.prototype = Object.create(Animal.prototype);
Dog.prototype.constructor = Dog;

Dog.prototype.bark = function() {
  console.log('Woof!');
};
```

### Q3: Implement instanceof

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

---

## Summary

### Key Concepts

1. **Prototypes** enable object inheritance in JavaScript
2. **Prototype chain** allows objects to inherit properties and methods
3. **`__proto__`** points to an object's prototype (for lookup)
4. **`prototype`** is a property of constructor functions (defines inheritance)
5. **Object.create()** creates objects with specific prototypes

### Best Practices

- ✅ Use prototypes for shared methods (memory efficiency)
- ✅ Use `Object.create()` for clean inheritance
- ✅ Use ES6 classes for syntactic clarity
- ❌ Avoid modifying built-in prototypes
- ❌ Avoid `__proto__` (use `Object.getPrototypeOf()`)

### Prototype Chain

```
instance -> Constructor.prototype -> Parent.prototype -> Object.prototype -> null
```

### Interview Tips

- Understand the difference between `__proto__` and `prototype`
- Know how to set up inheritance with constructor functions
- Be able to explain the prototype chain
- Understand `hasOwnProperty` vs inherited properties
- Know when to use prototypes vs closures for methods

Master prototypes to understand JavaScript's inheritance model! 🚀
