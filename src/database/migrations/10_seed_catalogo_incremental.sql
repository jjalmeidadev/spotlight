-- 10_seed_catalogo_incremental.sql - Completa catalogo sem depender de ids fixos.
--
-- Use em ambientes Supabase parcialmente populados, onde o 02_initial_data.sql
-- nao pode ser reaplicado com seguranca porque os SERIALs ja avancaram.
--
-- O script e idempotente:
--   - insere categorias/atores/teatros/espetaculos faltantes por nome/titulo;
--   - atualiza campos principais dos espetaculos do seed;
--   - cria associacoes por JOIN em nome/titulo, nunca por id fixo;
--   - cria sessoes por titulo do espetaculo + nome do teatro.
--
-- Depois de aplicar, rode a migration 09_espetaculo_midia.sql.

BEGIN;

-- --------------------------------------------------------------------------
-- 1) Dominios publicos do catalogo.
-- --------------------------------------------------------------------------

INSERT INTO public.categoria (nome, descricao) VALUES
    ('Teatro Musical', 'Espetaculos que combinam atuacao, canto e danca em narrativa teatral.'),
    ('Teatro Infantil', 'Producoes voltadas ao publico infanto-juvenil, com linguagem acessivel e ludica.'),
    ('Ballet Classico', 'Apresentacoes de danca classica com tecnica e repertorio da tradicao europeia.'),
    ('Concerto e Show', 'Eventos musicais ao vivo, incluindo shows tematicos e concertos orquestrais.'),
    ('Dramaturgia', 'Pecas teatrais com foco em texto, narrativa e interpretacao dramatica.'),
    ('Show Musical', 'Espetaculos que celebram a obra de artistas ou movimentos musicais brasileiros.')
ON CONFLICT (nome) DO UPDATE SET
    descricao = EXCLUDED.descricao;

INSERT INTO public.ator (nome, biografia) VALUES
    ('Claudia Raia', 'Atriz, dancarina e apresentadora brasileira, referencia no teatro musical nacional.'),
    ('Jarbas Homem de Mello', 'Ator, cantor e danciarino, um dos maiores nomes do teatro musical no Brasil.'),
    ('Mel Lisboa', 'Atriz brasileira com trajetoria consolidada em teatro, cinema e televisao.'),
    ('Marcus Ornellas', 'Ator e cantor com ampla carreira em musicais e producoes televisivas.'),
    ('Jullie', 'Cantora e atriz revelada em producoes musicais de grande bilheteria.'),
    ('Victor Ferrari', 'Ator com atuacoes marcantes em producoes teatrais e televisivas.'),
    ('Izabel Buarque', 'Atriz e cantora com presenca consolidada nos palcos brasileiros.'),
    ('Debora Falabella', 'Atriz de grande expressao no teatro e televisao brasileiros.'),
    ('Sergio Malheiros', 'Ator com carreira de destaque em musicais e producoes audiovisuais.'),
    ('Marisa Orth', 'Atriz e comediante com vasta experiencia em teatro musical e televisao.')
ON CONFLICT DO NOTHING;

INSERT INTO public.teatro (
    nome, endereco, cidade, estado, capacidade, acessibilidade, parceiro_naming_rights
) VALUES
    ('BTG Pactual Hall', 'Av. das Nacoes Unidas, 4777 - Vila Olimpia', 'Sao Paulo', 'SP', 1500, TRUE, 'BTG Pactual'),
    ('Nubank Hall', 'Av. Paulista, 900 - Bela Vista', 'Sao Paulo', 'SP', 900, TRUE, 'Nubank'),
    ('Teatro Ri Happy', 'Rua Voluntarios da Patria, 45 - Botafogo', 'Rio de Janeiro', 'RJ', 600, TRUE, 'Ri Happy'),
    ('TotalEnergies Arena', 'Av. Afonso Pena, 1500 - Centro', 'Belo Horizonte', 'MG', 1200, TRUE, 'TotalEnergies')
ON CONFLICT DO NOTHING;

-- --------------------------------------------------------------------------
-- 2) Espetaculos. Titulo e tratado como chave natural deste seed incremental.
-- --------------------------------------------------------------------------

