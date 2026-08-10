---
name: blazor-memory-error-handling
description: "Mandated skill covering IDisposable/IAsyncDisposable patterns, memory leak prevention, resource cleanup, event subscription management, CancellationToken propagation, and error handling strategies in Blazor components for .NET 8 and .NET 10."
metadata:
  category: Core Framework
  tags: [blazor, memory-leaks, disposal, idisposable, iasyncdisposable, subscriptions, error-handling, resource-cleanup, cancellationtoken, errorboundary]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  blazor_modes: ["Interactive Server", "Interactive WASM", "Blazor Hybrid"]
  note: "CRITICAL for Blazor Server — improper disposal causes memory leaks that accumulate over circuit lifetime"
---

# Skill: Blazor Memory Management & Error Handling (Mandated)

> **Applies to:** .NET 8 (LTS) · .NET 10 (LTS)
> **Proficiency required:** Intermediate
> **Criticality:** CRITICAL — Blazor Server memory leaks accumulate over circuit lifetime
> **Last reviewed:** April 2026
> **Microsoft Docs:** https://learn.microsoft.com/aspnet/core/blazor/components/lifecycle#component-disposal-with-idisposable-and-iasyncdisposable

---

## Summary

Blazor components must implement proper disposal to prevent memory leaks in long-running applications. In Blazor Server, a single component instance can live for the entire user session; improper cleanup causes persistent memory growth. This skill covers IDisposable/IAsyncDisposable patterns, resource management, event subscription cleanup, and error handling strategies.

---

## Part 1: IDisposable & IAsyncDisposable Patterns

### When to Use Which Interface

| Scenario | Interface | Method | Notes |
|----------|-----------|--------|-------|
| Synchronous cleanup only (timers, collections) | `IDisposable` | `Dispose()` | Simple pattern, not recommended for components |
| Async cleanup (JS modules, network) | `IAsyncDisposable` | `DisposeAsync()` | **Standard for Blazor components** |
| Both sync and async cleanup | Both (see pattern below) | Both methods | Rare; usually prefer `IAsyncDisposable` |

### Pattern 1: Basic IAsyncDisposable (Recommended)

```csharp
public partial class MyComponent : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private IDisposable? _subscription;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load JS module
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./my-component.js");
            
            // Subscribe to event bus
            _subscription = EventBus?.Subscribe<MyEvent>(OnEventRaised);
        }
    }
    
    private void OnEventRaised(MyEvent args) { /* handle */ }
    
    // ✅ CORRECT: Implement IAsyncDisposable
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // 1. Dispose JS module
        if (_module is not null)
            await _module.DisposeAsync();
        
        // 2. Unsubscribe from events
        _subscription?.Dispose();
    }
}
```

### Pattern 2: Disposables Collection (Multiple Resources)

```csharp
public partial class ComplexComponent : ComponentBase, IAsyncDisposable
{
    [Inject]
    public required IJSRuntime JS { get; set; }
    
    private List<IAsyncDisposable> _asyncDisposables = new();
    private List<IDisposable> _syncDisposables = new();
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load multiple modules
            var module1 = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module1.js");
            _asyncDisposables.Add(module1);
            
            var module2 = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module2.js");
            _asyncDisposables.Add(module2);
            
            // Add event subscriptions
            _syncDisposables.Add(EventBus?.Subscribe<Event1>(Handle1));
            _syncDisposables.Add(EventBus?.Subscribe<Event2>(Handle2));
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Dispose async first
        foreach (var disposable in _asyncDisposables)
        {
            await disposable.DisposeAsync();
        }
        
        // Then sync
        foreach (var disposable in _syncDisposables)
        {
            disposable?.Dispose();
        }
        
        _asyncDisposables.Clear();
        _syncDisposables.Clear();
    }
}
```

### Pattern 3: With Synchronous Cleanup (Both Interfaces)

```csharp
public partial class HybridComponent : ComponentBase, IDisposable, IAsyncDisposable
{
    private Timer? _timer;
    private IJSObjectReference? _module;
    private Cache? _cache;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Async: JS module
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
            
            // Sync: Timer
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            // Sync: Cache
            _cache = new Cache();
        }
    }
    
    private void DoWork(object? state) { /* work */ }
    
    // Synchronous cleanup
    void IDisposable.Dispose()
    {
        _timer?.Dispose();
        _cache?.Clear();
    }
    
    // Asynchronous cleanup
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Call sync cleanup first (if using IDisposable)
        ((IDisposable)this).Dispose();
        
        // Then async cleanup
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
```

### Pattern 4: Safe Guard for JSDisconnectedException

