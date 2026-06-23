param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

$git = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $git) {
    throw "git was not found in PATH."
}

$oldLocation = Get-Location
Set-Location -LiteralPath $RepositoryRoot
try {
    $paths = git ls-files -z | ForEach-Object { $_ -split "`0" } | Where-Object { $_ }
    $bomFiles = New-Object System.Collections.Generic.List[string]

    foreach ($path in $paths) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            continue
        }

        $stream = [System.IO.File]::OpenRead((Join-Path $RepositoryRoot $path))
        try {
            if ($stream.Length -lt 3) {
                continue
            }

            $buffer = New-Object byte[] 3
            [void]$stream.Read($buffer, 0, 3)
            if ($buffer[0] -eq 0xEF -and $buffer[1] -eq 0xBB -and $buffer[2] -eq 0xBF) {
                $bomFiles.Add($path)
            }
        }
        finally {
            $stream.Dispose()
        }
    }

    if ($bomFiles.Count -gt 0) {
        Write-Error ("UTF-8 BOM is not allowed:`n" + ($bomFiles -join "`n"))
        exit 1
    }

    Write-Output "OK: tracked files are UTF-8 without BOM."
}
finally {
    Set-Location $oldLocation
}
