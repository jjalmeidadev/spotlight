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
├── docker-compose.yml                    # Orquestração local dos serviços backend
├── documentos/                           # Documentação do projeto (markdowns + imagens)
│   ├── img/                              # Imagens, diagramas, mockups, wireframes
│   └── *.md                              # Projeto, Manual de instalação, padrões, SOA, etc.
├── src/
│   ├── aad/                              # Notebooks de análise e tratamento de dados
│   ├── backend/                          # Backend SOA em ASP.NET Core (.NET 9)
│   │   ├── Spotlight.sln                 # Solution: gateway + 4 serviços + projetos de teste
│   │   ├── api-gateway/                  # Spotlight.Gateway (porta 5000) — YARP
│   │   ├── services/
│   │   │   ├── catalogo/                 # Spotlight.Catalogo        (5001)
│   │   │   ├── usuario/                  # Spotlight.Usuario         (5002) + telemetria Mongo
│   │   │   ├── recomendacao/             # Spotlight.Recomendacao    (5003) + ML.NET strategy
│   │   │   └── geolocalizacao/           # Spotlight.Geolocalizacao  (5004) — Nominatim
│   │   ├── contracts/                    # Contratos de API/eventos e docs operacionais
│   │   ├── ml-engine/                    # Bindings / wrappers para o motor de ML
│   │   ├── scripts/                      # Scripts auxiliares (PowerShell)
│   │   └── tests/                        # Projetos xUnit por serviço
│   ├── frontend/                         # App Expo / React Native em arquitetura MVVM
│   │   ├── components/                   # Componentes visuais reutilizáveis
│   │   ├── screens/                      # Telas (Views)
│   │   ├── viewmodels/                   # ViewModels (hooks de tela)
│   │   ├── repositories/                 # Acesso aos serviços backend
│   │   ├── models/                       # Tipos/entidades de UI
│   │   ├── hooks/, theme/, config/       # Suporte (sessão, paleta, endpoints)
│   ├── database/                         # Scripts SQL (PostgreSQL/Supabase)
│   │   ├── 0X_*.sql                      # Bootstrap (criação, seed, rollback)
│   │   ├── migrations/                   # Migrations numeradas (04+) com rollback
│   │   └── load_test/{,pgbench/}         # Massa sintética e cenários de carga
│   ├── dataset/                          # Datasets processados (input do training)
│   └── models/                           # Notebooks + treino ML.NET + artefatos publicados
│       ├── Spotlight.Recomendacao.ModelTraining/   # Projeto .NET de treino
│       ├── artifacts/                              # Modelo .zip e similaridades
│       └── reports/                                # Métricas e exemplos do modelo
└── colab_csv_utf8/                       # Datasets originais (CSV bruto, read-only)
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

Regras de dependência (clean architecture):

- `presentation` depende de `application` e `infrastructure`.
- `application` depende apenas de `domain`.
- `infrastructure` implementa interfaces declaradas em `application/usecases` e `application/services`.
- `domain` não depende de nada.

#### 2.3.2 Convenções de Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Namespace | `Spotlight.<Servico>.<Camada>` | `Spotlight.Recomendacao.Application.Services` |
| Interface | Prefixo `I` | `IRecomendacaoRepository`, `IRecomendacaoStrategy` |
| Caso de uso | `<Verbo><Sujeito>UseCase` | `ListarRecomendacoesMeUseCase` |
| DTO | sufixo `Dto` | `RecomendacaoDto`, `ListaDto<T>` |
| Strategy | sufixo `Strategy` | `ModeloMlNetRecomendacaoStrategy` |
| Factory | sufixo `Factory` | `RecomendacaoStrategyFactory` |
| Repositório | sufixo `Repository` | `RecomendacaoRepository` |

#### 2.3.3 Estilo de Código

- **C# 12 / nullable enabled** em todos os projetos.
- **Primary constructors** para injeção (preferir a construtores explícitos):

  ```csharp
  public sealed class RecomendacaoService(IRecomendacaoStrategyFactory factory) : IRecomendacaoService
  {
      // ...
  }
  ```

