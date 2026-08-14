param(
    [switch]$SkipHostsEntry
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $labRoot ".env"
$envExample = Join-Path $labRoot ".env.example"

if (-not (Test-Path $envFile)) {
    Copy-Item $envExample $envFile
    Write-Host "Created $envFile from .env.example"
}

function Get-DotEnvValue {
    param([string]$Name)
    Get-Content $envFile | ForEach-Object {
        if ($_ -match "^\s*$([regex]::Escape($Name))\s*=\s*(.+?)\s*$") {
            return $Matches[1]
        }
    }
    throw "Missing $Name in $envFile"
}

$hostname = Get-DotEnvValue "GITLAB_HOSTNAME"
$httpPort = Get-DotEnvValue "GITLAB_HTTP_PORT"

if (-not $SkipHostsEntry) {
    $hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
    $entry = "127.0.0.1`t$hostname"
    $hostsContent = Get-Content $hostsPath -Raw
    if ($hostsContent -notmatch [regex]::Escape($hostname)) {
        Write-Host "Adding hosts entry: $entry"
        Write-Host "Administrator approval may be required."
        Add-Content -Path $hostsPath -Value $entry
    }
}

New-Item -ItemType Directory -Force -Path (Join-Path $labRoot "data\gitlab\config") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $labRoot "data\gitlab\logs") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $labRoot "data\gitlab\data") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $labRoot "runner-config") | Out-Null

Push-Location $labRoot
try {
    docker compose up -d
    Write-Host "Waiting for GitLab to become healthy (first start can take several minutes)..."
    docker compose ps
    Write-Host ""
    Write-Host "When healthy, open: http://${hostname}:${httpPort}"
    Write-Host "Initial root password:"
    docker compose exec -T gitlab grep 'Password:' /etc/gitlab/initial_root_password 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Initial password file not ready yet. Retry:"
        Write-Host "  docker compose exec gitlab grep 'Password:' /etc/gitlab/initial_root_password"
    }
    Write-Host ""
    Write-Host "Next: register a runner with scripts/register-runner.ps1"
}
finally {
    Pop-Location
}
