# Build Aganox Identity Server solution.
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$solution = Join-Path $repoRoot 'Aganox.Identity.sln'

dotnet build $solution -c $Configuration
exit $LASTEXITCODE
