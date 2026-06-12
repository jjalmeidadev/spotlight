-- diagnostico_seed_catalogo.sql
--
-- Diagnostico nao destrutivo para ambientes Supabase parcialmente populados.
-- Rode este arquivo antes de aplicar seeds incrementais. Ele nao altera dados.
--
-- Objetivo:
--   1) Ver quais tabelas principais existem e quantos registros possuem.
--   2) Conferir quais dos 15 espetaculos do seed oficial estao presentes.
--   3) Conferir dependencias de catalogo: categorias, atores, teatros, sessoes,
--      avaliacoes, recomendacoes, similaridades e midias.
--   4) Expor o estado das sequences SERIAL para evitar suposicoes de ids fixos.

CREATE TEMP TABLE IF NOT EXISTS _diag_midia_por_espetaculo (
    id_espetaculo INTEGER PRIMARY KEY,
    midias BIGINT NOT NULL
) ON COMMIT DROP;

CREATE TEMP TABLE IF NOT EXISTS _diag_midia_por_tipo (
    tipo_midia TEXT PRIMARY KEY,
    total BIGINT NOT NULL
) ON COMMIT DROP;

TRUNCATE _diag_midia_por_espetaculo;
TRUNCATE _diag_midia_por_tipo;

DO $$
BEGIN
    IF to_regclass('public.espetaculo_midia') IS NOT NULL THEN
        EXECUTE $sql$
            INSERT INTO _diag_midia_por_espetaculo (id_espetaculo, midias)
            SELECT id_espetaculo, COUNT(*)
            FROM public.espetaculo_midia
            GROUP BY id_espetaculo
        $sql$;

        EXECUTE $sql$
            INSERT INTO _diag_midia_por_tipo (tipo_midia, total)
            SELECT tipo_midia, COUNT(*)
            FROM public.espetaculo_midia
            GROUP BY tipo_midia
        $sql$;
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- 1) Contagem das tabelas principais.
-- ---------------------------------------------------------------------------

SELECT 'usuario' AS tabela, COUNT(*) AS total FROM public.usuario
UNION ALL SELECT 'categoria', COUNT(*) FROM public.categoria
UNION ALL SELECT 'ator', COUNT(*) FROM public.ator
UNION ALL SELECT 'teatro', COUNT(*) FROM public.teatro
UNION ALL SELECT 'espetaculo', COUNT(*) FROM public.espetaculo
UNION ALL SELECT 'espetaculo_categoria', COUNT(*) FROM public.espetaculo_categoria
UNION ALL SELECT 'espetaculo_ator', COUNT(*) FROM public.espetaculo_ator
UNION ALL SELECT 'sessao', COUNT(*) FROM public.sessao
UNION ALL SELECT 'avaliacao', COUNT(*) FROM public.avaliacao
UNION ALL SELECT 'recomendacao', COUNT(*) FROM public.recomendacao
UNION ALL SELECT 'similaridade_item', COUNT(*) FROM public.similaridade_item
UNION ALL SELECT 'espetaculo_midia', COALESCE(SUM(total), 0)::BIGINT FROM _diag_midia_por_tipo
ORDER BY tabela;

-- ---------------------------------------------------------------------------
-- 2) Espetaculos esperados pelo seed oficial vs. estado atual.
-- ---------------------------------------------------------------------------

WITH esperados(ordem, titulo, genero, status) AS (
    VALUES
        (1, 'Elis, a Musical', 'Teatro Musical', 'encerrado'),
        (2, 'A Novica Rebelde', 'Teatro Musical', 'em_cartaz'),
        (3, 'Mamma Mia!', 'Teatro Musical', 'em_cartaz'),
        (4, 'Zaquim', 'Teatro Infantil', 'em_cartaz'),
        (5, 'Pixar in Concert', 'Concerto e Show', 'em_breve'),
        (6, 'Ayrton Senna - O Musical', 'Teatro Musical', 'encerrado'),
        (7, 'Romeu & Julieta, ao som de Marisa Monte', 'Teatro Musical', 'encerrado'),
        (8, 'Musical Hairspray', 'Teatro Musical', 'encerrado'),
        (9, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 'Concerto e Show', 'encerrado'),
        (10, 'Nossa Historia com Chico Buarque', 'Show Musical', 'encerrado'),
        (11, 'Prima Facie com Debora Falabella', 'Dramaturgia', 'encerrado'),
        (12, 'A Cor Purpura, O Musical', 'Teatro Musical', 'encerrado'),
        (13, 'Bibi - Uma Vida em Musical', 'Show Musical', 'encerrado'),
        (14, 'Hair', 'Teatro Musical', 'encerrado'),
        (15, 'Chacrinha, o Musical', 'Show Musical', 'encerrado')
)
SELECT
    e.ordem,
    e.titulo,
    CASE WHEN s.id_espetaculo IS NULL THEN 'FALTANDO' ELSE 'OK' END AS status_seed,
    s.id_espetaculo AS id_atual,
    s.genero AS genero_atual,
    s.status::text AS status_atual,
    s.popularidade,
    s.nota_media
