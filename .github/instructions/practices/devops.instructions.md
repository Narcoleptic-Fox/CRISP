---
applyTo: "**/*devops*.md,**/*pipeline*.yml,**/*deployment*.md,**/*infra*.md"
---
# DevOps Documentation Guidelines

## Infrastructure Documentation

### Infrastructure Overview
- **Architecture Diagram**: Include a high-level infrastructure diagram using Draw.io (.drawio.png format).
- **Environment List**: Document all environments (development, testing, staging, production).
- **Resource Inventory**: List major components (compute, storage, networking, services).
- **Network Topology**: Document network segmentation, security groups, and traffic flow.
- **Scalability Design**: Document auto-scaling configurations and limits.
- **Disaster Recovery Plan**: Document backup strategies, recovery procedures, and RTO/RPO targets.

### Infrastructure as Code
- **Repository Structure**: Document organization of IaC files.
- **Tool Documentation**: Specify versions and configurations of tools used (Terraform, CloudFormation, etc.).
- **Module Documentation**: For each module include:
  - Purpose and resources created
  - Input variables with descriptions
  - Output variables with descriptions
  - Dependencies on other modules
  - Usage examples
- **State Management**: Document how state is stored and managed.
- **Secret Management**: Document how secrets are handled.

### Configuration Management
- **Tool Documentation**: Document configuration management tools (Ansible, Puppet, Chef).
- **Configuration Hierarchy**: Document configuration layers and precedence.
- **Role/Playbook Documentation**: For each role/playbook include:
  - Purpose
  - Variables and defaults
  - Dependencies
  - Platforms supported
  - Tags and when to use them

## CI/CD Pipeline Documentation

### Pipeline Overview
- **Pipeline Diagram**: Visual representation of pipeline stages (.drawio.png format).
- **Stage Descriptions**: Document purpose and actions in each stage.
- **Trigger Conditions**: Document what triggers pipeline execution.
- **Branching Strategy**: Document branch management and relationship to environments.
- **Approval Processes**: Document required approvals for deployments.

### Pipeline Configuration
- **Tool Documentation**: Specify CI/CD tools and versions (Jenkins, GitLab CI, GitHub Actions, etc.).
- **Pipeline as Code**: Document structure and location of pipeline definition files.
- **Environment Variables**: Document required environment variables.
- **Secrets Management**: Document how pipeline secrets are handled.
- **Artifact Management**: Document how build artifacts are stored and versioned.

### Testing in the Pipeline
- **Test Types**: Document automated tests in the pipeline (unit, integration, functional, security).
- **Test Environments**: Document environment setup for different test stages.
- **Quality Gates**: Document quality thresholds that must be passed.
- **Test Reports**: Document where test results are stored and how they are analyzed.

## Deployment Documentation

### Deployment Procedures
- **Deployment Strategy**: Document deployment approach (blue/green, canary, rolling).
- **Deployment Steps**: Detailed step-by-step deployment process.
- **Rollback Procedures**: Steps to roll back failed deployments.
- **Verification Steps**: Post-deployment verification checklist.
- **Downtime Expectations**: Document expected downtime, if any.

### Release Management
- **Version Naming**: Document versioning scheme.
- **Release Notes Template**: Standard format for release notes.
- **Change Log Management**: How changes are tracked between releases.
- **Release Approval Process**: Required stakeholder approvals.

## Monitoring and Observability

### Monitoring Setup
- **Monitoring Tools**: Document tools used for monitoring.
- **Metrics Collection**: Document what metrics are collected and their sources.
- **Dashboard Locations**: Links to monitoring dashboards.
- **Alert Configuration**: Document alerting rules, thresholds, and notification channels.
- **Log Management**: Document log collection, storage, and retention policies.

### Incident Response
- **Alert Severity Levels**: Define severity levels and response times.
- **On-Call Rotation**: Document on-call schedules and responsibilities.
- **Incident Response Playbooks**: Step-by-step guides for common incidents.
- **Post-Mortem Template**: Standard format for post-incident reviews.
- **Escalation Paths**: Who to contact when issues can't be resolved.

## Security Documentation

### Security Controls
- **Identity and Access Management**: Document access control policies.
- **Network Security**: Document firewall rules, security groups, and network ACLs.
- **Data Protection**: Document encryption at rest and in transit.
- **Key Management**: Document key rotation policies and procedures.
- **Compliance Requirements**: Document relevant compliance standards.

