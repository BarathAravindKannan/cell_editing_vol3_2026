# Skills Used in Integrated Bug Fix Workflow

## Overview

The **Integrated Bug Fix Workflow** (`opsx-bug-fix-integrated.prompt.md`) uses multiple skills at different stages. This document maps each stage to the skills it uses and how they interact.

---

## Skills Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│         INTEGRATED WORKFLOW SKILLS DEPENDENCY GRAPH             │
└─────────────────────────────────────────────────────────────────┘

Stage 1: Bug Intake
└─→ No skills (pure parsing + human gate)

Stage 2: Root Cause Analysis (/opsx:explore)
├─→ openspec-explore skill
├─→ feature-impact-analysis-skill
├─→ [Dynamic] Matching feature skill (sorting-skill, filtering-skill, etc.)
└─→ accessibility-requirements skill (for ARIA checks)

Stage 3: TDD Test Design (/opsx:propose)
├─→ openspec-propose skill
├─→ testing skill (bUnit template support)
└─→ [Feature-specific] Test templates

Stage 4: Fix Implementation (/opsx:apply)
├─→ openspec-apply-change skill
└─→ C# standards (built-in, not external skill)

Stage 5: Regression Testing (Automated)
├─→ testing skill (cross-feature test runner)
├─→ All feature skills:
│   ├─→ sorting-skill
│   ├─→ filtering-skill
│   ├─→ grouping-skill
│   ├─→ virtualization-skill
│   ├─→ scroll-management-skill
│   ├─→ editing-skill
│   ├─→ paging-skill
│   └─→ performance-optimization skill
└─→ javascript-interop skill (JS validation)

Stage 6: Security & Performance Audit (Automated)
├─→ accessibility-requirements skill
├─→ performance-optimization skill
├─→ testing skill (benchmarking)
└─→ javascript-interop skill (XSS/injection checks)

Stage 7: Archive (/opsx:archive)
└─→ openspec-archive-change skill
```

---

## Stage-by-Stage Skills Usage

### **STAGE 1: Bug Intake & Human Confirmation**

**Skills Used**: None (Pure workflow logic)

**Actions**:
- Parse bug description
- Extract domain keywords
- Assess severity
- Create OpenSpec change directory

**Output**: Bug acceptance or rejection

---

### **STAGE 2: Reproduction & Root Cause Analysis**

**Command**: `/opsx:explore <change-name>`

#### **Primary Skill: openspec-explore**
```yaml
Description: Think deeply, investigate problems, clarify requirements
Purpose: Guided exploration of bug location and root cause
When: Used to systematically investigate codebase
Actions:
  • Ask clarifying questions
  • Search codebase for affected code
  • Read domain-specific files
  • Create Exploration.md with findings
```

#### **Secondary Skill: feature-impact-analysis-skill**
```yaml
Description: Analyze cross-feature interactions in grid
Purpose: Identify which other features might be affected
When: After locating the bug, to map side effects
Actions:
  • Check if bug touches: virtualization, sorting, filtering, grouping, editing, paging, selection
  • Document feature interactions in Exploration.md
  • Identify regression test scenarios
```

#### **Dynamic Skill: Domain-Specific Feature Skill**
```yaml
Selected based on bug description keywords:
  • "scrolling" → scroll-management-skill
  • "sort" → sorting-skill
  • "filter" → filtering-skill
  • "group" → grouping-skill
  • "virtual" → virtualization-skill
  • "edit" → editing-skill
  • "page" → paging-skill

Purpose: Deep dive into specific feature architecture
When: Used to understand feature internals
Actions:
  • Load feature spec (openspec/specs/[feature]/spec.md)
  • Identify cross-feature patterns
  • Locate exact file + method + line
  • Create failing BUnit test
```

#### **Supporting Skill: accessibility-requirements**
```yaml
Description: WCAG 2.2 Level AA compliance, ARIA patterns, keyboard interaction
Purpose: Verify ARIA and keyboard behaviors not broken by bug
When: Used to check accessibility impact
Actions:
  • Verify ARIA roles/attributes correct in affected area
  • Check keyboard navigation still works
  • Validate focus management
