function Get-DotEnvValue {
    param(
        [string]$Name,
        [string]$Path
    )

    foreach ($line in Get-Content $Path) {
        if ($line -match '^\s*#' -or $line -match '^\s*$') {
            continue
        }

        if ($line -match "^\s*$([regex]::Escape($Name))\s*=\s*(.+?)\s*$") {
            return $Matches[1]
        }
    }

    throw "Missing $Name in $Path"
}
