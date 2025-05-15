---
applyTo: "**/*.js"
---
# JavaScript Programming Guidelines

## Syntax and Features

- Use ES6+ features whenever possible.
- Use `const` for variables that don't change, `let` for those that do. Avoid `var`.
- Use arrow functions for concise anonymous functions and to preserve `this` context.
- Use template literals for string interpolation.
- Use destructuring for objects and arrays.
- Use spread/rest operators for working with arrays and objects.
- Use default parameters and optional chaining.

## Naming Conventions

- **Functions, Variables, Methods, Properties**: Use camelCase (e.g., `getUserData`, `firstName`).
- **Classes, Components**: Use PascalCase (e.g., `UserProfile`, `DataService`).
- **Constants**: Use UPPER_SNAKE_CASE (e.g., `API_URL`, `MAX_RETRIES`).
- **Private Properties/Methods**: Use underscore prefix (e.g., `_privateMethod`).

## Functions

- Prefer pure functions (no side effects, same input = same output).
- Keep functions small and focused on a single task.
- Use default parameters instead of conditionals to set default values.
- Return early from functions to avoid deep nesting.

```javascript
// Good
function getUserName(user) {
  if (!user) return 'Guest';
  return user.name;
}

// Avoid
function getUserName(user) {
  let name;
  if (user) {
    name = user.name;
  } else {
    name = 'Guest';
  }
  return name;
}
```

## Modules

- Use ES modules (`import`/`export`) instead of CommonJS (`require`).
- Export a single responsibility per module.
- Use named exports for multiple functions/classes.
- Use default export for the primary function/class of a file.

## Asynchronous Code

- Use Promises or async/await for asynchronous operations.
- Always handle Promise rejections and errors.
- Avoid callback hell.

```javascript
// Good
async function fetchUserData(userId) {
  try {
    const response = await fetch(`/api/users/${userId}`);
    return await response.json();
  } catch (error) {
    console.error('Failed to fetch user:', error);
    return null;
  }
}
```

## Objects and Classes

- Use shorthand for object properties and methods when possible.
- Use computed property names when creating objects with dynamic keys.
- Use class syntax for creating objects with methods.
- Use getters and setters for computed properties.

## Arrays

- Use array methods (`map`, `filter`, `reduce`, etc.) instead of loops when applicable.
- Use the spread operator for array manipulation.
- Use `Array.from()` or the spread operator to convert array-like objects.

## Error Handling

- Use try/catch blocks to handle exceptions.
- Throw specific error objects with descriptive messages.
- Consider custom error classes for specific error types.

## Testing

- Write unit tests for functions and components.
- Use Jest or Mocha for testing frameworks.
- Use mocks for external dependencies.

## Tools and Linting

- Use ESLint to enforce code style.
- Consider Prettier for code formatting.
- Use a .editorconfig file for consistent editor settings.

## Resources

- [Mozilla Developer Network (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [Airbnb JavaScript Style Guide](https://github.com/airbnb/javascript)
- [JavaScript.info](https://javascript.info/)
- [Clean Code JavaScript](https://github.com/ryanmcdermott/clean-code-javascript)
