---
title: Client
description: The React client, how its types are generated from Core, how it talks to the API, and how sign-in works.
nav_order: 10
---

# Client

`CRISP.Client` is a React 19 single-page application built with Vite, styled with Tailwind 4 and
shadcn components, and typed from the server's contracts. This page explains the parts that make
it a CRISP client rather than any React app: generated types, the API layer, and the sign-in gate.

## Types are generated, never written

Every Core record that crosses the wire is marked `[ExportTsInterface]` (enums `[ExportTsEnum]`).
Building `CRISP.Core` runs TypeGen and writes one TypeScript file per type into
`src/CRISP.Client/src/api/`, then patches the three marker types (`ICommand`, `IQuery`,
`CreateCommand`) into a shape TypeScript can express and formats the result with Prettier.

`src/api/` is gitignored. Adding a field to a Core record flows to the client on the next build;
a hand-written DTO would drift, so there are none.

::: callout warning "A type is only generated if something marked reaches it"
`PagedQuery<T>` and `PagedResponse<T>` have no attribute of their own; they are emitted because
`GetItems` and `GetUsers` are marked and reference them. Removing the last marked consumer of a
shared type removes the type.
:::

## The API layer

`lib/api.ts` exports one function, `apiFetch<T>(path, init)`, and one error type, `ApiError`.
It acquires a token from MSAL (silently, or by redirect when interaction is required), sends the
request with `Authorization: Bearer` and `Accept: application/json`, throws `ApiError(status,
body)` on any non-2xx, and returns `undefined` for `204`.

Features wrap it in TanStack Query hooks (`useItems`, `useItemMutations`, ...). Mutations
invalidate the feature's query key on success. Error bodies are RFC 7807 problem details, so a
`RejectedException` message from the server is shown to the user through `extractErrorMessage`.

## Sign-in

`main.tsx` initialises MSAL, restores an active account, handles the redirect promise, and wraps
the router in `MsalProvider`. `ProtectedRoute` wraps every page in `AuthGate`, which signs the user
in by redirect when there is no account and then asks the server who they are.

`useCurrentUser()` calls `/api/v1/Users/me` and classifies the answer: `authorized` (a row exists
and is active), `deactivated` (a row exists and is not), or `unprovisioned` (`403` with
`code: "unprovisioned"`). `useCanWrite()` is the single place the UI asks whether to show create,
edit, delete and upload controls; it answers yes for Editor and Admin, and no while loading, so the
UI fails closed.

With `--auth none` the `.noauth` versions of `main.tsx`, `lib/api.ts`, `protected-route.tsx`,
`useCurrentUser.ts` and `top-nav.tsx` replace these: no MSAL, no bearer token, every route open,
`useCanWrite()` always true.

## Layout and features

```
src/
  api/            generated, do not edit
  auth/           MSAL config, AuthGate, useCurrentUser
  components/     app shell, top nav, user menu, shared widgets, ui/ (shadcn)
  features/       one folder per slice: hooks and dialogs (items/, users/, settings/, storage/)
  lib/            api.ts, utils
  routes/         pages and the router
```

The dashboard composes feature overviews; the sample contributes `ItemsOverview`. A new feature
adds a folder under `features/`, a page under `routes/`, a route in `routes/index.tsx` and a
navigation entry in `top-nav.tsx`.

## Checks

`npm run check` runs lint, format check, tests and a production build, in that order, and is what
the client CI job runs. `npx tsc --noEmit` alone is enough to know the generated types still match
the client.
