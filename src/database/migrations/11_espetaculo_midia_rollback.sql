-- 11_espetaculo_midia_rollback.sql

BEGIN;

DROP TABLE IF EXISTS public.espetaculo_midia;

COMMIT;
