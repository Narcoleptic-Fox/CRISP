---
applyTo: "**/.github/workflows/*.yml,**/github-actions.md"
---

# GitHub Actions Workflow Guidelines

## Overview

This document outlines standards and best practices for implementing Continuous Integration (CI) and Continuous Deployment (CD) workflows using GitHub Actions. Following these guidelines ensures reliable, secure, and efficient deployment pipelines on the GitHub platform.

## GitHub Actions Basics

- **Workflow Files**: Stored in `.github/workflows/*.yml`
- **Events**: Triggers such as push, pull_request, schedule
- **Jobs**: Collections of steps that run on the same runner
- **Steps**: Individual tasks that run commands or actions
- **Actions**: Reusable units of code that can be shared

## Standard Workflow Components

### 1. Continuous Integration

#### Code Quality Checks

```yaml
# Example GitHub Actions workflow for code quality
name: Code Quality

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Run linter
        run: npm run lint
      
  format:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Check formatting
        run: npm run format:check
```

#### Testing

```yaml
# Example GitHub Actions workflow for testing
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Run unit tests
        run: npm run test:unit
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          token: ${{ secrets.CODECOV_TOKEN }}
  
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Run integration tests
        run: npm run test:integration
```

#### Security Scanning

```yaml
# Example GitHub Actions workflow for security scanning
name: Security

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 0 * * 0'  # Weekly

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Run dependency audit
        run: npm audit
  
  code-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run CodeQL Analysis
        uses: github/codeql-action/analyze@v2
        with:
          languages: javascript
```

### 2. Continuous Deployment

#### Build and Artifact Creation

```yaml
# Example GitHub Actions workflow for building and creating artifacts
name: Build

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Upload build artifact
        uses: actions/upload-artifact@v3
        with:
          name: build-output
          path: build/
```

#### Deployment

```yaml
# Example GitHub Actions workflow for deployment
name: Deploy

on:
  push:
    branches: [ main ]

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Deploy to staging
        uses: some-deployment-action@v1
        with:
          api-key: ${{ secrets.DEPLOY_API_KEY }}
          environment: staging
  
  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Deploy to production
        uses: some-deployment-action@v1
        with:
          api-key: ${{ secrets.DEPLOY_API_KEY }}
          environment: production
```

## GitHub-Specific Features

### Environments and Secrets

- Create environments (Settings → Environments) for different deployment targets
- Configure environment-specific secrets and variables
- Set up required reviewers for production deployments
- Define environment-specific deployment rules

```yaml
jobs:
  deploy:
    environment:
      name: production
      url: https://example.com
    # Job configuration
```

### Branch Protection Rules

- Enable branch protection for main/production branches
- Require status checks to pass before merging
- Require pull request reviews
- Restrict who can push to matching branches

### GitHub Actions Optimizations

#### Caching Dependencies

```yaml
steps:
  - uses: actions/checkout@v3
  - name: Set up environment
    uses: actions/setup-node@v3
    with:
      node-version: '16'
  - name: Cache dependencies
    uses: actions/cache@v3
    with:
      path: ~/.npm
      key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
      restore-keys: |
        ${{ runner.os }}-node-
  - name: Install dependencies
    run: npm ci
```

#### Matrix Builds

```yaml
jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        node-version: [14, 16, 18]
    steps:
      - uses: actions/checkout@v3
      - name: Use Node.js ${{ matrix.node-version }}
        uses: actions/setup-node@v3
        with:
          node-version: ${{ matrix.node-version }}
      - name: Install dependencies
        run: npm ci
      - name: Run tests
        run: npm test
```

#### Reusable Workflows

```yaml
# .github/workflows/reusable-build.yml
name: Reusable Build Workflow
on:
  workflow_call:
    inputs:
      node-version:
        required: true
        type: string
    secrets:
      npm-token:
        required: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: ${{ inputs.node-version }}
          registry-url: 'https://registry.npmjs.org'
      - name: Install dependencies
        run: npm ci
        env:
          NODE_AUTH_TOKEN: ${{ secrets.npm-token }}
      - name: Build
        run: npm run build
```

