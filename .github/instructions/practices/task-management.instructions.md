---
applyTo: "**/*tasks*.md,**/*todo*.md,**/project-management*.md"
---

# Task Management Guidelines

## Overview

This document establishes standards and best practices for task management across projects. Following these guidelines ensures consistent, efficient, and transparent management of work items, enabling better collaboration and productivity.

## Task Definition Principles

### Clear Task Descriptions

- **Specific**: Tasks should clearly define what needs to be done
- **Measurable**: Include criteria to determine when the task is complete
- **Achievable**: Tasks should be realistically accomplishable
- **Relevant**: Relate tasks directly to project goals
- **Time-bound**: Include estimates or deadlines when appropriate

Example of a well-defined task:
```
Implement user authentication feature that supports login with email/password 
and Google OAuth. Include input validation, error handling, and redirect to 
dashboard after successful login. 
Estimated effort: 3 points (approx. 1-2 days)
Due: May 22, 2025
```

Example of a poorly defined task:
```
Add login functionality
```

### Task Size

- Break down large tasks into smaller, manageable pieces
- Aim for tasks that can be completed within 1-2 days
- Create sub-tasks for complex work items
- Ensure each task represents a meaningful unit of work

### Task Categories

Categorize tasks to help with organization and prioritization:

| Category | Examples |
|----------|----------|
| Feature | New functionality, enhancements |
| Bug | Defects, unexpected behavior |
| Task | Administrative, configuration, documentation |
| Technical Debt | Refactoring, optimization, cleanup |
| Research | Investigation, proof of concept |
| Support | Customer issues, help requests |

## Task Lifecycle

### Status Workflow

Standard task statuses should include:

1. **Backlog**: Not yet scheduled or prioritized
2. **To Do**: Scheduled for current/upcoming work period
3. **In Progress**: Currently being worked on
4. **Review**: Completed and awaiting review/testing
5. **Done**: Completed, reviewed, and accepted

For more complex projects, consider additional statuses:

- **Blocked**: Cannot proceed due to dependency or impediment
- **Deferred**: Postponed to a future date
- **Canceled**: No longer needed or relevant

### Transitioning Tasks

- Only move tasks to "In Progress" when actively working on them
- Limit the number of tasks in progress simultaneously
- Document blockers when moving a task to "Blocked" status
- Include relevant information when transitioning tasks:
  - Links to pull requests
  - Test results
  - Notes on implementation approach
  - Questions or concerns

### Task Progress Updates

- Update the task with the date started when beginning work
- Track and update progress regularly as work continues
- Update subtasks or checklist items as they are completed
- Document key milestones or decisions in task comments
- Record the completion date when the task is finished
- Include any relevant metrics or outcomes with the completed task

## Task Prioritization

### Priority Levels

Use clear priority levels to indicate importance:

| Priority | Description |
|----------|-------------|
| Critical | Must be addressed immediately; blocking progress |
| High | Important for current goals; should be addressed soon |
| Medium | Standard priority; address after higher priorities |
| Low | Nice to have; address when time permits |

### Prioritization Factors

Consider these factors when prioritizing tasks:

- **Business value**: Impact on users and stakeholders
- **Technical importance**: Impact on system health and sustainability
- **Urgency**: Time sensitivity and deadlines
- **Effort**: Amount of work required
- **Risk**: Potential issues or uncertainty
- **Dependencies**: Relationship to other tasks or features

### Prioritization Techniques

- **MoSCoW Method**: Must have, Should have, Could have, Won't have
- **Value vs. Effort**: Prioritize high-value, low-effort tasks first
- **Impact Mapping**: Link tasks to business objectives
- **RICE Framework**: Reach, Impact, Confidence, Effort

## Task Assignment

### Assignment Best Practices

- Assign tasks to specific individuals, not teams
- Consider skills, expertise, and workload when assigning tasks
- Allow team members to self-assign tasks when appropriate
- Avoid assigning too many tasks to one person
- Include clear expectations and context when assigning tasks

### Workload Management

- Maintain visibility into team members' current workload
- Respect team members' capacity limits
- Account for meetings, support rotations, and other responsibilities
- Plan for contingencies and unexpected work

## Task Tracking Tools

### Tool Selection Criteria

Select task management tools based on:

- Team size and distribution
- Project complexity
- Integration requirements
- Customization needs
- Reporting capabilities
- Ease of use

### Common Task Tracking Tools

- **Jira**: Full-featured project and issue tracking
- **GitHub Issues**: Software development focused tracking
- **Azure DevOps**: Integrated development and project management
- **Trello**: Visual, kanban-style boards
- **Asana**: Team collaboration and work management
- **ClickUp**: All-in-one productivity platform
- **Linear**: Streamlined software project management

