-- 09_rastreabilidade_transacoes_rollback.sql — Reverte 09_...
--
-- ATENÇÃO (reversão destrutiva): o DROP TABLE auditoria APAGA toda a trilha
-- de auditoria já registrada. Exporte os dados antes se precisar retê-los:
--   \copy (SELECT * FROM auditoria) TO 'auditoria_backup.csv' CSV HEADER
--
-- Após o rollback:
--   - As 8 triggers trg_auditoria_* deixam de existir; escritas voltam a NÃO
--     ser auditadas.
--   - A função public.registrar_auditoria() é removida.
--   - A tabela public.auditoria e seus índices/políticas são removidos.
--
-- Validação após o rollback:
--   SELECT tgname FROM pg_trigger WHERE tgname LIKE 'trg_auditoria_%';   -- 0 linhas
--   SELECT proname FROM pg_proc WHERE proname = 'registrar_auditoria';   -- 0 linhas
--   SELECT tablename FROM pg_tables WHERE tablename = 'auditoria';        -- 0 linhas

BEGIN;

-- 1) Remover as triggers (dependem da função).
DROP TRIGGER IF EXISTS trg_auditoria_usuario          ON usuario;
DROP TRIGGER IF EXISTS trg_auditoria_espetaculo       ON espetaculo;
DROP TRIGGER IF EXISTS trg_auditoria_teatro           ON teatro;
DROP TRIGGER IF EXISTS trg_auditoria_ator             ON ator;
DROP TRIGGER IF EXISTS trg_auditoria_sessao           ON sessao;
DROP TRIGGER IF EXISTS trg_auditoria_avaliacao        ON avaliacao;
DROP TRIGGER IF EXISTS trg_auditoria_recomendacao     ON recomendacao;
DROP TRIGGER IF EXISTS trg_auditoria_similaridade_item ON similaridade_item;

-- 2) Remover a função (já sem triggers dependentes).
DROP FUNCTION IF EXISTS public.registrar_auditoria();

-- 3) Remover a tabela (política e índices caem junto).
DROP TABLE IF EXISTS auditoria;

COMMIT;
