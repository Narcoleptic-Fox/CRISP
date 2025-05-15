# Instructions Directory

This directory contains various instruction files and guidelines organized by category.

## Directory Structure

- **diagrams/** - Instructions related to diagram standards and practices (UML, etc.)
- **documentation/** - Documentation standards, templates, and guidelines
- **languages/** - Language-specific coding standards and best practices
- **practices/** - Development practices, methodologies, and general guidelines

Each subdirectory contains instruction files with specific guidelines for their respective areas.

## File Naming Convention

Instruction files follow the naming convention: `[topic].instructions.md`

## Applying Instructions

Each instruction file contains frontmatter with an `applyTo` property that specifies which files the instructions should be applied to. For example:

```
---
applyTo: "**/*profile*.md,**/*profiles*.md"
---
```

This indicates that the instructions apply to all files in any directory that contain "profile" or "profiles" in their name and have the .md extension.
