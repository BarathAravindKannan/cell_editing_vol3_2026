```prompt
---
description: Optimized Bug Fix Workflow — Skill-Mapped, No Codebase Searches, 60% Token Reduction
version: 2.1
configuration_file: openspec/config-bug-fix-optimized.yaml
---

# Optimized Bug Fix Workflow (v2.1 — Skill-Mapped, Token-Efficient)

Fix bugs in Syncfusion Blazor Grid using **feature-skill mapping** instead of manual codebase searches. This prompt uses `openspec/config-bug-fix-optimized.yaml` to:

✅ **Load skills first** (pre-built knowledge, exact file locations)  
✅ **Read ONLY mapped files** (no codebase-wide searches)  
✅ **Respect line limits** (120 lines max per file)  
✅ **Cache all reads** (prevent token waste from duplicates)  
✅ **Pre-built interactions** (regression scope is explicit, no discovery)  

**Result: 3600+ tokens saved per bug (~60% reduction)**

---

## Input

```
/opsx:bug-fix [bug description]

Example:
/opsx:bug-fix When EnableVirtualization is true, scrolling to bottom causes IndexOutOfRangeException
```

---

## 5-Stage Workflow Overview

```
Stage 1: BUG VALIDATION (Skill Loading + Root Cause)
  → Load feature_skill_map.[domain] from config-bug-fix-optimized.yaml
  → Load SKILL.md for knowledge reference
  → Read ONLY mapped files (max 120 lines each)
  → Create failing BUnit test
  → Output: exploration.md
  → GATE: Human approves root cause + failing test

Stage 2: TDD DESIGN (Test Suite + Regression Scope)
  → Create full test suite (all FAIL)
  → Load spec from feature_skill_map.[domain].spec
  → Load interactions from feature_skill_map.[domain].interactions
  → Create proposal.md, design.md, tasks.md
  → Output: proposal.md, design.md, tasks.md, specs/
  → GATE: Human approves tests (all fail)

Stage 3: FIX VERIFICATION (Implementation + Regression)
  → Implement fix from tasks.md
  → Run tests: all PASS
  → Run regression tests for interacting domains ONLY (from feature_skill_map)
  → Output: Modified source files + updated tasks.md
  → GATE: Auto-proceed if 100% tests + regression pass

Stage 4: SECURITY & PERFORMANCE
  → Run security checklist
  → Measure performance (baseline vs post-fix)
  → Verify token budget enforcement
  → Output: Gate checklist (inline only)
  → GATE: Human approves security + performance

Stage 5: ARCHIVE
  → Move to archive folder
  → Delete ephemeral files
  → Keep 6 core artifacts only
  → Update lessons-learned.md
  → GATE: Auto-proceed
```

---

## Stage 1: BUG VALIDATION (Skill Loading + Root Cause Discovery)

### Step 1.1: Parse Bug Domain

From the bug description, identify the feature domain using keywords:
- "sort" → `sorting`
- "filter" → `filtering`
- "group" → `grouping`
- "scroll" / "virtual" → `virtualization`
- "infinite" → `infinite-scroll`
- "edit" → `editing`
- "select" → `selection`
- "page" → `paging`
- "column" → `columns`
- "event" / "callback" → `events`
- "interop" / "JavaScript" → `javascript-interop`

**MANDATORY**: If domain unclear, ask for clarification:
> "Which Blazor Grid feature is affected? (e.g., sorting, filtering, grouping, editing, virtualization, etc.)"

### Step 1.2: Load Feature Skill Map Entry

From `openspec/config-bug-fix-optimized.yaml`, fetch `feature_skill_map.[domain]`:

```yaml
Example: domain = "sorting"

feature_skill_map.sorting:
  skill: ".github/skills/sorting-skill/SKILL.md"
  spec: "openspec/specs/sorting/spec.md"
  components: ["GridSortSettings", "GridSortColumns", "GridSortColumn"]
  files:
    core: ["src/GridSortSettings.razor.cs", "src/GridSortColumns.razor.cs", "src/GridSortColumn.cs"]
    actions: ["src/Internal/Actions/Sort.cs"]
    api: ["src/SfGrid.Properties.cs (AllowSorting, ...)", "src/SfGrid.Methods.cs (SortColumnAsync, ...)"]
    ui: ["src/Internal/Renderer/GridHeaderCell.razor"]
  interactions: ["filtering", "grouping", "virtualization", ...]
  lines_per_file: 120
  risk_level: "high"
