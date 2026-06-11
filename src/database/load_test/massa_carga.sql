-- ===========================================================================
-- Massa de dados para teste de carga — Spotlight
-- ---------------------------------------------------------------------------
-- Gera volume sintético e representativo sobre o schema já criado
-- (01_create_database.sql) e suas migrations (04–08), para tornar os testes
-- de carga e performance relevantes.
--
-- Pré-requisitos (DoR):
--   - Schema criado e migrations 04–08 aplicadas (UNIQUE de sessao e de
--     recomendacao são usados pelos ON CONFLICT abaixo).
--   - Extensão pgcrypto disponível (já habilitada em 00_supabase_compat.sql).
--
-- Volume gerado (DoD) — ~208 mil linhas no total:
--   usuarios .......... 5.000   (via auth.users + trigger on_auth_user_created)
--   atores ............ 1.000
--   teatros ...........   100
--   espetaculos ....... 2.000
--   sessoes ........... 20.000
--   avaliacoes ........ 100.000
--   recomendacoes ..... 50.000
--   similaridade_item . 30.000
--
-- Para ESCALAR o volume, altere as constantes no bloco DECLARE abaixo.
--
-- Isolamento dos dados de carga (para limpeza seletiva via rollback):
--   - usuarios:    email LIKE '%@loadtest.local' e UUID prefixo 10000000-...
--   - espetaculos: produtora = 'Carga LoadTest'
--   - teatros:     parceiro_naming_rights = 'Carga LoadTest'
--   - atores:      nome LIKE 'Ator Carga %'
--   Sessões, avaliações, recomendações e similaridades só referenciam as
--   entidades de carga acima, então são removidas em cascata pelo rollback.
--
-- Execução (psql):
--   psql "$DATABASE_URL" -f src/database/load_test/massa_carga.sql
-- Também executável via SQL Editor do Supabase / DBeaver (sem metacomandos psql).
--
-- Reset: rode src/database/load_test/massa_carga_rollback.sql antes de recarregar.
-- ===========================================================================

BEGIN;

DO $$
DECLARE
    -- Volume gerado (edite para escalar a massa) ---------------------------
    n_usuarios      INTEGER := 5000;
    n_atores        INTEGER := 1000;
    n_teatros       INTEGER := 100;
    n_espetaculos   INTEGER := 2000;
    n_sessoes       INTEGER := 20000;
    n_avaliacoes    INTEGER := 100000;
    n_recomendacoes INTEGER := 50000;
    n_similaridades INTEGER := 30000;

    -- Hash de senha calculado UMA vez e reaproveitado (evita custo de bcrypt
    -- por linha; dados de carga não precisam de senhas distintas).
    v_senha_hash    TEXT;
