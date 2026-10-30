# DECISIONS-REQUIRED — parking lot for human decisions

The autonomy boundary (`.agent/policies/autonomy-boundary.md`) requires the agent
to PARK decisions here instead of asking mid-flow or deciding silently.
Date each entry. Resolved entries move to a "Resolved" section with the approver.

## Pending

| Date | Task | Decision needed | Why the agent cannot decide |
|---|---|---|---|
| 2026-09-03 | harness bootstrap | Approve task 1: switch `tests/bunit/Syncfusion.Blazor.Tests.Grids.csproj` from package ref `Syncfusion.Blazor.Grid 100.2.1` to a **project reference** to `src/Syncfusion.Blazor.Grid.csproj` (8 build errors today) | `**/*.csproj` is risk-gated |
| 2026-09-03 | harness bootstrap | Approve installing `dotnet tool install -g dotnet-gcdump` to activate the `leak-dotnet` outer gate | global tool install — user decides |
| 2026-09-03 | harness bootstrap | Approve generating the initial `.agent/baselines/PublicAPI.txt` snapshot from current HEAD (task 2). Baseline LOCKS current surface as the never-break line | PublicAPI files risk-gated; a baseline is contractual |
| 2026-09-03 | harness bootstrap | Approve `gitleaks` install to activate the `secrets` outer gate (`scoop install gitleaks` or GitHub release binary) | tool install — user decides |
| 2026-09-03 | gate finding | `syncfusion-version-consistency` gate fails: `Syncfusion.PdfExport.Net.Core`, `Syncfusion.ExcelExport.Net.Core`, `Syncfusion.Telemetry` use `Version="*"` in `src/Syncfusion.Blazor.Grid.csproj` (lines 61-63). Pinning them (e.g. `33.1.44`-family) is a `*.csproj` risk-path change | risk-gated; also a deliberate floating-version policy the team may have chosen |
| 2026-09-03 | gate finding | `vulnerable-packages` gate fails on test project: 4 transitive advisories (Newtonsoft.Json 9.0.1 HIGH, System.Net.Http 4.3.0 HIGH, System.Text.RegularExpressions 4.3.0 HIGH, AngleSharp 0.17.1 MODERATE) — test-only, not shipped in the package, but still gate-red. Remediation (transitive pin/upgrade) touches `*.csproj` | risk-gated; test-project-only exposure |
| 2026-09-03 | gate finding | `bunit-tests` gate fails on 1 test: CR Issues BLAZ-1015397 "SortComparer must not take more than 2000ms for 10k records" (1 m 17 s actual). Either a real perf regression in the sort path or a stale environment-sensitive threshold — needs a perf investigation task | a threshold relaxation would be gate-weakening: exactly what the harness forbids without human sign-off |

## Resolved

| Date | Decision | Approver | Outcome |
|---|---|---|---|
| 2026-09-03 | Generate the L1 harness at .agent/ with the gate set from the PROPOSE report | Rajendran (interview: "Yes, proceed") | Harness generated |
| 2026-09-03 | Mutation testing (Stryker.NET) inclusion | Rajendran | Declined — recorded as deliberately-not-proposed gate |
| 2026-09-03 | TS compile+lint gates on scripts/ | Rajendran | Approved |
| 2026-09-03 | Pin 3 floating Syncfusion versions + pin MediaQuery meta 33.1.44 + refine version gate to two-release-train model | Rajendran | DONE: PdfExport 100.2.1514, ExcelExport 100.2.1.23, Telemetry 100.2.58; gate green (13 refs, 0 floating) |
| 2026-09-03 | Remediate test-project CVEs | Rajendran | DONE: bunit 1.23.9, Test.Sdk 17.14.1, xunit 2.9.3, runner 2.8.2, MECM 8.0.1, System.Text.Json 8.0.5 — all HIGHS cleared; AngleSharp 0.17.1 Moderate ACCEPTED (see below) |
| 2026-09-03 | Install dotnet-gcdump + gitleaks | Rajendran | DONE: gcdump 10.0.731102 (leak gate active), gitleaks 8.28.0 (secrets gate green: 0 leaks / 971 commits) |
| 2026-09-03 | AngleSharp CVE-2026-54570 (Moderate) in test project | Evidence-driven | CANNOT remediate: bunit 1.23.9 binary-binds AngleSharp 0.17.x (AngleSharpWrappers). Overriding to patched 1.5.0 broke 358 tests with MissingMethodException (TRX evidence). Test-only exposure (untrusted HTML not parsed from customers in tests); upgrade to bunit 2.x (which targets AngleSharp 1.x) is the real fix — parked below. |
| 2026-09-03 | Sort-perf failure BLAZ_1015397 (77s vs 2000ms budget) | Evidence-driven | VERDICT: **stale/environment-fragile test, NOT a product regression**. Details in PROJECT-STATE.md; fix proposal parked below. |

## Pending (new from remediation work)

| Date | Task | Decision needed | Why the agent cannot decide |
|---|---|---|---|
| 2026-09-03 | bunit 2.x migration (targets AngleSharp 1.x, resolves the accepted Moderate advisory) | Approve a dedicated migration task: bunit 2.9.0 has no `bunit.web.testcomponents` counterpart on the feeds — fixture model changed; 269 razor fixtures need migration review | large behavioral surface; fixture model rewrite; user decides scope/timing |
| 2026-09-03 | BLAZ-1015397 test hardening | Approve rewriting the perf assertion: run sort N times and take the median instead of one wall-clock sample; keep a real budget (e.g. <2000ms median, CI-scaled). Alternative: mark Category=Perf and exclude from the bunit gate, running it as a dedicated perf job | any threshold change to an existing test needs sign-off (gate-weakening guard) |
| 2026-09-03 | Final post-remediation full-suite run | Confirm the 1202-test suite passes with the final matrix (v2 runner, AngleSharp 0.17.1) before merging. Run: clean bin/obj, `dotnet test ... --logger trx` | long (~4-6 min) run was skipped at user request; merge must not rely on my assertion |
