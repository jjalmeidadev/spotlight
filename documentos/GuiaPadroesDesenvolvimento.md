# Guia de Padrões de Desenvolvimento - Portal Aventura

## 1. Introdução

Este documento estabelece os padrões de codificação e padrões de projeto adotados no desenvolvimento do sistema de recomendação do Portal Aventura. O objetivo é garantir qualidade, manutenibilidade e consistência do código ao longo de todo o projeto.

## 2. Padrões de Codificação

### 2.1 Convenções Gerais

#### 2.1.1 Estrutura de Diretórios
```
g04/
├── documentos/          # Documentação do projeto
│   ├── img/            # Imagens e diagramas
│   └── *.md            # Arquivos Markdown de documentação
├── src/                # Código-fonte
│   ├── aad/            # Análise e tratamento de dados
│   ├── database/       # Scripts SQL e configurações de BD
│   ├── dataset/        # Datasets processados
│   └── models/         # Modelos de machine learning
└── colab_csv_utf8/     # Datasets originais
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

### 2.3 Padrões SQL

#### 2.3.1 Estrutura de Scripts
```sql
-- 01_create_database.sql
-- Descrição: Criação da estrutura do banco de dados

CREATE DATABASE IF NOT EXISTS portal_aventura;
USE portal_aventura;

-- Tabelas principais
CREATE TABLE usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### 2.3.2 Convenções SQL
- **Palavras-chave**: MAIÚSCULAS (ex: `SELECT`, `WHERE`)
- **Nomes de tabelas**: snake_case plural (ex: `usuarios`, `espetaculos`)
- **Nomes de colunas**: snake_case singular (ex: `user_id`, `item_id`)
- **Chaves primárias**: `id` ou `tabela_id`
- **Chaves estrangeiras**: `tabela_referenciada_id`

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
- [ ] Código segue convenções de nomenclatura
- [ ] Funções possuem docstrings completas
- [ ] Notebooks estão organizados em seções
- [ ] Paths usam pathlib para portabilidade
- [ ] Seeds aleatórias estão fixadas
- [ ] Métricas estão documentadas
- [ ] Visualizações salvas em `documentos/img/`

### Review de Código
- [ ] Builder Pattern usado para construções complexas
- [ ] Strategy Pattern para algoritmos intercambiáveis
- [ ] Separação clara de responsabilidades
- [ ] Código reutilizável e extensível
- [ ] Tratamento adequado de exceções
- [ ] Performance otimizada com matrizes esparsas

## 6. Conclusão

Este guia estabelece os padrões fundamentais para o desenvolvimento do Portal Aventura. A adoção consistente destes padrões garante:
- **Qualidade**: Código limpo e bem estruturado
- **Manutenibilidade**: Fácil compreensão e modificação
- **Escalabilidade**: Arquitetura preparada para crescimento
- **Colaboração**: Padrões claros para toda a equipe

Os padrões de projeto Builder e Strategy fornecem a flexibilidade necessária para evoluir o sistema de recomendação, permitindo experimentação com diferentes algoritmos e configurações sem comprometer a estrutura base do código.