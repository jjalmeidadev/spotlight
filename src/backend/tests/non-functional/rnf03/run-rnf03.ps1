param(
    [string]$EndpointsFile = "",
    [ValidateRange(1, 43200)]
    [int]$DurationMinutes = 60,
    [ValidateRange(5, 3600)]
    [int]$IntervalSeconds = 300,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$nonFunctionalRoot = Split-Path -Parent $scriptDirectory
if (-not $EndpointsFile) { $EndpointsFile = Join-Path $scriptDirectory "endpoints.example.json" }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $nonFunctionalRoot ".results/rnf03" }
if (-not (Test-Path $EndpointsFile)) { throw "Arquivo não encontrado: $EndpointsFile" }

$endpoints = Get-Content $EndpointsFile -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputPath = Join-Path $OutputDirectory "availability-$timestamp.csv"
"timestamp_utc,endpoint,url,status_code,success,duration_ms,error" | Set-Content -Encoding UTF8 $outputPath

$endAt = (Get-Date).AddMinutes($DurationMinutes)
while ((Get-Date) -lt $endAt) {
    foreach ($endpoint in $endpoints) {
        $started = Get-Date
        $statusCode = 0
        $success = $false
        $errorMessage = ""
        try {
            $response = Invoke-WebRequest -Uri $endpoint.url -UseBasicParsing -TimeoutSec 15
            $statusCode = [int]$response.StatusCode
            $success = $statusCode -ge 200 -and $statusCode -lt 400
        }
        catch {
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }
            $errorMessage = $_.Exception.Message.Replace('"', "'")
        }
        $duration = [Math]::Round(((Get-Date) - $started).TotalMilliseconds, 2)
        $utc = (Get-Date).ToUniversalTime().ToString("o")
        '"{0}","{1}","{2}",{3},{4},{5},"{6}"' -f $utc,$endpoint.name,$endpoint.url,$statusCode,$success.ToString().ToLowerInvariant(),$duration,$errorMessage | Add-Content -Encoding UTF8 $outputPath
    }
    $remaining = ($endAt - (Get-Date)).TotalSeconds
    if ($remaining -gt 0) { Start-Sleep -Seconds ([Math]::Min($IntervalSeconds, [int][Math]::Ceiling($remaining))) }
}
Write-Host "Monitoramento concluído: $outputPath"
