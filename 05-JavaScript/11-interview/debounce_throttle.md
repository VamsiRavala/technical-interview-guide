# Debouncing and Throttling in JavaScript

## Overview

**Debouncing** and **throttling** are performance optimization techniques used to control how many times a function executes over time, especially for events that fire rapidly (scrolling, resizing, typing, etc.).

---

## Debouncing

### What is Debouncing?

**Debouncing** ensures a function is only executed **after a certain amount of time has passed** since it was last called. If the function is called again before the delay expires, the timer resets.

**Use Case:** Wait for user to stop typing before making an API call.

### How It Works

```
User types: a → b → c → d → e (rapid succession)
Debounced function: Called only ONCE after user stops typing (after delay)
```

### Basic Implementation

```javascript
function debounce(func, delay) {
  let timeoutId;
  
  return function(...args) {
    // Clear previous timer
    clearTimeout(timeoutId);
    
    // Set new timer
    timeoutId = setTimeout(() => {
      func.apply(this, args);
    }, delay);
  };
}
```

### Example: Search Input

```javascript
const searchInput = document.getElementById('search');

// Without debounce - API called on every keystroke
searchInput.addEventListener('input', (e) => {
  searchAPI(e.target.value); // Called too many times!
});

// With debounce - API called only after user stops typing
const debouncedSearch = debounce((query) => {
  searchAPI(query);
}, 500);

searchInput.addEventListener('input', (e) => {
  debouncedSearch(e.target.value);
});

function searchAPI(query) {
  console.log('Searching for:', query);
  // fetch(`/api/search?q=${query}`)...
}
```

**Benefits:**
- User types "JavaScript" (10 keystrokes)
- Without debounce: 10 API calls
- With debounce (500ms): 1 API call (after 500ms of no typing)

### Advanced Debounce with Immediate Option

```javascript
function debounce(func, delay, immediate = false) {
  let timeoutId;
  
  return function(...args) {
    const callNow = immediate && !timeoutId;
    
    clearTimeout(timeoutId);
    
    timeoutId = setTimeout(() => {
      timeoutId = null;
      if (!immediate) {
        func.apply(this, args);
      }
    }, delay);
    
    if (callNow) {
      func.apply(this, args);
    }
  };
}

// Execute immediately on first call, then debounce
const handleClick = debounce(() => {
  console.log('Button clicked');
}, 1000, true);
```

---

## Throttling

### What is Throttling?

**Throttling** ensures a function is executed at most **once in a specified time period**, no matter how many times it's called.

**Use Case:** Limit how often scroll event handler fires.

### How It Works

```
User scrolls continuously for 5 seconds
Throttled function (1000ms): Called every 1 second (5 times total)
```

### Basic Implementation

```javascript
function throttle(func, limit) {
  let inThrottle;
  
  return function(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      
      setTimeout(() => {
        inThrottle = false;
      }, limit);
    }
  };
}
```

### Example: Scroll Event

```javascript
// Without throttle - fires hundreds of times per second
window.addEventListener('scroll', () => {
  console.log('Scroll position:', window.scrollY);
  updateProgressBar(); // Heavy operation
});

// With throttle - fires at most once per 200ms
const throttledScroll = throttle(() => {
  console.log('Scroll position:', window.scrollY);
  updateProgressBar();
}, 200);

window.addEventListener('scroll', throttledScroll);

function updateProgressBar() {
  const scrolled = (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100;
  document.getElementById('progress').style.width = scrolled + '%';
}
```

**Benefits:**
- User scrolls for 2 seconds
- Without throttle: ~200+ calls
- With throttle (200ms): ~10 calls

### Advanced Throttle with Leading and Trailing Options

