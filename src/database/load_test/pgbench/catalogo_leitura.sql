-- Cenário pgbench: navegação de catálogo (carga de LEITURA).
-- Espelha as queries críticas do EspetaculoRepository documentadas no README
-- (seção "Planos de Execução — Queries Críticas") mais a leitura de
-- recomendações por usuário. Cada transação simula uma visita típica.
--
-- Premissas de volume (massa de src/database/load_test/massa_carga.sql):
--   espetaculos id 1..2000  |  usuarios de carga: UUID 10000000-..-<hex>
\set esp random(1, 2000)
\set uid random(1, 5000)

-- 1) Tela de catálogo: em cartaz ordenado por popularidade (query padrão).
SELECT id_espetaculo, titulo, popularidade
FROM espetaculo
WHERE status = 'em_cartaz'
ORDER BY popularidade DESC
LIMIT 20;

-- 2) Filtro por gênero.
SELECT id_espetaculo, titulo
FROM espetaculo
WHERE genero = 'Teatro Musical'
LIMIT 20;

-- 3) Busca por título (trigram / ILIKE).
SELECT id_espetaculo, titulo
FROM espetaculo
WHERE titulo ILIKE '%carga%'
LIMIT 20;

-- 4) Sessões de um espetáculo, já ordenadas (índice composto).
SELECT id_sessao, data, horario, preco_minimo
FROM sessao
WHERE id_espetaculo = :esp
ORDER BY data, horario;

-- 5) Recomendações de um usuário, ordenadas por score.
SELECT id_espetaculo, score_recomendacao
FROM recomendacao
WHERE id_usuario = ('10000000-0000-0000-0000-' || lpad(to_hex(:uid), 12, '0'))::uuid
ORDER BY score_recomendacao DESC
LIMIT 10;
