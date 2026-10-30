# Agent.md — entry for coding agents (Syncfusion Blazor DataGrid)

Repo-root discovery file for Code Studio, Claude Code and generic agents. It routes you and fixes
the **order** of work; it does not duplicate the layers it points to.

**Sources of truth:** gates and thresholds live in `.agent/harness.yaml`; phases and approvals in
`openspec/config.yaml` + `.github/prompts/opsx-*`. The two are joined by
`.github/prompts/harness-binding.md` (read it once per session). It **wins** over any command or
threshold literal in an opsx prompt, `openspec/config*.yaml`, a skill or an agent file. The loop
protocol and boundaries are in `.agent/instructions.md`.

> This is the **Blazor** repo: C#/Razor library (`src/`), TypeScript interop (`scripts/`), xUnit +
> bUnit tests (`tests/bunit/`), .NET SDK. Commands from the EJ2 TypeScript repos (`npx gulp test`,
> Karma, `run_typecheck.py`, the `blazor/` lock) do **not** apply here. The untracked
> `harness-binding.md` at the repo root is a TypeScript-repo reference copy. Ignore it and use
> `.github/prompts/harness-binding.md`.

## 1. Auto-routing — MANDATORY first action for any bug / performance / feature text

The user does **not** need a slash command, a change id, or the word "OpenSpec". If the message
describes a defect, slowness, or a capability to add (e.g. *"fix the Grid scrolling slow when
touch scroll only"*), do this before touching any code:

```text
python .agent/tools/start-task.py "<user text verbatim>"
```

It classifies the request (perf words such as *slow / lag / jank / freeze* or a `BLAZ-<id>` ⇒
**bugfix**) and routes to grid feature(s) in `.agent/features.yaml`. It then creates
`openspec/changes/<OPSX-YYYYMMDD-slug>/.openspec.yaml`, creates **and picks** the harness task
(linked by `changeId`), and prints a **HANDOFF** block. Then:

1. Open the prompt named in HANDOFF and follow it phase by phase:
   - bug / perf → `.github/prompts/opsx-bug-workflow.prompt.md` (B0–B10, CP-1…CP-5)
   - feature / enhancement → `.github/prompts/opsx-feature-workflow.prompt.md` (F0–F10)
2. Do **not** ask the user for the type. Ask only if start-task exits `2` (no signal) or `3`
   (another task active — offer `--park-active`). Routine checkpoints auto-continue at HIGH or
   MEDIUM confidence. Pause for one focused question only at LOW confidence or an unconditional
   hard stop (binding §1a).
3. After the work is **committed**, run the full gate set, generate the evidence report, and
   finish:

```text
python .agent/tools/harness.py gate run --lane all --scope diff
python .agent/tools/evidence_report.py --task <T-id> --into-task --into-change
python .agent/tools/task.py done <T-id>        # refuses without passing evidence at HEAD
```

An inner-lane-only run leaves the outer gates *not run*, which gives verdict `partial`, and
`done` refuses that. Iterate with `--lane inner` and finish with `--lane all`. Questions and
code reading need no task. Changing code always does.

## 2. Who owns what

| Concern | Owner | Location |
|---|---|---|
| What / why / which hard stops need a human | **OpenSpec + confidence policy** | `openspec/config.yaml`, `openspec/changes/<change_id>/`, `.github/prompts/opsx-*`, `.github/prompts/harness-binding.md` |
| Feature → skill → files → tests → interactions | Feature registry | `.agent/features.yaml` (via `impact.py`) |
| How a grid feature must behave / render | Domain skills | `.github/skills/<feature>-skill/SKILL.md` (read before touching a feature) |
| Is it done (build, bUnit, diff coverage, API, audits) | **Harness** | `.agent/harness.yaml`, `.agent/tools/harness.py`, `.agent/evidence/<T-id>/` |
| Task state (one source) | `task.py` | `.agent/tasks/index.json` (+ `<T-id>.md`); never edit the JSON |
| Free text → change + task | `start-task.py` | links `changeId` ⇄ `T-id` (`.openspec.yaml harnessTask:`) |
| Evidence report | `evidence_report.py` | `.agent/evidence/<T-id>/evidence.md`, task `## Evidence`, change `tasks.md` |
| Command / threshold conflicts | `harness-binding.md` | wins over any literal in an opsx prompt or config |

```
free text ─► start-task.py ─► openspec/changes/<change_id>/.openspec.yaml  +  task T-xxx (active, baseSha)
                                   │                                              │
      opsx-bug-workflow B1–B5 / feature F1–F5 (confidence checkpoints)    per edit: bUnit-project build
                                   │                                     + impact.py --run unit
                              git commit ──────────────► harness gate run --lane inner --scope diff (iterate)
                                   │                                              │
                     review (reviewer.md + catalog) ◄──── gate run --lane all --scope diff (final)
                                   │                                              │
                       evidence_report.py ◄──────────── gate-summary.json @ HEAD
                                   │
                          task.py done ─► archive ─► bookkeeping commit (tasks/progress/changes only)
```

## 3. Session start, then read first

```text
python .agent/tools/harness.py doctor        # contract valid, tools present, no drift
python .agent/tools/task.py status
python .agent/tools/sync_prompts.py --check  # prompts mirrored, every cited path exists
```

If `doctor` reports drift or a missing tool, say so before working. A missing tool turns its
gates into NOT RUN, never PASS.

1. `.agent/instructions.md` — loop protocol, boundaries, "narrate, never assert"
2. `.github/prompts/harness-binding.md` — command map, confidence policy, phase ↔ gate mapping
3. `.agent/project-context.md` — architecture, hotspots, conventions
4. `.agent/harness.yaml` — gates, thresholds, risk paths, skills
5. `.agent/progress.md` — what previous sessions learned
6. `.agent/adapters/claude-code.md` | `codestudio.md` | `generic.md` — capability bindings for your host

## 4. Non-negotiables

- **Narrate, never assert.** Anything a tool measures (tests, coverage, API compat, leaks, perf, a11y, secrets) is claimed only by citing `gate-summary.json` or the generated evidence report. If no tool covers a claim, write `UNVERIFIED` (`harness.py explain "<claim>"`).
- Evidence before `task.py done` (`requireEvidenceForDone: true`). Evidence is bound to HEAD: commit first. The only commit after the final gate run is the post-`done` bookkeeping commit (`.agent/tasks/**`, `.agent/progress.md`, `openspec/changes/**`).
- **No mutation testing** (Stryker declined). Never weaken a gate: no lowered thresholds, skipped or deleted tests, weakened asserts, `[ExcludeFromCoverage]`, `#pragma` or suppressions. No `--override` except a real tooling outage, journalled.
- Repo-true commands only: per-edit `dotnet build tests/bunit/Syncfusion.Blazor.Tests.Grids.csproj --nologo -v:q`, fast tests `impact.py --run unit`, gates via `harness.py gate run`. Not the stale `Bunit/…` or root-level `dotnet build` from `openspec/config.yaml`.
- **Every bugfix and feature leaves a spec and tests behind, for future reference** (binding §2a; exact paths in HANDOFF and `.openspec.yaml`):
  1. a **new spec file** `openspec/changes/<change-id>/specs/<domain>/spec.md` (REQ + WHEN/THEN, in the format of the domain spec, e.g. `openspec/specs/scroll/spec.md`). The `openspec-staleness` gate fails without it, and the archive keeps it.
  2. a **new test file** for the change: `tests/bunit/CR Issues/BLAZ-<id>.razor` (or `<change-id>.razor`) for bugs, `tests/bunit/Actions/<change-id>.razor` for features.
  3. a **new fixture appended to the feature's existing test file** (e.g. `tests/bunit/Actions/ScrollbarVisibility.razor` for a scroll fix), tagged `[BLAZ-<id>]` / `[<change-id>]`. Append only: existing fixtures and asserts are never edited.
- Tests reference the local `src/Syncfusion.Blazor.Grid.csproj` (`test-project-local-ref` gate), never a published grid package.
- Never write Syncfusion API claims from memory: cite a skill or `.agent/knowledge/`. Never hide, suppress or CSS-hide the licensing banner.
- Human approval: risk paths (`**/*.csproj`, `**/package.json`, `sf.snk`, `**/*Licens*`, `**/PublicAPI.*`, `**/Migrations/**`, `src/SfGrid.razor`) need `task.py done <T> --approved-by "<name>"`. The `src/SfGrid.razor` pattern matches no file (the component is `src/SfGrid.razor.cs`; parked), so treat `src/SfGrid.razor.cs` parameter changes as a hard stop too. Also needing a human: public API changes, HIGH-risk features, and edits to `.github/skills/**`, `openspec/specs/**`, `openspec/config*.yaml`, existing bUnit fixtures, `sf.snk` or versions. Writing the **active** change dir `openspec/changes/<change_id>/` through the workflow is sanctioned. Park other decisions in `.agent/DECISIONS-REQUIRED.md`.
- Control-plane edits (`.agent/harness.yaml`, `.agent/features.yaml`) are followed by `harness.py lock`; `.agent/{workflows,agents,policies,checklists,templates}/**` by `harness.py cp-lock`.
- One task in flight (`maxActive: 1`). Never commit to protected branches; use a feature branch.

## 5. Gates (`.agent/harness.yaml`, tier L1)

| Gate | Lane | Blocking | Purpose |
|---|---|:---:|---|
| `harness-verify` | inner | yes | control plane matches `HARNESS-LOCK.json` |
| `feature-map-verify` | inner | yes | registry refs real, no orphan src/ts |
| `test-project-local-ref` | inner | yes | bUnit csproj builds local `src/` |
| `repro-proof` | inner | yes (bugfix) | repro fails at `baseSha`, passes at HEAD |
| `openspec-staleness` | inner | yes (`--spec`) | OpenSpec artifacts present, age reported |
| `build` | inner | yes | `dotnet build tests/bunit/Syncfusion.Blazor.Tests.Grids.csproj` (lib + tests) |
| `analyzers` | inner | yes | no net-new Roslyn errors (ratchet) |
| `bunit-regression` | inner | yes | no new failures vs `bunit-known-failures.txt`, test floor kept |
| `bunit-tests` | inner | yes | full suite `failed == 0` |
| `coverage-diff` | inner | yes | changed lines ≥ 80 % covered (coverlet) |
| `api-compat` | inner | yes | no break vs `.agent/baselines/PublicAPI.txt` |
| `ts-typecheck` | inner | yes | `tsc --noEmit` on `scripts/` (needs `node_modules`) |
| `syncfusion-version-consistency` | inner | yes | Syncfusion packages on one version |
| `syncfusion-banner-suppression` | inner | yes | licensing banner never hidden |
| `vulnerable-packages` | outer | yes | no vulnerable NuGet (transitive) |
| `secrets` | outer | yes | gitleaks, no committed secrets |
| `ts-lint` | outer | no | ESLint on `scripts/` |
| `a11y-bunit` | outer | no | ARIA/RTL/adaptive tests; `tests=0` ⇒ UNVERIFIED |
| `leak-bunit` | outer | no | render/dispose leak tests; `tests=0` ⇒ UNVERIFIED |
| `perf-ab` | outer | no | BASE vs HEAD medians ≤ 1.10; `tests=0` ⇒ UNVERIFIED |

`gate run --gate X` gives a `partial` verdict. Use it only to iterate, and always finish with the
full `--lane all` run. `--keep-going` collects every failure but never changes the verdict.

### Known state of the gates (2026-10-13, base `cb5f95c`, T-018)

These were red before any change and block `task.py done` for **every** task until a human
resolves them (`.agent/DECISIONS-REQUIRED.md`). Each run states the current truth: read the
latest `.agent/evidence/<T>/evidence.md`, not this table.

| Gate | State | Owner of the fix |
|---|---|---|
| `api-compat` | 5 public types missing vs baseline (`CellEditCanceledEventArgs`, …) | human: restore types or re-baseline |
| `syncfusion-version-consistency` | floating `Version="*"` in csproj files | human: `*.csproj` risk path |
| `ts-typecheck`, `ts-lint` | no `node_modules` (`npm install` needs the internal `@syncfusion` registry; `ghooks` writes `.git/hooks`) | human: run `npm install` |
| `secrets` | `gitleaks` not installed | human: install gitleaks |
| `bunit-tests`, `bunit-regression` | **flaky**, timing-dependent tests: `Filtering using buttons` (`tests/bunit/CR Issues/BLAZ-952838.razor:30`) and `Detail template with events` (`tests/bunit/Actions/DetailTemplate.razor`, `WaitForAssertion` timeout). On the same build, 3 of 6 full-suite runs had a failure | human: harden the tests, or accept them via `--allow-grow` |
| `perf-ab` (non-blocking) | no perf tests exist yet (`tests/bunit/Perf/<Feature>*.razor`) ⇒ `tests=0` ⇒ perf claims UNVERIFIED | T-016 |

Fixed in T-018:
- The `build` / `analyzers` / `vulnerable-packages` commands ran `dotnet` at the repo root (MSB1003). They now target the bUnit csproj.
- Unquoted `{EVIDENCE_DIR}` arguments split on the space in a checkout path such as `blazor-grid-components (1)`, which broke `feature-map-verify`, `coverage-diff`, `a11y-bunit`, `leak-bunit`, `ts-lint` and `secrets`. They are now quoted.
- `harness.py` never expanded globbed evidence paths, so `coverage-diff` (`cov/**/coverage.cobertura.xml`) could not be parsed on any machine. It now resolves to the newest match.
- The bUnit compile error at `tests/bunit/Actions/Virtualization.razor:529` is fixed.
- `bunit-known-failures.txt` was recorded: 0 known, floor 1218 (the full suite passed 1218/1218 on net8.0).

Windows: the .NET SDK may be a per-user install that is off PATH. `dotnet --version` should
print 10.x. If it doesn't, prepend `%LOCALAPPDATA%\Microsoft\dotnet` to PATH (Git Bash:
`/c/Users/<you>/AppData/Local/Microsoft/dotnet`) before any gate run.

