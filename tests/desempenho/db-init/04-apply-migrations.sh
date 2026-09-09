#!/usr/bin/env bash
# Aplica as migrations 04-09 forward (mesma cobertura usada pelo pgbench).
# As migrations 10+ tocam features especificas do Supabase (storage, RLS, app_metadata)
# que nao existem no Postgres local — sao puladas porque o backend bypassa RLS.
set -euo pipefail

cd /migrations

FORWARD_FILES=$(ls 0[4-9]_*.sql 2>/dev/null | grep -v '_rollback' | sort)

for file in $FORWARD_FILES; do
  echo "[init] aplicando migration: $file"
  psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
       --set ON_ERROR_STOP=1 --file "$file"
done

echo "[init] migrations 04-09 aplicadas."