```

**Output**: Exploration.md with:
- Exact file + method + line number
- Feature interactions identified
- Failing BUnit test created
- Root cause documented

---

### **STAGE 3: TDD Test Design**

**Command**: `/opsx:propose <change-name>`

#### **Primary Skill: openspec-propose**
```yaml
Description: Propose a new change with design and specification
Purpose: Convert exploration findings into design + tasks
When: After exploration.md approved
Actions:
  • Read Exploration.md findings
  • Create proposal.md (problem → root cause → solution)
  • Create design.md (architecture → files affected → alternatives)
  • Create tasks.md (actionable breakdown)
  • Create specs/ directory (if needed)
```

#### **Secondary Skill: testing**
```yaml
Description: bUnit component testing, test specifications
Purpose: Ensure test suite is comprehensive
When: Used to validate test case design
Actions:
  • Apply TEST-SPECIFICATION-TEMPLATE
  • Ensure tests are atomic and reproducible
  • Verify edge cases covered
  • Create test data fixtures
```

**Output**: Complete design artifacts:
- proposal.md
- design.md
- tasks.md (7-10 tasks):
  - Tasks 1-N: Test cases (all FAIL initially)
  - Task N+1: Fix implementation
  - Task N+2: Regression testing
  - Task N+3: Build + test verification

---

### **STAGE 4: Fix Implementation**

**Command**: `/opsx:apply <change-name>`

#### **Primary Skill: openspec-apply-change**
```yaml
Description: Implement tasks from an OpenSpec change
Purpose: Execute fix code based on design
When: After design.md approved
Actions:
  • Read tasks.md sequentially
  • Implement each task (no comments allowed)
  • Follow C# standards
  • Keep changes minimal and focused
  • Mark tasks complete [x]
  • Run tests after each task
```

**C# Standards Applied** (Built-in, not external skill):
```yaml
• Naming conventions: PascalCase (public), _camelCase (private)
• Guard clauses: Validate inputs before processing
• XML documentation: All public members documented
• Async/Await: ConfigureAwait(false) for library code
• Exception handling: Specific catches, proper logging
• No code comments (self-documenting code only)
```

**Output**: 
- Fixed code implemented
- All tests passing
- No compiler warnings
- tasks.md marked complete (100%)

---

### **STAGE 5: Regression Testing**

**Automatic After Stage 4** (No user interaction)

#### **Primary Skill: testing**
```yaml
Description: Cross-feature regression testing
Purpose: Verify no features broken by fix
When: Automatically after fix implementation
Actions:
  • Run sorting + filtering tests
  • Run grouping + virtualization tests
  • Run editing + selection tests
  • Run paging + all features tests
  • Run infinite scroll + all features tests
  • Validate state synchronization
```

#### **Feature-Specific Skills Used**:
```yaml
sorting-skill:
  • Verify sort order preserved after fix
  • Test sort + filter interaction
  • Test sort + grouping interaction

filtering-skill:
  • Verify filter predicates still work
  • Test filter + sort interaction
  • Test filter + grouping interaction

grouping-skill:
  • Verify group expand/collapse works
  • Test grouping + sort interaction
  • Test grouping + virtualization interaction

virtualization-skill:
  • Verify scroll viewport correct
  • Test virtual scroll + fix interaction
  • Verify cache invalidation still works

editing-skill:
  • Verify edit/add/delete still work
  • Test editing with sort/filter/grouping active
  • Test editing with virtualization active

paging-skill:
  • Verify page navigation works
  • Test paging with other features active

scroll-management-skill:
  • Verify horizontal/vertical scroll
  • Test frozen column scrolling
  • Verify scrollbar synchronization

performance-optimization:
  • Measure render time (should not increase)
  • Check re-render cycles (should not increase)
  • Verify no new allocations
```

#### **Supporting Skill: javascript-interop**
```yaml
Description: Safe JS interop patterns, IJSObjectReference lifecycle
Purpose: Verify JS interop paths still work
When: Used if fix touches JS interop
Actions:
  • Verify JS module cleanup
  • Check DotNetObjectReference disposal
  • Validate callback patterns
```

**Output**: 
- ✓ 106+ regression tests passing
- ✓ All feature combinations verified
- ✓ No regressions detected

---

### **STAGE 6: Security & Performance Audit**

**Automatic After Stage 5** (No user interaction, triggers approval gate)

#### **Skill: accessibility-requirements**
```yaml
Description: WCAG 2.2 Level AA compliance, ARIA, keyboard interaction
Purpose: Verify accessibility not broken
When: Automatic security check
Actions:
  • Verify ARIA roles/attributes correct
  • Check keyboard navigation works
  • Validate focus management
  • Verify screen reader compatibility
  • Check tab order correct
  • Validate semantic HTML
