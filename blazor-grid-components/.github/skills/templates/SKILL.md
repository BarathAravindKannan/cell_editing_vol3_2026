---
name: templates
description: Expert knowledge for the Templates feature in Syncfusion Blazor Data Grid. Use this skill for any feature-implementation or bug-fix task scoped to grid templates behaviour, including row templates, detail templates, column templates, edit templates, and aggregate templates, plus comprehensive DOM rendering structure and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 25 words -->

**Purpose**

Deep expertise on all template types in Blazor Data Grid, DOM rendering structure, template context objects, rendering flow across feature combinations (VirtualScroll, Freeze, Grouping), and interaction guarantees with export/validation systems.

---

**Agent Invocation**

Load this skill when:
- Creating or fixing any template rendering (row, detail, column, edit, aggregate, filter, toolbar, pager, tooltip, column-chooser)
- Debugging template context data flow
- Implementing template rendering under virtual scroll, freeze, or grouping
- Handling template validation or export callbacks
- Investigating template CSS class application or role attributes

---

## Knowledge References
<!-- token-budget: 60 words -->

**Files Consulted:**
- `src/GridTemplates.cs` — Grid-level templates
- `src/GridColumn.cs` — Column-level templates
- `src/GridEditSettings.cs` — Edit dialog templates
- `src/GridGroupSettings.razor.cs` — Group caption template
- `src/GridAggregateColumn.razor.cs` — Aggregate templates
- `src/GridPageSettings.razor.cs` — Pager template
- `src/GridColumnChooserSettings.cs` — Column chooser templates
- `src/EventModels/Grids.cs` — Most `*TemplateContext` classes (exception: `PagerTemplateContext` comes from the `Syncfusion.Blazor.Navigations` namespace)
- `src/GridEvents.cs` — Template-related event callbacks
- `src/Interfaces/IGrid.cs` — Template interface contracts
- `src/Internal/Renderer/**` — 15+ renderer components

---

## Training Insights Applied
<!-- token-budget: 80 words -->

**Key Rules:**

1. **Template Context Passing**: Context data is always passed as `context` implicit parameter. Type varies by template type (see Context Mapping table).
2. **Event Aggregator Triggers**: `Parent.EventAggregator.Trigger("RowTemplateData" | "EditTemplateData", data)` fires before template render to allow external subscriptions.
3. **Null Checks**: All template properties are nullable `RenderFragment<T>?`. Check `!= null` before invocation.
4. **CSS Class Naming**: Grid cells use `e-templatecell` class to mark template-rendered content; aggregates use `e-summarycell e-templatecell`.
5. **colspan Calculation**: Empty record template always spans full column count. Detail template expands to nested grid width.
6. **VirtualScroll Effect**: Virtual scroll uses separate `GridVirtualContent.razor` renderer; template checks remain identical but DOM is replaced on scroll.

---

## Code Location Map
<!-- token-budget: 100 words -->

### Template Definition Layer

| Template Type | File | Property | Context Type |
|---|---|---|---|
| Row | `GridTemplates.cs` | `RowTemplate` | `TValue` (data row) |
| Detail | `GridTemplates.cs` | `DetailTemplate` | `TValue` (parent row) |
| Empty Record | `GridTemplates.cs` | `EmptyRecordTemplate` | `EmptyRecordTemplateContext` |
| Tooltip | `GridTemplates.cs` | `TooltipTemplate` | `TooltipTemplateContext` |
| Toolbar | `GridTemplates.cs` | `ToolbarTemplate` | `object` |
| Column Cell | `GridColumn.cs` | `Template` | `TValue` (row data) |
| Column Header | `GridColumn.cs` | `HeaderTemplate` | `GridColumn` (column def) |
| Column Edit | `GridColumn.cs` | `EditTemplate` | `TValue` (row data) |
| Column Filter | `GridColumn.cs` | `FilterTemplate` | `object` (filter model) |
| Column Filter Item | `GridColumn.cs` | `FilterItemTemplate` | `FilterItemTemplateContext` |
| Edit Dialog | `GridEditSettings.cs` | `Template` | `TValue` (row data) |
| Edit Header | `GridEditSettings.cs` | `HeaderTemplate` | `TValue` (row data) |
| Edit Footer | `GridEditSettings.cs` | `FooterTemplate` | `TValue` (row data) |
| Edit Validator | `GridEditSettings.cs` | `Validator` | `ValidatorTemplateContext` |
| Group Caption | `GridGroupSettings.razor.cs` | `CaptionTemplate` | `CaptionTemplateContext` |
| Pager | `GridPageSettings.razor.cs` | `Template` | `PagerTemplateContext` (from `Syncfusion.Blazor.Navigations`) |
| Aggregate Footer | `GridAggregateColumn.razor.cs` | `FooterTemplate` | `AggregateTemplateContext` |
| Aggregate Group Caption | `GridAggregateColumn.razor.cs` | `GroupCaptionTemplate` | `AggregateTemplateContext` |
| Aggregate Group Footer | `GridAggregateColumn.razor.cs` | `GroupFooterTemplate` | `AggregateTemplateContext` |
| Column Chooser | `GridColumnChooserSettings.cs` | `Template` | `ColumnChooserTemplateContext` |
| Column Chooser Footer | `GridColumnChooserSettings.cs` | `FooterTemplate` | `ColumnChooserFooterTemplateContext` |

### Template Context Classes (EventModels/Grids.cs)

| Context Class | Properties | Used By |
|---|---|---|
| `EmptyRecordTemplateContext` | (empty placeholder) | `EmptyRecordTemplate` |
| `TooltipTemplateContext` | `Value`, `Column`, `ColIndex`, `RowIndex`, `IsHeaderCell` | `TooltipTemplate` |
| `AggregateTemplateContext` | Dynamic properties (aggregate values), `Type` | Footer, GroupCaption, GroupFooter templates |
| `CaptionTemplateContext` | `Key`, `Count`, `HeaderText`, `Field`, `ForeignKey`, `ForeignKeyValue`, `GroupAggregates` | `CaptionTemplate` |
| `FilterItemTemplateContext` | `Value`, `Column` | `FilterItemTemplate` |
| `ValidatorTemplateContext` | `Data`, `Column` | `Validator` |
| `ColumnChooserTemplateContext` | `Columns` (list) | `ColumnChooserSettings.Template` |
| `ColumnChooserFooterTemplateContext` | Action delegates for button clicks | `ColumnChooserSettings.FooterTemplate` |

### Renderer Components

| Renderer File | Renders | Template Property(ies) |
|---|---|---|
| `GridRow.razor` | Single row (base) | Checks `IsTemplate` flag, invokes `RowTemplate` |
| `GridContent.razor` | Content rows (normal scroll) | `DetailTemplate` presence check, `EmptyRecordTemplate` TD |
| `GridVirtualContent.razor` | Content rows (virtual scroll) | `DetailTemplate` presence check, `EmptyRecordTemplate` TD |
| `GridDetailRow.razor` | Expanded detail row | Renders `DetailTemplate` in TD colspan |
| `GridHeaderCell.razor` | Header cell | Renders `HeaderTemplate` if present |
| `FooterContent.razor` | Footer row cells | Renders aggregate `FooterTemplate` in TD |
| `GroupCaptionRenderer.cs` | Group caption row | Builds `CaptionTemplateContext`, renders `CaptionTemplate` |
| `GridTooltip.razor` | Tooltip content | Renders `TooltipTemplate` inside `<ContentTemplate>` |
| `NormalEdit.razor` | Inline/batch edit | `EditSettings.Template`, `Column.EditTemplate`, `Validator` (~32 lines) |
| `BatchEdit.razor` | Batch cell edit | `Column.EditTemplate`, validation tooltip rendering (SRP: +15 lines) |
| `DialogEdit.razor` | Dialog edit mode | `EditSettings.HeaderTemplate`, `Template`, `FooterTemplate`, `Column.EditTemplate` (SRP: +30 lines) |
| `GridToolbar.razor` | Toolbar buttons | `ToolbarTemplate` custom fragments |
| `ColumnChooser.razor` | Column chooser dialog | `ColumnChooserSettings.Template`, `FooterTemplate` |
| `Filter/FilterMenuRenderer.razor` | Filter menu dialog | `Column.FilterTemplate` |
| `Filter/FilterCheckBoxRenderer.razor` | Checkbox filter | Filter item logic |

---

## DOM Rendering Structure with ASCII Diagrams
<!-- token-budget: 250 words -->

