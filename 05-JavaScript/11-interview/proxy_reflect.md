# Proxy and Reflect in JavaScript

## What is a Proxy?

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

---

## Proxy Syntax

```javascript
const proxy = new Proxy(target, handler);
```

- **target**: The original object to wrap
- **handler**: An object with "traps" (methods that intercept operations)

---

## Common Proxy Traps

### 1. `get` Trap - Intercept Property Access

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

### 2. `set` Trap - Intercept Property Assignment

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

### 3. `has` Trap - Intercept `in` Operator

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

### 4. `deleteProperty` Trap - Intercept `delete`

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

### 5. `apply` Trap - Intercept Function Calls

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

### 6. `construct` Trap - Intercept `new` Operator

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

---

## All Available Traps

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

---

## What is Reflect?

**Reflect** is a built-in object that provides methods for interceptable JavaScript operations. It mirrors the Proxy traps and provides default behavior.

### Why Use Reflect?

1. **Default behavior**: Call the default operation inside traps
2. **Better error handling**: Returns boolean instead of throwing
3. **Cleaner code**: More functional approach

---

## Reflect Methods

### Basic Examples

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

### Reflect vs Regular Operations

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

---

## Proxy + Reflect: Best Practice

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

---

## Real-World Use Cases

### 1. Validation

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

### 2. Logging/Debugging

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

### 3. Negative Array Indices (Python-like)

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

### 4. Observable Objects (Reactive Data)

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

### 5. Private Properties

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

### 6. Default Values

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

### 7. Method Call Counter

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

---

## Performance Considerations

⚠️ **Proxies have performance overhead:**
- Slower than direct property access
- Not optimized by JavaScript engines
- Best for development tools, not hot paths

```javascript
// ❌ Don't use proxies in performance-critical loops
for (let i = 0; i < 1000000; i++) {
  proxy.value = i; // Slow
}

// ✅ Better: Direct access
for (let i = 0; i < 1000000; i++) {
  obj.value = i; // Fast
}
```

---

## Interview Questions

### Q1: Implement a read-only proxy

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

### Q2: Implement a range validator

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

### Q3: What's the output?

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
```
20  // 10 * 2
40  // 20 * 2
```

---

## Summary

### Proxy
- **Wraps objects** to intercept operations
- **Traps** define custom behavior
- **Use cases**: validation, logging, reactivity, access control
- **Performance**: Slower than direct access

### Reflect
- **Mirrors Proxy traps** with default operations
- **Better error handling** (returns boolean)
- **Use with Proxy** for cleaner code
- **Functional approach** to object operations

### Best Practices
- ✅ Use Reflect inside proxy traps
- ✅ Return proper values from traps (boolean for set/deleteProperty)
- ✅ Handle edge cases (symbols, non-existent properties)
- ❌ Avoid in performance-critical code
- ❌ Don't create deep proxy trees (performance)

### Common Patterns
1. **Validation** - Enforce rules on property assignment
2. **Logging** - Track property access/modification
3. **Reactivity** - Notify on changes (Vue.js 3 uses Proxy)
4. **Access control** - Hide/protect properties
5. **Default values** - Provide fallbacks

Master Proxy and Reflect for powerful metaprogramming! 🚀
