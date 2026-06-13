param(
    [string]$CatalogBaseUrl = "http://localhost:5001",
    [string]$RecommendationBaseUrl = "http://localhost:5003",
    [string]$UserBaseUrl = "http://localhost:5002",
    [string]$AccessToken = $env:RNF05_ACCESS_TOKEN,
    [string]$CommonUserToken = $env:RNF05_COMMON_USER_TOKEN,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory

if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $rnfRoot ".results\newman"
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

function Convert-ForDocker([string]$Url) {
    return $Url.Replace("://localhost", "://host.docker.internal").Replace("://127.0.0.1", "://host.docker.internal")
}

$collection = Join-Path $rnfRoot "postman\RNF05-Seguranca.postman_collection.json"
if (-not (Test-Path $collection)) { throw "Collection não encontrada: $collection" }

$catalogDocker = Convert-ForDocker $CatalogBaseUrl
$recommendationDocker = Convert-ForDocker $RecommendationBaseUrl
$userDocker = Convert-ForDocker $UserBaseUrl
$mountRoot = $rnfRoot.Replace("\", "/")
$relativeOutput = $OutputDirectory.Substring($rnfRoot.Length).TrimStart("\", "/").Replace("\", "/")
if (-not $relativeOutput) { $relativeOutput = ".results/newman" }

$args = @(
    "run", "--rm", "--add-host=host.docker.internal:host-gateway",
    "-v", "${mountRoot}:/etc/newman/rnf05:ro",
    "-v", "${mountRoot}/${relativeOutput}:/etc/newman/results:rw",
    "postman/newman:alpine", "run",
    "/etc/newman/rnf05/postman/RNF05-Seguranca.postman_collection.json",
    "--env-var", "catalog_base_url=$catalogDocker",
    "--env-var", "recommendation_base_url=$recommendationDocker",
    "--env-var", "user_base_url=$userDocker",
    "--env-var", "access_token=$AccessToken",
    "--env-var", "common_user_token=$CommonUserToken",
    "--reporters", "cli,json,junit",
    "--reporter-json-export", "/etc/newman/results/newman-results.json",
    "--reporter-junit-export", "/etc/newman/results/newman-results.xml",
    "--color", "off"
)

& docker @args 2>&1 | Tee-Object -FilePath (Join-Path $OutputDirectory "newman-console.txt")
$exitCode = $LASTEXITCODE
Write-Host "Newman finalizado. Evidências: $OutputDirectory"
exit $exitCode
