-- 09_espetaculo_midia.sql - Metadados de midias dos espetaculos.
--
-- Esta migration cria a tabela publica que relaciona cada espetaculo aos
-- arquivos ja enviados para o bucket Supabase Storage `espetaculos`.
--
-- Observacao operacional:
--   - Os arquivos binarios continuam no Supabase Storage.
--   - Esta tabela guarda apenas bucket/path/tipo/alt text.
--   - O bucket `espetaculos` ja existe no projeto cloud. O bloco opcional
--     abaixo so tenta garantir bucket/policies quando o schema `storage`
--     estiver disponivel (Supabase cloud/local completo). Em Docker local com
--     apenas compat de auth, ele e ignorado.
--
-- Validacao apos aplicar:
--   SELECT tipo_midia, COUNT(*)
--   FROM public.espetaculo_midia
--   GROUP BY tipo_midia
--   ORDER BY tipo_midia;
--   -- Esperado: 15 linhas por tipo (60 no total).

BEGIN;

-- --------------------------------------------------------------------------
-- 1) Garantia opcional do bucket/policies no Supabase Storage.
-- --------------------------------------------------------------------------

DO $$
BEGIN
    IF to_regclass('storage.buckets') IS NOT NULL THEN
        INSERT INTO storage.buckets (id, name, public)
        VALUES ('espetaculos', 'espetaculos', TRUE)
        ON CONFLICT (id) DO NOTHING;
    END IF;

    IF to_regclass('storage.objects') IS NOT NULL THEN
        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'storage'
              AND tablename = 'objects'
              AND policyname = 'espetaculos: leitura publica'
        ) THEN
            EXECUTE $policy$
                CREATE POLICY "espetaculos: leitura publica"
                ON storage.objects
                FOR SELECT
                USING (bucket_id = 'espetaculos')
            $policy$;
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'storage'
              AND tablename = 'objects'
              AND policyname = 'espetaculos: escrita por administradores'
        ) THEN
            EXECUTE $policy$
                CREATE POLICY "espetaculos: escrita por administradores"
                ON storage.objects
                FOR INSERT
                WITH CHECK (
                    bucket_id = 'espetaculos'
                    AND EXISTS (
                        SELECT 1
                        FROM public.usuario u
                        WHERE u.id_usuario = auth.uid()
                          AND u.tipo_usuario = 'administrador'
                    )
                )
            $policy$;
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'storage'
              AND tablename = 'objects'
              AND policyname = 'espetaculos: atualizacao por administradores'
        ) THEN
            EXECUTE $policy$
                CREATE POLICY "espetaculos: atualizacao por administradores"
                ON storage.objects
                FOR UPDATE
                USING (
                    bucket_id = 'espetaculos'
                    AND EXISTS (
                        SELECT 1
                        FROM public.usuario u
                        WHERE u.id_usuario = auth.uid()
                          AND u.tipo_usuario = 'administrador'
                    )
                )
                WITH CHECK (
                    bucket_id = 'espetaculos'
                    AND EXISTS (
                        SELECT 1
                        FROM public.usuario u
                        WHERE u.id_usuario = auth.uid()
                          AND u.tipo_usuario = 'administrador'
                    )
                )
            $policy$;
        END IF;

        IF NOT EXISTS (
            SELECT 1
            FROM pg_policies
            WHERE schemaname = 'storage'
              AND tablename = 'objects'
              AND policyname = 'espetaculos: remocao por administradores'
        ) THEN
            EXECUTE $policy$
                CREATE POLICY "espetaculos: remocao por administradores"
                ON storage.objects
                FOR DELETE
                USING (
                    bucket_id = 'espetaculos'
                    AND EXISTS (
                        SELECT 1
                        FROM public.usuario u
                        WHERE u.id_usuario = auth.uid()
                          AND u.tipo_usuario = 'administrador'
                    )
                )
            $policy$;
        END IF;
    END IF;
END $$;

