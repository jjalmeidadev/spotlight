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
Alinhado ao `ErrorHandlingMiddleware` já implementado. Toda resposta de erro segue:
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
- `401` — sem token.
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
  "localizacao": "Campinas, SP",
  "tipoUsuario": "cadastrado",
  "dataCadastro": "2026-03-01T10:00:00Z"
}
```
- `401` — sem token / inválido. `Erro`.
- `500` — `Erro`.

---

### 4.2 `PUT /usuario/me`
**Finalidade:** atualizar dados do próprio perfil. **Requer `cadastrado`.**

**Request**
- Headers: `Authorization`, `Content-Type`.
- Body (campos opcionais):
```json
{ "nome": "string (máx 150)", "dataNascimento": "1995-04-12", "localizacao": "string (máx 150)" }
```

**Responses**
- `200 OK` — perfil atualizado (mesmo schema de 4.1).
- `400` — validação (`Erro` com `details`).
- `401` — `Erro`.
- `500` — `Erro`.

---

### 4.3 `GET /usuario/me/preferencias`
**Finalidade:** retornar as preferências declaradas (onboarding). **Requer `cadastrado`.**
Mapeia o campo `perfil_preferencias` (JSONB).

**Responses**
- `200 OK`:
```json
{ "generosFavoritos": ["Comédia", "Musical"], "categoriasFavoritas": [3, 7], "faixaPrecoMax": 150.00 }
```
- `401` / `500` — `Erro`.

---

### 4.4 `PUT /usuario/me/preferencias`
**Finalidade:** atualizar as preferências (usado no onboarding e ajustes). **Requer `cadastrado`.**

**Request**
- Body:
```json
{ "generosFavoritos": ["Comédia", "Musical"], "categoriasFavoritas": [3, 7], "faixaPrecoMax": 150.00 }
```

**Responses**
- `200 OK` — preferências atualizadas (mesmo schema).
- `400` / `401` / `500` — `Erro`.

---

### 4.5 `GET /usuario/me/avaliacoes`
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
- `401` / `500` — `Erro`.

---

## 5. Serviço de Recomendação

### 5.1 `GET /recomendacao/me`
**Finalidade:** recomendações personalizadas para o usuário autenticado, ordenadas
por `score`. **Requer `cadastrado`.** Lê predições pré-processadas (tabela `recomendacao`).

**Request**
- Header: `Authorization`.
- Query params:
```json
{ "limite": "integer (default 10, máx 50)" }
```

**Responses**
- `200 OK`:
```json
{
  "data": [
    {
      "espetaculo": { "id": 12, "titulo": "O Auto da Compadecida", "genero": "Comédia",
                      "status": "em_cartaz", "notaMedia": 4.7 },
      "score": 0.934512,
      "tipoAlgoritmo": "SVD",
      "motivo": "Por que você gostou de outras comédias populares.",
      "dataGeracao": "2026-05-25T03:00:00Z"
    }
  ]
}
```
- `401` — `Erro`.
- `500` — `Erro`.

---

### 5.2 `GET /recomendacao/espetaculos/{id}/similares`
**Finalidade:** espetáculos similares a um dado espetáculo (item-item). Público.

**Request**
- Path param: `id`. Query: `limite` (default 10, máx 50).

**Responses**
- `200 OK`:
```json
{
  "data": [
    { "espetaculo": { "id": 18, "titulo": "Morte e Vida Severina", "genero": "Drama",
                      "status": "em_cartaz", "notaMedia": 4.5 },
      "score": 0.812340, "tipoAlgoritmo": "k-NN" }
  ]
}
```
- `404` (espetáculo inexistente) / `500` — `Erro`.

---

## 6. Rastreabilidade (rota → entidade do banco)

| Rota | Tabela(s) principal(is) |
|---|---|
| `/catalogo/espetaculos*` | `espetaculo`, `espetaculo_categoria`, `espetaculo_ator`, `sessao` |
| `/catalogo/categorias` | `categoria` |
| `/catalogo/teatros*` | `teatro` |
| `/catalogo/.../sessoes` | `sessao`, `teatro` |
| `/catalogo/.../avaliacoes` | `avaliacao`, `usuario` |
| `/usuario/me*` | `usuario` (inclui `perfil_preferencias` JSONB) |
| `/recomendacao/*` | `recomendacao`, `espetaculo` |

---

