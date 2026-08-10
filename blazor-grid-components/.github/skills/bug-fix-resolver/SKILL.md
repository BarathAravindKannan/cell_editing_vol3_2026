---
name: bug-fix-resolver-skill
description: Expert workflow for systematic bug resolution in the Syncfusion Blazor DataGrid. Provides guided 7-stage lifecycle with human governance gates, TDD-first testing, root-cause analysis, feature-impact awareness, cross-feature regression prevention, C# standards enforcement, security validation, and performance verification.
---

# Skill Instructions

<!-- token-budget: 25 words -->

**Purpose**  
Systematic bug resolution with human governance, TDD discipline, feature-aware regression prevention, and comprehensive quality standards.

---

**Agent Invocation**  
Load this skill when:
- Fixing bugs in Syncfusion Blazor DataGrid
- Investigating regression issues
- Performing root-cause analysis with feature impact assessment
- Conducting security and performance reviews of bug fixes
- Implementing TDD-first test design before fixes

---

## Knowledge References

<!-- token-budget: 80 words -->

- `/docs/dev-process/bug-fix-resolver-agent.md` — 7-stage workflow specification, approval gates, stage deliverables
- `/docs/dev-process/bug-fix-security-performance-standards.md` — C# standards, security checklist, performance verification patterns
- `/docs/dev-process/ANALYSIS-TEMPLATE.md` — Structured root-cause analysis framework
- `/docs/dev-process/TEST-SPECIFICATION-TEMPLATE.md` — TDD test design template (tests before fixes)
- `/docs/dev-process/FIX-APPROACH-TEMPLATE.md` — Implementation strategy framework
- `/docs/dev-process/HOW-TO-USE-BUG-FIX-RESOLVER.md` — User quick-start guide
- `/openspec/openspec/specs/overview/spec.md` — Component architecture overview

---

## Training Insights Applied

<!-- token-budget: 150 words -->

**7-Stage Lifecycle** (Non-negotiable)
1. **Bug Intake & Human Confirmation** — Reviewer validates bug report before investigation
2. **Issue Reproduction & Root Cause Analysis** — Create failing test; identify exact fault location; document assumptions; map feature interactions
3. **TDD Test Design** — Write test cases BEFORE fix code (failing test proves bug exists; passing test proves fix works)
4. **Fix Implementation** — Apply fix following C# standards and security checklist
5. **Regression Testing** — Cross-feature validation using feature-impact-analysis-skill; ensure sorting, filtering, grouping, virtualization, editing unaffected
6. **Security & Performance Audit** — Execute security checklist (XSS, SQL injection, auth, secrets, logging); measure baseline vs. post-fix memory/CPU
7. **Archive & Lessons Learned** — Document findings in OpenSpec; update feature specs if behavior changed

**Key Principles**
- **Human governance mandatory** — Approval gates at Stages 1, 2, 3, 6
- **Feature-aware analysis required** — Bug fixes must consider: sorting, filtering, grouping, virtualization, editing, paging, selection, infinite-scroll, aggregates
- **TDD discipline enforced** — Failing test written BEFORE fix code; test must correctly reproduce bug; test passes after fix
- **No behavior breakage** — Regression testing across all feature interactions; backward compatibility guaranteed
- **Security-first** — Input validation, ARIA patterns, secrets management, no XSS/injection vectors
- **Performance validated** — Memory baseline captured before fix; post-fix memory/CPU verified; no regressions accepted

---

## Code Location Map

<!-- token-budget: 100 words -->

**Core Lifecycle Files** (all bugs touch these)
- **`SfGrid.Lifecycle.cs`** — Component initialization, parameter updates, disposal
- **`SfGrid.Properties.cs`** — Public properties, parameter cascading
- **`SfGrid.Methods.cs`** — Public API methods (sorting, filtering, editing, etc.)
- **`SfGrid.razor.cs`** — Component class, event handlers

