param(
    [string]$VmName = "template-ci-lab",
    [string]$ApplicationName = "change-me"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $labRoot "..")
$secretsFile = Join-Path $labRoot "vm\lab-secrets.env"
. (Join-Path $PSScriptRoot "lib\multipass.ps1")

$mp = Get-MultipassExecutable
$secretsPath = "/etc/$ApplicationName/secrets.env"
$tempSecrets = Join-Path $env:TEMP "template-ci-lab-secrets.env"
Copy-Item $secretsFile $tempSecrets -Force

& $mp transfer $tempSecrets "${VmName}:/tmp/template-ci-lab-secrets.env"

Invoke-MultipassExec -VmName $VmName -MultipassExecutable $mp -Command @"
set -euo pipefail
sudo apt-get update
sudo DEBIAN_FRONTEND=noninteractive apt-get install -y curl openssh-server postgresql postgresql-contrib python3 python3-pip python3-venv
sudo systemctl enable --now postgresql
sudo systemctl enable --now ssh
sudo -u postgres psql -tc \"SELECT 1 FROM pg_roles WHERE rolname = 'template'\" | grep -q 1 || sudo -u postgres psql -c \"CREATE USER template WITH PASSWORD 'template-ci-lab-password';\"
sudo -u postgres psql -tc \"SELECT 1 FROM pg_database WHERE datname = 'template'\" | grep -q 1 || sudo -u postgres createdb -O template template
sudo install -d -o root -g root -m 0755 /etc/$ApplicationName
sudo install -o root -g root -m 0600 /tmp/template-ci-lab-secrets.env $secretsPath
sudo rm -f /tmp/template-ci-lab-secrets.env
"@

Remove-Item $tempSecrets -Force
Write-Host "Installed PostgreSQL and $secretsPath on $VmName."
