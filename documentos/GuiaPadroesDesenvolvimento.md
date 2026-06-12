#- Portal Aventura

## 1. Introdução

Este documento estabelece os padrões de codificação e padrões de projeto adotados no desenvolvimento do sistema de recomendação do Portal Aventura. O objetivo é garantir qualidade, manutenibilidade e consistência do código ao longo de todo o projeto.

## 2. Padrões de Codificação

### 2.1 Convenções Gerais

#### 2.1.1 Estrutura de Diretórios
```
g04/
├── documentos/                          # Documentação do projeto (markdowns + imagens)
│   ├── img/                             # Imagens, diagramas, mockups, wireframes
│   └── *.md                             # Projeto, Manual de instalação, padrões, SOA, etc.
├── src/
│   ├── aad/                             # Notebooks de análise e tratamento de dados
│   ├── backend/                         # Backend SOA em ASP.NET Core (.NET 9)
│   │   ├── Spotlight.sln                # Solution: gateway + 4 serviços + projetos de teste
│   │   ├── api-gateway/                 # Spotlight.Gateway (porta 5000) — YARP
│   │   ├── services/
│   │   │   ├── catalogo/                # Spotlight.Catalogo        (5001)
│   │   │   ├── usuario/                 # Spotlight.Usuario         (5002) + telemetria Mongo
│   │   │   ├── recomendacao/            # Spotlight.Recomendacao    (5003) + ML.NET strategy
│   │   │   └── geolocalizacao/          # Spotlight.Geolocalizacao  (5004) — Nominatim
│   │   ├── contracts/                   # Contratos de API/eventos e docs operacionais
│   │   ├── ml-engine/                   # Adapters de inferência/treino consumidos pelos serviços
│   │   ├── scripts/                     # Scripts auxiliares (PowerShell)
│   │   └── tests/                       # Projetos xUnit por serviço (Spotlight.<Servico>.Tests)
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
│   └── models/                          # Notebooks + treino ML.NET + artefatos publicados
│       ├── Spotlight.Recomendacao.ModelTraining/   # Projeto .NET de treino
│       ├── artifacts/                              # Modelo .zip e similaridades
│       └── reports/                                # Métricas e exemplos do modelo
└── colab_csv_utf8/                      # Datasets originais (CSV bruto)
```

#### 2.1.2 Nomenclatura de Arquivos
- **Notebooks Jupyter**: `snake_case` (ex: `sistema_recomendacao_knn.ipynb`)
- **Scripts SQL**: `XX_descricao.sql` onde XX é número sequencial
- **Datasets**: `nome_descritivo.csv` (ex: `recomendacao_balanceado.csv`)
- **Documentação**: `PascalCase.md` (ex: `GuiaPadroesDesenvolvimento.md`)

### 2.2 Padrões Python (Notebooks Jupyter)

#### 2.2.1 Estrutura de Notebooks
```python
# Estrutura padrão de um notebook:
# 1. Configuração do Ambiente
# 2. Carregamento de Dados
# 3. Análise Exploratória
# 4. Modelagem
# 5. Avaliação
# 6. Conclusão
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
```

#### 2.2.3 Configuração de Caminhos
```python
# Uso de pathlib para caminhos independentes de SO
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
- **Variáveis**: `snake_case` (ex: `matriz_item_usuario`)
- **Constantes**: `UPPER_SNAKE_CASE` (ex: `N_VIZINHOS`)
- **Funções**: `snake_case` com verbos descritivos
```python
def recomendar_espetaculos(item_id, n_recomendacoes=10, verbose=True):
    """
    Recomenda espetaculos similares a um espetaculo-alvo.
    
    Parametros
    ----------
    item_id : int
        ID numerico do espetaculo de referencia
    n_recomendacoes : int
        Numero de recomendacoes a retornar
    verbose : bool
        Se True, exibe cabecalho com espetaculo de referencia
    
    Retorna
    -------
    pd.DataFrame
        Dataframe com recomendacoes ordenadas por similaridade
    """
```

#### 2.2.5 Documentação Inline
```python
# Seções principais com markdown
## 1. Configuracao do Ambiente

# Comentários explicativos antes de blocos complexos
# Zero representa ausência de interação na matriz de similaridade
matriz_preenchida = matriz_item_usuario.fillna(0)
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

### 2.4 Padrões de Documentação

#### 2.4.1 Estrutura de Documentos Markdown
```markdown
# Título Principal

## Seção
### Subseção

| Coluna | Descrição |
|--------|-----------|
| Valor  | Explicação |
```

