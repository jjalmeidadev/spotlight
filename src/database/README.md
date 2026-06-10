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
- [9. Planos de Execução — Queries Críticas](#planos-de-execução--queries-críticas)
- [10. Timezone e Tipos Temporais](#timezone-e-tipos-temporais)
- [11. Estratégia de Migrations Incrementais](#estratégia-de-migrations-incrementais)
- [12. Rastreabilidade de Transações](#rastreabilidade-de-transações)
- [13. Massa de Dados para Teste de Carga](#massa-de-dados-para-teste-de-carga)
- [14. Teste de Carga (pgbench)](#teste-de-carga-pgbench)

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
├── 02_initial_data.sql      — seed com dados iniciais (categorias, teatros, atores, espetáculos e sessões)
├── 03_rollback.sql          — desfaz completamente a execução do 01_create_database.sql
├── migrations/              — alterações incrementais ao schema após o setup inicial
│   ├── 04_trigger_updated_at_espetaculo.sql           — trigger BEFORE UPDATE em espetaculo.updated_at
│   ├── 04_trigger_updated_at_espetaculo_rollback.sql
│   ├── 05_timestamptz.sql                             — TIMESTAMP → TIMESTAMPTZ em todas as colunas temporais
│   ├── 05_timestamptz_rollback.sql
│   ├── 06_indexes.sql                                 — pg_trgm + 4 índices para queries reais do catálogo
│   ├── 06_indexes_rollback.sql
│   ├── 07_constraints_sessao_usuario.sql              — UNIQUE em sessao e CHECK em usuario.data_nascimento
│   ├── 07_constraints_sessao_usuario_rollback.sql
│   ├── 08_tipo_algoritmo_e_unique_recomendacao.sql    — função is_tipo_algoritmo_valido + UNIQUE em recomendacao
│   ├── 08_tipo_algoritmo_e_unique_recomendacao_rollback.sql
│   ├── 09_rastreabilidade_transacoes.sql              — tabela auditoria + trigger genérica de auditoria nas tabelas transacionais
│   └── 09_rastreabilidade_transacoes_rollback.sql
├── load_test/               — massa sintética e teste de carga (pgbench)
│   ├── massa_carga.sql                                — gera ~208 mil linhas de dados de carga
│   ├── massa_carga_rollback.sql                       — remove apenas os dados de carga
│   ├── run_pgbench.sh                                 — runner dos cenários de carga
│   └── pgbench/
│       ├── catalogo_leitura.sql                       — cenário de leitura (catálogo/busca/recomendações)
│       └── avaliacao_escrita.sql                      — cenário de escrita (UPSERT + auditoria)
└── README.md                — este arquivo
```

> Cada migration tem um arquivo `_rollback.sql` correspondente. As migrations são aplicadas **manualmente** via SQL Editor do Supabase ou `psql` após o setup inicial (`01_create_database.sql` + `02_initial_data.sql`) e respeitam a ordem numérica. Veja a seção [Estratégia de Migrations Incrementais](#estratégia-de-migrations-incrementais) para detalhes.

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
| `DB_HOST` | `psql`, API Gateway e ferramentas de administração do banco |
| `DB_PORT` | `psql`, API Gateway e ferramentas de administração do banco |
| `DB_NAME` | Nome do banco usado nas conexões locais |
| `DB_USER` | Usuário usado nas conexões locais |
| `DB_PASSWORD` | Senha usada por `psql` e serviços que conectam no PostgreSQL |
| `POSTGRES_PASSWORD` | Container PostgreSQL (Docker) |
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
01_create_database.sql  →  02_initial_data.sql
```

O `02_initial_data.sql` depende das tabelas criadas pelo `01`. Nunca inverta a ordem.

Na organização das tasks, essa ordem corresponde a:

```
T1 — criação do schema  →  T2 — carga inicial de dados
```

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
- o script `01_create_database.sql` tiver criado as 10 tabelas do schema `public`;
- o script `02_initial_data.sql` tiver populado os dados iniciais sem erros de integridade referencial;
- as consultas de validação retornarem registros em `usuario`, `espetaculo` e `sessao`.

Com esses passos concluídos, o banco estará apto para uso pelo frontend, API Gateway, serviços SOA e rotinas de recomendação em ambiente de desenvolvimento, homologação ou implantação no servidor Inteli.

---

## Dicionário de Dados

### 1. Visão Geral

O banco de dados modela o domínio de um sistema de consulta, avaliação e recomendação de espetáculos teatrais. Ele contempla o cadastro de usuários, espetáculos, categorias, atores, teatros, sessões, avaliações e recomendações personalizadas.

| Item | Quantidade |
|---|---:|
| Tabelas | 12 |
| Tipos enumerados | 2 |
| Funções | 4 |
| Triggers | 10 |
| Índices explícitos | 20 |

> A contagem reflete o estado após a aplicação das migrations incrementais em `src/database/migrations/` (até a `09`). As funções somam `public.handle_new_user()` (01), `public.set_updated_at()` (04), `public.is_tipo_algoritmo_valido()` (08) e `public.registrar_auditoria()` (09). Os triggers somam `on_auth_user_created` (01), `trg_espetaculo_updated_at` (04) e os 8 triggers de auditoria `trg_auditoria_*` (09), um por tabela transacional.

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
| 11 | `similaridade_item` | Similaridade item-item entre espetáculos (global, leitura pública), populada em batch pelos algoritmos de recomendação. |
| 12 | `auditoria` | Trilha de rastreabilidade: registra toda escrita (`INSERT`/`UPDATE`/`DELETE`) nas tabelas transacionais, agrupável por transação. Adicionada na migration `09`. |

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
| `localizacao` | `JSONB` | Sim | - | - | - | Localização consentida do usuário, gerada pelo `NominatimGeocodingService` via `PUT /usuario/me/localizacao`. Estrutura serializada em camelCase (`JsonSerializerDefaults.Web`) — ver detalhes abaixo. |
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

##### Estrutura do JSONB `localizacao`

O campo é populado pelo `UsuarioLocalizacaoService` (via `PUT /usuario/me/localizacao`), que serializa o DTO `AtualizarLocalizacaoRequest` com `JsonSerializerDefaults.Web` — chaves em camelCase. Em produção, os valores vêm do `NominatimGeocodingService` (proxy do OpenStreetMap/Nominatim). No seed de bootstrap (`02_initial_data.sql`), o campo `provider` recebe `"seed"` para distinguir dados de carga inicial dos dados geocodificados.

Exemplo de objeto válido:

```json
{
  "formattedAddress": "Av. Paulista, 1578 - Bela Vista, São Paulo - SP",
  "latitude": -23.55052,
  "longitude": -46.633308,
  "placeId": "123456789",
  "provider": "openstreetmap_nominatim"
}
```

Validações aplicadas pelo `UsuarioLocalizacaoService` antes da gravação:

| Chave | Tipo | Obrigatória | Regra |
|---|---|---|---|
| `formattedAddress` | `string` | sim | não-vazia |
| `latitude` | `number` | sim | `-90 ≤ lat ≤ 90` |
| `longitude` | `number` | sim | `-180 ≤ lon ≤ 180` |
| `placeId` | `string` | sim | — (sem validação extra) |
| `provider` | `string` | sim | em produção: `"openstreetmap_nominatim"`; em seed: `"seed"` |

> **Contrato de escrita:** o campo `localizacao` **não é aceito** no body de `PUT /usuario/me`. Qualquer payload com essa chave é silenciosamente ignorado pelo deserializador (a propriedade não existe em `UsuarioUpdateDto`). Para atualizar a localização use exclusivamente `PUT /usuario/me/localizacao`, que valida o payload completo antes de gravar.

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
| `uq_recomendacao_usuario_espetaculo_algo` | `id_usuario, id_espetaculo, tipo_algoritmo` | UNIQUE — garante uma única recomendação ativa por par usuário/espetáculo/algoritmo e habilita o `ON CONFLICT ... DO UPDATE` (UPSERT) do Worker. Criado na migration `08_tipo_algoritmo_e_unique_recomendacao.sql`. |

#### Triggers e Functions

A constraint `chk_tipo_algoritmo` delega a validação à função reutilizável `public.is_tipo_algoritmo_valido(TEXT)` (definida em `08_tipo_algoritmo_e_unique_recomendacao.sql`). Adicionar um novo algoritmo passa a exigir apenas `CREATE OR REPLACE FUNCTION`, sem necessidade de `ALTER TYPE`.

### 12. Tabela `similaridade_item`

#### Descrição

Armazena pares de espetáculos similares (cálculo item-item) e o score associado por algoritmo de recomendação. Diferente de `recomendacao`, esta tabela é **global** (não associada a um usuário específico) e tem leitura pública. É populada em batch pelos Workers de ML que processam os eventos de telemetria do MongoDB.

#### Tipo de Tabela

Entidade transacional / saída global de algoritmo de recomendação item-item.

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `similaridade_item: leitura pública` | `SELECT` | Permite leitura pública: `USING (true)`. |

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_similaridade` | `SERIAL` | Não | Sequência automática | PK | - | Identificador único da linha de similaridade. |
| `id_espetaculo_origem` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo de referência. |
| `id_espetaculo_similar` | `INTEGER` | Não | - | FK | Referencia `espetaculo(id_espetaculo) ON DELETE CASCADE` | Espetáculo similar ao de origem. |
| `tipo_algoritmo` | `VARCHAR(50)` | Não | - | - | `chk_tipo_algoritmo_similaridade` | Algoritmo que produziu a similaridade. |
| `score_similaridade` | `NUMERIC(8,6)` | Não | - | - | `CHECK (score_similaridade BETWEEN 0 AND 1)` | Score normalizado em [0, 1]. |
| `data_geracao` | `TIMESTAMPTZ` | Não | `NOW()` | - | - | Data e hora de geração do par de similaridade. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `similaridade_item_pkey` | Primary Key | `id_similaridade` |
| Check implícito | Check | `score_similaridade BETWEEN 0 AND 1` |
| `chk_tipo_algoritmo_similaridade` | Check | `public.is_tipo_algoritmo_valido(tipo_algoritmo)` (após migration `08_...`; nome anterior era `chk_tipo_algoritmo_similar` com lista literal) |
| `chk_espetaculos_distintos` | Check | `id_espetaculo_origem <> id_espetaculo_similar` |
| `uq_similaridade_item` | Unique | `(id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo)` |
| FK implícita | Foreign Key | `id_espetaculo_origem -> espetaculo(id_espetaculo) ON DELETE CASCADE` |
| FK implícita | Foreign Key | `id_espetaculo_similar -> espetaculo(id_espetaculo) ON DELETE CASCADE` |

#### Relacionamentos

| Tipo | Tabela / Coluna | Cardinalidade |
|---|---|---|
| FK | `espetaculo(id_espetaculo)` <- `similaridade_item(id_espetaculo_origem)` | Um espetáculo pode ter vários similares. |
| FK | `espetaculo(id_espetaculo)` <- `similaridade_item(id_espetaculo_similar)` | Um espetáculo pode aparecer como similar de vários outros. |

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_similaridade_item_origem` | `id_espetaculo_origem, score_similaridade DESC` | Otimiza listagem dos espetáculos mais similares a um dado espetáculo. |

#### Triggers e Functions

A constraint `chk_tipo_algoritmo_similaridade` delega a validação à função `public.is_tipo_algoritmo_valido(TEXT)` — mesma função usada por `recomendacao`. Centralizar a lista de algoritmos aceitos em um único lugar evita divergência entre as duas tabelas.

### 13. Tabela `auditoria`

#### Descrição

Trilha de auditoria (audit log) que materializa a **rastreabilidade de transações** do banco. Cada operação de escrita (`INSERT`, `UPDATE`, `DELETE`) sobre as tabelas transacionais gera uma linha registrando o estado anterior e o novo (em JSONB), o ator responsável e o identificador da transação. Adicionada na migration [`09_rastreabilidade_transacoes.sql`](migrations/09_rastreabilidade_transacoes.sql).

A coluna `id_transacao` recebe `txid_current()`, então **todas as operações de um mesmo `COMMIT` compartilham o mesmo valor** — é o que permite reconstruir uma transação inteira a partir da trilha.

#### Tipo de Tabela

Tabela de auditoria / rastreabilidade (append-only por design; escrita exclusivamente pela trigger).

#### Políticas de Row Level Security

| Política | Operação | Regra |
|---|---|---|
| `auditoria: leitura por administradores` | `SELECT` | Restringe a leitura a administradores: `USING (EXISTS (SELECT 1 FROM usuario u WHERE u.id_usuario = auth.uid() AND u.tipo_usuario = 'administrador'))`. |

> Não há política de `INSERT`: as inserções vêm da função de trigger `public.registrar_auditoria()`, que é `SECURITY DEFINER` e ignora RLS. Sem política de `SELECT` para usuários comuns, a trilha fica visível apenas a administradores (e ao service role, que ignora RLS).

#### Colunas

| Coluna | Tipo | Nulo? | Default | PK / FK | Restrições | Descrição |
|---|---|---|---|---|---|---|
| `id_auditoria` | `BIGSERIAL` | Não | Sequência automática | PK | - | Identificador único e sequencial da entrada de auditoria. |
| `id_transacao` | `BIGINT` | Não | - | - | - | `txid_current()` — agrupa operações do mesmo `COMMIT`. |
| `tabela` | `VARCHAR(63)` | Não | - | - | - | Nome da tabela auditada (`TG_TABLE_NAME`). |
| `operacao` | `VARCHAR(10)` | Não | - | - | `CHECK (operacao IN ('INSERT','UPDATE','DELETE'))` | Tipo de operação registrada. |
| `registro_id` | `TEXT` | Sim | - | - | - | Valor da PK do registro afetado (extraído via argumento da trigger). |
| `dados_anteriores` | `JSONB` | Sim | - | - | - | Estado anterior do registro (`NULL` em `INSERT`). |
| `dados_novos` | `JSONB` | Sim | - | - | - | Estado novo do registro (`NULL` em `DELETE`). |
| `id_usuario_responsavel` | `UUID` | Sim | - | - | Sem FK (ver nota) | Ator autenticado (`auth.uid()`); `NULL` para escritas de sistema/seed/worker. |
| `papel_banco` | `TEXT` | Não | `current_user` | - | - | Role do banco que executou a operação. |
| `registrado_em` | `TIMESTAMPTZ` | Não | `NOW()` | - | - | Momento do registro da operação. |

> **Sem FK em `id_usuario_responsavel`:** o ator pode ser um processo de sistema sem linha em `usuario`, e a trilha deve sobreviver à exclusão do usuário — por isso a coluna é um `UUID` solto, não uma chave estrangeira.

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `auditoria_pkey` | Primary Key | `id_auditoria` |
| Check implícito | Check | `operacao IN ('INSERT','UPDATE','DELETE')` |

#### Relacionamentos

A tabela é **desacoplada por design** — não declara chaves estrangeiras. O vínculo com os registros auditados é lógico (`tabela` + `registro_id`) e com o ator é informativo (`id_usuario_responsavel`), garantindo que a trilha permaneça íntegra mesmo após exclusões em cascata nas tabelas de origem.

#### Índices

| Índice | Colunas | Objetivo |
|---|---|---|
| `idx_auditoria_transacao` | `id_transacao` | Reconstruir todas as operações de uma mesma transação. |
| `idx_auditoria_tabela_registro` | `tabela, registro_id` | Histórico de um registro específico. |
| `idx_auditoria_registrado_em` | `registrado_em DESC` | Varredura cronológica (operações mais recentes). |
| `idx_auditoria_usuario` | `id_usuario_responsavel` | Operações atribuídas a um ator. |

#### Triggers e Functions

Alimentada pela função de trigger genérica `public.registrar_auditoria()` (`SECURITY DEFINER`), instalada como 8 triggers `AFTER INSERT OR UPDATE OR DELETE` — uma por tabela transacional: `usuario`, `espetaculo`, `teatro`, `ator`, `sessao`, `avaliacao`, `recomendacao` e `similaridade_item`. A função recebe o nome da coluna de PK da tabela auditada como argumento (`TG_ARGV[0]`) para preencher `registro_id`. Detalhes operacionais na seção [Rastreabilidade de Transações](#rastreabilidade-de-transações).

### 14. Relacionamentos Gerais do Modelo

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

### 15. Índices Criados

A tabela abaixo consolida todos os índices explícitos do schema. Os marcados com `*` foram adicionados pelas migrations incrementais (`src/database/migrations/`).

| Índice | Tabela | Colunas | Origem | Finalidade |
|---|---|---|---|---|
| `idx_espetaculo_status` | `espetaculo` | `status` | `01_create_database.sql` | Otimiza filtros por status. |
| `idx_espetaculo_nota_media` | `espetaculo` | `nota_media DESC` | `01_create_database.sql` | Otimiza rankings por nota média. |
| `idx_espetaculo_popularidade` | `espetaculo` | `popularidade DESC` | `01_create_database.sql` | Otimiza rankings por popularidade. |
| `idx_sessao_espetaculo` | `sessao` | `id_espetaculo` | `01_create_database.sql` | Otimiza busca de sessões por espetáculo. |
| `idx_sessao_data` | `sessao` | `data` | `01_create_database.sql` | Otimiza busca de sessões por data. |
| `idx_sessao_teatro` | `sessao` | `id_teatro` | `01_create_database.sql` | Otimiza busca de sessões por teatro. |
| `idx_recomendacao_usuario` | `recomendacao` | `id_usuario` | `01_create_database.sql` | Otimiza busca de recomendações por usuário. |
| `idx_recomendacao_score` | `recomendacao` | `id_usuario, score_recomendacao DESC` | `01_create_database.sql` | Otimiza ranking de recomendações por usuário. |
| `idx_similaridade_item_origem` | `similaridade_item` | `id_espetaculo_origem, score_similaridade DESC` | `01_create_database.sql` | Otimiza listagem dos similares de um espetáculo. |
| `idx_avaliacao_espetaculo` | `avaliacao` | `id_espetaculo` | `01_create_database.sql` | Otimiza busca de avaliações por espetáculo. |
| `idx_espetaculo_ator_ator` | `espetaculo_ator` | `id_ator` | `01_create_database.sql` | Otimiza busca de espetáculos por ator. |
| `idx_espetaculo_titulo_trgm` * | `espetaculo` | `titulo` (GIN trigram) | `06_indexes.sql` | Acelera buscas `ILIKE '%termo%'` (requer extensão `pg_trgm`). |
| `idx_espetaculo_genero` * | `espetaculo` | `genero` | `06_indexes.sql` | Otimiza filtro `WHERE genero = @genero`. |
| `idx_espetaculo_status_popularidade` * | `espetaculo` | `status, popularidade DESC` | `06_indexes.sql` | Composto — cobre filtro de status combinado com ordenação por popularidade sem sort extra. |
| `idx_sessao_espetaculo_data_horario` * | `sessao` | `id_espetaculo, data, horario` | `06_indexes.sql` | Composto — cobre sessões de um espetáculo já ordenadas por data/horário. |
| `uq_recomendacao_usuario_espetaculo_algo` * | `recomendacao` | `id_usuario, id_espetaculo, tipo_algoritmo` | `08_tipo_algoritmo_e_unique_recomendacao.sql` | UNIQUE — viabiliza `ON CONFLICT ... DO UPDATE` (UPSERT) do Worker. |
| `idx_auditoria_transacao` * | `auditoria` | `id_transacao` | `09_rastreabilidade_transacoes.sql` | Reconstrói todas as operações de uma mesma transação. |
| `idx_auditoria_tabela_registro` * | `auditoria` | `tabela, registro_id` | `09_rastreabilidade_transacoes.sql` | Histórico de um registro específico. |
| `idx_auditoria_registrado_em` * | `auditoria` | `registrado_em DESC` | `09_rastreabilidade_transacoes.sql` | Varredura cronológica da trilha. |
| `idx_auditoria_usuario` * | `auditoria` | `id_usuario_responsavel` | `09_rastreabilidade_transacoes.sql` | Operações atribuídas a um ator. |

Além desses 20 índices nomeados, o PostgreSQL cria automaticamente índices para:
- Toda chave primária declarada.
- Toda constraint `UNIQUE`, incluindo a `uq_sessao_espetaculo_teatro_data_horario` (`id_espetaculo, id_teatro, data, horario`) adicionada em `07_constraints_sessao_usuario.sql` e a `uq_similaridade_item` (`id_espetaculo_origem, id_espetaculo_similar, tipo_algoritmo`) presente desde o schema inicial.

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
| RN-BD-30 | Dados de catálogo e agenda devem ser públicos para consulta. | Políticas de Row Level Security com `USING (true)`. | Políticas de `SELECT` em `categoria`, `ator`, `teatro`, `espetaculo`, `espetaculo_categoria`, `espetaculo_ator`, `sessao` e `avaliacao` |
| RN-BD-31 | Toda atualização em `espetaculo` deve registrar automaticamente a hora da modificação em `updated_at`, independente da aplicação setar o campo. | Função genérica reutilizável `public.set_updated_at()` + trigger `BEFORE UPDATE`. A função pode ser ligada a outras tabelas no futuro. | `04_trigger_updated_at_espetaculo.sql` — `public.set_updated_at()`, `trg_espetaculo_updated_at` |
| RN-BD-32 | Não pode haver mais de uma sessão para o mesmo espetáculo, no mesmo teatro, na mesma data e horário. | Constraint `UNIQUE` composta. | `07_constraints_sessao_usuario.sql` — `uq_sessao_espetaculo_teatro_data_horario` |
| RN-BD-33 | A data de nascimento, quando informada, deve estar entre 1900-01-01 e a data atual; `NULL` continua aceito para perfis sem cadastro de data. | Constraint `CHECK` (semântica three-valued logic do SQL ignora `NULL`). | `07_constraints_sessao_usuario.sql` — `ck_usuario_data_nascimento` |
| RN-BD-34 | Cada par `(usuario, espetaculo, tipo_algoritmo)` tem no máximo uma recomendação ativa. O Worker grava com `ON CONFLICT ... DO UPDATE` (UPSERT) e o campo `prioridade_manual` **não** é sobrescrito pelo UPSERT automático — priorização editorial é via rota administrativa. | `UNIQUE INDEX`. | `08_tipo_algoritmo_e_unique_recomendacao.sql` — `uq_recomendacao_usuario_espetaculo_algo` |
| RN-BD-35 | A lista de algoritmos de recomendação aceitos é definida em um único ponto reutilizado pelas duas tabelas (`recomendacao` e `similaridade_item`). Adicionar um algoritmo novo é uma única alteração, sem `ALTER TYPE` que bloquearia a tabela em produção. | Função SQL `IMMUTABLE` chamada pelos CHECKs `chk_tipo_algoritmo` (em `recomendacao`) e `chk_tipo_algoritmo_similaridade` (em `similaridade_item`). | `08_tipo_algoritmo_e_unique_recomendacao.sql` — `public.is_tipo_algoritmo_valido(TEXT)` |
| RN-BD-36 | Toda escrita (`INSERT`/`UPDATE`/`DELETE`) nas tabelas transacionais deve ser registrada em uma trilha de auditoria com estado anterior e novo. | Função de trigger genérica `SECURITY DEFINER` + 8 triggers `AFTER INSERT/UPDATE/DELETE`, gravando em `auditoria`. | `09_rastreabilidade_transacoes.sql` — `public.registrar_auditoria()`, `trg_auditoria_*` |
| RN-BD-37 | As operações de uma mesma transação devem ser correlacionáveis na trilha. | Coluna `auditoria.id_transacao` preenchida com `txid_current()` — compartilhada por todas as operações do mesmo `COMMIT`. | `09_rastreabilidade_transacoes.sql` — `auditoria.id_transacao` |
| RN-BD-38 | A trilha de auditoria só pode ser lida por administradores. | Política de Row Level Security restrita a `tipo_usuario = 'administrador'`; inserções via trigger `SECURITY DEFINER`. | `09_rastreabilidade_transacoes.sql` — política `auditoria: leitura por administradores` |

> **Sobre a decisão `VARCHAR + CHECK + função` no lugar de `ENUM` para `tipo_algoritmo`:** ENUMs nativos do PostgreSQL exigem `ALTER TYPE ... ADD VALUE` para incluir novos valores, operação que adquire lock na tabela. O time optou por `VARCHAR(50)` + constraint `CHECK` que chama a função `public.is_tipo_algoritmo_valido()`. Adicionar um algoritmo passa a ser um `CREATE OR REPLACE FUNCTION`, sem reescrita de constraint em duas tabelas e sem impacto operacional.

## Planos de Execução — Queries Críticas

Esta seção documenta o comportamento esperado dos quatro índices criados em `src/database/migrations/06_indexes.sql` para as queries reais executadas por `EspetaculoRepository.cs` no serviço de catálogo. Cada entrada lista a query SQL equivalente, o índice associado, o plano de execução esperado em produção e uma observação sobre o comportamento em ambiente de desenvolvimento com seed pequeno.

### 1. Busca por título

- **Query:** `SELECT * FROM espetaculo WHERE titulo ILIKE '%musical%'`
- **Índice:** `idx_espetaculo_titulo_trgm` (GIN trigram, requer extensão `pg_trgm`)
- **Plano esperado em produção:** `Bitmap Index Scan on idx_espetaculo_titulo_trgm`
- **Observação:** com poucos registros (seed) o PostgreSQL escolhe `Seq Scan` por ser mais eficiente — comportamento correto, não indica problema no índice.

### 2. Filtro por gênero

- **Query:** `SELECT * FROM espetaculo WHERE genero = 'Drama'`
- **Índice:** `idx_espetaculo_genero` (B-tree)
- **Plano esperado em produção:** `Index Scan using idx_espetaculo_genero`
- **Observação:** mesma situação do título — `Seq Scan` é esperado com volume baixo.

### 3. Filtro de status + ordenação por popularidade

- **Query:** `SELECT * FROM espetaculo WHERE status = 'em_cartaz' ORDER BY popularidade DESC LIMIT 20`
- **Índice:** `idx_espetaculo_status_popularidade` (B-tree composto)
- **Plano esperado em produção:** `Index Scan using idx_espetaculo_status_popularidade` — o índice composto evita um sort separado porque já entrega as linhas na ordem correta.
- **Observação:** este é o índice de maior impacto — é a query padrão da tela de catálogo.

### 4. Sessões de um espetáculo ordenadas

- **Query:** `SELECT * FROM sessao WHERE id_espetaculo = 1 ORDER BY data, horario`
- **Índice:** `idx_sessao_espetaculo_data_horario` (B-tree composto)
- **Plano esperado em produção:** `Index Scan using idx_sessao_espetaculo_data_horario` — o índice composto cobre o filtro e a ordenação sem sort adicional.
- **Observação:** elimina o custo de sort em espetáculos com muitas sessões.

> Os planos acima foram validados com `EXPLAIN` no ambiente Supabase com dados de seed. Com volume reduzido o PostgreSQL opta por `Seq Scan`, que é o comportamento correto — o otimizador escolhe automaticamente o índice quando o volume justificar.

## Timezone e Tipos Temporais

Todas as colunas temporais do schema são `TIMESTAMPTZ` (timestamp com fuso horário) a partir da migration [`05_timestamptz.sql`](migrations/05_timestamptz.sql). A versão original do schema (`01_create_database.sql`) declarou essas colunas como `TIMESTAMP` (sem fuso); a migration converteu todas elas interpretando o valor existente como UTC.

### Premissa da migração

Os valores gravados antes da migration foram **assumidos como UTC** — premissa válida porque:

- O container Docker do banco (`postgres:15-alpine` em `docker-compose.yml`) não configura a variável `TZ`, então o servidor opera em UTC por padrão.
- O Supabase cloud também opera em UTC sem configuração explícita.

A conversão usa `ALTER COLUMN ... TYPE TIMESTAMPTZ USING <coluna> AT TIME ZONE 'UTC'` para cada coluna afetada. O conjunto convertido cobre as 9 colunas temporais existentes (em 8 tabelas), listadas no comentário-cabeçalho da migration.

### Defaults

Nenhum `DEFAULT` precisou mudar: `NOW()` no PostgreSQL já é timezone-aware e retorna `TIMESTAMPTZ`. As triggers e funções definidas em SQL (`public.handle_new_user()`, `public.set_updated_at()`) também continuam funcionando sem alteração.

### Rollback

O arquivo [`05_timestamptz_rollback.sql`](migrations/05_timestamptz_rollback.sql) reverte cada coluna para `TIMESTAMP` (sem fuso). A reversão é destrutiva no sentido **semântico**: a informação de fuso é perdida e qualquer cliente que ler as colunas precisa assumir UTC explicitamente — caso contrário interpretará o valor como horário local e produzirá leituras incorretas em fusos diferentes de UTC.

## Estratégia de Migrations Incrementais

A pasta [`src/database/migrations/`](migrations/) contém alterações incrementais ao schema aplicadas **após** o setup inicial (`01_create_database.sql` + `02_initial_data.sql`). Esta seção descreve o contrato operacional dessas migrations.

### Princípios

1. **`01_create_database.sql` é imutável para alterações incrementais.** Qualquer mudança de schema posterior ao bootstrap deve ser feita via migration nova em `migrations/`. O arquivo original só é editado em cenários de reset completo (ambiente de desenvolvimento).
2. **Cada migration tem um rollback par.** O arquivo de reversão tem o mesmo prefixo numérico + sufixo `_rollback.sql` e desfaz idempotentemente o que a migration aplicou. O rollback deve incluir avisos quando a reversão for destrutiva (perda de informação, dependências externas, mudança de comportamento da aplicação).
3. **Ordem importa.** A numeração dupla (`04_`, `05_`, `06_`, `07_`, `08_`, `09_`) define a ordem obrigatória de aplicação. Migrations seguintes podem depender de objetos criados pelas anteriores (a migration `08`, por exemplo, depende de `recomendacao` e `similaridade_item` existirem desde `01`; a `09` depende de todas as tabelas transacionais para registrar a auditoria).
4. **Aplicação manual.** As migrations **não** são executadas automaticamente pelo Docker — apenas `01` e `02` rodam na inicialização do volume. Migrations em `migrations/` precisam ser aplicadas explicitamente via SQL Editor do Supabase ou via `psql`.

### Como aplicar uma migration

**Via SQL Editor do Supabase:**

1. Abra o Supabase Dashboard → SQL Editor → New Query.
2. Cole o conteúdo do arquivo `migrations/NN_*.sql` (onde `NN` é o próximo número não aplicado).
3. Execute. Cada migration está envolvida em `BEGIN/COMMIT`, então uma falha aborta tudo sem deixar estado parcial.
4. Confira a validação documentada no comentário-cabeçalho da migration (em geral uma query `information_schema` ou `pg_indexes`).

**Via `psql`:**

```bash
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/migrations/NN_*.sql
```

### Como reverter

Aplique o arquivo `_rollback.sql` correspondente:

```bash
psql -h <host> -p <porta> -U postgres -d postgres \
  -f src/database/migrations/NN_*_rollback.sql
```

> Antes de rodar um rollback, leia o aviso operacional no cabeçalho do arquivo — algumas reversões implicam mudança de contrato para a aplicação (ex: a aplicação volta a precisar setar `updated_at` manualmente após o rollback de `04_`).

### Convenções de nomenclatura

| Padrão | Exemplo |
|---|---|
| `NN_<descricao_curta>.sql` | `06_indexes.sql` |
| `NN_<descricao_curta>_rollback.sql` | `06_indexes_rollback.sql` |
| Funções genéricas reutilizáveis em `public.<nome>` | `public.set_updated_at`, `public.is_tipo_algoritmo_valido` |
| Triggers prefixadas por `trg_<tabela>_<evento>` | `trg_espetaculo_updated_at` |
| Constraints `UNIQUE` prefixadas por `uq_` | `uq_recomendacao_usuario_espetaculo_algo` |
| Constraints `CHECK` prefixadas por `ck_` ou `chk_` (mantém estilo da tabela alvo) | `ck_usuario_data_nascimento`, `chk_tipo_algoritmo` |

---

## Rastreabilidade de Transações

A migration [`09_rastreabilidade_transacoes.sql`](migrations/09_rastreabilidade_transacoes.sql) adiciona uma **trilha de auditoria** que registra toda operação de escrita (`INSERT`/`UPDATE`/`DELETE`) sobre as tabelas transacionais, permitindo responder *quem alterou o quê, quando e em qual transação*.

### Como funciona

- **Tabela `auditoria`** — uma linha por operação, contendo o estado anterior (`dados_anteriores`) e o novo (`dados_novos`) em JSONB, a tabela e a operação, o `registro_id` afetado e o ator responsável.
- **Função `public.registrar_auditoria()`** — trigger genérica `AFTER INSERT/UPDATE/DELETE` (`SECURITY DEFINER`). Recebe o nome da coluna de PK como argumento (`TG_ARGV[0]`) para preencher `registro_id` de forma genérica.
- **Triggers** instaladas em: `usuario`, `espetaculo`, `teatro`, `ator`, `sessao`, `avaliacao`, `recomendacao` e `similaridade_item` (8 no total).

### Correlação por transação

A coluna `id_transacao` recebe `txid_current()`, então **todas as operações de um mesmo `COMMIT` compartilham o mesmo identificador** — é o que torna possível reconstruir uma transação inteira a partir da trilha.

### Colunas da tabela `auditoria`

| Coluna | Tipo | Descrição |
|---|---|---|
| `id_auditoria` | `BIGSERIAL` PK | Identificador sequencial da entrada. |
| `id_transacao` | `BIGINT` | `txid_current()` — agrupa operações do mesmo COMMIT. |
| `tabela` | `VARCHAR(63)` | Tabela auditada (`TG_TABLE_NAME`). |
| `operacao` | `VARCHAR(10)` | `INSERT`, `UPDATE` ou `DELETE`. |
| `registro_id` | `TEXT` | Valor da PK do registro afetado. |
| `dados_anteriores` | `JSONB` | Estado anterior (`NULL` em `INSERT`). |
| `dados_novos` | `JSONB` | Estado novo (`NULL` em `DELETE`). |
| `id_usuario_responsavel` | `UUID` | `auth.uid()` — `NULL` para escritas de sistema/seed/worker. |
| `papel_banco` | `TEXT` | `current_user` — role do banco que executou. |
| `registrado_em` | `TIMESTAMPTZ` | Momento do registro. |

> **Segurança:** a tabela tem RLS habilitada e é legível apenas por usuários `administrador`; as inserções vêm da trigger `SECURITY DEFINER`, que ignora RLS. Não há FK em `id_usuario_responsavel` para que a trilha sobreviva à exclusão do usuário.

### Consultas típicas

```sql
-- Histórico completo de um registro:
SELECT operacao, dados_anteriores, dados_novos, id_usuario_responsavel, registrado_em
FROM auditoria
WHERE tabela = 'espetaculo' AND registro_id = '1'
ORDER BY registrado_em;

-- Reconstruir uma transação inteira:
SELECT tabela, operacao, registro_id
FROM auditoria
WHERE id_transacao = :txid
ORDER BY id_auditoria;
```

> **Interação com carga sintética:** as triggers disparam também durante a carga de `load_test/` (gerando ~1 linha de auditoria por linha inserida). Para medir o banco sem esse overhead, aplique a `09` **depois** do bulk load ou desabilite temporariamente as triggers durante a carga.

---

## Massa de Dados para Teste de Carga

Os scripts em [`src/database/load_test/`](load_test/) geram uma massa sintética e representativa sobre o schema já criado, tornando relevantes os testes de carga e performance (k6, `EXPLAIN ANALYZE` em volume real, etc.). Eles **não** fazem parte do setup inicial e devem ser aplicados manualmente, após `01`, `02` e as migrations `04`–`08`.

### Volume gerado

| Entidade | Linhas | Marcador de isolamento |
|---|---:|---|
| `usuario` (via `auth.users` + trigger) | 5.000 | `email LIKE '%@loadtest.local'` / UUID prefixo `10000000-…` |
| `ator` | 1.000 | `nome LIKE 'Ator Carga %'` |
| `teatro` | 100 | `parceiro_naming_rights = 'Carga LoadTest'` |
| `espetaculo` | 2.000 | `produtora = 'Carga LoadTest'` |
| `sessao` | ~20.000 | referencia espetáculos/teatros de carga |
| `avaliacao` | ~100.000 | referencia usuários/espetáculos de carga |
| `recomendacao` | ~50.000 | referencia usuários/espetáculos de carga |
| `similaridade_item` | ~30.000 | referencia espetáculos de carga |
| **Total** | **~208 mil** | |

> Os marcadores garantem que a massa seja isolável: as tabelas-fato só referenciam entidades de carga, então o rollback as remove em cascata sem tocar no seed curado de `02_initial_data.sql`.

### Características

- **Idempotente por guarda.** Se a massa já estiver carregada, `massa_carga.sql` aborta com mensagem orientando a rodar o rollback antes — não duplica dados.
- **Volume parametrizável.** As constantes no bloco `DECLARE` no topo de `massa_carga.sql` controlam o volume de cada entidade; ajuste-as para escalar a carga.
- **Pares únicos.** Avaliações, recomendações e similaridades são geradas com mapeamento determinístico que respeita as constraints `UNIQUE` (com `ON CONFLICT DO NOTHING` como rede de segurança).
- **Estatísticas atualizadas.** Ambos os scripts rodam `ANALYZE` ao final, de modo que os planos de execução observados nos testes reflitam o volume real.
- **Senha única reaproveitada.** O hash bcrypt é calculado uma vez e reutilizado em todos os usuários de carga, evitando o custo de hashing por linha.

### Como aplicar

```bash
# Carregar a massa
psql "$DATABASE_URL" -f src/database/load_test/massa_carga.sql

# Remover apenas a massa (preserva o seed)
psql "$DATABASE_URL" -f src/database/load_test/massa_carga_rollback.sql
```

Também é executável via SQL Editor do Supabase / DBeaver (não usa metacomandos `psql`). Validado em `postgres:16-alpine` com o schema + migrations `04`–`09`: carga completa em ~24 s, sem erros, e rollback retornando exatamente aos volumes do seed.

---

## Teste de Carga (pgbench)

Com a massa carregada, o comportamento do banco sob alto volume de operações foi avaliado com **`pgbench`** (ferramenta nativa de benchmark do PostgreSQL). Os scripts de cenário ficam versionados em [`src/database/load_test/pgbench/`](load_test/pgbench/) e o runner em [`run_pgbench.sh`](load_test/run_pgbench.sh).

### Método

- **Ferramenta:** `pgbench` 16 (cliente nativo).
- **Cenários** (cada transação simula uma interação real da aplicação):
  - `catalogo_leitura.sql` — **leitura**: catálogo em cartaz por popularidade, filtro por gênero, busca por título (trigram), sessões ordenadas e recomendações por usuário (as queries críticas da seção [Planos de Execução](#planos-de-execução--queries-críticas)).
  - `avaliacao_escrita.sql` — **escrita**: UPSERT de avaliação, que dispara a trigger de auditoria (migration `09`) — mede também o overhead da rastreabilidade.
- **Configuração:** 20 clientes concorrentes, 4 threads, 30 s por cenário.
- **Massa:** ~208 mil linhas (ver seção anterior).

### Resultados

Ambiente de referência: `postgres:16-alpine` em Docker local (4 threads). Os números são **indicativos** e servem de baseline comparável — em produção (Supabase gerenciado) variam conforme tier e rede.

| Cenário | Transações | TPS | Latência média | Falhas |
|---|---:|---:|---:|---:|
| Catálogo (leitura, 5 queries/tx) | 80.367 | **2.681** | 7,46 ms | 0 (0,000%) |
| Avaliação (escrita + auditoria) | 159.059 | **5.306** | 3,77 ms | 0 (0,000%) |

Latências por statement (cenário de leitura): cada uma das 5 queries ficou entre **1,3 e 1,6 ms**. No cenário de escrita, o UPSERT **com a trigger de auditoria disparando** custou **3,73 ms** por operação.

### Evidências de uso de índice sob volume

Sob ~208 mil linhas o otimizador passa a usar os índices criados na migration `06` (com o seed pequeno ele opta por `Seq Scan`, como observado na seção de Planos de Execução). `EXPLAIN ANALYZE` durante a carga:

```text
-- Catálogo em cartaz por popularidade:
Limit (actual time=0.051..0.101 rows=20)
  ->  Index Scan using idx_espetaculo_status_popularidade on espetaculo
        Index Cond: (status = 'em_cartaz')
Execution Time: 0.249 ms          -- índice composto cobre filtro + ordenação, sem sort

-- Recomendações por usuário:
->  Bitmap Index Scan on idx_recomendacao_usuario
      Index Cond: (id_usuario = '...')
```

### Análise

- **Estabilidade:** zero transações falhas em ~240 mil execuções somadas; sem erros de constraint, deadlock ou conexão.
- **Leitura:** ~2,7 mil tx/s mesmo com 5 queries por transação, cada query em ~1,5 ms — os índices da migration `06` entregam o catálogo sem `Seq Scan` nem sort adicional sob volume.
- **Escrita + rastreabilidade:** ~5,3 mil tx/s com a trilha de auditoria ativa. Cada escrita gerou a linha correspondente em `auditoria` (259.059 linhas em `avaliacao` ao final: 100 mil da massa + 159 mil da carga), confirmando que a rastreabilidade da migration `09` opera corretamente sob carga e com overhead baixo.
- **Conclusão:** o banco sustenta o volume-alvo do contexto da aplicação com folga e latência baixa; os índices e a trilha de auditoria se comportam como projetado.

### Como reproduzir

```bash
# 1) Banco no ar com schema + migrations 04–09 e massa carregada (seções acima).
# 2) Rodar os cenários:
DATABASE_URL="postgres://postgres:senha@host:porta/spotlight" \
CLIENTS=20 THREADS=4 DURATION=30 \
bash src/database/load_test/run_pgbench.sh
```
