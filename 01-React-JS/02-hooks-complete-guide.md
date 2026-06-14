# 📘 React Hooks — Detailed Guide with Flowchart

---

## 🔹 1) What are React Hooks?

- **Hooks** are special functions introduced in **React 16.8**.  
- They let you **use state, lifecycle methods, and other React features** in **functional components** (without writing classes).  
- Hooks provide a **simpler, cleaner way** to manage component logic and side effects.

---

## 🔹 2) Why Hooks?

- Before hooks: only **class components** could use state & lifecycle.  
- After hooks: **functional components** can handle everything.  
- Benefits:  
  - Reuse logic easily (custom hooks).  
  - Cleaner code (no `this`).  
  - Easier testing & composition.

---

## 🔹 3) Types of Hooks in React

### ✅ Basic Hooks
| Hook              | Purpose |
|-------------------|---------|
| **`useState`**    | Add state to functional components. |
| **`useEffect`**   | Side effects (API calls, subscriptions, DOM changes). |
| **`useContext`**  | Consume context values without `Consumer`. |

### ✅ Additional Hooks
| Hook              | Purpose |
|-------------------|---------|
| **`useReducer`**  | Alternative to `useState` for complex state logic. |
| **`useCallback`** | Memoize functions, avoid unnecessary re-creations. |
| **`useMemo`**     | Memoize values, optimize performance-heavy calculations. |
| **`useRef`**      | Access/manipulate DOM elements or persist mutable values. |
| **`useImperativeHandle`** | Customize ref handling in child components. |
| **`useLayoutEffect`** | Like `useEffect`, but fires synchronously after DOM mutations. |
| **`useDebugValue`** | Debugging info for custom hooks. |

### ✅ Custom Hooks
- **Your own hooks** built using existing hooks.  
- Naming convention: starts with `use`.  
- Example: `useFetch`, `useAuth`.

---

## 🔹 4) When to Use What?

- **`useState`** → Simple local state (counter, toggle).  
- **`useReducer`** → Complex state with multiple transitions.  
- **`useEffect`** → Data fetching, subscriptions, DOM changes.  
- **`useContext`** → Share data globally (theme, auth, language).  
- **`useMemo`** → Expensive calculations, optimize re-renders.  
- **`useCallback`** → Prevent re-creation of functions passed as props.  
- **`useRef`** → Access DOM nodes, persist values across renders without triggering re-render.  
- **`useLayoutEffect`** → Measure DOM size/layout before browser paint.  
- **Custom Hooks** → Reuse stateful logic across components.

---

## 🔹 5) React Hooks Workflow (Flowchart)

```text
 ┌──────────────────────┐
 │   Component Render   │
 └──────────┬───────────┘
            │
            ▼
 ┌──────────────────────┐
 │ useState()           │
 │ - Initialize state   │
 │ - Returns [state, setState] │
 └──────────┬───────────┘
            │
            ▼
 ┌──────────────────────┐
 │ useEffect()          │
 │ - Runs after render  │
 │ - Handles side effects │
 └──────────┬───────────┘
            │
            ▼
 ┌──────────────────────┐
 │ useContext()         │
 │ - Consume context    │
 └──────────┬───────────┘
            │
            ▼
 ┌──────────────────────┐
 │ Advanced Hooks       │
 │ - useReducer, useMemo │
 │ - useCallback, useRef │
 └──────────┬───────────┘
            │
            ▼
 ┌──────────────────────┐
 │ Custom Hooks         │
 │ - Build reusable     │
 │   logic with hooks   │
 └──────────────────────┘
```

---

## 🔑 Key Takeaways
- Hooks bring **state & lifecycle** to functional components.  
- Start with **basic hooks** (`useState`, `useEffect`).  
- Use **additional hooks** for optimization and complex state.  
- Write **custom hooks** for reusable logic.  
- Follow **rules of hooks**:  
  - Call only at the top level.  
  - Call only inside React functions.
 
  
# React Hooks — Live Examples (Concise)