WITH dados (
    titulo, descricao, genero, faixa_etaria, duracao,
    classificacao_indicativa, produtora, status, popularidade, nota_media
) AS (
    VALUES
        ('Elis, a Musical', 'Espetaculo que celebra a trajetoria e obra de Elis Regina, com musicas que marcaram geracoes.', 'Teatro Musical', 'Livre', 110, 'Livre', 'Aventura Teatros', 'encerrado', 9.875, 0.00),
        ('A Novica Rebelde', 'A historia da familia Von Trapp ganha vida com musicas inesqueciveis nesta adaptacao da Broadway.', 'Teatro Musical', 'Livre', 130, 'Livre', 'Aventura Teatros', 'em_cartaz', 8.92, 0.00),
        ('Mamma Mia!', 'A comedia musical com os grandes hits do ABBA em uma producao deslumbrante e cheia de energia.', 'Teatro Musical', 'Livre', 120, '12 anos', 'Aventura Teatros', 'em_cartaz', 9.15, 0.00),
        ('Zaquim', 'Espetaculo infantil encantador que apresenta as criancas o universo do teatro musical com humor e emocao.', 'Teatro Infantil', '3 anos', 60, 'Livre', 'Aventura Teatros', 'em_cartaz', 7.43, 0.00),
        ('Pixar in Concert', 'Concerto orquestral ao vivo com as trilhas sonoras dos filmes mais amados da Pixar, em parceria com Disney-Pixar.', 'Concerto e Show', 'Livre', 90, 'Livre', 'Aventura Teatros', 'em_breve', 6.5, 0.00),
        ('Ayrton Senna - O Musical', 'A vida e a lenda de Ayrton Senna no palco em um musical emocionante sobre paixao, velocidade e legado.', 'Teatro Musical', '10 anos', 120, '10 anos', 'Aventura Teatros', 'encerrado', 9.31, 0.00),
        ('Romeu & Julieta, ao som de Marisa Monte', 'A tragedia de Shakespeare reinterpretada com a trilha sonora da incomparavel Marisa Monte.', 'Teatro Musical', '12 anos', 100, '12 anos', 'Aventura Teatros', 'encerrado', 8.76, 0.00),
        ('Musical Hairspray', 'O musical Broadway celebra diversidade, inclusao e amor ao som de hits dos anos 1960 em Baltimore.', 'Teatro Musical', 'Livre', 120, 'Livre', 'Aventura Teatros', 'encerrado', 8.45, 0.00),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Show musical que celebra o legado artistico do Clube da Esquina com as obras de Milton Nascimento e Lo Borges.', 'Concerto e Show', 'Livre', 100, 'Livre', 'Aventura Teatros', 'encerrado', 8.2, 0.00),
        ('Nossa Historia com Chico Buarque', 'Espetaculo musical que homenageia a obra de Chico Buarque, um dos maiores compositores brasileiros.', 'Show Musical', '14 anos', 110, '14 anos', 'Aventura Teatros', 'encerrado', 8.55, 0.00),
        ('Prima Facie com Debora Falabella', 'Peca teatral com Debora Falabella em monologo poderoso sobre o sistema judiciario e a experiencia feminina.', 'Dramaturgia', '16 anos', 90, '16 anos', 'Aventura Teatros', 'encerrado', 9.1, 0.00),
        ('A Cor Purpura, O Musical', 'Adaptacao do classico romance de Alice Walker, celebrando resiliencia, amor e empoderamento feminino negro.', 'Teatro Musical', '12 anos', 140, '12 anos', 'Aventura Teatros', 'encerrado', 8.8, 0.00),
        ('Bibi - Uma Vida em Musical', 'Espetaculo que homenageia a vida e a carreira de Bibi Ferreira, icone do teatro musical brasileiro.', 'Show Musical', 'Livre', 110, 'Livre', 'Aventura Teatros', 'encerrado', 8.6, 0.00),
        ('Hair', 'O classico musical de 1968 sobre liberdade, paz e contracultura ganha nova roupagem nos palcos brasileiros.', 'Teatro Musical', '16 anos', 120, '16 anos', 'Aventura Teatros', 'encerrado', 7.9, 0.00),
        ('Chacrinha, o Musical', 'Espetaculo que homenageia o iconico apresentador Chacrinha e sua influencia na cultura popular brasileira.', 'Show Musical', 'Livre', 100, 'Livre', 'Aventura Teatros', 'encerrado', 8.1, 0.00)
),
atualizados AS (
    UPDATE public.espetaculo e
    SET descricao = d.descricao,
        genero = d.genero,
        faixa_etaria = d.faixa_etaria,
        duracao = d.duracao,
        classificacao_indicativa = d.classificacao_indicativa,
        produtora = d.produtora,
        status = d.status::status_espetaculo_enum,
        popularidade = d.popularidade,
        nota_media = d.nota_media
    FROM dados d
    WHERE e.titulo = d.titulo
    RETURNING e.titulo
)
INSERT INTO public.espetaculo (
    titulo, descricao, genero, faixa_etaria, duracao,
    classificacao_indicativa, produtora, status,
    popularidade, nota_media, id_admin
)
SELECT
    d.titulo, d.descricao, d.genero, d.faixa_etaria, d.duracao,
    d.classificacao_indicativa, d.produtora, d.status::status_espetaculo_enum,
    d.popularidade, d.nota_media,
    '00000000-0000-0000-0000-000000000001'
