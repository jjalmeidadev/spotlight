# Evidências — Rodada Final dos Testes Funcionais

**Execução:** 2026-06-24, 17:59–18:14 BRT.
**Ambiente:** Windows 11 + WSL2, .NET SDK 9.0.314, Docker Desktop 4.77.0 (engine 29.5.3).
**Resultado:** **139 / 139 aprovados, 0 reprovados, 0 ignorados**.

A rodada de 12/jun (em `../`) cobria 83 testes e os nomes de projeto em português (`Spotlight.Catalogo.Tests`, etc.). Esta rodada exercita os projetos atuais em inglês (`Spotlight.Catalog.Tests`, etc.) e a base evoluída para 139 testes — sem regressão.

## Como reproduzir

A partir da raiz do repositório, com o Docker Desktop em execução:

### Bateria consolidada (solution)

```bat
documentos\evidencias\testes-funcionais\rodada-final\run-tests.bat
```

Equivale a:

```bash
dotnet test src/backend/Spotlight.sln --nologo \
  --logger "trx;LogFileName=tf-solution.trx" \
  --logger "console;verbosity=normal" \
  --results-directory documentos/evidencias/testes-funcionais/rodada-final \
  -e RUN_MONGO_INTEGRATION_TESTS=true
```

> O comando consolidado roda os 5 projetos em paralelo e **sobrescreve** o `tf-solution.trx` a cada projeto — só o último sobrevive. Para preservar um `.trx` por serviço, use o script por-projeto abaixo.

### Uma `.trx` por projeto

```bat
documentos\evidencias\testes-funcionais\rodada-final\run-per-project.bat
```

Equivale a, para cada projeto de teste em `src/backend/tests/`:

```bash
for proj in Spotlight.Catalog.Tests Spotlight.Gateway.Tests Spotlight.Geolocation.Tests Spotlight.Recommendation.Tests Spotlight.User.Tests; do
  dotnet test src/backend/tests/$proj/$proj.csproj --nologo \
    --logger "trx;LogFileName=$proj.trx" \
    --results-directory documentos/evidencias/testes-funcionais/rodada-final \
    -e RUN_MONGO_INTEGRATION_TESTS=true
done
```

> **Por que `-e ...` em vez de `export ...`?** O `dotnet test` chamado a partir do WSL via `dotnet.exe` (binário Windows) inicia o `vstest.console.exe` com um bloco de environment próprio que não herda variáveis do shell WSL. Passar `-e CHAVE=VALOR` ao `dotnet test` injeta a variável diretamente no processo do runner — único caminho confiável neste setup.

## Resumo consolidado

| Projeto | Total | Aprovados | Ignorados | Reprovados | Tempo |
|---|---:|---:|---:|---:|---:|
| Spotlight.Catalog.Tests | 32 | 32 | 0 | 0 | 2 s |
| Spotlight.Gateway.Tests | 2 | 2 | 0 | 0 | 1 s |
| Spotlight.Geolocation.Tests | 9 | 9 | 0 | 0 | 193 ms |
| Spotlight.Recommendation.Tests | 47 | 47 | 0 | 0 | 1 s |
| Spotlight.User.Tests | 49 | 49 | 0 | 0 | 37 s |
| **Total** | **139** | **139** | **0** | **0** | **≈ 41 s** |

Os 37 s de `Spotlight.User.Tests` concentram o ciclo de provisionamento do container MongoDB efêmero (`mongo:7.0`, via `Testcontainers.MongoDb`) consumido pelos cinco testes de `TelemetriaMongoIntegrationTests`. Sem Docker disponível esses 5 testes reprovam com `DockerUnavailableException` — pré-condição obrigatória da rodada.

## Notas operacionais desta execução

1. **`tf-solution.trx` parcial.** A bateria consolidada (`run-tests.bat`) produz um `tf-solution.trx` truncado — só com os 2 testes do último projeto a terminar (Gateway nesta rodada). É limitação conhecida do `dotnet test` com `--logger trx` sem nome único por projeto; os 5 `.trx` por-projeto contêm a verdade completa.
2. **Test hosts órfãos travam dlls.** Após uma execução consolidada interrompida, processos `dotnet.exe` podem reter `obj/Debug/net9.0/Spotlight.<Servico>.dll` e quebrar a build seguinte. Antes de re-rodar, encerre processos `.NET Host` pendentes (`taskkill /F /IM dotnet.exe` no Windows).
3. **Pasta antiga `../`.** A rodada de 12/jun continua versionada em `documentos/evidencias/testes-funcionais/` (sem o subdiretório `rodada-final`). É a referência histórica do estado dos projetos com nomes em português.

## Arquivos

- `run-tests.bat`, `run-per-project.bat` — scripts de reprodução.
- `saida-completa.txt` — log integral do `dotnet test` da solution.
- `saida-por-projeto.txt` — log do `dotnet test` em modo por projeto.
- `saida-user-rerun.txt` — re-execução do User.Tests após subir o Docker Desktop (substitui os 5 reprovados Docker-unavailable da primeira tentativa).
- `Spotlight.<Servico>.Tests.trx` — relatório TRX (Visual Studio Test Results) por projeto, com `executionId`, `outcome`, `duration` e `startTime` de cada caso.

## Conferência rápida

```bash
grep -oE 'outcome="[^"]+"' documentos/evidencias/testes-funcionais/rodada-final/Spotlight.*.Tests.trx | sort | uniq -c
# 139 Passed, 5 Completed (1 por arquivo trx)
```
