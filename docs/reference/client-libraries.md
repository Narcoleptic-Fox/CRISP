---
title: Client libraries
description: The npm dependencies of CRISP.Client and why each is in the stack.
nav_order: 2
---

# Client libraries

Versions live in `src/CRISP.Client/package.json`; `package-lock.json` is committed and CI runs
`npm ci`. Packages marked *option* are absent from a solution generated with `--auth none`
(the `.noauth` files do not import them; they are still installed).

## Runtime

| Package | Why |
| --- | --- |
| `react`, `react-dom` | React 19. Function components, hooks, no class components anywhere |
| `react-router-dom` | Client-side routing with nested layouts (`AppShell` wraps every page) |
| `@tanstack/react-query` | Server state: fetching, caching, invalidation after mutations. Every feature hook is a query or mutation; there is no global store |
| `@azure/msal-browser`, `@azure/msal-react` | Entra ID sign-in by redirect and silent token acquisition for `apiFetch`. *option `--auth entra`* |
| `react-hook-form`, `@hookform/resolvers`, `zod` | Forms with a schema. Zod schemas mirror the Core validators' limits so the user sees the error before the server does |
| `tailwindcss`, `@tailwindcss/vite`, `tw-animate-css` | Utility CSS, compiled by the Vite plugin (no PostCSS config to maintain) |
| `shadcn`, `radix-ui`, `class-variance-authority`, `clsx`, `tailwind-merge` | The component layer. shadcn generates components into `components/ui/` as source you own, over Radix primitives; `cva`, `clsx` and `tailwind-merge` are what those generated files import |
| `lucide-react` | Icons, tree-shaken |
| `cmdk` | The command palette and autocomplete combobox (owner names) |
| `react-day-picker`, `date-fns` | Date picking and formatting. `date-fns` over Day.js for tree-shaking and immutability |
| `sonner` | Toasts, including the error toast `extractErrorMessage` feeds |
| `next-themes` | Light, dark and system theme with no flash, despite the name it has no Next.js dependency |
| `@fontsource-variable/inter` | The UI font, self-hosted so the CSP needs no font origin |

## Build

| Package | Why |
| --- | --- |
| `vite`, `@vitejs/plugin-react` | Dev server with HMR (what SpaProxy starts) and the production bundle that the Dockerfile copies to `wwwroot` |
| `typescript` | `npm run build` is `tsc --noEmit && vite build`: a type error fails the build, which is how a Core change that breaks the client is caught |
| `eslint`, `@eslint/js`, `typescript-eslint`, `eslint-plugin-react-hooks`, `eslint-plugin-react-refresh`, `globals` | Lint, including the rules-of-hooks rule that catches the conditional-hook bug before it ships |
| `prettier`, `prettier-plugin-tailwindcss` | Formatting, with class ordering, so diffs are about behaviour |
| `@types/*` | Type definitions for Node, React and React DOM |

## Generated, not installed

`src/api/` is written by TypeGen when Core builds and is gitignored. It is not a dependency and
must never be edited; see [Client](/architecture/client/).