#### 2.4.2 Tabelas de Métricas
```markdown
| Métrica | Valor |
|---------|-------|
| RMSE    | 1.5977 |
| MAE     | 1.2296 |
| Cobertura | 69.8% |
```

### 2.5 Padrões de Mensagens de Commit (Git Hooks)

Este projeto adota o padrão de **Conventional Commits** e utiliza um Git Hook para validar se todas as mensagens de commit seguem essa estrutura, garantindo a associação com a issue correspondente.

#### 2.5.1 Estrutura de Commit

A mensagem de commit deve seguir o seguinte formato:
```text
<type>: <descrição> #<issue>
```

* **Exemplos Válidos:**
  - `docs: atualiza documentação #12`
  - `feat: cria endpoint de login #34`
  - `fix(api): corrige validação #105`

* **Regras de Validação:**
  - O **type** deve ser um dos seguintes: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`, `revert`.
  - A **descrição** não pode ser vazia.
  - A **issue** deve ser identificada por `#` seguida de pelo menos 2 dígitos (ex: `#12`).

#### 2.5.2 Ativação do Hook de Validação

Para ativar a validação local automática antes de cada commit, execute o seguinte comando no terminal na raiz do repositório:

```bash
git config core.hooksPath .githooks
```

#### 2.5.3 Desativação do Hook de Validação

Caso seja necessário desativar a validação automática (restaurando o comportamento padrão de hooks do Git no diretório `.git/hooks`), execute:

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

### 3.1 Padrão Criacional: Builder Pattern

#### 3.1.1 Problema Concreto no Sistema
O sistema de recomendação do Portal Aventura enfrenta o seguinte problema concreto:

**Complexidade na construção do modelo de recomendação**: A criação de um modelo KNN funcional requer múltiplas etapas interdependentes que devem ser executadas em ordem específica:
1. Carregamento e validação dos dados
2. Construção da matriz item-usuário com tratamento de esparsidade
3. Configuração de hiperparâmetros do modelo
4. Transformação para matriz esparsa CSR
5. Treinamento do modelo
6. Geração de mapeamentos auxiliares (item_to_idx, idx_to_item)

Sem um padrão adequado, esse processo resultaria em:
- Código procedural longo e difícil de manter
- Duplicação ao criar variações do modelo (diferentes valores de k)
- Dificuldade para testar etapas individuais
- Acoplamento forte entre as etapas de construção

#### 3.1.2 Por que o Builder Pattern Resolve Este Problema
O **Builder Pattern** é o padrão criacional mais adequado porque:

1. **Construção passo a passo**: Permite controlar cada etapa da criação do modelo complexo
2. **Encapsulamento da complexidade**: Esconde os detalhes de implementação de cada etapa
3. **Flexibilidade de configuração**: Facilita criar diferentes configurações do modelo (k=15, k=20, etc.)
4. **Imutabilidade controlada**: Garante que o modelo só seja usado quando completamente construído
5. **Interface fluente**: Melhora a legibilidade com method chaining

#### 3.1.3 Comparação com Outros Padrões Criacionais GoF

| Padrão | Por que NÃO foi escolhido | Problema que resolveria | Nossa necessidade |
|--------|---------------------------|------------------------|-------------------|
| **Factory Method** | Criamos sempre o mesmo tipo de objeto (modelo KNN), não uma hierarquia | Criação polimórfica de objetos | Construção complexa de um único tipo |
| **Abstract Factory** | Não temos famílias de objetos relacionados | Criar famílias de objetos compatíveis | Construir um objeto complexo passo a passo |
| **Singleton** | Precisamos múltiplas instâncias para comparação (k=15 vs k=20) | Garantir instância única | Múltiplas configurações do modelo |
| **Prototype** | Não clonamos modelos existentes, construímos do zero | Clonar objetos complexos | Construir novos modelos com configurações específicas |

