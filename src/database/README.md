# Banco de Dados — Spotlight

#### Documentação técnica do modelo físico de dados da plataforma Spotlight, desenvolvida em parceria com a Aventura Teatros.

---

## Sumário

- [1. Visão Geral](#1-visão-geral)
- [2. Estrutura de Arquivos](#2-estrutura-de-arquivos)
- [3. Pré-requisitos](#3-pré-requisitos)
- [4. Variáveis de Ambiente](#4-variáveis-de-ambiente)
- [5. Execução dos Scripts](#5-execução-dos-scripts)
- [6. Implantação](#6-implantação)
- [7. Dicionário de Dados](#7-dicionário-de-dados)
- [8. Regras de Negócio](#8-regras-de-negócio)

---

## 1. Visão Geral

O **Spotlight** é uma plataforma de recomendação de espetáculos teatrais que conecta usuários ao catálogo da Aventura Teatros por meio de algoritmos de Machine Learning (k-NN, SVD, k-means). O banco de dados relacional é o núcleo da solução: armazena o catálogo de espetáculos, perfis de usuários, avaliações explícitas e as recomendações geradas pelo motor de ML.

**SGBD:** PostgreSQL 15+ provisionado via **Supabase**

**Por que dois bancos?** O PostgreSQL (Supabase) guarda dados transacionais e estruturados — catálogo, perfis, avaliações e recomendações processadas. O MongoDB armazena os eventos brutos de telemetria (cliques, tempo de tela) em alta volumetria. Os workers de ML consomem o MongoDB e escrevem os resultados processados de volta no PostgreSQL, na tabela `recomendacao`.

**Integração com Supabase Auth:** A tabela `usuario` referencia `auth.users` via UUID. O Supabase Auth é a única fonte de verdade para autenticação — senhas nunca são armazenadas no schema público. Um trigger automático (`on_auth_user_created`) cria o perfil do usuário no schema público imediatamente após o signup.

---

## 2. Estrutura de Arquivos

```
src/database/
├── 01_create_database.sql   — criação de tabelas, tipos ENUM, índices, RLS e trigger do Auth
├── 02_initial_data.sql      — seed com dados iniciais (categorias, teatros, atores, espetáculos e sessões)
├── 03_rollback.sql          — desfaz completamente a execução do 01_create_database.sql
└── README.md                — este arquivo
```

---

## 3. Pré-requisitos

| Dependência | Versão mínima | Observação |
|---|---|---|
| PostgreSQL | 15.0 | Incluso na imagem Docker do Supabase |
| Docker | 24.0 | Para implantação local ou no servidor Inteli |
| Docker Compose | 2.20 | Para orquestração dos containers |
| Supabase CLI | 1.150 | Opcional — facilita o gerenciamento local |

> Para execução apenas via SQL Editor do Supabase (cloud), Docker não é necessário.

---

## 4. Variáveis de Ambiente

Crie um arquivo `.env` na **raiz do repositório** (nunca em `src/database`). O arquivo `.env` deve estar no `.gitignore` — **jamais commite segredos no repositório**.

```env
# ─── PostgreSQL ──────────────────────────────────────────────
POSTGRES_PASSWORD=sua_senha_segura_aqui

# ─── Supabase Auth ───────────────────────────────────────────
# Gerados via: supabase start  ou  supabase gen keys
JWT_SECRET=seu_jwt_secret_com_minimo_32_caracteres
ANON_KEY=REDACTED_FOR_PUBLIC_RELEASE
SERVICE_ROLE_KEY=REDACTED_FOR_PUBLIC_RELEASE

# ─── URL pública do Supabase ─────────────────────────────────
SUPABASE_URL=http://localhost:8000
# Em produção (cloud): https://<project-ref>.supabase.co

# ─── Frontend / API Gateway ──────────────────────────────────
SITE_URL=http://localhost:3000
```

### Onde cada variável é consumida

| Variável | Consumido por |
|---|---|
| `POSTGRES_PASSWORD` | Container PostgreSQL (Docker) |
| `JWT_SECRET` | API Gateway (ASP.NET Core) — validação offline do JWT |
| `ANON_KEY` | React Native — chamadas públicas à API do Supabase |
| `SERVICE_ROLE_KEY` | Workers de ML e API Gateway — bypass de RLS para operações administrativas |
| `SUPABASE_URL` | React Native e API Gateway — endpoint base do Supabase |

---

## 5. Execução dos Scripts

### 5.1 Via SQL Editor do Supabase

1. Acesse o [Supabase Dashboard](https://supabase.com/dashboard) e abra o projeto
2. Vá em **SQL Editor → New Query**
3. Cole e execute `01_create_database.sql`
4. Aguarde a confirmação de sucesso
5. Cole e execute `02_initial_data.sql`

**Validação:** Acesse **Table Editor** e confirme que as 11 tabelas aparecem no schema `public`.

### 5.2 Via `psql` (linha de comando)

```bash
# Substitua <host>, <porta> e <senha> pelos valores do seu ambiente
export PGPASSWORD=<senha>

# 1. Cria todas as estruturas
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/01_create_database.sql

# 2. Insere os dados iniciais
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/02_initial_data.sql

# Validação: lista as tabelas criadas
psql -h <host> -p <porta> -U postgres -d postgres \
  -c "\dt public.*"
```

**Saída esperada na validação:**

```
          List of relations
 Schema |         Name          | Type  |  Owner
--------+-----------------------+-------+----------
 public | ator                  | table | postgres
 public | avaliacao             | table | postgres
 public | categoria             | table | postgres
 public | espetaculo            | table | postgres
 public | espetaculo_ator       | table | postgres
 public | espetaculo_categoria  | table | postgres
 public | recomendacao          | table | postgres
 public | sessao                | table | postgres
 public | teatro                | table | postgres
 public | usuario               | table | postgres
(10 rows)
```

### 5.3 Rollback (desfaz tudo)

```bash
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/03_rollback.sql
```

> O rollback é **destrutivo**. Apaga todos os dados e estruturas. Use apenas em desenvolvimento ou para reset total do ambiente.

### 5.4 Ordem obrigatória de execução

```
01_create_database.sql  →  02_initial_data.sql
```

O `02_initial_data.sql` depende das tabelas criadas pelo `01`. Nunca inverta a ordem.

---

## 6. Implantação

### 6.1 Docker Compose (local e servidor Inteli)

Crie o arquivo `docker-compose.yml` na raiz do repositório:

```yaml
version: "3.8"

services:
  db:
    image: supabase/postgres:15.1.0.147
    restart: unless-stopped
    ports:
      - "5432:5432"
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - db_data:/var/lib/postgresql/data
      - ./src/database/01_create_database.sql:/docker-entrypoint-initdb.d/01_create.sql
      - ./src/database/02_initial_data.sql:/docker-entrypoint-initdb.d/02_seed.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  db_data:
```

> Os scripts em `docker-entrypoint-initdb.d/` são executados automaticamente na **primeira inicialização** do container, em ordem alfabética. Em reinicializações subsequentes, o volume já contém os dados e os scripts não são re-executados.

### 6.2 Subindo o banco

```bash
# Na raiz do repositório, com o .env preenchido
docker compose up -d db

# Acompanhe os logs para confirmar a inicialização
docker compose logs -f db

# Saída esperada ao final:
# db  | database system is ready to accept connections
```

### 6.3 Instruções específicas — Servidor Inteli

O servidor Inteli exige que os containers sejam executados com configurações específicas de rede e porta. Siga os passos abaixo após obter acesso ao servidor:

```bash
# 1. Clone o repositório no servidor
git clone https://git.inteli.edu.br/graduacao/2026-1b/t17/g04.git
cd g04

# 2. Crie o .env com as variáveis do ambiente de produção
#    (solicite os valores ao responsável técnico do grupo)
cp .env.example .env
nano .env

# 3. Suba apenas o container do banco
docker compose up -d db

# 4. Valide a execução dos scripts de inicialização
docker compose exec db psql -U postgres -c "\dt public.*"
```

> A porta `5432` deve estar liberada no firewall do servidor Inteli. Confirme com o professor/tutor responsável pela infraestrutura antes de subir.

### 6.4 Reinicialização e reset

```bash
# Parar e remover containers sem apagar dados
docker compose down

# Reset total — apaga o volume e recria do zero
docker compose down -v
docker compose up -d db
```

---

## 7. Dicionário de Dados

### `usuario`

Espelho do `auth.users` do Supabase Auth. Armazena o perfil público de cada usuário. O `id_usuario` é gerado pelo Supabase Auth no signup — não pelo banco.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_usuario` | `UUID` | PK · FK → `auth.users(id)` · ON DELETE CASCADE | UUID do Supabase Auth. Fonte de verdade para autenticação. |
| `nome` | `VARCHAR(150)` | NOT NULL | Nome de exibição. Preenchido automaticamente pelo trigger de signup. |
| `email` | `VARCHAR(255)` | NOT NULL · UNIQUE · CHECK formato | E-mail único. Validado por regex no banco. |
| `data_nascimento` | `DATE` | — | Usada para filtragem por faixa etária nas recomendações. |
| `localizacao` | `VARCHAR(150)` | — | Cidade/região declarada. Usada para recomendar sessões próximas. |
| `data_cadastro` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Momento do cadastro na plataforma. |
| `tipo_usuario` | `tipo_usuario_enum` | NOT NULL · DEFAULT `'cadastrado'` | Perfil de acesso: `visitante`, `cadastrado` ou `administrador`. |
| `nivel_acesso` | `JSONB` | NOT NULL · DEFAULT `{}` | Permissões granulares. Ex: `{"gerenciar_espetaculos": true}`. |
| `perfil_preferencias` | `JSONB` | NOT NULL · DEFAULT `{}` | Preferências declaradas no onboarding. Alimenta o cold-start do k-NN. |

---

### `categoria`

Classificações temáticas dos espetáculos. Usadas para filtragem no portal e como feature no pipeline de ML.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_categoria` | `SERIAL` | PK | Identificador sequencial gerado automaticamente. |
| `nome` | `VARCHAR(100)` | NOT NULL · UNIQUE | Nome da categoria. Ex: Musical, Drama, Infantil. |
| `descricao` | `TEXT` | — | Descrição livre da categoria para exibição no portal. |

---

### `ator`

Artistas que compõem os elencos. Substitui o campo textual `elenco` em `espetaculo`, permitindo busca e recomendação por artista.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_ator` | `SERIAL` | PK | Identificador sequencial. |
| `nome` | `VARCHAR(150)` | NOT NULL | Nome artístico ou civil do ator. |
| `biografia` | `TEXT` | — | Trajetória do artista para exibição na página de detalhes. |
| `created_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data de inserção no catálogo. |

---

### `espetaculo`

Entidade central do catálogo. Os campos `popularidade` e `nota_media` são caches atualizados por workers externos — não calculados em tempo real.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_espetaculo` | `SERIAL` | PK | Identificador sequencial. |
| `titulo` | `VARCHAR(200)` | NOT NULL | Título oficial do espetáculo. |
| `descricao` | `TEXT` | — | Sinopse para exibição no portal. |
| `genero` | `VARCHAR(100)` | — | Gênero artístico livre. Ex: Musical, Dança. |
| `faixa_etaria` | `VARCHAR(50)` | — | Público-alvo declarado. Ex: Infantil, Adulto. |
| `duracao` | `INTEGER` | CHECK > 0 | Duração em minutos. |
| `classificacao_indicativa` | `VARCHAR(20)` | — | Classificação etária oficial. Ex: Livre, 12 anos. |
| `produtora` | `VARCHAR(150)` | — | Empresa produtora responsável. |
| `status` | `status_espetaculo_enum` | NOT NULL · DEFAULT `'em_breve'` | Estado: `em_cartaz`, `em_breve` ou `encerrado`. |
| `popularidade` | `NUMERIC(10,4)` | NOT NULL · DEFAULT 0 | Cache de popularidade calculado por worker via dados do MongoDB. |
| `nota_media` | `NUMERIC(3,2)` | NOT NULL · DEFAULT 0 · CHECK 0–10 | Média das avaliações explícitas. Atualizada via worker. |
| `id_admin` | `UUID` | FK → `usuario(id_usuario)` · ON DELETE SET NULL | UUID do administrador responsável pelo cadastro. |
| `created_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data de criação do registro. |
| `updated_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data da última atualização. |

---

### `espetaculo_categoria`

Tabela associativa N:M entre `espetaculo` e `categoria`. Um espetáculo pode pertencer a múltiplas categorias.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_espetaculo` | `INTEGER` | PK · FK → `espetaculo` · ON DELETE CASCADE | Referência ao espetáculo. |
| `id_categoria` | `INTEGER` | PK · FK → `categoria` · ON DELETE CASCADE | Referência à categoria. |

---

### `espetaculo_ator`

Tabela associativa N:M entre `espetaculo` e `ator`, com atributos contextuais de papel e ordem nos créditos.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_espetaculo` | `INTEGER` | PK · FK → `espetaculo` · ON DELETE CASCADE | Referência ao espetáculo. |
| `id_ator` | `INTEGER` | PK · FK → `ator` · ON DELETE CASCADE | Referência ao ator. |
| `papel` | `VARCHAR(150)` | — | Personagem ou função. Ex: Protagonista, Direção Musical. |
| `ordem_exibicao` | `INTEGER` | CHECK > 0 | Posição nos créditos do espetáculo. |

---

### `teatro`

Espaços culturais parceiros onde as sessões acontecem.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_teatro` | `SERIAL` | PK | Identificador sequencial. |
| `nome` | `VARCHAR(200)` | NOT NULL | Nome oficial do teatro. |
| `endereco` | `VARCHAR(300)` | NOT NULL | Endereço completo com logradouro e número. |
| `cidade` | `VARCHAR(100)` | NOT NULL | Cidade de localização. |
| `estado` | `CHAR(2)` | NOT NULL | UF em dois caracteres. Ex: SP, RJ. |
| `capacidade` | `INTEGER` | CHECK > 0 | Lotação máxima em número de assentos. |
| `acessibilidade` | `BOOLEAN` | NOT NULL · DEFAULT FALSE | Indica se o espaço possui recursos de acessibilidade. |
| `parceiro_naming_rights` | `VARCHAR(200)` | — | Nome do patrocinador com direito de nomenclatura. |
| `created_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data de cadastro no sistema. |

---

### `sessao`

Apresentação específica: combina espetáculo, teatro, data e horário. A venda de ingressos é delegada a plataformas parceiras via `link_compra`.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_sessao` | `SERIAL` | PK | Identificador sequencial. |
| `id_espetaculo` | `INTEGER` | NOT NULL · FK → `espetaculo` · ON DELETE CASCADE | Espetáculo apresentado. |
| `id_teatro` | `INTEGER` | NOT NULL · FK → `teatro` · ON DELETE RESTRICT | Teatro onde ocorre a sessão. |
| `data` | `DATE` | NOT NULL | Data da apresentação. |
| `horario` | `TIME` | NOT NULL | Horário de início. |
| `preco_minimo` | `NUMERIC(10,2)` | CHECK >= 0 | Menor valor de ingresso disponível. |
| `preco_maximo` | `NUMERIC(10,2)` | CHECK >= 0 | Maior valor de ingresso disponível. |
| `disponibilidade` | `INTEGER` | NOT NULL · DEFAULT 0 · CHECK >= 0 | Cache de ingressos disponíveis. Atualizado via integração com ticketeira. |
| `link_compra` | `TEXT` | — | URL da plataforma de venda parceira (ex: Sympla, Ingresso.com). |
| `created_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data de criação do registro. |

---

### `avaliacao`

Avaliações explícitas (nota + comentário) feitas por usuários cadastrados. Separadas das interações implícitas, que residem no MongoDB.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_avaliacao` | `SERIAL` | PK | Identificador sequencial. |
| `id_usuario` | `UUID` | NOT NULL · FK → `usuario` · ON DELETE CASCADE | Usuário avaliador. |
| `id_espetaculo` | `INTEGER` | NOT NULL · FK → `espetaculo` · ON DELETE CASCADE | Espetáculo avaliado. |
| `nota` | `NUMERIC(4,2)` | NOT NULL · CHECK 0–10 | Nota de 0 a 10 atribuída pelo usuário. |
| `comentario` | `TEXT` | — | Texto livre opcional. |
| `created_at` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Data da avaliação. |
| *(unique)* | — | UNIQUE (`id_usuario`, `id_espetaculo`) | Um usuário avalia cada espetáculo no máximo uma vez. |

---

### `recomendacao`

Saída dos algoritmos de ML (k-NN, SVD, k-means) para cada usuário. O campo `referencia_origem_id` rastreia a interação no MongoDB que originou a recomendação.

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| `id_recomendacao` | `SERIAL` | PK | Identificador sequencial. |
| `id_usuario` | `UUID` | NOT NULL · FK → `usuario` · ON DELETE CASCADE | Usuário que recebe a recomendação. |
| `id_espetaculo` | `INTEGER` | NOT NULL · FK → `espetaculo` · ON DELETE CASCADE | Espetáculo recomendado. |
| `tipo_algoritmo` | `VARCHAR(50)` | NOT NULL · CHECK IN (`'k-NN'`, `'SVD'`, `'k-means'`) | Algoritmo que gerou a recomendação. |
| `score_recomendacao` | `NUMERIC(8,6)` | NOT NULL · CHECK >= 0 | Pontuação de relevância calculada pelo algoritmo. |
| `motivo_recomendacao` | `TEXT` | — | Explicação legível do motivo da recomendação (exibida ao usuário). |
| `data_geracao` | `TIMESTAMP` | NOT NULL · DEFAULT NOW() | Momento em que a recomendação foi gerada. |
| `prioridade_manual` | `BOOLEAN` | NOT NULL · DEFAULT FALSE | Quando TRUE, recomendação foi promovida manualmente por administrador. |
| `referencia_origem_id` | `VARCHAR(50)` | — | ObjectId MongoDB da interação de origem. Garante rastreabilidade entre os dois bancos. |

---

### Índices criados

| Índice | Tabela | Coluna(s) | Finalidade |
|---|---|---|---|
| `idx_espetaculo_status` | `espetaculo` | `status` | Listagem do portal filtrada por status |
| `idx_espetaculo_nota_media` | `espetaculo` | `nota_media DESC` | Ranking por nota |
| `idx_espetaculo_popularidade` | `espetaculo` | `popularidade DESC` | Ranking por popularidade |
| `idx_sessao_espetaculo` | `sessao` | `id_espetaculo` | Agenda de sessões por espetáculo |
| `idx_sessao_data` | `sessao` | `data` | Filtro de sessões por data |
| `idx_sessao_teatro` | `sessao` | `id_teatro` | Sessões por teatro |
| `idx_recomendacao_usuario` | `recomendacao` | `id_usuario` | Feed personalizado por usuário |
| `idx_recomendacao_score` | `recomendacao` | `(id_usuario, score DESC)` | Feed ordenado por relevância |
| `idx_avaliacao_espetaculo` | `avaliacao` | `id_espetaculo` | Cálculo de `nota_media` |
| `idx_espetaculo_ator_ator` | `espetaculo_ator` | `id_ator` | Busca e recomendação por artista |

---

## 8. Regras de Negócio

Cada regra está implementada diretamente no banco de dados. A referência ao trecho do script SQL está indicada em cada item.

---

### RN-01 — E-mail com formato válido

**Tabela:** `usuario`
**Script:** `01_create_database.sql` — `CONSTRAINT chk_email_formato`

```sql
CONSTRAINT chk_email_formato CHECK (email ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$')
```

Todo e-mail deve conter exatamente um `@`, sem espaços, com domínio e extensão. O banco rejeita e-mails malformados antes que cheguem à camada de aplicação.

---

### RN-02 — Nota de avaliação entre 0 e 10

**Tabela:** `avaliacao`
**Script:** `01_create_database.sql` — coluna `nota`

```sql
nota NUMERIC(4, 2) NOT NULL CHECK (nota BETWEEN 0 AND 5)
```

Notas fora do intervalo [0, 5] são rejeitadas pelo banco, garantindo consistência antes do cálculo de `nota_media`.

---

### RN-03 — Um usuário avalia cada espetáculo no máximo uma vez

**Tabela:** `avaliacao`
**Script:** `01_create_database.sql` — `UNIQUE (id_usuario, id_espetaculo)`

```sql
UNIQUE (id_usuario, id_espetaculo)
```

Para atualizar uma nota, o usuário deve excluir a avaliação anterior e criar uma nova.

---

### RN-04 — Preço máximo não pode ser menor que o preço mínimo

**Tabela:** `sessao`
**Script:** `01_create_database.sql` — `CONSTRAINT chk_preco_intervalo`

```sql
CONSTRAINT chk_preco_intervalo CHECK (
    preco_maximo IS NULL OR preco_minimo IS NULL OR preco_maximo >= preco_minimo
)
```

Ambos os campos são opcionais, mas quando presentes, o intervalo deve ser válido.

---

### RN-05 — Duração do espetáculo deve ser positiva

**Tabela:** `espetaculo`
**Script:** `01_create_database.sql` — coluna `duracao`

```sql
duracao INTEGER CHECK (duracao > 0)
```

Espetáculos com duração zero ou negativa são inválidos e rejeitados no INSERT/UPDATE.

---

### RN-06 — Disponibilidade de sessão não pode ser negativa

**Tabela:** `sessao`
**Script:** `01_create_database.sql` — coluna `disponibilidade`

```sql
disponibilidade INTEGER NOT NULL DEFAULT 0 CHECK (disponibilidade >= 0)
```

Workers de integração com ticketeiras devem tratar esse limite antes de atualizar o campo.

---

### RN-07 — Tipo de algoritmo restrito a valores conhecidos

**Tabela:** `recomendacao`
**Script:** `01_create_database.sql` — `CONSTRAINT chk_tipo_algoritmo`

```sql
CONSTRAINT chk_tipo_algoritmo CHECK (
    tipo_algoritmo IN ('k-NN', 'SVD', 'k-means')
)
```

Garante que registros de algoritmos experimentais não entrem em produção sem atualização explícita do schema.

---

### RN-08 — Score de recomendação não negativo

**Tabela:** `recomendacao`
**Script:** `01_create_database.sql` — coluna `score_recomendacao`

```sql
score_recomendacao NUMERIC(8, 6) NOT NULL CHECK (score_recomendacao >= 0)
```

Scores negativos não têm semântica válida nos algoritmos adotados.

---

### RN-09 — Criação automática de perfil após signup

**Tabela:** `usuario`
**Script:** `01_create_database.sql` — trigger `on_auth_user_created`

```sql
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW
    EXECUTE FUNCTION public.handle_new_user();
```

Todo usuário que se cadastra via Supabase Auth tem seu perfil criado automaticamente em `usuario` com o mesmo UUID. Elimina a necessidade de chamada manual à API após o signup.

---

### RN-10 — Leitura restrita ao próprio perfil e às próprias recomendações

**Tabelas:** `usuario`, `recomendacao`
**Script:** `01_create_database.sql` — políticas RLS

```sql
-- usuario
CREATE POLICY "usuario: leitura do próprio perfil"
    ON usuario FOR SELECT
    USING (auth.uid() = id_usuario);

-- recomendacao
CREATE POLICY "recomendacao: leitura pelo próprio usuário"
    ON recomendacao FOR SELECT
    USING (auth.uid() = id_usuario);
```

Usuários autenticados leem apenas seus próprios dados. O `service_role` bypassa o RLS automaticamente para operações administrativas.

---

### RN-11 — Exclusão em cascata dos dados do usuário (LGPD)

**Tabelas:** `avaliacao`, `recomendacao`
**Script:** `01_create_database.sql` — `ON DELETE CASCADE` nas FKs de `id_usuario`

```sql
id_usuario UUID NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE
```

Quando um usuário é removido do Supabase Auth, todas as suas avaliações e recomendações são excluídas automaticamente, em conformidade com a LGPD.

---

### RN-12 — Teatro não pode ser removido com sessões ativas

**Tabela:** `sessao`
**Script:** `01_create_database.sql` — `ON DELETE RESTRICT` na FK de `id_teatro`

```sql
id_teatro INTEGER NOT NULL REFERENCES teatro(id_teatro) ON DELETE RESTRICT
```

A exclusão de um teatro com sessões programadas é bloqueada pelo banco. O administrador deve cancelar ou reatribuir as sessões antes de remover o teatro.