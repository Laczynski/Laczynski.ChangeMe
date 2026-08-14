param(
    [string]$ProjectName = "template-ci-lab",
    [string]$ProjectPath = "root/template-ci-lab"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $labRoot "..")
$envFile = Join-Path $labRoot ".env"
$stateFile = Join-Path $labRoot "keys\gitlab-setup.json"
. (Join-Path $PSScriptRoot "lib\dotenv.ps1")

if (-not (Test-Path $envFile)) {
    throw "Missing $envFile. Run bootstrap-gitlab.ps1 first."
}

$hostname = Get-DotEnvValue -Name "GITLAB_HOSTNAME" -Path $envFile
$httpPort = Get-DotEnvValue -Name "GITLAB_HTTP_PORT" -Path $envFile
$gitlabApi = "http://localhost:${httpPort}/api/v4"
$gitlabWeb = "http://localhost:${httpPort}"
$gitlabRunnerUrl = "http://${hostname}:${httpPort}"
$gitlabHttpRemote = "http://localhost:${httpPort}/${ProjectPath}.git"

New-Item -ItemType Directory -Force -Path (Join-Path $labRoot "keys") | Out-Null

function Invoke-GitLabApi {
    param(
        [string]$Method = "Get",
        [string]$Path,
        [hashtable]$Body
    )
    $headers = @{ "PRIVATE-TOKEN" = $script:PatToken }
    $uri = "$gitlabApi$Path"
    if ($Body) {
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers -Body $Body
    }
    return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers
}

Push-Location $labRoot
try {
    $patScriptPath = Join-Path $env:TEMP "template-ci-lab-pat.rb"
    @'
user = User.find_by(username: 'root')
token_value = 'ci-lab-automation-' + SecureRandom.hex(16)
pat = user.personal_access_tokens.find_by(name: 'ci-lab-automation')
pat&.revoke!
pat = user.personal_access_tokens.create!(
  name: 'ci-lab-automation',
  scopes: [:api, :read_repository, :write_repository],
  expires_at: 30.days.from_now
)
pat.set_token(token_value)
pat.save!
puts token_value
'@ | Set-Content -Path $patScriptPath -Encoding UTF8

    $script:PatToken = (Get-Content $patScriptPath | docker compose exec -T gitlab gitlab-rails runner -).Trim()
    Remove-Item $patScriptPath -Force

    if (-not $script:PatToken) {
        throw "Could not create GitLab personal access token."
    }

    $project = $null
    try {
        $project = Invoke-GitLabApi -Path "/projects/$([uri]::EscapeDataString($ProjectPath))"
    }
    catch {
        $project = Invoke-GitLabApi -Method Post -Path "/projects" -Body @{
            name = $ProjectName
            path = ($ProjectPath -split '/')[1]
            namespace_id = 1
            visibility = "private"
        }
    }

    $existingRunners = ""
    try {
        $existingRunners = docker compose exec -T gitlab-runner cat /etc/gitlab-runner/config.toml 2>$null
    }
    catch {
        $existingRunners = ""
    }

    if ($existingRunners -notmatch "template-ci-lab-docker") {
        $runner = Invoke-GitLabApi -Method Post -Path "/user/runners" -Body @{
            runner_type = "instance_type"
            description = "template-ci-lab-docker"
            tag_list = "docker,privileged"
        }

        $runnerToken = $runner.token
        if (-not $runnerToken) {
            throw "GitLab did not return a runner registration token."
        }

        docker compose run --rm gitlab-runner register `
            --non-interactive `
            --url $gitlabRunnerUrl `
            --token $runnerToken `
            --executor docker `
            --description "template-ci-lab-docker" `
            --docker-image "docker:28.5.1" `
            --docker-privileged `
            --docker-extra-hosts "gitlab.local:host-gateway" `
            --docker-volumes "/cache"
        docker compose restart gitlab-runner
    }
    else {
        $registeredRunners = @(Invoke-GitLabApi -Path "/runners/all?search=template-ci-lab-docker")
        $runner = $registeredRunners | Where-Object { $_.description -eq "template-ci-lab-docker" } | Select-Object -First 1
    }

    $state = @{
        gitlabApi = $gitlabApi
        gitlabWeb = $gitlabWeb
        projectId = $project.id
        projectPath = $project.path_with_namespace
        patToken = $script:PatToken
        runnerId = if ($runner) { $runner.id } else { $null }
    }
    $state | ConvertTo-Json | Set-Content -Path $stateFile -Encoding UTF8

    Push-Location $repoRoot
    try {
        $remoteUrl = "http://oauth2:$($script:PatToken)@localhost:${httpPort}/${ProjectPath}.git"
        $remotes = @(git remote)
        if ($remotes -notcontains "ci-lab") {
            git remote add ci-lab $gitlabHttpRemote
        }
        else {
            git remote set-url ci-lab $gitlabHttpRemote
        }

        $branch = (git rev-parse --abbrev-ref HEAD).Trim()
        git -c "http.extraHeader=PRIVATE-TOKEN: $($script:PatToken)" push $remoteUrl "${branch}:${branch}" --force
    }
    finally {
        Pop-Location
    }

    Write-Host "GitLab project : $gitlabWeb/$($project.path_with_namespace)"
    Write-Host "Runner         : registered (template-ci-lab-docker)"
    Write-Host "Remote         : ci-lab -> $gitlabHttpRemote"
    Write-Host "State file     : $stateFile"
}
finally {
    Pop-Location
}
