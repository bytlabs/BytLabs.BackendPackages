#!/usr/bin/env pwsh
# Packs every packable project and asserts each .nupkg carries the files a
# consuming agent needs: the XML API documentation and the package's markdown
# reference. Without these, an agent can only decompile the DLL or guess.
param(
    [string]$PackagesDir = './artifacts/verify-packages',
    [switch]$SkipPack
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not $SkipPack) {
    if (Test-Path $PackagesDir) { Remove-Item $PackagesDir -Recurse -Force }
    New-Item -ItemType Directory -Path $PackagesDir -Force | Out-Null
    Get-ChildItem -Path ./src -Filter *.csproj -Recurse |
        Where-Object { $_.Name -notmatch 'Tests?\.csproj$' } |
        ForEach-Object {
            Write-Host "packing $($_.Name)"
            dotnet pack $_.FullName -c Release -o $PackagesDir --nologo -v quiet
            if ($LASTEXITCODE -ne 0) { throw "pack failed: $($_.FullName)" }
        }
}

$nupkgs = Get-ChildItem -Path $PackagesDir -Filter *.nupkg
if ($nupkgs.Count -lt 11) { Write-Host "  FAIL: expected 11 nupkgs, found $($nupkgs.Count)"; exit 1 }

$failures = @()
foreach ($nupkg in $nupkgs) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
    try {
        $entries = $zip.Entries.FullName
        $id = $null
        foreach ($e in $entries) {
            if ($e -match '^([^/]+)\.nuspec$') { $id = $Matches[1]; break }
        }
        if (-not $id) { $failures += "$($nupkg.Name): no .nuspec"; continue }

        if ($entries -notcontains "lib/net8.0/$id.xml") {
            $failures += "$id : missing lib/net8.0/$id.xml (XML documentation not generated)"
        }
        if ($entries -notcontains "docs/$id.md") {
            $failures += "$id : missing docs/$id.md (package reference not packed)"
        }
    } finally { $zip.Dispose() }
}

if ($failures) {
    $failures | Sort-Object | ForEach-Object { Write-Host "  FAIL: $_" }
    Write-Host "`n$($failures.Count) package content problem(s) across $($nupkgs.Count) packages."
    exit 1
}
Write-Host "OK: all $($nupkgs.Count) packages ship XML documentation and a markdown reference."
exit 0
