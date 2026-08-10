---
name: accessibility-requirements
description: Mandated skill covering WCAG 2.2 Level AA compliance, semantic HTML5, ARIA patterns, keyboard interaction, focus management, and automated accessibility testing for Blazor UI components across all render modes in .NET 8 and .NET 10.
metadata:
  category: Accessibility
  tags: [wcag, wcag-2.2, aria, apg, keyboard-navigation, focus-management, axe-core, screen-reader, color-contrast, reduced-motion, target-size, dragging]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Static SSR", "Stream Rendering", "Interactive Server", "Interactive WASM", "Auto", "Blazor Hybrid"]
  csharp: ["C# 12", "C# 13", "C# 14"]
---

# Skill 08 — Accessibility (a11y) Requirements (Mandated)

> **Applies to:** .NET 8 (LTS) · .NET 10 (LTS)
> **Proficiency required:** Intermediate
> **Last reviewed:** April 2026
> **Standard:** WCAG 2.2 Level AA (W3C Recommendation — October 2023)
> **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/blazor/components/focus

---

## Summary

All UI components must meet **WCAG 2.2 Level AA** at minimum. Accessibility is a first-class requirement, not a post-release fix. .NET 8 Blazor Static SSR pages must be accessible without JavaScript; interactive components must maintain accessibility across all render modes.

---

## Key Competencies

### WCAG 2.2 Principles (POUR)

| Principle | Requirement |
|---|---|
| **Perceivable** | Text alternatives, captions, adaptable content, sufficient contrast (≥ 4.5:1 normal text, ≥ 3:1 large) |
| **Operable** | Full keyboard access, no keyboard traps, sufficient time, no seizure-triggering content |
| **Understandable** | Readable, predictable, input assistance (labels, error descriptions) |
| **Robust** | Compatible with current assistive technologies and browsers |

### WCAG 2.2 New Success Criteria (October 2023)

| SC | Level | Requirement |
|---|---|---|
| **2.4.11 Focus Appearance** | AA | Keyboard focus indicator must have ≥ 2px outline and sufficient contrast |
| **2.4.12 Focus Appearance (Enhanced)** | AAA | Larger focus indicator requirements |
| **2.5.3 Label in Name** | A | Accessible name must contain visible label text |
| **2.5.7 Dragging Movements** | AA | Provide single-pointer alternative for all drag operations |
| **2.5.8 Target Size (Minimum)** | AA | Interactive targets ≥ 24×24 CSS pixels (or sufficient spacing) |
| **3.2.6 Consistent Help** | A | Help mechanisms in consistent location across pages |
| **3.3.7 Redundant Entry** | A | Don't require users to re-enter info already provided in the same session |
| **3.3.8 Accessible Authentication** | AA | Don't require cognitive function tests (e.g., CAPTCHAs) without accessible alternative |

> **All new 2.2 AA criteria are mandatory** for components released after October 2023.

### Semantic HTML (foundation)
- Use native HTML semantics first before adding ARIA; native elements have built-in accessibility.
- Never use `<div>`/`<span>` as buttons, checkboxes, or interactive controls.
- Use `<fieldset>` + `<legend>` for groups of related form controls.

### ARIA (when native HTML is insufficient)
- Follow the **First Rule of ARIA**: if a native element can be used, use it.
- Required ARIA attributes: `role`, `aria-label`/`aria-labelledby`, `aria-describedby`, `aria-expanded`, `aria-controls`, `aria-selected`, `aria-checked`.
- Use `aria-live="polite"` for async updates (data loaded, success messages); `aria-live="assertive"` only for critical alerts.
- In Blazor, use `@ref` + `ElementReference.FocusAsync()` to programmatically manage focus on navigation or dialog open/close.

### Keyboard Interaction Standards
- All interactive elements reachable via `Tab`/`Shift+Tab`.
- Dropdown/list components follow ARIA Authoring Practices Guide (APG) keyboard patterns (e.g., `ArrowUp/Down` in listboxes).
- Modal dialogs must trap focus inside; restore focus to trigger element on close.
- `Escape` must close modals, dropdowns, and tooltips.

### Focus Management in Blazor
- After dynamic content updates (e.g., dialog open), use `ElementReference.FocusAsync()`.
- In Static SSR + Enhanced Navigation, verify focus position after navigation — Blazor Web App `.NET 8` manages focus for enhanced navigation by default.
- Use `tabindex="0"` for custom focusable elements; never use `tabindex` > 0.

### Accessible Color & Motion
- Minimum contrast ratios: 4.5:1 (normal text), 3:1 (large text ≥ 18pt or ≥ 14pt bold, UI components, graphical objects).
- Respect `prefers-reduced-motion`: disable or reduce animations for users who opt out.

---

## Measured Standards (Mandatory)

- [ ] Components pass `axe-core` automated scan with **zero critical or serious violations** — run in CI via Playwright + `@axe-core/playwright`.
- [ ] Manual keyboard navigation verified: every interactive control reachable and operable by keyboard alone.
- [ ] Color contrast ratios verified with browser DevTools or Lighthouse for all text and UI components.
- [ ] Component README includes an **Accessibility** section describing keyboard behavior, ARIA attributes used, and known screen reader considerations.
- [ ] `prefers-reduced-motion` respected in all CSS animations.
- [ ] Focus management verified for dialogs, dropdowns, and dynamic content changes.

---

## Minimal Artifacts (Required for New Components)

- [ ] `axe` Playwright test or axe-cli report attached to PR.
- [ ] Accessibility section in component README or XML docs.

---

## Measured Standards (Additional — WCAG 2.2)

- [ ] Interactive targets ≥ 24×24 CSS pixels (SC 2.5.8 — WCAG 2.2 AA).
- [ ] Keyboard focus indicator has ≥ 2px outline with sufficient contrast (SC 2.4.11 — WCAG 2.2 AA).
- [ ] Drag operations have a single-pointer alternative (SC 2.5.7 — WCAG 2.2 AA).
- [ ] Forms do not require users to re-enter information already submitted in the same session (SC 3.3.7).
- [ ] Authentication flows provide accessible alternative to cognitive function tests (SC 3.3.8).

---

## Resources

| Topic | URL |
|---|---|
| WCAG 2.2 guidelines | https://www.w3.org/TR/WCAG22 |
| WCAG 2.2 What's New | https://www.w3.org/WAI/standards-guidelines/wcag/new-in-22/ |
| ARIA Authoring Practices Guide (APG) | https://www.w3.org/WAI/ARIA/apg |
| axe-core | https://github.com/dequelabs/axe-core |
| Playwright axe integration | https://playwright.dev/docs/accessibility-testing |
| Blazor focus management | https://learn.microsoft.com/aspnet/core/blazor/components/focus |
| Microsoft Accessibility Insights | https://accessibilityinsights.io |
| MDN ARIA reference | https://developer.mozilla.org/docs/Web/Accessibility/ARIA |

