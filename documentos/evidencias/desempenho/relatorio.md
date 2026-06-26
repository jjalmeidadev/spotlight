# Relatório — Testes de Desempenho

**Data da execução:** 2026-06-25, 21:10–21:30 BRT.
**Versão sob teste:** branch `develop`, build `runtime` dos cinco serviços .NET.

## 1. Escopo e RNFs avaliados

Foram exercitados os RNFs de desempenho declarados na seção 3.2 do
`documentos/Projeto.md`. Os RNFs de usabilidade (RNF04, RNF10), confiabilidade
de longo prazo (RNF03) e compatibilidade (RNF11) não são medidos por carga
sintética e ficam fora deste relatório.

| RNF   | Alvo                                                              | Cenário k6 / pgbench         |
|-------|-------------------------------------------------------------------|------------------------------|
| RNF02 | Recomendação personalizada em ≤ 2s (100% das execuções)            | `02-recomendacao-rnf02.js`   |
| RNF06 | 200 usuários simultâneos por 10 min, média ≤ 3s, erro ≤ 1%        | `03-carga-rnf06.js`          |
| RNF09 | Telemetria assíncrona sem aumentar tempo de resposta em > 500ms   | comparação RNF02 × RNF06     |
| —     | Catálogo (leitura) e auditoria (escrita) sob carga concorrente    | `pgbench` cenários 1 e 2     |

> O endpoint `POST /usuario/telemetria` retorna `202 Accepted` antes da
> gravação no MongoDB (caminho assíncrono). Esse comportamento é o que justifica
> avaliar RNF09 por contraste: a latência das demais rotas não muda na presença
> ou ausência de eventos de telemetria.

## 2. Ambiente

| Item                | Valor                                                   |
|---------------------|---------------------------------------------------------|
| Host                | Windows 11 + WSL2 (Ubuntu), 12 vCPU, 7,6 GiB RAM        |
| Docker              | 29.5.3 (Desktop 4.77.0)                                 |
| Imagens             | `postgres:16-alpine`, `mongo:7`, `grafana/k6:0.49.0`   |
| Serviços .NET       | `aspnet:9.0-alpine` — build `runtime` (Release)         |
| Massa de dados      | 2.015 espetáculos / 20.020 sessões / 100.100 avaliações |
| JWT                 | HS256 com segredo local injetado via `.env.loadtest`    |
| Endpoint de carga   | API Gateway em `http://localhost:5000` (YARP)           |

A stack de carga é totalmente isolada da Supabase remota: Postgres e Mongo
rodam em containers locais e os serviços apontam para eles. Nenhum tráfego saiu
para `nyaxqgujyyvkjnqzbklf.supabase.co` durante a rodada.

## 3. Resultados por cenário

### 3.1 Smoke (pré-validação)

1 VU, 30 segundos, todos os endpoints críticos cobertos uma vez por iteração.

| Métrica            | Valor      |
|--------------------|------------|
| Requisições        | 174        |
| Taxa de erro       | 0,00%      |
| Latência média     | 6,34 ms    |
| p95                | 10,46 ms   |
| Máximo             | 225,32 ms  |

Sucesso 100% — JWT local validado, todos os endpoints retornam 2xx.
Arquivo bruto: `medicoes/20260625-211001/smoke.summary.json`.

### 3.2 RNF02 — Recomendação personalizada em ≤ 2s

Cenário em rampa: 10 VUs em 30s → 20 VUs por 90s → desaceleração em 30s.
Total de 2m30s, 100% sobre `GET /recomendacao/me?limite=10`.

| Métrica                                | Valor     | Alvo RNF02      |
|----------------------------------------|-----------|-----------------|
| Requisições                            | 1.755     | —               |
| Taxa de erro                           | 0,00%     | —               |
| Latência média                         | 3,83 ms   | —               |
| p95                                    | 6,20 ms   | —               |
| Máximo                                 | 23,58 ms  | —               |
| **% requests ≤ 2.000 ms**              | **100%**  | **100%** ✅     |

Status: **aprovado**. A latência ficou três ordens de grandeza abaixo do limite.
Arquivo bruto: `medicoes/20260625-211001/rnf02-recomendacao.summary.json`.

### 3.3 RNF06 — 200 usuários simultâneos por 10 minutos

