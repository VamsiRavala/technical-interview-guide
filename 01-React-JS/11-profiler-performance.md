# React Performance Optimization & Profiler

## Table of Contents
1. [Introduction](#introduction)
2. [React DevTools Profiler](#react-devtools-profiler)
3. [Performance Metrics](#performance-metrics)
4. [Identifying Bottlenecks](#identifying-bottlenecks)
5. [Memoization Techniques](#memoization-techniques)
6. [React 19 Compiler](#react-19-compiler)
7. [Code Splitting & Lazy Loading](#code-splitting--lazy-loading)
8. [Virtual Scrolling](#virtual-scrolling)
9. [Performance Optimization Checklist](#performance-optimization-checklist)
10. [Interview Questions](#interview-questions)

## Introduction

Performance optimization in React is crucial for delivering smooth user experiences. React 19 introduces the React Compiler that automatically optimizes your code, but understanding manual optimization techniques remains essential for complex scenarios.

## React DevTools Profiler

### Installing React DevTools

```bash
# Chrome/Firefox Extension
# Available in browser extension stores

# Standalone (for React Native)
npm install -g react-devtools
```

### Using the Profiler

The Profiler tab in React DevTools helps you visualize component render performance:

```jsx
import { Profiler } from 'react';

function onRenderCallback(
  id, // the "id" prop of the Profiler tree that has just committed
  phase, // either "mount" or "update"
  actualDuration, // time spent rendering the committed update
  baseDuration, // estimated time to render the entire subtree without memoization
  startTime, // when React began rendering this update
  commitTime, // when React committed this update
  interactions // the Set of interactions belonging to this update
) {
  console.log(`${id}'s ${phase} phase:`);
  console.log(`Actual duration: ${actualDuration}ms`);
  console.log(`Base duration: ${baseDuration}ms`);
}

function App() {
  return (
    <Profiler id="Navigation" onRender={onRenderCallback}>
      <Navigation />
      <Main />
    </Profiler>
  );
}
```

### Profiler Features

1. **Flame Graph**: Visual representation of component render hierarchy
2. **Ranked Chart**: Components sorted by render time
3. **Component Render Count**: Track unnecessary re-renders
4. **Commit Details**: Understand what triggered each commit

## Performance Metrics

### Core Web Vitals

```jsx
import { useEffect } from 'react';

function measurePerformance() {
  // Largest Contentful Paint (LCP)
  new PerformanceObserver((list) => {
    const entries = list.getEntries();
    const lastEntry = entries[entries.length - 1];
    console.log('LCP:', lastEntry.renderTime || lastEntry.loadTime);
  }).observe({ entryTypes: ['largest-contentful-paint'] });

  // First Input Delay (FID)
  new PerformanceObserver((list) => {
    list.getEntries().forEach((entry) => {
      console.log('FID:', entry.processingStart - entry.startTime);
    });
  }).observe({ entryTypes: ['first-input'] });

  // Cumulative Layout Shift (CLS)
  let clsScore = 0;
  new PerformanceObserver((list) => {
    list.getEntries().forEach((entry) => {
      if (!entry.hadRecentInput) {
        clsScore += entry.value;
        console.log('CLS:', clsScore);
      }
    });
  }).observe({ entryTypes: ['layout-shift'] });
}
```

### Custom Performance Monitoring

```jsx
import { useEffect, useRef } from 'react';

function useRenderTime(componentName) {
  const renderCount = useRef(0);
  const startTime = useRef(performance.now());

  useEffect(() => {
    const endTime = performance.now();
    const renderTime = endTime - startTime.current;
    renderCount.current++;
    
    console.log(`${componentName} render #${renderCount.current}: ${renderTime}ms`);
    startTime.current = performance.now();
  });
}

function ExpensiveComponent() {
  useRenderTime('ExpensiveComponent');
  
  return <div>Expensive operations here</div>;
}
```

## Identifying Bottlenecks

### Common Performance Issues

1. **Unnecessary Re-renders**
```jsx
// ❌ Bad: Child re-renders on every parent render
function Parent() {
  const [count, setCount] = useState(0);
  
  return (
    <div>
      <button onClick={() => setCount(count + 1)}>Count: {count}</button>
      <ExpensiveChild data={getData()} />
    </div>
  );
}

// ✅ Good: Memoized child with stable props
function Parent() {
  const [count, setCount] = useState(0);
  const data = useMemo(() => getData(), []);
  
  return (
    <div>
      <button onClick={() => setCount(count + 1)}>Count: {count}</button>
      <MemoizedExpensiveChild data={data} />
    </div>
  );
}

const MemoizedExpensiveChild = memo(ExpensiveChild);
```

2. **Large Lists Without Virtualization**
3. **Expensive Computations on Every Render**
4. **Inline Function/Object Creation**

### Detecting Re-renders

```jsx
import { useEffect, useRef } from 'react';

function useWhyDidYouUpdate(name, props) {
  const previousProps = useRef();

  useEffect(() => {
    if (previousProps.current) {
      const allKeys = Object.keys({ ...previousProps.current, ...props });
      const changedProps = {};

      allKeys.forEach(key => {
        if (previousProps.current[key] !== props[key]) {
          changedProps[key] = {
            from: previousProps.current[key],
            to: props[key]
          };
        }
      });

      if (Object.keys(changedProps).length) {
        console.log('[why-did-you-update]', name, changedProps);
      }
    }

    previousProps.current = props;
  });
}

function MyComponent(props) {
  useWhyDidYouUpdate('MyComponent', props);
  return <div>{props.data}</div>;
}
```

## Memoization Techniques

### React.memo

Prevents re-renders when props haven't changed:

```jsx
import { memo } from 'react';

// Basic usage
const ExpensiveComponent = memo(function ExpensiveComponent({ data }) {
  // Complex rendering logic
  return <div>{processData(data)}</div>;
});

// Custom comparison function
const UserCard = memo(
  function UserCard({ user, onSelect }) {
    return (
      <div onClick={() => onSelect(user.id)}>
        {user.name}
      </div>
    );
  },
  (prevProps, nextProps) => {
    // Only re-render if user.id or user.name changed
    return (
      prevProps.user.id === nextProps.user.id &&
      prevProps.user.name === nextProps.user.name
    );
  }
);
```

### useMemo

Memoizes expensive computations:

```jsx
import { useMemo } from 'react';

function DataTable({ data, filters }) {
  // ❌ Bad: Recalculates on every render
  const filteredData = data.filter(item => 
    filters.every(filter => filter(item))
  );

  // ✅ Good: Only recalculates when dependencies change
  const filteredData = useMemo(() => {
    return data.filter(item => 
      filters.every(filter => filter(item))
    );
  }, [data, filters]);

  const stats = useMemo(() => ({
    total: filteredData.length,
    average: filteredData.reduce((sum, item) => sum + item.value, 0) / filteredData.length,
    max: Math.max(...filteredData.map(item => item.value))
  }), [filteredData]);

  return (
    <div>
      <div>Total: {stats.total}, Average: {stats.average}</div>
      {filteredData.map(item => <Row key={item.id} data={item} />)}
    </div>
  );
}
```

### useCallback

Memoizes function references:

```jsx
import { useCallback, memo } from 'react';

function TodoList() {
  const [todos, setTodos] = useState([]);
  const [filter, setFilter] = useState('all');

  // ❌ Bad: New function on every render
  const handleToggle = (id) => {
    setTodos(todos.map(todo => 
      todo.id === id ? { ...todo, completed: !todo.completed } : todo
    ));
  };

  // ✅ Good: Stable function reference
  const handleToggle = useCallback((id) => {
    setTodos(todos => todos.map(todo => 
      todo.id === id ? { ...todo, completed: !todo.completed } : todo
    ));
  }, []); // Empty deps because we use functional update

  const handleDelete = useCallback((id) => {
    setTodos(todos => todos.filter(todo => todo.id !== id));
  }, []);

  return (
    <div>
      {todos.map(todo => (
        <MemoizedTodoItem
          key={todo.id}
          todo={todo}
          onToggle={handleToggle}
          onDelete={handleDelete}
        />
      ))}
    </div>
  );
}

const MemoizedTodoItem = memo(TodoItem);
```

### When NOT to Memoize

```jsx
// ❌ Don't memoize primitive computations
const doubled = useMemo(() => count * 2, [count]);

// ✅ Just compute it directly
const doubled = count * 2;

// ❌ Don't memoize if component always re-renders
function AlwaysUpdates({ timestamp }) {
  const expensive = useMemo(() => complexCalc(), []); // Useless
  return <div>{timestamp} - {expensive}</div>;
}
```

## React 19 Compiler

### Overview

The React Compiler automatically memoizes components and values, eliminating most manual optimization:

```jsx
// Before (React 18): Manual optimization needed
function SearchResults({ query }) {
  const results = useMemo(() => 
    expensiveSearch(query), 
    [query]
  );
  
  return <ResultsList results={results} />;
}

// After (React 19): Compiler handles it automatically
function SearchResults({ query }) {
  const results = expensiveSearch(query);
  return <ResultsList results={results} />;
}
```

### How It Works

The compiler analyzes your code and automatically:
1. Memoizes component outputs (like React.memo)
2. Memoizes expensive computations (like useMemo)
3. Memoizes callbacks (like useCallback)
4. Optimizes JSX creation

### Enabling the Compiler

```bash
# Install
npm install babel-plugin-react-compiler

# vite.config.js
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [['babel-plugin-react-compiler', {}]]
      }
    })
  ]
});
```

### Compiler Directives

```jsx
// Opt-out of compilation for specific components
'use no memo';
function NonOptimizedComponent() {
  // Manual control needed here
  return <div>...</div>;
}

// Ensure compilation
'use memo';
function OptimizedComponent() {
  // Compiler will optimize this
  return <div>...</div>;
}
```

### Limitations

The compiler can't optimize:
- Components with side effects in render
- Components using refs for DOM manipulation
- Direct DOM access during render

## Code Splitting & Lazy Loading

### Route-Based Code Splitting

```jsx
import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

// Lazy load route components
const Home = lazy(() => import('./pages/Home'));
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Profile = lazy(() => import('./pages/Profile'));

function App() {
  return (
    <BrowserRouter>
      <Suspense fallback={<LoadingSpinner />}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/profile" element={<Profile />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
```

### Component-Based Splitting

```jsx
import { lazy, Suspense, useState } from 'react';

const HeavyChart = lazy(() => import('./components/HeavyChart'));
const VideoPlayer = lazy(() => import('./components/VideoPlayer'));

function Dashboard() {
  const [showChart, setShowChart] = useState(false);

  return (
    <div>
      <button onClick={() => setShowChart(true)}>
        Load Chart
      </button>
      
      {showChart && (
        <Suspense fallback={<ChartSkeleton />}>
          <HeavyChart data={data} />
        </Suspense>
      )}
    </div>
  );
}
```

### Preloading

```jsx
// Preload on hover for instant feel
const UserProfile = lazy(() => import('./UserProfile'));

function UserCard({ user }) {
  const handleMouseEnter = () => {
    // Preload component before click
    import('./UserProfile');
  };

  return (
    <Link 
      to={`/user/${user.id}`}
      onMouseEnter={handleMouseEnter}
    >
      {user.name}
    </Link>
  );
}
```

### Named Exports

```jsx
// components/Charts.jsx
export function BarChart() { /* ... */ }
export function LineChart() { /* ... */ }
export function PieChart() { /* ... */ }

// Using specific exports
const BarChart = lazy(() => 
  import('./components/Charts').then(module => ({
    default: module.BarChart
  }))
);
```

## Virtual Scrolling

### Why Virtual Scrolling?

Rendering thousands of DOM elements causes:
- Slow initial render
- High memory usage
- Janky scrolling

### Using react-window

```jsx
import { FixedSizeList } from 'react-window';

function VirtualizedList({ items }) {
  const Row = ({ index, style }) => (
    <div style={style} className="list-item">
      {items[index].name}
    </div>
  );

  return (
    <FixedSizeList
      height={600}
      itemCount={items.length}
      itemSize={50}
      width="100%"
    >
      {Row}
    </FixedSizeList>
  );
}
```

### Variable Size Items

```jsx
import { VariableSizeList } from 'react-window';

function VariableSizeVirtualList({ items }) {
  const getItemSize = (index) => {
    // Calculate height based on content
    return items[index].expanded ? 200 : 50;
  };

  const Row = ({ index, style }) => (
    <div style={style}>
      <ItemCard item={items[index]} />
    </div>
  );

  return (
    <VariableSizeList
      height={600}
      itemCount={items.length}
      itemSize={getItemSize}
      width="100%"
    >
      {Row}
    </VariableSizeList>
  );
}
```

### Using TanStack Virtual

```jsx
import { useVirtualizer } from '@tanstack/react-virtual';
import { useRef } from 'react';

function TanStackVirtualList({ items }) {
  const parentRef = useRef(null);

  const virtualizer = useVirtualizer({
    count: items.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 50,
    overscan: 5 // Render 5 extra items
  });

  return (
    <div ref={parentRef} style={{ height: '600px', overflow: 'auto' }}>
      <div
        style={{
          height: `${virtualizer.getTotalSize()}px`,
          position: 'relative'
        }}
      >
        {virtualizer.getVirtualItems().map(virtualItem => (
          <div
            key={virtualItem.index}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              height: `${virtualItem.size}px`,
              transform: `translateY(${virtualItem.start}px)`
            }}
          >
            <ItemCard item={items[virtualItem.index]} />
          </div>
        ))}
      </div>
    </div>
  );
}
```

## Performance Optimization Checklist

### Development Phase

- [ ] Enable React DevTools Profiler
- [ ] Set up performance monitoring
- [ ] Use React.StrictMode to catch issues early
- [ ] Enable React 19 Compiler
- [ ] Configure proper build tools (Vite/Webpack)

### Component Optimization

- [ ] Use React.memo for expensive components
- [ ] Apply useMemo for expensive calculations
- [ ] Apply useCallback for stable function references
- [ ] Avoid inline object/array creation in props
- [ ] Split large components into smaller ones
- [ ] Use key prop correctly in lists

### Data & State Management

- [ ] Implement proper state colocation
- [ ] Use Context wisely (split contexts)
- [ ] Consider external state management for complex apps
- [ ] Optimize selector functions
- [ ] Implement optimistic updates

### Assets & Loading

- [ ] Implement code splitting
- [ ] Use lazy loading for routes
- [ ] Optimize images (WebP, lazy loading)
- [ ] Implement virtual scrolling for long lists
- [ ] Preload critical resources
- [ ] Use proper bundling strategies

### Runtime Performance

- [ ] Debounce/throttle expensive operations
- [ ] Use Web Workers for heavy computations
- [ ] Implement request deduplication
- [ ] Optimize CSS (avoid expensive selectors)
- [ ] Use CSS containment
- [ ] Minimize layout thrashing

### Monitoring

- [ ] Track Core Web Vitals
- [ ] Monitor bundle size
- [ ] Set up performance budgets
- [ ] Use real user monitoring (RUM)
- [ ] Profile production builds
- [ ] Monitor memory leaks

## Interview Questions

### Basic Questions

**Q1: What is React.memo and when should you use it?**

A: React.memo is a higher-order component that memoizes a component's output. It prevents re-renders when props haven't changed. Use it for:
- Components that render often with the same props
- Pure components with expensive render logic
- Components in lists that receive stable data

Avoid it for components that always render with different props or have cheap render logic.

**Q2: Explain the difference between useMemo and useCallback.**

A: 
- `useMemo`: Memoizes the **result** of a computation. Returns a cached value.
- `useCallback`: Memoizes the **function itself**. Returns a cached function reference.

```jsx
const value = useMemo(() => expensiveCalc(a, b), [a, b]); // Memoizes result
const callback = useCallback(() => doSomething(a, b), [a, b]); // Memoizes function
```

**Q3: What is the React Profiler and how do you use it?**

A: The React Profiler is a tool in React DevTools that measures how often a component renders and the "cost" of rendering. It provides:
- Flame graphs showing component hierarchy
- Render durations
- Commit information
- Interaction tracking

You can also use the `<Profiler>` component programmatically to collect performance data.

### Intermediate Questions

**Q4: How does the React 19 Compiler work?**

A: The React Compiler automatically optimizes React code by:
1. Analyzing component code at build time
2. Automatically memoizing components, values, and callbacks
3. Eliminating unnecessary re-renders without manual optimization
4. Converting imperative code to more efficient forms

It makes manual use of useMemo, useCallback, and React.memo less necessary.

**Q5: Explain virtual scrolling and when to use it.**

A: Virtual scrolling renders only the visible items in a large list, plus a buffer. It:
- Only renders items in the viewport
- Reuses DOM nodes as you scroll
- Drastically reduces DOM node count

Use it when:
- Rendering 100+ list items
- Each item is relatively uniform in size
- Performance issues occur with standard rendering

Libraries: react-window, @tanstack/react-virtual

**Q6: What causes unnecessary re-renders in React?**

A: Common causes:
1. Parent component re-renders (children re-render by default)
2. Inline object/array creation in props
3. New function creation on each render
4. Context value changes affecting all consumers
5. State updates with same value (pre-React 18)
6. Missing or incorrect dependencies in hooks

### Advanced Questions

**Q7: How would you optimize a component that renders a list of 10,000 items?**

A: Multi-layered approach:
```jsx
// 1. Virtual scrolling
import { FixedSizeList } from 'react-window';

// 2. Memoize list items
const ListItem = memo(({ item, onClick }) => {
  return <div onClick={() => onClick(item.id)}>{item.name}</div>;
});

// 3. Stable callback
const handleClick = useCallback((id) => {
  // Handle click
}, []);

// 4. Windowing implementation
function OptimizedList({ items }) {
  return (
    <FixedSizeList
      height={600}
      itemCount={items.length}
      itemSize={35}
      width="100%"
    >
      {({ index, style }) => (
        <div style={style}>
          <ListItem item={items[index]} onClick={handleClick} />
        </div>
      )}
    </FixedSizeList>
  );
}
```

**Q8: Explain the performance implications of Context API.**

A: Context has performance considerations:
1. **All consumers re-render** when context value changes
2. **Object identity matters**: New object = new value
3. **No built-in optimization**: Unlike props, Context doesn't use shallow comparison

Solutions:
```jsx
// Split contexts
const StateContext = createContext();
const DispatchContext = createContext(); // Never changes

// Memoize value
const value = useMemo(() => ({ state, dispatch }), [state]);

// Use selectors (with external library)
const name = useContextSelector(UserContext, state => state.name);
```

**Q9: How do you prevent memory leaks in React applications?**

A: Key strategies:
```jsx
// 1. Clean up subscriptions
useEffect(() => {
  const subscription = api.subscribe();
  return () => subscription.unsubscribe();
}, []);

// 2. Cancel async operations
useEffect(() => {
  let cancelled = false;
  
  async function fetchData() {
    const data = await api.fetch();
    if (!cancelled) setData(data);
  }
  
  fetchData();
  return () => { cancelled = true; };
}, []);

// 3. Clear timers
useEffect(() => {
  const timer = setInterval(() => tick(), 1000);
  return () => clearInterval(timer);
}, []);

// 4. Remove event listeners
useEffect(() => {
  const handler = () => console.log('scroll');
  window.addEventListener('scroll', handler);
  return () => window.removeEventListener('scroll', handler);
}, []);
```

**Q10: How would you implement code splitting in a large React application?**

A: Comprehensive strategy:
```jsx
// 1. Route-based splitting (primary)
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Settings = lazy(() => import('./pages/Settings'));

// 2. Component-based splitting (heavy components)
const Chart = lazy(() => import('./components/Chart'));

// 3. Library splitting (large dependencies)
const Editor = lazy(() => 
  import(/* webpackChunkName: "editor" */ './Editor')
);

// 4. Preloading on interaction
function AdminPanel() {
  const [showEditor, setShowEditor] = useState(false);
  
  const preload = () => import('./Editor');
  
  return (
    <button 
      onMouseEnter={preload}
      onClick={() => setShowEditor(true)}
    >
      Open Editor
    </button>
  );
}

// 5. Configure webpack/vite for optimal splitting
// vite.config.js
export default {
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          charts: ['chart.js', 'react-chartjs-2']
        }
      }
    }
  }
};
```

---

**Last Updated: January 2026 - React 19**

*See also: [React Hooks](./02-hooks-complete-guide.md) | [Suspense & Concurrent](./12-suspense-concurrent.md) | [Testing](./13-testing.md)*
