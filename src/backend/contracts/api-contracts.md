# Contratos de API — Spotlight

Especificação das rotas que serão implementadas no backend, servindo como
**fonte da verdade** para o desenvolvimento, para o Swagger  e para o
frontend. Baseado no modelo de dados em `src/database/01_create_database.sql`.

> **Versão legível por máquina:** estes contratos também estão formalizados em
> [`openapi.json`](./openapi.json) (OpenAPI 3.0.3), que pode ser carregado no
> Swagger UI / Redoc e usado para validação e geração de clientes. Este `.md` é a
> referência narrativa; o `openapi.json` é o artefato consumível. Ao alterar um,
> atualize o outro.

> **Status:**
> As convenções abaixo (envelope de resposta, erros, paginação, autenticação)
> precisam ser ratificadas pela equipe antes do início da implementação.

---

## 1. Convenções gerais

### 1.1 Roteamento (via API Gateway)
Todas as chamadas entram pelo **gateway** (`http://localhost:5000`), que encaminha
para os serviços:

| Prefixo | Serviço | Porta direta |
|---|---|---|
| `/catalogo/*` | Catálogo | 5001 |
| `/usuario/*` | Usuário | 5002 |
| `/recomendacao/*` | Recomendação | 5003 |
| `/geolocalizacao/*` | Geolocalização | 5004 |

Os paths neste documento são relativos ao gateway (ex.: `GET /catalogo/espetaculos`).

### 1.2 Autenticação
JWT emitido pelo **Supabase Auth**, enviado no header:
```
Authorization: Bearer <token-jwt>
```
Níveis (campo `tipo_usuario`): `visitante`, `cadastrado`, `administrador`.

| Acesso | Quem |
|---|---|
| Leitura do catálogo (espetáculos, teatros, categorias, sessões) | Público (sem token) |
| Avaliações, perfil próprio, recomendações próprias | `cadastrado` |
| Criar/editar/excluir espetáculo | `administrador` |

**401 vs 403** (vale para **todas** as rotas protegidas, não só as de admin):
- `401 Unauthorized` → token **ausente ou inválido** (não autenticado).
- `403 Forbidden` → token **válido**, mas o nível do usuário é **insuficiente** para a
  rota. Ex.: um `visitante` autenticado chamando uma rota de `cadastrado`, ou um
  `cadastrado` chamando uma rota de `administrador`.

Portanto, toda rota que exige `cadastrado` ou `administrador` documenta **401 e 403**.

### 1.3 Formato
- `Content-Type: application/json` em requests com body e em todas as responses.
- Datas em **ISO 8601** (`date`: `YYYY-MM-DD`; `date-time`: `YYYY-MM-DDTHH:mm:ssZ`).
- IDs de usuário são **UUID**; demais IDs são inteiros.

### 1.4 Paginação (listas)
Query params `page` (default 1) e `pageSize` (default 20, máx 100). Resposta:
```json
{
  "data": [ /* itens */ ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 137, "totalPages": 7 }
}
```

### 1.5 Padrão de erro
Toda resposta de erro segue este envelope:
```json
{
  "error": "Mensagem legível do erro.",
  "traceId": "00-abc123...-def456-00"
}
```
Em erros de validação (400), inclui `details` por campo:
```json
{
  "error": "Dados de entrada inválidos.",
  "traceId": "00-abc123...-def456-00",
  "details": {
    "titulo": ["O título é obrigatório."],
    "duracao": ["A duração deve ser maior que zero."]
  }
}
```

> **Estado da implementação:** o `ErrorHandlingMiddleware` **atual** trata apenas
> exceções **não capturadas**, retornando `500` com `error` e `traceId`. O envelope
> acima para os demais casos (`400` com `details`, `401`, `403`, `404`, `409`)
> é o **alvo do contrato** e **ainda precisa ser implementado** — seja estendendo o
> middleware, seja tratando esses status nos controllers/serviços. Até lá, considere
> este formato como especificação, não como comportamento já garantido.

