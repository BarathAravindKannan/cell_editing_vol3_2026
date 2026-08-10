---
name: dotnet-security
description: Mandated skill covering authentication and authorization (Microsoft Entra ID / ASP.NET Core Identity), anti-forgery protection, secret management, input validation, XSS/injection prevention, Content Security Policy, transport security, and NuGet supply chain security for Blazor applications on .NET 8 and .NET 10.
metadata:
  category: Security
  tags: [authentication, authorization, entra-id, anti-forgery, xss, csrf, csp, tls, secrets, nuget-audit, oidc, identity, keyvault, owasp]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Static SSR", "Interactive Server", "Interactive WASM", "Auto", "Blazor Hybrid"]
  csharp: ["C# 12", "C# 13", "C# 14"]
---

# Skill 06 — .NET & Blazor Security Best Practices (Mandated)

> **Applies to:** .NET 8 (LTS) · .NET 10 (LTS)
> **Proficiency required:** Intermediate → Advanced
> **Last reviewed:** April 2026
> **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/blazor/security
> **OWASP Top 10:** https://owasp.org/www-project-top-ten/

---

## Summary

Security is non-negotiable. Every PR that touches authentication, authorization, input handling, or data flow must satisfy this checklist. The **secure-by-default** posture introduced in .NET 8 (anti-forgery, enhanced auth, OIDC improvements) must be understood and applied correctly.

---

## Key Competencies

### Authentication & Authorization (.NET 8/10)

#### Microsoft Entra ID (formerly Azure AD) — Recommended for Production
```csharp
// Program.cs — Microsoft Entra ID (OIDC) integration
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy; // require auth everywhere by default
});
```

```json
// appsettings.json — Entra ID configuration (values from Azure portal)
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "CallbackPath": "/signin-oidc"
  }
}
```

**Key rules:**
- Use **Microsoft Entra ID** (via `Microsoft.Identity.Web`) for enterprise and SaaS applications — replaces raw OIDC configuration.
- Understand `.NET 8` cookie-based auth for Blazor Web Apps with `AddAuthentication().AddCookies()` for non-Entra scenarios.
- In .NET 8+, use `AuthenticationStateProvider` and `<AuthorizeView>` / `[Authorize]` attribute in components.
- **Server-side resource authorization must never rely solely on client-side UI hiding.** Always enforce on the server.
- Use `IAuthorizationService` for resource-based authorization in service layer code (not just role checks).
- Apply `[Authorize(Policy = "RequireAdminRole")]` policies; define policies in `AddAuthorization` rather than inline role strings.

### Anti-Forgery (.NET 8)
- Blazor Web Apps auto-generate anti-forgery tokens for SSR forms via `AntiforgeryToken` component.
- Ensure `AddAntiforgery()` is registered and `UseAntiforgery()` is placed in the middleware pipeline.
- API endpoints called from interactive Blazor must validate CSRF tokens or use `SameSite` cookie policy.

### Secret & Configuration Management
- **No secrets in source code.** Use `dotnet user-secrets` for local development.
- In production: Azure Key Vault, AWS Secrets Manager, or environment variable injection via CI.
- Use `Microsoft.Extensions.Configuration` with `IOptions<T>` pattern; avoid `IConfiguration` direct reads in library code.

### Input Validation & Output Encoding
- Validate all inputs server-side regardless of client-side validation.
- Use `MarkupString` only for known-safe HTML content; prefer `@`-encoding for user-supplied text.
- Sanitize rich text input with a server-side HTML sanitizer library (e.g., `HtmlSanitizer`).
- Use `DataAnnotations` and `IValidatableObject` for model validation; supplement with `FluentValidation` where needed.

### XSS, Injection & CSP
- Blazor's `@` binding automatically HTML-encodes output — never bypass with `MarkupString` without review.
- Define a `Content-Security-Policy` header on the server; Blazor Server requires `'unsafe-eval'` only if using dynamic compilation.
- Avoid storing sensitive data in `localStorage` or `sessionStorage`; use HttpOnly cookies.

