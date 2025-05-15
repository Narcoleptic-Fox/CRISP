---
applyTo: "**/*review*.md,**/*PULL_REQUEST_TEMPLATE.md,**/*CODE_REVIEW*.md"
---

# Code Review Checklists

## Overview

This document provides standardized checklists for conducting thorough code reviews. These checklists help ensure consistent quality, knowledge sharing, and collaborative improvement across all projects.

## General Code Review Principles

- **Be Respectful**: Critique code, not people
- **Be Thorough**: Take the time needed to perform a proper review
- **Be Clear**: Provide specific, actionable feedback
- **Be Collaborative**: Reviews are a learning opportunity for everyone
- **Be Pragmatic**: Focus on significant issues, not nitpicking

## Pull Request Quality Checklist

Before submitting a pull request, authors should ensure:

- [ ] PR has a descriptive title following project conventions
- [ ] PR description explains the change, why it's needed, and how it was implemented
- [ ] PR is appropriately sized (< 400 lines of code when possible)
- [ ] PR is linked to relevant issues/tickets
- [ ] All CI checks are passing
- [ ] Self-review has been performed

## General Code Review Checklist

For all code reviews:

- [ ] Code follows the project's style guide and conventions
- [ ] Changes solve the stated problem or requirement
- [ ] No unnecessary code changes or scope creep
- [ ] No commented-out code unless clearly explained
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] No obvious performance issues
- [ ] Error handling is appropriate and comprehensive
- [ ] Logging is sufficient and follows project standards
- [ ] Documentation is updated (including comments, READMEs, API docs)
- [ ] Test coverage is adequate

## Security Review Checklist

Security-focused checks:

- [ ] Input validation is implemented for all user inputs
- [ ] Authentication and authorization checks are in place
- [ ] No sensitive data is exposed (API keys, credentials, PII)
- [ ] SQL queries are parameterized to prevent injection
- [ ] Output is properly encoded to prevent XSS
- [ ] CSRF protection is implemented for state-changing operations
- [ ] Security headers are properly configured
- [ ] Secure defaults are used
- [ ] No hardcoded secrets or credentials
- [ ] Dependencies are free from known vulnerabilities

## Frontend-Specific Review Checklist

For frontend code:

- [ ] UI matches design specifications/mockups
- [ ] Responsive design works across target devices
- [ ] Accessibility requirements are met (ARIA, keyboard navigation, screen readers)
- [ ] Client-side validations are implemented
- [ ] Error states are handled gracefully
- [ ] Loading states are implemented
- [ ] Browser compatibility requirements are met
- [ ] No memory leaks in component lifecycles
- [ ] CSS follows project organization and naming conventions
- [ ] Proper event handling and cleanup
- [ ] Internationalization (i18n) support where required

## Backend-Specific Review Checklist

For backend code:

- [ ] API endpoints follow RESTful conventions or GraphQL schemas
- [ ] Database queries are optimized
- [ ] Transactions are used appropriately
- [ ] Rate limiting is implemented where necessary
- [ ] Pagination is implemented for list endpoints
- [ ] Error responses follow API standards
- [ ] API versioning strategy is followed
- [ ] Background processes handle failures gracefully
- [ ] Resource cleanup is implemented (connections, file handles)
- [ ] Appropriate HTTP status codes are used

## Test Code Review Checklist

For test code:

- [ ] Tests are clear and readable
- [ ] Tests verify functionality, not implementation details
- [ ] Edge cases are covered
- [ ] Mocks and stubs are used appropriately
- [ ] Test data is appropriate and maintainable
- [ ] Test setup and teardown is handled properly
- [ ] Tests are independent and don't depend on execution order
- [ ] Performance tests have appropriate thresholds
- [ ] Test coverage meets project requirements
- [ ] Flaky tests are addressed

## Infrastructure Review Checklist

For infrastructure code:

- [ ] Infrastructure changes follow IaC (Infrastructure as Code) principles
- [ ] Resource naming follows conventions
- [ ] Least privilege principles are applied
- [ ] High availability considerations are addressed
- [ ] Scalability considerations are addressed
- [ ] Monitoring and alerting are configured
- [ ] Backup and recovery processes are defined
- [ ] Cost implications are considered and documented
- [ ] Security groups and network ACLs are properly configured
- [ ] Secrets management follows best practices

## Database Changes Review Checklist

For database changes:

- [ ] Schema changes are backward compatible or have migration plans
- [ ] Indexes are appropriate for query patterns
- [ ] Data types are appropriate
- [ ] Constraints (foreign keys, unique, etc.) are properly defined
- [ ] Large migrations have rollback plans
- [ ] Database permissions follow least privilege
- [ ] No direct table manipulation in application code (use stored procedures/ORMs)
- [ ] No sensitive data stored in plain text
- [ ] Database naming conventions are followed
- [ ] Performance impact of changes is considered

## Review Feedback Guidelines

When providing review feedback:

- Explain the "why" behind suggestions
- Provide links to relevant documentation or examples
- Distinguish between required changes and suggestions
- Offer solutions, not just point out problems
- Use clear, constructive language
- Acknowledge good practices and clever solutions

## Code Review Process

1. **Preparation**: Understand the requirements and context
2. **First Pass**: Quick overview of the entire change
3. **Detailed Review**: Thorough examination with the appropriate checklist
4. **Testing**: Run the code locally if necessary
5. **Feedback**: Provide clear, actionable comments
6. **Follow-up**: Verify that feedback was addressed
7. **Approval**: Approve the PR when requirements are met

## Code Review Size Guidelines

- **Small** (< 200 lines): Complete review in one session
- **Medium** (200-500 lines): Break into logical sections
- **Large** (> 500 lines): Consider breaking into smaller PRs

## Review Comment Templates

### For requesting changes

```
I suggest changing [specific code] because [reason]. This would improve [benefit].
Example:
```

### For suggestions

```
Consider [alternative approach] here, which might [benefit].
```

### For clarification

```
Could you explain the reasoning behind [specific code]? I'm trying to understand how it [functionality].
```

## Post-Approval Changes

After a PR has been approved:

- Only make the changes requested in the review
- For new changes, request another review
- Document any post-approval changes in comments

## Special Case Reviews

### Emergency Fixes

For urgent production fixes:

- Prioritize review timeliness
- Focus on correctness and potential regressions
- Document technical debt for follow-up
- Ensure post-incident review

### Large Refactoring

For significant refactoring efforts:

- Review in smaller logical chunks if possible
- Focus on maintaining functionality
- Verify comprehensive test coverage
- Consider pair review for complex changes
