#!/usr/bin/env pwsh
# launch-stop.ps1
# Stops Tailwind watch + Aspire watch by PID files, cleans up.
# Emits a single machine-readable result line on stdout.

$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path "$PSScriptRoot\..\..\..\..")

$TmpDir       = '.claude\tmp'
$AspirePidF   = "$TmpDir\aspire.pid"
$TailwindPidF = "$TmpDir\tailwind.pid"

function Read-PidFile([string]$pidFile) {
    if (-not (Test-Path $pidFile)) { return 0 }
    $raw = (Get-Content $pidFile -Raw -ErrorAction SilentlyContinue)
    if (-not $raw) { return 0 }
    $n = 0
    if (-not [int]::TryParse($raw.Trim(), [ref]$n)) { return 0 }
    return $n
}

function Resolve-State([int]$procId) {
    if ($procId -le 0) { return 'absent' }
    if (Get-Process -Id $procId -ErrorAction SilentlyContinue) {
        & taskkill /T /F /PID $procId *> $null
        return 'killed'
    }
    return 'stale_cleaned'
}

$aspireExists   = Test-Path $AspirePidF
$tailwindExists = Test-Path $TailwindPidF
if (-not $aspireExists -and -not $tailwindExists) {
    Write-Output 'STOP_FAIL reason=no_session'
    exit 1
}

$aState = Resolve-State (Read-PidFile $AspirePidF)
$tState = Resolve-State (Read-PidFile $TailwindPidF)

Remove-Item -Force -ErrorAction SilentlyContinue $AspirePidF, $TailwindPidF
Write-Output "STOP_OK aspire=$aState tailwind=$tState"
exit 0