### 1. Base Grid Structure (Normal Scroll)

**ASCII Tree Diagram:**
```
e-grid
├── e-gridheader
│   └── table
│       └── thead
│           └── tr.e-columnheader
│               ├── th.e-headercell  [Col 1]
│               │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ──┐
│               │        HeaderTemplate renders here                  │ ◄── GridHeaderCell.razor
│               │        @column.HeaderTemplate(column)               │
│               │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ──┘
│               ├── th.e-headercell  [Col 2 — no HeaderTemplate]
│               └── th.e-headercell  [Col N]
│
├── e-gridcontent
│   └── table
│       └── tbody
│           │
│           ├── [IF RowTemplate set] ────────────────────────────────┐
│           │   tr.e-row.e-templaterow                               │ ◄── GridRow.razor
│           │   │  EventAggregator.Trigger("RowTemplateData", data)  │    (IsTemplate=true)
│           │   └── @GridTemplates.RowTemplate(rowData)              │
│           │       └── [user-defined <td> × N columns]             │
│           │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘
│           │
│           ├── [NORMAL ROW — RowTemplate NOT set] ─────────────────┐
│           │   tr.e-row                                             │ ◄── GridRow.razor
│           │   ├── td.e-rowcell                                     │    (IsTemplate=false)
│           │   │   └── [IF Column.Template set]                     │
│           │   │       @column.Template(rowData)  [e-templatecell]  │
│           │   │   └── [ELSE] cell value text                       │
│           │   ├── td.e-rowcell  [Col 2 value]                      │
│           │   └── td.e-rowcell  [Col N value]                      │
│           │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘
│           │
│           ├── [DETAIL ROW — DetailTemplate set, initially hidden] ─┐
│           │   tr.e-detailrow  [display:none]                       │ ◄── GridDetailRow.razor
│           │   └── td.e-detailcell [colspan=N]                      │
│           │       └── @GridTemplates.DetailTemplate(rowData)       │
│           │           └── [nested SfGrid or custom markup]         │
│           │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘
│           │
│           └── [EMPTY ROW — no data] ───────────────────────────────┐
│               tr.e-emptyrow                                         │ ◄── GridContent.razor
│               └── td.e-emptycell [colspan=N]                       │
│                   └── @GridTemplates.EmptyRecordTemplate(ctx)       │
│                   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘
│
└── e-gridfooter
    └── table
        └── tfoot
            └── tr.e-summaryrow
                ├── td.e-summarycell.e-templatecell  [Col 1]
                │   └── @AggregateColumn.FooterTemplate(aggregateCtx) ◄── FooterContent.razor
                └── td.e-summarycell                 [Col 2 — no template, raw value]
```

**HTML DOM Reference:**
```html
<div class="e-grid">
  <div class="e-gridheader">
    <table>
      <thead>
        <tr class="e-columnheader">
          <th class="e-headercell">
            @column.HeaderTemplate(column)   <!-- HeaderTemplate injection point -->
          </th>
        </tr>
      </thead>
    </table>
  </div>

  <div class="e-gridcontent">
    <table>
      <tbody>
        <!-- CASE A: RowTemplate set — IsTemplate = true -->
        <tr class="e-row e-templaterow">
          @((IGrid)Parent).GridTemplates.RowTemplate(rowData)
          <!-- Must return exactly N <td> elements -->
        </tr>

        <!-- CASE B: RowTemplate NOT set — normal row with Column.Template -->
        <tr class="e-row">
          <td class="e-rowcell e-templatecell">  <!-- e-templatecell added when Template set -->
            @column.Template(rowData)
          </td>
          <td class="e-rowcell">plain cell value</td>
        </tr>

        <!-- CASE C: DetailTemplate present — hidden until user expands -->
        <tr class="e-detailrow" style="display: none;">
          <td class="e-detailcell" colspan="N">
            @((IGrid)Parent).GridTemplates.DetailTemplate(rowData)
          </td>
        </tr>

        <!-- CASE D: No data — EmptyRecordTemplate -->
        <tr class="e-emptyrow">
          <td class="e-emptycell" colspan="N">
            @((IGrid)Parent).GridTemplates.EmptyRecordTemplate(new EmptyRecordTemplateContext())
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <div class="e-gridfooter">
    <table>
      <tfoot>
        <tr class="e-summaryrow">
          <td class="e-summarycell e-templatecell">
            @cell.AggregateColumn.FooterTemplate(aggregateContext)
          </td>
        </tr>
      </tfoot>
    </table>
  </div>
</div>
```

### 2. Group + Aggregate Structure

**ASCII Tree Diagram:**
```
e-gridcontent
└── table
    └── tbody
        │
        ├── [GROUP BLOCK: "Category = Electronics"] ─────────────────────────────────────┐
        │   │                                                                              │
        │   ├── tr.e-groupcaption  [role="row"] ─────────────────────────────────────┐   │
        │   │   └── td  [colspan=N, class="e-indentcell + e-groupcaptionrow"]        │   │
        │   │       ├── [IF CaptionTemplate set]                                      │   │
        │   │       │   @GroupSettings.CaptionTemplate(captionContext)  ◄── GroupCaptionRenderer.cs
        │   │       │   captionContext = {                                             │   │
        │   │       │     Key        : "Electronics"                                  │   │
        │   │       │     Count      : 5                                              │   │
        │   │       │     HeaderText : "Category"                                     │   │
        │   │       │     GroupAggregates : AggregateTemplateContext                  │   │
        │   │       │   }                                                              │   │
        │   │       └── [ELSE] default "Category: Electronics - 5 items"             │   │
        │   │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ───────┘   │
        │   │                                                                              │
        │   ├── tr.e-row  [data row 1]                                                    │
        │   │   └── td.e-rowcell  [Col 1] ... [Col N]                                    │
        │   │       └── @column.Template(rowData) [if Template set]                      │
        │   │                                                                              │
        │   ├── tr.e-row  [data row 2]                                                    │
        │   │   └── td.e-rowcell  ...                                                    │
        │   │                                                                              │
        │   └── tr.e-groupfooter ──────────────────────────────────────────────────────┐  │
        │       ├── td.e-summarycell.e-templatecell  [aggregate col]                   │  │
        │       │   └── @AggregateColumn.GroupFooterTemplate(aggregateContext)  ◄── FooterContent.razor
        │       └── td.e-summarycell  [non-template aggregate col]                    │  │
        │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────────┘  │
        │                                                                                  │
        └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘

        [REPEAT GROUP BLOCK for each distinct group key]
```

**HTML DOM Reference:**
```html
<tbody>
  <!-- GROUP CAPTION ROW -->
  <tr class="e-groupcaption" role="row">
    <td class="e-indentcell e-groupcaptionrow" colspan="N">
      <!-- IF CaptionTemplate set -->
      @Parent.GroupSettings.CaptionTemplate(captionContext)
      <!--
        captionContext = CaptionTemplateContext {
          Key            = "Electronics",
          Count          = 5,
          HeaderText     = "Category",
          Field          = "Category",
          ForeignKey     = null,
          GroupAggregates = AggregateTemplateContext { Sum = 1200, Count = 5 }
        }
      -->
    </td>
  </tr>

  <!-- DATA ROWS WITHIN THIS GROUP -->
  <tr class="e-row">
    <td class="e-rowcell">...</td>
  </tr>

  <!-- GROUP FOOTER ROW (aggregate summary at group end) -->
  <tr class="e-groupfooter">
    <td class="e-summarycell">plain aggregate value</td>
    <td class="e-summarycell e-templatecell">
      <!-- IF GroupFooterTemplate set on AggregateColumn -->
      @AggregateColumn.GroupFooterTemplate(aggregateContext)
    </td>
  </tr>
</tbody>
```

### 3. Virtual Scroll Structure