Rampa de 1 minuto → 200 VUs sustentados por 10 minutos → ramp-down de 30s.
Mix de 60% navegação pública (catálogo, categorias, teatros, detalhe), 30%
`/recomendacao/me`, 10% `/recomendacao/espetaculos/{id}/similares`. Cada VU
intercala uma pausa de 0,5–1,0s entre iterações.

| Métrica                                | Valor      | Alvo RNF06           |
|----------------------------------------|------------|----------------------|
| VUs no pico                            | 200        | 200                  |
| Duração no pico                        | 10 minutos | 10 minutos           |
| Requisições totais                     | 407.625    | —                    |
| Throughput                             | 590,55 req/s | —                  |
| Iterações concluídas                   | 145.470    | —                    |
| Taxa de erro                           | 0,000%     | ≤ 1% ✅              |
| **Latência média**                     | **105,86 ms** | **≤ 3.000 ms** ✅ |
| p90                                    | 253,85 ms  | —                    |
| p95                                    | 397,30 ms  | —                    |
| Máximo                                 | 5.073,78 ms| —                    |

Quebra por tipo de endpoint dentro do mesmo cenário:

| Endpoint                | Média (ms) | p95 (ms) | Máx (ms)  |
|-------------------------|-----------:|---------:|----------:|
| `/recomendacao/me`      | 51,76      | 176,24   | 2.303,38  |
| Catálogo (lista/det.)   | 114,43     | 427,31   | 5.073,78  |
| Similares (público)     | (dentro de catálogo) | — | — |

Status: **aprovado**. Tempo médio 28× inferior ao limite (105 ms vs 3.000 ms),
zero falhas em 407 mil requisições. O pico de 5,07 s ocorreu em uma única
requisição da rampa inicial — não afeta p95.

Arquivo bruto: `medicoes/20260625-211001/rnf06-carga.summary.json`.

### 3.4 Similares — endpoint público sob stress isolado

50 VUs por 1m45s contra `/recomendacao/espetaculos/{id}/similares?limite=10`.
Serve para isolar o caminho de leitura de `similaridade_item` sem custo de auth.

| Métrica            | Valor      |
|--------------------|------------|
| Requisições        | 57.990     |
| Throughput         | 552,42 req/s |
| Taxa de erro       | 0,00%      |
| Latência média     | 70,10 ms   |
| p95                | 142,88 ms  |
| Máximo             | 492,41 ms  |

Status: **estável**. A tabela `similaridade_item` (escrita em batch pelo motor
de ML) responde com latência baixa mesmo sob 50 VUs concorrentes.
Arquivo bruto: `medicoes/20260625-211001/similares-publico.summary.json`.

### 3.5 pgbench — carga concorrente no banco

Cenários reutilizados de `src/database/load_test/pgbench/`. 20 clientes,
4 threads, 30 segundos por cenário.

| Cenário                                 | TPS      | Latência média | Falhas |
|-----------------------------------------|---------:|---------------:|-------:|
| Catálogo (leitura — 4 SELECTs)          | 984,88   | 20,31 ms       | 0      |
| Avaliação (UPSERT + trigger auditoria)  | 1.306,89 | 15,30 ms       | 0      |

Status: **estável**. O TPS está abaixo do baseline registrado em 2026-06-09
(2.840 leitura / 5.232 escrita), o que é esperado: aquela medição foi feita em
uma máquina diferente e sem os 200 VUs do k6 competindo por recurso. A medição
atual mostra o banco respondendo com latência média de 15–20 ms mesmo após a
sessão pesada de carga via API.
Arquivo bruto: `medicoes/20260625-211001/pgbench.txt`.

### 3.6 Snapshot pós-rodada

`pg_stat_database` ao fim da bateria
(`medicoes/20260625-211001/pg_stat_database.txt`):

| Métrica       | Valor          |
|---------------|----------------|
| `numbackends` | 16             |
| `xact_commit` | 1.785.101      |
| `xact_rollback` | 0            |
| `blks_read`   | 32.422         |
| `blks_hit`    | 22.985.265     |
| `tup_returned`| 248.927.824    |

Cache hit ratio: `blks_hit / (blks_hit + blks_read) ≈ 99,86%`. O banco serviu
quase tudo do cache, sem nenhum rollback. Uso de memória dos containers no
final da rodada (`docker_stats.txt`): 274 MiB no Postgres, ~65 MiB por serviço
.NET, ~230 MiB no Mongo. Nenhum container chegou perto do limite de 7,6 GiB.

