param(
    [string]$BaseUrl = "http://localhost:8081",
    [ValidateRange(1, 20)]
    [int]$Runs = 5,
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

function Find-RepositoryRoot {
    param([string]$StartPath)
    $current = Get-Item $StartPath
    while ($null -ne $current) {
        if (Test-Path (Join-Path $current.FullName "package.json")) {
            return $current.FullName
        }
        $current = $current.Parent
    }
    throw "Raiz do repositório não encontrada."
}

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Find-RepositoryRoot -StartPath $scriptDirectory

if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $repositoryRoot "src/backend/tests/non-functional/.results/rnf01"
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

try {
    $response = Invoke-WebRequest -Uri $BaseUrl -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400) {
        throw "A URL respondeu HTTP $($response.StatusCode)."
    }
}
catch {
    throw "Expo Web não está acessível em $BaseUrl. Inicie com 'npm run web'. Erro: $($_.Exception.Message)"
}

for ($run = 1; $run -le $Runs; $run++) {
    $prefix = Join-Path $OutputDirectory ("lighthouse-mobile-run-{0:D2}" -f $run)
    Write-Host "RNF01 — Lighthouse execução $run de $Runs"

    & npx --yes lighthouse@12.3.0 $BaseUrl `
        --quiet `
        --chrome-flags="--headless --no-sandbox" `
        --form-factor=mobile `
        --throttling-method=simulate `
        --only-categories=performance `
        --output=json `
        --output=html `
        --output-path=$prefix

    if ($LASTEXITCODE -ne 0) {
        throw "Lighthouse falhou na execução $run."
    }
}

$summaryPath = Join-Path $OutputDirectory "lighthouse-summary.csv"
"run,performance_score,fcp_ms,lcp_ms,speed_index_ms,tti_ms,total_blocking_time_ms" | Set-Content -Encoding UTF8 $summaryPath

for ($run = 1; $run -le $Runs; $run++) {
    $jsonPath = Join-Path $OutputDirectory (("lighthouse-mobile-run-{0:D2}.report.json") -f $run)
    if (-not (Test-Path $jsonPath)) {
        $jsonPath = Join-Path $OutputDirectory (("lighthouse-mobile-run-{0:D2}.json") -f $run)
    }
    if (-not (Test-Path $jsonPath)) {
        continue
    }

    $data = Get-Content $jsonPath -Raw | ConvertFrom-Json
    $score = [Math]::Round(($data.categories.performance.score * 100), 2)
    $fcp = [Math]::Round($data.audits.'first-contentful-paint'.numericValue, 2)
    $lcp = [Math]::Round($data.audits.'largest-contentful-paint'.numericValue, 2)
    $speed = [Math]::Round($data.audits.'speed-index'.numericValue, 2)
    $tti = [Math]::Round($data.audits.interactive.numericValue, 2)
    $tbt = [Math]::Round($data.audits.'total-blocking-time'.numericValue, 2)

    "$run,$score,$fcp,$lcp,$speed,$tti,$tbt" | Add-Content -Encoding UTF8 $summaryPath
}

Write-Host "Relatórios gerados em: $OutputDirectory"
Write-Host "Resumo: $summaryPath"