### 1.6 Códigos de status usados
| Código | Significado |
|---|---|
| 200 | OK (leitura/atualização bem-sucedida) |
| 201 | Created (recurso criado) |
| 204 | No Content (exclusão bem-sucedida) |
| 400 | Requisição inválida (validação) |
| 401 | Não autenticado (token ausente/ inválido) |
| 403 | Autenticado, mas sem permissão |
| 404 | Recurso não encontrado |
| 409 | Conflito (ex.: avaliação duplicada) |
| 500 | Erro interno |

---

## 2. Schemas compartilhados

### Espetaculo (resumo — usado em listas)
```json
{
  "id": 12,
  "titulo": "O Auto da Compadecida",
  "genero": "Comédia",
  "status": "em_cartaz",
  "faixaEtaria": "Livre",
  "classificacaoIndicativa": "L",
  "duracao": 120,
  "notaMedia": 4.7,
  "popularidade": 8123.5
}
```

### EspetaculoDetalhe (detalhe — inclui relacionamentos)
```json
{
  "id": 12,
  "titulo": "O Auto da Compadecida",
  "descricao": "Adaptação da obra de Ariano Suassuna...",
  "genero": "Comédia",
  "status": "em_cartaz",
  "faixaEtaria": "Livre",
  "classificacaoIndicativa": "L",
  "duracao": 120,
  "produtora": "Aventura Produções",
  "notaMedia": 4.7,
  "popularidade": 8123.5,
  "categorias": [ { "id": 3, "nome": "Teatro Popular" } ],
  "atores": [ { "id": 5, "nome": "João Silva", "papel": "João Grilo", "ordemExibicao": 1 } ],
  "sessoes": [
    {
      "id": 88, "data": "2026-06-10", "horario": "20:00",
      "precoMinimo": 40.00, "precoMaximo": 120.00, "disponibilidade": 230,
      "teatro": { "id": 2, "nome": "Teatro Aventura", "cidade": "Campinas", "estado": "SP" }
    }
  ],
  "createdAt": "2026-05-01T14:00:00Z",
  "updatedAt": "2026-05-20T09:30:00Z"
}
```

### Erro
```json
{ "error": "string", "traceId": "string", "details": { "campo": ["mensagem"] } }
```

---

## 3. Serviço de Catálogo

### 3.1 `GET /catalogo/espetaculos`
**Finalidade:** listar espetáculos do catálogo, com filtros e paginação. Público.

**Request**
- Headers: nenhum obrigatório.
- Query params:
```json
{
  "q": "string (opcional) — busca por título",
  "status": "em_cartaz | em_breve | encerrado (opcional)",
  "genero": "string (opcional)",
  "categoriaId": "integer (opcional)",
  "ordenarPor": "popularidade | nota_media | titulo (default: popularidade)",
  "page": "integer (default 1)",
  "pageSize": "integer (default 20, máx 100)"
}
```

**Responses**
- `200 OK` — lista paginada de `Espetaculo`:
```json
{
  "data": [
    { "id": 12, "titulo": "O Auto da Compadecida", "genero": "Comédia",
      "status": "em_cartaz", "faixaEtaria": "Livre", "classificacaoIndicativa": "L",
      "duracao": 120, "notaMedia": 4.7, "popularidade": 8123.5 }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 137, "totalPages": 7 }
}
```
- `400 Bad Request` — parâmetro inválido (ex.: `status` fora do enum). Schema `Erro`.
- `500` — `Erro`.

---

### 3.2 `GET /catalogo/espetaculos/{id}`
**Finalidade:** detalhe de um espetáculo, com categorias, atores e sessões. Público.

**Request**
- Path param: `id` (integer).

**Responses**
- `200 OK` — `EspetaculoDetalhe` (ver schema na seção 2).
- `404 Not Found`:
```json
{ "error": "Espetáculo 999 não encontrado.", "traceId": "00-...-00" }
```
- `500` — `Erro`.

---

### 3.3 `POST /catalogo/espetaculos`
**Finalidade:** criar um espetáculo. **Requer `administrador`.**

