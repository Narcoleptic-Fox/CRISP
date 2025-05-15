---
applyTo: "**/*.drawio,**/*.drawio.png,**/*UML*.md"
---
# UML and Diagram Guidelines

## General Diagramming Principles

### File Format and Storage
- **Use Draw.io Format**: Create all diagrams using Draw.io (diagrams.net).
- **Save as .drawio.png**: Save diagrams as `.drawio.png` files to preserve both the editable diagram data and have a viewable image.
- **File Naming**: Use descriptive names following the pattern `[diagram-type]-[subject].drawio.png` (e.g., `class-diagram-user-module.drawio.png`).
- **Version Control**: Store diagrams in version control alongside related code or documentation.
- **Organization**: Group related diagrams in appropriate directories.

### Diagram Creation Best Practices
- **Purpose**: Each diagram should have a clear, specific purpose.
- **Scope**: Limit diagrams to a manageable scope; create multiple diagrams if needed.
- **Consistency**: Use consistent styles, symbols, and notations across all diagrams.
- **Layout**: Arrange elements to minimize crossing lines and maximize readability.
- **Whitespace**: Use adequate whitespace for clarity.
- **Title**: Include a clear title on each diagram.
- **Legend**: Add a legend for custom notations or color coding.
- **Revision Date**: Include the last updated date.

### Draw.io Specific Guidelines
- **Use Templates**: Leverage built-in UML templates in Draw.io for consistency.
- **Layers**: Use layers to organize complex diagrams (e.g., separate core elements from annotations).
- **Custom Properties**: Add metadata using custom properties.
- **Colors**: Use a consistent color scheme that works well in both light and dark modes.
- **Fonts**: Stick to readable, web-safe fonts.
- **Export Settings**: When saving as `.drawio.png`, ensure "Include diagram data" is selected.

## UML Diagram Types

### Structural Diagrams

#### Class Diagram
- **Purpose**: Show static structure of classes, interfaces, attributes, methods, and relationships.
- **Key Elements**:
  - Classes with attributes and methods
  - Interfaces
  - Inheritance, association, aggregation, composition relationships
- **Best Practices**:
  - Show only relevant attributes and methods
  - Group related classes together
  - Use different colors for different types of classes (e.g., abstract, concrete)
  - Include multiplicity on relationships

#### Component Diagram
- **Purpose**: Show components of a system and their dependencies.
- **Key Elements**:
  - Components with provided and required interfaces
  - Dependencies between components
- **Best Practices**:
  - Focus on high-level components
  - Show key dependencies
  - Group related components visually

#### Deployment Diagram
- **Purpose**: Show physical deployment of artifacts to hardware nodes.
- **Key Elements**:
  - Nodes (hardware/software execution environments)
  - Artifacts (deployable units)
  - Communication paths
- **Best Practices**:
  - Include key hardware characteristics when relevant
  - Show communication protocols
  - Group related deployment environments

#### Package Diagram
- **Purpose**: Show organization of packages and their dependencies.
- **Key Elements**:
  - Packages
  - Package dependencies
- **Best Practices**:
  - Use packages to show logical groupings
  - Show import/export relationships
  - Keep the hierarchy shallow

### Behavioral Diagrams

#### Sequence Diagram
- **Purpose**: Show interactions between objects over time.
- **Key Elements**:
  - Lifelines (participants)
  - Messages between participants
  - Activation bars showing processing time
  - Conditional, loop, and alternative fragments
- **Best Practices**:
  - Read from top to bottom (time progression)
  - Label messages clearly with method names
  - Use activation bars consistently
  - Keep diagrams focused on one scenario

#### Activity Diagram
- **Purpose**: Show workflow or business process steps.
- **Key Elements**:
  - Actions
  - Decision/merge nodes
  - Fork/join for parallel activities
  - Start/end nodes
- **Best Practices**:
  - Flow from top to bottom or left to right
  - Label decision points clearly
  - Use swimlanes to show responsibility
  - Keep the flow straightforward

