#!/usr/bin/env bash
# ===========================================================================
# Teste de carga do banco com pgbench (T9).
# ---------------------------------------------------------------------------
# Executa dois cenários sobre um banco já populado com a massa sintética
# (src/database/load_test/massa_carga.sql) e com as migrations 04–09 aplicadas:
#   1) catalogo_leitura.sql  — carga de leitura (navegação de catálogo)
#   2) avaliacao_escrita.sql — carga de escrita (UPSERT + trigger de auditoria)
#
# Pré-requisitos (DoR): massa carregada, banco no ar, pgbench instalado.
#
# Uso:
#   DATABASE_URL="postgres://user:pass@host:porta/db" \
#   CLIENTS=20 THREADS=4 DURATION=30 \
#   bash src/database/load_test/run_pgbench.sh
#
# Variáveis (com defaults):
#   DATABASE_URL  conexão libpq (obrigatória)
#   CLIENTS       conexões concorrentes        (default 20)
#   THREADS       threads do pgbench           (default 4)
#   DURATION      duração de cada cenário (s)  (default 30)
# ===========================================================================
set -euo pipefail

CLIENTS="${CLIENTS:-20}"
THREADS="${THREADS:-4}"
DURATION="${DURATION:-30}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/pgbench"

if [[ -z "${DATABASE_URL:-}" ]]; then
    echo "ERRO: defina DATABASE_URL." >&2
    exit 1
fi

run_scenario() {
    local nome="$1" arquivo="$2"
    echo "============================================================"
    echo ">>> Cenário: ${nome}  (clients=${CLIENTS}, threads=${THREADS}, ${DURATION}s)"
    echo "============================================================"
    pgbench -n -r \
        -c "${CLIENTS}" -j "${THREADS}" -T "${DURATION}" \
        -f "${SCRIPT_DIR}/${arquivo}" \
        "${DATABASE_URL}"
    echo
}

run_scenario "Catálogo (leitura)" "catalogo_leitura.sql"
run_scenario "Avaliação (escrita + auditoria)" "avaliacao_escrita.sql"