**ASCII Tree Diagram — Viewport Concept:**
```
Total Data: 1000 rows
Viewport height: 500px  |  Row height: 40px  →  ~12 rows visible at a time

 ┌─────────────────────────────────────────────────────────────────┐
 │  e-grid                                                         │
 │  └── e-gridcontent  [overflow: hidden; height: 500px]          │
 │      └── e-virtualtable  [position: relative]                  │
 │          │                                                      │
 │          │  ┌──────────────────────────────────────────────┐   │
 │          │  │  e-virtualtrack  [height = 1000 × 40 = 40000px]│  │ ← full scroll height
 │          │  │                                               │   │
 │          │  │  table  [position: absolute; top: scrollTop]  │   │
 │          │  │  └── tbody                                    │   │
 │          │  │      │                                        │   │   Only rendered rows
 │          │  │      ├── tr.e-row [row index 0]  ◄───────────┼───┼── visible in viewport
 │          │  │      │   └── td  @column.Template(data[0])   │   │
 │          │  │      ├── tr.e-row [row index 1]               │   │
 │          │  │      │   └── td  @column.Template(data[1])   │   │
 │          │  │      ├── tr.e-row [row index 2]               │   │
 │          │  │      │   └── @GridTemplates.RowTemplate(...)  │   │  ← RowTemplate works same
 │          │  │      │       (IsTemplate = true)               │   │
 │          │  │      ├── tr.e-row [row index 3..11]           │   │
 │          │  │      │   └── ...                              │   │
 │          │  │      └── ── ── ── ── ── ── ── ── ── ── ───────┘   │
 │          │  │    rows 12–999: NOT in DOM (recycled on scroll)    │
 │          │  └──────────────────────────────────────────────────┘   │
 │          │                                                      │
 │  ┌───────┴──────────────────────────────────────────────────┐  │
 │  │ Scroll Event → recalculate visible range → re-render     │  │
 │  │ new rows in tbody. Template logic UNCHANGED.             │  │
 │  └──────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────┘

DetailTemplate State across scroll:
  Row 3 expanded → detail row rendered below tr[3]
  User scrolls → tr[3] exits viewport → detail tr removed from DOM
  User scrolls back → tr[3] re-enters viewport → DetailTemplate re-invoked
  State (expanded/collapsed) preserved in memory, NOT in DOM
```

**HTML DOM Reference:**
```html
<div class="e-grid">
  <div class="e-gridcontent">
    <!-- GridVirtualContent.razor (replaces GridContent.razor) -->
    <div class="e-virtualtable">
      <div class="e-virtualtrack" style="height: 40000px;">  <!-- total scroll height -->
        <table style="position: absolute; top: [scrollTop]px;">
          <tbody>
            <!-- ONLY rows[0..11] rendered initially -->
            <!-- Template logic IDENTICAL to base grid -->
            <tr class="e-row">
              <td class="e-rowcell e-templatecell">
                @column.Template(rowData)
              </td>
            </tr>
            <!-- EmptyRecordTemplate still renders if data is empty -->
            <tr class="e-emptyrow">
              <td colspan="N">
                @((IGrid)Parent).GridTemplates.EmptyRecordTemplate(new EmptyRecordTemplateContext())
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</div>
```

### 4. Freeze Column Template Structure

**ASCII Tree Diagram — Side-by-Side Layout:**
```
 ┌─────────────────────────────────────────────────────────────────────────────────┐
 │  e-grid                                                                         │
 │                                                                                 │
 │  ┌──── e-gridheader ───────────────────────────────────────────────────────┐   │
 │  │                                                                          │   │
 │  │  ┌─ e-frozenheader [fixed, no scroll] ─┐  ┌─ e-movableheader [scroll] ─┐ │   │
 │  │  │  table > thead                      │  │  table > thead              │ │   │
 │  │  │  ├── th [Col 1 - frozen]            │  │  ├── th [Col 3 - movable]   │ │   │
 │  │  │  │   @col.HeaderTemplate(col) ◄─────┤  │  │   @col.HeaderTemplate() ◄┤ │   │
 │  │  │  └── th [Col 2 - frozen]            │  │  └── th [Col N - movable]   │ │   │
 │  │  └─────────────────────────────────────┘  └──────────────────────────── ┘ │   │
 │  └──────────────────────────────────────────────────────────────────────────┘   │
 │                                                                                 │
 │  ┌──── e-gridcontent ──────────────────────────────────────────────────────┐   │
 │  │  display: flex                                                            │   │
 │  │                                                                          │   │
 │  │  ┌─ e-frozencontent [fixed] ──────────┐  ┌─ e-movablecontent [scroll] ──┐ │   │
 │  │  │  table > tbody                     │  │  table > tbody                │ │   │
 │  │  │                                    │  │                               │ │   │
 │  │  │  tr.e-row [row 1]                  │  │  tr.e-row [row 1]             │ │   │
 │  │  │  ├── td.e-rowcell.e-freezecell     │  │  ├── td.e-rowcell             │ │   │
 │  │  │  │   @col1.Template(rowData) ◄─────┤  │  │   @col3.Template(rowData)◄─┤ │   │
 │  │  │  └── td.e-rowcell.e-freezecell     │  │  └── td.e-rowcell             │ │   │
 │  │  │      @col2.Template(rowData) ◄─────┤  │      [plain value]            │ │   │
 │  │  │                                    │  │                               │ │   │
 │  │  │  tr.e-row [row 2] ...              │  │  tr.e-row [row 2] ...         │ │   │
 │  │  │                                    │  │                               │ │   │
 │  │  │  ← FROZEN: no horizontal scroll →  │  │  ← SCROLLS horizontally →    │ │   │
 │  │  └────────────────────────────────────┘  └──────────────────────────────┘ │   │
 │  │                                                                            │   │
 │  │  DetailTemplate: expands ONLY in e-movablecontent (right side)            │   │
 │  │  CaptionTemplate: spans FULL width via td colspan=total_cols              │   │
 │  └──────────────────────────────────────────────────────────────────────────┘   │
 └─────────────────────────────────────────────────────────────────────────────────┘

Template behaviour per section:
  Col 1 (frozen)  → @col1.Template(rowData)  with e-freezecell CSS class
  Col 2 (frozen)  → @col2.Template(rowData)  with e-freezecell CSS class
  Col 3 (movable) → @col3.Template(rowData)  normal e-rowcell
  DetailTemplate  → renders ONLY inside e-movablecontent
  CaptionTemplate → single TD colspan=N spans both sections
```

**HTML DOM Reference:**
```html
<div class="e-grid">
  <div class="e-gridcontent" style="display: flex;">

    <!-- FROZEN SECTION (fixed, does not scroll horizontally) -->
    <div class="e-frozencontent" style="width: 200px; flex-shrink: 0;">
      <table>
        <tbody>
          <tr class="e-row">
            <td class="e-rowcell e-freezecell">
              @frozenColumn.Template(rowData)   <!-- e-freezecell added by renderer -->
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MOVABLE SECTION (scrolls horizontally) -->
    <div class="e-movablecontent" style="flex: 1; overflow-x: auto;">
      <table>
        <tbody>
          <tr class="e-row">
            <td class="e-rowcell">
              @movableColumn.Template(rowData)  <!-- normal e-rowcell, no e-freezecell -->
            </td>
          </tr>

          <!-- DetailTemplate ONLY appears here, NOT in frozen section -->
          <tr class="e-detailrow">
            <td class="e-detailcell" colspan="movableColCount">
              @((IGrid)Parent).GridTemplates.DetailTemplate(rowData)
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>

  <!-- Group CaptionTemplate spans FULL width -->
  <!-- Rendered as single <td colspan="totalCols"> across both sections -->
</div>
```

### 5. Edit Template (Inline Mode) Structure

**ASCII Tree Diagram:**
```
tbody
│
├── tr.e-row  [row above — read-only]
│   └── td.e-rowcell ...
│
├── ── ── EDIT ROW (replaces the selected row) ── ── ── ── ── ── ── ── ── ── ──┐
│   tr.e-editrow                                                                 │ ◄── NormalEdit.razor
│   │                                                                            │
│   ├── CASE A: EditSettings.Template set ──────────────────────────────────┐   │
│   │   td  [colspan=N]                                                     │   │
│   │   └── EventAggregator.Trigger("EditTemplateData", rowData)            │   │
│   │   └── @EditSettings.Template(rowData)                                 │   │
│   │       └── [full custom form — user owns all inputs]                   │   │
│   │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘   │
│   │                                                                            │
│   ├── CASE B: EditSettings.Template NOT set — column-by-column ───────────┐   │
│   │   ├── td.e-editcell  [Col 1]                                          │   │
│   │   │   ├── [IF col.EditTemplate set]                                   │   │
│   │   │   │   EventAggregator.Trigger("EditTemplateData", rowData)        │   │
│   │   │   │   @col.EditTemplate(rowData)                                  │   │
│   │   │   └── [ELSE] default editor (TextBox / NumericBox / DatePicker)   │   │
│   │   ├── td.e-editcell  [Col 2]                                          │   │
│   │   │   └── @col.EditTemplate(rowData)  OR  <SfTextBox>                │   │
│   │   └── td.e-editcell  [Col N]                                          │   │
│   │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘   │
│   │                                                                            │
│   └── VALIDATOR (overlaid on edit row if Validator set) ──────────────────┐   │
│       div.e-validation                                                     │   │
│       └── @EditSettings.Validator(new ValidatorTemplateContext {           │   │
│               Data = rowData, Column = column                              │   │
│           })                                                                │   │
│       └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘   │
│                                                                                 │
└── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘
│
└── tr.e-row  [row below — read-only]
    └── td.e-rowcell ...
```

