# Quick Review - Comprehensive Guide for Senior Developers (13+ Years Experience)

---

## 📘 React JS - Complete Coverage

### **1. React Fundamentals**
**Definition**: React is a declarative, component-based JavaScript library developed by Facebook for building dynamic, high-performance user interfaces. It uses a virtual DOM to efficiently update and render components, follows unidirectional data flow, and enables building complex UIs from small, isolated pieces of code called components.

**Key Concepts**: 
- Component-based architecture promotes reusability and maintainability
- Declarative approach makes code more predictable and easier to debug
- Virtual DOM minimizes expensive DOM operations

```jsx
// Functional Component with Props
function UserProfile({ user, onUpdate }) {
  return (
    <div className="profile">
      <h2>{user.name}</h2>
      <p>Email: {user.email}</p>
      <button onClick={() => onUpdate(user.id)}>Update</button>
    </div>
  );
}

// Usage
<UserProfile 
  user={{ id: 1, name: "Vamsi", email: "vamsi@example.com" }} 
  onUpdate={handleUpdate} 
/>
```

### **2. React Hooks - useState**
**Definition**: useState is a Hook that allows you to add React state to functional components. It returns a stateful value and a function to update it. During the initial render, the returned state matches the value passed as the first argument. The setState function is used to update the state and trigger a re-render.

**When to Use**: 
- Managing local component state
- Handling form inputs
- Toggle states, counters, flags

```jsx
function ShoppingCart() {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);

  const addItem = (item) => {
    setItems(prevItems => [...prevItems, item]);
    setTotal(prevTotal => prevTotal + item.price);
  };

  const removeItem = (itemId) => {
    setItems(prevItems => prevItems.filter(item => item.id !== itemId));
    const removedItem = items.find(item => item.id === itemId);
    setTotal(prevTotal => prevTotal - removedItem.price);
  };

  return (
    <div>
      <h2>Cart Total: ${total}</h2>
      {items.map(item => (
        <div key={item.id}>
          {item.name} - ${item.price}
          <button onClick={() => removeItem(item.id)}>Remove</button>
        </div>
      ))}
    </div>
  );
}
```

### **3. React Hooks - useEffect**
**Definition**: useEffect Hook lets you perform side effects in functional components. It serves the same purpose as componentDidMount, componentDidUpdate, and componentWillUnmount in class components. Effects run after every completed render by default, but you can control when they run using the dependency array.

**Key Points**:
- First argument: function containing side effect logic
- Second argument: dependency array (controls when effect runs)
- Return cleanup function for subscriptions or timers
- Empty array [] means run once on mount
- No array means run after every render

```jsx
function UserDashboard({ userId }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  // Effect with dependency - runs when userId changes
  useEffect(() => {
    let cancelled = false;

    const fetchUser = async () => {
      setLoading(true);
      try {
        const response = await fetch(`/api/users/${userId}`);
        const data = await response.json();
        if (!cancelled) {
          setUser(data);
        }
      } catch (error) {
        console.error('Failed to fetch user:', error);
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    fetchUser();

    // Cleanup function
    return () => {
      cancelled = true;
    };
  }, [userId]); // Only re-run if userId changes

  // Effect for WebSocket connection
  useEffect(() => {
    const ws = new WebSocket('ws://localhost:8080');
    
    ws.onmessage = (event) => {
      console.log('Message received:', event.data);
    };

    // Cleanup WebSocket on unmount
    return () => {
      ws.close();
    };
  }, []); // Empty array - run once on mount

  if (loading) return <div>Loading...</div>;
  return <div>Welcome, {user?.name}</div>;
}
```

### **4. React Hooks - useContext**
**Definition**: useContext Hook allows you to consume context values without wrapping components in Context.Consumer. Context provides a way to pass data through the component tree without manually passing props at every level, solving the "prop drilling" problem.

```jsx
// Create Context
const ThemeContext = React.createContext();
const UserContext = React.createContext();

// Provider Component
function App() {
  const [theme, setTheme] = useState('dark');
  const [user, setUser] = useState({ name: 'Vamsi', role: 'Lead' });

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      <UserContext.Provider value={{ user, setUser }}>
        <Dashboard />
      </UserContext.Provider>
    </ThemeContext.Provider>
  );
}

// Consumer Component
function Dashboard() {
  const { theme, setTheme } = useContext(ThemeContext);
  const { user } = useContext(UserContext);

  return (
    <div className={`dashboard ${theme}`}> 
      <h1>Welcome, {user.name}</h1>
      <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}> 
        Toggle Theme
      </button>
    </div>
  );
}
```

