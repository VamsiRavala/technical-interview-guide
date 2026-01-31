# Variable
-- Box with some label on it
-- you can put any type

# String:
-- properties and toolbox
    var mystring="This is my string"
    mystring.__proto__             mystring.hasOwnProperty        mystring.isPrototypeOf
    mystring.propertyIsEnumerable  mystring.toLocaleString
    
    mystring.anchor                mystring.at                    mystring.big
    mystring.blink                 mystring.bold                  mystring.charAt
    mystring.charCodeAt            mystring.codePointAt           mystring.concat
    mystring.constructor           mystring.endsWith              mystring.fixed
    mystring.fontcolor             mystring.fontsize              mystring.includes
    mystring.indexOf               mystring.isWellFormed          mystring.italics
    mystring.lastIndexOf           mystring.length                mystring.link
    mystring.localeCompare         mystring.match                 mystring.matchAll
    mystring.normalize             mystring.padEnd                mystring.padStart
    mystring.repeat                mystring.replace               mystring.replaceAll
    mystring.search                mystring.slice                 mystring.small
    mystring.split                 mystring.startsWith            mystring.strike
    mystring.sub                   mystring.substr                mystring.substring
    mystring.sup                   mystring.toLocaleLowerCase     mystring.toLocaleUpperCase
    mystring.toLowerCase           mystring.toString              mystring.toUpperCase
    mystring.toWellFormed          mystring.trim                  mystring.trimEnd
    mystring.trimLeft              mystring.trimRight             mystring.trimStart
    mystring.valueOf

  ## Template literals
  -- let mystring=`This is "string"`

  <img width="983" height="258" alt="image" src="https://github.com/user-attachments/assets/bb36c5e0-4ec0-4e2b-bd89-1ad3e975636d" />

  let mystring=`This is ${var_name}`


# Numbers:
 - Javascript is Number
<img width="1293" height="403" alt="image" src="https://github.com/user-attachments/assets/f5d554cf-c8e8-453f-8db8-ea49e8ff7acd" />
<img width="1407" height="146" alt="image" src="https://github.com/user-attachments/assets/b39e79a9-10ba-43b5-bb43-7b0f7f7e995d" />

  NaN: (Not a Number)

  - Math.
      - Helper objects - having lot of functions
        <img width="1418" height="367" alt="image" src="https://github.com/user-attachments/assets/79f9e000-d17b-48cb-87d5-1cc9fa89abdb" />

# Boolean:
  - data type flag
  - true or false
  - True (it will consider as variable)
  - <img width="1161" height="362" alt="image" src="https://github.com/user-attachments/assets/a8fbe57e-f449-4e7a-8fd2-999749af90dc" />

## Double Equal to vs Thriple (== vs ===):
-- Comparision

# Varible Mutability:

var --> is changable/mutable

set Once never change later
const --> immutable variable

const pi=3.14

just like other variable you can refer any time

change const value you will get error. 

let --> mutable

var vs let:


# JavaScript: var vs let vs const

Understanding the differences between `var`, `let`, and `const` is crucial for writing clean and predictable JavaScript.

---

## 1. Scope

- **`var`** → Function-scoped  
- **`let`** → Block-scoped  
- **`const`** → Block-scoped

**Example:**
```javascript
if (true) {
  var x = 10;
  let y = 20;
  const z = 30;
}
console.log(x); // ✅ 10
console.log(y); // ❌ ReferenceError
console.log(z); // ❌ ReferenceError
```

---

## 2. Hoisting

- **`var`** is hoisted and initialized with `undefined`.  
- **`let`** and **`const`** are hoisted but remain in the **temporal dead zone (TDZ)** until declared.

**Example:**
```javascript
console.log(a); // ✅ undefined
var a = 5;

console.log(b); // ❌ ReferenceError
let b = 10;

console.log(c); // ❌ ReferenceError
const c = 15;
```

---

## 3. Re-declaration & Re-assignment

- **`var`**: can be re-declared and re-assigned.  
- **`let`**: can be re-assigned, but not re-declared in the same scope.  
- **`const`**: cannot be re-declared or re-assigned (though objects/arrays can be mutated).

**Example:**
```javascript
// var
var a = 1;
var a = 2; // ✅ re-declaration works
a = 3;    // ✅ re-assignment works

// let
let b = 1;
// let b = 2; ❌ SyntaxError (no re-declaration)
b = 3;    // ✅ re-assignment works

// const
const c = 1;
// const c = 2; ❌ SyntaxError (no re-declaration)
// c = 3;       ❌ TypeError (no re-assignment)

// BUT objects can mutate
const obj = { name: "Alice" };
obj.name = "Bob"; // ✅ mutation works
```

---

## 4. Best Practices

- **Avoid `var`** in modern JavaScript.  
- **Use `let`** for variables that need to change.  
- **Use `const`** by default for variables that shouldn’t change.

---

✅ **Summary Table:**

| Feature         | var              | let             | const              |
|-----------------|------------------|-----------------|--------------------|
| Scope           | Function         | Block           | Block              |
| Hoisting        | Yes (undefined)  | Yes (TDZ)       | Yes (TDZ)          |
| Re-declaration  | Yes              | No              | No                 |
| Re-assignment   | Yes              | Yes             | No (except object) |



        
  