- **Records** para DTOs e VOs imutáveis (`public sealed record ModeloRecomendacaoPredicao(int ItemId, decimal Score, ...)`).
- **`sealed`** por padrão em classes concretas que não foram desenhadas para herança.
- **Async/await** em toda IO; sempre propagar `CancellationToken`.
- **Comentários XML** apenas em tipos/membros públicos não-óbvios. Evitar comentários redundantes que repetem o nome do método.
- **Logging** via `ILogger<T>` (Serilog). Mensagens em português, parâmetros estruturados:

  ```csharp
  logger.LogWarning(ex, "Falha ao executar strategy ML.NET; usando strategy PostgreSQL.");
  ```

#### 2.3.4 Acesso a Dados (sem ORM)

O acesso ao PostgreSQL é feito **exclusivamente com Npgsql** + SQL puro. Nada de Entity Framework Core.

- Sempre usar `NpgsqlDataSource` registrado como singleton no `Program.cs`.
- Parâmetros nomeados (`@usuario`, `@limite`) — nunca interpolar valores na string SQL.
- Mapear manualmente o `DataReader` para entidades de domínio.
- Repositórios ficam em `infrastructure/persistence` e implementam interfaces declaradas em `application/usecases`.

```csharp
public class RecomendacaoRepository(NpgsqlDataSource dataSource) : IRecomendacaoRepository
{
    public async Task<IReadOnlyList<Recomendacao>> ListarPorUsuarioAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT r.score_recomendacao, r.tipo_algoritmo, r.motivo_recomendacao, ...
            FROM recomendacao r
            JOIN espetaculo e ON e.id_espetaculo = r.id_espetaculo
            WHERE r.id_usuario = @usuario
            ORDER BY r.score_recomendacao DESC
            LIMIT @limite;
            """;
        cmd.Parameters.AddWithValue("usuario", usuarioId);
        cmd.Parameters.AddWithValue("limite", limite);

        var itens = new List<Recomendacao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(/* mapeamento manual */);
        }
        return itens;
    }
}
```

### 2.4 Padrões TypeScript / React Native (Frontend)

#### 2.4.1 Arquitetura MVVM

O app segue **MVVM** com três responsabilidades bem separadas:

| Camada | Diretório | Responsabilidade |
|--------|-----------|------------------|
| **Model** | `src/frontend/models/` | Tipos de domínio do app (interfaces/types puros, sem React). |
| **View** | `src/frontend/screens/` | Componentes de tela (`.tsx`) — apenas JSX + binding com o ViewModel. |
| **ViewModel** | `src/frontend/viewmodels/` | Hooks `useXxxViewModel` que encapsulam estado, efeitos e regra de apresentação. |
| **Repository** | `src/frontend/repositories/` | Acesso a APIs (`fetch`), Supabase Storage e fontes externas. |

A View **não** chama `fetch` direto — sempre via ViewModel, que por sua vez delega ao Repository.

```ts
// repositories/CatalogRepository.ts
export async function listarRecomendacoesUsuario(
  accessToken: string,
  limite = 10,
): Promise<RecomendacaoUsuarioApi[]> {
  const payload = await fetchJson<Lista<RecomendacaoUsuarioApi>>(
    `${API_BASE_URL}/recomendacao/me?limite=${limite}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  );
  return payload.data ?? [];
}

// viewmodels/useCatalogViewModel.ts
export function useCatalogViewModel(accessToken?: string | null) {
  const [recomendacoes, setRecomendacoes] = useState<EspetaculoResumoApi[]>([]);
  // ... estado + efeitos ...

  useEffect(() => {
    if (!accessToken) return;
    listarRecomendacoesUsuario(accessToken)
      .then((itens) => setRecomendacoes(itens.map((item) => item.espetaculo)))
      .catch(() => setRecomendacoes([]));
  }, [accessToken]);

  return { recomendacoes, /* ... */ };
}

