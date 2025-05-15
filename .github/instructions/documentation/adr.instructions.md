---
applyTo: "**/*ADR*.md,**/*ArchitecturalDecision*.md,**/decisions/*"
---
# Architecture Decision Records (ADR) Guidelines

## What is an ADR?

An Architecture Decision Record (ADR) is a document that captures an important architectural decision made along with its context and consequences. ADRs are immutable records that serve as historical documentation of significant decisions affecting system architecture.

## Purpose of ADRs

- Document significant architectural decisions and their rationale
- Provide context for future team members
- Enable better decision-making by making the decision process explicit
- Create a historical record of architectural evolution
- Support knowledge sharing across the team

## Standard ADR Format

### Title
- Concise noun phrase indicating the decision
- Example: "ADR 001: Use PostgreSQL as Primary Database"

### Status
- One of: Proposed, Accepted, Rejected, Deprecated, Superseded
- If superseded, include a link to the new ADR

### Context
- Describe the forces at play, including technological, business, and team constraints
- Explain the problem being addressed
- Include any principles or requirements that influenced the decision

### Decision
- State the decision clearly
- Describe the architecture or design choice in plain language
- Be specific about the technology, pattern, or approach chosen

### Consequences
- Describe the resulting context after applying the decision
- Include both positive and negative consequences
- Consider impact on non-functional requirements
- Address trade-offs made
- List follow-up decisions that may be needed

### Metadata
- Date of decision
- Decision makers/participants
- Related requirements or tickets
- References to supporting documentation

## Example ADR Template

```markdown
# ADR {number}: {title}

## Status

{status} {date}

## Context

{context description}

## Decision

{decision description}

## Consequences

### Positive

{positive consequences}

### Negative

{negative consequences}

### Risks

{identified risks}

### Dependencies

{decision dependencies}

## Related Documents

{links to related documents}
```

## Best Practices for ADRs

### When to Write an ADR

- Major technology selections or changes
- Significant architectural patterns or styles
- API design decisions that impact multiple systems
- Data model decisions that are difficult to change later
- Cross-cutting concerns like security or performance
- Trade-offs with significant long-term impact

### Writing Style

- Be concise and to the point
- Use plain language; avoid unnecessary jargon
- Focus on the "why" more than the "what"
- Document alternatives considered and why they were rejected
- Include context that may not be obvious to future readers
- Address concerns from different stakeholder perspectives

### Organization

- Use sequential numbering for ADRs (e.g., ADR-001, ADR-002)
- Maintain ADRs in a dedicated folder in version control
- Include an index file listing all ADRs
- Link related ADRs to each other
- Consider using tags for categorization

### ADR Lifecycle

- Start with a "Proposed" status for discussion
- Move to "Accepted" once the team agrees
- Mark as "Rejected" if the proposal is not implemented
- Use "Deprecated" for decisions no longer relevant
- Mark as "Superseded" with a link to the new ADR when replaced

## Optional ADR Sections

### Alternatives Considered
- Document other options that were evaluated
- Explain why each alternative was not chosen
- Include pros and cons of each alternative

### Compliance
- Note any regulatory or policy requirements addressed by the decision
- Document how the decision aligns with architectural principles

### Cost Analysis
- Include estimates of implementation cost
- Document ongoing maintenance or operational costs
- Consider technical debt implications

### Implementation Plan
- High-level steps for implementing the decision
- Timeline considerations
- Migration approach for existing systems

## Tools and Approaches

### ADR Management Tools
- [adr-tools](https://github.com/npryce/adr-tools) - Command-line tools for working with ADRs
- [log4brains](https://github.com/thomvaill/log4brains) - Knowledge base and CLI to manage ADRs
- [Markdown](https://www.markdownguide.org/) - Simple format for writing ADRs
- [PlantUML](https://plantuml.com/) - For including diagrams in ADRs

### ADR Integration
- Link ADRs to user stories or requirements in issue trackers
- Reference ADRs in code comments when implementing the decisions
- Review ADRs during architectural reviews
- Include ADR review in Definition of Done for architectural changes

## References

- [Architectural Decision Records by Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [Documenting Architecture Decisions by ThoughtWorks](https://www.thoughtworks.com/radar/techniques/lightweight-architecture-decision-records)
- [Sustainable Architectural Design Decisions (IEEE Software)](https://ieeexplore.ieee.org/document/7930199)
- [Architecture Decision Records in Action (O'Reilly)](https://www.oreilly.com/content/architecture-decision-records-in-action/)
- [ADR GitHub organization](https://adr.github.io/)
- [Documenting Software Architecture (SEI)](https://www.sei.cmu.edu/architecture/tools/document/)