```csharp
public partial class SafeJsComponent : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is null)
            return;
        
        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // JS context already gone (Blazor Server disconnect)
            // Safe to ignore
            Console.WriteLine("JS context disconnected during dispose");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error disposing module: {ex.Message}");
            // Log and continue; don't rethrow
        }
    }
}
```

---

## Part 2: Memory Leak Prevention

### Leak #1: Event Subscriptions Not Unsubscribed

**❌ BAD — Memory leak**:
```csharp
public partial class EventSubscriberBad : ComponentBase
{
    [Inject] private IEventBus? EventBus { get; set; }
    
    protected override void OnInitialized()
    {
        // Subscribed but never unsubscribed!
        EventBus?.Subscribe<MyEvent>(Handle);
    }
    
    private void Handle(MyEvent e) { }
}
```

**✅ GOOD — Proper cleanup**:
```csharp
public partial class EventSubscriber : ComponentBase, IAsyncDisposable
{
    [Inject] private IEventBus? EventBus { get; set; }
    
    private IDisposable? _subscription;
    
    protected override void OnInitialized()
    {
        // Store subscription for cleanup
        _subscription = EventBus?.Subscribe<MyEvent>(Handle);
    }
    
    private void Handle(MyEvent e) { }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Unsubscribe
        _subscription?.Dispose();
        await ValueTask.CompletedTask;
    }
}
```

### Leak #2: JS Objects Not Disposed

**❌ BAD — JS module reference leaked**:
```csharp
public partial class JsComponentBad : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Module loaded but never disposed!
            var module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
            await module.InvokeVoidAsync("init");
        }
    }
}
```

**✅ GOOD — Proper disposal**:
```csharp
public partial class JsComponent : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    private IJSObjectReference? _module;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
            await _module.InvokeVoidAsync("init");
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
```

### Leak #3: DotNetObjectReference Not Disposed

**❌ BAD — .NET reference held forever**:
```csharp
public partial class CallbackComponentBad : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
            
            // ❌ LEAK: DotNetObjectReference never disposed!
            var dotNetRef = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("registerCallback", dotNetRef);
        }
    }
    
    [JSInvokable]
    public Task OnJsCallback(string data) 
    { 
        Console.WriteLine($"JS called: {data}");
        return Task.CompletedTask;
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
        // ❌ MISSING: dotNetRef.Dispose()
    }
}
```

**✅ GOOD — Reference cleaned up**:
```csharp
public partial class CallbackComponent : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<CallbackComponent>? _dotNetRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./module.js");
            
            // ✅ Store reference for cleanup
            _dotNetRef = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("registerCallback", _dotNetRef);
        }
    }
    
    [JSInvokable]
    public Task OnJsCallback(string data) 
    { 
        Console.WriteLine($"JS called: {data}");
        return Task.CompletedTask;
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // 1. Dispose .NET reference
        _dotNetRef?.Dispose();
        
        // 2. Dispose JS module
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
```

### Leak #4: Cascading Parameters Not Cleared

**❌ BAD — Large reference kept in memory**:
```csharp
public partial class ChildComponentBad : ComponentBase
{
    [CascadingParameter] public LargeDataModel? LargeData { get; set; }
    
    private ProcessedData _processed = null!;
    
    protected override void OnInitialized()
    {
        // Keeps reference to LargeData even after component removed
        _processed = ProcessData(LargeData);
    }
}
```

**✅ GOOD — Reference released**:
```csharp
public partial class ChildComponent : ComponentBase, IAsyncDisposable
{
    [CascadingParameter] public LargeDataModel? LargeData { get; set; }
    
    private ProcessedData? _processed;
    
    protected override void OnInitialized()
    {
        _processed = ProcessData(LargeData);
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Clear reference
        _processed = null;
        await ValueTask.CompletedTask;
    }
}
```

---

## Part 3: Error Handling Patterns

### Pattern 1: Try-Catch in Lifecycle Hooks

```csharp
public partial class SafeComponent : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    private IJSObjectReference? _module;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _module = await JS.InvokeAsync<IJSObjectReference>(
                    "import", "./module.js");
            }
            catch (JSException ex)
            {
                Console.Error.WriteLine($"JS import failed: {ex.Message}");
                // Gracefully degrade; component works without JS
                _module = null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error in OnAfterRender: {ex}");
                // Log but don't crash component
            }
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error disposing module: {ex.Message}");
                // Don't rethrow from DisposeAsync
            }
        }
    }
}
```

### Pattern 2: Error Handling During JS Interop