-- --------------------------------------------------------------------------
-- 2) Tabela de metadados de midia.
-- --------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.espetaculo_midia (
    id_midia        SERIAL       PRIMARY KEY,
    id_espetaculo   INTEGER      NOT NULL REFERENCES public.espetaculo(id_espetaculo) ON DELETE CASCADE,
    tipo_midia      VARCHAR(40)  NOT NULL,
    storage_bucket  VARCHAR(100) NOT NULL DEFAULT 'espetaculos',
    storage_path    TEXT         NOT NULL,
    alt_text        TEXT,
    ordem_exibicao  INTEGER      NOT NULL DEFAULT 1 CHECK (ordem_exibicao > 0),
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_espetaculo_midia_tipo CHECK (
        tipo_midia IN (
            'catalog_card',
            'catalog_hero_background',
            'catalog_hero_artwork',
            'detail_image'
        )
    ),
    CONSTRAINT uq_espetaculo_midia_tipo_ordem UNIQUE (
        id_espetaculo, tipo_midia, ordem_exibicao
    ),
    CONSTRAINT uq_espetaculo_midia_bucket_path UNIQUE (
        storage_bucket, storage_path
    )
);

ALTER TABLE public.espetaculo_midia ENABLE ROW LEVEL SECURITY;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'espetaculo_midia'
          AND policyname = 'espetaculo_midia: leitura publica'
    ) THEN
        EXECUTE $policy$
            CREATE POLICY "espetaculo_midia: leitura publica"
            ON public.espetaculo_midia
            FOR SELECT
            USING (true)
        $policy$;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename = 'espetaculo_midia'
          AND policyname = 'espetaculo_midia: escrita por administradores'
    ) THEN
        EXECUTE $policy$
            CREATE POLICY "espetaculo_midia: escrita por administradores"
            ON public.espetaculo_midia
            FOR ALL
            USING (
                EXISTS (
                    SELECT 1
                    FROM public.usuario u
                    WHERE u.id_usuario = auth.uid()
                      AND u.tipo_usuario = 'administrador'
                )
            )
            WITH CHECK (
                EXISTS (
                    SELECT 1
                    FROM public.usuario u
                    WHERE u.id_usuario = auth.uid()
                      AND u.tipo_usuario = 'administrador'
                )
            )
        $policy$;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_espetaculo_midia_espetaculo_tipo
    ON public.espetaculo_midia (id_espetaculo, tipo_midia);

-- --------------------------------------------------------------------------
-- 3) Seed de metadados das midias ja enviadas ao bucket `espetaculos`.
--    Usa titulo, nao id fixo, porque id_espetaculo e SERIAL e pode variar
--    entre ambientes caso o seed tenha sido reaplicado parcialmente.
-- --------------------------------------------------------------------------

DO $$
DECLARE
    faltantes TEXT;
BEGIN
    WITH esperados(titulo) AS (
        VALUES
            ('Elis, a Musical'),
            ('A Novica Rebelde'),
            ('Mamma Mia!'),
            ('Zaquim'),
            ('Pixar in Concert'),
            ('Ayrton Senna - O Musical'),
            ('Romeu & Julieta, ao som de Marisa Monte'),
            ('Musical Hairspray'),
            ('Clube da Esquina - Os Sonhos Nao Envelhecem'),
            ('Nossa Historia com Chico Buarque'),
            ('Prima Facie com Debora Falabella'),
            ('A Cor Purpura, O Musical'),
            ('Bibi - Uma Vida em Musical'),
            ('Hair'),
            ('Chacrinha, o Musical')
    )
    SELECT string_agg(titulo, ', ' ORDER BY titulo)
    INTO faltantes
    FROM esperados e
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.espetaculo s
        WHERE s.titulo = e.titulo
    );

    IF faltantes IS NOT NULL THEN
        RAISE EXCEPTION
            'Seed de espetaculos ausente ou diferente. Rode 02_initial_data.sql antes da migration 09, ou confira titulos faltantes: %',
            faltantes;
    END IF;
END $$;