**Request**
- Headers: `Authorization: Bearer <jwt>`, `Content-Type: application/json`.
- Body:
```json
{
  "titulo": "string (obrigatório, máx 200)",
  "descricao": "string (opcional)",
  "genero": "string (opcional, máx 100)",
  "faixaEtaria": "string (opcional, máx 50)",
  "duracao": "integer (opcional, > 0) — minutos",
  "classificacaoIndicativa": "string (opcional, máx 20)",
  "produtora": "string (opcional, máx 150)",
  "status": "em_cartaz | em_breve | encerrado (default em_breve)",
  "categoriaIds": [3, 7],
  "atores": [ { "atorId": 5, "papel": "João Grilo", "ordemExibicao": 1 } ]
}
```

**Responses**
- `201 Created` — `EspetaculoDetalhe` do recurso criado, com header `Location: /catalogo/espetaculos/{id}`.
- `400 Bad Request` — validação (`Erro` com `details`).
- `401 Unauthorized` — sem token. `Erro`.
- `403 Forbidden` — token válido mas não-admin. `Erro`.
- `500` — `Erro`.

**Exemplo de erro 400**
```json
{
  "error": "Dados de entrada inválidos.",
  "traceId": "00-...-00",
  "details": { "titulo": ["O título é obrigatório."], "duracao": ["Deve ser maior que zero."] }
}
```

---

### 3.4 `PUT /catalogo/espetaculos/{id}`
**Finalidade:** atualizar um espetáculo. **Requer `administrador`.**

**Request**
- Path param: `id` (integer). Headers: `Authorization`, `Content-Type`.
- Body: mesmos campos do POST (todos opcionais; envia o que muda).

**Responses**
- `200 OK` — `EspetaculoDetalhe` atualizado.
- `400` / `401` / `403` / `404` / `500` — `Erro`.

---

### 3.5 `DELETE /catalogo/espetaculos/{id}`
**Finalidade:** excluir um espetáculo. **Requer `administrador`.**

**Request**
- Path param: `id` (integer). Header: `Authorization`.

**Responses**
- `204 No Content` — sem body.
- `401` / `403` / `404` / `500` — `Erro`.

---

### 3.6 `GET /catalogo/categorias`
**Finalidade:** listar categorias. Público.

**Responses**
- `200 OK`:
```json
{ "data": [ { "id": 3, "nome": "Teatro Popular", "descricao": "..." } ] }
```
- `500` — `Erro`.

---

### 3.7 `GET /catalogo/teatros`  ·  `GET /catalogo/teatros/{id}`
**Finalidade:** listar teatros / detalhar um teatro. Público.

**Responses (`GET /catalogo/teatros/{id}`)**
- `200 OK`:
```json
{
  "id": 2, "nome": "Teatro Aventura", "endereco": "Rua X, 100",
  "cidade": "Campinas", "estado": "SP", "capacidade": 800,
  "acessibilidade": true, "parceiroNamingRights": null
}
```
- `404` / `500` — `Erro`.

---

### 3.8 `GET /catalogo/espetaculos/{id}/sessoes`
**Finalidade:** listar sessões de um espetáculo. Público.

**Responses**
- `200 OK`:
```json
{
  "data": [
    { "id": 88, "data": "2026-06-10", "horario": "20:00",
      "precoMinimo": 40.00, "precoMaximo": 120.00, "disponibilidade": 230,
      "linkCompra": "https://...",
      "teatro": { "id": 2, "nome": "Teatro Aventura", "cidade": "Campinas", "estado": "SP" } }
  ]
}
```
- `404` (espetáculo inexistente) / `500` — `Erro`.

---

### 3.9 `GET /catalogo/espetaculos/{id}/avaliacoes`
**Finalidade:** listar avaliações de um espetáculo. Público.

**Responses**
- `200 OK`:
```json
{
  "data": [
    { "id": 41, "nota": 5.0, "comentario": "Imperdível!",
      "usuario": { "id": "a1b2...", "nome": "Maria" }, "createdAt": "2026-05-18T20:10:00Z" }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 64, "totalPages": 4 }
}
```
- `404` / `500` — `Erro`.

---

### 3.10 `POST /catalogo/espetaculos/{id}/avaliacoes`
**Finalidade:** avaliar um espetáculo. **Requer `cadastrado`.** Um usuário só pode
avaliar cada espetáculo uma vez (constraint `UNIQUE (id_usuario, id_espetaculo)`).
As avaliações são dados transacionais e permanecem na base relacional
PostgreSQL/Supabase, na tabela `avaliacao`. Elas também podem alimentar o modelo
de recomendação como feedback explícito, mas não devem ser duplicadas como nota
canônica na collection MongoDB de telemetria.

