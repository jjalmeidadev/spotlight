-- Cenário pgbench: avaliação de espetáculo (carga de ESCRITA).
-- Simula um usuário avaliando um espetáculo via UPSERT — o mesmo padrão da
-- aplicação. Cada escrita dispara a trigger de auditoria (migration 09),
-- então este cenário mede também o overhead da rastreabilidade de transações.
--
-- Premissas de volume (massa de src/database/load_test/massa_carga.sql):
--   espetaculos id 1..2000  |  usuarios de carga: UUID 10000000-..-<hex>
\set esp random(1, 2000)
\set uid random(1, 5000)

INSERT INTO avaliacao (id_usuario, id_espetaculo, nota, comentario)
VALUES (
    ('10000000-0000-0000-0000-' || lpad(to_hex(:uid), 12, '0'))::uuid,
    :esp,
    round((random() * 5)::numeric, 2),
    'Avaliacao gerada em teste de carga (pgbench).'
)
ON CONFLICT (id_usuario, id_espetaculo)
DO UPDATE SET
    nota       = EXCLUDED.nota,
    comentario = EXCLUDED.comentario;
