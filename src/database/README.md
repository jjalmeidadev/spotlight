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
- [7. Dicionário de Dados](#dicionário-de-dados)
- [8. Regras de Negócio](#regras-de-negócio-implementadas-no-banco-de-dados)

---

## 1. Visão Geral

O **Spotlight** é uma plataforma de recomendação de espetáculos teatrais que conecta usuários ao catálogo da Aventura Teatros por meio de algoritmos de Machine Learning (k-NN, SVD, k-means). O banco de dados relacional é o núcleo da solução: armazena o catálogo de espetáculos, perfis de usuários, avaliações explícitas e as recomendações geradas pelo motor de ML.

**SGBD:** PostgreSQL 15.x provisionado via **Supabase**

**Projeto Supabase:** `nyaxqgujyyvkjnqzbklf`

**URL do projeto Supabase:** `https://nyaxqgujyyvkjnqzbklf.supabase.co`

**Por que dois bancos?** O PostgreSQL (Supabase) guarda dados transacionais e estruturados — catálogo, perfis, avaliações e recomendações processadas. O MongoDB armazena os eventos brutos de telemetria (cliques, tempo de tela) em alta volumetria. Os workers de ML consomem o MongoDB e escrevem os resultados processados de volta no PostgreSQL, na tabela `recomendacao`.

**Integração com Supabase Auth:** A tabela `usuario` referencia `auth.users` via UUID. O Supabase Auth é a única fonte de verdade para autenticação — senhas nunca são armazenadas no schema público. Um trigger automático (`on_auth_user_created`) cria o perfil do usuário no schema público imediatamente após o signup.

---

## 2. Estrutura de Arquivos

```
src/database/
├── 00_supabase_compat.sql   — prepara extensões e objetos mínimos do schema auth para execução local em Docker
├── 01_create_database.sql   — criação de tabelas, tipos ENUM, índices, RLS e trigger do Auth
├── 02_initial_data.sql      — seed com dados iniciais (usuários, categorias, teatros, atores, espetáculos, sessões, avaliações, recomendações e similaridade item-item)
├── 03_rollback.sql          — desfaz completamente a execução do 01_create_database.sql
└── README.md                — este arquivo
```

---

## 3. Pré-requisitos

| Dependência | Versão mínima | Observação |
|---|---|---|
| PostgreSQL | 15.x | Compatível com Supabase Postgres. Para Docker local, usar `supabase/postgres:15.1.0.147`. |
| Docker | 24.0 | Para implantação local ou no servidor Inteli. |
| Docker Compose | 2.20 | Para orquestração dos containers. |
| Supabase CLI | 1.150 | Opcional — facilita o gerenciamento local. |
| DBeaver | 24.x | Opcional e recomendado para visualizar tabelas e executar scripts por interface gráfica. |
| `psql` | 15.x | Necessário para executar e validar scripts por linha de comando. |

> Para execução apenas via SQL Editor do Supabase (cloud), Docker não é necessário.

> O script de carga inicial pode usar `crypt()` e `gen_salt()`. Portanto, o ambiente precisa ter a extensão `pgcrypto` disponível quando `02_initial_data.sql` for executado.

---

## 4. Variáveis de Ambiente

Crie um arquivo `.env` na **raiz do repositório** (nunca em `src/database`). O arquivo `.env` deve estar no `.gitignore` — **jamais commite segredos no repositório**.

```env
# ─── PostgreSQL ──────────────────────────────────────────────
POSTGRES_DB=postgres
POSTGRES_USER=postgres
POSTGRES_PASSWORD=password
POSTGRES_PORT=5432
POSTGRES_CONNECTION_STRING=postgresql://postgres:password@localhost:5432/postgres

# ─── Supabase Auth ───────────────────────────────────────────
# Gerados via: supabase start  ou  supabase gen keys
JWT_SECRET=seu_jwt_secret_com_minimo_32_caracteres
ANON_KEY=chave_anon_do_supabase
SERVICE_ROLE_KEY=chave_service_role_do_supabase

# ─── URL pública do Supabase ─────────────────────────────────
SUPABASE_URL=https://nyaxqgujyyvkjnqzbklf.supabase.co
# Em ambiente local Supabase: http://localhost:8000

# ─── Frontend / API Gateway ──────────────────────────────────
SITE_URL=http://localhost:3000
```

### Onde cada variável é consumida

| Variável | Consumido por |
|---|---|
| `POSTGRES_DB` | Container PostgreSQL (Docker) — nome do banco padrão |
| `POSTGRES_USER` | Container PostgreSQL (Docker) — usuário padrão |
| `POSTGRES_PASSWORD` | Container PostgreSQL (Docker) — senha do usuário |
| `POSTGRES_PORT` | Docker Compose — porta exposta no host |
| `POSTGRES_CONNECTION_STRING` | Clientes locais, API Gateway e scripts de migração |
| `JWT_SECRET` | API Gateway (ASP.NET Core) — validação offline do JWT |
| `ANON_KEY` | React Native — chamadas públicas à API do Supabase |
| `SERVICE_ROLE_KEY` | Workers de ML e API Gateway — bypass de RLS para operações administrativas |
| `SUPABASE_URL` | React Native e API Gateway — endpoint base do Supabase |

---

## 5. Execução dos Scripts

### 5.1 Via SQL Editor do Supabase

1. Acesse o [Supabase Dashboard](https://supabase.com/dashboard) e abra o projeto
2. Confirme que o projeto aberto é `nyaxqgujyyvkjnqzbklf`
3. Vá em **SQL Editor → New Query**
4. Cole e execute `01_create_database.sql`
5. Aguarde a confirmação de sucesso
6. Cole e execute `02_initial_data.sql`

**Validação:** Acesse **Table Editor** e confirme que as 10 tabelas aparecem no schema `public`.

> Atenção: o `01_create_database.sql` é um script de criação para ambiente limpo. Ele não apaga estruturas existentes. Para resetar um ambiente de desenvolvimento, execute primeiro `03_rollback.sql` e depois rode novamente `01_create_database.sql` e `02_initial_data.sql`.

### 5.2 Via DBeaver

O DBeaver pode ser usado como ferramenta visual para conectar ao PostgreSQL do Supabase, ao banco local via Docker ou ao servidor Inteli.

1. Abra o DBeaver
2. Vá em **Banco de Dados → Nova Conexão**
3. Selecione **PostgreSQL**
4. Preencha os dados de conexão conforme o ambiente:

| Campo | Valor |
|---|---|
| Host | valor de `DB_HOST` ou host informado pelo Supabase/Inteli |
| Porta | valor de `DB_PORT` |
| Database | valor de `DB_NAME` |
| Usuário | valor de `DB_USER` |
| Senha | valor de `DB_PASSWORD` |

5. Clique em **Testar Conexão**
6. Se a conexão funcionar, clique em **Concluir**
7. Abra um editor SQL na conexão criada
8. Execute o conteúdo de `01_create_database.sql`
9. Execute o conteúdo de `02_initial_data.sql`
10. Valide a criação das tabelas com:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

Validação funcional:

```sql
SELECT
  (SELECT COUNT(*) FROM public.usuario) AS usuarios,
  (SELECT COUNT(*) FROM public.espetaculo) AS espetaculos,
  (SELECT COUNT(*) FROM public.sessao) AS sessoes;
```

> O DBeaver não armazena o banco. Ele apenas conecta em um PostgreSQL existente, como Supabase, Docker local ou servidor Inteli.

### 5.3 Via `psql` (linha de comando)

```bash
# Substitua os valores conforme seu ambiente
export PGPASSWORD=$DB_PASSWORD

# 1. Cria todas as estruturas
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
  -f src/database/01_create_database.sql

# 2. Insere os dados iniciais
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
  -f src/database/02_initial_data.sql

# Validação: lista as tabelas criadas
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
  -c "\dt public.*"

# Validação funcional: conta registros principais
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME \
  -c "SELECT 'usuarios' AS tabela, COUNT(*) FROM public.usuario UNION ALL SELECT 'espetaculos', COUNT(*) FROM public.espetaculo UNION ALL SELECT 'sessoes', COUNT(*) FROM public.sessao;"
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

### 5.4 Rollback (desfaz tudo)

```bash
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/03_rollback.sql
```

> O rollback é **destrutivo**. Apaga todos os dados e estruturas. Use apenas em desenvolvimento ou para reset total do ambiente.

### 5.5 Ordem obrigatória de execução

```
00_supabase_compat.sql  →  01_create_database.sql  →  02_initial_data.sql
```

- `00_supabase_compat.sql` prepara extensões e o schema `auth` mínimo (necessário apenas para execução **local em Docker** — no Supabase cloud esses objetos já existem nativamente).
- `01_create_database.sql` cria todas as estruturas do schema `public`.
- `02_initial_data.sql` depende das tabelas criadas pelo `01`. Nunca inverta a ordem.

---

## 6. Implantação

### 6.1 Docker Compose 

A raiz do repositório contém o arquivo `docker-compose.yml` responsável por subir o banco `db`.

Ele usa a imagem `postgres:15-alpine`, expõe a porta configurada em `POSTGRES_PORT`, persiste os dados no volume nomeado `spotlight_db_data` e executa automaticamente os scripts de inicialização em ordem:

```text
00_supabase_compat.sql -> 01_create_database.sql -> 02_initial_data.sql
```

O script `00_supabase_compat.sql` cria a extensão `pgcrypto`, o schema `auth`, a tabela mínima `auth.users` e a função `auth.uid()`. Isso permite executar localmente os scripts modelados para Supabase sem precisar subir toda a stack do Supabase Auth.

> Os scripts em `docker-entrypoint-initdb.d/` são executados automaticamente na **primeira inicialização** do container, em ordem alfabética. Em reinicializações subsequentes, o volume já contém os dados e os scripts não são re-executados.

### 6.2 Subindo o banco

```bash
# Na raiz do repositório
cp .env.example .env

# Edite POSTGRES_PASSWORD antes de compartilhar o ambiente
nano .env

# Suba o banco
docker compose up -d db

# Acompanhe os logs para confirmar a inicialização
docker compose logs -f db

# Saída esperada ao final:
# db  | database system is ready to accept connections

# Validação funcional
docker compose exec db psql -U postgres -d postgres \
  -c "SELECT COUNT(*) AS total_espetaculos FROM public.espetaculo;"
```

Valide as tabelas e os dados iniciais:

```bash
docker compose exec db psql -U postgres -d postgres -c "\dt public.*"
docker compose exec db psql -U postgres -d postgres -c "SELECT COUNT(*) FROM espetaculo;"
```
### 6.3 Instruções específicas

Para o projeto Supabase cloud (`nyaxqgujyyvkjnqzbklf`), execute os scripts pelo **SQL Editor** ou por `psql` usando a connection string do painel do Supabase.

Ordem:

```
01_create_database.sql
02_initial_data.sql
```

Validação recomendada no SQL Editor:

```sql
SELECT
  (SELECT COUNT(*) FROM public.usuario) AS usuarios,
  (SELECT COUNT(*) FROM public.espetaculo) AS espetaculos,
  (SELECT COUNT(*) FROM public.sessao) AS sessoes;
```

> Não execute `03_rollback.sql` no Supabase cloud se o ambiente já tiver dados úteis. O rollback é destrutivo e deve ser usado apenas para reset autorizado do ambiente.

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
docker compose exec db psql -U postgres -d postgres -c "\dt public.*"
```

> A porta `5432` deve estar liberada no firewall do servidor Inteli. Confirme com o professor/tutor responsável pela infraestrutura antes de subir.

### 6.5 Ajustes e quirks conhecidos

- **Encoding:** use UTF-8 (`LANG=C.UTF-8`) para evitar problemas com acentos em dados textuais.
- **Timezone:** use `America/Sao_Paulo` quando o ambiente permitir configuração de `TZ`.
- **Extensão `pgcrypto`:** necessária para `crypt()` e `gen_salt()` no seed de usuários do Supabase Auth.
- **Script de criação:** `01_create_database.sql` deve ser executado em ambiente limpo. Ele não deve ser usado como migração incremental.
- **Reset destrutivo:** use `03_rollback.sql` apenas quando houver intenção explícita de apagar as estruturas e dados do ambiente.
- **Dados iniciais do Auth:** se o seed inserir registros em `auth.users`, os UUIDs precisam respeitar o formato válido `8-4-4-4-12` e a execução deve ocorrer em ambiente com permissão para escrita no schema `auth`.

### 6.6 Reinicialização e reset

```bash
# Parar e remover containers sem apagar dados
docker compose down

# Reset total — apaga o volume e recria do zero
docker compose down -v
docker compose up -d db
```

### 6.7 Conclusão da implantação

Ao final da implantação, o ambiente estará pronto quando:

- o PostgreSQL/Supabase estiver acessível pelo Supabase SQL Editor, DBeaver, `psql` ou container Docker;
- o script `01_create_database.sql` tiver criado as 11 tabelas do schema `public`;
- o script `02_initial_data.sql` tiver populado os dados iniciais sem erros de integridade referencial;
- as consultas de validação retornarem registros em `usuario`, `espetaculo` e `sessao`.

Com esses passos concluídos, o banco estará apto para uso pelo frontend, API Gateway, serviços SOA e rotinas de recomendação em ambiente de desenvolvimento, homologação ou implantação no servidor Inteli.

---

## Dicionário de Dados

### 1. Visão Geral

O banco de dados modela o domínio de um sistema de consulta, avaliação e recomendação de espetáculos teatrais. Ele contempla o cadastro de usuários, espetáculos, categorias, atores, teatros, sessões, avaliações, recomendações personalizadas e similaridade item-item entre espetáculos.

| Item | Quantidade |
|---|---:|
| Tabelas | 11 |
| Tipos enumerados | 2 |
| Funções | 1 |
| Triggers | 1 |
| Índices explícitos | 11 |

#### Tabelas do Modelo

| Ordem | Tabela | Papel no modelo |
|---:|---|---|
| 1 | `usuario` | Perfil de usuário integrado ao provedor de autenticação. |
| 2 | `categoria` | Domínio de classificação dos espetáculos. |
| 3 | `ator` | Cadastro de atores do catálogo. |
| 4 | `teatro` | Cadastro dos espaços físicos de apresentação. |
| 5 | `espetaculo` | Cadastro principal dos espetáculos. |
| 6 | `espetaculo_categoria` | Associação N:N entre espetáculos e categorias. |
| 7 | `espetaculo_ator` | Associação N:N entre espetáculos e atores. |
| 8 | `sessao` | Agenda de apresentações dos espetáculos nos teatros. |
| 9 | `avaliacao` | Avaliações de usuários sobre espetáculos. |
| 10 | `recomendacao` | Recomendações personalizadas geradas para usuários. |
| 11 | `similaridade_item` | Similaridade item-item global entre espetáculos (populada em batch pelo k-NN). |

#### Tipos Enumerados

##### `tipo_usuario_enum`

| Valor | Descrição |
|---|---|
| `visitante` | Usuário com acesso limitado ou perfil não autenticado plenamente. |
| `cadastrado` | Usuário autenticado comum. É o valor padrão. |
| `administrador` | Usuário com permissões administrativas previstas pelo modelo. |

##### `status_espetaculo_enum`

| Valor | Descrição |
|---|---|
| `em_cartaz` | Espetáculo atualmente em exibição. |
| `em_breve` | Espetáculo anunciado, ainda não disponível. É o valor padrão. |
| `encerrado` | Espetáculo que não está mais em exibição. |

### 2. Tabela `usuario`

#### Descrição

Armazena os perfis dos usuários da aplicação, vinculados diretamente à tabela externa `auth.users`. Essa estrutura é compatível com ambientes Supabase/PostgreSQL com autenticação gerenciada.

#### Tipo de Tabela

Entidade principal.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `usuario: leitura do próprio perfil` | `SELECT` | Permite leitura apenas do próprio perfil: `auth.uid() = id_usuario`. |
| `usuario: atualização do próprio perfil` | `UPDATE` | Permite atualização apenas do próprio perfil: `auth.uid() = id_usuario`. |
| `usuario: inserção via trigger de auth` | `INSERT` | Permite inserção via trigger com `WITH CHECK (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_usuario` | `UUID` | Não | - | PK / FK | Referencia `auth.users(id)` com `ON DELETE CASCADE` | Identificador único do usuário, sincronizado com o provedor de autenticação. |
| `nome` | `VARCHAR(150)` | Não | - | - | - | Nome exibido do usuário. |
| `email` | `VARCHAR(255)` | Não | - | - | `UNIQUE`, `chk_email_formato` | E-mail do usuário. Deve ser único e seguir formato válido. |
| `data_nascimento` | `DATE` | Sim | - | - | - | Data de nascimento do usuário. |
| `localizacao` | `JSONB` | Sim | - | - | - | Localização consentida do usuário, com endereço normalizado, coordenadas e provedor. |
| `data_cadastro` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de criação do perfil. |
| `tipo_usuario` | `tipo_usuario_enum` | Não | `'cadastrado'` | - | Valores do ENUM | Tipo do usuário no sistema. |
| `nivel_acesso` | `JSONB` | Não | `'{}'::JSONB` | - | - | Estrutura flexível para permissões, papéis ou escopos adicionais. |
| `perfil_preferencias` | `JSONB` | Não | `'{}'::JSONB` | - | - | Preferências do usuário para personalização e recomendação. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `usuario_pkey` | Primary Key | `id_usuario` |
| `usuario_email_key` | Unique | `email` |
| `chk_email_formato` | Check | `email ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$'` |
| FK implícita | Foreign Key | `id_usuario -> auth.users(id) ON DELETE CASCADE` |

#### Campos JSONB

O campo `nivel_acesso` pode armazenar permissões e escopos adicionais, por exemplo:

```json
{
  "permissoes": ["gerenciar_espetaculos", "gerenciar_sessoes"],
  "escopos": ["admin"]
}
```

O campo `perfil_preferencias` pode armazenar preferências de recomendação, por exemplo:

```json
{
  "categorias_favoritas": [1, 3, 5],
  "generos": ["drama", "musical"],
  "cidade_preferida": "São Paulo",
  "faixa_preco": {
    "min": 50,
    "max": 200
  }
}
```

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK recebida | `auth.users(id)` -> `usuario(id_usuario)` | Um usuário autenticado possui um perfil. |
| Referenciada por | `espetaculo(id_admin)` | Um usuário administrador pode gerenciar vários espetáculos. |
| Referenciada por | `avaliacao(id_usuario)` | Um usuário pode registrar várias avaliações. |
| Referenciada por | `recomendacao(id_usuario)` | Um usuário pode receber várias recomendações. |

#### Índices

Não há índices explícitos além dos índices automáticos criados pela chave primária e pela constraint `UNIQUE` sobre `email`.

#### Triggers e Functions

| Objeto | Tipo | Descrição |
|---|---|---|
| `public.handle_new_user()` | Function | Insere automaticamente um perfil em `usuario` após a criação de um registro em `auth.users`. |
| `on_auth_user_created` | Trigger | Executada `AFTER INSERT ON auth.users`, chama `public.handle_new_user()`. |

### 3. Tabela `categoria`

#### Descrição

Armazena categorias usadas para classificar espetáculos, como drama, comédia, musical, infantil, entre outras.

#### Tipo de Tabela

Lookup / domínio de classificação.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `categoria: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_categoria` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único da categoria. |
| `nome` | `VARCHAR(100)` | Não | - | - | `UNIQUE` | Nome da categoria. |
| `descricao` | `TEXT` | Sim | - | - | - | Descrição textual da categoria. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `categoria_pkey` | Primary Key | `id_categoria` |
| `categoria_nome_key` | Unique | `nome` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| Referenciada por | `espetaculo_categoria(id_categoria)` | Uma categoria pode estar associada a vários espetáculos. |

#### Índices

Não há índices explícitos além dos índices automáticos da chave primária e da constraint `UNIQUE` sobre `nome`.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 4. Tabela `ator`

#### Descrição

Armazena informações sobre atores participantes dos espetáculos.

#### Tipo de Tabela

Entidade principal.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `ator: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_ator` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único do ator. |
| `nome` | `VARCHAR(150)` | Não | - | - | - | Nome do ator. |
| `biografia` | `TEXT` | Sim | - | - | - | Texto biográfico do ator. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de cadastro do ator. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `ator_pkey` | Primary Key | `id_ator` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| Referenciada por | `espetaculo_ator(id_ator)` | Um ator pode participar de vários espetáculos. |

#### Índices

Não há índices explícitos além do índice automático da chave primária.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 5. Tabela `teatro`

#### Descrição

Armazena os locais físicos onde os espetáculos são apresentados.

#### Tipo de Tabela

Entidade principal.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `teatro: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_teatro` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único do teatro. |
| `nome` | `VARCHAR(200)` | Não | - | - | - | Nome do teatro. |
| `endereco` | `VARCHAR(300)` | Não | - | - | - | Endereço completo do teatro. |
| `cidade` | `VARCHAR(100)` | Não | - | - | - | Cidade onde o teatro está localizado. |
| `estado` | `CHAR(2)` | Não | - | - | - | Unidade federativa em formato de sigla. |
| `capacidade` | `INTEGER` | Sim | - | - | `CHECK (capacidade > 0)` | Capacidade máxima de público. |
| `acessibilidade` | `BOOLEAN` | Não | `FALSE` | - | - | Indica se o teatro possui recursos de acessibilidade. |
| `parceiro_naming_rights` | `VARCHAR(200)` | Sim | - | - | - | Nome de parceiro comercial associado ao naming rights. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de cadastro do teatro. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `teatro_pkey` | Primary Key | `id_teatro` |
| Check implícito | Check | `capacidade > 0` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| Referenciada por | `sessao(id_teatro)` | Um teatro pode possuir várias sessões. |

#### Índices

Não há índices explícitos próprios além do índice automático da chave primária. A coluna é apoiada pelo índice `idx_sessao_teatro`, criado na tabela `sessao`.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 6. Tabela `espetaculo`

#### Descrição

Armazena os espetáculos disponíveis no sistema, incluindo dados descritivos, classificação, status, popularidade e nota média.

#### Tipo de Tabela

Entidade principal.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `espetaculo: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_espetaculo` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único do espetáculo. |
| `titulo` | `VARCHAR(200)` | Não | - | - | - | Título do espetáculo. |
| `descricao` | `TEXT` | Sim | - | - | - | Descrição textual do espetáculo. |
| `genero` | `VARCHAR(100)` | Sim | - | - | - | Gênero principal do espetáculo. |
| `faixa_etaria` | `VARCHAR(50)` | Sim | - | - | - | Faixa etária recomendada. |
| `duracao` | `INTEGER` | Sim | - | - | `CHECK (duracao > 0)` | Duração do espetáculo, preferencialmente em minutos. |
| `classificacao_indicativa` | `VARCHAR(20)` | Sim | - | - | - | Classificação indicativa oficial ou editorial. |
| `produtora` | `VARCHAR(150)` | Sim | - | - | - | Produtora responsável pelo espetáculo. |
| `status` | `status_espetaculo_enum` | Não | `'em_breve'` | - | Valores do ENUM | Situação atual do espetáculo. |
| `popularidade` | `NUMERIC(10,4)` | Não | `0` | - | - | Métrica numérica de popularidade. |
| `nota_media` | `NUMERIC(3,2)` | Não | `0` | - | `CHECK (nota_media BETWEEN 0 AND 5)` | Nota média consolidada das avaliações. |
| `id_admin` | `UUID` | Sim | - | FK | Referencia `usuario(id_usuario) ON DELETE SET NULL` | Usuário administrador responsável pelo cadastro ou gestão. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de criação do registro. |
| `updated_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora da última atualização. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `espetaculo_pkey` | Primary Key | `id_espetaculo` |
| Check implícito | Check | `duracao > 0` |
| Check implícito | Check | `nota_media BETWEEN 0 AND 5` |
| FK implícita | Foreign Key | `id_admin -> usuario(id_usuario) ON DELETE SET NULL` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `usuario(id_usuario)` <- `espetaculo(id_admin)` | Um administrador pode gerenciar vários espetáculos. |
| Referenciada por | `espetaculo_categoria(id_espetaculo)` | Um espetáculo pode ter várias categorias. |
| Referenciada por | `espetaculo_ator(id_espetaculo)` | Um espetáculo pode ter vários atores. |
| Referenciada por | `sessao(id_espetaculo)` | Um espetáculo pode possuir várias sessões. |
| Referenciada por | `avaliacao(id_espetaculo)` | Um espetáculo pode receber várias avaliações. |
| Referenciada por | `recomendacao(id_espetaculo)` | Um espetáculo pode aparecer em várias recomendações. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_espetaculo_status` | `status` | Otimiza filtros por status do espetáculo. |
| `idx_espetaculo_nota_media` | `nota_media DESC` | Otimiza rankings por nota média. |
| `idx_espetaculo_popularidade` | `popularidade DESC` | Otimiza rankings por popularidade. |

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 7. Tabela `espetaculo_categoria`

#### Descrição

Tabela associativa que representa a relação muitos-para-muitos entre espetáculos e categorias.

#### Tipo de Tabela

Associativa / junção N:N.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `espetaculo_categoria: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_espetaculo` | `INTEGER` | Não | - | PK / FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo associado à categoria. |
| `id_categoria` | `INTEGER` | Não | - | PK / FK | Referencia `categoria(id_categoria) ON DELETE CASCADE` | Categoria associada ao espetáculo. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `espetaculo_categoria_pkey` | Primary Key composta | `(id_espetaculo, id_categoria)` |
| FK implícita | Foreign Key | `id_espetaculo -> espetaculo(id_espetaculo) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_categoria -> categoria(id_categoria) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `espetaculo(id_espetaculo)` <- `espetaculo_categoria(id_espetaculo)` | Muitos registros podem apontar para um espetáculo. |
| FK | `categoria(id_categoria)` <- `espetaculo_categoria(id_categoria)` | Muitos registros podem apontar para uma categoria. |

#### Índices

Não há índices explícitos. A chave primária composta cria índice automático em `(id_espetaculo, id_categoria)`.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 8. Tabela `espetaculo_ator`

#### Descrição

Tabela associativa que representa a relação muitos-para-muitos entre espetáculos e atores, incluindo informações específicas da participação do ator no espetáculo.

#### Tipo de Tabela

Associativa / junção N:N com atributos próprios.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `espetaculo_ator: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_espetaculo` | `INTEGER` | Não | - | PK / FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo em que o ator participa. |
| `id_ator` | `INTEGER` | Não | - | PK / FK | Referencia `ator(id_ator) ON DELETE CASCADE` | Ator participante do espetáculo. |
| `papel` | `VARCHAR(150)` | Sim | - | - | - | Nome do personagem ou função no espetáculo. |
| `ordem_exibicao` | `INTEGER` | Sim | - | - | `CHECK (ordem_exibicao > 0)` | Ordem de exibição do ator no elenco. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `espetaculo_ator_pkey` | Primary Key composta | `(id_espetaculo, id_ator)` |
| Check implícito | Check | `ordem_exibicao > 0` |
| FK implícita | Foreign Key | `id_espetaculo -> espetaculo(id_espetaculo) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_ator -> ator(id_ator) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `espetaculo(id_espetaculo)` <- `espetaculo_ator(id_espetaculo)` | Muitos registros podem apontar para um espetáculo. |
| FK | `ator(id_ator)` <- `espetaculo_ator(id_ator)` | Muitos registros podem apontar para um ator. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_espetaculo_ator_ator` | `id_ator` | Otimiza consultas de espetáculos por ator. |

A chave primária composta também cria índice automático em `(id_espetaculo, id_ator)`.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 9. Tabela `sessao`

#### Descrição

Armazena as sessões de apresentação dos espetáculos, associando espetáculo, teatro, data, horário, preços, disponibilidade e link de compra.

#### Tipo de Tabela

Entidade transacional / agenda de apresentações.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `sessao: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_sessao` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único da sessão. |
| `id_espetaculo` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo apresentado na sessão. |
| `id_teatro` | `INTEGER` | Não | - | FK | Referencia `teatro(id_teatro) ON DELETE RESTRICT` | Teatro onde a sessão ocorre. |
| `data` | `DATE` | Não | - | - | - | Data da sessão. |
| `horario` | `TIME` | Não | - | - | - | Horário da sessão. |
| `preco_minimo` | `NUMERIC(10,2)` | Sim | - | - | `CHECK (preco_minimo >= 0)` | Menor preço disponível para a sessão. |
| `preco_maximo` | `NUMERIC(10,2)` | Sim | - | - | `CHECK (preco_maximo >= 0)` | Maior preço disponível para a sessão. |
| `disponibilidade` | `INTEGER` | Não | `0` | - | `CHECK (disponibilidade >= 0)` | Quantidade disponível de ingressos ou assentos. |
| `link_compra` | `TEXT` | Sim | - | - | - | URL para compra de ingressos. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de criação da sessão. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `sessao_pkey` | Primary Key | `id_sessao` |
| Check implícito | Check | `preco_minimo >= 0` |
| Check implícito | Check | `preco_maximo >= 0` |
| Check implícito | Check | `disponibilidade >= 0` |
| `chk_preco_intervalo` | Check | `preco_maximo IS NULL OR preco_minimo IS NULL OR preco_maximo >= preco_minimo` |
| FK implícita | Foreign Key | `id_espetaculo -> espetaculo(id_espetaculo) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_teatro -> teatro(id_teatro) ON DELETE RESTRICT` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `espetaculo(id_espetaculo)` <- `sessao(id_espetaculo)` | Um espetáculo pode ter várias sessões. |
| FK | `teatro(id_teatro)` <- `sessao(id_teatro)` | Um teatro pode receber várias sessões. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_sessao_espetaculo` | `id_espetaculo` | Otimiza consultas de sessões por espetáculo. |
| `idx_sessao_data` | `data` | Otimiza consultas por data. |
| `idx_sessao_teatro` | `id_teatro` | Otimiza consultas de sessões por teatro. |

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 10. Tabela `avaliacao`

#### Descrição

Armazena avaliações feitas por usuários para espetáculos, contendo nota e comentário opcional.

#### Tipo de Tabela

Entidade transacional / associativa com atributos.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `avaliacao: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |
| `avaliacao: inserção pelo próprio usuário` | `INSERT` | Usuário só pode inserir avaliação para si mesmo: `auth.uid() = id_usuario`. |
| `avaliacao: exclusão pelo próprio usuário` | `DELETE` | Usuário só pode excluir sua própria avaliação: `auth.uid() = id_usuario`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_avaliacao` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único da avaliação. |
| `id_usuario` | `UUID` | Não | - | FK | Referencia `usuario(id_usuario) ON DELETE CASCADE` | Usuário autor da avaliação. |
| `id_espetaculo` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo avaliado. |
| `nota` | `NUMERIC(4,2)` | Não | - | - | `CHECK (nota BETWEEN 0 AND 5)` | Nota atribuída pelo usuário. |
| `comentario` | `TEXT` | Sim | - | - | - | Comentário textual opcional. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora da avaliação. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `avaliacao_pkey` | Primary Key | `id_avaliacao` |
| Unique implícita | Unique | `(id_usuario, id_espetaculo)` |
| Check implícito | Check | `nota BETWEEN 0 AND 5` |
| FK implícita | Foreign Key | `id_usuario -> usuario(id_usuario) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_espetaculo -> espetaculo(id_espetaculo) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `usuario(id_usuario)` <- `avaliacao(id_usuario)` | Um usuário pode realizar várias avaliações. |
| FK | `espetaculo(id_espetaculo)` <- `avaliacao(id_espetaculo)` | Um espetáculo pode receber várias avaliações. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_avaliacao_espetaculo` | `id_espetaculo` | Otimiza consultas de avaliações por espetáculo. |

Também são criados índices automáticos pela chave primária e pela restrição `UNIQUE (id_usuario, id_espetaculo)`.

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 11. Tabela `recomendacao`

#### Descrição

Armazena recomendações personalizadas de espetáculos para usuários, geradas por algoritmos de recomendação ou por priorização manual.

#### Tipo de Tabela

Entidade transacional / saída de algoritmo de recomendação.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `recomendacao: leitura pelo próprio usuário` | `SELECT` | Usuário só pode consultar recomendações destinadas a ele: `auth.uid() = id_usuario`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_recomendacao` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único da recomendação. |
| `id_usuario` | `UUID` | Não | - | FK | Referencia `usuario(id_usuario) ON DELETE CASCADE` | Usuário destinatário da recomendação. |
| `id_espetaculo` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo recomendado. |
| `tipo_algoritmo` | `VARCHAR(50)` | Não | - | - | `chk_tipo_algoritmo` | Algoritmo utilizado para gerar a recomendação. |
| `score_recomendacao` | `NUMERIC(8,6)` | Não | - | - | `CHECK (score_recomendacao >= 0)` | Pontuação calculada pelo algoritmo. |
| `motivo_recomendacao` | `TEXT` | Sim | - | - | - | Explicação textual da recomendação. |
| `data_geracao` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de geração da recomendação. |
| `prioridade_manual` | `BOOLEAN` | Não | `FALSE` | - | - | Indica se houve priorização editorial ou manual. |
| `referencia_origem_id` | `VARCHAR(50)` | Sim | - | - | - | Identificador externo ou referência da origem da recomendação. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `recomendacao_pkey` | Primary Key | `id_recomendacao` |
| Check implícito | Check | `score_recomendacao >= 0` |
| `chk_tipo_algoritmo` | Check | `tipo_algoritmo IN ('k-NN', 'SVD', 'k-means')` |
| FK implícita | Foreign Key | `id_usuario -> usuario(id_usuario) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_espetaculo -> espetaculo(id_espetaculo) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `usuario(id_usuario)` <- `recomendacao(id_usuario)` | Um usuário pode receber várias recomendações. |
| FK | `espetaculo(id_espetaculo)` <- `recomendacao(id_espetaculo)` | Um espetáculo pode ser recomendado para vários usuários. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_recomendacao_usuario` | `id_usuario` | Otimiza consultas de recomendações por usuário. |
| `idx_recomendacao_score` | `id_usuario, score_recomendacao DESC` | Otimiza listagem das melhores recomendações por usuário. |

#### Triggers e Functions

Não há triggers ou functions associadas diretamente.

### 12. Tabela `similaridade_item`

#### Descrição

Armazena a similaridade item-item entre espetáculos calculada de forma **global** (igual para todos os usuários) pelo motor de Machine Learning. Suporta a rota pública `GET /recomendacao/espetaculos/{id}/similares`, que apresenta espetáculos parecidos com um de referência (ex.: na tela de detalhes do app).

#### Tipo de Tabela

Saída de algoritmo de recomendação / artefato de cache de similaridade global.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `similaridade_item: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. Os dados são globais e não envolvem identidade de usuário. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_similaridade` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único do registro de similaridade. |
| `id_espetaculo_origem` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo de referência. |
| `id_espetaculo_similar` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo apontado como similar ao de origem. |
| `tipo_algoritmo` | `VARCHAR(50)` | Não | - | - | `chk_tipo_algoritmo_similar` | Algoritmo utilizado para calcular a similaridade. |
| `score_similaridade` | `NUMERIC(8,6)` | Não | - | - | `CHECK (score_similaridade BETWEEN 0 AND 1)` | Pontuação de similaridade normalizada no intervalo [0,1]. |
| `data_geracao` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de geração do registro pelo batch de ML. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `similaridade_item_pkey` | Primary Key | `id_similaridade` |
| Check implícito | Check | `score_similaridade BETWEEN 0 AND 1` |
| `chk_tipo_algoritmo_similar` | Check | `tipo_algoritmo IN ('k-NN', 'SVD', 'k-means')` |
| `chk_espetaculos_distintos` | Check | `id_espetaculo_origem <> id_espetaculo_similar` |
| `uq_similaridade_item` | Unique | `(id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo)` |
| FK implícita | Foreign Key | `id_espetaculo_origem -> espetaculo(id_espetaculo) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_espetaculo_similar -> espetaculo(id_espetaculo) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `espetaculo(id_espetaculo)` <- `similaridade_item(id_espetaculo_origem)` | Um espetáculo pode ter vários similares apontando para ele como origem. |
| FK | `espetaculo(id_espetaculo)` <- `similaridade_item(id_espetaculo_similar)` | Um espetáculo pode ser apontado como similar de vários outros. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_similaridade_item_origem` | `id_espetaculo_origem, score_similaridade DESC` | Otimiza a consulta "dado um espetáculo, listar similares ordenados por score". |

#### Triggers e Functions

Não há triggers ou functions associadas diretamente. A tabela é populada em batch pelo pipeline de ML (k-NN).

### 13. Relacionamentos Gerais do Modelo

| Relacionamento | Cardinalidade | Implementação |
|---|---|---|
| `auth.users` -> `usuario` | 1:1 | `usuario.id_usuario` referencia `auth.users.id`. |
| `usuario` -> `espetaculo` | 1:N | `espetaculo.id_admin`. |
| `espetaculo` -> `categoria` | N:N | `espetaculo_categoria`. |
| `espetaculo` -> `ator` | N:N | `espetaculo_ator`. |
| `espetaculo` -> `sessao` | 1:N | `sessao.id_espetaculo`. |
| `teatro` -> `sessao` | 1:N | `sessao.id_teatro`. |
| `usuario` -> `avaliacao` | 1:N | `avaliacao.id_usuario`. |
| `espetaculo` -> `avaliacao` | 1:N | `avaliacao.id_espetaculo`. |
| `usuario` -> `recomendacao` | 1:N | `recomendacao.id_usuario`. |
| `espetaculo` -> `recomendacao` | 1:N | `recomendacao.id_espetaculo`. |
| `espetaculo` -> `similaridade_item` | 1:N | `similaridade_item.id_espetaculo_origem`. |
| `espetaculo` -> `similaridade_item` | 1:N | `similaridade_item.id_espetaculo_similar`. |

### 14. Índices Criados

| Índice | Tabela | Colunas | Finalidade |
|---|---|---|---|
| `idx_espetaculo_status` | `espetaculo` | `status` | Otimiza filtros por status. |
| `idx_espetaculo_nota_media` | `espetaculo` | `nota_media DESC` | Otimiza rankings por nota média. |
| `idx_espetaculo_popularidade` | `espetaculo` | `popularidade DESC` | Otimiza rankings por popularidade. |
| `idx_sessao_espetaculo` | `sessao` | `id_espetaculo` | Otimiza busca de sessões por espetáculo. |
| `idx_sessao_data` | `sessao` | `data` | Otimiza busca de sessões por data. |
| `idx_sessao_teatro` | `sessao` | `id_teatro` | Otimiza busca de sessões por teatro. |
| `idx_recomendacao_usuario` | `recomendacao` | `id_usuario` | Otimiza busca de recomendações por usuário. |
| `idx_recomendacao_score` | `recomendacao` | `id_usuario, score_recomendacao DESC` | Otimiza ranking de recomendações por usuário. |
| `idx_avaliacao_espetaculo` | `avaliacao` | `id_espetaculo` | Otimiza busca de avaliações por espetáculo. |
| `idx_espetaculo_ator_ator` | `espetaculo_ator` | `id_ator` | Otimiza busca de espetáculos por ator. |
| `idx_similaridade_item_origem` | `similaridade_item` | `id_espetaculo_origem, score_similaridade DESC` | Otimiza a consulta dos similares de um espetáculo de referência. |

## Regras de Negócio Implementadas no Banco de Dados

Esta seção consolida as regras de negócio que devem ser garantidas diretamente pelo banco de dados, por meio de chaves primárias, chaves estrangeiras, constraints, tipos enumerados, políticas de Row Level Security, triggers e funções.

| ID | Regra de negócio | Implementação no banco | Objeto SQL |
|---|---|---|---|
| RN-BD-01 | Todo usuário da aplicação deve estar associado a um usuário autenticado. | Chave estrangeira com remoção em cascata. | `usuario.id_usuario -> auth.users(id) ON DELETE CASCADE` |
| RN-BD-02 | O perfil de usuário deve ser criado automaticamente quando um usuário for cadastrado no provedor de autenticação. | Trigger `AFTER INSERT` em `auth.users` executando função de criação de perfil. | `on_auth_user_created`, `public.handle_new_user()` |
| RN-BD-03 | O e-mail do usuário deve ser único e possuir formato válido. | Constraint `UNIQUE` e constraint `CHECK` com expressão regular. | `usuario.email`, `chk_email_formato` |
| RN-BD-04 | Usuários só podem ler e atualizar o próprio perfil. | Políticas de Row Level Security com `auth.uid() = id_usuario`. | Políticas `usuario: leitura do próprio perfil` e `usuario: atualização do próprio perfil` |
| RN-BD-05 | O tipo de usuário deve ser limitado aos papéis previstos pelo sistema. | Tipo enumerado com valor padrão. | `tipo_usuario_enum`, `usuario.tipo_usuario DEFAULT 'cadastrado'` |
| RN-BD-06 | O status do espetáculo deve ser limitado aos estados válidos do ciclo de exibição. | Tipo enumerado com valor padrão. | `status_espetaculo_enum`, `espetaculo.status DEFAULT 'em_breve'` |
| RN-BD-07 | O nome de cada categoria deve ser único. | Constraint `UNIQUE`. | `categoria.nome` |
| RN-BD-08 | A duração de um espetáculo, quando informada, deve ser maior que zero. | Constraint `CHECK`. | `espetaculo.duracao CHECK (duracao > 0)` |
| RN-BD-09 | A nota média de um espetáculo deve permanecer entre 0 e 5. | Constraint `CHECK` e valor padrão. | `espetaculo.nota_media CHECK (nota_media BETWEEN 0 AND 5)` |
| RN-BD-10 | A exclusão do administrador responsável não deve remover o espetáculo. | Chave estrangeira com `ON DELETE SET NULL`. | `espetaculo.id_admin -> usuario(id_usuario)` |
| RN-BD-11 | Um espetáculo não pode ser associado mais de uma vez à mesma categoria. | Chave primária composta. | `espetaculo_categoria(id_espetaculo, id_categoria)` |
| RN-BD-12 | As associações de categoria devem ser removidas quando o espetáculo ou a categoria forem excluídos. | Chaves estrangeiras com `ON DELETE CASCADE`. | `espetaculo_categoria.id_espetaculo`, `espetaculo_categoria.id_categoria` |
| RN-BD-13 | Um ator não pode ser associado mais de uma vez ao mesmo espetáculo. | Chave primária composta. | `espetaculo_ator(id_espetaculo, id_ator)` |
| RN-BD-14 | A ordem de exibição do ator, quando informada, deve ser maior que zero. | Constraint `CHECK`. | `espetaculo_ator.ordem_exibicao CHECK (ordem_exibicao > 0)` |
| RN-BD-15 | As associações de elenco devem ser removidas quando o espetáculo ou o ator forem excluídos. | Chaves estrangeiras com `ON DELETE CASCADE`. | `espetaculo_ator.id_espetaculo`, `espetaculo_ator.id_ator` |
| RN-BD-16 | A capacidade do teatro, quando informada, deve ser maior que zero. | Constraint `CHECK`. | `teatro.capacidade CHECK (capacidade > 0)` |
| RN-BD-17 | Um teatro com sessões associadas não pode ser excluído. | Chave estrangeira com `ON DELETE RESTRICT`. | `sessao.id_teatro -> teatro(id_teatro)` |
| RN-BD-18 | Toda sessão deve estar vinculada a um espetáculo e a um teatro. | Chaves estrangeiras obrigatórias (`NOT NULL`). | `sessao.id_espetaculo`, `sessao.id_teatro` |
| RN-BD-19 | A exclusão de um espetáculo deve remover suas sessões, avaliações, recomendações e associações de catálogo. | Chaves estrangeiras com `ON DELETE CASCADE`. | `sessao`, `avaliacao`, `recomendacao`, `espetaculo_categoria`, `espetaculo_ator` |
| RN-BD-20 | Preços de sessão não podem ser negativos e o preço máximo não pode ser menor que o preço mínimo. | Constraints `CHECK`. | `sessao.preco_minimo`, `sessao.preco_maximo`, `chk_preco_intervalo` |
| RN-BD-21 | A disponibilidade de uma sessão não pode ser negativa. | Constraint `CHECK` e valor padrão. | `sessao.disponibilidade DEFAULT 0 CHECK (disponibilidade >= 0)` |
| RN-BD-22 | Cada usuário pode avaliar um mesmo espetáculo apenas uma vez. | Constraint `UNIQUE`. | `avaliacao(id_usuario, id_espetaculo)` |
| RN-BD-23 | A nota de uma avaliação deve estar entre 0 e 5. | Constraint `CHECK`. | `avaliacao.nota CHECK (nota BETWEEN 0 AND 5)` |
| RN-BD-24 | Usuários só podem inserir ou excluir avaliações próprias. | Políticas de Row Level Security com `auth.uid() = id_usuario`. | Políticas `avaliacao: inserção pelo próprio usuário` e `avaliacao: exclusão pelo próprio usuário` |
| RN-BD-25 | Avaliações devem ser removidas quando o usuário ou o espetáculo associado forem excluídos. | Chaves estrangeiras com `ON DELETE CASCADE`. | `avaliacao.id_usuario`, `avaliacao.id_espetaculo` |
| RN-BD-26 | Usuários só podem consultar recomendações destinadas a eles. | Política de Row Level Security com `auth.uid() = id_usuario`. | Política `recomendacao: leitura pelo próprio usuário` |
| RN-BD-27 | O algoritmo de recomendação deve pertencer à lista de algoritmos aceitos. | Constraint `CHECK`. | `chk_tipo_algoritmo` |
| RN-BD-28 | O score de recomendação não pode ser negativo. | Constraint `CHECK`. | `recomendacao.score_recomendacao CHECK (score_recomendacao >= 0)` |
| RN-BD-29 | Recomendações devem ser removidas quando o usuário ou o espetáculo associado forem excluídos. | Chaves estrangeiras com `ON DELETE CASCADE`. | `recomendacao.id_usuario`, `recomendacao.id_espetaculo` |
| RN-BD-30 | O score de similaridade entre dois espetáculos deve estar normalizado no intervalo [0, 1]. | Constraint `CHECK`. | `similaridade_item.score_similaridade CHECK (score_similaridade BETWEEN 0 AND 1)` |
| RN-BD-31 | Um espetáculo não pode ser registrado como similar de si mesmo. | Constraint `CHECK`. | `chk_espetaculos_distintos CHECK (id_espetaculo_origem <> id_espetaculo_similar)` |
| RN-BD-32 | Não pode existir mais de um registro de similaridade para o mesmo par de espetáculos sob o mesmo algoritmo. | Constraint `UNIQUE`. | `uq_similaridade_item (id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo)` |
| RN-BD-33 | A similaridade item-item deve ser legível publicamente, pois é global (não vinculada a usuário). | Política de Row Level Security permissiva para leitura. | Política `similaridade_item: leitura pública` |
| RN-BD-34 | Os registros de similaridade devem ser removidos quando qualquer um dos espetáculos envolvidos for excluído. | Chaves estrangeiras com `ON DELETE CASCADE` em ambos os FKs. | `similaridade_item.id_espetaculo_origem`, `similaridade_item.id_espetaculo_similar` |
| RN-BD-30 | Dados de catálogo e agenda devem ser públicos para consulta. | Políticas de Row Level Security com `USING (true)`. | Políticas de `SELECT` em `categoria`, `ator`, `teatro`, `espetaculo`, `espetaculo_categoria`, `espetaculo_ator`, `sessao` e `avaliacao` |
