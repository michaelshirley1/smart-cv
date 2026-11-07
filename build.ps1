param(
    [ValidateSet('win-x64', 'win-arm64', 'osx-x64', 'osx-arm64', 'linux-x64', 'linux-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$Version = "$((Get-Content (Join-Path $PSScriptRoot 'VERSION') -Raw).Trim()).0"
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

npm ci
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

npm run build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet publish server/SmartCv.csproj -c Release -r $Runtime --self-contained -p:PublishSingleFile=true -p:Version=$Version -o "publish/$Runtime"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Built publish/$Runtime ($Version)"
