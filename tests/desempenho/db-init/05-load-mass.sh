#!/usr/bin/env bash
# Carrega a massa sintetica (~208k linhas) usada pelo pgbench.
# Garante que o catalogo, sessoes, avaliacoes e recomendacoes
# tenham volume representativo para os testes de carga.
set -euo pipefail

echo "[init] carregando massa sintetica..."
psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
     --set ON_ERROR_STOP=1 --file /massa_carga.sql

echo "[init] massa carregada. Counts:"
psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -tA -c "
  SELECT 'espetaculo='  || (SELECT COUNT(*) FROM espetaculo)
      || ' sessao='     || (SELECT COUNT(*) FROM sessao)
      || ' avaliacao='  || (SELECT COUNT(*) FROM avaliacao)
      || ' recomendacao='|| (SELECT COUNT(*) FROM recomendacao);
"
