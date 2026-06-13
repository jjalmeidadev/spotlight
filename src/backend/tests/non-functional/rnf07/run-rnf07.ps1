param(
    [string]$EventsFile = "",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$nonFunctionalRoot = Split-Path -Parent $scriptDirectory
if (-not $EventsFile) { $EventsFile = Join-Path $scriptDirectory "events.example.json" }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $nonFunctionalRoot ".results/rnf07" }
if (-not (Test-Path $EventsFile)) { throw "Arquivo não encontrado: $EventsFile" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$events = Get-Content $EventsFile -Raw | ConvertFrom-Json
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$eventOutput = Join-Path $OutputDirectory "events-$timestamp.csv"
$logOutput = Join-Path $OutputDirectory "docker-logs-$timestamp.txt"
"event_id,name,service,started_utc,finished_utc,duration_ms,status_code,success,error" | Set-Content -Encoding UTF8 $eventOutput

$since = (Get-Date).ToUniversalTime().AddSeconds(-5).ToString("o")
foreach ($event in $events) {
    $id = [Guid]::NewGuid().ToString()
    $session = "rnf07-session-$timestamp"
    $device = "rnf07-windows-powershell"
    $started = (Get-Date).ToUniversalTime()
    $status = 0; $success = $false; $errorMessage = ""
    try {
        $response = Invoke-WebRequest -Uri $event.url -Method $event.method -UseBasicParsing -TimeoutSec 15 -Headers @{
            "X-Correlation-ID" = $id
            "X-Session-ID" = $session
            "X-Device-ID" = $device
        }
        $status = [int]$response.StatusCode
        $success = $event.expected -contains $status
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) { $status = [int]$_.Exception.Response.StatusCode }
        $success = $event.expected -contains $status
        $errorMessage = $_.Exception.Message.Replace('"', "'")
    }
    $finished = (Get-Date).ToUniversalTime()
    $duration = [Math]::Round(($finished-$started).TotalMilliseconds,2)
    '"{0}","{1}","{2}","{3}","{4}",{5},{6},{7},"{8}"' -f $id,$event.name,$event.service,$started.ToString('o'),$finished.ToString('o'),$duration,$status,$success.ToString().ToLowerInvariant(),$errorMessage | Add-Content -Encoding UTF8 $eventOutput
}

Start-Sleep -Seconds 2
docker compose logs --no-color --since $since catalogo usuario recomendacao geolocalizacao 2>&1 | Set-Content -Encoding UTF8 $logOutput
Write-Host "Eventos: $eventOutput"
Write-Host "Logs: $logOutput"