**Request**
- Path param: `id`. Headers: `Authorization`, `Content-Type`.
- Body:
```json
{ "nota": 4.5, "comentario": "string (opcional)" }
```

**Responses**
- `201 Created`:
```json
{ "id": 41, "nota": 4.5, "comentario": "...", "createdAt": "2026-05-26T19:00:00Z" }
```
- `400` — nota fora de 0–5 (`Erro` com `details`).
- `401` — token ausente ou inválido. `Erro`.
- `403` — token válido, mas usuário sem permissão (ex.: `visitante`). `Erro`.
- `404` — espetáculo inexistente.
- `409 Conflict` — usuário já avaliou este espetáculo:
```json
{ "error": "Você já avaliou este espetáculo.", "traceId": "00-...-00" }
```
- `500` — `Erro`.

---

## 4. Serviço de Usuário

### 4.1 `GET /usuario/me`
**Finalidade:** retornar o perfil do usuário autenticado. **Requer `cadastrado`.**

**Request**
- Header: `Authorization: Bearer <jwt>`.

**Responses**
- `200 OK`:
```json
{
  "id": "a1b2c3d4-...",
  "nome": "Maria Souza",
  "email": "maria@exemplo.com",
  "dataNascimento": "1995-04-12",
  "localizacao": {
    "formattedAddress": "Av. Paulista, 1578 - Bela Vista, São Paulo - SP",
    "latitude": -23.55052,
    "longitude": -46.633308,
    "placeId": "123456789",
    "provider": "openstreetmap_nominatim"
  },
  "tipoUsuario": "cadastrado",
  "dataCadastro": "2026-03-01T10:00:00Z"
}
```
- `401` — token ausente ou inválido. `Erro`.
- `403` — token válido, mas usuário sem permissão (ex.: `visitante`). `Erro`.
- `500` — `Erro`.

> `localizacao` é `null` quando o usuário ainda não consentiu a coleta. Quando presente, o objeto sempre tem as 5 chaves documentadas — gravadas exclusivamente por `PUT /usuario/me/localizacao` (4.3).

---

### 4.2 `PUT /usuario/me`
**Finalidade:** atualizar dados do próprio perfil. **Requer `cadastrado`.**

**Request**
- Headers: `Authorization`, `Content-Type`.
- Body (campos opcionais):
```json
{ "nome": "string (máx 150)", "dataNascimento": "1995-04-12" }
```

> O campo `localizacao` **não** faz parte deste contrato. Mesmo que enviado no body, é silenciosamente ignorado pelo deserializador (a propriedade não existe no `UsuarioUpdateDto`) — a coluna JSONB no banco nunca é tocada por esta rota. Para atualizar a localização, use `PUT /usuario/me/localizacao` (4.3), que valida o payload estruturado antes de gravar.

**Responses**
- `200 OK` — perfil atualizado (mesmo schema de 4.1).
- `400` — validação (`Erro` com `details`).
- `401` — token ausente ou inválido. `Erro`.
- `403` — token válido, mas usuário sem permissão. `Erro`.
- `500` — `Erro`.

---

### 4.3 `PUT /usuario/me/localizacao`
**Finalidade:** atualizar a localização consentida do usuário autenticado. **Requer `cadastrado`.**
Esta é a **única** rota que escreve na coluna JSONB `usuario.localizacao` — `PUT /usuario/me` (4.2) não a aceita.

**Request**
- Headers: `Authorization`, `Content-Type`.
- Body (todas as 5 chaves obrigatórias, em camelCase):
```json
{
  "formattedAddress": "Rua Caetanopolis, Sao Paulo, SP",
  "latitude": -23.5470245,
  "longitude": -46.7542871,
  "placeId": "8893681",
  "provider": "openstreetmap_nominatim"
}
```

Validações aplicadas antes da gravação:

