# PROJECT STATE — Syncfusion Blazor DataGrid

Last updated: 2026-09-03

## Where the code is

- Library: `src/` (multi-target net8/9/10, v33.1.44, strong-named, NuGet-published)
- Branch: blaz-100-review / development merges via gitea PRs (Jenkins CI: `Jenkinsfile`)
- Recent work: event-driven data binding (1036480) merged via PR #376

## Where the harness is

- `.agent/` created 2026-09-03, tier L1, profile dotnet-blazor + syncfusion overlay
- Harness first task (T-001) DONE: test csproj switched from published
  package 100.2.1 to the local src project reference — suite builds (0 errors)
  and ran: 1201/1202 passed (evidence `.agent/evidence/T-001/bunit.trx`)
- API baseline recorded: 408 members at `.agent/baselines/PublicAPI.txt`,
  api-compat gate green
- Gate-red items parked in DECISIONS-REQUIRED.md: 3 floating Syncfusion
  versions in src csproj, 4 transitive CVEs in the test project, 1 failing
  perf test (CR BLAZ-1015397)

## What is true that tools verified

- Test project builds & suite runs against LOCAL src: 1201/1202 passed at T-001
  (TRX evidence .agent/evidence/T-001/bunit.trx)
- Version gate green: 13 Syncfusion refs, all exact-pin, two legitimate trains
  (33.1.x UI / 100.2.x doc-telemetry)
- All HIGH CVEs cleared on all three projects; AngleSharp 0.17.1 Moderate
  accepted in test project (bunit binary-binding; TRX: 358 MissingMethodException
  failures when overridden to 1.5.0 — the suite is the oracle, it rejected the override)
- Secrets gate green: gitleaks 8.28.0, 0 leaks across 971 commits
- dotnet-gcdump 10.0.731102 installed; leak-probe script at scripts/leak-probe.ps1
- xunit.runner.visualstudio must stay on the 2.x line while xunit is v2 —
  the 3.x adapter silently discovered zero tests (verified via --list-tests)
- Sort-perf investigation BLAZ_1015397 (see below)

## Sort-perf failure BLAZ_1015397 — verdict (2026-09-03)

Test asserts one wall-clock sample of `SortColumnAsync(CustomerID, Descending)`
on 10k records stays under 2000ms; observed 77.9s in the T-001 CI run.

Evidence gathered:
1. The comparer is a hand-rolled `CustomExpandoComparer` using **runtime binder
   (`dynamic`)** per comparison — `dynamic` dispatch is orders of magnitude
   slower than static casts, ~O(n log n) binder calls on 10k rows.
2. The test also enables **virtualization + filtering + grouping** with a
   **column Template** on the sorted column — template rendering amplifies the
   sort action's re-render far beyond a pure sort.
3. Product sort path (`src/Internal/Actions/Data.cs` SortQuery → query.Sort
   with comparer) contains no regression candidate: unchanged churn,
   comparer resolution logic identical to the passing sibling CR tests
   (BLAZ-846870/900932 with the same comparer pattern pass in <200ms without
   templates/virtualization).
4. bunit rendering under testhost is not a perf environment — one sample,
   no warm-up, no median: environment-fragile by construction.

**Verdict: stale/environment-fragile test, not a shipped product regression.**
The sibling comparer CR tests pass in milliseconds. The 77s outlier came from
the template + virtualization + dynamic comparer combination measured once in a
loaded parallel test host. Fix proposal (parked in DECISIONS-REQUIRED.md):
median-of-N sampling with a real budget, or Category=Perf split.

## 2026-09-04 — harness completion

- Control plane verified: 70 files (16 workflows, 15 agents, 22 policies,
  5 checklists, 12 templates), 0 warnings, all 9 task types routable,
  HARNESS-LOCK.json green; skills vocabulary (19) mapped onto real commands.
- **Real product defect found & fixed by the suite-as-oracle**: 6 async-void
  EventAggregator/timer handlers whose continuations crashed the testhost
  ("Test Run Aborted" at 852 then 1034 tests). All 6 hardened (observed
  continuations, dispatcher-safe StateHasChanged via InvokeAsync, disposed
  guards). Final run: **1202/1202 executed, 0 aborts, 1175 passed**.
  Evidence: `.agent/evidence/T-001/bunit-post-asyncvoid-fix.trx`.
  Note: my first instinct (convert handler to `async Task`) was WRONG — the
  aggregator's Trigger path binds `Action<object>`; the compile caught it.
  The correct fix keeps the sync contract and guards the continuation.
- Remaining red (27): bunit 1.23.9 semantic changes (stale-DOM event
  dispatch enforced, stricter FindComponent), all passed under bunit 1.10.14.
  NOT product regressions. Tasks: T-005 (adapt tests) / T-004 (bunit 2.x).
- csproj pins (src + MediaQuery) were re-applied 2026-09-04 after an external
  revert; re-verify version gate after any branch sync.

## What is believed but UNVERIFIED

- The final post-remediation 1202-test run (skipped at user request) — must
  pass before merge; don't take my word for it
- No memory-leak claims until leak-dotnet gate produces real gcdump evidence
- A11y/RTL/adaptive claims until a11y-bunit greens
