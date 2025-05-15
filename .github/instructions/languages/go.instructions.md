---
applyTo: "**/*.go"
---
# Go Programming Guidelines

## Code Style

- Follow the official [Go Code Review Comments](https://github.com/golang/go/wiki/CodeReviewComments) and [Effective Go](https://golang.org/doc/effective_go) guidelines.
- Use `gofmt` or `goimports` to automatically format your code.
- Organize imports into groups: standard library, third-party packages, local packages.
- Keep functions short and focused on a single task.
- Avoid unnecessary else statements; return early instead.
- Use meaningful variable names; short ones for short scopes, descriptive ones for larger scopes.
- Avoid package-level variables when possible.

## Naming Conventions

- **Package Names**: Short, lowercase, no underscores or camelCase: `http`, `strings`, `io`.
- **Variable and Function Names**: 
  - Use camelCase: `userCount`, `waitGroup`.
  - Use succinct but descriptive names.
  - Use short names for limited scopes (i, j, k for loops).
- **Interface Names**: One-method interfaces named with "er" suffix: `Reader`, `Writer`, `Formatter`.
- **Exported Names**: Capitalize the first letter of exported functions, variables, constants, types: `Println`, `StatusOK`.
- **Unexported Names**: Lowercase first letter for unexported elements: `conn`, `mutex`.
- **Constants**: Use camelCase or uppercase depending on scope and purpose.
- **Error Variables**: Prefix with `Err` or `err`: `ErrNotFound`, `errInvalidInput`.

## Package Organization

- Create a separate package for each major component or functionality.
- Design packages around coherent functionalities, not just data types.
- Follow the [Standard Go Project Layout](https://github.com/golang-standards/project-layout) for larger projects.
- Keep `main` packages small, delegating most functionality to importable packages.
- For library packages, focus on clear API design with minimal public functions and types.

## Error Handling

- Always check errors and handle them appropriately.
- Return errors rather than using panic (except in truly exceptional cases).
- Use the `errors` package or `fmt.Errorf` to create descriptive error messages.
- For custom error types, implement the `error` interface.
- Consider using error wrapping (Go 1.13+): `fmt.Errorf("failed to fetch data: %w", err)`.
- Use error sentinel values (`ErrNotFound`, etc.) for specific expected errors.

```go
if err != nil {
    return fmt.Errorf("failed to process file %s: %w", filename, err)
}
```

## Concurrency

- Use goroutines wisely, considering their overhead and management.
- Always use appropriate synchronization mechanisms (`sync.Mutex`, `sync.WaitGroup`, channels).
- Prefer channels for communication between goroutines.
- Consider using context for cancellation and timeouts.
- Be careful with goroutine leaks; ensure all goroutines can terminate.
- Use buffered channels when appropriate to avoid blocking.

```go
func process(jobs <-chan Job, results chan<- Result, wg *sync.WaitGroup) {
    defer wg.Done()
    
    for job := range jobs {
        results <- processJob(job)
    }
}
```

## Data Structures

- Use slices instead of arrays in most cases.
- Use maps for lookups and sets.
- Consider using custom types for semantic clarity: `type UserID string`.
- Use embedding for composition over inheritance.
- Use struct tags for encoding/decoding: `json:"name,omitempty"`.
- Initialize structs with field names for clarity and maintainability.

## Functions and Methods

- Keep functions short and focused on one task.
- Use multiple return values to return both results and errors.
- Use named return parameters for documentation or when they clarify code.
- Methods with pointer receivers can modify the receiver; methods with value receivers cannot.
- Choose pointer receivers when the struct is large or needs to be modified.

```go
func (u *User) UpdateName(name string) error {
    if name == "" {
        return errors.New("name cannot be empty")
    }
    u.Name = name
    return nil
}
```

## Interfaces

- Keep interfaces small, preferably one or two methods.
- Define interfaces where they're used, not where types are defined.
- Use interfaces to accept the minimum required functionality.
- Remember that empty interfaces (`interface{}`) lose type safety.
- Consider using type assertions or type switches when working with `interface{}`.

## Testing

- Use the standard `testing` package for unit tests.
- Name test files with `_test.go` suffix.
- Name test functions as `TestXxx` where Xxx describes what you're testing.
- Use table-driven tests for testing multiple cases.
- Use `testify` or similar packages for more advanced assertions.
- Write benchmarks for performance-critical code: `BenchmarkXxx`.

```go
func TestAdd(t *testing.T) {
    tests := []struct{
        name string
        a, b int
        want int
    }{
        {"positive", 2, 3, 5},
        {"negative", -2, -3, -5},
        {"mixed", -2, 3, 1},
    }
    
    for _, tt := range tests {
        t.Run(tt.name, func(t *testing.T) {
            got := Add(tt.a, tt.b)
            if got != tt.want {
                t.Errorf("Add(%d, %d) = %d, want %d", tt.a, tt.b, got, tt.want)
            }
        })
    }
}
```

## Documentation

- Write godoc comments for all exported functions, types, constants, and variables.
- Start comments with the name of the thing being documented.
- Format examples as testable code using the `Example` test function format.
- Include example usage in documentation comments.

```go
// Add returns the sum of a and b.
func Add(a, b int) int {
    return a + b
}
```

## Resources

- [Effective Go](https://golang.org/doc/effective_go)
- [Go Code Review Comments](https://github.com/golang/go/wiki/CodeReviewComments)
- [Standard Go Project Layout](https://github.com/golang-standards/project-layout)
- [Go by Example](https://gobyexample.com/)
- [The Go Blog](https://blog.golang.org/)
- [Go Proverbs](https://go-proverbs.github.io/)
