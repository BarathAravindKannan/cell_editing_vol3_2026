# Bug-Fix-Resolver: File Manifest & Directory Structure

## Overview

This document provides a complete inventory of all files that make up the Bug-Fix-Resolver system, organized by location and purpose.

---

## File Structure

```
workspace/
│
├── .codestudio/
│   ├── skills/
│   │   └── bug-fix-resolver/                          [SKILL DIRECTORY]
│   │       ├── SKILL.md                               [EXPERT KNOWLEDGE]
│   │       ├── README.md                              [USER GUIDE]
│   │       ├── OPENSPEC-INTEGRATION.md                [OPENSPEC INTEGRATION]
│   │       └── FILE-MANIFEST.md                       [THIS FILE]
│   │
│   └── prompts/
│       └── opsx-bug-fix-resolver.prompt.md            [EXECUTABLE WORKFLOW]
│
├── docs/
│   └── dev-process/
│       ├── bug-fix-resolver-agent.md                  [FULL SPECIFICATION]
│       ├── bug-fix-security-performance-standards.md  [STANDARDS & CHECKLIST]
│       ├── HOW-TO-USE-BUG-FIX-RESOLVER.md             [QUICK START]
│       ├── BUG-FIX-RESOLVER-README.md                 [OVERVIEW]
│       ├── BUG-FIX-RESOLVER-INDEX.md                  [NAVIGATION]
│       ├── BUG-FIX-RESOLVER-DELIVERY.md               [DELIVERY MANIFEST]
│       │
│       ├── ANALYSIS-TEMPLATE.md                       [STAGE 2 TEMPLATE]
│       ├── TEST-SPECIFICATION-TEMPLATE.md             [STAGE 3 TEMPLATE]
│       ├── FIX-APPROACH-TEMPLATE.md                   [STAGE 4 TEMPLATE]
│       │
│       ├── ANALYSIS-<BUG_ID>.md                       [GENERATED PER BUG]
│       ├── TEST-SPECIFICATION-<BUG_ID>.md             [GENERATED PER BUG]
│       ├── FIX-APPROACH-<BUG_ID>.md                   [GENERATED PER BUG]
│       └── PERFORMANCE-<BUG_ID>.md                    [GENERATED PER BUG]
│
├── openspec/
│   ├── config.yaml                                    [UPDATED: operations.bug_fix entry]
│   └── specs/
│       ├── sorting/spec.md                            [DOMAIN SPECS]
│       ├── filtering/spec.md
│       ├── grouping/spec.md
│       ├── editing/spec.md
│       ├── virtualization/spec.md
│       ├── paging/spec.md
│       ├── selection/spec.md
│       ├── infinite-scroll/spec.md
│       ├── aggregates/spec.md
│       ├── events/spec.md
│       ├── lifecycle/spec.md
│       ├── interop/spec.md
│       ├── columns/spec.md
│       ├── templates/spec.md
│       ├── overview/spec.md
│       └── lessons-learned.md                         [UPDATED AFTER EACH BUG FIX]
│
└── test/
    └── Bugs/
        └── <BUG_ID>/
            ├── Reproduction.cs                        [FAILING TEST - STAGE 2]
            └── Specification.cs                       [COMPLETE TEST SUITE - STAGE 3]
```

---

## Core Files (4 files)

### 1. `.codestudio/skills/bug-fix-resolver/SKILL.md`
**Purpose**: Expert knowledge system for bug-fix-resolver skill  
**Size**: ~3KB  
**Contents**:
- Skill name and description
- Training insights and key principles
- Code location map for Blazor Grid
- Feature interaction matrix
- Approval gate checklists
- Prompt template for agent invocation

**Usage**: Automatically loaded when `/bugfix:` command invoked

---

### 2. `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md`
**Purpose**: Executable 7-stage workflow prompt  
**Size**: ~15KB  
**Contents**:
- Activation command: `/bugfix: <BUG_ID> <description>`
- 7-stage workflow with detailed instructions
- Stage-by-stage execution with human gates
- Structured confirmation templates
- Token budget management
- Examples and troubleshooting

**Usage**: Executed by agent when bug-fix-resolver skill loaded

---

### 3. `openspec/config.yaml`
**Purpose**: Register bug-fix operation in OpenSpec framework  
**Size**: Updated (added ~20 lines)  
**Changes**:
- Added `bug_fix_resolver` prompt entry
- Added `operations.bug_fix` registration with:
  - Command syntax
  - Skill reference
  - Workflow stages (7)
  - Approval gates (4)
  - Mandatory documents
  - Feature skill integration

**Usage**: OpenSpec framework reference for bug-fix operation discovery

---