WITH midias(titulo, tipo_midia, storage_bucket, storage_path, alt_text, ordem_exibicao) AS (
    VALUES
        -- Elis, a Musical
        ('Elis, a Musical', 'catalog_card', 'espetaculos', 'catalog_card/elis_catalog_card.png', 'Card de Elis, a Musical', 1),
        ('Elis, a Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/elis_catalog_hero_background.png', 'Fundo do hero de Elis, a Musical', 1),
        ('Elis, a Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/elis_catalog_hero_artwork .png', 'Imagem destacada do hero de Elis, a Musical', 1),
        ('Elis, a Musical', 'detail_image', 'espetaculos', 'detail_image/elis__detail_image.png', 'Imagem de detalhe de Elis, a Musical', 1),

        -- A Novica Rebelde
        ('A Novica Rebelde', 'catalog_card', 'espetaculos', 'catalog_card/rebelde_catalog_card.png', 'Card de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/rebelde_catalog_hero_background.png', 'Fundo do hero de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/rebelde_catalog_hero_artwork.png', 'Imagem destacada do hero de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'detail_image', 'espetaculos', 'detail_image/rebelde__detail_image.png', 'Imagem de detalhe de A Novica Rebelde', 1),

        -- Mamma Mia!
        ('Mamma Mia!', 'catalog_card', 'espetaculos', 'catalog_card/mamma_catalog_card.png', 'Card de Mamma Mia!', 1),
        ('Mamma Mia!', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/mamma_catalog_hero_background.png', 'Fundo do hero de Mamma Mia!', 1),
        ('Mamma Mia!', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/mamma_catalog_hero_artwork .png', 'Imagem destacada do hero de Mamma Mia!', 1),
        ('Mamma Mia!', 'detail_image', 'espetaculos', 'detail_image/mamma__detail_image.png', 'Imagem de detalhe de Mamma Mia!', 1),

        -- Zaquim
        ('Zaquim', 'catalog_card', 'espetaculos', 'catalog_card/zaquim_catalog_card.png', 'Card de Zaquim', 1),
        ('Zaquim', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/zaquim_catalog_hero_background.png', 'Fundo do hero de Zaquim', 1),
        ('Zaquim', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/zaquim_catalog_hero_artwork .png', 'Imagem destacada do hero de Zaquim', 1),
        ('Zaquim', 'detail_image', 'espetaculos', 'detail_image/zaquim__detail_image.png', 'Imagem de detalhe de Zaquim', 1),

        -- Pixar in Concert
        ('Pixar in Concert', 'catalog_card', 'espetaculos', 'catalog_card/pixar_catalog_card.png', 'Card de Pixar in Concert', 1),
        ('Pixar in Concert', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/pixar_catalog_hero_background.png', 'Fundo do hero de Pixar in Concert', 1),
        ('Pixar in Concert', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/pixar_catalog_hero_artwork .png', 'Imagem destacada do hero de Pixar in Concert', 1),
        ('Pixar in Concert', 'detail_image', 'espetaculos', 'detail_image/pixar_detail_image.png', 'Imagem de detalhe de Pixar in Concert', 1),

        -- Ayrton Senna - O Musical
        ('Ayrton Senna - O Musical', 'catalog_card', 'espetaculos', 'catalog_card/ayrton_catalog_card.png', 'Card de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/ayrton_catalog_hero_background.png', 'Fundo do hero de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/ayrton_catalog_hero_artwork.png', 'Imagem destacada do hero de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'detail_image', 'espetaculos', 'detail_image/ayrton_detail_image.png', 'Imagem de detalhe de Ayrton Senna - O Musical', 1),

        -- Romeu & Julieta, ao som de Marisa Monte
        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_card', 'espetaculos', 'catalog_card/romeu_catalog_card.png', 'Card de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/romeu_catalog_hero_background.png', 'Fundo do hero de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/romeu_catalog_hero_artwork .png', 'Imagem destacada do hero de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'detail_image', 'espetaculos', 'detail_image/romeu__detail_image.png', 'Imagem de detalhe de Romeu & Julieta, ao som de Marisa Monte', 1),

        -- Musical Hairspray
        ('Musical Hairspray', 'catalog_card', 'espetaculos', 'catalog_card/hairspray_catalog_card.png', 'Card de Musical Hairspray', 1),
        ('Musical Hairspray', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/hairspray_catalog_hero_background.png', 'Fundo do hero de Musical Hairspray', 1),
        ('Musical Hairspray', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/hairspray_catalog_hero_artwork.png', 'Imagem destacada do hero de Musical Hairspray', 1),
        ('Musical Hairspray', 'detail_image', 'espetaculos', 'detail_image/hairspray__detail_image.png', 'Imagem de detalhe de Musical Hairspray', 1),

        -- Clube da Esquina - Os Sonhos Nao Envelhecem
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_card', 'espetaculos', 'catalog_card/clube_catalog_card.png', 'Card de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/clube_catalog_hero_background.png', 'Fundo do hero de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/clube_catalog_hero_artwork .png', 'Imagem destacada do hero de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'detail_image', 'espetaculos', 'detail_image/clube__detail_image.png', 'Imagem de detalhe de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),

        -- Nossa Historia com Chico Buarque
        ('Nossa Historia com Chico Buarque', 'catalog_card', 'espetaculos', 'catalog_card/nossa_catalog_card.png', 'Card de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/nossa_catalog_hero_background.png', 'Fundo do hero de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/nossa_catalog_hero_artwork .png', 'Imagem destacada do hero de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'detail_image', 'espetaculos', 'detail_image/nossa__detail_image.png', 'Imagem de detalhe de Nossa Historia com Chico Buarque', 1),

        -- Prima Facie com Debora Falabella
        ('Prima Facie com Debora Falabella', 'catalog_card', 'espetaculos', 'catalog_card/prima_catalog_card.png', 'Card de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/prima_catalog_hero_background.png', 'Fundo do hero de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/prima_catalog_hero_artwork .png', 'Imagem destacada do hero de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'detail_image', 'espetaculos', 'detail_image/prima__detail_image.png', 'Imagem de detalhe de Prima Facie com Debora Falabella', 1),

        -- A Cor Purpura, O Musical
        ('A Cor Purpura, O Musical', 'catalog_card', 'espetaculos', 'catalog_card/purpura_catalog_card.png', 'Card de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/purpura_catalog_hero_background.png', 'Fundo do hero de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/purpura_catalog_hero_artwork .png', 'Imagem destacada do hero de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'detail_image', 'espetaculos', 'detail_image/purpura__detail_image.png', 'Imagem de detalhe de A Cor Purpura, O Musical', 1),

        -- Bibi - Uma Vida em Musical
        ('Bibi - Uma Vida em Musical', 'catalog_card', 'espetaculos', 'catalog_card/bibi_catalog_card.png', 'Card de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/bibi_catalog_hero_background.png', 'Fundo do hero de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/bibi_catalog_hero_artwork .png', 'Imagem destacada do hero de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'detail_image', 'espetaculos', 'detail_image/bibi__detail_image.png', 'Imagem de detalhe de Bibi - Uma Vida em Musical', 1),

        -- Hair
        ('Hair', 'catalog_card', 'espetaculos', 'catalog_card/hair_catalog_card.png', 'Card de Hair', 1),
        ('Hair', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/hair_catalog_hero_background.png', 'Fundo do hero de Hair', 1),
        ('Hair', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/hair_catalog_hero_artwork .png', 'Imagem destacada do hero de Hair', 1),
        ('Hair', 'detail_image', 'espetaculos', 'detail_image/hair_detail_image.png', 'Imagem de detalhe de Hair', 1),

        -- Chacrinha, o Musical
        ('Chacrinha, o Musical', 'catalog_card', 'espetaculos', 'catalog_card/chacrinha_catalog_card.png', 'Card de Chacrinha, o Musical', 1),
        ('Chacrinha, o Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/chacrinha_catalog_hero_background.png', 'Fundo do hero de Chacrinha, o Musical', 1),
        ('Chacrinha, o Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/chacrinha_catalog_hero_artwork.png', 'Imagem destacada do hero de Chacrinha, o Musical', 1),
        ('Chacrinha, o Musical', 'detail_image', 'espetaculos', 'detail_image/chacrinha__detail_image.png', 'Imagem de detalhe de Chacrinha, o Musical', 1)
)
INSERT INTO public.espetaculo_midia (
    id_espetaculo, tipo_midia, storage_bucket, storage_path, alt_text, ordem_exibicao
)
SELECT
    e.id_espetaculo,
    m.tipo_midia,
    m.storage_bucket,
    m.storage_path,
    m.alt_text,
    m.ordem_exibicao
FROM midias m
JOIN public.espetaculo e ON e.titulo = m.titulo
ON CONFLICT (id_espetaculo, tipo_midia, ordem_exibicao) DO UPDATE SET
    storage_bucket = EXCLUDED.storage_bucket,
    storage_path   = EXCLUDED.storage_path,
    alt_text       = EXCLUDED.alt_text;

COMMIT;
