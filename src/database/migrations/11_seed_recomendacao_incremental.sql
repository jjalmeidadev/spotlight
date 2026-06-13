-- 11_seed_recomendacao_incremental.sql - Avaliacoes, recomendacoes e similares.
--
-- Seed incremental equivalente ao bloco final de 02_initial_data.sql, mas sem
-- depender de ids fixos de espetaculo. Todos os relacionamentos com espetaculo
-- sao resolvidos por titulo.
--
-- Pre-requisitos:
--   - usuarios seedados em public.usuario;
--   - catalogo completo aplicado por 10_seed_catalogo_incremental.sql;
--   - migration 08 aplicada para UNIQUE em recomendacao, se este arquivo for
--     reexecutado de forma idempotente.
--
-- Validacao apos aplicar:
--   SELECT COUNT(*) FROM public.avaliacao;
--   SELECT COUNT(*) FROM public.recomendacao;
--   SELECT COUNT(*) FROM public.similaridade_item;

BEGIN;

-- --------------------------------------------------------------------------
-- 1) Validacoes de dependencias.
-- --------------------------------------------------------------------------

DO $$
DECLARE
    usuarios_faltantes TEXT;
    espetaculos_faltantes TEXT;
BEGIN
    WITH esperados(id_usuario) AS (
        VALUES
            ('00000000-0000-0000-0000-000000000001'::uuid),
            ('00000000-0000-0000-0000-000000000002'::uuid),
            ('00000000-0000-0000-0000-000000000003'::uuid),
            ('00000000-0000-0000-0000-000000000004'::uuid),
            ('00000000-0000-0000-0000-000000000005'::uuid),
            ('00000000-0000-0000-0000-000000000006'::uuid),
            ('00000000-0000-0000-0000-000000000007'::uuid),
            ('00000000-0000-0000-0000-000000000008'::uuid),
            ('00000000-0000-0000-0000-000000000009'::uuid),
            ('00000000-0000-0000-0000-000000000010'::uuid),
            ('00000000-0000-0000-0000-000000000011'::uuid),
            ('00000000-0000-0000-0000-000000000012'::uuid),
            ('00000000-0000-0000-0000-000000000013'::uuid),
            ('00000000-0000-0000-0000-000000000014'::uuid),
            ('00000000-0000-0000-0000-000000000015'::uuid),
            ('00000000-0000-0000-0000-000000000016'::uuid),
            ('00000000-0000-0000-0000-000000000017'::uuid),
            ('00000000-0000-0000-0000-000000000018'::uuid),
            ('00000000-0000-0000-0000-000000000019'::uuid),
            ('00000000-0000-0000-0000-000000000020'::uuid),
            ('00000000-0000-0000-0000-000000000021'::uuid),
            ('00000000-0000-0000-0000-000000000022'::uuid),
            ('00000000-0000-0000-0000-000000000023'::uuid),
            ('00000000-0000-0000-0000-000000000024'::uuid),
            ('00000000-0000-0000-0000-000000000025'::uuid)
    )
    SELECT string_agg(id_usuario::text, ', ' ORDER BY id_usuario)
    INTO usuarios_faltantes
    FROM esperados e
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.usuario u
        WHERE u.id_usuario = e.id_usuario
    );

    IF usuarios_faltantes IS NOT NULL THEN
        RAISE EXCEPTION
            'Usuarios seed faltando em public.usuario. Rode o seed de usuarios antes deste arquivo: %',
            usuarios_faltantes;
    END IF;

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
    INTO espetaculos_faltantes
    FROM esperados e
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.espetaculo s
        WHERE s.titulo = e.titulo
    );

    IF espetaculos_faltantes IS NOT NULL THEN
        RAISE EXCEPTION
            'Espetaculos seed faltando. Rode 10_seed_catalogo_incremental.sql antes deste arquivo: %',
            espetaculos_faltantes;
    END IF;
END $$;

-- --------------------------------------------------------------------------
-- 2) Avaliacoes.
-- --------------------------------------------------------------------------

