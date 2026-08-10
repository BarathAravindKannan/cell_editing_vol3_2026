# Bug-Fix-Resolver Skill

## Overview

The **Bug-Fix-Resolver Skill** is an expert knowledge system that provides structured guidance for systematic bug resolution in the Syncfusion Blazor DataGrid. It implements a disciplined 7-stage lifecycle with human governance gates, TDD-first testing, feature-aware regression prevention, and comprehensive quality assurance.

This skill is designed to work seamlessly with the OpenSpec framework and the feature-impact-analysis-skill to ensure no regressions are introduced when fixing bugs.

---

## Quick Start

### 1. Activate Bug-Fix Workflow

```bash
/bugfix: BLAZ-1247 Sort order not preserved after applying filter
```

The system will:
- Guide you through all 7 stages
- Request human confirmation at critical gates
- Track deliverables in structured templates
- Verify cross-feature interactions
- Audit security and performance

### 2. Follow the 7-Stage Workflow

```
Stage 1: Bug Intake & Human Confirmation
    ↓ (Human approves)
Stage 2: Issue Reproduction & Root Cause Analysis
    ↓ (Human confirms root cause)
Stage 3: TDD Test Design (Tests Before Fix)
    ↓ (Human approves test suite)
Stage 4: Fix Implementation
    ↓ (Human confirms fix)
Stage 5: Regression Testing (Cross-Feature Validation)
    ↓ (Human approves regression results)
Stage 6: Security & Performance Audit
    ↓ (Human approves security/performance)
Stage 7: Archive & Lessons Learned
    ✓ Complete
```

### 3. Use Decision Commands

At each approval gate, use the `/confirm` command:

```bash
/confirm: BLAZ-1247 APPROVE      ← Move to next stage
/confirm: BLAZ-1247 REVISE       ← Request modifications
/confirm: BLAZ-1247 REJECT       ← Cancel bug fix
/confirm: BLAZ-1247 ESCALATE     ← Escalate to architect
```

---

## Key Principles

### ✓ Human-in-the-Loop Governance
- 4 mandatory approval gates (Stages 1, 2, 3, 6)
- No automated decisions on critical paths
- Clear confirmation templates for reviewers

### ✓ TDD-First Discipline
- **Failing test written BEFORE fix code**
- Tests prove bug exists (fail initially)
- Tests prove fix works (pass after fix)
- Example:
  ```
  Stage 3: Write failing tests covering:
    • Primary bug scenario
    • Edge cases (boundary, null, empty)
    • Feature interactions (sort+filter, group+virtual, edit+select)
  
  Stage 4: Implement fix
  
  Result: All tests pass ✓
  ```

### ✓ Feature-Aware Regression Prevention
- Integrates feature-impact-analysis-skill
- Maps affected features: sorting, filtering, grouping, virtualization, editing, etc.
- Cross-feature tests verify no regressions:
  - Sort + Filter ✓
  - Group + Virtualization ✓
  - Edit + Selection ✓
  - Infinite-scroll + Paging ✓

### ✓ Security & Performance Verified
- Complete security checklist (XSS, SQL injection, secrets, auth, logging)
- Performance baseline captured before/after fix
- Memory leak detection
- No performance regressions accepted

### ✓ OpenSpec Integration
- All findings documented in OpenSpec format
- Feature specs updated if behavior clarified
- Lessons learned captured for future reference
- Change archived in OpenSpec for audit trail

---

## Deliverables

Each bug fix produces structured documentation:

### Root Cause Analysis
**File**: `docs/dev-process/ANALYSIS-<BUG_ID>.md`

Contents:
- Bug reproduction test (failing initially)
- Feature interactions identified
- Root cause with exact file/method/line reference
- Assumptions and unknowns documented
- Confidence level (HIGH/MEDIUM/LOW)

### Test Specification
**File**: `docs/dev-process/TEST-SPECIFICATION-<BUG_ID>.md`

Contents:
- Complete test suite (before fix)
- Test intent documentation
- Edge cases and boundary conditions
- Feature interaction scenarios
- Initial test failure proof

### Fix Approach
**File**: `docs/dev-process/FIX-APPROACH-<BUG_ID>.md`

Contents:
- Fix implementation strategy
- Code changes with explanations
- C# standards applied
- Backward compatibility verified
- Minimal/focused approach

### Performance & Security Audit
**File**: `docs/dev-process/PERFORMANCE-<BUG_ID>.md`

Contents:
- Security checklist results
- Performance metrics (before/after)
- Memory usage baseline
- CPU profiling
- Regression status

### PR Documentation
**File**: Git PR

