# Skills — Developer Competency Library

## Overview

This folder contains the mandated skill definitions and standards for developers working on the Syncfusion ASP.NET Core Blazor component library. Each skill folder contains a single `SKILL.md` file that defines required competencies, measurable standards, minimal acceptance artifacts, and suggested resources.

Every `SKILL.md` file includes a YAML front-matter header with:
- **`name`** — human-readable skill name
- **`description`** — short summary of what the skill covers
- **`metadata`** — structured metadata block:
  - `category` — skill category (e.g., Core Framework, Security, Testing)
  - `tags` — list of searchable keywords for this skill
- **`compatibility`** — compatibility block:
  - `dotnet` — supported .NET runtime versions
  - `blazor_modes` — Blazor render modes this skill applies to
  - `csharp` — C# language versions covered
  - `note` *(optional)* — important compatibility caveat

## How to Use

- Each skill file includes: Summary, Key Competencies, Measured Standards, Minimal Artifacts, and Resources.
- Team leads should require contributors to meet the **Measured Standards** and attach the **Minimal Artifacts** to PRs for new components or major changes.
- Skill IDs are used in PR checklists and code review templates to reference specific competency requirements.

## Skill Index

| # | Folder | Skill Name | Category | Tags (excerpt) |
|---|--------|------------|----------|----------------|
| 01 | [blazor-framework/SKILL.md](blazor-framework/SKILL.md) | blazor-framework | Core Framework | render-modes, routing, state-management, forms, ssr, aspire |
| 02 | [csharp-dotnet/SKILL.md](csharp-dotnet/SKILL.md) | csharp-dotnet | Language & Runtime | csharp, async-await, nullable, nuget, sdk-style, source-generators |
| 03 | [razor-components-design/SKILL.md](razor-components-design/SKILL.md) | razor-components-design | Component Design | razor, parameters, event-callback, generics, xml-docs, quickgrid |
| 04 | [html-css/SKILL.md](html-css/SKILL.md) | html-css | Frontend | html, css-isolation, css-variables, responsive, theming, container-queries |
| 05 | [javascript-interop/SKILL.md](javascript-interop/SKILL.md) | javascript-interop | Interop | ijsruntime, js-modules, interop, wasm, browser-apis, import-map |
| 06 | [dotnet-security/SKILL.md](dotnet-security/SKILL.md) | dotnet-security | Security | authentication, anti-forgery, xss, csrf, csp, nuget-audit, entra-id |
| 07 | [testing/SKILL.md](testing/SKILL.md) | testing | Quality & Testing | bunit, playwright, xunit, ci-cd, coverage, code-analysis, mutation |
| 08 | [accessibility-requirements/SKILL.md](accessibility-requirements/SKILL.md) | accessibility-requirements | Accessibility | wcag, aria, keyboard-navigation, axe-core, focus-management, wcag-2.2 |
| 09 | [performance-optimization/SKILL.md](performance-optimization/SKILL.md) | performance-optimization | Performance | virtualize, aot, native-aot, should-render, benchmarkdotnet, lighthouse |
| 10 | [deployment/SKILL.md](deployment/SKILL.md) | deployment | Deployment & Infrastructure | docker, azure, aspire, health-checks, opentelemetry, aca, ci-cd |
| 11 | [blazor-lifecycle/SKILL.md](blazor-lifecycle/SKILL.md) | blazor-lifecycle | Core Framework | lifecycle, hooks, OnInitialized, OnAfterRender, DisposeAsync |
| 12 | [blazor-memory-error-handling/SKILL.md](blazor-memory-error-handling/SKILL.md) | blazor-memory-error-handling | Core Framework | memory, disposal, iasyncdisposable, subscriptions, error-handling |
| 13 | [javascript-memory-error-handling/SKILL.md](javascript-memory-error-handling/SKILL.md) | javascript-memory-error-handling | Interop | js, memory-leaks, event-listeners, closures, error-handling |

## Compatibility

All skills target:
- **.NET 8 (LTS)** — minimum supported runtime
- **.NET 10 (LTS)** — current latest LTS runtime (released November 2025)
- **C# 12 – C# 14** — language features covered across both runtimes
- **Blazor Web App** unified hosting model (Static SSR, Interactive Server, Interactive WASM, Auto)
- **Blazor Hybrid** (MAUI / WPF / WinForms) where noted
- **.NET Aspire** — referenced in deployment and observability skills

## Microsoft Documentation Standards

All skills align with canonical Microsoft documentation:
- [ASP.NET Core Blazor docs](https://learn.microsoft.com/aspnet/core/blazor)
- [.NET API Browser](https://learn.microsoft.com/dotnet/api/)
- [.NET Architecture Guides](https://learn.microsoft.com/dotnet/architecture/)
- [Microsoft Security Response Center (MSRC)](https://www.microsoft.com/msrc)
- [OWASP Top 10 for .NET](https://owasp.org/www-project-top-ten/)

## Contributing

When updating skill files:
- Update the YAML front-matter `last_reviewed` date.
- Include references to **canonical Microsoft documentation** (`learn.microsoft.com`) as primary sources.
- Add a minimal example or checklist that can be validated in code reviews.
- Keep `skill_id` values stable — they are referenced in PR templates and agent definitions.
- Test all code snippets against both `net8.0` and `net10.0` before committing.
- Follow [Microsoft Writing Style Guide](https://learn.microsoft.com/style-guide/welcome/) for prose.