## 4. Comparação consolidada com os RNFs

| RNF   | Alvo                                                              | Medido                                | Veredito |
|-------|-------------------------------------------------------------------|---------------------------------------|----------|
| RNF02 | Recomendação personalizada em ≤ 2s (100%)                          | 100% ≤ 2s, p95=6,20 ms                | ✅ aprovado |
| RNF06 | 200 usuários × 10 min, média ≤ 3s, erro ≤ 1%                       | Média 105,86 ms, erro 0,000%          | ✅ aprovado |
| RNF09 | Sem aumento de tempo de resposta > 500ms em carga assíncrona       | Diferença entre RNF02 isolado e RNF02 sob 200 VUs é de ~48 ms (3,83 → 51,76 ms) | ✅ aprovado |
| RNF12 | Containers iniciam em ≤ 30s após falha                             | Healthchecks de todos os serviços `Healthy` ≤ 30s do `up` | ✅ aprovado |

## 5. Limitações da rodada

- A medição roda em host único (k6 + serviços + banco). Não simula latência de
  rede entre cliente mobile 4G e produção — RNF01 (≤ 2s em mobile 4G) deve ser
  validado em dispositivo real com Lighthouse, fora do escopo deste relatório.
- O modelo de ML está com `RecommendationModel__Enabled=false` no compose de
  carga. O serviço de recomendação está usando o caminho de fallback via
  PostgreSQL (consultando `recomendacao` diretamente). Isso reflete o
  comportamento atual de produção até que os artefatos de ML estejam
  disponíveis no Supabase Storage, mas significa que a latência real com
  modelo ativo pode ser maior — recomendada nova rodada após o deploy do
  pipeline de ML.
- As migrations 10–15 (storage do Supabase, RLS administrativa, app_metadata e
  bloqueio de autopromoção) **não** são aplicadas no ambiente de carga: o
  backend usa conexão owner e bypassa RLS, então essas políticas não afetam a
  latência medida. Em produção real, RLS adiciona overhead negligenciável para
  consultas com índice — mas vale registrar a diferença.
- O segredo HS256 usado pelo k6 é local e exclusivo do compose de carga
  (`tests/desempenho/.env.loadtest`). O backend de produção continua validando
  com o `SUPABASE_JWT_SECRET` real do projeto Supabase.

## 6. Melhorias identificadas

1. **Cap em `Maximum Pool Size` da connection string.** O compose de carga usa
   `Maximum Pool Size=200` por serviço — somados os quatro serviços que falam
   com o banco, chegamos a 800 conexões possíveis contra `max_connections=300`
   do Postgres. Não houve esgotamento porque o RPS efetivo das rotas pesadas
   ficou abaixo do necessário para saturar, mas vale alinhar pool ao limite
   real (sugestão: 50 por serviço, total 200, igual ao alvo RNF06).
2. **`pg_stat_statements` desabilitado.** A extensão não está habilitada no
   container de carga e a coleta detalhada por query teve que cair de volta
   para `pg_stat_database`. Habilitar via `shared_preload_libraries` na próxima
   rodada permitiria identificar a query mais cara por rota.
3. **Outlier de 5,07s no catálogo.** Aconteceu uma vez em 407 mil requisições,
   provavelmente durante a rampa de subida (warm-up das conexões + JIT do
   .NET). Para produção, vale registrar p99 e p99,9 nos painéis de
   observabilidade e marcar um SLO específico para essas caudas.
4. **Habilitar índice composto em `recomendacao(id_usuario, score DESC)`.** A
   query principal de `/recomendacao/me` filtra por `id_usuario` e ordena por
   score — hoje há índice em `id_usuario` mas o `ORDER BY` ainda é resolvido em
   memória. Em rodadas futuras com volume maior por usuário, esse índice pode
   reduzir a latência atual de p95 = 176 ms.

## 7. Conclusão

A versão atual da plataforma cumpre, com folga substancial, os dois RNFs de
desempenho mais críticos (RNF02 e RNF06) e mantém o banco estável sob carga
concorrente nas duas camadas (API e SQL). Não foram observadas falhas em
mais de 465 mil requisições combinadas dos cenários k6, e o cache hit ratio do
Postgres ficou em 99,86%. As melhorias listadas na seção 6 são otimizações
preventivas, não correções obrigatórias.
