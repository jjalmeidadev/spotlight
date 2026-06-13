-- 06_indexes.sql — Índices novos para queries reais do serviço de catálogo.
--
-- Cada índice tem uma query concreta que o justifica (referência no
-- EspetaculoRepository.cs). Quatro foram propostos originalmente em um
-- mapeamento maior; dois foram descartados por não terem consulta que os
-- justificasse no estado atual do código:
--   - (id_usuario, data_geracao DESC) em recomendacao — coberto por
--     idx_recomendacao_score
--   - GIN em usuario.perfil_preferencias — nenhuma query filtra por chaves
--     do JSONB hoje
--
-- Validação após aplicar (esperado: Bitmap Index Scan sobre o índice novo,
-- não Seq Scan em espetaculo):
--   EXPLAIN (ANALYZE) SELECT * FROM espetaculo WHERE titulo ILIKE '%musical%';
--   EXPLAIN (ANALYZE) SELECT * FROM espetaculo
--     WHERE status = 'em_cartaz' ORDER BY popularidade DESC LIMIT 20;

BEGIN;

-- Passo 1 — extensão necessária para o índice GIN trigram.
-- No Supabase cloud a extensão geralmente já está disponível; se precisar,
-- habilitar via Dashboard → Database → Extensions antes de rodar esta migration.
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Passo 2 — os 4 índices.

-- 1) Busca por título (ILIKE '%q%' em EspetaculoRepository.cs:67).
--    Substitui varredura sequencial quando o padrão não tem âncora à esquerda.
CREATE INDEX IF NOT EXISTS idx_espetaculo_titulo_trgm
    ON espetaculo USING GIN (titulo gin_trgm_ops);

-- 2) Filtro por gênero (WHERE genero = @genero em EspetaculoRepository.cs:79).
CREATE INDEX IF NOT EXISTS idx_espetaculo_genero
    ON espetaculo (genero);

-- 3) Filtro de status + ordenação por popularidade
--    (WHERE status = X ORDER BY popularidade DESC em EspetaculoRepository.cs:74, :111).
CREATE INDEX IF NOT EXISTS idx_espetaculo_status_popularidade
    ON espetaculo (status, popularidade DESC);

-- 4) Sessões de um espetáculo ordenadas por data/horário
--    (WHERE id_espetaculo = X ORDER BY data, horario em EspetaculoRepository.cs:258-259).
CREATE INDEX IF NOT EXISTS idx_sessao_espetaculo_data_horario
    ON sessao (id_espetaculo, data, horario);

COMMIT;
