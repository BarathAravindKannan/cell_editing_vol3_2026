---
name: complex-column-skill
description: Expert knowledge for Complex Column rendering in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation, bug-fix, or rendering-analysis task scoped to nested `GridColumn.Columns`, stacked header DOM structure, leaf-column flattening, and safe interactions with freezing, reordering, filtering, virtualization, export, and complex field paths.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Authoritative reference for Complex Column rendering in the Syncfusion Blazor DataGrid. In this codebase, “complex column” most usefully means a nested column tree created by `GridColumn.Columns`, which renders as stacked or multi-level headers. This skill explains how the tree is converted into header rows, how DOM nodes are emitted, where leaf columns are still flattened for data operations, and how that structure behaves with freezing, virtualization, reordering, filtering, export, and dot-path complex data fields.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix` | `rendering-analysis`
- Load this skill for stacked header bugs, nested column rendering changes, header DOM analysis, or any issue where `GridColumn.Columns` alters row/column structure.
- Do not use this as the primary skill for pure dot-path data binding (`Field="Name.FirstName"`) unless that work also touches stacked header rendering.
- Always finish with regression verification using `docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## Knowledge References
<!-- token-budget: 60 words -->

All content derived from reading these files — do NOT reproduce their full source elsewhere:

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
- `docs/architecture/component-architecture.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/dependency-map.md`
- `docs/overview/product-overview.md`
- `docs/overview/glossary.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `docs/code-guidelines/logging-guidelines.md`
- `openspec/specs/export/spec.md`
- `.github/skills/fixed-column-skill/SKILL.md`
- `.github/skills/frozen-column-skill/SKILL.md`
- `.github/skills/virtualization-dom-structure-skill/SKILL.md`
- `.github/skills/foreignkey-column-skill/SKILL.md`
- `src/GridColumn.cs`
- `src/SfGrid.razor.cs`
- `src/Internal/SfGrid.razor`
- `src/Internal/Base/Utils.cs`
- `src/Internal/Renderer/GridHeader.razor`
- `src/Internal/Renderer/GridHeaderCell.razor`
- `src/Internal/Renderer/GridVirtualHeader.razor`
- `scripts/interfaces.ts`
- `scripts/sf-grid-fn.ts`
- `scripts/header-drag-drop.ts`
- `scripts/reorder.ts`
- `scripts/freeze.ts`
- `tests/CR Issues/BLAZ-903424.razor`
- `tests/CR Issues/BLAZ-832308.razor`
- `tests/Actions/Columns.razor`
- `tests/CR Issues/BLAZ-17213.razor`
- `tests/CR Issues/BLAZ-11574.razor`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never guess which “complex” is meant**: in this repo, nested header trees (`GridColumn.Columns`) and dot-path fields (`Name.FirstName`) are different features. Header nesting changes DOM structure; dot-path fields change value resolution.
- **Leaf columns remain the operational truth**: sorting, filtering, searching, editing, export, and most JS helpers eventually depend on flattened leaf columns even when the header renders as a tree.
- **Presentation and business layers must stay separated**: stacked header DOM is built in renderer components, while query and state operations still live in grid/action logic.
- **Regression analysis is mandatory**: training highlights cross-feature breakage as the main risk. Any stacked header change must be checked with freezing, reorder, filtering, export, and keyboard navigation.
- **Chunk by header pipeline**: `GridHeader.razor`, `GridHeaderCell.razor`, and JS reorder helpers should be analyzed as a rendering chunk; do not mix unrelated row/content logic unless the bug crosses the boundary.
- **No direct render forcing**: preserve existing render scheduling, event aggregation, and Blazor lifecycle flow.
- **Source of truth for DOM classes matters**: `.e-stackedheadercell` and `.e-stackedheadercelldiv` are not cosmetic; JS drag, resize, freeze-height sync, and tests depend on them.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/GridColumn.cs` | Public column model; `GridColumn.Columns` declares nested header trees |
| `src/Internal/SfGrid.razor` | Root component that selects standard header vs virtual header render path |
| `src/Internal/Renderer/GridHeader.razor` | Core stacked-header row generation: `GetRows()` (line 287), `GetStackedColumns()` (line 396), `GenerateCells()` (line 424), `MeasureColumnDepth()` (line 473), and `GetColspanCount()` (line 521) |
| `src/Internal/Renderer/GridHeaderCell.razor` | Emits stacked header `<th>` DOM, assigns `.e-stackedheadercell` / `.e-stackedheadercelldiv` classes, computes visibility via `IsStackedHeaderVisible()` (line 692), rowspan, colspan, and frozen classes |
| `src/Internal/Renderer/GridVirtualHeader.razor` | Virtualized header wrapper; still delegates actual `<table>/<thead>` generation to `GridHeader` |
| `src/SfGrid.razor.cs` | Freeze/fixed class/style logic for stacked trees, frozen subtree detection, and stacked freeze width calculations |
| `src/Internal/Base/Utils.cs` | Recursive helpers like `GetColumnByField()` and `GetColumnByFColUidOrField()` that must work across nested trees |
| `scripts/interfaces.ts` | Client column contract; nested header trees represented by `columns?: Column[]` |
| `scripts/sf-grid-fn.ts` | Builds JS column model, flattens leaf columns via `getColumns()` (line 197), retrieves stacked parent columns via `getStackedColumns()` (line 559), and resolves stacked header targets via `getStackedHeaderColumnByHeaderText()` (line 616) |
| `scripts/header-drag-drop.ts` | Drag clone creation and stacked-header grouping/reorder target resolution |
| `scripts/reorder.ts` | Same-level reorder validation, stacked parent collection, fixed-column protection |
| `scripts/freeze.ts` | Stacked header height synchronization across frozen/movable panes |
| `tests/CR Issues/BLAZ-903424.razor` | Visibility regression: stacked parents hide when all children are hidden |
| `tests/CR Issues/BLAZ-832308.razor` | Frozen + stacked movable header rendering regression |
| `tests/Actions/Columns.razor` | Fixed-column + stacked-header order regression coverage |

