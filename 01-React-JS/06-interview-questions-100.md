## What is React, and what are its main features?
 - React is a JavaScript library developed by Facebook for creating user interfaces, particularly in single-page applications.
 -  It enables the use of reusable components that manage their own state.
   ### Advantages
   -  component-driven architecture,
   -  optimized updates through the virtual DOM,
   -  a declarative approach for better readability,
   -  and robust community backing.

## JSX:
- JSX, short for JavaScript XML, is a syntax extension for JavaScript that allows you to write HTML-like code within JavaScript
- It makes building React components easier. JSX gets converted into JavaScript function calls,
- often by Babel. For instance, <div>Hello, world!</div> is transformed into React.createElement('div', null, 'Hello, world!').

## Babel:
 - Babel is an open-source JavaScript compiler (or transpiler).
 - Its main job is to take modern JavaScript code (using ES6+ features and beyond) and convert it into older versions of JavaScript that work on environments or browsers which don’t yet support those new features.
  ### Why Developers Use Babel
  - Browser Compatibility - Not all browsers support the latest JavaScript features (e.g., async/await, optional chaining, modules). Babel converts them into equivalent older syntax.
  - Future-Proofing - Developers can start using upcoming ECMAScript features today without waiting for all browsers to catch up.
  - Ecosystem Integration - Babel integrates seamlessly with tools like Webpack, Rollup, and Parcel to create modern build pipelines.

## Virtual DOM:
 - The virtual DOM is a simplified version of the actual DOM used by React
 - It allows for efficient UI updates by comparing the virtual DOM to the real DOM and making only the necessary changes through a process known as reconciliation.

## How Does virtual DOM in React work
 - The virtual DOM in React is an in-memory representation of the real DOM.
 - When state or props change, React creates a new virtual DOM tree, compares it to the previous one using a diffing algorithm, and efficiently updates only the parts of the real DOM that changed.

   ### Benefits:
      - It improves performance by reducing costly direct DOM manipulations and makes UI updates declarative and predictable.
   ### Downsides:
    - There's some overhead from diffing and extra memory usage, and in very dynamic UIs, it may not always outperform manual optimizations.
  
## What is difference Between React Node, React Element, and React Component
- A React Node refers to any unit that can be rendered in React, such as an element, string, number, or null.
- A React Element is an immutable object that defines what should be rendered, typically created using JSX or React.createElement.
- A React Component is either a function or class that returns React Elements, enabling the creation of reusable UI components.

<img width="1024" height="1536" alt="image" src="https://github.com/user-attachments/assets/32fc7edc-e150-4a80-9d8a-cb56ee478baa" />

## What are React Fragments used for?
- React Fragments allow you to group multiple elements without adding extra nodes to the DOM.
- They are particularly useful when you need to return multiple elements from a component but don't want to wrap them in a container element.
- You can utilize shorthand syntax <>...</> or React.Fragment.
  ```javascript
  return (
  <>
    <ChildComponent1 />
    <ChildComponent2 />
  </>
   );
    ```
## What is the purpose of the "key" prop in React?
  - In React, the key prop is used to uniquely identify elements in a list, allowing React to optimize rendering by updating and reordering items more efficiently.
  - Without unique keys, React might re-render elements unnecessarily, causing performance problems and potential bugs.

  ```javascript
  {
    items.map((item) => <ListItem key={item.id} value={item.value} />);
  }
```
## What is the consequence of using array indices as keys in React?
 - Using array indices as keys can lead to performance issues and unexpected behavior,
 - Especially when reordering or deleting items.
 - React relies on keys to identify elements uniquely, and using indices can cause components to be re-rendered unnecessarily or display incorrect data.

## What are props in React? How are they different from state?
- Props (short for properties) are inputs to React components that allow you to pass data from a parent component to a child component.
- They are immutable and are used to configure a component/They are read-only (immutable inside the child).
- Props let components be reusable and dynamic.
- In contrast, state is internal to a component and can change over time, typically due to user interactions or other events.

