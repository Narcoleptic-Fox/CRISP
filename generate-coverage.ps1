# Script to generate and display code coverage for CRISP project
# Usage: .\generate-coverage.ps1 [-Filter <test filter>] [-OpenReport]
# Example: .\generate-coverage.ps1 -Filter "FullyQualifiedName~FluentValidator" -OpenReport

param(
    [string]$Filter = "",
    [switch]$OpenReport = $false
)

# Configuration
$projectRoot = "$PSScriptRoot"
$testProject = "CRISP.Tests"
$reportFolder = "coverage-report"
$testResultsFolder = "TestResults"

# Ensure we're in the project root directory
Set-Location $projectRoot

# Display script header
Write-Host "============================" -ForegroundColor Cyan
Write-Host "CRISP Code Coverage Generator" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Green
} 
catch {
    Write-Host "Error: .NET SDK is not installed or not found in PATH." -ForegroundColor Red
    exit 1
}

# Check if ReportGenerator is installed
try {
    $reportGenVersion = reportgenerator -h | Select-String "ReportGenerator" | Select-Object -First 1
    if ($reportGenVersion) {
        Write-Host "ReportGenerator found." -ForegroundColor Green
    }
} 
catch {
    Write-Host "ReportGenerator not found. Installing..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error installing ReportGenerator." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "ReportGenerator installed successfully." -ForegroundColor Green
}

# Create the test command based on filter parameter
$testCommand = "dotnet test `"$projectRoot`" --collect:`"XPlat Code Coverage`""
if ($Filter) {
    $testCommand += " --filter `"$Filter`""
    Write-Host "Running tests with filter: $Filter" -ForegroundColor Cyan
} 
else {
    Write-Host "Running all tests" -ForegroundColor Cyan
}

# Run tests with coverage collection
Write-Host "Running tests with coverage collection..." -ForegroundColor Yellow
Invoke-Expression $testCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Some tests failed, but continuing with coverage report generation." -ForegroundColor Yellow
}

# Generate the coverage report
Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow

# Ensure the report directory exists
if (-not (Test-Path $reportFolder)) {
    New-Item -ItemType Directory -Path $reportFolder | Out-Null
}

# Generate the report
$coverageFiles = Resolve-Path "$projectRoot\$testProject\$testResultsFolder\*\coverage.cobertura.xml" -ErrorAction SilentlyContinue
if (-not $coverageFiles) {
    Write-Host "Error: No coverage files found. Make sure tests ran successfully." -ForegroundColor Red
    exit 1
}

reportgenerator -reports:"$projectRoot\$testProject\$testResultsFolder\*\coverage.cobertura.xml" -targetdir:"$reportFolder" -reporttypes:Html

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generating coverage report." -ForegroundColor Red
    exit 1
}

Write-Host "Coverage report generated successfully in the '$reportFolder' folder." -ForegroundColor Green

# Display coverage summary from the report
try {
    $indexHtmlPath = "$projectRoot\$reportFolder\index.html"
    $content = Get-Content $indexHtmlPath -Raw
    
    # Extract coverage percentages
    $lineCoverage = if ($content -match 'Line coverage.*?(\d+\.\d+)%') { $matches[1] } else { "unknown" }
    $branchCoverage = if ($content -match 'Branch coverage.*?(\d+\.\d+)%') { $matches[1] } else { "unknown" }
    
    Write-Host ""
    Write-Host "Coverage Summary:" -ForegroundColor Cyan
    Write-Host "----------------" -ForegroundColor Cyan
    Write-Host "Line Coverage: $lineCoverage%" -ForegroundColor $(if ([double]$lineCoverage -ge 80) { "Green" } elseif ([double]$lineCoverage -ge 60) { "Yellow" } else { "Red" })
    Write-Host "Branch Coverage: $branchCoverage%" -ForegroundColor $(if ([double]$branchCoverage -ge 80) { "Green" } elseif ([double]$branchCoverage -ge 60) { "Yellow" } else { "Red" })
    Write-Host ""
}
catch {
    Write-Host "Could not extract coverage percentages from the report." -ForegroundColor Yellow
}

# Open the report in a browser if requested
if ($OpenReport) {
    Write-Host "Opening coverage report in default browser..." -ForegroundColor Yellow
    Invoke-Item "$projectRoot\$reportFolder\index.html"
}
else {
    Write-Host "To view the coverage report, open: $projectRoot\$reportFolder\index.html" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green