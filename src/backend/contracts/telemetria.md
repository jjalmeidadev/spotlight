# Telemetria MongoDB

Este documento consolida a base de implementação da telemetria no projeto:

- Definir quais eventos coletar e por que.
- Modelar o schema logico dos documentos persistidos no MongoDB.

O contrato HTTP relacionado esta em `api-contracts.md`, secao `4.7 POST /usuario/telemetria`.

---

## Objetivo

A telemetria registra sinais comportamentais do usuario no aplicativo mobile para:

- medir engajamento por tela e por espetaculo;
- alimentar recomendacoes futuras com sinais implicitos;
- auditar origem de interacoes que impactam popularidade e recomendacao;
- identificar falhas de jornada, abandono e baixo interesse;
- preservar a experiencia do usuario sem bloquear navegacao.

Os eventos sao gravados na collection MongoDB `interacoes`, em modelo append-only.
Dados transacionais continuam no PostgreSQL/Supabase.

Avaliacoes de espetaculos sao excecao importante: a nota e o comentario sao
dados transacionais e permanecem na tabela relacional `avaliacao`. O modelo de
recomendacao deve consumir essa tabela como fonte de feedback explicito e cruzar
esses dados com os sinais implicitos do MongoDB quando necessario. A telemetria
nao deve duplicar a nota como fonte canonica.

---

## Catalogo de Eventos

| Evento (`eventType`) | Quando disparar | Por que coletar | Entidade esperada | Propriedades minimas |
|---|---|---|---|---|
| `session_started` | Ao iniciar uma sessao de uso do app | Medir volume de sessoes, recorrencia e funil de entrada | Nenhuma | `entryPoint` |
| `screen_viewed` | Ao abrir uma tela relevante | Medir navegacao e identificar telas com maior/menor engajamento | Opcional | `routeName` |
| `show_card_clicked` | Ao tocar em um card de espetaculo no catalogo/home | Medir interesse inicial antes da visualizacao detalhada | `espetaculo` | `origin`, `position` |
| `show_viewed` | Ao abrir a tela de detalhe de um espetaculo | Sinal forte de interesse implicito para recomendacao | `espetaculo` | `origin` |
| `search_performed` | Ao executar uma busca no catalogo | Identificar intencao declarada e termos com demanda | `busca` | `query`, `resultsCount` |
| `favorite_toggled` | Ao favoritar ou desfavoritar um espetaculo | Sinal explicito de interesse ou perda de interesse | `espetaculo` | `active` |
| `purchase_clicked` | Ao abrir link/fluxo de compra | Medir conversao e intencao de compra | `sessao` ou `espetaculo` | `provider`, `sessionIdExternal` |
| `session_ended` | Ao encerrar sessao ou detectar app em background por tempo limite | Calcular duracao de sessao e abandono | Nenhuma | `durationMs`, `reason` |

### Eventos de Prioridade para a Primeira Implementacao

Para reduzir escopo inicial, a primeira implementacao deve priorizar:

1. `session_started`
2. `screen_viewed`
3. `show_card_clicked`
4. `show_viewed`
5. `search_performed`

Os eventos `favorite_toggled`, `purchase_clicked` e `session_ended` podem entrar
na sequencia, pois dependem de fluxos especificos do app.

Avaliacoes nao entram neste catalogo como documento de telemetria com nota. O
fluxo de avaliacao deve usar `POST /catalogo/espetaculos/{id}/avaliacoes` e
persistir na base relacional.

---

## Entidades Referenciadas

| `entity.type` | Quando usar | `entity.id` |
|---|---|---|
| `espetaculo` | Eventos ligados a uma obra do catalogo | ID do espetaculo no PostgreSQL |
| `sessao` | Eventos ligados a uma sessao/data/horario especifico | ID da sessao no PostgreSQL |
| `teatro` | Eventos ligados a teatro/local | ID do teatro no PostgreSQL |
| `categoria` | Eventos ligados a filtro/categoria | ID da categoria no PostgreSQL |
| `busca` | Evento de busca textual | Hash ou identificador gerado para a busca, quando aplicavel |

