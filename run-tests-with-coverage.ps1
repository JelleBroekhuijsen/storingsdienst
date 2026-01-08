# PowerShell script to run tests with code coverage
# Run this script from the solution root directory

param(
    [int]$CoverageThreshold = 71,  # Target: 80% (currently 71.73% - increase when GraphService is completed)
    [switch]$OpenReport
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Running Tests with Code Coverage" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous test results
if (Test-Path "./TestResults") {
    Write-Host "Cleaning previous test results..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "./TestResults"
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore storingsdienst.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore dependencies"
    exit 1
}

# Run tests with coverage
Write-Host ""
Write-Host "Running tests with code coverage..." -ForegroundColor Yellow
dotnet test storingsdienst.sln `
    --configuration Release `
    --no-restore `
    --logger "trx;LogFileName=test-results.trx" `
    --results-directory ./TestResults `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput=./TestResults/ `
    /p:Threshold=$CoverageThreshold `
    /p:ThresholdType=line

$testExitCode = $LASTEXITCODE

# Generate coverage report
Write-Host ""
Write-Host "Generating coverage report..." -ForegroundColor Yellow

# Install ReportGenerator if not already installed
$reportGenPath = (Get-Command reportgenerator -ErrorAction SilentlyContinue)
if (-not $reportGenPath) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

reportgenerator `
    "-reports:./TestResults/**/coverage.cobertura.xml" `
    "-targetdir:./TestResults/CoverageReport" `
    "-reporttypes:Html;MarkdownSummary;Cobertura;TextSummary"

# Display summary
Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Read and display coverage summary
$summaryPath = "./TestResults/CoverageReport/Summary.txt"
if (Test-Path $summaryPath) {
    Get-Content $summaryPath | Write-Host
} else {
    Write-Warning "Coverage summary not found at $summaryPath"
}

Write-Host ""
Write-Host "Full report available at: ./TestResults/CoverageReport/index.html" -ForegroundColor Green

# Open report in browser if requested
if ($OpenReport) {
    $reportPath = Resolve-Path "./TestResults/CoverageReport/index.html"
    Start-Process $reportPath
}

Write-Host ""
if ($testExitCode -eq 0) {
    Write-Host "All tests passed! :)" -ForegroundColor Green
} else {
    Write-Host "Some tests failed or coverage threshold not met." -ForegroundColor Red
    Write-Host "Coverage threshold: $CoverageThreshold%" -ForegroundColor Yellow
}

exit $testExitCode
