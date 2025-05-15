---
applyTo: "**/.gitlab-ci.yml,**/gitlab-ci*.md"
---

# GitLab CI/CD Workflow Guidelines

## Overview

This document outlines standards and best practices for implementing Continuous Integration (CI) and Continuous Deployment (CD) workflows using GitLab CI/CD. Following these guidelines ensures reliable, secure, and efficient deployment pipelines on the GitLab platform.

## GitLab CI/CD Basics

- **Configuration File**: Stored in `.gitlab-ci.yml` at the root of the repository
- **Pipelines**: Collection of jobs organized in stages
- **Stages**: Organize jobs into groups that run sequentially
- **Jobs**: Individual tasks that run scripts
- **Runners**: Servers that execute the jobs

## Standard Workflow Components

### 1. Basic Pipeline Structure

```yaml
# Example GitLab CI/CD configuration
stages:
  - lint
  - test
  - build
  - deploy

variables:
  NODE_VERSION: "16"

lint:
  stage: lint
  image: node:${NODE_VERSION}
  script:
    - npm ci
    - npm run lint

test:
  stage: test
  image: node:${NODE_VERSION}
  script:
    - npm ci
    - npm run test
  artifacts:
    reports:
      coverage: coverage/coverage-final.json
    paths:
      - coverage/

build:
  stage: build
  image: node:${NODE_VERSION}
  script:
    - npm ci
    - npm run build
  artifacts:
    paths:
      - dist/
    expire_in: 1 week
  only:
    - main
    - develop

deploy-staging:
  stage: deploy
  environment:
    name: staging
    url: https://staging.example.com
  script:
    - echo "Deploying to staging environment"
    - npm install -g some-deployment-tool
    - some-deployment-tool deploy --env staging
  only:
    - develop

deploy-production:
  stage: deploy
  environment:
    name: production
    url: https://example.com
  script:
    - echo "Deploying to production environment"
    - npm install -g some-deployment-tool
    - some-deployment-tool deploy --env production
  only:
    - main
  when: manual
```

### 2. Code Quality Checks

```yaml
lint:
  stage: lint
  image: node:16
  script:
    - npm ci
    - npm run lint
    - npm run format:check

code_quality:
  stage: lint
  image: docker:stable
  services:
    - docker:dind
  variables:
    DOCKER_DRIVER: overlay2
  script:
    - docker pull codeclimate/codeclimate
    - docker run --env CODECLIMATE_CODE="$PWD" --volume "$PWD":/code --volume /var/run/docker.sock:/var/run/docker.sock --volume /tmp/cc:/tmp/cc codeclimate/codeclimate analyze -f json > gl-code-quality-report.json
  artifacts:
    reports:
      codequality: gl-code-quality-report.json
    paths:
      - gl-code-quality-report.json
```

### 3. Testing

```yaml
unit_test:
  stage: test
  image: node:16
  script:
    - npm ci
    - npm run test:unit
  artifacts:
    reports:
      junit: junit-unit.xml
    paths:
      - coverage/

integration_test:
  stage: test
  image: node:16
  services:
    - postgres:13
  variables:
    POSTGRES_DB: test_db
    POSTGRES_USER: test_user
    POSTGRES_PASSWORD: test_password
    DATABASE_URL: postgres://test_user:test_password@postgres:5432/test_db
  script:
    - npm ci
    - npm run test:integration
  artifacts:
    reports:
      junit: junit-integration.xml
```

### 4. Security Scanning

```yaml
dependency_scanning:
  stage: test
  image: node:16
  script:
    - npm ci
    - npm audit --json > npm-audit.json
    - npx @gitlab/gl-dependency-scanning-report -o gl-dependency-scanning-report.json npm-audit.json
  artifacts:
    reports:
      dependency_scanning: gl-dependency-scanning-report.json
    paths:
      - gl-dependency-scanning-report.json

sast:
  stage: test
  image: docker:stable
  variables:
    DOCKER_DRIVER: overlay2
  services:
    - docker:dind
  script:
    - docker pull registry.gitlab.com/gitlab-org/security-products/sast:latest
    - docker run --env SAST_ANALYZER_IMAGE=registry.gitlab.com/gitlab-org/security-products/analyzers/nodejs:latest --volume "$PWD:/code" --volume /var/run/docker.sock:/var/run/docker.sock registry.gitlab.com/gitlab-org/security-products/sast /app/bin/run /code
  artifacts:
    reports:
      sast: gl-sast-report.json
    paths:
      - gl-sast-report.json

secret_detection:
  stage: test
  image: docker:stable
  services:
    - docker:dind
  variables:
    DOCKER_DRIVER: overlay2
  script:
    - docker pull registry.gitlab.com/gitlab-org/security-products/secrets:latest
    - docker run --env SAST_ANALYZER_IMAGE=registry.gitlab.com/gitlab-org/security-products/analyzers/secrets:latest --volume "$PWD:/code" --volume /var/run/docker.sock:/var/run/docker.sock registry.gitlab.com/gitlab-org/security-products/secrets /app/bin/run /code
  artifacts:
    reports:
      secret_detection: gl-secret-detection-report.json
    paths:
      - gl-secret-detection-report.json
```