```javascript
function throttle(func, limit, options = {}) {
  let timeout;
  let previous = 0;
  
  const { leading = true, trailing = true } = options;
  
  return function(...args) {
    const now = Date.now();
    
    if (!previous && !leading) {
      previous = now;
    }
    
    const remaining = limit - (now - previous);
    
    if (remaining <= 0 || remaining > limit) {
      if (timeout) {
        clearTimeout(timeout);
        timeout = null;
      }
      previous = now;
      func.apply(this, args);
    } else if (!timeout && trailing) {
      timeout = setTimeout(() => {
        previous = leading ? Date.now() : 0;
        timeout = null;
        func.apply(this, args);
      }, remaining);
    }
  };
}

// Execute on leading edge only
const handleScroll = throttle(() => {
  console.log('Scrolling');
}, 1000, { leading: true, trailing: false });

// Execute on trailing edge only
const handleResize = throttle(() => {
  console.log('Resizing');
}, 1000, { leading: false, trailing: true });
```

---

## Debounce vs Throttle

| Aspect | Debounce | Throttle |
|--------|----------|----------|
| **Execution** | After inactivity period | At regular intervals |
| **First call** | Delayed (or immediate with option) | Immediate (with leading) |
| **Subsequent calls** | Reset timer | Ignored until interval passes |
| **Best for** | Waiting for user to finish action | Limiting execution rate |
| **Example** | Search input, form validation | Scroll, resize, mouse move |

### Visual Comparison

```
User Action: |||||||||||||||||||||||||| (continuous events)

Debounce:    .......................... ✓ (executes once at end)

Throttle:    ✓...✓...✓...✓...✓...✓     (executes at intervals)
```

---

## Real-World Use Cases

### 1. Search/Autocomplete (Debounce)

```javascript
const searchBox = document.getElementById('search');

const debouncedSearch = debounce(async (query) => {
  if (query.length < 3) return;
  
  try {
    const response = await fetch(`/api/autocomplete?q=${query}`);
    const suggestions = await response.json();
    displaySuggestions(suggestions);
  } catch (error) {
    console.error('Search failed:', error);
  }
}, 300);

searchBox.addEventListener('input', (e) => {
  debouncedSearch(e.target.value);
});

function displaySuggestions(suggestions) {
  const list = document.getElementById('suggestions');
  list.innerHTML = suggestions
    .map(s => `<li>${s}</li>`)
    .join('');
}
```

### 2. Window Resize (Debounce)

```javascript
const debouncedResize = debounce(() => {
  console.log('Window resized to:', window.innerWidth, window.innerHeight);
  
  // Heavy layout calculations
  recalculateLayout();
  adjustChartDimensions();
}, 250);

window.addEventListener('resize', debouncedResize);
```

### 3. Infinite Scroll (Throttle)

```javascript
const throttledScroll = throttle(() => {
  const scrollPosition = window.innerHeight + window.scrollY;
  const documentHeight = document.documentElement.scrollHeight;
  
  // Load more when user is 200px from bottom
  if (documentHeight - scrollPosition < 200) {
    loadMoreContent();
  }
}, 200);

window.addEventListener('scroll', throttledScroll);

async function loadMoreContent() {
  const content = await fetch('/api/more-content');
  appendContent(content);
}
```

### 4. Button Click Prevention (Debounce)

```javascript
const saveButton = document.getElementById('save');

// Prevent multiple submissions
const debouncedSave = debounce(async () => {
  saveButton.disabled = true;
  saveButton.textContent = 'Saving...';
  
  try {
    await saveData();
    showSuccess('Data saved!');
  } catch (error) {
    showError('Save failed');
  } finally {
    saveButton.disabled = false;
    saveButton.textContent = 'Save';
  }
}, 1000, true); // immediate = true for first click

saveButton.addEventListener('click', debouncedSave);
```

### 5. Mouse Move Tracking (Throttle)

```javascript
const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');

const throttledDraw = throttle((x, y) => {
  ctx.fillStyle = 'blue';
  ctx.fillRect(x - 2, y - 2, 4, 4);
}, 50);

canvas.addEventListener('mousemove', (e) => {
  const rect = canvas.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const y = e.clientY - rect.top;
  
  throttledDraw(x, y);
});
```

### 6. API Rate Limiting (Throttle)

