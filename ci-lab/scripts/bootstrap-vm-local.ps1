param(
    [string]$VmName = "template-ci-lab"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $labRoot "..")
$ansibleRoot = Join-Path $repoRoot "deploy\ansible"
$remoteRoot = "/tmp/ci-lab-ansible"
. (Join-Path $PSScriptRoot "lib\multipass.ps1")

$mp = Get-MultipassExecutable

Invoke-MultipassExec -VmName $VmName -MultipassExecutable $mp -Command "sudo rm -rf $remoteRoot"
& $mp transfer -r $ansibleRoot "${VmName}:${remoteRoot}"

Invoke-MultipassExec -VmName $VmName -MultipassExecutable $mp -Command @"
set -euo pipefail
python3 -m venv /tmp/ansible-venv
/tmp/ansible-venv/bin/pip install --requirement $remoteRoot/requirements.txt
export ANSIBLE_CONFIG='$remoteRoot/ansible.cfg'
/tmp/ansible-venv/bin/ansible-playbook \
  --inventory $remoteRoot/inventory/hosts.yml \
  --limit ci-lab \
  --connection local \
  $remoteRoot/playbooks/bootstrap.yml
"@

Write-Host "Bootstrap completed on $VmName."