### 5. Deployment

```yaml
build:
  stage: build
  image: node:16
  script:
    - npm ci
    - npm run build
  artifacts:
    paths:
      - build/
    expire_in: 1 week

deploy_staging:
  stage: deploy
  image: alpine:latest
  environment:
    name: staging
    url: https://staging.example.com
  before_script:
    - apk add --no-cache curl
    - curl -sL https://gitlab.example.com/api/v4/projects/123/packages/generic/deployment-tool/1.0.0/deploy-tool -o /usr/local/bin/deploy-tool
    - chmod +x /usr/local/bin/deploy-tool
  script:
    - deploy-tool --env staging --path build/
  only:
    - develop

deploy_production:
  stage: deploy
  image: alpine:latest
  environment:
    name: production
    url: https://example.com
  before_script:
    - apk add --no-cache curl
    - curl -sL https://gitlab.example.com/api/v4/projects/123/packages/generic/deployment-tool/1.0.0/deploy-tool -o /usr/local/bin/deploy-tool
    - chmod +x /usr/local/bin/deploy-tool
  script:
    - deploy-tool --env production --path build/
  only:
    - main
  when: manual
```

## GitLab-Specific Features

### GitLab CI/CD Variables

```yaml
variables:
  # Global variables
  NODE_VERSION: "16"
  NPM_TOKEN: ${CI_JOB_TOKEN}

job1:
  variables:
    # Job-specific variables
    FOO: "bar"
  script:
    - echo $FOO
    - echo $NODE_VERSION
```

### GitLab Environment Configuration

```yaml
deploy_staging:
  environment:
    name: staging
    url: https://staging.example.com
    on_stop: stop_staging
    auto_stop_in: 1 day

stop_staging:
  script:
    - echo "Stopping staging environment"
  environment:
    name: staging
    action: stop
  when: manual
```

### GitLab Caching and Artifacts

```yaml
build:
  stage: build
  image: node:16
  cache:
    key:
      files:
        - package-lock.json
    paths:
      - node_modules/
    policy: pull-push
  script:
    - npm ci
    - npm run build
  artifacts:
    paths:
      - build/
    expire_in: 1 week
```

### GitLab Review Apps

```yaml
review:
  stage: deploy
  script:
    - echo "Deploy review app"
  environment:
    name: review/$CI_COMMIT_REF_SLUG
    url: https://$CI_COMMIT_REF_SLUG.example.com
    on_stop: stop_review
  only:
    - merge_requests

stop_review:
  stage: deploy
  script:
    - echo "Remove review app"
  environment:
    name: review/$CI_COMMIT_REF_SLUG
    action: stop
  when: manual
  only:
    - merge_requests
```

## GitLab CI/CD Optimizations

### Parallel Execution

```yaml
tests:
  stage: test
  parallel: 3
  script:
    - npm ci
    - npx jest --shard=$CI_NODE_INDEX/$CI_NODE_TOTAL
```

### Matrix Jobs

```yaml
test:
  stage: test
  parallel:
    matrix:
      - NODE_VERSION: ["14", "16", "18"]
        DATABASE: ["mysql", "postgres"]
  image: node:${NODE_VERSION}
  services:
    - ${DATABASE}:latest
  script:
    - echo "Testing with Node.js ${NODE_VERSION} and ${DATABASE}"
    - npm ci
    - npm test
```

### Workflow Rules

```yaml
workflow:
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
    - if: $CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH
    - if: $CI_COMMIT_TAG
```

### Needs and Dependencies

```yaml
stages:
  - build
  - test
  - deploy

build:
  stage: build
  script:
    - npm ci
    - npm run build

test1:
  stage: test
  needs: [build]
  script:
    - npm test:unit

test2:
  stage: test
  needs: [build]
  script:
    - npm test:integration

deploy:
  stage: deploy
  needs: [test1, test2]
  script:
    - deploy-app
```

## GitLab CI/CD Security Best Practices

### Secrets Management

- Use GitLab CI/CD Variables for sensitive information
- Mask variables containing sensitive values
- Use protected variables for protected branches
- Consider using HashiCorp Vault integration for sensitive secrets

