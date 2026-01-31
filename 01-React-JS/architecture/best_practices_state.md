# Best Practices for Managing State at Scale in React

## 0) First, classify the state
- **Server state**: data that lives on the backend (lists, entities, auth session). It can be refetched and becomes stale.  
- **Client state**: purely UI/client concerns (modals, wizards, feature flags, local edits).  
👉 Treat these differently—don’t force one tool to do both.

---

## 1) Use the right tool for the right state
- **Server state** → **TanStack Query (React Query)** or **RTK Query**  
  - Handles caching, deduping, background refetch, pagination, optimistic updates, retries.
- **Complex shared client state** → **Redux Toolkit** (slices, immutable updates, middleware, DevTools).
- **Lightweight global client state** → **Zustand/Jotai** with `useSyncExternalStore` (small, fast, selector-based).
- **Local/UI state** → `useState` in the component; lift only when necessary.
- **Forms** → Dedicated libs (React Hook Form/Formik) + schema validation (Zod/Yup).

---

## 2) Structure by “feature,” not by “type”
- Prefer **feature folders** (`/features/cart`, `/features/auth`) that co-locate components, state, hooks, tests.
- Keep **state and mutations close** to where they’re used; promote upward only when multiple features need it.
- Export **public APIs** from each feature (selectors, hooks) instead of exposing internals.

---

## 3) Normalize and select
- For entity-heavy apps, **normalize** (by id) to avoid nested duplication.  
  - In Redux Toolkit, use `createEntityAdapter`.  
- Always read via **selectors** (Reselect or store-provided selectors) for:
  - Encapsulation (change structure without breaking callers)
  - Memoization (stable outputs → fewer re-renders)

---

## 4) Keep rendering cheap
- **Split contexts**; avoid “one giant context” causing app-wide re-renders.  
- Prefer **selector-based** access (Redux `useSelector`, Zustand selectors, `use-context-selector`).
- Use `React.memo`, `useMemo`, `useCallback` to cap re-renders along hot paths.
- **Virtualize** big lists (react-window/react-virtualized).
- Defer non-urgent work with **transitions** (`useTransition`) and **`Suspense`** where it helps UX.

---

## 5) Effects & data flow discipline
- Keep **effects minimal and idempotent**; effects are for *syncing with the outside world* (DOM, storage, network), not for compute.
- Move **business rules** into reducers/async thunks or dedicated services—not inside components.
- Avoid “**render → effect → setState**” loops; prefer derived state or selectors.

---

## 6) Error, loading, and empty states are first-class
- Centralize patterns for:
  - **Loading** (skeletons/spinners), **error** boundaries, and **empty** results.
- For server state, favor **stale-while-revalidate** UX (show cached data immediately, update in background).
- Standardize **retry/backoff** and **error-toasts** in one place (interceptors or query error handlers).

---

## 7) Persistence & offline (only what you must)
- Persist **minimal** client state (e.g., auth token, feature toggles).  
- Use **whitelists** with versioned migrations; beware of restoring stale server data from storage.
- If offline is a requirement, plan **queue/rollback** flows and test them (optimistic updates with reconciliation).

---

## 8) Types & contracts
- Use **TypeScript** everywhere; derive types from API schemas (OpenAPI/GraphQL codegen, Zod).  
- Keep types in feature boundaries; export **narrow types** to avoid bleed-through.
- Write **selector types** that hide internal structure.

---

## 9) Testing the right things
- **Unit-test reducers/selectors** (pure, cheap, high value).  
- **Integration-test hooks** (React Testing Library) for data flows.  
- **E2E** key flows (Cypress/Playwright) focusing on stateful journeys (checkout, onboarding).

---

## 10) Performance guardrails and DX
- Enforce **rules-of-hooks** and exhaustive-deps via ESLint; add custom lint rules for state patterns.
- Watch **bundle size** (code-split by route/feature; dynamic imports for rarely used slices).
- Measure with **React Profiler** and browser performance panel; track regressions in CI where possible.

---

## 11) Security & multi-tab consistency
- Keep tokens out of Redux; store in **httpOnly cookies** when possible.  
- For multi-tab sync (logout, feature flags), use **BroadcastChannel** or storage events.
- Don’t leak secrets in DevTools/action logs.

---

## 12) Migration & evolution
- Start **simple** (local state + React Query).  
- When cross-feature client state grows, introduce **Redux Toolkit** where needed—**per feature**, not globally at once.
- Keep **adapters** thin; avoid coupling UI to a single store implementation.

---

## Quick “what should I use?” table

| Situation | Recommended |
|---|---|
| Data from server (lists, details, cache, pagination) | **TanStack Query / RTK Query** |
| Many features share complex client state | **Redux Toolkit (+ Entity Adapter + Reselect)** |
| Small global state (a few flags/values) | **Zustand/Jotai** |
| Local UI only | **useState / useReducer in component** |
| Heavy forms | **React Hook Form + Zod** |
| Optimistic updates / offline queue | **RTK Query or React Query + custom queue/middleware** |

---

## Common anti-patterns to avoid
- One mega **context** for “global state everything”.
- Storing **server state** in Redux by default (you lose caching, staleness, retries).
- Deeply nested objects without **normalization**.
- **Effects** doing business logic or data transforms best suited for reducers/selectors.
- Unkeyed or unstable keys in lists, causing remounts and lost state.
