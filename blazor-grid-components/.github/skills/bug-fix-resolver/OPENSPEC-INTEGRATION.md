# Bug-Fix-Resolver: OpenSpec Integration Guide

## Overview

The **Bug-Fix-Resolver Skill** is fully integrated with the OpenSpec framework. This document explains how the skill fits into the OpenSpec ecosystem, how it uses existing skills, and how it generates OpenSpec artifacts.

---

## Architecture

```
OpenSpec Framework
│
├── openspec/config.yaml
│   └── operations.bug_fix (registered here)
│
├── .codestudio/skills/
│   ├── bug-fix-resolver/
│   │   ├── SKILL.md                    ← Expert knowledge
│   │   ├── README.md                   ← User guide
│   │   └── OPENSPEC-INTEGRATION.md     ← This file
│   │
│   ├── feature-impact-analysis-skill/  ← Invoked at Stage 2
│   ├── sorting-skill/                  ← Loaded on demand
│   ├── filtering-skill/                ← Loaded on demand
│   ├── grouping-skill/                 ← Loaded on demand
│   ├── editing-skill/                  ← Loaded on demand
│   ├── paging-skill/                   ← Loaded on demand
│   ├── selection-skill/                ← Loaded on demand
│   ├── virtualization-skill/           ← Loaded on demand
│   ├── infinite-scroll-skill/          ← Loaded on demand
│   └── testing/                        ← Loaded on demand
│
├── .codestudio/prompts/
│   ├── opsx-bug-fix-resolver.prompt.md ← Executable workflow
│   ├── opsx-explore.prompt.md
│   ├── opsx-propose.prompt.md
│   ├── opsx-apply.prompt.md
│   └── opsx-archive.prompt.md
│
├── openspec/specs/
│   ├── sorting/spec.md                 ← Domain specs
│   ├── filtering/spec.md               ← Referenced when bug
│   ├── grouping/spec.md                ← in specific feature
│   ├── editing/spec.md
│   ├── virtualization/spec.md
│   ├── paging/spec.md
│   ├── selection/spec.md
│   ├── infinite-scroll/spec.md
│   ├── aggregates/spec.md
│   ├── events/spec.md
│   ├── lifecycle/spec.md
│   ├── interop/spec.md
│   ├── columns/spec.md
│   ├── templates/spec.md
│   ├── lessons-learned.md              ← Updated after fix
│   └── overview/spec.md
│
└── docs/dev-process/
    ├── bug-fix-resolver-agent.md       ← Workflow spec
    ├── ANALYSIS-TEMPLATE.md            ← Stage 2 output
    ├── TEST-SPECIFICATION-TEMPLATE.md  ← Stage 3 output
    ├── FIX-APPROACH-TEMPLATE.md        ← Stage 4 output
    ├── ANALYSIS-<BUG_ID>.md            ← Generated per bug
    ├── TEST-SPECIFICATION-<BUG_ID>.md  ← Generated per bug
    ├── FIX-APPROACH-<BUG_ID>.md        ← Generated per bug
    └── PERFORMANCE-<BUG_ID>.md         ← Generated per bug
```

---

## Registration in OpenSpec

### config.yaml Entry

```yaml
operations:
  bug_fix:
    name: "Bug-Fix-Resolver"
    command: "/bugfix: <BUG_ID> <description>"
    skill: "bug-fix-resolver-skill"
    prompt: "bug_fix_resolver"
    description: "Systematic bug resolution with 7-stage workflow, human governance, TDD discipline, and feature-aware regression prevention"
    workflow_stages: 7
    approval_gates: ["Stage 1: Bug Validity", "Stage 2: Root Cause", "Stage 3: Test Coverage", "Stage 6: Security & Performance"]
    mandatory_docs:
      - "ANALYSIS-TEMPLATE.md"
      - "TEST-SPECIFICATION-TEMPLATE.md"
      - "FIX-APPROACH-TEMPLATE.md"
    output_location: "docs/dev-process/"
    feature_skill_integration: "feature-impact-analysis-skill"
```

### Discovery via OpenSpec

```bash
# List all registered operations
openspec list --json

# Output includes:
# {
#   "operations": {
#     "bug_fix": {
#       "name": "Bug-Fix-Resolver",
#       "command": "/bugfix: <BUG_ID> <description>",
#       "skill": "bug-fix-resolver-skill",
#       ...
#     }
#   }
# }
```

---

## Skill Integration Matrix

### Primary Skill: bug-fix-resolver-skill
**Location**: `.codestudio/skills/bug-fix-resolver/SKILL.md`

