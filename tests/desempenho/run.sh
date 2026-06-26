#!/usr/bin/env bash
# Orquestra a rodada de desempenho: sobe stack, espera saude, dispara k6
# para cada cenario e arquiva os resultados em resultados/.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

COMPOSE=(docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml)

STAMP="$(date +%Y%m%d-%H%M%S)"
OUT="resultados/${STAMP}"
mkdir -p "$OUT"

log() { echo -e "\n=== $* ==="; }

log "subindo stack (db, mongo, services, gateway)"
"${COMPOSE[@]}" up -d --build db mongo
"${COMPOSE[@]}" up -d --build catalogo usuario recomendacao geolocalizacao
"${COMPOSE[@]}" up -d --build gateway

log "aguardando gateway responder em /health"
for i in $(seq 1 60); do
  if curl -fsS http://localhost:5000/health >/dev/null 2>&1; then
    echo "gateway ok"; break
  fi
  sleep 5
  if [[ $i -eq 60 ]]; then
    echo "gateway nao subiu em 5 minutos. Logs:" >&2
    "${COMPOSE[@]}" logs --tail=100 gateway recomendacao usuario catalogo >&2
    exit 1
  fi
done

run_k6() {
  local nome="$1" arquivo="$2"
  shift 2
  log "k6 -> $nome"
  "${COMPOSE[@]}" run --rm \
    -v "$SCRIPT_DIR/$OUT":/out \
    k6 run \
    --summary-export "/out/${nome}.summary.json" \
    --out "json=/out/${nome}.events.json" \
    "$@" "/scripts/${arquivo}" \
    | tee "$OUT/${nome}.stdout.log"
}

run_k6 smoke              01-smoke.js
run_k6 rnf02-recomendacao 02-recomendacao-rnf02.js
run_k6 rnf06-carga        03-carga-rnf06.js
run_k6 similares-publico  04-similares-publico.js

log "rodando pgbench (DB layer)"
DB_URL="postgresql://postgres:${POSTGRES_PASSWORD:-spotlight_loadtest}@localhost:5433/postgres"
PGBENCH_OUT="$OUT/pgbench.txt"
docker run --rm --network spotlight-loadtest_loadtest_net \
  -v "$SCRIPT_DIR/../../src/database/load_test/pgbench:/pgbench:ro" \
  postgres:16-alpine bash -lc '
    set -e
    DB_URL="postgresql://postgres:'"${POSTGRES_PASSWORD:-spotlight_loadtest}"'@db:5432/postgres"
    for sql in catalogo_leitura.sql avaliacao_escrita.sql; do
      echo "########## pgbench: $sql ##########"
      pgbench -n -r -c 20 -j 4 -T 30 -f "/pgbench/$sql" "$DB_URL"
      echo
    done
  ' | tee "$PGBENCH_OUT"

log "coletando logs e metricas finais"
"${COMPOSE[@]}" logs --tail=200 gateway recomendacao usuario catalogo db > "$OUT/containers.log" 2>&1 || true

log "snapshot pg_stat_statements"
docker compose --env-file .env.loadtest -f docker-compose.loadtest.yml exec -T db \
  psql -U postgres -d postgres -c "
    CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
    SELECT calls, mean_exec_time, max_exec_time, rows, substr(query,1,200) AS query
      FROM pg_stat_statements
      ORDER BY mean_exec_time DESC
      LIMIT 25;
  " > "$OUT/pg_stat_statements_top.txt" 2>&1 || true

echo "resultados em $OUT"
