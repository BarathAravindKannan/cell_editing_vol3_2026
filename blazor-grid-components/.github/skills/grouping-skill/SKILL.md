---
name: grouping-skill
description: Expert knowledge for the Grouping feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to grouping behaviour, including drag-to-group, group drop area, caption rows, expand/collapse, lazy loading, group aggregates, cross-feature interaction guarantees with sorting, filtering, paging, virtualization, infinite scrolling, editing, and state persistence.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Grouping in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing grouping features, debugging caption row generation, adding lazy load grouping, or fixing regressions involving grouping with sorting, filtering, paging, virtualization, infinite scrolling, editing, aggregates, or state persistence.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/openspec/specs/grouping/spec.md` — Complete grouping specification
- `/docs/architecture/system-architecture.md` — Grouping<T> module lifecycle, GroupModelGenerator, event flow, PropertyChanges tracking
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, async/await rules
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, guard clauses, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — grouping must inject sort entries (`IsFromGroup = true`), clear selection unless `PersistSelection = true`, reset paging to page 1, invalidate virtual cache
- **Module communication via EventAggregator** — `ModelChanged(RequestType = Grouping)` triggers cross-module notifications
- **PropertyChanges dictionary drives incremental updates** — `AllowGrouping` / `GroupSettings` change triggers header re-render, drop area visibility
- **Action event pipeline mandatory** — `Grouping` (before, cancellable) → execute → `Grouped` (after)
- **Guard clauses first** — validate `AllowGrouping`, `GridColumn.AllowGrouping` before any operation
- **Single source of truth** — `GroupSettings.Columns` (string[]) is canonical grouped state; `Grouping<T>` reads/mutates; sort entries injected with `IsFromGroup = true`

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`GridGroupSettings.razor.cs`** — Settings component: `Columns`, `ShowDropArea`, `ShowGroupedColumn`, `ShowToggleButton`, `EnableLazyLoading`, `ExpandAllGroups`, `PersistGroupState`, `CaptionTemplate`
- **`Internal/Actions/Group.cs`** — Core grouping engine (`Grouping<T>`): `GroupColumn`, `UngroupColumn`, `GroupAddSortingQuery`, expand/collapse logic
- **`Internal/Generators/GroupModelGenerator.cs`** — Row/cell generation for grouped data: caption rows, group footer rows, hierarchy building
- **`Internal/Renderer/GroupCaptionRenderer.cs`** — Caption cell rendering: default text format, `CaptionTemplate` handling
- **`Internal/Renderer/GroupDropArea.razor`** — Drop area UI: chip display, drag-drop handling
- **`Internal/Renderer/GroupedHeader.razor`** — Grouped column chip UI: ungroup button, reorder support
- **`SfGrid.Properties.cs`** — Public API: `AllowGrouping` (bool, default false)
- **`SfGrid.Methods.cs`** — Public methods: `GroupColumnAsync`, `UngroupColumnAsync`, `ClearGroupingAsync`, `ExpandAllGroupAsync`, `CollapseAllGroupAsync`

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Grouping + Sorting | Grouped columns inject sort entries with `IsFromGroup = true`; sort entries always precede user sorts; ungrouping removes injected sort entry | Critical |
| Grouping + Filtering | Filter applies before grouping; grouped columns excluded from `ClearFiltering`; `GroupedData` recalculated from filtered set | Critical |
| Grouping + Paging | `EnableLazyLoading = true` suppresses `PageQuery`; pager shows grouped page count; `CurrentPage` resets to 1 on group/ungroup | Critical |
| Grouping + Virtualization | `EnableVirtualization = true` + grouping uses virtual grouped rows; `ExpandAllGroups = true` pre-expands all (ignored if lazy loading); `SetGeneratedData` called after expand/collapse | Critical |
| Grouping + Infinite Scrolling | Infinite scroll + grouping integrates via `InfiniteScrollModule`; grouped data fetched incrementally | High |
| Grouping + Editing | Edit during grouped view operates on underlying data row; caption rows non-editable; group state preserved unless `PersistGroupState = false` | High |
| Grouping + Selection | Selection cleared after group/ungroup unless `GridSelectionSettings.PersistSelection = true`; caption rows selectable | High |
| Grouping + Aggregates | Group footer rows generated when `GroupFooterTemplate` set; caption aggregates in `CaptionTemplate` via `context.GroupAggregates` | High |
| Grouping + State Persistence | `EnablePersistence = true` restores `GroupSettings.Columns` from `localStorage["grid{ID}"]`; `PersistGroupState` session-only (not persisted on reload) | Medium |
| Grouping + Frozen Columns | Grouped columns can be frozen; caption cells span across frozen and movable sections | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Grouping  

**Context**  
You are implementing or fixing Grouping for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Grouping organizes data rows into hierarchical caption groups based on one or more column field values. All grouping logic executes server-side in .NET via the `Grouping<T>` action module and `GroupModelGenerator<T>`.

**Core Architecture**  
- **Module**: `Grouping<T>` in `Internal/Actions/Group.cs`
- **Row Generator**: `GroupModelGenerator<T>` in `Internal/Generators/GroupModelGenerator.cs`
- **State**: `GroupSettings.Columns` (string[]) — single source of truth for grouped columns
- **UI Components**: `GroupDropArea.razor` (drop zone), `GroupedHeader.razor` (chip per grouped column), `GroupCaptionRenderer.cs` (caption cell rendering)
- **Public API**: `AllowGrouping` (master switch), `GridGroupSettings` (configuration), `GroupColumnAsync`, `UngroupColumnAsync`, `ClearGroupingAsync`, `ExpandAllGroupAsync`, `CollapseAllGroupAsync`

**Key Behaviors**  
1. **Master switch**: `AllowGrouping = true` enables grouping interactions; `false` makes all grouping APIs no-ops.
2. **Initial grouping**: `GridGroupSettings.Columns = ["CustomerID", "ShipCity"]` groups on init; rows grouped first by CustomerID, then ShipCity.
3. **Drag-to-group**: User drags column header to drop area → `GroupColumnAsync` called → column added to `GroupSettings.Columns`.
4. **Sort injection**: Grouping injects sort entry with `IsFromGroup = true`; grouped columns always precede user sorts in `SortSettings.Columns`.
5. **Caption row generation**: `GroupModelGenerator` produces caption rows (`RowType="GroupCaption"`) with expand/collapse cell, caption cell, indent cells.
6. **Expand/collapse**: Click caption row toggles `IsExpand`; child rows `Visible` set accordingly; keyboard: `Ctrl+Down` expand all, `Ctrl+Up` collapse all.
7. **Lazy loading**: `EnableLazyLoading = true` defers child row fetch until caption expanded; data query with `FilterQuery` + `GroupQuery` executed on expand.
8. **Persist state**: `PersistGroupState = true` retains expand/collapse state across sort/filter/page/edit; cleared on group/ungroup; session-only (not browser reload).
9. **Event pipeline**: `Grouping` (before, cancellable) → execute → `Grouped` (after); `Action` = `Add` / `Remove` / `Reset`.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Sorting**: Grouped columns inject sort entries with `IsFromGroup = true`; always precede user sorts; ungroup removes injected sort; user cannot manually remove grouped column sort.
- **Filtering**: Filter applies before grouping; grouped columns excluded from `ClearFiltering`; `GroupedData` recalculated from filtered set.
- **Paging**: `EnableLazyLoading = true` suppresses `PageQuery` (lazy load manages paging separately); `CurrentPage` resets to 1 on group/ungroup.
- **Virtualization**: Virtual + grouping uses `VirtualScrollModule.SetGeneratedData`; `ExpandAllGroups = true` pre-expands all (ignored if lazy loading).
- **Infinite Scrolling**: Grouped data fetched incrementally via `InfiniteScrollModule`; caption rows included in infinite buffer.
- **Editing**: Caption rows non-editable; data rows editable; group state preserved if `PersistGroupState = true`.
- **Selection**: Selection cleared after group/ungroup unless `PersistSelection = true`; caption rows selectable.
- **Aggregates**: Group footer rows (`RowType="Summary"`) generated when `GroupFooterTemplate` set; caption aggregates in `CaptionTemplate` via `context.GroupAggregates`.

**API Contracts**  
- **`GroupColumnAsync(string columnName)`**: Add column to grouped set; inject sort entry; trigger `Grouping` / `Grouped` events; no-op if `AllowGrouping = false` or column already grouped.
- **`UngroupColumnAsync(string columnName)`**: Remove column from grouped set; remove injected sort entry; restore column visibility; trigger events.
- **`ClearGroupingAsync()`**: Remove all grouped columns; restore visibility; trigger `Grouped` with `Action = Reset`.
- **`ExpandAllGroupAsync()`**: Set all caption rows `IsExpand = true`; update `Visible` for all child rows.
- **`CollapseAllGroupAsync()`**: Set all caption rows `IsExpand = false`; hide all child rows.

**GridGroupSettings Properties**  
- **`Columns`**: string[] — field names to group on; reflects current grouped state.
- **`ShowDropArea`**: bool (default true) — visibility of drop area banner.
- **`ShowGroupedColumn`**: bool (default false) — when false, hides source column after grouping (`GridColumn.Visible = false`, `IsHiddenByGrouping = true`).
- **`ShowToggleButton`**: bool (default false) — group/ungroup button in column headers.
- **`ShowUngroupButton`**: bool (default true) — ungroup button on each chip in drop area.
- **`EnableLazyLoading`**: bool (default false) — defer child row fetch until expand.
- **`ExpandAllGroups`**: bool (default false) — auto-expand all groups on init (only with virtualization; ignored if lazy loading).
- **`PersistGroupState`**: bool (default false) — retain expand/collapse state across sort/filter/page/edit; session-only.
- **`CaptionTemplate`**: RenderFragment<object> — custom caption cell template; context = `CaptionTemplateContext` (Field, Key, Count, Level, HeaderText, ForeignKey, GroupGuid, GroupAggregates).

**Event Payloads**  
- **`Grouping`**: `ColumnName`, `Action` (Add/Remove/Reset), cancellable via `args.Cancel`.
- **`Grouped`**: `ColumnName`, `Action` (Add/Remove/Reset).

**Edge Cases**  
- `AllowGrouping = false` + `GroupColumnAsync` → no-op; no exception.
- Column with `AllowGrouping = false` → cannot be grouped via drag or API.
- Fixed columns (checkbox, detail expand, row drag) → cannot be grouped.
- Column already grouped → `GroupColumnAsync` no-op; no duplicate entry.
- `ShowGroupedColumn = false` → column hidden after grouping; restored on ungroup.
- Existing sort entry (user-created) → preserved on grouping; direction not overwritten.
- `EnableLazyLoading = true` + `ExpandAllGroups = true` → lazy loading takes precedence; groups render collapsed.
- `PersistGroupState = true` + group/ungroup → `GroupStates` cleared (state reset).
- `PersistGroupState = true` + browser reload → state NOT restored (session-only).
- Foreign key column grouped → `CaptionTemplateContext.ForeignKey` populated with resolved display value.
- Group footer rows → generated when `GridAggregate` has `GroupFooterTemplate`; `Visible` matches caption `IsExpand`.
- Keyboard shortcuts: `Ctrl+Down` expand all, `Ctrl+Up` collapse all, `Alt+Up` collapse nearest parent, `Alt+Down` expand nearest parent.

**Before Making Changes**  
1. Read `/docs/training/` — understand sort injection, event pipeline, state management, cross-feature interaction.
2. Verify guard clauses: `AllowGrouping`, `GridColumn.AllowGrouping`, fixed column checks before mutation.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Test with sorting, filtering, paging, virtualization, infinite scrolling, editing, aggregates enabled.
5. Verify sort entry injection with `IsFromGroup = true` and removal on ungroup.
6. Validate `CurrentPage` reset to 1 on group/ungroup.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public methods/properties have XML doc comments with `<summary>`, `<param>`, `<returns>`, `<remarks>`.
- Event pipeline complete: `Grouping` → execute → `Grouped`.
- Sort entries injected with `IsFromGroup = true` for grouped columns.
- Selection cleared after group/ungroup unless `PersistSelection = true`.
- Virtual cache invalidated via `SetGeneratedData` when virtualization enabled.
- `CurrentPage` reset to 1 on group/ungroup changes.
- `GroupStates` cleared on group/ungroup when `PersistGroupState = true`.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Call Flow**  
Grouping requires JavaScript interop for lazy loading, virtual scroll synchronization, and infinite scroll coordination.

| C# Location | Method | JavaScript Function | Purpose |
|-------------|--------|---------------------|---------|
| `Group.cs:289` | `InvokeMethod` | `updateOptions` | Sync group options to JS Grid instance |
| `Group.cs:430` | `InvokeMethod<int>` | `lazyLoadGridHeight` | Get content height for lazy load calculations |
| `Group.cs:453,459` | `InvokeMethod` | `lazyGroupExpand` | Trigger virtual scroll refresh after lazy expand |
| `Group.cs:1204,1295` | `InvokeMethod` | `resetExpandCollapseAllScroll` | Reset infinite scroll position after expand/collapse all |

**TypeScript Signatures** (`scripts/sf-grid.ts`)  
```typescript
updateOptions(dataId: string, options: IGridOptions): void              // line 169
lazyGroupExpand(dataId: string, options: IGridOptions): void            // line 188
lazyLoadGridHeight(dataId: string): number                              // line 1099
resetExpandCollapseAllScroll(dataId: string, requestType: string): void // line 1108
```

**Responsibility Split**  
| Responsibility | C# (Group.cs) | JavaScript (sf-grid.ts) |
|----------------|---------------|-------------------------|
| Group/ungroup logic | ✅ Calculate GroupedData, inject sort entries | ❌ |
| Lazy load query | ✅ Build FilterQuery + GroupQuery on expand | ❌ |
| Virtual scroll sync | ❌ | ✅ Refresh virtual offsets after lazy expand |
| Infinite scroll reset | ❌ | ✅ Reset scroll positions after expand/collapse all |
| Grid height calculation | ❌ | ✅ Measure content.offsetHeight for lazy load |
| Options synchronization | ✅ Send updated options | ✅ Apply to JS Grid instance |

**CRITICAL Rules**  
1. **Always `ConfigureAwait(true)`** on all `InvokeMethod` calls (Blazor context required for state mutation).
2. **Lazy loading requires JS height check** before query execution (prevents blank space if content shorter than viewport).
3. **Virtual + grouping requires `lazyGroupExpand`** after data ready (forces virtual content re-render).
4. **Infinite scroll + expand/collapse all requires reset** (prevents scroll position corruption when group visibility changes).
