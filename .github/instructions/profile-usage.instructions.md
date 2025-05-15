---
applyTo: "**/*profile*.md,**/*profiles*.md"
---
# Profile Usage Guidelines

## Overview

This document outlines when and how to use different profiles across software development, documentation, and project management. Profiles help standardize approaches for different contexts, ensuring consistency and quality while addressing the specific needs of each scenario.

## Development Profiles

### Microservice Development Profile

**When to Use:**
- Building distributed systems with independent services
- When services need independent deployment and scaling
- For large systems that benefit from domain-driven design
- When different services require different technology stacks

**Key Components:**
- Service definition and boundaries
- API contracts and documentation
- Data isolation strategies
- Inter-service communication patterns
- CI/CD pipeline configuration
- Monitoring and observability setup

**Associated Documentation:**
- API Documentation (REST API instructions)
- Architecture Decision Records
- DevOps Configuration

### Monolithic Application Profile

**When to Use:**
- For smaller applications or initial prototypes
- When simplicity is prioritized over scalability
- For applications with tightly coupled domains
- When development speed is prioritized over long-term flexibility

**Key Components:**
- Module organization within the monolith
- Shared library management
- Database access strategies
- UI integration approaches
- Deployment strategy

**Associated Documentation:**
- Software Design Document
- Database Documentation

### Frontend Application Profile

**When to Use:**
- Developing user-facing web applications
- Building single-page applications (SPAs)
- Creating progressive web apps (PWAs)
- Implementing complex UI with rich interactions

**Key Components:**
- Component architecture
- State management approach
- Routing strategy
- API integration patterns
- Responsive design implementation
- Performance optimization strategies

**Associated Documentation:**
- UX Documentation
- UI Component Library
- Accessibility Guidelines

### Real-time System Profile

**When to Use:**
- Developing systems with strict timing requirements
- Applications requiring immediate data processing
- Interactive systems requiring low latency
- High-frequency data streaming applications

**Key Components:**
- Concurrency model
- Message handling architecture
- Buffer management
- Performance optimization techniques
- Failure handling strategies

**Associated Documentation:**
- Performance Testing Documentation
- System Architecture Document

## Documentation Profiles

### Technical Reference Profile

**When to Use:**
- API documentation
- SDK documentation
- Library reference manuals
- Technical specifications

**Key Components:**
- Function/method signatures
- Parameter descriptions
- Return values
- Error codes and handling
- Code examples
- Type definitions

**Associated Documentation:**
- API Documentation Format
- Code Comment Standards

### End-User Documentation Profile

**When to Use:**
- User manuals
- Online help systems
- Tutorial guides
- Customer-facing documentation

**Key Components:**
- Task-based organization
- Step-by-step instructions
- Screenshots and illustrations
- Troubleshooting guides
- Glossary of terms
- Getting started sections

**Associated Documentation:**
- UX Documentation
- Technical Writing Guidelines

### Project Documentation Profile

**When to Use:**
- Project plans
- Status reports
- Project charters
- Sprint documentation
- Release plans

**Key Components:**
- Project overview and goals
- Timeline and milestones
- Resource allocation
- Risk assessment
- Decision log
- Status tracking

**Associated Documentation:**
- Project Management Templates
- Technical Writing Guidelines

### Architecture Documentation Profile

**When to Use:**
- System architecture descriptions
- Integration documentation
- Technical decision records
- Infrastructure design documents

**Key Components:**
- Architecture diagrams (.drawio.png format)
- Component descriptions
- Integration points
- Data flow diagrams
- Deployment models
- Non-functional requirements implementation

**Associated Documentation:**
- Software Design Document
- Architecture Decision Records
- UML Diagram Standards

## Testing Profiles

### Unit Testing Profile

**When to Use:**
- Testing individual components or functions
- Implementing Test-Driven Development (TDD)
- Verifying isolated functionality
- Regression testing of core components