```javascript
  function Greeting(props) {
  return <h1>Hello, {props.name}!</h1>;
}

function App() {
  return (
    <div>
      <Greeting name="Satheesh" />
      <Greeting name="Alex" />
    </div>
  );
}

export default App;
```
## What is the difference between React's class components and functional components?
 - Class components are ES6 classes that extend React.Component and can hold state, lifecycle methods, and other features.
 - Functional components are simpler, functional JavaScript components that take props as input and return JSX. With the introduction of hooks, functional components can now manage state and lifecycle methods, making them more versatile.
 ### Which Should You Use?
 - Use Functional Components ✅
 - Modern React (post 16.8) encourages using them.
 - Hooks provide all class features (and more).

## When should you use a class component over a function component?
 - You’re maintaining or extending an older React codebase that already uses classes.
 - You’re using very old versions of React (<16.8) where Hooks don’t exist.
 - Before React 16.8, functional components couldn’t hold state. Developers used class components with this.state.
```javascript
   class Counter extends React.Component {
  constructor(props) {
    super(props);
    this.state = { count: 0 };
  }

  increment = () => {
    this.setState({ count: this.state.count + 1 });
  };

  render() {
    return (
      <div>
        <p>Count: {this.state.count}</p>
        <button onClick={this.increment}>+1</button>
      </div>
    );
  }
}
  ```
## React Fiber
 - React Fiber is a complete rewrite of the React core algorithm,
 - designed to improve performance and enable new features like async rendering, error boundaries, and incremental rendering.
 - It breaks down the rendering process into smaller chunks, allowing React to pause, abort, or prioritize updates as needed.

## What is reconciliation?
- Reconciliation is the process by which React updates the DOM to match the virtual DOM efficiently.
- It involves comparing the new virtual DOM tree with the previous one and determining the minimum number of changes required to update the actual DOM.

  <img width="1920" height="1024" alt="image" src="https://github.com/user-attachments/assets/41491747-b908-44b0-bcd4-0917d50c5d45" />

## Shadow DOM and Virtual DOM?
- Virtual DOM → A smart copy of the page that makes updates faster by only changing what’s needed.
- Shadow DOM → A mini secret DOM inside a component that keeps its styles and structure private. ✅ Result: Encapsulation — safer, reusable components without CSS conflicts.

## What is the difference between Controlled and Uncontrolled React components?
- In controlled components, form data is managed through the component's state, making it the definitive source of truth. Input value changes are handled by event handlers.
- In uncontrolled components, the form state is managed internally and accessed via refs.
-  Controlled components provide more control and are easier to test, while uncontrolled components are simpler for basic use cases.

```javascript
 function ControlledInput() {
  const [value, setValue] = React.useState('');
  return (
    <input
      type="text"
      value={value}
      onChange={(e) => setValue(e.target.value)}
    />
  );
}

```

```javascript
function UncontrolledInput() {
  const inputRef = React.useRef();
  return <input type="text" ref={inputRef} />;
}
```
## Pure Components:
- Pure Components in React are components that only re-render when their props or state change. 
- They use shallow comparison to check if the props or state have changed, preventing unnecessary re-renders and improving performance.
- Class components can extend React.PureComponent to become pure
- Functional components can use React.memo for the same effect

```javascript
  const PureFunctionalExample = React.memo(function ({ value }) {
  return <div>{value}</div>;
});
  ```
##  What is the difference between createElement and cloneElement?
createElement:
 - Used to create a new React element.
 - It takes the type of the element (e.g., 'div', a React component), props, and children, and returns a new React element.
 - Commonly used internally by JSX or when dynamically creating elements. Example:
 - React.createElement('div', { className: 'container' }, 'Hello World');
   ##
cloneElement:
 - Used to clone an existing React element and optionally modify its props.
 - It allows you to clone a React element and pass new props or override the existing ones, keeping the original element's children and state.
 - Useful when you want to manipulate an element without recreating it. Example:
```javascript
 const element = <button className="btn">Click Me</button>;
 const clonedElement = React.cloneElement(element, { className: 'btn-primary' });
  ```

## What is the role of PropTypes in React?
 - PropTypes in React is used for type-checking props passed to components,
 - ensuring the correct data types are used and warning developers during development.
```javascript
import PropTypes from 'prop-types';

function MyComponent({ name, age }) {
  return (
    <div>
      {name} is {age} years old
    </div>
  );
}

MyComponent.propTypes = {
  name: PropTypes.string.isRequired,
  age: PropTypes.number.isRequired,
};

  ```