#### 3.1.4 Implementação Detalhada
```python
class ModeloRecomendacaoBuilder:
    """
    Builder para construção incremental do sistema de recomendação.
    Permite configuração flexível dos componentes do modelo.
    """
    
    def __init__(self):
        self.dados = None
        self.matriz = None
        self.modelo = None
        self.metricas = {}
    
    def carregar_dados(self, path):
        """Etapa 1: Carregamento dos dados"""
        self.dados = pd.read_csv(path)
        return self
    
    def construir_matriz(self):
        """Etapa 2: Construção da matriz item-usuário"""
        self.matriz = self.dados.pivot_table(
            index='item_id',
            columns='user_id', 
            values='rating'
        )
        return self
    
    def configurar_modelo(self, n_vizinhos=20):
        """Etapa 3: Configuração do modelo KNN"""
        self.modelo = NearestNeighbors(
            n_neighbors=n_vizinhos,
            metric='cosine',
            algorithm='brute'
        )
        return self
    
    def treinar(self):
        """Etapa 4: Treinamento do modelo"""
        matriz_esparsa = csr_matrix(self.matriz.fillna(0))
        self.modelo.fit(matriz_esparsa)
        return self
    
    def build(self):
        """Retorna o modelo completo construído"""
        return {
            'modelo': self.modelo,
            'matriz': self.matriz,
            'metricas': self.metricas
        }

# Uso do Builder Pattern
sistema = ModeloRecomendacaoBuilder()
    .carregar_dados('../dataset/recomendacao_balanceado.csv')
    .construir_matriz()
    .configurar_modelo(n_vizinhos=20)
    .treinar()
    .build()
```

#### 3.1.5 Aplicação Prática no Código Real do Projeto

No notebook `sistema_recomendacao_knn.ipynb`, o processo atual de construção do modelo segue exatamente as etapas que o Builder encapsula:

```python
# Código atual (procedural) - linha por linha no notebook:
df = pd.read_csv('../dataset/recomendacao_balanceado.csv')
matriz_item_usuario = df.pivot_table(index='item_id', columns='user_id', values='rating')
matriz_preenchida = matriz_item_usuario.fillna(0)
matriz_esparsa = csr_matrix(matriz_preenchida.values)
modelo_knn = NearestNeighbors(n_neighbors=20, metric='cosine', algorithm='brute')
modelo_knn.fit(matriz_esparsa)

# Com Builder Pattern - mais limpo e reutilizável:
modelo = ModeloRecomendacaoBuilder()
    .carregar_dados('../dataset/recomendacao_balanceado.csv')
    .construir_matriz()
    .configurar_modelo(n_vizinhos=20)
    .treinar()
    .build()
```

#### 3.1.6 Benefícios Concretos para o Portal Aventura

1. **Experimentação facilitada**: Testar diferentes valores de k sem duplicar código
2. **Manutenção simplificada**: Mudanças em uma etapa não afetam as outras
3. **Testes unitários**: Cada método do builder pode ser testado isoladamente
4. **Documentação viva**: O código se torna autodocumentado
5. **Extensibilidade**: Fácil adicionar novas etapas (ex: normalização, validação)

#### 3.1.7 Validação da Escolha
O Builder Pattern é a escolha correta porque nosso problema central é a **complexidade de construção**, não a criação de diferentes tipos (Factory), nem a necessidade de instância única (Singleton), nem a clonagem (Prototype). É um caso clássico de uso do Builder conforme descrito no livro GoF: "Separar a construção de um objeto complexo de sua representação".

### 3.2 Padrão Comportamental: Strategy Pattern

#### 3.2.1 Contexto de Aplicação
O **Strategy Pattern** é implementado para permitir diferentes estratégias de recomendação (KNN, SVD, Funk SVD) de forma intercambiável.

