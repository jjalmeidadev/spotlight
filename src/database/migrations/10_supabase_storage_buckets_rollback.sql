-- 10_supabase_storage_buckets_rollback.sql
-- Remove o bucket privado de artefatos do modelo criado pela migration 10.
-- A remocao do bucket so funciona se ele estiver vazio.

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.schemata
        WHERE schema_name = 'storage'
    ) THEN
        RAISE NOTICE 'Schema storage nao encontrado. Nada a reverter.';
        RETURN;
    END IF;

    EXECUTE $sql$
        DELETE FROM storage.buckets
        WHERE id = 'ml-artifacts'
    $sql$;

    RAISE NOTICE 'Bucket ml-artifacts removido apenas se estava vazio.';
END $$;