**Feature-Specific Action Modules**
- **`Internal/Actions/Sort.cs`** — Sorting logic (~400 lines modified)
- **`Internal/Actions/Filter.cs`** — Filtering logic (~910 lines modified)
- **`Internal/Actions/Group.cs`** — Grouping logic (~1060 lines modified)
- **`Internal/Actions/Edit.cs`** — Editing (CRUD) logic (**heavily restructured: 4244 lines changed**)
- **`Internal/Actions/Paging.cs`** — **NEW** — Paging logic extracted into dedicated module (+213 lines)
- **`Internal/Actions/Selection.cs`** — Selection logic (~748 lines modified)
- **`Internal/Actions/VirtualScroll.cs`** — Virtualization logic (~241 lines modified)
- **`Internal/Actions/InfiniteScroll.cs`** — Infinite scroll logic (minimal changes)
- **`Internal/Actions/Aggregate.cs`** — **NEW** — `ReactiveAggregate<T>` partial class part 1 (+453 lines)
- **`Internal/Actions/ReactiveAggregate.cs`** — `ReactiveAggregate<T>` partial class part 2 (~242 lines modified)
- **`Internal/Actions/Freeze.cs`** — **NEW** — Freeze/frozen column logic extracted (+1289 lines)
- **`Internal/Actions/Searching.cs`** — **NEW** — Search logic extracted (+199 lines)

**Event & Settings Files**
- **`GridEvents.cs`** — Event definitions (OnBeforeEdit, OnBeforeSave, etc.)
- **`EventModels/Grids.cs`** — Event argument models
- **`GridEditSettings.cs`** — Edit mode configuration
- **`GridFilterSettings.razor.cs`** — Filter mode configuration
- **`GridSortSettings.razor.cs`** — Sort mode configuration
- **`GridGroupSettings.razor.cs`** — Group mode configuration

**JS Interop**
- **`scripts/sf-grid.ts`** — Grid JS module
- **`scripts/virtual-scroll.ts`** — Virtualization JS helpers
- **`scripts/edit.ts`** — Editing JS helpers

---

## Approval Gate Checklist

<!-- token-budget: 120 words -->

**Stage 1 Gate: Bug Validity**
- [ ] Bug description unambiguous
- [ ] Reproduction steps testable and repeatable
- [ ] Severity/priority assigned
- [ ] Not a duplicate of existing bug

**Stage 2 Gate: Root Cause Identified**
- [ ] Bug reproduced with failing BUnit test
- [ ] Exact fault location documented (file + method + line)
- [ ] Feature interactions identified (sort, filter, group, etc. affected?)
- [ ] Root cause hypothesis confirmed or rejected
- [ ] Assumptions documented in ANALYSIS-TEMPLATE.md

**Stage 3 Gate: Test Coverage Complete**
- [ ] Test cases cover bug scenario
- [ ] Test cases cover feature interactions
- [ ] All tests fail before fix (prove bug exists)
- [ ] Test edge cases identified
- [ ] TEST-SPECIFICATION-TEMPLATE.md complete

**Stage 6 Gate: Security & Performance Verified**
- [ ] Security checklist passed (input validation, XSS, secrets, auth, logging)
- [ ] Performance baseline measured
- [ ] Post-fix memory/CPU measured
- [ ] No regression in performance
- [ ] No new memory leaks detected

**Final Gate: Archive & Lessons Learned**
- [ ] All 7 stages completed
- [ ] bUnit tests passing
- [ ] Integration tests passing
- [ ] PR created with detailed changelog
- [ ] Lessons learned documented in OpenSpec

---

## Interaction Matrix (Cross-Feature Regression Prevention)

<!-- token-budget: 150 words -->

| Feature Pair | Interaction Risk | Regression Test |
|---|---|---|
| Bug + Sorting | Fix affects row order or sort UI? | Verify sort works on filtered/grouped data post-fix |
| Bug + Filtering | Fix affects filter predicates or UI? | Verify filter works on sorted/grouped data post-fix |
| Bug + Grouping | Fix affects group state or captions? | Verify groups expand/collapse; aggregates recompute post-fix |
| Bug + Virtualization | Fix affects viewport rendering or scroll? | Verify virtualized grid scrolls smoothly; cache invalidated correctly post-fix |
| Bug + Editing | Fix affects row edit state or validation? | Verify edit/add/delete works with filters, sort, groups active post-fix |
| Bug + Paging | Fix affects page navigation or size? | Verify paging works with filters, sort, groups active post-fix |
| Bug + Selection | Fix affects selection state persistence? | Verify selection preserved/cleared appropriately post-fix |
| Bug + Infinite Scroll | Fix affects buffer or row count? | Verify infinite buffer updates correctly post-fix |
| Bug + Aggregates | Fix affects aggregate calculations? | Verify aggregates recompute on filter/group change post-fix |