BEGIN
    -- Guarda de idempotência: evita duplicar a massa em reexecuções.
    IF EXISTS (SELECT 1 FROM usuario WHERE email LIKE '%@loadtest.local') THEN
        RAISE EXCEPTION 'Massa de carga já carregada. Rode massa_carga_rollback.sql antes de recarregar.';
    END IF;

    v_senha_hash := crypt('LoadTest@2025!', gen_salt('bf', 8));

    -- 1) Usuários ---------------------------------------------------------
    -- Inserir em auth.users dispara o trigger on_auth_user_created, que cria
    -- a linha correspondente em public.usuario.
    RAISE NOTICE 'Gerando % usuarios...', n_usuarios;
    INSERT INTO auth.users (
        id, aud, role, email, encrypted_password,
        email_confirmed_at, raw_user_meta_data, created_at, updated_at
    )
    SELECT
        ('10000000-0000-0000-0000-' || lpad(to_hex(g), 12, '0'))::uuid,
        'authenticated', 'authenticated',
        'carga' || g || '@loadtest.local',
        v_senha_hash,
        NOW(),
        jsonb_build_object('nome', 'Usuario Carga ' || g),
        NOW(), NOW()
    FROM generate_series(1, n_usuarios) AS g;

    -- Enriquece os perfis de carga para exercitar filtros de preferência e
    -- consultas geográficas (localizacao em JSONB no mesmo formato do contrato).
    UPDATE usuario u
    SET
        data_nascimento     = DATE '1970-01-01' + ((u_seq.email_seq * 37) % 16000),
        perfil_preferencias = jsonb_build_object(
            'generos',
            (ARRAY['Teatro Musical','Teatro Infantil','Ballet Classico',
                   'Concerto e Show','Dramaturgia','Show Musical'])[1 + (u_seq.email_seq % 6)]
        ),
        localizacao = jsonb_build_object(
            'formattedAddress', (ARRAY['Sao Paulo, SP','Rio de Janeiro, RJ',
                                       'Belo Horizonte, MG','Curitiba, PR',
                                       'Porto Alegre, RS'])[1 + (u_seq.email_seq % 5)],
            'latitude',  -23.55 + (u_seq.email_seq % 1000) * 0.001,
            'longitude', -46.63 + (u_seq.email_seq % 1000) * 0.001,
            'placeId',   '',
            'provider',  'seed'
        )
    FROM (
        SELECT id_usuario,
               (substring(email FROM 'carga([0-9]+)@'))::int AS email_seq
        FROM usuario
        WHERE email LIKE '%@loadtest.local'
    ) u_seq
    WHERE u.id_usuario = u_seq.id_usuario;

    -- 2) Atores -----------------------------------------------------------
    RAISE NOTICE 'Gerando % atores...', n_atores;
    INSERT INTO ator (nome, biografia)
    SELECT 'Ator Carga ' || g,
           'Biografia sintetica gerada para teste de carga (#' || g || ').'
    FROM generate_series(1, n_atores) AS g;

    -- 3) Teatros ----------------------------------------------------------
    RAISE NOTICE 'Gerando % teatros...', n_teatros;
    INSERT INTO teatro (nome, endereco, cidade, estado, capacidade, acessibilidade, parceiro_naming_rights)
    SELECT
        'Teatro Carga ' || g,
        'Endereco sintetico, ' || g || ' - Centro',
        (ARRAY['Sao Paulo','Rio de Janeiro','Belo Horizonte','Curitiba','Porto Alegre'])[1 + (g % 5)],
        (ARRAY['SP','RJ','MG','PR','RS'])[1 + (g % 5)],
        300 + (g % 1500),
        (g % 2 = 0),
        'Carga LoadTest'
    FROM generate_series(1, n_teatros) AS g;

    -- 4) Espetáculos ------------------------------------------------------
    RAISE NOTICE 'Gerando % espetaculos...', n_espetaculos;
    INSERT INTO espetaculo (
        titulo, descricao, genero, faixa_etaria, duracao,
        classificacao_indicativa, produtora, status,
        popularidade, nota_media, id_admin
    )
    SELECT
        'Espetaculo Carga ' || g,
        'Espetaculo sintetico gerado para teste de carga (#' || g || ').',
        (ARRAY['Teatro Musical','Teatro Infantil','Ballet Classico',
               'Concerto e Show','Dramaturgia','Show Musical'])[1 + (g % 6)],
        (ARRAY['Livre','10 anos','12 anos','14 anos','16 anos'])[1 + (g % 5)],
        60 + (g % 120),
        (ARRAY['Livre','10 anos','12 anos','14 anos','16 anos'])[1 + (g % 5)],
        'Carga LoadTest',
        (ARRAY['em_cartaz','em_breve','encerrado'])[1 + (g % 3)]::status_espetaculo_enum,
        round((random() * 10)::numeric, 4),
        round((random() * 5)::numeric, 2),
        '00000000-0000-0000-0000-000000000001'
    FROM generate_series(1, n_espetaculos) AS g;

    -- 5) Espetáculo x Categoria (associa cada espetáculo de carga a 1-2 categorias existentes)
    RAISE NOTICE 'Associando espetaculos a categorias...';
    INSERT INTO espetaculo_categoria (id_espetaculo, id_categoria)
    SELECT e.id_espetaculo, c.id_categoria
    FROM (SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) AS rn
          FROM espetaculo WHERE produtora = 'Carga LoadTest') e
    JOIN LATERAL (
        SELECT id_categoria FROM categoria
        ORDER BY ((e.rn + id_categoria)) -- distribuição pseudo-aleatória estável
        LIMIT 1 + (e.rn % 2)
    ) c ON TRUE
    ON CONFLICT DO NOTHING;

    -- 6) Espetáculo x Ator (associa cada espetáculo de carga a 1-2 atores de carga)
    RAISE NOTICE 'Associando espetaculos a atores...';
    INSERT INTO espetaculo_ator (id_espetaculo, id_ator, papel, ordem_exibicao)
    WITH le AS (
        SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) - 1 AS rn
        FROM espetaculo WHERE produtora = 'Carga LoadTest'
    ),
    la AS (
        SELECT id_ator, row_number() OVER (ORDER BY id_ator) - 1 AS rn
        FROM ator WHERE nome LIKE 'Ator Carga %'
    ),
    na AS (SELECT count(*) AS c FROM la)
    SELECT le.id_espetaculo, la.id_ator, 'Elenco de carga', 1 + (le.rn % 5)
    FROM le
    JOIN na ON TRUE
    JOIN la ON la.rn = (le.rn % na.c)
    ON CONFLICT DO NOTHING;

    -- 7) Sessões ----------------------------------------------------------
    RAISE NOTICE 'Gerando % sessoes...', n_sessoes;
    INSERT INTO sessao (id_espetaculo, id_teatro, data, horario, preco_minimo, preco_maximo, disponibilidade, link_compra)
    WITH le AS (
        SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) - 1 AS rn
        FROM espetaculo WHERE produtora = 'Carga LoadTest'
    ),
    lt AS (
        SELECT id_teatro, row_number() OVER (ORDER BY id_teatro) - 1 AS rn
        FROM teatro WHERE parceiro_naming_rights = 'Carga LoadTest'
    ),
    ne AS (SELECT count(*) AS c FROM le),
    nt AS (SELECT count(*) AS c FROM lt)
    SELECT
        le.id_espetaculo,
        lt.id_teatro,
        CURRENT_DATE + (g % 365),
        (ARRAY['18:00','19:00','20:00','21:00'])[1 + (g % 4)]::time,
        (50 + (g % 100))::numeric,
        (50 + (g % 100) + 100 + (g % 200))::numeric,
        (g % 1000),
        'https://bilheteria.aventura.com.br/carga/' || g
    FROM generate_series(0, n_sessoes - 1) AS g
    JOIN ne ON TRUE
    JOIN nt ON TRUE
    JOIN le ON le.rn = (g % ne.c)
    JOIN lt ON lt.rn = ((g / ne.c) % nt.c)
    ON CONFLICT ON CONSTRAINT uq_sessao_espetaculo_teatro_data_horario DO NOTHING;

    -- 8) Avaliações (pares usuario x espetaculo únicos) -------------------
    RAISE NOTICE 'Gerando % avaliacoes...', n_avaliacoes;
    INSERT INTO avaliacao (id_usuario, id_espetaculo, nota, comentario)
    WITH lu AS (
        SELECT id_usuario, row_number() OVER (ORDER BY id_usuario) - 1 AS rn
        FROM usuario WHERE email LIKE '%@loadtest.local'
    ),
    le AS (
        SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) - 1 AS rn
        FROM espetaculo WHERE produtora = 'Carga LoadTest'
    ),
    nu AS (SELECT count(*) AS c FROM lu),
    ne AS (SELECT count(*) AS c FROM le)
    SELECT
        lu.id_usuario,
        le.id_espetaculo,
        round((random() * 5)::numeric, 2),
        'Avaliacao sintetica de carga.'
    FROM generate_series(0, n_avaliacoes - 1) AS g
    JOIN nu ON TRUE
    JOIN ne ON TRUE
    JOIN lu ON lu.rn = (g % nu.c)
    JOIN le ON le.rn = ((g / nu.c) % ne.c)
    ON CONFLICT (id_usuario, id_espetaculo) DO NOTHING;

    -- 9) Recomendações ----------------------------------------------------
    RAISE NOTICE 'Gerando % recomendacoes...', n_recomendacoes;
    INSERT INTO recomendacao (id_usuario, id_espetaculo, tipo_algoritmo, score_recomendacao, motivo_recomendacao)
    WITH lu AS (
        SELECT id_usuario, row_number() OVER (ORDER BY id_usuario) - 1 AS rn
        FROM usuario WHERE email LIKE '%@loadtest.local'
    ),
    le AS (
        SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) - 1 AS rn
        FROM espetaculo WHERE produtora = 'Carga LoadTest'
    ),
    nu AS (SELECT count(*) AS c FROM lu),
    ne AS (SELECT count(*) AS c FROM le)
    SELECT
        lu.id_usuario,
        le.id_espetaculo,
        (ARRAY['k-NN','SVD','k-means'])[1 + (g % 3)],
        round((random())::numeric, 6),
        'Recomendacao sintetica de carga.'
    FROM generate_series(0, n_recomendacoes - 1) AS g
    JOIN nu ON TRUE
    JOIN ne ON TRUE
    JOIN lu ON lu.rn = (g % nu.c)
    JOIN le ON le.rn = ((g / nu.c) % ne.c)
    ON CONFLICT (id_usuario, id_espetaculo, tipo_algoritmo) DO NOTHING;

    -- 10) Similaridade item-item (pares de espetáculos distintos) ---------
    RAISE NOTICE 'Gerando % similaridades...', n_similaridades;
    INSERT INTO similaridade_item (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo, score_similaridade)
    WITH le AS (
        SELECT id_espetaculo, row_number() OVER (ORDER BY id_espetaculo) - 1 AS rn
        FROM espetaculo WHERE produtora = 'Carga LoadTest'
    ),
    ne AS (SELECT count(*) AS c FROM le)
    SELECT
        o.id_espetaculo,
        s.id_espetaculo,
        (ARRAY['k-NN','SVD','k-means'])[1 + (g % 3)],
        round((random())::numeric, 6)
    FROM generate_series(0, n_similaridades - 1) AS g
    JOIN ne ON TRUE
    JOIN le o ON o.rn = (g % ne.c)
    JOIN le s ON s.rn = ((g % ne.c) + 1 + ((g / ne.c) % GREATEST(ne.c - 1, 1))) % ne.c
    ON CONFLICT ON CONSTRAINT uq_similaridade_item DO NOTHING;

    RAISE NOTICE 'Massa de carga gerada com sucesso.';
END $$;

-- Atualiza estatísticas do planejador para que os testes de carga reflitam
-- planos de execução realistas sobre o novo volume.
ANALYZE usuario;
ANALYZE ator;
ANALYZE teatro;
ANALYZE espetaculo;
ANALYZE espetaculo_categoria;
ANALYZE espetaculo_ator;
ANALYZE sessao;
ANALYZE avaliacao;
ANALYZE recomendacao;
ANALYZE similaridade_item;

COMMIT;
