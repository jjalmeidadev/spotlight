-- 04_trigger_updated_at_espetaculo.sql — Mantém espetaculo.updated_at coerente
-- em qualquer UPDATE, independente de a aplicação setar o campo.
--
-- A coluna espetaculo.updated_at TIMESTAMP NOT NULL DEFAULT NOW() já existe
-- (definida em 01_create_database.sql) — esta migration cria apenas a função
-- de trigger e a trigger BEFORE UPDATE. A função pode ser reutilizada por
-- outras tabelas no futuro (basta criar a trigger correspondente).

BEGIN;

CREATE OR REPLACE FUNCTION public.set_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_espetaculo_updated_at ON espetaculo;
CREATE TRIGGER trg_espetaculo_updated_at
    BEFORE UPDATE ON espetaculo
    FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

COMMIT;
