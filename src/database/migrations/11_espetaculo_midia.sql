-- 11_espetaculo_midia.sql - Metadados de midias dos espetaculos.
--
-- Os arquivos binarios ficam no Supabase Storage. Esta tabela guarda apenas
-- bucket, path, tipo e alt text para que catalogo/recomendacao retornem URLs
-- reais no frontend.

BEGIN;

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
END $$;

CREATE INDEX IF NOT EXISTS idx_espetaculo_midia_espetaculo_tipo
    ON public.espetaculo_midia (id_espetaculo, tipo_midia);

WITH midias(titulo, tipo_midia, storage_bucket, storage_path, alt_text, ordem_exibicao) AS (
    VALUES
        ('Elis, a Musical', 'catalog_card', 'espetaculos', 'catalog_card/elis_catalog_card.png', 'Card de Elis, a Musical', 1),
        ('Elis, a Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/elis_catalog_hero_background.png', 'Fundo do hero de Elis, a Musical', 1),
        ('Elis, a Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/elis_catalog_hero_artwork .png', 'Imagem destacada do hero de Elis, a Musical', 1),
        ('Elis, a Musical', 'detail_image', 'espetaculos', 'detail_image/elis__detail_image.png', 'Imagem de detalhe de Elis, a Musical', 1),

        ('A Novica Rebelde', 'catalog_card', 'espetaculos', 'catalog_card/rebelde_catalog_card.png', 'Card de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/rebelde_catalog_hero_background.png', 'Fundo do hero de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/rebelde_catalog_hero_artwork.png', 'Imagem destacada do hero de A Novica Rebelde', 1),
        ('A Novica Rebelde', 'detail_image', 'espetaculos', 'detail_image/rebelde__detail_image.png', 'Imagem de detalhe de A Novica Rebelde', 1),

        ('Mamma Mia!', 'catalog_card', 'espetaculos', 'catalog_card/mamma_catalog_card.png', 'Card de Mamma Mia!', 1),
        ('Mamma Mia!', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/mamma_catalog_hero_background.png', 'Fundo do hero de Mamma Mia!', 1),
        ('Mamma Mia!', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/mamma_catalog_hero_artwork .png', 'Imagem destacada do hero de Mamma Mia!', 1),
        ('Mamma Mia!', 'detail_image', 'espetaculos', 'detail_image/mamma__detail_image.png', 'Imagem de detalhe de Mamma Mia!', 1),

        ('Zaquim', 'catalog_card', 'espetaculos', 'catalog_card/zaquim_catalog_card.png', 'Card de Zaquim', 1),
        ('Zaquim', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/zaquim_catalog_hero_background.png', 'Fundo do hero de Zaquim', 1),
        ('Zaquim', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/zaquim_catalog_hero_artwork .png', 'Imagem destacada do hero de Zaquim', 1),
        ('Zaquim', 'detail_image', 'espetaculos', 'detail_image/zaquim__detail_image.png', 'Imagem de detalhe de Zaquim', 1),

        ('Pixar in Concert', 'catalog_card', 'espetaculos', 'catalog_card/pixar_catalog_card.png', 'Card de Pixar in Concert', 1),
        ('Pixar in Concert', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/pixar_catalog_hero_background.png', 'Fundo do hero de Pixar in Concert', 1),
        ('Pixar in Concert', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/pixar_catalog_hero_artwork .png', 'Imagem destacada do hero de Pixar in Concert', 1),
        ('Pixar in Concert', 'detail_image', 'espetaculos', 'detail_image/pixar_detail_image.png', 'Imagem de detalhe de Pixar in Concert', 1),

        ('Ayrton Senna - O Musical', 'catalog_card', 'espetaculos', 'catalog_card/ayrton_catalog_card.png', 'Card de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/ayrton_catalog_hero_background.png', 'Fundo do hero de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/ayrton_catalog_hero_artwork.png', 'Imagem destacada do hero de Ayrton Senna - O Musical', 1),
        ('Ayrton Senna - O Musical', 'detail_image', 'espetaculos', 'detail_image/ayrton_detail_image.png', 'Imagem de detalhe de Ayrton Senna - O Musical', 1),

        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_card', 'espetaculos', 'catalog_card/romeu_catalog_card.png', 'Card de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/romeu_catalog_hero_background.png', 'Fundo do hero de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/romeu_catalog_hero_artwork .png', 'Imagem destacada do hero de Romeu & Julieta, ao som de Marisa Monte', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'detail_image', 'espetaculos', 'detail_image/romeu__detail_image.png', 'Imagem de detalhe de Romeu & Julieta, ao som de Marisa Monte', 1),

        ('Musical Hairspray', 'catalog_card', 'espetaculos', 'catalog_card/hairspray_catalog_card.png', 'Card de Musical Hairspray', 1),
        ('Musical Hairspray', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/hairspray_catalog_hero_background.png', 'Fundo do hero de Musical Hairspray', 1),
        ('Musical Hairspray', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/hairspray_catalog_hero_artwork.png', 'Imagem destacada do hero de Musical Hairspray', 1),
        ('Musical Hairspray', 'detail_image', 'espetaculos', 'detail_image/hairspray__detail_image.png', 'Imagem de detalhe de Musical Hairspray', 1),

        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_card', 'espetaculos', 'catalog_card/clube_catalog_card.png', 'Card de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/clube_catalog_hero_background.png', 'Fundo do hero de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/clube_catalog_hero_artwork .png', 'Imagem destacada do hero de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'detail_image', 'espetaculos', 'detail_image/clube__detail_image.png', 'Imagem de detalhe de Clube da Esquina - Os Sonhos Nao Envelhecem', 1),

        ('Nossa Historia com Chico Buarque', 'catalog_card', 'espetaculos', 'catalog_card/nossa_catalog_card.png', 'Card de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/nossa_catalog_hero_background.png', 'Fundo do hero de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/nossa_catalog_hero_artwork .png', 'Imagem destacada do hero de Nossa Historia com Chico Buarque', 1),
        ('Nossa Historia com Chico Buarque', 'detail_image', 'espetaculos', 'detail_image/nossa__detail_image.png', 'Imagem de detalhe de Nossa Historia com Chico Buarque', 1),

        ('Prima Facie com Debora Falabella', 'catalog_card', 'espetaculos', 'catalog_card/prima_catalog_card.png', 'Card de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/prima_catalog_hero_background.png', 'Fundo do hero de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/prima_catalog_hero_artwork .png', 'Imagem destacada do hero de Prima Facie com Debora Falabella', 1),
        ('Prima Facie com Debora Falabella', 'detail_image', 'espetaculos', 'detail_image/prima__detail_image.png', 'Imagem de detalhe de Prima Facie com Debora Falabella', 1),

        ('A Cor Purpura, O Musical', 'catalog_card', 'espetaculos', 'catalog_card/purpura_catalog_card.png', 'Card de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/purpura_catalog_hero_background.png', 'Fundo do hero de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/purpura_catalog_hero_artwork .png', 'Imagem destacada do hero de A Cor Purpura, O Musical', 1),
        ('A Cor Purpura, O Musical', 'detail_image', 'espetaculos', 'detail_image/purpura__detail_image.png', 'Imagem de detalhe de A Cor Purpura, O Musical', 1),

        ('Bibi - Uma Vida em Musical', 'catalog_card', 'espetaculos', 'catalog_card/bibi_catalog_card.png', 'Card de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/bibi_catalog_hero_background.png', 'Fundo do hero de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/bibi_catalog_hero_artwork .png', 'Imagem destacada do hero de Bibi - Uma Vida em Musical', 1),
        ('Bibi - Uma Vida em Musical', 'detail_image', 'espetaculos', 'detail_image/bibi__detail_image.png', 'Imagem de detalhe de Bibi - Uma Vida em Musical', 1),

        ('Hair', 'catalog_card', 'espetaculos', 'catalog_card/hair_catalog_card.png', 'Card de Hair', 1),
        ('Hair', 'catalog_hero_background', 'espetaculos', 'catalog_hero_background/hair_catalog_hero_background.png', 'Fundo do hero de Hair', 1),
        ('Hair', 'catalog_hero_artwork', 'espetaculos', 'catalog_hero_artwork/hair_catalog_hero_artwork .png', 'Imagem destacada do hero de Hair', 1),
        ('Hair', 'detail_image', 'espetaculos', 'detail_image/hair_detail_image.png', 'Imagem de detalhe de Hair', 1),

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
