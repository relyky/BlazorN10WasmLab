#!/usr/bin/env pwsh
# launch-start.ps1
# Starts Tailwind watch + Aspire watch, writes PIDs, polls readiness.
# Emits a single machine-readable result line on stdout.

param(
    [Parameter(Mandatory)]
    [string]$Url
)

$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path "$PSScriptRoot\..\..\..\..")

$TmpDir       = '.claude\tmp'
$AspirePidF   = "$TmpDir\aspire.pid"
$TailwindPidF = "$TmpDir\tailwind.pid"
$AspireLog    = "$TmpDir\aspire-startup.log"
$TailwindLog  = "$TmpDir\tailwind-watch.log"
$TailwindCli  = '.tools\tailwindcss.exe'

function Get-LivePid([string]$pidFile) {
    if (-not (Test-Path $pidFile)) { return 0 }
    $raw = (Get-Content $pidFile -Raw -ErrorAction SilentlyContinue)
    if (-not $raw) { return 0 }
    $n = 0
    if (-not [int]::TryParse($raw.Trim(), [ref]$n)) { return 0 }
    if (Get-Process -Id $n -ErrorAction SilentlyContinue) { return $n } else { return 0 }
}

function Stop-Tree([int]$procId) {
    if ($procId -gt 0) { & taskkill /T /F /PID $procId *> $null }
}

function Emit-Fail([string]$payload) {
    Write-Output "LAUNCH_FAIL $payload"
    exit 1
}

# --- Four-quadrant PID check
$liveAspire   = Get-LivePid $AspirePidF
$liveTailwind = Get-LivePid $TailwindPidF
if ($liveAspire -gt 0 -or $liveTailwind -gt 0) {
    $a = if ($liveAspire -gt 0)   { $liveAspire }   else { '-' }
    $t = if ($liveTailwind -gt 0) { $liveTailwind } else { '-' }
    Emit-Fail "reason=already_running aspire_pid=$a tailwind_pid=$t"
}
Remove-Item -Force -ErrorAction SilentlyContinue $AspirePidF, $TailwindPidF

if (-not (Test-Path $TailwindCli)) { Emit-Fail 'reason=tailwind_cli_missing' }
New-Item -ItemType Directory -Force -Path $TmpDir | Out-Null
Remove-Item -Force -ErrorAction SilentlyContinue $AspireLog, $TailwindLog

# --- Tailwind watch
$twCmd  = "$TailwindCli -i BlazorN10WasmLab\BlazorN10WasmLab\Styles\app.css -o BlazorN10WasmLab\BlazorN10WasmLab\wwwroot\app.css --watch=always > $TailwindLog 2>&1"
$twProc = Start-Process -FilePath 'cmd.exe' -ArgumentList '/c', $twCmd -PassThru -WindowStyle Hidden
$twProc.Id | Out-File -FilePath $TailwindPidF -Encoding ascii

$deadline = (Get-Date).AddSeconds(30)
$ok = $false
while ((Get-Date) -lt $deadline) {
    if ((Test-Path $TailwindLog) -and
        (Select-String -Path $TailwindLog -Pattern 'Done in' -Quiet -ErrorAction SilentlyContinue)) {
        $ok = $true; break
    }
    Start-Sleep -Seconds 2
}
if (-not $ok) {
    Stop-Tree $twProc.Id
    Remove-Item -Force -ErrorAction SilentlyContinue $TailwindPidF
    Emit-Fail 'reason=tailwind_timeout'
}

# --- Aspire watch
$apCmd  = "dotnet watch --project BlazorN10WasmLab.AppHost --non-interactive run > $AspireLog 2>&1"
$apProc = Start-Process -FilePath 'cmd.exe' -ArgumentList '/c', $apCmd -PassThru -WindowStyle Hidden
$apProc.Id | Out-File -FilePath $AspirePidF -Encoding ascii

$deadline = (Get-Date).AddSeconds(60)
$ok = $false
while ((Get-Date) -lt $deadline) {
    if ((Test-Path $AspireLog) -and
        (Select-String -Path $AspireLog -Pattern 'Distributed application started' -Quiet -ErrorAction SilentlyContinue)) {
        $ok = $true; break
    }
    Start-Sleep -Seconds 3
}
if (-not $ok) {
    Stop-Tree $apProc.Id
    Stop-Tree $twProc.Id
    Remove-Item -Force -ErrorAction SilentlyContinue $AspirePidF, $TailwindPidF
    Emit-Fail 'reason=aspire_timeout'
}

Write-Output "LAUNCH_OK url=$Url aspire_pid=$($apProc.Id) tailwind_pid=$($twProc.Id)"
exit 0