O campo `entity` e opcional no payload geral, mas obrigatorio para eventos que
dependem de uma entidade de dominio, como `show_viewed` e `show_card_clicked`.

---

## Schema Logico da Collection `interacoes`

Cada documento representa um evento unico recebido pelo backend.

```json
{
  "_id": "ObjectId(...)",
  "event_id": "evt_01HX9F7M9XK2",
  "user_id": "a1b2c3d4-0000-0000-0000-000000000000",
  "anonymous_id": "anon_01HX9F7M8T",
  "session_id": "sess_01HX9F7M8T",
  "event_type": "show_viewed",
  "source": "mobile",
  "screen": "Detalhes",
  "occurred_at": "2026-06-09T14:32:00Z",
  "received_at": "2026-06-09T14:32:01Z",
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
  }
}
```

### Campos Raiz

| Campo | Tipo | Obrigatorio | Origem | Observacao |
|---|---|---|---|---|
| `_id` | `ObjectId` | Sim | MongoDB | Gerado automaticamente. |
| `event_id` | `string` | Sim | Cliente ou backend | Deve ser unico por evento quando enviado pelo cliente. |
| `user_id` | `string` | Condicional | JWT | Presente quando o usuario estiver autenticado. |
| `anonymous_id` | `string` | Condicional | Cliente | Obrigatorio quando nao houver `user_id`. |
| `session_id` | `string` | Sim | Cliente | Agrupa eventos de uma sessao do app. |
| `event_type` | `string` | Sim | Cliente | Deve pertencer ao catalogo de eventos. |
| `source` | `string` | Sim | Cliente | Valor inicial: `mobile`. |
| `screen` | `string` | Nao | Cliente | Nome da tela no app. |
| `occurred_at` | `date` | Sim | Cliente/backend | Momento em que o evento ocorreu. |
| `received_at` | `date` | Sim | Backend | Momento em que o backend recebeu o evento. |
| `entity` | `object` | Condicional | Cliente | Referencia a entidade de dominio. |
| `properties` | `object` | Nao | Cliente | Metadados livres e nao sensiveis do evento. |
| `device` | `object` | Nao | Cliente | Metadados nao sensiveis do dispositivo/app. |

Regra de identidade: todo documento deve ter pelo menos um dos campos `user_id`
ou `anonymous_id`.

---

## Propriedades por Evento

### `session_started`