---

## Prompt Template

<!-- token-budget: 350 words — self-contained, no external doc reads required -->

**Mode**: `bug-fix`  
**Skill**: Bug-Fix-Resolver  

**Context**  
You are resolving a bug in the Syncfusion Blazor DataGrid. This skill provides a structured 7-stage workflow with mandatory human approval gates, TDD-first testing discipline, feature-aware regression prevention, and comprehensive security/performance audits. Your role is to guide the fixer through each stage, collect human confirmations, and ensure no regressions occur.

**Core Workflow** (7 Stages)

**Stage 1: Bug Intake & Human Confirmation**
- Parse bug description, reproduction steps, severity
- Request human reviewer confirmation that bug is valid (not duplicate)
- Proceed only after APPROVE

**Stage 2: Reproduction & Root Cause Analysis**
- Create failing BUnit test that reproduces the bug (test must FAIL initially)
- Investigate code systematically using feature-impact-analysis-skill
- Identify exact fault: file, method name, line number
- Map which features interact (sorting, filtering, grouping, virtualization, etc.)
- Document findings in ANALYSIS-TEMPLATE.md
- Request human confirmation: "Root cause identified and documented?"
- Proceed only after APPROVE

**Stage 3: TDD Test Design**
- Write complete test suite BEFORE writing fix code
- Tests must fail initially (prove bug exists)
- Tests cover bug scenario + edge cases + feature interactions
- Follow TEST-SPECIFICATION-TEMPLATE.md
- Request human confirmation: "Test suite complete? All tests fail initially?"
- Proceed only after APPROVE

**Stage 4: Fix Implementation**
- Apply fix following C# standards (naming, XML docs, code style)
- Reference FIX-APPROACH-TEMPLATE.md for implementation strategy
- Keep changes minimal and focused on root cause
- Run tests: all should pass now
- Request human confirmation: "Fix implemented? All tests pass?"
- Proceed to Stage 5

**Stage 5: Regression Testing**
- Run cross-feature regression tests (sorting + filter, grouping + virtualization, etc.)
- Verify no unintended behavior changes
- Check state synchronization, event flows, lifecycle patterns
- Request human confirmation: "Regression testing passed? No side effects?"
- Proceed only after APPROVE

**Stage 6: Security & Performance Audit**
- Execute security checklist: input validation, XSS, SQL injection, secrets, auth, logging
- Measure performance baseline (before fix) and post-fix (memory, CPU)
- Verify no memory leaks or performance regressions
- Request human confirmation: "Security passed? Performance acceptable?"
- Proceed only after APPROVE

**Stage 7: Archive & Lessons Learned**
- Create comprehensive PR with detailed changelog
- Document findings in OpenSpec: update feature specs if needed
- Add lessons-learned entry to openspec/specs/lessons-learned.md
- Update bug ticket with resolution details

**Key Behaviors**
1. **Never proceed without explicit human approval** at gates 1, 2, 3, 6
2. **Failing test MUST exist before fix** — test failing proves bug, passing proves fix
3. **Feature interactions mandatory** — bug fixes touching edit/sort/filter must verify all combinations work
4. **Security-first** — run full security checklist before merge
5. **Performance verified** — memory and CPU baseline captured and validated post-fix
6. **Minimal changes** — surgical fixes only, no refactoring unless directly related to bug

---

## FAQ

**Q: What if I can't reproduce the bug?**  
A: Document findings in ANALYSIS-TEMPLATE.md. Request human reviewer guidance: "Bug not reproducible with provided steps. Suggest: [A] Ask user for more detail, [B] Assume fixed in latest build, [C] Archive as unable-to-reproduce."

**Q: What if the bug affects multiple features?**  
A: Use feature-impact-analysis-skill to map all affected areas. Create separate test cases for each interaction. Request human confirmation at Stage 3.

**Q: What if performance degrades after fix?**  
A: Revert fix. Request human architect review: "Performance regression detected. Recommend: [A] Optimize fix approach, [B] Accept minimal regression, [C] Reassess root cause."

**Q: How do I update feature specs?**  
A: If bug fix reveals missing/incorrect specification, update openspec/specs/[feature]/spec.md with corrected behavior. Document reasoning in Stage 7 archive.