#### State Diagram
- **Purpose**: Show states of an object and transitions between states.
- **Key Elements**:
  - States
  - Transitions
  - Events and guards
  - Initial and final states
- **Best Practices**:
  - Focus on one object or component
  - Label transitions with events and conditions
  - Include significant states only
  - Group related states visually

#### Use Case Diagram
- **Purpose**: Show system functionality from the user perspective.
- **Key Elements**:
  - Actors
  - Use cases
  - Relationships (include, extend, generalization)
  - System boundary
- **Best Practices**:
  - Name use cases with verb phrases
  - Show only direct system interactions
  - Group related use cases
  - Keep the diagram uncluttered

## Non-UML Diagrams

### Entity-Relationship Diagram (ERD)
- **Purpose**: Show data entities, attributes, and relationships.
- **Key Elements**:
  - Entities
  - Attributes
  - Relationships with cardinality
- **Best Practices**:
  - Show primary and foreign keys
  - Indicate relationship types clearly
  - Group related entities

### Data Flow Diagram (DFD)
- **Purpose**: Show how data moves through a system.
- **Key Elements**:
  - Processes
  - Data stores
  - External entities
  - Data flows
- **Best Practices**:
  - Label all flows
  - Use consistent levels of abstraction
  - Number processes for reference

### Architecture Diagram
- **Purpose**: Show high-level system structure and organization.
- **Key Elements**:
  - Layers or tiers
  - Major components
  - External systems
  - Communication paths
- **Best Practices**:
  - Focus on the big picture
  - Show key integration points
  - Include a legend for notation

### C4 Model Diagrams
- **Purpose**: Show software architecture at different levels of abstraction.
- **Levels**:
  - Context: System in its environment
  - Container: Applications and data stores
  - Component: Components inside containers
  - Code: Classes and relationships
- **Best Practices**:
  - Start with context and drill down
  - Use consistent notation across levels
  - Include responsibilities in element descriptions

## Diagram Review Checklist

### Content
- [ ] Diagram has a clear purpose and audience
- [ ] All elements are necessary and relevant
- [ ] Appropriate level of detail for the audience
- [ ] Correct UML/diagram notation
- [ ] Consistent with other documentation and code
- [ ] Accurate representation of the system

### Format
- [ ] Saved as `.drawio.png` with embedded data
- [ ] Clear title and optional description
- [ ] Appropriate use of colors and styles
- [ ] Readable fonts and labels
- [ ] Good layout with minimal crossing lines
- [ ] Legend included for custom notations
- [ ] Date of last update

### Technical Accuracy
- [ ] Relationships are correct (types and multiplicity)
- [ ] All necessary elements are included
- [ ] No contradictions with other diagrams
- [ ] Correctly represents actual or planned implementation
- [ ] Follows established architectural patterns

## Tool-Specific Tips

### Draw.io (diagrams.net)
- Use keyboard shortcuts for efficiency (Ctrl+D to duplicate, Alt+Drag to clone)
- Use the "Connect" mode (compass icon) to create properly attached relationships
- Enable snap to grid for better alignment
- Use containers to group related elements
- Leverage built-in UML shape libraries
- Use Export > PNG > "Include a copy of my diagram" to create `.drawio.png` files

### Integration with Documentation
- In Markdown: `![Diagram Title](path/to/diagram.drawio.png)`
- In HTML docs: `<img src="path/to/diagram.drawio.png" alt="Diagram Title">`
- In Word: Insert > Picture > From File

## Resources

- [Draw.io/diagrams.net Official Documentation](https://www.diagrams.net/doc/)
- [UML Specification](https://www.omg.org/spec/UML/)
- [C4 Model for Software Architecture](https://c4model.com/)
- [UML Distilled by Martin Fowler](https://martinfowler.com/books/uml.html)
- [The Object Primer by Scott Ambler](http://www.agilemodeling.com/artifacts/)
