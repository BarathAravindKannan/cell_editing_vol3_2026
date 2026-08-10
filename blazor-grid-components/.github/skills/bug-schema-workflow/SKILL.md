```skill
---
name: bug-schema-workflow
description: >
  10-stage, TDD-driven, human-gated bug-fix workflow for the Syncfusion Blazor DataGrid.
  Driven by bug-schema.yaml. Uses /opsx:generate <artifact-id> commands.
  Complements spec-driven schema (feature work). Use this skill for BUG FIXES ONLY.
---

# Bug-Schema Workflow Skill

## Schema Routing Summary

| Intent | Schema | Entry Command |
|--------|--------|--------------|
| Fix a bug | **bug-schema** | `/opsx:generate bug-report <change-name>` |
| Implement a feature | **spec-driven** | `/opsx:propose <change-name>` |

---

## When to Load This Skill

Load this skill when the user:
- Describes a bug or regression in the Syncfusion Blazor DataGrid
- Uses any `/opsx:generate` command
- References a BUG ID (e.g., `BLAZ-1247`, `BUG-001`)
- Says: "fix a bug", "something is broken", "regression", "error when..."

Do NOT load this skill for feature requests — use `spec-driven` / `/opsx:propose` instead.

---

## 10-Stage Workflow

| Stage | Command | Output | Gate |
|-------|---------|--------|------|
| 1 | `/opsx:generate bug-report` | `bug-report.md` | ⏸️ Human |
| 2 | `/opsx:generate root-cause` | `root-cause.md` + failing test | ⏸️ Human |
| 3 | `/opsx:generate tdd-tests` | `tdd-tests.md` (all FAIL) | ⏸️ Human |
| 4 | `/opsx:generate fix-spec` | `fix-spec.md` | ⏸️ Human |
| 5 | `/opsx:generate implementation` | `implementation-notes.md` + source fix | ⏸️ Human |
| 6 | `/opsx:generate regression-tests` | `regression-tests.md` | ⏸️ Human |
| 7 | `/opsx:generate performance-tests` | `performance-tests.md` | ⏸️ Human |
| 8 | `/opsx:generate security-tests` | `security-tests.md` | ⏸️ Human |
| 9 | `/opsx:generate human-approval` | `approval.md` | ⏸️ Human (APPROVED) |
| 10 | `/opsx:generate final-verification` | `final-verification.md` | ✅ Auto → Apply unlocked |

---

## Key Files

| File | Purpose |
|------|---------|
| `bug-schema.yaml` | Schema definition + artifact dependency graph |
| `openspec/config.yaml → schemas.bug-schema` | Registration in OpenSpec config |
| `openspec/config.yaml → operations.bug_fix` | Operation entry for CLI discovery |
| `.github/prompts/opsx-generate.prompt.md` | Dispatcher — routes to per-stage prompts |
| `.github/prompts/opsx-generate-<id>.prompt.md` | Per-stage execution prompts (10 files) |
| `openspec/templates/<id>.md` | Artifact templates (10 files) |
| `openspec/changes/<change-name>/` | Output directory per bug |

---

## TDD Discipline (Non-Negotiable)

```
Stage 2 → Create ONE failing test (proves bug exists)
Stage 3 → Create FULL test suite (ALL tests FAIL)
Stage 5 → Implement fix → ALL tests PASS
Stage 6 → Run regression suite → 0 failures
```

Tests that pass BEFORE the fix is applied are INVALID — they do not prove the bug.

---

## Token Budget (from openspec/config.yaml)

| Stage | Budget |
|-------|--------|
| Stages 1-5 (explore/propose/apply) | 2400–2600 tokens |
| Stages 6-9 (audit) | 1600 tokens |
| Stage 10 (archive) | 500 tokens |
| Global hard limit | 3000 tokens |

---

## Integration with Existing Skills

At Stage 2 (root-cause), this skill invokes:
- `feature-impact-analysis-skill` — to map cross-feature interactions
- Matching domain skill (e.g., `sorting-skill`, `filtering-skill`) — loaded from `domain_map`

At Stage 6 (regression-tests), it uses:
- `domain_map.[domain].interactions` from `openspec/config.yaml`
- All interacting domain skills for regression coverage

---

## Quick-Start

```
# Start a new bug fix:
/opsx:generate bug-report BLAZ-1247

# After each human approval, proceed to next stage:
/opsx:generate root-cause BLAZ-1247
/opsx:generate tdd-tests BLAZ-1247
/opsx:generate fix-spec BLAZ-1247
/opsx:generate implementation BLAZ-1247
/opsx:generate regression-tests BLAZ-1247
/opsx:generate performance-tests BLAZ-1247
/opsx:generate security-tests BLAZ-1247
/opsx:generate human-approval BLAZ-1247
/opsx:generate final-verification BLAZ-1247

# After Stage 10 completes:
/opsx:apply BLAZ-1247      ← merge/deploy
/opsx:archive BLAZ-1247    ← archive artifacts
```
```