---

## Rendering Approach
<!-- token-budget: 220 words -->

Complex Column rendering uses a **dual model**:

1. **Hierarchical model for header rendering**
   - `GridColumn.Columns` creates a tree.
   - `GridHeader.GetRows()` detects any nested children and switches the grid into stacked-header mode.
   - `GetStackedColumns(level, ...)` groups columns by depth.
   - `GenerateCells(...)` creates `Cell<object>` items with `CellType.StackedHeader`, `ColSpan`, and `RowSpan`.

2. **Flattened model for operations**
   - `GridUtils.GetColumns(Parent)` flattens the tree to leaf columns.
   - Width control, sorting, filtering, searching, editing, export, and many JS interactions operate on this flattened list.
   - The header is multi-row, but the body stays a normal row/leaf-cell table.

3. **Renderer split**
   - `SfGrid.razor` picks `GridHeader` or `GridVirtualHeader`.
   - `GridVirtualHeader` only wraps translation/track markup; actual header row generation still comes from `GridHeader`.
   - `GridHeaderCell` decides whether a `<th>` is stacked or leaf and emits the correct class and accessibility attributes.

4. **Client model split**
   - `sf-grid-fn.ts.getColumns()` returns a flat client column model for most interactions.
   - `getStackedColumns()` and `getStackedHeaderColumnByHeaderText()` keep parent-header awareness for drag/drop and lookup.

### Critical distinction

```text
Nested header tree:
  <GridColumn HeaderText="Order Details">
      <GridColumns>...</GridColumns>
  </GridColumn>
  => multi-row header DOM

Dot-path field:
  <GridColumn Field="Name.FirstName" />
  => normal single header cell, complex data lookup only
```

---

## DOM Rendering Structure
<!-- token-budget: 420 words -->

### 1. Mental model: tree to rows

```text
Declared column tree
────────────────────────────────────────────
Root
├─ Order ID                      (leaf)
├─ Order Details                 (parent)
│  ├─ Order Date                 (leaf)
│  └─ Freight                    (leaf)
└─ Ship Details                  (parent)
   ├─ Shipped Date               (leaf)
   └─ Ship Country               (leaf)

Depth grouping built by GridHeader.GetStackedColumns()
────────────────────────────────────────────
Level 1: [Order ID] [Order Details] [Ship Details]
Level 2:            [Order Date] [Freight] [Shipped Date] [Ship Country]

Generated header cells
────────────────────────────────────────────
Order ID      -> rowspan=2 colspan=1 cellType=Data
Order Details -> rowspan=1 colspan=2 cellType=StackedHeader
Ship Details  -> rowspan=1 colspan=2 cellType=StackedHeader
```