FROM dados d
WHERE NOT EXISTS (
    SELECT 1
    FROM public.espetaculo e
    WHERE e.titulo = d.titulo
);

-- --------------------------------------------------------------------------
-- 3) Associacoes N:N por chave natural.
-- --------------------------------------------------------------------------

WITH rel(titulo, categoria) AS (
    VALUES
        ('Elis, a Musical', 'Teatro Musical'),
        ('A Novica Rebelde', 'Teatro Musical'),
        ('Mamma Mia!', 'Teatro Musical'),
        ('Zaquim', 'Teatro Infantil'),
        ('Pixar in Concert', 'Concerto e Show'),
        ('Pixar in Concert', 'Teatro Infantil'),
        ('Ayrton Senna - O Musical', 'Teatro Musical'),
        ('Romeu & Julieta, ao som de Marisa Monte', 'Teatro Musical'),
        ('Romeu & Julieta, ao som de Marisa Monte', 'Dramaturgia'),
        ('Musical Hairspray', 'Teatro Musical'),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Concerto e Show'),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Show Musical'),
        ('Nossa Historia com Chico Buarque', 'Show Musical'),
        ('Prima Facie com Debora Falabella', 'Dramaturgia'),
        ('A Cor Purpura, O Musical', 'Teatro Musical'),
        ('Bibi - Uma Vida em Musical', 'Show Musical'),
        ('Hair', 'Teatro Musical'),
        ('Chacrinha, o Musical', 'Show Musical')
)
INSERT INTO public.espetaculo_categoria (id_espetaculo, id_categoria)
SELECT e.id_espetaculo, c.id_categoria
FROM rel r
JOIN public.espetaculo e ON e.titulo = r.titulo
JOIN public.categoria c ON c.nome = r.categoria
ON CONFLICT DO NOTHING;

WITH rel(titulo, ator, papel, ordem_exibicao) AS (
    VALUES
        ('Elis, a Musical', 'Claudia Raia', 'Elis Regina', 1),
        ('Elis, a Musical', 'Jarbas Homem de Mello', 'Samuel', 2),
        ('A Novica Rebelde', 'Mel Lisboa', 'Maria', 1),
        ('A Novica Rebelde', 'Marcus Ornellas', 'Capitao Von Trapp', 2),
        ('Mamma Mia!', 'Jullie', 'Donna', 1),
        ('Mamma Mia!', 'Victor Ferrari', 'Sam', 2),
        ('Zaquim', 'Izabel Buarque', 'Narradora', 1),
        ('Ayrton Senna - O Musical', 'Jarbas Homem de Mello', 'Ayrton Senna', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'Mel Lisboa', 'Julieta', 1),
        ('Romeu & Julieta, ao som de Marisa Monte', 'Marcus Ornellas', 'Romeu', 2),
        ('Musical Hairspray', 'Sergio Malheiros', 'Corny Collins', 1),
        ('Musical Hairspray', 'Marisa Orth', 'Velma Von Tussle', 2),
        ('Prima Facie com Debora Falabella', 'Debora Falabella', 'Advogada', 1),
        ('A Cor Purpura, O Musical', 'Mel Lisboa', 'Celie', 1),
        ('Bibi - Uma Vida em Musical', 'Claudia Raia', 'Bibi Ferreira', 1),
        ('Hair', 'Sergio Malheiros', 'Berger', 1),
        ('Chacrinha, o Musical', 'Marisa Orth', 'Chacrinha', 1)
)
INSERT INTO public.espetaculo_ator (id_espetaculo, id_ator, papel, ordem_exibicao)
SELECT e.id_espetaculo, a.id_ator, r.papel, r.ordem_exibicao
FROM rel r
JOIN public.espetaculo e ON e.titulo = r.titulo
JOIN public.ator a ON a.nome = r.ator
ON CONFLICT (id_espetaculo, id_ator) DO UPDATE SET
    papel = EXCLUDED.papel,
    ordem_exibicao = EXCLUDED.ordem_exibicao;

-- --------------------------------------------------------------------------
-- 4) Sessoes dos espetaculos em cartaz/em breve.
-- --------------------------------------------------------------------------