**Provides**:
- 7-stage workflow specification
- Approval gate definitions
- Feature interaction matrix
- Security & performance checklist
- Code location map for Blazor Grid

**Used By**: opsx-bug-fix-resolver.prompt.md (the executable workflow)

### Dynamic Feature Skills (Loaded on Demand)

At **Stage 2: Root Cause Analysis**, the system:

1. **Identifies affected feature** from bug description
   - Example: "Sort order bug" → sorting-skill
   - Example: "Filter not working" → filtering-skill
   - Example: "Virtual scroll regression" → virtualization-skill

2. **Loads matching skill**
   ```
   Bug mentions "sort"?
     → Load sorting-skill
     → Load openspec/specs/sorting/spec.md
     → Extract interaction matrix
     → Map cross-feature risks
   ```

3. **Invokes feature-impact-analysis-skill**
   - Maps all affected features
   - Identifies regression risks
   - Recommends regression test scenarios
   - Returns feature interaction graph

4. **Generates regression test plan**
   - Sort + Filter tests
   - Sort + Grouping tests
   - Sort + Virtualization tests
   - Etc. (per interaction matrix)

**Feature Skills Available**:
- `sorting-skill` → openspec/specs/sorting/spec.md
- `filtering-skill` → openspec/specs/filtering/spec.md
- `grouping-skill` → openspec/specs/grouping/spec.md
- `editing-skill` → openspec/specs/editing/spec.md
- `paging-skill` → openspec/specs/paging/spec.md
- `selection-skill` → openspec/specs/selection/spec.md
- `virtualization-skill` → openspec/specs/virtualization/spec.md
- `infinite-scroll-skill` → openspec/specs/infinite-scroll/spec.md
- `aggregates-skill` → openspec/specs/aggregates/spec.md (if exists)
- `testing` → Provides bUnit patterns

---

## Workflow: From Bug to OpenSpec Artifact

### Flow Diagram

```
User Input
│
├─ /bugfix: BLAZ-1247 Sort order not preserved after filter
│
├─ Load bug-fix-resolver-skill
│   └─ Read .codestudio/skills/bug-fix-resolver/SKILL.md
│
├─ Execute opsx-bug-fix-resolver.prompt.md
│   └─ 7-stage workflow begins
│
├─ STAGE 2: Root Cause Analysis
│   ├─ Identify feature: "sorting"
│   ├─ Load sorting-skill
│   ├─ Read openspec/specs/sorting/spec.md
│   ├─ Invoke feature-impact-analysis-skill
│   └─ Generate ANALYSIS-<BUG_ID>.md
│
├─ STAGE 3: Test Design
│   ├─ Load testing skill
│   ├─ Write test cases
│   └─ Generate TEST-SPECIFICATION-<BUG_ID>.md
│
├─ STAGE 4: Implementation
│   ├─ Load feature skill (sorting-skill)
│   ├─ Reference code location map
│   ├─ Apply fix
│   └─ Generate FIX-APPROACH-<BUG_ID>.md
│
├─ STAGE 5: Regression Testing
│   ├─ Load cross-feature skills (filter, group, virtual, etc.)
│   ├─ Run regression test matrix
│   └─ Verify no breakage
│
├─ STAGE 6: Security & Performance
│   ├─ Execute security checklist
│   ├─ Measure performance
│   └─ Generate PERFORMANCE-<BUG_ID>.md
│
├─ STAGE 7: Archive & Lessons Learned
│   ├─ Create PR
│   ├─ Update openspec/specs/sorting/spec.md (if needed)
│   ├─ Append openspec/specs/lessons-learned.md
│   └─ Archive in OpenSpec
│
└─ Complete ✓
```

---

## Artifact Generation

### Stage 2 Output: ANALYSIS-<BUG_ID>.md

**Contents** (generated from ANALYSIS-TEMPLATE.md):
- Reproduction test code (failing initially)
- Feature interactions mapped using feature-impact-analysis-skill
- Root cause identification with exact file/method/line
- OpenSpec domain spec reference
- Assumptions documented

**Example**:
```markdown
# Bug BLAZ-1247 Analysis

## Reproduction
Test: test/Bugs/BLAZ-1247/Reproduction.cs
Status: ✗ FAILS (proves bug exists)

## Feature Interactions
- Affected: Sorting + Filtering
- Feature-Impact-Analysis Result:
  - Sort feature: openspec/specs/sorting/spec.md
  - Filter feature: openspec/specs/filtering/spec.md
  - Risk: Virtualization cache not invalidated

## Root Cause
File: src/Internal/Actions/Sort.cs
Method: ApplySort() (line 234)
Issue: Cache invalidation missing after sort applied

## Assumptions
- [List of assumptions verified during analysis]
```

