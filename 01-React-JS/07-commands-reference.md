# 🚀 React Important Commands

## 📦 Project Setup & Initialization
- **Create a new React app (with Vite, recommended for speed):**
  ```bash
  npm create vite@latest my-app
  cd my-app
  npm install
  npm run dev
  ```

- **Create a new React app (with CRA – Create React App):**
  ```bash
  npx create-react-app my-app
  cd my-app
  npm start
  ```

---

## 📦 Package Management
- **Install dependencies:**
  ```bash
  npm install <package-name>
  ```
  or
  ```bash
  yarn add <package-name>
  ```

- **Remove dependencies:**
  ```bash
  npm uninstall <package-name>
  ```

- **Install dev dependency:**
  ```bash
  npm install <package-name> --save-dev
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