Contents:
- Bug summary and root cause
- Test coverage details
- Performance impact
- Feature interactions verified
- Changelog entry

---

## Feature Interaction Matrix

The skill automatically considers cross-feature impact:

| Bug + Feature | Risk Level | Regression Test |
|---|---|---|
| Sort | Critical | Verify sort works with filter/group/virtual active |
| Filter | Critical | Verify filter works with sort/group/virtual active |
| Group | Critical | Verify groups expand/collapse; aggregates recompute |
| Virtualization | Critical | Verify virtual scroll; cache invalidated correctly |
| Editing | Critical | Verify edit/add/delete with sort/filter/group active |
| Paging | High | Verify page navigation with filter/sort/group active |
| Selection | High | Verify selection persisted/cleared appropriately |
| Infinite Scroll | High | Verify buffer updates correctly |
| Aggregates | High | Verify aggregates recompute on filter/group change |

---

## Security & Performance Checklist

### Security Audit (Stage 6)

```
✓ Input Validation
  - All user inputs validated
  - No buffer overflows
  - Type safety enforced

✓ XSS Prevention
  - No MarkupString misuse
  - HTML properly escaped
  - No script injection vectors

✓ Secrets Management
  - No hardcoded credentials
  - No API keys in code
  - No connection strings exposed

✓ Authentication/Authorization
  - Proper access control maintained
  - No privilege escalation

✓ Logging
  - No sensitive data logged
  - No stack traces with PII
  - Proper log levels used

✓ Accessibility
  - ARIA roles correct
  - Keyboard navigation preserved
  - Focus management intact
```

### Performance Verification (Stage 6)

```
Metrics Captured:
  - Memory usage (MB)
  - CPU usage (%)
  - Render time (ms)
  - Garbage collection pressure

Before Fix vs. After Fix:
  - Memory delta acceptable (±5%)
  - CPU delta acceptable (no degradation)
  - Render time delta acceptable (±10%)

Status: ✓ NO REGRESSION
```

---

## Approval Gate Checklist

### Gate 1: Bug Validity (Stage 1)
```
[ ] Bug description clearly identifies the issue
[ ] Reproduction steps are testable and repeatable
[ ] Expected behavior vs. actual behavior stated clearly
[ ] Severity level appropriate
[ ] Not a duplicate of existing bug
```

### Gate 2: Root Cause Identified (Stage 2)
```
[ ] Bug reproduced with failing BUnit test
[ ] Exact fault location documented (file + method + line)
[ ] Feature interactions identified
[ ] Root cause hypothesis confirmed
[ ] Assumptions documented
```

### Gate 3: Test Coverage Complete (Stage 3)
```
[ ] Test cases cover bug scenario
[ ] Test cases cover feature interactions
[ ] All tests fail before fix (prove bug exists)
[ ] Test edge cases identified
[ ] Test specification complete
```

### Gate 6: Security & Performance Verified (Stage 6)
```
[ ] Security checklist passed
[ ] Performance baseline measured
[ ] Post-fix memory/CPU measured
[ ] No regression in performance
[ ] No new memory leaks detected
```

---

## Usage Examples

### Example 1: Sort Order Bug

```bash
/bugfix: BLAZ-1247 Sort order not preserved after applying filter

Stage 1: ✓ Bug approved (human confirms)
Stage 2: ✓ Root cause found: Internal/Actions/Sort.cs line 234
         (Cache not invalidated after sort applied)
Stage 3: ✓ Tests written covering sort+filter interaction
Stage 4: ✓ Cache invalidation added
Stage 5: ✓ Cross-feature tests: sort, filter, group, virtual all pass
Stage 6: ✓ Security OK, Performance: memory -1MB (acceptable)
Stage 7: ✓ PR created, lessons learned documented

RESULT: BUG FIXED ✓
```

### Example 2: Edit Mode Regression

```bash
/bugfix: BLAZ-1504 Edit dialog not opening on virtualized grid

Stage 1: ✓ Bug approved
Stage 2: ✓ Root cause found: JS interop timeout in VirtualScroll.ts
Stage 3: ✓ Tests written for edit + virtualization
Stage 4: ✓ Timeout increased, scrollIntoView called first
Stage 5: ✓ Regression test: edit+sort+virtual = PASS
Stage 6: ✓ Performance acceptable
Stage 7: ✓ Archived

RESULT: BUG FIXED ✓
```

---

## Commands Reference

### Start Bug Fix
```bash
/bugfix: <BUG_ID> <brief_description>
```