### Minimum Tool Requirements

Task tracking tools should support:

- Task creation, assignment, and status tracking
- Priority and category designation
- Due dates and time tracking
- Comments and attachments
- Search and filtering
- Notifications and updates
- Reporting and visualization

## Task Estimation

### Estimation Methods

- **Time-based**: Estimate hours or days required
- **Story Points**: Relative sizing based on complexity
- **T-shirt Sizing**: XS, S, M, L, XL categories
- **Planning Poker**: Team-based consensus estimation

### Estimation Best Practices

- Include the full task lifecycle in estimates (design, implementation, testing, review)
- Account for unknowns and potential challenges
- Learn from historical data and previous estimates
- Re-estimate when significant new information emerges
- Record both estimated and actual effort for future reference

## Task Documentation

### Required Information

Each task should include:

- **Title**: Concise description of the task
- **Description**: Detailed explanation of what needs to be done
- **Acceptance Criteria**: Conditions that must be met for completion
- **Related Items**: Links to related tasks, requirements, designs
- **Tags/Labels**: Categories, components, or other classifications
- **Priority**: Importance level
- **Estimate**: Expected effort or complexity
- **Assignee**: Person responsible for the task
- **Due Date**: When the task should be completed

### Documentation Best Practices

- Use templates for consistency
- Include screenshots or diagrams when helpful
- Link to relevant resources and documentation
- Document decisions and changes
- Update documentation as the task progresses
- Maintain an audit trail of progress updates and date stamps

Sample task template:
```
## Task Description
[Detailed explanation of what needs to be done]

## Acceptance Criteria
- [ ] [Criterion 1]
- [ ] [Criterion 2]
- [ ] [Criterion 3]

## Additional Information
- **Related to**: [Links to related items]
- **Dependencies**: [List of dependencies]
- **Notes**: [Additional context or information]

## Progress Tracking
- **Date Started**: [Date when work began]
- **Status Updates**:
  - [YYYY-MM-DD]: [Brief description of progress made]
  - [YYYY-MM-DD]: [Brief description of progress made]
- **Date Completed**: [Date when task was finished]
```

## Task Reporting and Metrics

### Key Metrics

Track and report on these metrics:

- **Velocity**: Amount of work completed per time period
- **Cycle Time**: Time from start to completion
- **Lead Time**: Time from creation to completion
- **Burndown/Burnup**: Progress visualization over time
- **Cumulative Flow**: Visualization of tasks in each status
- **Escaped Defects**: Bugs found after completion
- **Blocked Time**: Duration tasks remain blocked

### Reporting Cadence

- Daily: Quick status updates (standups)
- Weekly: Progress reports and upcoming work
- Monthly: Trends and performance metrics
- Quarterly: Strategic review and planning

## Task Management in Agile Environments

### Agile Task Management

- Organize tasks into sprints or iterations
- Conduct regular backlog refinement
- Hold sprint planning sessions
- Use daily standups for status updates
- Conduct retrospectives to improve processes

### User Stories

Format user stories with:

- **As a [role]**: Who will benefit from the feature
- **I want [goal]**: What they want to accomplish
- **So that [benefit]**: Why they want to accomplish it

Example:
```
As a registered user,
I want to reset my password via email,
So that I can regain access to my account when I forget my password.
```

## Task Management for Remote Teams

### Remote Collaboration Best Practices

- Document decisions and discussions
- Use asynchronous communication when possible
- Schedule regular video check-ins
- Ensure tasks have clear ownership
- Make work visible through shared dashboards
- Use collaboration tools for real-time work sessions

### Time Zone Considerations

- Document team members' working hours
- Schedule meetings in overlapping hours
- Set reasonable expectations for response times
- Use time zone friendly annotations for deadlines

## Task Management Antipatterns

Avoid these common task management mistakes:

- **Task Hoarding**: Assigning too many tasks to one person
- **Overestimation**: Chronically underdelivering on estimates
- **Scope Creep**: Allowing tasks to expand beyond their original definition
- **Zombie Tasks**: Keeping outdated or irrelevant tasks active
- **Priority Inflation**: Marking too many tasks as high priority
- **Interrupted Flow**: Frequently reassigning people to different tasks
- **Missing Context**: Creating tasks without sufficient information
- **Invisible Work**: Significant effort that isn't captured in tasks
- **Stale Updates**: Failing to update task status and progress regularly
- **Missing Timestamps**: Not recording when tasks are started and completed

## Task Management Implementation Checklist

- [ ] Select appropriate task tracking tool
- [ ] Define task statuses and workflow
- [ ] Create task templates
- [ ] Establish prioritization system
- [ ] Define estimation approach
- [ ] Set up regular reporting
- [ ] Train team members on process
- [ ] Schedule regular process reviews