Usage in another workflow:
```yaml
jobs:
  call-build:
    uses: ./.github/workflows/reusable-build.yml
    with:
      node-version: '16'
    secrets:
      npm-token: ${{ secrets.NPM_TOKEN }}
```

### GitHub Actions Limitations and Solutions

#### Timeouts and Job Duration
- Default timeout: 6 hours
- Strategies for long-running jobs:
  - Break into smaller jobs
  - Use self-hosted runners
  - Implement appropriate timeouts

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    timeout-minutes: 30  # Custom timeout
```

#### Storage and Artifacts
- Free plan: 500 MB storage, 1 GB for artifacts
- Strategies to manage storage:
  - Limit artifact retention
  - Compress artifacts before upload
  - Clean up old artifacts

```yaml
- name: Upload artifact
  uses: actions/upload-artifact@v3
  with:
    name: build
    path: build/
    retention-days: 5  # Custom retention period
```

## Security Best Practices

- Limit permissions for GitHub tokens
- Pin action versions with SHA references
- Avoid printing secrets to logs
- Validate inputs for scripts and commands
- Use OpenID Connect for cloud provider authentication

```yaml
jobs:
  deploy:
    # Limit token permissions
    permissions:
      contents: read
      deployments: write
    runs-on: ubuntu-latest
    steps:
      # Pin action versions with SHA
      - uses: actions/checkout@8e5e7e5ab8b370d6c329ec480221332ada57f0ab # v3.5.2
```

## GitHub Actions Monitoring

- Use status badges on README
- Set up notifications for workflow failures
- Monitor workflow usage and costs
- Configure billing alerts
- Review workflow analytics

## GitHub Actions CI/CD Patterns

### Monorepo Pattern

```yaml
name: Monorepo CI

on:
  push:
    paths:
      - 'packages/frontend/**'
      - 'packages/backend/**'
      - 'packages/common/**'

jobs:
  detect-changes:
    runs-on: ubuntu-latest
    outputs:
      frontend: ${{ steps.filter.outputs.frontend }}
      backend: ${{ steps.filter.outputs.backend }}
    steps:
      - uses: actions/checkout@v3
      - uses: dorny/paths-filter@v2
        id: filter
        with:
          filters: |
            frontend:
              - 'packages/frontend/**'
              - 'packages/common/**'
            backend:
              - 'packages/backend/**'
              - 'packages/common/**'

  frontend:
    needs: detect-changes
    if: ${{ needs.detect-changes.outputs.frontend == 'true' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: cd packages/frontend && npm ci
      - name: Run tests
        run: cd packages/frontend && npm test
```

### Feature Branch Pattern

```yaml
name: Feature Branch CI

on:
  push:
    branches:
      - 'feature/**'
      - 'bugfix/**'
      
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Run tests
        run: npm test
      - name: Create preview environment
        id: preview
        run: |
          BRANCH_NAME=${GITHUB_REF##*/}
          echo "Creating preview for branch $BRANCH_NAME"
          PREVIEW_URL="https://preview-${BRANCH_NAME}.example.com"
          echo "url=$PREVIEW_URL" >> $GITHUB_OUTPUT
      - name: Comment PR with preview URL
        uses: actions/github-script@v6
        with:
          script: |
            const previewUrl = '${{ steps.preview.outputs.url }}';
            const issueNumber = context.issue.number;
            github.rest.issues.createComment({
              issue_number: issueNumber,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: `Preview environment deployed to: ${previewUrl}`
            });
```

## Migration from Other CI/CD Systems

### From Jenkins to GitHub Actions

| Jenkins | GitHub Actions |
|---------|----------------|
| Jenkinsfile | workflow YAML files |
| stages | jobs |
| steps | steps |
| agents | runners |
| shared libraries | reusable workflows and composite actions |
| credentials | secrets |

### From CircleCI to GitHub Actions

| CircleCI | GitHub Actions |
|---------|----------------|
| orbs | actions |
| workflows | workflow files |
| jobs | jobs |
| executors | runners |
| commands | composite actions |
| contexts | environments and secrets |

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Actions Marketplace](https://github.com/marketplace?type=actions)
- [GitHub Actions Community Forum](https://github.community/c/actions/41)
- [GitHub Actions Starter Workflows](https://github.com/actions/starter-workflows)
