-- 05_timestamptz_rollback.sql — Reverte TIMESTAMPTZ para TIMESTAMP (sem fuso).
--
-- AVISO: a reversão remove a informação de fuso horário das colunas. Cada valor
-- é convertido para o instante equivalente em UTC e armazenado como TIMESTAMP
-- (sem fuso). Após o rollback, qualquer cliente que ler estas colunas precisa
-- assumir UTC explicitamente — caso contrário interpretará o valor como
-- horário local e produzirá leituras incorretas em fusos diferentes de UTC.
--
-- Use apenas se a migration 05_timestamptz.sql precisar ser desfeita em um
-- ambiente que ainda não tenha aplicações dependendo de TIMESTAMPTZ.
--
-- Os DEFAULTs (NOW()) continuam válidos para TIMESTAMP sem alteração.

BEGIN;

ALTER TABLE ator              ALTER COLUMN created_at    TYPE TIMESTAMP USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE avaliacao         ALTER COLUMN created_at    TYPE TIMESTAMP USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE espetaculo        ALTER COLUMN created_at    TYPE TIMESTAMP USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE espetaculo        ALTER COLUMN updated_at    TYPE TIMESTAMP USING updated_at    AT TIME ZONE 'UTC';
ALTER TABLE recomendacao      ALTER COLUMN data_geracao  TYPE TIMESTAMP USING data_geracao  AT TIME ZONE 'UTC';
ALTER TABLE sessao            ALTER COLUMN created_at    TYPE TIMESTAMP USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE similaridade_item ALTER COLUMN data_geracao  TYPE TIMESTAMP USING data_geracao  AT TIME ZONE 'UTC';
ALTER TABLE teatro            ALTER COLUMN created_at    TYPE TIMESTAMP USING created_at    AT TIME ZONE 'UTC';
ALTER TABLE usuario           ALTER COLUMN data_cadastro TYPE TIMESTAMP USING data_cadastro AT TIME ZONE 'UTC';

COMMIT;