#### 3.2.2 Implementação
```python
from abc import ABC, abstractmethod

class EstrategiaRecomendacao(ABC):
    """Interface para estratégias de recomendação"""
    
    @abstractmethod
    def treinar(self, dados):
        """Treina o modelo com os dados fornecidos"""
        pass
    
    @abstractmethod
    def recomendar(self, item_id, n_recomendacoes=10):
        """Retorna recomendações para um item"""
        pass
    
    @abstractmethod
    def avaliar(self, dados_teste):
        """Avalia o desempenho do modelo"""
        pass

class RecomendacaoKNN(EstrategiaRecomendacao):
    """Estratégia de recomendação usando KNN"""
    
    def __init__(self, n_vizinhos=20):
        self.n_vizinhos = n_vizinhos
        self.modelo = None
        self.matriz = None
    
    def treinar(self, dados):
        # Construir matriz item-usuário
        self.matriz = dados.pivot_table(
            index='item_id',
            columns='user_id',
            values='rating'
        ).fillna(0)
        
        # Treinar modelo KNN
        self.modelo = NearestNeighbors(
            n_neighbors=self.n_vizinhos,
            metric='cosine'
        )
        self.modelo.fit(csr_matrix(self.matriz))
        return self
    
    def recomendar(self, item_id, n_recomendacoes=10):
        idx = self.item_to_idx[item_id]
        dist, indices = self.modelo.kneighbors(
            self.matriz[idx],
            n_neighbors=n_recomendacoes + 1
        )
        # Processar e retornar recomendações
        return self._processar_recomendacoes(indices, dist)
    
    def avaliar(self, dados_teste):
        # Calcular RMSE, MAE e cobertura
        metricas = {
            'RMSE': self._calcular_rmse(dados_teste),
            'MAE': self._calcular_mae(dados_teste),
            'Cobertura': self._calcular_cobertura(dados_teste)
        }
        return metricas

class RecomendacaoSVD(EstrategiaRecomendacao):
    """Estratégia de recomendação usando SVD"""
    
    def __init__(self, n_fatores=50):
        self.n_fatores = n_fatores
        self.modelo = None
    
    def treinar(self, dados):
        # Implementação SVD
        pass
    
    def recomendar(self, item_id, n_recomendacoes=10):
        # Implementação específica SVD
        pass
    
    def avaliar(self, dados_teste):
        # Avaliação específica SVD
        pass

class SistemaRecomendacao:
    """Contexto que usa as estratégias"""
    
    def __init__(self, estrategia: EstrategiaRecomendacao):
        self.estrategia = estrategia
    
    def mudar_estrategia(self, estrategia: EstrategiaRecomendacao):
        """Permite trocar a estratégia em tempo de execução"""
        self.estrategia = estrategia
    
    def executar_recomendacao(self, item_id, n=10):
        """Delega a recomendação para a estratégia atual"""
        return self.estrategia.recomendar(item_id, n)
    
    def comparar_estrategias(self, estrategias, dados_teste):
        """Compara múltiplas estratégias"""
        resultados = []
        for estrategia in estrategias:
            self.mudar_estrategia(estrategia)
            metricas = estrategia.avaliar(dados_teste)
            resultados.append({
                'modelo': estrategia.__class__.__name__,
                **metricas
            })
        return pd.DataFrame(resultados)

# Uso do Strategy Pattern
# Criar diferentes estratégias
knn_strategy = RecomendacaoKNN(n_vizinhos=20)
svd_strategy = RecomendacaoSVD(n_fatores=50)

# Criar sistema com estratégia inicial
sistema = SistemaRecomendacao(knn_strategy)

# Treinar e usar
knn_strategy.treinar(dados_treino)
recomendacoes = sistema.executar_recomendacao(item_id=16, n=10)

# Trocar estratégia em tempo de execução
sistema.mudar_estrategia(svd_strategy)
svd_strategy.treinar(dados_treino)
recomendacoes_svd = sistema.executar_recomendacao(item_id=16, n=10)

# Comparar estratégias
resultados = sistema.comparar_estrategias(
    [knn_strategy, svd_strategy],
    dados_teste
)
```

#### 3.2.3 Justificativa
O Strategy Pattern foi escolhido porque:
- **Flexibilidade de algoritmos**: Permite trocar entre KNN, SVD e outros modelos facilmente
- **Princípio Open/Closed**: Aberto para extensão (novos algoritmos), fechado para modificação
- **Comparação facilitada**: Permite avaliar múltiplos modelos com a mesma interface
- **Manutenibilidade**: Cada estratégia é independente e pode ser modificada isoladamente

### 3.3 Aplicação no Backend C# da Sprint 4

Na Sprint 4, os padrões de projeto também foram aplicados no backend C#, dentro do serviço `Spotlight.Recomendacao`. Essa aplicação endereça diretamente a evolução do artefato de desenvolvimento: o sistema deixou de ter os padrões apenas descritos no contexto dos notebooks de ML e passou a usá-los no fluxo real da API de recomendação.

#### 3.3.1 Padrão Comportamental: Strategy no Serviço de Recomendação

O padrão **Strategy** foi aplicado para permitir que o endpoint de recomendação personalize a escolha do algoritmo sem alterar o controller nem o use case.

Arquivo principal:

```text
src/backend/services/recomendacao/application/services/RecomendacaoService.cs
```

Classes e interfaces envolvidas:

| Elemento | Papel no padrão |
|----------|-----------------|
| `IRecomendacaoStrategy` | Contrato comum para qualquer estratégia de recomendação |
| `ModeloMlNetRecomendacaoStrategy` | Estratégia que usa o modelo incrementado ML.NET e enriquece o resultado com dados do PostgreSQL |
| `PostgresPreProcessadoRecomendacaoStrategy` | Estratégia de fallback baseada nas recomendações pré-processadas no PostgreSQL |
| `RecomendacaoService` | Contexto que delega a execução para a strategy ativa |

