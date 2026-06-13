param(
    [string]$ComposeFile = "docker-compose.yml",
    [string]$ServicesFile = "src/backend/tests/non-functional/rnf12/services.json",
    [string]$OutputPath = "src/backend/tests/non-functional/.results/rnf12/container-audit.csv"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $ComposeFile)) { throw "Compose não encontrado: $ComposeFile" }
if (-not (Test-Path $ServicesFile)) { throw "Lista não encontrada: $ServicesFile" }
$compose=Get-Content $ComposeFile -Raw; $services=Get-Content $ServicesFile -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
"service,dockerfile_exists,listed_in_compose,healthcheck,restart_policy,network,status" | Set-Content -Encoding UTF8 $OutputPath
foreach($s in $services){
    $dockerfile=Test-Path $s.dockerfile
    $listed=$compose -match "(?m)^\s{2}$([regex]::Escape($s.name)):\s*$"
    $block=''; if($listed){ $m=[regex]::Match($compose,"(?ms)^\s{2}$([regex]::Escape($s.name)):\s*$.*?(?=^\s{2}[A-Za-z0-9_-]+:\s*$|^networks:|\z)"); $block=$m.Value }
    $health=$block -match '(?m)^\s+healthcheck:'
    $restart=$block -match '(?m)^\s+restart:\s*unless-stopped'
    $network=$block -match '(?m)^\s+networks:'
    $ok=$dockerfile -and $listed -and $health -and $restart -and $network
    '"{0}",{1},{2},{3},{4},{5},"{6}"' -f $s.name,$dockerfile,$listed,$health,$restart,$network,$(if($ok){'conforme'}else{'não conforme'}) | Add-Content -Encoding UTF8 $OutputPath
}
$gatewayDockerfile=Test-Path 'src/backend/api-gateway/Dockerfile'
$gatewayCompose=$compose -match '(?m)^\s{2}(gateway|api-gateway):\s*$'
Write-Host "Auditoria gerada: $OutputPath"
Write-Host "Gateway com Dockerfile: $gatewayDockerfile; no Compose: $gatewayCompose"