- It helps catch errors early, improving code quality. 
- However, modern React components tend to be written in TypeScript and prop type mismatches can be caught at compile time rather than at runtime.
  
## What are stateless components?
- Stateless components in React are components that do not manage or hold any internal state. They simply receive data via props and render UI based on that data. These components are often functional components and are used for presentational purposes.
```javascript
  function StatelessComponent({ message }) {
  return <div>{message}</div>;
}
 ```

## stateful components
- Stateful components in React are components that manage and hold their own internal state.
- They can modify their state in response to user interactions or other events and re-render themselves when the state changes.
```javascript
  function StatefulComponent() {
  const [count, setCount] = React.useState(0);

  return (
    <div>
      <p>{count}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
    </div>
  );
}
 ```

## What are the recommended ways for type checking of React component props?

- TypeScript: A superset of JavaScript that adds optional static typing. It provides strong type checking, autocompletion, and other benefits at development time.

```javascript
  interface MyComponentProps {
  name: string;
  age: number;
}

function MyComponent({ name, age }: MyComponentProps) {
  return <div>{name} is {age} years old</div>;
}
```
- PropTypes: A runtime type-checking tool for React props, primarily for development purposes. It helps catch errors by checking prop types during development but doesn't offer full static analysis like TypeScript.

```javascript
  import PropTypes from 'prop-types';

function MyComponent({ name, age }) {
  return (
    <div>
      {name} is {age} years old
    </div>
  );
}

MyComponent.propTypes = {
  name: PropTypes.string.isRequired,
  age: PropTypes.number.isRequired,
};
 ```

## Why does React recommend against mutating state?
 - React advises against mutating state as it can lead to unexpected behaviors and bugs.
 - State immutability helps efficiently determine when components need re-rendering;
 - direct mutations may prevent React from detecting changes.

# React Hooks
- Mastering React hooks is important in front end interviews because hooks are the standard way to manage state, side effects, and component lifecycle in modern React.
- Demonstrating a solid understanding of hooks shows you can write clean, functional components and solve complex problems without relying on outdated class patterns.

## What are the benefits of using hooks in React?
- Hooks enable the use of state and other React features in functional components, replacing the need for class components.
- They streamline code by reducing the reliance on lifecycle methods, enhance readability, and facilitate the reuse of stateful logic across components.
- Popular hooks like useState and useEffect are used for managing state and side effects.

## What are the rules of React hooks
- React hooks should be called at the top level of a function, not inside loops, conditions, or nested functions.
- They must only be used within React function components or custom hooks. These guidelines ensure proper state management and lifecycle behavior.

## What is the difference between useEffect and useLayoutEffect in React?
 - useEffect and useLayoutEffect both handle side effects in React functional components but differ in when they run:
 - useEffect runs asynchronously after the DOM has rendered, making it suitable for tasks like data fetching or subscriptions.
 - useLayoutEffect runs synchronously after DOM updates but before the browser paints, ideal for tasks like measuring DOM elements or aligning the UI with the DOM. Example:

``` javascript
import React, { useEffect, useLayoutEffect, useRef } from 'react';

function Example() {
  const ref = useRef();

  useEffect(() => {
    console.log('useEffect: Runs after DOM paint');
  });

  useLayoutEffect(() => {
    console.log('useLayoutEffect: Runs before DOM paint');
    console.log('Element width:', ref.current.offsetWidth);
  });

  return <div ref={ref}>Hello</div>;
}

 ```
## What does the dependency array of useEffect affect?
- The dependency array of useEffect controls when the effect re-runs:

 - If it's empty, the effect runs only once after the initial render.
 - If it contains variables, the effect re-runs whenever any of those variables change.
 - If omitted, the effect runs after every render.

## What is the useRef hook in React and when should it be used?
 - The useRef hook creates a mutable object that persists through renders,
 - allowing direct access to DOM elements, storing mutable values without causing re-renders, and maintaining references to values.

  ```javascript
   import React, { useRef, useEffect } from 'react';

     function TextInputWithFocusButton() {
       const inputEl = useRef(null);
       useEffect(() => {
         inputEl.current.focus();
       }, []);
       return <input ref={inputEl} type="text" />;
     }
  ```

