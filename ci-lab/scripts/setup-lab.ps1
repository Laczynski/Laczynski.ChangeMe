param(
    [switch]$SkipGitLab,
    [switch]$SkipVm
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot

if (-not $SkipGitLab) {
    & (Join-Path $PSScriptRoot "setup-gitlab.ps1")
}

if (-not $SkipVm) {
    & (Join-Path $PSScriptRoot "setup-vm.ps1")
    & (Join-Path $PSScriptRoot "bootstrap-vm-local.ps1")
}

Write-Host ""
Write-Host "Lab setup finished."
Write-Host "GitLab UI : http://localhost:8929/root/template-ci-lab"
Write-Host "State     : $(Join-Path $labRoot 'keys\gitlab-setup.json')"
