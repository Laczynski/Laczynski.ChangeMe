param(
    [string]$KeyName = "deployment"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$keysDir = Join-Path $labRoot "keys"
New-Item -ItemType Directory -Force -Path $keysDir | Out-Null

$privateKey = Join-Path $keysDir "$KeyName"
$publicKey = "$privateKey.pub"

if (Test-Path $privateKey) {
    Write-Host "Key already exists: $privateKey"
}
else {
    if (Get-Command ssh-keygen -ErrorAction SilentlyContinue) {
        & ssh-keygen -t ed25519 -f $privateKey -N '""' -C "template-ci-lab-$KeyName"
    }
    else {
        throw "ssh-keygen not found. Install OpenSSH client or generate keys manually in $keysDir"
    }
}

$publicKeyContent = Get-Content $publicKey -Raw
Write-Host ""
Write-Host "Private key (GitLab File variable DEPLOY_SSH_PRIVATE_KEY):"
Write-Host "  $privateKey"
Write-Host ""
Write-Host "Public key (deployment_public_keys in host_vars/ci-lab.yml):"
Write-Host $publicKeyContent.Trim()
Write-Host ""
Write-Host "Known hosts for GitLab File variable DEPLOY_KNOWN_HOSTS:"
Write-Host "  Run after VM is ready: ssh-keyscan -p 22 <vm-ip> > ci-lab/keys/known_hosts"