### Transport Security
- Enforce HTTPS in all environments: `UseHttpsRedirection()` + `UseHsts()`.
- Use `SameSite=Strict` or `SameSite=Lax` on authentication cookies.
- Pin minimum TLS to 1.2; recommend 1.3 in Kestrel configuration.

### Dependency & Supply Chain Security
- Run `dotnet list package --vulnerable` in CI to detect known CVEs.
- Enable `<NuGetAudit>true</NuGetAudit>` and `<NuGetAuditMode>all</NuGetAuditMode>` in `.csproj` or `Directory.Build.props` (.NET 8+).
- Review and pin third-party NuGet versions; use **Central Package Management** (`Directory.Packages.props`).
- Use **GitHub Dependabot** or **Renovate** for automated dependency update PRs.
- Configure `<NuGetAuditLevel>high</NuGetAuditLevel>` to fail CI on high/critical CVEs only (avoid noise from low/medium).
- Sign NuGet packages (`<SignAssembly>`) and verify signatures in CI for internal package feeds.

### OWASP Top 10 — .NET Mitigations

| OWASP Risk | .NET / Blazor Mitigation |
|---|---|
| A01 Broken Access Control | `IAuthorizationService`, server-side checks, never trust client |
| A02 Cryptographic Failures | Use `IDataProtectionProvider`; avoid MD5/SHA1; enforce TLS 1.2+ |
| A03 Injection | Parameterized queries (EF Core); never raw SQL with user input |
| A05 Security Misconfiguration | `UseHsts()`, `UseHttpsRedirection()`, minimal exposed ports |
| A06 Vulnerable Components | `NuGetAudit`, `dotnet list package --vulnerable` in CI |
| A07 Identity/Auth Failures | Microsoft Entra ID / ASP.NET Core Identity; MFA; short-lived tokens |
| A09 Logging & Monitoring | `ILogger<T>` + Application Insights / OpenTelemetry; never log PII |

---

## Measured Standards (Mandatory)

- [ ] No secrets in source code; verified by a secrets-scanning step (e.g., `gitleaks`) in CI.
- [ ] `NuGetAudit` enabled and CI fails on high/critical CVEs.
- [ ] `<AuthorizeView>` used for UI gating; independent server-side policy checks present for all state-changing operations.
- [ ] Anti-forgery middleware registered and verified for all SSR form endpoints.
- [ ] `MarkupString` usage requires code review sign-off and a comment explaining the sanitization applied.
- [ ] HTTPS enforced; HSTS configured for production deployments.

---

## Minimal Artifacts (Required)

- [ ] Threat checklist in PR description for features touching auth, data, or external integrations.
- [ ] Evidence of `dotnet list package --vulnerable` run clean (or documented exceptions) before release.

---

## Resources

| Topic | URL |
|---|---|
| ASP.NET Core security overview | https://learn.microsoft.com/aspnet/core/security |
| Blazor authentication & authorization | https://learn.microsoft.com/aspnet/core/blazor/security |
| Anti-forgery in .NET 8 | https://learn.microsoft.com/aspnet/core/blazor/forms#antiforgery-support |
| NuGet audit (.NET 8) | https://learn.microsoft.com/nuget/concepts/auditing-packages |
| Microsoft Entra ID + ASP.NET Core | https://learn.microsoft.com/entra/identity-platform/quickstart-v2-aspnet-core-webapp |
| Microsoft.Identity.Web | https://learn.microsoft.com/azure/active-directory/develop/microsoft-identity-web |
| Data Protection API | https://learn.microsoft.com/aspnet/core/security/data-protection/introduction |
| ASP.NET Core Authorization | https://learn.microsoft.com/aspnet/core/security/authorization/introduction |
| OWASP .NET Security Cheat Sheet | https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html |
| Azure Key Vault + .NET | https://learn.microsoft.com/azure/key-vault/secrets/quick-create-net |

