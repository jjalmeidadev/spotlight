CREATE TYPE tipo_usuario_enum AS ENUM (
    'visitante',
    'cadastrado',
    'administrador'
);

CREATE TYPE status_espetaculo_enum AS ENUM (
    'em_cartaz',
    'em_breve',
    'encerrado'
);

CREATE TABLE usuario (
    id_usuario          UUID                PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    nome                VARCHAR(150)        NOT NULL,
    email               VARCHAR(255)        NOT NULL UNIQUE,
    data_nascimento     DATE,
    localizacao         VARCHAR(150),
    data_cadastro       TIMESTAMP           NOT NULL DEFAULT NOW(),
    tipo_usuario        tipo_usuario_enum   NOT NULL DEFAULT 'cadastrado',
    nivel_acesso        JSONB               NOT NULL DEFAULT '{}'::JSONB,
    perfil_preferencias JSONB               NOT NULL DEFAULT '{}'::JSONB,

    CONSTRAINT chk_email_formato CHECK (email ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$')
);

ALTER TABLE usuario ENABLE ROW LEVEL SECURITY;

CREATE POLICY "usuario: leitura do próprio perfil"
    ON usuario FOR SELECT
    USING (auth.uid() = id_usuario);

CREATE POLICY "usuario: atualização do próprio perfil"
    ON usuario FOR UPDATE
    USING (auth.uid() = id_usuario);

CREATE POLICY "usuario: inserção via trigger de auth"
    ON usuario FOR INSERT
    WITH CHECK (true);

CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    INSERT INTO public.usuario (
        id_usuario,
        nome,
        email,
        data_nascimento
    )
    VALUES (
        NEW.id,
        COALESCE(NEW.raw_user_meta_data->>'nome', split_part(NEW.email, '@', 1)),
        NEW.email,
        NULLIF(NEW.raw_user_meta_data->>'dataNascimento', '')::DATE
    );
    RETURN NEW;
END;
$$;

CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW
    EXECUTE FUNCTION public.handle_new_user();

CREATE TABLE categoria (
    id_categoria  SERIAL        PRIMARY KEY,
    nome          VARCHAR(100)  NOT NULL UNIQUE,
    descricao     TEXT
);

ALTER TABLE categoria ENABLE ROW LEVEL SECURITY;

CREATE POLICY "categoria: leitura pública"
    ON categoria FOR SELECT
    USING (true);

CREATE TABLE ator (
    id_ator    SERIAL        PRIMARY KEY,
    nome       VARCHAR(150)  NOT NULL,
    biografia  TEXT,
    created_at TIMESTAMP     NOT NULL DEFAULT NOW()
);

ALTER TABLE ator ENABLE ROW LEVEL SECURITY;

CREATE POLICY "ator: leitura pública"
    ON ator FOR SELECT
    USING (true);

CREATE TABLE espetaculo (
    id_espetaculo            SERIAL                 PRIMARY KEY,
    titulo                   VARCHAR(200)           NOT NULL,
    descricao                TEXT,
    genero                   VARCHAR(100),
    faixa_etaria             VARCHAR(50),
    duracao                  INTEGER                CHECK (duracao > 0),
    classificacao_indicativa VARCHAR(20),
    produtora                VARCHAR(150),
    status                   status_espetaculo_enum NOT NULL DEFAULT 'em_breve',
    popularidade             NUMERIC(10, 4)         NOT NULL DEFAULT 0,
    nota_media               NUMERIC(3, 2)          NOT NULL DEFAULT 0
                                                    CHECK (nota_media BETWEEN 0 AND 5),
    id_admin                 UUID                   REFERENCES usuario(id_usuario) ON DELETE SET NULL,
    created_at               TIMESTAMP              NOT NULL DEFAULT NOW(),
    updated_at               TIMESTAMP              NOT NULL DEFAULT NOW()
);

ALTER TABLE espetaculo ENABLE ROW LEVEL SECURITY;

CREATE POLICY "espetaculo: leitura pública"
    ON espetaculo FOR SELECT
    USING (true);

CREATE TABLE espetaculo_categoria (
    id_espetaculo  INTEGER  NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
    id_categoria   INTEGER  NOT NULL REFERENCES categoria(id_categoria)   ON DELETE CASCADE,

    PRIMARY KEY (id_espetaculo, id_categoria)
);

ALTER TABLE espetaculo_categoria ENABLE ROW LEVEL SECURITY;

CREATE POLICY "espetaculo_categoria: leitura pública"
    ON espetaculo_categoria FOR SELECT
    USING (true);

CREATE TABLE espetaculo_ator (
    id_espetaculo  INTEGER      NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
    id_ator        INTEGER      NOT NULL REFERENCES ator(id_ator)             ON DELETE CASCADE,
    papel          VARCHAR(150),
    ordem_exibicao INTEGER      CHECK (ordem_exibicao > 0),

    PRIMARY KEY (id_espetaculo, id_ator)
);

ALTER TABLE espetaculo_ator ENABLE ROW LEVEL SECURITY;

CREATE POLICY "espetaculo_ator: leitura pública"
    ON espetaculo_ator FOR SELECT
    USING (true);

CREATE TABLE teatro (
    id_teatro              SERIAL        PRIMARY KEY,
    nome                   VARCHAR(200)  NOT NULL,
    endereco               VARCHAR(300)  NOT NULL,
    cidade                 VARCHAR(100)  NOT NULL,
    estado                 CHAR(2)       NOT NULL,
    capacidade             INTEGER       CHECK (capacidade > 0),
    acessibilidade         BOOLEAN       NOT NULL DEFAULT FALSE,
    parceiro_naming_rights VARCHAR(200),
    created_at             TIMESTAMP     NOT NULL DEFAULT NOW()
);

ALTER TABLE teatro ENABLE ROW LEVEL SECURITY;

CREATE POLICY "teatro: leitura pública"
    ON teatro FOR SELECT
    USING (true);

CREATE TABLE sessao (
    id_sessao       SERIAL         PRIMARY KEY,
    id_espetaculo   INTEGER        NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
    id_teatro       INTEGER        NOT NULL REFERENCES teatro(id_teatro)         ON DELETE RESTRICT,
    data            DATE           NOT NULL,
    horario         TIME           NOT NULL,
    preco_minimo    NUMERIC(10, 2) CHECK (preco_minimo >= 0),
    preco_maximo    NUMERIC(10, 2) CHECK (preco_maximo >= 0),
    disponibilidade INTEGER        NOT NULL DEFAULT 0 CHECK (disponibilidade >= 0),
    link_compra     TEXT,
    created_at      TIMESTAMP      NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_preco_intervalo CHECK (
        preco_maximo IS NULL OR preco_minimo IS NULL OR preco_maximo >= preco_minimo
    )
);

ALTER TABLE sessao ENABLE ROW LEVEL SECURITY;

CREATE POLICY "sessao: leitura pública"
    ON sessao FOR SELECT
    USING (true);

CREATE TABLE avaliacao (
    id_avaliacao  SERIAL         PRIMARY KEY,
    id_usuario    UUID           NOT NULL REFERENCES usuario(id_usuario)       ON DELETE CASCADE,
    id_espetaculo INTEGER        NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
    nota          NUMERIC(4, 2)  NOT NULL CHECK (nota BETWEEN 0 AND 5),
    comentario    TEXT,
    created_at    TIMESTAMP      NOT NULL DEFAULT NOW(),

    UNIQUE (id_usuario, id_espetaculo)
);

ALTER TABLE avaliacao ENABLE ROW LEVEL SECURITY;

CREATE POLICY "avaliacao: leitura pública"
    ON avaliacao FOR SELECT
    USING (true);

CREATE POLICY "avaliacao: inserção pelo próprio usuário"
    ON avaliacao FOR INSERT
    WITH CHECK (auth.uid() = id_usuario);

CREATE POLICY "avaliacao: exclusão pelo próprio usuário"
    ON avaliacao FOR DELETE
    USING (auth.uid() = id_usuario);

CREATE TABLE recomendacao (
    id_recomendacao      SERIAL         PRIMARY KEY,
    id_usuario           UUID           NOT NULL REFERENCES usuario(id_usuario)       ON DELETE CASCADE,
    id_espetaculo        INTEGER        NOT NULL REFERENCES espetaculo(id_espetaculo) ON DELETE CASCADE,
    tipo_algoritmo       VARCHAR(50)    NOT NULL,
    score_recomendacao   NUMERIC(8, 6)  NOT NULL CHECK (score_recomendacao >= 0),
    motivo_recomendacao  TEXT,
    data_geracao         TIMESTAMP      NOT NULL DEFAULT NOW(),
    prioridade_manual    BOOLEAN        NOT NULL DEFAULT FALSE,
    referencia_origem_id VARCHAR(50),

    CONSTRAINT chk_tipo_algoritmo CHECK (
        tipo_algoritmo IN ('k-NN', 'SVD', 'k-means')
    )
);

ALTER TABLE recomendacao ENABLE ROW LEVEL SECURITY;

CREATE POLICY "recomendacao: leitura pelo próprio usuário"
    ON recomendacao FOR SELECT
    USING (auth.uid() = id_usuario);

CREATE INDEX idx_espetaculo_status
    ON espetaculo(status);

CREATE INDEX idx_espetaculo_nota_media
    ON espetaculo(nota_media DESC);

CREATE INDEX idx_espetaculo_popularidade
    ON espetaculo(popularidade DESC);

CREATE INDEX idx_sessao_espetaculo
    ON sessao(id_espetaculo);

CREATE INDEX idx_sessao_data
    ON sessao(data);

CREATE INDEX idx_sessao_teatro
    ON sessao(id_teatro);

CREATE INDEX idx_recomendacao_usuario
    ON recomendacao(id_usuario);

CREATE INDEX idx_recomendacao_score
    ON recomendacao(id_usuario, score_recomendacao DESC);

CREATE INDEX idx_avaliacao_espetaculo
    ON avaliacao(id_espetaculo);

CREATE INDEX idx_espetaculo_ator_ator
    ON espetaculo_ator(id_ator);
