<#
.SYNOPSIS
    Kills any running instances of the Storingsdienst app, rebuilds, and restarts it.

.DESCRIPTION
    This script:
    1. Finds and kills any processes using port 5266
    2. Rebuilds the solution
    3. Starts the app

.PARAMETER NoBuild
    Skip the build step and just restart the app.

.EXAMPLE
    .\Restart-App.ps1

.EXAMPLE
    .\Restart-App.ps1 -NoBuild
#>

param(
    [switch]$NoBuild
)

$ErrorActionPreference = "Continue"
$ProjectPath = Join-Path $PSScriptRoot "..\src\Storingsdienst\Storingsdienst"

Write-Host "=== Storingsdienst App Restart Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill any existing instances
Write-Host "Step 1: Checking for running instances on port 5266..." -ForegroundColor Yellow

$foundProcesses = $false

# Try Get-NetTCPConnection first (Windows PowerShell / Windows with module)
if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
    $connections = Get-NetTCPConnection -LocalPort 5266 -ErrorAction SilentlyContinue
    if ($connections) {
        $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($processid in $pids) {
            if ($processid -gt 0) {
                $process = Get-Process -Id $processid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "  Killing process: $($process.ProcessName) (PID: $processid)" -ForegroundColor Red
                    Stop-Process -Id $processid -Force -ErrorAction SilentlyContinue
                    $foundProcesses = $true
                }
            }
        }
    }
}
# Fallback: Use netstat (works on Windows with PowerShell Core)
elseif ($IsWindows -or $env:OS -eq "Windows_NT") {
    $netstatOutput = netstat -ano 2>$null | Select-String ":5266\s"
    if ($netstatOutput) {
        foreach ($line in $netstatOutput) {
            # Extract PID from the last column
            if ($line -match '\s+(\d+)\s*$') {
                $processid = [int]$Matches[1]
                if ($processid -gt 0) {
                    $process = Get-Process -Id $processid -ErrorAction SilentlyContinue
                    if ($process) {
                        Write-Host "  Killing process: $($process.ProcessName) (PID: $processid)" -ForegroundColor Red
                        Stop-Process -Id $processid -Force -ErrorAction SilentlyContinue
                        $foundProcesses = $true
                    }
                }
            }
        }
    }
}
# Fallback for Linux/macOS: Use lsof
else {
    $lsofOutput = lsof -i :5266 2>$null | Select-String -NotMatch "^COMMAND"
    if ($lsofOutput) {
        foreach ($line in $lsofOutput) {
            # lsof output: COMMAND PID USER ...
            if ($line -match '^\S+\s+(\d+)') {
                $processid = [int]$Matches[1]
                if ($processid -gt 0) {
                    Write-Host "  Killing process with PID: $processid" -ForegroundColor Red
                    Stop-Process -Id $processid -Force -ErrorAction SilentlyContinue
                    $foundProcesses = $true
                }
            }
        }
    }
}

if ($foundProcesses) {
    # Wait for processes to fully terminate
    Start-Sleep -Seconds 2
    Write-Host "  Processes terminated." -ForegroundColor Green
} else {
    Write-Host "  No running instances found." -ForegroundColor Green
}

Write-Host ""

# Step 2: Rebuild (unless -NoBuild is specified)
if (-not $NoBuild) {
    Write-Host "Step 2: Rebuilding the solution..." -ForegroundColor Yellow
    Push-Location $ProjectPath
    try {
        $buildOutput = dotnet build --no-incremental 2>&1
        $buildExitCode = $LASTEXITCODE

        if ($buildExitCode -ne 0) {
            Write-Host "  Build failed!" -ForegroundColor Red
            $buildOutput | ForEach-Object { Write-Host "  $_" }
            Pop-Location
            exit 1
        }

        # Count warnings and errors
        $warnings = ($buildOutput | Select-String "warning" | Measure-Object).Count
        $errors = ($buildOutput | Select-String "error" | Measure-Object).Count

        Write-Host "  Build succeeded ($warnings warnings, $errors errors)" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "Step 2: Skipping build (-NoBuild specified)" -ForegroundColor Yellow
}

Write-Host ""

# Step 3: Start the app
Write-Host "Step 3: Starting the app..." -ForegroundColor Yellow
Push-Location $ProjectPath
try {
    Write-Host "  Starting on http://localhost:5266" -ForegroundColor Cyan
    Write-Host "  Press Ctrl+C to stop the app" -ForegroundColor Gray
    Write-Host ""

    dotnet run
}
finally {
    Pop-Location
}