### **5. React Hooks - useReducer**
**Definition**: useReducer is a Hook for managing complex state logic. It's an alternative to useState when you have complex state logic involving multiple sub-values or when the next state depends on the previous one. It follows Redux pattern with actions and reducers.

```jsx
// Reducer function
const cartReducer = (state, action) => {
  switch (action.type) {
    case 'ADD_ITEM':
      return {
        ...state,
        items: [...state.items, action.payload],
        total: state.total + action.payload.price
      };
    case 'REMOVE_ITEM':
      const itemToRemove = state.items.find(item => item.id === action.payload);
      return {
        ...state,
        items: state.items.filter(item => item.id !== action.payload),
        total: state.total - itemToRemove.price
      };
    case 'CLEAR_CART':
      return { items: [], total: 0 };
    case 'APPLY_DISCOUNT':
      return {
        ...state,
        total: state.total * (1 - action.payload / 100)
      };
    default:
      return state;
  }
};

function ShoppingCart() {
  const [state, dispatch] = useReducer(cartReducer, { items: [], total: 0 });

  const addItem = (item) => {
    dispatch({ type: 'ADD_ITEM', payload: item });
  };

  const removeItem = (itemId) => {
    dispatch({ type: 'REMOVE_ITEM', payload: itemId });
  };

  const applyDiscount = (percentage) => {
    dispatch({ type: 'APPLY_DISCOUNT', payload: percentage });
  };

  return (
    <div>
      <h2>Total: ${state.total.toFixed(2)}</h2>
      <button onClick={() => applyDiscount(10)}>Apply 10% Discount</button>
      <button onClick={() => dispatch({ type: 'CLEAR_CART' })}>Clear Cart</button>
    </div>
  );
}
```

### **6. React Hooks - useMemo**
**Definition**: useMemo is a Hook that memoizes expensive calculations, returning a memoized value that only recomputes when dependencies change. It's used for performance optimization to avoid expensive calculations on every render.

```jsx
function ProductList({ products, filter, sortBy }) {
  // Expensive filtering operation - only recalculates when products or filter changes
  const filteredProducts = useMemo(() => {
    console.log('Filtering products...');
    return products.filter(product => 
      product.name.toLowerCase().includes(filter.toLowerCase())
    );
  }, [products, filter]);

  // Expensive sorting operation - only recalculates when filtered products or sortBy changes
  const sortedProducts = useMemo(() => {
    console.log('Sorting products...');
    return [...filteredProducts].sort((a, b) => {
      if (sortBy === 'price') return a.price - b.price;
      if (sortBy === 'name') return a.name.localeCompare(b.name);
      return 0;
    });
  }, [filteredProducts, sortBy]);

  return (
    <div>
      {sortedProducts.map(product => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
}
```

### **7. React Hooks - useCallback**
**Definition**: useCallback returns a memoized callback function that only changes if dependencies change. It's useful when passing callbacks to optimized child components that rely on reference equality to prevent unnecessary renders.

```jsx
function ParentComponent() {
  const [count, setCount] = useState(0);
  const [items, setItems] = useState([]);

  // Without useCallback - new function created on every render
  // const handleItemClick = (id) => {
  //   console.log('Item clicked:', id);
  // };

  // With useCallback - same function reference unless dependencies change
  const handleItemClick = useCallback((id) => {
    console.log('Item clicked:', id);
  }, [items]);

  const handleAddItem = useCallback(() => {
    setItems(prev => [...prev, { id: Date.now(), name: `Item ${prev.length + 1}` }]);
  }, []);

  return (
    <div>
      <h2>Count: {count}</h2>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      <button onClick={handleAddItem}>Add Item</button>
      {/* ItemList won't re-render when count changes because handleItemClick reference is stable */}
      <ItemList items={items} onItemClick={handleItemClick} />
    </div>
  );
}

// Memoized child component
const ItemList = React.memo(({ items, onItemClick }) => {
  console.log('ItemList rendered');
  return (
    <ul>
      {items.map(item => (
        <li key={item.id} onClick={() => onItemClick(item.id)}>
          {item.name}
        </li>
      ))}
    </ul>
  );
});
```

