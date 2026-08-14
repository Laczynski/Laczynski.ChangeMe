param(
    [Parameter(Mandatory = $true)]
    [string]$RunnerToken
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $labRoot ".env"
. (Join-Path $PSScriptRoot "lib\dotenv.ps1")

$hostname = Get-DotEnvValue -Name "GITLAB_HOSTNAME" -Path $envFile
$httpPort = Get-DotEnvValue -Name "GITLAB_HTTP_PORT" -Path $envFile
$gitlabUrl = "http://${hostname}:${httpPort}"

Push-Location $labRoot
try {
    docker compose run --rm gitlab-runner register `
        --non-interactive `
        --url $gitlabUrl `
        --token $RunnerToken `
        --executor docker `
        --description "template-ci-lab-docker" `
        --docker-image "docker:28.5.1" `
        --docker-privileged `
        --docker-extra-hosts "gitlab.local:host-gateway" `
        --docker-volumes "/cache"

    Write-Host "Runner registered. Restarting runner service..."
    docker compose restart gitlab-runner
    Write-Host "Verify in GitLab: Admin -> CI/CD -> Runners"
}
finally {
    Pop-Location
}
