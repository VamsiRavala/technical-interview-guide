### Primitive vs non-primitive:

1. Primitive - String, Number, bigInteger, Undefined, Null, Symbol
2. Non-Promotive: Means can store multiple values/Complex values - Object, Array

### Hoisting

     This means that irrespective of where the variables and functions are declared, they are moved on top of the scope. The scope can be both local and global.

Example 1:
```javascript
hoistedVariable = 3;
console.log(hoistedVariable); // outputs 3 even when the variable is declared after it is initialized	
var hoistedVariable;
```
Example 2:
```javascript
hoistedFunction();  // Outputs " Hello world! " even when the function is declared after calling

function hoistedFunction(){ 
  console.log(" Hello world! ");
}
```

## Debugger:
The debugger for the browser must be activated in order to debug the code. 
Built-in debuggers may be switched on and off, requiring the user to report faults. 
The remaining section of the code should stop execution before moving on to the next line while debugging.

## Difference between “ == “ and “ === “ operators.

```javascript
var x = 2;
var y = "2";
(x == y)  // Returns true since the value of both x and y is the same
(x === y) // Returns false since the typeof x is "number" and typeof y is "string"
```

## difference b/w var and let:
- var is used long before from begining
- let introduced 2015
- let is scoped
- var is function scope
- let and const not hoisted

## Implicit Type Coercion in javascript.
Implicit type coercion in javascript is the automatic conversion of value from one data type to another. It takes place when the operands of an expression are of different data types.

String coercion
String coercion takes place while using the ‘ + ‘ operator. When a number is added to a string, the number type is always converted to the string type.

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
x + y   // Returns "24Hello";
```
Note - ‘ + ‘ operator when used to add two numbers, outputs a number. The same ‘ + ‘ operator when used to add two strings, outputs the concatenated string:
```javascript
var name = "Vivek";
var surname = " Bisht";
name + surname     // Returns "Vivek Bisht"
```
Note - Type coercion also takes place when using the ‘ - ‘ operator, but the difference while using ‘ - ‘ operator is that, a string is converted to a number and then subtraction takes place.
```javascript
var x = 3;
Var y = "3";
x - y    //Returns 0 since the variable y (string type) is converted to a number type
```

OR ( | | ) operator - If the first value is truthy, then the first value is returned. Otherwise, always the second value gets returned.

AND ( && ) operator - If both the values are truthy, always the second value is returned. If the first value is falsy then the first value is returned or if the second value is falsy then the second value is returned.

Example:
```javascript
var x = 220;
var y = "Hello";
var z = undefined;
        
x | | y    // Returns 220 since the first value is truthy
        
x | | z   // Returns 220 since the first value is truthy
        
x && y    // Returns "Hello" since both the values are truthy
        
y && z   // Returns undefined since the second value is falsy
        
if( x && y ){ 
  console.log("Code runs" ); // This block runs because x && y returns "Hello" (Truthy)
}   
        
if( x || z ){
  console.log("Code runs");  // This block runs because x || y returns 220(Truthy)
}

```

## Is javascript a statically typed or a dynamically typed language?
<img width="665" height="347" alt="image" src="https://github.com/user-attachments/assets/4881a1ed-7e45-4e26-a0d4-1b9810ecda26" />

## NaN:
Not a Number -->
```javascript
isNaN("Hello")  // Returns true
isNaN(345)   // Returns false
isNaN('1')  // Returns false, since '1' is converted to Number type which results in 0 ( a number) 
isNaN(true) // Returns false, since true converted to Number type results in 1 ( a number)
isNaN(false) // Returns false
isNaN(undefined) // Returns true
```

## Explain passed by value and passed by reference.
```javascript
var y = #8454; // y pointing to address of the value 234

var z = y; 
     
var z = #5411; // z pointing to a completely new address of the value 234
     
// Changing the value of y
y = 23;
console.log(z);  // Returns 234, since z points to a new address in the memory so changes in y will not effect z
```
## What is an Immediately Invoked Function in JavaScript?
(function(){ 
  // Do something;
})();

## javascript strict-mode?

In 'Strict mode,' however, all forms of errors, including silent errors, will be thrown. As a result, debugging becomes a lot simpler.  Thus programmer's chances of making an error are lowered.

Characteristics of strict mode in javascript

- Duplicate arguments are not allowed by developers.
- In strict mode, you won't be able to use the JavaScript keyword as a parameter or function name.
- The 'use strict' keyword is used to define strict mode at the start of the script. Strict mode is supported by all browsers.
- Engineers will not be allowed to create global variables in 'Strict Mode

## Higher Order Function in Javascript:
Functions that operate on other functions, either by taking them as arguments or by returning them, are called higher-order functions.
```javascript
function higherOrder(fn) {
  fn();
}
   
higherOrder(function() { console.log("Hello world") });  
```

## this:
The “this” keyword refers to the object that the function is a property of
```javascript
var obj = {
    name:  "vivek",
    getName: function(){
    console.log(this.name);
  }
}
   
obj.getName();
```

## Self Invoking Functions:
Without being requested, a self-invoking expression is automatically invoked (initiated). If a function expression is followed by (), it will execute automatically. A function declaration cannot be invoked by itself.

## Explain call(), apply() and, bind() methods.

- call() method allows an object to use the method (function) of another object.
```javascript
function sayHello(){
  return "Hello " + this.name;
}
        
