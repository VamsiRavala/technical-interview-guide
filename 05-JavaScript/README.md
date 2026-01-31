# JavaScript — Complete Programming Guide

> Comprehensive guide to JavaScript fundamentals, advanced concepts, and interview preparation.

---

## 📚 Table of Contents

### Fundamentals
1. **[Syntax](01-syntax.md)** - Basic JavaScript syntax
2. **[Variables](02-variables.md)** - var, let, const, and variable declarations
3. **[Operators & Control Structures](03-operators-control.md)** - Operators and flow control
4. **[Functions](04-functions.md)** - Function fundamentals
5. **[Arrow Functions](05-arrow-functions.md)** - Modern function syntax
6. **[Objects](06-objects.md)** - Object-oriented JavaScript
7. **[Arrays](07-arrays.md)** - Array methods and operations
8. **[Map & Set](08-map-set.md)** - Modern data structures
9. **[Callback Functions](09-callback-functions.md)** - Understanding callbacks
10. **[Async Programming](10-async.md)** - Promises and async/await

### Interview Preparation
11. **[Interview Topics](11-interview/)** - Advanced concepts and interview questions
    - Closures
    - Currying
    - Memoization
    - Generator functions
    - Destructuring
    - Rest parameters and spread operator
    - Async and Promises
    - Array filter and methods
    - Error handling
    - ES5 vs ES6
    - Prototypes
    - And more...

---

## 🎯 Learning Path

### Beginner (Week 1-2)
1. **Core Syntax**
   - Start with [Syntax](01-syntax.md)
   - Learn [Variables](02-variables.md) (var, let, const)
   - Study [Operators & Control Structures](03-operators-control.md)
   - Practice basic coding exercises

2. **Functions & Objects**
   - Master [Functions](04-functions.md)
   - Learn [Arrow Functions](05-arrow-functions.md)
   - Understand [Objects](06-objects.md)
   - Practice with simple programs

### Intermediate (Week 3-4)
1. **Data Structures**
   - Deep dive into [Arrays](07-arrays.md)
   - Learn [Map & Set](08-map-set.md)
   - Study [Callback Functions](09-callback-functions.md)

2. **Async Programming**
   - Master [Async Programming](10-async.md)
   - Understand Promises
   - Learn async/await
   - Handle errors properly

### Advanced (Week 5-6)
1. **Advanced Concepts**
   - Study all topics in [Interview folder](11-interview/)
   - Closures and scope
   - Prototypes and inheritance
   - ES6+ features
   - Design patterns

2. **Interview Preparation**
   - Practice coding challenges
   - Review interview questions
   - Build real-world projects
   - Optimize code performance

---

## 🔑 Key Concepts Summary

### Variables & Data Types
- **Primitives**: string, number, boolean, null, undefined, symbol, bigint
- **Reference Types**: objects, arrays, functions
- **Variable Declarations**: var (function-scoped), let (block-scoped), const (immutable binding)

### Functions
- **Function Declarations**: `function name() {}`
- **Function Expressions**: `const fn = function() {}`
- **Arrow Functions**: `const fn = () => {}`
- **IIFE**: Immediately Invoked Function Expressions
- **Higher-Order Functions**: Functions that take or return functions

### Async Programming
- **Callbacks**: Function passed as argument
- **Promises**: `.then()`, `.catch()`, `.finally()`
- **Async/Await**: Syntactic sugar over promises
- **Error Handling**: try/catch blocks

### Modern JavaScript (ES6+)
- Template literals
- Destructuring assignment
- Spread operator & rest parameters
- Default parameters
- Classes and modules
- Map, Set, WeakMap, WeakSet
- Symbols and iterators

---

## 💡 Quick Reference

### Common Array Methods
```javascript
// Iteration
forEach(), map(), filter(), reduce()

// Search
find(), findIndex(), indexOf(), includes()

// Transform
slice(), splice(), concat(), flat()

// Test
some(), every()

// Sort
sort(), reverse()
```

### Promise Patterns
```javascript
// Single promise
fetch(url).then(response => response.json()).then(data => console.log(data))

// Multiple promises
Promise.all([p1, p2, p3])
Promise.race([p1, p2, p3])
Promise.allSettled([p1, p2, p3])

// Async/await
async function fetchData() {
  try {
    const response = await fetch(url)
    const data = await response.json()
    return data
  } catch (error) {
    console.error(error)
  }
}
```

### Object Methods
```javascript
// Keys and values
Object.keys(obj)
Object.values(obj)
Object.entries(obj)

// Merging
Object.assign(target, source)
{ ...obj1, ...obj2 } // spread

// Freeze and seal
Object.freeze(obj)
Object.seal(obj)
```

