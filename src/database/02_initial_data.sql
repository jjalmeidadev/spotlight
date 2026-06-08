BEGIN;

INSERT INTO auth.users (
    id, aud, role, email, encrypted_password,
    email_confirmed_at, raw_user_meta_data, created_at, updated_at
) VALUES
    (
        '00000000-0000-0000-0000-000000000001',
        'authenticated', 'authenticated',
        'admin@aventura.com.br',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Admin Aventura"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000002',
        'authenticated', 'authenticated',
        'ana.clara@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Ana Clara Amaral"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000003',
        'authenticated', 'authenticated',
        'carlos.nobrega@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Carlos Nobrega"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000004',
        'authenticated', 'authenticated',
        'mariana.lacerda@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Mariana Lacerda Souza"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000005',
        'authenticated', 'authenticated',
        'pedro.lima@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Pedro Henrique Lima"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000006',
        'authenticated', 'authenticated',
        'juliana.santos@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Juliana Santos"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000007',
        'authenticated', 'authenticated',
        'rodrigo.alves@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Rodrigo Alves"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000008',
        'authenticated', 'authenticated',
        'beatriz.melo@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Beatriz Melo"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000009',
        'authenticated', 'authenticated',
        'thiago.costa@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Thiago Costa"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000010',
        'authenticated', 'authenticated',
        'camila.ferreira@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Camila Ferreira"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000011',
        'authenticated', 'authenticated',
        'lucas.oliveira@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Lucas Oliveira"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000012',
        'authenticated', 'authenticated',
        'amanda.ribeiro@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Amanda Ribeiro"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000013',
        'authenticated', 'authenticated',
        'gustavo.souza@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Gustavo Souza"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000014',
        'authenticated', 'authenticated',
        'fernanda.lima@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Fernanda Lima"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000015',
        'authenticated', 'authenticated',
        'rafael.carvalho@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Rafael Carvalho"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000016',
        'authenticated', 'authenticated',
        'priscila.mendes@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Priscila Mendes"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000017',
        'authenticated', 'authenticated',
        'eduardo.nunes@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Eduardo Nunes"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000018',
        'authenticated', 'authenticated',
        'leticia.rocha@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Leticia Rocha"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000019',
        'authenticated', 'authenticated',
        'vinicius.martins@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Vinicius Martins"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000020',
        'authenticated', 'authenticated',
        'carolina.andrade@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Carolina Andrade"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000021',
        'authenticated', 'authenticated',
        'felipe.gomes@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Felipe Gomes"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000022',
        'authenticated', 'authenticated',
        'natalia.freitas@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Natalia Freitas"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000023',
        'authenticated', 'authenticated',
        'andre.barbosa@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Andre Barbosa"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000024',
        'authenticated', 'authenticated',
        'paula.teixeira@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Paula Teixeira"}'::jsonb,
        NOW(), NOW()
    ),
    (
        '00000000-0000-0000-0000-000000000025',
        'authenticated', 'authenticated',
        'marcos.pereira@email.com',
        crypt('Spotlight@2025!', gen_salt('bf', 10)),
        NOW(),
        '{"nome": "Marcos Pereira"}'::jsonb,
        NOW(), NOW()
    )
