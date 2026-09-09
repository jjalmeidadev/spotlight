-- 09_rastreabilidade_transacoes.sql — Rastreabilidade de transações no banco.
--
-- Motivação:
--   O schema não registrava QUEM alterou O QUÊ e QUANDO. Esta migration adiciona
--   uma trilha de auditoria (audit log) que captura toda operação de escrita
--   (INSERT/UPDATE/DELETE) sobre as tabelas transacionais, agrupando-as pelo
--   identificador da transação do PostgreSQL (txid_current()), de modo que
--   mudanças feitas no mesmo COMMIT sejam correlacionáveis.
--
-- O que esta migration cria:
--   1) Tabela public.auditoria — uma linha por operação, com estado anterior e
--      novo em JSONB, ator responsável e id da transação.
--   2) Função public.registrar_auditoria() — trigger genérica AFTER, recebe o
--      nome da coluna de PK como argumento para registrar o registro_id.
--   3) Triggers AFTER INSERT/UPDATE/DELETE nas tabelas transacionais.
--
-- Rastreabilidade do ator:
--   - id_usuario_responsavel = auth.uid() (usuário autenticado; NULL para
--     escritas de sistema/seed/worker que não trafegam JWT).
--   - papel_banco = current_user (role do banco que executou a operação).
--
-- NOTA de carga: as triggers disparam também durante a carga de massa sintética
-- (src/database/load_test/). Para medir o banco sem o overhead de auditoria,
-- desabilite as triggers durante o bulk load ou aplique esta migration depois.

BEGIN;

-- --------------------------------------------------------------------------
-- 1) Tabela de auditoria (trilha de transações).
--    Sem FK em id_usuario_responsavel: o ator pode ser um processo de sistema
--    sem linha em usuario, e o log deve sobreviver à exclusão do usuário.
-- --------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS auditoria (
    id_auditoria           BIGSERIAL    PRIMARY KEY,
    id_transacao           BIGINT       NOT NULL,
    tabela                 VARCHAR(63)  NOT NULL,
    operacao               VARCHAR(10)  NOT NULL
                                        CHECK (operacao IN ('INSERT', 'UPDATE', 'DELETE')),
    registro_id            TEXT,
    dados_anteriores       JSONB,
    dados_novos            JSONB,
    id_usuario_responsavel UUID,
    papel_banco            TEXT         NOT NULL DEFAULT current_user,
    registrado_em          TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE auditoria IS
    'Trilha de auditoria: uma linha por operação de escrita nas tabelas transacionais, agrupável por id_transacao.';

-- Índices para as consultas típicas da trilha.
CREATE INDEX IF NOT EXISTS idx_auditoria_transacao
    ON auditoria (id_transacao);
CREATE INDEX IF NOT EXISTS idx_auditoria_tabela_registro
    ON auditoria (tabela, registro_id);
CREATE INDEX IF NOT EXISTS idx_auditoria_registrado_em
    ON auditoria (registrado_em DESC);
CREATE INDEX IF NOT EXISTS idx_auditoria_usuario
    ON auditoria (id_usuario_responsavel);

-- RLS: a trilha é sensível. Habilitada e legível apenas por administradores;
-- as inserções vêm da trigger SECURITY DEFINER, que ignora RLS.
ALTER TABLE auditoria ENABLE ROW LEVEL SECURITY;

CREATE POLICY "auditoria: leitura por administradores"
    ON auditoria FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM usuario u
            WHERE u.id_usuario = auth.uid()
              AND u.tipo_usuario = 'administrador'
        )
    );

-- --------------------------------------------------------------------------
-- 2) Função de trigger genérica.
--    TG_ARGV[0] = nome da coluna de PK da tabela auditada (para registro_id).
-- --------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION public.registrar_auditoria()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
DECLARE
    v_pk_col      TEXT  := TG_ARGV[0];
    v_dados_ant   JSONB;
    v_dados_novo  JSONB;
    v_registro_id TEXT;
