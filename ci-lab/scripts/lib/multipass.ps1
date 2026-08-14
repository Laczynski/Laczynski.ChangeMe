function Get-MultipassExecutable {
    $command = Get-Command multipass -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $defaultPath = "C:\Program Files\Multipass\bin\multipass.exe"
    if (Test-Path $defaultPath) {
        return $defaultPath
    }

    throw "Multipass is required. Install from https://multipass.run/install"
}

function Ensure-MultipassBackend {
  param(
    [string]$MultipassExecutable = (Get-MultipassExecutable)
  )

  & $MultipassExecutable set local.driver=virtualbox | Out-Null

  if (-not (Get-Command VBoxManage -ErrorAction SilentlyContinue)) {
    Write-Host "Installing Oracle VirtualBox (required on Windows Home)..."
    winget install --id Oracle.VirtualBox --accept-package-agreements --accept-source-agreements
  }
}

function Get-MultipassVmIpv4 {
  param(
    [string]$VmName,
    [string]$MultipassExecutable = (Get-MultipassExecutable)
  )

  $info = & $MultipassExecutable info $VmName --format json | ConvertFrom-Json
  $ipv4 = $info.info.$VmName.ipv4 | Where-Object { $_ -and $_ -ne "N/A" } | Select-Object -First 1
  if ($ipv4) {
    return $ipv4
  }

  $rawOutput = (& $MultipassExecutable exec $VmName -- hostname -I 2>$null) -join " "
  $cleanOutput = [regex]::Replace($rawOutput, '\x1B\[[0-9;]*[A-Za-z]', '')
  $internalIp = ($cleanOutput.Trim().Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries) | Where-Object { $_ -match '^\d+\.\d+\.\d+\.\d+$' } | Select-Object -First 1)
  if ($internalIp) {
    return $internalIp
  }

  throw "Could not read IPv4 for VM '$VmName'."
}

function Invoke-MultipassExec {
  param(
    [string]$VmName,
    [string]$Command,
    [string]$MultipassExecutable = (Get-MultipassExecutable)
  )

  & $MultipassExecutable exec $VmName -- bash -lc $Command
  if ($LASTEXITCODE -ne 0) {
    throw "Multipass exec failed on '$VmName'."
  }
}