| Chave | Tipo | Regra |
|---|---|---|
| `formattedAddress` | `string` | não-vazia |
| `latitude` | `number` | `-90 ≤ lat ≤ 90` |
| `longitude` | `number` | `-180 ≤ lon ≤ 180` |
| `placeId` | `string` | obrigatório, sem validação extra |
| `provider` | `string` | obrigatório |

> **Fluxo recomendado para o cliente:** obtenha o objeto chamando `GET /geolocalizacao/geocoding` ou `GET /geolocalizacao/geocoding/reverse` (5.1 e 5.2) e repasse a resposta inteira como body desta rota. Os valores de `placeId` e `provider` vêm preenchidos pelo serviço de geocoding (`provider = "openstreetmap_nominatim"`) — o cliente não deve fabricar esses campos manualmente.

**Responses**
- `200 OK`:
```json
{ "message": "Localizacao atualizada com sucesso." }
```
- `400` — validação (`Erro` com `details`).
- `401` — token ausente ou inválido. `Erro`.
- `403` — token válido, mas usuário sem permissão. `Erro`.
- `404` — usuário inexistente. `Erro`.
- `500` — `Erro`.

---

### 4.4 `GET /usuario/me/preferencias`
**Finalidade:** retornar as preferências declaradas (onboarding). **Requer `cadastrado`.**
Mapeia o campo `perfil_preferencias` (JSONB).

**Responses**
- `200 OK`:
```json
{ "generosFavoritos": ["Comédia", "Musical"], "categoriasFavoritas": [3, 7], "faixaPrecoMax": 150.00 }
```
- `401` — token ausente ou inválido. `403` — token válido sem permissão. `500` — `Erro`.

---

### 4.5 `PUT /usuario/me/preferencias`
**Finalidade:** atualizar as preferências (usado no onboarding e ajustes). **Requer `cadastrado`.**

**Request**
- Body:
```json
{ "generosFavoritos": ["Comédia", "Musical"], "categoriasFavoritas": [3, 7], "faixaPrecoMax": 150.00 }
```

**Responses**
- `200 OK` — preferências atualizadas (mesmo schema).
- `400` — validação. `401` — token ausente/inválido. `403` — sem permissão. `500` — `Erro`.

---

### 4.6 `GET /usuario/me/avaliacoes`
**Finalidade:** histórico de avaliações do usuário autenticado. **Requer `cadastrado`.**

**Responses**
- `200 OK`:
```json
{
  "data": [
    { "id": 41, "nota": 4.5, "comentario": "...",
      "espetaculo": { "id": 12, "titulo": "O Auto da Compadecida" },
      "createdAt": "2026-05-18T20:10:00Z" }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 8, "totalPages": 1 }
}
```
- `401` — token ausente/inválido. `403` — sem permissão. `500` — `Erro`.

---

### 4.7 `POST /usuario/telemetria`
**Finalidade:** receber eventos de telemetria comportamental gerados pelo aplicativo
mobile, como visualização de tela, clique em card, busca e abertura de
compra. Os eventos válidos são enviados para persistência na coleção
`interacoes` do MongoDB.

> O catálogo completo de eventos, a justificativa de coleta, as propriedades por
> evento e o schema lógico da collection estão detalhados em
> [`telemetria.md`](./telemetria.md).

**Autenticação**
- Header `Authorization: Bearer <jwt>` opcional.
- Se o JWT for enviado e válido, o backend associa o evento ao `sub` do token.
- Se o JWT não for enviado, `anonymousId` é obrigatório.
- Se o JWT for enviado, mas for inválido, a resposta deve ser `401`.

**Request**
- Headers: `Content-Type: application/json`; `Authorization` opcional.
- Body:
```json
{
  "eventId": "evt_01HX9F7M9XK2",
  "eventType": "show_viewed",
  "source": "mobile",
  "sessionId": "sess_01HX9F7M8T",
  "occurredAt": "2026-06-09T14:32:00Z",
  "screen": "Detalhes",
  "entity": {
    "type": "espetaculo",
    "id": "42"
  },
  "properties": {
    "durationMs": 23000,
    "origin": "catalog"
  },
  "device": {
    "platform": "ios",
    "appVersion": "1.0.0"
  },
  "anonymousId": "anon_01HX9F7M8T"
}
```