### **8. React Hooks - useRef**
**Definition**: useRef returns a mutable ref object whose .current property persists across renders. It's used for accessing DOM elements directly, storing mutable values that don't cause re-renders when updated, and keeping previous values.

```jsx
function VideoPlayer() {
  const videoRef = useRef(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const previousValue = useRef(0);
  const renderCount = useRef(0);

  // Increment render count without causing re-render
  useEffect(() => {
    renderCount.current += 1;
  });

  const togglePlay = () => {
    if (isPlaying) {
      videoRef.current.pause();
    } else {
      videoRef.current.play();
    }
    setIsPlaying(!isPlaying);
  };

  const seekTo = (seconds) => {
    videoRef.current.currentTime = seconds;
  };

  return (
    <div>
      <video ref={videoRef} src="/video.mp4" />
      <button onClick={togglePlay}>{isPlaying ? 'Pause' : 'Play'}</button>
      <button onClick={() => seekTo(30)}>Skip to 30s</button>
      <p>Component rendered {renderCount.current} times</p>
    </div>
  );
}

// Using useRef for focus management
function LoginForm() {
  const usernameRef = useRef(null);
  const passwordRef = useRef(null);

  useEffect(() => {
    // Auto-focus username input on mount
    usernameRef.current.focus();
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!usernameRef.current.value) {
      usernameRef.current.focus();
      return;
    }
    if (!passwordRef.current.value) {
      passwordRef.current.focus();
      return;
    }
    // Submit form
  };

  return (
    <form onSubmit={handleSubmit}>
      <input ref={usernameRef} type="text" placeholder="Username" />
      <input ref={passwordRef} type="password" placeholder="Password" />
      <button type="submit">Login</button>
    </form>
  );
}
```

### **9. React Hooks - Custom Hooks**
**Definition**: Custom Hooks are JavaScript functions whose names start with "use" and that may call other Hooks. They let you extract component logic into reusable functions, enabling code sharing between components without wrapper components.

```jsx
// Custom Hook for API calls
function useApi(url) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch');
        const result = await response.json();
        setData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [url]);

  return { data, loading, error };
}

// Custom Hook for form handling
function useForm(initialValues, onSubmit) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setValues(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const validationErrors = validate(values);
    if (Object.keys(validationErrors).length === 0) {
      onSubmit(values);
    } else {
      setErrors(validationErrors);
    }
  };

  const validate = (values) => {
    const errors = {};
    if (!values.email) errors.email = 'Email is required';
    if (!values.password) errors.password = 'Password is required';
    return errors;
  };

  return { values, errors, handleChange, handleSubmit };
}

// Custom Hook for local storage
function useLocalStorage(key, initialValue) {
  const [storedValue, setStoredValue] = useState(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(error);
      return initialValue;
    }
  });

  const setValue = (value) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value;
      setStoredValue(valueToStore);
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      console.error(error);
    }
  };

  return [storedValue, setValue];
}

// Usage of custom hooks
function UserProfile() {
  const { data: user, loading, error } = useApi('/api/user/profile');
  const [theme, setTheme] = useLocalStorage('theme', 'light');
  const { values, errors, handleChange, handleSubmit } = useForm(
    { email: '', password: '' },
    (data) => console.log('Form submitted:', data)
  );

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className={theme}>
      <h1>Welcome, {user.name}</h1>
      <button onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}>
        Toggle Theme
      </button>
      <form onSubmit={handleSubmit}>
        <input name="email" value={values.email} onChange={handleChange} />
        {errors.email && <span>{errors.email}</span>}
        <input name="password" type="password" value={values.password} onChange={handleChange} />
        {errors.password && <span>{errors.password}</span>}
        <button type="submit">Update</button>
      </form>
    </div>
  );
}
```

### **10. Virtual DOM**
**Definition**: The Virtual DOM is a lightweight, in-memory representation of the actual DOM. React maintains a virtual DOM tree and whenever state changes, it creates a new virtual DOM tree, compares it with the previous one using a diffing algorithm, and calculates the minimum number of changes needed to update the real DOM. This process is called reconciliation and makes React highly performant.