**HTML DOM Reference:**
```html
<tbody>
  <!-- CASE A: EditSettings.Template set — full-width custom form -->
  <tr class="e-editrow">
    <td colspan="N">
      <!-- EventAggregator fired before render -->
      @Parent.EditSettings.Template(rowData)
    </td>
  </tr>

  <!-- CASE B: EditSettings.Template NOT set — column-by-column edit -->
  <tr class="e-editrow">
    <td class="e-editcell">
      <!-- Column.EditTemplate OR default editor -->
      @if (col.EditTemplate != null)
        @col.EditTemplate(rowData)   <!-- EventAggregator fired before render -->
      else
        <input class="e-input" type="text" value="@cellValue" />
    </td>
    <td class="e-editcell">
      @col2.EditTemplate(rowData)
    </td>
  </tr>

  <!-- VALIDATOR — rendered alongside edit row -->
  <div class="e-validation">
    @Parent.EditSettings.Validator(new ValidatorTemplateContext { Data = rowData, Column = col })
  </div>
</tbody>
```

### 6. Edit Template (Dialog Mode) Structure

**ASCII Tree Diagram:**
```
 ┌──── SfDialog (e-dialog.e-edit-dialog) ──────────────────────────────────────────┐
 │                                                                                  │
 │  ┌─ e-dlg-header ─────────────────────────────────────────────────────────┐    │
 │  │  [IF EditSettings.HeaderTemplate set]                                   │    │
 │  │  @EditSettings.HeaderTemplate(rowData)  ◄── DialogEdit.razor           │    │
 │  │  [ELSE] default title "Edit Record"                                     │    │
 │  └─────────────────────────────────────────────────────────────────────────┘    │
 │                                                                                  │
 │  ┌─ e-dlg-content ────────────────────────────────────────────────────────┐    │
 │  │                                                                         │    │
 │  │  CASE A: EditSettings.Template set ─────────────────────────────────┐  │    │
 │  │  │  EventAggregator.Trigger("EditTemplateData", rowData)            │  │    │
 │  │  │  @EditSettings.Template(rowData)                                 │  │    │
 │  │  │  └── [full custom form — user owns all fields]                   │  │    │
 │  │  └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘  │    │
 │  │                                                                         │    │
 │  │  CASE B: EditSettings.Template NOT set — column-by-column form ──────┐  │    │
 │  │  │  div.e-form-group  [Col 1]                                        │  │    │
 │  │  │  ├── label  "Column Header"                                       │  │    │
 │  │  │  └── [IF col.EditTemplate set]                                    │  │    │
 │  │  │      EventAggregator.Trigger("EditTemplateData", rowData)         │  │    │
 │  │  │      @col.EditTemplate(rowData)                                   │  │    │
 │  │  │      [ELSE] <SfTextBox> / <SfNumericTextBox> / <SfDatePicker>    │  │    │
 │  │  │  div.e-form-group  [Col 2]                                        │  │    │
 │  │  │  ├── label  ...                                                   │  │    │
 │  │  │  └── @col.EditTemplate(rowData)  OR  default editor              │  │    │
 │  │  │  ...                                                              │  │    │
 │  │  │  [Validator template — shown on validation failure]               │  │    │
 │  │  │  @EditSettings.Validator(ValidatorTemplateContext{...})           │  │    │
 │  │  └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘  │    │
 │  │                                                                         │    │
 │  └─────────────────────────────────────────────────────────────────────────┘    │
 │                                                                                  │
 │  ┌─ e-dlg-footer ─────────────────────────────────────────────────────────┐    │
 │  │  [IF EditSettings.FooterTemplate set]                                   │    │
 │  │  @EditSettings.FooterTemplate(rowData)  ◄── DialogEdit.razor           │    │
 │  │  [ELSE] default "Update" + "Cancel" buttons                            │    │
 │  └─────────────────────────────────────────────────────────────────────────┘    │
 └──────────────────────────────────────────────────────────────────────────────────┘
```

**HTML DOM Reference:**
```html
<!-- SfDialog rendered by DialogEdit.razor -->
<div class="e-dialog e-edit-dialog" style="width: 330px;">

  <div class="e-dlg-header">
    <!-- IF EditSettings.HeaderTemplate set -->
    @Parent.EditSettings.HeaderTemplate(rowData)
    <!-- ELSE: default title -->
  </div>

  <div class="e-dlg-content">

    <!-- CASE A: Full custom form template -->
    @if (EditSettings.Template != null) {
      @Parent.EditSettings.Template(rowData)   <!-- EventAggregator fired first -->
    }

    <!-- CASE B: Column-by-column auto-generated form -->
    @foreach (var col in Columns) {
      <div class="e-form-group">
        <label>@col.HeaderText</label>
        @if (col.EditTemplate != null)
          @col.EditTemplate(rowData)          <!-- EventAggregator fired first -->
        else
          <input class="e-input" type="text" />  <!-- or SfNumericTextBox etc. -->
      </div>
    }

    <!-- Validator renders inline on invalid input -->
    @Parent.EditSettings.Validator(new ValidatorTemplateContext { Data = rowData })
  </div>

  <div class="e-dlg-footer">
    <!-- IF EditSettings.FooterTemplate set -->
    @Parent.EditSettings.FooterTemplate(rowData)
    <!-- ELSE: default Update + Cancel buttons -->
  </div>

</div>
```

### 7. Filter Template (Menu Mode) Structure

**ASCII Tree Diagram:**
```
 ┌──── Filter Menu Dialog (e-filtermenu) ─── FilterMenuRenderer.razor ─────────────┐
 │                                                                                   │
 │  ┌─ e-dlg-content ────────────────────────────────────────────────────────┐     │
 │  │                                                                         │     │
 │  │  CASE A: Column.FilterTemplate set ─────────────────────────────────┐  │     │
 │  │  │  @Column.FilterTemplate(filterModel)                             │  │     │
 │  │  │  └── [user-defined filter input: SfDatePicker, SfSlider, etc.]   │  │     │
 │  │  └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘  │     │
 │  │                                                                         │     │
 │  │  CASE B: No FilterTemplate — standard filter UI ────────────────────┐  │     │
 │  │  │  <input class="e-input" type="text" />                           │  │     │
 │  │  │  OR  <SfNumericTextBox> / <SfDatePicker> by column type          │  │     │
 │  │  └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘  │     │
 │  │                                                                         │     │
 │  │  Action buttons: [Filter]  [Clear]                                      │     │
 │  └─────────────────────────────────────────────────────────────────────────┘     │
 └───────────────────────────────────────────────────────────────────────────────────┘

 ┌──── Checkbox / Excel Filter Dialog ─── FilterCheckBoxRenderer.razor ─────────────┐
 │                                                                                   │
 │  div.e-checkboxfilter                                                             │
 │  ├── div.e-searchinput  [search box]                                             │
 │  └── ul.e-checklist                                                              │
 │      ├── li.e-item                                                               │
 │      │   ├── input[type="checkbox"]                                             │
 │      │   └── span  [IF FilterItemTemplate NOT set] → plain text value           │
 │      │       [IF FilterItemTemplate set] ─────────────────────────────────────┐ │
 │      │       @Column.FilterItemTemplate(                                       │ │
 │      │         new FilterItemTemplateContext {                                  │ │
 │      │           Value  = "Electronics",                                        │ │
 │      │           Column = column                                                │ │
 │      │         }                                                                 │ │
 │      │       )  ◄── FilterCheckBoxRenderer.razor.cs                            │ │
 │      │       └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────────┘ │
 │      ├── li.e-item  [next value]                                                │
 │      └── ...                                                                    │
 └───────────────────────────────────────────────────────────────────────────────────┘
```

