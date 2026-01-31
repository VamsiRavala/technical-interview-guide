# React Suspense & Concurrent Features

## Table of Contents
1. [Introduction](#introduction)
2. [Suspense for Data Fetching](#suspense-for-data-fetching)
3. [Suspense Boundaries](#suspense-boundaries)
4. [Error Boundaries with Suspense](#error-boundaries-with-suspense)
5. [Transitions (startTransition & useTransition)](#transitions-starttransition--usetransition)
6. [useDeferredValue](#usedeferredvalue)
7. [Concurrent Rendering](#concurrent-rendering)
8. [Server Components](#server-components)
9. [Streaming SSR](#streaming-ssr)
10. [Best Practices & Patterns](#best-practices--patterns)
11. [Interview Questions](#interview-questions)

## Introduction

React 19 brings significant improvements to Suspense and concurrent features, enabling better user experiences through:
- Non-blocking UI updates
- Intelligent prioritization of updates
- Seamless loading states
- Streaming server rendering
- Automatic batching of updates

## Suspense for Data Fetching

### Basic Suspense Usage (React 19)

In React 19, Suspense works seamlessly with the `use` hook and async/await:

```jsx
import { Suspense, use } from 'react';

// Resource that throws a promise
function fetchUser(id) {
  return fetch(`/api/users/${id}`).then(res => res.json());
}

function UserProfile({ userId }) {
  // `use` hook works with promises - suspends until resolved
  const user = use(fetchUser(userId));
  
  return (
    <div>
      <h1>{user.name}</h1>
      <p>{user.email}</p>
    </div>
  );
}

function App() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <UserProfile userId={1} />
    </Suspense>
  );
}
```

### Using the `use` Hook

The `use` hook is React 19's way to read resources:

```jsx
import { use, Suspense } from 'react';

function Comments({ commentsPromise }) {
  // Can call `use` conditionally!
  const comments = use(commentsPromise);
  
  return (
    <ul>
      {comments.map(comment => (
        <li key={comment.id}>{comment.text}</li>
      ))}
    </ul>
  );
}

function Post({ postId }) {
  const post = use(fetchPost(postId));
  const commentsPromise = fetchComments(postId);
  
  return (
    <article>
      <h2>{post.title}</h2>
      <p>{post.body}</p>
      
      <Suspense fallback={<CommentsSkeleton />}>
        <Comments commentsPromise={commentsPromise} />
      </Suspense>
    </article>
  );
}

function App() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Post postId={1} />
    </Suspense>
  );
}
```

### Suspense with TanStack Query

```jsx
import { useSuspenseQuery } from '@tanstack/react-query';
import { Suspense } from 'react';

function UserDashboard({ userId }) {
  // Suspends automatically
  const { data: user } = useSuspenseQuery({
    queryKey: ['user', userId],
    queryFn: () => fetchUser(userId)
  });
  
  const { data: posts } = useSuspenseQuery({
    queryKey: ['posts', userId],
    queryFn: () => fetchUserPosts(userId)
  });

  return (
    <div>
      <h1>{user.name}</h1>
      <PostList posts={posts} />
    </div>
  );
}

function App() {
  return (
    <Suspense fallback={<DashboardSkeleton />}>
      <UserDashboard userId={1} />
    </Suspense>
  );
}
```

### Creating Suspense-Enabled Resources

```jsx
// Simple resource wrapper
function wrapPromise(promise) {
  let status = 'pending';
  let result;
  
  const suspender = promise.then(
    (data) => {
      status = 'success';
      result = data;
    },
    (error) => {
      status = 'error';
      result = error;
    }
  );
  
  return {
    read() {
      if (status === 'pending') throw suspender;
      if (status === 'error') throw result;
      return result;
    }
  };
}

// Usage
const userResource = wrapPromise(fetchUser(1));

function UserProfile() {
  const user = userResource.read(); // Suspends on first call
  return <div>{user.name}</div>;
}
```

### Parallel Data Fetching

```jsx
import { use, Suspense } from 'react';

function DashboardData() {
  // All promises start in parallel
  const userPromise = fetchUser();
  const postsPromise = fetchPosts();
  const notificationsPromise = fetchNotifications();
  
  return (
    <div>
      <Suspense fallback={<UserSkeleton />}>
        <UserInfo userPromise={userPromise} />
      </Suspense>
      
      <Suspense fallback={<PostsSkeleton />}>
        <Posts postsPromise={postsPromise} />
      </Suspense>
      
      <Suspense fallback={<NotificationsSkeleton />}>
        <Notifications notificationsPromise={notificationsPromise} />
      </Suspense>
    </div>
  );
}

function UserInfo({ userPromise }) {
  const user = use(userPromise);
  return <div>{user.name}</div>;
}
```

## Suspense Boundaries

### Strategic Boundary Placement

```jsx
function App() {
  return (
    <div>
      {/* Top-level boundary for critical content */}
      <Suspense fallback={<PageLoader />}>
        <Navigation />
        
        {/* Nested boundary for independent sections */}
        <div className="content">
          <Suspense fallback={<SidebarSkeleton />}>
            <Sidebar />
          </Suspense>
          
          <main>
            <Suspense fallback={<MainContentSkeleton />}>
              <MainContent />
            </Suspense>
          </main>
        </div>
      </Suspense>
      
      {/* Always visible footer */}
      <Footer />
    </div>
  );
}
```

### Boundary Granularity

```jsx
// ❌ Too coarse: One delay blocks everything
function CoarseGrained() {
  return (
    <Suspense fallback={<FullPageLoader />}>
      <FastComponent />
      <SlowComponent />
      <AnotherFastComponent />
    </Suspense>
  );
}

// ✅ Optimal: Independent loading states
function OptimalGranularity() {
  return (
    <div>
      <FastComponent /> {/* No suspense needed */}
      
      <Suspense fallback={<ComponentSkeleton />}>
        <SlowComponent />
      </Suspense>
      
      <AnotherFastComponent /> {/* No suspense needed */}
    </div>
  );
}
```

### Conditional Suspense

```jsx
function UserProfile({ userId, showDetails }) {
  const user = use(fetchUser(userId));
  
  return (
    <div>
      <h1>{user.name}</h1>
      
      {showDetails && (
        <Suspense fallback={<DetailsSkeleton />}>
          <UserDetails userId={userId} />
        </Suspense>
      )}
    </div>
  );
}
```

### Suspense with Tabs

```jsx
import { useState, Suspense } from 'react';

function TabbedContent() {
  const [activeTab, setActiveTab] = useState('profile');
  
  return (
    <div>
      <Tabs active={activeTab} onChange={setActiveTab} />
      
      <Suspense fallback={<TabContentSkeleton />}>
        {activeTab === 'profile' && <ProfileTab />}
        {activeTab === 'posts' && <PostsTab />}
        {activeTab === 'settings' && <SettingsTab />}
      </Suspense>
    </div>
  );
}
```

## Error Boundaries with Suspense

### Combined Error and Suspense Boundaries

```jsx
import { Component, Suspense } from 'react';

class ErrorBoundary extends Component {
  state = { hasError: false, error: null };
  
  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }
  
  componentDidCatch(error, errorInfo) {
    console.error('Error caught:', error, errorInfo);
    // Log to error reporting service
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <div className="error-state">
          <h2>Something went wrong</h2>
          <button onClick={() => this.setState({ hasError: false })}>
            Try again
          </button>
        </div>
      );
    }
    
    return this.props.children;
  }
}

function App() {
  return (
    <ErrorBoundary>
      <Suspense fallback={<Loading />}>
        <UserDashboard />
      </Suspense>
    </ErrorBoundary>
  );
}
```

### Granular Error Handling

```jsx
function DataFetchingComponent() {
  return (
    <div>
      <ErrorBoundary fallback={<SidebarError />}>
        <Suspense fallback={<SidebarSkeleton />}>
          <Sidebar />
        </Suspense>
      </ErrorBoundary>
      
      <ErrorBoundary fallback={<ContentError />}>
        <Suspense fallback={<ContentSkeleton />}>
          <MainContent />
        </Suspense>
      </ErrorBoundary>
    </div>
  );
}
```

### React 19 Error Boundary Improvements

```jsx
import { use } from 'react';

function UserProfile({ userId }) {
  try {
    const user = use(fetchUser(userId));
    return <div>{user.name}</div>;
  } catch (error) {
    // Can handle errors locally in React 19
    if (error instanceof NotFoundError) {
      return <UserNotFound />;
    }
    throw error; // Re-throw to boundary
  }
}
```

## Transitions (startTransition & useTransition)

### startTransition

Mark updates as non-urgent to keep UI responsive:

```jsx
import { startTransition, useState } from 'react';

function SearchResults() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  
  const handleChange = (e) => {
    const value = e.target.value;
    
    // Urgent: Update input immediately
    setQuery(value);
    
    // Non-urgent: Update results without blocking
    startTransition(() => {
      setResults(searchItems(value));
    });
  };
  
  return (
    <div>
      <input value={query} onChange={handleChange} />
      <ResultsList results={results} />
    </div>
  );
}
```

### useTransition Hook

Get loading state for transitions:

```jsx
import { useTransition, useState } from 'react';

function TabContainer() {
  const [activeTab, setActiveTab] = useState('home');
  const [isPending, startTransition] = useTransition();
  
  const selectTab = (tab) => {
    startTransition(() => {
      setActiveTab(tab);
    });
  };
  
  return (
    <div>
      <div className="tabs">
        <Tab 
          active={activeTab === 'home'} 
          onClick={() => selectTab('home')}
        >
          Home
        </Tab>
        <Tab 
          active={activeTab === 'posts'} 
          onClick={() => selectTab('posts')}
        >
          Posts {isPending && <Spinner />}
        </Tab>
      </div>
      
      <div style={{ opacity: isPending ? 0.7 : 1 }}>
        {activeTab === 'home' && <HomeTab />}
        {activeTab === 'posts' && <PostsTab />}
      </div>
    </div>
  );
}
```

### Optimistic Updates with Transitions

```jsx
function TodoList() {
  const [todos, setTodos] = useState([]);
  const [isPending, startTransition] = useTransition();
  
  const addTodo = async (text) => {
    const optimisticTodo = {
      id: Date.now(),
      text,
      status: 'pending'
    };
    
    // Show optimistic update immediately
    setTodos([...todos, optimisticTodo]);
    
    // Perform actual update in transition
    startTransition(async () => {
      try {
        const newTodo = await api.createTodo(text);
        setTodos(todos => 
          todos.map(t => t.id === optimisticTodo.id ? newTodo : t)
        );
      } catch (error) {
        // Rollback on error
        setTodos(todos => 
          todos.filter(t => t.id !== optimisticTodo.id)
        );
      }
    });
  };
  
  return (
    <div>
      {isPending && <ProgressBar />}
      <TodoForm onSubmit={addTodo} />
      <ul>
        {todos.map(todo => (
          <TodoItem key={todo.id} todo={todo} />
        ))}
      </ul>
    </div>
  );
}
```

### Transitions vs Debouncing

```jsx
// ❌ Old way: Debouncing
function SearchWithDebounce() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  
  useEffect(() => {
    const timer = setTimeout(() => {
      setResults(searchItems(query));
    }, 300);
    
    return () => clearTimeout(timer);
  }, [query]);
  
  return <input value={query} onChange={e => setQuery(e.target.value)} />;
}

// ✅ New way: Transitions (instant feedback)
function SearchWithTransition() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  
  const handleChange = (e) => {
    setQuery(e.target.value); // Instant
    startTransition(() => {
      setResults(searchItems(e.target.value)); // Non-blocking
    });
  };
  
  return <input value={query} onChange={handleChange} />;
}
```

## useDeferredValue

### Basic Usage

Defer non-critical updates:

```jsx
import { useDeferredValue, useState } from 'react';

function SearchPage() {
  const [query, setQuery] = useState('');
  const deferredQuery = useDeferredValue(query);
  
  return (
    <div>
      <input 
        value={query} 
        onChange={e => setQuery(e.target.value)}
        placeholder="Search..."
      />
      
      {/* Uses deferred value - won't block input */}
      <SearchResults query={deferredQuery} />
    </div>
  );
}

function SearchResults({ query }) {
  const results = searchItems(query); // Expensive operation
  
  return (
    <ul>
      {results.map(item => (
        <li key={item.id}>{item.name}</li>
      ))}
    </ul>
  );
}
```

### useDeferredValue with Initial Value (React 19)

```jsx
import { useDeferredValue } from 'react';

function FilteredList({ filter }) {
  // React 19: Provide initial value to avoid empty state flash
  const deferredFilter = useDeferredValue(filter, '');
  
  const filteredItems = items.filter(item => 
    item.name.includes(deferredFilter)
  );
  
  return (
    <ul>
      {filteredItems.map(item => (
        <li key={item.id}>{item.name}</li>
      ))}
    </ul>
  );
}
```

### Showing Stale Content

```jsx
function ProductList({ searchQuery }) {
  const deferredQuery = useDeferredValue(searchQuery);
  const isStale = searchQuery !== deferredQuery;
  
  return (
    <div style={{ opacity: isStale ? 0.5 : 1 }}>
      <ProductGrid query={deferredQuery} />
    </div>
  );
}
```

### useDeferredValue vs useTransition

```jsx
// useDeferredValue: You don't control the state
function SearchWithDeferred({ externalQuery }) {
  const deferredQuery = useDeferredValue(externalQuery);
  return <Results query={deferredQuery} />;
}

// useTransition: You control the state
function SearchWithTransition() {
  const [query, setQuery] = useState('');
  const [isPending, startTransition] = useTransition();
  
  const handleChange = (value) => {
    startTransition(() => setQuery(value));
  };
  
  return <Results query={query} isPending={isPending} />;
}
```

## Concurrent Rendering

### How Concurrent Rendering Works

React can:
1. **Interrupt rendering** to handle urgent updates
2. **Reuse work** from interrupted renders
3. **Prioritize updates** automatically
4. **Render in the background** without blocking

```jsx
import { useTransition } from 'react';

function App() {
  const [isPending, startTransition] = useTransition();
  const [page, setPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  
  // High priority: Input updates
  const handleSearch = (e) => {
    setSearchQuery(e.target.value);
  };
  
  // Low priority: Page navigation
  const handlePageChange = (newPage) => {
    startTransition(() => {
      setPage(newPage);
    });
  };
  
  return (
    <div>
      <input value={searchQuery} onChange={handleSearch} />
      <SearchResults query={searchQuery} page={page} />
      <Pagination page={page} onChange={handlePageChange} />
    </div>
  );
}
```

### Automatic Batching

React 19 automatically batches all updates:

```jsx
// React 18: Only event handlers batched
setTimeout(() => {
  setCount(c => c + 1);
  setFlag(f => !f);
  // Triggers two re-renders
}, 1000);

// React 19: Everything batched automatically
setTimeout(() => {
  setCount(c => c + 1);
  setFlag(f => !f);
  // Triggers one re-render
}, 1000);

fetch('/api/data').then(() => {
  setData(newData);
  setLoading(false);
  // Single re-render in React 19
});
```

### Priority Levels

```jsx
import { startTransition } from 'react';

function PriorityExample() {
  // Urgent (immediate): User input
  const handleInput = (e) => {
    setInputValue(e.target.value); // High priority
  };
  
  // Normal: Regular updates
  const handleClick = () => {
    setCounter(c => c + 1);
  };
  
  // Low priority: Background work
  const handleBackgroundUpdate = () => {
    startTransition(() => {
      setExpensiveData(processData());
    });
  };
  
  return (
    <div>
      <input onChange={handleInput} />
      <button onClick={handleClick}>Increment</button>
      <button onClick={handleBackgroundUpdate}>Process Data</button>
    </div>
  );
}
```

## Server Components

### React Server Components (RSC)

Server Components run only on the server, reducing bundle size:

```jsx
// UserProfile.server.jsx - Server Component
import { db } from './database';

async function UserProfile({ userId }) {
  // Direct database access - runs on server
  const user = await db.users.findById(userId);
  const posts = await db.posts.findByUserId(userId);
  
  return (
    <div>
      <h1>{user.name}</h1>
      <PostsList posts={posts} />
    </div>
  );
}

// No JavaScript shipped to client for this component!
export default UserProfile;
```

### Server vs Client Components

```jsx
// page.server.jsx
import ClientButton from './ClientButton.client.jsx';

async function Page() {
  const data = await fetchData(); // Server only
  
  return (
    <div>
      <h1>Server-rendered content</h1>
      {/* Client component for interactivity */}
      <ClientButton data={data} />
    </div>
  );
}

// ClientButton.client.jsx
'use client'; // Marks as client component

import { useState } from 'react';

function ClientButton({ data }) {
  const [count, setCount] = useState(0);
  
  return (
    <button onClick={() => setCount(count + 1)}>
      Count: {count} | Data: {data}
    </button>
  );
}
```

### Mixing Server and Client Components

```jsx
// Dashboard.server.jsx
import Sidebar from './Sidebar.client.jsx';
import { db } from './db';

async function Dashboard({ userId }) {
  // Runs on server
  const user = await db.users.findById(userId);
  const notifications = await db.notifications.findRecent(userId);
  
  return (
    <div>
      {/* Server-rendered, no JS */}
      <Header user={user} />
      
      {/* Client component, interactive */}
      <Sidebar notifications={notifications} />
      
      {/* Server component, no JS */}
      <MainContent userId={userId} />
    </div>
  );
}
```

### Benefits of Server Components

1. **Zero Bundle Size**: Server components add no JavaScript
2. **Direct Backend Access**: No API routes needed
3. **Automatic Code Splitting**: Client components split automatically
4. **SEO-Friendly**: Fully rendered HTML
5. **Better Performance**: Less JS to download and parse

## Streaming SSR

### Traditional SSR vs Streaming

```jsx
// Traditional SSR - waits for everything
async function traditionalSSR() {
  const userData = await fetchUser();
  const postsData = await fetchPosts();
  const commentsData = await fetchComments();
  
  // Send HTML only after everything loads
  return renderToString(<App user={userData} posts={postsData} />);
}

// Streaming SSR - sends content progressively
function streamingSSR() {
  return renderToReadableStream(
    <Suspense fallback={<Skeleton />}>
      <App />
    </Suspense>
  );
}
```

### Using Streaming with React 19

```jsx
// app/page.jsx (Next.js App Router)
import { Suspense } from 'react';

async function Posts() {
  const posts = await fetchPosts(); // Slow
  return <PostsList posts={posts} />;
}

async function Sidebar() {
  const data = await fetchSidebarData(); // Fast
  return <SidebarContent data={data} />;
}

export default function Page() {
  return (
    <div>
      {/* Fast content shows immediately */}
      <Suspense fallback={<SidebarSkeleton />}>
        <Sidebar />
      </Suspense>
      
      {/* Slow content streams in later */}
      <Suspense fallback={<PostsSkeleton />}>
        <Posts />
      </Suspense>
    </div>
  );
}
```

### Selective Hydration

```jsx
// Components hydrate independently as they stream in
function App() {
  return (
    <div>
      {/* Hydrates first */}
      <Header />
      
      {/* Hydrates when content arrives */}
      <Suspense fallback={<MainSkeleton />}>
        <MainContent />
      </Suspense>
      
      {/* Hydrates independently */}
      <Suspense fallback={<CommentsSkeleton />}>
        <Comments />
      </Suspense>
    </div>
  );
}
```

## Best Practices & Patterns

### 1. Suspense Boundary Strategy

```jsx
// ✅ Strategic boundaries for optimal UX
function OptimalBoundaries() {
  return (
    <div>
      {/* Critical navigation - top-level boundary */}
      <Suspense fallback={<NavSkeleton />}>
        <Navigation />
      </Suspense>
      
      <div className="content">
        {/* Independent sections, independent loading */}
        <Suspense fallback={<SidebarSkeleton />}>
          <Sidebar />
        </Suspense>
        
        <main>
          <Suspense fallback={<ContentSkeleton />}>
            <MainContent />
          </Suspense>
        </main>
      </div>
    </div>
  );
}
```

### 2. Preloading Data

```jsx
// Start fetching before rendering
function ProductPage({ productId }) {
  // Preload on hover
  const preloadProduct = () => {
    queryClient.prefetchQuery({
      queryKey: ['product', productId],
      queryFn: () => fetchProduct(productId)
    });
  };
  
  return (
    <Link 
      to={`/product/${productId}`}
      onMouseEnter={preloadProduct}
    >
      View Product
    </Link>
  );
}
```

### 3. Graceful Degradation

```jsx
function UserProfile({ userId }) {
  return (
    <ErrorBoundary fallback={<ProfileError />}>
      <Suspense fallback={<ProfileSkeleton />}>
        <ProfileData userId={userId} />
      </Suspense>
    </ErrorBoundary>
  );
}
```

### 4. Combine Suspense with Transitions

```jsx
function SearchPage() {
  const [query, setQuery] = useState('');
  const [deferredQuery, setDeferredQuery] = useState('');
  const [isPending, startTransition] = useTransition();
  
  const handleSearch = (value) => {
    setQuery(value); // Immediate
    startTransition(() => {
      setDeferredQuery(value); // Deferred
    });
  };
  
  return (
    <div>
      <SearchInput value={query} onChange={handleSearch} />
      
      <div style={{ opacity: isPending ? 0.6 : 1 }}>
        <Suspense fallback={<ResultsSkeleton />}>
          <SearchResults query={deferredQuery} />
        </Suspense>
      </div>
    </div>
  );
}
```

### 5. Progressive Enhancement

```jsx
function Dashboard() {
  return (
    <div>
      {/* Essential content - no Suspense */}
      <DashboardHeader />
      
      {/* Enhanced content - progressive */}
      <Suspense fallback={null}>
        <RecentActivity />
      </Suspense>
      
      <Suspense fallback={null}>
        <Recommendations />
      </Suspense>
    </div>
  );
}
```

## Interview Questions

### Basic Questions

**Q1: What is Suspense and how does it work?**

A: Suspense is a React component that handles loading states declaratively. It "suspends" rendering when a component needs to wait for asynchronous data:

```jsx
<Suspense fallback={<Loading />}>
  <ComponentThatFetchesData />
</Suspense>
```

When a child component suspends (throws a promise), React:
1. Catches the promise
2. Shows the fallback
3. Waits for the promise to resolve
4. Re-renders with the data

**Q2: What is the difference between startTransition and Suspense?**

A: They serve different purposes:
- **Suspense**: Handles loading states for async operations
- **startTransition**: Marks updates as low-priority to keep UI responsive

They work together: use Suspense for loading states and transitions for non-urgent updates like tab switching.

**Q3: What is the `use` hook in React 19?**

A: The `use` hook reads resources (promises, Context) and suspends rendering if needed:

```jsx
function UserProfile({ userId }) {
  const user = use(fetchUser(userId)); // Suspends until data loads
  return <div>{user.name}</div>;
}
```

Unlike hooks like useState, `use` can be called conditionally.

### Intermediate Questions

**Q4: How do you handle errors with Suspense?**

A: Combine Error Boundaries with Suspense:

```jsx
<ErrorBoundary fallback={<ErrorUI />}>
  <Suspense fallback={<Loading />}>
    <DataComponent />
  </Suspense>
</ErrorBoundary>
```

If the suspended promise rejects, the Error Boundary catches it.

**Q5: Explain useTransition and when to use it.**

A: `useTransition` marks state updates as non-urgent:

```jsx
const [isPending, startTransition] = useTransition();

startTransition(() => {
  setTab('posts'); // Non-blocking update
});
```

Use it for:
- Tab switching
- Filter/search updates
- Navigation
- Any update that shouldn't block user input

**Q6: What is concurrent rendering?**

A: Concurrent rendering allows React to:
- Interrupt rendering for urgent updates
- Work on multiple renders simultaneously
- Prioritize updates intelligently
- Keep UI responsive during heavy operations

Enabled by features like Suspense, transitions, and useDeferredValue.

### Advanced Questions

**Q7: How do React Server Components work?**

A: Server Components run only on the server:
1. Render on the server with full backend access
2. Send serialized output to client
3. Client renders without component JavaScript
4. Can pass data to Client Components

Benefits: Reduced bundle size, direct DB access, better performance.

**Q8: Explain streaming SSR and its benefits.**

A: Streaming SSR sends HTML progressively:

Traditional SSR:
1. Server waits for all data
2. Renders complete HTML
3. Sends to client
4. Client hydrates everything

Streaming SSR:
1. Server sends HTML as it renders
2. Fast content shows immediately
3. Slow content streams in with Suspense
4. Selective hydration as content arrives

Benefits: Faster TTFB, better perceived performance, more resilient.

**Q9: How would you implement optimistic UI updates with transitions?**

A: Use transitions for non-blocking updates and revert on error:

```jsx
function TodoList() {
  const [todos, setTodos] = useState([]);
  const [isPending, startTransition] = useTransition();
  
  const deleteTodo = (id) => {
    const previousTodos = todos;
    
    // Optimistic update
    setTodos(todos.filter(t => t.id !== id));
    
    startTransition(async () => {
      try {
        await api.deleteTodo(id);
      } catch (error) {
        // Revert on failure
        setTodos(previousTodos);
        showError('Failed to delete');
      }
    });
  };
  
  return <TodoItems todos={todos} onDelete={deleteTodo} />;
}
```

**Q10: What's the difference between useDeferredValue and useTransition?**

A: Both defer non-urgent updates, but differ in control:

**useDeferredValue**: For values you don't control
```jsx
function Search({ externalQuery }) {
  const deferredQuery = useDeferredValue(externalQuery);
  return <Results query={deferredQuery} />;
}
```

**useTransition**: For state you control + loading state
```jsx
function Search() {
  const [query, setQuery] = useState('');
  const [isPending, startTransition] = useTransition();
  
  const handleChange = (value) => {
    startTransition(() => setQuery(value));
  };
  
  return <Results query={query} loading={isPending} />;
}
```

Choose useDeferredValue when receiving props, useTransition when managing state.

---

**Last Updated: January 2026 - React 19**

*See also: [React Hooks](./03-hooks.md) | [Performance](./11-profiler-performance.md) | [Server Components](./09-server-components.md)*
