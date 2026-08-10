---
name: foreignkey-column-skill
description: Expert knowledge for the ForeignKey Column feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to foreign key column behaviour, including display-value resolution, foreign-data filtering and searching, dropdown editing, duplicate field handling, complex/nested column combinations, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for ForeignKey Column in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Paired custom agent: not yet created.
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to `GridForeignColumn`, `ForeignKey<T>`, or foreign key column lookup/filter/edit behaviour.
- One agent invocation = one feature skill maximum.
- Always finish with regression verification using `docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## Knowledge References
<!-- token-budget: 60 words -->

All content derived from reading these files — do NOT reproduce their content here:

- `docs/ai-agents/skills/_skill-template-standard.md`
- `docs/training/00-START-HERE.md`
- `docs/training/README.md`
- `docs/training/TRAINING-INDEX.md`
- `docs/training/DELIVERY-SUMMARY.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/01-getting-started/project-setup-guide.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/architecture/dependency-map.md`
- `docs/architecture/data-flow.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `docs/code-guidelines/logging-guidelines.md`
- `docs/overview/product-overview.md`
- `docs/overview/features-extracted.md`
- `docs/overview/glossary.md`
- `docs/ai-agents/skills/sorting-skill.md`
- `docs/ai-agents/skills/filterbar-skill.md`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Foreign data is a separate source, not a display-only alias**: training flags ForeignKey filtering as regression-sensitive. Any change must preserve the two-field contract: local key field (`Field` / `ForeignKeyField`) and display field (`ForeignKeyValue`).
- **Filter-before-sort order remains mandatory**: foreign key support adds lookup queries, but it must not change `DataGenerator<T>.GenerateQuery()` ordering. Filtering/searching still compose before sorting and paging.
- **Action pipeline and guards remain mandatory**: foreign key flows triggered by filter/search/edit must still pass through `OnActionBegin`/`OnActionComplete`, early guards, and null-safe module access.
- **Chunk narrowly**: `ForeignKey.cs`, `GridForeignColumn.razor`, and editor components should be handled as separate chunks; do not send the whole foreign key surface to a coding agent at once.
- **No direct render forcing**: delayed foreign data assignment is intentionally completed through `OnAfterRenderAsync` + `UpdateForeignData()`, not ad hoc `StateHasChanged()` calls.
- **`ConfigureAwait(true)` and XML docs stay required**: any async or public-surface touch must preserve project conventions.
- **Use source, not inference**: tests show real edge cases—duplicate foreign columns, different `ForeignKeyField` vs `Field`, dotted complex fields such as `Name.FirstName`, null sort values, remote filter rendering, and huge lookup data.
- **Complex-field paths are first-class**: foreign key columns may bind to nested model paths while the lookup key/value still comes from a flat foreign datasource. Preserve dot-path splitting, predicate generation, and type resolution for complex objects instead of normalizing fields to a flat name.
- **Keyboard/edit regressions are feature-crossing**: if a foreign key bug also affects editing or filter UI navigation, split the change into scoped tasks and verify both paths.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/Internal/Actions/ForeignKey.cs` | Primary `ForeignKey<T>` module — lookup query generation, filter/search detection, complex-field splitting, and foreign column data loading; handles `GetForeignKeyData<TModel>()` (line 47), `GenerateColumnQuery()` (line 149), `isNeedForeignKeyAction()` (line 279), `GetForeignKeyDataSource()` (line 392) (376 lines) |
| `src/Internal/GridForeignColumn.razor` | `GridForeignColumn` surface — foreign datasource parameter, filter UI fragment, editor fragment, delayed datasource refresh, default local sort comparer (301 lines) |
| `src/Internal/Renderer/ForeignKeySubComponents.razor` | Holds filter/editor render fragments and intentionally suppresses rerender |
| `src/Internal/Editors/ForeignKeyEditCell.razor` | Dropdown editor that binds key values while displaying `ForeignKeyValue`; handles duplicate-field ID collisions |
| `src/Internal/Editors/DropDownEditCell.razor` | Shared dropdown editor path that delegates to `ForeignKeyEditCell` for FK columns and builds `GridForeignKeyColumnParameters` |
| `src/GridColumn.cs` | Declares `ForeignKeyField`, `ForeignKeyValue`, and local `ForeignKeySorting` behaviour |
| `src/SfGrid.Lifecycle.cs` | Instantiates `ForeignKeyModule` during grid initialization |
| `src/SfGrid.razor.cs` | Detects foreign-key actions, resolves complex column types with `ResolveColumnValueTypeDynamic()` (line 1048), handles `ForeignKeyValue` field matching in filter predicates (line 1983), stores `ForeignKeyModule` reference (line 226), runs pre-query FK loading, executes post-query FK enrichment, restores persisted duplicate FK columns by `Uid`/`Index` |
| `src/SfGrid.Methods.cs` | Public helpers such as `GetForeignKeyColumnsAsync()` and column lookup APIs |
| `tests/bunit/Actions/ForeignKey.razor` | Baseline coverage for render, sort, filter, search, edit, group, aggregate, selection, export, and custom datasource scenarios |
| `tests/bunit/CR Issues/BLAZ-11573.razor` | Different `ForeignKeyField` and `Field` mapping regression |
| `tests/bunit/CR Issues/BLAZ-11574.razor` | Complex-data foreign key predicate regression for nested field paths combined with foreign lookup data |
| `tests/bunit/CR Issues/BLAZ-14707.razor` | Multiple foreign key filter columns regression |
| `tests/bunit/CR Issues/BLAZ-17588.razor` | FilterBar rendering with remote foreign data regression |
| `tests/bunit/CR Issues/BLAZ-24921.razor` | Huge foreign lookup data sorting/filtering regression |
| `tests/bunit/CR Issues/BLAZ-25015.razor` | Duplicate `ForeignKeyValue` across columns regression |
| `tests/bunit/CR Issues/BLAZ-921305.razor` | Foreign key sorting with null values regression |

**Last Verified:** June 23, 2026 — All file paths and line numbers verified against source code

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live docs, code, and tests.  
> Omitted pairs have no meaningful interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| ForeignKey + FilterBar | Predicate field must target `GridColumn.ForeignKeyValue`, not raw `Field`, when filtering by displayed text. `GridForeignColumn` filter autocomplete must use the column's own data manager/child content for remote/custom FK sources. | Critical |
| ForeignKey + FilterMenu / Excel / CheckBox Filter | Menu-based filter UIs must resolve distinct values from the foreign datasource, keep `Uid`-based matching for duplicate FK columns, and avoid collapsing two columns that share the same `ForeignKeyValue`. | Critical |
| ForeignKey + Complex / Nested Columns | Dotted fields such as `Name.FirstName` must keep nested-path predicate generation, complex-object type resolution, and lookup-field mapping intact. FK filtering/searching/editing must not flatten the local field or lose the foreign display/value contract. | Critical |
| ForeignKey + Editing | `ForeignKeyEditCell` must display `ForeignKeyValue` but write back the key field (`ForeignKeyField ?? Field`). Duplicate FK columns must keep stable DOM IDs via `Uid`, or edit widgets collide. | Critical |
| ForeignKey + Sorting | Local FK columns assign `ForeignKeySorting` so display-text sorting works without changing the underlying key contract. Null-value handling and remote/custom data paths must remain intact. | High |
| ForeignKey + Grouping | Group captions and grouped results must resolve display text, not raw IDs. Group + FK + filter/sort paths must keep current view records and caption rendering stable. | High |
| ForeignKey + Searching | `ForeignKeyModule.isNeedForeignKeyAction()` and `GenerateColumnQuery()` must fetch FK column data only when search/filter requires it and respect `AllowSearching` on each column. | High |
| ForeignKey + Paging / Virtualization | FK lookup queries are built from current page / current virtual slice keys. Changes must not assume the full dataset is present or load stale lookup data for the viewport. | High |
| ForeignKey + Persistence / Reorder | Persist restore logic must match duplicate FK columns by `Index`/`Uid`, not `Field` alone, or reordered duplicate columns restore the wrong persisted column state. | High |
| ForeignKey + Export | Exported output must use resolved display values while still respecting active sort/filter/group state on FK columns. | Medium |
| ForeignKey + Aggregates | Aggregate templates over FK fields must keep footer/group aggregate rendering stable after lookup substitution and current-view refreshes. | Medium |
| ForeignKey + Selection | Row selection and event args must stay aligned with FK-enriched current view data; lookup enrichment must not desync selected row metadata after filter/page refresh. | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```text
Mode: {feature-implementation | bug-fix}
Skill: ForeignKey Column
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

