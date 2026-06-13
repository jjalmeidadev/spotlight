param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("catalogo", "usuario", "recomendacao", "geolocalizacao")]
    [string]$Service,
    [Parameter(Mandatory = $true)]
    [string]$HealthUrl,
    [ValidateRange(5, 300)]
    [int]$OutageSeconds = 30,
    [switch]$Execute
)

$ErrorActionPreference = "Stop"
if (-not $Execute) {
    Write-Host "Modo seguro: nenhuma indisponibilidade foi provocada."
    Write-Host "Use -Execute somente em ambiente autorizado."
    exit 0
}

$started = Get-Date
Write-Host "Parando $Service em $started"
docker compose stop $Service | Out-Host
Start-Sleep -Seconds $OutageSeconds
Write-Host "Iniciando $Service"
docker compose start $Service | Out-Host

$deadline = (Get-Date).AddMinutes(5)
$recoveredAt = $null
while ((Get-Date) -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
            $recoveredAt = Get-Date
            break
        }
    } catch {}
    Start-Sleep -Seconds 2
}

if ($null -eq $recoveredAt) { throw "Serviço não recuperado dentro de 5 minutos." }
Write-Host ("Recuperação confirmada em {0:N2} segundos." -f (($recoveredAt - $started).TotalSeconds))
