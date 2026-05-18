param(
    [string]$OutputPath = "artifacts/static-aot-threads",
    [string]$IntermediatePath = "artifacts/blazor-aot-threads-publish",
    [string]$BaseHref = "/",
    [switch]$NoRestore,
    [switch]$NoHeaders,
    [switch]$NoJekyll
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src/SchoolGrowth.Blazor/SchoolGrowth.Blazor.csproj"
$publishPath = Join-Path $repoRoot $IntermediatePath
$staticPath = Join-Path $repoRoot $OutputPath

function Normalize-BaseHref([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return "/"
    }

    $result = $value.Trim()
    if (-not $result.StartsWith("/")) {
        $result = "/$result"
    }

    if (-not $result.EndsWith("/")) {
        $result = "$result/"
    }

    return $result
}

$normalizedBaseHref = Normalize-BaseHref $BaseHref

Write-Host "Publishing threaded AOT Blazor app..."
Write-Host "  Project: $project"
Write-Host "  Intermediate: $publishPath"
Write-Host "  Static output: $staticPath"
Write-Host "  Base href: $normalizedBaseHref"

if (Test-Path $publishPath) {
    Remove-Item -LiteralPath $publishPath -Recurse -Force
}

if (Test-Path $staticPath) {
    Remove-Item -LiteralPath $staticPath -Recurse -Force
}

$publishArgs = @(
    "publish",
    $project,
    "-c",
    "Release",
    "-p:RunAOTCompilation=true",
    "-p:WasmEnableThreads=true",
    "-o",
    $publishPath
)

if ($NoRestore) {
    $publishArgs += "--no-restore"
}

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$publishedWwwroot = Join-Path $publishPath "wwwroot"
if (-not (Test-Path $publishedWwwroot)) {
    throw "Publish did not produce expected wwwroot folder: $publishedWwwroot"
}

New-Item -ItemType Directory -Force -Path $staticPath | Out-Null
Copy-Item -Path (Join-Path $publishedWwwroot "*") -Destination $staticPath -Recurse -Force

$indexPath = Join-Path $staticPath "index.html"
if (Test-Path $indexPath) {
    $index = Get-Content -LiteralPath $indexPath -Raw
    $index = [regex]::Replace($index, '<base href="[^"]*"\s*/>', "<base href=`"$normalizedBaseHref`" />")
    Set-Content -LiteralPath $indexPath -Value $index -NoNewline
}

if (-not $NoHeaders) {
    $headersPath = Join-Path $staticPath "_headers"
    @"
/*
  Cross-Origin-Opener-Policy: same-origin
  Cross-Origin-Embedder-Policy: require-corp
  Cross-Origin-Resource-Policy: same-origin
"@ | Set-Content -LiteralPath $headersPath -NoNewline
}

if (-not $NoJekyll) {
    New-Item -ItemType File -Force -Path (Join-Path $staticPath ".nojekyll") | Out-Null
}

Write-Host ""
Write-Host "Static threaded AOT deployment is ready:"
Write-Host "  $staticPath"
Write-Host ""
Write-Host "Upload the contents of that folder to the static host."