**Why Virtual DOM?**:
- Direct DOM manipulation is expensive
- Batch updates for better performance
- Enables declarative programming model
- Cross-platform rendering (React Native)

```jsx
// Virtual DOM Concept Illustration
function Counter() {
  const [count, setCount] = useState(0);

  // When count changes:
  // 1. React creates new Virtual DOM tree
  // 2. Compares with previous Virtual DOM (diffing)
  // 3. Calculates minimal changes
  // 4. Updates only changed parts in real DOM

  return (
    <div>
      <h1>Count: {count}</h1> {/* Only this text node updates in real DOM */}
      <button onClick={() => setCount(count + 1)}>Increment</button>
    </div>
  );
}
```

### **11. Reconciliation & Diffing Algorithm**
**Definition**: Reconciliation is the algorithm React uses to diff one tree with another to determine which parts need to be changed. React implements a heuristic O(n) algorithm based on two assumptions: 1) Elements of different types produce different trees, 2) Developer can hint at stable elements with key prop.

**Diffing Rules**:
- Different element types → destroy old tree, build new tree
- Same element type → keep DOM node, update changed attributes
- Component elements → update props, call lifecycle methods
- Recursing on children → uses keys to match elements

```jsx
// Example demonstrating importance of keys in reconciliation
function TodoList() {
  const [todos, setTodos] = useState([
    { id: 1, text: 'Learn React' },
    { id: 2, text: 'Build Project' },
    { id: 3, text: 'Deploy' }
  ]);

  // ❌ BAD: Using index as key
  // When list reorders, React recreates DOM nodes unnecessarily
  return (
    <ul>
      {todos.map((todo, index) => (
        <li key={index}>{todo.text}</li>
      ))}
    </ul>
  );

  // ✅ GOOD: Using stable unique id as key
  // React can identify same items and reorder DOM nodes efficiently
  return (
    <ul>
      {todos.map(todo => (
        <li key={todo.id}>{todo.text}</li>
      ))}
    </ul>
  );
}

// Example showing element type change
function App() {
  const [showDiv, setShowDiv] = useState(true);

  return (
    <div>
      {showDiv ? (
        // When toggled, React destroys old <div> and creates new <section>
        <div>Content as div</div>
      ) : (
        <section>Content as section</section>
      )}
    </div>
  );
}
```

### **12. JSX (JavaScript XML)**
**Definition**: JSX is a syntax extension for JavaScript that looks similar to XML/HTML. It allows you to write HTML-like code in JavaScript files, which gets transformed into React.createElement() calls by Babel. JSX makes React code more readable and allows you to use the full power of JavaScript within markup.

```jsx
// JSX is transformed to JavaScript
// JSX:
const element = <h1 className="greeting">Hello, {name}!</h1>;

// Transforms to:
const element = React.createElement(
  'h1',
  { className: 'greeting' },
  'Hello, ',
  name,
  '!'
);

// JSX with expressions
function UserGreeting({ user, isLoggedIn }) {
  return (
    <div>
      {/* JavaScript expressions in curly braces */}
      <h1>{isLoggedIn ? `Welcome back, ${user.name}` : 'Please sign in'}</h1>
      
      {/* Conditional rendering */}
      {user.isAdmin && <AdminPanel />}
      
      {/* Mapping arrays */}
      <ul>
        {user.permissions.map(permission => (
          <li key={permission.id}>{permission.name}</li>
        ))}
      </ul>
      
      {/* Inline styles */}
      <p style={{ color: 'blue', fontSize: '16px' }}>
        Member since: {new Date(user.createdAt).toLocaleDateString()}
      </p>
      
      {/* Event handlers */}
      <button onClick={() => handleLogout(user.id)}>Logout</button>
    </div>
  );
}
```

### **13. Props (Properties)**
**Definition**: Props (short for properties) are arguments passed into React components, similar to function parameters. Props are immutable and flow down from parent to child components (unidirectional data flow). They allow components to be dynamic and reusable.

