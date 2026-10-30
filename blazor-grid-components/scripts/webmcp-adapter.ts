/**
 * WebMCP Adapter (Browser Side)
 * -----------------------------
 * Registers Blazor DataGrid tools with the browser's proposed
 * `document.modelContext.registerTool()` API (WebMCP).
 *
 * Flow:
 *   1. C# WebMcpAdapter collects tool schemas
 *   2. C# calls InvokeMethod("sfBlazor.Grid.registerWebMcpTools", dataId, tools, prefix)
 *   3. This module registers each tool with `document.modelContext.registerTool()`
 *   4. When an AI agent invokes a tool, the browser forwards the call back to this handler
 *   5. We serialize the args and call back into .NET via JSInvokable "InvokeWebMcpToolExecute"
 *
 * Graceful Degradation:
 *   - If `document.modelContext` is unavailable (e.g. standard browsers, no agent host),
 *     this module silently no-ops.
 *   - All errors are logged but never thrown, so they cannot break the grid.
 */

import { isNullOrUndefined } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';

export type ToolDescriptor = {
    name: string;
    description?: string;
    inputSchema?: Record<string, unknown>;
    outputSchema?: Record<string, unknown>;
    annotations?: Record<string, unknown>;
    [key: string]: unknown;
};

type ToolExecutionResponse = {
    content: Array<{ type: string; text: string }>;
    isError: boolean;
};

interface DotNetRefLike {
    invokeMethodAsync(method: string, ...args: unknown[]): Promise<string | null>;
}

interface ModelContextApi {
    registerTool: {
        (tool: ToolDescriptor): unknown;
        (name: string, tool: Record<string, unknown>): unknown;
    };
    unregisterTool?: (name: string) => unknown;
    tools?: {
        register: (
            name: string,
            execute: (...args: unknown[]) => Promise<unknown>,
            options: {
                description: string;
                inputSchema: Record<string, unknown>;
                outputSchema: Record<string, unknown>;
            }
        ) => unknown;
    };
}

/**
 * Track registered tools per grid instance for cleanup/dispose.
 * key: gridDataId, value: array of registration handles (version-dependent)
 */
const registeredTools = new Map<string, string[]>();

/**
 * Register WebMCP tools with `document.modelContext.registerTool()`.
 *
 * The dotnetRef is auto-discovered by looking up the grid instance via
 * `window.sfBlazor.getCompInstance(dataId)` which is the same lookup pattern
 * every existing handler in `sf-grid.ts` uses. This keeps the JS-invoke
 * surface decoupled from internal SfGrid state.
 *
 * @param dataId  - The dataId of the grid component (e.g., "SfGrid-123")
 * @param tools   - Array of tool descriptors sent from C# (Name, Description, InputSchema, etc.)
 * @param prefix  - The tool-name prefix to avoid collisions across multiple grids (e.g., "sfgridqz2q34iovpgg").
 *                  The full qualified name becomes `${prefix}_${tool.name}` (e.g., "sfgridqz2q34iovpgg_getVisibleColumns").
 *                  Do NOT include a trailing underscore - that is appended here. The C# dispatcher
 *                  splits on the first `_` to recover the base tool name.
 */