WITH dados(id_usuario, titulo, nota, comentario) AS (
    VALUES
        ('00000000-0000-0000-0000-000000000001'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000001'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000001'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000001'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000002'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000002'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000002'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000002'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000003'::uuid, 'Zaquim', 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
        ('00000000-0000-0000-0000-000000000003'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000003'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000003'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000004'::uuid, 'Zaquim', 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
        ('00000000-0000-0000-0000-000000000004'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000004'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000004'::uuid, 'Bibi - Uma Vida em Musical', 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
        ('00000000-0000-0000-0000-000000000005'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000005'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000005'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000005'::uuid, 'Nossa Historia com Chico Buarque', 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
        ('00000000-0000-0000-0000-000000000006'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000006'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000006'::uuid, 'Musical Hairspray', 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
        ('00000000-0000-0000-0000-000000000006'::uuid, 'A Cor Purpura, O Musical', 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
        ('00000000-0000-0000-0000-000000000007'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000007'::uuid, 'Prima Facie com Debora Falabella', 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
        ('00000000-0000-0000-0000-000000000007'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000007'::uuid, 'Hair', 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
        ('00000000-0000-0000-0000-000000000008'::uuid, 'Elis, a Musical', 4, 'Otima producao, elenco talentoso e trilha sonora impecavel.'),
        ('00000000-0000-0000-0000-000000000008'::uuid, 'A Novica Rebelde', 3, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000008'::uuid, 'Musical Hairspray', 3, 'Bom espetaculo, mas esperava mais profundidade dramatica.'),
        ('00000000-0000-0000-0000-000000000008'::uuid, 'Bibi - Uma Vida em Musical', 3, 'Bom espetaculo, roteiro poderia explorar mais a obra da homenageada.'),
        ('00000000-0000-0000-0000-000000000009'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000009'::uuid, 'Nossa Historia com Chico Buarque', 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
        ('00000000-0000-0000-0000-000000000009'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000009'::uuid, 'Chacrinha, o Musical', 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'Zaquim', 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'Zaquim', 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'Bibi - Uma Vida em Musical', 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'Chacrinha, o Musical', 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000012'::uuid, 'Zaquim', 4, 'Divertido e acessivel, toda a familia se encantou.'),
        ('00000000-0000-0000-0000-000000000012'::uuid, 'Mamma Mia!', 4, 'Energia contagiante, musicas que ficam na cabeca por dias.'),
        ('00000000-0000-0000-0000-000000000012'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
        ('00000000-0000-0000-0000-000000000012'::uuid, 'A Cor Purpura, O Musical', 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
        ('00000000-0000-0000-0000-000000000013'::uuid, 'Prima Facie com Debora Falabella', 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
        ('00000000-0000-0000-0000-000000000013'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000013'::uuid, 'Hair', 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
        ('00000000-0000-0000-0000-000000000013'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000014'::uuid, 'Nossa Historia com Chico Buarque', 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
        ('00000000-0000-0000-0000-000000000014'::uuid, 'A Cor Purpura, O Musical', 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
        ('00000000-0000-0000-0000-000000000014'::uuid, 'Bibi - Uma Vida em Musical', 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
        ('00000000-0000-0000-0000-000000000014'::uuid, 'Chacrinha, o Musical', 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000015'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000015'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000015'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000015'::uuid, 'Musical Hairspray', 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
        ('00000000-0000-0000-0000-000000000016'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
        ('00000000-0000-0000-0000-000000000016'::uuid, 'Nossa Historia com Chico Buarque', 3, 'Bom espetaculo, mas senti falta de mais interacao com o publico.'),
        ('00000000-0000-0000-0000-000000000016'::uuid, 'Bibi - Uma Vida em Musical', 3, 'Bom espetaculo, roteiro poderia explorar mais a obra da homenageada.'),
        ('00000000-0000-0000-0000-000000000016'::uuid, 'Chacrinha, o Musical', 2, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000017'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000017'::uuid, 'Nossa Historia com Chico Buarque', 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
        ('00000000-0000-0000-0000-000000000017'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000017'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000018'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000018'::uuid, 'Musical Hairspray', 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
        ('00000000-0000-0000-0000-000000000018'::uuid, 'A Cor Purpura, O Musical', 4, 'Elenco talentoso, coreografias lindas e texto muito bem adaptado.'),
        ('00000000-0000-0000-0000-000000000018'::uuid, 'Bibi - Uma Vida em Musical', 4, 'Espetaculo cheio de nostalgia e qualidade tecnica.'),
        ('00000000-0000-0000-0000-000000000019'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000019'::uuid, 'Ayrton Senna - O Musical', 5, 'Emocionante do inicio ao fim, Senna no palco e uma experiencia unica.'),
        ('00000000-0000-0000-0000-000000000019'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000019'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000020'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000020'::uuid, 'Prima Facie com Debora Falabella', 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
        ('00000000-0000-0000-0000-000000000020'::uuid, 'Nossa Historia com Chico Buarque', 4, 'Show musical sensivel e bem produzido, publico em lagrimas.'),
        ('00000000-0000-0000-0000-000000000020'::uuid, 'Hair', 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
        ('00000000-0000-0000-0000-000000000021'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000021'::uuid, 'Zaquim', 5, 'Meu filho adorou cada minuto, espetaculo magico para criancas.'),
        ('00000000-0000-0000-0000-000000000021'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 4, 'Show emocionante que honra um dos maiores legados da MPB.'),
        ('00000000-0000-0000-0000-000000000021'::uuid, 'Prima Facie com Debora Falabella', 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
        ('00000000-0000-0000-0000-000000000022'::uuid, 'Nossa Historia com Chico Buarque', 3, 'Bom espetaculo, mas senti falta de mais interacao com o publico.'),
        ('00000000-0000-0000-0000-000000000022'::uuid, 'A Cor Purpura, O Musical', 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
        ('00000000-0000-0000-0000-000000000022'::uuid, 'A Novica Rebelde', 3, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000022'::uuid, 'Musical Hairspray', 3, 'Bom espetaculo, mas esperava mais profundidade dramatica.'),
        ('00000000-0000-0000-0000-000000000023'::uuid, 'Mamma Mia!', 5, 'ABBA e teatro ao mesmo tempo, simplesmente incrivel!'),
        ('00000000-0000-0000-0000-000000000023'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 5, 'A fusao de Shakespeare com Marisa Monte foi simplesmente genial.'),
        ('00000000-0000-0000-0000-000000000023'::uuid, 'Hair', 3, 'Musical datado em alguns aspectos, mas com momentos brilhantes.'),
        ('00000000-0000-0000-0000-000000000023'::uuid, 'Chacrinha, o Musical', 3, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000024'::uuid, 'Elis, a Musical', 5, 'Espetaculo emocionante, uma homenagem perfeita a Elis Regina.'),
        ('00000000-0000-0000-0000-000000000024'::uuid, 'Musical Hairspray', 4, 'Producao impecavel, elenco afinado e coreografias incriveis.'),
        ('00000000-0000-0000-0000-000000000024'::uuid, 'A Novica Rebelde', 4, 'Producao caprichada, Mel Lisboa esta perfeita no papel.'),
        ('00000000-0000-0000-0000-000000000024'::uuid, 'Prima Facie com Debora Falabella', 5, 'Debora Falabella arrebata do inicio ao fim, peca imprescindivel.'),
        ('00000000-0000-0000-0000-000000000025'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 3, 'Bom show, mas o segundo tempo poderia ser mais dinamico.'),
        ('00000000-0000-0000-0000-000000000025'::uuid, 'A Cor Purpura, O Musical', 3, 'Boa producao, mas o ritmo cai um pouco no segundo ato.'),
        ('00000000-0000-0000-0000-000000000025'::uuid, 'Chacrinha, o Musical', 2, 'Entretenimento descompromissado, valeu pela nostalgia.'),
        ('00000000-0000-0000-0000-000000000025'::uuid, 'Ayrton Senna - O Musical', 4, 'Jarbas Homem de Mello entrega uma atuacao a altura da lenda.')
)
INSERT INTO public.avaliacao (id_usuario, id_espetaculo, nota, comentario)
SELECT d.id_usuario, e.id_espetaculo, d.nota, d.comentario
FROM dados d
JOIN public.espetaculo e ON e.titulo = d.titulo
ON CONFLICT (id_usuario, id_espetaculo) DO NOTHING;

UPDATE public.espetaculo
SET nota_media = sub.media
FROM (
    SELECT id_espetaculo, ROUND(AVG(nota)::NUMERIC, 2) AS media
    FROM public.avaliacao
    GROUP BY id_espetaculo
) AS sub
WHERE public.espetaculo.id_espetaculo = sub.id_espetaculo;

-- --------------------------------------------------------------------------
-- 3) Recomendacoes por usuario.
-- --------------------------------------------------------------------------

WITH dados(id_usuario, titulo, tipo_algoritmo, score_recomendacao, motivo_recomendacao, prioridade_manual, referencia_origem_id) AS (
    VALUES
        ('00000000-0000-0000-0000-000000000002'::uuid, 'Mamma Mia!', 'k-NN', 0.9215, 'Usuarios com perfil similar avaliaram positivamente Mamma Mia!', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000003'::uuid, 'Pixar in Concert', 'k-NN', 0.845, 'Pixar in Concert alinhado ao perfil familiar e musical do usuario.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000004'::uuid, 'Pixar in Concert', 'k-NN', 0.91, 'Pixar in Concert ideal para o perfil familiar identificado.', TRUE, NULL),
        ('00000000-0000-0000-0000-000000000005'::uuid, 'Elis, a Musical', 'SVD', 0.9356, 'Elis, a Musical com altissima predicao de afinidade para o perfil.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000006'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 'SVD', 0.883, 'Romeu e Julieta recomendado por fatoracao matricial de afinidade latente.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000007'::uuid, 'Musical Hairspray', 'k-NN', 0.8672, 'Hairspray apontado por vizinhos com perfil analogo de dramaturgia.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000008'::uuid, 'Ayrton Senna - O Musical', 'SVD', 0.891, 'Ayrton Senna - O Musical com alta predicao de afinidade para o perfil.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000009'::uuid, 'A Novica Rebelde', 'k-NN', 0.812, 'A Novica Rebelde indicada por usuarios com perfil de show ao vivo.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'Ayrton Senna - O Musical', 'k-NN', 0.876, 'Ayrton Senna apontado por similaridade com avaliacoes recentes.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'Pixar in Concert', 'k-NN', 0.905, 'Pixar in Concert ideal para perfis com preferencia infantil.', TRUE, NULL),
        ('00000000-0000-0000-0000-000000000012'::uuid, 'A Novica Rebelde', 'SVD', 0.788, 'A Novica Rebelde recomendada por similaridade com avaliacoes do cluster.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000013'::uuid, 'A Cor Purpura, O Musical', 'k-means', 0.801, 'Segmento Dramaturgos - A Cor Purpura com alta aprovacao do cluster.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000014'::uuid, 'Mamma Mia!', 'k-NN', 0.856, 'Mamma Mia! indicado por usuarios com perfil de show musical.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000015'::uuid, 'Mamma Mia!', 'SVD', 0.892, 'Mamma Mia! com alta predicao de afinidade por fatoracao matricial.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000016'::uuid, 'A Novica Rebelde', 'k-NN', 0.834, 'A Novica Rebelde alinhada ao perfil de shows ao vivo do usuario.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000017'::uuid, 'Elis, a Musical', 'SVD', 0.921, 'Elis, a Musical com altissima predicao para perfil de show musical.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000018'::uuid, 'Ayrton Senna - O Musical', 'k-NN', 0.867, 'Ayrton Senna apontado por vizinhos com perfil analogo.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000019'::uuid, 'A Novica Rebelde', 'k-NN', 0.879, 'A Novica Rebelde indicada por alta similaridade com avaliacoes anteriores.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000020'::uuid, 'Clube da Esquina - Os Sonhos Nao Envelhecem', 'k-means', 0.812, 'Segmento MPB - Clube da Esquina com alta aprovacao do cluster.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000021'::uuid, 'A Cor Purpura, O Musical', 'SVD', 0.845, 'A Cor Purpura recomendada por fatoracao matricial de afinidade latente.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000022'::uuid, 'Elis, a Musical', 'k-NN', 0.898, 'Elis, a Musical apontada por usuarios com perfil de show musical.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000023'::uuid, 'Musical Hairspray', 'k-NN', 0.863, 'Hairspray indicado por alta similaridade com avaliacoes do usuario.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000024'::uuid, 'Romeu & Julieta, ao som de Marisa Monte', 'SVD', 0.877, 'Romeu e Julieta recomendado por afinidade latente identificada.', FALSE, NULL),
        ('00000000-0000-0000-0000-000000000025'::uuid, 'Elis, a Musical', 'k-NN', 0.891, 'Elis, a Musical apontada por similaridade com perfil de shows musicais.', FALSE, NULL)
)
INSERT INTO public.recomendacao (
    id_usuario, id_espetaculo, tipo_algoritmo,
    score_recomendacao, motivo_recomendacao,
    prioridade_manual, referencia_origem_id
)
SELECT
    d.id_usuario,
    e.id_espetaculo,
    d.tipo_algoritmo,
    d.score_recomendacao,
    d.motivo_recomendacao,
    d.prioridade_manual,
    d.referencia_origem_id
FROM dados d
JOIN public.espetaculo e ON e.titulo = d.titulo
ON CONFLICT (id_usuario, id_espetaculo, tipo_algoritmo) DO UPDATE SET
    score_recomendacao = EXCLUDED.score_recomendacao,
    motivo_recomendacao = EXCLUDED.motivo_recomendacao,
    prioridade_manual = EXCLUDED.prioridade_manual,
    referencia_origem_id = EXCLUDED.referencia_origem_id,
    data_geracao = NOW();

-- --------------------------------------------------------------------------
-- 4) Similaridade item-item global.
-- --------------------------------------------------------------------------

WITH dados(titulo_origem, titulo_similar, tipo_algoritmo, score_similaridade) AS (
    VALUES
        ('Elis, a Musical', 'Ayrton Senna - O Musical', 'k-NN', 0.8945),
        ('Elis, a Musical', 'Clube da Esquina - Os Sonhos Nao Envelhecem', 'k-NN', 0.8612),
        ('Elis, a Musical', 'Mamma Mia!', 'k-NN', 0.7421),
        ('A Novica Rebelde', 'Mamma Mia!', 'k-NN', 0.8732),
        ('A Novica Rebelde', 'Musical Hairspray', 'k-NN', 0.8104),
        ('A Novica Rebelde', 'Chacrinha, o Musical', 'k-NN', 0.7896),
        ('Mamma Mia!', 'Musical Hairspray', 'k-NN', 0.8821),
        ('Mamma Mia!', 'Chacrinha, o Musical', 'k-NN', 0.8503),
        ('Mamma Mia!', 'A Novica Rebelde', 'k-NN', 0.8312),
        ('Pixar in Concert', 'Clube da Esquina - Os Sonhos Nao Envelhecem', 'k-NN', 0.7615),
        ('Pixar in Concert', 'Mamma Mia!', 'k-NN', 0.6932),
        ('Ayrton Senna - O Musical', 'Elis, a Musical', 'k-NN', 0.8945),
        ('Ayrton Senna - O Musical', 'Clube da Esquina - Os Sonhos Nao Envelhecem', 'k-NN', 0.8214),
        ('Romeu & Julieta, ao som de Marisa Monte', 'A Cor Purpura, O Musical', 'k-NN', 0.8123),
        ('Romeu & Julieta, ao som de Marisa Monte', 'Prima Facie com Debora Falabella', 'k-NN', 0.7842),
        ('Musical Hairspray', 'Mamma Mia!', 'k-NN', 0.8821),
        ('Musical Hairspray', 'Chacrinha, o Musical', 'k-NN', 0.8456),
        ('Musical Hairspray', 'A Novica Rebelde', 'k-NN', 0.7912),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Elis, a Musical', 'k-NN', 0.8612),
        ('Clube da Esquina - Os Sonhos Nao Envelhecem', 'Ayrton Senna - O Musical', 'k-NN', 0.8214),
        ('Prima Facie com Debora Falabella', 'A Cor Purpura, O Musical', 'k-NN', 0.8421),
        ('Prima Facie com Debora Falabella', 'Romeu & Julieta, ao som de Marisa Monte', 'k-NN', 0.7842),
        ('A Cor Purpura, O Musical', 'Prima Facie com Debora Falabella', 'k-NN', 0.8421),
        ('A Cor Purpura, O Musical', 'Romeu & Julieta, ao som de Marisa Monte', 'k-NN', 0.8123),
        ('A Cor Purpura, O Musical', 'Musical Hairspray', 'k-NN', 0.7456),
        ('Chacrinha, o Musical', 'Mamma Mia!', 'k-NN', 0.8503),
        ('Chacrinha, o Musical', 'Musical Hairspray', 'k-NN', 0.8456)
)
INSERT INTO public.similaridade_item (
    id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo, score_similaridade
)
SELECT
    origem.id_espetaculo,
    espetaculo_similar.id_espetaculo,
    d.tipo_algoritmo,
    d.score_similaridade
FROM dados d
JOIN public.espetaculo origem ON origem.titulo = d.titulo_origem
JOIN public.espetaculo espetaculo_similar ON espetaculo_similar.titulo = d.titulo_similar
ON CONFLICT (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo) DO UPDATE SET
    score_similaridade = EXCLUDED.score_similaridade,
    data_geracao = NOW();

COMMIT;
