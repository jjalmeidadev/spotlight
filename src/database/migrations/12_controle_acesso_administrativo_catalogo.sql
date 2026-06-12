-- 12_controle_acesso_administrativo_catalogo.sql
-- Restringe as operacoes de escrita do catalogo a usuarios administradores,
-- preservando as policies existentes de leitura publica.
--
-- A migration:
--   1) Cria public.is_admin() somente se a funcao ainda nao existir.
--   2) Verifica pg_policies antes de criar cada policy.
--   3) Aborta se encontrar outra policy de escrita nas tabelas alvo, pois
--      policies permissivas sao combinadas com OR e poderiam ampliar acesso.
--
-- Validacao apos aplicar:
--   SELECT public.is_admin();
--
--   SELECT tablename, policyname, cmd
--   FROM pg_policies
--   WHERE schemaname = 'public'
--     AND tablename IN (
--       'espetaculo', 'teatro', 'ator', 'categoria',
--       'espetaculo_categoria', 'espetaculo_ator', 'sessao'
--     )
--   ORDER BY tablename, cmd, policyname;
--   -- Esperado por tabela: SELECT publico + INSERT/UPDATE/DELETE administrativos.

BEGIN;

DO $$
DECLARE
    v_retorno_boolean  BOOLEAN;
    v_security_definer BOOLEAN;
    v_volatilidade     "char";
BEGIN
    IF to_regprocedure('public.is_admin()') IS NULL THEN
        EXECUTE $function$
            CREATE FUNCTION public.is_admin()
            RETURNS BOOLEAN
            LANGUAGE SQL
            STABLE
            SECURITY DEFINER
            SET search_path = pg_catalog, public
            AS $body$
                SELECT EXISTS (
                    SELECT 1
                    FROM public.usuario AS u
                    WHERE u.id_usuario = auth.uid()
                      AND u.tipo_usuario = 'administrador'
                );
            $body$
        $function$;

        COMMENT ON FUNCTION public.is_admin() IS
            'Criada pela migration 12_controle_acesso_administrativo_catalogo.';
    ELSE
        SELECT
            p.prorettype = 'pg_catalog.bool'::regtype,
            p.prosecdef,
            p.provolatile
        INTO
            v_retorno_boolean,
            v_security_definer,
            v_volatilidade
        FROM pg_proc AS p
        WHERE p.oid = to_regprocedure('public.is_admin()');

        IF NOT v_retorno_boolean
           OR NOT v_security_definer
           OR v_volatilidade <> 's' THEN
            RAISE EXCEPTION
                'public.is_admin() existente deve retornar BOOLEAN e ser SECURITY DEFINER STABLE.';
        END IF;
    END IF;
END $$;

DO $$
DECLARE
    v_tabela        TEXT;
    v_conflitos     TEXT;
    v_policy_insert TEXT;
    v_policy_update TEXT;
    v_policy_delete TEXT;
BEGIN
    FOREACH v_tabela IN ARRAY ARRAY[
        'espetaculo',
        'teatro',
        'ator',
        'categoria',
        'espetaculo_categoria',
        'espetaculo_ator',
        'sessao'
    ]
    LOOP
        v_policy_insert := v_tabela || ': insercao por administradores';
        v_policy_update := v_tabela || ': atualizacao por administradores';
        v_policy_delete := v_tabela || ': remocao por administradores';

        SELECT string_agg(
            format('%I (%s)', p.policyname, p.cmd),
            ', ' ORDER BY p.policyname
        )
        INTO v_conflitos
        FROM pg_policies AS p
        WHERE p.schemaname = 'public'
          AND p.tablename = v_tabela
          AND p.cmd IN ('ALL', 'INSERT', 'UPDATE', 'DELETE')
          AND p.policyname NOT IN (
              v_policy_insert,
              v_policy_update,
              v_policy_delete
          );

        IF v_conflitos IS NOT NULL THEN
            RAISE EXCEPTION
                'Policies de escrita conflitantes em public.%: %. Revise antes de aplicar a migration 12.',
                v_tabela,
                v_conflitos;
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'public'
              AND tablename = v_tabela
              AND policyname = v_policy_insert
        ) THEN
            EXECUTE format(
                'CREATE POLICY %I ON public.%I FOR INSERT WITH CHECK (public.is_admin())',
                v_policy_insert,
                v_tabela
            );
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'public'
              AND tablename = v_tabela
              AND policyname = v_policy_update
        ) THEN
            EXECUTE format(
                'CREATE POLICY %I ON public.%I FOR UPDATE USING (public.is_admin()) WITH CHECK (public.is_admin())',
                v_policy_update,
                v_tabela
            );
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'public'
              AND tablename = v_tabela
              AND policyname = v_policy_delete
        ) THEN
            EXECUTE format(
                'CREATE POLICY %I ON public.%I FOR DELETE USING (public.is_admin())',
                v_policy_delete,
                v_tabela
            );
        END IF;
    END LOOP;
END $$;

COMMIT;