> A practical, example-first guide to the most-used React hooks. Each section includes a **what/when** and a **live-style** example you can drop into a React app.

**Scope:** Focused on stable, commonly used hooks: `useState`, `useEffect`, `useLayoutEffect`, `useMemo`, `useCallback`, `useRef`, `useId`, `useContext`, `useReducer`, `useImperativeHandle`, `useDebugValue`, `useTransition`, `useDeferredValue`, `useSyncExternalStore`, `useInsertionEffect`.

---

## 1) `useState` — Local component state
**Use when:** A component needs to hold simple, local state (inputs, toggles, counters).

```jsx
import { useState } from "react";

export default function Counter() {
  const [count, setCount] = useState(0);
  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => setCount(c => c + 1)}>+1</button>
      <button onClick={() => setCount(0)}>Reset</button>
    </div>
  );
}
```

---

## 2) `useEffect` — Side effects after paint
**Use when:** Syncing with the outside world: fetch, subscriptions, timers, title updates. Runs after the component renders/paints.

```jsx
import { useEffect, useState } from "react";

export default function UserName() {
  const [name, setName] = useState("loading...");
  useEffect(() => {
    let alive = true;
    (async () => {
      const res = await fetch("/api/me");
      if (!alive) return;
      const data = await res.json();
      setName(data.name);
    })();
    return () => { alive = false }; // cleanup on unmount
  }, []);
  return <h1>Hello, {name}</h1>;
}
```

---

## 3) `useLayoutEffect` — Synchronous DOM effect before paint
**Use when:** You must measure DOM/layout and synchronously adjust before the browser paints (avoid flicker).

```jsx
import { useLayoutEffect, useRef } from "react";

export default function AutoFocusInput() {
  const ref = useRef(null);
  useLayoutEffect(() => {
    ref.current?.focus(); // focus before paint
  }, []);
  return <input ref={ref} placeholder="Auto focused" />;
}
```

---

## 4) `useMemo` — Memoize expensive computations
**Use when:** Deriving a value that's expensive to compute or stable across renders.

```jsx
import { useMemo, useState } from "react";

function fib(n) {
  if (n <= 1) return n;
  return fib(n - 1) + fib(n - 2);
}

export default function ExpensiveCalc() {
  const [n, setN] = useState(35);
  const value = useMemo(() => fib(n), [n]);
  return (
    <div>
      <input type="number" value={n} onChange={e => setN(+e.target.value)} />
      <p>fib({n}) = {value}</p>
    </div>
  );
}
```

---

## 5) `useCallback` — Stable function identity
**Use when:** Passing callbacks to memoized children or deps arrays to avoid unnecessary re-renders.

```jsx
import { useCallback, useState, memo } from "react";

const AddButton = memo(function AddButton({ onAdd }) {
  console.log("Button render");
  return <button onClick={() => onAdd(1)}>Add 1</button>;
});

export default function Cart() {
  const [qty, setQty] = useState(0);
  const handleAdd = useCallback((n) => setQty(q => q + n), []);
  return (
    <div>
      <p>Quantity: {qty}</p>
      <AddButton onAdd={handleAdd} />
    </div>
  );
}
```

---

## 6) `useRef` — Mutable container / DOM ref
**Use when:** Access a DOM node or store a mutable value that persists across renders without causing re-renders.

```jsx
import { useRef, useState } from "react";

export default function Stopwatch() {
  const [ms, setMs] = useState(0);
  const timer = useRef(null);
  const start = () => {
    if (timer.current) return;
    const t0 = performance.now();
    timer.current = requestAnimationFrame(function tick(now) {
      setMs(now - t0);
      timer.current = requestAnimationFrame(tick);
    });
  };
  const stop = () => {
    if (timer.current) cancelAnimationFrame(timer.current);
    timer.current = null;
  };
  return (
    <div>
      <p>Elapsed: {ms.toFixed(0)}ms</p>
      <button onClick={start}>Start</button>
      <button onClick={stop}>Stop</button>
    </div>
  );
}
```