## What is the purpose of callback function argument format of setState() in React class components and when should it be used?
- The callback function format of setState() in React ensures that state updates are based on the most current state and props.
- This is essential when the new state depends on the previous state. Instead of passing an object directly to setState(), you provide a function that takes the previous state and props as arguments, returning the updated state.

```javascript
 this.setState((prevState, props) => ({
  counter: prevState.counter + props.increment,
}));
 ```
## What is the useCallback hook in React and when should it be used?

 - The useCallback hook memoizes functions to prevent their recreation on every render.
 - This is especially beneficial when passing callbacks to optimized child components that depend on reference equality to avoid unnecessary renders.
 - Use it when a function is passed as a prop to a child component.
```javascript
   const memoizedCallback = useCallback(() => {
   doSomething(a, b);
  }, [a, b]);
```
## What is the useMemo hook in React and when should it be used?
- The useMemo hook memoizes costly calculations, recomputing them only when dependencies change.
- This enhances performance by avoiding unnecessary recalculations.
- It should be used for computationally intensive functions that don't need to run on every render.
  const memoizedValue = useMemo(() => computeExpensiveValue(a, b), [a, b]);

## Real-World Examples
  
- useMemo

  - Sorting, filtering, or mapping large datasets.

  - Complex mathematical computations (e.g., analytics dashboards).

  - Formatting expensive values (e.g., currency formatting thousands of items).

- useCallback

  - Passing event handlers to child components that are memoized (React.memo).

  - Optimizing lists where each item has buttons or handlers.

  - Preventing unnecessary renders in components using callbacks as props.
 
  ## What is the useReducer hook in React and when should it be used?

  - The useReducer hook manages complex state logic in functional components, serving as an alternative to useState.
  - It's ideal when state has multiple fields (and there are constraints around how they should be mutated), or when the next state relies on the previous one.

  - The useReducer hook accepts a reducer function + an initial state. The reducer function is passed the current state and action and returns a new state.

  - const [state, dispatch] = useReducer(reducer, initialState);
 ```javascript
  import React, { useReducer } from "react";

// 1. Define the initial state
const initialState = {
  cart: [],
  total: 0,
};

// 2. Define the reducer function
function cartReducer(state, action) {
  switch (action.type) {
    case "ADD_ITEM":
      return {
        ...state,
        cart: [...state.cart, action.payload],
        total: state.total + action.payload.price,
      };

    case "REMOVE_ITEM":
      const updatedCart = state.cart.filter((_, i) => i !== action.index);
      const removedItem = state.cart[action.index];
      return {
        ...state,
        cart: updatedCart,
        total: state.total - removedItem.price,
      };

    case "CLEAR_CART":
      return initialState;

    default:
      return state;
  }
}

// 3. Use the reducer in a component
function ShoppingCart() {
  const [state, dispatch] = useReducer(cartReducer, initialState);

  const addItem = (item) => {
    dispatch({ type: "ADD_ITEM", payload: item });
  };

  return (
    <div>
      <h2>Shopping Cart</h2>
      <button onClick={() => addItem({ name: "Laptop", price: 1000 })}>
        Add Laptop
      </button>
      <button onClick={() => addItem({ name: "Phone", price: 500 })}>
        Add Phone
      </button>
      <button onClick={() => dispatch({ type: "CLEAR_CART" })}>
        Clear Cart
      </button>

      <h3>Items:</h3>
      <ul>
        {state.cart.map((item, index) => (
          <li key={index}>
            {item.name} - ${item.price}
            <button onClick={() => dispatch({ type: "REMOVE_ITEM", index })}>
              Remove
            </button>
          </li>
        ))}
      </ul>

      <h3>Total: ${state.total}</h3>
    </div>
  );
}

export default ShoppingCart;

   ```

## What is the useId hook in React and when should it be used?

- The useId hook generates unique IDs for elements within a component, which is crucial for accessibility by dynamically creating ids that can be used for linking form inputs and labels. - - It guarantees unique IDs across the application even if the component renders multiple times.
```javascript
  import { useId } from 'react';

function MyComponent() {
  const id = useId();

  return (
    <div>
      <label htmlFor={id}>Name:</label>
      <input id={id} type="text" />
    </div>
  );
}
 ```

