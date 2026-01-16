<#
.SYNOPSIS
    Kills any running instances of the Storingsdienst app, rebuilds, and restarts it.

.DESCRIPTION
    This script:
    1. Finds and kills any processes using ports 5265 and 5266
    2. Rebuilds the solution
    3. Starts the app
    4. Optionally runs end-to-end tests

.PARAMETER NoBuild
    Skip the build step and just restart the app.

.PARAMETER RunTests
    Run end-to-end tests after starting the app.

.PARAMETER TestFilter
    Filter for which tests to run (e.g., "FullyQualifiedName~DebugPageContent"). Only used with -RunTests.

.EXAMPLE
    .\Restart-App.ps1

.EXAMPLE
    .\Restart-App.ps1 -NoBuild

.EXAMPLE
    .\Restart-App.ps1 -RunTests

.EXAMPLE
    .\Restart-App.ps1 -RunTests -TestFilter "FullyQualifiedName~LanguageSelector"
#>

param(
    [switch]$NoBuild,
    [switch]$RunTests,
    [string]$TestFilter
)

$ErrorActionPreference = "Continue"
$ProjectPath = Join-Path $PSScriptRoot "..\src\Storingsdienst\Storingsdienst"

Write-Host "=== Storingsdienst App Restart Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill any existing instances
Write-Host "Step 1: Checking for running instances on ports 5265 and 5266..." -ForegroundColor Yellow

function Stop-ProcessesOnPort {
    param (
        [int]$Port
    )
    
    $foundProcesses = $false
    
    # Try Get-NetTCPConnection first (Windows PowerShell / Windows with module)
    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        if ($connections) {
            $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
            foreach ($processid in $pids) {
                if ($processid -gt 0) {
                    $process = Get-Process -Id $processid -ErrorAction SilentlyContinue
                    if ($process) {
                        Write-Host "  Killing process on port ${Port}: $($process.ProcessName) (PID: $processid)" -ForegroundColor Red
                        Stop-Process -Id $processid -Force -ErrorAction SilentlyContinue
                        $foundProcesses = $true
                    }
                }
            }
        }
    }
    # Fallback: Use netstat (works on Windows with PowerShell Core)
    elseif ($IsWindows -or $env:OS -eq "Windows_NT") {
        $netstatOutput = netstat -ano 2>$null | Select-String ":${Port}\s"
        if ($netstatOutput) {
            foreach ($line in $netstatOutput) {
                # Extract PID from the last column
                if ($line -match '\s+(\d+)\s*$') {
                    $processid = [int]$Matches[1]
                    if ($processid -gt 0) {
                        $process = Get-Process -Id $processid -ErrorAction SilentlyContinue
                        if ($process) {
                            Write-Host "  Killing process on port ${Port}: $($process.ProcessName) (PID: $processid)" -ForegroundColor Red
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
        $lsofOutput = lsof -i :${Port} 2>$null | Select-String -NotMatch "^COMMAND"
        if ($lsofOutput) {
            foreach ($line in $lsofOutput) {
                # lsof output: COMMAND PID USER ...
                if ($line -match '^\S+\s+(\d+)') {
                    $processid = [int]$Matches[1]
                    if ($processid -gt 0) {
                        Write-Host "  Killing process on port ${Port} with PID: $processid" -ForegroundColor Red
                        Stop-Process -Id $processid -Force -ErrorAction SilentlyContinue
                        $foundProcesses = $true
                    }
                }
            }
        }
    }
    
    return $foundProcesses
}

# Check both HTTP (5265) and HTTPS (5266) ports
$foundProcesses = $false
$foundProcesses = (Stop-ProcessesOnPort -Port 5265) -or $foundProcesses
$foundProcesses = (Stop-ProcessesOnPort -Port 5266) -or $foundProcesses

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

if ($RunTests) {
    # Start app in background for testing
    Push-Location $ProjectPath
    try {
        Write-Host "  Starting on http://localhost:5265 and https://localhost:5266 (background)" -ForegroundColor Cyan
        
        $job = Start-Job -ScriptBlock {
            param($path)
            Set-Location $path
            dotnet run
        } -ArgumentList $ProjectPath
        
        Write-Host "  Waiting for app to start..." -ForegroundColor Gray
        Start-Sleep -Seconds 5
        
        Write-Host "  App started in background (Job ID: $($job.Id))" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    Write-Host ""
    Write-Host "Step 4: Running end-to-end tests..." -ForegroundColor Yellow
    
    $testPath = Join-Path $PSScriptRoot "..\tests\Storingsdienst.E2E.Tests"
    Push-Location $testPath
    try {
        if ($TestFilter) {
            Write-Host "  Test filter: $TestFilter" -ForegroundColor Gray
            dotnet test --settings playwright.runsettings --filter $TestFilter --logger "console;verbosity=detailed"
        } else {
            dotnet test --settings playwright.runsettings --logger "console;verbosity=detailed"
        }
        $testExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
    
    Write-Host ""
    Write-Host "Step 5: Stopping background app..." -ForegroundColor Yellow
    Stop-Job -Job $job -ErrorAction SilentlyContinue
    Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    
    # Clean up any remaining processes
    Stop-ProcessesOnPort -Port 5265 | Out-Null
    Stop-ProcessesOnPort -Port 5266 | Out-Null
    
    Write-Host "  Background app stopped." -ForegroundColor Green
    Write-Host ""
    
    if ($testExitCode -eq 0) {
        Write-Host "=== Tests Passed ===" -ForegroundColor Green
    } else {
        Write-Host "=== Tests Failed ===" -ForegroundColor Red
        exit $testExitCode
    }
} else {
    # Start app in foreground (normal mode)
    Push-Location $ProjectPath
    try {
        Write-Host "  Starting on http://localhost:5265 and https://localhost:5266" -ForegroundColor Cyan
        Write-Host "  Press Ctrl+C to stop the app" -ForegroundColor Gray
        Write-Host ""

        dotnet run
    }
    finally {
        Pop-Location
    }
}