var obj = {name: "Sandy"};
        
sayHello.call(obj);
        
// Returns "Hello Sandy"
```

- The apply method is similar to the call() method. The only difference is that,

call() method takes arguments separately whereas, apply() method takes arguments as an array.
```javascript
function saySomething(message){
  return this.name + " is " + message;
}        
var person4 = {name:  "John"};
saySomething.apply(person4, ["awesome"]);
```

- bind():
This method returns a new function, where the value of “this” keyword will be bound to the owner object, which is provided as a parameter.
```javascript
var bikeDetails = {
    displayDetails: function(registrationNumber,brandName){
    return this.name+ " , "+ "bike details: "+ registrationNumber + " , " + brandName;
  }
}
   
var person1 = {name:  "Vivek"};
     
var detailsOfPerson1 = bikeDetails.displayDetails.bind(person1, "TS0122", "Bullet");
      
// Binds the displayDetails function to the person1 object
        
      
detailsOfPerson1();
//Returns Vivek, bike details: TS0122, Bullet
```

## exec () and test () methods in javascript?
- test () and exec () are RegExp expression methods used in javascript.
- exec () to search a string for a specific pattern, and if it finds it, it'll return the pattern directly; else, it'll return an 'empty' result.
- use a test () to find a string for a specific pattern. It will return the Boolean value 'true' on finding the given text otherwise, it will return 'false'.

## currying?
- Currying is an advanced technique to transform a function of arguments n, to n functions of one or fewer arguments.
``` javascript
function add (a) {
  return function(b){
    return a + b;
  }
}

add(3)(4)
```

## External JavaScript
External JavaScript is the JavaScript Code (script) written in a separate file with the extension.js, and then we link that file inside the <head> or <body> element of the HTML file where the code is to be placed. 

## Scope and Scope Chain in javascript.
Scope in JS determines the accessibility of variables and functions at various parts of one’s code.
In general terms, the scope will let us know at a given part of code, what are variables and functions we can or cannot access.

There are three types of scopes in JS:

- Global Scope
- Local or Function Scope
- Block Scope

Global Scope: Variables or functions declared in the global namespace have global scope, which means all the variables and functions having global scope can be accessed from anywhere inside the code.
```javascript
var globalVariable = "Hello world";

function sendMessage(){
  return globalVariable; // can access globalVariable since it's written in global space
}
function sendMessage2(){
  return sendMessage(); // Can access sendMessage function since it's written in global space
}
sendMessage2();  // Returns “Hello world”
```

Function Scope: Any variables or functions declared inside a function have local/function scope, which means that all the variables and functions declared inside a function, can be accessed from within the function and not outside of it.

Block Scope: Block scope is related to the variables declared using let and const. Variables declared with var do not have block scope. Block scope tells us that any variable declared inside a block { }, can be accessed only inside that block and cannot be accessed outside of it.
Scope Chain: JavaScript engine also uses Scope to find variables. Let’s understand that using an example:
```javascript
var y = 24;

function favFunction(){
  var x = 667;
  var anotherFavFunction = function(){
    console.log(x); // Does not find x inside anotherFavFunction, so looks for variable inside favFunction, outputs 667
  }

  var yetAnotherFavFunction = function(){
    console.log(y); // Does not find y inside yetAnotherFavFunction, so looks for variable inside favFunction and does not find it, so looks for variable in global scope, finds it and outputs 24
  }

  anotherFavFunction();
  yetAnotherFavFunction();
}
favFunction();
```
## Closures :
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
    console.log(obj1.name + " is "+ "awesome"); // Has access to obj1 even when the randomFunc function is executed

  }
}

var initialiseClosure = randomFunc(); // Returns a function

initialiseClosure();
```
Let’s understand the code above,

The function randomFunc() gets executed and returns a function when we assign it to a variable:

var initialiseClosure = randomFunc();
The returned function is then executed when we invoke initialiseClosure:

initialiseClosure(); 
The line of code above outputs “Vivian is awesome” and this is possible because of closure.

console.log(obj1.name + " is "+ "awesome");
When the function randomFunc() runs, it seems that the returning function is using the variable obj1 inside it:

Therefore randomFunc(), instead of destroying the value of obj1 after execution, saves the value in the memory for further reference. This is the reason why the returning function is able to use the variable declared in the outer scope even after the function is already executed.

This ability of a function to store a variable for further reference even after it is executed is called Closure

## advantages of javascript

- Javascript is executed on the client-side as well as server-side also. There are a variety of Frontend Frameworks that you may study and utilize. However, if you want to use JavaScript on the backend, you'll need to learn NodeJS. It is currently the only JavaScript framework that may be used on the backend.
- Javascript is a simple language to learn.
- Web pages now have more functionality because of Javascript.
- To the end-user, Javascript is quite quick

## object prototypes?
All javascript objects inherit properties from a prototype. For example,
Date objects inherit properties from the Date prototype
Math objects inherit properties from the Math prototype
Array objects inherit properties from the Array prototype.
On top of the chain is Object.prototype. Every prototype inherits properties and methods from the Object.prototype.
A prototype is a blueprint of an object. The prototype allows us to use properties and methods on an object even if the properties and methods do not exist on the current object.

