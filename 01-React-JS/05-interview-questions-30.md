
# 30 Essential React Hooks Interview Questions (Enterprise-Level, Elaborated with Examples)
https://www.greatfrontend.com/blog/30-essential-react-hooks-interview-questions-you-must-know


This document covers **React hooks interview questions** with **detailed answers, enterprise-level use cases, and code snippets**.

---

## 1. What are React hooks?
- Hooks are special functions that let you "hook into" React features (state, lifecycle, context) from functional components.  
- They replace class components for most use cases.  
- Promote cleaner, functional programming patterns.

**Enterprise Example:**  
- A banking dashboard component uses `useState` to manage account filters, `useEffect` to fetch data, and `useContext` for global auth.

```jsx
function AccountList() {
  const [accounts, setAccounts] = useState([]);
  const { token } = useContext(AuthContext);

  useEffect(() => {
    fetch("/api/accounts", { headers: { Authorization: token } })
      .then(res => res.json())
      .then(setAccounts);
  }, [token]);

  return accounts.map(acc => <div>{acc.name}</div>);
}
```

---

## 2. Benefits of using hooks
- Simplify codebase (no need for classes).  
- Reusable logic via **custom hooks**.  
- Improved readability and separation of concerns.  
- Easier to share logic between teams in enterprise-scale apps.

**Enterprise Example:**  
Custom `useApi` hook for consistent API requests across a large SaaS product.

---

## 3. Rules of React hooks
- Call hooks **only at the top level** of components/custom hooks.  
- Call hooks **only in React function components or custom hooks**.  
- Helps React maintain proper order of hook calls.

**Enterprise Example:**  
A linting setup in enterprise projects enforces hook rules to prevent inconsistent behavior.

---

## 4. Purpose of `useState`
- Manages component-local state.  
- Returns `[value, setter]`.  

**Enterprise Example:**  
CRM system: track “current customer tab” state.

```jsx
const [tab, setTab] = useState("overview");
```

---

## 5. How `useState` triggers re-renders
- Setter triggers re-render.  
- React reconciles new VDOM with old → updates only necessary nodes.

**Enterprise Example:**  
In an e-commerce site, changing product quantity updates only that item row, not the entire cart.

---

## 6. Passing function to setter
- Useful when new state depends on previous state.

```jsx
setCount(prev => prev + 1);
```

**Enterprise Example:**  
Real-time bidding app increments bid safely under concurrent updates.

---

## 7. How `useEffect` works
- Handles side effects: fetching, subscriptions, timers.  
- Runs after render.  

**Enterprise Example:**  
Analytics tool: log page views only once on mount.

```jsx
useEffect(() => {
  logEvent("PageView", { path: window.location.pathname });
}, []);
```

---

## 8. Dependency types in `useEffect`
- `[]`: once (mount).  
- `[dep]`: runs when dep changes.  
- none: runs every render.

**Enterprise Example:**  
Auto-refresh dashboard when `userId` changes.

---

## 9. Fetch data with `useEffect`
```jsx
useEffect(() => {
  fetch(`/api/orders/${userId}`)
    .then(res => res.json())
    .then(setOrders);
}, [userId]);
```

**Enterprise Example:**  
Retail ERP: fetch order details when selected user changes.

---

## 10. Cleanup in `useEffect`
- Return function to cleanup.

```jsx
useEffect(() => {
  const ws = new WebSocket("/ws");
  ws.onmessage = handleMsg;
  return () => ws.close();
}, []);
```

**Enterprise Example:**  
Stock trading app unsubscribes from WebSocket feed when leaving a page.

---

## 11. Purpose of `useRef`
- Stores mutable values or DOM references.  

**Enterprise Example:**  
Video conferencing tool stores `mediaStream` in `useRef` to avoid re-renders.

---

## 12. `useEffect` vs `componentDidMount`
- `useEffect([])`: functional equivalent of `componentDidMount`.  
- `componentDidMount`: class-based only.

**Enterprise Example:**  
Legacy migration from classes to hooks.

---

## 13. Purpose of `useContext`
- Share global state without prop drilling.  

**Enterprise Example:**  
Enterprise design system shares theme and language settings via context.

---

## 14. Purpose of `useId`
- Generate SSR-safe unique IDs.

**Enterprise Example:**  
Healthcare portal uses unique IDs for form fields for accessibility compliance.

---

## 15. Custom hooks
- Reuse logic like fetching, auth, form validation.  

**Enterprise Example:**  
`useAuth` hook centralizes JWT logic across all apps.

---

## 16. When useReducer over useState
- Use for complex state logic with multiple transitions.  

**Enterprise Example:**  
Workflow automation tool manages multi-step state via reducer.

---

## 17. Purpose of `useMemo`
- Cache expensive computations.  

**Enterprise Example:**  
Analytics dashboard: memoize aggregated report results to avoid recalculating.

---

## 18. Difference: `useMemo` vs `useCallback`
- `useMemo`: memoizes a value.  
- `useCallback`: memoizes a function.

**Enterprise Example:**  
- `useMemo` for filtered data in reports.  
- `useCallback` for stable event handlers passed to grids.

---

## 19. When to use `useMemo` vs `useCallback`
- Use `useMemo` for values.  
- Use `useCallback` for functions.

---

## 20. Purpose of `useImperativeHandle`
- Expose imperative methods for controlled components.

**Enterprise Example:**  
Enterprise form builder: child exposes `validate()` to parent.

---

## 21. Purpose of `useDeferredValue`
- Defer heavy updates for smoother UI.

**Enterprise Example:**  
Large HR system filters thousands of employees without freezing typing.

---

## 22. Purpose of `useTransition`
- Split urgent (typing) vs non-urgent (filter results).  

**Enterprise Example:**  
Enterprise search: keeps typing snappy while results update gradually.

---

## 23. Pitfalls of wrong dependencies
- Missing dep: stale values.  
- Extra dep: infinite loops.

**Enterprise Example:**  
Finance dashboards risk showing stale KPIs if deps missed.

---

## 24. `useEffect` vs `useLayoutEffect`
- `useEffect`: async after paint.  
- `useLayoutEffect`: sync before paint.

**Enterprise Example:**  
Data grid aligns sticky headers precisely with `useLayoutEffect`.

---

## 25. Purpose of `useCallback`
- Memoizes callback to avoid unnecessary renders.

**Enterprise Example:**  
Enterprise charts: pass stable handlers to chart library.

---

## 26. How `useReducer` works
- Uses reducer function and dispatch pattern.  

**Enterprise Example:**  
Task management system: actions like "ADD_TASK", "COMPLETE_TASK".

---

## 27. Purpose of `useLayoutEffect`
- Measure/adjust DOM synchronously.  

**Enterprise Example:**  
Design editor adjusts canvas size before painting.

---

## 28. Prevent unnecessary re-renders
- Use `React.memo`, `useCallback`, `useMemo`.  
- Normalize state structures.

**Enterprise Example:**  
CRM avoids re-rendering 10k row grid by memoizing row data.

---

## 29. When to use custom hooks
- For reusable, shared logic.  

**Enterprise Example:**  
`useApi` hook centralizes API request handling, retries, error handling across all modules.

---

## 30. Why use function form of setter?
- Guarantees update based on latest state.  

```jsx
setBalance(prev => prev + deposit);
```

**Enterprise Example:**  
Banking app ensures balance updates correctly under multiple simultaneous deposits.
