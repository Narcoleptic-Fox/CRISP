---
applyTo: "**/*.ts"
---
# TypeScript Programming Guidelines

## Naming Conventions

- **Classes, Interfaces, Types, Enums**: Use PascalCase (e.g., `UserAccount`, `ApiResponse`).
- **Variables, Functions, Methods, Properties**: Use camelCase (e.g., `userName`, `getData()`).
- **Constants**: Use UPPER_SNAKE_CASE for true constants (e.g., `MAX_RETRY_COUNT`).
- **Private/Protected Members**: Consider prefixing with underscore (e.g., `_privateVariable`).
- **Type Parameters**: Use PascalCase, prefixed with `T` (e.g., `TEntity`, `TKey`).

## Types

- Always define proper types rather than using `any` when possible.
- Use interfaces for object shapes that represent data.
- Use type aliases for complex types or unions.
- Prefer type annotations for function returns even when TypeScript can infer them.
- Use `unknown` instead of `any` when the type is truly unknown.

```typescript
// Good
interface User {
  id: number;
  name: string;
  email?: string;
}

// Avoid
const getUser = () => { /* returns any */ };
```

## Null Handling

- Enable `strictNullChecks` in `tsconfig.json`.
- Use the optional chaining operator (`?.`) for potentially null/undefined properties.
- Use the nullish coalescing operator (`??`) for default values.
- Use non-null assertion operator (`!`) sparingly and only when you're certain.

## Modern TypeScript Features

- Use template literal types for string manipulation at the type level.
- Leverage conditional types for complex type relationships.
- Use mapped types to transform existing types.
- Use utility types like `Partial<T>`, `Required<T>`, `Pick<T>`, etc.

## Asynchronous Code

- Use `async`/`await` instead of raw Promises when possible.
- Always handle Promise rejections.
- Type your Promise return values.

```typescript
// Good
async function fetchUser(id: string): Promise<User> {
  const response = await fetch(`/api/users/${id}`);
  return response.json();
}
```

## ES Module Syntax

- Use named exports for multiple exports from a file.
- Use default exports for main components/classes of a file.
- Be consistent with import styles.

## Type Guards

- Use user-defined type guards with `is` for runtime type checking.
- Consider using discriminated unions for complex object hierarchies.

```typescript
function isUser(obj: any): obj is User {
  return obj && typeof obj.id === 'number' && typeof obj.name === 'string';
}
```

## Configuration

- Use a consistent `tsconfig.json` across projects.
- Consider enabling strict mode for more robust typing.
- Use ESLint with TypeScript-specific rules.

## Testing

- Use Jest or Mocha with ts-jest for unit testing.
- Type test fixtures and mocks.

## Resources

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [TypeScript Deep Dive](https://basarat.gitbook.io/typescript/)
- [TypeScript ESLint](https://github.com/typescript-eslint/typescript-eslint)
