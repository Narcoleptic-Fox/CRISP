# Security Policy

## Supported Versions

We release patches for security vulnerabilities in the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

We take the security of CRISP seriously. If you believe you have found a security vulnerability, please report it to us as described below.

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to the project maintainers. You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

Please include the following information in your report:

* Type of issue (e.g. buffer overflow, SQL injection, cross-site scripting, etc.)
* Full paths of source file(s) related to the manifestation of the issue
* The location of the affected source code (tag/branch/commit or direct URL)
* Any special configuration required to reproduce the issue
* Step-by-step instructions to reproduce the issue
* Proof-of-concept or exploit code (if possible)
* Impact of the issue, including how an attacker might exploit the issue

This information will help us triage your report more quickly.

## Preferred Languages

We prefer all communications to be in English.

## Security Best Practices

When using CRISP in your applications, we recommend:

1. **Input Validation**: Always validate inputs at the boundary using the built-in validation behaviors
2. **Authorization**: Implement proper authorization using the AuthorizationBehavior
3. **Rate Limiting**: Use RateLimitingBehavior to prevent abuse
4. **Input Sanitization**: Enable InputSanitizationBehavior for user-facing applications
5. **Error Handling**: Use structured error handling to avoid information leakage
6. **Dependencies**: Keep all dependencies up to date

## Security Features

CRISP includes several built-in security features:

* **Input Sanitization**: Automatic sanitization of user inputs
* **Authorization Pipeline**: Role-based and policy-based authorization
* **Rate Limiting**: Built-in rate limiting capabilities  
* **Validation Pipeline**: Multi-layer validation (FluentValidation + DataAnnotations)
* **Error Handling**: Secure error responses that don't leak sensitive information
* **Security Auditing**: Optional security audit logging