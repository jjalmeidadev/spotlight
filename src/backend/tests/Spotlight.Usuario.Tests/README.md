# Spotlight.Usuario.Tests - Preferencias do Usuario

## Fluxo testado

Endpoint publico via gateway: `PUT /usuario/me/preferencias`

Endpoint interno do servico testado: `PUT /me/preferencias`

Objetivo: validar o fluxo real de atualizacao de preferencias do usuario autenticado, cobrindo a persistencia no PostgreSQL e a chamada HTTP para notificacao de recomendacao.

O fluxo permite que um usuario atualize preferencias declaradas, como generos favoritos, categorias favoritas e faixa maxima de preco. Essas informacoes sao persistidas no campo `perfil_preferencias` da tabela `usuario` e notificadas a um endpoint HTTP de recomendacao para recalculo futuro das sugestoes.

## Coordenacao do grupo

| Aluno | Fluxo testado | Dependencia possuida (Testcontainers) | Dependencia consumida (WireMock.Net) | RF | RNF / ISO 25010 | Status |
|---|---|---|---|---|---|---|
| Mariana | PUT /usuario/me/preferencias | PostgreSQL - tabela usuario, campo perfil_preferencias | Endpoint HTTP de notificacao para recomendacao | RF03 | RNF03 - Confiabilidade | Implementado |
| Vitor | PUT /usuario/me/localizacao | PostgreSQL - tabela usuario, campo localizacao | OpenStreetMap/Nominatim | RF03 | RNF03 - Confiabilidade | Em desenvolvimento |
| Rafael | GET /recomendacao/me | PostgreSQL - tabelas recomendacao, usuario, espetaculo | OpenStreetMap/Nominatim para contexto geografico | RF04 | RNF03 - Confiabilidade | Implementado |
| Celso | GET /recomendacao/espetaculos/{id}/similares | PostgreSQL - tabelas similaridade_item e espetaculo | Motor externo de similaridade/k-NN simulado | RF14 | RNF02 - Desempenho | Em desenvolvimento |
| Leonardo | POST /catalogo/espetaculos/{id}/avaliacoes | PostgreSQL - tabelas usuario, espetaculo e avaliacao | Motor de recomendacao/telemetria recebendo evento de rating | RF09 ou RF11 | RNF03 - Confiabilidade | Em desenvolvimento |
| Mary | POST /catalogo/espetaculos | PostgreSQL - tabelas espetaculo, categoria e relacionamentos | API externa de armazenamento/validacao de imagem ou ticketeira simulada | RF16 | RNF05 - Seguranca | Em desenvolvimento |
| Jaime | GET /geolocalizacao/geocoding/reverse | PostgreSQL - tabela geocode_cache | OpenStreetMap/Nominatim | RF03 ou RF12 | RNF03 - Confiabilidade | Implementado |

## Dependencias classificadas

- Dependencia possuida: PostgreSQL do servico de usuario, executado via `Testcontainers.PostgreSql`, com validacao da tabela `usuario` e do campo `perfil_preferencias`.
- Dependencia consumida: endpoint HTTP de notificacao para recomendacao, simulado com `WireMock.Net` no endpoint `POST /recommendation/preferences-updated`.

## Cenarios

1. Sucesso: WireMock responde `200 OK`, o endpoint responde com sucesso, as preferencias sao persistidas no PostgreSQL e a chamada HTTP de notificacao e registrada.
2. Erro: WireMock responde `503 Service Unavailable`, o backend converte a falha externa em `409 Conflict` e retorna erro controlado ao consumidor.

## RF validado

RF03: como usuario, eu quero definir minhas preferencias, como genero, faixa de preco e localizacao, para receber recomendacoes mais relevantes e alinhadas ao meu perfil.

Falha critica coberta: indisponibilidade do endpoint HTTP de notificacao para recomendacao durante a atualizacao das preferencias.

## RNF validado

RNF03 - Confiabilidade: o fluxo deve se comportar de forma previsivel quando a dependencia externa falha, evitando falha silenciosa ou comportamento inesperado no backend.

## Como executar

Requisito: Docker em execucao para o Testcontainers.

```powershell
dotnet test src\backend\tests\Spotlight.Usuario.Tests\Spotlight.Usuario.Tests.csproj --collect:"XPlat Code Coverage"
```

Resultado validado localmente:

```text
Aprovado: 23
Falhou: 0
Ignorado: 0
Total: 23
```

## Branch

```text
test/integration-preferencias-usuario
```