### Stage 3 Output: TEST-SPECIFICATION-<BUG_ID>.md

**Contents** (generated from TEST-SPECIFICATION-TEMPLATE.md):
- Complete test suite (all tests fail initially)
- Test scenarios covering bug + edge cases + feature interactions
- Feature interaction scenarios from feature-impact-analysis-skill
- Test intent documentation

**Example**:
```markdown
# Bug BLAZ-1247 Test Specification

## Test Suite
Location: test/Bugs/BLAZ-1247/Specification.cs

### Primary Bug Scenario
[Test code] — ✗ FAILS (before fix)

### Edge Cases
[Test code] — ✗ FAILS (before fix)

### Feature Interactions
- Sort + Filter: [Test code] — ✗ FAILS
- Sort + Grouping: [Test code] — ✗ FAILS
- Sort + Virtualization: [Test code] — ✗ FAILS
```

### Stage 4 Output: FIX-APPROACH-<BUG_ID>.md

**Contents** (generated from FIX-APPROACH-TEMPLATE.md):
- Fix implementation strategy
- Code changes with explanations
- Reference to feature spec patterns
- C# standards applied
- Backward compatibility verified

**Example**:
```markdown
# Bug BLAZ-1247 Fix Approach

## Strategy
Invalidate virtualization cache when sort is applied.

## Implementation
File: src/Internal/Actions/Sort.cs
Method: ApplySort()
Change: Add InvalidateVirtualizationCache() call

## Code Pattern
References: openspec/specs/sorting/spec.md section 3.2
Pattern: Cache invalidation on state change (lines 156-162 in spec)

## C# Standards
- ✓ Guard clauses applied
- ✓ XML documentation updated
- ✓ Method naming follows PascalCase convention
```

### Stage 6 Output: PERFORMANCE-<BUG_ID>.md

**Contents**:
- Security audit checklist results
- Performance metrics (before/after)
- Memory baseline and post-fix measurements
- Regression status

**Example**:
```markdown
# Bug BLAZ-1247 Performance & Security

## Security Audit
✓ Input validation: OK
✓ XSS prevention: OK
✓ Secrets management: OK

## Performance Metrics

| Metric | Before | After | Delta |
|--------|--------|-------|-------|
| Memory | 125 MB | 124 MB | -1 MB |
| CPU | 8% | 8% | → OK |
| Render | 45 ms | 46 ms | +1 ms |

Status: ✓ NO REGRESSION
```

---

## Lessons Learned Integration

After **Stage 7: Archive**, the bug fix is captured in OpenSpec:

### openspec/specs/lessons-learned.md

**Entry Format**:
```markdown
### BUG-<ID> — <One-line summary>
**Date**: <date>
**Feature**: <feature_name> (from domain_map)
**Root Cause**: <root cause> (file:line)
**Prevention**: <how to prevent this in future>
**Recommendation**: <recommended pattern or guard>
**Spec Reference**: openspec/specs/<feature>/spec.md
**Test Coverage**: test/Bugs/<BUG_ID>/Specification.cs

#### Details
[Summary of finding]
```

**Example**:
```markdown
### BUG-1247 — Sort order not preserved after filter applied
**Date**: 2026-03-19
**Feature**: sorting
**Root Cause**: Internal/Actions/Sort.cs:ApplySort() (line 234) - virtualization cache not invalidated
**Prevention**: Always invalidate caches after sort state change; verify in code review
**Recommendation**: Use pattern from openspec/specs/sorting/spec.md section 3.2 for cache invalidation on state change

#### Details
When sort was applied to a filtered+virtualized grid, the virtual cache was not invalidated, causing stale rows to display. Pattern now requires explicit cache invalidation after sort operations.
```

---

## Feature Impact Analysis Integration

### How feature-impact-analysis-skill is Invoked

**At Stage 2**:

```
Bug Description: "Sort order not preserved after applying filter"

System identifies: Feature area = "sorting"

Invokes feature-impact-analysis-skill:
  ├─ Load sorting-skill
  ├─ Check sorting-skill interaction matrix
  ├─ Interactions found: filtering, virtualization, grouping, paging
  ├─ For each interaction:
  │   ├─ Load corresponding skill (filtering-skill, virtualization-skill, etc.)
  │   ├─ Read feature spec: openspec/specs/[feature]/spec.md
  │   └─ Identify regression risk
  ├─ Return feature interaction graph
  ├─ Generate regression test scenarios
  └─ Document in ANALYSIS-<BUG_ID>.md
```