```

#### **Skill: performance-optimization**
```yaml
Description: Rendering performance, re-render avoidance, virtualization, memory
Purpose: Verify no performance regression
When: Automatic performance audit
Actions:
  • Measure baseline memory (before fix)
  • Measure post-fix memory
  • Compare: must be < 5% regression
  • Measure render time
  • Check GC collections
  • Verify no memory leaks
  • Test with large datasets
  • Validate WebAssembly AOT (if applicable)
```

#### **Skill: javascript-interop**
```yaml
Description: Safe JS interop patterns, module isolation, data marshaling
Purpose: Verify no security vulnerabilities in JS calls
When: Used if fix involves JS
Actions:
  • Check for XSS vulnerabilities (eval, MarkupString misuse)
  • Verify data sanitization
  • Validate no secrets in JS objects
  • Check module isolation
  • Verify error handling in interop
```

**Built-in Security Checklist**:
```yaml
• Input validation: No null/buffer overflows
• XSS prevention: No MarkupString misuse, no eval()
• SQL/Injection: Not applicable to Blazor grid (but validate principle)
• Secrets management: No hardcoded API keys, passwords, tokens
• Authorization/Authentication: Not broken by fix
• Logging: No PII, credentials, or sensitive data in logs
• ARIA patterns: Maintained (accessibility = security)
• Keyboard navigation: Still functional
```

**Output**:
- ✓ Security checklist passed
- ✓ Performance baseline captured (125.2 MB)
- ✓ Post-fix performance measured (124.8 MB)
- ✓ No regression detected (-0.4 MB ✓)
- ✓ No memory leaks (GC returns all memory)

---

### **STAGE 7: Archive & Lessons Learned**

**Command**: `/opsx:archive <change-name>`

#### **Primary Skill: openspec-archive-change**
```yaml
Description: Archive completed change to archive folder
Purpose: Finalize bug fix and preserve artifacts
When: After all stages complete
Actions:
  • Create archive directory (YYYY-MM-DD-<change-name>)
  • Move change folder to archive
  • Keep ONLY: .openspec.yaml, Exploration.md, proposal.md, design.md, tasks.md, specs/
  • Delete temporary files (summaries, reports, audit docs)
  • Update openspec/specs/lessons-learned.md
  • Generate changelog entry
```

**Output**: 
- ✓ Change archived
- ✓ Lessons learned documented
- ✓ Ready for production merge

---

## Skills Interdependencies

```
┌─────────────────────────────────────────────────────────────────┐
│                    SKILLS INTERACTION MAP                       │
└─────────────────────────────────────────────────────────────────┘

openspec-explore
    │
    ├─→ feature-impact-analysis-skill
    │   (Determine affected features)
    │
    ├─→ [Domain Skill]
    │   (sorting-skill, filtering-skill, etc.)
    │   (Depends on bug keywords)
    │
    └─→ accessibility-requirements
        (Check ARIA/keyboard impact)

        ↓ [Creates Exploration.md]

openspec-propose
    │
    ├─→ testing skill
    │   (Design test suite)
    │
    └─→ [Domain Skill]
        (Map affected files)

        ↓ [Creates proposal.md, design.md, tasks.md]

openspec-apply-change
    │
    ├─→ testing skill
    │   (Run failing tests before fix)
    │   (Run passing tests after fix)
    │
    ├─→ C# Standards
    │   (Naming, documentation, guards, async)
    │
    └─→ [Domain Skill]
        (Understand what to fix)

        ↓ [Implements fix code]

testing skill (Regression)
    │
    ├─→ sorting-skill
    ├─→ filtering-skill
    ├─→ grouping-skill
    ├─→ virtualization-skill
    ├─→ editing-skill
    ├─→ paging-skill
    ├─→ scroll-management-skill
    ├─→ javascript-interop
    │
    └─→ performance-optimization
        (Measure performance)

        ↓ [Verifies no regressions]

accessibility-requirements
    │
    └─→ [Stage 6 Audit]

performance-optimization
    │
    ├─→ javascript-interop
    │   (JS performance impact)
    │
    └─→ [Stage 6 Audit]

        ↓ [Security & Performance verified]

