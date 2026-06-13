param(
    [Parameter(Mandatory = $true)]
    [string]$SummaryPath,

    [ValidateSet("positive", "negative")]
    [string]$Scenario = "positive",

    [string]$ResourcesPath = "",

    [string]$LogsPath = "",

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

function Get-MetricValue {
    param(
        [object]$Metrics,
        [string]$MetricName,
        [string]$ValueName,
        [double]$DefaultValue = 0
    )

    if ($null -eq $Metrics) {
        return $DefaultValue
    }

    $metricProperty = $Metrics.PSObject.Properties[
        $MetricName
    ]

    if ($null -eq $metricProperty) {
        return $DefaultValue
    }

    $metric = $metricProperty.Value

    if ($null -eq $metric.values) {
        return $DefaultValue
    }

    $valueProperty = $metric.values.PSObject.Properties[
        $ValueName
    ]

    if ($null -eq $valueProperty) {
        return $DefaultValue
    }

    return [double]$valueProperty.Value
}

function Format-Number {
    param(
        [double]$Value,
        [int]$Decimals = 2
    )

    return $Value.ToString(
        "N$Decimals",
        [Globalization.CultureInfo]::GetCultureInfo(
            "pt-BR"
        )
    )
}

function Get-ResourceSummary {
    param(
        [string]$Path
    )

    $defaultResult = [PSCustomObject]@{
        SampleCount = 0
        MaxCpu = "Não coletado"
        AverageCpu = "Não coletado"
        MaxMemory = "Não coletado"
        AverageMemory = "Não coletado"
    }

    if (
        -not $Path -or
        -not (Test-Path $Path)
    ) {
        return $defaultResult
    }

    $rows = @(
        Import-Csv $Path
    )

    if ($rows.Count -eq 0) {
        return $defaultResult
    }

    $cpuValues = @()
    $memoryValues = @()

    foreach ($row in $rows) {
        if ($row.cpu_percent) {
            $cpuText = $row.cpu_percent `
                -replace "%", "" `
                -replace ",", "."

            $cpuValue = 0.0

            if (
                [double]::TryParse(
                    $cpuText,
                    [Globalization.NumberStyles]::Any,
                    [Globalization.CultureInfo]::InvariantCulture,
                    [ref]$cpuValue
                )
            ) {
                $cpuValues += $cpuValue
            }
        }

        if ($row.memory_percent) {
            $memoryText = $row.memory_percent `
                -replace "%", "" `
                -replace ",", "."

            $memoryValue = 0.0

            if (
                [double]::TryParse(
                    $memoryText,
                    [Globalization.NumberStyles]::Any,
                    [Globalization.CultureInfo]::InvariantCulture,
                    [ref]$memoryValue
                )
            ) {
                $memoryValues += $memoryValue
            }
        }
    }

    $maxCpu = if ($cpuValues.Count -gt 0) {
        "$(Format-Number (($cpuValues | Measure-Object -Maximum).Maximum) 2)%"
    }
    else {
        "Não coletado"
    }

    $averageCpu = if ($cpuValues.Count -gt 0) {
        "$(Format-Number (($cpuValues | Measure-Object -Average).Average) 2)%"
    }
    else {
        "Não coletado"
    }

    $maxMemory = if ($memoryValues.Count -gt 0) {
        "$(Format-Number (($memoryValues | Measure-Object -Maximum).Maximum) 2)%"
    }
    else {
        "Não coletado"
    }

    $averageMemory = if ($memoryValues.Count -gt 0) {
        "$(Format-Number (($memoryValues | Measure-Object -Average).Average) 2)%"
    }
    else {
        "Não coletado"
    }

    return [PSCustomObject]@{
        SampleCount = $rows.Count
        MaxCpu = $maxCpu
        AverageCpu = $averageCpu
        MaxMemory = $maxMemory
        AverageMemory = $averageMemory
    }
}

if (-not (Test-Path $SummaryPath)) {
    throw "Arquivo de resumo não encontrado: $SummaryPath"
}

$SummaryPath = (
    Resolve-Path $SummaryPath
).Path

if (-not $OutputPath) {
    $summaryDirectory = Split-Path `
        -Parent `
        $SummaryPath

    $summaryName = [
        IO.Path
    ]::GetFileNameWithoutExtension(
        $SummaryPath
    )

    $OutputPath = Join-Path `
        $summaryDirectory `
        "$summaryName-report.md"
}

$summary = Get-Content `
    -Path $SummaryPath `
    -Raw |
    ConvertFrom-Json

$metrics = $summary.metrics

$totalRequests = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_reqs" `
    -ValueName "count"

$requestsPerSecond = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_reqs" `
    -ValueName "rate"

$averageDuration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "avg"

$minimumDuration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "min"

$medianDuration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "med"

$p90Duration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "p(90)"

$p95Duration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "p(95)"

$maximumDuration = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_duration" `
    -ValueName "max"

$httpErrorRate = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "http_req_failed" `
    -ValueName "rate"

$applicationErrorRate = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "application_errors" `
    -ValueName "rate"

$catalogAverage = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "catalog_duration" `
    -ValueName "avg"

$searchAverage = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "search_duration" `
    -ValueName "avg"

$recommendationAverage = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "recommendation_duration" `
    -ValueName "avg"

$iterations = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "iterations" `
    -ValueName "count"

$iterationRate = Get-MetricValue `
    -Metrics $metrics `
    -MetricName "iterations" `
    -ValueName "rate"

$maximumAverageDuration = 3000
$maximumErrorRate = 0.01

$effectiveErrorRate = [Math]::Max(
    $httpErrorRate,
    $applicationErrorRate
)

$durationApproved = (
    $averageDuration -lt
    $maximumAverageDuration
)

$errorApproved = (
    $effectiveErrorRate -lt
    $maximumErrorRate
)

$approved = (
    $durationApproved -and
    $errorApproved
)

$durationStatus = if ($durationApproved) {
    "Atendido"
}
else {
    "Não atendido"
}

$errorStatus = if ($errorApproved) {
    "Atendido"
}
else {
    "Não atendido"
}

$verdict = if ($approved) {
    "Aprovado"
}
else {
    "Reprovado"
}

$scenarioName = if (
    $Scenario -eq "positive"
) {
    "Cenário principal"
}
else {
    "Cenário de sobrecarga e recuperação"
}

$scenarioDescription = if (
    $Scenario -eq "positive"
) {
    "Carga constante de 200 usuários virtuais simultâneos durante 10 minutos."
}
else {
    "Carga progressiva até o limite de sobrecarga, seguida da redução controlada de usuários para análise de recuperação."
}

$resourceSummary = Get-ResourceSummary `
    -Path $ResourcesPath

$logsEvidence = if (
    $LogsPath -and
    (Test-Path $LogsPath)
) {
    "Coletados"
}
else {
    "Não coletados"
}

$executionDate = Get-Date `
    -Format "dd/MM/yyyy HH:mm:ss"

$report = @"
# RNF06 — Carga e escalabilidade

## Objetivo

Validar a capacidade dos serviços de catálogo, busca e recomendação de responder às requisições simultâneas sem ultrapassar os limites definidos de tempo médio e taxa de erro.

## Configuração da execução

| Item | Configuração |
|---|---|
| Ferramenta | k6 |
| Cenário | $scenarioName |
| Descrição | $scenarioDescription |
| Critério de tempo médio | Inferior a 3.000 ms |
| Critério de erro | Inferior a 1% |
| Data da geração | $executionDate |

## Resultados gerais

| Métrica | Resultado |
|---|---:|
| Total de requisições | $(Format-Number $totalRequests 0) |
| Requisições por segundo | $(Format-Number $requestsPerSecond 2) req/s |
| Iterações concluídas | $(Format-Number $iterations 0) |
| Iterações por segundo | $(Format-Number $iterationRate 2) |
| Tempo mínimo | $(Format-Number $minimumDuration 2) ms |
| Tempo médio | $(Format-Number $averageDuration 2) ms |
| Mediana | $(Format-Number $medianDuration 2) ms |
| Percentil 90 | $(Format-Number $p90Duration 2) ms |
| Percentil 95 | $(Format-Number $p95Duration 2) ms |
| Tempo máximo | $(Format-Number $maximumDuration 2) ms |
| Taxa de erro HTTP | $(Format-Number ($httpErrorRate * 100) 2)% |
| Taxa de erro da aplicação | $(Format-Number ($applicationErrorRate * 100) 2)% |

## Resultados por operação

| Operação | Tempo médio |
|---|---:|
| Consulta ao catálogo | $(Format-Number $catalogAverage 2) ms |
| Busca de espetáculos | $(Format-Number $searchAverage 2) ms |
| Recomendação personalizada | $(Format-Number $recommendationAverage 2) ms |

## Consumo de recursos

| Métrica | Resultado |
|---|---:|
| Amostras coletadas | $($resourceSummary.SampleCount) |
| CPU média | $($resourceSummary.AverageCpu) |
| CPU máxima | $($resourceSummary.MaxCpu) |
| Memória média | $($resourceSummary.AverageMemory) |
| Memória máxima | $($resourceSummary.MaxMemory) |
| Logs dos serviços | $logsEvidence |

## Critérios de aceitação

| Critério | Resultado | Situação |
|---|---:|---|
| Tempo médio inferior a 3.000 ms | $(Format-Number $averageDuration 2) ms | $durationStatus |
| Taxa de erro inferior a 1% | $(Format-Number ($effectiveErrorRate * 100) 2)% | $errorStatus |

## Conclusão

**Resultado final: $verdict.**

O resultado foi determinado por meio dos thresholds configurados no teste de carga. Para aprovação, o sistema deve apresentar tempo médio inferior a três segundos e taxa de erro inferior a um por cento.

No cenário de sobrecarga, os resultados devem ser analisados em conjunto com o consumo de CPU, memória, logs dos serviços e comportamento de recuperação após a redução da carga.

## Evidências

- Resumo do k6: ``$SummaryPath``
- Monitoramento de recursos: ``$ResourcesPath``
- Logs dos serviços: ``$LogsPath``
"@

$outputDirectory = Split-Path `
    -Parent `
    $OutputPath

if ($outputDirectory) {
    New-Item `
        -ItemType Directory `
        -Force `
        -Path $outputDirectory |
        Out-Null
}

[IO.File]::WriteAllText(
    $OutputPath,
    $report,
    [Text.UTF8Encoding]::new($false)
)

Write-Host "Relatório RNF06 gerado em:"
Write-Host $OutputPath