**Campos**

| Campo | Tipo | Obrigatório | Regra |
|---|---|---|---|
| `eventId` | `string` | Não | Idempotência no cliente. Se omitido, o backend gera um identificador. |
| `eventType` | `string` | Sim | Enum inicial: `session_started`, `screen_viewed`, `show_card_clicked`, `show_viewed`, `search_performed`, `favorite_toggled`, `purchase_clicked`, `session_ended`. |
| `source` | `string` | Sim | Origem do evento. Enum inicial: `mobile`. |
| `sessionId` | `string` | Sim | Identificador da sessão de uso do app. |
| `occurredAt` | `date-time` | Não | Momento em que o evento ocorreu no cliente. Se omitido, o backend usa o horário de recebimento. |
| `screen` | `string` | Não | Tela onde o evento ocorreu. |
| `entity.type` | `string` | Condicional | Obrigatório quando `entity` for enviado. Enum inicial: `espetaculo`, `sessao`, `teatro`, `categoria`, `busca`. |
| `entity.id` | `string` | Condicional | Obrigatório quando `entity` for enviado. |
| `properties` | `object` | Não | Metadados livres do evento. Não deve conter dados sensíveis. |
| `device` | `object` | Não | Metadados do dispositivo/app. Não deve conter dados sensíveis. |
| `anonymousId` | `string` | Condicional | Obrigatório quando não houver usuário autenticado. |

> Limite alvo para `properties` + `device`: 16 KB serializados. Validações mais
> estritas por tipo de evento devem ser implementadas na tarefa de validação do
> payload.

**Responses**
- `202 Accepted` — evento validado e aceito para processamento de telemetria:
```json
{ "eventId": "evt_01HX9F7M9XK2", "status": "accepted" }
```
> A telemetria é best-effort: falhas transitórias de escrita no MongoDB são
> registradas em log e não bloqueiam a navegação do aplicativo.
- `400 Bad Request` — validação (`Erro` com `details`).
- `401 Unauthorized` — token enviado, mas ausente/inválido para associação autenticada. `Erro`.
- `503 Service Unavailable` — telemetria/MongoDB não configurados no ambiente. `Erro`.
- `500` — falha inesperada. `Erro`.

**Documento persistido no MongoDB**

O backend normaliza o payload para a coleção `interacoes` com campos em
`snake_case`:
```json
{
  "event_id": "evt_01HX9F7M9XK2",
  "user_id": "a1b2c3d4-0000-0000-0000-000000000000",
  "anonymous_id": "anon_01HX9F7M8T",
  "session_id": "sess_01HX9F7M8T",
  "event_type": "show_viewed",
  "source": "mobile",
  "screen": "Detalhes",
  "occurred_at": "2026-06-09T14:32:00Z",
  "received_at": "2026-06-09T14:32:01Z",
  "entity": { "type": "espetaculo", "id": "42" },
  "properties": { "durationMs": 23000, "origin": "catalog" },
  "device": { "platform": "ios", "appVersion": "1.0.0" }
}
```

---

## 5. Serviço de Geolocalização

### 5.1 `GET /geolocalizacao/geocoding`
**Finalidade:** converter endereço textual em endereço normalizado e coordenadas via OpenStreetMap/Nominatim.

**Request**
- Query: `address`.

**Responses**
- `200 OK`:
```json
{
  "formattedAddress": "Sao Paulo, Regiao Sudeste, Brasil",
  "latitude": -23.55052,
  "longitude": -46.633308,
  "placeId": "298285",
  "provider": "openstreetmap_nominatim"
}
```
- `400` — validação (`Erro` com `details`).
- `404` — localização não encontrada.
- `409` — falha ao consultar provedor externo.
- `500` — `Erro`.

---

### 5.2 `GET /geolocalizacao/geocoding/reverse`
**Finalidade:** converter latitude/longitude em endereço normalizado via OpenStreetMap/Nominatim.

**Request**
- Query: `latitude`, `longitude`.

**Responses**
- `200 OK` — mesmo schema de 5.1.
- `400` — validação (`Erro` com `details`).
- `404` — localização não encontrada.
- `409` — falha ao consultar provedor externo.
- `500` — `Erro`.

