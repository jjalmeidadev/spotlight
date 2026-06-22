# Guia de Padrões de Desenvolvimento — Portal Aventura

## 1. Introdução

Este documento estabelece os padrões de codificação e padrões de projeto adotados no desenvolvimento do Portal Aventura. O objetivo é garantir qualidade, manutenibilidade e consistência do código em todas as camadas do produto:

- **Frontend** mobile em React Native + Expo (TypeScript), organizado por MVVM.
- **Backend** em .NET 9 com microsserviços (`catalogo`, `geolocalizacao`, `recomendacao`, `usuario`) e API Gateway, organizados por Clean Architecture.
- **Banco de dados** PostgreSQL hospedado no Supabase (com RLS), acessado por SQL puro via Npgsql — sem ORM.
- **Modelos de recomendação** em notebooks Jupyter (KNN, SVD) e modelo incrementado em ML.NET embarcado no microsserviço de recomendação.

O guia serve como referência rápida para novas implementações e como ponto de revisão em code review.

## 2. Padrões de Codificação

### 2.1 Convenções Gerais

#### 2.1.1 Estrutura de Diretórios

```text
g04/
├── App.js                                # Bootstrap do app Expo (delegado para src/frontend)
├── app.json, package.json                # Configuração do Expo / Node
├── docker-compose.yml                    # Orquestração local dos serviços backend (+ .dev/.prod)
├── Dockerfile.frontend                   # Imagem do frontend para build/serve web
├── deploy/nginx/                         # Proxy reverso (nginx) do deploy
├── documentos/                           # Documentação do projeto (markdowns + imagens)
│   ├── img/                              # Imagens, diagramas, mockups, wireframes
│   ├── evidencias/                       # Evidências de teste (TRX, saídas de execução)
│   └── *.md                              # Projeto, Manual de instalação, padrões, SOA, etc.
├── src/
│   ├── aad/                              # Notebooks de análise e tratamento de dados
│   ├── backend/                          # Backend SOA em ASP.NET Core (.NET 9)
│   │   ├── Spotlight.sln                 # Solution: gateway + 4 serviços + projetos de teste
│   │   ├── api-gateway/                  # Spotlight.Gateway (porta 5000) — YARP
│   │   ├── services/
│   │   │   ├── catalog/                 # Spotlight.Catalog         (5001)
│   │   │   ├── user/                    # Spotlight.User            (5002) + telemetria Mongo
│   │   │   ├── recommendation/          # Spotlight.Recommendation  (5003) + ML.NET strategy
│   │   │   └── geolocation/             # Spotlight.Geolocation     (5004) — Nominatim
│   │   ├── contracts/                   # Contratos de API/eventos e docs operacionais
│   │   ├── ml-engine/                   # Motor de ML; training/ contém o projeto de treino ML.NET
│   │   │   └── training/Spotlight.Recommendation.ModelTraining/  # Projeto .NET de treino
│   │   ├── scripts/                     # Scripts auxiliares (PowerShell)
│   │   └── tests/                       # Projetos xUnit por serviço (Spotlight.<Servico>.Tests) + non-functional/
│   ├── frontend/                        # App Expo / React Native em arquitetura MVVM
│   │   ├── components/                  # Componentes visuais reutilizáveis
│   │   ├── screens/                     # Telas (Views)
│   │   ├── viewmodels/                  # ViewModels (hooks de tela)
│   │   ├── repositories/                # Acesso aos serviços backend
│   │   ├── models/                      # Tipos/entidades de UI
│   │   ├── hooks/, theme/, config/      # Suporte (sessão, paleta, endpoints)
│   ├── database/                        # Scripts SQL (PostgreSQL/Supabase)
│   │   ├── 0X_*.sql                     # Bootstrap (criação, seed, rollback)
│   │   ├── migrations/                  # Migrations numeradas (04+) com rollback
│   │   └── load_test/{,pgbench/}        # Massa sintética e cenários de carga
│   ├── dataset/                         # Datasets processados (input do training)
│   └── models/                          # Notebooks + artefatos de ML publicados
│       ├── artifacts/                              # Modelo .zip e similaridades
│       └── reports/                                # Métricas e exemplos do modelo
```

#### 2.1.2 Nomenclatura de Arquivos

| Tipo | Convenção | Exemplo |
|------|-----------|---------|
| Notebooks Jupyter | `snake_case.ipynb` | `sistema_recomendacao_knn.ipynb` |
| Scripts SQL DDL | `NN_descricao.sql` (numeração sequencial) | `01_create_database.sql` |
| Migrations SQL | `NN_descricao.sql` + `NN_descricao_rollback.sql` | `08_tipo_algoritmo_e_unique_recomendacao.sql` |
| Datasets | `nome_descritivo.csv` | `recomendacao_balanceado.csv` |
| Arquivos C# (classes) | `PascalCase.cs` | `RecomendacaoService.cs` |
| Arquivos TS (componentes/screens) | `PascalCase.tsx` | `CatalogView.tsx` |
| Hooks/ViewModels TS | `useNomeViewModel.ts` | `useCatalogViewModel.ts` |
| Repositórios TS | `NomeRepository.ts` | `CatalogRepository.ts` |
| Documentação | `PascalCase.md` | `GuiaPadroesDesenvolvimento.md` |

### 2.2 Padrões Python (Notebooks de ML)

#### 2.2.1 Estrutura de Notebooks

```python
# Estrutura padrão de um notebook:
# 1. Configuração do ambiente
# 2. Carregamento de dados
# 3. Análise exploratória
# 4. Modelagem
# 5. Avaliação
# 6. Conclusão / exportação de artefatos
```

#### 2.2.2 Importações

