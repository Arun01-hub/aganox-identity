# Run Aganox Identity Server tests.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$solution = Join-Path $repoRoot 'Aganox.Identity.sln'

dotnet test $solution --no-build:$false
exit $LASTEXITCODE