---

## 7) `useId` — Stable unique IDs for accessibility
**Use when:** Labeling form fields (SSR-friendly).

```jsx
import { useId } from "react";

export default function LabeledInput() {
  const id = useId();
  return (
    <div>
      <label htmlFor={id}>Email</label>
      <input id={id} type="email" placeholder="you@example.com" />
    </div>
  );
}
```

---

## 8) `useContext` — Read from a Context
**Use when:** Avoid prop drilling for globally needed values (theme, auth).

```jsx
import { createContext, useContext, useState } from "react";

const ThemeContext = createContext();

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState("light");
  const toggle = () => setTheme(t => (t === "light" ? "dark" : "light"));
  return (
    <ThemeContext.Provider value={{ theme, toggle }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function ThemeButton() {
  const { theme, toggle } = useContext(ThemeContext);
  return <button onClick={toggle}>Theme: {theme}</button>;
}
```

---

## 9) `useReducer` — Complex state transitions
**Use when:** Multiple related state changes or when state logic is non-trivial.

```jsx
import { useReducer } from "react";

function reducer(state, action) {
  switch (action.type) {
    case "add": return [...state, { id: crypto.randomUUID(), text: action.text }];
    case "remove": return state.filter(t => t.id !== action.id);
    default: return state;
  }
}

export default function Todos() {
  const [todos, dispatch] = useReducer(reducer, []);
  let input;
  return (
    <div>
      <input ref={el => (input = el)} placeholder="New todo" />
      <button onClick={() => dispatch({ type: "add", text: input.value })}>Add</button>
      <ul>
        {todos.map(t => (
          <li key={t.id}>
            {t.text} <button onClick={() => dispatch({ type: "remove", id: t.id })}>x</button>
          </li>
        ))}
      </ul>
    </div>
  );
}
```

---

## 10) `useImperativeHandle` (+ `forwardRef`) — Expose an imperative API
**Use when:** A parent needs limited imperative control over a child (e.g., `focus()`).

```jsx
import { forwardRef, useImperativeHandle, useRef } from "react";

const FancyInput = forwardRef(function FancyInput(_, ref) {
  const inputRef = useRef(null);
  useImperativeHandle(ref, () => ({
    focus: () => inputRef.current?.focus(),
    clear: () => (inputRef.current.value = ""),
  }));
  return <input ref={inputRef} placeholder="Fancy input" />;
});

export default function Form() {
  const ref = useRef(null);
  return (
    <div>
      <FancyInput ref={ref} />
      <button onClick={() => ref.current.focus()}>Focus</button>
      <button onClick={() => ref.current.clear()}>Clear</button>
    </div>
  );
}
```

---

## 11) `useDebugValue` — Label custom hook state in DevTools
**Use when:** Building custom hooks; provide readable debug info.

```jsx
import { useDebugValue, useState, useEffect } from "react";

function useOnlineStatus() {
  const [online, setOnline] = useState(navigator.onLine);
  useEffect(() => {
    const on = () => setOnline(true);
    const off = () => setOnline(false);
    window.addEventListener("online", on);
    window.addEventListener("offline", off);
    return () => {
      window.removeEventListener("online", on);
      window.removeEventListener("offline", off);
    };
  }, []);
  useDebugValue(online ? "Online" : "Offline");
  return online;
}

export default function Status() {
  const online = useOnlineStatus();
  return <p>Status: {online ? "Online" : "Offline"}</p>;
}
```

---

## 12) `useTransition` — Mark updates as non-urgent
**Use when:** Split urgent (typing) vs. non-urgent (filtering large lists) updates.

