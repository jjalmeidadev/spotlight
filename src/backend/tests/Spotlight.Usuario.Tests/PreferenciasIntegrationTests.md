# Ponderada Aula 8 - Integracao de Preferencias

## Fluxo testado

`PUT /usuario/me/preferencias` - atualizacao de preferencias declaradas do usuario autenticado (RF03).

O fluxo permite salvar generos favoritos, categorias favoritas e faixa maxima de preco no campo `usuario.perfil_preferencias`. Apos persistir no PostgreSQL, o servico notifica uma API externa que representa o motor de recomendacao para recalculo futuro das sugestoes.

## Coordenacao no grupo

Fluxo reservado para Mariana Reis.

Sem duplicacao com os fluxos dos colegas:

- Rafael Campos: `GET /recomendacao/me`
- Mariana Reis: `PUT /usuario/me/preferencias`

## Classificacao das dependencias

### Possuida - Testcontainers

**PostgreSQL do servico Usuario** - o grupo controla schema, scripts SQL e persistencia do campo `perfil_preferencias`.

Testado com `postgres:16-alpine` via `Testcontainers.PostgreSql`.

No setup do fixture, o teste aplica os scripts reais do projeto:

- `src/database/00_supabase_compat.sql`
- `src/database/01_create_database.sql`

O fixture tambem semeia um usuario minimo em `auth.users`, permitindo que o trigger `on_auth_user_created` crie o perfil correspondente em `public.usuario`. Assim, o teste valida o fluxo contra constraints e tipos reais do PostgreSQL, incluindo a coluna JSONB `perfil_preferencias`.

### Consumida - WireMock.Net

**API externa do motor de recomendacao** - dependencia HTTP consumida pelo servico Usuario para avisar que as preferencias foram alteradas.

Endpoint simulado:

```http
POST /recommendation/preferences-updated
```

O WireMock.Net devolve:

- `200 OK` no cenario de sucesso;
- `503 Service Unavailable` no cenario de indisponibilidade.

Com isso, o teste fica deterministico e nao depende de uma API real de recomendacao estar disponivel durante a execucao.

## Rastreabilidade

**RF03** (`documentos/Projeto.md`, secao 3.1) - "Como usuario, eu quero definir minhas preferencias (como genero, faixa de preco e localizacao) para receber recomendacoes mais relevantes e alinhadas ao meu perfil."

Este teste valida o RF03 porque:

- envia preferencias pela API autenticada;
- persiste os dados no PostgreSQL real do teste;
- verifica o conteudo gravado em `usuario.perfil_preferencias`;
- confirma que o motor de recomendacao foi notificado.

Sem esse teste, uma quebra no contrato do endpoint, no formato JSONB ou na notificacao externa poderia passar despercebida em testes unitarios com fakes.

**RNF03 - Confiabilidade** (`documentos/Projeto.md`, secao 3.2, "Disponibilidade minima de 99%"; ISO/IEC 25010: Reliability).

O cenario de erro prova comportamento controlado quando a dependencia externa esta indisponivel. Quando o motor de recomendacao retorna `503`, o backend responde `409 Conflict` com envelope de erro, sem falha silenciosa e sem excecao nao tratada. Isso protege a confiabilidade do fluxo, pois torna explicita a indisponibilidade da dependencia consumida.

## Cenarios implementados

### Sucesso

`DeveAtualizarPreferenciasQuandoMotorRecomendacaoConfirma`

Valida:

- PostgreSQL descartavel sobe via Testcontainers;
- schema real do projeto e aplicado;
- WireMock responde `200 OK`;
- API retorna `200 OK`;
- `perfil_preferencias` e atualizado no banco;
- WireMock registra exatamente uma chamada ao motor de recomendacao.

### Erro

`DeveRetornarConflitoQuandoMotorRecomendacaoEstaIndisponivel`

Valida:

- PostgreSQL descartavel sobe via Testcontainers;
- WireMock responde `503 Service Unavailable`;
- API retorna `409 Conflict`;
- envelope de erro contem mensagem de indisponibilidade do motor;
- WireMock registra exatamente uma chamada ao motor de recomendacao.

## O que muda na producao

Mudanca aditiva no servico Usuario:

- `PUT /usuario/me/preferencias` passa a notificar o motor de recomendacao apos persistir preferencias.
- Quando `RECOMMENDATION_ENGINE_BASE_URL` nao esta configurado, o servico usa `NoOpPreferenciasRecomendacaoNotifier`, mantendo compatibilidade com execucao local.
- Quando a URL esta configurada, o servico usa `HttpPreferenciasRecomendacaoNotifier` e trata indisponibilidade externa como `ConflictException`.

## Observacoes

- O teste nao executa `02_initial_data.sql`; ele semeia apenas os registros minimos necessarios para manter o fixture deterministico.
- O endpoint e testado por HTTP real via `WebApplicationFactory<Program>`, com JWT assinado no proprio teste.
- Os testes usam o pipeline real da aplicacao, incluindo autenticacao, autorizacao, controller, use case, repositorio PostgreSQL e notifier HTTP.

## Pipeline e cobertura

Execucao local com Docker Desktop ativo:

```powershell
dotnet test src\backend\tests\Spotlight.Usuario.Tests\Spotlight.Usuario.Tests.csproj --collect:"XPlat Code Coverage"
```

Resultado:

```text
Aprovado: 23
Falhou: 0
Ignorado: 0
Total: 23
```

Cobertura dos componentes diretamente relacionados ao fluxo:

```text
AtualizarPreferenciasUseCase: 100%
ObterPreferenciasUseCase: 100%
PreferenciasDto: 100%
UsuarioContextoHttp: 100%
HttpPreferenciasRecomendacaoNotifier: 80%
Program: 89.3%
```

Cobertura global do assembly `Spotlight.Usuario`:

```text
Line coverage: 59.9%
Branch coverage: 51.1%
Method coverage: 75.6%
```

A cobertura global inclui controllers, middlewares, health checks, MongoDB e repositorios que nao fazem parte direta do fluxo individual desta ponderada.

## Checklist da entrega

- [x] Fluxo unico definido e sem duplicacao
- [x] Dependencia possuida com Testcontainers
- [x] Dependencia consumida com WireMock.Net
- [x] Um cenario de sucesso
- [x] Um cenario de erro
- [x] Rastreabilidade funcional com RF03
- [x] Rastreabilidade nao funcional com RNF03 / Confiabilidade
- [x] `dotnet test` verde localmente

## Branch

```text
test/integration-preferencias-usuario
```
