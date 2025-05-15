---
applyTo: "**"
---

# General Programming Guidelines

## Code Organization

- **Consistency**: Follow consistent coding patterns throughout your project.
- **Modularity**: Break code into reusable modules with clear responsibilities.
- **DRY Principle**: Don't Repeat Yourself - avoid code duplication.
- **KISS Principle**: Keep It Simple, Stupid - prefer simple solutions over complex ones.
- **SOLID Principles**:
  - Single Responsibility Principle
  - Open/Closed Principle
  - Liskov Substitution Principle
  - Interface Segregation Principle
  - Dependency Inversion Principle

## Naming Conventions

- Use descriptive, clear names for variables, functions, and classes.
- Be consistent with your naming style (camelCase, PascalCase, snake_case, etc.).
- Avoid cryptic abbreviations unless they are widely understood.

## Documentation

- Document the "why" not just the "what".
- Keep comments up-to-date with code changes.
- Write self-documenting code where possible.
- Include examples for non-obvious functionality.

## Error Handling

- Never silently swallow exceptions.
- Provide meaningful error messages.
- Log errors with sufficient context for debugging.
- Design for failure - consider what happens when things go wrong.

## Testing

- Write tests for all new functionality.
- Consider Test-Driven Development (TDD) for complex features.
- Include unit tests, integration tests, and end-to-end tests as appropriate.
- Aim for high test coverage on critical paths.

## Version Control

- Write clear, descriptive commit messages.
- Make small, focused commits.
- Use branches for features, bug fixes, or experiments.
- Review code before merging into the main branch.

## Performance Considerations

- Write correct code first, then optimize if necessary.
- Profile before optimizing - identify actual bottlenecks.
- Consider time and space complexity for algorithms.
- Be mindful of resource usage (memory, CPU, network, etc.).

## Security

- Validate all user inputs.
- Protect against common vulnerabilities (XSS, CSRF, SQL injection, etc.).
- Follow the principle of least privilege.
- Keep dependencies updated to patch security vulnerabilities.

## Code Reviews

- Be constructive and respectful in reviews.
- Look for bugs, edge cases, and maintainability issues.
- Consider readability and future developers.
- Verify that tests are adequate and passing.

## Continuous Learning

- Stay updated with best practices and new technologies.
- Share knowledge with your team.
- Retrospect on past projects to improve future ones.
- Be open to feedback and alternative approaches.
