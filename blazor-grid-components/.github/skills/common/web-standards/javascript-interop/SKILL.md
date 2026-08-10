---
name: javascript-interop
description: Mandated skill covering safe and performant JS interop patterns in Blazor — including render-mode constraints, module-based isolation, IJSObjectReference lifecycle, DotNetObjectReference callbacks, import maps, service abstraction, and data marshaling for .NET 8 and .NET 10.
metadata:
  category: Interop
  tags: [javascript, ijsruntime, js-modules, ijsobjectreference, dotnet-object-reference, interop, wasm, browser-apis, import-map, jsobjectreference, net10]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Interactive Server", "Interactive WASM", "Auto", "Blazor Hybrid"]
  csharp: ["C# 12", "C# 13", "C# 14"]
  note: "JS interop is NOT available during Static SSR or pre-rendering. Guard all calls with OnAfterRenderAsync(firstRender)."
---

# Skill 05 — JavaScript Interop (IJSRuntime) (Mandated)

> **Applies to:** .NET 8 (LTS) · .NET 10 (LTS)
> **Proficiency required:** Intermediate
> **Last reviewed:** April 2026
> **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability

---

## Summary

JavaScript interop remains a core integration point for browser APIs and existing JS libraries. In .NET 8/10, JS interop must be used safely within the correct render modes (JS interop is unavailable in Static SSR), and should leverage the module-based isolation model introduced in .NET 5 and now fully established as the standard.

---

## Key Competencies

### Render Mode Constraints
- **JS interop is not available during Static SSR pre-rendering.** Guard all interop calls with `OnAfterRender[Async]` or check for an interactive render mode.
- Call `IJSRuntime.InvokeAsync` only after the component has reached the browser (first render is complete).
- For components that must work in SSR, provide a graceful no-op fallback for JS-dependent features.

### Module-Based Isolation (.NET 5+, now standard)
- Use `IJSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/module.js")` to load isolated JS modules.
- Maintain module references as `IJSObjectReference` fields; dispose them in `DisposeAsync`.
- This prevents global namespace pollution and enables tree-shaking in WASM bundles.

```csharp
// Standard pattern — module loading with cleanup
private IJSObjectReference? _module;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _module = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./_content/MyLib/js/my-component.js");
    }
}

public async ValueTask DisposeAsync()
{
    if (_module is not null)
        await _module.DisposeAsync();
}
```

### IJSRuntime vs IJSInProcessRuntime
- Use `IJSRuntime` (async) in all general-purpose code.
- Cast to `IJSInProcessRuntime` only when running on WASM and when synchronous performance is critical; document the fallback for Server mode.
- In .NET 8+, prefer `IJSRuntime` even in WASM to keep code portable across modes.

### Data Marshaling & Serialization
- Use `System.Text.Json`-serializable types for parameters and return values.
- Avoid sending large object graphs across the boundary; prefer IDs and fetch data on the JS side.
- Use `DotNetObjectReference<T>` to pass .NET callbacks to JS; always dispose after use.

```csharp
// Passing .NET callback to JS
var dotNetRef = DotNetObjectReference.Create(this);
await JS.InvokeVoidAsync("registerCallback", dotNetRef);
// Dispose in DisposeAsync
```

### Service Abstraction Pattern
- Wrap all `IJSRuntime` calls in a typed service (e.g., `IClipboardService`, `IResizeObserverService`).
- Components must **not** call `IJSRuntime` directly; use the injected service abstraction.
- This allows unit testing with mock implementations and isolates browser dependency.

### Error Handling
- `InvokeAsync` throws `JSException` on JavaScript errors; always catch and log with `ILogger<T>`.
- Use `CancellationToken` overloads for interop that may time out (e.g., large file operations, clipboard API).
- Catch `JSDisconnectedException` in `DisposeAsync` — circuit may disconnect before dispose completes.

### Import Maps (.NET 10)

.NET 10 Blazor Web Apps support **import maps** for managing JavaScript module specifiers without bundlers:

```html
<!-- In App.razor or _Host.cshtml — define import map -->
<script type="importmap">
{
  "imports": {
    "chart.js": "/_content/MyLib/js/chart.esm.min.js",
    "@shoelace-style/shoelace": "https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2/cdn/shoelace.js"
  }
}
</script>
```

```javascript
// JS module can now use bare specifiers
import Chart from 'chart.js';
```

> **Reference:** https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0#blazor

### .NET 10 JS Interop Improvements

| Improvement | Detail |
|---|---|
| **`[JSExportAttribute]`** | Export .NET methods to JS without `DotNetObjectReference` — cleaner pattern for WASM |
| **`IJSObjectReference.InvokeAsync<JSObject>`** | Pass JS object handles without serialization — avoids large marshaling overhead |
| **`JSImportAttribute` / `JSExportAttribute`** | Native JS interop in WASM AOT; avoids `IJSRuntime` overhead for hot paths |

```csharp
// .NET WASM AOT — native JS interop (no IJSRuntime, no serialization overhead)
[JSImport("globalThis.console.log")]
internal static partial void ConsoleLog(string message);

[JSExport]
internal static int Add(int a, int b) => a + b;
```

> Use `[JSImport]`/`[JSExport]` only in Blazor WASM with AOT — not available in Interactive Server.

---

## Measured Standards (Mandatory)

- [ ] No direct `IJSRuntime` calls in component `.razor` files; interop is wrapped in injected typed services.
- [ ] All `IJSObjectReference` instances disposed in `DisposeAsync` / `IAsyncDisposable`.
- [ ] Components that use JS interop explicitly state `⚠ Requires interactive render mode` in their XML docs and README.
- [ ] `IJSInProcessRuntime` usage documented with fallback path for Blazor Server.
- [ ] All JS interop guarded with `firstRender` check in `OnAfterRenderAsync`.
- [ ] `JSDisconnectedException` caught in `DisposeAsync`.
- [ ] JS modules follow `_content/<LibraryName>/js/<module>.js` static asset path convention.

---

## Minimal Artifacts (Required)

- [ ] A typed JS interop service interface + implementation with `IAsyncDisposable`, and a unit test using a mock `IJSRuntime`.
- [ ] Corresponding JS module file following the `_content/LibName/` static asset convention.
- [ ] XML `<remarks>` on the component stating the interactive render mode requirement.

---

## Resources

| Topic | URL |
|---|---|
| JS interop in Blazor | https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability |
| Call .NET from JS | https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript |
| Static assets in class libraries | https://learn.microsoft.com/aspnet/core/razor-pages/ui-class |
| JSImport / JSExport (WASM) | https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/import-export-interop |
| Import maps (.NET 10) | https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0#blazor |

