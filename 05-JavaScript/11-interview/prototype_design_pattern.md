
# prototype design pattern
### 🔄 Class vs Prototype Pattern in JavaScript

## 🔹 1. Conceptual Difference

-   **Class (OOP style)**
    -   A **blueprint** for creating objects.\
    -   Properties and methods are defined inside the class.\
    -   Instances are created with `new`, which links them to the
        class's prototype.
-   **Prototype Pattern**
    -   Uses an **existing object** as a base (prototype).\
    -   New objects are created by **cloning** this prototype.\
    -   No rigid blueprint needed, more flexible.

------------------------------------------------------------------------

## 🔹 2. Example in JavaScript

### Using a **Class**

``` js
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

------------------------------------------------------------------------

### Using the **Prototype Pattern**

``` js
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

------------------------------------------------------------------------

## 🔹 3. Key Differences

  --------------------------------------------------------------------------------
  Aspect                        Class               Prototype Pattern
  ----------------------------- ------------------- ------------------------------
  **Definition**                Defined using       Starts with an existing object
                                `class` keyword     
                                (blueprint)         

  **Creation**                  `new ClassName()`   `Object.create(prototype)`

  **Inheritance**               `extends` keyword   Prototype chaining

  **Flexibility**               Structured,         Dynamic, flexible
                                OOP-like            

  **Readability**               Familiar to OOP     Native JavaScript style
                                devs                

  **Best Use Case**             Entities, models,   Lightweight cloning, config
                                clear structure     objects
  --------------------------------------------------------------------------------

------------------------------------------------------------------------

## 🔹 4. Real-Life Analogy

-   **Class** → **Blueprint of a house** 🏠\
    Define layout, rooms, and design → build many houses from the same
    plan.

-   **Prototype Pattern** → **Copy an existing house** 🏡\
    Make slight modifications (paint, furniture, extensions).

Both give you a house, but the **approach differs**.

------------------------------------------------------------------------

## ✅ Summary

-   Use **Classes** for structured, blueprint-driven designs (great for
    teams and OOP developers).\
-   Use **Prototype Pattern** when you want flexibility and lightweight
    cloning (great for dynamic or config-driven systems).
