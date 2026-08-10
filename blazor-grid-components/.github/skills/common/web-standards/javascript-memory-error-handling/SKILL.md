---
name: javascript-memory-error-handling
description: "Mandated skill covering memory leak prevention, proper resource cleanup, AbortController usage, event listener management, closure handling, and error handling patterns in JavaScript/TypeScript for Blazor interop modules and standalone ES modules."
metadata:
  category: Interop
  tags: [javascript, typescript, memory-leaks, disposal, event-listeners, error-handling, abortcontroller, weakref, finalizationregistry, closures, gc, performance]
compatibility:
  dotnet: ["net8.0 (LTS)", "net10.0 (LTS)"]
  targets: ["Blazor WASM", "Blazor Server", "ES Modules"]
  note: "Critical for long-running SPAs and Blazor Server circuits where memory leaks accumulate over session lifetime"
---

# Skill: JavaScript Memory Management & Error Handling (Mandated)

> **Applies to:** Blazor WASM · Blazor Server · ES Modules
> **Proficiency required:** Intermediate
> **Criticality:** HIGH — Memory leaks in JS accumulate over session lifetime in Blazor Server circuits
> **Last reviewed:** April 2026

---

## Summary

JavaScript memory leaks are often invisible until they cause performance degradation or crashes. In Blazor Server (where a single JS context lives for the entire user session), proper resource cleanup is **critical**. This skill covers detection, prevention, and recovery patterns for memory leaks and provides comprehensive error handling strategies.

---

## Part 1: Memory Leak Prevention

### Pattern 1: Event Listener Cleanup

**❌ BAD — Memory leak**:
```javascript
export function setupResizeListener(callback) {
  // Listener is never removed — accumulates on each reconnect
  window.addEventListener('resize', callback);
}
```

**✅ GOOD — Proper cleanup**:
```javascript
export function setupResizeListener(element, callback) {
  // Store reference for cleanup
  element._resizeCallback = callback;
  window.addEventListener('resize', callback);
  
  // Return dispose function
  return () => {
    window.removeEventListener('resize', callback);
    delete element._resizeCallback;
  };
}

// Or for modern modules with cleanup:
export class ResizeObserver {
  constructor(element, callback) {
    this.element = element;
    this.callback = callback;
    this.observer = new window.ResizeObserver(callback);
  }
  
  init() {
    this.observer.observe(this.element);
  }
  
  dispose() {
    this.observer.disconnect();
    this.element = null;
    this.callback = null;
  }
}
```

### Pattern 2: Detached DOM Elements