```

### Step 1.3: Load Skill File

Read the SKILL.md file from `feature_skill_map.[domain].skill`:

```bash
File: .github/skills/sorting-skill/SKILL.md

This file contains:
  ✓ Knowledge references (no manual search needed)
  ✓ Code location map (exact methods/properties)
  ✓ Training insights (Blazor patterns, event flow)
  ✓ Interaction matrix (cross-feature test scope)
  ✓ API contracts (method signatures)
  ✓ Edge cases (boundary conditions)
```

**ACTION**: Read SKILL.md **FIRST** — it has all the expert knowledge pre-compiled. This eliminates 40% of manual file searches.

### Step 1.4: Read Mapped Files (NO Codebase-Wide Searches)

**STRICT RULE**: Read ONLY files listed in `feature_skill_map.[domain].files`

From the sorting domain example:

```
✓ ALLOWED (listed in feature_skill_map.sorting.files):
  - src/GridSortSettings.razor.cs        (max 120 lines)
  - src/GridSortColumns.razor.cs         (max 120 lines)
  - src/GridSortColumn.cs                (max 120 lines)
  - src/Internal/Actions/Sort.cs         (max 120 lines)
  - src/SfGrid.Properties.cs             (section: AllowSorting, SortSettings only)
  - src/SfGrid.Methods.cs                (section: SortColumnAsync, etc. only)
  - src/Internal/Renderer/GridHeaderCell.razor (max 120 lines)

✗ FORBIDDEN (not in feature_skill_map.sorting.files):
  - Random .cs files in src/
  - Entire src/ folder
  - src/bin/, obj/, node_modules/
  - Unrelated domains (e.g., Filtering, Grouping code when fixing Sort bug)
  - API reference files outside the mapped list
```

**ENFORCEMENT**: If attempting to read a file outside the mapped list, ask for approval:
> "File 'src/GridColumnChooser.razor.cs' is not listed in feature_skill_map.sorting.files. Should I read it? (This will consume extra tokens)"

### Step 1.5: Log All File Reads (Caching)

Before reading a file, check `.read-cache.csv` (in the change directory):

```csv
FILE_PATH,LINE_START,LINE_END,STAGE,TIMESTAMP
src/GridSortSettings.razor.cs,1,120,stage-1,2026-03-30T10:00:00
src/Internal/Actions/Sort.cs,1,80,stage-1,2026-03-30T10:02:00
```

**RULE**:
- If file + line range already logged → **SKIP** (reference previous findings)
- If need NEW lines beyond previous read → append-only read (only new lines)
  - Example: Logged 1-50; now need 40-150 → read ONLY 51-150 (saves ~200 tokens)
- If file not in cache → read new, log it

### Step 1.6: Identify Root Cause (File:Method:Line)

Using SKILL.md knowledge + mapped files, pinpoint the bug:

```
✓ GOOD: "Bug is in src/Internal/Actions/Sort.cs, method SortClickHandler (line 42-55). 
         When AllowMultiSorting=true and shift-click fires, the sort priority badge 
         is not updated because PropertyChanges['RefreshColumnHeader'] is missing."

✗ BAD: "The bug is somewhere in the sorting code." (Too vague)
```

**MANDATORY**: Always cite file:method:line in exploration.md

### Step 1.7: Create Failing BUnit Test

Create a test that FAILS with the bug present:

**Location**: `tests/Bunit/Bugs/<BUG_ID>/Reproduction.cs`

```csharp
[Test]
public async Task When_SortClickWithShift_AndAllowMultiSorting_Then_SortPriorityBadgeUpdates()
{
    // Arrange
    var grid = RenderComponent<SfGrid<OrderData>>(parameters => parameters
        .Add(p => p.AllowSorting, true)
        .Add(p => p.AllowMultiSorting, true)
    );
    
    // Act
    var headerCell = grid.FindComponent<GridHeaderCell>();
    await grid.InvokeAsync(async () => await headerCell.Instance.OnHeaderClick(...));
    
    // Assert
    var badge = headerCell.Find(".e-sort-priority-badge");
    Assert.That(badge.TextContent, Is.EqualTo("2"));  // FAILS before fix
}
```

**This test proves the bug is real and reproducible.**

### Step 1.8: Create exploration.md

```markdown
# Bug Investigation: BLAZ-XXXX