### 4. `docs/dev-process/bug-fix-resolver-agent.md`
**Purpose**: Complete 7-stage workflow specification  
**Size**: ~30KB  
**Contents**:
- Full specification of all 7 stages
- Stage inputs, actions, gates, deliverables
- Example walkthrough (Bug #1247)
- Approval gate templates
- Issue reproduction patterns
- TDD test design guidelines
- Security and performance standards
- Cross-feature regression prevention

**Usage**: Reference document for understanding complete workflow

---

## User Guide Files (4 files)

### 5. `.codestudio/skills/bug-fix-resolver/README.md`
**Purpose**: Skill README with quick start and usage guide  
**Size**: ~8KB  
**Contents**:
- Overview and quick start
- 7-stage workflow diagram
- Key principles (governance, TDD, feature-aware)
- Deliverables description
- Feature interaction matrix
- Security & performance checklist
- Approval gate checklist
- Usage examples
- Commands reference
- File structure
- Troubleshooting

**Usage**: First file developers read when using bug-fix-resolver

---

### 6. `docs/dev-process/HOW-TO-USE-BUG-FIX-RESOLVER.md`
**Purpose**: Quick-start guide for end users  
**Size**: ~12KB  
**Contents**:
- Before/After scenarios
- Step-by-step user guide
- Command syntax
- Approval gate workflow
- Generated output files
- Where to find templates
- Example bug fix (sort order bug)
- FAQ
- Troubleshooting

**Usage**: First reference for new users starting bug fix

---

### 7. `.codestudio/skills/bug-fix-resolver/OPENSPEC-INTEGRATION.md`
**Purpose**: Explains OpenSpec integration and artifact generation  
**Size**: ~12KB  
**Contents**:
- Architecture diagram
- Skill registration in OpenSpec
- Feature skill integration matrix
- Workflow: Bug to OpenSpec artifact flow
- Artifact generation (ANALYSIS, TEST-SPEC, FIX-APPROACH, PERFORMANCE)
- Lessons learned integration
- Feature impact analysis invocation
- OpenSpec commands integration
- Configuration & customization
- Token budget management

**Usage**: Reference for understanding OpenSpec integration

---

### 8. `.codestudio/skills/bug-fix-resolver/FILE-MANIFEST.md`
**Purpose**: Complete file inventory and reference  
**Size**: This file  
**Contents**:
- Directory structure
- File descriptions
- Cross-reference guide

**Usage**: Navigation hub for finding specific files

---

## Template Files (3 files)

### 9. `docs/dev-process/ANALYSIS-TEMPLATE.md`
**Purpose**: Template for Stage 2 (Root Cause Analysis) output  
**Size**: ~4KB  
**Structure**:
```
Bug ID: BLAZ-XXXX
Title: [Bug title]

## Reproduction
- Steps to reproduce
- Test code (failing)

## Feature Interactions
- Affected features
- Impact on each

## Root Cause
- File + Method + Line
- Issue description

## Assumptions
- List of assumptions made

## Confidence Level
- HIGH / MEDIUM / LOW
```

**Usage**: Developers copy and complete for each bug

---

### 10. `docs/dev-process/TEST-SPECIFICATION-TEMPLATE.md`
**Purpose**: Template for Stage 3 (TDD Test Design) output  
**Size**: ~5KB  
**Structure**:
```
Bug ID: BLAZ-XXXX

## Test Suite Location
- test/Bugs/BLAZ-XXXX/Specification.cs

## Primary Scenario
- Test code (fails before fix)

## Edge Cases
- Boundary conditions
- Null values
- Empty collections

## Feature Interactions
- Sort + Filter test
- Group + Virtual test
- Edit + Select test

## Test Status
- [ ] All tests fail (before fix)
- [ ] Test count: N
```

**Usage**: Developers copy and complete for each bug

---

### 11. `docs/dev-process/FIX-APPROACH-TEMPLATE.md`
**Purpose**: Template for Stage 4 (Fix Implementation) output  
**Size**: ~6KB  
**Structure**:
```
Bug ID: BLAZ-XXXX

## Strategy
- Fix approach overview

## Implementation
- File: [path]
- Method: [name]
- Change: [description]

## Code Pattern
- Reference to feature spec
- Pattern from guidelines

## C# Standards
- [ ] Guard clauses applied
- [ ] XML documentation
- [ ] Naming conventions

## Backward Compatibility
- [ ] No breaking changes
- [ ] Event behavior unchanged
```

**Usage**: Developers copy and complete for each bug

---

## Reference & Standards Files (2 files)

### 12. `docs/dev-process/bug-fix-security-performance-standards.md`
**Purpose**: C# standards, security checklist, performance verification  
**Size**: ~8KB  
**Contents**:
**Part 1: C# Standards**
- Naming conventions (PascalCase, _camelCase, UPPER_SNAKE_CASE)
- XML documentation requirements
- Code style (braces, indentation, spacing)
- Async/await patterns

**Part 2: Security Checklist**
- Input validation
- XSS prevention
- SQL injection prevention
- Secrets management
- Authentication/Authorization
- Logging best practices
- ARIA accessibility

**Part 3: Performance Verification**
- Baseline measurement
- Memory profiling
- CPU analysis
- Render time measurement
- Memory leak detection
- Virtualization impact
- Re-render optimization

**Part 4: Integrated Checklist**
- Pre-implementation checklist
- Post-implementation checklist
- Performance regression criteria

**Usage**: Referenced at Stage 4 (Implementation) and Stage 6 (Audit)

---

### 13. `docs/dev-process/BUG-FIX-RESOLVER-INDEX.md`
**Purpose**: Navigation hub organized by role  
**Size**: ~4KB  
**Sections**:
- **For Developers Starting a Bug Fix**
- **For Reviewers at Approval Gates**
- **For Architects Analyzing Patterns**
- **For QA Testing Regressions**
- **File Navigation by Stage**
- **Quick Links**

**Usage**: Reference for finding right documentation by role/stage

---

## Generated Output Files (Per Bug)

### 14. `docs/dev-process/ANALYSIS-<BUG_ID>.md`
**Purpose**: Root cause analysis for specific bug  
**Generated By**: Stage 2  
**Example**: `ANALYSIS-BLAZ-1247.md`  
**Contents**:
- Reproduction test (failing)
- Feature interactions (from feature-impact-analysis-skill)
- Root cause identification
- Exact file, method, line number
- Assumptions documented

---

### 15. `docs/dev-process/TEST-SPECIFICATION-<BUG_ID>.md`
**Purpose**: Complete test suite for specific bug  
**Generated By**: Stage 3  
**Example**: `TEST-SPECIFICATION-BLAZ-1247.md`  
**Contents**:
- All test code (fails before fix)
- Primary scenario + edge cases
- Feature interaction scenarios
- Test status (all failing)

---

### 16. `docs/dev-process/FIX-APPROACH-<BUG_ID>.md`
**Purpose**: Fix implementation documentation  
**Generated By**: Stage 4  
**Example**: `FIX-APPROACH-BLAZ-1247.md`  
**Contents**:
- Fix strategy
- Code changes with explanations
- C# standards verification
- Backward compatibility notes

---

### 17. `docs/dev-process/PERFORMANCE-<BUG_ID>.md`
**Purpose**: Security and performance audit results  
**Generated By**: Stage 6  
**Example**: `PERFORMANCE-BLAZ-1247.md`  
**Contents**:
- Security audit checklist results
- Performance metrics (before/after)
- Memory baseline and measurements
- Regression status

---

### 18. `test/Bugs/<BUG_ID>/Reproduction.cs`
**Purpose**: Failing test that reproduces bug  
**Generated By**: Stage 2  
**Example**: `test/Bugs/BLAZ-1247/Reproduction.cs`  
**Status**: Fails before fix, passes after fix

---

### 19. `test/Bugs/<BUG_ID>/Specification.cs`
**Purpose**: Complete test suite (primary + edge cases + features)  
**Generated By**: Stage 3  
**Example**: `test/Bugs/BLAZ-1247/Specification.cs`  
**Status**: All tests fail before fix, all pass after fix

---

## Related Documentation (3 files)

### 20. `docs/dev-process/BUG-FIX-RESOLVER-README.md`
**Purpose**: System overview and reference guide  
**Size**: ~10KB  
**Contents**:
- What is Bug-Fix-Resolver?
- Why 7 stages?
- System components
- Integration with OpenSpec
- Usage overview
- Key files and locations

**Usage**: System overview for new team members

---

### 21. `docs/dev-process/BUG-FIX-RESOLVER-DELIVERY.md`
**Purpose**: Delivery manifest showing all created files  
**Size**: ~8KB  
**Contents**:
- Delivery overview
- Files created (grouped by category)
- File descriptions
- Total lines of code/documentation
- Integration points

**Usage**: Project delivery checklist

---

### 22. `openspec/specs/lessons-learned.md`
**Purpose**: Captured learnings from each bug fix  
**Updated**: After each bug fix (Stage 7)  
**Entry Format** (per bug):
```markdown
### BUG-<ID> — <One-line summary>
**Date**: <date>
**Feature**: <feature>
**Root Cause**: <cause> (file:line)
**Prevention**: <how to prevent>
**Recommendation**: <recommended pattern>
```

**Usage**: Reference for preventing future bugs; loaded by feature-impact-analysis-skill

---

## Cross-Reference Guide

### By Stage

**Stage 1: Bug Intake**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 1 section)
- Standards: `bug-fix-resolver-agent.md` (Stage 1 section)