FROM esperados e
LEFT JOIN public.espetaculo s ON s.titulo = e.titulo
ORDER BY e.ordem;

-- ---------------------------------------------------------------------------
-- 3) Categorias esperadas vs. estado atual.
-- ---------------------------------------------------------------------------

WITH esperadas(nome) AS (
    VALUES
        ('Teatro Musical'),
        ('Teatro Infantil'),
        ('Ballet Classico'),
        ('Concerto e Show'),
        ('Dramaturgia'),
        ('Show Musical')
)
SELECT
    e.nome,
    CASE WHEN c.id_categoria IS NULL THEN 'FALTANDO' ELSE 'OK' END AS status_seed,
    c.id_categoria AS id_atual,
    c.descricao
FROM esperadas e
LEFT JOIN public.categoria c ON c.nome = e.nome
ORDER BY e.nome;

-- ---------------------------------------------------------------------------
-- 4) Atores esperados vs. estado atual.
-- ---------------------------------------------------------------------------

WITH esperados(nome) AS (
    VALUES
        ('Claudia Raia'),
        ('Jarbas Homem de Mello'),
        ('Mel Lisboa'),
        ('Marcus Ornellas'),
        ('Jullie'),
        ('Victor Ferrari'),
        ('Izabel Buarque'),
        ('Debora Falabella'),
        ('Sergio Malheiros'),
        ('Marisa Orth')
)
SELECT
    e.nome,
    CASE WHEN a.id_ator IS NULL THEN 'FALTANDO' ELSE 'OK' END AS status_seed,
    a.id_ator AS id_atual
FROM esperados e
LEFT JOIN public.ator a ON a.nome = e.nome
ORDER BY e.nome;

-- ---------------------------------------------------------------------------
-- 5) Teatros esperados vs. estado atual.
-- ---------------------------------------------------------------------------

WITH esperados(nome) AS (
    VALUES
        ('BTG Pactual Hall'),
        ('Nubank Hall'),
        ('Teatro Ri Happy'),
        ('TotalEnergies Arena')
)
SELECT
    e.nome,
    CASE WHEN t.id_teatro IS NULL THEN 'FALTANDO' ELSE 'OK' END AS status_seed,
    t.id_teatro AS id_atual,
    t.cidade,
    t.estado,
    t.capacidade
FROM esperados e
LEFT JOIN public.teatro t ON t.nome = e.nome
ORDER BY e.nome;

-- ---------------------------------------------------------------------------
-- 6) Cobertura de relacionamentos e dados derivados por espetaculo existente.
-- ---------------------------------------------------------------------------

SELECT
    s.id_espetaculo,
    s.titulo,
    COUNT(DISTINCT ec.id_categoria) AS categorias,
    COUNT(DISTINCT ea.id_ator) AS atores,
    COUNT(DISTINCT se.id_sessao) AS sessoes,
    COUNT(DISTINCT av.id_avaliacao) AS avaliacoes,
    COUNT(DISTINCT ri.id_recomendacao) AS recomendacoes_recebidas,
    COUNT(DISTINCT si.id_similaridade) AS similares_origem,
    COALESCE(MAX(mp.midias), 0) AS midias
FROM public.espetaculo s
LEFT JOIN public.espetaculo_categoria ec ON ec.id_espetaculo = s.id_espetaculo
LEFT JOIN public.espetaculo_ator ea ON ea.id_espetaculo = s.id_espetaculo
LEFT JOIN public.sessao se ON se.id_espetaculo = s.id_espetaculo
LEFT JOIN public.avaliacao av ON av.id_espetaculo = s.id_espetaculo
LEFT JOIN public.recomendacao ri ON ri.id_espetaculo = s.id_espetaculo
LEFT JOIN public.similaridade_item si ON si.id_espetaculo_origem = s.id_espetaculo
LEFT JOIN _diag_midia_por_espetaculo mp ON mp.id_espetaculo = s.id_espetaculo
GROUP BY s.id_espetaculo, s.titulo
ORDER BY s.id_espetaculo;

-- ---------------------------------------------------------------------------
-- 7) Midias por tipo, se a migration 09 ja tiver criado a tabela.
-- ---------------------------------------------------------------------------

SELECT
    tipo_midia,
    total
FROM _diag_midia_por_tipo
ORDER BY tipo_midia;

-- ---------------------------------------------------------------------------
-- 8) Estado das sequences SERIAL principais.
-- ---------------------------------------------------------------------------

SELECT
    'espetaculo_id_espetaculo_seq' AS sequence_name,
    last_value,
    is_called
FROM public.espetaculo_id_espetaculo_seq
UNION ALL
SELECT 'categoria_id_categoria_seq', last_value, is_called
FROM public.categoria_id_categoria_seq
UNION ALL
SELECT 'ator_id_ator_seq', last_value, is_called
FROM public.ator_id_ator_seq
UNION ALL
SELECT 'teatro_id_teatro_seq', last_value, is_called
FROM public.teatro_id_teatro_seq
UNION ALL
SELECT 'sessao_id_sessao_seq', last_value, is_called
FROM public.sessao_id_sessao_seq
ORDER BY sequence_name;