---

## 6. Serviço de Recomendação

### 6.1 Schemas do serviço de recomendação

#### EspetáculoRecomendado

Objeto usado dentro das respostas de recomendação. Representa o subconjunto do espetáculo necessário para exibição em listas e carrosséis.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | integer | Sim | Identificador do espetáculo. |
| `titulo` | string | Sim | Título do espetáculo. |
| `genero` | string ou null | Não | Gênero textual do espetáculo, quando cadastrado. |
| `status` | string | Sim | Status do espetáculo. Ex.: `em_cartaz`, `em_breve`, `encerrado`. |
| `notaMedia` | number | Sim | Média de avaliações do espetáculo. |

Exemplo:

```json
{
  "id": 12,
  "titulo": "O Auto da Compadecida",
  "genero": "Comédia",
  "status": "em_cartaz",
  "notaMedia": 4.7
}
```

#### RecomendacaoItem

Item retornado em `GET /recomendacao/me`.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `espetaculo` | EspetáculoRecomendado | Sim | Espetáculo recomendado. |
| `score` | number | Sim | Pontuação calculada pelo algoritmo de recomendação. Deve ser maior ou igual a zero. |
| `tipoAlgoritmo` | string | Sim | Algoritmo responsável pela recomendação. Ex.: `SVD`, `k-NN`, `content_based`. |
| `motivo` | string ou null | Não | Explicação textual da recomendação. |
| `dataGeracao` | string date-time | Sim | Momento em que a recomendação foi gerada. |

Exemplo:

```json
{
  "espetaculo": {
    "id": 12,
    "titulo": "O Auto da Compadecida",
    "genero": "Comédia",
    "status": "em_cartaz",
    "notaMedia": 4.7
  },
  "score": 0.934512,
  "tipoAlgoritmo": "SVD",
  "motivo": "Por que você gostou de outras comédias populares.",
  "dataGeracao": "2026-05-25T03:00:00Z"
}
```

#### SimilarItem

Item retornado em `GET /recomendacao/espetaculos/{id}/similares`.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `espetaculo` | EspetáculoRecomendado | Sim | Espetáculo similar ao espetáculo de origem. |
| `score` | number | Sim | Grau de similaridade no intervalo esperado de 0 a 1. |
| `tipoAlgoritmo` | string | Sim | Algoritmo usado para calcular similaridade. Ex.: `k-NN`, `SVD`. |

Exemplo:

```json
{
  "espetaculo": {
    "id": 18,
    "titulo": "Morte e Vida Severina",
    "genero": "Drama",
    "status": "em_cartaz",
    "notaMedia": 4.5
  },
  "score": 0.81234,
  "tipoAlgoritmo": "k-NN"
}
```

#### Lista de recomendação

Envelope usado pelas rotas de recomendação.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `data` | array | Sim | Lista de itens retornados. Pode ser vazia quando não houver recomendações disponíveis. |

Exemplo:

```json
{
  "data": []
}
```

---

### 6.2 `GET /recomendacao/me`

**Finalidade:** recomendações personalizadas para o usuário autenticado, ordenadas por `score`. **Requer `cadastrado`.** Lê predições pré-processadas da tabela `recomendacao`.

**Regra básica:** o backend retorna recomendações já calculadas pelo pipeline/modelo de recomendação. A rota não treina o modelo nem recalcula scores em tempo real; ela consulta os resultados persistidos para o usuário autenticado.

**Request**

- Headers:

| Header | Obrigatório | Descrição |
|---|---|---|
| `Authorization: Bearer <token-jwt>` | Sim | JWT válido de usuário `cadastrado`. |

- Query params:

| Campo | Tipo | Obrigatório | Regra |
|---|---|---|---|
| `limite` | integer | Não | Default 10. Valores menores que 1 usam 10. Valor máximo 50. |

Exemplo de request:

```http
GET /recomendacao/me?limite=5
Authorization: Bearer <token-jwt>
```

**Responses**

- `200 OK` - lista de `RecomendacaoItem`:

