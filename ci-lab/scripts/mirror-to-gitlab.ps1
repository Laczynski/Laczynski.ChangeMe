param(
    [string]$RemoteName = "ci-lab",
    [string]$DefaultBranch = "main"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $labRoot "..")
$envFile = Join-Path $labRoot ".env"

function Get-DotEnvValue {
    param([string]$Name)
    Get-Content $envFile | ForEach-Object {
        if ($_ -match "^\s*$([regex]::Escape($Name))\s*=\s*(.+?)\s*$") {
            return $Matches[1]
        }
    }
    throw "Missing $Name in $envFile"
}

if (-not (Test-Path $envFile)) {
    throw "Run scripts/bootstrap-gitlab.ps1 first to create ci-lab/.env"
}

$hostname = Get-DotEnvValue "GITLAB_HOSTNAME"
$httpPort = Get-DotEnvValue "GITLAB_HTTP_PORT"
$sshPort = Get-DotEnvValue "GITLAB_SSH_PORT"
$gitlabHttp = "http://${hostname}:${httpPort}"
$gitlabSsh = "ssh://git@${hostname}:${sshPort}"

Push-Location $repoRoot
try {
    if (-not (git remote get-url $RemoteName 2>$null)) {
        Write-Host "Add remote after creating a blank project in GitLab:"
        Write-Host "  git remote add $RemoteName $gitlabSsh/<group>/<project>.git"
        Write-Host "  git push -u $RemoteName $DefaultBranch"
        Write-Host "  git push $RemoteName --tags"
        return
    }

    git push $RemoteName HEAD:$DefaultBranch
    Write-Host "Pushed current branch to $RemoteName ($gitlabHttp)"
}
finally {
    Pop-Location
}
