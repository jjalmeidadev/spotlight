# Banco de Dados

Este diretório reúne os scripts SQL responsáveis pela criação, carga inicial e reversão da estrutura relacional do sistema de teatro e espetáculos. A base foi modelada para sustentar o catálogo de espetáculos, sessões, teatros, usuários, avaliações e recomendações personalizadas.

## Contextualização dos Arquivos

| Arquivo | Finalidade |
|---|---|
| `01_create_database.sql` | Script principal de criação da base relacional. Remove objetos anteriores quando existirem, cria os tipos enumerados, tabelas, constraints, políticas de RLS, trigger de criação de usuário e índices de apoio às consultas. |
| `02_initial_data.sql` | Script reservado para inserção de dados iniciais relevantes para iniciar a solução, como categorias, atores, teatros, espetáculos, sessões e registros mínimos necessários para validações e demonstrações. No momento, o arquivo ainda está vazio. |
| `03_rollback.sql` | Script de reversão da estrutura criada, removendo trigger, função, tabelas e tipos enumerados em ordem compatível com as dependências do banco. Deve ser usado em cenários controlados de limpeza ou recriação do ambiente. |

## Dicionário de Dados

### 1. Visão Geral

O banco de dados modela o domínio de um sistema de consulta, avaliação e recomendação de espetáculos teatrais. Ele contempla o cadastro de usuários, espetáculos, categorias, atores, teatros, sessões, avaliações e recomendações personalizadas.

| Item | Quantidade |
|---|---:|
| Tabelas | 10 |
| Tipos enumerados | 2 |
| Funções | 1 |
| Triggers | 1 |
| Índices explícitos | 10 |

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
| `localizacao` | `VARCHAR(150)` | Sim | - | - | - | Localização informada pelo usuário. |
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
| `nota_media` | `NUMERIC(3,2)` | Não | `0` | - | `CHECK (nota_media BETWEEN 0 AND 10)` | Nota média consolidada das avaliações. |
| `id_admin` | `UUID` | Sim | - | FK | Referencia `usuario(id_usuario) ON DELETE SET NULL` | Usuário administrador responsável pelo cadastro ou gestão. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora de criação do registro. |
| `updated_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora da última atualização. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `espetaculo_pkey` | Primary Key | `id_espetaculo` |
| Check implícito | Check | `duracao > 0` |
| Check implícito | Check | `nota_media BETWEEN 0 AND 10` |
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
| `nota` | `NUMERIC(4,2)` | Não | - | - | `CHECK (nota BETWEEN 0 AND 10)` | Nota atribuída pelo usuário. |
| `comentario` | `TEXT` | Sim | - | - | - | Comentário textual opcional. |
| `created_at` | `TIMESTAMP` | Não | `NOW()` | - | - | Data e hora da avaliação. |

#### Constraints

| Nome | Tipo | Definição |
|---|---|---|
| `avaliacao_pkey` | Primary Key | `id_avaliacao` |
| Unique implícita | Unique | `(id_usuario, id_espetaculo)` |
| Check implícito | Check | `nota BETWEEN 0 AND 10` |
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

### 12. Relacionamentos Gerais do Modelo

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

### 13. Índices Criados

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

