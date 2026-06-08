# Enriquecimento geográfico das recomendações (RF04 + RNF03)

> Documenta a mudança de produção introduzida na branch
> `test/integration-recomendacao-geo` (Ponderada Aula 8 — Testes de Integração SOA).

## 1. O que mudou

O endpoint `GET /recomendacao/me` passou a **enriquecer** cada recomendação com um
campo opcional `contextoGeografico` (rótulo de cidade/região, ex.: `"São Paulo"`).

Fluxo novo dentro de [`ListarRecomendacoesMeUseCase`](../application/usecases/ListarRecomendacoesMeUseCase.cs):

1. Lê as recomendações do PostgreSQL (comportamento original — `recomendacao JOIN espetaculo`).
2. Lê a **localização consentida** do usuário (`usuario.localizacao`, JSONB) via
   [`LocalizacaoUsuarioRepository`](../infrastructure/persistence/LocalizacaoUsuarioRepository.cs).
3. Se houver coordenadas, faz **reverse geocoding** numa API externa
   (OpenStreetMap/Nominatim) via [`NominatimGeocodingClient`](../infrastructure/geocoding/NominatimGeocodingClient.cs)
   e anexa o rótulo resultante a cada item.

## 2. Por que essa mudança existe

A atividade exige um **único fluxo** que combine, no mesmo teste de integração:

- uma **dependência possuída** — o PostgreSQL do serviço (Testcontainers); e
- uma **dependência consumida** — uma API HTTP externa (WireMock.Net).

Até então, `develop` não tinha nenhum fluxo que combinasse Postgres + chamada HTTP
externa no mesmo serviço (`recomendacao` lê do banco mas não chamava nada externo;
`geolocalizacao` chama o Nominatim mas não usa banco). Esta mudança cria esse fluxo
combinado real, alinhado ao design já previsto (recomendação enriquecida com
contexto geográfico).

A mudança é **aditiva e defensiva**: não altera o contrato existente (campo novo é
opcional/anulável) e não muda o comportamento quando o enriquecimento não está
disponível.

## 3. Classificação das dependências

| Dependência | Tipo | Controle do grupo | No teste |
|---|---|---|---|
| PostgreSQL (`recomendacao`, `espetaculo`, `usuario`) | **Possuída** | Schema, migrations e deploy são do grupo | `Testcontainers` (`supabase/postgres:15.1.0.147`) |
| API de geocodificação (OpenStreetMap/Nominatim) | **Consumida** | Terceiro externo, sem controle de uptime/contrato | `WireMock.Net` (stub HTTP) |

## 4. Configuração

Nova variável (lida via `IConfiguration`, com fallback de env var):

| Chave | Default | Descrição |
|---|---|---|
| `GEOCODING_BASE_URL` | `https://nominatim.openstreetmap.org` | Base URL do serviço de reverse geocoding |
| `GEOCODING_USER_AGENT` | `spotlight-inteli-g04/1.0` | User-Agent exigido pela política do Nominatim |

> As leituras de `DATABASE_CONNECTION_STRING` e `SUPABASE_JWT_SECRET` foram migradas
> de `Environment.GetEnvironmentVariable(...)` para `IConfiguration[...]`. Em produção
> o comportamento é idêntico (a configuração agrega as variáveis de ambiente); a
> mudança permite que os testes de integração injetem valores determinísticos e não
> vazem para um `.env` local apontando para o Supabase de produção.

## 5. Degradação graciosa — evidência do RNF03 (Confiabilidade)

O reverse geocoding é **best effort**. Em **qualquer** falha — dependência não
configurada, usuário sem localização, status não-2xx, timeout (HttpClient com
timeout de 3s) ou exceção de rede — o caso de uso captura, registra um *warning* e
**retorna as recomendações normalmente, com `contextoGeografico = null`**. O
endpoint **nunca** propaga 5xx por causa do geocoder.

Isso sustenta diretamente o **RNF03 — Disponibilidade mínima de 99% (Confiabilidade)**
(`documentos/Projeto.md` §3.2 e §9.2.1.3): a indisponibilidade de um serviço externo
não compromete a funcionalidade central de descoberta de espetáculos (RF04).

O cenário de erro é coberto pelo teste de integração
`DeveRetornarRecomendacoesSemContextoQuandoGeocoderIndisponivel`.

## 6. Compatibilidade

As dependências de enriquecimento são injetadas como parâmetros **opcionais** do
caso de uso. Os testes unitários existentes, que constroem
`new ListarRecomendacoesMeUseCase(repositorio)`, continuam compilando e passando sem
alteração — o enriquecimento simplesmente vira NO-OP nesse caminho.