**Key Components:**
- Test fixtures and setup
- Assertion patterns
- Mocking and stubbing approach
- Test naming conventions
- Coverage requirements

**Associated Documentation:**
- Testing Documentation
- Language-Specific Guidelines

### Integration Testing Profile

**When to Use:**
- Testing component interactions
- Validating subsystem communication
- Testing database interactions
- API contract verification

**Key Components:**
- Test environment configuration
- Data setup and teardown
- Service virtualization strategy
- Test sequencing
- Error handling verification

**Associated Documentation:**
- Testing Documentation
- DevOps Documentation

### End-to-End Testing Profile

**When to Use:**
- Testing complete user flows
- Validating business scenarios
- Acceptance testing
- System verification before release

**Key Components:**
- User journey selection
- Test environment requirements
- Test data management
- UI automation approach
- Performance measurement points

**Associated Documentation:**
- Testing Documentation
- UX Documentation

### Performance Testing Profile

**When to Use:**
- Load testing web applications
- Stress testing infrastructure
- Validating response time requirements
- Capacity planning
- Identifying bottlenecks

**Key Components:**
- Load model definition
- Test scenario design
- Metrics collection strategy
- Baseline establishment
- Threshold definitions
- Analysis methodology

**Associated Documentation:**
- Testing Documentation
- DevOps Documentation
- Performance Requirements

## Database Profiles

### Transactional Database Profile

**When to Use:**
- OLTP (Online Transaction Processing) systems
- Systems requiring ACID compliance
- Applications with complex data relationships
- Systems with high data integrity requirements

**Key Components:**
- Normalization strategy
- Transaction boundaries
- Indexing approach
- Constraint definitions
- Query optimization guidelines

**Associated Documentation:**
- Database Documentation
- Performance Guidelines

### Analytical Database Profile

**When to Use:**
- OLAP (Online Analytical Processing) systems
- Data warehousing applications
- Business intelligence systems
- Reporting systems

**Key Components:**
- Dimensional modeling approach
- Star/snowflake schema design
- ETL/ELT patterns
- Materialized view strategy
- Partitioning approach

**Associated Documentation:**
- Database Documentation
- Data Modeling Standards

### NoSQL Database Profile

**When to Use:**
- Applications requiring high scalability
- Systems with flexible schema requirements
- Real-time big data applications
- Document or graph-based data models

**Key Components:**
- Data modeling approach
- Consistency model selection
- Partition strategy
- Query patterns
- Denormalization strategy

**Associated Documentation:**
- Database Documentation
- Scalability Guidelines

## Security Profiles

### Public Web Application Security Profile

**When to Use:**
- Internet-facing web applications
- Consumer-oriented services
- Public APIs
- E-commerce applications

**Key Components:**
- OWASP Top 10 mitigations
- Authentication mechanism
- Authorization framework
- Input validation strategy
- HTTPS implementation
- CSRF protection
- CSP implementation

**Associated Documentation:**
- Security Documentation
- DevOps Configuration
- Compliance Requirements

### Enterprise Application Security Profile

**When to Use:**
- Internal business applications
- HR and finance systems
- Corporate intranets
- Business-critical applications

**Key Components:**
- SSO integration
- Role-based access control
- Audit logging strategy
- Data classification handling
- Internal network security
- Compliance documentation

**Associated Documentation:**
- Security Documentation
- Compliance Documentation
- Enterprise Integration Standards

### Data Privacy Profile

**When to Use:**
- Applications handling PII (Personally Identifiable Information)
- Healthcare applications (HIPAA)
- Financial services (PCI-DSS)
- Systems subject to GDPR, CCPA or similar regulations

**Key Components:**
- Data minimization strategy
- Data retention policies
- Consent management
- Encryption requirements
- Anonymization/pseudonymization approach
- Data subject rights implementation

**Associated Documentation:**
- Privacy Impact Assessment
- Data Protection Documentation
- Compliance Checklist