```csharp
public partial class JsCallComponent : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    public async Task SafeInvoke(IJSObjectReference module, string method)
    {
        try
        {
            await module.InvokeVoidAsync(method);
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server: JS context lost (navigation, reconnect failure)
            Console.WriteLine("JS connection lost");
        }
        catch (JSException ex)
        {
            // JavaScript threw an error
            Console.Error.WriteLine($"JS error: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            // Timeout or cancellation
            Console.Error.WriteLine($"Operation cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Other errors
            Console.Error.WriteLine($"Unexpected error: {ex}");
        }
    }
}
```

### Pattern 3: Catch JSException in DisposeAsync

```csharp
public partial class RobustComponent : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is null)
            return;
        
        try
        {
            // JS context might be gone (Blazor Server reconnect)
            await _module.DisposeAsync();
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine($"JSException in DisposeAsync (expected): {ex.Message}");
        }
        catch (JSDisconnectedException ex)
        {
            Console.Error.WriteLine($"JS already disconnected: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log unexpected errors but don't rethrow
            Console.Error.WriteLine($"Error disposing module: {ex}");
        }
    }
}
```

### Pattern 4: Cascading Error Boundary

```csharp
<!-- ErrorBoundary wraps components that might error -->
<ErrorBoundary>
    <ChildComponent />
</ErrorBoundary>

@code {
    // In parent component
}
```

**In child component**:
```csharp
public partial class ChildWithErrorHandling : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    public async Task DoWork()
    {
        try
        {
            // Risky operation
            await JS.InvokeVoidAsync("riskyOperation");
        }
        catch (JSException ex)
        {
            // Re-throw to ErrorBoundary or handle gracefully
            throw new InvalidOperationException($"JS operation failed: {ex.Message}", ex);
        }
    }
}
```

### Pattern 5: Retry Logic with Exponential Backoff

```csharp
public partial class ResilienceComponent : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    private async Task InvokeWithRetry(IJSObjectReference module, 
        string method, int maxRetries = 3, int delayMs = 500)
    {
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await module.InvokeVoidAsync(method);
                Console.WriteLine($"Success on attempt {attempt}");
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt < maxRetries)
                {
                    int delay = delayMs * (int)Math.Pow(2, attempt - 1);
                    Console.WriteLine($"Attempt {attempt} failed, retrying after {delay}ms: {ex.Message}");
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new InvalidOperationException(
            $"Failed after {maxRetries} attempts: {lastException?.Message}", lastException);
    }
}
```

---

## Part 4: Common Patterns & Anti-Patterns

### Anti-Pattern 1: No Disposal

```csharp
// ❌ WRONG
public partial class BadComponent : ComponentBase
{
    private IJSObjectReference? _module;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./module.js");
    }
    
    // NO DisposeAsync implementation!
}
```

### Anti-Pattern 2: Fire-and-forget Tasks

```csharp
// ❌ WRONG
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Fire-and-forget — don't await, don't track
        _ = JS.InvokeVoidAsync("init"); // Error will be silent!
    }
}

// ✅ CORRECT
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        try
        {
            await JS.InvokeVoidAsync("init");
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine($"Init failed: {ex}");
        }
    }
}
```

### Anti-Pattern 3: Synchronous Wait in Async Context

```csharp
// ❌ WRONG — Deadlock risk
protected override void OnInitialized()
{
    var result = JS.InvokeAsync<string>("getValue").Result; // DEADLOCK!
}

// ✅ CORRECT
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var result = await JS.InvokeAsync<string>("getValue");
    }
}
```

---

## Mandatory Standards

### Disposal Implementation
- [ ] All components with JS modules implement `IAsyncDisposable`
- [ ] All JS modules disposed in `DisposeAsync` / `Dispose`
- [ ] All event subscriptions disposed
- [ ] All `DotNetObjectReference` instances disposed
- [ ] No `JSException` rethrown from `DisposeAsync`
- [ ] `JSDisconnectedException` handled gracefully in `DisposeAsync`

### Error Handling
- [ ] All JS interop calls wrapped in try-catch
- [ ] `JSException` caught separately from other exceptions
- [ ] `JSDisconnectedException` handled in dispose
- [ ] Errors logged with context (method, parameters, stack)
- [ ] No fire-and-forget tasks (`_ = ...`)
- [ ] All async operations awaited

### Memory Safety
- [ ] No strong references to large cascading parameters after component disposal
- [ ] Event subscriptions cleaned up via stored `IDisposable` token
- [ ] Timers / `PeriodicTimer` cleared and awaited in dispose
- [ ] Collections cleared in dispose
- [ ] `CancellationTokenSource` cancelled then disposed
- [ ] No memory leaks detected via `dotnet-counters` or `dotnet-trace`