**❌ BAD — Memory leak (WeakMap doesn't help if element reference held elsewhere)**:
```javascript
const elementCache = new Map(); // Strong reference!

export function cacheElement(id, element) {
  elementCache.set(id, element);  // Element can't be GC'd
}
```

**✅ GOOD — Explicit cleanup or WeakMap**:
```javascript
// Option 1: Explicit cleanup
const elementCache = new Map();

export function cacheElement(id, element) {
  elementCache.set(id, element);
}

export function clearElementCache(id) {
  elementCache.delete(id);  // Explicitly remove
}

export function clearAllCache() {
  elementCache.clear();  // Clean up on dispose
}

// Option 2: WeakMap (automatically GC'd when references removed)
const elementCache = new WeakMap();

export function cacheElement(element, metadata) {
  elementCache.set(element, metadata);  // Auto-removed when element GC'd
}

export function getMetadata(element) {
  return elementCache.get(element);
}
```

### Pattern 3: Interval/Timeout Cleanup with AbortController

**❌ BAD — Timers keep running after component disposed**:
```javascript
export function startPolling(callback) {
  setInterval(callback, 5000);  // Never cleared!
}
```

**✅ GOOD — AbortController pattern (modern, cancellable)**:
```javascript
export function startPolling(callback, interval = 5000) {
  const controller = new AbortController();
  const { signal } = controller;

  const timerId = setInterval(() => {
    if (signal.aborted) return;
    callback();
  }, interval);

  // Also support signal-based abort from caller
  signal.addEventListener('abort', () => clearInterval(timerId));

  // Return dispose function
  return () => controller.abort();
}

// Fetch with AbortController — prevent dangling requests
export async function fetchWithAbort(url, signal) {
  try {
    const response = await fetch(url, { signal });
    return await response.json();
  } catch (err) {
    if (err.name === 'AbortError') return null; // cancelled — not an error
    throw err;
  }
}

// Usage in Blazor component:
// private IJSObjectReference? _module;
// private Func<Task>? _dispose;
// 
// protected override async Task OnAfterRenderAsync(bool firstRender)
// {
//     if (firstRender)
//     {
//         _module = await JS.InvokeAsync<IJSObjectReference>("import", "./module.js");
//         _dispose = await _module.InvokeAsync<Func<Task>>("startPolling", 
//             new DotNetObjectReference(this));
//     }
// }
// 
// public async ValueTask DisposeAsync()
// {
//     if (_dispose != null) await _dispose();
//     if (_module != null) await _module.DisposeAsync();
// }
```

### Pattern 4: Closure Cleanup

**❌ BAD — Closure captures parent references, preventing GC**:
```javascript
export function createEventHandler(largeData) {
  return () => {
    console.log(largeData.length); // Closure captures entire largeData
  };
  // largeData is kept in memory even after handler removed
}
```

**✅ GOOD — Explicit memory management in closures**:
```javascript
export function createEventHandler(largeData) {
  let data = largeData;
  
  const handler = () => {
    if (data) console.log(data.length);
  };
  
  // Return both handler AND cleanup
  return {
    handler,
    dispose: () => {
      data = null;  // Release reference
    }
  };
}

// Or use WeakMap for cached data:
const dataCache = new WeakMap();

export function createEventHandler(key, largeData) {
  dataCache.set(key, largeData);
  
  return () => {
    const data = dataCache.get(key);
    if (data) console.log(data.length);
  };
  // largeData is auto-GC'd when key is GC'd
}
```

### Pattern 5: Module Cleanup (Singleton Pattern)

**❌ BAD — Module state persists across reconnects**:
```javascript
// module.js
let state = {}; // Persists forever
let listeners = []; // Accumulates listeners

export function addListener(callback) {
  listeners.push(callback);  // Never cleared
}
```

**✅ GOOD — Explicit module disposal**:
```javascript
// module.js
let state = {};
let listeners = [];

export function addListener(callback) {
  listeners.push(callback);
  
  // Return unsubscribe function
  return () => {
    listeners = listeners.filter(l => l !== callback);
  };
}

export function dispose() {
  // Clear all state on module reload
  state = {};
  listeners.length = 0;  // Or listeners = []
}

// Blazor component calls dispose before reimporting:
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./module.js");
    }
}

public async ValueTask DisposeAsync()
{
    if (_module is not null)
    {
        await _module.InvokeVoidAsync("dispose");
        await _module.DisposeAsync();
    }
}
```

### Pattern 6: DOM Element Reference Management

**❌ BAD — References prevent garbage collection**:
```javascript
const elementReferences = [];

export function captureElement(element) {
  elementReferences.push(element);  // Strong reference
}
```

**✅ GOOD — Clear references on removal**:
```javascript
const elementReferences = new WeakSet(); // Weak references

export function captureElement(element) {
  elementReferences.add(element);  // Auto-removes when element dies
}

// Or explicit cleanup:
const elementReferences = new Map();

export function captureElement(id, element) {
  elementReferences.set(id, element);
}

export function releaseElement(id) {
  elementReferences.delete(id);
}

export function clearAllElements() {
  elementReferences.clear();
}
```

---

## Part 2: Error Handling Patterns

### Pattern 1: Try-Catch with Logging

```javascript
export async function fetchData(url) {
  try {
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    console.error('[fetchData]', error.message, {
      url,
      stack: error.stack,
      timestamp: new Date().toISOString()
    });
    throw error;  // Re-throw for caller to handle
  }
}
```

### Pattern 2: Error Recovery with Retry

```javascript
export async function fetchWithRetry(url, options = {}) {
  const {
    maxRetries = 3,
    retryDelay = 1000,
    backoff = 1.5
  } = options;
  
  let lastError;
  
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      lastError = error;
      
      if (attempt < maxRetries) {
        const delay = retryDelay * Math.pow(backoff, attempt - 1);
        console.warn(`Retry ${attempt}/${maxRetries} after ${delay}ms`, error.message);
        await new Promise(r => setTimeout(r, delay));
      }
    }
  }
  
  throw new Error(`Failed after ${maxRetries} retries: ${lastError.message}`);
}
```

### Pattern 3: Safe Callback Invocation (Blazor)

```javascript
export async function invokeDotNet(dotnetRef, method, ...args) {
  try {
    return await dotnetRef.invokeMethodAsync(method, ...args);
  } catch (error) {
    // Distinguish between JS errors and .NET errors
    if (error.message.includes('JSDisconnectedException')) {
      console.warn('Connection lost, JS interop unavailable');
      return null;
    }
    
    console.error(`[invokeDotNet] ${method}:`, error);
    throw error;
  }
}
```

### Pattern 4: Promise Error Handling

```javascript
export function setupPromiseErrorHandler() {
  // Handle unhandled promise rejections
  window.addEventListener('unhandledrejection', event => {
    console.error('[unhandledrejection]', {
      reason: event.reason.message,
      stack: event.reason.stack,
      context: 'unhandled promise rejection'
    });
    
    // Prevent default browser error reporting if desired
    event.preventDefault();
  });
}

// Also handle global errors
export function setupGlobalErrorHandler() {
  window.addEventListener('error', event => {
    console.error('[global error]', {
      message: event.message,
      filename: event.filename,
      lineno: event.lineno,
      colno: event.colno,
      stack: event.error?.stack
    });
  });
}
```

### Pattern 5: Timeout Wrapper

```javascript
export async function withTimeout(promise, timeoutMs) {
  const timeoutPromise = new Promise((_, reject) =>
    setTimeout(() => reject(new Error(`Timeout after ${timeoutMs}ms`)), timeoutMs)
  );
  
  return Promise.race([promise, timeoutPromise]);
}

// Usage:
export async function fetchWithTimeout(url, timeoutMs = 5000) {
  try {
    return await withTimeout(fetch(url), timeoutMs);
  } catch (error) {
    console.error('Fetch timeout or error', error);
    throw error;
  }
}
```

### Pattern 6: Graceful Degradation

```javascript
export function initializeFeature(element) {
  try {
    // Try modern API
    if (typeof element.animate === 'function') {
      return setupAnimationAPI(element);
    }
    
    // Fallback to CSS
    if (CSS && CSS.supports && CSS.supports('animation', 'none')) {
      return setupCSSAnimation(element);
    }
    
    // Final fallback
    console.warn('Neither Animation API nor CSS animations available');
    return () => {}; // No-op dispose
  } catch (error) {
    console.error('Feature initialization failed', error);
    return () => {}; // No-op dispose on error
  }
}
```

---

## Part 3: Detection & Profiling

### Chrome DevTools Memory Profiler

1. **Take heap snapshot** before and after operation
2. **Compare snapshots** — growth indicates leak
3. **Filter by constructor** — find accumulated objects
4. **Check detached DOM nodes** — unreachable elements in memory

### Performance Monitor

```javascript
// Monitor memory usage
export function logMemoryStats() {
  if (performance.memory) {
    console.log('Memory:', {
      usedJSHeapSize: (performance.memory.usedJSHeapSize / 1048576).toFixed(2) + ' MB',
      totalJSHeapSize: (performance.memory.totalJSHeapSize / 1048576).toFixed(2) + ' MB',
      jsHeapSizeLimit: (performance.memory.jsHeapSizeLimit / 1048576).toFixed(2) + ' MB'
    });
  }
}

// Periodic monitoring
export function startMemoryMonitoring(intervalMs = 5000) {
  const intervalId = setInterval(logMemoryStats, intervalMs);
  return () => clearInterval(intervalId);
}
```

---

## Mandatory Standards

### Memory Management
- [ ] All event listeners have corresponding remove calls
- [ ] All `setInterval`/`setTimeout` calls have corresponding clear calls  
- [ ] All DOM element references cleared when no longer needed
- [ ] Module has explicit `dispose()` function for cleanup
- [ ] Closures explicitly release captured references
- [ ] `DotNetObjectReference` disposed in C# `DisposeAsync`

### Error Handling
- [ ] All async operations wrapped in try-catch
- [ ] Errors logged with context (URL, timestamp, stack)
- [ ] JSException handled separately in Blazor interop
- [ ] Unhandled promise rejections captured globally
- [ ] Timeouts implemented for network requests
- [ ] Graceful fallback for unavailable APIs

### Testing
- [ ] Memory leak test — attach/detach component 10 times, verify memory stable
- [ ] Error test — verify error is logged correctly
- [ ] Cleanup test — verify dispose removes all listeners and clears state

---

## WeakRef & FinalizationRegistry (Modern JS — ES2021+)

Use `WeakRef` and `FinalizationRegistry` for optional back-references to DOM elements that shouldn't prevent garbage collection:

```javascript
// WeakRef — holds a weak reference; .deref() returns undefined when GC'd
const elementRef = new WeakRef(domElement);

// Later access — guard with deref() check
const el = elementRef.deref();
if (el) {
    el.style.display = 'none';
}

// FinalizationRegistry — run cleanup when object is GC'd
const registry = new FinalizationRegistry((key) => {
    console.log(`Element ${key} was garbage collected`);
    cache.delete(key);
});

registry.register(domElement, 'myElementKey');
```

> **When to use:** Only for optional caches or debug logging. Never use `WeakRef` for primary logic — the object may be GC'd at any time.

---

## Common Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| Listeners never removed | Accumulation per reconnect | Store reference, remove in dispose |
| Timers never cleared | CPU/memory accumulation | `clearInterval`/`AbortController` in dispose |
| Closures capture large objects | Memory grows per instance | Release references in cleanup |
| No module `dispose()` | State persists across reloads | Add explicit `dispose()` export |
| Unhandled promise rejections | Silent failures | Add `unhandledrejection` global handler |
| No timeout on network calls | Hanging requests block GC | Use `AbortController` with timeout |
| `fetch` without `signal` | Can't cancel on component disposal | Always pass `AbortController.signal` |
| Strong `Map` references to DOM | Elements can't be GC'd | Use `WeakMap` for element metadata |

---

## Example: Complete Memory-Safe Module

```javascript
// safe-module.js

let state = {};
let listeners = [];
let timers = [];
let elements = new WeakMap();

// Public API
export function init(options = {}) {
  state = { initialized: true, ...options };
}

export function onStateChange(callback) {
  listeners.push(callback);
  
  // Return unsubscribe
  return () => {
    listeners = listeners.filter(l => l !== callback);
  };
}

export function startMonitoring(interval = 1000) {
  const timerId = setInterval(() => {
    listeners.forEach(cb => {
      try {
        cb(state);
      } catch (error) {
        console.error('[onStateChange callback]', error);
      }
    });
  }, interval);
  
  timers.push(timerId);
  
  // Return stop function
  return () => {
    clearInterval(timerId);
    timers = timers.filter(id => id !== timerId);
  };
}

export function cacheMetadata(element, metadata) {
  elements.set(element, metadata);  // WeakMap auto-cleanup
}

export function dispose() {
  // Clear all state
  state = {};
  listeners.length = 0;
  
  // Clear all timers
  timers.forEach(timerId => clearInterval(timerId));
  timers.length = 0;
  
  console.log('[safe-module] Disposed');
}
```

**Blazor usage**:
```csharp
private IJSObjectReference? _module;
private Func<Task>? _stopMonitoring;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _module = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./_content/MyLib/js/safe-module.js");
        
        await _module.InvokeVoidAsync("init", new { version = "1.0" });
        _stopMonitoring = await _module.InvokeAsync<Func<Task>>(
            "startMonitoring", 5000);
    }
}

public async ValueTask DisposeAsync()
{
    if (_module is not null)
    {
        // Stop monitoring before dispose
        if (_stopMonitoring is not null)
            await _stopMonitoring();
        
        // Dispose module
        await _module.InvokeVoidAsync("dispose");
        await _module.DisposeAsync();
    }
}
```

