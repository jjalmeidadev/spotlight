-- 08_tipo_algoritmo_e_unique_recomendacao_rollback.sql — Reverte 08_...
--
-- Após o rollback:
--   - A função public.is_tipo_algoritmo_valido() deixa de existir.
--   - Os CHECKs voltam à forma original com a lista literal IN (...).
--   - O UNIQUE em recomendacao é removido — caminhos de UPSERT do Worker
--     param de funcionar e múltiplas linhas para o mesmo trio passam a ser
--     aceitas novamente.
--
-- Validação após o rollback:
--   SELECT proname FROM pg_proc WHERE proname = 'is_tipo_algoritmo_valido';
--   -- Esperado: 0 linhas.
--   SELECT indexname FROM pg_indexes
--   WHERE indexname = 'uq_recomendacao_usuario_espetaculo_algo';
--   -- Esperado: 0 linhas.

BEGIN;

-- Remover UNIQUE primeiro (independente das CHECKs).
DROP INDEX IF EXISTS uq_recomendacao_usuario_espetaculo_algo;

-- Recriar CHECKs com a lista literal original.
ALTER TABLE similaridade_item
    DROP CONSTRAINT IF EXISTS chk_tipo_algoritmo_similaridade;
ALTER TABLE similaridade_item
    ADD CONSTRAINT chk_tipo_algoritmo_similar
    CHECK (tipo_algoritmo IN ('k-NN', 'SVD', 'k-means'));

ALTER TABLE recomendacao
    DROP CONSTRAINT IF EXISTS chk_tipo_algoritmo;
ALTER TABLE recomendacao
    ADD CONSTRAINT chk_tipo_algoritmo
    CHECK (tipo_algoritmo IN ('k-NN', 'SVD', 'k-means'));

-- Função só pode ser dropada depois que as CHECKs que dependem dela
-- tiverem sido removidas (acima).
DROP FUNCTION IF EXISTS public.is_tipo_algoritmo_valido(TEXT);

COMMIT;
