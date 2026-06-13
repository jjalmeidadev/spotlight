-- 13_rls_similaridade_item_avaliacao_rollback.sql
-- Remove somente as policies adicionadas pela migration 13.
--
-- Cada policy e removida apenas quando seu comentario confirma que a propria
-- migration 13 a criou. Policies preexistentes com os mesmos nomes sao
-- preservadas.
--
-- Validacao apos o rollback:
--   SELECT c.relname AS tablename, p.polname AS policyname
--   FROM pg_policy AS p
--   JOIN pg_class AS c ON c.oid = p.polrelid
--   JOIN pg_namespace AS n ON n.oid = c.relnamespace
--   WHERE n.nspname = 'public'
--     AND obj_description(p.oid, 'pg_policy') =
--         'Criada pela migration 13_rls_similaridade_item_avaliacao.'
--   ORDER BY c.relname, p.polname;
--   -- Esperado: zero linhas.

BEGIN;

DO $$
DECLARE
    v_policy RECORD;
BEGIN
    FOR v_policy IN
        SELECT *
        FROM (
            VALUES
                ('similaridade_item', 'similaridade_item: insercao por administradores'),
                ('similaridade_item', 'similaridade_item: atualizacao por administradores'),
                ('similaridade_item', 'similaridade_item: remocao por administradores'),
                ('avaliacao', 'avaliacao: atualizacao pelo proprio usuario')
        ) AS policies(tablename, policyname)
    LOOP
        IF EXISTS (
            SELECT 1
            FROM pg_policy AS p
            JOIN pg_class AS c
              ON c.oid = p.polrelid
            JOIN pg_namespace AS n
              ON n.oid = c.relnamespace
            WHERE n.nspname = 'public'
              AND c.relname = v_policy.tablename
              AND p.polname = v_policy.policyname
              AND obj_description(p.oid, 'pg_policy') =
                  'Criada pela migration 13_rls_similaridade_item_avaliacao.'
        ) THEN
            EXECUTE format(
                'DROP POLICY %I ON public.%I',
                v_policy.policyname,
                v_policy.tablename
            );
        END IF;
    END LOOP;
END $$;

COMMIT;