```yaml
variables:
  API_TOKEN:
    value: "secret-token"
    masked: true
    protected: true
```

### Docker Image Security

- Use specific image versions instead of `latest`
- Use official images when possible
- Scan container images for vulnerabilities

```yaml
container_scanning:
  stage: test
  image: docker:stable
  services:
    - docker:dind
  variables:
    DOCKER_DRIVER: overlay2
    CI_APPLICATION_REPOSITORY: $CI_REGISTRY_IMAGE/my-image
    CI_APPLICATION_TAG: $CI_COMMIT_REF_SLUG
  script:
    - docker pull $CI_APPLICATION_REPOSITORY:$CI_APPLICATION_TAG
    - docker run -d --name db arminc/clair-db:latest
    - docker run -p 6060:6060 --link db:postgres arminc/clair-local-scan:latest
    - clair-scanner --ip $(hostname -i) --reportPath gl-container-scanning-report.json $CI_APPLICATION_REPOSITORY:$CI_APPLICATION_TAG
  artifacts:
    reports:
      container_scanning: gl-container-scanning-report.json
```

## GitLab Runner Configuration

### Self-hosted Runners

```yaml
# Example .gitlab-runner/config.toml
[[runners]]
  name = "custom-runner"
  url = "https://gitlab.example.com/"
  token = "PROJECT_TOKEN"
  executor = "docker"
  [runners.docker]
    tls_verify = false
    image = "ruby:2.7"
    privileged = false
    disable_entrypoint_overwrite = false
    oom_kill_disable = false
    disable_cache = false
    volumes = ["/cache"]
    shm_size = 0
```

### Tags and Specific Runners

```yaml
job:
  tags:
    - docker
    - high-memory
  script:
    - npm ci
    - npm run build
```

## GitLab CI/CD Monitoring

- Use GitLab CI/CD Analytics for pipeline performance
- Set up alerts for pipeline failures
- Monitor runner metrics
- Review job logs for performance issues

## GitLab CI/CD Patterns

### Feature Branch Pattern

```yaml
workflow:
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
    - if: $CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH
    - if: $CI_COMMIT_TAG

stages:
  - test
  - review
  - staging
  - production

test:
  stage: test
  script:
    - npm ci
    - npm test

review:
  stage: review
  script:
    - echo "Deploy to review app"
  environment:
    name: review/$CI_COMMIT_REF_SLUG
    url: https://$CI_ENVIRONMENT_SLUG.example.com
    on_stop: stop_review
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"

stop_review:
  stage: review
  script:
    - echo "Stop review app"
  environment:
    name: review/$CI_COMMIT_REF_SLUG
    action: stop
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
      when: manual
```

### Monorepo Pattern

```yaml
workflow:
  rules:
    - if: $CI_PIPELINE_SOURCE == "merge_request_event"
    - if: $CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH

stages:
  - test
  - build
  - deploy

frontend-test:
  stage: test
  script:
    - cd frontend
    - npm ci
    - npm test
  rules:
    - changes:
        - frontend/**/*

backend-test:
  stage: test
  script:
    - cd backend
    - npm ci
    - npm test
  rules:
    - changes:
        - backend/**/*

frontend-build:
  stage: build
  script:
    - cd frontend
    - npm ci
    - npm run build
  rules:
    - changes:
        - frontend/**/*

backend-build:
  stage: build
  script:
    - cd backend
    - npm ci
    - npm run build
  rules:
    - changes:
        - backend/**/*
```

## Migration from Other CI/CD Systems

### From Jenkins to GitLab CI

| Jenkins | GitLab CI/CD |
|---------|----------------|
| Jenkinsfile | .gitlab-ci.yml |
| stages | stages |
| steps | script |
| agents | runners |
| shared libraries | included templates |
| credentials | CI/CD Variables |

### From GitHub Actions to GitLab CI

| GitHub Actions | GitLab CI/CD |
|----------------|----------------|
| workflow files | .gitlab-ci.yml |
| jobs | jobs |
| steps | script |
| runners | runners |
| secrets | CI/CD Variables |
| environments | environments |

## Resources

- [GitLab CI/CD Documentation](https://docs.gitlab.com/ee/ci/)
- [GitLab CI/CD Examples](https://docs.gitlab.com/ee/ci/examples/)
- [GitLab CI/CD Templates](https://gitlab.com/gitlab-org/gitlab/-/tree/master/lib/gitlab/ci/templates)
- [GitLab CI/CD Best Practices](https://docs.gitlab.com/ee/ci/yaml/workflow.html)