**HTML DOM Reference:**
```html
<!-- Filter Menu (FilterMenuRenderer.razor) -->
<div class="e-filtermenu e-popup">
  <div class="e-dlg-content">

    <!-- CASE A: Column.FilterTemplate set -->
    @if (Column.FilterTemplate != null) {
      @Column.FilterTemplate(filterModel)   <!-- filterModel = object with current filter value -->
    }

    <!-- CASE B: Default filter inputs -->
    else {
      <input class="e-input e-filterinput" type="text" />
    }

    <div class="e-flm-actioncnt">
      <button class="e-flmenu-okbtn">Filter</button>
      <button class="e-flmenu-cancelbtn">Clear</button>
    </div>
  </div>
</div>

<!-- Checkbox Filter (FilterCheckBoxRenderer.razor) -->
<div class="e-checkboxfilter">
  <ul class="e-ul e-checklist">
    <li class="e-item">
      <input type="checkbox" />
      <!-- IF FilterItemTemplate set -->
      @Column.FilterItemTemplate(new FilterItemTemplateContext {
        Value  = "Electronics",
        Column = column
      })
      <!-- ELSE: plain label text -->
    </li>
  </ul>
</div>
```

### 8. Tooltip Template Structure

**ASCII Tree Diagram:**
```
User hovers over cell
        │
        ▼
 GridTooltip.razor intercepts mouse event
        │
        ▼
 Populate TooltipTemplateContext:
 {
   Value       = "Laptop Pro 15"   ← cell value (or bound value for template cols)
   Column      = GridColumn ref
   ColIndex    = 2                  ← 0-based column index
   RowIndex    = 4                  ← 0-based row index
   IsHeaderCell = false             ← true only when hovering header TH
 }
        │
        ▼
 ┌──── SfTooltip popup ─────────────────────────────────────────────────────┐
 │                                                                           │
 │  div.e-tooltip  [role="tooltip"]                                         │
 │  └── div.e-tooltip-content                                               │
 │      │                                                                   │
 │      ├── [IF GridTemplates.TooltipTemplate set] ──────────────────────┐  │
 │      │   @((IGrid)Parent).GridTemplates.TooltipTemplate(tooltipInfo)  │  │  ◄── GridTooltip.razor
 │      │   └── [user-defined: span, img, formatted text, etc.]          │  │
 │      │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘  │
 │      │                                                                   │
 │      └── [ELSE] plain text: cellValue.ToString()                        │
 │                                                                           │
 └───────────────────────────────────────────────────────────────────────────┘

Trigger conditions:
  ● ShowTooltip = true  (SfGrid property — MUST be set or TooltipTemplate never renders)
  ● Cell or header hovered for > [tooltipDelay]ms
  ● IsHeaderCell = true when TH is hovered

IsHeaderCell usage example:
  @if (context.IsHeaderCell)  → show column description
  @else                       → show formatted cell data
```

**HTML DOM Reference:**
```html
<!-- GridTooltip.razor — rendered as overlay popup -->
<div class="e-tooltip e-popup" role="tooltip">
  <div class="e-tooltip-content">

    <!-- IF TooltipTemplate set -->
    @((IGrid)Parent).GridTemplates.TooltipTemplate(
      new TooltipTemplateContext {
        Value        = cellValue,
        Column       = column,
        ColIndex     = colIndex,
        RowIndex     = rowIndex,
        IsHeaderCell = isHeader
      }
    )

    <!-- ELSE: default tooltip text -->

  </div>
</div>
```

### 9. Column Chooser Template Structure

**ASCII Tree Diagram:**
```
User clicks Column Chooser toolbar button
        │
        ▼
 ColumnChooser.razor opens SfDialog
        │
 ┌──── div.e-cc-dialog (SfDialog overlay) ────────────────────────────────────────┐
 │                                                                                 │
 │  div.e-dlg-header                                                              │
 │  └── "Column Chooser"  (default title, customizable via dialog header)         │
 │                                                                                 │
 │  div.e-dlg-content                                                             │
 │  │                                                                             │
 │  ├── [IF ColumnChooserSettings.Template set] ────────────────────────────────┐ │  ◄── ColumnChooser.razor
 │  │   @Parent.ColumnChooserSettings.Template(                                 │ │
 │  │     new ColumnChooserTemplateContext {                                     │ │
 │  │       Columns = List<GridColumn> (all columns)                            │ │
 │  │     }                                                                     │ │
 │  │   )                                                                       │ │
 │  │   └── [user-defined: search box, grouped checkboxes, drag list, etc.]    │ │
 │  │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────┘ │
 │  │                                                                             │
 │  └── [ELSE] default checkbox list ────────────────────────────────────────────│
 │      div.e-cc-list                                                             │
 │      └── foreach column → label > input[type=checkbox] + HeaderText            │
 │                                                                                 │
 │  div.e-dlg-footer                                                              │
 │  │                                                                             │
 │  ├── [IF ColumnChooserSettings.FooterTemplate set] ─────────────────────────┐ │  ◄── ColumnChooser.razor
 │  │   @Parent.ColumnChooserSettings.FooterTemplate(                          │ │
 │  │     new ColumnChooserFooterTemplateContext {                              │ │
 │  │       ApplyClick = EventCallback (shows/hides selected columns),         │ │
 │  │       ResetClick = EventCallback (restores all columns visible)          │ │
 │  │     }                                                                    │ │
 │  │   )                                                                      │ │
 │  │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ────────┘ │
 │  │                                                                             │
 │  └── [ELSE] default footer: "OK" + "Cancel" buttons                           │
 │                                                                                 │
 └─────────────────────────────────────────────────────────────────────────────────┘

Context classes:
  ColumnChooserTemplateContext  → Columns : List<GridColumn>
  ColumnChooserFooterTemplateContext → ApplyClick : EventCallback
                                       ResetClick : EventCallback

Note: Template replaces ENTIRE dialog body; FooterTemplate replaces only the footer strip.
Both can be set independently.
```

**HTML DOM Reference:**
```html
<!-- ColumnChooser.razor -->
<div class="e-cc-dialog">
  <div class="e-dlg-header">Column Chooser</div>

  <div class="e-dlg-content">
    <!-- IF ColumnChooserSettings.Template != null -->
    @Parent.ColumnChooserSettings.Template(
      new ColumnChooserTemplateContext { Columns = Columns }
    )

    <!-- ELSE: default checkbox list -->
    <div class="e-cc-list">
      @foreach (var col in Columns)
        <label><input type="checkbox" /> @col.HeaderText</label>
    </div>
  </div>

  <div class="e-dlg-footer">
    <!-- IF ColumnChooserSettings.FooterTemplate != null -->
    @Parent.ColumnChooserSettings.FooterTemplate(
      new ColumnChooserFooterTemplateContext {
        ApplyClick = () => { ... },
        ResetClick = () => { ... }
      }
    )
  </div>
</div>
```

### 10. Pager Template Structure

**ASCII Tree Diagram:**
```
 SfGrid renders after grid body
        │
        ▼
 GridPageSettings.razor → SfPager component
        │
 ┌──── div.e-pager ────────────────────────────────────────────────────────────────┐
 │                                                                                  │
 │  Populate PagerTemplateContext:                                                  │
 │  {                                                                               │
 │    TotalItemsCount = 500         ← total records in datasource                  │
 │    StartIndex      = 1           ← first record index on current page (1-based) │
 │    EndIndex        = 12          ← last record index on current page            │
 │    CurrentPage     = 1           ← current active page number (1-based)         │
 │    TotalPages      = 42          ← Math.Ceiling(TotalItemsCount / PageSize)     │
 │    PageSize        = 12          ← GridPageSettings.PageSize value              │
 │  }                                                                               │
 │                                                                                  │
 │  ├── [IF GridPageSettings.Template set] ─────────────────────────────────────┐  │  ◄── SfPager component
 │  │   @Parent.PageSettings.Template(pagerInfo)                                │  │
 │  │   └── [user-defined: custom nav buttons, page size selector, info text]  │  │
 │  │                                                                            │  │
 │  │   IMPORTANT: User is responsible for page navigation logic                │  │
 │  │   Use GoToPage(n) on the SfGrid ref to navigate programmatically          │  │
 │  │   └── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ─────────┘  │
 │  │                                                                              │
 │  └── [ELSE] default pager ───────────────────────────────────────────────────── │
 │      div.e-pagercontainer                                                        │
 │      ├── div.e-first  (|< First)                                                │
 │      ├── div.e-prev   (< Prev)                                                  │
 │      ├── div.e-numericcontainer (page number buttons: 1 2 3 4 5)               │
 │      ├── div.e-next   (Next >)                                                  │
 │      ├── div.e-last   (Last >|)                                                 │
 │      └── div.e-pagerconstant (Page size dropdown + "12 records per page" text) │
 │                                                                                  │
 └──────────────────────────────────────────────────────────────────────────────────┘

Context mutation — when page changes:
  DataStateChanged event fires → new PagerTemplateContext constructed → Template re-renders

VirtualScroll + Pager:
  ● Template still provided PagerTemplateContext but navigation via scrolling
  ● CurrentPage / TotalPages update as virtual viewport scrolls
```

