param(
    [string]$VmName = "template-ci-lab",
    [int]$Cpus = 2,
    [string]$Memory = "4G",
    [string]$Disk = "25G",
    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = "Stop"
$labRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot "lib\multipass.ps1")

Ensure-MultipassBackend
$mp = Get-MultipassExecutable

$existing = & $mp list --format json | ConvertFrom-Json
if ($existing.list | Where-Object { $_.name -eq $VmName }) {
    Write-Host "VM '$VmName' already exists."
}
else {
    & $mp launch 24.04 `
        --name $VmName `
        --cpus $Cpus `
        --memory $Memory `
        --disk $Disk `
        --timeout $TimeoutSeconds
    Write-Host "Created VM '$VmName'."
}

$ipv4 = Get-MultipassVmIpv4 -VmName $VmName -MultipassExecutable $mp

Write-Host ""
Write-Host "VM name : $VmName"
Write-Host "IPv4    : $ipv4 (NAT on VirtualBox; host SSH may be unavailable)"
Write-Host "Shell   : multipass shell $VmName"
Write-Host ""
Write-Host "Next: scripts/prepare-vm.ps1"
