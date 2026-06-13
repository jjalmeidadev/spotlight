param(
    [Parameter(Mandatory = $true)]
    [string]$ResultsDirectory,
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $ResultsDirectory)) { throw "Diretório não encontrado: $ResultsDirectory" }
if (-not $OutputPath) { $OutputPath = Join-Path $ResultsDirectory "RNF05-RELATORIO.md" }

function Read-JsonSafe([string]$Path) {
    if (Test-Path $Path) { return Get-Content $Path -Raw | ConvertFrom-Json }
    return $null
}

$static = Read-JsonSafe (Join-Path $ResultsDirectory "static\static-security-results.json")
$transport = Read-JsonSafe (Join-Path $ResultsDirectory "transport\transport-results.json")
$password = Read-JsonSafe (Join-Path $ResultsDirectory "password-storage\password-storage-results.json")
$newman = Read-JsonSafe (Join-Path $ResultsDirectory "newman\newman-results.json")
$zap = Read-JsonSafe (Join-Path $ResultsDirectory "zap\zap-baseline.json")

$newmanAssertions = if ($newman) { [int]$newman.run.stats.assertions.total } else { 0 }
$newmanFailed = if ($newman) { [int]$newman.run.stats.assertions.failed } else { 0 }
$newmanStatus = if (-not $newman) { "Não executado" } elseif ($newmanFailed -eq 0) { "Aprovado" } else { "Reprovado" }
$staticStatus = if (-not $static) { "Não executado" } elseif ($static.passed) { "Aprovado" } else { "Requer análise" }
$transportStatus = if (-not $transport) { "Não executado" } elseif ($transport.passed) { "Aprovado" } else { "Reprovado" }
$passwordStatus = if (-not $password) { "Não executado" } elseif ($password.passed) { "Aprovado" } else { "Reprovado" }

$zapHigh = 0; $zapMedium = 0; $zapLow = 0; $zapInfo = 0
if ($zap -and $zap.site) {
    foreach ($site in @($zap.site)) {
        foreach ($alert in @($site.alerts)) {
            switch ([int]$alert.riskcode) { 3 { $zapHigh++ }; 2 { $zapMedium++ }; 1 { $zapLow++ }; default { $zapInfo++ } }
        }
    }
}
$zapStatus = if (-not $zap) { "Não executado" } elseif ($zapHigh -eq 0) { "Sem alertas altos" } else { "Reprovado" }

$overall = if ($newmanStatus -eq "Aprovado" -and $transportStatus -eq "Aprovado" -and $passwordStatus -eq "Aprovado" -and $zapHigh -eq 0) { "Aprovado" } else { "Pendente ou requer correção" }
$report = @"
# RNF05 — Relatório consolidado de segurança

## Resultado

| Frente | Resultado | Evidência resumida |
|---|---|---|
| JWT e autorização via Newman | $newmanStatus | $newmanAssertions asserções; $newmanFailed falhas |
| Inspeção estática | $staticStatus | $(if($static){"$($static.passedCount) atendidos; $($static.failedCount) não atendidos"}else{"Sem resultado"}) |
| Transporte HTTPS | $transportStatus | $(if($transport){"HTTPS=$($transport.httpsAccepted); HTTP protegido=$($transport.httpBlockedOrRedirectedToHttps)"}else{"Sem resultado"}) |
| Bcrypt e texto plano | $passwordStatus | $(if($password){"bcrypt=$($password.bcryptHashes); custo mínimo=$($password.minimumBcryptCost); suspeitas=$($password.plaintextSuspicions)"}else{"Sem resultado"}) |
| OWASP ZAP Baseline | $zapStatus | Altos=$zapHigh; médios=$zapMedium; baixos=$zapLow; informativos=$zapInfo |

## Critérios

- 100% das requisições protegidas rejeitam token ausente, inválido ou expirado com HTTP 401.
- Usuário autenticado sem papel administrativo recebe HTTP 403.
- Tráfego HTTP é bloqueado ou redirecionado para HTTPS.
- Nenhuma senha é armazenada em texto plano.
- Hashes de senha utilizam bcrypt com custo mínimo 10.
- Nenhuma vulnerabilidade de severidade alta permanece sem tratamento.

## Veredito

**$overall.**

Os artefatos JSON, JUnit, HTML, Markdown e CSV existentes neste diretório compõem as evidências reproduzíveis da execução.
"@
$report | Set-Content -Encoding UTF8 -Path $OutputPath
Write-Host "Relatório consolidado criado: $OutputPath"
