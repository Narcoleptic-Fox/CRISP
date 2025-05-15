---
applyTo: "**/*UX*.md,**/*UI*.md,**/*design*.md,**/*user*experience*.md"
---
# User Experience Documentation Guidelines

## UX Strategy Documents

### UX Strategy Document
- **Purpose**: Define overall UX vision, principles, and approach.
- **Audience**: Stakeholders, product team, designers, developers.
- **Sections to Include**:
  - UX vision and goals
  - User-centered design principles
  - Target audience definition
  - Key performance indicators (KPIs)
  - Design system approach
  - Research methodology
  - Competitive analysis summary
  - UX roadmap

### Design Principles Document
- **Purpose**: Establish core principles guiding design decisions.
- **Audience**: UX team, product team, developers.
- **Sections to Include**:
  - Each principle with name, description, and examples
  - How principles support business and user goals
  - How to apply principles to decision-making
  - Visual examples demonstrating each principle

## User Research Documentation

### Research Plan
- **Purpose**: Define research goals, methods, and timeline.
- **Audience**: UX team, product team, stakeholders.
- **Sections to Include**:
  - Research objectives
  - Research questions
  - Methodology and rationale
  - Participant profiles
  - Recruitment approach
  - Schedule and timeline
  - Required resources
  - Roles and responsibilities

### Research Plan Template
```markdown
# Research Plan: [Project Name]

## Research Objectives
What we aim to learn from this research.

## Research Questions
Specific questions we need to answer.

## Methodology
- **Research Methods**: [Methods to be used]
- **Rationale**: Why these methods were chosen

## Participant Information
- **Participant Profiles**: Target user types
- **Number of Participants**: How many participants
- **Recruitment Criteria**: Screening criteria
- **Recruitment Method**: How participants will be sourced

## Timeline
| Phase | Dates | Activities | Deliverables |
|-------|-------|------------|--------------|
| Phase | Dates | Activities | Deliverables |

## Resources Needed
- Team members involved
- Equipment needed
- Software requirements
- Facilities needed
- Incentives for participants

## Roles and Responsibilities
| Role | Responsibilities | Person Assigned |
|------|------------------|-----------------|
| Role | Responsibilities | Name |
```

### User Interview Guide
- **Purpose**: Structure user interviews consistently.
- **Audience**: UX researchers, stakeholders observing research.
- **Sections to Include**:
  - Introduction script
  - Warm-up questions
  - Main interview questions by topic
  - Follow-up probes
  - Task instructions (if applicable)
  - Wrap-up questions
  - Thank you and next steps

### Research Report
- **Purpose**: Document research findings and insights.
- **Audience**: Product team, stakeholders, developers.
- **Sections to Include**:
  - Executive summary
  - Research objectives and questions
  - Methodology overview
  - Participant demographics
  - Key findings with supporting evidence
  - Insights and recommendations
  - Impact on product decisions
  - Next steps

### Research Report Template
```markdown
# Research Report: [Project Name]

## Executive Summary
Brief overview of key findings and recommendations.

## Research Background
- **Objectives**: What we aimed to learn
- **Research Questions**: Questions we sought to answer
- **Methodology**: Methods used and why
- **Participants**: Number and key characteristics

## Key Findings
### Finding 1: [Title]
- Description of the finding
- Supporting evidence (quotes, observations, data)
- Implications

### Finding 2: [Title]
- Description of the finding
- Supporting evidence (quotes, observations, data)
- Implications

## Insights and Patterns
Broader patterns and insights across findings.

## Recommendations
| Recommendation | Priority | Effort | Impact |
|----------------|----------|--------|--------|
| Recommendation | H/M/L | H/M/L | H/M/L |

## Appendices
- Detailed methodology
- Participant demographics
- Research materials
- Raw data summary
```

## User Modeling Documentation

### User Persona Document
- **Purpose**: Create representative models of target users.
- **Audience**: Product team, designers, developers, stakeholders.
- **Sections to Include**:
  - Name and photo (representative)
  - Quote that captures their attitude
  - Demographics
  - Goals and motivations
  - Pain points and frustrations
  - Behaviors and habits
  - Technology proficiency
  - Contextual information
  - Scenarios of product use

### Persona Template
```markdown
# Persona: [Name]

![Persona Image](path/to/image.drawio.png)

## Quote
"A representative quote that captures their perspective."

## Demographics
- Age: 
- Occupation:
- Education:
- Location:
- Family Status:
- Income Level:

## Goals and Motivations
- Primary Goal:
- Secondary Goals:
- What motivates them:

## Frustrations and Pain Points
- Primary Frustrations:
- Challenges:
- Current Workarounds:

## Behaviors and Habits
- Typical Day:
- Technology Usage:
- Relevant Habits:
- Information Sources:

## Technology Proficiency
- Devices Used:
- Software Familiarity:
- Comfort Level:
- Preferred Platforms:

## Scenarios
- **Scenario 1**: Description of how this persona would use the product in a specific situation.
- **Scenario 2**: Another contextual scenario.
```