**Stage 2: Root Cause Analysis**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 2 section)
- Template: `docs/dev-process/ANALYSIS-TEMPLATE.md`
- Output: `docs/dev-process/ANALYSIS-<BUG_ID>.md`
- Test: `test/Bugs/<BUG_ID>/Reproduction.cs`

**Stage 3: TDD Test Design**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 3 section)
- Template: `docs/dev-process/TEST-SPECIFICATION-TEMPLATE.md`
- Output: `docs/dev-process/TEST-SPECIFICATION-<BUG_ID>.md`
- Tests: `test/Bugs/<BUG_ID>/Specification.cs`

**Stage 4: Fix Implementation**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 4 section)
- Template: `docs/dev-process/FIX-APPROACH-TEMPLATE.md`
- Output: `docs/dev-process/FIX-APPROACH-<BUG_ID>.md`
- Standards: `bug-fix-security-performance-standards.md` (Part 1)

**Stage 5: Regression Testing**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 5 section)
- Reference: `.codestudio/skills/bug-fix-resolver/SKILL.md` (Interaction Matrix)
- Skills: Feature-specific skills (sorting-skill, filtering-skill, etc.)

**Stage 6: Security & Performance**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 6 section)
- Standards: `bug-fix-security-performance-standards.md` (Parts 2-3)
- Output: `docs/dev-process/PERFORMANCE-<BUG_ID>.md`