```jsx
import { useState, useTransition } from "react";

const items = Array.from({ length: 5000 }, (_, i) => `Item ${i}`);

export default function FilteredList() {
  const [query, setQuery] = useState("");
  const [list, setList] = useState(items);
  const [isPending, startTransition] = useTransition();

  function onChange(e) {
    const q = e.target.value;
    setQuery(q); // urgent
    startTransition(() => { // non-urgent
      const next = items.filter(i => i.toLowerCase().includes(q.toLowerCase()));
      setList(next);
    });
  }

  return (
    <div>
      <input value={query} onChange={onChange} placeholder="Filter..." />
      {isPending && <p>Updating…</p>}
      <ul>{list.map(i => <li key={i}>{i}</li>)}</ul>
    </div>
  );
}
```

---

## 13) `useDeferredValue` — Defer a derived value
**Use when:** Keep input responsive by deferring heavy computations/display.

```jsx
import { useDeferredValue, useMemo, useState } from "react";

const big = Array.from({ length: 3000 }, (_, i) => `Row ${i}`);

function Table({ query }) {
  const deferred = useDeferredValue(query);
  const rows = useMemo(
    () => big.filter(r => r.toLowerCase().includes(deferred.toLowerCase())),
    [deferred]
  );
  return <ul>{rows.map(r => <li key={r}>{r}</li>)}</ul>;
}

export default function Search() {
  const [q, setQ] = useState("");
  return (
    <div>
      <input value={q} onChange={e => setQ(e.target.value)} placeholder="Search..." />
      <Table query={q} />
    </div>
  );
}
```

---

## 14) `useSyncExternalStore` — Subscribe to external stores
**Use when:** Integrate with Redux-like or custom stores safely with concurrent rendering.

```jsx
import { useSyncExternalStore } from "react";

// A tiny store
const store = (() => {
  let state = { count: 0 };
  let listeners = new Set();
  return {
    getSnapshot: () => state,
    subscribe: (cb) => { listeners.add(cb); return () => listeners.delete(cb); },
    inc: () => { state = { count: state.count + 1 }; listeners.forEach(l => l()); },
  };
})();

export default function ExternalCounter() {
  const snapshot = useSyncExternalStore(
    store.subscribe,
    store.getSnapshot,
    store.getSnapshot // server snapshot
  );
  return (
    <div>
      <p>Count: {snapshot.count}</p>
      <button onClick={store.inc}>+1</button>
    </div>
  );
}
```

---

## 15) `useInsertionEffect` — Inject styles before layout
**Use when:** CSS-in-JS libraries need to insert styles before layout to avoid flicker. Use sparingly.

```jsx
import { useInsertionEffect, useState } from "react";

function useDynamicStyle(cssText) {
  useInsertionEffect(() => {
    const style = document.createElement("style");
    style.textContent = cssText;
    document.head.appendChild(style);
    return () => { document.head.removeChild(style); };
  }, [cssText]);
}

export default function ColorBox() {
  const [color, setColor] = useState("#88c");
  useDynamicStyle(`.box { width: 120px; height: 60px; background: ${color}; }`);
  return (
    <div>
      <div className="box" />
      <input type="color" value={color} onChange={e => setColor(e.target.value)} />
    </div>
  );
}
```

---

## When to pick which?
- **Local UI value?** `useState` → maybe `useReducer` if complex.
- **Side effects?** `useEffect` (layout-sensitive? `useLayoutEffect`).
- **Performance?** `useMemo`/`useCallback` for expensive derivations and stable identities.
- **External data/store?** `useSyncExternalStore` (or your library’s hook).
- **Smooth interactions?** `useTransition` / `useDeferredValue`.
- **DOM or mutable ref?** `useRef`. Expose API? `useImperativeHandle`.
- **Global values?** `useContext` (+ custom hooks).

---

## Tips
- Keep renders **pure** (no side effects in render).
- Effects should handle **subscribe → cleanup** patterns.
- Don’t overuse `useMemo`/`useCallback`; measure first.
- Derive state when possible; avoid duplicating sources of truth.



---

## Additional Hook Details


## what it is (useRef)
The useRef hook is a powerful tool in React that often flies under the radar for many developers. While its primary purpose is to reference a DOM element, it can also be used to persist values across renders without causing a re-render. This article will explore various real-life examples of how useRef can be effectively utilized in your React projects.