export function registerWebMcpTools(
    dataId: string,
    tools: ToolDescriptor[],
    prefix: string
): void {
    try {
        const documentModelContext = (document as Document & { modelContext?: ModelContextApi }).modelContext;

        // Silent no-op: when document.modelContext is missing (no MCP host), bail gracefully
        if (!document || isNullOrUndefined(documentModelContext)) {
            // Helpful diagnostic for dev mode only
            if (typeof console !== 'undefined' && typeof (console as Console & { __webmcpDebug?: boolean }).__webmcpDebug !== 'undefined') {
                console.debug('[WebMCP] document.modelContext not available — skipping tool registration');
            }
            return;
        }

        if (isNullOrUndefined(tools) || tools.length === 0) {
            return;
        }

        // Unregister any prior registrations for this grid (idempotent re-registration)
        unregisterWebMcpTools(dataId);

        const names: string[] = [];
        const modelCtx = documentModelContext;

        for (const tool of tools) {
            try {
                const toolName = typeof tool.name === 'string' ? tool.name : '';

                // The C# dispatcher splits on the first '_' to recover the base tool name.
                // Always emit `${prefix}_${tool.name}` so the dispatcher's split is unambiguous.
                // Strip any trailing '_' the caller may have supplied to keep the boundary clean.
                const cleanPrefix = prefix.endsWith('_') ? prefix.slice(0, -1) : prefix;
                const fullName = cleanPrefix.length > 0
                    ? `${cleanPrefix}_${toolName}`
                    : toolName;
                // Lazy-closure handler: looks up the dotnetRef fresh each call so that
                // re-init scenarios where the dotnetRef gets replaced still work.
                const execute = createToolHandler(dataId, fullName);

                // Build the ModelContextTool object. Note the spec uses `execute` (not
                // `handler`) for the invocation function. Browser implementations vary
                // in their argument shape, so we try the single-argument form first
                // (current Chromium WebMCP IDL: registerTool(ModelContextTool tool)) and
                // fall back to the two-argument form (name, tool) used in earlier drafts.
                const toolDescriptor: ToolDescriptor = {
                    name: fullName,
                    description: tool.description || '',
                    inputSchema: tool.inputSchema || {},
                    outputSchema: tool.outputSchema || {},
                    annotations: tool.annotations || {},
                    execute,
                    // Alias kept for older proposed-spec implementations that expect `handler`
                    handler: execute
                };

                if (typeof modelCtx.registerTool === 'function') {
                    // Try the modern single-argument signature first
                    try {
                        modelCtx.registerTool({ ...toolDescriptor });
                    } catch (singleArgEx) {
                        // Fall back to the older two-argument signature: (name, tool)
                        try {
                            const rest: Record<string, unknown> = { ...toolDescriptor };
                            delete rest.name;
                            modelCtx.registerTool(fullName, rest);
                        } catch {
                            // Both signatures failed; surface the original single-arg error so
                            // the developer sees the canonical WebMCP message.
                            throw singleArgEx;
                        }
                    }
                    names.push(fullName);
                } else if (typeof modelCtx.tools?.register === 'function') {
                    // Alternative shape (proposal stage): document.modelContext.tools.register()
                    modelCtx.tools.register(fullName, execute, {
                        description: tool.description || '',
                        inputSchema: tool.inputSchema || {},
                        outputSchema: tool.outputSchema || {}
                    });
                    names.push(fullName);
                }
            } catch (perToolEx) {
                // Per-tool failures must not abort registration of remaining tools
                if (typeof console !== 'undefined') {
                    console.warn(`[WebMCP] Failed to register tool '${tool.name}':`, perToolEx);
                }
            }
        }

        registeredTools.set(dataId, names);
    } catch (ex) {
        // Top-level guard: never throw out of registration
        if (typeof console !== 'undefined') {
            console.warn('[WebMCP] Tool registration failed:', ex);
        }
    }
}

/**
 * Unregister all tools associated with a given grid dataId.
 * Called when the grid is disposed or re-registering.
 *
 * @param dataId - The grid's dataId
 */
export function unregisterWebMcpTools(dataId: string): void {
    const names = registeredTools.get(dataId);
    if (!names || names.length === 0) {
        return;
    }
    try {
        const modelCtx = (document as Document & { modelContext?: ModelContextApi }).modelContext;
        if (modelCtx && typeof modelCtx.unregisterTool === 'function') {
            for (const name of names) {
                try {
                    modelCtx.unregisterTool(name);
                } catch (perEx) {
                    // Best-effort: log and continue
                    if (typeof console !== 'undefined') {
                        console.warn(`[WebMCP] Failed to unregister tool '${name}':`, perEx);
                    }
                }
            }
        }
    } finally {
        registeredTools.delete(dataId);
    }
}

