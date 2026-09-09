-- 13_rls_similaridade_item_avaliacao.sql
-- Protege a escrita em similaridade_item e permite que o autor atualize
-- somente a propria avaliacao.
--
-- A migration:
--   1) Valida as dependencias e o RLS das tabelas alvo.
--   2) Verifica pg_policies antes de criar cada policy.
--   3) Aborta se encontrar policies de escrita conflitantes.
--   4) Preserva todas as policies existentes.
--
-- Validacao apos aplicar:
--   SELECT tablename, policyname, cmd, qual, with_check
--   FROM pg_policies
--   WHERE schemaname = 'public'
--     AND tablename IN ('similaridade_item', 'avaliacao')
--   ORDER BY tablename, cmd, policyname;

BEGIN;

DO $$
DECLARE
    v_tabela TEXT;
BEGIN
    IF to_regprocedure('public.is_admin()') IS NULL THEN
        RAISE EXCEPTION
            'A funcao public.is_admin() deve existir antes da migration 13.';
    END IF;

    IF to_regprocedure('auth.uid()') IS NULL THEN
        RAISE EXCEPTION
            'A funcao auth.uid() deve existir antes da migration 13.';
    END IF;

    FOREACH v_tabela IN ARRAY ARRAY['similaridade_item', 'avaliacao']
    LOOP
        IF to_regclass(format('public.%I', v_tabela)) IS NULL THEN
            RAISE EXCEPTION
                'A tabela public.% deve existir antes da migration 13.',
                v_tabela;
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_class AS c
            JOIN pg_namespace AS n
              ON n.oid = c.relnamespace
            WHERE n.nspname = 'public'
              AND c.relname = v_tabela
              AND c.relkind IN ('r', 'p')
              AND c.relrowsecurity
        ) THEN
            RAISE EXCEPTION
                'RLS deve estar habilitado em public.% antes da migration 13.',
                v_tabela;
        END IF;
    END LOOP;
END $$;

DO $$
DECLARE
    v_conflitos     TEXT;
    v_policy_insert TEXT := 'similaridade_item: insercao por administradores';
    v_policy_update TEXT := 'similaridade_item: atualizacao por administradores';
    v_policy_delete TEXT := 'similaridade_item: remocao por administradores';
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_policy AS p
        JOIN pg_class AS c
          ON c.oid = p.polrelid
        JOIN pg_namespace AS n
          ON n.oid = c.relnamespace
        WHERE n.nspname = 'public'
          AND c.relname = 'similaridade_item'
          AND p.polname IN (
              v_policy_insert,
              v_policy_update,
              v_policy_delete
          )
          AND obj_description(p.oid, 'pg_policy') IS DISTINCT FROM
              'Criada pela migration 13_rls_similaridade_item_avaliacao.'
    ) THEN
        RAISE EXCEPTION
            'Existe policy homonima nao criada pela migration 13 em public.similaridade_item. Revise antes de aplicar.';
    END IF;

    SELECT string_agg(
        format('%I (%s)', p.policyname, p.cmd),
        ', ' ORDER BY p.policyname
    )
    INTO v_conflitos
    FROM pg_policies AS p
    WHERE p.schemaname = 'public'
      AND p.tablename = 'similaridade_item'
      AND p.cmd IN ('ALL', 'INSERT', 'UPDATE', 'DELETE')
      AND p.policyname NOT IN (
          v_policy_insert,
          v_policy_update,
          v_policy_delete
      );

    IF v_conflitos IS NOT NULL THEN
        RAISE EXCEPTION
            'Policies de escrita conflitantes em public.similaridade_item: %. Revise antes de aplicar a migration 13.',
            v_conflitos;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'similaridade_item'
          AND policyname = v_policy_insert
    ) THEN
        EXECUTE format(
            'CREATE POLICY %I ON public.similaridade_item FOR INSERT WITH CHECK (public.is_admin())',
            v_policy_insert
        );

        EXECUTE format(
            'COMMENT ON POLICY %I ON public.similaridade_item IS %L',
            v_policy_insert,
            'Criada pela migration 13_rls_similaridade_item_avaliacao.'
        );
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'similaridade_item'
          AND policyname = v_policy_update
    ) THEN
        EXECUTE format(
            'CREATE POLICY %I ON public.similaridade_item FOR UPDATE USING (public.is_admin()) WITH CHECK (public.is_admin())',
            v_policy_update
        );

        EXECUTE format(
            'COMMENT ON POLICY %I ON public.similaridade_item IS %L',
            v_policy_update,
            'Criada pela migration 13_rls_similaridade_item_avaliacao.'
        );
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'similaridade_item'
          AND policyname = v_policy_delete
    ) THEN
        EXECUTE format(
            'CREATE POLICY %I ON public.similaridade_item FOR DELETE USING (public.is_admin())',
            v_policy_delete
        );

        EXECUTE format(
            'COMMENT ON POLICY %I ON public.similaridade_item IS %L',
            v_policy_delete,
            'Criada pela migration 13_rls_similaridade_item_avaliacao.'
        );
    END IF;
END $$;

DO $$
DECLARE
    v_conflitos     TEXT;
    v_policy_update TEXT := 'avaliacao: atualizacao pelo proprio usuario';
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_policy AS p
        JOIN pg_class AS c
          ON c.oid = p.polrelid
        JOIN pg_namespace AS n
          ON n.oid = c.relnamespace
        WHERE n.nspname = 'public'
          AND c.relname = 'avaliacao'
          AND p.polname = v_policy_update
          AND obj_description(p.oid, 'pg_policy') IS DISTINCT FROM
              'Criada pela migration 13_rls_similaridade_item_avaliacao.'
    ) THEN
        RAISE EXCEPTION
            'Existe policy homonima nao criada pela migration 13 em public.avaliacao. Revise antes de aplicar.';
    END IF;

    SELECT string_agg(
        format('%I (%s)', p.policyname, p.cmd),
        ', ' ORDER BY p.policyname
    )
    INTO v_conflitos
    FROM pg_policies AS p
    WHERE p.schemaname = 'public'
      AND p.tablename = 'avaliacao'
      AND p.cmd IN ('ALL', 'UPDATE')
      AND p.policyname <> v_policy_update;

    IF v_conflitos IS NOT NULL THEN
        RAISE EXCEPTION
            'Policies de UPDATE conflitantes em public.avaliacao: %. Revise antes de aplicar a migration 13.',
            v_conflitos;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'avaliacao'
          AND policyname = v_policy_update
    ) THEN
        EXECUTE format(
            'CREATE POLICY %I ON public.avaliacao FOR UPDATE USING (auth.uid() = id_usuario) WITH CHECK (auth.uid() = id_usuario)',
            v_policy_update
        );

        EXECUTE format(
            'COMMENT ON POLICY %I ON public.avaliacao IS %L',
            v_policy_update,
            'Criada pela migration 13_rls_similaridade_item_avaliacao.'
        );
    END IF;
END $$;

COMMIT;