```jsx
// Basic Props
function ProductCard({ product, onAddToCart }) {
  return (
    <div className="product-card">
      <img src={product.imageUrl} alt={product.name} />
      <h3>{product.name}</h3>
      <p>{product.description}</p>
      <p className="price">${product.price}</p>
      <button onClick={() => onAddToCart(product)}>Add to Cart</button>
    </div>
  );
}

// Props with default values
function Button({ text = 'Click me', variant = 'primary', onClick }) {
  return (
    <button className={`btn btn-${variant}`} onClick={onClick}>
      {text}
    </button>
  );
}

// Props destructuring with rest operator
function Card({ title, children, ...otherProps }) {
  return (
    <div className="card" {...otherProps}>
      <h2>{title}</h2>
      <div className="card-body">{children}</div>
    </div>
  );
}

// PropTypes for type checking (runtime validation)
import PropTypes from 'prop-types';

function User({ name, age, email, role }) {
  return (
    <div>
      <h2>{name}</h2>
      <p>Age: {age}</p>
      <p>Email: {email}</p>
      <p>Role: {role}</p>
    </div>
  );
}

User.propTypes = {
  name: PropTypes.string.isRequired,
  age: PropTypes.number.isRequired,
  email: PropTypes.string.isRequired,
  role: PropTypes.oneOf(['admin', 'user', 'guest']).isRequired
};

User.defaultProps = {
  role: 'user'
};
```

### **14. State Management**
**Definition**: State management refers to how you handle data that changes over time in your application. React provides built-in solutions (useState, useReducer, Context API) for local and global state, while external libraries (Redux, Zustand, Recoil, Jotai) offer more advanced patterns for complex applications.

```jsx
// Local State with useState
function Counter() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount(count + 1)}>Count: {count}</button>;
}

// Global State with Context API
const AppContext = React.createContext();

function AppProvider({ children }) {
  const [user, setUser] = useState(null);
  const [cart, setCart] = useState([]);
  const [theme, setTheme] = useState('light');

  const login = async (credentials) => {
    const user = await authService.login(credentials);
    setUser(user);
  };

  const logout = () => {
    setUser(null);
    setCart([]);
  };

  const addToCart = (item) => {
    setCart(prev => [...prev, item]);
  };

  const value = {
    user,
    cart,
    theme,
    login,
    logout,
    addToCart,
    setTheme
  };

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

// Custom hook for consuming context
function useApp() {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within AppProvider');
  }
  return context;
}

// Redux-style state management with useReducer
const initialState = {
  user: null,
  cart: [],
  loading: false,
  error: null
};

function appReducer(state, action) {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, loading: true, error: null };
    case 'LOGIN_SUCCESS':
      return { ...state, loading: false, user: action.payload };
    case 'LOGIN_ERROR':
      return { ...state, loading: false, error: action.payload };
    case 'LOGOUT':
      return { ...state, user: null, cart: [] };
    case 'ADD_TO_CART':
      return { ...state, cart: [...state.cart, action.payload] };
    default:
      return state;
  }
}

function App() {
  const [state, dispatch] = useReducer(appReducer, initialState);

  const login = async (credentials) => {
    dispatch({ type: 'LOGIN_START' });
    try {
      const user = await authService.login(credentials);
      dispatch({ type: 'LOGIN_SUCCESS', payload: user });
    } catch (error) {
      dispatch({ type: 'LOGIN_ERROR', payload: error.message });
    }
  };

  return (
    <AppContext.Provider value={{ state, dispatch, login }}>
      <Dashboard />
    </AppContext.Provider>
  );
}
```

### **15. Error Boundaries**
**Definition**: Error Boundaries are React components that catch JavaScript errors anywhere in their child component tree, log those errors, and display a fallback UI instead of crashing the entire application. They catch errors during rendering, in lifecycle methods, and in constructors of the whole tree below them. Note: Error boundaries do NOT catch errors in event handlers, asynchronous code, or the error boundary itself.