**Result**:
- Complete map of affected features
- List of regression test scenarios
- Priority of each test scenario
- Examples of each interaction

**Example Feature Interaction Map**:
```
sorting (primary feature)
├─ interactions: [filtering, grouping, virtualization, paging, selection]
├─ interaction: sorting + filtering
│   ├─ risk: HIGH
│   ├─ test: Apply sort, then apply filter; verify sort order preserved
│   └─ spec: openspec/specs/filtering/spec.md section 4.1
├─ interaction: sorting + virtualization
│   ├─ risk: CRITICAL
│   ├─ test: Apply sort on virtual grid; verify cache invalidated
│   └─ spec: openspec/specs/virtualization/spec.md section 2.3
└─ ... (other interactions)
```

---

## OpenSpec Commands Integration

### Relevant Commands

```bash
# Start a bug fix (activates this skill)
/bugfix: <BUG_ID> <description>

# Confirm/reject at approval gates
/confirm: <BUG_ID> APPROVE
/confirm: <BUG_ID> REVISE
/confirm: <BUG_ID> ESCALATE

# Explore bug before fixing (optional pre-Stage 1)
/opsx:explore <BUG_ID>

# Propose alternative approach (if needed)
/opsx:propose <BUG_ID> <approach>

# Apply changes (Stage 4)
/opsx:apply <BUG_ID>

# Archive completed fix
/opsx:archive <BUG_ID>

# List all bugs in progress
openspec list --filter type=bug-fix
```

---

## Configuration & Customization

### Add New Feature Skill

To enable regression testing for a new feature:

1. Create new skill in `.codestudio/skills/<feature>-skill/`
2. Add domain mapping entry in `openspec/config.yaml`
3. Bug-Fix-Resolver will automatically load it when:
   - Bug mentions the feature
   - feature-impact-analysis-skill identifies interaction

Example:
```yaml
# In openspec/config.yaml
domain_map:
  new_feature:
    category: "core_data"
    components: ["GridNewFeature"]
    files: ["GridNewFeature.cs", "Internal/Actions/NewFeature.cs"]
    spec: "openspec/specs/new-feature/spec.md"
    interactions: ["sorting", "filtering", "virtualization"]
    risk_level: "high"
```

### Update Approval Gates

Edit `.codestudio/skills/bug-fix-resolver/SKILL.md` **Approval Gate Checklist** section to add new criteria.

### Customize Templates

Edit template files in `docs/dev-process/`:
- `ANALYSIS-TEMPLATE.md`
- `TEST-SPECIFICATION-TEMPLATE.md`
- `FIX-APPROACH-TEMPLATE.md`

---

## Token Budget Management

The bug-fix-resolver manages OpenSpec token budgets:

```
Per Bug Fix Invocation:

Stage 1 (Intake):           ~150 tokens
Stage 2 (Analysis):         ~400 tokens
  - Load bug-fix-resolver-skill: ~100 tokens
  - Load feature skills: ~150 tokens
  - feature-impact-analysis-skill: ~150 tokens
Stage 3 (TDD Design):       ~300 tokens
Stage 4 (Implementation):   ~250 tokens
Stage 5 (Regression):       ~200 tokens
Stage 6 (Security/Perf):    ~350 tokens
Stage 7 (Archive):          ~150 tokens
─────────────────────────────────────
Total per bug:              ~1800 tokens (within 3000 limit)
```

If approaching limit:
1. Checkpoint findings in docs/
2. Request human reviewer to continue
3. Archive intermediate state

---

## Summary

The Bug-Fix-Resolver is a fully OpenSpec-integrated skill system that:

✓ **Registers** in `openspec/config.yaml` as an operation  
✓ **Loads** feature skills dynamically based on bug domain  
✓ **Invokes** feature-impact-analysis-skill for cross-feature analysis  
✓ **Generates** OpenSpec artifacts (analysis, tests, fix, performance)  
✓ **Updates** feature specs when behavior clarified  
✓ **Captures** lessons learned for future reference  
✓ **Archives** changes in OpenSpec for audit trail  

**Next Steps**:
1. Review `.codestudio/skills/bug-fix-resolver/SKILL.md` for expert knowledge
2. Review `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` for workflow
3. Start a bug fix: `/bugfix: <BUG_ID> <description>`
4. Follow the 7-stage workflow with human gates
5. Deliver PR + documentation + lessons learned

---

**Version**: 1.0  
**Last Updated**: March 19, 2026  
**Framework**: Syncfusion Blazor DataGrid  
**OpenSpec Version**: 2.0
