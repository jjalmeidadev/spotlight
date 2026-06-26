# Testes de Desempenho — Spotlight

Suite de testes de carga para validar os RNFs de desempenho da plataforma em
ambiente isolado (Postgres + Mongo locais), sem tocar a infraestrutura
remota da Supabase ou MongoDB Atlas.

## Cobertura

| RNF | Descricao resumida | Cenario que valida |
|-----|--------------------|--------------------|
| RNF02 | Recomendacao personalizada em ate 2s | `k6/02-recomendacao-rnf02.js` |
| RNF06 | 200 usuarios simultaneos, media ≤ 3s, erro ≤ 1% | `k6/03-carga-rnf06.js` |
| RNF07 | Logs estruturados ≤ 1s (verificacao indireta via container logs) | logs colhidos em `resultados/<timestamp>/containers.log` |
| RNF09 | Telemetria assincrona sem aumentar tempo de resposta | comparacao entre `02-` e `03-` |
| Banco | Throughput leitura/escrita catalogo + avaliacao | `pgbench` (mesma carga do baseline 2026-06-09) |

## Pre-requisitos

- Docker Desktop ativo (socket em `/var/run/docker.sock`).
- 4+ GB de RAM livre.
- Portas 5000, 5433 e 27018 livres no host.

## Como rodar (rodada completa)

```bash
cd tests/desempenho
./run.sh
```

A saida fica em `resultados/<timestamp>/` com:

- `*.summary.json` — sumario k6 (counters, trends, taxas)
- `*.events.json`  — eventos brutos do k6
- `*.stdout.log`   — stdout legivel
- `pgbench.txt`    — TPS e latencia por cenario pgbench
- `containers.log` — ultimas 200 linhas de logs por servico
- `pg_stat_statements_top.txt` — 25 queries mais lentas

## Rodadas individuais

```bash
docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml up -d --build
docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml \
  run --rm -v $(pwd)/resultados:/out k6 run /scripts/02-recomendacao-rnf02.js
```

## Variaveis de tuning

Sobrescreva via env do shell antes do `./run.sh`:

| Var | Default | Efeito |
|-----|---------|--------|
| `LOAD_VUS`        | 200   | n.º de VUs no teste de carga RNF06 |
| `LOAD_DURATION`   | `10m` | duracao no pico |
| `LOAD_RAMP_UP`    | `1m`  | rampa de subida |
| `LOAD_RAMP_DOWN`  | `30s` | rampa de descida |

## Como o JWT eh montado

Sem `/login` no backend (auth e delegada para Supabase), o k6 assina JWTs HS256
localmente em `k6/lib/jwt.js`. O backend (`AuthSetup.cs`) valida com o segredo
`SUPABASE_JWT_SECRET` injetado pelo `.env.loadtest`. Os `sub` (UUIDs) sao os
mesmos do seed `02_initial_data.sql`, garantindo que o servico de recomendacao
encontre o usuario por `id_usuario`.

## Limpeza

```bash
docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml down -v
```
