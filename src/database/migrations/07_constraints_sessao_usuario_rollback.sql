-- 07_constraints_sessao_usuario_rollback.sql — Reverte 07_constraints_sessao_usuario.sql.
--
-- Após o rollback, duplicatas de sessão e datas de nascimento absurdas voltam
-- a ser aceitas pelo banco — o controle precisa ser feito em camada de
-- aplicação até que estas constraints sejam restabelecidas.
--
-- Validação após o rollback:
--   SELECT conname FROM pg_constraint
--   WHERE conname IN (
--     'uq_sessao_espetaculo_teatro_data_horario',
--     'ck_usuario_data_nascimento'
--   );
-- Esperado: zero linhas retornadas.

BEGIN;

ALTER TABLE usuario DROP CONSTRAINT IF EXISTS ck_usuario_data_nascimento;
ALTER TABLE sessao  DROP CONSTRAINT IF EXISTS uq_sessao_espetaculo_teatro_data_horario;

COMMIT;