/**
 * Build the handler function that the browser calls when the agent invokes a tool.
 * The handler packs the args into a JSON string and forwards to .NET.
 *
 * The dotnetRef is resolved at invocation time (not captured in closure) by querying
 * the grid instance via `window.sfBlazor.getCompInstance(dataId)`. This pattern
 * matches the rest of `sf-grid.ts` and ensures re-initialization scenarios work.
 */
function createToolHandler(
    dataId: string,
    fullToolName: string
): (...args: unknown[]) => Promise<ToolExecutionResponse> {
    return async function (inputArgs: unknown): Promise<ToolExecutionResponse> {
        try {
            // Look up the dotnetRef fresh on every call so re-init is robust
            const gridInstance = (window as Window & { sfBlazor?: { getCompInstance?: (dataId: string) => SfGrid | undefined } }).sfBlazor?.getCompInstance?.(dataId);
            // The property is `dotNetRef` (camelCase) per sf-grid-fn.ts constructor
            const dotnetRef = (gridInstance as unknown as { dotNetRef?: DotNetRefLike } | undefined)?.dotNetRef;

            if (!dotnetRef) {
                throw new Error(`Grid instance or dotnetRef not available for dataId '${dataId}'. ` +
                                `Ensure the grid has finished initialization before invoking tools.`);
            }

            // Normalize the agent's arguments to a plain dictionary.
            // Many browsers pass the args directly; some wrap them in { arguments: ... } or { params: ... }.
            const argsDict = normalizeArgs(inputArgs);

            // Serialize to JSON so the C# side gets a clean dictionary
            const argsJson = JSON.stringify(argsDict);

            // Call back into C# via JSInvokable method
            const responseJson = await dotnetRef.invokeMethodAsync(
                'InvokeWebMcpToolExecute',
                fullToolName,
                argsJson
            );

            // Parse the response and unwrap the MCP content
            if (!responseJson) {
                return { content: [{ type: 'text', text: '' }], isError: true };
            }
            return JSON.parse(responseJson) as ToolExecutionResponse;
        } catch (ex) {
            if (typeof console !== 'undefined') {
                console.error(`[WebMCP] Error in handler for ${fullToolName}:`, ex);
            }
            // Return a structured error so the agent can recover gracefully
            return {
                content: [
                    {
                        type: 'text',
                        text: JSON.stringify({
                            errorCode: 1002,
                            errorName: 'ExecutionFailed',
                            message: `Handler for '${fullToolName}' failed: ${(ex as Error)?.message ?? String(ex)}`
                        })
                    }
                ],
                isError: true
            };
        }
    };
}

/**
 * Normalize agent tool args into a flat dictionary for JSON serialization.
 */
function normalizeArgs(input: unknown): Record<string, unknown> {
    if (input == null) {
        return {};
    }
    if (typeof input !== 'object') {
        return { value: input };
    }
    if (Array.isArray(input)) {
        return { value: input };
    }

    const objInput = input as Record<string, unknown>;

    // Wrapped forms
    if (!isNullOrUndefined(objInput.arguments) && typeof objInput.arguments === 'object') {
        return { ...(objInput.arguments as Record<string, unknown>) };
    }
    if (!isNullOrUndefined(objInput.params) && typeof objInput.params === 'object') {
        return { ...(objInput.params as Record<string, unknown>) };
    }
    // Default: shallow-clone
    return { ...objInput };
}

/**
 * Internal helper for testing & debug: returns metadata about registered tools.
 */
export function getRegisteredToolsInfo(): { [dataId: string]: string[] } {
    // Return a copy so consumers can't mutate internal state
    const result: { [dataId: string]: string[] } = {};
    registeredTools.forEach((names, dataId) => {
        Object.defineProperty(result, dataId, {
            value: [...names],
            enumerable: true,
            configurable: true,
            writable: true
        });
    });
    return result;
}
