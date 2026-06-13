param(
    [string]$CatalogBaseUrl = "http://localhost:5001",
    [string]$RecommendationBaseUrl = "http://localhost:5003",
    [string]$UserBaseUrl = "http://localhost:5002",
    [string]$HttpsUrl = "",
    [string]$HttpUrl = "",
    [string]$ZapTargetUrl = "",
    [string]$OpenApiUrl = "",
    [string]$AccessToken = $env:RNF05_ACCESS_TOKEN,
    [string]$CommonUserToken = $env:RNF05_COMMON_USER_TOKEN,
    [string]$DatabaseUrl = $env:RNF05_DATABASE_URL,
    [switch]$RunActiveApiScan,
    [switch]$SkipNewman,
    [switch]$SkipZap,
    [switch]$SkipTransport,
    [switch]$SkipPasswordStorage
)

$ErrorActionPreference = "Continue"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$results = Join-Path $rnfRoot ".results\execution-$timestamp"
New-Item -ItemType Directory -Force -Path $results | Out-Null
$steps = @()

function Invoke-ChildScript {
    param(
        [string]$Name,
        [string]$ScriptPath,
        [string[]]$Arguments = @()
    )

    Write-Host "`n=== $Name ==="
    & powershell -NoProfile -ExecutionPolicy Bypass -File $ScriptPath @Arguments
    $code = $LASTEXITCODE
    $script:steps += [PSCustomObject]@{ name = $Name; exitCode = $code }
}

Invoke-ChildScript -Name "Inspeção estática" `
    -ScriptPath (Join-Path $scriptDirectory "run-static-security-check.ps1") `
    -Arguments @("-OutputDirectory", (Join-Path $results "static"))

if (-not $SkipNewman) {
    Invoke-ChildScript -Name "Postman/Newman" `
        -ScriptPath (Join-Path $scriptDirectory "run-newman.ps1") `
        -Arguments @(
            "-CatalogBaseUrl", $CatalogBaseUrl,
            "-RecommendationBaseUrl", $RecommendationBaseUrl,
            "-UserBaseUrl", $UserBaseUrl,
            "-AccessToken", $AccessToken,
            "-CommonUserToken", $CommonUserToken,
            "-OutputDirectory", (Join-Path $results "newman")
        )
}

if (-not $SkipTransport -and $HttpsUrl) {
    Invoke-ChildScript -Name "HTTPS" `
        -ScriptPath (Join-Path $scriptDirectory "run-transport-check.ps1") `
        -Arguments @(
            "-HttpsUrl", $HttpsUrl,
            "-HttpUrl", $HttpUrl,
            "-OutputDirectory", (Join-Path $results "transport")
        )
}

if (-not $SkipPasswordStorage -and $DatabaseUrl) {
    Invoke-ChildScript -Name "Bcrypt" `
        -ScriptPath (Join-Path $scriptDirectory "run-password-storage-check.ps1") `
        -Arguments @(
            "-DatabaseUrl", $DatabaseUrl,
            "-OutputDirectory", (Join-Path $results "password-storage")
        )
}

if (-not $SkipZap -and $ZapTargetUrl) {
    $zapArguments = @(
        "-TargetUrl", $ZapTargetUrl,
        "-OpenApiUrl", $OpenApiUrl,
        "-OutputDirectory", (Join-Path $results "zap")
    )
    if ($RunActiveApiScan) { $zapArguments += "-RunActiveApiScan" }
    Invoke-ChildScript -Name "OWASP ZAP" `
        -ScriptPath (Join-Path $scriptDirectory "run-zap.ps1") `
        -Arguments $zapArguments
}

$steps | ConvertTo-Json | Set-Content -Encoding UTF8 -Path (Join-Path $results "execution-steps.json")
& (Join-Path $scriptDirectory "generate-rnf05-report.ps1") -ResultsDirectory $results
Write-Host "`nRNF05 finalizado. Evidências: $results"
