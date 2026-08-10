---
name: csharp-dotnet
description: Mandated skill covering modern C# language features (C# 12–14), async/await patterns, source generators, nullable reference types, NuGet packaging, Central Package Management, SDK-style projects, and multi-targeting for .NET 8 and .NET 10 LTS Blazor library development.
metadata:
  category: Language & Runtime
  tags: [csharp, dotnet, async-await, nullable, records, generics, nuget, msbuild, sdk-style, source-generators, interceptors, net10]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Static SSR", "Interactive Server", "Interactive WASM", "Auto", "Blazor Hybrid"]
  csharp: ["C# 12", "C# 13", "C# 14"]
---

# Skill 02 — C# and .NET Fundamentals (Mandated)

> **Applies to:** C# 12 (.NET 8 LTS) · C# 14 (.NET 10 LTS)
> **Proficiency required:** Intermediate → Advanced
> **Last reviewed:** April 2026
> **Microsoft Docs:** https://learn.microsoft.com/dotnet/csharp/whats-new/

---

## Summary

Strong, idiomatic C# and modern .NET skills are mandatory for building maintainable Blazor components and class libraries. Developers must keep pace with language and runtime evolution across .NET 8 (LTS) and .NET 10 (LTS), using new features where they improve clarity, safety, or performance.

---

## Key Competencies

### Modern C# Language Features

**C# 12 (.NET 8 LTS)**
- **Primary constructors** on classes and structs — simplify DI-constructor boilerplate
- **Collection expressions** (`[1, 2, 3]`, spread operator `..`) — unified syntax for arrays, lists, spans
- **`ref readonly` parameters** — pass large structs without copy, enforce read-only
- **Inline arrays** — fixed-size arrays on stack for performance-critical code
- **`nameof` in attributes** — avoids magic strings in custom attributes
- **`using` alias for any type** — alias tuples, arrays, and pointer types

**C# 13 (.NET 9)**
- **`params` collections** — `params IEnumerable<T>`, `params Span<T>` overloads
- **`field` keyword** — access backing field inside property accessor without naming it
- **`Lock` type** — new `System.Threading.Lock` replacing `lock(object)` for better performance
- **`allows ref struct`** anti-constraint — enables `ref struct` types in generic APIs

**C# 14 (.NET 10 LTS)**
- **Extension members** — declare static and instance extension methods/properties in `extension` blocks; cleaner API surface
- **`field` keyword GA** — fully stable backing-field access in property accessors
- **`params Span<T>`** — zero-allocation variadic parameters in hot paths
- **Implicit span conversions** — simplified buffer management APIs

**Cross-version fundamentals:**
- Nullable reference types — enable `<Nullable>enable</Nullable>` in all projects; use `?` annotations and null-forgiving operator only where justified with comment.
- Records, init-only properties, and `with` expressions for immutable DTOs/value objects.
- Pattern matching: `switch` expressions, positional, list, property, and `not`/`and`/`or` patterns.
- Source generators — understand `IIncrementalGenerator`; use for code generation over reflection.

### Async & Task-Based Programming
- All public async APIs must return `Task` or `Task<T>` and carry the `Async` suffix.
- Use `ValueTask<T>` for hot paths where synchronous completion is common.
- Apply `ConfigureAwait(false)` in library (non-UI) code to avoid deadlocks.
- Never use `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` in library code. If a sync entry-point is unavoidable, document it explicitly and isolate it.
- Use `CancellationToken` in all I/O-bound APIs; propagate tokens through the call chain.

### Generics, Delegates & LINQ
- Write generic components and utilities using constraints (`where T : class`, `where T : notnull`).
- Prefer `Func<T>` / `Action<T>` over custom delegate types for callbacks.
- Use LINQ with care in render paths; prefer `foreach` or pooled collections for hot loops.

### Exception Handling & Logging
- Use structured logging with `ILogger<T>` (Microsoft.Extensions.Logging); never use `Console.WriteLine` in library code.
- Catch specific exceptions; avoid empty `catch` blocks.
- Use `ExceptionDispatchInfo` for re-throwing without losing stack traces.
- Integrate OpenTelemetry (`ActivitySource`) for distributed tracing in .NET 8+.

### NuGet Packaging & Versioning
- Use `<Version>`, `<AssemblyVersion>`, and `<FileVersion>` in `.csproj`.
- Follow Semantic Versioning 2.0 (SemVer); document breaking changes in CHANGELOG.
- Use `<PackageValidationBaselineVersion>` (NuGet package validation) to detect API breaking changes automatically in CI.
- Multi-target with `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` where necessary; use conditional compilation `#if NET10_0_OR_GREATER`.

### Project & Build System
- SDK-style `.csproj` projects; understand `<ItemGroup>`, `<PropertyGroup>`, `<Import>`, and MSBuild tasks.
- Use **Central Package Management** (`Directory.Packages.props` + `Directory.Build.props`) for consistent NuGet versions and shared build settings across all projects in a repo.
- Use `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to enforce zero-warning builds in CI.
- Enable `.editorconfig` and `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` for consistent formatting.
- Use **NuGet lock files** (`RestorePackagesWithLockFile=true`) for reproducible CI restores.
- Enable **Roslyn analyzers** for security and reliability: `Microsoft.CodeAnalysis.NetAnalyzers` (built-in .NET 8+), `StyleCop.Analyzers`, `SonarAnalyzer.CSharp`.

### Source Generators & Roslyn

- Prefer **incremental source generators** (`IIncrementalGenerator`) over reflection for compile-time code generation — avoids runtime overhead.
- Use `[GeneratedRegex]` (built-in .NET 7+) for regex compilation.
- Use `JsonSerializerContext` with `[JsonSerializable]` for AOT-compatible JSON serialization (required for Native AOT and WASM AOT).

```csharp
// AOT-safe JSON serialization — required for Blazor WASM AOT / Native AOT
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
internal partial class AppJsonContext : JsonSerializerContext { }

// Register in Program.cs
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));
```

---

## Measured Standards (Mandatory)

- [ ] All public async APIs return `Task`/`Task<T>`/`ValueTask<T>` and use `Async` suffix — enforced by Roslyn analyzer.
- [ ] Nullable reference types enabled on all projects; zero `#nullable disable` suppressions without documented justification.
- [ ] All new public members carry complete XML documentation (`<summary>`, `<param>`, `<returns>`, `<remarks>`, `<example>`).
- [ ] No `.Result`/`.Wait()` in library code — CA2007/ConfigureAwait analyzer must be green.
- [ ] Multi-targeted projects must compile and test successfully on both `net8.0` and `net10.0` in CI.
- [ ] `PackageValidationBaselineVersion` configured for any published NuGet package.

---

## Minimal Artifacts (Required)

- [ ] Library project targeting `net8.0;net10.0` with nullable enabled, XML docs generated (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`).
- [ ] A sample `dotnet pack` CI step producing a versioned `.nupkg`.
- [ ] At least one test exercising async cancellation behavior.

---

## Resources

| Topic | URL |
|---|---|
| C# 12 What's New | https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-12 |
| C# 13 What's New | https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-13 |
| C# 14 What's New | https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14 |
| .NET async in-depth | https://learn.microsoft.com/dotnet/standard/async-in-depth |
| NuGet Package Validation | https://learn.microsoft.com/nuget/package-validation |
| Central Package Management | https://learn.microsoft.com/nuget/consume-packages/central-package-management |
| Source generators (incremental) | https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview |
| AOT-safe JSON serialization | https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation |
| NuGet lock files | https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#locking-dependencies |
| .NET 10 release notes | https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview |

