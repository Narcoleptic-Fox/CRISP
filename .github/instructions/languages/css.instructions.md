---
applyTo: "**/*.css"
---
# CSS Guidelines

## Syntax and Formatting

- Use consistent indentation (usually 2 or 4 spaces).
- Place opening brace on the same line as the selector.
- Include one space before the opening brace.
- Use one declaration per line.
- End all declarations with a semicolon.
- Include one space after each colon.
- Use lowercase and shorthand hex values: `#fff` instead of `#FFFFFF`.
- Use single or double quotes consistently, preferring double quotes: `content: ""`.
- Include leading zeros in decimal values: `0.5` instead of `.5`.
- Separate selector and declaration blocks by a new line.
- Separate rule sets by a blank line.

```css
/* Good */
.selector {
  property: value;
  property2: value;
}

/* Avoid */
.selector{ property:value; }
```

## Naming Conventions

- Use hyphen-delimited class names: `.button-primary`.
- Use meaningful, descriptive names that reflect purpose, not presentation.
- Use BEM (Block Element Modifier), SMACSS, or other naming convention consistently.
- Avoid overly long class names.

```css
/* BEM Example */
.card {}                  /* Block */
.card__title {}           /* Element */
.card__button {}          /* Element */
.card__button--primary {} /* Modifier */
```

## Selectors

- Keep selectors specific but not too specific to avoid specificity issues.
- Avoid using IDs for styling; prefer classes.
- Avoid deep nesting; aim for no more than 3 levels deep.
- Minimize coupling to HTML structure when possible.
- Use combinators thoughtfully: `>` for direct children, `+` for adjacent siblings.

## Properties

- Group related properties together.
- Consider ordering properties consistently (alphabetically or by type).
- Use shorthand properties where appropriate: `margin: 10px 20px` instead of separate properties.

## Units

- Use relative units (rem, em, %) when possible for better responsiveness.
- Use `rem` for font sizes, margins, and padding to respect user preferences.
- Use viewport units (vw, vh) for layout elements that should respond to viewport.
- Use px for borders and small, fixed-size elements.
- Use unitless values for line-height: `line-height: 1.5`.

## Colors

- Use consistent color format (hex, rgb, hsl) throughout project.
- Consider using CSS custom properties (variables) for color themes:
  ```css
  :root {
    --primary-color: #3498db;
    --secondary-color: #2ecc71;
  }

  .button {
    background-color: var(--primary-color);
  }
  ```

## Media Queries

- Place media queries close to their relevant rule sets when using a component-based approach.
- Use mobile-first approach for responsive design:
  ```css
  /* Base styles for mobile */
  .element {
    width: 100%;
  }

  /* Larger screens */
  @media (min-width: 768px) {
    .element {
      width: 50%;
    }
  }
  ```

## Layout

- Use CSS Grid for two-dimensional layouts.
- Use Flexbox for one-dimensional layouts.
- Avoid using floats for layout.
- Use relative positioning by default, absolute only when necessary.

## CSS Architecture

- Consider using modular CSS architecture (BEM, SMACSS, ITCSS, etc.).
- Split CSS into logical components or modules.
- Use CSS custom properties for themes and global values.
- Consider using utility classes for common patterns.

## Performance

- Minimize redundant CSS.
- Avoid universal selectors (`*`) and overly broad selectors.
- Use efficient selectors to help browser rendering.
- Consider using CSS containment for complex layouts.
- Avoid `@import` in CSS files; use build tools for combining files.

## Browser Support

- Use vendor prefixes or tools like Autoprefixer for better compatibility.
- Include fallbacks for critical properties when necessary.
- Test across multiple browsers and devices.

## Modern CSS Features

- Use CSS Grid and Flexbox for layout.
- Leverage CSS custom properties (variables) for theming and repeated values.
- Use `calc()` for dynamic calculations.
- Consider using CSS animations and transitions instead of JavaScript when appropriate.
- Utilize new selectors like `:is()`, `:where()`, and `:has()` with appropriate browser support checks.

## Resources

- [MDN CSS Documentation](https://developer.mozilla.org/en-US/docs/Web/CSS)
- [CSS-Tricks](https://css-tricks.com/)
- [Every Layout](https://every-layout.dev/)
- [BEM Methodology](https://en.bem.info/methodology/)
- [SMACSS](http://smacss.com/)
