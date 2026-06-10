-- 10_supabase_storage_buckets.sql
-- Cria o bucket privado de Storage usado pelo Spotlight para artefatos de
-- treinamento/inferencia do modelo de recomendacao.
--
-- Execute no SQL Editor do Supabase. Este script depende do schema `storage`,
-- criado pela propria plataforma Supabase.

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.schemata
        WHERE schema_name = 'storage'
    ) THEN
        RAISE NOTICE 'Schema storage nao encontrado. Execute esta migration no Supabase.';
        RETURN;
    END IF;

    EXECUTE $sql$
        INSERT INTO storage.buckets (
            id,
            name,
            public,
            file_size_limit,
            allowed_mime_types
        )
        VALUES
            (
                'ml-artifacts',
                'ml-artifacts',
                false,
                104857600,
                ARRAY['application/zip', 'application/json', 'text/csv', 'text/plain']
            )
        ON CONFLICT (id) DO UPDATE
        SET
            public = EXCLUDED.public,
            file_size_limit = EXCLUDED.file_size_limit,
            allowed_mime_types = EXCLUDED.allowed_mime_types
    $sql$;

    RAISE NOTICE 'Bucket privado ml-artifacts configurado.';
END $$;