**Stage 7: Archive**
- Workflow: `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 7 section)
- Update: `openspec/specs/lessons-learned.md`

### By Role

**Developer (Starting Bug Fix)**
1. Start: `/bugfix: <BUG_ID> <description>`
2. Read: `docs/dev-process/HOW-TO-USE-BUG-FIX-RESOLVER.md`
3. Reference: `.codestudio/skills/bug-fix-resolver/README.md`
4. Navigate: `docs/dev-process/BUG-FIX-RESOLVER-INDEX.md`

**Reviewer (At Approval Gates)**
1. Gate 1: See `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md` (Stage 1 gate template)
2. Gate 2: Review `ANALYSIS-<BUG_ID>.md` + `test/Bugs/<BUG_ID>/Reproduction.cs`
3. Gate 3: Review `TEST-SPECIFICATION-<BUG_ID>.md` + `test/Bugs/<BUG_ID>/Specification.cs`
4. Gate 6: Review `PERFORMANCE-<BUG_ID>.md` + security checklist results

**Architect**
1. Patterns: `openspec/specs/lessons-learned.md`
2. Features: `openspec/specs/<feature>/spec.md` (per domain)
3. Integration: `.codestudio/skills/bug-fix-resolver/OPENSPEC-INTEGRATION.md`

**QA (Regression Testing)**
1. scripts: `test/Bugs/<BUG_ID>/Specification.cs` (feature interaction tests)
2. Matrix: `.codestudio/skills/bug-fix-resolver/SKILL.md` (Interaction Matrix section)
3. Standards: `bug-fix-security-performance-standards.md` (Part 3)

---

## Summary Statistics

| Category | Count | Total Size |
|---|---|---|
| Core Skill Files | 4 | ~25 KB |
| User Guide Files | 4 | ~35 KB |
| Template Files | 3 | ~15 KB |
| Reference Files | 2 | ~12 KB |
| Generated per Bug | 4 | ~20 KB (per bug) |
| Test Files per Bug | 2 | ~10 KB (per bug) |
| Related Docs | 3 | ~18 KB |
| **Total Base System** | **20 files** | **~105 KB** |
| **Per Bug Addition** | **6 files** | **~30 KB** |

---

## Getting Started

### For First-Time Users
1. Read `.codestudio/skills/bug-fix-resolver/README.md` (5 min)
2. Read `docs/dev-process/HOW-TO-USE-BUG-FIX-RESOLVER.md` (10 min)
3. Start a bug fix: `/bugfix: <BUG_ID> <description>`

### For System Integrators
1. Review `openspec/config.yaml` (operations.bug_fix entry)
2. Review `.codestudio/skills/bug-fix-resolver/OPENSPEC-INTEGRATION.md`
3. Verify feature skills installed

### For Code Reviewers
1. Bookmark `docs/dev-process/BUG-FIX-RESOLVER-INDEX.md` (navigation)
2. Review approval gate templates in `.codestudio/prompts/opsx-bug-fix-resolver.prompt.md`
3. Keep `bug-fix-security-performance-standards.md` accessible

### For Architects
1. Review `openspec/specs/lessons-learned.md` (quarterly)
2. Monitor `openspec/specs/<feature>/spec.md` updates
3. Reference `.codestudio/skills/bug-fix-resolver/OPENSPEC-INTEGRATION.md` for patterns

---

## Version History

| Version | Date | Changes |
|---|---|---|
| 1.0 | 2026-03-19 | Initial release |

---

**Last Updated**: March 19, 2026  
**Framework**: Syncfusion Blazor DataGrid  
**OpenSpec Version**: 2.0
