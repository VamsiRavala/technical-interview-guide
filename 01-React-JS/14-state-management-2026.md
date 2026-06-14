# React State Management - 2026 Landscape

## Table of Contents
1. [Introduction](#introduction)
2. [Context API (React 19)](#context-api-react-19)
3. [Redux Toolkit](#redux-toolkit)
4. [Zustand](#zustand)
5. [Jotai](#jotai)
6. [Recoil](#recoil)
7. [XState](#xstate)
8. [TanStack Query for Server State](#tanstack-query-for-server-state)
9. [Comparison & When to Use What](#comparison--when-to-use-what)
10. [Modern Patterns & Best Practices](#modern-patterns--best-practices)
11. [Interview Questions](#interview-questions)

## Introduction

The React state management landscape in 2026 offers diverse solutions for different needs. The key is understanding:
- **Client state** vs **Server state**
- **UI state** vs **Business logic**
- **Local** vs **Global** state
- **Performance requirements**
- **Team size and complexity**

## Context API (React 19)

### Basic Usage

```jsx
import { createContext, useContext, useState } from 'react';

const ThemeContext = createContext();

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState('light');
  
  const value = {
    theme,
    toggleTheme: () => setTheme(t => t === 'light' ? 'dark' : 'light')
  };
  
  // React 19: No need for .Provider
  return (
    <ThemeContext value={value}>
      {children}
    </ThemeContext>
  );
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}

// Usage
function Button() {
  const { theme, toggleTheme } = useTheme();
  return (
    <button 
      className={`btn-${theme}`}
      onClick={toggleTheme}
    >
      Toggle Theme
    </button>
  );
}
```

### React 19: use() Hook with Context

```jsx
import { use, createContext } from 'react';

const UserContext = createContext();

function UserProfile() {
  // Can use 'use' conditionally in React 19!
  const user = use(UserContext);
  
  if (!user) {
    return <div>Please log in</div>;
  }
  
  return <div>Welcome, {user.name}</div>;
}
```

### Optimizing Context Performance

#### Problem: All Consumers Re-render

```jsx
// ❌ Bad: All consumers re-render on any change
function AppProvider({ children }) {
  const [user, setUser] = useState(null);
  const [theme, setTheme] = useState('light');
  
  const value = { user, setUser, theme, setTheme };
  
  return (
    <AppContext value={value}>
      {children}
    </AppContext>
  );
}
```

#### Solution 1: Split Contexts

```jsx
// ✅ Good: Split concerns
const UserContext = createContext();
const ThemeContext = createContext();

function AppProvider({ children }) {
  const [user, setUser] = useState(null);
  const [theme, setTheme] = useState('light');
  
  return (
    <UserContext value={{ user, setUser }}>
      <ThemeContext value={{ theme, setTheme }}>
        {children}
      </ThemeContext>
    </UserContext>
  );
}
```

#### Solution 2: Separate State and Dispatch

```jsx
import { createContext, useReducer, useMemo } from 'react';

const StateContext = createContext();
const DispatchContext = createContext();

function appReducer(state, action) {
  switch (action.type) {
    case 'SET_USER':
      return { ...state, user: action.payload };
    case 'SET_THEME':
      return { ...state, theme: action.payload };
    default:
      return state;
  }
}

function AppProvider({ children }) {
  const [state, dispatch] = useReducer(appReducer, {
    user: null,
    theme: 'light'
  });
  
  // dispatch never changes - no re-renders
  return (
    <StateContext value={state}>
      <DispatchContext value={dispatch}>
        {children}
      </DispatchContext>
    </StateContext>
  );
}

// Hooks
export function useAppState() {
  return useContext(StateContext);
}

export function useAppDispatch() {
  return useContext(DispatchContext);
}

// Usage
function UserMenu() {
  const { user } = useAppState();
  const dispatch = useAppDispatch();
  
  const logout = () => {
    dispatch({ type: 'SET_USER', payload: null });
  };
  
  return <div>{user?.name}</div>;
}
```

#### Solution 3: Context Selectors (External Library)

```bash
npm install use-context-selector
```

```jsx
import { createContext, useContextSelector } from 'use-context-selector';

const AppContext = createContext();

function UserName() {
  // Only re-renders when user.name changes
  const name = useContextSelector(AppContext, state => state.user.name);
  return <div>{name}</div>;
}

function ThemeButton() {
  // Only re-renders when theme changes
  const theme = useContextSelector(AppContext, state => state.theme);
  return <button className={theme}>Click</button>;
}
```

### When to Use Context

✅ **Use Context for:**
- Theme settings
- User authentication
- Language/locale
- Feature flags
- Infrequently changing data

❌ **Avoid Context for:**
- Frequently changing data
- Complex state logic
- Performance-critical applications
- Large-scale applications

## Redux Toolkit

### Setup

```bash
npm install @reduxjs/toolkit react-redux
```

```js
// store/index.js
import { configureStore } from '@reduxjs/toolkit';
import userReducer from './userSlice';
import todosReducer from './todosSlice';

export const store = configureStore({
  reducer: {
    user: userReducer,
    todos: todosReducer
  }
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
```

### Creating Slices

```js
// store/todosSlice.js
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

// Async thunk
export const fetchTodos = createAsyncThunk(
  'todos/fetchTodos',
  async (userId) => {
    const response = await fetch(`/api/todos?userId=${userId}`);
    return response.json();
  }
);

const todosSlice = createSlice({
  name: 'todos',
  initialState: {
    items: [],
    status: 'idle',
    error: null
  },
  reducers: {
    addTodo: (state, action) => {
      // Immer allows "mutations"
      state.items.push(action.payload);
    },
    toggleTodo: (state, action) => {
      const todo = state.items.find(t => t.id === action.payload);
      if (todo) {
        todo.completed = !todo.completed;
      }
    },
    removeTodo: (state, action) => {
      state.items = state.items.filter(t => t.id !== action.payload);
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTodos.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(fetchTodos.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
      })
      .addCase(fetchTodos.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      });
  }
});

export const { addTodo, toggleTodo, removeTodo } = todosSlice.actions;
export default todosSlice.reducer;
```

### Using Redux in Components

```jsx
import { useSelector, useDispatch } from 'react-redux';
import { addTodo, toggleTodo, fetchTodos } from './store/todosSlice';

function TodoList() {
  const dispatch = useDispatch();
  const todos = useSelector(state => state.todos.items);
  const status = useSelector(state => state.todos.status);
  
  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchTodos(userId));
    }
  }, [status, dispatch]);
  
  const handleAdd = (text) => {
    dispatch(addTodo({ id: Date.now(), text, completed: false }));
  };
  
  return (
    <div>
      {todos.map(todo => (
        <TodoItem
          key={todo.id}
          todo={todo}
          onToggle={() => dispatch(toggleTodo(todo.id))}
        />
      ))}
    </div>
  );
}
```

### RTK Query (API Integration)

```js
// store/api.js
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export const api = createApi({
  baseQuery: fetchBaseQuery({ baseUrl: '/api' }),
  tagTypes: ['Todo'],
  endpoints: (builder) => ({
    getTodos: builder.query({
      query: () => 'todos',
      providesTags: ['Todo']
    }),
    addTodo: builder.mutation({
      query: (todo) => ({
        url: 'todos',
        method: 'POST',
        body: todo
      }),
      invalidatesTags: ['Todo']
    }),
    updateTodo: builder.mutation({
      query: ({ id, ...patch }) => ({
        url: `todos/${id}`,
        method: 'PATCH',
        body: patch
      }),
      invalidatesTags: ['Todo']
    })
  })
});

export const { useGetTodosQuery, useAddTodoMutation, useUpdateTodoMutation } = api;
```

```jsx
// Using RTK Query
function TodoList() {
  const { data: todos, isLoading, error } = useGetTodosQuery();
  const [addTodo] = useAddTodoMutation();
  const [updateTodo] = useUpdateTodoMutation();
  
  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  
  return (
    <div>
      {todos.map(todo => (
        <TodoItem
          key={todo.id}
          todo={todo}
          onToggle={() => updateTodo({ 
            id: todo.id, 
            completed: !todo.completed 
          })}
        />
      ))}
    </div>
  );
}
```

## Zustand

### Setup

```bash
npm install zustand
```

### Creating Stores

```js
// store/useStore.js
import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

// Simple store
export const useCounterStore = create((set) => ({
  count: 0,
  increment: () => set((state) => ({ count: state.count + 1 })),
  decrement: () => set((state) => ({ count: state.count - 1 })),
  reset: () => set({ count: 0 })
}));

// With TypeScript
interface BearState {
  bears: number;
  increase: () => void;
  removeAll: () => void;
}

export const useBearStore = create<BearState>((set) => ({
  bears: 0,
  increase: () => set((state) => ({ bears: state.bears + 1 })),
  removeAll: () => set({ bears: 0 })
}));

// Complex store with async actions
export const useTodoStore = create(
  devtools(
    persist(
      (set, get) => ({
        todos: [],
        addTodo: (text) =>
          set((state) => ({
            todos: [...state.todos, { id: Date.now(), text, completed: false }]
          })),
        toggleTodo: (id) =>
          set((state) => ({
            todos: state.todos.map((todo) =>
              todo.id === id ? { ...todo, completed: !todo.completed } : todo
            )
          })),
        fetchTodos: async () => {
          const response = await fetch('/api/todos');
          const todos = await response.json();
          set({ todos });
        },
        // Computed values
        completedTodos: () => get().todos.filter(t => t.completed),
        activeTodos: () => get().todos.filter(t => !t.completed)
      }),
      { name: 'todo-storage' }
    )
  )
);
```

### Using Zustand

```jsx
import { useCounterStore, useTodoStore } from './store/useStore';

function Counter() {
  // Subscribe to entire store
  const { count, increment, decrement } = useCounterStore();
  
  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={increment}>+</button>
      <button onClick={decrement}>-</button>
    </div>
  );
}

function TodoList() {
  // Subscribe to specific slice (prevents unnecessary re-renders)
  const todos = useTodoStore((state) => state.todos);
  const addTodo = useTodoStore((state) => state.addTodo);
  const toggleTodo = useTodoStore((state) => state.toggleTodo);
  
  return (
    <div>
      {todos.map(todo => (
        <TodoItem
          key={todo.id}
          todo={todo}
          onToggle={() => toggleTodo(todo.id)}
        />
      ))}
    </div>
  );
}

// Access store outside components
function logTodos() {
  const todos = useTodoStore.getState().todos;
  console.log(todos);
}
```

### Zustand Slices Pattern

```js
// store/slices/userSlice.js
export const createUserSlice = (set, get) => ({
  user: null,
  setUser: (user) => set({ user }),
  logout: () => set({ user: null })
});

// store/slices/settingsSlice.js
export const createSettingsSlice = (set) => ({
  theme: 'light',
  language: 'en',
  setTheme: (theme) => set({ theme }),
  setLanguage: (language) => set({ language })
});

// store/index.js
import { create } from 'zustand';
import { createUserSlice } from './slices/userSlice';
import { createSettingsSlice } from './slices/settingsSlice';

export const useStore = create((...args) => ({
  ...createUserSlice(...args),
  ...createSettingsSlice(...args)
}));
```

## Jotai

### Setup

```bash
npm install jotai
```

### Creating Atoms

```js
// store/atoms.js
import { atom } from 'jotai';

// Primitive atom
export const countAtom = atom(0);

// Derived/computed atom
export const doubledCountAtom = atom((get) => get(countAtom) * 2);

// Writable derived atom
export const incrementAtom = atom(
  (get) => get(countAtom),
  (get, set) => set(countAtom, get(countAtom) + 1)
);

// Async atom
export const userAtom = atom(async (get) => {
  const userId = get(userIdAtom);
  const response = await fetch(`/api/users/${userId}`);
  return response.json();
});

// Atom with write function
export const todosAtom = atom([]);

export const addTodoAtom = atom(
  null,
  (get, set, text) => {
    const newTodo = { id: Date.now(), text, completed: false };
    set(todosAtom, [...get(todosAtom), newTodo]);
  }
);

export const toggleTodoAtom = atom(
  null,
  (get, set, id) => {
    set(
      todosAtom,
      get(todosAtom).map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo
      )
    );
  }
);
```

### Using Jotai

```jsx
import { useAtom, useAtomValue, useSetAtom } from 'jotai';
import { countAtom, doubledCountAtom, incrementAtom } from './store/atoms';

function Counter() {
  const [count, setCount] = useAtom(countAtom);
  const doubled = useAtomValue(doubledCountAtom);
  const increment = useSetAtom(incrementAtom);
  
  return (
    <div>
      <p>Count: {count}</p>
      <p>Doubled: {doubled}</p>
      <button onClick={increment}>Increment</button>
    </div>
  );
}

// With Provider (for isolated stores)
import { Provider } from 'jotai';

function App() {
  return (
    <Provider>
      <Counter />
    </Provider>
  );
}
```

### Jotai Utils

```js
import { atomWithStorage, atomWithReducer, atomFamily } from 'jotai/utils';

// Persist to localStorage
export const themeAtom = atomWithStorage('theme', 'light');

// Reducer pattern
const todoReducer = (state, action) => {
  switch (action.type) {
    case 'ADD':
      return [...state, action.payload];
    case 'REMOVE':
      return state.filter(t => t.id !== action.payload);
    default:
      return state;
  }
};

export const todosAtomWithReducer = atomWithReducer([], todoReducer);

// Atom family (parameterized atoms)
export const userAtomFamily = atomFamily((userId) =>
  atom(async () => {
    const response = await fetch(`/api/users/${userId}`);
    return response.json();
  })
);

// Usage
function UserProfile({ userId }) {
  const user = useAtomValue(userAtomFamily(userId));
  return <div>{user.name}</div>;
}
```

## Recoil

### Setup

```bash
npm install recoil
```

### Creating Atoms and Selectors

```js
// store/atoms.js
import { atom, selector, selectorFamily } from 'recoil';

// Atom
export const countState = atom({
  key: 'countState',
  default: 0
});

// Selector (derived state)
export const doubledCountState = selector({
  key: 'doubledCountState',
  get: ({ get }) => {
    const count = get(countState);
    return count * 2;
  }
});

// Async selector
export const userState = selector({
  key: 'userState',
  get: async ({ get }) => {
    const userId = get(userIdState);
    const response = await fetch(`/api/users/${userId}`);
    return response.json();
  }
});

// Selector family (parameterized)
export const userByIdState = selectorFamily({
  key: 'userByIdState',
  get: (userId) => async () => {
    const response = await fetch(`/api/users/${userId}`);
    return response.json();
  }
});

// Writable selector
export const todoStatsState = selector({
  key: 'todoStatsState',
  get: ({ get }) => {
    const todos = get(todosState);
    return {
      total: todos.length,
      completed: todos.filter(t => t.completed).length,
      active: todos.filter(t => !t.completed).length
    };
  }
});
```

### Using Recoil

```jsx
import { RecoilRoot, useRecoilState, useRecoilValue, useSetRecoilState } from 'recoil';
import { countState, doubledCountState } from './store/atoms';

function Counter() {
  const [count, setCount] = useRecoilState(countState);
  const doubled = useRecoilValue(doubledCountState);
  
  return (
    <div>
      <p>Count: {count}</p>
      <p>Doubled: {doubled}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
    </div>
  );
}

function App() {
  return (
    <RecoilRoot>
      <Counter />
    </RecoilRoot>
  );
}
```

### Recoil Features

```jsx
import { 
  useRecoilStateLoadable, 
  useRecoilCallback, 
  useRecoilTransaction 
} from 'recoil';

// Handle async state
function UserProfile() {
  const userLoadable = useRecoilStateLoadable(userState);
  
  switch (userLoadable.state) {
    case 'hasValue':
      return <div>{userLoadable.contents.name}</div>;
    case 'loading':
      return <div>Loading...</div>;
    case 'hasError':
      return <div>Error: {userLoadable.contents.message}</div>;
  }
}

// Read atoms without causing re-renders
function useLogState() {
  return useRecoilCallback(({ snapshot }) => async () => {
    const count = await snapshot.getPromise(countState);
    console.log('Current count:', count);
  });
}

// Atomic state updates
function useResetAll() {
  return useRecoilTransaction(({ set }) => () => {
    set(countState, 0);
    set(userState, null);
    set(todosState, []);
  });
}
```

## XState

### Setup

```bash
npm install xstate @xstate/react
```

### Creating State Machines

```js
// machines/authMachine.js
import { createMachine, assign } from 'xstate';

export const authMachine = createMachine({
  id: 'auth',
  initial: 'loggedOut',
  context: {
    user: null,
    error: null
  },
  states: {
    loggedOut: {
      on: {
        LOGIN: 'loggingIn'
      }
    },
    loggingIn: {
      invoke: {
        src: 'loginUser',
        onDone: {
          target: 'loggedIn',
          actions: assign({
            user: (context, event) => event.data
          })
        },
        onError: {
          target: 'loggedOut',
          actions: assign({
            error: (context, event) => event.data
          })
        }
      }
    },
    loggedIn: {
      on: {
        LOGOUT: {
          target: 'loggedOut',
          actions: assign({
            user: null
          })
        }
      }
    }
  }
}, {
  services: {
    loginUser: async (context, event) => {
      const response = await fetch('/api/login', {
        method: 'POST',
        body: JSON.stringify(event.credentials)
      });
      return response.json();
    }
  }
});
```

### Using XState

```jsx
import { useMachine } from '@xstate/react';
import { authMachine } from './machines/authMachine';

function LoginForm() {
  const [state, send] = useMachine(authMachine);
  
  const handleSubmit = (credentials) => {
    send({ type: 'LOGIN', credentials });
  };
  
  return (
    <div>
      {state.matches('loggedOut') && (
        <form onSubmit={handleSubmit}>
          <input name="email" />
          <input name="password" type="password" />
          <button type="submit">Log In</button>
        </form>
      )}
      
      {state.matches('loggingIn') && <div>Logging in...</div>}
      
      {state.matches('loggedIn') && (
        <div>
          <p>Welcome, {state.context.user.name}</p>
          <button onClick={() => send('LOGOUT')}>Log Out</button>
        </div>
      )}
      
      {state.context.error && (
        <div className="error">{state.context.error.message}</div>
      )}
    </div>
  );
}
```

### Complex State Machine Example

```js
// machines/todoMachine.js
import { createMachine, assign } from 'xstate';

export const todoMachine = createMachine({
  id: 'todos',
  initial: 'loading',
  context: {
    todos: [],
    error: null
  },
  states: {
    loading: {
      invoke: {
        src: 'fetchTodos',
        onDone: {
          target: 'idle',
          actions: assign({
            todos: (_, event) => event.data
          })
        },
        onError: {
          target: 'error',
          actions: assign({
            error: (_, event) => event.data
          })
        }
      }
    },
    idle: {
      on: {
        ADD_TODO: {
          target: 'creating'
        },
        TOGGLE_TODO: {
          actions: assign({
            todos: (context, event) =>
              context.todos.map(todo =>
                todo.id === event.id
                  ? { ...todo, completed: !todo.completed }
                  : todo
              )
          })
        },
        DELETE_TODO: {
          target: 'deleting'
        }
      }
    },
    creating: {
      invoke: {
        src: 'createTodo',
        onDone: {
          target: 'idle',
          actions: assign({
            todos: (context, event) => [...context.todos, event.data]
          })
        },
        onError: 'idle'
      }
    },
    deleting: {
      invoke: {
        src: 'deleteTodo',
        onDone: {
          target: 'idle',
          actions: assign({
            todos: (context, event) =>
              context.todos.filter(todo => todo.id !== event.data)
          })
        },
        onError: 'idle'
      }
    },
    error: {
      on: {
        RETRY: 'loading'
      }
    }
  }
});
```

## TanStack Query for Server State

### Setup

```bash
npm install @tanstack/react-query
```

### Configuration

```jsx
// App.jsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      cacheTime: 1000 * 60 * 10, // 10 minutes
      retry: 3,
      refetchOnWindowFocus: false
    }
  }
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <YourApp />
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
```

### Basic Queries

```jsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

function UserProfile({ userId }) {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['user', userId],
    queryFn: () => fetch(`/api/users/${userId}`).then(res => res.json()),
    staleTime: 5000
  });
  
  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  
  return (
    <div>
      <h1>{data.name}</h1>
      <button onClick={() => refetch()}>Refresh</button>
    </div>
  );
}
```

### Mutations

```jsx
function CreateTodo() {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: (newTodo) => 
      fetch('/api/todos', {
        method: 'POST',
        body: JSON.stringify(newTodo)
      }).then(res => res.json()),
    onSuccess: () => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: ['todos'] });
    },
    onError: (error) => {
      console.error('Failed to create todo:', error);
    }
  });
  
  const handleSubmit = (text) => {
    mutation.mutate({ text, completed: false });
  };
  
  return (
    <div>
      <form onSubmit={(e) => {
        e.preventDefault();
        handleSubmit(e.target.text.value);
      }}>
        <input name="text" />
        <button disabled={mutation.isPending}>
          {mutation.isPending ? 'Creating...' : 'Add Todo'}
        </button>
      </form>
      {mutation.isError && <div>Error: {mutation.error.message}</div>}
    </div>
  );
}
```

### Advanced Patterns

```jsx
// Optimistic updates
function UpdateTodo() {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: updateTodo,
    onMutate: async (updatedTodo) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['todos'] });
      
      // Snapshot previous value
      const previousTodos = queryClient.getQueryData(['todos']);
      
      // Optimistically update
      queryClient.setQueryData(['todos'], (old) =>
        old.map(todo =>
          todo.id === updatedTodo.id ? updatedTodo : todo
        )
      );
      
      return { previousTodos };
    },
    onError: (err, newTodo, context) => {
      // Rollback on error
      queryClient.setQueryData(['todos'], context.previousTodos);
    },
    onSettled: () => {
      // Always refetch after error or success
      queryClient.invalidateQueries({ queryKey: ['todos'] });
    }
  });
  
  return mutation;
}

// Infinite queries
function InfinitePosts() {
  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage
  } = useInfiniteQuery({
    queryKey: ['posts'],
    queryFn: ({ pageParam = 0 }) =>
      fetch(`/api/posts?page=${pageParam}`).then(res => res.json()),
    getNextPageParam: (lastPage, pages) => lastPage.nextCursor,
    initialPageParam: 0
  });
  
  return (
    <div>
      {data?.pages.map((page, i) => (
        <div key={i}>
          {page.posts.map(post => (
            <PostCard key={post.id} post={post} />
          ))}
        </div>
      ))}
      
      <button
        onClick={() => fetchNextPage()}
        disabled={!hasNextPage || isFetchingNextPage}
      >
        {isFetchingNextPage ? 'Loading...' : 'Load More'}
      </button>
    </div>
  );
}

// Prefetching
function PostList() {
  const queryClient = useQueryClient();
  
  const prefetchPost = (postId) => {
    queryClient.prefetchQuery({
      queryKey: ['post', postId],
      queryFn: () => fetchPost(postId)
    });
  };
  
  return (
    <div>
      {posts.map(post => (
        <Link
          key={post.id}
          to={`/posts/${post.id}`}
          onMouseEnter={() => prefetchPost(post.id)}
        >
          {post.title}
        </Link>
      ))}
    </div>
  );
}
```

## Comparison & When to Use What

### Comparison Table

| Feature | Context | Redux Toolkit | Zustand | Jotai | Recoil | XState | TanStack Query |
|---------|---------|--------------|---------|-------|--------|--------|----------------|
| **Bundle Size** | Built-in | ~12KB | ~1KB | ~3KB | ~14KB | ~15KB | ~12KB |
| **Learning Curve** | Easy | Medium | Easy | Easy | Medium | Hard | Medium |
| **DevTools** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **TypeScript** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Async Support** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅✅ |
| **Middleware** | ❌ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| **Persistence** | Manual | Via middleware | Via middleware | Via utils | Via effects | Via actors | Via persist |
| **Time Travel** | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Server State** | ❌ | Manual | Manual | Manual | Manual | Manual | ✅✅ |

### Decision Tree

```
START: What type of state?
│
├─ SERVER STATE (API data, cache)
│  └─ Use TanStack Query
│
├─ COMPLEX LOGIC / WORKFLOWS
│  └─ Use XState
│
├─ GLOBAL APPLICATION STATE
│  ├─ Large team / need strict patterns?
│  │  └─ Use Redux Toolkit
│  │
│  ├─ Small/medium app, want simplicity?
│  │  └─ Use Zustand
│  │
│  ├─ Need atomic updates / granular?
│  │  ├─ Prefer atoms?
│  │  │  └─ Use Jotai
│  │  └─ Need more features?
│  │     └─ Use Recoil
│  │
│  └─ Simple shared state?
│     └─ Use Context API
│
└─ LOCAL COMPONENT STATE
   └─ Use useState / useReducer
```

### Specific Use Cases

**Context API**
```
✅ Theme, locale, auth
✅ Small apps
✅ Infrequent updates
❌ Frequent updates
❌ Complex logic
```

**Redux Toolkit**
```
✅ Large applications
✅ Complex state logic
✅ Team needs structure
✅ Time-travel debugging
❌ Small apps (overkill)
❌ Simple state needs
```

**Zustand**
```
✅ Simple global state
✅ Quick setup
✅ Small bundle size
✅ No provider needed
❌ Complex workflows
❌ Need strict patterns
```

**Jotai**
```
✅ Atomic state management
✅ Bottom-up approach
✅ Minimal rerenders
✅ Derived state
❌ Complex async flows
❌ Need centralized store
```

**Recoil**
```
✅ Atomic state (like Jotai)
✅ Async selectors
✅ Complex derived state
✅ Facebook ecosystem
❌ Bundle size matters
❌ Simple apps
```

**XState**
```
✅ Complex workflows
✅ State machines
✅ Explicit transitions
✅ Visualize logic
❌ Simple state
❌ Learning curve
```

**TanStack Query**
```
✅ Server state
✅ API data
✅ Caching
✅ Pagination/infinite scroll
❌ Local UI state
❌ Simple fetching
```

## Modern Patterns & Best Practices

### Pattern 1: Separate Client and Server State

```jsx
// ❌ Bad: Mix client and server state
const [user, setUser] = useState(null);
const [theme, setTheme] = useState('light');

// ✅ Good: Separate concerns
// Server state
const { data: user } = useQuery({
  queryKey: ['user'],
  queryFn: fetchUser
});

// Client state
const [theme, setTheme] = useState('light');
```

### Pattern 2: Colocation

```jsx
// ❌ Bad: All state at top level
function App() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedTab, setSelectedTab] = useState('home');
  const [searchQuery, setSearchQuery] = useState('');
  
  return (
    <div>
      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
      <Tabs selected={selectedTab} onChange={setSelectedTab} />
      <Search query={searchQuery} onChange={setSearchQuery} />
    </div>
  );
}

// ✅ Good: Colocate state with components
function App() {
  return (
    <div>
      <Modal /> {/* Manages own state */}
      <Tabs /> {/* Manages own state */}
      <Search /> {/* Manages own state */}
    </div>
  );
}
```

### Pattern 3: Composite Stores

```jsx
// Combine multiple solutions
import { create } from 'zustand';
import { QueryClient } from '@tanstack/react-query';

// Client state (Zustand)
export const useUIStore = create((set) => ({
  theme: 'light',
  sidebar: 'collapsed',
  toggleTheme: () => set(state => ({ 
    theme: state.theme === 'light' ? 'dark' : 'light' 
  }))
}));

// Server state (TanStack Query)
export const queryClient = new QueryClient();

// Complex workflows (XState)
export const authMachine = createMachine({...});

function App() {
  const theme = useUIStore(state => state.theme);
  const { data: user } = useQuery(['user'], fetchUser);
  const [state, send] = useMachine(authMachine);
  
  return (/* ... */);
}
```

### Pattern 4: Optimistic Updates

```jsx
function useOptimisticTodo() {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: toggleTodo,
    onMutate: async (todoId) => {
      await queryClient.cancelQueries(['todos']);
      
      const previous = queryClient.getQueryData(['todos']);
      
      queryClient.setQueryData(['todos'], old =>
        old.map(todo =>
          todo.id === todoId
            ? { ...todo, completed: !todo.completed }
            : todo
        )
      );
      
      return { previous };
    },
    onError: (err, variables, context) => {
      queryClient.setQueryData(['todos'], context.previous);
      showError('Failed to update todo');
    },
    onSettled: () => {
      queryClient.invalidateQueries(['todos']);
    }
  });
  
  return mutation;
}
```

### Pattern 5: Selective Subscriptions

```jsx
// ❌ Bad: Subscribe to everything
function UserName() {
  const { user, theme, settings } = useStore();
  return <div>{user.name}</div>;
}

// ✅ Good: Subscribe to only what you need
function UserName() {
  const name = useStore(state => state.user.name);
  return <div>{name}</div>;
}
```

## Interview Questions

### Basic Questions

**Q1: What's the difference between local and global state?**

A: 
- **Local state**: Owned by single component, managed with useState/useReducer
- **Global state**: Shared across components, managed with Context/Redux/Zustand/etc.

Choose local state by default, lift to global only when needed.

**Q2: When should you use Context vs a state management library?**

A: Use Context for:
- Simple shared state
- Infrequent updates
- Theme, auth, locale

Use state library for:
- Frequent updates
- Complex state logic
- Performance-critical apps
- Large teams needing structure

**Q3: What is server state and why treat it differently?**

A: Server state is data from APIs that:
- You don't own
- Can be outdated (stale)
- Needs caching
- Requires synchronization

Use TanStack Query instead of Redux/Zustand for server state.

### Intermediate Questions

**Q4: How do you optimize Context performance?**

A: Multiple strategies:
```jsx
// 1. Split contexts
<UserContext value={userData}>
  <ThemeContext value={themeData}>

// 2. Separate state and dispatch
<StateContext value={state}>
  <DispatchContext value={dispatch}>

// 3. Use selectors (external library)
const name = useContextSelector(ctx, s => s.user.name);

// 4. Memoize values
const value = useMemo(() => ({ user, theme }), [user, theme]);
```

**Q5: Compare Zustand and Redux Toolkit.**

A:
**Zustand**:
- Minimal boilerplate
- 1KB bundle
- No provider needed
- Simple API
- Good for small/medium apps

**Redux Toolkit**:
- More structure
- DevTools time-travel
- Middleware ecosystem
- RTK Query for APIs
- Good for large apps

**Q6: Explain atomic state management (Jotai/Recoil).**

A: Atomic approach:
- State split into small atoms
- Components subscribe to specific atoms
- Minimal re-renders
- Bottom-up composition

```jsx
const nameAtom = atom('John');
const ageAtom = atom(25);

// Only re-renders when name changes
function Name() {
  const name = useAtomValue(nameAtom);
  return <div>{name}</div>;
}
```

### Advanced Questions

**Q7: How would you structure state in a large e-commerce app?**

A: Hybrid approach:
```jsx
// 1. Server state (TanStack Query)
const { data: products } = useQuery(['products'], fetchProducts);
const { data: user } = useQuery(['user'], fetchUser);

// 2. UI state (Zustand)
const useUIStore = create((set) => ({
  cartOpen: false,
  filterPanelOpen: true
}));

// 3. Cart state (Zustand with persistence)
const useCartStore = create(
  persist((set) => ({
    items: [],
    addItem: (item) => set(state => ({
      items: [...state.items, item]
    }))
  }), { name: 'cart' })
);

// 4. Checkout workflow (XState)
const checkoutMachine = createMachine({
  initial: 'cart',
  states: {
    cart: { on: { CHECKOUT: 'shipping' } },
    shipping: { on: { NEXT: 'payment' } },
    payment: { on: { PAY: 'confirming' } },
    confirming: { /* ... */ }
  }
});
```

**Q8: Implement optimistic updates with rollback.**

A:
```jsx
function useOptimisticUpdate() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: updateItem,
    onMutate: async (newItem) => {
      // Cancel queries
      await queryClient.cancelQueries(['items']);
      
      // Snapshot
      const previous = queryClient.getQueryData(['items']);
      
      // Optimistic update
      queryClient.setQueryData(['items'], old =>
        old.map(item => item.id === newItem.id ? newItem : item)
      );
      
      return { previous };
    },
    onError: (err, newItem, context) => {
      // Rollback
      queryClient.setQueryData(['items'], context.previous);
      toast.error('Update failed');
    },
    onSuccess: () => {
      toast.success('Updated!');
    },
    onSettled: () => {
      // Refetch regardless
      queryClient.invalidateQueries(['items']);
    }
  });
}
```

**Q9: How do you handle complex async workflows?**

A: Use XState for complex state machines:
```jsx
const fetchMachine = createMachine({
  initial: 'idle',
  states: {
    idle: {
      on: { FETCH: 'loading' }
    },
    loading: {
      invoke: {
        src: 'fetchData',
        onDone: 'success',
        onError: 'failure'
      },
      after: {
        10000: 'timeout'
      }
    },
    success: {
      on: { REFETCH: 'loading' }
    },
    failure: {
      on: { RETRY: 'loading' }
    },
    timeout: {
      on: { RETRY: 'loading' }
    }
  }
});
```

Benefits:
- Explicit state transitions
- Impossible states prevented
- Easy to visualize
- Handles timeouts/retries

**Q10: What's your recommendation for state management in 2026?**

A: Modern stack:
```jsx
// 1. TanStack Query for ALL server state
const { data } = useQuery(['users'], fetchUsers);

// 2. Zustand for global UI state
const useUIStore = create((set) => ({
  theme: 'light',
  sidebar: 'open'
}));

// 3. useState for local state
const [isOpen, setIsOpen] = useState(false);

// 4. XState for complex workflows (optional)
const [state, send] = useMachine(checkoutMachine);

// 5. Context for rarely-changing values
<ThemeContext value={theme}>
```

This covers 95% of use cases efficiently.

---

**Last Updated: January 2026 - React 19**

*See also: [React Hooks](./02-hooks-complete-guide.md) | [Performance](./11-profiler-performance.md) | [Suspense](./12-suspense-concurrent.md) | [Testing](./13-testing.md)*
