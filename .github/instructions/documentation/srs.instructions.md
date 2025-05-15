---
applyTo: "**/*SRS*.md,**/*Requirements*.md,**/*Specification*.md"
---
# Software Requirements Specification (SRS) Guidelines

## Document Structure

### 1. Introduction
- **1.1 Purpose**: Define the purpose of the SRS document.
- **1.2 Scope**: Define the scope of the software being specified.
- **1.3 Definitions, Acronyms, and Abbreviations**: Define specialized terms.
- **1.4 References**: List all documents referenced in the SRS.
- **1.5 Overview**: Provide an overview of the rest of the document.

### 2. Overall Description
- **2.1 Product Perspective**: Describe the context of the product.
- **2.2 Product Functions**: Summarize the major functions of the product.
- **2.3 User Characteristics**: Describe the users of the system.
- **2.4 Constraints**: Document design constraints (hardware limits, regulatory policies, etc.).
- **2.5 Assumptions and Dependencies**: List factors that affect requirements.

### 3. Specific Requirements
- **3.1 External Interface Requirements**:
  - **3.1.1 User Interfaces**: Describe all user interfaces.
  - **3.1.2 Hardware Interfaces**: Describe connections to hardware.
  - **3.1.3 Software Interfaces**: Describe connections to other software.
  - **3.1.4 Communications Interfaces**: Describe network requirements.
- **3.2 Functional Requirements**: Document all functional requirements, organized by feature.
  - **3.2.1 Feature 1**:
    - REQ-1.1: Specific requirement
    - REQ-1.2: Specific requirement
  - **3.2.2 Feature 2**:
    - REQ-2.1: Specific requirement
    - REQ-2.2: Specific requirement
- **3.3 Non-Functional Requirements**:
  - **3.3.1 Performance**: Document performance requirements.
  - **3.3.2 Security**: Document security requirements.
  - **3.3.3 Reliability**: Document reliability requirements.
  - **3.3.4 Availability**: Document availability requirements.
  - **3.3.5 Maintainability**: Document maintainability requirements.
  - **3.3.6 Portability**: Document portability requirements.
  - **3.3.7 Usability**: Document usability requirements.
  - **3.3.8 Compliance**: Document regulatory compliance requirements.
- **3.4 Data Requirements**:
  - **3.4.1 Data Entities**: Document major data entities.
  - **3.4.2 Data Retention**: Document data retention requirements.
  - **3.4.3 Data Migration**: Document data migration requirements.

### 4. Analysis Models
- **4.1 Use Case Model**: Include use cases or user stories.
- **4.2 Data Flow Diagrams**: Include data flow diagrams if relevant.
- **4.3 State Diagrams**: Include state diagrams for complex state-based behavior.

### 5. Validation
- **5.1 Acceptance Criteria**: Define the criteria for acceptance testing.
- **5.2 Traceability Matrix**: Map requirements to validation methods.

### Appendices
- **A. Glossary**: Define all terms and acronyms used.
- **B. Open Issues**: Document any unresolved issues.
- **C. Prototypes and Mockups**: Include UI mockups, prototypes, or wireframes.

## IEEE 830 Compliance

To ensure compliance with IEEE 830 (Recommended Practice for Software Requirements Specifications), ensure your SRS is:

- **Correct**: Each requirement represents what the system should do.
- **Unambiguous**: Each requirement has only one interpretation.
- **Complete**: All relevant requirements are included.
- **Consistent**: Requirements don't contradict each other.
- **Ranked for Importance/Stability**: Requirements have priorities assigned.
- **Verifiable**: Each requirement can be verified by testing.
- **Modifiable**: The structure allows changes to be made easily.
- **Traceable**: The origin and evolution of each requirement can be traced.

## Requirement Writing Principles

### SMART Requirements
- **Specific**: Clearly defined with no room for interpretation.
- **Measurable**: Can be verified through testing.
- **Achievable**: Realistic within the constraints of the project.
- **Relevant**: Directly related to the goals of the system.
- **Time-bound**: Can be implemented within the project timeline.

### Atomic Requirements
- Each requirement should describe one thing.
- Avoid compound requirements with "and" or "or".

### Requirement Templates
- **User Story Format**: As a [role], I want [feature] so that [benefit].
- **Shall Statement**: The system shall [action] [condition] [standard].

### Requirements Quality Checklist
- Is the requirement necessary?
- Is the requirement implementation-free (focused on what, not how)?
- Is the requirement clear and unambiguous?
- Is the requirement complete on its own?
- Is the requirement consistent with other requirements?
- Is the requirement traceable to business needs?
- Is the requirement testable?

## Common Pitfalls to Avoid

- Mixing requirements with design solutions
- Using ambiguous language ("user-friendly", "fast", "efficient")
- Writing requirements too generally
- Including contradictory requirements
- Creating untestable requirements
- Omitting key stakeholder requirements
- Over-specifying requirements beyond what's needed

## Requirement Management Best Practices

- Use a consistent numbering scheme for requirements
- Establish a change control process for requirements
- Trace requirements to their source (business need, stakeholder, etc.)
- Prioritize requirements (e.g., MoSCoW: Must have, Should have, Could have, Won't have)
- Version control your requirements document
- Record requirement rationales
- Review requirements with stakeholders

## Tools and Techniques

- Requirements workshops or JAD sessions
- User interviews and surveys
- Observation of existing systems
- Prototyping and mockups
- Use case modeling
- Business process modeling
- Requirements tracking tools (JIRA, Azure DevOps, etc.)

## References

- IEEE 830-1998 Recommended Practice for Software Requirements Specifications
- ISO/IEC/IEEE 29148:2018 Systems and software engineering — Life cycle processes — Requirements engineering
- Requirements Engineering: Processes and Techniques (Kotonya and Sommerville)
- Writing Effective Use Cases (Cockburn)
- User Stories Applied (Cohn)