ON CONFLICT (id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- Atualizações dos perfis em public.usuario (criados via trigger on_auth_user_created).
--
-- O campo `localizacao` segue a estrutura JSONB real do contrato — mesmas 5
-- chaves em camelCase produzidas pelo NominatimGeocodingService:
--   { formattedAddress, latitude, longitude, placeId, provider }
--
-- Convenções deste seed:
--   - `provider = "seed"` distingue dados de bootstrap dos dados geocodificados
--     pelo serviço em runtime (que usam `"openstreetmap_nominatim"`).
--   - `placeId = ""` porque não há documento Nominatim associado neste estágio.
--   - `latitude`/`longitude` são aproximações centradas no município (precisão
--     suficiente para testes de filtros e recomendações geográficas).
-- Especificação completa em src/database/README.md.
-- ---------------------------------------------------------------------------

UPDATE usuario SET
    data_nascimento     = '1985-06-10',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'administrador',
    nivel_acesso        = '{"can_manage_shows": true, "can_manage_users": true}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Ballet Classico"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000001';

UPDATE usuario SET
    data_nascimento     = '2007-11-22',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Dramaturgia"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000002';

UPDATE usuario SET
    data_nascimento     = '1958-04-30',
    localizacao         = '{"formattedAddress": "Rio de Janeiro, RJ", "latitude": -22.9068, "longitude": -43.1729, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Infantil", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000003';

UPDATE usuario SET
    data_nascimento     = '1990-07-08',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Infantil"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000004';

UPDATE usuario SET
    data_nascimento     = '2002-05-14',
    localizacao         = '{"formattedAddress": "Belo Horizonte, MG", "latitude": -19.9167, "longitude": -43.9345, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000005';

UPDATE usuario SET
    data_nascimento     = '1995-03-22',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Show Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000006';

UPDATE usuario SET
    data_nascimento     = '1988-09-15',
    localizacao         = '{"formattedAddress": "Rio de Janeiro, RJ", "latitude": -22.9068, "longitude": -43.1729, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Dramaturgia"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000007';

UPDATE usuario SET
    data_nascimento     = '1999-12-01',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000008';

UPDATE usuario SET
    data_nascimento     = '1975-07-19',
    localizacao         = '{"formattedAddress": "Porto Alegre, RS", "latitude": -30.0346, "longitude": -51.2177, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Concerto e Show", "Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000009';

UPDATE usuario SET
    data_nascimento     = '2001-02-28',
    localizacao         = '{"formattedAddress": "Curitiba, PR", "latitude": -25.4284, "longitude": -49.2733, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Ballet Classico", "Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000010';

UPDATE usuario SET
    data_nascimento     = '1992-08-04',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Infantil", "Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000011';

UPDATE usuario SET
    data_nascimento     = '1985-11-17',
    localizacao         = '{"formattedAddress": "Rio de Janeiro, RJ", "latitude": -22.9068, "longitude": -43.1729, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Infantil", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000012';

UPDATE usuario SET
    data_nascimento     = '1970-01-30',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Dramaturgia", "Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000013';

UPDATE usuario SET
    data_nascimento     = '2003-06-25',
    localizacao         = '{"formattedAddress": "Salvador, BA", "latitude": -12.9714, "longitude": -38.5014, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Show Musical", "Teatro Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000014';

UPDATE usuario SET
    data_nascimento     = '1980-04-12',
    localizacao         = '{"formattedAddress": "Belo Horizonte, MG", "latitude": -19.9167, "longitude": -43.9345, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Dramaturgia"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000015';

UPDATE usuario SET
    data_nascimento     = '1993-10-08',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Concerto e Show", "Show Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000016';

UPDATE usuario SET
    data_nascimento     = '1967-03-21',
    localizacao         = '{"formattedAddress": "Rio de Janeiro, RJ", "latitude": -22.9068, "longitude": -43.1729, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000017';

UPDATE usuario SET
    data_nascimento     = '1998-07-14',
    localizacao         = '{"formattedAddress": "Florianópolis, SC", "latitude": -27.5954, "longitude": -48.5480, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Show Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000018';

UPDATE usuario SET
    data_nascimento     = '2000-09-03',
    localizacao         = '{"formattedAddress": "Recife, PE", "latitude": -8.0539, "longitude": -34.8811, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000019';

UPDATE usuario SET
    data_nascimento     = '1987-12-19',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Dramaturgia", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000020';

UPDATE usuario SET
    data_nascimento     = '1994-05-07',
    localizacao         = '{"formattedAddress": "Brasília, DF", "latitude": -15.7801, "longitude": -47.9292, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Teatro Infantil"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000021';

UPDATE usuario SET
    data_nascimento     = '1979-08-23',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Show Musical", "Concerto e Show"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000022';

UPDATE usuario SET
    data_nascimento     = '2004-01-16',
    localizacao         = '{"formattedAddress": "Rio de Janeiro, RJ", "latitude": -22.9068, "longitude": -43.1729, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Dramaturgia"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000023';

UPDATE usuario SET
    data_nascimento     = '1991-04-09',
    localizacao         = '{"formattedAddress": "Campinas, SP", "latitude": -22.9099, "longitude": -47.0626, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Teatro Musical", "Ballet Classico"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000024';

UPDATE usuario SET
    data_nascimento     = '1983-11-02',
    localizacao         = '{"formattedAddress": "São Paulo, SP", "latitude": -23.5505, "longitude": -46.6333, "placeId": "", "provider": "seed"}'::jsonb,
    tipo_usuario        = 'cadastrado',
    nivel_acesso        = '{}'::jsonb,
    perfil_preferencias = '{"generos": ["Concerto e Show", "Show Musical"]}'::jsonb
WHERE id_usuario = '00000000-0000-0000-0000-000000000025';

INSERT INTO categoria (nome, descricao) VALUES
    ('Teatro Musical', 'Espetaculos que combinam atuacao, canto e danca em narrativa teatral.'),
    ('Teatro Infantil', 'Producoes voltadas ao publico infanto-juvenil, com linguagem acessivel e ludica.'),
    ('Ballet Classico', 'Apresentacoes de danca classica com tecnica e repertorio da tradicao europeia.'),
    ('Concerto e Show', 'Eventos musicais ao vivo, incluindo shows tematicos e concertos orquestrais.'),
    ('Dramaturgia', 'Pecas teatrais com foco em texto, narrativa e interpretacao dramatica.'),
    ('Show Musical', 'Espetaculos que celebram a obra de artistas ou movimentos musicais brasileiros.')
ON CONFLICT (nome) DO NOTHING;

INSERT INTO ator (nome, biografia) VALUES
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

INSERT INTO espetaculo (
    titulo, descricao, genero, faixa_etaria, duracao,
    classificacao_indicativa, produtora, status,
    popularidade, nota_media, id_admin
) VALUES
    ('Elis, a Musical', 'Espetaculo que celebra a trajetoria e obra de Elis Regina, com musicas que marcaram geracoes.', 'Teatro Musical', 'Livre', 110, 'Livre', 'Aventura Teatros', 'encerrado', 9.875, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('A Novica Rebelde', 'A historia da familia Von Trapp ganha vida com musicas inesqueciveis nesta adaptacao da Broadway.', 'Teatro Musical', 'Livre', 130, 'Livre', 'Aventura Teatros', 'em_cartaz', 8.92, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Mamma Mia!', 'A comedia musical com os grandes hits do ABBA em uma producao deslumbrante e cheia de energia.', 'Teatro Musical', 'Livre', 120, '12 anos', 'Aventura Teatros', 'em_cartaz', 9.15, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Zaquim', 'Espetaculo infantil encantador que apresenta as criancas o universo do teatro musical com humor e emocao.', 'Teatro Infantil', '3 anos', 60, 'Livre', 'Aventura Teatros', 'em_cartaz', 7.43, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Pixar in Concert', 'Concerto orquestral ao vivo com as trilhas sonoras dos filmes mais amados da Pixar, em parceria com Disney-Pixar.', 'Concerto e Show', 'Livre', 90, 'Livre', 'Aventura Teatros', 'em_breve', 6.5, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Ayrton Senna - O Musical', 'A vida e a lenda de Ayrton Senna no palco em um musical emocionante sobre paixao, velocidade e legado.', 'Teatro Musical', '10 anos', 120, '10 anos', 'Aventura Teatros', 'encerrado', 9.31, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Romeu & Julieta, ao som de Marisa Monte', 'A tragedia de Shakespeare reinterpretada com a trilha sonora da incomparavel Marisa Monte.', 'Teatro Musical', '12 anos', 100, '12 anos', 'Aventura Teatros', 'encerrado', 8.76, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Musical Hairspray', 'O musical Broadway celebra diversidade, inclusao e amor ao som de hits dos anos 1960 em Baltimore.', 'Teatro Musical', 'Livre', 120, 'Livre', 'Aventura Teatros', 'encerrado', 8.45, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Show musical que celebra o legado artistico do Clube da Esquina com as obras de Milton Nascimento e Lo Borges.', 'Concerto e Show', 'Livre', 100, 'Livre', 'Aventura Teatros', 'encerrado', 8.2, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Nossa Historia com Chico Buarque', 'Espetaculo musical que homenageia a obra de Chico Buarque, um dos maiores compositores brasileiros.', 'Show Musical', '14 anos', 110, '14 anos', 'Aventura Teatros', 'encerrado', 8.55, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Prima Facie com Debora Falabella', 'Peca teatral com Debora Falabella em monólogo poderoso sobre o sistema judiciario e a experiencia feminina.', 'Dramaturgia', '16 anos', 90, '16 anos', 'Aventura Teatros', 'encerrado', 9.1, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('A Cor Purpura, O Musical', 'Adaptacao do classico romance de Alice Walker, celebrando resiliencia, amor e empoderamento feminino negro.', 'Teatro Musical', '12 anos', 140, '12 anos', 'Aventura Teatros', 'encerrado', 8.8, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Bibi - Uma Vida em Musical', 'Espetaculo que homenageia a vida e a carreira de Bibi Ferreira, icone do teatro musical brasileiro.', 'Show Musical', 'Livre', 110, 'Livre', 'Aventura Teatros', 'encerrado', 8.6, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Hair', 'O classico musical de 1968 sobre liberdade, paz e contracultura ganha nova roupagem nos palcos brasileiros.', 'Teatro Musical', '16 anos', 120, '16 anos', 'Aventura Teatros', 'encerrado', 7.9, 0.00, '00000000-0000-0000-0000-000000000001'),
    ('Chacrinha, o Musical', 'Espetaculo que homenageia o icônico apresentador Chacrinha e sua influencia na cultura popular brasileira.', 'Show Musical', 'Livre', 100, 'Livre', 'Aventura Teatros', 'encerrado', 8.1, 0.00, '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

INSERT INTO espetaculo_categoria (id_espetaculo, id_categoria) VALUES
    (1, 1),
    (2, 1),
    (3, 1),
    (4, 2),
    (5, 4),
    (5, 2),
    (6, 1),
    (7, 1),
    (7, 5),
    (8, 1),
    (9, 4),
    (9, 6),
    (10, 6),
    (11, 5),
    (12, 1),
    (13, 6),
    (14, 1),
    (15, 6)
ON CONFLICT DO NOTHING;

INSERT INTO espetaculo_ator (id_espetaculo, id_ator, papel, ordem_exibicao) VALUES
    (1, 1, 'Elis Regina', 1),
    (1, 2, 'Samuel', 2),
    (2, 3, 'Maria', 1),
    (2, 4, 'Capitao Von Trapp', 2),
    (3, 5, 'Donna', 1),
    (3, 6, 'Sam', 2),
    (4, 7, 'Narradora', 1),
    (6, 2, 'Ayrton Senna', 1),
    (7, 3, 'Julieta', 1),
    (7, 4, 'Romeu', 2),
    (8, 9, 'Corny Collins', 1),
    (8, 10, 'Velma Von Tussle', 2),
    (11, 8, 'Advogada', 1),
    (12, 3, 'Celie', 1),
    (13, 1, 'Bibi Ferreira', 1),
    (14, 9, 'Berger', 1),
    (15, 10, 'Chacrinha', 1)
ON CONFLICT DO NOTHING;

INSERT INTO teatro (nome, endereco, cidade, estado, capacidade, acessibilidade, parceiro_naming_rights) VALUES
    ('BTG Pactual Hall', 'Av. das Nacoes Unidas, 4777 - Vila Olimpia', 'Sao Paulo', 'SP', 1500, TRUE, 'BTG Pactual'),
    ('Nubank Hall', 'Av. Paulista, 900 - Bela Vista', 'Sao Paulo', 'SP', 900, TRUE, 'Nubank'),
    ('Teatro Ri Happy', 'Rua Voluntarios da Patria, 45 - Botafogo', 'Rio de Janeiro', 'RJ', 600, TRUE, 'Ri Happy'),
    ('TotalEnergies Arena', 'Av. Afonso Pena, 1500 - Centro', 'Belo Horizonte', 'MG', 1200, TRUE, 'TotalEnergies')
ON CONFLICT DO NOTHING;

INSERT INTO sessao (id_espetaculo, id_teatro, data, horario, preco_minimo, preco_maximo, disponibilidade, link_compra) VALUES
    (2, 1, CURRENT_DATE +  7, '20:00', 120.00, 380.00, 800, 'https://bilheteria.aventura.com.br/novica-rebelde'),
    (2, 1, CURRENT_DATE +  8, '20:00', 120.00, 380.00, 600, 'https://bilheteria.aventura.com.br/novica-rebelde'),
    (2, 1, CURRENT_DATE + 14, '19:00', 120.00, 380.00, 1200, 'https://bilheteria.aventura.com.br/novica-rebelde'),
    (2, 1, CURRENT_DATE + 21, '20:00', 120.00, 380.00, 900, 'https://bilheteria.aventura.com.br/novica-rebelde'),
    (2, 1, CURRENT_DATE + 22, '19:00', 120.00, 380.00, 1100, 'https://bilheteria.aventura.com.br/novica-rebelde'),
    (3, 2, CURRENT_DATE +  5, '21:00', 150.00, 420.00, 500, 'https://bilheteria.aventura.com.br/mamma-mia'),
    (3, 2, CURRENT_DATE +  6, '21:00', 150.00, 420.00, 450, 'https://bilheteria.aventura.com.br/mamma-mia'),
    (3, 2, CURRENT_DATE + 12, '19:30', 150.00, 420.00, 700, 'https://bilheteria.aventura.com.br/mamma-mia'),
    (3, 2, CURRENT_DATE + 13, '19:30', 150.00, 420.00, 600, 'https://bilheteria.aventura.com.br/mamma-mia'),
    (3, 2, CURRENT_DATE + 19, '21:00', 150.00, 420.00, 800, 'https://bilheteria.aventura.com.br/mamma-mia'),
    (4, 3, CURRENT_DATE +  3, '15:00', 60.00, 180.00, 350, 'https://bilheteria.aventura.com.br/zaquim'),
    (4, 3, CURRENT_DATE +  4, '11:00', 60.00, 180.00, 400, 'https://bilheteria.aventura.com.br/zaquim'),
    (4, 3, CURRENT_DATE + 10, '15:00', 60.00, 180.00, 380, 'https://bilheteria.aventura.com.br/zaquim'),
    (4, 3, CURRENT_DATE + 11, '11:00', 60.00, 180.00, 420, 'https://bilheteria.aventura.com.br/zaquim'),
    (4, 3, CURRENT_DATE + 17, '15:00', 60.00, 180.00, 360, 'https://bilheteria.aventura.com.br/zaquim'),
    (5, 4, CURRENT_DATE + 30, '20:00', 200.00, 600.00, 1000, 'https://bilheteria.aventura.com.br/pixar-concert'),
    (5, 4, CURRENT_DATE + 31, '20:00', 200.00, 600.00, 1100, 'https://bilheteria.aventura.com.br/pixar-concert'),
    (5, 4, CURRENT_DATE + 37, '19:00', 200.00, 600.00, 950, 'https://bilheteria.aventura.com.br/pixar-concert'),
    (5, 4, CURRENT_DATE + 38, '19:00', 200.00, 600.00, 1050, 'https://bilheteria.aventura.com.br/pixar-concert'),
    (5, 4, CURRENT_DATE + 44, '20:00', 200.00, 600.00, 1200, 'https://bilheteria.aventura.com.br/pixar-concert')
ON CONFLICT DO NOTHING;

INSERT INTO avaliacao (id_usuario, id_espetaculo, nota, comentario) VALUES
    ('00000000-0000-0000-0000-000000000001', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000001', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000001', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000001', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000002', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000002', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000002', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000002', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000003', 4, 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
    ('00000000-0000-0000-0000-000000000003', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000003', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000003', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000004', 4, 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
    ('00000000-0000-0000-0000-000000000004', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000004', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000004', 13, 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
    ('00000000-0000-0000-0000-000000000005', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000005', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000005', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000005', 10, 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
    ('00000000-0000-0000-0000-000000000006', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000006', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000006', 8, 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
    ('00000000-0000-0000-0000-000000000006', 12, 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
    ('00000000-0000-0000-0000-000000000007', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000007', 11, 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
    ('00000000-0000-0000-0000-000000000007', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000007', 14, 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
    ('00000000-0000-0000-0000-000000000008', 1, 4, 'Otima producao, elenco talentoso e trilha sonora impecavel.'),
    ('00000000-0000-0000-0000-000000000008', 2, 3, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000008', 8, 3, 'Bom espetaculo, mas esperava mais profundidade dramatica.'),
    ('00000000-0000-0000-0000-000000000008', 13, 3, 'Bom espetaculo, roteiro poderia explorar mais a obra da homenageada.'),
    ('00000000-0000-0000-0000-000000000009', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000009', 10, 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
    ('00000000-0000-0000-0000-000000000009', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000009', 15, 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000010', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000010', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000010', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000010', 4, 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
    ('00000000-0000-0000-0000-000000000011', 4, 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
    ('00000000-0000-0000-0000-000000000011', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000011', 13, 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
    ('00000000-0000-0000-0000-000000000011', 15, 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000012', 4, 4, 'Divertido e acessivel, toda a familia se encantou.'),
    ('00000000-0000-0000-0000-000000000012', 3, 4, 'Energia contagiante, musicas que ficam na cabeca por dias.'),
    ('00000000-0000-0000-0000-000000000012', 9, 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
    ('00000000-0000-0000-0000-000000000012', 12, 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
    ('00000000-0000-0000-0000-000000000013', 11, 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
    ('00000000-0000-0000-0000-000000000013', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000013', 14, 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
    ('00000000-0000-0000-0000-000000000013', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000014', 10, 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
    ('00000000-0000-0000-0000-000000000014', 12, 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
    ('00000000-0000-0000-0000-000000000014', 13, 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
    ('00000000-0000-0000-0000-000000000014', 15, 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000015', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000015', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000015', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000015', 8, 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
    ('00000000-0000-0000-0000-000000000016', 9, 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
    ('00000000-0000-0000-0000-000000000016', 10, 3, 'Bom espetaculo, mas senti falta de mais interacao com o publico.'),
    ('00000000-0000-0000-0000-000000000016', 13, 3, 'Bom espetaculo, roteiro poderia explorar mais a obra da homenageada.'),
    ('00000000-0000-0000-0000-000000000016', 15, 2, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000017', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000017', 10, 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
    ('00000000-0000-0000-0000-000000000017', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000017', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000018', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000018', 8, 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
    ('00000000-0000-0000-0000-000000000018', 12, 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
    ('00000000-0000-0000-0000-000000000018', 13, 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
    ('00000000-0000-0000-0000-000000000019', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000019', 6, 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
    ('00000000-0000-0000-0000-000000000019', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000019', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000020', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000020', 11, 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
    ('00000000-0000-0000-0000-000000000020', 10, 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
    ('00000000-0000-0000-0000-000000000020', 14, 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
    ('00000000-0000-0000-0000-000000000021', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000021', 4, 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
    ('00000000-0000-0000-0000-000000000021', 9, 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
    ('00000000-0000-0000-0000-000000000021', 11, 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
    ('00000000-0000-0000-0000-000000000022', 10, 3, 'Bom espetaculo, mas senti falta de mais interacao com o publico.'),
    ('00000000-0000-0000-0000-000000000022', 12, 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
    ('00000000-0000-0000-0000-000000000022', 2, 3, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000022', 8, 3, 'Bom espetaculo, mas esperava mais profundidade dramatica.'),
    ('00000000-0000-0000-0000-000000000023', 3, 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
    ('00000000-0000-0000-0000-000000000023', 7, 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
    ('00000000-0000-0000-0000-000000000023', 14, 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
    ('00000000-0000-0000-0000-000000000023', 15, 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000024', 1, 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
    ('00000000-0000-0000-0000-000000000024', 8, 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
    ('00000000-0000-0000-0000-000000000024', 2, 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
    ('00000000-0000-0000-0000-000000000024', 11, 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
    ('00000000-0000-0000-0000-000000000025', 9, 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
    ('00000000-0000-0000-0000-000000000025', 12, 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
    ('00000000-0000-0000-0000-000000000025', 15, 2, 'Entretenimento descompromissado, valeu pela nostalgia.'),
    ('00000000-0000-0000-0000-000000000025', 6, 4, 'Jarbas Homem de Mello entrega uma atuacao a altura da lenda.')
ON CONFLICT (id_usuario, id_espetaculo) DO NOTHING;

INSERT INTO recomendacao (
    id_usuario, id_espetaculo, tipo_algoritmo,
    score_recomendacao, motivo_recomendacao,
    prioridade_manual, referencia_origem_id
) VALUES
    ('00000000-0000-0000-0000-000000000002', 3, 'k-NN', 0.9215, 'Usuarios com perfil similar avaliaram positivamente Mamma Mia!', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000003', 5, 'k-NN', 0.845, 'Pixar in Concert alinhado ao perfil familiar e musical do usuario.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000004', 5, 'k-NN', 0.91, 'Pixar in Concert ideal para o perfil familiar identificado.', TRUE, NULL),
    ('00000000-0000-0000-0000-000000000005', 1, 'SVD', 0.9356, 'Elis, a Musical com altissima predicao de afinidade para o perfil.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000006', 7, 'SVD', 0.883, 'Romeu e Julieta recomendado por fatoracao matricial de afinidade latente.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000007', 8, 'k-NN', 0.8672, 'Hairspray apontado por vizinhos com perfil analogo de dramaturgia.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000008', 6, 'SVD', 0.891, 'Ayrton Senna - O Musical com alta predicao de afinidade para o perfil.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000009', 2, 'k-NN', 0.812, 'A Novica Rebelde indicada por usuarios com perfil de show ao vivo.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000010', 6, 'k-NN', 0.876, 'Ayrton Senna apontado por similaridade com avaliacoes recentes.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000011', 5, 'k-NN', 0.905, 'Pixar in Concert ideal para perfis com preferencia infantil.', TRUE, NULL),
    ('00000000-0000-0000-0000-000000000012', 2, 'SVD', 0.788, 'A Novica Rebelde recomendada por similaridade com avaliacoes do cluster.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000013', 12, 'k-means', 0.801, 'Segmento Dramaturgos - A Cor Purpura com alta aprovacao do cluster.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000014', 3, 'k-NN', 0.856, 'Mamma Mia! indicado por usuarios com perfil de show musical.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000015', 3, 'SVD', 0.892, 'Mamma Mia! com alta predicao de afinidade por fatoracao matricial.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000016', 2, 'k-NN', 0.834, 'A Novica Rebelde alinhada ao perfil de shows ao vivo do usuario.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000017', 1, 'SVD', 0.921, 'Elis, a Musical com altissima predicao para perfil de show musical.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000018', 6, 'k-NN', 0.867, 'Ayrton Senna apontado por vizinhos com perfil analogo.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000019', 2, 'k-NN', 0.879, 'A Novica Rebelde indicada por alta similaridade com avaliacoes anteriores.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000020', 9, 'k-means', 0.812, 'Segmento MPB - Clube da Esquina com alta aprovacao do cluster.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000021', 12, 'SVD', 0.845, 'A Cor Purpura recomendada por fatoracao matricial de afinidade latente.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000022', 1, 'k-NN', 0.898, 'Elis, a Musical apontada por usuarios com perfil de show musical.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000023', 8, 'k-NN', 0.863, 'Hairspray indicado por alta similaridade com avaliacoes do usuario.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000024', 7, 'SVD', 0.877, 'Romeu e Julieta recomendado por afinidade latente identificada.', FALSE, NULL),
    ('00000000-0000-0000-0000-000000000025', 1, 'k-NN', 0.891, 'Elis, a Musical apontada por similaridade com perfil de shows musicais.', FALSE, NULL)
ON CONFLICT DO NOTHING;

INSERT INTO similaridade_item (
    id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo, score_similaridade
) VALUES
    -- Similares de "Elis, a Musical" (1) — biografias musicais brasileiras.
    (1, 6, 'k-NN', 0.8945),
    (1, 9, 'k-NN', 0.8612),
    (1, 3, 'k-NN', 0.7421),
    -- Similares de "A Novica Rebelde" (2) — musicais clássicos.
    (2, 3, 'k-NN', 0.8732),
    (2, 8, 'k-NN', 0.8104),
    (2, 15, 'k-NN', 0.7896),
    -- Similares de "Mamma Mia!" (3) — musicais leves/populares.
    (3, 8, 'k-NN', 0.8821),
    (3, 15, 'k-NN', 0.8503),
    (3, 2, 'k-NN', 0.8312),
    -- Similares de "Pixar in Concert" (5) — concertos/famílias.
    (5, 9, 'k-NN', 0.7615),
    (5, 3, 'k-NN', 0.6932),
    -- Similares de "Ayrton Senna - O Musical" (6) — biografias musicais.
    (6, 1, 'k-NN', 0.8945),
    (6, 9, 'k-NN', 0.8214),
    -- Similares de "Romeu e Julieta" (7) — dramaturgia clássica.
    (7, 12, 'k-NN', 0.8123),
    (7, 11, 'k-NN', 0.7842),
    -- Similares de "Hairspray" (8) — musicais populares.
    (8, 3, 'k-NN', 0.8821),
    (8, 15, 'k-NN', 0.8456),
    (8, 2, 'k-NN', 0.7912),
    -- Similares de "Clube da Esquina" (9) — música popular brasileira.
    (9, 1, 'k-NN', 0.8612),
    (9, 6, 'k-NN', 0.8214),
    -- Similares de "Morte e Vida Severina" (11) — dramaturgia brasileira.
    (11, 12, 'k-NN', 0.8421),
    (11, 7, 'k-NN', 0.7842),
    -- Similares de "A Cor Púrpura" (12) — dramas musicais.
    (12, 11, 'k-NN', 0.8421),
    (12, 7, 'k-NN', 0.8123),
    (12, 8, 'k-NN', 0.7456),
    -- Similares de "Hairspray" (15 — entrada extra) — entretenimento musical.
    (15, 3, 'k-NN', 0.8503),
    (15, 8, 'k-NN', 0.8456)
ON CONFLICT (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo) DO NOTHING;

UPDATE espetaculo
SET nota_media = sub.media
FROM (
    SELECT id_espetaculo, ROUND(AVG(nota)::NUMERIC, 2) AS media
    FROM avaliacao
    GROUP BY id_espetaculo
) AS sub
WHERE espetaculo.id_espetaculo = sub.id_espetaculo;

COMMIT;