## Bug Description
[Reproduce the user's bug description]

## Domain
Sorting (identified from keywords: "sort", "multi-sort", etc.)

## Root Cause
File: src/Internal/Actions/Sort.cs
Method: SortClickHandler (lines 42-55)
Issue: Missing PropertyChanges['RefreshColumnHeader'] assignment after appending sort column

## Files Read (Skill-Mapped)
- src/GridSortSettings.razor.cs (120 lines)
- src/Internal/Actions/Sort.cs (80 lines)
- src/SfGrid.Properties.cs (section: AllowSorting, AllowMultiSorting)
- .github/skills/sorting-skill/SKILL.md (knowledge reference)

## Failing Test
tests/Bunit/Bugs/BLAZ-XXXX/Reproduction.cs → Test fails before fix, passes after

## Confidence Level
HIGH (exact line identified; fix is straightforward)

## Next Steps
1. Create full test suite (Stage 2)
2. Implement fix (Stage 3)
3. Verify regression tests (interactions: filtering, grouping, virtualization, etc.)
```

### Stage 1 Gate

**GATE**: Human must approve:
- ✅ Root cause identified (file:method:line)
- ✅ Failing test confirms bug
- ✅ Confidence level acceptable

**Approval Command**: `/opsx:bug-fix-approve STAGE-1`

---

## Stage 2: TDD DESIGN (Test Suite + Regression Scope)

### Step 2.1: Create Full Test Suite (All FAIL)

Using SKILL.md knowledge + domain spec, write 8-12 test cases that ALL FAIL before the fix:

**Location**: `tests/Bunit/Bugs/<BUG_ID>/Specification.cs`

```csharp
// Primary scenario (the actual bug)
[Test]
public async Task When_SortClickWithShift_AndAllowMultiSorting_Then_SortPriorityBadgeUpdates()
{ /* FAILS before fix */ }

// Edge case: empty dataset
[Test]
public async Task When_SortClick_OnEmptyGrid_Then_NoException()
{ /* FAILS before fix */ }

// Interaction: sort + filter
[Test]
public async Task When_Sort_AndFilterActive_Then_SortAppliesOnFilteredData()
{ /* FAILS before fix */ }

// Interaction: sort + grouping
[Test]
public async Task When_Sort_AndGrouped_Then_GroupColumnsRemainFirst()
{ /* FAILS before fix */ }

// Interaction: sort + virtualization
[Test]
public async Task When_Sort_AndVirtualizationEnabled_Then_VirtualCacheCleared()
{ /* FAILS before fix */ }

// ... (8-12 tests total, all FAIL)
```

**All tests MUST FAIL** before you implement the fix (proves TDD discipline).

### Step 2.2: Load Domain Spec

From `feature_skill_map.[domain].spec`:

```bash
Example: feature_skill_map.sorting.spec = "openspec/specs/sorting/spec.md"

Read this spec to understand:
  ✓ Sorting behavior requirements (SHALL / MUST)
  ✓ Event pipeline (OnActionBegin → Sorting → execute → Sorted → OnActionComplete)
  ✓ Multi-sort priority rules
  ✓ Interaction guarantees (grouped columns always first, etc.)
  ✓ API contracts (SortColumnAsync, SortColumnsAsync, ClearSortingAsync)
```

### Step 2.3: Load Regression Test Scope from Interactions

From `feature_skill_map.[domain].interactions`:

```yaml
Example: feature_skill_map.sorting.interactions = 
  ["filtering", "grouping", "virtualization", "infinite-scroll", "export", "editing", "selection", "paging", "frozen-columns"]

For the sort bug fix, regression tests must cover:
  ✓ sorting tests (12 tests)
  ✓ filtering tests (must pass when sort+filter both active)
  ✓ grouping tests (grouped columns must stay first)
  ✓ virtualization tests (virtual cache must clear on sort)
  ✓ infinite-scroll tests (scroll position must reset)
  ✓ export tests (export must apply current sort order)
  ✓ editing tests (edit session must handle re-sort)
  ✓ selection tests (selection cleared unless PersistSelection=true)
  ✓ paging tests (current page preserved)
  ✓ frozen-columns tests (frozen cols render sorted data)
  
Total: 85+ regression tests (no manual discovery needed — it's explicit)
```

**NO NEED TO SEARCH** — the interactions list is pre-built in config-bug-fix-optimized.yaml

### Step 2.4: Create proposal.md, design.md, tasks.md

**proposal.md**: Problem → Root Cause → Solution (500 words)

```markdown
# Proposal: BLAZ-XXXX — Sort Priority Badge Not Updating on Multi-Sort

## Problem
When AllowMultiSorting=true and user shift-clicks a header to add a second sort column, 
the priority badge (showing sort order "1", "2", etc.) is not updated.

## Root Cause
In src/Internal/Actions/Sort.cs, method SortClickHandler (lines 42-55):
- When isMultiSort=true, the code appends to SortSettings.Columns ✓
- BUT: PropertyChanges['RefreshColumnHeader'] is NOT set
- Result: GridHeaderCell.razor does NOT re-render to show updated badge

## Solution
Add PropertyChanges['RefreshColumnHeader'] = true after appending sort column

## Implementation Impact
- Files modified: src/Internal/Actions/Sort.cs (1 method)
- Tests affected: 85+ regression tests (from interactions list)
- Risk level: HIGH (multi-sort is widely used; must preserve all interactions)

## Non-Goals
- Do not change sort direction logic
- Do not change grouped column precedence
```

**design.md**: Approach → Alternatives → Patterns

**tasks.md**: Actionable tasks (each ~2 hours)

```markdown
# Tasks: BLAZ-XXXX

## Task 1: Understand Sort Architecture (Skill-Based)
- Read .github/skills/sorting-skill/SKILL.md
- Read openspec/specs/sorting/spec.md (event pipeline section)
- Reference: SortClickHandler method flow
Time: 30 min

## Task 2: Modify Sort.cs (Implementation)
- File: src/Internal/Actions/Sort.cs
- Method: SortClickHandler (lines 42-55)
- Change: Add PropertyChanges['RefreshColumnHeader'] = true after line 50
- Reference: SKILL.md (code location map) for PropertyChanges pattern
Time: 15 min

## Task 3: Run Sorting Tests (Verification)
- Run: dotnet test tests/Bunit/Bugs/BLAZ-XXXX/Specification.cs
- Expected: All 12 sorting tests PASS
- Verify: Sort priority badge renders correctly
Time: 10 min

## Task 4: Run Regression Tests (Cross-Feature)
- For each interaction domain (filtering, grouping, virtualization, etc.):
  - Run: dotnet test tests/Bunit/[Domain]/
  - Expected: 100% PASS
- Example: "dotnet test tests/Bunit/Grouping/" → must PASS (no regressions)
Time: 20 min

## Task 5: Security & Performance Audit (Stage 4)
- Security: No user input changes; no new XSS vectors
- Performance: PropertyChanges assignment has O(1) cost; no regression expected
- Memory: No new allocations
Time: 10 min

## Task 6: Run Final Test Suite
- Run: dotnet build Syncfusion.Blazor.Grid.csproj -c Debug
- Run: dotnet test Bunit/Syncfusion.Blazor.Tests.Grids.csproj -c Debug
- Expected: 100% PASS
Time: 20 min
```

### Stage 2 Gate

**GATE**: Human must approve:
- ✅ Test suite complete (8-12 tests)
- ✅ All tests FAIL before fix (proves TDD)
- ✅ Regression test scope explicit (from interactions list)
- ✅ Proposal + design + tasks clear and actionable

**Approval Command**: `/opsx:bug-fix-approve STAGE-2`

---

## Stage 3: FIX VERIFICATION (Implementation + Regression)

### Step 3.1: Implement Fix from tasks.md

Execute each task sequentially:
1. Read SKILL.md (knowledge base)
2. Modify the file (code change)
3. Run tests immediately
4. Fix any failures
5. Continue to next task

**MANDATORY**: After each file modification, run `dotnet test` to verify tests pass.

### Step 3.2: Verify All Tests PASS

```bash
# Primary tests (bug scenario)
dotnet test tests/Bunit/Bugs/BLAZ-XXXX/Specification.cs
# Expected: 12 tests PASS

# Regression tests (cross-feature)
dotnet test tests/Bunit/Sorting/           # 12 tests
dotnet test tests/Bunit/Filtering/         # 15 tests  (interaction)
dotnet test tests/Bunit/Grouping/          # 10 tests  (interaction)
dotnet test tests/Bunit/Virtualization/    # 8 tests   (interaction)
dotnet test tests/Bunit/InfiniteScroll/    # 9 tests   (interaction)
dotnet test tests/Bunit/Export/            # 6 tests   (interaction)
dotnet test tests/Bunit/Editing/           # 14 tests  (interaction)
dotnet test tests/Bunit/Selection/         # 12 tests  (interaction)
dotnet test tests/Bunit/Paging/            # 8 tests   (interaction)

# Total: 85+ tests PASS
```

**GATE**: Auto-proceed if 100% of tests pass (no manual approval needed)

### Step 3.3: Update tasks.md

Update the tasks.md with "COMPLETED" checkboxes:

```markdown
## Task 2: Modify Sort.cs (Implementation)
- ✅ COMPLETED
- Change applied: PropertyChanges['RefreshColumnHeader'] = true
- File: src/Internal/Actions/Sort.cs, line 52
- Tests: 12/12 PASS after change
```

---

## Stage 4: SECURITY & PERFORMANCE AUDIT

### Step 4.1: Security Checklist

From `docs/dev-process/bug-fix-security-performance-standards.md`:

```markdown
## Security Checklist

- [ ] No user input validation issues (N/A — no user input change)
- [ ] No XSS vectors introduced (N/A — no HTML generation change)
- [ ] No SQL injection (N/A — not data access change)
- [ ] No secrets leaked in logs (✓ PASS — no logging change)
- [ ] No authentication/authorization bypass (N/A)
- [ ] No ARIA accessibility issues (✓ PASS — render refresh preserves ARIA)
- [ ] No improper disposal/cleanup (✓ PASS — PropertyChanges is auto-cleanup)

STATUS: ✅ PASS
```

### Step 4.2: Performance Verification

```markdown
## Performance Baseline

BEFORE FIX:
- Sort priority badge: Not rendered (user sees blank)
- Render cycle: Header re-render skipped
- Memory: No change expected

AFTER FIX:
- Sort priority badge: Rendered correctly
- Render cycle: Header re-render triggers (expected: +1 render)
- Memory: +0 bytes (PropertyChanges assignment only)
- Performance: <5% regression threshold (target: 0%)

TEST RESULT: ✅ PASS
  - Sorting 1000 items with multi-sort: 45ms (baseline: 45ms)
  - Sort priority badge rendering: <1ms per badge
  - Memory: No increase observed
```

### Step 4.3: Token Budget Enforcement

From `config-bug-fix-optimized.yaml`:

```
Stage 4 budget: 1600 tokens
Current usage: ~1200 tokens
Remaining: 400 tokens
Status: ✅ PASS (within 85% threshold)
```

### Stage 4 Gate

**GATE**: Human must approve:
- ✅ Security checklist passed
- ✅ Performance regression <5%
- ✅ Token budget enforcement verified

**Approval Command**: `/opsx:bug-fix-approve STAGE-4`

---

## Stage 5: ARCHIVE

### Step 5.1: Move to Archive Folder

System automatically moves change to:
```
openspec/changes/BLAZ-XXXX-<change-name>/
```

Retains ONLY 6 core artifacts:
- `.openspec.yaml` (metadata)
- `exploration.md`
- `proposal.md`
- `design.md`
- `tasks.md`
- `specs/` (if any)

### Step 5.2: Delete Ephemeral Files

Deletes (no longer needed):
- `.read-cache.csv` (ephemeral)
- `*.log` files
- `*-summary.md`, `*-analysis.md`, `*-report.md` (prohibited patterns)
- Intermediate temp files

### Step 5.3: Update lessons-learned.md

Appends to `openspec/specs/lessons-learned.md`:

```markdown
### BLAZ-XXXX — Sort Priority Badge Not Updating on Multi-Sort

**Date**: 2026-03-30  
**Feature**: Sorting  
**Root Cause**: Missing PropertyChanges['RefreshColumnHeader'] in SortClickHandler (line 50)  
**Prevention**: Always set PropertyChanges when modifying UI-affecting state  
**Recommendation**: Use feature_skill_map to load SKILL.md first (prevented 40 min of manual search)  
**Files Modified**: src/Internal/Actions/Sort.cs (1 method)  
**Tests Added**: 12 primary + 85 regression tests (0 failures)  
**Tokens Saved**: 3600+ vs manual search approach (config-bug-fix-optimized.yaml)  
```

### Stage 5 Gate

**GATE**: Auto-proceed (no manual approval)

Archive is complete. Bug fix is done.

---

## Approval Gates Summary

| Stage | Gate Type | Approval Command | Next Stage |
|-------|-----------|------------------|-----------|
| 1 | ⏸️ Manual | `/opsx:bug-fix-approve STAGE-1` | Stage 2 |
| 2 | ⏸️ Manual | `/opsx:bug-fix-approve STAGE-2` | Stage 3 |
| 3 | ✅ Auto | (100% tests pass) | Stage 4 |
| 4 | ⏸️ Manual | `/opsx:bug-fix-approve STAGE-4` | Stage 5 |
| 5 | ✅ Auto | (archive complete) | Done |

---

## Key Optimization Rules (60% Token Reduction)

### Rule 1: Load Skill FIRST
- BEFORE: Manual search + codebase analysis
- AFTER: Load SKILL.md (pre-compiled knowledge)
- **Tokens saved: 400+**

### Rule 2: Read ONLY Mapped Files
- BEFORE: Grep entire src/, read random .cs files
- AFTER: Read files from feature_skill_map.[domain].files only
- **Tokens saved: 1000+**

### Rule 3: Respect Line Limits
- BEFORE: Read full source files (500+ lines each)
- AFTER: Read only feature_skill_map.[domain].lines_per_file (120 lines max)
- **Tokens saved: 300+**

### Rule 4: Cache Reads
- BEFORE: Re-read same file in Stage 2, 3, 4
- AFTER: Check .read-cache.csv; skip duplicates
- **Tokens saved: 600+**

### Rule 5: Pre-Built Interactions
- BEFORE: Manual discovery of cross-feature tests
- AFTER: Use feature_skill_map.[domain].interactions (explicit list)
- **Tokens saved: 800+**

### Rule 6: No Codebase Searches
- BEFORE: "Let me search for usages of this method..."
- AFTER: "It's listed in SKILL.md (code location map)"
- **Tokens saved: 500+**

**Total: 3600+ tokens saved per bug (~60% reduction)**

---

## Troubleshooting

### Issue: Domain Not Recognized

**Solution**: Parse bug description more carefully for feature keywords:
- "Sort order not correct" → sorting
- "Filter not applying" → filtering
- "Group header missing" → grouping
- "Scroll lag" → virtualization

If still unclear, ask the user to specify the Blazor Grid feature name.

### Issue: SKILL.md Not Found

**Solution**: The skill file might not exist yet for that domain.
1. Check `.github/skills/` folder for available skills
2. If missing, use the domain spec instead: `openspec/specs/[domain]/spec.md`
3. Proceed with mapped files from feature_skill_map

### Issue: Mapped Files Not Sufficient

**Solution**: Before reading additional files:
1. Check if the information is already in SKILL.md
2. Check if the information is already in the domain spec
3. If truly necessary, ask for approval before reading extra files (will consume tokens)

### Issue: Tests Not Passing After Fix

**Solution**: 
1. Verify the fix matches the root cause (review exploration.md)
2. Run single test with verbose output: `dotnet test [test-file] -v detailed`
3. Check for side effects in regression tests (might need to adjust fix)
4. If stuck, ask human for guidance before proceeding

---

## Key Guarantees

✅ **Domain-Based Reading**: ONLY feature_skill_map files allowed  
✅ **Skill-Powered**: SKILL.md loaded first (eliminates 40% of manual searches)  
✅ **Strict File Limits**: 120 lines max per file (focuses on relevant code)  
✅ **Read Caching**: .read-cache.csv prevents duplicate reads (30-40% token savings)  
✅ **Pre-Built Interactions**: No discovery needed; scope is explicit  
✅ **5 TDD Checkpoints**: Test-fail → fix → test-pass → regression → audit  
✅ **60% Token Reduction**: vs. config.yaml v2.0 (3600+ tokens saved per bug)  
✅ **Clean Archive**: Only 6 core artifacts retained  
✅ **Lessons Learned**: Updated for future bug prevention  

---

## Configuration Reference

All rules, file maps, and token budgets defined in:
- **`openspec/config-bug-fix-optimized.yaml`** (NEW — primary reference)
- `openspec/config.yaml` (v2.0 — legacy, still supported)

---

**Ready to fix a bug? Run:**
```
/opsx:bug-fix [bug description]
```

Example:
```
/opsx:bug-fix When EnableVirtualization is true, scrolling to bottom causes IndexOutOfRangeException
```

The workflow will automatically:
1. Parse the domain (virtualization)
2. Load feature_skill_map.virtualization from config-bug-fix-optimized.yaml
3. Load SKILL.md (.github/skills/virtualization-skill/SKILL.md)
4. Read ONLY mapped files (max 120 lines each)
5. Identify root cause
6. Create failing test
7. Guide you through 4 approval stages
8. Archive with lessons learned

**Result: Bug fixed with 60% fewer tokens consumed.**

```
