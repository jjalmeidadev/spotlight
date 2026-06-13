param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("catalogo", "usuario", "recomendacao", "geolocalizacao")]
    [string]$Service,
    [string]$ServicesFile = "",
    [ValidateRange(30, 600)]
    [int]$TimeoutSeconds = 300,
    [switch]$Execute
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$nonFunctionalRoot = Split-Path -Parent $scriptDirectory
if (-not $ServicesFile) { $ServicesFile = Join-Path $scriptDirectory "services.json" }
$services = Get-Content $ServicesFile -Raw | ConvertFrom-Json
$target = $services | Where-Object { $_.name -eq $Service }
if (-not $target) { throw "Serviço não encontrado: $Service" }

if (-not $Execute) {
    Write-Host "Modo seguro. Comando planejado:"
    Write-Host "docker compose build $Service"
    Write-Host "docker compose up -d --no-deps $Service"
    Write-Host "Use -Execute apenas em ambiente autorizado."
    exit 0
}

$outputDirectory = Join-Path $nonFunctionalRoot ".results/rnf08"
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$csvPath = Join-Path $outputDirectory "deploy-$Service-$timestamp.csv"
$reportPath = Join-Path $outputDirectory "deploy-$Service-$timestamp.md"
"timestamp_utc,service,status_code,available,duration_ms,phase" | Set-Content -Encoding UTF8 $csvPath

function Probe-Service([object]$item, [string]$phase) {
    $started=Get-Date; $status=0; $available=$false
    try { $r=Invoke-WebRequest -Uri $item.healthUrl -UseBasicParsing -TimeoutSec 5; $status=[int]$r.StatusCode; $available=$status -ge 200 -and $status -lt 400 } catch { if ($_.Exception.Response -and $_.Exception.Response.StatusCode) { $status=[int]$_.Exception.Response.StatusCode } }
    $duration=[Math]::Round(((Get-Date)-$started).TotalMilliseconds,2)
    '"{0}","{1}",{2},{3},{4},"{5}"' -f (Get-Date).ToUniversalTime().ToString('o'),$item.name,$status,$available.ToString().ToLowerInvariant(),$duration,$phase | Add-Content -Encoding UTF8 $csvPath
    return $available
}

foreach ($item in $services) { [void](Probe-Service $item "before") }
$deployStarted=Get-Date
& docker compose build $Service | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Build falhou para $Service. Rollback: manter a imagem/container anterior e executar 'docker compose up -d --no-deps $Service'." }
& docker compose up -d --no-deps $Service | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Atualização falhou para $Service. Execute rollback com a imagem anterior."
}

$deadline=(Get-Date).AddSeconds($TimeoutSeconds); $targetRecovered=$false; $otherFailures=0
while ((Get-Date) -lt $deadline) {
    foreach ($item in $services) {
        $ok=Probe-Service $item "during"
        if ($item.name -eq $Service -and $ok) { $targetRecovered=$true }
        if ($item.name -ne $Service -and -not $ok) { $otherFailures++ }
    }
    if ($targetRecovered) { break }
    Start-Sleep -Seconds 2
}
$deployFinished=Get-Date; $downtime=($deployFinished-$deployStarted).TotalSeconds
$status=if($targetRecovered -and $downtime -le 300 -and $otherFailures -eq 0){'Aprovado'}else{'Não aprovado'}
@"
# RNF08 — Deploy independente de $Service

- Início: $($deployStarted.ToString('o'))
- Recuperação/fim: $($deployFinished.ToString('o'))
- Tempo até recuperação: $([Math]::Round($downtime,2)) s
- Falhas observadas nos demais serviços: $otherFailures
- Limite: 300 s
- Resultado: **$status**

## Rollback

Em caso de falha, restaure a referência da imagem anterior e execute:

    docker compose up -d --no-deps $Service
"@ | Set-Content -Encoding UTF8 $reportPath
Write-Host "Evidência: $csvPath"
Write-Host "Relatório: $reportPath"
if (-not $targetRecovered) { exit 1 }
