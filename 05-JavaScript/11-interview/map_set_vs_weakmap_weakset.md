# 🔄 Set, Map, WeakSet, WeakMap in JavaScript

JavaScript provides powerful collection types beyond arrays and objects.
Let's break them down.

------------------------------------------------------------------------

## 🔹 1. Set

-   Stores **unique values** (no duplicates).\
-   Values can be any type (primitives or objects).\
-   Insertion order is preserved.

``` js
const mySet = new Set([1, 2, 3, 3, 4]);
console.log(mySet); // Set(4) { 1, 2, 3, 4 }

mySet.add(5);
console.log(mySet.has(3)); // true
mySet.delete(2);

for (let val of mySet) console.log(val); // 1, 3, 4, 5
```

✅ **Best for:** Removing duplicates, storing unique values.

------------------------------------------------------------------------

## 🔹 2. Map

-   Stores **key--value pairs**.\
-   Keys can be **any type** (even objects or functions).\
-   Preserves insertion order.

``` js
const myMap = new Map();
myMap.set("name", "Alice");
myMap.set(1, "number one");
myMap.set({ id: 1 }, "object key");

console.log(myMap.get("name")); // Alice
console.log(myMap.has(1));      // true
myMap.delete("name");
```

✅ **Best for:** Key--value storage when keys aren't limited to strings.

------------------------------------------------------------------------

## 🔹 3. WeakSet

-   Like **Set**, but only stores **objects**.\
-   References are **weak** → objects can be garbage-collected if no
    other reference exists.\
-   Not iterable (no `.forEach`, no `size`).

``` js
let obj1 = { name: "Alice" };
let obj2 = { name: "Bob" };

const weakSet = new WeakSet([obj1, obj2]);
console.log(weakSet.has(obj1)); // true

obj1 = null; // obj1 removed automatically from weakSet
```

✅ **Best for:** Temporary object references without memory leaks.

------------------------------------------------------------------------

## 🔹 4. WeakMap

-   Like **Map**, but keys must be **objects**.\
-   References are **weak** → if the key object is garbage-collected,
    entry is removed.\
-   Not iterable (no `.forEach`, no `size`).

``` js
let obj = { id: 1 };
const weakMap = new WeakMap();
weakMap.set(obj, "secret data");

console.log(weakMap.get(obj)); // secret data
obj = null; // entry removed automatically
```

✅ **Best for:** Storing private metadata tied to object lifecycle.

------------------------------------------------------------------------

## 🔹 Key Differences (Cheat Sheet)

  ----------------------------------------------------------------------------
  Feature                Set      Map          WeakSet         WeakMap
  ---------------------- -------- ------------ --------------- ---------------
  Stores                 Unique   Key--Value   Objects only    Key--Value
                         values   pairs                        pairs (keys =
                                                               objects only)

  Key types              N/A      Any type     Objects only    Objects only

  Garbage Collection     ❌ No    ❌ No        ✅ Yes          ✅ Yes

  Iterable               ✅ Yes   ✅ Yes       ❌ No           ❌ No

  Duplicates             ❌ No    ❌ No (keys  ❌ No           ❌ No
                                  unique)                      
  ----------------------------------------------------------------------------

------------------------------------------------------------------------

## 🔹 Real-Life Analogy

-   **Set** → A guest list 📝 (no duplicate names).\
-   **Map** → A phone book 📒 (name → phone number).\
-   **WeakSet** → A backstage pass 🎟️ (only valid while the person
    exists).\
-   **WeakMap** → A locker room 🛅 (key = person, value = locker; if the
    person leaves, the locker is removed).

------------------------------------------------------------------------

## ✅ Summary

-   **Set** → Unique values.\
-   **Map** → Key--value pairs (any type of key).\
-   **WeakSet** → Object references without preventing garbage
    collection.\
-   **WeakMap** → Object-keyed private data tied to object lifecycle.