```json
{
  "data": [
    {
      "espetaculo": {
        "id": 12,
        "titulo": "O Auto da Compadecida",
        "genero": "Comédia",
        "status": "em_cartaz",
        "notaMedia": 4.7
      },
      "score": 0.934512,
      "tipoAlgoritmo": "SVD",
      "motivo": "Por que você gostou de outras comédias populares.",
      "dataGeracao": "2026-05-25T03:00:00Z"
    }
  ]
}
```

- `200 OK` - sem recomendações disponíveis:

```json
{
  "data": []
}
```

- `401 Unauthorized` - token ausente ou inválido. Schema `Erro`.
- `403 Forbidden` - token válido, mas usuário sem permissão. Schema `Erro`.
- `500 Internal Server Error` - erro inesperado. Schema `Erro`.

---

### 6.3 `GET /recomendacao/espetaculos/{id}/similares`

**Finalidade:** espetáculos similares a um dado espetáculo (item-item). **Público.**

> **Fonte dos dados:** a similaridade item-item é **global** (igual para todos os usuários), então **não** sai da tabela `recomendacao` - essa é por usuário e protegida por RLS (`auth.uid() = id_usuario`). Estes similares vêm da tabela dedicada `similaridade_item` (definida em `01_create_database.sql`), populada em batch pelos algoritmos de recomendação (k-NN, SVD, k-means) com leitura pública via política RLS `similaridade_item: leitura pública`.

**Request**

- Path params:

| Campo | Tipo | Obrigatório | Regra |
|---|---|---|---|
| `id` | integer | Sim | Identificador do espetáculo de origem. |

- Query params:

| Campo | Tipo | Obrigatório | Regra |
|---|---|---|---|
| `limite` | integer | Não | Default 10. Valores menores que 1 usam 10. Valor máximo 50. |

Exemplo de request:

```http
GET /recomendacao/espetaculos/12/similares?limite=5
```

**Responses**

- `200 OK` - lista de `SimilarItem`:

```json
{
  "data": [
    {
      "espetaculo": {
        "id": 18,
        "titulo": "Morte e Vida Severina",
        "genero": "Drama",
        "status": "em_cartaz",
        "notaMedia": 4.5
      },
      "score": 0.81234,
      "tipoAlgoritmo": "k-NN"
    }
  ]
}
```

- `200 OK` - sem similares disponíveis:

```json
{
  "data": []
}
```

- `404 Not Found` - espetáculo de origem inexistente. Schema `Erro`.
- `500 Internal Server Error` - erro inesperado. Schema `Erro`.

---
## 7. Rastreabilidade (rota → entidade do banco)

| Rota | Tabela(s) principal(is) |
|---|---|
| `/catalogo/espetaculos*` | `espetaculo`, `espetaculo_categoria`, `espetaculo_ator`, `sessao` |
| `/catalogo/categorias` | `categoria` |
| `/catalogo/teatros*` | `teatro` |
| `/catalogo/.../sessoes` | `sessao`, `teatro` |
| `/catalogo/.../avaliacoes` | `avaliacao`, `usuario` |
| `/usuario/me*` | `usuario` (inclui `perfil_preferencias` JSONB) |
| `/geolocalizacao/geocoding*` | API externa OpenStreetMap/Nominatim; sem persistência direta |
| `/recomendacao/me` | `recomendacao` (por-usuário, RLS), `espetaculo` |
| `/recomendacao/espetaculos/{id}/similares` | `similaridade_item` (global, leitura pública), `espetaculo` |

> **`similaridade_item`:** estrutura para similaridade global item-item, populada
> em batch pelos algoritmos de recomendação. Definida em `01_create_database.sql`
> com os campos `id_espetaculo_origem` (FK), `id_espetaculo_similar` (FK),
> `score_similaridade NUMERIC(8,6)` em `[0, 1]`, `tipo_algoritmo VARCHAR(50)` e
> `data_geracao TIMESTAMPTZ`. A validação de `tipo_algoritmo` é delegada à função
> central `public.is_tipo_algoritmo_valido()` (migration
> `08_tipo_algoritmo_e_unique_recomendacao.sql`) que reaproveita a mesma lista de
> algoritmos usada em `recomendacao`. RLS de **leitura pública** habilitada
> (`similaridade_item: leitura pública` com `USING (true)`).

---