openspec-archive-change
    │
    └─→ [Finalize]

        ↓ [Change archived]
```

---

## Quick Reference: Which Skill Used When

| Stage | Skill Name | Triggered By | Purpose |
|-------|-----------|--------------|---------|
| 2 | openspec-explore | `/opsx:explore` | Find bug root cause |
| 2 | feature-impact-analysis-skill | openspec-explore | Map feature interactions |
| 2 | [Domain Skill] | Bug keywords | Deep dive into feature |
| 2 | accessibility-requirements | openspec-explore | Check ARIA/keyboard |
| 3 | openspec-propose | `/opsx:propose` | Create design artifacts |
| 3 | testing | openspec-propose | Design test suite |
| 4 | openspec-apply-change | `/opsx:apply` | Implement fix code |
| 5 | testing | Auto after Stage 4 | Run regression tests |
| 5 | [All Feature Skills] | testing | Cross-feature validation |
| 5 | javascript-interop | testing | Verify JS paths |
| 6 | accessibility-requirements | Auto after Stage 5 | Security audit |
| 6 | performance-optimization | Auto after Stage 5 | Performance audit |
| 6 | javascript-interop | Auto after Stage 5 | JS security audit |
| 7 | openspec-archive-change | `/opsx:archive` | Archive change |

---

## How Skills are Selected Dynamically

### **Domain Skill Selection (Stage 2)**

Based on bug keywords, the correct feature skill is auto-loaded:

```
Bug Description Keywords → Detected Domain → Loaded Skill
──────────────────────────────────────────────────────────

"scrolling", "virtualization" → virtualization-skill
"sort order", "sorting" → sorting-skill
"filter", "predicate" → filtering-skill
"group", "grouping" → grouping-skill
"edit", "CRUD" → editing-skill
"page", "paging" → paging-skill
"select", "selection" → selection-skill
"scroll", "horizontal" → scroll-management-skill
"infinity", "infinite scroll" → scroll-management-skill
```

---

## Cross-Skill Communication

### **Artifact Passing**

```
Stage 2 Output: Exploration.md
    ↓
Stage 3 Input: openspec-propose reads Exploration.md
    ↓
Stage 3 Output: proposal.md, design.md, tasks.md
    ↓
Stage 4 Input: openspec-apply-change reads tasks.md
    ↓
Stage 4 Output: Fixed code, all tests passing
    ↓
Stage 5 Input: Automated regression tests on fixed code
    ↓
Stage 5 Output: Regression test results
    ↓
Stage 6 Input: Automated security + performance audit
    ↓
Stage 6 Output: Audit results
    ↓
Stage 7 Input: openspec-archive-change reads all artifacts
    ↓
Stage 7 Output: Archived change with lessons learned
```

---

## Summary: Total Skills Used

| Category | Skills | Count |
|----------|--------|-------|
| **Core OpenSpec** | openspec-explore, openspec-propose, openspec-apply-change, openspec-archive-change | 4 |
| **Feature-Specific** | sorting-skill, filtering-skill, grouping-skill, virtualization-skill, editing-skill, paging-skill, scroll-management-skill | 7 |
| **Cross-Cutting** | feature-impact-analysis-skill, testing, performance-optimization, accessibility-requirements, javascript-interop | 5 |
| **Built-in** | C# Standards (naming, documentation, guards, async) | 1 |
| **TOTAL** | | **17** |

---

## Skill Maturity & Reliability

| Skill | Status | Dependencies | Risk |
|-------|--------|--------------|------|
| openspec-explore | ✓ Stable | Human input | LOW |
| openspec-propose | ✓ Stable | Exploration.md | LOW |
| openspec-apply-change | ✓ Stable | proposal.md + design.md + tasks.md | LOW |
| openspec-archive-change | ✓ Stable | All artifacts | LOW |
| feature-impact-analysis-skill | ✓ Stable | Domain knowledge | MEDIUM |
| testing | ✓ Stable | bUnit framework | LOW |
| performance-optimization | ✓ Stable | Benchmarking tools | MEDIUM |
| accessibility-requirements | ✓ Stable | ARIA spec | LOW |
| javascript-interop | ✓ Stable | JS expertise | MEDIUM |
| [Domain Skills] | ✓ Stable | Feature knowledge | LOW |