WITH rel(titulo, teatro, data_offset, horario, preco_minimo, preco_maximo, disponibilidade, link_compra) AS (
    VALUES
        ('A Novica Rebelde', 'BTG Pactual Hall', 7, '20:00'::time, 120.00, 380.00, 800, 'https://bilheteria.aventura.com.br/novica-rebelde'),
        ('A Novica Rebelde', 'BTG Pactual Hall', 8, '20:00'::time, 120.00, 380.00, 600, 'https://bilheteria.aventura.com.br/novica-rebelde'),
        ('A Novica Rebelde', 'BTG Pactual Hall', 14, '19:00'::time, 120.00, 380.00, 1200, 'https://bilheteria.aventura.com.br/novica-rebelde'),
        ('A Novica Rebelde', 'BTG Pactual Hall', 21, '20:00'::time, 120.00, 380.00, 900, 'https://bilheteria.aventura.com.br/novica-rebelde'),
        ('A Novica Rebelde', 'BTG Pactual Hall', 22, '19:00'::time, 120.00, 380.00, 1100, 'https://bilheteria.aventura.com.br/novica-rebelde'),
        ('Mamma Mia!', 'Nubank Hall', 5, '21:00'::time, 150.00, 420.00, 500, 'https://bilheteria.aventura.com.br/mamma-mia'),
        ('Mamma Mia!', 'Nubank Hall', 6, '21:00'::time, 150.00, 420.00, 450, 'https://bilheteria.aventura.com.br/mamma-mia'),
        ('Mamma Mia!', 'Nubank Hall', 12, '19:30'::time, 150.00, 420.00, 700, 'https://bilheteria.aventura.com.br/mamma-mia'),
        ('Mamma Mia!', 'Nubank Hall', 13, '19:30'::time, 150.00, 420.00, 600, 'https://bilheteria.aventura.com.br/mamma-mia'),
        ('Mamma Mia!', 'Nubank Hall', 19, '21:00'::time, 150.00, 420.00, 800, 'https://bilheteria.aventura.com.br/mamma-mia'),
        ('Zaquim', 'Teatro Ri Happy', 3, '15:00'::time, 60.00, 180.00, 350, 'https://bilheteria.aventura.com.br/zaquim'),
        ('Zaquim', 'Teatro Ri Happy', 4, '11:00'::time, 60.00, 180.00, 400, 'https://bilheteria.aventura.com.br/zaquim'),
        ('Zaquim', 'Teatro Ri Happy', 10, '15:00'::time, 60.00, 180.00, 380, 'https://bilheteria.aventura.com.br/zaquim'),
        ('Zaquim', 'Teatro Ri Happy', 11, '11:00'::time, 60.00, 180.00, 420, 'https://bilheteria.aventura.com.br/zaquim'),
        ('Zaquim', 'Teatro Ri Happy', 17, '15:00'::time, 60.00, 180.00, 360, 'https://bilheteria.aventura.com.br/zaquim'),
        ('Pixar in Concert', 'TotalEnergies Arena', 30, '20:00'::time, 200.00, 600.00, 1000, 'https://bilheteria.aventura.com.br/pixar-concert'),
        ('Pixar in Concert', 'TotalEnergies Arena', 31, '20:00'::time, 200.00, 600.00, 1100, 'https://bilheteria.aventura.com.br/pixar-concert'),
        ('Pixar in Concert', 'TotalEnergies Arena', 37, '19:00'::time, 200.00, 600.00, 950, 'https://bilheteria.aventura.com.br/pixar-concert'),
        ('Pixar in Concert', 'TotalEnergies Arena', 38, '19:00'::time, 200.00, 600.00, 1050, 'https://bilheteria.aventura.com.br/pixar-concert'),
        ('Pixar in Concert', 'TotalEnergies Arena', 44, '20:00'::time, 200.00, 600.00, 1200, 'https://bilheteria.aventura.com.br/pixar-concert')
)
INSERT INTO public.sessao (
    id_espetaculo, id_teatro, data, horario,
    preco_minimo, preco_maximo, disponibilidade, link_compra
)
SELECT
    e.id_espetaculo,
    t.id_teatro,
    CURRENT_DATE + r.data_offset,
    r.horario,
    r.preco_minimo,
    r.preco_maximo,
    r.disponibilidade,
    r.link_compra
FROM rel r
JOIN public.espetaculo e ON e.titulo = r.titulo
JOIN public.teatro t ON t.nome = r.teatro
ON CONFLICT DO NOTHING;

COMMIT;
