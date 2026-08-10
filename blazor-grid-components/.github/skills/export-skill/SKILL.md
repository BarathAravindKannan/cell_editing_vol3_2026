# Export Skill
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Export (Excel/CSV/PDF) in the Syncfusion Blazor Grid. Guarantees no breakage with grouping, aggregates, virtualization, filtering, sorting, paging.

---

## Agent Invocation

```yaml
name: export-skill
description: Expert knowledge for the Export feature (Excel/CSV/PDF generation) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to export behaviour, including Excel/CSV serialization, PDF rendering, MemoryStream return paths, group/aggregate/detail-template export, custom data sources, column subsets, query cell events, theming, cross-feature interaction guarantees with virtualization, grouping, filtering, sorting, paging, aggregates, and foreign-key columns.
```

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/openspec/openspec/specs/export/spec.md` (19,268 chars: 3 export formats, data retrieval, lifecycle events, group/aggregate/detail rendering)
- `/docs/training/01-getting-started/architecture-overview.md` (module injection, EventAggregator, 4-layer architecture)
- `/docs/architecture/system-architecture.md` (module lifecycle, PropertyChanges dictionary, JS interop)
- `/docs/code-guidelines/coding-standards.md` (async/await with ConfigureAwait, guard clauses, parameter patterns)
- `/docs/tech-stack/tech-stack.md` (Blazor patterns, C# 12/13/14, external library dependencies)

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- Export must bypass virtualization viewport to fetch all records (requirement: VirtualizationExport)
- Export must reset infinite-scroll state before fetching data when EnableCache=false (requirement: InfiniteScrollExport)
- Export renders group caption rows with indent cells when grid is grouped (requirement: GroupingInExport)
- Export renders aggregate rows (footer, group-footer, group-caption inline) when aggregates are configured (requirement: AggregateRowsInExport)
- Foreign-key columns must export display value, not raw foreign-key value (requirement: ForeignKeyColumnExport)
- Task.Run used for Excel/PDF rendering to avoid blocking Blazor sync context (technical note: threading)

---

## Code Location Map
<!-- token-budget: 80 words -->

- `Internal/Export/GridExcelExport.cs` — Excel + CSV renderer (2366 lines); ExcelExport method (line 426), CsvExport method (line 496), header/stacked-header/group/aggregate/detail row rendering
- `Internal/Export/GridPdfExport.cs` — PDF renderer (2585 lines); PdfExport method (line 480), PDF grid cell styling, page template header/footer
- `Internal/Export/ExportHelper.cs` — Shared data-fetching via DataProcess, column visibility filter, foreign-key resolution
- `Internal/Export/AutoFormat.cs` — Theme/style presets (default-theme applies header/caption/record cell fonts, colors, borders)
- `SfGrid.Methods.cs` — Public API entry points: ExportToExcelAsync (line 882), ExportToCsvAsync (line 605), ExportToPdfAsync (line 2118); each has 2 overloads (with/without asMemoryStream)
- `SfGrid.Properties.cs` — AllowExcelExport (line 62, default=false), AllowPdfExport (line 154, default=false)
- `GridEvents.cs` — OnExcelExport (line 2580), OnPdfExport (line 2375), ExcelQueryCellInfoEvent (line 2682), PdfQueryCellInfoEvent (line 2410), ExportComplete (line 2788)

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Export Flow:**

1. **GridExcelExport.cs (line 484)** in ExcelExport method:
   ```csharp
   await GridModel.InvokeMethod("sfBlazor.Grid.exportSave", 
       new object[] { _fileName, Convert.ToBase64String(outputStream.ToArray()) })
   ```

2. **GridPdfExport.cs (line 540)** in PdfExport method:
   ```csharp
   await GridProperty?.InvokeMethod("sfBlazor.Grid.exportSave", 
       new object[] { _fileName, Convert.ToBase64String(ms.ToArray()) })
   ```

3. **scripts/sf-grid.ts (line 724)** — TypeScript function:
   ```typescript
   exportSave(filename: string, bytesBase64: string) {
       // Creates download link with data URI or uses msSaveBlob for Edge
       // Decodes base64, creates Blob, triggers browser download
   }
   ```

**Responsibility Split:**

| Component | Responsibility |
|-----------|----------------|
| C# Export Classes | Excel/PDF rendering, data serialization, base64 encoding |
| InvokeMethod | Blazor → JS bridge, passes filename + base64 string |
| TypeScript exportSave | Browser download trigger (data URI link or msSaveBlob) |

**CRITICAL Rules:**
1. Export methods skip JS interop call when `asMemoryStream=true` (return MemoryStream directly to caller)
2. ExportComplete event raised ONLY after JS interop call completes (not raised for MemoryStream path)
3. TypeScript must handle both modern browsers (data URI) and legacy Edge (msSaveBlob)
4. Base64 encoding happens C#-side to avoid large binary transfers across interop boundary

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination                   | Must Preserve                                                                                                                                       | Risk     |
|-------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|----------|
| Export + Grouping             | Caption rows must be rendered with indent cells; nested data rows must reflect grouping hierarchy; group-footer/group-caption aggregates inline     | Critical |
| Export + Aggregates           | Footer aggregate row must appear after all data rows; group-footer aggregate row must follow each group's data rows; caption aggregate inline      | Critical |
| Export + Virtualization       | Export must bypass virtual viewport and fetch all records via GenerateAndExecuteQuery (regardless of visible rows)                                  | Critical |
| Export + Filtering            | Exported data must respect active filter predicate (only filtered rows appear in output)                                                            | High     |
| Export + Sorting              | Exported rows must be in sorted order matching grid's current sort state                                                                            | High     |
| Export + Paging               | ExportType=AllPages exports all records; ExportType=CurrentPage exports only current page's records                                                 | High     |
| Export + Foreign-Key Columns  | Export must resolve foreign-key display value via GetForeignData (not raw foreign-key value)                                                        | High     |
| Export + Infinite Scroll      | Export must call ResetInfiniteProperties before fetching data when EnableCache=false                                                                | High     |
| Export + Stacked Headers      | Export must render multi-level header rows with correct RowSpan/ColumnSpan to match stacked structure                                               | Medium   |
| Export + Detail Template      | Export must raise ExcelDetailTemplateExporting/PdfDetailTemplateExporting event per parent row; detail rows rendered when mode is Expand/Collapse  | Medium   |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

### Mode: export-implementation

**Task**: Implement or fix Excel/CSV/PDF export functionality in the Syncfusion Blazor DataGrid (`SfGrid<TValue>`).

**Core Behaviour Rules**:
1. **Guard Properties**: `AllowExcelExport` gates Excel/CSV; `AllowPdfExport` gates PDF. When false, export methods return null without processing.
2. **Three Formats**: ExportToExcelAsync (.xlsx), ExportToCsvAsync (.csv via Excel pipeline with IsCsvExport=true), ExportToPdfAsync (.pdf).
3. **Two Return Paths**: Default triggers browser download via JS interop call `sfBlazor.Grid.exportSave(fileName, base64)`. When `asMemoryStream=true`, return `MemoryStream` and skip JS call.
4. **ExportType**: `AllPages` fetches all records; `CurrentPage` fetches only current page's records.
5. **Custom Data Source**: `ExcelExportProperties.DataSource` / `PdfExportProperties.DataSource` overrides grid's bound data source.
6. **Column Subset**: `Columns` property filters which columns appear in export.
7. **Include Flags**: `IncludeHiddenColumn`, `IncludeTemplateColumn`, `IncludeCommandColumn` control visibility of hidden/template/command columns.
8. **Header Row**: `IncludeHeaderRow=false` suppresses column header row entirely.

**Cross-Feature Integration**:
- **Grouping**: Render caption rows with indent cells, nested data rows reflect hierarchy, group-footer/group-caption aggregates inline.
- **Aggregates**: Render footer aggregate row after all data rows, group-footer aggregate row after each group's data rows.
- **Virtualization**: Bypass virtual viewport — call `GenerateAndExecuteQuery` to fetch all records (ignore visible row cache).
- **Infinite Scroll**: Call `ResetInfiniteProperties` before fetching data when `EnableCache=false`.
- **Filtering/Sorting**: Exported data must respect active filter predicate and sort state (DataProcess composes grid's current Query object).
- **Foreign-Key Columns**: Resolve display value via `GridUtils.GetForeignData` (not raw foreign-key value).
- **Stacked Headers**: Render multi-level header rows with correct RowSpan/ColumnSpan.
- **Detail Template**: Raise `ExcelDetailTemplateExporting` / `PdfDetailTemplateExporting` event per parent row; render detail rows when mode is Expand/Collapse.

**Lifecycle Events**:
1. `OnExcelExport` / `OnPdfExport` — raised before data processing begins.
2. `DataReadyEvent` (internal) — raised after query completes; allows override of Data/Aggregates before rendering.
3. `ExcelQueryCellInfoEvent` / `PdfQueryCellInfoEvent` — raised per data cell (customize cell value/style).
4. `ExcelHeaderQueryCellInfoEvent` / `PdfHeaderQueryCellInfoEvent` — raised per header cell (customize header style).
5. `ExcelAggregateTemplateInfo` / `PdfAggregateTemplateInfo` — raised per aggregate cell (customize summary row).
6. `ExcelGroupCaptionTemplateInfo` / `PdfGroupCaptionTemplateInfo` — raised per group caption row.
7. `ExcelDetailTemplateExporting` / `PdfDetailTemplateExporting` — raised per parent row (populate RowInfo for detail rows).
8. `ExportComplete` — raised after browser download completes (NOT raised when asMemoryStream=true).
9. `OnActionFailure` — raised if exception occurs during data processing.

**Implementation Constraints**:
- Excel/PDF rendering runs via `Task.Run(...)` off Blazor sync context (avoid blocking UI thread).
- Data pre-processing (`DataProcess`) awaits on sync context with `ConfigureAwait(true)`.
- Empty grid body (count=0) renders single merged cell with localized empty-record text.
- Boolean values exported as localized strings via `Localizer.GetText("Grid_True")` / `Localizer.GetText("Grid_False")`.
- Default theme is "default-theme" (AutoFormat); set `Theme="none"` to suppress styling.

**Code Locations**:
- `Internal/Export/GridExcelExport.cs` — ExcelExport method, header/group/aggregate/detail row rendering.
- `Internal/Export/GridPdfExport.cs` — PdfExport method, PDF grid cell styling, page template header/footer.
- `Internal/Export/ExportHelper.cs` — DataProcess method (shared data-fetching, foreign-key resolution, column visibility filter).

**Before Implementation**:
- Read `/docs/training/01-getting-started/architecture-overview.md` for module injection pattern.
- Confirm `AllowExcelExport` / `AllowPdfExport` guard property is checked first.
- Verify Interaction Matrix: grouping (caption rows), aggregates (footer rows), virtualization (bypass viewport), filtering/sorting (query composition), foreign-key (display value resolution).

---

### Mode: export-bug-fix

**Task**: Fix a bug in Excel/CSV/PDF export functionality without breaking existing behaviour.

**Mandatory Checks**:
1. **Guard Properties**: Verify `AllowExcelExport` / `AllowPdfExport` is checked and method returns null when false.
2. **Memory Stream Path**: Confirm `ExportComplete` event is NOT raised when `asMemoryStream=true`.
3. **Virtualization Bypass**: Verify export calls `GenerateAndExecuteQuery` to fetch all records (not using virtual viewport cache).
4. **Group Caption Rows**: Confirm caption rows are rendered with indent cells and nested data rows reflect hierarchy.
5. **Aggregate Rows**: Verify footer/group-footer/group-caption aggregates are rendered at correct positions.
6. **Foreign-Key Resolution**: Confirm foreign-key columns export display value (not raw foreign-key value).
7. **Filtered Data**: Verify exported data respects active filter predicate (DataProcess composes grid's Query object).
8. **Sorted Data**: Confirm exported rows are in sorted order matching grid's current sort state.
9. **ExportType**: Verify `AllPages` exports all records; `CurrentPage` exports only current page's records.
10. **Column Subset**: Confirm `Columns` property filters which columns appear in export.

**Interaction Matrix Validation**:
- Export + Grouping: Caption rows, nested data rows, group-footer/group-caption aggregates inline.
- Export + Aggregates: Footer aggregate row after all data rows, group-footer aggregate row after each group's data rows.
- Export + Virtualization: Bypass viewport, fetch all records.
- Export + Filtering: Respect active filter predicate.
- Export + Sorting: Match grid's current sort state.
- Export + Paging: AllPages vs CurrentPage.
- Export + Foreign-Key Columns: Display value, not raw foreign-key value.
- Export + Infinite Scroll: Reset infinite-scroll state before fetching data.
- Export + Stacked Headers: Multi-level header rows with correct RowSpan/ColumnSpan.
- Export + Detail Template: Raise detail-template event per parent row, render detail rows when mode is Expand/Collapse.

**Before Fix**:
- Read `/docs/training/00-START-HERE.md` — #1 rule: never break existing behavior.
- Review existing unit tests for export feature.
- Cross-check Interaction Matrix with live specs from `/openspec/openspec/specs/export/spec.md`.
- Verify lifecycle events (OnExcelExport, QueryCellInfo, ExportComplete, OnActionFailure) are raised at correct times.
