param(
    [Parameter(Mandatory = $true)]
    [string]$HttpsUrl,
    [string]$HttpUrl = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$rnfRoot = Split-Path -Parent $scriptDirectory
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $rnfRoot ".results\transport" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

function Invoke-NoRedirect([string]$Url) {
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(20)
    try {
        $response = $client.GetAsync($Url).GetAwaiter().GetResult()
        $headers = @{}
        foreach ($header in $response.Headers) { $headers[$header.Key.ToLowerInvariant()] = ($header.Value -join ", ") }
        foreach ($header in $response.Content.Headers) { $headers[$header.Key.ToLowerInvariant()] = ($header.Value -join ", ") }
        return [PSCustomObject]@{
            reachable = $true
            statusCode = [int]$response.StatusCode
            location = if ($response.Headers.Location) { $response.Headers.Location.ToString() } else { "" }
            headers = $headers
            error = ""
        }
    }
    catch {
        return [PSCustomObject]@{ reachable = $false; statusCode = 0; location = ""; headers = @{}; error = $_.Exception.Message }
    }
    finally { $client.Dispose(); $handler.Dispose() }
}

$httpsUri = [Uri]$HttpsUrl
if ($httpsUri.Scheme -ne "https") { throw "HttpsUrl deve utilizar https://" }
$httpsResult = Invoke-NoRedirect $HttpsUrl
$httpsAccepted = $httpsResult.reachable -and $httpsResult.statusCode -lt 500

$httpResult = $null
$httpProtected = $null
if ($HttpUrl) {
    $httpUri = [Uri]$HttpUrl
    if ($httpUri.Scheme -ne "http") { throw "HttpUrl deve utilizar http://" }
    $httpResult = Invoke-NoRedirect $HttpUrl
    $redirectCodes = @(301, 302, 307, 308)
    $redirectedToHttps = $httpResult.reachable -and ($redirectCodes -contains $httpResult.statusCode) -and $httpResult.location.StartsWith("https://")
    $blocked = -not $httpResult.reachable
    $httpProtected = $redirectedToHttps -or $blocked
}

$headers = $httpsResult.headers
$result = [ordered]@{
    checkedAt = (Get-Date).ToString("o")
    httpsUrl = $HttpsUrl
    httpsReachable = $httpsResult.reachable
    httpsStatusCode = $httpsResult.statusCode
    httpsAccepted = $httpsAccepted
    httpUrl = $HttpUrl
    httpStatusCode = if ($httpResult) { $httpResult.statusCode } else { $null }
    httpLocation = if ($httpResult) { $httpResult.location } else { "" }
    httpBlockedOrRedirectedToHttps = $httpProtected
    strictTransportSecurity = $headers.ContainsKey("strict-transport-security")
    xContentTypeOptions = $headers.ContainsKey("x-content-type-options")
    contentSecurityPolicy = $headers.ContainsKey("content-security-policy")
    passed = $httpsAccepted -and (($null -eq $httpProtected) -or $httpProtected)
}

$jsonPath = Join-Path $OutputDirectory "transport-results.json"
$mdPath = Join-Path $OutputDirectory "transport-results.md"
$result | ConvertTo-Json -Depth 6 | Set-Content -Encoding UTF8 -Path $jsonPath

$httpDescription = if ($null -eq $httpProtected) { "Não informado" } elseif ($httpProtected) { "Bloqueado ou redirecionado para HTTPS" } else { "Aceitou tráfego HTTP sem redirecionamento" }
$markdown = @"
# RNF05 — Transporte HTTPS

| Verificação | Resultado |
|---|---|
| URL HTTPS | ``$HttpsUrl`` |
| HTTPS acessível | $($httpsResult.reachable) |
| Status HTTPS | $($httpsResult.statusCode) |
| HTTP inseguro | $httpDescription |
| HSTS | $($result.strictTransportSecurity) |
| X-Content-Type-Options | $($result.xContentTypeOptions) |
| Content-Security-Policy | $($result.contentSecurityPolicy) |
| Situação | $(if ($result.passed) { "Aprovado" } else { "Reprovado" }) |
"@
$markdown | Set-Content -Encoding UTF8 -Path $mdPath
Write-Host "Verificação de transporte concluída: $jsonPath"
if (-not $result.passed) { exit 1 }
