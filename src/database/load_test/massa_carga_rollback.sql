-- ===========================================================================
-- Rollback da massa de teste de carga — Spotlight
-- ---------------------------------------------------------------------------
-- Remove EXCLUSIVAMENTE os dados gerados por massa_carga.sql, preservando o
-- seed curado (02_initial_data.sql) e o schema.
--
-- Estratégia: as entidades de carga têm marcadores próprios; as tabelas-fato
-- (sessao, avaliacao, recomendacao, similaridade_item, associações N:N) só
-- referenciam essas entidades e são removidas em cascata (ON DELETE CASCADE).
--
-- Execução (psql):
--   psql "$DATABASE_URL" -f src/database/load_test/massa_carga_rollback.sql
-- ===========================================================================

BEGIN;

-- 1) Espetáculos de carga — cascateia sessao, avaliacao, recomendacao,
--    similaridade_item, espetaculo_categoria e espetaculo_ator relacionados.
DELETE FROM espetaculo WHERE produtora = 'Carga LoadTest';

-- 2) Teatros de carga — seguro após a remoção das sessões de carga
--    (sessao.id_teatro é ON DELETE RESTRICT).
DELETE FROM teatro WHERE parceiro_naming_rights = 'Carga LoadTest';

-- 3) Atores de carga.
DELETE FROM ator WHERE nome LIKE 'Ator Carga %';

-- 4) Usuários de carga — remover de auth.users cascateia para public.usuario
--    (usuario.id_usuario REFERENCES auth.users ON DELETE CASCADE).
DELETE FROM auth.users WHERE email LIKE '%@loadtest.local';

-- Atualiza estatísticas após a limpeza.
ANALYZE usuario;
ANALYZE ator;
ANALYZE teatro;
ANALYZE espetaculo;
ANALYZE sessao;
ANALYZE avaliacao;
ANALYZE recomendacao;
ANALYZE similaridade_item;

COMMIT;
