---
applyTo: "**/ci-cd.md,**/pipeline.md,**/cicd*.md"
---

# CI/CD Workflows Guidelines

## Overview

This document outlines platform-agnostic standards and best practices for Continuous Integration (CI) and Continuous Deployment (CD) workflows. For platform-specific implementations, refer to:

- [GitHub Actions Workflows](github-actions-workflows.instructions.md)
- [GitLab CI/CD Workflows](gitlab-ci-workflows.instructions.md)

Following these guidelines ensures reliable, secure, and efficient deployment pipelines regardless of the CI/CD platform used.

## General Principles

- **Automation First**: Automate everything that can be automated
- **Fast Feedback**: Design pipelines to provide feedback as quickly as possible
- **Reliability**: Pipelines should be reliable and fail only when there are actual issues
- **Idempotency**: Running the same pipeline multiple times should produce the same result
- **Security**: Pipelines should enforce security checks and protect sensitive information
- **Transparency**: Pipeline status, history, and logs should be easily accessible

## Standard Pipeline Components

A well-designed CI/CD pipeline should generally include these components, regardless of the platform used:

### 1. Continuous Integration

#### Code Quality Checks

Every CI/CD pipeline should include automated code quality checks:

- **Linting**: Analyze code for potential errors and style violations
- **Formatting**: Ensure code follows consistent formatting standards
- **Static Analysis**: Identify potential bugs, vulnerabilities, and code smells

#### Testing

Comprehensive testing should be part of every pipeline:

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **End-to-End Tests**: Test complete user workflows
- **Coverage Reporting**: Track code coverage metrics

#### Security Scanning

Security checks should be integrated into every pipeline:

- **Dependency Scanning**: Check for vulnerabilities in dependencies
- **Static Application Security Testing (SAST)**: Analyze code for security vulnerabilities
- **Secret Detection**: Identify potential secrets or credentials in code
- **Container Scanning**: Check container images for vulnerabilities

### 2. Continuous Deployment

#### Build and Artifact Creation

Build processes should be automated and produce consistent artifacts:

- **Compilation/Transpilation**: Convert source code into deployable form
- **Asset Processing**: Optimize assets for production
- **Artifact Creation**: Package the application for deployment
- **Artifact Storage**: Store artifacts securely for later deployment

#### Deployment

Deployment processes should be automated with appropriate safeguards:

- **Environment Management**: Deploy to different environments (dev, staging, production)
- **Approval Processes**: Require manual approvals for critical environments
- **Rollback Capabilities**: Support easy rollbacks if issues are detected
- **Post-deployment Verification**: Verify successful deployment

## Environment-Specific Configurations

### Development

- Automatic deployments on commit to development branches
- Full suite of tests and quality checks
- Ephemeral environments for feature testing

### Staging/QA

- Deployment after successful development pipeline
- Integration testing with production-like data
- Performance testing
- User acceptance testing

### Production

- Manual approval or automated based on quality gates
- Canary or blue-green deployment strategies
- Automated rollback capabilities
- Post-deployment verification

## Secrets Management

- Never hardcode secrets in pipeline configuration files
- Store secrets in the CI/CD platform's secrets manager
- Rotate secrets regularly
- Use least-privilege principles for service accounts

## Pipeline Performance Optimization

- Use caching for dependencies
- Parallelize independent jobs
- Use appropriate compute resources for specialized workloads
- Skip unnecessary steps when appropriate

## Monitoring and Notifications

- Set up notifications for pipeline failures
- Add status badges to repositories
- Generate and publish pipeline analytics
- Integrate with incident management systems for critical pipelines

## Pipeline as Code Best Practices

- Version control all pipeline definitions
- Review pipeline changes with the same rigor as application code
- Document complex pipeline logic
- Create reusable components for common patterns
- Test pipeline changes in isolation before merging

## Rollback Strategies

- Keep multiple artifact versions accessible
- Implement automated rollback triggers
- Test rollback procedures regularly
- Maintain database migration reversibility

## CI/CD Platform Selection Criteria

When choosing a CI/CD platform, consider the following factors:

1. **Integration**: How well it integrates with your existing tools and workflows
2. **Scalability**: Ability to handle your team's workload
3. **Extensibility**: Support for custom plugins/extensions
4. **Security**: Security features and compliance capabilities
5. **Cost**: Pricing model and resource consumption
6. **Ease of Use**: Learning curve and documentation quality
7. **Community Support**: Size and activity of the user community
8. **Vendor Lock-in**: How difficult it would be to migrate to another platform

## Common CI/CD Patterns

### Pipeline Patterns

- **Basic Pipeline**: Linear sequence of stages (build → test → deploy)
- **Fan-out/Fan-in**: Parallel execution of independent tasks
- **Environment Promotion**: Progressive deployment through environments
- **Feature Branch Workflow**: Separate pipelines for feature branches
- **Trunk-based Development**: Frequent integration to main branch
- **Canary Deployment**: Gradual rollout to subset of users
- **Blue/Green Deployment**: Switch between two identical environments

### Repository Patterns

- **Monorepo**: Single repository containing multiple projects
- **Poly-repo**: Separate repository for each project
- **Hybrid**: Mix of monorepo and poly-repo approaches

## Platform-Specific Guidelines

For detailed, platform-specific implementation guidance, refer to:

- [GitHub Actions Workflows](github-actions-workflows.instructions.md)
- [GitLab CI/CD Workflows](gitlab-ci-workflows.instructions.md)

Additional platform-specific guides can be added as needed for:
- Jenkins
- Azure DevOps
- CircleCI
- Travis CI
- TeamCity
- Bamboo