Load this only for `GridForeignColumn`, `ForeignKey<T>`, foreign lookup filtering/searching, FK dropdown editing, or foreign-key + complex-column work.

Before changing code, consult:
- docs/training/02-requirements-analysis/understanding-requirements.md
- docs/training/06-reference/quick-reference-guides.md

Known rules:
- Core files: src/Internal/Actions/ForeignKey.cs, src/Internal/GridForeignColumn.razor, src/Internal/Editors/ForeignKeyEditCell.razor, src/SfGrid.razor.cs
- Display field is `ForeignKeyValue`; key field is `ForeignKeyField ?? Field`
- Local field may be a dotted complex path such as `Name.FirstName`; preserve nested-path handling instead of flattening it
- Filtering/search on FK columns may require preloading lookup data through `ForeignKeyModule.GetForeignKeyData(...)`
- Local FK sorting may rely on `ForeignKeySorting`; do not break null handling
- Duplicate FK columns with the same `Field` require `Uid`-based editor/filter element identity
- Complex FK scenarios depend on `ForeignKey.cs` query generation and `SfGrid.razor.cs` type resolution staying aligned for the same dotted field
- Delayed `ForeignDataSource` updates flow through `OnAfterRenderAsync` + `UpdateForeignData()`

Constraints:
- No API changes unless explicitly requested
- No direct `StateHasChanged()` or direct JS runtime calls
- Keep guard clauses first, keep `ConfigureAwait(true)`, preserve XML docs on public touches
- Do not break filter, sort, group, edit, paging, or persistence interactions listed in the matrix above

Output:
1. Root cause / design note
2. Modified method(s) only
3. Interaction risks touched
4. Required regression tests

After implementation, run:
docs/ai-agents/prompts/regression-verification-prompt.md
```