## 6. Tooling cheat sheet

```text
python .agent/tools/start-task.py "<free text>"         # intake (--dry-run, --type, --park-active, --slug)
python .agent/tools/task.py list | status | info <id> | pick <id> | block "<why>" --id <id> | done <id>
python .agent/tools/impact.py --text "<bug>"            # feature routing (B1)
python .agent/tools/impact.py --out .agent/evidence/<T>/impact.json --run unit   # fast impacted tests
python .agent/tools/harness.py doctor | validate | verify | lock | gate list
python .agent/tools/harness.py gate run --lane inner --scope diff                # iterate
python .agent/tools/harness.py gate run --lane all --scope diff                  # final verdict
python .agent/tools/evidence_report.py --task <T> --into-task --into-change      # evidence report
python .agent/tools/sync_prompts.py [--check]           # mirror .github/prompts -> .codestudio/prompts, validate
python .agent/tools/bunit_regression.py --update-baseline --trx <full.trx>       # baseline: human-approved only
```

Use a real Python 3.12+ (`PYTHONUTF8=1` recommended on Windows).

## 7. Where things are

- Hotspots (review first): `src/Internal/Actions/Edit.cs`, `src/SfGrid.Methods.cs`, `src/SfGrid.razor.cs`, `scripts/sf-grid-fn.ts`, `src/Internal/Renderer/GridRow.CellRenderer.cs`, `src/Internal/Actions/FocusHandler.cs`, `src/SfGrid.Lifecycle.cs`
- Reviewer and defect catalog: `.agent/agents/reviewer.md`, `.agent/agents/catalog-defects-blazor.md`
- Skill-mapped reading rules (token budget): `.codestudio/prompts/opsx-bug-fix-optimized.prompt.md` (its stages map onto B1–B10)
- Domain specs: `openspec/specs/<domain>/spec.md`; archived changes: `openspec/changes/archive/`
- Policies / workflows: `.agent/policies/`, `.agent/workflows/`; decisions: `.agent/DECISIONS-REQUIRED.md`; state: `.agent/PROJECT-STATE.md`

## 8. Adapters

- Claude Code: `.agent/adapters/claude-code.md`
- Code Studio: `.agent/adapters/codestudio.md` (entry: `.codestudio/codestudio-instructions.md`, prompts mirrored in `.codestudio/prompts/`)
- Generic: `.agent/adapters/generic.md`
