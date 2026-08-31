#!/usr/bin/env pwsh
# Prints the XML documentation entry for a type from inside a built .nupkg.
param(
    [Parameter(Mandatory)][string]$Nupkg,
    [Parameter(Mandatory)][string]$XmlEntry,
    [int]$Length = 1500
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $Nupkg))
try {
    $entry = $zip.GetEntry($XmlEntry)
    if (-not $entry) { throw "no entry '$XmlEntry' in $Nupkg" }
    $reader = New-Object System.IO.StreamReader($entry.Open())
    try   { $text = $reader.ReadToEnd() }
    finally { $reader.Dispose() }
    $text.Substring(0, [Math]::Min($Length, $text.Length))
} finally { $zip.Dispose() }
