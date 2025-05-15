---
applyTo: "**/*.md,**/*.markdown"
---
# Markdown Guidelines

## Basic Syntax

- **Headings**: Use `#` for h1, `##` for h2, etc. Leave a space after the hash.
  ```markdown
  # Heading 1
  ## Heading 2
  ### Heading 3
  ```

- **Bold**: Surround text with double asterisks or underscores.
  ```markdown
  **bold text** or __bold text__
  ```

- **Italic**: Surround text with single asterisks or underscores.
  ```markdown
  *italic text* or _italic text_
  ```

- **Blockquotes**: Use `>` at the beginning of a line.
  ```markdown
  > This is a blockquote
  > Multi-line blockquote
  ```

- **Lists**: Use `-`, `*`, or `+` for unordered lists; numbers for ordered lists.
  ```markdown
  - Item 1
  - Item 2
    - Nested item
  
  1. First item
  2. Second item
  ```

- **Code**: Use backticks for inline code, triple backticks for code blocks.
  ```markdown
  `inline code`
  
  ```javascript
  // code block with language syntax highlighting
  function greeting() {
    return 'Hello world!';
  }
  ```
  ```

- **Horizontal Rule**: Use three or more hyphens, asterisks, or underscores.
  ```markdown
  ---
  ```

- **Links**: Use `[text](https://example.com "Optional title")` format.
  ```markdown
  [GitHub](https://github.com "GitHub Homepage")
  ```

- **Images**: Use `![alt text](URL "Optional title")` format.
  ```markdown
  ![Logo](images/logo.png "Company Logo")
  ```

## Extended Syntax

- **Tables**: Use pipes and hyphens.
  ```markdown
  | Header 1 | Header 2 |
  | -------- | -------- |
  | Cell 1   | Cell 2   |
  | Cell 3   | Cell 4   |
  ```

- **Fenced Code Blocks**: Use triple backticks with optional language identifier.
  ```markdown
  ```json
  {
    "firstName": "John",
    "lastName": "Doe"
  }
  ```
  ```

- **Footnotes**: Use `[^label]` in text and `[^label]: explanation` at the end.
  ```markdown
  Here's a statement with a footnote[^1].
  
  [^1]: This is the footnote content.
  ```

- **Strikethrough**: Use double tildes.
  ```markdown
  ~~strikethrough text~~
  ```

- **Task Lists**: Use `- [ ]` for unchecked and `- [x]` for checked items.
  ```markdown
  - [x] Completed task
  - [ ] Incomplete task
  ```

- **Emoji**: Use emoji shortcodes.
  ```markdown
  :smile: :heart: :thumbsup:
  ```

## Document Structure Best Practices

- Begin each document with a top-level heading.
- Use headings to create a logical hierarchy of information.
- Keep heading levels in order (don't skip from `##` to `####`).
- Include a table of contents for longer documents.
- Leave blank lines before and after headings, lists, and code blocks.

## Content Guidelines

- Write concisely and with clarity.
- Use consistent terminology throughout the document.
- Keep paragraphs focused on a single topic.
- Use lists for steps, requirements, or features.
- Include examples to illustrate complex concepts.
- Use proper grammar and punctuation.

## Linking Best Practices

- Use relative links for files within the same repository.
- Ensure all links are functional and point to the correct destination.
- Provide descriptive link text instead of generic phrases like "click here".
- For external resources, consider if they might change or become unavailable.

## Images and Media

- Include meaningful alt text for all images.
- Use SVG or PNG for diagrams, screenshots, and logos.
- Keep image sizes reasonable.
- Place images close to the related text.
- Consider if an image is necessary or if text would suffice.

## Technical Documentation Specific

- Include version information when documenting software features.
- Clearly distinguish between required and optional parameters.
- Document expected inputs and outputs for code examples.
- Include error cases and handling.
- Update documentation when the related code changes.

## README Files

- Include a clear project title and description.
- List prerequisites and installation steps.
- Provide basic usage examples.
- Include information about contributing, license, and contact.
- Keep it concise but informative.

## Markdown in Different Environments

- Be aware that different platforms may render Markdown slightly differently.
- Test your Markdown in the target environment.
- GitHub, GitLab, and other platforms have some unique extensions.
- When in doubt, use standard Markdown for maximum compatibility.

## Resources

- [CommonMark Specification](https://commonmark.org/)
- [GitHub Flavored Markdown Spec](https://github.github.com/gfm/)
- [Markdown Guide](https://www.markdownguide.org/)
- [Daring Fireball: Markdown Basics](https://daringfireball.net/projects/markdown/basics)
