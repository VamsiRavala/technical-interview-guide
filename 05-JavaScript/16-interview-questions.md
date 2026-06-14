# JavaScript Interview Questions

A curated set of core JavaScript interview questions and answers covering data types, hoisting, operators, scope, closures, `this`, and more.

## Primitive vs Non-Primitive

1. **Primitive** — String, Number, BigInteger, Undefined, Null, Symbol
2. **Non-primitive** — Can store multiple/complex values: Object, Array

## Hoisting

Hoisting means that, irrespective of where variables and functions are declared, they are moved to the top of the scope. The scope can be both local and global.

Example 1:

```javascript
hoistedVariable = 3;
console.log(hoistedVariable); // outputs 3 even when the variable is declared after it is initialized
var hoistedVariable;
```

Example 2:

```javascript
hoistedFunction();  // Outputs "Hello world!" even when the function is declared after calling

function hoistedFunction(){
  console.log(" Hello world! ");
}
```

## Debugger

The debugger for the browser must be activated in order to debug the code. Built-in debuggers may be switched on and off, requiring the user to report faults. The remaining section of the code should stop execution before moving on to the next line while debugging.

## Difference between `==` and `===` operators

```javascript
var x = 2;
var y = "2";
(x == y)  // Returns true since the value of both x and y is the same
(x === y) // Returns false since the typeof x is "number" and typeof y is "string"
```

## Difference between `var` and `let`

- `var` has been used long before, from the beginning
- `let` was introduced in 2015
- `let` is block scoped
- `var` is function scoped
- `let` and `const` are not hoisted (in the same usable way as `var`)

## Implicit Type Coercion in JavaScript

Implicit type coercion in JavaScript is the automatic conversion of a value from one data type to another. It takes place when the operands of an expression are of different data types.

**String coercion** takes place while using the `+` operator. When a number is added to a string, the number type is always converted to the string type.

Example 1:

```javascript
var x = 3;
var y = "3";
x + y // Returns "33"
```

Example 2:

```javascript
var x = 24;
var y = "Hello";
x + y   // Returns "24Hello"
```

Note: The `+` operator, when used to add two numbers, outputs a number. The same `+` operator, when used to add two strings, outputs the concatenated string:

```javascript
var name = "Vivek";
var surname = " Bisht";
name + surname     // Returns "Vivek Bisht"
```

Note: Type coercion also takes place when using the `-` operator, but the difference is that a string is converted to a number and then subtraction takes place.

```javascript
var x = 3;
var y = "3";
x - y    // Returns 0 since the variable y (string type) is converted to a number type
```

**OR (`||`) operator** — If the first value is truthy, then the first value is returned. Otherwise, the second value is always returned.

**AND (`&&`) operator** — If both values are truthy, the second value is always returned. If the first value is falsy, the first value is returned; or if the second value is falsy, the second value is returned.

Example:

```javascript
var x = 220;
var y = "Hello";
var z = undefined;

x || y    // Returns 220 since the first value is truthy

x || z    // Returns 220 since the first value is truthy

x && y    // Returns "Hello" since both the values are truthy

y && z    // Returns undefined since the second value is falsy

if( x && y ){
  console.log("Code runs"); // This block runs because x && y returns "Hello" (Truthy)
}

if( x || z ){
  console.log("Code runs");  // This block runs because x || z returns 220 (Truthy)
}
```

## Is JavaScript a Statically Typed or a Dynamically Typed Language?

JavaScript is a **dynamically typed** language. In a dynamically typed language, the type of a variable is checked during run-time (as opposed to a statically typed language, where the type is checked during compile-time). Because JavaScript is loosely/dynamically typed, variables are not directly associated with any particular value type, and any variable can be assigned (and re-assigned) values of all types.

## NaN

NaN stands for "Not a Number".

```javascript
isNaN("Hello")    // Returns true
isNaN(345)        // Returns false
isNaN('1')        // Returns false, since '1' is converted to Number type which results in 1 (a number)
isNaN(true)       // Returns false, since true converted to Number type results in 1 (a number)
isNaN(false)      // Returns false
isNaN(undefined)  // Returns true
```

## Pass by Value vs Pass by Reference

```javascript
var y = 234; // y pointing to address of the value 234

var z = y;

// z now points to a completely new address of the value 234

// Changing the value of y
y = 23;
console.log(z);  // Returns 234, since z points to a new address in memory so changes in y will not affect z
```

Primitives are passed by value; objects are passed by reference (technically, a copy of the reference).

## What is an Immediately Invoked Function in JavaScript?

```javascript
(function(){
  // Do something;
})();
```

## JavaScript Strict Mode

In 'Strict mode', all forms of errors, including silent errors, will be thrown. As a result, debugging becomes a lot simpler, and the programmer's chances of making an error are lowered.

Characteristics of strict mode in JavaScript:

- Duplicate arguments are not allowed by developers.
- In strict mode, you won't be able to use a JavaScript keyword as a parameter or function name.
- The `'use strict'` keyword is used to define strict mode at the start of the script. Strict mode is supported by all browsers.
- Engineers will not be allowed to create global variables in strict mode.

## Higher Order Function in JavaScript

Functions that operate on other functions, either by taking them as arguments or by returning them, are called higher-order functions.

```javascript
function higherOrder(fn) {
  fn();
}

higherOrder(function() { console.log("Hello world") });
```

## The `this` Keyword

The `this` keyword refers to the object that the function is a property of.

```javascript
var obj = {
  name: "vivek",
  getName: function(){
    console.log(this.name);
  }
}

obj.getName();
```

## Self-Invoking Functions

Without being requested, a self-invoking expression is automatically invoked (initiated). If a function expression is followed by `()`, it will execute automatically. A function declaration cannot be invoked by itself.

