
## constructor function in javascript
Constructor functions are used to create objects in javascript.
When do we use constructor functions?

If we want to create multiple objects having similar properties and methods, constructor functions are used.

function Person(name,age,gender){
  this.name = name;
  this.age = age;
  this.gender = gender;
}


var person1 = new Person("Vivek", 76, "male");
console.log(person1);

var person2 = new Person("Courtney", 34, "female");
console.log(person2);

## What is DOM?
- DOM stands for Document Object Model.  DOM is a programming interface for HTML and XML documents.
- When the browser tries to render an HTML document, it creates an object based on the HTML document called DOM. Using this DOM, we can manipulate or change various elements inside the HTML document.

- <img width="1130" height="532" alt="image" src="https://github.com/user-attachments/assets/9fff84cc-c6f7-4250-8699-ba6402921ec3" />


## Which method is used to retrieve a character from a certain index
The charAt() function of the JavaScript string finds a char element at the supplied index. The index number begins at 0 and continues up to n-1, Here n is the string length. The index value must be positive, higher than, or the same as the string length

## What do you mean by BOM?
Browser Object Model is known as BOM. It allows users to interact with the browser. 
A browser's initial object is a window. As a result, you may call all of the window's functions directly or by referencing the window. 
The document, history, screen, navigator, location, and other attributes are available in the window object


