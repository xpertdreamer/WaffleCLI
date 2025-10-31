#!/usr/bin/env pwsh

# Colors for output
$ErrorActionPreference = "Stop"

Write-Host "🚀 Installing WaffleCLI Templates..." -ForegroundColor Green

# Check if dotnet is available
try {
    $null = Get-Command dotnet -ErrorAction Stop
} catch {
    Write-Host "❌ .NET SDK is not installed. Please install .NET 8.0 or later." -ForegroundColor Red
    exit 1
}

Write-Host "📦 Installing basic template..." -ForegroundColor Yellow
dotnet new install ./wafflecli-basic

# Verify installation
Write-Host "`n✅ Verification:" -ForegroundColor Green
dotnet new list WaffleCLI

Write-Host "`n🎉 Templates installed successfully!" -ForegroundColor Green
Write-Host "`n📝 Usage examples:" -ForegroundColor Cyan
Write-Host "   dotnet new wafflecli-basic -n MyApp" -ForegroundColor White