```javascript
class APIClient {
  constructor() {
    // Allow 10 requests per second
    this.makeRequest = throttle(this._makeRequest.bind(this), 100);
  }
  
  async _makeRequest(endpoint, options) {
    const response = await fetch(endpoint, options);
    return response.json();
  }
}

const client = new APIClient();

// Multiple rapid calls will be throttled
for (let i = 0; i < 50; i++) {
  client.makeRequest('/api/data');
}
```

---

## Modern Alternatives

### Using Lodash

```javascript
import { debounce, throttle } from 'lodash';

// Debounce
const debouncedFn = debounce((value) => {
  console.log(value);
}, 500);

// Throttle
const throttledFn = throttle((value) => {
  console.log(value);
}, 1000);

// Cancel debounce
debouncedFn.cancel();

// Flush debounce (execute immediately)
debouncedFn.flush();
```

### Using Intersection Observer (for scroll)

Instead of throttling scroll events, use Intersection Observer:

```javascript
const observer = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      loadMoreContent();
    }
  });
}, {
  rootMargin: '200px' // Trigger 200px before element is visible
});

const sentinel = document.getElementById('sentinel');
observer.observe(sentinel);
```

### Using RequestAnimationFrame (for visual updates)

```javascript
let ticking = false;

function onScroll() {
  if (!ticking) {
    requestAnimationFrame(() => {
      updateProgressBar();
      ticking = false;
    });
    ticking = true;
  }
}

window.addEventListener('scroll', onScroll);
```

---

## Interview Questions

### Q1: Implement a basic debounce function

```javascript
function debounce(func, delay) {
  let timeoutId;
  
  return function(...args) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
      func.apply(this, args);
    }, delay);
  };
}

// Test
const log = debounce(() => console.log('Debounced!'), 1000);
log(); log(); log(); // Only last call executes after 1s
```

### Q2: Implement a basic throttle function

```javascript
function throttle(func, limit) {
  let inThrottle;
  
  return function(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => inThrottle = false, limit);
    }
  };
}

// Test
const log = throttle(() => console.log('Throttled!'), 1000);
log(); log(); log(); // First call executes immediately, others ignored for 1s
```

### Q3: When to use debounce vs throttle?

**Debounce:**
- Search inputs (wait for user to finish typing)
- Form validation (validate after user stops typing)
- Button clicks (prevent double submission)
- Window resize (recalculate layout after resize stops)

**Throttle:**
- Scroll events (update progress bar)
- Mouse move events (drawing, tracking)
- API rate limiting (limit requests per second)
- Game loop (limit frame updates)

---

## Performance Comparison

### Without Optimization

```javascript
let count = 0;
window.addEventListener('scroll', () => {
  count++;
});

// After 10 seconds of scrolling: count could be 1000+
```

### With Debounce

```javascript
let count = 0;
const debouncedHandler = debounce(() => {
  count++;
}, 500);
window.addEventListener('scroll', debouncedHandler);

// After 10 seconds of scrolling: count = 1-2
```

### With Throttle

```javascript
let count = 0;
const throttledHandler = throttle(() => {
  count++;
}, 500);
window.addEventListener('scroll', throttledHandler);

// After 10 seconds of scrolling: count = ~20
```

---

## Summary

### Debounce
- ✅ Execute **after inactivity** period
- ✅ Best for: search, validation, resize
- ✅ Reduces calls significantly
- ✅ Use when you want to wait for user to finish action

### Throttle
- ✅ Execute **at regular intervals**
- ✅ Best for: scroll, mouse move, rate limiting
- ✅ Guarantees execution at set intervals
- ✅ Use when you want to limit execution rate

### Key Takeaways
1. Both improve performance by reducing function calls
2. Debounce waits for calm, throttle ensures regular pace
3. Choose based on use case: "wait until done" vs "limit rate"
4. Modern alternatives: Intersection Observer, requestAnimationFrame
5. Libraries like Lodash provide robust implementations

Master debouncing and throttling to build performant web applications! 🚀
