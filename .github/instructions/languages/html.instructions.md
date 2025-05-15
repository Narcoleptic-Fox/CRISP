---
applyTo: "**/*.html"
---
# HTML Guidelines

## Document Structure

- Always include `<!DOCTYPE html>` declaration.
- Use proper HTML5 document structure with `<html>`, `<head>`, and `<body>` elements.
- Set the language attribute in the html tag: `<html lang="en">`.
- Include viewport meta tag for responsive design:
  ```html
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  ```
- Include character encoding: `<meta charset="UTF-8">`.
- Use descriptive `<title>` elements.

## Semantic HTML

- Use semantic elements to describe content (`<header>`, `<footer>`, `<main>`, `<section>`, `<article>`, etc.).
- Use `<nav>` for navigation links.
- Use `<button>` for clickable controls that trigger actions.
- Use `<a>` for links to other pages or locations.
- Use heading tags (`<h1>` through `<h6>`) in proper hierarchical order.
- Use `<figure>` and `<figcaption>` for images with captions.
- Use `<aside>` for content tangentially related to the main content.
- Use `<time>` for dates and times.

## Forms

- Always use `<label>` elements with `for` attributes to associate with form controls.
- Use appropriate input types (`email`, `tel`, `date`, `number`, etc.).
- Include placeholder text for guidance, but don't rely on it exclusively.
- Group related form controls with `<fieldset>` and `<legend>`.
- Use validation attributes (`required`, `pattern`, `min`, `max`, etc.).
- Include descriptive error messages for form validation.

## Accessibility

- Use ARIA attributes when necessary (`aria-label`, `aria-describedby`, etc.).
- Include alt text for images: `<img src="image.jpg" alt="Description">`.
- Use `tabindex` appropriately to manage focus order.
- Ensure sufficient color contrast.
- Make sure interactive elements are keyboard accessible.
- Structure content logically with headings and landmarks.
- Test with screen readers.

## Best Practices

- Keep markup clean and minimal.
- Use proper indentation for nested elements.
- Use lowercase for element names and attributes.
- Use double quotes for attribute values.
- Avoid inline styles; use external CSS.
- Avoid obsolete elements and attributes.
- Make sure all elements are properly closed.
- Avoid deep nesting of elements.

## Meta Information

- Include appropriate meta tags:
  ```html
  <meta name="description" content="Page description">
  <meta name="keywords" content="relevant, keywords">
  <meta name="author" content="Author Name">
  ```
- Include Open Graph tags for social media sharing:
  ```html
  <meta property="og:title" content="Page Title">
  <meta property="og:description" content="Page Description">
  <meta property="og:image" content="image-url.jpg">
  ```

## Performance

- Load JavaScript files just before the closing `</body>` tag or use `defer` attribute.
- Use `async` attribute for non-essential scripts.
- Optimize images and use responsive images with `srcset` and `sizes` attributes.
- Consider using lazy loading for images and iframes: `loading="lazy"`.

## Validation

- Validate HTML using tools like the W3C Validator.
- Fix any validation errors or warnings.

## Resources

- [HTML Living Standard](https://html.spec.whatwg.org/)
- [MDN Web Docs - HTML](https://developer.mozilla.org/en-US/docs/Web/HTML)
- [W3C HTML Validator](https://validator.w3.org/)
- [HTML5 Doctor](http://html5doctor.com/)
- [WebAIM - Web Accessibility](https://webaim.org/)