```python
# Bibliotecas principais
import pandas as pd
import numpy as np

# Visualização
import matplotlib.pyplot as plt
import seaborn as sns

# Modelo e avaliação
from sklearn.neighbors import NearestNeighbors
from sklearn.model_selection import train_test_split
from scipy.sparse import csr_matrix
```

#### 2.2.3 Configuração de Caminhos

```python
from pathlib import Path

CWD = Path.cwd().resolve()
if CWD.parent.name == 'src':
    PROJECT_ROOT = CWD.parents[1]
elif (CWD / 'src').exists():
    PROJECT_ROOT = CWD
else:
    PROJECT_ROOT = (CWD / '../..').resolve()
```

#### 2.2.4 Convenções de Código

- **Variáveis**: `snake_case` (`matriz_item_usuario`).
- **Constantes**: `UPPER_SNAKE_CASE` (`N_VIZINHOS`, `SEED`).
- **Funções**: `snake_case` com verbo descritivo no nome.
- **Reprodutibilidade**: fixar seeds (`random_state=42`).
- **Docstrings** no padrão numpy/google com parâmetros, retornos e observações:

```python
def recomendar_espetaculos(item_id, n_recomendacoes=10, verbose=True):
    """Recomenda espetaculos similares a um espetaculo-alvo.

    Parametros
    ----------
    item_id : int
        ID numerico do espetaculo de referencia.
    n_recomendacoes : int
        Numero de recomendacoes a retornar.
    verbose : bool
        Se True, exibe cabecalho com espetaculo de referencia.

    Retorna
    -------
    pd.DataFrame
        Dataframe com recomendacoes ordenadas por similaridade.
    """
```

### 2.3 Padrões C# (.NET 9)

#### 2.3.1 Organização por Microsserviço

Cada microsserviço (`catalogo`, `geolocalizacao`, `recomendacao`, `usuario`) segue **Clean Architecture** com quatro camadas:

```text
services/<servico>/
├── domain/
│   ├── entities/        # Entidades de domínio e exceções de negócio
│   └── value-objects/
├── application/
│   ├── dtos/            # DTOs do contrato público (entrada/saída)
│   ├── services/        # Serviços de aplicação (orquestram strategies)
│   └── usecases/        # Casos de uso (1 por endpoint) + interfaces de repositório
├── infrastructure/
│   ├── auth/            # JWT, policies, contexto de usuário
│   ├── persistence/     # Implementações de repositório (Npgsql)
│   ├── model/           # Clientes para artefatos de ML (ML.NET, etc.)
│   └── storage/         # Sync com Supabase Storage, etc.
├── presentation/
│   ├── controllers/     # Controllers HTTP (ApiController)
│   └── middleware/      # ErrorHandling, request logging customizado
└── Program.cs           # Composition Root (DI, pipeline, Swagger, health)
```

### 2.3 Padrões SQL (PostgreSQL / Supabase)

#### 2.3.1 Estrutura de Scripts
Os scripts em `src/database/` usam PostgreSQL (Supabase) com `auth.users` como fonte da identidade. Cada entidade do domínio tem RLS habilitado e políticas explícitas.

```sql
-- 01_create_database.sql
-- Descrição: Criação da estrutura do banco de dados

CREATE TYPE tipo_usuario_enum AS ENUM (
    'visitante', 'cadastrado', 'administrador'
);

CREATE TABLE usuario (
    id_usuario      UUID              PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    nome            VARCHAR(150)      NOT NULL,
    email           VARCHAR(255)      NOT NULL UNIQUE,
    data_cadastro   TIMESTAMP         NOT NULL DEFAULT NOW(),
    tipo_usuario    tipo_usuario_enum NOT NULL DEFAULT 'cadastrado',

    CONSTRAINT chk_email_formato CHECK (email ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$')
);

ALTER TABLE usuario ENABLE ROW LEVEL SECURITY;

CREATE POLICY "usuario: leitura do próprio perfil"
    ON usuario FOR SELECT
    USING (auth.uid() = id_usuario);
```

#### 2.3.2 Convenções SQL
- **Palavras-chave**: MAIÚSCULAS (`SELECT`, `WHERE`, `JOIN`).
- **Nomes de tabelas**: `snake_case` no **singular** (`usuario`, `espetaculo`, `sessao`) — alinha com a nomenclatura de entidades C#.
- **Nomes de colunas**: `snake_case` (`nota_media`, `data_cadastro`).
- **Chaves primárias**: `id_<entidade>` (`id_usuario`, `id_espetaculo`) — torna o JOIN auto-documentado.
- **Chaves estrangeiras**: nome igual ao da PK referenciada (`id_usuario` em `avaliacao` referencia `usuario.id_usuario`).
- **Tipos**: `UUID` quando vinculado a `auth.users`; `SERIAL`/`INT` para entidades sem identidade externa.
- **Enums**: ENUMs nativos (`CREATE TYPE ..._enum`) em vez de `VARCHAR + CHECK`.
- **Migrations**: `src/database/migrations/NN_descricao.sql` numeradas; cada migration deve ter rollback equivalente em `src/database/03_rollback.sql` ou arquivo `NN_descricao_rollback.sql`.
- **RLS**: toda tabela com dado de usuário final tem `ENABLE ROW LEVEL SECURITY` + política nomeada (`"<tabela>: <ação descrita>"`).

#### 2.6.1 Estrutura de Markdown

```markdown
# Título Principal

## Seção
### Subseção

| Coluna | Descrição |
|--------|-----------|
| Valor  | Explicação |
```

#### 2.6.2 Tabelas de Métricas

```markdown
| Métrica   | Valor   |
|-----------|---------|
| RMSE      | 1.5977  |
| MAE       | 1.2296  |
| Cobertura | 69.8%   |
```

#### 2.6.3 Diagramas