// screens/CatalogView.tsx
export function CatalogView() {
  const vm = useCatalogViewModel(session?.access_token);
  return <CatalogList items={vm.recomendacoes} />;
}
```

#### 2.4.2 Convenções TypeScript

- **`strict: true`** no `tsconfig.json`; nunca `any` sem comentário justificando.
- **Tipos de API** com sufixo `Api` (`EspetaculoResumoApi`) para diferenciar do modelo de domínio.
- Funções utilitárias e helpers em `lowerCamelCase` (`obterUrlMidia`, `normalizarTitulo`).
- Componentes React em `PascalCase`.
- Hooks sempre começam com `use` (regra do React).
- Tratamento de erro **explícito** no ViewModel: estado `error: string | null` + flag `loading`. A View renderiza o estado, não o `try/catch`.
- Cleanup de efeitos com a flag `ativo` para evitar `setState` após unmount em `fetch` assíncrono.

### 2.5 Padrões SQL (PostgreSQL / Supabase)

#### 2.5.1 Estrutura de Scripts

Os scripts ficam em `src/database/` com numeração sequencial e arquivo de rollback espelhado para migrations:

```text
src/database/
├── 00_supabase_compat.sql            # Compatibilidade com Supabase (schema auth, etc.)
├── 01_create_database.sql            # DDL principal
├── 02_initial_data.sql               # Seed
├── 03_rollback.sql                   # Rollback do DDL inicial
└── migrations/
    ├── 04_trigger_updated_at_espetaculo.sql
    ├── 04_trigger_updated_at_espetaculo_rollback.sql
    └── ...
```

#### 2.5.2 Convenções SQL

- **Palavras-chave em MAIÚSCULAS** (`SELECT`, `WHERE`, `JOIN`, `LATERAL`).
- **Tabelas no singular** (convenção adotada no schema atual: `usuario`, `espetaculo`, `recomendacao`).
- **Colunas em `snake_case`**, prefixadas pela tabela quando significativo (`id_usuario`, `id_espetaculo`, `score_recomendacao`).
- **Chaves primárias** UUID quando referenciam `auth.users` do Supabase, `SERIAL`/`INT` para entidades internas.
- **Enums tipados** (`tipo_usuario_enum`, `status_espetaculo_enum`) ao invés de strings livres.
- **Row Level Security (RLS)** habilitado para tabelas com dados de usuário; políticas declaradas no mesmo arquivo da tabela.
- **Migrations** sempre acompanhadas de script de rollback.
- **`TIMESTAMPTZ`** em todas as colunas de data/hora (timezone-aware).

```sql
CREATE TYPE status_espetaculo_enum AS ENUM (
    'em_cartaz',
    'em_breve',
    'encerrado'
);

CREATE TABLE espetaculo (
    id_espetaculo  SERIAL                  PRIMARY KEY,
    titulo         VARCHAR(200)            NOT NULL,
    genero         VARCHAR(80),
    status         status_espetaculo_enum  NOT NULL DEFAULT 'em_breve',
    nota_media     NUMERIC(3,2)            NOT NULL DEFAULT 0,
    criado_em      TIMESTAMPTZ             NOT NULL DEFAULT NOW()
);

ALTER TABLE espetaculo ENABLE ROW LEVEL SECURITY;

CREATE POLICY "espetaculo: leitura pública"
    ON espetaculo FOR SELECT
    USING (true);
```

### 2.6 Padrões de Documentação

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

## 3. Padrões de Projeto

Os padrões abaixo são aplicados de forma consistente nas camadas correspondentes do projeto.

### 3.1 Clean Architecture (Backend C#)

Cada microsserviço segue Clean Architecture (ver 2.3.1). Os benefícios concretos observados:

- **Testabilidade**: casos de uso e strategies dependem de interfaces, então são testáveis com fakes em xUnit sem subir banco ou HTTP.
- **Substituibilidade**: a camada `infrastructure` pode ser trocada (ex.: Npgsql → outra biblioteca) sem tocar nos casos de uso.
- **Limites claros**: controllers ficam finos — só fazem binding de request e delegam ao caso de uso correspondente.

### 3.2 Strategy Pattern (Backend C#)

#### 3.2.1 Problema

O endpoint `GET /recomendacao/me` pode atender o usuário de duas formas:

1. Usando o modelo incrementado ML.NET, quando o artefato está presente e habilitado.
2. Usando as recomendações pré-processadas no PostgreSQL, como fallback.

A escolha do algoritmo não pode vazar para o controller nem para o caso de uso.

#### 3.2.2 Implementação

```csharp
// application/services/RecomendacaoService.cs

public interface IRecomendacaoStrategy
{
    Task<IReadOnlyList<RecomendacaoDto>> ListarAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken);
}

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

