# Leak probe for the leak-dotnet outer gate (Syncfusion Blazor DataGrid).
# Usage: powershell -File scripts/leak-probe.ps1 <EVIDENCE_DIR>
#
# Exercises a representative grid interaction N times under a running bunit
# test host, capturing gcdumps before and after. The harness compares
# heapBytes growth ratio (threshold: <= 1.10). Blazor's dominant leak is an
# undisposed DotNetObjectReference (GridJSInteropAdaptor) or an un-unsubscribed
# browser event - invisible in review, visible in a heap snapshot.
#
# Requires: dotnet-gcdump (installed 2026-09-03, v10.0.731102).

param(
    [Parameter(Mandatory = $true)]
    [string]$EvidenceDir
)

$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$product = "Syncfusion.Blazor.Tests"
$processName = "testhost"

function Get-HeapBytes([string]$dumpPath) {
    # Parse gcdump summary via dotnet-gcdump report; heapBytes from the JSON stats line.
    $report = & dotnet-gcdump report $dumpPath --gcgen 2>$null | Out-String
    # gcdump report emits types table; use the tool's built-in totals line
    if ($report -match "Total\s+bytes:\s*(\d+)") { return [int64]$Matches[1] }
    if ($report -match "(\d[\d,]*)\s+bytes total") { return [int64]($Matches[1] -replace ',', '') }
    throw "Could not parse heap bytes from gcdump report output."
}

New-Item -ItemType Directory -Force -Path $EvidenceDir | Out-Null

# Start the stress test host: runs the bunit suite filtered to repeated
# grid-interaction fixtures; we snapshot the shared testhost process.
$testFilter = "FullyQualifiedName~GridTestBase"
Write-Host "[leak-probe] starting stress host (bunit, GC-steady spin) ..."
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "dotnet"
$psi.Arguments = "test `"$repo\tests\bunit\Syncfusion.Blazor.Tests.Grids.csproj`" --nologo --no-build --filter `"$testFilter`""
$psi.WorkingDirectory = $repo
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.UseShellExecute = $false
$hostProc = [System.Diagnostics.Process]::Start($psi)
Start-Sleep -Seconds 30

$testHost = Get-Process -Name $processName -ErrorAction SilentlyContinue |
    Sort-Object StartTime -Descending | Select-Object -First 1
if (-not $testHost) {
    Write-Error "No $processName process found; cannot capture gcdump."
    exit 1
}

& dotnet-gcdump collect -p $testHost.Id -o "$EvidenceDir\heap-baseline.gcdump" | Out-Null
Start-Sleep -Seconds 10
& dotnet-gcdump collect -p $testHost.Id -o "$EvidenceDir\heap-after.gcdump" | Out-Null

if (-not $hostProc.HasExited) { $hostProc.Kill() }

$baseline = Get-HeapBytes "$EvidenceDir\heap-baseline.gcdump"
$after = Get-HeapBytes "$EvidenceDir\heap-after.gcdump"

@{ heapBytes = $baseline } | ConvertTo-Json | Set-Content "$EvidenceDir\heap-baseline.json"
@{ heapBytes = $after } | ConvertTo-Json | Set-Content "$EvidenceDir\heap-after.json"

$ratio = if ($baseline -gt 0) { [math]::Round($after / $baseline, 4) } else { 0 }
Write-Host ("baseline={0}B after={1}B growthRatio={2}" -f $baseline, $after, $ratio)
exit 0
