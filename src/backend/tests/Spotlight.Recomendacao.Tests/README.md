# Testes — Spotlight.Recomendacao

Suíte de testes do serviço `recomendacao`. Contém testes unitários e testes de integração ponta a ponta (Ponderada Aula 8 — Testes de Integração SOA).

## Coordenação do grupo — Ponderada Aula 8

Cada integrante testou um fluxo distinto que combina obrigatoriamente uma **dependência possuída** (banco de dados via Testcontainers) e uma **dependência consumida** (API externa via WireMock.Net).

| Aluno | Fluxo testado | Dependência possuída (Testcontainers) | Dependência consumida (WireMock.Net) | RF | RNF | Link do MR |
|---|---|---|---|---|---|---|
| Mariana Lacerda | `PUT /usuario/me/preferencias` | PostgreSQL — tabela `usuario` (`perfil_preferencias`) | Motor de recomendação/notificação (stub) | RF03 | RNF03 — Confiabilidade | — |
| Vitor Ribeiro | `PUT /usuario/me/localizacao` | PostgreSQL — tabela `usuario` (`localizacao`) | OpenStreetMap/Nominatim (stub) | RF03 | RNF03 — Confiabilidade | — |
| Rafael Campos | `GET /recomendacao/me` | PostgreSQL — tabelas `recomendacao`, `usuario`, `espetaculo` | OpenStreetMap/Nominatim — contexto geográfico (stub) | RF04 | RNF03 — Confiabilidade | [test/integration-recomendacao-geo](https://git.inteli.edu.br/graduacao/2026-1b/t17/g04/-/merge_requests) |
| Celso Rodrigues | `GET /recomendacao/espetaculos/{id}/similares` | PostgreSQL — tabelas `similaridade_item`, `espetaculo` | Motor externo de similaridade (stub) | RF14 | RNF02 — Desempenho | — |
| Leonardo Ramos | `POST /catalogo/espetaculos/{id}/avaliacoes` | PostgreSQL — tabelas `usuario`, `espetaculo`, `avaliacao` | Motor de recomendação/telemetria (stub) | RF09 | RNF03 — Confiabilidade | — |
| Mariana Pereira | `POST /catalogo/espetaculos` | PostgreSQL — tabelas `espetaculo`, `categoria` | API de validação de imagem (stub) | RF16 | RNF05 — Segurança | — |
| Jaime Almeida | `GET /geolocalizacao/geocoding/reverse` | Cache/PostgreSQL | OpenStreetMap/Nominatim (stub) | RF03 | RNF03 — Confiabilidade | — |

## Fluxo de Rafael — `GET /recomendacao/me` (branch `test/integration-recomendacao-geo`)

### Dependências no teste

| Dependência | Tipo | Justificativa | Ferramenta |
|---|---|---|---|
| PostgreSQL (`recomendacao`, `espetaculo`, `usuario`) | **Possuída** | Schema, migrations e deploy são gerenciados pelo grupo | `Testcontainers` — `postgres:15-alpine` |
| API de geocodificação OpenStreetMap/Nominatim | **Consumida** | Serviço externo de terceiro, sem controle de uptime/contrato | `WireMock.Net` — stub HTTP |

### Regra de negócio (RF04)

> O usuário autenticado visualiza suas recomendações personalizadas, enriquecidas com o contexto geográfico derivado da sua localização consentida.

O fluxo lê recomendações pré-calculadas do PostgreSQL, extrai as coordenadas salvas em `usuario.localizacao` (JSONB) e faz *reverse geocoding* no Nominatim para anexar um rótulo de cidade/região (`contextoGeografico`) a cada item da lista.

### Requisito Não Funcional (RNF03 — Confiabilidade / ISO 25010)

O enriquecimento geográfico é **best-effort**: qualquer falha na dependência consumida (Nominatim indisponível, timeout de 3 s, status não-2xx, usuário sem localização consentida) é capturada internamente e **não propaga erro para o cliente**. O endpoint retorna sempre 200 com as recomendações; `contextoGeografico` fica `null` quando o enriquecimento não é possível.

Isso sustenta o **RNF03 — Disponibilidade mínima de 99%** (`documentos/Projeto.md` §3.2 e §9.2.1.3): a indisponibilidade de um serviço externo não compromete a funcionalidade central (RF04).

### Cenários de teste

| Arquivo | Cenário | Tipo |
|---|---|---|
| `Integration/RecomendacaoMeGeoIntegrationTests.cs` | Geocoder responde 200 → `contextoGeografico` preenchido | Sucesso |
| `Integration/RecomendacaoMeGeoIntegrationTests.cs` | Geocoder retorna 503 → `contextoGeografico` null, 200 OK | Degradação (RNF03) |
| `Integration/RecomendacaoMeGeoIntegrationTests.cs` | Usuário sem localização (`localizacao IS NULL`) → `contextoGeografico` null | Degradação (RNF03) |
| `Integration/RecomendacaoMeGeoIntegrationTests.cs` | Coordenadas em formato string JSON → geocoder acionado, `contextoGeografico` preenchido | Tolerância de formato |
| `Integration/EspetaculosSimilaresIntegrationTests.cs` | `GET /espetaculos/{id}/similares` ordenado por score | Sucesso |
| `Integration/EspetaculosSimilaresIntegrationTests.cs` | Espetáculo inexistente → 404 | Erro |
| `Integration/EspetaculosSimilaresIntegrationTests.cs` | Documento OpenAPI distingue rotas públicas e autenticadas | Contrato |

### Como rodar

```bash
# Unitários
DOTNET_ROLL_FORWARD=Major dotnet test --filter "Category!=Integration"

# Integração (requer Docker)
DOTNET_ROLL_FORWARD=Major dotnet test --filter "Category=Integration"
```
