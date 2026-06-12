-- 12_controle_acesso_administrativo_catalogo_rollback.sql
-- Remove somente as policies adicionadas pela migration 12.
--
-- public.is_admin() e removida apenas quando seu comentario confirma que a
-- propria migration 12 criou a funcao. Uma funcao preexistente e preservada.
--
-- Validacao apos o rollback:
--   SELECT tablename, policyname
--   FROM pg_policies
--   WHERE schemaname = 'public'
--     AND policyname LIKE '% por administradores'
--   ORDER BY tablename, policyname;

BEGIN;

DROP POLICY IF EXISTS "espetaculo: insercao por administradores"
    ON public.espetaculo;
DROP POLICY IF EXISTS "espetaculo: atualizacao por administradores"
    ON public.espetaculo;
DROP POLICY IF EXISTS "espetaculo: remocao por administradores"
    ON public.espetaculo;

DROP POLICY IF EXISTS "teatro: insercao por administradores"
    ON public.teatro;
DROP POLICY IF EXISTS "teatro: atualizacao por administradores"
    ON public.teatro;
DROP POLICY IF EXISTS "teatro: remocao por administradores"
    ON public.teatro;

DROP POLICY IF EXISTS "ator: insercao por administradores"
    ON public.ator;
DROP POLICY IF EXISTS "ator: atualizacao por administradores"
    ON public.ator;
DROP POLICY IF EXISTS "ator: remocao por administradores"
    ON public.ator;

DROP POLICY IF EXISTS "categoria: insercao por administradores"
    ON public.categoria;
DROP POLICY IF EXISTS "categoria: atualizacao por administradores"
    ON public.categoria;
DROP POLICY IF EXISTS "categoria: remocao por administradores"
    ON public.categoria;

DROP POLICY IF EXISTS "espetaculo_categoria: insercao por administradores"
    ON public.espetaculo_categoria;
DROP POLICY IF EXISTS "espetaculo_categoria: atualizacao por administradores"
    ON public.espetaculo_categoria;
DROP POLICY IF EXISTS "espetaculo_categoria: remocao por administradores"
    ON public.espetaculo_categoria;

DROP POLICY IF EXISTS "espetaculo_ator: insercao por administradores"
    ON public.espetaculo_ator;
DROP POLICY IF EXISTS "espetaculo_ator: atualizacao por administradores"
    ON public.espetaculo_ator;
DROP POLICY IF EXISTS "espetaculo_ator: remocao por administradores"
    ON public.espetaculo_ator;

DROP POLICY IF EXISTS "sessao: insercao por administradores"
    ON public.sessao;
DROP POLICY IF EXISTS "sessao: atualizacao por administradores"
    ON public.sessao;
DROP POLICY IF EXISTS "sessao: remocao por administradores"
    ON public.sessao;

DO $$
BEGIN
    IF to_regprocedure('public.is_admin()') IS NOT NULL
       AND obj_description(
           to_regprocedure('public.is_admin()')::OID,
           'pg_proc'
       ) = 'Criada pela migration 12_controle_acesso_administrativo_catalogo.' THEN
        EXECUTE 'DROP FUNCTION public.is_admin()';
    END IF;
END $$;

COMMIT;