**HTML DOM Reference:**
```html
<!-- GridPageSettings.razor → SfPager component -->
<div class="e-pager">
  <!-- IF GridPageSettings.Template != null -->
  @Parent.PageSettings.Template(
    new PagerTemplateContext {
      TotalItemsCount = totalRecords,
      StartIndex      = pageStart,
      EndIndex        = pageEnd,
      CurrentPage     = currentPage,
      TotalPages      = totalPages,
      PageSize        = pageSize
    }
  )

  <!-- ELSE: default pager UI -->
  <div class="e-pagercontainer">
    <span>Total: @totalRecords records</span>
    <button>&lt; Previous</button>
    <input type="number" />
    <button>Next &gt;</button>
  </div>
</div>
```

---

## Template Rendering Flow Across Feature Combinations
<!-- token-budget: 200 words -->

### Scenario 1: Base Grid + Row/Column Template

```
Initialization:
  1. Grid initializes with Data = [rows...]
  2. GridContent.razor checks:
     - DetailTemplate != null → Add expand icon to first column
     - RowTemplate != null → Set row.IsTemplate = true
  
Rendering Flow:
  3. For each row in Data:
     a. If row.IsTemplate == true:
        - GridRow.razor renders RowTemplate(row.Data)
        - EventAggregator.Trigger("RowTemplateData", row.Data)
        - RowTemplate must return N <td> elements (N = column count)
     
     b. If row.IsTemplate == false:
        - Render standard <tr> with <td> cells
        - For each column:
          - If Column.Template != null:
            - Render Column.Template(row.Data) in <td>
            - Add CSS class "e-templatecell"
          - Else: render cell value

  4. If DetailTemplate != null:
     - Add <tr class="e-detailrow"> after each row
     - When expand icon clicked:
       - Fire DetailRowExpanding event
       - Render DetailTemplate(row.Data) in TD colspan
       - Fire DetailRowExpanded event
```

### Scenario 2: Grouping + Aggregate + Caption Template

```
Initialization:
  1. GroupSettings.GroupByFields = ["Category"]
  2. GroupSettings.CaptionTemplate = CaptionTemplate delegate
  3. Aggregates defined (e.g., Sum, Count)

Rendering Flow:
  3. GroupModule.GenerateGroupedRows(data):
     a. Group rows by [Category]
     b. For each group:
        - Create GroupCaptionRow
        - Build CaptionTemplateContext:
          * Key = group key value ("Electronics")
          * Count = row count in group
          * HeaderText = column name ("Category")
          * GroupAggregates = AggregateTemplateContext (Sum, Count)
        
        - If CaptionTemplate != null:
          * Render CaptionTemplate(captionContext) in full-width TD
          * Add CSS "e-groupcaption"
        
        - If AggregateColumn.GroupCaptionTemplate != null:
          * Render GroupCaptionTemplate(aggregateContext) in aggregate column TD
        
        - Render group data rows
        
        - If AggregateColumn.GroupFooterTemplate != null:
          * Render GroupFooterTemplate(aggregateContext) at group end
```

### Scenario 3: Virtual Scroll + Row Template

```
Initialization:
  1. VirtualScroll enabled
  2. Row data = [1000+ rows]
  3. Viewport height = 500px, row height = 40px → ~12 rows visible

Rendering Flow:
  3. Initial render:
     a. GridVirtualContent.razor calculates visible range: rows[0-12]
     b. Renders only rows 0-12:
        - If row.IsTemplate == true:
          * Render RowTemplate(row) (same as base case)
        - Else:
          * Render normal row with Column.Template if present
     
  4. On scroll (e.g., scroll down 300px):
     a. Virtual scroll detects scroll position
     b. Recalculates visible range: rows[7-19]
     c. Replace entire tbody with new rows 7-19
     d. Template rendering logic UNCHANGED:
        - RowTemplate invoked for row.IsTemplate == true
        - Column.Template invoked for each column
     
  5. DetailTemplate behavior:
     a. If detail expanded at row 5:
        - Detail stays expanded during scroll
        - If row 5 scrolls out of view:
          * Detail <tr class="e-detailrow"> is removed from DOM
          * Collapsed state maintained internally
        - When row 5 scrolls back into view:
          * Detail row re-rendered (DetailTemplate re-invoked)
```

### Scenario 4: Freeze Columns + Column Template

```
Initialization:
  1. FreezeSettings.FrozenColumns = 2 (freeze first 2 columns)
  2. Column[0].Template = CellTemplate1
  3. Column[2].Template = CellTemplate2

Rendering Flow:
  3. Renderer creates two table sections:
     
     Frozen Section:
       <div class="e-frozencols" style="width: 200px;">
         <table>
           <tr>
             <td class="e-rowcell e-freezecell">
               @Column[0].Template(rowData)
               ← Template renders with e-freezecell class
             </td>
             <td class="e-rowcell e-freezecell">
               @Column[1].Template(rowData)
             </td>
           </tr>
         </table>
       </div>
     
     Unfrozen Section (scrollable):
       <div class="e-unfrozencols" style="overflow-x: auto;">
         <table>
           <tr>
             <td class="e-rowcell">
               @Column[2].Template(rowData)
               ← Template renders normally
             </td>
             ...
           </tr>
         </table>
       </div>
  
  4. Group caption:
     a. CaptionTemplate spans BOTH frozen and unfrozen sections
     b. DOM structure:
        <tr class="e-groupcaption">
          <td colspan="totalCols" class="e-spacecell">
            [Frozen area caption][Unfrozen area caption]
          </td>
        </tr>
  
  5. DetailTemplate:
     a. Expands ONLY in unfrozen section (on right side)
     b. Does NOT span frozen columns
```

### Scenario 5: Edit Mode + Edit Template

```
Initialization:
  1. EditSettings.Mode = Normal (inline)
  2. EditSettings.Template = CustomFormTemplate
  3. Column[0].EditTemplate = CustomColumnEditor

Rendering Flow (Inline):
  
  1. User clicks Edit button on row:
     a. Fire ActionBegin(EditActionEventArgs)
     b. Find row in data, set to edit state
  
  2. Renderer switches to NormalEdit.razor:
     a. If EditSettings.Template != null:
        - Render full-width edit row:
          <tr class="e-editrow">
            <td colspan="colCount">
              @EditSettings.Template(rowData)
            </td>
          </tr>
        - EventAggregator.Trigger("EditTemplateData", rowData)
     
     b. Else (EditSettings.Template == null):
        - Render column-by-column edit:
          <tr class="e-editrow">
            @foreach (col in Columns)
              <td class="e-editcell">
                @if (col.EditTemplate != null)
                  @col.EditTemplate(rowData)
                else
                  <input type="text" value="@GetCellValue(rowData, col)" />
              </td>
          </tr>
  
  3. Validation:
     a. If EditSettings.Validator != null:
        - Render ValidatorTemplateContext { Data = rowData, Column = col }
        - Display validation errors inline
  
  4. User clicks Save:
     a. Fire ActionComplete(EditActionEventArgs)
     b. Exit edit mode, render normal row again

Rendering Flow (Dialog):
  
  1. User clicks Edit → Open DialogEdit.razor:
     a. If EditSettings.HeaderTemplate != null:
        - Render in dialog header
        - @EditSettings.HeaderTemplate(rowData)
     
     b. Dialog content:
        - If EditSettings.Template != null:
          * Render full custom form
        - Else:
          * Render form fields for each column:
            @foreach (col in Columns)
              <div class="e-form-group">
                <label>@col.HeaderText</label>
                @if (col.EditTemplate != null)
                  @col.EditTemplate(rowData)
                else
                  <input type="text" />
              </div>
     
     c. If EditSettings.FooterTemplate != null:
        - Render in dialog footer
        - @EditSettings.FooterTemplate(rowData)
```

### Scenario 6: Export (PDF/Excel) + Aggregate Template

