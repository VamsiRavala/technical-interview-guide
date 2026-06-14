# React Portals

> **Updated for React 19 (2026)** - Complete guide to rendering components outside the DOM hierarchy

---

## Table of Contents
- [What are Portals?](#what-are-portals)
- [Creating Portals](#creating-portals)
- [Common Use Cases](#common-use-cases)
- [Event Bubbling with Portals](#event-bubbling-with-portals)
- [Best Practices](#best-practices)
- [Advanced Patterns](#advanced-patterns)

---

## What are Portals?

Portals provide a way to render children into a DOM node that exists **outside the parent component's DOM hierarchy**.

### Key Benefits
- 🎯 Render components anywhere in the DOM
- 🔄 Maintain React's event bubbling
- 🎨 Escape CSS overflow/z-index constraints
- ♿ Improve accessibility for modals and overlays

### Syntax

```jsx
ReactDOM.createPortal(child, container)
```

- **child**: Any renderable React child (element, string, fragment)
- **container**: A DOM element where the child should be rendered

---

## Creating Portals

### Basic Portal Setup

```jsx
import { createPortal } from 'react-dom';

function Modal({ children }) {
  // Get reference to the portal container
  const portalContainer = document.getElementById('modal-root');
  
  return createPortal(
    <div className="modal-overlay">
      <div className="modal-content">
        {children}
      </div>
    </div>,
    portalContainer
  );
}
```

### HTML Setup

```html
<!-- public/index.html -->
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>React App</title>
  </head>
  <body>
    <div id="root"></div>
    <div id="modal-root"></div>
    <div id="tooltip-root"></div>
    <div id="notification-root"></div>
  </body>
</html>
```

### Using the Portal

```jsx
function App() {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <div className="app">
      <button onClick={() => setIsOpen(true)}>
        Open Modal
      </button>
      
      {isOpen && (
        <Modal>
          <h2>Modal Title</h2>
          <p>Modal content goes here</p>
          <button onClick={() => setIsOpen(false)}>
            Close
          </button>
        </Modal>
      )}
    </div>
  );
}
```

---

## Common Use Cases

### 1. **Modal Dialogs**

```jsx
import { createPortal } from 'react-dom';
import { useEffect, useState } from 'react';

function Modal({ isOpen, onClose, children }) {
  const [container] = useState(() => {
    const el = document.createElement('div');
    el.classList.add('modal-root');
    return el;
  });

  useEffect(() => {
    document.body.appendChild(container);
    
    // Prevent body scroll when modal is open
    document.body.style.overflow = 'hidden';
    
    return () => {
      document.body.removeChild(container);
      document.body.style.overflow = 'unset';
    };
  }, [container]);

  useEffect(() => {
    const handleEscape = (e) => {
      if (e.key === 'Escape') onClose();
    };
    
    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
    }
    
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return createPortal(
    <div 
      className="modal-overlay" 
      onClick={onClose}
      role="dialog"
      aria-modal="true"
    >
      <div 
        className="modal-content" 
        onClick={(e) => e.stopPropagation()}
      >
        <button 
          className="modal-close" 
          onClick={onClose}
          aria-label="Close modal"
        >
          ×
        </button>
        {children}
      </div>
    </div>,
    container
  );
}

// Usage
function App() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  return (
    <>
      <button onClick={() => setIsModalOpen(true)}>
        Open Modal
      </button>
      
      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)}
      >
        <h2>Confirm Action</h2>
        <p>Are you sure you want to proceed?</p>
        <button onClick={() => setIsModalOpen(false)}>
          Confirm
        </button>
      </Modal>
    </>
  );
}
```

### 2. **Tooltips**

```jsx
import { createPortal } from 'react-dom';
import { useState, useRef, useEffect } from 'react';

function Tooltip({ children, content }) {
  const [isVisible, setIsVisible] = useState(false);
  const [position, setPosition] = useState({ top: 0, left: 0 });
  const triggerRef = useRef(null);

  const updatePosition = () => {
    if (triggerRef.current) {
      const rect = triggerRef.current.getBoundingClientRect();
      setPosition({
        top: rect.top + window.scrollY - 40,
        left: rect.left + window.scrollX + rect.width / 2,
      });
    }
  };

  useEffect(() => {
    if (isVisible) {
      updatePosition();
      window.addEventListener('scroll', updatePosition);
      window.addEventListener('resize', updatePosition);
      
      return () => {
        window.removeEventListener('scroll', updatePosition);
        window.removeEventListener('resize', updatePosition);
      };
    }
  }, [isVisible]);

  return (
    <>
      <span
        ref={triggerRef}
        onMouseEnter={() => setIsVisible(true)}
        onMouseLeave={() => setIsVisible(false)}
      >
        {children}
      </span>
      
      {isVisible && createPortal(
        <div
          className="tooltip"
          style={{
            position: 'absolute',
            top: `${position.top}px`,
            left: `${position.left}px`,
            transform: 'translateX(-50%)',
          }}
          role="tooltip"
        >
          {content}
        </div>,
        document.body
      )}
    </>
  );
}

// Usage
function App() {
  return (
    <div>
      <Tooltip content="This is helpful information">
        <button>Hover me</button>
      </Tooltip>
    </div>
  );
}
```

### 3. **Toast Notifications**

```jsx
import { createPortal } from 'react-dom';
import { useState, useCallback } from 'react';

function Toast({ message, type = 'info', onClose }) {
  useEffect(() => {
    const timer = setTimeout(onClose, 3000);
    return () => clearTimeout(timer);
  }, [onClose]);

  return createPortal(
    <div className={`toast toast-${type}`}>
      <span>{message}</span>
      <button onClick={onClose}>×</button>
    </div>,
    document.getElementById('notification-root')
  );
}

function ToastContainer() {
  const [toasts, setToasts] = useState([]);

  const addToast = useCallback((message, type = 'info') => {
    const id = Date.now();
    setToasts((prev) => [...prev, { id, message, type }]);
  }, []);

  const removeToast = useCallback((id) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  return (
    <>
      {toasts.map((toast) => (
        <Toast
          key={toast.id}
          message={toast.message}
          type={toast.type}
          onClose={() => removeToast(toast.id)}
        />
      ))}
    </>
  );
}

// Usage with Context
const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const showToast = (message, type = 'info') => {
    const id = Date.now();
    setToasts((prev) => [...prev, { id, message, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 3000);
  };

  return (
    <ToastContext.Provider value={showToast}>
      {children}
      <ToastContainer toasts={toasts} />
    </ToastContext.Provider>
  );
}

export const useToast = () => useContext(ToastContext);
```

### 4. **Dropdown Menus**

```jsx
import { createPortal } from 'react-dom';
import { useState, useRef, useEffect } from 'react';

function Dropdown({ trigger, children }) {
  const [isOpen, setIsOpen] = useState(false);
  const [position, setPosition] = useState({ top: 0, left: 0 });
  const triggerRef = useRef(null);
  const dropdownRef = useRef(null);

  useEffect(() => {
    if (isOpen && triggerRef.current) {
      const rect = triggerRef.current.getBoundingClientRect();
      setPosition({
        top: rect.bottom + window.scrollY,
        left: rect.left + window.scrollX,
      });
    }
  }, [isOpen]);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target) &&
        !triggerRef.current.contains(event.target)
      ) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }
  }, [isOpen]);

  return (
    <>
      <div ref={triggerRef} onClick={() => setIsOpen(!isOpen)}>
        {trigger}
      </div>
      
      {isOpen && createPortal(
        <div
          ref={dropdownRef}
          className="dropdown-menu"
          style={{
            position: 'absolute',
            top: `${position.top}px`,
            left: `${position.left}px`,
          }}
        >
          {children}
        </div>,
        document.body
      )}
    </>
  );
}

// Usage
function App() {
  return (
    <Dropdown trigger={<button>Menu</button>}>
      <div className="dropdown-item">Profile</div>
      <div className="dropdown-item">Settings</div>
      <div className="dropdown-item">Logout</div>
    </Dropdown>
  );
}
```

---

## Event Bubbling with Portals

Even though a portal can be rendered anywhere in the DOM, it behaves like a normal React child in every other way, **including event bubbling**.

### Example

```jsx
import { createPortal } from 'react-dom';

function Modal({ children, onClose }) {
  return createPortal(
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>,
    document.getElementById('modal-root')
  );
}

function Parent() {
  const [count, setCount] = useState(0);
  
  // This click handler will be called even though the button
  // is rendered in a portal outside this component's DOM tree
  return (
    <div onClick={() => setCount(c => c + 1)}>
      <p>Clicks: {count}</p>
      
      <Modal onClose={() => console.log('Modal closed')}>
        <button>Click me</button>
        {/* Clicking this button will bubble up to Parent's onClick */}
      </Modal>
    </div>
  );
}
```

### Key Points about Event Bubbling

1. **Events bubble through React tree, not DOM tree**
   ```jsx
   // Even though Modal is rendered elsewhere in the DOM,
   // events bubble up through the React component tree
   <Parent>
     <Modal> {/* Portal */}
       <Button /> {/* Events bubble to Parent */}
     </Modal>
   </Parent>
   ```

2. **Stop Propagation**
   ```jsx
   function Modal({ children }) {
     return createPortal(
       <div onClick={(e) => e.stopPropagation()}>
         {children}
       </div>,
       document.getElementById('modal-root')
     );
   }
   ```

---

## Best Practices

### 1. **Create Portal Container in useEffect**

```jsx
function Portal({ children }) {
  const [container] = useState(() => document.createElement('div'));

  useEffect(() => {
    document.body.appendChild(container);
    return () => {
      document.body.removeChild(container);
    };
  }, [container]);

  return createPortal(children, container);
}
```

### 2. **Handle Accessibility**

```jsx
function Modal({ isOpen, onClose, children, ariaLabel }) {
  useEffect(() => {
    if (isOpen) {
      // Trap focus within modal
      const previousActiveElement = document.activeElement;
      
      return () => {
        previousActiveElement?.focus();
      };
    }
  }, [isOpen]);

  if (!isOpen) return null;

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label={ariaLabel}
      tabIndex={-1}
    >
      {children}
    </div>,
    document.body
  );
}
```

### 3. **Prevent Body Scroll**

```jsx
useEffect(() => {
  if (isOpen) {
    const originalOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    
    return () => {
      document.body.style.overflow = originalOverflow;
    };
  }
}, [isOpen]);
```

### 4. **Clean Up Event Listeners**

```jsx
useEffect(() => {
  const handleEscape = (e) => {
    if (e.key === 'Escape') onClose();
  };

  if (isOpen) {
    document.addEventListener('keydown', handleEscape);
    return () => {
      document.removeEventListener('keydown', handleEscape);
    };
  }
}, [isOpen, onClose]);
```

---

## Advanced Patterns

### 1. **Portal Manager with Context**

```jsx
import { createContext, useContext, useState } from 'react';

const PortalContext = createContext(null);

export function PortalProvider({ children }) {
  const [portals, setPortals] = useState([]);

  const addPortal = (id, content) => {
    setPortals((prev) => [...prev, { id, content }]);
  };

  const removePortal = (id) => {
    setPortals((prev) => prev.filter((p) => p.id !== id));
  };

  return (
    <PortalContext.Provider value={{ addPortal, removePortal }}>
      {children}
      {portals.map((portal) => (
        <Portal key={portal.id}>{portal.content}</Portal>
      ))}
    </PortalContext.Provider>
  );
}

export const usePortal = () => useContext(PortalContext);
```

### 2. **Stacked Modals**

```jsx
function ModalManager() {
  const [modals, setModals] = useState([]);

  const openModal = (content) => {
    const id = Date.now();
    setModals((prev) => [...prev, { id, content }]);
    return id;
  };

  const closeModal = (id) => {
    setModals((prev) => prev.filter((m) => m.id !== id));
  };

  return (
    <>
      {modals.map((modal, index) => (
        <Modal
          key={modal.id}
          zIndex={1000 + index}
          onClose={() => closeModal(modal.id)}
        >
          {modal.content}
        </Modal>
      ))}
    </>
  );
}
```

### 3. **Animated Portals**

```jsx
import { createPortal } from 'react-dom';
import { useState, useEffect } from 'react';

function AnimatedModal({ isOpen, onClose, children }) {
  const [shouldRender, setShouldRender] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setShouldRender(true);
    }
  }, [isOpen]);

  const handleAnimationEnd = () => {
    if (!isOpen) {
      setShouldRender(false);
    }
  };

  if (!shouldRender) return null;

  return createPortal(
    <div
      className={`modal-overlay ${isOpen ? 'fade-in' : 'fade-out'}`}
      onAnimationEnd={handleAnimationEnd}
    >
      <div className="modal-content">
        {children}
      </div>
    </div>,
    document.body
  );
}
```

---

## Interview Questions

### Q1: What is a portal in React and when would you use it?
**Answer:** A portal is a way to render children into a DOM node that exists outside the parent component's DOM hierarchy. Use portals for:
- Modals and dialogs (to avoid z-index and overflow issues)
- Tooltips and popovers
- Toast notifications
- Dropdowns that need to escape their parent's styling constraints

### Q2: How do events work with portals?
**Answer:** Events bubble up through the React component tree, not the DOM tree. Even if a portal renders its content in a different place in the DOM, events will bubble up through the React component hierarchy as if the portal didn't exist.

### Q3: What's the syntax for creating a portal?
**Answer:** `ReactDOM.createPortal(child, container)` where child is the React element to render and container is the DOM element where it should be rendered.

### Q4: What are the differences between portals and regular rendering?
**Answer:** 
- **DOM Location**: Portals render in a different DOM location but maintain React hierarchy
- **Event Bubbling**: Events bubble through React tree, not DOM tree
- **Context**: Portals still have access to React context from their parent
- **Styling**: Can escape parent's CSS constraints (overflow, z-index)

### Q5: How do you clean up portal containers?
**Answer:** Use useEffect to append the container on mount and remove it on unmount:
```jsx
useEffect(() => {
  document.body.appendChild(container);
  return () => {
    document.body.removeChild(container);
  };
}, [container]);
```

---

## Summary

- ✅ Portals render components outside parent DOM hierarchy
- ✅ Perfect for modals, tooltips, and overlays
- ✅ Events bubble through React tree, not DOM tree
- ✅ Maintain context and React lifecycle
- ✅ Escape CSS constraints like overflow and z-index
- ⚠️ Remember to clean up portal containers
- ⚠️ Handle accessibility (focus trap, aria labels)

---

**Next Steps:**
- Learn about [Error Boundaries](09-error-boundaries.md) for error handling
- Explore [React Profiler & Performance](11-profiler-performance.md) optimization
- Check out [Suspense & Concurrent Features](12-suspense-concurrent.md)

---

*Last Updated: January 2026 - React 19*