#### 3.2.3 Justificativa

- **Open/Closed**: novas estratégias (ex.: motor remoto via HTTP) podem ser adicionadas sem alterar o `RecomendacaoService` nem o controller.
- **Fallback explícito**: a strategy ML.NET delega de forma controlada para a strategy PostgreSQL.
- **Testabilidade**: cada strategy é coberta isoladamente em `Spotlight.Recomendacao.Tests/RecomendacaoUseCasesTests.cs`.

### 3.3 Factory Method (Backend C#)

#### 3.3.1 Problema

O `RecomendacaoService` precisa de uma strategy, mas a escolha entre ML.NET e PostgreSQL depende de:

- flag `RecommendationModel:Enabled` em configuração;
- presença dos artefatos no disco (modelo ZIP, CSV de candidatos).

Não queremos que o service nem o controller conheçam esses detalhes.

#### 3.3.2 Implementação

```csharp
public interface IRecomendacaoStrategyFactory
{
    IRecomendacaoStrategy Criar();
}

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

O `RecomendacaoService` apenas pede uma strategy:

```csharp
var strategy = strategyFactory.Criar();
var recomendacoes = await strategy.ListarAsync(usuarioId, limiteNormalizado, cancellationToken);
```

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

- Datasets originais em `colab_csv_utf8/` (somente leitura), processados em `src/dataset/`.
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

### 5.1 Antes do Commit

- [ ] Código segue convenções de nomenclatura da seção 2.
- [ ] Funções públicas e tipos não-óbvios têm documentação (XML/JSDoc/docstring).
- [ ] Paths em Python usam `pathlib`; paths em C# usam `Path.Combine`.
- [ ] Seeds aleatórias estão fixadas em notebooks.
- [ ] Migrations SQL têm script de rollback espelhado.
- [ ] Sem `Console.WriteLine`/`console.log` esquecidos — usar `ILogger`/erro tratado.
- [ ] Mensagem de commit segue Conventional Commits com `#issue`.

### 5.2 Code Review — Backend C#

- [ ] Camadas respeitam Clean Architecture (sem `infrastructure` dentro de `application`).
- [ ] Controllers só fazem binding e delegam ao use case.
- [ ] SQL parametrizado, nunca interpolado.
- [ ] `CancellationToken` propagado em toda chamada assíncrona.
- [ ] `await using` em conexões/comandos/readers do Npgsql.
- [ ] Novos algoritmos de recomendação são adicionados como novas **strategies** + ajuste na **factory**.
- [ ] Exceções de domínio (`NotFoundException`, etc.) são lançadas em vez de retornar `null`/códigos HTTP no use case.

### 5.3 Code Review — Frontend RN

- [ ] Tela (`.tsx`) não contém `fetch` nem lógica de negócio — apenas binding.
- [ ] ViewModel tem `loading`, `error` e cleanup com flag `ativo`.
- [ ] Acesso a API/Storage está em `repositories/`.
- [ ] Tipos de API (`*Api`) separados dos modelos de domínio.
- [ ] Sem `any` sem comentário justificando.

### 5.4 Code Review — SQL

- [ ] Tabela nova: nome no singular, `snake_case`.
- [ ] PKs/FKs com nome explícito (`id_<tabela>`).
- [ ] Datas em `TIMESTAMPTZ`.
- [ ] RLS habilitado para tabelas com dados de usuário, com políticas declaradas.
- [ ] Enums tipados para campos com valores fechados.

## 6. Conclusão

Os padrões deste guia refletem o estado atual do Portal Aventura:

- **Clean Architecture + Strategy + Factory + Repository** no backend C# garantem que o algoritmo de recomendação possa evoluir sem mexer em controllers nem em casos de uso.
- **MVVM + Repository** no frontend mantêm Views simples e testáveis, e centralizam o contato com o gateway.
- **SQL puro com Npgsql** sobre PostgreSQL/Supabase preserva controle fino sobre o schema e RLS, sem o acoplamento de um ORM.
- **Builder** continua sendo a forma recomendada de organizar a construção de modelos nos notebooks de ML.

A aderência consistente a esses padrões reduz fricção em code review, facilita onboarding e mantém o código previsível em todas as camadas do produto.
