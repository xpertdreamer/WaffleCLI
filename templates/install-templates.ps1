#!/usr/bin/env pwsh

Write-Host "🚀 Installing WaffleCLI Templates..." -ForegroundColor Green

# Install templates from local folders
Write-Host "📦 Installing basic template..." -ForegroundColor Yellow
dotnet new install ./wafflecli-basic

# Verify installation
Write-Host "`n✅ Verification:" -ForegroundColor Green
dotnet new list WaffleCLI

Write-Host "`n🎉 Templates installed successfully!" -ForegroundColor Green
Write-Host "`n📝 Usage examples:" -ForegroundColor Cyan
Write-Host "   dotnet new wafflecli-basic -n MyApp" -ForegroundColor White