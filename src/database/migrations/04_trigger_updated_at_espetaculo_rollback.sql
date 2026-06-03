-- 04_trigger_updated_at_espetaculo_rollback.sql — Reverte 04_trigger_updated_at_espetaculo.sql.
--
-- AVISO: após o rollback, a aplicação volta a ser responsável por manter
-- espetaculo.updated_at coerente em cada UPDATE. Se o repositório C# tiver
-- removido o set manual (parte da mesma alteração), reaplicá-lo antes do
-- rollback evita updated_at desatualizado em ambiente revertido.

BEGIN;

DROP TRIGGER IF EXISTS trg_espetaculo_updated_at ON espetaculo;
DROP FUNCTION IF EXISTS public.set_updated_at();

COMMIT;
