# Evidências — Bateria Automatizada de Testes Funcionais

**Execução:** 2026-06-24 (BRT).
**Ambiente:** macOS (Darwin 25.5), .NET SDK 10.0.300 com `DOTNET_ROLL_FORWARD=Major` para executar o testhost `net9.0`, Docker 29.2.1 (`mongo:7.0` efêmero via Testcontainers).
**Resultado:** **139 / 139 aprovados, 0 reprovados, 0 ignorados**.

## Como reproduzir

A partir da raiz do repositório, com o Docker em execução.

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

> **Variável `RUN_MONGO_INTEGRATION_TESTS`.** Com ela em `true` **e** Docker disponível, o `Testcontainers.MongoDb` provisiona uma instância `mongo:7.0` efêmera e os 5 testes de integração de telemetria rodam. **Sem Docker, omita a variável** (não passe `-e ...`): o `MongoIntegrationFactAttribute` mantém esses 5 testes como `Ignorado` em vez de reprovar.
>
> **Por que `-e ...` em vez de `export ...`?** Em WSL, o `dotnet test` via `dotnet.exe` (binário Windows) inicia o `vstest.console.exe` com um bloco de environment próprio que não herda variáveis do shell. Passar `-e CHAVE=VALOR` injeta a variável diretamente no processo do runner.
>
> **Runtime `net9.0` ausente.** Em máquinas só com o SDK do .NET 10, exporte `DOTNET_ROLL_FORWARD=Major` antes de rodar os testes para que o testhost `net9.0` use o runtime 10 instalado.

## Resumo consolidado

| Projeto | Total | Aprovados | Ignorados | Reprovados |
|---|---:|---:|---:|---:|
| Spotlight.Catalog.Tests | 32 | 32 | 0 | 0 |
| Spotlight.Gateway.Tests | 2 | 2 | 0 | 0 |
| Spotlight.Geolocation.Tests | 9 | 9 | 0 | 0 |
| Spotlight.Recommendation.Tests | 47 | 47 | 0 | 0 |
| Spotlight.User.Tests | 49 | 49 | 0 | 0 |
| **Total** | **139** | **139** | **0** | **0** |

A classe `TelemetriaMongoIntegrationTests` (5 testes, dentro de `Spotlight.User.Tests`) é destravada pelo `MongoIntegrationFactAttribute` quando `RUN_MONGO_INTEGRATION_TESTS=true` está presente no processo e há Docker disponível; o `Testcontainers.MongoDb` provisiona uma instância `mongo:7.0` efêmera por classe de teste (o que responde pelos ~2 min de duração desse projeto).

## Arquivos

- `run-tests.bat`, `run-per-project.bat` — scripts de reprodução.
- `saida-completa.txt` — log integral do `dotnet test` da solution.
- `saida-por-projeto.txt` — log do `dotnet test` em modo por projeto (linha-resumo `Aprovado!  – Com falha: 0…` por serviço).
- `Spotlight.<Servico>.Tests.trx` — relatório TRX (Visual Studio Test Results) por projeto, com `executionId`, `outcome`, `duration` e `startTime` de cada caso. Os nomes de arquivo seguem o nome do projeto/assembly (em inglês); os nomes qualificados dos testes dentro do TRX usam o namespace do código (em português, ex.: `Spotlight.Usuario.Tests`), espelhando a convenção do repositório: pasta/projeto em inglês, namespace em português.

## Conferência rápida

```bash
grep -hoE 'outcome="[^"]+"' documentos/evidencias/testes-funcionais/*.trx | sort | uniq -c
# 139 Passed, 5 Completed (1 marcador de execução por arquivo .trx)
```