```jsx
// Error Boundary Component (must be class component as of React 18)
class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null
    };
  }

  static getDerivedStateFromError(error) {
    // Update state so next render shows fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // Log error to error reporting service
    console.error('Error caught by boundary:', error, errorInfo);
    
    // Send to monitoring service (e.g., Sentry)
    logErrorToService(error, errorInfo);
    
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
  }

  render() {
    if (this.state.hasError) {
      // Custom fallback UI
      return (
        <div className="error-container">
          <h2>Something went wrong</h2>
          <details style={{ whiteSpace: 'pre-wrap' }}>
            {this.state.error && this.state.error.toString()}
            <br />
            {this.state.errorInfo && this.state.errorInfo.componentStack}
          </details>
          <button onClick={() => window.location.reload()}>
            Reload Page
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}

// Usage
function App() {
  return (
    <ErrorBoundary>
      <Header />
      <ErrorBoundary>
        <Sidebar />
      </ErrorBoundary>
      <ErrorBoundary>
        <MainContent />
      </ErrorBoundary>
      <Footer />
    </ErrorBoundary>
  );
}

// Component that might throw error
function ProblematicComponent({ data }) {
  if (!data) {
    throw new Error('Data is required!');
  }
  return <div>{data.value}</div>;
}
```

### **16. React Portals**
**Definition**: Portals provide a way to render children into a DOM node that exists outside the DOM hierarchy of the parent component. This is useful for modals, tooltips, dropdowns, and any UI element that needs to "break out" of its container while maintaining React's event bubbling behavior.

```jsx
import { createPortal } from 'react-dom';

// Modal Component using Portal
function Modal({ isOpen, onClose, children }) {
  if (!isOpen) return null;

  // Render modal into document.body instead of parent component
  return createPortal(
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>×</button>
        {children}
      </div>
    </div>,
    document.body // Target DOM node
  );
}

// Tooltip Component using Portal
function Tooltip({ children, text, position }) {
  const [isVisible, setIsVisible] = useState(false);
  const [coords, setCoords] = useState({ x: 0, y: 0 });
  const targetRef = useRef(null);

  const showTooltip = () => {
    const rect = targetRef.current.getBoundingClientRect();
    setCoords({
      x: rect.left + rect.width / 2,
      y: rect.top
    });
    setIsVisible(true);
  };

  return (
    <> 
      <div
        ref={targetRef}
        onMouseEnter={showTooltip}
        onMouseLeave={() => setIsVisible(false)}
      >
        {children}
      </div>
      {isVisible && createPortal(
        <div
          className="tooltip"
          style={{
            position: 'fixed',
            left: coords.x,
            top: coords.y - 30,
            transform: 'translateX(-50%)'
          }}
        >
          {text}
        </div>,
        document.body
      )}
    </>
  );
}

// Dropdown using Portal (prevents overflow issues)
function Dropdown({ trigger, items }) {
  const [isOpen, setIsOpen] = useState(false);
  const [position, setPosition] = useState({ top: 0, left: 0 });
  const triggerRef = useRef(null);

  const toggleDropdown = () => {
    if (!isOpen) {
      const rect = triggerRef.current.getBoundingClientRect();
      setPosition({
        top: rect.bottom + window.scrollY,
        left: rect.left + window.scrollX
      });
    }
    setIsOpen(!isOpen);
  };

  return (
    <> 
      <div ref={triggerRef} onClick={toggleDropdown}>
        {trigger}
      </div>
      {isOpen && createPortal(
        <div
          className="dropdown"
          style={{
            position: 'absolute',
            top: position.top,
            left: position.left
          }}
        >
          {items.map((item, index) => (
            <div key={index} className="dropdown-item" onClick={() => {
              item.onClick();
              setIsOpen(false);
            }}>
              {item.label}
            </div>
          ))}
        </div>,
        document.body
      )}
    </>
  );
}

// Usage
function App() {
  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <div className="app">
      <button onClick={() => setIsModalOpen(true)}>Open Modal</button>
      
      <Tooltip text="This is a helpful tip">
        <span>Hover me</span>
      </Tooltip>
      
      <Dropdown
        trigger={<button>Menu</button>}
        items={[
          { label: 'Profile', onClick: () => console.log('Profile') },
          { label: 'Settings', onClick: () => console.log('Settings') },
          { label: 'Logout', onClick: () => console.log('Logout') }
        ]}
      />
      
      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)}>
        <h2>Modal Title</h2>
        <p>Modal content goes here</p>
      </Modal>
    </div>
  );
}
```

---

## 📘 .NET Web APIs & C# - Complete Coverage

