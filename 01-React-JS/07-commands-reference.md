# 🚀 React Important Commands

> **Updated for React 19 & 2026 Tooling** - Modern commands and best practices

---

## 📦 Project Setup & Initialization

### React 19 with Modern Tools (Recommended)

- **Create with Vite 5+ (Fastest, React 19):**
  ```bash
  # Using npm
  npm create vite@latest my-app -- --template react
  cd my-app
  npm install
  npm run dev
  
  # Using pnpm (fastest package manager)
  pnpm create vite my-app --template react
  cd my-app
  pnpm install
  pnpm dev
  
  # Using bun (ultra-fast runtime)
  bun create vite my-app --template react
  cd my-app
  bun install
  bun dev
  ```

- **Create with Next.js 15+ (React 19 with Server Components):**
  ```bash
  npx create-next-app@latest my-app
  # Choose: TypeScript, ESLint, Tailwind, App Router
  cd my-app
  npm run dev
  ```

- **Create with Remix (Full-stack React 19):**
  ```bash
  npx create-remix@latest my-app
  cd my-app
  npm run dev
  ```

### Legacy (React 18)

- **Create React App (CRA - deprecated, use Vite instead):**
  ```bash
  npx create-react-app my-app
  cd my-app
  npm start
  ```

---

## 📦 Package Management (2026)

### npm (Node Package Manager)
```bash
# Install dependencies
npm install <package-name>

# Install dev dependency
npm install -D <package-name>

# Install specific version
npm install react@19

# Update all dependencies
npm update

# Remove package
npm uninstall <package-name>

# Check outdated packages
npm outdated
```

### pnpm (Performant npm - Recommended)
```bash
# Install pnpm globally
npm install -g pnpm

# Install dependencies (3x faster than npm)
pnpm install <package-name>
pnpm add <package-name>

# Dev dependency
pnpm add -D <package-name>

# Update
pnpm update

# Remove
pnpm remove <package-name>
```

### yarn (Fast, reliable)
```bash
# Install yarn
npm install -g yarn

# Add dependency
yarn add <package-name>

# Dev dependency
yarn add -D <package-name>

# Remove
yarn remove <package-name>
```

### bun (Ultra-fast, all-in-one)
```bash
# Install bun
curl -fsSL https://bun.sh/install | bash

# Install dependencies
bun install <package-name>
bun add <package-name>

# Dev dependency
bun add -d <package-name>

# Remove
bun remove <package-name>
```

---

## ▶️ Running & Building
- **Start development server:**
  ```bash
  npm start     # (CRA)
  npm run dev   # (Vite, Next.js)
  ```

- **Build production files:**
  ```bash
  npm run build
  ```

- **Preview production build (Vite):**
  ```bash
  npm run preview
  ```

---

## ⚛️ React-Specific Utilities
- **Generate a new component manually (no built-in command, but a common pattern):**
  ```bash
  mkdir src/components
  touch src/components/MyComponent.jsx
  ```

- **Install React Router (for navigation):**
  ```bash
  npm install react-router-dom
  ```

- **Install state management (e.g., Redux Toolkit):**
  ```bash
  npm install @reduxjs/toolkit react-redux
  ```

- **Install Tailwind CSS (styling utility):**
  ```bash
  npm install -D tailwindcss postcss autoprefixer
  npx tailwindcss init -p
  ```

---

## 🛠 Debugging & Utilities
- **Lint code (if ESLint installed):**
  ```bash
  npm run lint
  ```

- **Format code with Prettier:**
  ```bash
  npx prettier --write .
  ```

- **Check outdated dependencies:**
  ```bash
  npm outdated
  ```

---

## 🆕 React 19 Specific

### Install React 19
```bash
# Install React 19
npm install react@19 react-dom@19

# With TypeScript types
npm install -D @types/react@19 @types/react-dom@19
```

### React 19 Migration
```bash
# Run codemods for automatic migration
npx react-codemod react-19/replace-reactdom-render ./src
npx react-codemod react-19/replace-string-ref ./src
npx react-codemod react-19/replace-act-import ./src
```

---

## 🔧 Modern Dev Tools (2026)

### Biome (Fast Linter + Formatter - Alternative to ESLint/Prettier)
```bash
# Install Biome
npm install -D @biomejs/biome

# Initialize
npx @biomejs/biome init

# Check code
npx @biomejs/biome check .

# Format code
npx @biomejs/biome format --write .

# Fix issues
npx @biomejs/biome check --apply .
```

### Vite 5+ Features
```bash
# Install Vite plugins
npm install -D @vitejs/plugin-react-swc  # SWC for faster builds

# Environment-specific build
npm run build -- --mode production
npm run build -- --mode staging
```

### TypeScript
```bash
# Install TypeScript
npm install -D typescript @types/react @types/react-dom

# Initialize tsconfig
npx tsc --init

# Type check
npx tsc --noEmit
```

---

## ⚛️ React Ecosystem (2026)

### State Management

