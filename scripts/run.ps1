# Start Aganox Identity Server (OpenIddict).
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repoRoot 'src\Aganox.IdentityServer\Aganox.IdentityServer.csproj'

Push-Location (Split-Path $project -Parent)
try {
    dotnet run --launch-profile https
}
finally {
    Pop-Location
}