### **1. C# OOP - Encapsulation**
**Definition**: Encapsulation is the bundling of data (fields) and methods that operate on that data within a single unit (class), while hiding the internal state and requiring all interaction through well-defined public methods. It's achieved using access modifiers (private, protected, public, internal) and properties. Encapsulation protects object integrity by preventing outside code from directly accessing internal state, enforcing validation rules, and allowing implementation changes without affecting external code.

**Benefits**:
- Data hiding and protection
- Modularity and maintainability
- Flexibility to change implementation
- Control over data through getters/setters

```csharp
// Complete encapsulation example
public class BankAccount
{
    // Private fields - hidden from outside access
    private string _accountNumber;
    private decimal _balance;
    private string _accountHolderName;
    private DateTime _createdDate;
    private List<Transaction> _transactions;

    // Public properties with validation
    public string AccountNumber
    {
        get => _accountNumber;
        private set // Private setter - can only be set internally
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Account number cannot be empty");
            _accountNumber = value;
        }
    }

    public decimal Balance
    {
        get => _balance;
        private set => _balance = value; // Balance can only be modified through Deposit/Withdraw
    }

    public string AccountHolderName
    {
        get => _accountHolderName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Account holder name is required");
            _accountHolderName = value;
        }
    }

    // Read-only property (no setter)
    public DateTime CreatedDate => _createdDate;

    // Constructor
    public BankAccount(string accountNumber, string accountHolderName, decimal initialDeposit)
    {
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
        _createdDate = DateTime.Now;
        _transactions = new List<Transaction>();
        
        if (initialDeposit > 0)
        {
            Deposit(initialDeposit, "Initial deposit");
        }
    }

    // Public methods providing controlled access
    public void Deposit(decimal amount, string description)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");

        _balance += amount;
        _transactions.Add(new Transaction
        {
            Type = TransactionType.Deposit,
            Amount = amount,
            Description = description,
            Date = DateTime.Now,
            BalanceAfter = _balance
        });
    }

    public void Withdraw(decimal amount, string description)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");

        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds");

        _balance -= amount;
        _transactions.Add(new Transaction
        {
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Description = description,
            Date = DateTime.Now,
            BalanceAfter = _balance
        });
    }

    // Method to get transaction history (returns copy, not reference)
    public IReadOnlyList<Transaction> GetTransactionHistory()
    {
        return _transactions.AsReadOnly();
    }

    // Private helper method - internal implementation detail
    private void ValidateTransaction(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
    }
}

public class Transaction
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public decimal BalanceAfter { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal
}

// Usage
var account = new BankAccount("ACC001", "Vamsi Ravala", 1000);
account.Deposit(500, "Salary");
account.Withdraw(200, "ATM Withdrawal");
// account._balance = 5000; // ERROR: Cannot access private field
// account.Balance = 5000;   // ERROR: Balance setter is private
Console.WriteLine($"Current Balance: {account.Balance}");
```

### **2. C# OOP - Inheritance**
**Definition**: Inheritance is a mechanism where a new class (derived/child class) is created from an existing class (base/parent class), inheriting its members (fields, properties, methods). The derived class can reuse, extend, or override the base class functionality. C# supports single inheritance (a class can inherit from only one base class) but implements multiple inheritance of behavior through interfaces. Inheritance promotes code reusability and establishes IS-A relationships.

**Key Concepts**:
- Base class (parent) and derived class (child)
- Use 'base' keyword to access base class members
- Constructor chaining with base()
- Method overriding with virtual/override keywords
- Sealed classes cannot be inherited
- Abstract classes must be inherited

