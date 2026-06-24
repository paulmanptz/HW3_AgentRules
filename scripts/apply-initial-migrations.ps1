param(
    [string]$ConnectionString = $env:DEFAULT_CONNECTION
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot "src"

function Invoke-EfDatabaseUpdate {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Project,

        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $arguments = @(
        "ef",
        "database",
        "update",
        "--project",
        $Project,
        "--startup-project",
        "MasterApp.WebApi",
        "--context",
        $Context
    )

    if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
        $arguments += "--connection"
        $arguments += $ConnectionString
    }

    Write-Host "Applying migrations for $Context..."
    & dotnet @arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed for $Context with exit code $LASTEXITCODE."
    }
}

Push-Location $srcRoot
try {
    Invoke-EfDatabaseUpdate -Project "Auth\MasterApp.Auth.Infrastructure" -Context "AuthDbContext"
    Invoke-EfDatabaseUpdate -Project "Master\MasterApp.Infrastructure" -Context "AppDbContext"
    Invoke-EfDatabaseUpdate -Project "Files\MasterApp.Files.Infrastructure" -Context "FileDbContext"

    Write-Host "All migrations applied successfully."
}
finally {
    Pop-Location
}
