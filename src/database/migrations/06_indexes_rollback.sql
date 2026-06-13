-- 06_indexes_rollback.sql — Reverte 06_indexes.sql.
--
-- AVISO sobre a extensão pg_trgm: o DROP EXTENSION abaixo só deve ser
-- executado se a extensão foi adicionada por 06_indexes.sql. Se ela já
-- existia no ambiente antes da migration (por exemplo, habilitada pelo
-- Supabase ou usada por outro objeto do banco), comente a linha
-- correspondente — caso contrário o rollback vai derrubar funcionalidade
-- de outros componentes que dependem da extensão.
--
-- Validação após o rollback:
--   SELECT indexname FROM pg_indexes
--   WHERE schemaname = 'public'
--     AND indexname IN (
--       'idx_espetaculo_titulo_trgm',
--       'idx_espetaculo_genero',
--       'idx_espetaculo_status_popularidade',
--       'idx_sessao_espetaculo_data_horario'
--     );
-- Esperado: zero linhas retornadas.

BEGIN;

DROP INDEX IF EXISTS idx_sessao_espetaculo_data_horario;
DROP INDEX IF EXISTS idx_espetaculo_status_popularidade;
DROP INDEX IF EXISTS idx_espetaculo_genero;
DROP INDEX IF EXISTS idx_espetaculo_titulo_trgm;

-- Só dropar se a extensão foi adicionada por 06_indexes.sql.
-- Comente esta linha se pg_trgm já existia antes (caso comum no Supabase).
DROP EXTENSION IF EXISTS pg_trgm;

COMMIT;