### Testing
- [ ] Create/dispose component 10 times; `gc-heap-size-mb` stable (≤ 1 MB growth)
- [ ] JS errors logged with `ILogger<T>` (not `Console.Error.WriteLine`)
- [ ] Disconnect during dispose handled gracefully (`JSDisconnectedException` caught)
- [ ] `CancellationToken` cancellation verified in unit test

---

## Part 5: CancellationToken — Propagation Pattern

All async lifecycle hooks and background work must propagate `CancellationToken` to prevent lingering work after component disposal.

```csharp
@implements IAsyncDisposable

@code {
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        // Pass component's own token to all async operations
        _data = await DataService.GetAsync(_cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        // Cancel any in-flight work immediately on disposal
        await _cts.CancelAsync();   // .NET 8+ CancelAsync (non-blocking)
        _cts.Dispose();
    }
}
```

> **Rule:** Never pass `CancellationToken.None` to service calls from lifecycle hooks. Always use a component-owned `CancellationTokenSource`.

---

## Part 6: ErrorBoundary & DispatchExceptionAsync (.NET 10)

Wrap risky child components in `<ErrorBoundary>`. In .NET 10, use `DispatchExceptionAsync` to route background exceptions to the nearest error boundary:

```razor
<ErrorBoundary>
    <ChildContent>
        <RiskyDataComponent />
    </ChildContent>
    <ErrorContent Context="ex">
        <p class="alert alert-danger">Error: @ex.Message</p>
    </ErrorContent>
</ErrorBoundary>
```

```csharp
// .NET 10+ — route background task exceptions to ErrorBoundary
private async Task PollData()
{
    try
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            await Task.Delay(5000, _cts.Token);
            _data = await DataService.GetAsync(_cts.Token);
            await InvokeAsync(StateHasChanged);
        }
    }
    catch (OperationCanceledException) { /* expected on dispose */ }
    catch (Exception ex)
    {
        await DispatchExceptionAsync(ex); // routes to ErrorBoundary
    }
}
```

---

## Memory Leak Detection

### Using dotnet-counters (Blazor Server)

```bash
# Monitor memory growth over time
dotnet counters monitor System.Runtime -p <PID> --refresh-interval 1

# Watch for:
# - alloc-rate-bytes/sec (should stabilize after warm-up)
# - gc-heap-size-mb (should not continuously grow)
# - exception-count (unexpected spikes indicate error swallowing)
```

### dotnet-trace (Heap profiling)

```bash
dotnet trace collect -p <PID> --providers Microsoft-DotNETRuntime:0x1:5
dotnet trace convert --format Speedscope trace.nettrace
```

### Browser DevTools (Blazor WASM)

1. Open DevTools → Memory tab
2. Take heap snapshot before/after component creation
3. Create/remove component 10 times
4. Take final snapshot
5. Compare: if memory grows by > 1 MB, there's a likely leak

### Code Review Checklist

Before merging, verify component has:
- ✅ `IAsyncDisposable` interface
- ✅ `DisposeAsync` implementation
- ✅ All JS modules in `DisposeAsync`
- ✅ All subscriptions in `DisposeAsync`
- ✅ All `DotNetObjectReference` in `DisposeAsync`
- ✅ `CancellationTokenSource` cancelled and disposed
- ✅ Try-catch in lifecycle hooks
- ✅ No fire-and-forget tasks (`_ = ...`)
- ✅ `JSDisconnectedException` handled in `DisposeAsync`

---

## Summary: The Memory-Safe Component

```csharp
public partial class CompleteExample : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private IEventBus? EventBus { get; set; }
    
    private IJSObjectReference? _module;
    private IDisposable? _subscription;
    private DotNetObjectReference<CompleteExample>? _dotNetRef;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Load module
                _module = await JS.InvokeAsync<IJSObjectReference>(
                    "import", "./module.js");
                
                // Register callback
                _dotNetRef = DotNetObjectReference.Create(this);
                await _module.InvokeVoidAsync("init", _dotNetRef);
                
                // Subscribe to events
                _subscription = EventBus?.Subscribe<MyEvent>(Handle);
            }
            catch (JSException ex)
            {
                Console.Error.WriteLine($"Init error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex}");
            }
        }
    }
    
    [JSInvokable]
    public Task OnJsEvent(string data)
    {
        Console.WriteLine($"From JS: {data}");
        return Task.CompletedTask;
    }
    
    private void Handle(MyEvent e) { }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        try
        {
            _dotNetRef?.Dispose();
            _subscription?.Dispose();
            
            if (_module is not null)
                await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Expected in Blazor Server during reconnect
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Dispose error: {ex.Message}");
        }
    }
}
```