## DevOps Profiles

### Continuous Integration Profile

**When to Use:**
- Setting up automated build systems
- Implementing code quality gates
- Establishing testing automation
- Creating developer feedback loops

**Key Components:**
- Pipeline definition
- Build automation scripts
- Test automation integration
- Code quality tool configuration
- Artifact repository setup

**Associated Documentation:**
- DevOps Documentation
- CI Pipeline Configuration

### Continuous Deployment Profile

**When to Use:**
- Implementing automated release processes
- Setting up deployment automation
- Establishing production safeguards
- Creating release pipelines

**Key Components:**
- Deployment strategy (blue/green, canary, etc.)
- Environment promotion flow
- Approval gates
- Rollback mechanisms
- Monitoring integration

**Associated Documentation:**
- DevOps Documentation
- Release Management Guidelines

### Infrastructure as Code Profile

**When to Use:**
- Cloud infrastructure provisioning
- Environment configuration management
- Network and security setup
- Infrastructure automation

**Key Components:**
- IaC tool selection (Terraform, CloudFormation, etc.)
- Resource organization strategy
- State management approach
- Secret handling
- Module structure

**Associated Documentation:**
- DevOps Documentation
- Cloud Platform Guidelines
- Security Configuration

## Project Management Profiles

### Agile Development Profile

**When to Use:**
- Teams working in sprints
- Projects with evolving requirements
- Teams practicing Scrum or Kanban
- Customer-collaborative development

**Key Components:**
- Sprint planning structure
- User story format
- Definition of Ready/Done
- Retrospective format
- Velocity tracking approach
- Backlog refinement process

**Associated Documentation:**
- Agile Process Documentation
- User Story Templates

### Waterfall Project Profile

**When to Use:**
- Projects with fixed requirements
- Regulated environments requiring phase approvals
- Contract-based projects with defined deliverables
- Large-scale implementations with multiple dependencies

**Key Components:**
- Phase gate definitions
- Requirements documentation templates
- Sign-off procedures
- Change control process
- Risk register structure
- Project plan format

**Associated Documentation:**
- Project Plan Template
- Requirements Document Template

### Maintenance Project Profile

**When to Use:**
- Ongoing support of existing systems
- Bug fix prioritization
- Technical debt management
- Legacy system maintenance

**Key Components:**
- Issue triage process
- Service level definitions
- Hotfix process
- Knowledge transfer requirements
- Technical debt tracking
- Code archeology guidelines

**Associated Documentation:**
- Support Playbook
- Incident Response Documentation

## Choosing the Right Profile

When selecting a profile for your project, consider:

1. **Project Type**: The nature of what you're building
2. **Team Expertise**: The skills and experience available
3. **Business Context**: Budget, timeline, and organizational constraints
4. **Long-term Goals**: Future maintainability and scalability needs
5. **Regulatory Environment**: Compliance and legal requirements

It's common to combine multiple profiles for different aspects of a project. For example, you might use the Microservice Development Profile with the Continuous Deployment Profile and the Public Web Application Security Profile.

## Profile Implementation

### Profile Adoption Process
1. **Assessment**: Evaluate project needs against available profiles
2. **Selection**: Choose appropriate profiles for different aspects
3. **Tailoring**: Adjust profile requirements to project specifics
4. **Documentation**: Document profile choices in project charter
5. **Training**: Ensure team understands profile requirements
6. **Implementation**: Apply profile guidelines in development
7. **Review**: Periodically review profile effectiveness

### Profile Customization Guidelines
- Start with standard profiles before customizing
- Document all deviations from standard profiles
- Justify customizations based on project requirements
- Seek approval for major profile modifications
- Consider contributing successful customizations back to standard profiles

## Resources

- Project-specific profile templates
- Example implementations of each profile
- Profile selection decision tree
- Profile compliance checklists
- Training materials for each profile
