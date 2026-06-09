# Spotlight.Geolocation.Tests

## Fluxo testado

Endpoint publico via gateway: `GET /geolocalizacao/geocoding/reverse?latitude={latitude}&longitude={longitude}`

Endpoint interno do servico testado: `GET /geocoding/reverse?latitude={latitude}&longitude={longitude}`

Objetivo: validar o fluxo de reverse geocoding do servico real `Spotlight.Geolocalizacao`, que recebe latitude/longitude, consulta OpenStreetMap/Nominatim e persiste o resultado normalizado em cache relacional.

## Coordenacao do grupo

| Aluno | Fluxo testado | Dependencia possuida (Testcontainers) | Dependencia consumida (WireMock.Net) | RF | RNF / ISO 25010 | Status |
|---|---|---|---|---|---|---|
| Mariana | PUT /usuario/me/preferencias | PostgreSQL - tabela usuario, campo perfil_preferencias | Motor de recomendacao/notificacao de preferencias | RF03 | RNF03 - Confiabilidade | Em desenvolvimento |
| Vitor | PUT /usuario/me/localizacao | PostgreSQL - tabela usuario, campo localizacao | OpenStreetMap/Nominatim | RF03 | RNF03 - Confiabilidade | Em desenvolvimento |
| Rafael | GET /recomendacao/me | PostgreSQL - tabelas recomendacao, usuario, espetaculo | OpenStreetMap/Nominatim para contexto geografico | RF04 | RNF02 - Desempenho ou RNF03 - Confiabilidade | Em desenvolvimento |
| Celso | GET /recomendacao/espetaculos/{id}/similares | PostgreSQL - tabelas similaridade_item e espetaculo | Motor externo de similaridade/k-NN simulado | RF14 | RNF02 - Desempenho | Em desenvolvimento |
| Leonardo | POST /catalogo/espetaculos/{id}/avaliacoes | PostgreSQL - tabelas usuario, espetaculo e avaliacao | Motor de recomendacao/telemetria recebendo evento de rating | RF09 ou RF11 | RNF05 - Seguranca ou RNF03 - Confiabilidade | Em desenvolvimento |
| Mary | POST /catalogo/espetaculos | PostgreSQL - tabelas espetaculo, categoria e relacionamentos | API externa de armazenamento/validacao de imagem ou ticketeira simulada | RF16 | RNF05 - Seguranca | Em desenvolvimento |
| Jaime | GET /geolocalizacao/geocoding/reverse | PostgreSQL - tabela geocode_cache | OpenStreetMap/Nominatim | RF03 ou RF12 | RNF03 - Confiabilidade | Implementado |

## Dependencias classificadas

- Dependencia possuida: PostgreSQL, executado via `Testcontainers.PostgreSql`, com tabela `geocode_cache`.
- Dependencia consumida: API externa OpenStreetMap/Nominatim, simulada com `WireMock.Net`.

## Cenarios

1. Sucesso: WireMock retorna endereco formatado, o endpoint interno responde `200 OK`, a API externa e chamada e o resultado e persistido no Postgres.
2. Erro: WireMock retorna `503 Service Unavailable`, o servico converte a falha externa em `409 Conflict` e nenhum registro e persistido no cache.

## RF validado

RF03/RF12: o fluxo de geolocalizacao apoia preferencias/localizacao do usuario e recomendacoes contextuais baseadas em localizacao. O teste valida a normalizacao de coordenadas em endereco, etapa necessaria para usar contexto geografico no sistema.

Falha critica coberta: indisponibilidade da API externa de geocodificacao.

## RNF validado

RNF03 - Confiabilidade: o fluxo deve se comportar de forma previsivel quando a dependencia externa falha, sem persistir dados incompletos ou incorretos no cache.

## Como executar

Requisito: Docker em execucao para o Testcontainers.

```powershell
dotnet test src\backend\tests\Spotlight.Geolocation.Tests -v minimal
```

Resultado esperado: 2 testes aprovados.