## What is useRef?
The useRef hook returns a mutable object with a .current property that you can use to store a value. Unlike useState, updating a useRef value does not trigger a component re-render. Here’s a basic example:
```jsx
import React, { useRef } from 'react';

function FocusInput() {
  const inputRef = useRef(null);

  const focusInput = () => {
    inputRef.current.focus();
  };

  return (
    <div>
      <input ref={inputRef} type="text" />
      <button onClick={focusInput}>Focus Input</button>
    </div>
  );
}
```
In this example, the useRef hook is used to directly reference the input element, allowing us to call the focus() method on it when the button is clicked.

## Real-Life Examples of useRef
1. Accessing DOM Elements
One of the most common uses of useRef is to directly access and manipulate DOM elements. This can be particularly useful for integrating with third-party libraries or handling complex UI interactions.
```jsx
import React, { useRef, useEffect } from 'react';

function VideoPlayer() {
  const videoRef = useRef(null);

  useEffect(() => {
    // Automatically play the video when the component mounts
    videoRef.current.play();
  }, []);

  return (
    <div>
      <video ref={videoRef} width="600" controls>
        <source src="video.mp4" type="video/mp4" />
        Your browser does not support the video tag.
      </video>
    </div>
  );
}
```

In this example, the useRef hook is used to access a video element, allowing us to programmatically control the video playback.


2. Persisting Values Across Renders
Sometimes, you need to persist a value across renders without triggering a re-render. This is where useRef comes in handy.

```jsx
import React, { useState, useRef } from 'react';

function Counter() {
  const [count, setCount] = useState(0);
  const renderCount = useRef(0);

  useEffect(() => {
    renderCount.current++;
  });

  return (
    <div>
      <p>Count: {count}</p>
      <p>This component has re-rendered {renderCount.current} times</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
    </div>
  );
}
```
Here, useRef is used to track how many times the component has re-rendered. Unlike state, updating renderCount.current does not cause the component to re-render.

3. Storing Previous State Values
You might want to compare the current state with the previous one. useRef allows you to store the previous state value without triggering a re-render.

```jsx
import React, { useState, useEffect, useRef } from 'react';

function PreviousStateExample() {
  const [name, setName] = useState('John');
  const prevNameRef = useRef('');

  useEffect(() => {
    prevNameRef.current = name;
  }, [name]);

  return (
    <div>
      <p>Current Name: {name}</p>
      <p>Previous Name: {prevNameRef.current}</p>
      <input
        type="text"
        value={name}
        onChange={(e) => setName(e.target.value)}
      />
    </div>
  );
}
```
In this example, useRef is used to store the previous value of the name state, allowing us to compare it with the current value.

4. Avoiding Re-Initialization of Expensive Calculations
When dealing with expensive calculations or initializations, you might want to store the result and reuse it across renders. useRef is perfect for this use case.

```jsx
import React, { useRef } from 'react';

function ExpensiveCalculationComponent() {
  const expensiveValue = useRef(calculateExpensiveValue());

  function calculateExpensiveValue() {
    console.log('Calculating expensive value...');
    return 42; // Example value
  }

  return (
    <div>
      <p>Expensive Value: {expensiveValue.current}</p>
    </div>
  );
}
```
Here, calculateExpensiveValue is only called once, during the first render. The result is stored in useRef, preventing unnecessary recalculations on subsequent renders.

5. Managing SetTimeout and SetInterval
When dealing with setTimeout or setInterval, it’s often necessary to clear these timers when the component unmounts or when the timer is no longer needed. useRef can be used to store the timer ID.

```jsx
import React, { useState, useRef, useEffect } from 'react';

function Timer() {
  const [count, setCount] = useState(0);
  const timerRef = useRef(null);

  useEffect(() => {
    timerRef.current = setInterval(() => {
      setCount((prevCount) => prevCount + 1);
    }, 1000);

    return () => {
      clearInterval(timerRef.current);
    };
  }, []);

  return <p>Count: {count}</p>;
}
```
