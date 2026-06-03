-- 05_timestamptz.sql — Migra colunas TIMESTAMP (sem fuso) para TIMESTAMPTZ.
--
-- Contexto: o schema inicial (01_create_database.sql) declarou todas as colunas
-- temporais como TIMESTAMP. Os valores existentes foram gravados em UTC, que é
-- o default do container postgres:15-alpine e do Supabase cloud (nenhum TZ é
-- aplicado em runtime). Esta migration converte cada coluna para TIMESTAMPTZ
-- interpretando o valor existente como UTC.
--
-- Os DEFAULTs continuam usando NOW(), que já é timezone-aware — nenhuma
-- alteração de default é necessária.
--
-- Validação após aplicar:
--   SELECT table_name, column_name
--   FROM information_schema.columns
--   WHERE table_schema = 'public'
--     AND data_type = 'timestamp without time zone'
--   ORDER BY table_name, column_name;
-- Esperado: zero linhas retornadas.

BEGIN;

ALTER TABLE ator              ALTER COLUMN created_at    TYPE TIMESTAMPTZ USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE avaliacao         ALTER COLUMN created_at    TYPE TIMESTAMPTZ USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE espetaculo        ALTER COLUMN created_at    TYPE TIMESTAMPTZ USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE espetaculo        ALTER COLUMN updated_at    TYPE TIMESTAMPTZ USING updated_at    AT TIME ZONE 'UTC';
ALTER TABLE recomendacao      ALTER COLUMN data_geracao  TYPE TIMESTAMPTZ USING data_geracao  AT TIME ZONE 'UTC';
ALTER TABLE sessao            ALTER COLUMN created_at    TYPE TIMESTAMPTZ USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE similaridade_item ALTER COLUMN data_geracao  TYPE TIMESTAMPTZ USING data_geracao  AT TIME ZONE 'UTC';
ALTER TABLE teatro            ALTER COLUMN created_at    TYPE TIMESTAMPTZ USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE usuario           ALTER COLUMN data_cadastro TYPE TIMESTAMPTZ USING data_cadastro AT TIME ZONE 'UTC';

COMMIT;
