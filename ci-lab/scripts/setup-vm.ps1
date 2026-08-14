param(
    [string]$VmName = "template-ci-lab",
    [string]$ApplicationName = "change-me"
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = Resolve-Path (Join-Path $labRoot "..")
$inventoryExample = Join-Path $labRoot "inventory\host_vars\ci-lab.yml.example"
$inventoryTarget = Join-Path $repoRoot "deploy\ansible\inventory\host_vars\ci-lab.yml"
$hostsFile = Join-Path $repoRoot "deploy\ansible\inventory\hosts.yml"
. (Join-Path $PSScriptRoot "lib\multipass.ps1")

Ensure-MultipassBackend

& (Join-Path $PSScriptRoot "new-vm.ps1") -VmName $VmName
& (Join-Path $PSScriptRoot "prepare-vm.ps1") -VmName $VmName -ApplicationName $ApplicationName
& (Join-Path $PSScriptRoot "new-deployment-key.ps1")

$mp = Get-MultipassExecutable
$ipv4 = Get-MultipassVmIpv4 -VmName $VmName -MultipassExecutable $mp
$publicKey = (Get-Content (Join-Path $labRoot "keys\deployment.pub") -Raw).Trim()

$knownHostsPath = Join-Path $labRoot "keys\known_hosts"
"# NAT VM; GitLab deploy jobs cannot reach this address from Docker on Windows." | Set-Content -Path $knownHostsPath -Encoding ascii
"127.0.0.1 ssh-ed25519 placeholder" | Add-Content -Path $knownHostsPath -Encoding ascii

$inventory = Get-Content $inventoryExample -Raw
$inventory = $inventory -replace 'ansible_host:.*', "ansible_host: $ipv4"
$inventory = $inventory -replace 'ansible_connection:.*\r?\n', ""
$inventory = $inventory -replace 'public_url:.*', "public_url: http://$ipv4"
$inventory = $inventory -replace 'server_name:.*', "server_name: $ipv4"
$inventory = $inventory -replace 'AuthOptions__FrontendBaseUrl:.*', "AuthOptions__FrontendBaseUrl: http://$ipv4"
$inventory = $inventory -replace 'ssh-ed25519 AAAA\.\.\.replace-with-public-key-from-ci-lab-keys-deployment\.pub', $publicKey
$inventory = $inventory -replace 'deployment_enabled: false', 'deployment_enabled: true'
Set-Content -Path $inventoryTarget -Value $inventory -Encoding UTF8

$hostsContent = Get-Content $hostsFile -Raw
if ($hostsContent -notmatch '(?m)^\s*ci-lab:\s*$') {
    $hostsContent = $hostsContent -replace '(?m)^(\s*production:\s*)$', "`$1`n        ci-lab:"
    Set-Content -Path $hostsFile -Value $hostsContent.TrimEnd() -Encoding UTF8
}

Write-Host ""
Write-Host "VM IPv4         : $ipv4"
Write-Host "Inventory file  : $inventoryTarget"
Write-Host ""
Write-Host "Next: scripts/bootstrap-vm-local.ps1"