### User Journey Map
- **Purpose**: Visualize user's experience through time and touchpoints.
- **Audience**: UX team, product team, stakeholders.
- **Elements to Include**:
  - User stages/phases
  - User actions at each stage
  - User thoughts and emotions
  - Pain points and opportunities
  - Touchpoints and channels
  - Supporting resources
  - Emotional journey graph

### User Journey Map Template
- Create journey maps using Draw.io (.drawio.png format).
- Include all phases from awareness to post-use.
- Show emotional highs and lows visually.
- Include opportunities for improvement.

## Information Architecture Documentation

### Site Map
- **Purpose**: Document the hierarchical structure of content and features.
- **Audience**: UX team, content strategists, developers.
- **Elements to Include**:
  - Page hierarchy
  - Navigation structure
  - Content groupings
  - Page types
  - Cross-linking relationships

### Content Inventory
- **Purpose**: Catalog and assess all content in the product.
- **Audience**: Content strategists, UX team, product team.
- **Elements to Include**:
  - Content ID
  - Content title/name
  - Content type
  - Location/URL
  - Owner/author
  - Last updated date
  - Content status
  - Quality assessment
  - Content metrics (if available)
  - Notes for improvement

### Content Model
- **Purpose**: Define structured content types and their relationships.
- **Audience**: Content strategists, designers, developers.
- **Elements to Include**:
  - Content types
  - Content attributes
  - Relationships between content types
  - Content governance rules
  - Metadata schema

## Interaction Design Documentation

### Wireframes
- **Purpose**: Illustrate layout, information hierarchy, and functionality.
- **Audience**: UX team, developers, stakeholders.
- **Best Practices**:
  - Create wireframes using Draw.io (.drawio.png format).
  - Include clear labels and annotations.
  - Show different states (e.g., empty, loading, error).
  - Use consistent patterns across screens.
  - Include a version number and date.

### User Flow Diagrams
- **Purpose**: Map out paths users take to complete tasks.
- **Audience**: UX team, developers, product team.
- **Elements to Include**:
  - Starting point
  - Decision points
  - Actions
  - Screen states
  - Success and failure endpoints
  - Alternative paths
  - Error states and recovery flows

### User Flow Template
- Create user flows using Draw.io (.drawio.png format).
- Use consistent symbols for different types of steps.
- Include a legend explaining symbols.
- Number steps for easy reference.
- Group related flows visually.

### Interactive Prototype Documentation
- **Purpose**: Provide context for interactive prototypes.
- **Audience**: Developers, stakeholders, test participants.
- **Elements to Include**:
  - Prototype purpose and scope
  - Tools used and access instructions
  - Supported user flows
  - Limitations and assumptions
  - Testing instructions

## Visual Design Documentation

### UI Component Documentation
- **Purpose**: Define reusable UI components and their usage.
- **Audience**: Designers, developers, product team.
- **Elements to Include**:
  - Component name and description
  - Visual examples of all states
  - Usage guidelines
  - Behavior specifications
  - Accessibility considerations
  - Code reference (if available)
  - Spacing and layout guidelines

### Style Guide
- **Purpose**: Document visual design standards.
- **Audience**: Design team, developers, other contributors.
- **Elements to Include**:
  - Color palette with hex codes and usage guidelines
  - Typography system with fonts, sizes, and usage
  - Iconography guidelines and library
  - Spacing and layout standards
  - Photography/imagery guidelines
  - Voice and tone for copy
  - Logo usage rules

### Design System Documentation
- **Purpose**: Comprehensive repository of design patterns and components.
- **Audience**: Designers, developers, product team.
- **Elements to Include**:
  - Design principles
  - Component library with usage guidelines
  - Pattern library
  - Code snippets and technical implementation
  - Accessibility standards
  - Content guidelines
  - Tools and resources

## Usability Testing Documentation

### Usability Test Plan
- **Purpose**: Define goals, methodology, and logistics for usability testing.
- **Audience**: UX team, stakeholders, observers.
- **Elements to Include**:
  - Test objectives
  - Research questions
  - Methodology
  - Participant criteria
  - Task scenarios
  - Testing environment
  - Roles and responsibilities
  - Schedule and timeline
  - Metrics to be collected

### Usability Test Plan Template
```markdown
# Usability Test Plan: [Project Name]

## Test Objectives
What we aim to learn from this test.

## Research Questions
Specific questions we need to answer.

## Methodology
- **Test Format**: [Moderated/Unmoderated]
- **Test Location**: [Remote/In-person/Lab]
- **Session Length**: [Duration]
- **Recording Method**: [Screen/Audio/Video]

## Participants
- **Number of Participants**: [Number]
- **Participant Profiles**: [Description]
- **Recruitment Method**: [Method]

## Tasks
1. Task 1: [Description]
   - Success criteria:
   - Time estimate:
2. Task 2: [Description]
   - Success criteria:
   - Time estimate:

## Metrics
- Quantitative metrics to collect
- Qualitative data to gather

## Test Environment
- Devices/browsers to test
- Setup requirements
- Materials needed

## Schedule
| Date | Time | Participant | Facilitator | Note-taker | Observers |
|------|------|-------------|------------|------------|-----------|
| Date | Time | ID | Name | Name | Names |

## Roles and Responsibilities
| Role | Responsibilities | Person Assigned |
|------|------------------|-----------------|
| Role | Responsibilities | Name |
```