```
Initialization:
  1. User clicks "Export to Excel"
  2. Grid has grouping + aggregates
  3. AggregateColumn.GroupCaptionTemplate defined

Export Flow:
  
  1. ExcelExport.cs processes grid data:
     a. For each group:
        - Fire ExcelGroupCaptionTemplateInfo(ExcelCaptionTemplateArgs)
        - Access GroupKey, template customization options
        - If custom rendering: apply to Excel cell
  
  2. For each aggregate cell:
     a. Fire ExcelAggregateTemplateInfo(ExcelAggregateEventArgs)
     b. EventArgs includes:
        - AggregateTemplateType: GroupCaption | GroupFooter | Footer
        - GroupKey (if group caption/footer)
        - AggregateValue
        - CustomTemplate flag
     c. User can modify formatting, colors, etc.
  
  3. For detail rows:
     a. If DetailTemplate != null:
        - Fire ExcelDetailTemplateExporting(ExcelDetailTemplateEventArgs<TValue>)
        - Supports nested grid export in detail
        - EventArgs has styling options
```

---

## Interaction Matrix (Template Feature Combinations)
<!-- token-budget: 150 words -->

| Combination | Must Preserve | Risk | Notes |
|---|---|---|---|
| Templates + Sorting | Column.Template renders same data regardless of sort order; RowTemplate respects sorted row sequence | Medium | Row index in template context may differ from original data index |
| Templates + Filtering | Column.Template only renders filtered rows; RowTemplate applies to visible rows only | Medium | DetailTemplate collapse state may be lost when filter applied |
| Templates + Grouping | CaptionTemplate renders with GroupAggregates; Column.Template renders within each group; RowTemplate applies to group data | **Critical** | GroupAggregates context only populated if aggregate columns exist; empty groups still render captions |
| Templates + Virtual Scroll | RowTemplate/Column.Template invoked only for visible rows; DetailTemplate state preserved across scroll | Medium | Template re-invoked when row re-enters viewport; context data refetched |
| Templates + Freeze Columns | Frozen columns render Column.Template with e-freezecell class; DetailTemplate expands only on unfrozen side | High | CaptionTemplate must span both frozen+unfrozen sections or freeze breaks |
| Templates + Infinite Scroll | Column.Template rendered for newly loaded rows; RowTemplate applies same as base | Medium | DetailTemplate collapse state reset when new batch loads |
| Templates + Pagination | All templates render normally; EmptyRecordTemplate only if current page has no data | Low | Context data bound to current page rows only |
| Templates + Selection | Row selection state (checkbox) does NOT interfere with RowTemplate/Column.Template; Selection maintained | Low | EditTemplate in edit row does not affect row selection state |
| Templates + Editing | EditSettings.Template replaces normal row; Column.EditTemplate used per-column; DetailTemplate disabled during edit | **Critical** | After save: verify RowTemplate/Column.Template re-renders correctly with updated data |
| Templates + Validation | ValidatorTemplateContext renders validation messages alongside EditTemplate | High | Validator must trigger before row-level edit complete event |
| Templates + Export (PDF/Excel) | ExcelAggregateTemplateInfo / PdfDetailTemplateExporting events allow template customization in export | High | Nested grid in DetailTemplate requires explicit export event handling |
| Templates + Tooltip | TooltipTemplate renders custom content; works with Column.Template and RowTemplate cells | Medium | Tooltip only shows if ShowTooltip=true; Context includes column/row index |
| Templates + ColumnChooser | ColumnChooserSettings.Template allows custom column list UI; hides default checkbox list | Medium | Template must handle column visibility state (checked/unchecked) |
| Templates + Filter Template | Column.FilterTemplate custom filter UI; Column.FilterItemTemplate for checkbox items | Medium | FilterTemplate input state must reset when filter cleared |
| Templates + Toolbar | ToolbarTemplate replaces default toolbar; custom template must handle all toolbar actions manually | High | ToolbarTemplate must call grid methods (Edit, Save, Delete) explicitly |

---

## Template Context Objects (Complete Reference)
<!-- token-budget: 120 words -->

### EmptyRecordTemplateContext
```csharp
public class EmptyRecordTemplateContext 
{
    // Empty placeholder — no properties
    // Used only to signal empty state to template
}
```

### TooltipTemplateContext
```csharp
public class TooltipTemplateContext
{
    public object Value { get; set; }           // Cell value
    public GridColumn Column { get; set; }      // Column definition
    public int ColIndex { get; set; }           // Column index (0-based)
    public int RowIndex { get; set; }           // Row index (0-based)
    public bool IsHeaderCell { get; set; }      // True if header cell
}
```

### AggregateTemplateContext
```csharp
public class AggregateTemplateContext
{
    public string Type { get; set; }            // Aggregate type (Sum, Count, Avg, etc.)
    // Additional properties populated dynamically based on aggregate functions
    // Example: context.Sum, context.Count (accessed via dynamic indexing)
}
```

### CaptionTemplateContext
```csharp
public class CaptionTemplateContext
{
    public object Key { get; set; }                           // Group key value
    public int Count { get; set; }                            // Rows in group
    public string HeaderText { get; set; }                    // Column name
    public string Field { get; set; }                         // Column field
    public string ForeignKey { get; set; }                    // FK display value
    public object ForeignKeyValue { get; set; }               // FK raw value
    public AggregateTemplateContext GroupAggregates { get; set; }  // Group-level aggregates
}
```

### ValidatorTemplateContext
```csharp
public class ValidatorTemplateContext
{
    public object Data { get; set; }                          // Row data being edited
    public GridColumn Column { get; set; }                    // Column being validated
}
```

### FilterItemTemplateContext
```csharp
public class FilterItemTemplateContext
{
    public string Value { get; set; }                         // Item value
    public GridColumn Column { get; set; }                    // Column definition
}
```

### ColumnChooserTemplateContext
```csharp
public class ColumnChooserTemplateContext
{
    public List<GridColumn> Columns { get; set; }             // All available columns
}
```

### ColumnChooserFooterTemplateContext
```csharp
public class ColumnChooserFooterTemplateContext
{
    public Func<Task> ApplyClick { get; set; }                // Apply button handler
    public Func<Task> ResetClick { get; set; }                // Reset button handler
}
```

---

## Template-Related Events & Callbacks
<!-- token-budget: 80 words -->

| Event | Args Type | When Fired | Use Case |
|---|---|---|---|
| `DetailRowExpanding` | `DetailDataBoundEventArgs` | Before detail row expands | Lazy-load detail data |
| `DetailRowExpanded` | `DetailDataBoundEventArgs` | After detail row expands | Trigger custom logic |
| `DetailRowCollapsing` | `DetailDataBoundEventArgs` | Before detail row collapses | Save detail state |
| `DetailRowCollapsed` | `DetailDataBoundEventArgs` | After detail row collapses | Cleanup |
| `PdfAggregateTemplateInfo` | `PdfAggregateEventArgs` | During PDF export of aggregate cells | Customize export format |
| `PdfGroupCaptionTemplateInfo` | `PdfCaptionTemplateArgs` | During PDF export of group captions | Customize group caption export |
| `PdfDetailTemplateExporting` | `PdfDetailTemplateEventArgs<TValue>` | Before detail row appended to PDF | Export nested grid |
| `ExcelAggregateTemplateInfo` | `ExcelAggregateEventArgs` | During Excel export of aggregate cells | Customize export format |
| `ExcelGroupCaptionTemplateInfo` | `ExcelCaptionTemplateArgs` | During Excel export of group captions | Customize group caption export |
| `ExcelDetailTemplateExporting` | `ExcelDetailTemplateEventArgs<TValue>` | Before detail row appended to Excel | Export nested grid |

---

## Event Aggregator Trigger Points
<!-- token-budget: 60 words -->

Templates trigger internal event aggregator before rendering:

| Trigger Point | Event Name | Payload | Renderer |
|---|---|---|---|
| Before RowTemplate render | `"RowTemplateData"` | `RowTemplateData { Data = row.Data }` | `GridRow.razor` |
| Before Column.EditTemplate render | `"EditTemplateData"` | `EditTemplateData { Data = rowData, Column = col }` | `NormalEdit.razor` |
| Before EditSettings.Template render | `"EditTemplateData"` | Same as above | `DialogEdit.razor` |

Subscribers can intercept via `Parent.EventAggregator.On(...)`.

---

## Implementation Checklist for Template Development
<!-- token-budget: 90 words -->

When implementing or fixing a template feature:

