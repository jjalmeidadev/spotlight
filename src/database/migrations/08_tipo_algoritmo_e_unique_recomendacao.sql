-- 08_tipo_algoritmo_e_unique_recomendacao.sql — Centraliza a lista de
-- algoritmos válidos em uma função e adiciona UNIQUE em recomendacao.
--
-- Motivação:
--   1) chk_tipo_algoritmo (em recomendacao) e chk_tipo_algoritmo_similar
--      (em similaridade_item) duplicavam a mesma lista IN ('k-NN','SVD','k-means').
--      Adicionar um algoritmo novo exigia editar dois CHECKs. Esta migration
--      extrai a verificação para uma função pública reutilizável.
--   2) recomendacao não tinha UNIQUE — o Worker de ML poderia inserir
--      múltiplas linhas para o mesmo par (usuario, espetaculo, algoritmo).
--      O index UNIQUE viabiliza o padrão de UPSERT documentado abaixo.
--
-- DoR exige que as duplicatas em (id_usuario, id_espetaculo, tipo_algoritmo)
-- já tenham sido conferidas com resultado limpo. Para nova verificação:
--   SELECT id_usuario, id_espetaculo, tipo_algoritmo, COUNT(*)
--   FROM recomendacao
--   GROUP BY id_usuario, id_espetaculo, tipo_algoritmo
--   HAVING COUNT(*) > 1;
--
-- Padrão de UPSERT para o Worker ao gravar recomendações:
--   INSERT INTO recomendacao (
--       id_usuario, id_espetaculo, tipo_algoritmo,
--       score_recomendacao, motivo_recomendacao,
--       data_geracao, referencia_origem_id
--   ) VALUES (...)
--   ON CONFLICT (id_usuario, id_espetaculo, tipo_algoritmo)
--   DO UPDATE SET
--       score_recomendacao   = EXCLUDED.score_recomendacao,
--       motivo_recomendacao  = EXCLUDED.motivo_recomendacao,
--       data_geracao         = EXCLUDED.data_geracao,
--       referencia_origem_id = EXCLUDED.referencia_origem_id;
--   -- NOTA: prioridade_manual NÃO é sobrescrito (admin seta via rota própria).

BEGIN;

-- --------------------------------------------------------------------------
-- 1) Função central de validação. Adicionar algoritmo novo daqui pra frente
--    é um único CREATE OR REPLACE; nenhum ALTER TABLE necessário.
-- --------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION public.is_tipo_algoritmo_valido(t TEXT)
RETURNS BOOLEAN
LANGUAGE SQL
IMMUTABLE
AS $$
    SELECT t IN ('k-NN', 'SVD', 'k-means')
$$;

-- --------------------------------------------------------------------------
-- 2) Substituir os CHECKs duplicados pela função reutilizável.
--    Nomes originais (apurados via information_schema / grep no schema):
--      recomendacao        → chk_tipo_algoritmo
--      similaridade_item   → chk_tipo_algoritmo_similar
-- --------------------------------------------------------------------------

ALTER TABLE recomendacao
    DROP CONSTRAINT chk_tipo_algoritmo;
ALTER TABLE recomendacao
    ADD CONSTRAINT chk_tipo_algoritmo
    CHECK (public.is_tipo_algoritmo_valido(tipo_algoritmo));

ALTER TABLE similaridade_item
    DROP CONSTRAINT chk_tipo_algoritmo_similar;
ALTER TABLE similaridade_item
    ADD CONSTRAINT chk_tipo_algoritmo_similaridade
    CHECK (public.is_tipo_algoritmo_valido(tipo_algoritmo));

-- --------------------------------------------------------------------------
-- 3) UNIQUE em recomendacao — viabiliza o UPSERT do Worker e impede que
--    múltiplas recomendações conflitantes coexistam para o mesmo par.
-- --------------------------------------------------------------------------

CREATE UNIQUE INDEX IF NOT EXISTS uq_recomendacao_usuario_espetaculo_algo
    ON recomendacao (id_usuario, id_espetaculo, tipo_algoritmo);

COMMIT;