Com isso, a API consegue alternar entre:

1. modelo incrementado ML.NET, quando `RecommendationModel:Enabled` está habilitado;
2. recomendações pré-processadas no PostgreSQL, quando o modelo não está habilitado, não retorna itens ou falha.

Essa decisão reduz acoplamento e melhora a manutenibilidade, porque novos algoritmos podem ser adicionados como novas strategies sem modificar o contrato público da API.

#### 3.3.2 Padrão Criacional: Factory para Selecionar a Strategy Ativa

O padrão criacional aplicado no backend foi uma **Factory** responsável por selecionar a strategy de recomendação ativa.

Classes e interfaces envolvidas:

| Elemento | Papel no padrão |
|----------|-----------------|
| `IRecomendacaoStrategyFactory` | Contrato de criação/seleção da strategy |
| `RecomendacaoStrategyFactory` | Implementação que decide entre ML.NET e PostgreSQL |

A factory encapsula a decisão de qual strategy usar:

```csharp
public IRecomendacaoStrategy Criar() =>
    modeloClient.Habilitado ? modeloStrategy : postgresStrategy;
```

Sem a factory, o `RecomendacaoService` precisaria conhecer detalhes de configuração do modelo e de fallback. Com a factory, essa decisão fica centralizada, testável e substituível por injeção de dependência.

#### 3.3.3 Registro por Injeção de Dependência

O registro das implementações ocorre no `Program.cs` do serviço de recomendação:

```text
src/backend/services/recomendacao/Program.cs
```

Registros principais:

```csharp
builder.Services.AddSingleton<IModeloRecomendacaoClient, MlNetModeloRecomendacaoClient>();
builder.Services.AddScoped<ModeloMlNetRecomendacaoStrategy>();
builder.Services.AddScoped<PostgresPreProcessadoRecomendacaoStrategy>();
builder.Services.AddScoped<IRecomendacaoStrategyFactory, RecomendacaoStrategyFactory>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
```

Essa estrutura mantém o controller dependente apenas do use case/service, preservando a Clean Architecture adotada no backend.

#### 3.3.4 Validação por Testes

A aplicação dos padrões foi validada no projeto:

```text
src/backend/tests/Spotlight.Recomendacao.Tests/RecomendacaoUseCasesTests.cs
```

Cenários cobertos:

- normalização de limite e mapeamento usando a strategy PostgreSQL;
- seleção da strategy ML.NET pela factory quando o modelo está habilitado;
- uso do ranking do modelo incrementado e enriquecimento dos itens no PostgreSQL;
- fallback para PostgreSQL quando necessário.

Resultado validado localmente:

```text
dotnet test src/backend/tests/Spotlight.Recomendacao.Tests/Spotlight.Recomendacao.Tests.csproj
Aprovado: 16
Falhou: 0
Ignorado: 0
```

## 4. Boas Práticas Implementadas

### 4.1 Gestão de Dados
- **Versionamento de datasets**: Manter versões balanceadas e não balanceadas
- **Separação clara**: Dados brutos em `colab_csv_utf8/`, processados em `src/dataset/`
- **Reproducibilidade**: Seeds fixas para operações aleatórias (`random_state=42`)

### 4.2 Experimentação e Avaliação
- **Múltiplas métricas**: RMSE, MAE, Cobertura, Hit Rate@10
- **Comparação sistemática**: Tabela comparativa entre modelos
- **Visualizações**: Gráficos salvos em `documentos/img/`

### 4.3 Documentação
- **Docstrings detalhadas**: Parâmetros, retornos e exceções documentados
- **Seções estruturadas**: Notebooks organizados em seções numeradas
- **Tabelas de resultados**: Métricas apresentadas de forma clara

### 4.4 Performance
- **Matrizes esparsas**: Uso de `csr_matrix` para economia de memória
- **Processamento paralelo**: `n_jobs=-1` para usar todos os cores
- **Cache de resultados**: Evitar recálculos desnecessários

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

Este guia estabelece os padrões fundamentais para o desenvolvimento do Portal Aventura. A adoção consistente destes padrões garante:
- **Qualidade**: Código limpo e bem estruturado
- **Manutenibilidade**: Fácil compreensão e modificação
- **Escalabilidade**: Arquitetura preparada para crescimento
- **Colaboração**: Padrões claros para toda a equipe

Os padrões de projeto Builder e Strategy fornecem a flexibilidade necessária para evoluir o sistema de recomendação, permitindo experimentação com diferentes algoritmos e configurações sem comprometer a estrutura base do código.