### 2. Standard header DOM

```text
div.e-gridheader
└─ div.e-headercontent
   └─ table.e-table#<grid>_header_table
      ├─ WidthController                    [uses flattened leaf columns for widths]
      └─ thead
         ├─ tr.e-columnheader              [header level 1]
         │  ├─ th.e-headercell
         │  │  └─ div.e-headercelldiv
         │  │     └─ span.e-headertext      "Order ID"
         │  ├─ th.e-headercell.e-stackedheadercell
         │  │  └─ div.e-stackedheadercelldiv
         │  │     └─ "Order Details"
         │  └─ th.e-headercell.e-stackedheadercell
         │     └─ div.e-stackedheadercelldiv
         │        └─ "Ship Details"
         └─ tr.e-columnheader              [header level 2]
            ├─ th.e-headercell
            │  └─ div.e-headercelldiv      "Order Date"
            ├─ th.e-headercell
            │  └─ div.e-headercelldiv      "Freight"
            ├─ th.e-headercell
            │  └─ div.e-headercelldiv      "Shipped Date"
            └─ th.e-headercell
               └─ div.e-headercelldiv      "Ship Country"
```

### 3. Attribute contract on stacked `<th>`

```text
th.e-headercell.e-stackedheadercell
├─ data-uid = cell.Uid
├─ aria-colindex = visual column index
├─ rowspan = 1                    [parent header]
├─ colspan = number of visible leaf descendants
└─ child div.e-stackedheadercelldiv
```

Leaf headers under a stacked tree differ:

```text
th.e-headercell
├─ rowspan = remaining depth to bottom
├─ colspan = 1
└─ child div.e-headercelldiv
```

### 4. Visibility rules

```text
Parent stacked header renders only when at least one descendant leaf is visible.

child visibility false for all descendants
        ↓
GridHeaderCell.IsStackedHeaderVisible(column) = false
        ↓
parent stacked th gets hidden / omitted
```

This is the regression guarded by `tests/CR Issues/BLAZ-903424.razor`.

### 5. Body DOM is not nested

```text
Header: tree-shaped, multi-row
Body:   flat leaf-cell rows only

tbody
└─ tr.e-row
   ├─ td.e-rowcell   [Order ID]
   ├─ td.e-rowcell   [Order Date]
   ├─ td.e-rowcell   [Freight]
   ├─ td.e-rowcell   [Shipped Date]
   └─ td.e-rowcell   [Ship Country]
```

### 6. Virtualized header DOM

```text
div.e-gridheader
└─ div.e-headercontent
   └─ div.e-virtualtable   [translateX applied here]
      └─ GridHeader        [same stacked-header table shape as standard path]

Optional:
div.e-virtualtrack         [horizontal scroll spacer]
```

### 7. Frozen / fixed stacked header mental model

```text
stacked parent subtree
├─ frozen-left descendants
├─ movable descendants
└─ frozen-right descendants

Result:
the same parent text may be split logically across frozen/movable partitions,
but each rendered `<th>` still follows the stacked-header class contract.
```

---

## JavaScript Interop
<!-- token-budget: 30 words -->

Complex Column rendering is **primarily server-side Blazor** with no direct JavaScript interop calls in the rendering pipeline. TypeScript helpers (`sf-grid-fn.ts`, `reorder.ts`, `freeze.ts`, `header-drag-drop.ts`) are invoked indirectly via grid lifecycle events and DOM interactions, not from these renderer components directly. The stacked header DOM structure (`.e-stackedheadercell`, `.e-stackedheadercelldiv` classes) is consumed by these TypeScript modules for drag/drop, reorder validation, and freeze height synchronization.

---

## ASCII Decision Flow
<!-- token-budget: 140 words -->