```csharp
// Base class
public class Employee
{
    // Protected members accessible to derived classes
    protected string _employeeId;
    protected string _name;
    protected decimal _baseSalary;

    public string EmployeeId
    {
        get => _employeeId;
        set => _employeeId = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public decimal BaseSalary
    {
        get => _baseSalary;
        set => _baseSalary = value;
    }

    // Base constructor
    public Employee(string employeeId, string name, decimal baseSalary)
    {
        _employeeId = employeeId;
        _name = name;
        _baseSalary = baseSalary;
    }

    // Virtual method - can be overridden in derived classes
    public virtual decimal CalculateSalary()
    {
        return _baseSalary;
    }

    // Virtual method with default implementation
    public virtual string GetEmployeeInfo()
    {
        return $"ID: {_employeeId}, Name: {_name}, Salary: {CalculateSalary():C}";
    }

    // Non-virtual method - cannot be overridden
    public void PrintEmployeeDetails()
    {
        Console.WriteLine(GetEmployeeInfo());
    }
}

// Derived class - Manager inherits from Employee
public class Manager : Employee
{
    public decimal Bonus { get; set; }
    public int TeamSize { get; set; }

    // Constructor chaining - calls base class constructor
    public Manager(string employeeId, string name, decimal baseSalary, decimal bonus, int teamSize)
        : base(employeeId, name, baseSalary) // Call base constructor
    {
        Bonus = bonus;
        TeamSize = teamSize;
    }

    // Override base class method
    public override decimal CalculateSalary()
    {
        // Access base class method
        decimal baseSalary = base.CalculateSalary();
        return baseSalary + Bonus + (TeamSize * 500); // Team management bonus
    }

    // Override GetEmployeeInfo
    public override string GetEmployeeInfo()
    {
        string baseInfo = base.GetEmployeeInfo();
        return $"{baseInfo}, Bonus: {Bonus:C}, Team Size: {TeamSize}";
    }

    // New method specific to Manager
    public void ConductMeeting()
    {
        Console.WriteLine($"{Name} is conducting a meeting with {TeamSize} team members.");
    }
}

// Another derived class - Developer
public class Developer : Employee
{
    public string ProgrammingLanguage { get; set; }
    public int ProjectsCompleted { get; set; }

    public Developer(string employeeId, string name, decimal baseSalary, string language)
        : base(employeeId, name, baseSalary)
    {
        ProgrammingLanguage = language;
        ProjectsCompleted = 0;
    }

    // Override CalculateSalary
    public override decimal CalculateSalary()
    {
        decimal baseSalary = base.CalculateSalary();
        decimal projectBonus = ProjectsCompleted * 1000;
        return baseSalary + projectBonus;
    }

    public override string GetEmployeeInfo()
    {
        string baseInfo = base.GetEmployeeInfo();
        return $"{baseInfo}, Language: {ProgrammingLanguage}, Projects: {ProjectsCompleted}";
    }

    // Developer-specific method
    public void WriteCode()
    {
        Console.WriteLine($"{Name} is writing code in {ProgrammingLanguage}.");
    }

    public void CompleteProject()
    {
        ProjectsCompleted++;
        Console.WriteLine($"{Name} completed project #{ProjectsCompleted}");
    }
}

// Multi-level inheritance
public class SeniorDeveloper : Developer
{
    public int YearsOfExperience { get; set; }
    public bool IsTechLead { get; set; }

    public SeniorDeveloper(string employeeId, string name, decimal baseSalary, 
        string language, int experience)
        : base(employeeId, name, baseSalary, language)
    {
        YearsOfExperience = experience;
    }

    public override decimal CalculateSalary()
    {
        decimal devSalary = base.CalculateSalary();
        decimal experienceBonus = YearsOfExperience * 2000;
        decimal leadBonus = IsTechLead ? 5000 : 0;
        return devSalary + experienceBonus + leadBonus;
    }

    public void MentorJuniors()
    {
        Console.WriteLine($"{Name} is mentoring junior developers.");
    }
}

// Usage demonstrating inheritance
class Program
{
    static void Main()
    {
        // Base class reference, derived class object
        Employee emp1 = new Manager("M001", "Vamsi Ravala", 80000, 15000, 10);
        Employee emp2 = new Developer("D001", "John Doe", 70000, "C#");
        Employee emp3 = new SeniorDeveloper("SD001", "Jane Smith", 90000, "C#", 8);

        // Polymorphic behavior
        List<Employee> employees = new List<Employee> { emp1, emp2, emp3 };
        
        foreach (var emp in employees)
        {
            emp.PrintEmployeeDetails(); // Calls overridden methods
        }

        // Specific derived class features
        Manager manager = (Manager)emp1;
        manager.ConductMeeting();

        Developer developer = (Developer)emp2;
        developer.WriteCode();
        developer.CompleteProject();

        SeniorDeveloper seniorDev = (SeniorDeveloper)emp3;
        seniorDev.IsTechLead = true;
        seniorDev.MentorJuniors();
    }
}
```

(Continue with the remaining topics following the same detailed pattern...)

Would you like me to continue generating the complete comprehensive content for all remaining topics?