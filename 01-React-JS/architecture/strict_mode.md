# React Strict Mode — What It Is and Why It Matters

## What is Strict Mode?
**Strict Mode** is a **development-only** feature that helps catch bugs and unsafe patterns early.  
It doesn’t render anything visible and **does not run in production builds**.

```tsx
// index.tsx
import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App";

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

---

## What Strict Mode Does (React 18+)
- **Double-invokes render + effects on mount (dev only)**  
  Surfaces missing cleanups and non-idempotent logic.
- **Warns on legacy/unsafe APIs**  
  e.g., deprecated lifecycles, string refs, `findDOMNode`.
- **Prepares for concurrent rendering**  
  Ensures components behave when React pauses/resumes or replays work.
- **Extra safety checks**  
  e.g., validating `useId` stability.

> ⚠️ Note: Double-invoke happens only in development, not in production.

---

## Why You Should Care
- **Catch bugs early**: memory leaks, duplicate API calls, race conditions.  
- **Future-proofing**: smooth upgrades to new React features (concurrent mode, Suspense).  
- **Library hygiene**: exposes third-party code that misses cleanups.

---

## Common Issues Revealed by Strict Mode

| Symptom in Dev | Root Cause | Fix |
|---|---|---|
| API request fires twice | Non-idempotent `useEffect` | Use AbortController, ensure effect is idempotent |
| Timer/listener stacks up | Missing cleanup | Return cleanup in `useEffect` |
| "setState during render" warning | Doing work in render | Move work to effect or handler |
| Random IDs change | Misuse of IDs | Use `useId()` |

---

## Best Practices
- Effects must be **idempotent** with proper **cleanup**.  
- Keep render **pure** (no side effects).  
- Use **AbortController** for fetches.  
- Wrap misbehaving **third-party components** outside Strict Mode temporarily if needed.

Example:

```tsx
function usePolling(url: string, ms: number) {
  useEffect(() => {
    let abort = new AbortController();
    const tick = async () => {
      try {
        await fetch(url, { signal: abort.signal });
      } catch {}
    };
    const id = setInterval(tick, ms);
    return () => {
      abort.abort();        // ✅ cleanup async work
      clearInterval(id);    // ✅ cleanup timer
    };
  }, [url, ms]);
}
```

---

## When to Disable It
- Rarely disable. Keep Strict Mode **on globally**.  
- If a third-party library breaks, disable it **only around that subtree** until fixed.

---

## TL;DR
**Use Strict Mode in development.**  
It double-mounts components to catch side-effect bugs, warns on unsafe patterns, and makes your app ready for concurrent React — **with zero production cost**.
