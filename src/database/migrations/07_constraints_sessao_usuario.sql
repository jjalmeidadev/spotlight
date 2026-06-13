-- 07_constraints_sessao_usuario.sql — Duas constraints de integridade:
--   1) UNIQUE em sessao(id_espetaculo, id_teatro, data, horario)
--   2) CHECK em usuario.data_nascimento (entre 1900-01-01 e CURRENT_DATE)
--
-- DoR exige que as queries de detecção abaixo retornem resultado limpo antes
-- de rodar esta migration. Caso retornem linhas, corrigir os dados primeiro.

BEGIN;

-- --------------------------------------------------------------------------
-- 1) UNIQUE em sessao — impede duplicatas de mesma sessão (mesmo espetáculo,
--    mesmo teatro, mesma data e horário).
--
-- Verificar duplicatas antes de aplicar (esperado: 0 linhas):
--   SELECT id_espetaculo, id_teatro, data, horario, COUNT(*)
--   FROM sessao
--   GROUP BY id_espetaculo, id_teatro, data, horario
--   HAVING COUNT(*) > 1;
-- --------------------------------------------------------------------------

ALTER TABLE sessao
    ADD CONSTRAINT uq_sessao_espetaculo_teatro_data_horario
    UNIQUE (id_espetaculo, id_teatro, data, horario);

-- --------------------------------------------------------------------------
-- 2) CHECK em usuario.data_nascimento — rejeita datas absurdas (antes de 1900
--    ou no futuro). A coluna é nullable; CHECK ignora NULL por padrão, então
--    perfis sem data de nascimento continuam aceitos.
--
-- Verificar violações existentes antes de aplicar (esperado: 0):
--   SELECT COUNT(*) FROM usuario
--   WHERE data_nascimento IS NOT NULL
--     AND (data_nascimento < DATE '1900-01-01'
--          OR data_nascimento > CURRENT_DATE);
-- --------------------------------------------------------------------------

ALTER TABLE usuario
    ADD CONSTRAINT ck_usuario_data_nascimento
    CHECK (
        data_nascimento >= DATE '1900-01-01'
        AND data_nascimento <= CURRENT_DATE
    );

COMMIT;
