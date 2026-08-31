#!/usr/bin/env pwsh
# Asserts every packable project declares an explicit <PackageId>, so that
# renaming a .csproj can never silently change a published package ID.
#
# This has already happened twice:
#   BytLabs.DataAccess.MongDB  -> BytLabs.DataAccess.MongoDB  at 1.2.0
#   BytLabs.Api.Graphql        -> BytLabs.Hotchocolate        at 5.2.0-alpha.123 (reverted)
$ErrorActionPreference = 'Stop'

$expected = @{
    'src/BytLabs.Core/BytLabs.Domain/BytLabs.Domain.csproj'                                               = 'BytLabs.Domain'
    'src/BytLabs.Core/BytLabs.Application/BytLabs.Application.csproj'                                     = 'BytLabs.Application'
    'src/BytLabs.DataAccess/BytLabs.DataAccess/BytLabs.DataAccess.csproj'                                 = 'BytLabs.DataAccess'
    'src/BytLabs.DataAccess/BytLabs.DataAccess.MongoDB/BytLabs.DataAccess.MongoDB.csproj'                 = 'BytLabs.DataAccess.MongoDB'
    'src/BytLabs.DataAccess/BytLabs.DataAccess.EntityFramework/BytLabs.DataAccess.EntityFramework.csproj' = 'BytLabs.DataAccess.EntityFramework'
    'src/BytLabs.Api/BytLabs.Api/BytLabs.Api.csproj'                                                      = 'BytLabs.Api'
    'src/BytLabs.Api/BytLabs.Api.Graphql/BytLabs.Api.Graphql.csproj'                                    = 'BytLabs.Api.Graphql'
    'src/BytLabs.Multitenancy/BytLabs.Multitenancy/BytLabs.Multitenancy.csproj'                           = 'BytLabs.Multitenancy'
    'src/BytLabs.Observability/BytLabs.Observability/BytLabs.Observability.csproj'                        = 'BytLabs.Observability'
    'src/BytLabs.States/BytLabs.States.Domain/BytLabs.States.Domain.csproj'                               = 'BytLabs.States.Domain'
    'src/BytLabs.Infrastructure/BytLabs.Infrastructure/BytLabs.Infrastructure.csproj'                     = 'BytLabs.Infrastructure'
}

$failures = @()
foreach ($proj in $expected.Keys | Sort-Object) {
    if (-not (Test-Path $proj)) { $failures += "MISSING PROJECT: $proj"; continue }
    $xml = [xml](Get-Content $proj -Raw)
    $id  = $xml.Project.PropertyGroup.PackageId | Where-Object { $_ }
    if (-not $id)                     { $failures += "NO <PackageId>: $proj (expected '$($expected[$proj])')" }
    elseif ($id -ne $expected[$proj]) { $failures += "WRONG <PackageId>: $proj has '$id', expected '$($expected[$proj])'" }
}

if ($failures) {
    $failures | ForEach-Object { Write-Host "  FAIL: $_" }
    Write-Host "`n$($failures.Count) package ID problem(s)."
    exit 1
}
Write-Host "OK: all $($expected.Count) packable projects declare the expected PackageId."
exit 0
