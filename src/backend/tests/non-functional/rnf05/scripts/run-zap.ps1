param(
    [Parameter(Mandatory = $true)]
    [string]$TargetUrl,
    [string]$OpenApiUrl = "",
    [string]$OutputDirectory = "",
    [switch]$RunActiveApiScan
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $rnfRoot ".results\zap" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
Copy-Item (Join-Path $rnfRoot "config\zap-rules.conf") (Join-Path $OutputDirectory "zap-rules.conf") -Force

function Convert-ForDocker([string]$Url) {
    return $Url.Replace("://localhost", "://host.docker.internal").Replace("://127.0.0.1", "://host.docker.internal")
}
$target = Convert-ForDocker $TargetUrl
$outputMount = (Resolve-Path $OutputDirectory).Path.Replace("\", "/")

$baselineArgs = @(
    "run", "--rm", "--add-host=host.docker.internal:host-gateway",
    "-v", "${outputMount}:/zap/wrk/:rw",
    "ghcr.io/zaproxy/zaproxy:stable", "zap-baseline.py",
    "-t", $target, "-m", "2", "-I",
    "-c", "/zap/wrk/zap-rules.conf",
    "-J", "zap-baseline.json", "-r", "zap-baseline.html", "-w", "zap-baseline.md"
)
& docker @baselineArgs 2>&1 | Tee-Object -FilePath (Join-Path $OutputDirectory "zap-baseline-console.txt")
$baselineExit = $LASTEXITCODE

$apiExit = 0
if ($RunActiveApiScan) {
    if (-not $OpenApiUrl) { throw "OpenApiUrl é obrigatório quando RunActiveApiScan é usado." }
    $openApi = Convert-ForDocker $OpenApiUrl
    $apiArgs = @(
        "run", "--rm", "--add-host=host.docker.internal:host-gateway",
        "-v", "${outputMount}:/zap/wrk/:rw",
        "ghcr.io/zaproxy/zaproxy:stable", "zap-api-scan.py",
        "-t", $openApi, "-f", "openapi", "-I",
        "-J", "zap-api-scan.json", "-r", "zap-api-scan.html", "-w", "zap-api-scan.md"
    )
    & docker @apiArgs 2>&1 | Tee-Object -FilePath (Join-Path $OutputDirectory "zap-api-scan-console.txt")
    $apiExit = $LASTEXITCODE
}
Write-Host "ZAP finalizado. Evidências: $OutputDirectory"
if ($baselineExit -gt 1 -or $apiExit -gt 1) { exit 1 }
