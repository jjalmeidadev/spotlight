-- 09_espetaculo_midia_rollback.sql - Reverte metadados de midias.
--
-- Apos o rollback:
--   - A tabela public.espetaculo_midia deixa de existir.
--   - Os arquivos do Supabase Storage NAO sao removidos.
--   - O bucket `espetaculos` e suas policies tambem sao preservados para evitar
--     apagar assets compartilhados acidentalmente.
--
-- Validacao apos o rollback:
--   SELECT to_regclass('public.espetaculo_midia');
--   -- Esperado: NULL.

BEGIN;

DROP TABLE IF EXISTS public.espetaculo_midia CASCADE;

COMMIT;
