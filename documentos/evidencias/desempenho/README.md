# Evidências — Testes de Desempenho

**Execução:** 2026-06-25, 21:10–21:30 BRT.
**Ambiente:** Windows 11 + WSL2 (Ubuntu), Docker Desktop 4.77.0 (engine 29.5.3),
12 vCPU, 7.6 GiB RAM, host único (sem cluster).
**Stack sob teste:** API Gateway + serviços `catalogo`, `usuario`, `recomendacao`,
`geolocalizacao` (release builds), PostgreSQL 16-alpine local, MongoDB 7.
**Massa de dados:** 2.015 espetáculos, 20.020 sessões, 100.100 avaliações,
50.024 recomendações (gerada por `src/database/load_test/massa_carga.sql`).
**Ferramentas:** k6 0.49 (API) + pgbench 16.13 (banco).

## Resumo

| Cenário | RNFs validados | Resultado | Observação |
|---|---|---|---|
| Smoke (1 VU / 30s)         | sanity         | 174/174 OK, p95=10ms     | Pré-validação |
| Recomendação focada        | **RNF02**      | 100% ≤ 2s, p95=6ms       | 1.755 requests |
| Carga sustentada 200 VUs   | **RNF06**      | 0 erros, média=106ms, p95=397ms | 407.625 requests em 10 min |
| Similares (público)        | RNF06 público  | 0 erros, p95=143ms       | 57.990 requests em 1min45s |
| pgbench catálogo (leitura) | banco          | 984 TPS, 0 falhas        | 20 clientes, 30s |
| pgbench avaliação (escrita+auditoria) | banco | 1.306 TPS, 0 falhas | 20 clientes, 30s |

Veja `relatorio.md` para análise completa e melhorias propostas.

## Estrutura

```
desempenho/
├── README.md                 — este arquivo
├── relatorio.md              — análise, comparação com RNFs e melhorias
└── medicoes/
    └── 20260625-211001/
        ├── smoke.summary.json
        ├── smoke.stdout.log
        ├── rnf02-recomendacao.summary.json
        ├── rnf02-recomendacao.stdout.log
        ├── rnf06-carga.summary.json
        ├── rnf06-carga.stdout.log
        ├── similares-publico.summary.json
        ├── similares-publico.stdout.log
        ├── pgbench.txt
        ├── pg_stat_database.txt
        ├── docker_stats.txt
        └── containers.log
```

## Como reproduzir

A suíte vive em `tests/desempenho/`. A partir da raiz do repositório, com o
Docker Desktop em execução:

```bash
cd tests/desempenho
./run.sh
```

O `run.sh` faz, em sequência:

1. Sobe Postgres + Mongo locais (compose `docker-compose.loadtest.yml`), aplica
   `00_supabase_compat.sql` + `01_create_database.sql` + `02_initial_data.sql`,
   roda forward das migrations 04–09 e carrega `massa_carga.sql`.
2. Sobe os 5 serviços .NET em build `runtime` apontando para o Postgres local.
3. Roda os 4 scripts k6: `01-smoke.js`, `02-recomendacao-rnf02.js`,
   `03-carga-rnf06.js`, `04-similares-publico.js`.
4. Roda o `pgbench` (mesmos cenários de leitura/escrita do baseline 2026-06-09).
5. Coleta `pg_stat_statements` e logs dos containers em
   `tests/desempenho/resultados/<timestamp>/`.

Os arquivos consolidados aqui em `medicoes/` são uma cópia desse mesmo
`tests/desempenho/resultados/<timestamp>/`.

### Variáveis de tuning

| Variável | Default | Efeito |
|---|---|---|
| `LOAD_VUS`       | 200    | VUs de pico no cenário RNF06 |
| `LOAD_DURATION`  | `10m`  | Tempo de sustentação no pico |
| `LOAD_RAMP_UP`   | `1m`   | Rampa de subida |
| `LOAD_RAMP_DOWN` | `30s`  | Rampa de descida |

### Limpeza

```bash
cd tests/desempenho
docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml down -v
```

## Autenticação no ambiente de carga

O backend valida JWT HS256 contra o segredo em `SUPABASE_JWT_SECRET`. O ambiente
de carga injeta um segredo local exclusivo via `tests/desempenho/.env.loadtest`
(não é o mesmo do Supabase de produção). O k6 monta os tokens em
`tests/desempenho/k6/lib/jwt.js` usando UUIDs dos usuários de seed
(`02_initial_data.sql`), garantindo que o serviço de recomendação encontre o
usuário por `id_usuario` ao consultar a tabela `recomendacao`.