### Confirm/Reject at Gates
```bash
/confirm: <BUG_ID> APPROVE      ← Move to next stage
/confirm: <BUG_ID> REVISE       ← Request modifications
/confirm: <BUG_ID> REJECT       ← Cancel bug fix
/confirm: <BUG_ID> HOLD         ← Pause for more info
/confirm: <BUG_ID> ESCALATE     ← Escalate to architect
```

### Related Commands
```bash
/opsx:explore <BUG_ID>          ← Explore bug in detail
/opsx:propose <BUG_ID>          ← Propose fix approach
/opsx:apply <BUG_ID>            ← Apply approved changes
/opsx:archive <BUG_ID>          ← Archive completed fix
```

---

## Integration Points

### Feature-Impact-Analysis-Skill
Automatically invoked at **Stage 2** to identify:
- Which features are affected by the bug
- Which features could be affected by the fix
- Cross-feature regression risks
- Required regression tests

### OpenSpec Framework
- All findings stored in OpenSpec format
- Feature specs updated if necessary
- Lessons learned appended to lessons-learned.md
- Change archived for audit trail
- Enables future reference and pattern recognition

### bUnit Testing
- Test reproduction created in `test/Bugs/<BUG_ID>/`
- Test cases verify bug exists and fix works
- Cross-feature regression tests included
- All tests must pass before merge

### C# Standards
- Code follows naming conventions (PascalCase, _camelCase)
- XML documentation on all public members
- Guard clauses for validation
- Async/await with ConfigureAwait(false)

---

## Troubleshooting

### Issue: Can't Reproduce Bug

**Response from Stage 2:**
- Document findings in ANALYSIS-TEMPLATE.md
- Request human guidance:
  - "Ask user for more detail"
  - "Assume fixed in latest build"
  - "Archive as unable-to-reproduce"

### Issue: Root Cause Unclear

**Response:**
- Request architect review
- Suggest spike investigation
- Pause at Stage 2 for human decision

### Issue: Fix Creates Regression

**Response:**
- Revert fix immediately
- Request human review of alternative approach
- Return to Stage 2 for re-analysis

### Issue: Security Concerns

**Response:**
- HALT at Stage 6
- Escalate to security team
- Do not proceed without approval

### Issue: Performance Degrades

**Response:**
- HALT at Stage 6
- Optimize fix or request human approval
- Do not accept performance regression

---

## File Structure

```
.codestudio/skills/bug-fix-resolver/
├── SKILL.md              ← Skill definition (this system)
├── README.md             ← This file

.codestudio/prompts/
├── opsx-bug-fix-resolver.prompt.md  ← Executable workflow

docs/dev-process/
├── bug-fix-resolver-agent.md              ← Full specification
├── bug-fix-security-performance-standards.md
├── ANALYSIS-TEMPLATE.md                   ← Template for Stage 2
├── TEST-SPECIFICATION-TEMPLATE.md         ← Template for Stage 3
├── FIX-APPROACH-TEMPLATE.md               ← Template for Stage 4
├── HOW-TO-USE-BUG-FIX-RESOLVER.md         ← User guide
├── BUG-FIX-RESOLVER-INDEX.md              ← Navigation hub
├── ANALYSIS-<BUG_ID>.md                   ← Generated per bug
├── TEST-SPECIFICATION-<BUG_ID>.md         ← Generated per bug
├── FIX-APPROACH-<BUG_ID>.md               ← Generated per bug
└── PERFORMANCE-<BUG_ID>.md                ← Generated per bug

openspec/config.yaml
└── operations.bug_fix entry (registered)

test/Bugs/<BUG_ID>/
├── Reproduction.cs       ← Failing test (Stage 2)
└── Specification.cs      ← Full test suite (Stage 3)
```

---

## Next Steps

1. **Review** the complete specification: `docs/dev-process/bug-fix-resolver-agent.md`
2. **Start** a bug fix: `/bugfix: <BUG_ID> <description>`
3. **Follow** the workflow: 7 stages with human gates
4. **Deliver**: PR + documentation + lessons learned

---

## Support

- **Full Workflow**: `docs/dev-process/bug-fix-resolver-agent.md`
- **Security & Performance**: `docs/dev-process/bug-fix-security-performance-standards.md`
- **Quick Start**: `docs/dev-process/HOW-TO-USE-BUG-FIX-RESOLVER.md`
- **Templates**: ANALYSIS, TEST-SPECIFICATION, FIX-APPROACH (all in docs/dev-process/)
- **Configuration**: `openspec/config.yaml` (operations.bug_fix section)

---

**Version**: 1.0  
**Last Updated**: March 19, 2026  
**Framework**: Syncfusion Blazor DataGrid  
**Language**: C# / Blazor