Diagramas (arquitetura, sequência, ER) ficam em `documentos/img/` em PNG/SVG, com a fonte (Mermaid, draw.io) versionada junto quando possível.

### 2.7 Padrões de Mensagens de Commit (Conventional Commits)

#### 2.7.1 Estrutura

```text
<type>: <descrição> #<issue>
```

* **Exemplos válidos:**
  - `docs: atualiza guia de padrões #234`
  - `feat: cria endpoint de recomendação personalizada #34`
  - `fix(api): corrige validação de limite #105`

* **Regras:**
  - `type` ∈ {`feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, `revert`}.
  - Descrição não pode ser vazia.
  - Issue obrigatória, com pelo menos 2 dígitos (`#12`).

#### 2.7.2 Ativação do Hook Local

Para habilitar a validação automática no commit:

```bash
git config core.hooksPath .githooks
```

Para desabilitar:

```bash
git config --unset core.hooksPath
```

### 2.6 Padrões Frontend (React Native / TypeScript)

O app em `src/frontend/` segue arquitetura **MVVM**: a View (componente da tela) consome um ViewModel (hook) que orquestra estado e chamadas aos Repositories. Componentes visuais são puros e não tocam em rede.

#### 2.6.1 Camadas e nomenclatura
| Camada | Diretório | Convenção de arquivo | Exemplo |
|--------|-----------|----------------------|---------|
| View (tela) | `screens/` | `<Nome>View.tsx` ou `<Nome>Screen.tsx` | `CatalogView.tsx`, `LoadingScreen.tsx` |
| Componente visual | `components/` | `<Nome>.tsx` (PascalCase) | `CatalogRail.tsx`, `SearchBar.tsx` |
| ViewModel | `viewmodels/` | `use<Nome>ViewModel.ts` | `useCatalogViewModel.ts` |
| Repository | `repositories/` | `<Nome>Repository.ts` | `CatalogRepository.ts` |
| Model/Tipo | `models/` | `<Nome>.ts` (PascalCase) | `CatalogItem.ts`, `Usuario.ts` |
| Configuração | `config/`, `theme/`, `hooks/` | `camelCase.ts` | `api.ts`, `catalogImages.ts` |

#### 2.6.2 Convenções de Código
- **Linguagem**: TypeScript com tipagem explícita em props e retornos de função pública. `any` é proibido fora de adaptadores de terceiros.
- **Componentes**: `function ComponentName() {}` (sem `React.FC`). Props tipadas em `type <Componente>Props`.
- **Hooks (ViewModels)**: sempre prefixados com `use`. Devem retornar um objeto nomeado (não tupla) para que a View desestruture por nome.
- **Acesso a backend**: somente nos Repositories. Views e ViewModels nunca chamam `fetch`/axios diretamente.
- **Tipos de API**: sufixo `Api` para o contrato bruto vindo do backend (`EspetaculoResumoApi`), separado dos modelos de UI.
- **Identificadores**: `camelCase` para variáveis/funções, `PascalCase` para tipos/componentes, `UPPER_SNAKE_CASE` para constantes de módulo.
- **Estilos**: `StyleSheet.create` no fim do arquivo da View; tokens compartilhados em `theme/`.
- **Cleanup de efeitos**: useEffect com dado assíncrono deve usar flag local (`let ativo = true`) e cancelar setState após unmount.

### 2.7 Padrões Backend (C# / .NET 9 / Clean Architecture)

Cada microsserviço sob `src/backend/services/<servico>/` segue a mesma divisão em quatro camadas. Dependências apontam **sempre para dentro** (presentation → application → domain; infrastructure implementa contratos de domain/application).

#### 2.7.1 Camadas e nomenclatura
| Camada | Diretório | Responsabilidade | Sufixo |
|--------|-----------|------------------|--------|
| `domain/` | `entities/`, `value-objects/` | Entidades, exceções de domínio, contratos de repositório | `<Entidade>`, `I<Entidade>Repository`, `NotFoundException` |
| `application/` | `usecases/`, `dtos/` | Casos de uso e DTOs de entrada/saída | `<Acao><Entidade>UseCase`, `<Entidade>Dto` |
| `infrastructure/` | `persistence/`, `auth/`, ... | Implementação de repositórios (Npgsql), autenticação, integrações | `<Entidade>Repository`, `AuthSetup` |
| `presentation/` | `controllers/`, `middleware/` | Controllers HTTP, pipeline de exceções | `<Entidade>sController`, `ErrorHandlingMiddleware` |

#### 2.7.2 Convenções de Código
- **Namespaces**: refletem a camada — `Spotlight.<Servico>.<Camada>.<Subpasta>` (ex.: `Spotlight.Catalogo.Application.UseCases`).
- **Nomes**: `PascalCase` para classes/métodos/propriedades; `_camelCase` para campos privados; constantes em `PascalCase` (ex.: `PageSizeMaximo`).
- **Métodos assíncronos**: sufixo `Async` e parâmetro `CancellationToken` obrigatório em todo método público que faz I/O.
- **Use cases**: classe única por caso de uso, ponto de entrada `ExecutarAsync(...)`, validação/normalização em método `private static`.
- **Injeção de dependência**: por **construtor primário** (C# 12) — `public class X(IDep d)`. Sem property injection, sem service locator.
- **DTOs**: classes anêmicas com `{ get; set; }`; `record` somente quando o DTO precisa de igualdade estrutural (eventos de domínio).
- **Acesso a dados**: SQL puro via `NpgsqlDataSource` injetado (singleton). **Não usar EF Core** — o padrão do projeto é Npgsql/ADO.NET.
- **Exceções de domínio**: lançar `NotFoundException`/`ValidationException` próprias do serviço; o `ErrorHandlingMiddleware` traduz para `ErroDto` + status HTTP.
- **Comentários XML**: `///` em controllers, use cases e contratos públicos (alimenta Swagger).

#### 2.7.3 Convenções de API REST
- **Idioma**: rotas e DTOs em português (`/espetaculos`, `EspetaculoResumoDto`).
- **Rotas**: substantivos no plural em `kebab-case` (`/espetaculos/{id}/avaliacoes`). O prefixo do serviço (`/catalogo`, `/usuario`, etc.) é adicionado pelo gateway YARP e **não** aparece na rota interna do controller.
- **Verbos HTTP**: GET (consulta), POST (criação), PUT (atualização completa), DELETE (remoção). `204 No Content` para DELETE bem-sucedido.
- **Envelope de resposta**: listagens retornam `ListaDto<T>` (`{ data }`) ou `ListaPaginadaDto<T>` (`{ data, pagination }`). Erros retornam `ErroDto`.
- **Autorização**: `[Authorize(Policy = AuthSetup.Policy<Papel>)]` nas rotas protegidas; rotas públicas marcadas com `<summary>` explícito.
- **OpenAPI**: todo endpoint declara `[ProducesResponseType]` para 200/201 e cada status de erro relevante.

### 2.8 Padrões de Testes

- **Framework**: xUnit por serviço, em `src/backend/tests/Spotlight.<Servico>.Tests/`.
- **Nomenclatura de teste**: `<Sujeito>_<Cenario>_<ResultadoEsperado>` — exemplo: `ListarEspetaculos_NormalizaFiltrosEPaginacao_MapeiaResultado`.
- **Fakes**: implementações in-memory dos repositórios (`FakeEspetaculoRepository`), no mesmo arquivo do teste ou em `Fakes/` quando reutilizadas.
- **Categorias**: testes que dependem de banco/HTTP real recebem `[Trait("Category", "Integration")]` — o pipeline CI roda apenas `Category!=Integration` na etapa `test:unit`.
- **Cobertura**: cada use case deve ter ao menos um caminho feliz e um caminho de exceção (`NotFoundException`/`ValidationException`).

## 3. Padrões de Projeto

Os padrões abaixo são aplicados de forma consistente nas camadas correspondentes do projeto.

### 3.1 Clean Architecture (Backend C#)

Cada microsserviço segue Clean Architecture (ver 2.3.1). Os benefícios concretos observados:

- **Testabilidade**: casos de uso e strategies dependem de interfaces, então são testáveis com fakes em xUnit sem subir banco ou HTTP.
- **Substituibilidade**: a camada `infrastructure` pode ser trocada (ex.: Npgsql → outra biblioteca) sem tocar nos casos de uso.
- **Limites claros**: controllers ficam finos — só fazem binding de request e delegam ao caso de uso correspondente.

### 3.2 Strategy Pattern — Padrão Comportamental (Backend C#)

O **Strategy** é o **padrão comportamental** (classificação GoF — *Behavioral Patterns*) aplicado no backend C#. Padrões comportamentais tratam de **como objetos colaboram e distribuem responsabilidades em tempo de execução**; o Strategy, especificamente, **encapsula uma família de algoritmos intercambiáveis atrás de uma interface comum**, permitindo trocar o algoritmo usado sem alterar o código que o consome.

No Portal Aventura ele resolve a seleção do algoritmo de recomendação personalizada: a regra de negócio (`RecomendacaoService`) depende apenas da abstração `IRecomendacaoStrategy` e nunca sabe qual algoritmo concreto está rodando.

- **Implementação real:** [`RecomendacaoService.cs`](../src/backend/services/recommendation/application/services/RecomendacaoService.cs) — interface `IRecomendacaoStrategy` (linhas 45–55) e as estratégias concretas `ModeloMlNetRecomendacaoStrategy` (linhas 76–132) e `PostgresPreProcessadoRecomendacaoStrategy` (linhas 134–162).
- **Cobertura de testes:** `Spotlight.Recommendation.Tests/RecomendacaoServiceTests.cs` e `RecomendacaoUseCasesTests.cs`.

#### 3.2.1 Problema

O endpoint `GET /recomendacao/me` pode atender o usuário de duas formas:

1. Usando o modelo incrementado ML.NET, quando o artefato está presente e habilitado.
2. Usando as recomendações pré-processadas no PostgreSQL, como fallback.

A escolha e a execução do algoritmo não podem vazar para o controller nem para o caso de uso — esses só conhecem o contrato `IRecomendacaoStrategy`.

#### 3.2.2 Participantes (mapeamento GoF → projeto)

| Papel no Strategy (GoF) | Classe no projeto |
|-------------------------|-------------------|
| `Strategy` (interface) | `IRecomendacaoStrategy` |
| `ConcreteStrategy` A | `ModeloMlNetRecomendacaoStrategy` (algoritmo ML.NET) |
| `ConcreteStrategy` B | `PostgresPreProcessadoRecomendacaoStrategy` (fallback pré-processado) |
| `Context` | `RecomendacaoService` (consome a strategy via `IRecomendacaoStrategyFactory`) |

#### 3.2.3 Implementação

```csharp
// src/backend/services/recommendation/application/services/RecomendacaoService.cs

// Strategy: contrato comum que torna os algoritmos intercambiáveis.
public interface IRecomendacaoStrategy
{
    Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);
}

// ConcreteStrategy A: algoritmo ML.NET, com fallback controlado.
public sealed class ModeloMlNetRecomendacaoStrategy(
    IRecomendacaoRepository repositorio,
    IModeloRecomendacaoClient modeloClient,
    PostgresPreProcessadoRecomendacaoStrategy fallbackStrategy,
    ILogger<ModeloMlNetRecomendacaoStrategy> logger) : IRecomendacaoStrategy
{
    public async Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken)
    {
        try
        {
            var predicoes = await modeloClient.PredizerAsync(usuarioId, limite, cancellationToken);
            if (predicoes.Count == 0)
                return await fallbackStrategy.ListarAsync(usuarioId, limite, cancellationToken);

            // ... enriquece com dados do Postgres e retorna ...
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Falha ao executar strategy ML.NET; usando strategy PostgreSQL.");
            return await fallbackStrategy.ListarAsync(usuarioId, limite, cancellationToken);
        }
    }
}

// ConcreteStrategy B: recomendações pré-processadas no PostgreSQL.
public sealed class PostgresPreProcessadoRecomendacaoStrategy(
    IRecomendacaoRepository repositorio) : IRecomendacaoStrategy
{
    public async Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken)
    {
        var itens = await repositorio.ListarPorUsuarioAsync(usuarioId, limite, cancellationToken);
        return itens.Select(Mapear).ToList();
    }
}
```

O `Context` consome a strategy sem conhecer a implementação concreta — a seleção é delegada à factory (ver 3.3):

```csharp
public sealed class RecomendacaoService(
    IRecomendacaoStrategyFactory strategyFactory) : IRecomendacaoService
{
    public async Task<ListaDto<RecomendacaoDto>> ListarPersonalizadasAsync(
        Guid usuarioId, int? limite, CancellationToken cancellationToken)
    {
        var strategy = strategyFactory.Criar();              // qual algoritmo? o service não sabe
        var recomendacoes = await strategy.ListarAsync(
            usuarioId, NormalizarLimite(limite), cancellationToken);
        return new ListaDto<RecomendacaoDto> { Data = recomendacoes };
    }
}
```

#### 3.2.4 Decisão de design justificada

- **Por que comportamental (Strategy) e não condicional inline:** a alternativa ingênua seria um `if (modeloHabilitado) { ... } else { ... }` dentro do service, misturando duas lógicas de recomendação numa só classe. O Strategy isola cada algoritmo na sua própria classe, com responsabilidade única, e mantém o `RecomendacaoService` agnóstico ao algoritmo.
- **Open/Closed Principle:** novas estratégias (ex.: motor remoto via HTTP, modelo A/B) entram como novas implementações de `IRecomendacaoStrategy`, sem alterar `RecomendacaoService` nem o controller.
- **Fallback explícito e resiliente:** a strategy ML.NET delega de forma controlada para a strategy PostgreSQL quando o modelo não retorna predições ou lança exceção — a degradação é uma decisão de design, não um efeito colateral.
- **Testabilidade:** cada strategy depende apenas de interfaces (`IRecomendacaoRepository`, `IModeloRecomendacaoClient`) e é coberta isoladamente com fakes em xUnit, sem subir banco nem HTTP.
- **Complementaridade com o padrão criacional:** o Strategy responde *"como o algoritmo é executado"*; o **Factory Method** (3.3) responde *"qual algoritmo escolher"*. A separação evita que a regra de seleção contamine as estratégias.

### 3.3 Factory Method — Padrão Criacional (Backend C#)

O **Factory Method** é o **padrão criacional** (classificação GoF — *Creational Patterns*) aplicado no backend C#. Padrões criacionais tratam de **como objetos são instanciados, abstraindo o processo de criação** de quem os consome; o Factory Method, especificamente, **define uma operação responsável por criar um objeto, deixando que a regra de qual implementação concreta retornar fique encapsulada na fábrica**.

No Portal Aventura ele resolve a **seleção da strategy** de recomendação: o `RecomendacaoService` precisa de uma `IRecomendacaoStrategy`, mas a decisão de *qual* instanciar depende de configuração e da disponibilidade de artefatos — detalhes que não devem vazar para o service nem para o controller. Esse padrão é o complemento criacional do Strategy comportamental documentado em 3.2.

- **Implementação real:** [`RecomendacaoService.cs`](../src/backend/services/recommendation/application/services/RecomendacaoService.cs) — interface `IRecomendacaoStrategyFactory` (linhas 57–64) e a fábrica concreta `RecomendacaoStrategyFactory` (linhas 66–74).
- **Composição/registro no DI:** [`Program.cs`](../src/backend/services/recommendation/Program.cs) (linhas 49–53).

#### 3.3.1 Problema

O `RecomendacaoService` precisa de uma strategy, mas a escolha entre ML.NET e PostgreSQL depende de:

- flag `RecommendationModel:Enabled` em configuração;
- presença dos artefatos no disco (modelo ZIP, CSV de candidatos).

Não queremos que o service nem o controller conheçam esses detalhes — eles devem apenas *pedir* uma strategy pronta para uso.

#### 3.3.2 Participantes (mapeamento GoF → projeto)

| Papel no Factory Method (GoF) | Classe no projeto |
|-------------------------------|-------------------|
| `Creator` (interface da fábrica) | `IRecomendacaoStrategyFactory` |
| `ConcreteCreator` | `RecomendacaoStrategyFactory` |
| `Product` (interface do objeto criado) | `IRecomendacaoStrategy` |
| `ConcreteProduct` | `ModeloMlNetRecomendacaoStrategy` / `PostgresPreProcessadoRecomendacaoStrategy` |
| `Client` | `RecomendacaoService` |

#### 3.3.3 Implementação

```csharp
// src/backend/services/recommendation/application/services/RecomendacaoService.cs

// Creator: contrato da fábrica — esconde a regra de seleção do produto.
public interface IRecomendacaoStrategyFactory
{
    IRecomendacaoStrategy Criar();
}

// ConcreteCreator: encapsula a decisão de qual strategy entregar.
public sealed class RecomendacaoStrategyFactory(
    IModeloRecomendacaoClient modeloClient,
    ModeloMlNetRecomendacaoStrategy modeloStrategy,
    PostgresPreProcessadoRecomendacaoStrategy postgresStrategy)
    : IRecomendacaoStrategyFactory
{
    public IRecomendacaoStrategy Criar() =>
        modeloClient.Habilitado ? modeloStrategy : postgresStrategy;
}
```

O `RecomendacaoService` (Client) apenas pede uma strategy, sem saber qual será criada:

```csharp
var strategy = strategyFactory.Criar();
var recomendacoes = await strategy.ListarAsync(usuarioId, limiteNormalizado, cancellationToken);
```

A composição das dependências (instâncias concretas das strategies e da própria fábrica) é feita no `Program.cs`, mantendo a fábrica livre de `new` manual:

```csharp
// src/backend/services/recommendation/Program.cs
builder.Services.AddScoped<ModeloMlNetRecomendacaoStrategy>();
builder.Services.AddScoped<PostgresPreProcessadoRecomendacaoStrategy>();
builder.Services.AddScoped<IRecomendacaoStrategyFactory, RecomendacaoStrategyFactory>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
```

#### 3.3.4 Decisão de design justificada

- **Por que separar a criação (Factory) da execução (Strategy):** o Strategy responde *"como o algoritmo roda"*; o Factory Method responde *"qual algoritmo escolher"*. Misturar as duas responsabilidades faria cada strategy conhecer a flag de configuração e a existência dos artefatos — acoplamento que o padrão criacional remove ao concentrar a decisão num único ponto.
- **Single Responsibility:** a regra de seleção (`modeloClient.Habilitado ? ... : ...`) vive só na fábrica. Service e controller permanecem agnósticos.
- **Open/Closed Principle:** acrescentar uma nova fonte de recomendação significa registrar a nova `ConcreteProduct` e ajustar a fábrica, sem tocar no `RecomendacaoService` nem no endpoint.
- **Integração com o DI (composição):** a fábrica recebe as strategies já resolvidas pelo container (`AddScoped`), então não usa `new` nem service locator — a criação dos produtos respeita o ciclo de vida *scoped* (uma instância por request).
- **Testabilidade:** como a fábrica depende apenas de abstrações (`IModeloRecomendacaoClient`), é possível testar a regra de seleção alternando o flag `Habilitado` com um fake, sem subir modelo nem banco.

### 3.4 Repository Pattern

#### 3.4.1 No Backend

Cada microsserviço declara interfaces de repositório em `application/usecases/` e as implementa em `infrastructure/persistence/` com Npgsql. Isso isola SQL do domínio.

```csharp
// application/usecases/IRecomendacaoRepository.cs
public interface IRecomendacaoRepository
{
    Task<IReadOnlyList<Recomendacao>> ListarPorUsuarioAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, EspetaculoRecomendado>> ObterEspetaculosPorIdsAsync(
        IEnumerable<int> espetaculoIds, CancellationToken cancellationToken);
}
```

#### 3.4.2 No Frontend

O mesmo conceito vale no app: os repositórios em `src/frontend/repositories/` encapsulam o `fetch` para o gateway e a montagem de URLs do Supabase Storage. ViewModels nunca chamam `fetch` direto.

```ts
// repositories/CatalogRepository.ts
export async function listarEspetaculosCatalogo(): Promise<EspetaculoResumoApi[]> {
  const url = `${API_BASE_URL}/catalogo/espetaculos?pageSize=50&ordenarPor=popularidade`;
  const payload = await fetchJson<ListaPaginada<EspetaculoResumoApi>>(url);
  return payload.data ?? [];
}
```

### 3.5 Dependency Injection (Backend C#)

Toda composição é feita no `Program.cs` do microsserviço. Padrões adotados:

- `NpgsqlDataSource` como **singleton** (pool de conexões).
- Repositórios e use cases como **scoped** (1 por request).
- Strategies concretas registradas e a factory também — assim o DI resolve as dependências de cada strategy sem `new` manual.

```csharp
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<IRecomendacaoRepository, RecomendacaoRepository>();
builder.Services.AddScoped<ISimilaridadeRepository, SimilaridadeRepository>();
builder.Services.AddSingleton<IModeloRecomendacaoClient, MlNetModeloRecomendacaoClient>();
builder.Services.AddScoped<ModeloMlNetRecomendacaoStrategy>();
builder.Services.AddScoped<PostgresPreProcessadoRecomendacaoStrategy>();
builder.Services.AddScoped<IRecomendacaoStrategyFactory, RecomendacaoStrategyFactory>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
builder.Services.AddScoped<ListarRecomendacoesMeUseCase>();
```

### 3.6 Error Handling via Middleware (Backend C#)

Exceções de domínio (`NotFoundException`, `ValidationException`, `ConflictException`) são traduzidas para o envelope `{ error, traceId, details? }` por um middleware único registrado no início do pipeline:

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
```

Vantagens:

- Controllers permanecem livres de `try/catch`.
- O contrato de erro é único em todos os endpoints.
- Stack trace nunca vaza em produção; em dev, detalhes vão no campo `details`.

### 3.7 MVVM (Frontend)

Aplicado conforme a seção 2.4.1. Vale destacar os critérios para alocar lógica:

| Tipo de lógica | Onde colocar |
|----------------|--------------|
| Estado de UI (input, filtros, loading) | ViewModel (`useXxxViewModel`) |
| Cálculos derivados do estado | ViewModel, com `useMemo` |
| Acesso a API / Storage | Repository |
| Renderização e estilo | View (`.tsx` em `screens/`) |
| Tipos de domínio do app | Model (`models/`) |

### 3.8 Builder Pattern (Notebooks de ML)

Para a construção incremental dos modelos KNN/SVD nos notebooks, é recomendado encapsular as etapas (carregar dados → construir matriz → configurar → treinar → avaliar) em um Builder. Isso reduz duplicação ao comparar configurações (ex.: `k=15` vs `k=20`) e mantém o notebook legível.

```python
class ModeloRecomendacaoBuilder:
    """Builder para construção incremental do sistema de recomendação."""

    def __init__(self):
        self.dados = None
        self.matriz = None
        self.modelo = None

    def carregar_dados(self, path):
        self.dados = pd.read_csv(path)
        return self

    def construir_matriz(self):
        self.matriz = self.dados.pivot_table(
            index='item_id', columns='user_id', values='rating'
        )
        return self

    def configurar_modelo(self, n_vizinhos=20):
        self.modelo = NearestNeighbors(
            n_neighbors=n_vizinhos, metric='cosine', algorithm='brute'
        )
        return self

    def treinar(self):
        matriz_esparsa = csr_matrix(self.matriz.fillna(0))
        self.modelo.fit(matriz_esparsa)
        return self

    def build(self):
        return {'modelo': self.modelo, 'matriz': self.matriz}


sistema = (
    ModeloRecomendacaoBuilder()
    .carregar_dados('../dataset/recomendacao_balanceado.csv')
    .construir_matriz()
    .configurar_modelo(n_vizinhos=20)
    .treinar()
    .build()
)
```

### 3.9 Comparação dos Padrões Criacionais Considerados

| Padrão | Onde se aplica | Por que foi escolhido |
|--------|----------------|------------------------|
| **Builder** | Notebooks de ML | Construção complexa passo a passo de um único tipo de objeto (modelo de recomendação) |
| **Factory Method** | Backend C# (recomendação) | Seleção dinâmica entre strategies conforme configuração e disponibilidade de artefatos |
| **Strategy** | Backend C# (recomendação) | Algoritmos intercambiáveis (ML.NET ↔ PostgreSQL pré-processado) com mesma interface |
| **Repository** | Backend C# e frontend TS | Isolar acesso a dados/API do resto da aplicação |
| **MVVM** | Frontend RN | Separar View, estado/regra de apresentação (ViewModel) e dados (Repository) |

Padrões considerados e **não** adotados:

- **Singleton** para o modelo de recomendação: não, pois precisamos avaliar múltiplas configurações em paralelo nos experimentos.
- **Abstract Factory**: não há famílias de objetos relacionados; o problema atual é uma seleção 1-de-N.
- **Prototype**: modelos são construídos do zero a partir dos artefatos, não clonados.

## 4. Boas Práticas Implementadas

### 4.1 Gestão de Dados

- Datasets de apoio e amostras versionados em `src/dataset/`, consumidos pelos notebooks de `src/aad/` e pelo treino do modelo.
- Seeds fixas (`random_state=42`) para todos os experimentos.
- Migrations versionadas em `src/database/migrations/` com rollback espelhado.
- Tabelas com dados de usuário **sempre** com RLS habilitado.

### 4.2 Backend

- **Sem ORM**: SQL puro via Npgsql; `NpgsqlDataSource` como singleton.
- **Use case por endpoint**: facilita teste e mantém controllers magros.
- **Strategies + Factory** para escolha de algoritmo de recomendação.
- **Middleware** único traduz exceções de domínio para o envelope de erro padronizado.
- **JWT do Supabase** validado por chave simétrica e/ou JWKS; policies (`Cadastrado`, `Administrador`) em `infrastructure/auth/AuthSetup.cs`.
- **Health checks**: `/health` (liveness) e `/health/ready` (readiness com check no Postgres).

### 4.3 Frontend

- **MVVM** estrito: View → ViewModel → Repository.
- **Sem fetch direto na View**.
- **Estado de erro e loading explícitos** no ViewModel.
- **Cleanup de efeitos assíncronos** com flag `ativo` para evitar `setState` após unmount.
- **Tipos de API separados dos modelos de domínio** (sufixo `Api`).

### 4.4 ML / Notebooks

- Estrutura de seções numeradas (Configuração, Carregamento, EDA, Modelagem, Avaliação, Conclusão).
- Métricas múltiplas: RMSE, MAE, Cobertura, Hit Rate@K.
- Comparação sistemática de variantes em tabela.
- Artefatos finais exportados para `src/models/artifacts/` para consumo pelo backend.
- Uso de `csr_matrix` e `n_jobs=-1` para escalabilidade.

### 4.5 Testes

- Projetos xUnit em `src/backend/tests/Spotlight.<Servico>.Tests/`.
- Testes de caso de uso usam **fakes** das interfaces (`IRecomendacaoRepository`, `IModeloRecomendacaoClient`) — não mocks de banco.
- Testes de integração (quando aplicável) usam `WebApplicationFactory<Program>` com o `Program` parcial exposto em cada microsserviço.

## 5. Checklist de Verificação

### Antes do Commit
- [ ] Código segue convenções de nomenclatura da camada/linguagem
- [ ] Mensagem de commit segue Conventional Commits + `#<issue>`
- [ ] Notebooks: seções numeradas, paths com `pathlib`, seeds fixadas
- [ ] Backend C#: método público assíncrono recebe `CancellationToken` e tem sufixo `Async`
- [ ] Backend C#: novos use cases/repositórios registrados no `Program.cs`
- [ ] Frontend TS: acesso a backend permanece no Repository (View/ViewModel não chamam `fetch`)
- [ ] SQL: PostgreSQL puro, tabelas no singular, PK `id_<entidade>`, RLS habilitado se houver dado de usuário

### Review de Código
- [ ] Builder Pattern para construções complexas (modelo de ML)
- [ ] Strategy Pattern para algoritmos intercambiáveis (KNN/ML.NET/Postgres fallback)
- [ ] Camadas da Clean Architecture preservadas (dependências apontam para dentro)
- [ ] MVVM no frontend: View renderiza, ViewModel orquestra, Repository acessa rede
- [ ] DTOs separados das entidades de domínio e dos tipos `Api` do frontend
- [ ] Tratamento de erro via `NotFoundException`/`ValidationException` + `ErrorHandlingMiddleware`
- [ ] Cobertura mínima de testes xUnit no caminho feliz e no caminho de exceção

## 6. Revisão de Padrões — Lacunas e Recomendações

Esta seção registra a auditoria dos padrões em uso no repositório, comparando o que estava documentado no guia com o que foi observado no código (backend C#, frontend RN/TS, SQL e testes). As lacunas marcadas como **aplicado** já estão incorporadas nas seções acima; as **pendentes** ficam como recomendações para próximas iterações.

### 7.1 Resumo das Lacunas

| # | Lacuna identificada | Severidade | Recomendação | Status |
|---|---------------------|------------|--------------|--------|
| 1 | Seção SQL exemplificava sintaxe MySQL (`AUTO_INCREMENT`, `USE`, `IF NOT EXISTS` no `CREATE DATABASE`) e tabelas no plural, divergindo do banco real (PostgreSQL/Supabase, singular, UUID, RLS). | Alta | Reescrever 2.3 com PostgreSQL e refletir a convenção `id_<entidade>` usada em `src/database/`. | Aplicado (2.3) |
| 2 | Padrões do frontend (React Native + TypeScript) não estavam documentados, embora o Index já citasse cobertura de TS. | Alta | Adicionar seção 2.6 com camadas MVVM, nomenclatura de arquivos e regras de acesso a rede. | Aplicado (2.6) |
| 3 | Backend C# tinha apenas os padrões de projeto descritos (3.3); faltavam as convenções gerais (camadas Clean Architecture, sufixos `UseCase`/`Dto`/`Repository`, construtor primário, `Async` + `CancellationToken`). | Alta | Adicionar seção 2.7 cobrindo camadas, código e API REST. | Aplicado (2.7) |
| 4 | Testes xUnit por serviço não tinham convenção registrada (nome do teste, `Trait("Category", "Integration")` usado no CI). | Média | Adicionar seção 2.8 com nomenclatura e categorias. | Aplicado (2.8) |
| 5 | Diagrama de diretórios não incluía `src/backend/ml-engine/`, que já existe e é consumido pelos serviços. | Baixa | Atualizar 2.1.1. | Aplicado (2.1.1) |
| 6 | Não há configuração compartilhada de linter/formatter no repositório (`.editorconfig`/ESLint/Prettier). Atualmente o estilo é mantido manualmente. | Média | Adicionar `.editorconfig` na raiz (indentação, finais de linha) e ESLint + Prettier para `src/frontend/`. Para o backend, adotar `dotnet format` como passo opcional no pipeline. | Pendente |
| 7 | Não há análise estática de C# (`dotnet format --verify-no-changes`, analyzers) no pipeline `build`/`test:unit`. | Média | Acrescentar `dotnet format --verify-no-changes` ao job `build` do `.gitlab-ci.yml`. | Pendente |
| 8 | Não há padrão documentado para tratamento de configuração sensível (apenas `.env.example` na raiz). Variáveis novas tendem a se espalhar pelo `appsettings.json` e pelo `Program.cs`. | Baixa | Padronizar leitura via `Environment.GetEnvironmentVariable(...)` + fallback em `appsettings.json`, conforme já praticado no `catalogo/Program.cs`. | Pendente (documentar como ADR) |
| 9 | Mensagens de log seguem formato livre — Serilog já está configurado, mas não há convenção sobre quais propriedades enriquecer (`UserId`, `RequestId`, `Servico`). | Baixa | Definir conjunto mínimo de propriedades em um documento operacional separado. | Pendente |

### 7.2 Pontos Aderentes (sem ajuste necessário)

- **Estrutura de diretórios** dos quatro microsserviços é homogênea (`application/`, `domain/`, `infrastructure/`, `presentation/`).
- **Convenção de commit** (`<type>: <descrição> #<issue>`) está implementada via Git Hook e respeitada no histórico recente.
- **Padrões de projeto** Builder/Strategy/Factory estão aplicados no código real (notebooks de ML e `Spotlight.Recomendacao`) e cobertos por testes.
- **Separação de DTOs** entre `application/dtos` (backend) e `models/`+sufixo `Api` (frontend) está consistente.
- **RLS no banco** está habilitado nas tabelas de usuário, em linha com a recomendação do Supabase.

### 7.3 Próximos Passos Recomendados

1. Adicionar `.editorconfig` na raiz e ESLint/Prettier ao frontend (item 6).
2. Plugar `dotnet format --verify-no-changes` no job `build` do GitLab CI (item 7).
3. Registrar como ADR a estratégia de configuração por `.env` + `appsettings.json` (item 8).
4. Definir o vocabulário mínimo de log estruturado no documento operacional (item 9).

## 7. Conclusão

Os padrões deste guia refletem o estado atual do Portal Aventura:

- **Clean Architecture + Strategy + Factory + Repository** no backend C# garantem que o algoritmo de recomendação possa evoluir sem mexer em controllers nem em casos de uso.
- **MVVM + Repository** no frontend mantêm Views simples e testáveis, e centralizam o contato com o gateway.
- **SQL puro com Npgsql** sobre PostgreSQL/Supabase preserva controle fino sobre o schema e RLS, sem o acoplamento de um ORM.
- **Builder** continua sendo a forma recomendada de organizar a construção de modelos nos notebooks de ML.

A aderência consistente a esses padrões reduz fricção em code review, facilita onboarding e mantém o código previsível em todas as camadas do produto.
