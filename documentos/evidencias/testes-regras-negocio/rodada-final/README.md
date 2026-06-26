# Evidencias - Rodada Final dos Testes de Regras de Negocio

**Objetivo:** Executar testes automatizados de regras de negocio - rodada final  
**Execucao:** 2026-06-24, 17:59-18:14 BRT  
**Ambiente:** Windows 11 + WSL2, .NET SDK 9.0.314, Docker Desktop 4.77.0 (engine 29.5.3)  
**Resultado:** **139 / 139 aprovados, 0 reprovados, 0 ignorados**.

Esta pasta registra o recorte de regras de negocio da rodada final. A execucao reutiliza a bateria automatizada versionada em `src/backend/tests/`, com destaque para os testes de Catalog, Recommendation e User que cobrem validacoes de dominio, limites de entrada, policies RLS documentadas, trigger de usuario e regras de recomendacao.

## Como reproduzir

A partir da raiz do repositorio, com Docker Desktop em execucao:

```bat
documentos\evidencias\testes-regras-negocio\rodada-final\run-business-rules.bat
```

Equivale a:

```bash
dotnet test src/backend/tests/Spotlight.Catalog.Tests/Spotlight.Catalog.Tests.csproj --nologo ^
  --logger "trx;LogFileName=business-rules-catalog.trx" ^
  --results-directory documentos/evidencias/testes-regras-negocio/rodada-final

dotnet test src/backend/tests/Spotlight.Recommendation.Tests/Spotlight.Recommendation.Tests.csproj --nologo ^
  --logger "trx;LogFileName=business-rules-recommendation.trx" ^
  --results-directory documentos/evidencias/testes-regras-negocio/rodada-final

dotnet test src/backend/tests/Spotlight.User.Tests/Spotlight.User.Tests.csproj --nologo ^
  --logger "trx;LogFileName=business-rules-user.trx" ^
  --results-directory documentos/evidencias/testes-regras-negocio/rodada-final ^
  -e RUN_MONGO_INTEGRATION_TESTS=true
```

## Resumo consolidado

| Projeto | Total | Aprovados | Ignorados | Reprovados | Observacao |
|---|---:|---:|---:|---:|---|
| Spotlight.Catalog.Tests | 32 | 32 | 0 | 0 | Avaliacao, catalogo, administracao e validacoes de dominio |
| Spotlight.Recommendation.Tests | 47 | 47 | 0 | 0 | Limites, ranking, fallback ML.NET/Postgres e similares |
| Spotlight.User.Tests | 49 | 49 | 0 | 0 | Trigger/perfil, preferencias, contexto JWT, telemetria e Mongo |
| **Recorte principal** | **128** | **128** | **0** | **0** | Regras de negocio diretamente ligadas ao projeto todo |
| **Solution completa** | **139** | **139** | **0** | **0** | Inclui Gateway e Geolocation como smoke/readiness |

## Regras criticas cobertas

| Regra critica | Evidencia |
|---|---|
| Avaliacao de espetaculo aceita nota valida e rejeita nota fora de 0-5 | `CriarAvaliacao_ComNotaLimiteEComentarioNulo_RepassaDadosParaRepositorio`, `CriarAvaliacao_ComNotaInvalida_LancaValidation` |
| Um usuario nao deve avaliar espetaculo inexistente | `CriarAvaliacao_QuandoEspetaculoNaoExiste_LancaNotFoundENaoCriaAvaliacao` |
| Cada usuario avalia um espetaculo apenas uma vez | Constraint `UNIQUE (id_usuario, id_espetaculo)` mapeada em `src/database/README.md` e validada no recorte de banco/regras |
| Trigger de criacao de usuario deve garantir perfil em `public.usuario` | `UsuarioMeController_ObterPreferencias_GarantePerfilQuandoEmailExiste` e regra RN-BD-02 |
| Policies RLS devem restringir dados privados e escrita de avaliacao | Regras RN-BD-04, RN-BD-24, RN-BD-26, RN-BD-39 e RN-BD-40 documentadas e revisadas |
| Recomendacao deve respeitar limite, ranking e fallback | `ListarRecomendacoesMe_QuandoModeloRetornaMaisItensQueOLimite_CortaResultadoFinal` e testes de `MlNetStrategy`/`PostgresStrategy` |
| Telemetria deve validar payload e ser idempotente por `event_id` | `PostTelemetria_ComEventIdDuplicado_Retorna202MasNaoDuplicaDocumento` e validacoes de `TelemetriaService` |

## Arquivos

- `run-business-rules.bat` - script padronizado para reproduzir o recorte principal.
- `saida-regras-negocio.txt` - log-resumo da execucao usada na documentacao.
- `business-rules-*.trx` - gerados ao reexecutar o script localmente.

Os TRX oficiais da rodada completa permanecem versionados em `documentos/evidencias/testes-funcionais/rodada-final/`, pois a execucao foi a mesma bateria final de backend.