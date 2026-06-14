# React Testing - Modern Practices (2026)

## Table of Contents
1. [Introduction](#introduction)
2. [React Testing Library](#react-testing-library)
3. [Vitest Integration](#vitest-integration)
4. [Component Testing Patterns](#component-testing-patterns)
5. [Mock Strategies](#mock-strategies)
6. [Testing Hooks](#testing-hooks)
7. [Testing Async Components](#testing-async-components)
8. [E2E Testing with Playwright](#e2e-testing-with-playwright)
9. [Integration Testing](#integration-testing)
10. [Best Practices](#best-practices)
11. [Interview Questions](#interview-questions)

## Introduction

Modern React testing in 2026 focuses on testing user behavior rather than implementation details. The ecosystem has evolved with:
- **React Testing Library**: User-centric testing approach
- **Vitest**: Fast, modern test runner with native ESM support
- **MSW (Mock Service Worker)**: Network-level API mocking
- **Playwright**: Reliable E2E testing
- **Testing Suspense and Server Components**

## React Testing Library

### Core Philosophy

Test components the way users interact with them:

```jsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';

// ❌ Bad: Testing implementation details
it('updates state when clicked', () => {
  const { container } = render(<Counter />);
  expect(container.firstChild.state.count).toBe(0);
});

// ✅ Good: Testing user behavior
it('increments count when button clicked', async () => {
  const user = userEvent.setup();
  render(<Counter />);
  
  const button = screen.getByRole('button', { name: /increment/i });
  await user.click(button);
  
  expect(screen.getByText('Count: 1')).toBeInTheDocument();
});
```

### Essential Queries

```jsx
import { render, screen } from '@testing-library/react';

function UserProfile() {
  return (
    <div>
      <h1>John Doe</h1>
      <img src="/avatar.jpg" alt="User avatar" />
      <button aria-label="Edit profile">Edit</button>
      <input placeholder="Search..." />
      <div data-testid="status">Active</div>
    </div>
  );
}

describe('UserProfile queries', () => {
  it('finds elements by role', () => {
    render(<UserProfile />);
    
    // Preferred: Accessible queries
    screen.getByRole('heading', { name: 'John Doe' });
    screen.getByRole('button', { name: /edit/i });
    screen.getByRole('img', { name: /avatar/i });
  });
  
  it('finds by label text', () => {
    screen.getByLabelText('Edit profile');
  });
  
  it('finds by placeholder', () => {
    screen.getByPlaceholderText('Search...');
  });
  
  it('finds by test id (last resort)', () => {
    screen.getByTestId('status');
  });
});
```

### Query Priority

1. **getByRole**: Most accessible (preferred)
2. **getByLabelText**: Forms
3. **getByPlaceholderText**: Forms
4. **getByText**: Non-interactive content
5. **getByDisplayValue**: Current form values
6. **getByAltText**: Images
7. **getByTitle**: Title attribute
8. **getByTestId**: Last resort

### Async Queries

```jsx
import { render, screen, waitFor } from '@testing-library/react';

it('shows loaded data', async () => {
  render(<UserProfile userId={1} />);
  
  // Wait for element to appear
  const heading = await screen.findByRole('heading', { name: /john/i });
  expect(heading).toBeInTheDocument();
  
  // Wait for condition
  await waitFor(() => {
    expect(screen.getByText(/loaded/i)).toBeInTheDocument();
  });
});

it('handles loading states', async () => {
  render(<AsyncComponent />);
  
  // Should show loading initially
  expect(screen.getByText(/loading/i)).toBeInTheDocument();
  
  // Should disappear when done
  await waitFor(() => {
    expect(screen.queryByText(/loading/i)).not.toBeInTheDocument();
  });
});
```

### User Interactions

```jsx
import userEvent from '@testing-library/user-event';

describe('User interactions', () => {
  it('handles form submission', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(<LoginForm onSubmit={onSubmit} />);
    
    // Type in inputs
    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    
    // Submit form
    await user.click(screen.getByRole('button', { name: /submit/i }));
    
    expect(onSubmit).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'password123'
    });
  });
  
  it('handles keyboard navigation', async () => {
    const user = userEvent.setup();
    render(<Dropdown options={['One', 'Two', 'Three']} />);
    
    const trigger = screen.getByRole('button');
    await user.click(trigger);
    
    // Navigate with keyboard
    await user.keyboard('{ArrowDown}');
    await user.keyboard('{ArrowDown}');
    await user.keyboard('{Enter}');
    
    expect(screen.getByText('Two')).toBeInTheDocument();
  });
  
  it('handles file upload', async () => {
    const user = userEvent.setup();
    const file = new File(['content'], 'test.txt', { type: 'text/plain' });
    
    render(<FileUpload />);
    
    const input = screen.getByLabelText(/upload/i);
    await user.upload(input, file);
    
    expect(screen.getByText('test.txt')).toBeInTheDocument();
  });
});
```

## Vitest Integration

### Setup

```bash
npm install -D vitest @vitest/ui @testing-library/react @testing-library/user-event jsdom
```

```js
// vitest.config.js
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.js',
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: ['**/*.config.js', '**/test/**']
    }
  }
});
```

```js
// src/test/setup.js
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// Cleanup after each test
afterEach(() => {
  cleanup();
});
```

### Writing Tests with Vitest

```jsx
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

describe('Counter Component', () => {
  beforeEach(() => {
    // Reset before each test
    vi.clearAllMocks();
  });
  
  it('renders initial count', () => {
    render(<Counter initialCount={5} />);
    expect(screen.getByText('Count: 5')).toBeInTheDocument();
  });
  
  it('increments count', async () => {
    const user = userEvent.setup();
    render(<Counter />);
    
    await user.click(screen.getByRole('button', { name: /increment/i }));
    
    expect(screen.getByText('Count: 1')).toBeInTheDocument();
  });
});
```

### Snapshot Testing

```jsx
import { render } from '@testing-library/react';
import { expect, it } from 'vitest';

it('matches snapshot', () => {
  const { container } = render(<UserCard user={mockUser} />);
  expect(container).toMatchSnapshot();
});

it('matches inline snapshot', () => {
  const { container } = render(<Button>Click me</Button>);
  expect(container.firstChild).toMatchInlineSnapshot(`
    <button class="btn">
      Click me
    </button>
  `);
});
```

### Running Tests

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest --coverage",
    "test:watch": "vitest --watch"
  }
}
```

## Component Testing Patterns

### Testing Forms

```jsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

describe('ContactForm', () => {
  it('validates required fields', async () => {
    const user = userEvent.setup();
    render(<ContactForm />);
    
    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);
    
    expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    expect(screen.getByText(/message is required/i)).toBeInTheDocument();
  });
  
  it('submits valid form', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(<ContactForm onSubmit={onSubmit} />);
    
    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/message/i), 'Hello world');
    await user.click(screen.getByRole('button', { name: /submit/i }));
    
    expect(onSubmit).toHaveBeenCalledWith({
      email: 'test@example.com',
      message: 'Hello world'
    });
  });
  
  it('displays server errors', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(new Error('Network error'));
    
    render(<ContactForm onSubmit={onSubmit} />);
    
    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.click(screen.getByRole('button', { name: /submit/i }));
    
    expect(await screen.findByText(/network error/i)).toBeInTheDocument();
  });
});
```

### Testing Lists and Filtering

```jsx
describe('TodoList', () => {
  const todos = [
    { id: 1, text: 'Buy milk', completed: false },
    { id: 2, text: 'Walk dog', completed: true },
    { id: 3, text: 'Write code', completed: false }
  ];
  
  it('renders all todos', () => {
    render(<TodoList todos={todos} />);
    
    expect(screen.getByText('Buy milk')).toBeInTheDocument();
    expect(screen.getByText('Walk dog')).toBeInTheDocument();
    expect(screen.getByText('Write code')).toBeInTheDocument();
  });
  
  it('filters active todos', async () => {
    const user = userEvent.setup();
    render(<TodoList todos={todos} />);
    
    await user.click(screen.getByRole('button', { name: /active/i }));
    
    expect(screen.getByText('Buy milk')).toBeInTheDocument();
    expect(screen.queryByText('Walk dog')).not.toBeInTheDocument();
  });
  
  it('toggles todo completion', async () => {
    const user = userEvent.setup();
    const onToggle = vi.fn();
    
    render(<TodoList todos={todos} onToggle={onToggle} />);
    
    const checkbox = screen.getAllByRole('checkbox')[0];
    await user.click(checkbox);
    
    expect(onToggle).toHaveBeenCalledWith(1);
  });
});
```

### Testing Conditional Rendering

```jsx
describe('UserDashboard', () => {
  it('shows loading state', () => {
    render(<UserDashboard loading={true} />);
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });
  
  it('shows error state', () => {
    const error = new Error('Failed to load');
    render(<UserDashboard error={error} />);
    expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
  });
  
  it('shows empty state', () => {
    render(<UserDashboard data={[]} />);
    expect(screen.getByText(/no data/i)).toBeInTheDocument();
  });
  
  it('shows data when available', () => {
    const data = [{ id: 1, name: 'Item' }];
    render(<UserDashboard data={data} />);
    expect(screen.getByText('Item')).toBeInTheDocument();
  });
});
```

### Testing with Context

```jsx
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from './ThemeContext';

function renderWithTheme(component, theme = 'light') {
  return render(
    <ThemeProvider value={theme}>
      {component}
    </ThemeProvider>
  );
}

describe('ThemedButton', () => {
  it('applies light theme', () => {
    renderWithTheme(<ThemedButton>Click</ThemedButton>, 'light');
    const button = screen.getByRole('button');
    expect(button).toHaveClass('btn-light');
  });
  
  it('applies dark theme', () => {
    renderWithTheme(<ThemedButton>Click</ThemedButton>, 'dark');
    const button = screen.getByRole('button');
    expect(button).toHaveClass('btn-dark');
  });
});
```

## Mock Strategies

### MSW (Mock Service Worker)

```bash
npm install -D msw
```

```js
// src/mocks/handlers.js
import { http, HttpResponse } from 'msw';

export const handlers = [
  http.get('/api/users/:id', ({ params }) => {
    return HttpResponse.json({
      id: params.id,
      name: 'John Doe',
      email: 'john@example.com'
    });
  }),
  
  http.post('/api/users', async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json(
      { id: 123, ...body },
      { status: 201 }
    );
  }),
  
  http.delete('/api/users/:id', () => {
    return new HttpResponse(null, { status: 204 });
  })
];
```

```js
// src/mocks/server.js
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);
```

```js
// src/test/setup.js
import { beforeAll, afterEach, afterAll } from 'vitest';
import { server } from '../mocks/server';

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### Using MSW in Tests

```jsx
import { http, HttpResponse } from 'msw';
import { server } from './mocks/server';

describe('UserProfile', () => {
  it('loads and displays user', async () => {
    render(<UserProfile userId={1} />);
    
    expect(await screen.findByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
  });
  
  it('handles error state', async () => {
    // Override handler for this test
    server.use(
      http.get('/api/users/:id', () => {
        return HttpResponse.json(
          { error: 'User not found' },
          { status: 404 }
        );
      })
    );
    
    render(<UserProfile userId={999} />);
    
    expect(await screen.findByText(/not found/i)).toBeInTheDocument();
  });
  
  it('retries failed requests', async () => {
    let attempts = 0;
    
    server.use(
      http.get('/api/users/:id', () => {
        attempts++;
        if (attempts < 3) {
          return HttpResponse.error();
        }
        return HttpResponse.json({ id: 1, name: 'John' });
      })
    );
    
    render(<UserProfile userId={1} />);
    
    expect(await screen.findByText('John')).toBeInTheDocument();
    expect(attempts).toBe(3);
  });
});
```

### Mocking Modules

```jsx
import { vi } from 'vitest';

// Mock entire module
vi.mock('./analytics', () => ({
  trackEvent: vi.fn(),
  trackPageView: vi.fn()
}));

// Mock specific functions
vi.mock('./api', () => ({
  fetchUser: vi.fn(() => Promise.resolve({ id: 1, name: 'Test' }))
}));

describe('Analytics tracking', () => {
  it('tracks button click', async () => {
    const { trackEvent } = await import('./analytics');
    const user = userEvent.setup();
    
    render(<Button>Click me</Button>);
    await user.click(screen.getByRole('button'));
    
    expect(trackEvent).toHaveBeenCalledWith('button_click', {
      label: 'Click me'
    });
  });
});
```

### Mocking Timers

```jsx
import { vi } from 'vitest';

describe('Timer component', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });
  
  afterEach(() => {
    vi.restoreAllMocks();
  });
  
  it('updates after delay', () => {
    render(<Timer delay={1000} />);
    
    expect(screen.getByText('0')).toBeInTheDocument();
    
    vi.advanceTimersByTime(1000);
    
    expect(screen.getByText('1')).toBeInTheDocument();
  });
  
  it('cleans up interval', () => {
    const { unmount } = render(<Timer />);
    
    const spy = vi.spyOn(global, 'clearInterval');
    unmount();
    
    expect(spy).toHaveBeenCalled();
  });
});
```

## Testing Hooks

### Using @testing-library/react-hooks

```bash
npm install -D @testing-library/react-hooks
```

```jsx
import { renderHook, waitFor } from '@testing-library/react';
import { act } from 'react';

describe('useCounter', () => {
  it('initializes with default value', () => {
    const { result } = renderHook(() => useCounter());
    expect(result.current.count).toBe(0);
  });
  
  it('initializes with custom value', () => {
    const { result } = renderHook(() => useCounter(10));
    expect(result.current.count).toBe(10);
  });
  
  it('increments count', () => {
    const { result } = renderHook(() => useCounter());
    
    act(() => {
      result.current.increment();
    });
    
    expect(result.current.count).toBe(1);
  });
  
  it('decrements count', () => {
    const { result } = renderHook(() => useCounter(5));
    
    act(() => {
      result.current.decrement();
    });
    
    expect(result.current.count).toBe(4);
  });
});
```

### Testing Async Hooks

```jsx
describe('useFetchUser', () => {
  it('fetches user data', async () => {
    const { result } = renderHook(() => useFetchUser(1));
    
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    expect(result.current.data).toEqual({ id: 1, name: 'John' });
  });
  
  it('handles errors', async () => {
    server.use(
      http.get('/api/users/:id', () => HttpResponse.error())
    );
    
    const { result } = renderHook(() => useFetchUser(1));
    
    await waitFor(() => {
      expect(result.current.error).toBeTruthy();
    });
  });
});
```

### Testing Hooks with Context

```jsx
function wrapper({ children }) {
  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}

describe('useUserQuery', () => {
  it('fetches user with context', async () => {
    const { result } = renderHook(
      () => useUserQuery(1),
      { wrapper }
    );
    
    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
    
    expect(result.current.data.name).toBe('John');
  });
});
```

## Testing Async Components

### Testing with Suspense

```jsx
import { Suspense } from 'react';

describe('Suspense component', () => {
  it('shows fallback while loading', async () => {
    render(
      <Suspense fallback={<div>Loading...</div>}>
        <AsyncUserProfile userId={1} />
      </Suspense>
    );
    
    // Initially shows fallback
    expect(screen.getByText('Loading...')).toBeInTheDocument();
    
    // Then shows content
    expect(await screen.findByText('John Doe')).toBeInTheDocument();
    expect(screen.queryByText('Loading...')).not.toBeInTheDocument();
  });
  
  it('handles multiple suspending components', async () => {
    render(
      <Suspense fallback={<div>Loading...</div>}>
        <UserProfile userId={1} />
        <UserPosts userId={1} />
      </Suspense>
    );
    
    // Wait for all to load
    await screen.findByText('John Doe');
    await screen.findByText('Post 1');
  });
});
```

### Testing Error Boundaries

```jsx
describe('ErrorBoundary', () => {
  it('catches and displays errors', () => {
    const ThrowError = () => {
      throw new Error('Test error');
    };
    
    // Suppress console.error for this test
    const spy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    render(
      <ErrorBoundary>
        <ThrowError />
      </ErrorBoundary>
    );
    
    expect(screen.getByText(/test error/i)).toBeInTheDocument();
    
    spy.mockRestore();
  });
  
  it('recovers from errors', async () => {
    const user = userEvent.setup();
    let shouldThrow = true;
    
    function MaybeThrow() {
      if (shouldThrow) throw new Error('Error');
      return <div>Success</div>;
    }
    
    const { rerender } = render(
      <ErrorBoundary>
        <MaybeThrow />
      </ErrorBoundary>
    );
    
    expect(screen.getByText(/error/i)).toBeInTheDocument();
    
    shouldThrow = false;
    await user.click(screen.getByRole('button', { name: /try again/i }));
    
    expect(screen.getByText('Success')).toBeInTheDocument();
  });
});
```

### Testing with React Query

```jsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  });
}

function renderWithQueryClient(component) {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      {component}
    </QueryClientProvider>
  );
}

describe('UserList with React Query', () => {
  it('loads and displays users', async () => {
    renderWithQueryClient(<UserList />);
    
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
    
    const users = await screen.findAllByRole('listitem');
    expect(users).toHaveLength(3);
  });
  
  it('refetches on mutation', async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<UserList />);
    
    await screen.findAllByRole('listitem');
    
    await user.click(screen.getByRole('button', { name: /add user/i }));
    
    // Should show 4 users after refetch
    await waitFor(() => {
      expect(screen.getAllByRole('listitem')).toHaveLength(4);
    });
  });
});
```

## E2E Testing with Playwright

### Setup

```bash
npm install -D @playwright/test
npx playwright install
```

```js
// playwright.config.js
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure'
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } }
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI
  }
});
```

### Writing E2E Tests

```js
// e2e/login.spec.js
import { test, expect } from '@playwright/test';

test.describe('Login flow', () => {
  test('successful login', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'password123');
    await page.click('button[type="submit"]');
    
    await expect(page).toHaveURL('/dashboard');
    await expect(page.getByText('Welcome back')).toBeVisible();
  });
  
  test('shows error for invalid credentials', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('input[name="email"]', 'wrong@example.com');
    await page.fill('input[name="password"]', 'wrongpass');
    await page.click('button[type="submit"]');
    
    await expect(page.getByText(/invalid credentials/i)).toBeVisible();
    await expect(page).toHaveURL('/login');
  });
  
  test('validates required fields', async ({ page }) => {
    await page.goto('/login');
    await page.click('button[type="submit"]');
    
    await expect(page.getByText(/email is required/i)).toBeVisible();
  });
});
```

### Advanced Playwright Patterns

```js
test.describe('Shopping cart', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('[name="email"]', 'test@example.com');
    await page.fill('[name="password"]', 'password');
    await page.click('button[type="submit"]');
    await page.waitForURL('/dashboard');
  });
  
  test('adds item to cart', async ({ page }) => {
    await page.goto('/products');
    
    // Click first "Add to Cart" button
    await page.click('button:has-text("Add to Cart")').first();
    
    // Check cart badge
    await expect(page.locator('[data-testid="cart-badge"]')).toHaveText('1');
  });
  
  test('removes item from cart', async ({ page, context }) => {
    // Add item first
    await page.goto('/products/1');
    await page.click('button:has-text("Add to Cart")');
    
    // Go to cart
    await page.click('[aria-label="Cart"]');
    
    // Remove item
    await page.click('button:has-text("Remove")');
    
    await expect(page.getByText('Your cart is empty')).toBeVisible();
  });
  
  test('persists cart across page reloads', async ({ page }) => {
    await page.goto('/products/1');
    await page.click('button:has-text("Add to Cart")');
    
    await page.reload();
    
    await expect(page.locator('[data-testid="cart-badge"]')).toHaveText('1');
  });
});
```

### Visual Regression Testing

```js
test('homepage matches snapshot', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveScreenshot('homepage.png');
});

test('button states', async ({ page }) => {
  await page.goto('/components');
  
  const button = page.getByRole('button', { name: 'Submit' });
  await expect(button).toHaveScreenshot('button-default.png');
  
  await button.hover();
  await expect(button).toHaveScreenshot('button-hover.png');
  
  await button.focus();
  await expect(button).toHaveScreenshot('button-focus.png');
});
```

## Integration Testing

### Testing Component Integration

```jsx
describe('Blog post workflow', () => {
  it('creates, edits, and deletes post', async () => {
    const user = userEvent.setup();
    renderWithProviders(<BlogApp />);
    
    // Create post
    await user.click(screen.getByRole('button', { name: /new post/i }));
    await user.type(screen.getByLabelText(/title/i), 'My Post');
    await user.type(screen.getByLabelText(/content/i), 'Content here');
    await user.click(screen.getByRole('button', { name: /publish/i }));
    
    expect(await screen.findByText('My Post')).toBeInTheDocument();
    
    // Edit post
    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByLabelText(/title/i));
    await user.type(screen.getByLabelText(/title/i), 'Updated Post');
    await user.click(screen.getByRole('button', { name: /save/i }));
    
    expect(await screen.findByText('Updated Post')).toBeInTheDocument();
    
    // Delete post
    await user.click(screen.getByRole('button', { name: /delete/i }));
    await user.click(screen.getByRole('button', { name: /confirm/i }));
    
    expect(screen.queryByText('Updated Post')).not.toBeInTheDocument();
  });
});
```

### Testing with Multiple Components

```jsx
describe('User authentication flow', () => {
  it('completes full auth flow', async () => {
    const user = userEvent.setup();
    renderWithProviders(<App />);
    
    // Initial state - logged out
    expect(screen.getByRole('button', { name: /log in/i })).toBeInTheDocument();
    
    // Navigate to login
    await user.click(screen.getByRole('button', { name: /log in/i }));
    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument();
    
    // Fill form and submit
    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password');
    await user.click(screen.getByRole('button', { name: /submit/i }));
    
    // Should redirect to dashboard
    expect(await screen.findByText(/welcome back/i)).toBeInTheDocument();
    
    // Should show user menu
    expect(screen.getByRole('button', { name: /account/i })).toBeInTheDocument();
    
    // Logout
    await user.click(screen.getByRole('button', { name: /account/i }));
    await user.click(screen.getByRole('menuitem', { name: /log out/i }));
    
    // Back to logged out state
    expect(screen.getByRole('button', { name: /log in/i })).toBeInTheDocument();
  });
});
```

## Best Practices

### 1. Test User Behavior, Not Implementation

```jsx
// ❌ Bad: Testing implementation
it('calls handleClick when button clicked', () => {
  const handleClick = vi.fn();
  render(<Button onClick={handleClick} />);
  // Testing internal mechanics
});

// ✅ Good: Testing user outcome
it('increments counter when button clicked', async () => {
  const user = userEvent.setup();
  render(<Counter />);
  await user.click(screen.getByRole('button'));
  expect(screen.getByText('Count: 1')).toBeInTheDocument();
});
```

### 2. Use Accessible Queries

```jsx
// ❌ Bad
screen.getByTestId('submit-button');

// ✅ Good
screen.getByRole('button', { name: /submit/i });
```

### 3. Avoid Testing Third-Party Libraries

```jsx
// ❌ Bad: Testing React Router
it('renders route', () => {
  render(<Route path="/users" element={<Users />} />);
  // Don't test that Router works
});

// ✅ Good: Test your component's behavior
it('shows user list', () => {
  render(<Users />, { wrapper: RouterWrapper });
  expect(screen.getByRole('list')).toBeInTheDocument();
});
```

### 4. Keep Tests Independent

```jsx
// ❌ Bad: Tests depend on each other
let userId;

it('creates user', () => {
  userId = createUser();
});

it('updates user', () => {
  updateUser(userId); // Depends on previous test
});

// ✅ Good: Independent tests
it('creates user', () => {
  const userId = createUser();
  expect(userId).toBeDefined();
});

it('updates user', () => {
  const userId = createUser(); // Create fresh data
  updateUser(userId);
});
```

### 5. Use Custom Render Functions

```jsx
// test/utils.jsx
export function renderWithProviders(
  ui,
  {
    route = '/',
    ...renderOptions
  } = {}
) {
  window.history.pushState({}, 'Test page', route);
  
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } }
  });
  
  function Wrapper({ children }) {
    return (
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <ThemeProvider>
            {children}
          </ThemeProvider>
        </BrowserRouter>
      </QueryClientProvider>
    );
  }
  
  return render(ui, { wrapper: Wrapper, ...renderOptions });
}
```

## Interview Questions

### Basic Questions

**Q1: What is React Testing Library and how is it different from Enzyme?**

A: React Testing Library focuses on testing components from a user's perspective, encouraging tests that interact with the DOM as users would. Enzyme tests implementation details like state and props.

RTL advantages:
- Tests behavior, not implementation
- More maintainable tests
- Encourages accessibility
- Works with React 19 features

**Q2: What query should you prioritize when selecting elements?**

A: Priority order:
1. `getByRole` - Most accessible
2. `getByLabelText` - For forms
3. `getByPlaceholderText` - Forms
4. `getByText` - Non-interactive content
5. `getByTestId` - Last resort

**Q3: How do you test async operations?**

A: Use async queries and waitFor:
```jsx
// Wait for element to appear
const element = await screen.findByText('Loaded');

// Wait for condition
await waitFor(() => {
  expect(screen.getByText('Done')).toBeInTheDocument();
});
```

### Intermediate Questions

**Q4: How do you mock API calls in tests?**

A: Use MSW (Mock Service Worker):
```jsx
import { http, HttpResponse } from 'msw';
import { server } from './mocks/server';

server.use(
  http.get('/api/users', () => {
    return HttpResponse.json([{ id: 1, name: 'John' }]);
  })
);
```

Benefits:
- Network-level mocking
- Works in tests and development
- No code changes needed
- Realistic error simulation

**Q5: What's the difference between getBy, queryBy, and findBy?**

A:
- **getBy**: Synchronous, throws if not found
- **queryBy**: Synchronous, returns null if not found
- **findBy**: Async, waits for element, throws if not found

Use cases:
```jsx
// Element should exist
screen.getByRole('button');

// Check element doesn't exist
expect(screen.queryByText('Error')).not.toBeInTheDocument();

// Wait for async element
await screen.findByText('Loaded');
```

**Q6: How do you test custom hooks?**

A: Use renderHook from @testing-library/react:
```jsx
import { renderHook, act } from '@testing-library/react';

const { result } = renderHook(() => useCounter());

act(() => {
  result.current.increment();
});

expect(result.current.count).toBe(1);
```

### Advanced Questions

**Q7: How would you test a component that uses Suspense?**

A:
```jsx
import { Suspense } from 'react';

it('handles suspense', async () => {
  render(
    <Suspense fallback={<div>Loading...</div>}>
      <AsyncComponent />
    </Suspense>
  );
  
  // Shows fallback initially
  expect(screen.getByText('Loading...')).toBeInTheDocument();
  
  // Then shows content
  const content = await screen.findByText('Content');
  expect(content).toBeInTheDocument();
  
  // Fallback is gone
  expect(screen.queryByText('Loading...')).not.toBeInTheDocument();
});
```

**Q8: How do you test complex user interactions?**

A: Use userEvent with proper setup:
```jsx
const user = userEvent.setup();

// Complex flow
await user.type(input, 'search term');
await user.keyboard('{ArrowDown}');
await user.keyboard('{Enter}');
await user.click(submitButton);

// File upload
await user.upload(fileInput, file);

// Drag and drop
await user.pointer([
  { target: element, keys: '[MouseLeft>]' },
  { coords: { x: 100, y: 100 } },
  { keys: '[/MouseLeft]' }
]);
```

**Q9: What's the best way to test a form with validation?**

A:
```jsx
it('validates form', async () => {
  const user = userEvent.setup();
  const onSubmit = vi.fn();
  
  render(<Form onSubmit={onSubmit} />);
  
  // Submit empty form
  await user.click(screen.getByRole('button', { name: /submit/i }));
  
  // Check validation errors
  expect(screen.getByText(/email is required/i)).toBeInTheDocument();
  expect(onSubmit).not.toHaveBeenCalled();
  
  // Fill valid data
  await user.type(screen.getByLabelText(/email/i), 'test@example.com');
  await user.type(screen.getByLabelText(/password/i), 'password123');
  
  // Submit should work
  await user.click(screen.getByRole('button', { name: /submit/i }));
  
  expect(onSubmit).toHaveBeenCalledWith({
    email: 'test@example.com',
    password: 'password123'
  });
});
```

**Q10: How would you implement integration testing for a full user flow?**

A: Create comprehensive tests that cover multiple components:
```jsx
describe('E-commerce checkout flow', () => {
  it('completes purchase', async () => {
    const user = userEvent.setup();
    renderWithProviders(<App />);
    
    // Browse products
    await user.click(screen.getByText(/products/i));
    
    // Add to cart
    await user.click(screen.getAllByText(/add to cart/i)[0]);
    expect(screen.getByText(/1 item/i)).toBeInTheDocument();
    
    // Go to cart
    await user.click(screen.getByLabelText(/cart/i));
    expect(screen.getByRole('heading', { name: /cart/i })).toBeInTheDocument();
    
    // Proceed to checkout
    await user.click(screen.getByText(/checkout/i));
    
    // Fill shipping info
    await user.type(screen.getByLabelText(/address/i), '123 Main St');
    await user.type(screen.getByLabelText(/city/i), 'New York');
    
    // Fill payment info
    await user.type(screen.getByLabelText(/card number/i), '4242424242424242');
    
    // Complete order
    await user.click(screen.getByText(/place order/i));
    
    // Verify success
    expect(await screen.findByText(/order confirmed/i)).toBeInTheDocument();
    expect(screen.getByText(/order #/i)).toBeInTheDocument();
  });
});
```

Key principles:
- Test realistic user flows
- Don't mock too much
- Verify end-to-end behavior
- Check critical paths
- Handle async properly

---

**Last Updated: January 2026 - React 19**

*See also: [React Hooks](./02-hooks-complete-guide.md) | [Performance](./11-profiler-performance.md) | [Suspense](./12-suspense-concurrent.md)*
