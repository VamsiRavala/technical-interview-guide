# React 19 Migration Guide

> **Updated for React 19 (2026)** - Complete guide to migrating from React 18 to React 19

---

## Table of Contents
- [What's New in React 19](#whats-new-in-react-19)
- [Breaking Changes](#breaking-changes)
- [Migration Steps](#migration-steps)
- [New Features to Adopt](#new-features-to-adopt)
- [Codemod Tools](#codemod-tools)
- [Common Pitfalls](#common-pitfalls)
- [Upgrade Checklist](#upgrade-checklist)

---

## What's New in React 19

### Major Features

1. **React Compiler** (Auto-optimization)
   - Automatic memoization without `useMemo`/`useCallback`
   - Eliminates need for manual optimization in most cases

2. **React Server Components (RSC)**
   - Server vs Client Components
   - Zero-bundle components
   - Integrated with Next.js 15+ and other frameworks

3. **Actions**
   - New form handling with `useActionState`
   - `useOptimistic` for optimistic UI updates
   - Built-in pending states

4. **New Hooks**
   - `use()` - Read context and promises
   - `useActionState()` - Form state management
   - `useOptimistic()` - Optimistic updates
   - `useFormStatus()` - Form submission status

5. **Improvements**
   - `ref` as a prop (no more `forwardRef`)
   - Context as a provider (no more `.Provider`)
   - Document metadata support (title, meta, link)
   - ref cleanup functions
   - `useDeferredValue` initial value support

---

## Breaking Changes

### 1. **Removed: forwardRef**

**Before (React 18):**
```jsx
import { forwardRef } from 'react';

const MyInput = forwardRef((props, ref) => {
  return <input ref={ref} {...props} />;
});
```

**After (React 19):**
```jsx
// ref is now a regular prop
function MyInput({ ref, ...props }) {
  return <input ref={ref} {...props} />;
}
```

### 2. **Removed: Context.Provider**

**Before (React 18):**
```jsx
const ThemeContext = createContext('light');

function App() {
  return (
    <ThemeContext.Provider value="dark">
      <Component />
    </ThemeContext.Provider>
  );
}
```

**After (React 19):**
```jsx
const ThemeContext = createContext('light');

function App() {
  return (
    <ThemeContext value="dark">
      <Component />
    </ThemeContext>
  );
}
```

### 3. **String Refs Removed**

**Before (React 18):**
```jsx
class MyComponent extends React.Component {
  componentDidMount() {
    this.refs.myInput.focus();
  }
  
  render() {
    return <input ref="myInput" />;
  }
}
```

**After (React 19):**
```jsx
class MyComponent extends React.Component {
  myInputRef = createRef();
  
  componentDidMount() {
    this.myInputRef.current.focus();
  }
  
  render() {
    return <input ref={this.myInputRef} />;
  }
}
```

### 4. **defaultProps Deprecated for Function Components**

**Before (React 18):**
```jsx
function Button({ color }) {
  return <button style={{ color }}>{children}</button>;
}

Button.defaultProps = {
  color: 'blue'
};
```

**After (React 19):**
```jsx
function Button({ color = 'blue', children }) {
  return <button style={{ color }}>{children}</button>;
}
```

### 5. **PropTypes Moved to Separate Package**

```bash
# Install prop-types separately
npm install prop-types
```

```jsx
import PropTypes from 'prop-types';

function Button({ onClick, children }) {
  return <button onClick={onClick}>{children}</button>;
}

Button.propTypes = {
  onClick: PropTypes.func.isRequired,
  children: PropTypes.node
};
```

---

## Migration Steps

### Step 1: Update Dependencies

```bash
# Using npm
npm install react@19 react-dom@19

# Using yarn
yarn add react@19 react-dom@19

# Using pnpm
pnpm add react@19 react-dom@19
```

### Step 2: Update TypeScript Types (if using TypeScript)

```bash
npm install -D @types/react@19 @types/react-dom@19
```

Update `tsconfig.json`:
```json
{
  "compilerOptions": {
    "jsx": "react-jsx",
    "types": ["react/next", "react-dom/next"]
  }
}
```

### Step 3: Run React 19 Codemod

```bash
npx react-codemod@latest react-19/replace-reactdom-render ./src
npx react-codemod@latest react-19/replace-string-ref ./src
npx react-codemod@latest react-19/replace-act-import ./src
```

### Step 4: Update forwardRef Usages

Find all `forwardRef` uses:
```bash
grep -r "forwardRef" ./src
```

Manual migration:
```jsx
// Before
const Input = forwardRef(({ value, onChange }, ref) => (
  <input ref={ref} value={value} onChange={onChange} />
));

// After
function Input({ value, onChange, ref }) {
  return <input ref={ref} value={value} onChange={onChange} />;
}
```

### Step 5: Update Context Providers

Find all `.Provider` uses:
```bash
grep -r "\.Provider" ./src
```

Replace:
```jsx
// Before
<MyContext.Provider value={value}>

// After
<MyContext value={value}>
```

### Step 6: Test Your Application

```bash
# Run your test suite
npm test

# Manual testing
npm start

# Check for console warnings
# React 19 provides helpful migration warnings
```

---

## New Features to Adopt

### 1. **Use Actions for Forms**

```jsx
import { useActionState } from 'react';

function AddTodoForm() {
  const [state, formAction, isPending] = useActionState(
    async (previousState, formData) => {
      const title = formData.get('title');
      try {
        await addTodo(title);
        return { success: true };
      } catch (error) {
        return { error: error.message };
      }
    },
    { success: false }
  );

  return (
    <form action={formAction}>
      <input name="title" disabled={isPending} />
      <button type="submit" disabled={isPending}>
        {isPending ? 'Adding...' : 'Add Todo'}
      </button>
      {state.error && <p>{state.error}</p>}
    </form>
  );
}
```

### 2. **Use `useOptimistic` for Better UX**

```jsx
import { useOptimistic } from 'react';

function TodoList({ todos, onToggle }) {
  const [optimisticTodos, setOptimisticTodos] = useOptimistic(todos);

  const toggleTodo = async (id) => {
    // Optimistically update UI
    setOptimisticTodos((current) =>
      current.map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo
      )
    );

    // Perform actual update
    await onToggle(id);
  };

  return (
    <ul>
      {optimisticTodos.map((todo) => (
        <li key={todo.id}>
          <input
            type="checkbox"
            checked={todo.completed}
            onChange={() => toggleTodo(todo.id)}
          />
          {todo.title}
        </li>
      ))}
    </ul>
  );
}
```

### 3. **Use `use()` Hook for Async Data**

```jsx
import { use, Suspense } from 'react';

function UserProfile({ userPromise }) {
  // use() can read promises directly
  const user = use(userPromise);

  return (
    <div>
      <h2>{user.name}</h2>
      <p>{user.email}</p>
    </div>
  );
}

function App() {
  const userPromise = fetchUser(userId);

  return (
    <Suspense fallback={<div>Loading...</div>}>
      <UserProfile userPromise={userPromise} />
    </Suspense>
  );
}
```

### 4. **Document Metadata in Components**

```jsx
function BlogPost({ post }) {
  return (
    <article>
      {/* These will be hoisted to <head> */}
      <title>{post.title} - My Blog</title>
      <meta name="description" content={post.excerpt} />
      <meta property="og:title" content={post.title} />
      <link rel="canonical" href={`https://myblog.com/posts/${post.slug}`} />
      
      <h1>{post.title}</h1>
      <div>{post.content}</div>
    </article>
  );
}
```

### 5. **Ref Cleanup Functions**

```jsx
function VideoPlayer({ src }) {
  return (
    <video
      ref={(node) => {
        if (node) {
          node.play();
          
          // Return cleanup function
          return () => {
            node.pause();
          };
        }
      }}
      src={src}
    />
  );
}
```

---

## Codemod Tools

### Available Codemods

```bash
# Replace ReactDOM.render with createRoot
npx react-codemod react-19/replace-reactdom-render

# Replace string refs with createRef
npx react-codemod react-19/replace-string-ref

# Replace test utils
npx react-codemod react-19/replace-act-import

# Update prop-types imports
npx react-codemod react-19/update-prop-types-imports
```

### Manual Review Needed

Some migrations require manual review:
- Custom hooks with complex logic
- Third-party library integrations
- Advanced ref patterns
- Complex context usage

---

## Common Pitfalls

### 1. **Forgetting to Remove forwardRef**

```jsx
// ❌ Don't keep forwardRef wrapper
const Input = forwardRef((props, ref) => {
  return <input ref={ref} {...props} />;
});

// ✅ Remove it, use ref as prop
function Input({ ref, ...props }) {
  return <input ref={ref} {...props} />;
}
```

### 2. **Still Using .Provider**

```jsx
// ❌ Old syntax still works but deprecated
<ThemeContext.Provider value={theme}>

// ✅ New syntax
<ThemeContext value={theme}>
```

### 3. **Not Leveraging New Features**

```jsx
// ❌ Still using old patterns
const [data, setData] = useState(null);
const [loading, setLoading] = useState(false);

useEffect(() => {
  setLoading(true);
  fetchData().then(result => {
    setData(result);
    setLoading(false);
  });
}, []);

// ✅ Use new features
function Component({ dataPromise }) {
  const data = use(dataPromise);
  return <div>{data}</div>;
}
```

### 4. **Ignoring Compiler Hints**

React 19 compiler provides optimization hints. Don't ignore them:

```jsx
// ❌ Compiler can't optimize this
const handleClick = () => {
  doSomething();
  doSomethingElse();
};

// ✅ Follow compiler suggestions
// Compiler will auto-memoize when appropriate
```

---

## Upgrade Checklist

### Pre-Migration

- [ ] Update Node.js to v18 or higher
- [ ] Review React 19 release notes
- [ ] Update all React-related dependencies
- [ ] Back up your codebase or create a branch
- [ ] Run existing test suite to establish baseline

### During Migration

- [ ] Update react and react-dom to v19
- [ ] Update TypeScript types if using TypeScript
- [ ] Run codemods for automated fixes
- [ ] Replace all `forwardRef` usages
- [ ] Update Context.Provider to Context
- [ ] Remove string refs
- [ ] Move defaultProps to function parameters
- [ ] Update prop-types imports

### Testing

- [ ] Run full test suite
- [ ] Test in development mode
- [ ] Test in production build
- [ ] Manual QA of critical paths
- [ ] Check browser console for warnings
- [ ] Performance testing

### Adoption of New Features

- [ ] Consider using Actions for forms
- [ ] Implement useOptimistic where applicable
- [ ] Adopt use() for async data fetching
- [ ] Add document metadata in components
- [ ] Use ref cleanup functions
- [ ] Let compiler handle optimizations

### Post-Migration

- [ ] Update documentation
- [ ] Train team on new features
- [ ] Monitor error tracking services
- [ ] Collect performance metrics
- [ ] Address any deprecation warnings

---

## Framework-Specific Migrations

### Next.js

```bash
# Update to Next.js 15+ for React 19 support
npm install next@15
```

Configure App Router for RSC:
```jsx
// app/layout.js
export default function RootLayout({ children }) {
  return (
    <html>
      <body>{children}</body>
    </html>
  );
}

// app/page.js (Server Component by default)
async function Page() {
  const data = await fetchData();
  return <div>{data}</div>;
}
```

### Remix

```bash
# Update Remix for React 19
npm install @remix-run/react@latest
```

### Vite

Update vite config:
```js
// vite.config.js
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [
    react({
      jsxRuntime: 'automatic', // React 19 uses automatic runtime
    }),
  ],
});
```

---

## Resources

### Official Documentation
- [React 19 Release Notes](https://react.dev/blog/2024/04/25/react-19)
- [React 19 Upgrade Guide](https://react.dev/blog/2024/04/25/react-19-upgrade-guide)
- [React Compiler](https://react.dev/learn/react-compiler)

### Tools
- [React Codemods](https://github.com/reactjs/react-codemod)
- [TypeScript](https://www.typescriptlang.org/)
- [React DevTools](https://react.dev/learn/react-developer-tools)

### Community
- [React GitHub Discussions](https://github.com/facebook/react/discussions)
- [React Discord](https://discord.gg/react)

---

## Summary

- ✅ Update to React 19 is straightforward with codemods
- ✅ Major breaking changes: forwardRef, Context.Provider, string refs
- ✅ New features provide better DX and performance
- ✅ React Compiler eliminates manual optimization
- ✅ Actions simplify form handling
- ⚠️ Test thoroughly after migration
- ⚠️ Update dependencies and types

---

*Last Updated: January 2026 - React 19*
