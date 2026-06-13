# Evidências — Bateria Automatizada de Testes Funcionais

**Execução:** 2026-06-12, 18:48–19:15 BRT.
**Ambiente:** Windows 11 + WSL2, .NET SDK 9.0.314, Docker Desktop 4.77.0 (engine 29.5.3).
**Resultado:** **83 / 83 aprovados, 0 reprovados, 0 ignorados**.

## Como reproduzir

A partir da raiz do repositório, com o Docker Desktop em execução:

### Bateria consolidada (solution)

```bat
documentos\evidencias\testes-funcionais\run-tests.bat
```

Equivale a:

```bash
dotnet test src/backend/Spotlight.sln --nologo \
  --logger "trx;LogFileName=tf-solution.trx" \
  --logger "console;verbosity=normal" \
  --results-directory documentos/evidencias/testes-funcionais \
  -e RUN_MONGO_INTEGRATION_TESTS=true
```

### Uma `.trx` por projeto

```bat
documentos\evidencias\testes-funcionais\run-per-project.bat
```

Equivale a, para cada projeto de teste em `src/backend/tests/`:

```bash
dotnet test src/backend/tests/$proj/$proj.csproj --nologo \
  --logger "trx;LogFileName=$proj.trx" \
  --results-directory documentos/evidencias/testes-funcionais \
  -e RUN_MONGO_INTEGRATION_TESTS=true
```

> **Por que `-e ...` em vez de `export ...`?** O `dotnet test` chamado a partir do WSL via `dotnet.exe` (binário Windows) inicia o `vstest.console.exe` com um bloco de environment próprio que não herda variáveis do shell WSL. Passar `-e CHAVE=VALOR` ao `dotnet test` injeta a variável diretamente no processo do runner — único caminho confiável neste setup.

## Resumo consolidado

| Projeto | Total | Aprovados | Ignorados | Reprovados |
|---|---:|---:|---:|---:|
| Spotlight.Catalogo.Tests | 20 | 20 | 0 | 0 |
| Spotlight.Gateway.Tests | 2 | 2 | 0 | 0 |
| Spotlight.Geolocalizacao.Tests | 7 | 7 | 0 | 0 |
| Spotlight.Recomendacao.Tests | 16 | 16 | 0 | 0 |
| Spotlight.Usuario.Tests | 38 | 38 | 0 | 0 |
| **Total** | **83** | **83** | **0** | **0** |

A classe `TelemetriaMongoIntegrationTests` (5 testes) é destravada pelo `MongoIntegrationFactAttribute` quando `RUN_MONGO_INTEGRATION_TESTS=true` está presente no processo; o `Testcontainers.MongoDb` provisiona uma instância `mongo:7.0` efêmera por classe de teste. Sem Docker disponível, esses 5 testes ficam marcados como `Ignorado` (e não como reprovado), mantendo o restante da suíte verde.

## Arquivos

- `run-tests.bat`, `run-per-project.bat` — scripts de reprodução.
- `saida-completa.txt` — log integral do `dotnet test` da solution.
- `saida-por-projeto.txt` — log do `dotnet test` em modo por projeto.
- `Spotlight.<Servico>.Tests.trx` — relatório TRX (Visual Studio Test Results) por projeto, com `executionId`, `outcome`, `duration` e `startTime` de cada caso.

## Conferência rápida

```bash
grep -oE 'outcome="[^"]+"' documentos/evidencias/testes-funcionais/*.trx | sort | uniq -c
# 83 Passed, 5 Completed (1 por arquivo trx)
```
