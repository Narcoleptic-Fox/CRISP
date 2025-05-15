---
applyTo: "**/*.doc,**/*.docx,**/*.md,**/*.txt,**/*Documentation*/**"
---
# Technical Writing Guidelines

## Document Structure

### Document Sections
- **Title Page**: Include document title, version, date, and author information.
- **Table of Contents**: Automatically generated from headings.
- **Executive Summary/Abstract**: Brief overview of the document's purpose and content.
- **Introduction**: Purpose, scope, intended audience, and document organization.
- **Body**: Main content organized in a logical structure.
- **Conclusion/Summary**: Recap of key points.
- **References/Bibliography**: Sources cited in the document.
- **Appendices**: Supplementary material.
- **Glossary**: Definitions of terms used in the document.
- **Index**: For longer documents, an alphabetical list of topics.
- **Revision History**: Document change log.

### Formatting
- Use consistent heading styles throughout the document.
- Apply hierarchical numbering for sections (1, 1.1, 1.1.1).
- Use bulleted lists for unordered items and numbered lists for sequential or prioritized items.
- Include page numbers and document identifiers in headers or footers.
- Apply consistent paragraph and line spacing.
- Use tables to organize complex information.
- Consider accessibility guidelines for formatting (high contrast, readable fonts, etc.).

## Writing Style

### Clarity and Precision
- Write in clear, concise language.
- Use active voice rather than passive voice when possible.
- Define acronyms and technical terms at first use.
- Be specific and precise in descriptions.
- Avoid ambiguity and vagueness.
- Use concrete examples to illustrate concepts.

### Consistency
- Maintain consistent terminology throughout the document.
- Use style guides appropriate for your field (e.g., Microsoft Style Guide, Google Developer Documentation Style Guide).
- Apply consistent capitalization, hyphenation, and punctuation rules.
- Use the same tense throughout (usually present tense).
- Use a consistent person/point of view (usually third person).

### Audience Consideration
- Identify your primary audience and their technical level.
- Adjust technical depth based on audience knowledge.
- Define terms that may be unfamiliar to your audience.
- Consider cultural and linguistic differences for international audiences.

### Tone and Voice
- Maintain a professional, objective tone.
- Avoid colloquialisms, slang, and idioms.
- Use a consistent voice across documentation.
- Minimize use of jargon except when targeting technical specialists.

## Visual Elements

### Effective Use of Visuals
- Use diagrams, charts, screenshots, and other visuals to supplement text.
- Ensure all visuals have appropriate captions and references in the text.
- Use consistent visual styling across all diagrams.
- Create visuals with accessibility in mind (color blind-friendly palettes, text alternatives).
- Balance text and visuals throughout the document.

### Diagram Best Practices
- Label all components of diagrams.
- Include legends for complex diagrams.
- Use appropriate diagram types:
  - Flowcharts for processes
  - Entity-relationship diagrams for data structures
  - Sequence diagrams for interactions
  - Architectural diagrams for system components
- Maintain visual hierarchy in diagrams.
- Keep diagrams simple and focused on key points.

## Technical Documentation Types

### API Documentation
- Include endpoint descriptions, parameters, request/response formats.
- Provide authentication requirements.
- Include example requests and responses.
- Document error codes and their meanings.
- Consider using standards like OpenAPI/Swagger.

### User Guides
- Organize by user tasks.
- Include step-by-step instructions.
- Use screenshots to illustrate UI elements.
- Address common user questions and issues.
- Include troubleshooting sections.

### Code Documentation
- Document public APIs, classes, and functions.
- Explain parameters, return values, and exceptions.
- Provide usage examples.
- Document assumptions and limitations.
- Follow language-specific standards (Javadoc, JSDoc, etc.).

### Technical Specifications
- Define scope and requirements clearly.
- Include design rationale.
- Document interfaces and dependencies.
- Address performance and security considerations.
- Include testing approach.

## Review and Quality Assurance

### Technical Review
- Verify technical accuracy with subject matter experts.
- Check that all procedures work as documented.
- Ensure code examples are correct and follow best practices.
- Validate that diagrams accurately represent the system.

### Editorial Review
- Check grammar, spelling, and punctuation.
- Ensure consistent formatting and style.
- Verify cross-references and links.
- Check for readability and clarity.

### User Testing
- Test instructions with actual users when possible.
- Gather and incorporate feedback.
- Verify that documentation meets user needs.
- Identify and fill gaps in content.

## Documentation Management

### Version Control
- Use version control systems for documentation.
- Maintain a change log or revision history.
- Consider branching strategies for major document versions.
- Archive old versions of documentation.

### Documentation as Code
- Consider treating documentation as code (docs-as-code).
- Use plain text formats like Markdown.
- Apply automated testing to documentation.
- Implement continuous integration for documentation.

### Maintenance Plan
- Schedule regular reviews of documentation.
- Update documentation when the product changes.
- Remove outdated or deprecated information.
- Track documentation issues and priorities.

## Standards Compliance

### IEEE Standards
- IEEE 1063-2001 (Software User Documentation)
- IEEE 829 (Software Test Documentation)
- IEEE 1016 (Software Design Descriptions)
- IEEE 830 (Software Requirements Specifications)

### ISO Standards
- ISO/IEC/IEEE 26511 (Systems and software engineering — Requirements for managers of information for users of systems, software, and services)
- ISO/IEC/IEEE 26512 (Systems and software engineering — Requirements for acquirers and suppliers of information for users)
- ISO/IEC/IEEE 26513 (Systems and software engineering — Requirements for testers and reviewers of information for users)
- ISO/IEC/IEEE 26514 (Systems and software engineering — Requirements for designers and developers of information for users)

### Accessibility Standards
- WCAG (Web Content Accessibility Guidelines)
- Section 508 (US federal regulations for accessibility)
- PDF/UA (Universal Accessibility for PDF documents)

## Tools and Technologies

### Documentation Tools
- Word processors (Microsoft Word, Google Docs)
- Technical documentation platforms (MadCap Flare, Adobe FrameMaker)
- Wiki platforms (Confluence, MediaWiki)
- Markup languages (Markdown, reStructuredText, AsciiDoc)
- API documentation tools (Swagger, ReadMe, Postman)

### Collaboration Tools
- Version control systems (Git, Subversion)
- Review tools (GitHub PR reviews, Reviewable)
- Style and grammar checkers (Grammarly, Vale)
- Diagram creation tools (Lucidchart, Draw.io, PlantUML)

## Resources

- [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)
- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [The Chicago Manual of Style](https://www.chicagomanualofstyle.org/)
- [Write the Docs](https://www.writethedocs.org/)
- [IEEE Software Engineering Standards](https://www.ieee.org/)
- [Information Mapping](https://www.informationmapping.com/)
- [Docs Like Code](https://docslikecode.com/)