- [ ] **Context Class**: Verify context object has all required properties populated
- [ ] **Null Checks**: All template properties are nullable; check `!= null` before invocation
- [ ] **Event Aggregator**: Trigger before render if external subscriptions needed
- [ ] **CSS Classes**: Apply `e-templatecell` to template-rendered cells
- [ ] **colspan**: Calculate correctly for EmptyRecordTemplate and DetailTemplate
- [ ] **Virtual Scroll**: Test template behavior with large datasets (1000+ rows)
- [ ] **Freeze + Group**: Verify template spans frozen+unfrozen sections correctly
- [ ] **Export Events**: Fire PDF/Excel template export events before rendering
- [ ] **Keyboard A11y**: Ensure templates respect role attributes and tab order
- [ ] **Validation**: Run through Interaction Matrix combinations before PR submission

---

## Prompt Template for Custom Agents
<!-- token-budget: 300 words -->

### Mode: feature-implementation | bug-fix
### Skill: Templates

**Context**

You are implementing or fixing **Templates** in the Syncfusion Blazor Data Grid. Templates enable custom rendering of rows, columns, edit forms, aggregate cells, group captions, and more. All templates use `RenderFragment<TContext>?` where `TContext` varies by template type (see Context Mapping above).

**Mandatory Pre-Task Actions**

1. Read `/docs/training/` folder entirely to internalize grid behaviour rules and known edge cases.
2. Identify which template type(s) you're working on (Row, Column, Edit, Aggregate, Group Caption, etc.).
3. Locate the template definition in the Code Location Map (above).
4. Find the renderer component(s) that invoke that template (see Renderer Components table).
5. Review the appropriate DOM Rendering Diagram for that template type.

**Key Rules**

- **Null Checking**: Always check `template != null` before `template(context)`.
- **Context Passing**: Verify context object has all properties populated (see Template Context Objects).
- **Event Triggers**: Call `Parent.EventAggregator.Trigger(eventName, data)` before template invocation if external subscription needed.
- **CSS Classes**: Apply `e-templatecell` to template-rendered TD elements.
- **ColSpan**: EmptyRecordTemplate and DetailTemplate must calculate colspan = total column count (including hidden columns if rendered).
- **VirtualScroll Impact**: Template logic unchanged; only visible rows rendered. DetailTemplate state preserved across scroll.
- **Freeze Impact**: Frozen section and unfrozen section render independently. DetailTemplate expands only on unfrozen side.
- **Group Impact**: CaptionTemplate receives CaptionTemplateContext with GroupAggregates. Verify GroupAggregates is not null when aggregates defined.

**Testing Checklist**

- [ ] Template renders with correct context data
- [ ] Empty/null data handled gracefully
- [ ] Template works with VirtualScroll enabled (test 1000+ rows)
- [ ] Template works with Freeze Columns enabled
- [ ] Template works with Grouping enabled
- [ ] Template works with Editing (Edit/Delete actions)
- [ ] Export events (PDF/Excel) fire correctly
- [ ] Validation (if applicable) shows/hides correctly
- [ ] No console errors or performance regression

**Reference Files**

- Definition: `src/GridTemplates.cs`, `src/GridColumn.cs`, `src/GridEditSettings.cs`, etc.
- Contexts: `src/EventModels/Grids.cs`
- Renderers: `src/Internal/Renderer/*.razor`
- Tests: `tests/Grids/GridTemplate*.cs`

**When Done**

- Verify no breaking changes to existing templates in other features (check Interaction Matrix).
- Run full grid test suite before PR submission.
- Add test cases for the template combination being implemented.

---

## Quick Reference: Which Template to Use?

| User Goal | Use This Template | Location |
|---|---|---|
| Customize entire row HTML | `RowTemplate` | `GridTemplates.cs` |
| Show row details (hierarchy) | `DetailTemplate` | `GridTemplates.cs` |
| Show when no data | `EmptyRecordTemplate` | `GridTemplates.cs` |
| Custom cell hover tooltip | `TooltipTemplate` | `GridTemplates.cs` |
| Replace default toolbar | `ToolbarTemplate` | `GridTemplates.cs` |
| Customize individual cell display | `Column.Template` | `GridColumn.cs` |
| Customize column header | `Column.HeaderTemplate` | `GridColumn.cs` |
| Custom cell editor (inline edit) | `Column.EditTemplate` | `GridColumn.cs` |
| Custom filter UI (in menu) | `Column.FilterTemplate` | `GridColumn.cs` |
| Custom filter checkboxes | `Column.FilterItemTemplate` | `GridColumn.cs` |
| Custom edit form (full page) | `EditSettings.Template` | `GridEditSettings.cs` |
| Custom edit dialog header | `EditSettings.HeaderTemplate` | `GridEditSettings.cs` |
| Custom edit dialog footer | `EditSettings.FooterTemplate` | `GridEditSettings.cs` |
| Custom validation messages | `EditSettings.Validator` | `GridEditSettings.cs` |
| Customize group header text | `GroupSettings.CaptionTemplate` | `GridGroupSettings.razor.cs` |
| Custom pager UI | `PageSettings.Template` | `GridPageSettings.razor.cs` |
| Aggregate footer values | `AggregateColumn.FooterTemplate` | `GridAggregateColumn.razor.cs` |
| Aggregate group header | `AggregateColumn.GroupCaptionTemplate` | `GridAggregateColumn.razor.cs` |
| Aggregate group footer | `AggregateColumn.GroupFooterTemplate` | `GridAggregateColumn.razor.cs` |
| Custom column chooser UI | `ColumnChooserSettings.Template` | `GridColumnChooserSettings.cs` |
| Custom column chooser buttons | `ColumnChooserSettings.FooterTemplate` | `GridColumnChooserSettings.cs` |

---

## CRITICAL Rules
<!-- token-budget: 100 words -->

**These rules MUST be followed for all template implementations. Violations will cause rendering failures or data inconsistencies.**

### Template Invocation Rules
1. **Null Checks Mandatory**: All template properties are nullable `RenderFragment<T>?`. Always check `template != null` before invoking.
2. **Context Object Validation**: Verify all context properties are populated before passing to template (see Template Context Objects section).
3. **EventAggregator Required**: Fire `Parent.EventAggregator.Trigger(eventName, data)` before template invocation for `RowTemplateData` and `EditTemplateData` events.

### DOM & CSS Rules
4. **CSS Class Application**: Apply `e-templatecell` class to all template-rendered TD elements. Aggregates use `e-summarycell e-templatecell`.
5. **colspan Calculation**: `EmptyRecordTemplate` and `DetailTemplate` must set `colspan` equal to total visible column count, including indent columns.
6. **Row Structure Integrity**: `RowTemplate` must return exactly N `<td>` elements matching column count. Missing or extra TDs break layout.

### Feature Interaction Rules
7. **VirtualScroll Compatibility**: Template logic unchanged under virtual scroll; templates re-invoked when rows re-enter viewport. Do NOT cache row-specific data in template.
8. **Freeze Column Handling**: Frozen columns render `Column.Template` with `e-freezecell` class. `DetailTemplate` expands only in unfrozen section (right side).
9. **Grouping Aggregates**: `CaptionTemplate` receives `CaptionTemplateContext.GroupAggregates` only if aggregate columns exist. Check for null before accessing aggregate values.
10. **Edit Mode Coordination**: `EditSettings.Template` replaces entire row when set. After save, verify `RowTemplate`/`Column.Template` re-render with updated data.
11. **Export Events**: Fire `PdfAggregateTemplateInfo`, `ExcelAggregateTemplateInfo`, `PdfDetailTemplateExporting`, etc. before rendering aggregate/detail content in exports.

### Performance & Validation Rules
12. **No Blocking Operations**: Templates render synchronously. Never call `await` or blocking IO inside template rendering code.
13. **Validation Sequencing**: `EditSettings.Validator` must trigger validation before `ActionComplete` event fires on save.
14. **Memory Management**: Dispose subscriptions created via `EventAggregator.On(...)` in component disposal to prevent memory leaks.

**Violation Examples:**
- ❌ Calling template without null check → `NullReferenceException`
- ❌ Wrong colspan in `DetailTemplate` → Layout breaks, cells misaligned
- ❌ Missing `e-templatecell` class → Export fails, CSS selectors broken
- ❌ Accessing `context.GroupAggregates.Sum` without null check → Runtime error in non-aggregate groups
- ❌ Using `await` in template → `InvalidOperationException` (templates are synchronous)

---

**Generated**: 2026-04-09  
**Skill Version**: 1.0  
**Blazor Grid Version**: 9.0+  
**Template Standard**: Follows `_skill-template-standard.md`