---

## 🎓 Common Interview Topics

### 1. Core Concepts
- **Hoisting**: Variable and function declarations
- **Scope**: Global, function, block scope
- **Closures**: Functions with private variables
- **'this' keyword**: Context binding
- **Prototypes**: Inheritance mechanism

### 2. ES6+ Features
- Arrow functions vs regular functions
- Let vs const vs var
- Template literals
- Destructuring
- Spread and rest operators
- Classes and modules
- Promises and async/await

### 3. Async Programming
- Event loop and call stack
- Callbacks and callback hell
- Promises chaining
- Async/await patterns
- Error handling
- Parallel vs sequential execution

### 4. Functions
- Pure functions
- Higher-order functions
- Currying and partial application
- Memoization
- Function composition
- IIFE patterns

### 5. Objects & Arrays
- Object creation patterns
- Prototypal inheritance
- Array manipulation methods
- Shallow vs deep copy
- Object/array destructuring

---

## 🚀 Modern JavaScript Best Practices

### Code Style
- ✅ Use `const` by default, `let` when needed, avoid `var`
- ✅ Prefer arrow functions for callbacks
- ✅ Use template literals for string interpolation
- ✅ Destructure objects and arrays
- ✅ Use default parameters
- ✅ Avoid nested callbacks (callback hell)

### Performance
- ✅ Use `map()`, `filter()`, `reduce()` appropriately
- ✅ Avoid blocking the main thread
- ✅ Use Web Workers for heavy computations
- ✅ Implement debouncing and throttling
- ✅ Lazy load when possible

### Error Handling
- ✅ Always handle promise rejections
- ✅ Use try/catch with async/await
- ✅ Validate inputs
- ✅ Provide meaningful error messages
- ✅ Log errors appropriately

### Code Organization
- ✅ Use modules (import/export)
- ✅ Follow single responsibility principle
- ✅ Keep functions small and focused
- ✅ Use meaningful variable names
- ✅ Comment complex logic

---

## 📖 Interview Preparation Strategy

### Week Before Interview
- Review all fundamental concepts
- Practice common interview questions
- Solve coding challenges on LeetCode/HackerRank
- Review ES6+ features
- Practice explaining concepts

### Day Before Interview
- Review closures and prototypes
- Practice async/await patterns
- Go through common algorithms
- Review your past projects
- Prepare questions to ask

### During Interview
- Think out loud
- Start with simple solution
- Consider edge cases
- Discuss time/space complexity
- Ask clarifying questions

---

## 🎯 Practice Resources

### Online Platforms
- [LeetCode](https://leetcode.com) - Coding challenges
- [HackerRank](https://hackerrank.com) - Programming practice
- [Codewars](https://codewars.com) - Kata challenges
- [JavaScript30](https://javascript30.com) - 30 day challenge

### Reference Sites
- [MDN Web Docs](https://developer.mozilla.org) - Comprehensive JS reference
- [JavaScript.info](https://javascript.info) - Modern JavaScript tutorial
- [You Don't Know JS](https://github.com/getify/You-Dont-Know-JS) - Free book series

---

## 💻 Sample Interview Questions

### Easy
1. What is the difference between `let`, `const`, and `var`?
2. Explain hoisting in JavaScript
3. What are arrow functions?
4. What is the difference between `==` and `===`?
5. What are template literals?

### Medium
1. Explain closures with an example
2. What is the event loop?
3. Explain promises and async/await
4. What is prototypal inheritance?
5. Explain `this` keyword in different contexts

### Hard
1. Implement a debounce function
2. Implement Promise.all from scratch
3. Explain event delegation
4. Implement a deep clone function
5. Solve currying and function composition problems

---

## 🔗 Related Topics

### Front-End
- **Frameworks**: React, Vue, Angular
- **Build Tools**: Webpack, Vite, Rollup
- **Testing**: Jest, Mocha, Cypress
- **TypeScript**: Typed superset of JavaScript

### Back-End
- **Node.js**: Server-side JavaScript
- **Express.js**: Web framework
- **Databases**: MongoDB, PostgreSQL
- **APIs**: REST, GraphQL

### Tools & Environment
- **Package Managers**: npm, yarn, pnpm
- **Linters**: ESLint, Prettier
- **Version Control**: Git
- **Browser DevTools**: Chrome DevTools

---

## 🤝 Contributing

Contributions welcome! Please ensure:
- Code examples are tested
- Explanations are clear and concise
- Follow ES6+ standards
- Include comments for complex code

---

*Master JavaScript and ace your interviews!* 💪
