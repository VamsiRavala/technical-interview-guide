# Error Boundaries in React

> **Updated for React 19 (2026)** - Comprehensive guide to error handling in React applications

---

## Table of Contents
- [What are Error Boundaries?](#what-are-error-boundaries)
- [Class-Based Error Boundaries](#class-based-error-boundaries)
- [React Error Boundary Library](#react-error-boundary-library)
- [Best Practices](#best-practices)
- [Fallback UI Patterns](#fallback-ui-patterns)
- [Common Use Cases](#common-use-cases)
- [Limitations](#limitations)

---

## What are Error Boundaries?

Error boundaries are React components that **catch JavaScript errors anywhere in their child component tree**, log those errors, and display a fallback UI instead of crashing the entire application.

### Key Points
- Error boundaries catch errors during **rendering**, in **lifecycle methods**, and in **constructors**
- They do **NOT** catch errors in:
  - Event handlers
  - Asynchronous code (setTimeout, promises)
  - Server-side rendering
  - Errors thrown in the error boundary itself

---

## Class-Based Error Boundaries

As of React 19, error boundaries still require class components. A component becomes an error boundary if it defines either (or both) of these lifecycle methods:

### Basic Implementation

```jsx
import React from 'react';

class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null, errorInfo: null };
  }

  static getDerivedStateFromError(error) {
    // Update state so the next render shows the fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // Log error details to an error reporting service
    console.error('Error caught by boundary:', error, errorInfo);
    
    // You can also log to services like Sentry, LogRocket, etc.
    // logErrorToService(error, errorInfo);
    
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
  }

  render() {
    if (this.state.hasError) {
      // Render fallback UI
      return (
        <div style={{ padding: '20px', textAlign: 'center' }}>
          <h2>Something went wrong</h2>
          <details style={{ whiteSpace: 'pre-wrap' }}>
            {this.state.error && this.state.error.toString()}
            <br />
            {this.state.errorInfo && this.state.errorInfo.componentStack}
          </details>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
```

### Usage Example

```jsx
import ErrorBoundary from './ErrorBoundary';
import UserProfile from './UserProfile';
import Dashboard from './Dashboard';

function App() {
  return (
    <div>
      <ErrorBoundary>
        <UserProfile />
      </ErrorBoundary>
      
      <ErrorBoundary>
        <Dashboard />
      </ErrorBoundary>
    </div>
  );
}
```

---

## React Error Boundary Library

The `react-error-boundary` library provides a more feature-rich and easier-to-use solution.

### Installation

```bash
npm install react-error-boundary
# or
yarn add react-error-boundary
# or
pnpm add react-error-boundary
```

### Basic Usage

```jsx
import { ErrorBoundary } from 'react-error-boundary';

function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <div role="alert">
      <p>Something went wrong:</p>
      <pre style={{ color: 'red' }}>{error.message}</pre>
      <button onClick={resetErrorBoundary}>Try again</button>
    </div>
  );
}

function App() {
  return (
    <ErrorBoundary
      FallbackComponent={ErrorFallback}
      onReset={() => {
        // Reset the state of your app here
      }}
      onError={(error, errorInfo) => {
        // Log to error reporting service
        console.log('Error:', error);
        console.log('Error Info:', errorInfo);
      }}
    >
      <YourApp />
    </ErrorBoundary>
  );
}
```

### Advanced Features

#### 1. Reset Error Boundary on Location Change

```jsx
import { ErrorBoundary } from 'react-error-boundary';
import { useLocation } from 'react-router-dom';

function App() {
  const location = useLocation();
  
  return (
    <ErrorBoundary
      FallbackComponent={ErrorFallback}
      resetKeys={[location.pathname]} // Reset on route change
    >
      <YourApp />
    </ErrorBoundary>
  );
}
```

#### 2. Render Fallback Prop

```jsx
<ErrorBoundary
  fallbackRender={({ error, resetErrorBoundary }) => (
    <div>
      <h1>Error: {error.message}</h1>
      <button onClick={resetErrorBoundary}>Retry</button>
    </div>
  )}
>
  <YourComponent />
</ErrorBoundary>
```

#### 3. useErrorHandler Hook

```jsx
import { useErrorHandler } from 'react-error-boundary';

function MyComponent() {
  const handleError = useErrorHandler();

  async function fetchData() {
    try {
      const response = await fetch('/api/data');
      const data = await response.json();
      return data;
    } catch (error) {
      handleError(error); // Throws to nearest error boundary
    }
  }

  // Event handlers with error boundaries
  const handleClick = () => {
    try {
      // Some risky operation
      riskyFunction();
    } catch (error) {
      handleError(error);
    }
  };

  return <button onClick={handleClick}>Click me</button>;
}
```

---

## Best Practices

### 1. **Granular Error Boundaries**
Place error boundaries at different levels of your component tree:

```jsx
function App() {
  return (
    <ErrorBoundary fallback={<AppError />}>
      <Header />
      
      <ErrorBoundary fallback={<SidebarError />}>
        <Sidebar />
      </ErrorBoundary>
      
      <main>
        <ErrorBoundary fallback={<ContentError />}>
          <Content />
        </ErrorBoundary>
      </main>
    </ErrorBoundary>
  );
}
```

### 2. **Log to Error Tracking Services**

```jsx
import * as Sentry from '@sentry/react';

function logError(error, errorInfo) {
  Sentry.captureException(error, {
    contexts: {
      react: {
        componentStack: errorInfo.componentStack,
      },
    },
  });
}

<ErrorBoundary onError={logError}>
  <App />
</ErrorBoundary>
```

### 3. **Meaningful Fallback UI**

```jsx
function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <div className="error-container">
      <img src="/error-illustration.svg" alt="Error" />
      <h2>Oops! Something went wrong</h2>
      <p>
        We're sorry for the inconvenience. Our team has been notified and 
        is working on fixing this issue.
      </p>
      <button onClick={resetErrorBoundary}>
        Return to Home
      </button>
      {process.env.NODE_ENV === 'development' && (
        <details>
          <summary>Error details</summary>
          <pre>{error.message}</pre>
        </details>
      )}
    </div>
  );
}
```

### 4. **Environment-Specific Behavior**

```jsx
function ErrorFallback({ error, resetErrorBoundary }) {
  const isDevelopment = process.env.NODE_ENV === 'development';
  
  return (
    <div>
      <h2>Something went wrong</h2>
      {isDevelopment ? (
        <details>
          <summary>Error Details (Development Only)</summary>
          <pre>{error.stack}</pre>
        </details>
      ) : (
        <p>Please try again later or contact support.</p>
      )}
      <button onClick={resetErrorBoundary}>Try again</button>
    </div>
  );
}
```

---

## Fallback UI Patterns

### 1. **Minimal Fallback**
```jsx
const MinimalFallback = ({ resetErrorBoundary }) => (
  <div>
    <p>Something went wrong</p>
    <button onClick={resetErrorBoundary}>Reload</button>
  </div>
);
```

### 2. **Detailed Fallback**
```jsx
const DetailedFallback = ({ error, resetErrorBoundary }) => (
  <div className="error-page">
    <h1>Application Error</h1>
    <div className="error-info">
      <h3>Error Message:</h3>
      <code>{error.message}</code>
    </div>
    <div className="error-actions">
      <button onClick={resetErrorBoundary}>Try Again</button>
      <button onClick={() => window.location.href = '/'}>Go Home</button>
    </div>
  </div>
);
```

### 3. **Inline Component Fallback**
```jsx
const InlineFallback = () => (
  <div style={{ 
    padding: '1rem', 
    border: '1px solid #ff6b6b', 
    borderRadius: '4px',
    background: '#ffe0e0'
  }}>
    <p>This section couldn't load. Please refresh the page.</p>
  </div>
);
```

---

## Common Use Cases

### 1. **Wrapping Async Components**

```jsx
import { lazy, Suspense } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

const LazyComponent = lazy(() => import('./LazyComponent'));

function App() {
  return (
    <ErrorBoundary fallback={<ErrorPage />}>
      <Suspense fallback={<Loading />}>
        <LazyComponent />
      </Suspense>
    </ErrorBoundary>
  );
}
```

### 2. **Third-Party Widget Integration**

```jsx
function Dashboard() {
  return (
    <div>
      <MyContent />
      
      {/* Isolate potentially buggy third-party widget */}
      <ErrorBoundary fallback={<div>Widget unavailable</div>}>
        <ThirdPartyWidget />
      </ErrorBoundary>
    </div>
  );
}
```

### 3. **Per-Route Error Boundaries**

```jsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route 
          path="/" 
          element={
            <ErrorBoundary fallback={<HomeFallback />}>
              <Home />
            </ErrorBoundary>
          } 
        />
        <Route 
          path="/dashboard" 
          element={
            <ErrorBoundary fallback={<DashboardFallback />}>
              <Dashboard />
            </ErrorBoundary>
          } 
        />
      </Routes>
    </BrowserRouter>
  );
}
```

---

## Limitations

### What Error Boundaries DON'T Catch

1. **Event Handlers**
```jsx
// ❌ Error boundary won't catch this
function MyComponent() {
  const handleClick = () => {
    throw new Error('Event handler error');
  };
  
  return <button onClick={handleClick}>Click</button>;
}

// ✅ Solution: Use try-catch or useErrorHandler
function MyComponent() {
  const handleError = useErrorHandler();
  
  const handleClick = () => {
    try {
      throw new Error('Event handler error');
    } catch (error) {
      handleError(error);
    }
  };
  
  return <button onClick={handleClick}>Click</button>;
}
```

2. **Asynchronous Code**
```jsx
// ❌ Error boundary won't catch this
useEffect(() => {
  setTimeout(() => {
    throw new Error('Async error');
  }, 1000);
}, []);

// ✅ Solution: Handle errors properly
useEffect(() => {
  const timer = setTimeout(() => {
    try {
      throw new Error('Async error');
    } catch (error) {
      handleError(error);
    }
  }, 1000);
  
  return () => clearTimeout(timer);
}, []);
```

3. **Server-Side Rendering**
- Error boundaries work differently in SSR
- Use framework-specific error handling (Next.js error pages, etc.)

---

## Interview Questions

### Q1: What is an error boundary in React?
**Answer:** An error boundary is a React component that catches JavaScript errors anywhere in its child component tree, logs those errors, and displays a fallback UI instead of crashing the entire application. They catch errors during rendering, in lifecycle methods, and in constructors of the whole tree below them.

### Q2: Why can't we use error boundaries with function components?
**Answer:** As of React 19, error boundaries still require class components because they rely on the lifecycle methods `getDerivedStateFromError` and `componentDidCatch`, which don't have hooks equivalents yet. However, you can use libraries like `react-error-boundary` which provide hooks like `useErrorHandler` to work with error boundaries in functional components.

### Q3: What errors do error boundaries NOT catch?
**Answer:** Error boundaries do NOT catch:
- Errors in event handlers
- Asynchronous code (setTimeout, promises, async/await)
- Server-side rendering errors
- Errors thrown in the error boundary itself (rather than its children)

### Q4: Where should you place error boundaries in your application?
**Answer:** Place error boundaries at strategic points:
- At the top level to catch all errors
- Around route components to prevent entire app crashes
- Around third-party components that may be unstable
- Around independent widgets or features that can fail independently
The key is to be granular enough to isolate failures but not so granular that you have error boundaries everywhere.

### Q5: How do you reset an error boundary?
**Answer:** You can reset an error boundary by:
1. Changing the component's key prop
2. Using the `resetErrorBoundary` callback provided by `react-error-boundary` library
3. Using `resetKeys` prop to automatically reset when certain values change
4. Updating parent state that causes a re-mount

---

## Summary

- ✅ Error boundaries catch rendering errors in child components
- ✅ Use class components or `react-error-boundary` library
- ✅ Place boundaries strategically for proper error isolation
- ✅ Provide meaningful fallback UI with recovery options
- ✅ Log errors to monitoring services for debugging
- ❌ Don't rely on error boundaries for event handlers or async code
- ❌ Don't put error boundaries everywhere - be strategic

---

**Next Steps:**
- Learn about [Portals](10-portals.md) for rendering outside the DOM hierarchy
- Explore [React Profiler & Performance](11-profiler-performance.md) optimization techniques
- Check out [Testing](13-testing.md) for error boundary testing strategies

---

*Last Updated: January 2026 - React 19*
