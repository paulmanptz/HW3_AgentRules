param(
    [string]$ConnectionString = $env:DEFAULT_CONNECTION,
    [string]$HostName = $env:PGHOST,
    [string]$Port = $env:PGPORT,
    [string]$Database = $env:PGDATABASE,
    [string]$Username = $env:PGUSER,
    [string]$Password = $env:PGPASSWORD,
    [string]$PsqlPath = "psql"
)

$ErrorActionPreference = "Stop"

$sqlDir = Join-Path $PSScriptRoot "sql"
$sqlScripts = @(
    "01-initial-auth.sql",
    "02-initial-app.sql",
    "03-initial-files.sql"
)

function Read-NpgsqlConnectionString {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $result = @{}

    foreach ($part in $Value -split ";") {
        if ([string]::IsNullOrWhiteSpace($part)) {
            continue
        }

        $keyValue = $part -split "=", 2
        if ($keyValue.Count -ne 2) {
            continue
        }

        $key = $keyValue[0].Trim().ToLowerInvariant()
        $result[$key] = $keyValue[1].Trim()
    }

    return $result
}

function Set-ConnectionValue {
    param(
        [hashtable]$Values,
        [string[]]$Keys,
        [ref]$Target
    )

    if (-not [string]::IsNullOrWhiteSpace($Target.Value)) {
        return
    }

    foreach ($key in $Keys) {
        if ($Values.ContainsKey($key) -and -not [string]::IsNullOrWhiteSpace($Values[$key])) {
            $Target.Value = $Values[$key]
            return
        }
    }
}

$useRawConnectionString = $false

if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
    if ($ConnectionString -match "^\s*(postgres|postgresql)://") {
        $useRawConnectionString = $true
    }
    else {
        $connectionValues = Read-NpgsqlConnectionString -Value $ConnectionString

        Set-ConnectionValue -Values $connectionValues -Keys @("host", "server") -Target ([ref]$HostName)
        Set-ConnectionValue -Values $connectionValues -Keys @("port") -Target ([ref]$Port)
        Set-ConnectionValue -Values $connectionValues -Keys @("database", "db") -Target ([ref]$Database)
        Set-ConnectionValue -Values $connectionValues -Keys @("username", "user id", "userid", "uid", "user") -Target ([ref]$Username)
        Set-ConnectionValue -Values $connectionValues -Keys @("password", "pwd") -Target ([ref]$Password)
    }
}

if (-not $useRawConnectionString) {
    if ([string]::IsNullOrWhiteSpace($Port)) {
        $Port = "5432"
    }

    $missing = @()
    if ([string]::IsNullOrWhiteSpace($HostName)) { $missing += "HostName" }
    if ([string]::IsNullOrWhiteSpace($Database)) { $missing += "Database" }
    if ([string]::IsNullOrWhiteSpace($Username)) { $missing += "Username" }

    if ($missing.Count -gt 0) {
        throw "Missing connection settings: $($missing -join ', '). Pass -ConnectionString or explicit -HostName/-Database/-Username parameters."
    }
}

foreach ($scriptName in $sqlScripts) {
    $scriptPath = Join-Path $sqlDir $scriptName
    if (-not (Test-Path $scriptPath)) {
        throw "SQL script not found: $scriptPath"
    }
}

$previousPgPassword = $env:PGPASSWORD

try {
    if (-not [string]::IsNullOrWhiteSpace($Password)) {
        $env:PGPASSWORD = $Password
    }

    foreach ($scriptName in $sqlScripts) {
        $scriptPath = Join-Path $sqlDir $scriptName
        Write-Host "Applying $scriptName..."

        if ($useRawConnectionString) {
            & $PsqlPath $ConnectionString -v "ON_ERROR_STOP=1" -f $scriptPath
        }
        else {
            & $PsqlPath -h $HostName -p $Port -d $Database -U $Username -v "ON_ERROR_STOP=1" -f $scriptPath
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to apply $scriptName. psql exit code: $LASTEXITCODE."
        }
    }

    Write-Host "All SQL migration scripts applied successfully."
}
finally {
    $env:PGPASSWORD = $previousPgPassword
}
