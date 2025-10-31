#!/usr/bin/env pwsh

Write-Host "🗑️ Uninstalling WaffleCLI Templates..." -ForegroundColor Yellow

dotnet new uninstall ./wafflecli-basic

Write-Host "✅ Templates uninstalled!" -ForegroundColor Green