### Usability Test Script
- **Purpose**: Guide facilitator through consistent test sessions.
- **Audience**: Test facilitator, observers.
- **Elements to Include**:
  - Introduction and welcome
  - Consent form process
  - Background questions
  - Think-aloud instructions
  - Task scenarios with success criteria
  - Post-task questions
  - Post-test questions
  - Closing remarks

### Usability Test Report
- **Purpose**: Document test findings and recommendations.
- **Audience**: Product team, stakeholders, developers.
- **Elements to Include**:
  - Executive summary
  - Test objectives and methodology
  - Participant demographics
  - Task success rates and metrics
  - Key findings with supporting evidence
  - Priority issues identified
  - Recommendations with priority levels
  - Next steps

### Usability Test Report Template
```markdown
# Usability Test Report: [Project Name]

## Executive Summary
Brief overview of key findings and recommendations.

## Test Background
- **Objectives**: Goals of the test
- **Methodology**: Test approach
- **Participants**: Number and key characteristics

## Results Summary
- **Task Completion Rates**: Success percentage by task
- **Task Completion Times**: Average time by task
- **Satisfaction Scores**: Average ratings
- **System Usability Scale (SUS) Score**: Overall usability score

## Key Findings
### Issue 1: [Title]
- Description of the issue
- Evidence (observations, quotes, metrics)
- Severity rating
- Recommendation

### Issue 2: [Title]
- Description of the issue
- Evidence (observations, quotes, metrics)
- Severity rating
- Recommendation

## Positive Findings
Aspects that worked well or received positive feedback.

## Recommendations
| Recommendation | Priority | Effort | Impact |
|----------------|----------|--------|--------|
| Recommendation | H/M/L | H/M/L | H/M/L |

## Next Steps
Proposed actions based on the findings.

## Appendices
- Detailed methodology
- Task scenarios
- Participant information
- Raw data
- Recordings reference
```

## Accessibility Documentation

### Accessibility Guidelines Document
- **Purpose**: Define accessibility standards and requirements.
- **Audience**: Designers, developers, content creators, QA.
- **Elements to Include**:
  - Accessibility compliance level (WCAG 2.1 A, AA, AAA)
  - Key requirements by category:
    - Perceivable
    - Operable
    - Understandable
    - Robust
  - Implementation guidelines for common elements
  - Testing procedures
  - Tools and resources

### Accessibility Audit Report
- **Purpose**: Document compliance with accessibility standards.
- **Audience**: Product team, stakeholders, developers.
- **Elements to Include**:
  - Compliance level assessed
  - Testing methodology
  - Summary of findings
  - Issues by severity
  - Remediation recommendations
  - Timeline for fixes
  - Retest plan

## Best Practices

### Documentation Management
- **Version Control**: Maintain document versioning.
- **Centralization**: Store UX documentation in a central repository.
- **Naming Conventions**: Use consistent file naming.
- **Templates**: Create and use standardized templates.
- **Update Schedule**: Establish regular review and update cycles.
- **Ownership**: Assign clear ownership for each document.

### Visual Documentation
- Use Draw.io (.drawio.png format) for all diagrams.
- Include legends for diagram symbols and colors.
- Use consistent visual language across all diagrams.
- Balance detail with clarity.
- Use appropriate diagram types for different purposes.

### Writing for UX Documentation
- Write concisely and clearly.
- Avoid jargon unless necessary.
- Use active voice.
- Include examples and illustrations.
- Structure content with clear headings.
- Link related documentation.
- Separate facts from assumptions.

## Tools and Resources

### UX Documentation Tools
- **Diagramming**: Draw.io/diagrams.net (save as .drawio.png)
- **Collaborative Documentation**: Confluence, Notion, Google Docs
- **Design Systems**: Storybook, Zeroheight, Figma
- **User Research**: Dovetail, UserZoom, Optimal Workshop
- **Prototyping**: Figma, Axure, Adobe XD
- **Version Control**: GitHub, GitLab
- **Knowledge Management**: Notion, Confluence

### UX Standards and References
- [Nielsen Norman Group](https://www.nngroup.com/)
- [Interaction Design Foundation](https://www.interaction-design.org/)
- [WCAG 2.1 Guidelines](https://www.w3.org/TR/WCAG21/)
- [Material Design](https://material.io/)
- [Apple Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- [Microsoft Fluent Design](https://www.microsoft.com/design/fluent/)
- [IBM Carbon Design System](https://www.carbondesignsystem.com/)