### Security Procedures
- **Security Scanning**: Document automated security scans in the pipeline.
- **Vulnerability Management**: Process for addressing discovered vulnerabilities.
- **Penetration Testing**: Schedule and scope of penetration tests.
- **Security Incident Response**: Procedures for security breaches.

## Service Documentation

### Service Catalog
- **Service Overview**: High-level description of each service.
- **Service Dependencies**: Document upstream and downstream dependencies.
- **Service Level Objectives (SLOs)**: Performance and availability targets.
- **Service Level Agreements (SLAs)**: Formal agreements with users.
- **Service Owner**: Primary contact for the service.

### API Documentation
- **API Specifications**: Document API endpoints using OpenAPI/Swagger.
- **Authentication Methods**: Document API authentication requirements.
- **Rate Limits**: Document API rate limits and quotas.
- **Error Handling**: Document API error codes and resolution steps.
- **Sample Requests**: Include example API calls.

### Runbooks
- **Operational Tasks**: Procedures for routine operational tasks.
- **Troubleshooting Guides**: Step-by-step problem resolution steps.
- **Maintenance Procedures**: Guidelines for planned maintenance.
- **Backup and Restore**: Procedures for backup and recovery.

## Best Practices for DevOps Documentation

### Documentation Structure
- Use a consistent template structure for similar documentation types.
- Group related documentation together.
- Separate conceptual (why) from procedural (how) content.
- Maintain a table of contents for larger documentation sets.
- Include a revision history.

### Documentation as Code
- Store documentation in version control alongside code.
- Use plain text formats (Markdown, AsciiDoc) for documentation.
- Review documentation changes with code reviews.
- Automate documentation testing and generation where possible.
- Consider using static site generators for documentation portals.

### Automation and Generation
- Automate generation of documentation from code where possible.
- Use tools to extract documentation from comments and annotations.
- Generate diagrams from infrastructure code when feasible.
- Validate links and references automatically.

### Documentation Maintenance
- Assign clear ownership for documentation.
- Schedule regular documentation reviews.
- Update documentation as part of the definition of done for changes.
- Archive or clearly mark outdated documentation.
- Track documentation health metrics.

## Tools and Resources

### Documentation Tools
- **Markdown Editors**: VS Code, Typora, Mark Text
- **Diagramming**: Draw.io/diagrams.net (save as .drawio.png)
- **Static Site Generators**: MkDocs, Hugo, Jekyll
- **Wiki Platforms**: Confluence, GitLab/GitHub Wiki

### DevOps Documentation Tools
- **Infrastructure Diagrams**: Cloudcraft, Terraform Visualizer
- **API Documentation**: Swagger UI, ReDoc, Postman
- **Runbook Automation**: Rundeck, Ansible Tower
- **Collaboration**: Confluence, Google Docs

### Style Guides
- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [Microsoft Writing Style Guide](https://docs.microsoft.com/en-us/style-guide/welcome/)
- [AWS Documentation Guide](https://docs.aws.amazon.com/awsstylehistory/latest/styleguide/welcome.html)

## Templates

### Infrastructure Module Template
```markdown
# Module Name

## Purpose
Brief description of what this module does.

## Resources Created
- Resource 1: Description
- Resource 2: Description

## Input Variables
| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| var_name | Description of variable | string | `default` | yes/no |

## Output Variables
| Name | Description |
|------|-------------|
| output_name | Description of output |

## Dependencies
- Other module dependencies

## Usage Example
```terraform
module "example" {
  source = "./modules/example"
  var_name = "value"
}
```

### Runbook Template
```markdown
# Runbook: [Operation Name]

## Purpose
Brief description of what this runbook is for.

## Prerequisites
- Required permissions
- Required tools
- Required knowledge

## Procedure
1. Step 1
2. Step 2
   - Sub-step
   - Sub-step
3. Step 3

## Verification
- How to verify the operation was successful

## Rollback
- Steps to roll back if necessary

## Notes
- Additional information
- Common issues and solutions
```

## Resources

- [Site Reliability Engineering (Google)](https://sre.google/books/)
- [The Phoenix Project](https://itrevolution.com/book/the-phoenix-project/)
- [Infrastructure as Code (Kief Morris)](https://infrastructure-as-code.com/book/)
- [Accelerate: The Science of Lean Software and DevOps](https://itrevolution.com/book/accelerate/)
- [DevOps Handbook](https://itrevolution.com/book/the-devops-handbook/)
