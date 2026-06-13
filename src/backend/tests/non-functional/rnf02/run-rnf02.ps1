param(
    [string]$EnvFile = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$nonFunctionalRoot = Split-Path -Parent $scriptDirectory

if (-not $EnvFile) {
    $EnvFile = Join-Path $scriptDirectory ".env.rnf02"
}
if (-not (Test-Path $EnvFile)) {
    throw "Crie $EnvFile com base em .env.rnf02.example."
}
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $nonFunctionalRoot ".results/rnf02"
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

foreach ($line in Get-Content $EnvFile) {
    $trimmed = $line.Trim()
    if (-not $trimmed -or $trimmed.StartsWith("#")) { continue }
    $parts = $trimmed -split "=", 2
    if ($parts.Count -ne 2) { continue }
    [Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim(), "Process")
}

if ($env:RECOMMENDATION_BASE_URL) {
    $env:RECOMMENDATION_BASE_URL = $env:RECOMMENDATION_BASE_URL.Replace("://localhost", "://host.docker.internal").Replace("://127.0.0.1", "://host.docker.internal")
}

$required = @("PROFILE_1_TOKEN", "PROFILE_2_TOKEN", "PROFILE_3_TOKEN")
foreach ($name in $required) {
    if (-not [Environment]::GetEnvironmentVariable($name, "Process")) {
        throw "Variável obrigatória ausente: $name"
    }
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$consolePath = Join-Path $OutputDirectory "rnf02-$timestamp-console.txt"

$arguments = @(
    "run", "--rm",
    "--add-host=host.docker.internal:host-gateway",
    "-v", "${scriptDirectory}:/scripts:ro",
    "-v", "${OutputDirectory}:/results",
    "-e", "RECOMMENDATION_BASE_URL",
    "-e", "RECOMMENDATION_PATH",
    "-e", "PROFILE_1_TOKEN",
    "-e", "PROFILE_2_TOKEN",
    "-e", "PROFILE_3_TOKEN",
    "-e", "RNF02_MAX_DURATION_MS",
    "grafana/k6:latest",
    "run", "/scripts/recommendation-latency.js"
)

& docker @arguments 2>&1 | Tee-Object -FilePath $consolePath
exit $LASTEXITCODE