```text
GridColumn.Columns exists?
        │
        ├─ no  ─► single header row
        │        └─ every leaf => th.e-headercell > div.e-headercelldiv
        │
        └─ yes ─► stacked header path
                 │
                 ├─ GridHeader.MeasureColumnDepth()
                 ├─ GridHeader.GetStackedColumns(levels)
                 ├─ GridHeader.GenerateCells()
                 │    ├─ parent => CellType.StackedHeader, colspan=N, rowspan=1
                 │    └─ leaf   => CellType.Data,         colspan=1, rowspan=remainingDepth
                 │
                 ├─ GridHeaderCell.GetClasses()
                 │    └─ adds .e-stackedheadercell for parent cells
                 │
                 ├─ GridHeaderCell.GetDivClassName()
                 │    └─ parent => .e-stackedheadercelldiv
                 │       leaf   => .e-headercelldiv
                 │
                 └─ JS helpers inspect those classes for drag, resize, freeze sync
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Complex Column + Frozen Columns | Stacked parent/child subtrees must keep correct left/right/fixed classes, offsets, and equalized header heights across panes | Critical |
| Complex Column + Reordering | Reorder must stay same-level only; stacked parents use `.e-stackedheadercell` / `.e-stackedheadercelldiv` lookup and must not cross illegal boundaries | Critical |
| Complex Column + Fixed Column | Fixed-first ordering may split stacked parent trees; rendered order and parent visibility must remain deterministic | Critical |
| Complex Column + Column Virtualization | `GridVirtualHeader` must preserve the same stacked DOM contract while only translating/wrapping the header surface | High |
| Complex Column + Filtering / Sorting | Data operations must still target flattened leaf columns even though header DOM is hierarchical | High |
| Complex Column + Show/Hide Column | Parent stacked headers must disappear when all descendants are hidden and shrink colspan when some descendants remain visible | High |
| Complex Column + Export | Excel/PDF export must reproduce header depth using matching rowspan/colspan semantics | High |
| Complex Column + Keyboard Navigation | Header focus order and `aria-colindex` must stay aligned with the rendered visual order across stacked levels | Medium |
| Complex Column + Complex Field Paths | `Field="Name.FirstName"` remains a leaf data-binding feature; do not accidentally force stacked-header rendering semantics onto dot-path columns | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```text
Mode: {feature-implementation | bug-fix | rendering-analysis}
Skill: Complex Column
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

Use this skill when the issue involves nested `GridColumn.Columns`, stacked headers, complex header DOM, rowspan/colspan bugs, frozen/virtualized stacked headers, or stacked-header reorder/visibility behavior.

Key rules:
- Treat complex columns as a header-tree feature first.
- Distinguish it from dot-path data fields like `Name.FirstName`; those are data-binding features, not stacked-header DOM features.
- Primary files: `src/Internal/Renderer/GridHeader.razor`, `src/Internal/Renderer/GridHeaderCell.razor`, `src/Internal/Renderer/GridVirtualHeader.razor`, `src/SfGrid.razor.cs`, `scripts/sf-grid-fn.ts`, `scripts/reorder.ts`, `scripts/header-drag-drop.ts`, `scripts/freeze.ts`.
- Rendering pipeline:
  1. detect nested `GridColumn.Columns`
  2. measure depth
  3. group columns by level
  4. generate cells with rowspan/colspan
  5. emit `.e-stackedheadercell` + `.e-stackedheadercelldiv` for parent headers
- Body rows remain flat leaf-column rows.
- Width control, filter/sort/search/edit/export generally use flattened leaf columns from `GridUtils.GetColumns(...)`.
- Preserve parent visibility logic: a stacked parent should render only if at least one descendant leaf remains visible.
- Preserve same-level reorder restrictions and frozen-pane class/style logic.

Constraints:
- No API changes unless explicitly requested.
- No direct JS runtime calls outside existing interop paths.
- Do not introduce render-force hacks; preserve current Blazor lifecycle scheduling.
- Validate interactions with frozen columns, reorder, show/hide, virtualization, export, and keyboard focus.
- Consult `/docs/training/` before making changes.

Output:
1. Root cause / design note
2. Modified method(s) only
3. DOM contract preserved
4. Regression tests required
```