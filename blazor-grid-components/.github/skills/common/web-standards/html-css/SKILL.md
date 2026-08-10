---
name: html-css
description: Mandated skill covering semantic HTML5, ARIA integration, CSS isolation, CSS custom properties, container queries, responsive design, theming systems, and performance-safe animation patterns for Blazor UI component authors targeting .NET 8 and .NET 10.
metadata:
  category: Frontend
  tags: [html5, css, css-isolation, css-variables, container-queries, aria, responsive, theming, flexbox, grid, animations, layer, cascade-layers]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Static SSR", "Stream Rendering", "Interactive Server", "Interactive WASM", "Auto"]
  csharp: ["C# 12", "C# 13", "C# 14"]
---

# Skill 04 — HTML & CSS for Component Authors (Mandated)

> **Applies to:** .NET 8 (LTS) · .NET 10 (LTS) — Blazor Web App project template
> **Proficiency required:** Intermediate
> **Last reviewed:** April 2026
> **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/blazor/components/css-isolation

---

## Summary

Component authors must produce semantically correct, accessible, responsive, and themeable HTML/CSS. In .NET 8+ Blazor Web Apps, HTML is served directly from the Static SSR pipeline for many pages, making markup quality and CSS scoping more critical than ever.

---

## Key Competencies

### Semantic HTML
- Use the right element for the right job: `<button>` for actions, `<a>` for navigation, `<nav>`, `<main>`, `<aside>`, `<section>`, `<article>`, `<header>`, `<footer>` for structure.
- Never use `<div>` or `<span>` for interactive controls; they lack keyboard and accessibility semantics.
- Provide `<label>` for every form input; use `for`/`id` association or wrap input in `<label>`.

### ARIA & Accessibility Integration
- Add `role`, `aria-label`, `aria-describedby`, `aria-live`, `aria-expanded`, and `aria-controls` attributes where native semantics are insufficient.
- Manage `aria-busy="true"` during async loading states (especially relevant for `[StreamRendering]` pages).

### CSS Architecture
- **CSS Isolation (Blazor):** Use `.razor.css` scoped stylesheets for component-level styles; understand the `b-*` scope attribute injection.
- **CSS Custom Properties (Variables):** Define design tokens for colors, spacing, and typography at `:root`; consume in component stylesheets to enable theming.
- **Flexbox & Grid:** Use Flexbox for one-dimensional layouts; CSS Grid for two-dimensional. Avoid legacy float-based layouts.
- **Responsive Design:** Mobile-first breakpoints using `min-width` media queries. Components must be responsive at `320px` minimum width.

### Theming System
- Use CSS custom properties for all color and spacing tokens; do not hardcode values in component styles.
- Support Light and Dark themes via `prefers-color-scheme` media query and/or explicit theme class on `<html>`.
- Respect `.NET 8` Blazor Web App default template conventions for theme variables when working in-tree.

### Modern CSS Features (2024–2025)

Use modern CSS features supported in all evergreen browsers and Edge:

| Feature | Usage |
|---|---|
| **Container queries** (`@container`) | Component-level responsive layout; prefer over media queries for reusable components |
| **CSS Cascade Layers** (`@layer`) | Manage specificity conflict in design systems; layer order: `base`, `components`, `utilities` |
| **`:has()` selector** | Style parent based on child state (e.g., `form:has(:invalid)`) |
| **`color-mix()`** | Generate tints/shades from a base token color |
| **`@starting-style`** | Animate element entry (replacing JS-based enter animations) |
| **Subgrid** | Align nested grid children to parent grid tracks |

```css
/* Container query — component responds to its own container width, not viewport */
@container (min-width: 400px) {
    .card__title {
        font-size: 1.25rem;
    }
}

/* Cascade layers — explicit specificity management */
@layer base, components, utilities;

@layer components {
    .btn { /* component styles at known specificity */ }
}
```

### Performance & Best Practices
- Minimize CSS specificity; prefer class selectors over ID or element selectors.
- Avoid `!important` unless overriding third-party styles; document when used.
- Use `will-change` and `contain` CSS properties for animations; avoid layout-triggering animations.
- Prefer `transform`/`opacity` for animations (GPU composited, no layout cost).
- Use `content-visibility: auto` on off-screen sections to defer rendering cost.
- Avoid `@import` in stylesheets (render-blocking); use CSS bundling instead.

---

## Measured Standards (Mandatory)

- [ ] All interactive components pass `axe` automated accessibility check with zero critical or serious violations.
- [ ] Styles use `.razor.css` isolation or a clearly documented namespace; no unscoped global styles introduced without approval.
- [ ] Components render correctly at `320px` (mobile) and `1440px` (desktop) viewport widths.
- [ ] Color tokens reference CSS custom properties; no hardcoded color values in `.razor.css` files.
- [ ] No `<div>` used as a button, link, or interactive control.

---

## Minimal Artifacts (Required for New Components)

- [ ] `.razor.css` file demonstrating CSS variable usage and at least one responsive rule.
- [ ] Screenshots or automated visual regression snapshot at mobile and desktop breakpoints.

---

## Resources

| Topic | URL |
|---|---|
| MDN HTML reference | https://developer.mozilla.org/docs/Web/HTML/Element |
| MDN ARIA roles | https://developer.mozilla.org/docs/Web/Accessibility/ARIA/Roles |
| CSS isolation in Blazor | https://learn.microsoft.com/aspnet/core/blazor/components/css-isolation |
| CSS custom properties (MDN) | https://developer.mozilla.org/docs/Web/CSS/Using_CSS_custom_properties |
| Container queries (MDN) | https://developer.mozilla.org/docs/Web/CSS/CSS_containment/Container_queries |
| CSS Cascade Layers | https://developer.mozilla.org/docs/Web/CSS/@layer |
| `:has()` selector | https://developer.mozilla.org/docs/Web/CSS/:has |
| HTML Living Standard | https://html.spec.whatwg.org/multipage/ |
| Can I Use (browser support) | https://caniuse.com |