BEGIN
    IF TG_OP = 'DELETE' THEN
        v_dados_ant   := to_jsonb(OLD);
        v_dados_novo  := NULL;
        v_registro_id := v_dados_ant ->> v_pk_col;
    ELSIF TG_OP = 'UPDATE' THEN
        v_dados_ant   := to_jsonb(OLD);
        v_dados_novo  := to_jsonb(NEW);
        v_registro_id := v_dados_novo ->> v_pk_col;
    ELSE -- INSERT
        v_dados_ant   := NULL;
        v_dados_novo  := to_jsonb(NEW);
        v_registro_id := v_dados_novo ->> v_pk_col;
    END IF;

    INSERT INTO public.auditoria (
        id_transacao, tabela, operacao, registro_id,
        dados_anteriores, dados_novos, id_usuario_responsavel, papel_banco
    ) VALUES (
        txid_current(), TG_TABLE_NAME, TG_OP, v_registro_id,
        v_dados_ant, v_dados_novo, auth.uid(), current_user
    );

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    END IF;
    RETURN NEW;
END;
$$;

-- --------------------------------------------------------------------------
-- 3) Triggers nas tabelas transacionais. O argumento é a coluna de PK.
--    Tabelas associativas (espetaculo_categoria, espetaculo_ator) têm PK
--    composta e ficam fora desta trilha por ora.
-- --------------------------------------------------------------------------

DROP TRIGGER IF EXISTS trg_auditoria_usuario ON usuario;
CREATE TRIGGER trg_auditoria_usuario
    AFTER INSERT OR UPDATE OR DELETE ON usuario
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_usuario');

DROP TRIGGER IF EXISTS trg_auditoria_espetaculo ON espetaculo;
CREATE TRIGGER trg_auditoria_espetaculo
    AFTER INSERT OR UPDATE OR DELETE ON espetaculo
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_espetaculo');

DROP TRIGGER IF EXISTS trg_auditoria_teatro ON teatro;
CREATE TRIGGER trg_auditoria_teatro
    AFTER INSERT OR UPDATE OR DELETE ON teatro
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_teatro');

DROP TRIGGER IF EXISTS trg_auditoria_ator ON ator;
CREATE TRIGGER trg_auditoria_ator
    AFTER INSERT OR UPDATE OR DELETE ON ator
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_ator');

DROP TRIGGER IF EXISTS trg_auditoria_sessao ON sessao;
CREATE TRIGGER trg_auditoria_sessao
    AFTER INSERT OR UPDATE OR DELETE ON sessao
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_sessao');

DROP TRIGGER IF EXISTS trg_auditoria_avaliacao ON avaliacao;
CREATE TRIGGER trg_auditoria_avaliacao
    AFTER INSERT OR UPDATE OR DELETE ON avaliacao
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_avaliacao');

DROP TRIGGER IF EXISTS trg_auditoria_recomendacao ON recomendacao;
CREATE TRIGGER trg_auditoria_recomendacao
    AFTER INSERT OR UPDATE OR DELETE ON recomendacao
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_recomendacao');

DROP TRIGGER IF EXISTS trg_auditoria_similaridade_item ON similaridade_item;
CREATE TRIGGER trg_auditoria_similaridade_item
    AFTER INSERT OR UPDATE OR DELETE ON similaridade_item
    FOR EACH ROW EXECUTE FUNCTION public.registrar_auditoria('id_similaridade');

COMMIT;

-- Validação após aplicar:
--   -- Tabela e índices criados:
--   SELECT tablename FROM pg_tables WHERE tablename = 'auditoria';            -- 1 linha
--   SELECT count(*) FROM pg_indexes WHERE tablename = 'auditoria';           -- 5 (PK + 4)
--   -- Triggers de auditoria registradas:
--   SELECT tgname FROM pg_trigger WHERE tgname LIKE 'trg_auditoria_%';       -- 8 linhas
--   -- Teste funcional (gera 1 linha em auditoria por operação):
--   UPDATE espetaculo SET popularidade = popularidade WHERE id_espetaculo = 1;
--   SELECT id_transacao, tabela, operacao, registro_id
--   FROM auditoria ORDER BY id_auditoria DESC LIMIT 1;