```bash
# Redux Toolkit (RTK Query for API)
npm install @reduxjs/toolkit react-redux

# Zustand (Lightweight)
npm install zustand

# Jotai (Atomic)
npm install jotai

# TanStack Query v5 (Server State)
npm install @tanstack/react-query
```

### Routing

```bash
# React Router v6+
npm install react-router-dom

# TanStack Router (Type-safe)
npm install @tanstack/react-router
```

### Forms

```bash
# React Hook Form
npm install react-hook-form

# Zod (Validation)
npm install zod

# React Hook Form + Zod
npm install @hookform/resolvers
```

### UI Libraries

```bash
# shadcn/ui (Copy-paste components)
npx shadcn-ui@latest init

# Radix UI (Headless)
npm install @radix-ui/react-dialog

# Headless UI
npm install @headlessui/react

# Mantine
npm install @mantine/core @mantine/hooks
```

### Styling

```bash
# Tailwind CSS v4 (2026)
npm install -D tailwindcss@next postcss autoprefixer

# Styled Components
npm install styled-components

# CSS Modules (built-in with Vite)
# No installation needed
```

---

## 🧪 Testing (2026)

### Vitest (Modern Test Runner - Recommended)
```bash
# Install Vitest
npm install -D vitest @vitest/ui

# Install React Testing Library
npm install -D @testing-library/react @testing-library/jest-dom
npm install -D @testing-library/user-event

# Run tests
npm run test
npm run test:ui  # With UI
```

### Playwright (E2E Testing)
```bash
# Install Playwright
npm init playwright@latest

# Run E2E tests
npx playwright test
npx playwright test --ui  # Interactive mode
npx playwright codegen    # Record tests
```

### MSW (Mock Service Worker - API Mocking)
```bash
# Install MSW
npm install -D msw

# Initialize
npx msw init public/
```

---

## 🚀 Build & Deploy

### Production Build
```bash
# Vite build
npm run build

# Analyze bundle size
npm install -D rollup-plugin-visualizer
npm run build -- --visualize

# Build with source maps
npm run build -- --sourcemap
```

### Preview Production Build
```bash
# Vite preview
npm run preview

# Serve with custom port
npm run preview -- --port 3000
```

### Deploy Commands
```bash
# Vercel
npm install -g vercel
vercel deploy

# Netlify
npm install -g netlify-cli
netlify deploy --prod

# Build for static hosting
npm run build
# Then upload /dist folder
```

---

## 🔍 Code Quality

### ESLint (React 19 Config)
```bash
# Install ESLint
npm install -D eslint

# Install React plugin
npm install -D eslint-plugin-react eslint-plugin-react-hooks

# Run linter
npx eslint .
npx eslint . --fix
```

### Prettier
```bash
# Install Prettier
npm install -D prettier

# Format code
npx prettier --write .
npx prettier --check .
```

### Biome (All-in-one)
```bash
# Install Biome (faster alternative)
npm install -D @biomejs/biome

# Format and lint
npx @biomejs/biome check --apply .
```

---

## 📊 Performance & Monitoring

### React DevTools
```bash
# Install globally
npm install -g react-devtools

# Run standalone
react-devtools
```

### Bundle Analyzer
```bash
# Vite
npm install -D rollup-plugin-visualizer

# Webpack (CRA)
npm install -D webpack-bundle-analyzer
```

### Lighthouse
```bash
# Run Lighthouse audit
npx lighthouse https://your-site.com --view
```

---

## 🔐 Security

### Dependency Audit
```bash
# npm audit
npm audit
npm audit fix

# pnpm audit
pnpm audit
pnpm audit --fix

# yarn audit
yarn audit
```

### Update Dependencies
```bash
# Check for updates
npx npm-check-updates

# Update all to latest
npx npm-check-updates -u
npm install
```

---

## 🎯 Quick Reference Table

| Task | Command |
|------|---------|
| Create React app | `npm create vite@latest` |
| Install React 19 | `npm install react@19 react-dom@19` |
| Start dev server | `npm run dev` |
| Build for production | `npm run build` |
| Run tests | `npm run test` |
| Lint code | `npx eslint .` |
| Format code | `npx prettier --write .` or `npx biome format --write .` |
| Type check | `npx tsc --noEmit` |
| Run E2E tests | `npx playwright test` |
| Analyze bundle | Add visualizer plugin + `npm run build` |
| Security audit | `npm audit` |
| Update deps | `npx npm-check-updates -u` |

---

## 💡 Pro Tips

1. **Use pnpm or bun for faster installs** - 3-5x faster than npm
2. **Use Biome instead of ESLint + Prettier** - 100x faster, single tool
3. **Use Vite instead of CRA** - 10-100x faster dev server
4. **Use Vitest instead of Jest** - Faster, better DX, Vite integration
5. **Use TanStack Query for server state** - Don't manage API state manually
6. **Use shadcn/ui for components** - Copy-paste, fully customizable
7. **Enable React Compiler** - Automatic optimization in React 19

---

*Last Updated: January 2026 - React 19*