## Explain `call()`, `apply()`, and `bind()` Methods

`call()` method allows an object to use the method (function) of another object.

```javascript
function sayHello(){
  return "Hello " + this.name;
}

var obj = {name: "Sandy"};

sayHello.call(obj);

// Returns "Hello Sandy"
```

The `apply()` method is similar to `call()`. The only difference is that `call()` takes arguments separately, whereas `apply()` takes arguments as an array.

```javascript
function saySomething(message){
  return this.name + " is " + message;
}
var person4 = {name: "John"};
saySomething.apply(person4, ["awesome"]);
```

`bind()` returns a new function where the value of the `this` keyword will be bound to the owner object, which is provided as a parameter.

```javascript
var bikeDetails = {
  displayDetails: function(registrationNumber, brandName){
    return this.name + " , " + "bike details: " + registrationNumber + " , " + brandName;
  }
}

var person1 = {name: "Vivek"};

var detailsOfPerson1 = bikeDetails.displayDetails.bind(person1, "TS0122", "Bullet");

// Binds the displayDetails function to the person1 object

detailsOfPerson1();
// Returns "Vivek , bike details: TS0122 , Bullet"
```

## `exec()` and `test()` Methods in JavaScript

- `test()` and `exec()` are RegExp expression methods used in JavaScript.
- `exec()` searches a string for a specific pattern, and if it finds it, it returns the pattern directly; else it returns an 'empty' (null) result.
- `test()` searches a string for a specific pattern. It returns the Boolean value `true` on finding the given text; otherwise it returns `false`.

## Currying

Currying is an advanced technique to transform a function of `n` arguments into `n` functions of one or fewer arguments.

```javascript
function add (a) {
  return function(b){
    return a + b;
  }
}

add(3)(4)
```

## External JavaScript

External JavaScript is the JavaScript code (script) written in a separate file with the extension `.js`, and then we link that file inside the `<head>` or `<body>` element of the HTML file where the code is to be placed.

## Scope and Scope Chain in JavaScript

Scope in JS determines the accessibility of variables and functions at various parts of one's code. In general terms, scope lets us know, at a given part of code, what variables and functions we can or cannot access.

There are three types of scopes in JS:

- Global Scope
- Local or Function Scope
- Block Scope

**Global Scope:** Variables or functions declared in the global namespace have global scope, which means all the variables and functions having global scope can be accessed from anywhere inside the code.

```javascript
var globalVariable = "Hello world";

function sendMessage(){
  return globalVariable; // can access globalVariable since it's written in global space
}
function sendMessage2(){
  return sendMessage(); // Can access sendMessage function since it's written in global space
}
sendMessage2();  // Returns "Hello world"
```

**Function Scope:** Any variables or functions declared inside a function have local/function scope, which means all the variables and functions declared inside a function can be accessed from within the function and not outside of it.

**Block Scope:** Block scope is related to the variables declared using `let` and `const`. Variables declared with `var` do not have block scope. Block scope tells us that any variable declared inside a block `{ }` can be accessed only inside that block and cannot be accessed outside of it.

**Scope Chain:** The JavaScript engine uses scope to find variables. Example:

```javascript
var y = 24;

function favFunction(){
  var x = 667;
  var anotherFavFunction = function(){
    console.log(x); // Does not find x inside anotherFavFunction, so looks for variable inside favFunction, outputs 667
  }

  var yetAnotherFavFunction = function(){
    console.log(y); // Does not find y inside yetAnotherFavFunction, so looks inside favFunction and does not find it, so looks in global scope, finds it and outputs 24
  }

  anotherFavFunction();
  yetAnotherFavFunction();
}
favFunction();
```

## Closures

Closures are an ability of a function to remember the variables and functions that are declared in its outer scope.

```javascript
var Person = function(pName){
  var name = pName;

  this.getName = function(){
    return name;
  }
}

var person = new Person("Neelesh");
console.log(person.getName());
```

```javascript
function randomFunc(){
  var obj1 = {name:"Vivian", age:45};

  return function(){
    console.log(obj1.name + " is " + "awesome"); // Has access to obj1 even when the randomFunc function is executed
  }
}

var initialiseClosure = randomFunc(); // Returns a function

initialiseClosure();
```

Understanding the code above:

The function `randomFunc()` gets executed and returns a function when we assign it to a variable:

```javascript
var initialiseClosure = randomFunc();
```

The returned function is then executed when we invoke `initialiseClosure`:

```javascript
initialiseClosure();
```

The line of code above outputs "Vivian is awesome", and this is possible because of closure.

```javascript
console.log(obj1.name + " is " + "awesome");
```

When the function `randomFunc()` runs, the returning function uses the variable `obj1` inside it. Therefore `randomFunc()`, instead of destroying the value of `obj1` after execution, saves the value in memory for further reference. This is why the returning function is able to use the variable declared in the outer scope even after the function has already executed.

This ability of a function to store a variable for further reference even after it is executed is called a **Closure**.

## Advantages of JavaScript

- JavaScript is executed on the client-side as well as the server-side. There are a variety of frontend frameworks you may study and utilize. To use JavaScript on the backend, you'll need to learn Node.js. It is the primary JavaScript runtime used on the backend.
- JavaScript is a simple language to learn.
- Web pages have more functionality because of JavaScript.
- To the end-user, JavaScript is quite quick.

## Object Prototypes

All JavaScript objects inherit properties from a prototype. For example:

- Date objects inherit properties from the Date prototype
- Math objects inherit properties from the Math prototype
- Array objects inherit properties from the Array prototype

On top of the chain is `Object.prototype`. Every prototype inherits properties and methods from `Object.prototype`.

A prototype is a blueprint of an object. The prototype allows us to use properties and methods on an object even if the properties and methods do not exist on the current object.