## Can you explain how to create and use custom hooks in React?

- To create and use custom hooks in React:

- Create a function that starts with use and uses built-in hooks like useState or useEffect
 Return the values or functions you want to share.
Example:
```javascript
function useForm(initialState) {
  const [formData, setFormData] = useState(initialState);
  const handleChange = (e) =>
    setFormData({ ...formData, [e.target.name]: e.target.value });
  return [formData, handleChange];
}

function MyForm() {
  const [formData, handleChange] = useForm({ name: '', email: '' });
  return <input name="name" value={formData.name} onChange={handleChange} />;
}
 ```

## error boundaries in React for
 - Error boundaries are a feature in React that help manage and handle errors in a more graceful way.
 - How to implement error boundaries
  - static getDerivedStateFromError(error): This method is used to update the state so the next render will show the fallback UI.
  - componentDidCatch(error, info): This method is used to log error information.
```javascript
    import React, { Component } from 'react';

class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error) {
    // Update state so the next render shows the fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // You can also log the error to an error reporting service
    console.error('Error caught by ErrorBoundary: ', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      // You can render any custom fallback UI
      return <h1>Something went wrong.</h1>;
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
 ```
Limitations
Error boundaries have some limitations:

- They do not catch errors inside event handlers. For event handlers, you need to use regular JavaScript try/catch blocks.
- They do not catch errors in asynchronous code (e.g., setTimeout or requestAnimationFrame callbacks).
- They do not catch errors during server-side rendering.
- They do not catch errors thrown in the error boundary itself.
## What is React Suspense?
- React Suspense allows handling asynchronous operations more elegantly within components. 
- It provides fallback content while waiting for resources like data or code to load. You can use it alongside React.lazy() for code splitting.

## Explain prop drilling

Prop drilling is when you pass data from a parent component to a deeply nested child component through props, even if intermediate components don't use it.
```javascript
function Grandparent() {
  const data = 'Hello from Grandparent';
  return <Parent data={data} />;
}

function Parent({ data }) {
  return <Child data={data} />;
}

function Child({ data }) {
  return <p>{data}</p>;
}
```

## Lazy Loading 
- Lazy loading in React is a technique where components are loaded only when they are needed, rather than at the initial page load.
- This helps reduce the initial load time and improve performance by splitting the code into smaller chunks.
```javascript
import React, { Suspense, lazy } from 'react';

const LazyComponent = lazy(() => import('./LazyComponent'));

function App() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <LazyComponent />
    </Suspense>
  );
}
 ```

## React component lifecycle methods 

- Mounting:
  - constructor: Initializes state or binds methods
  - componentDidMount: Runs after the component mounts, useful for API calls or subscriptions

 Updating:
   - shouldComponentUpdate: Determines if the component should re-render
   - componentDidUpdate: Runs after updates, useful for side effects

 Unmounting:
  - componentWillUnmount: Cleans up (e.g., removing event listeners).


## React Router
- React Router is a popular routing library for React applications that enables navigation between different components based on the URL.
- It provides declarative routing, allowing you to define routes and their corresponding components in a straightforward manner.

## How does React Router work, and how do you implement dynamic routing?
 - React Router maps URL paths to components, enabling navigation in single-page apps.
 -  Dynamic routing allows you to use URL parameters to render components based on dynamic values.
```javascript
   import { BrowserRouter, Routes, Route, useParams } from 'react-router-dom';

function UserPage() {
  const { id } = useParams(); // Access dynamic parameter
  return <h1>User ID: {id}</h1>;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/user/:id" element={<UserPage />} /> {/* Dynamic path */}
      </Routes>
    </BrowserRouter>
  );
}
 ```

What is the difference between BrowserRouter and HashRouter?
BrowserRouter: Uses the HTML5 History API to manage navigation, enabling clean URLs without the hash (#). It requires server-side configuration to handle routes correctly, especially for deep linking.

HashRouter: Uses the hash (#) portion of the URL to simulate navigation. It doesn't require server-side configuration, as the hash is never sent to the server. This makes it suitable for environments where server-side routing isn't possible (e.g., static hosting).


https://www.youtube.com/watch?v=nI8PYZNFtac

  

   