```json
{
  "eventType": "session_started",
  "properties": {
    "entryPoint": "app_open"
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `entryPoint` | `string` | Nao | Ex.: `app_open`, `push_notification`, `deep_link`. |

### `screen_viewed`

```json
{
  "eventType": "screen_viewed",
  "screen": "Catalogo",
  "properties": {
    "routeName": "CatalogView"
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `routeName` | `string` | Sim | Nome tecnico da rota/tela. |

### `show_card_clicked`

```json
{
  "eventType": "show_card_clicked",
  "entity": { "type": "espetaculo", "id": "42" },
  "properties": {
    "origin": "catalog",
    "position": 3
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `origin` | `string` | Sim | Ex.: `catalog`, `recommendation`, `search`. |
| `position` | `number` | Nao | Posicao do item na lista/carrossel. |

### `show_viewed`

```json
{
  "eventType": "show_viewed",
  "entity": { "type": "espetaculo", "id": "42" },
  "properties": {
    "origin": "catalog",
    "durationMs": 23000
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `origin` | `string` | Nao | Origem da navegacao ate o detalhe. |
| `durationMs` | `number` | Nao | Tempo aproximado de permanencia, quando disponivel. |

### `search_performed`

```json
{
  "eventType": "search_performed",
  "entity": { "type": "busca", "id": "search_01HX9F7M" },
  "properties": {
    "query": "Hamlet",
    "resultsCount": 6
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `query` | `string` | Sim | Termo buscado. |
| `resultsCount` | `number` | Sim | Quantidade de resultados retornados. |

### `favorite_toggled`

```json
{
  "eventType": "favorite_toggled",
  "entity": { "type": "espetaculo", "id": "42" },
  "properties": {
    "active": true
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `active` | `boolean` | Sim | `true` para favoritado, `false` para removido. |

### `purchase_clicked`

```json
{
  "eventType": "purchase_clicked",
  "entity": { "type": "sessao", "id": "88" },
  "properties": {
    "provider": "ticket_partner",
    "sessionIdExternal": "abc123"
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `provider` | `string` | Nao | Provedor/ticketeira de destino. |
| `sessionIdExternal` | `string` | Nao | ID externo da sessao, quando houver. |

### `session_ended`

```json
{
  "eventType": "session_ended",
  "properties": {
    "durationMs": 420000,
    "reason": "background_timeout"
  }
}
```

| Propriedade | Tipo | Obrigatoria | Observacao |
|---|---|---|---|
| `durationMs` | `number` | Sim | Duracao aproximada da sessao. |
| `reason` | `string` | Nao | Ex.: `app_closed`, `logout`, `background_timeout`. |

---

## Regras de Privacidade

- Nao armazenar CPF, telefone, email, endereco completo ou tokens em `properties`.
- Nao armazenar conteudo livre que possa conter dados sensiveis sem sanitizacao.
- `device` deve conter apenas metadados tecnicos nao sensiveis, como plataforma,
  versao do app, versao do SO e modelo generico.
- `anonymous_id` deve ser um identificador tecnico do app, sem informacao pessoal.

---

## Indices MongoDB

O servico de usuario tenta criar os indices abaixo de forma idempotente no
startup, quando `MONGODB_CONNECTION_STRING`, `MONGODB_DATABASE` e
`MONGODB_TELEMETRY_COLLECTION` estiverem configurados. A implementacao fica em
`MongoTelemetryIndexInitializer` e usa os mesmos nomes de campos gravados na
collection `interacoes`. Se o MongoDB/Atlas estiver indisponivel no startup, o
servico registra warning e continua subindo; o readiness continua responsavel
por expor a indisponibilidade operacional.

| Indice | Campos | Objetivo |
|---|---|---|
| `ux_event_id` | `{ event_id: 1 }` unique sparse | Evitar duplicidade quando o cliente enviar `eventId`. |
| `ix_user_time` | `{ user_id: 1, occurred_at: -1 }` | Consultar historico recente de usuario autenticado. |
| `ix_anonymous_time` | `{ anonymous_id: 1, occurred_at: -1 }` | Consultar historico anonimo antes de login. |
| `ix_entity_time` | `{ "entity.type": 1, "entity.id": 1, occurred_at: -1 }` | Agregar eventos por espetaculo, sessao, teatro ou busca. |
| `ix_event_type_time` | `{ event_type: 1, occurred_at: -1 }` | Medir volume por tipo de evento. |
| `ix_session_time` | `{ session_id: 1, occurred_at: 1 }` | Reconstruir jornada dentro de uma sessao. |

### Validacao operacional

Para validar os indices no Atlas ou em um MongoDB acessivel via `mongosh`:

```bash
mongosh "$MONGODB_CONNECTION_STRING" --eval "db.getSiblingDB('$MONGODB_DATABASE').getCollection('$MONGODB_TELEMETRY_COLLECTION').getIndexes()"
```

Resultado esperado: alem do indice `_id_` criado pelo MongoDB, a collection deve
listar `ux_event_id`, `ix_user_time`, `ix_anonymous_time`, `ix_entity_time`,
`ix_event_type_time` e `ix_session_time`.

Se a criacao de `ux_event_id` falhar por documentos legados duplicados em
`event_id`, a collection deve ser saneada antes de reexecutar o servico. Em
ambientes novos, o endpoint sempre normaliza um `event_id`, gerado pelo backend
quando o cliente nao informa.

---

## Criterios de Pronto para T15/T16

- O catalogo inicial de eventos esta definido com motivacao de coleta.
- Cada evento tem entidade esperada e propriedades minimas.
- O schema logico da collection `interacoes` esta definido.
- O contrato HTTP usa os mesmos nomes de eventos e campos do schema MongoDB.
- As proximas tasks podem implementar endpoint, validacao, indices e frontend sem
  decidir novamente a taxonomia de telemetria.
