# Modelo de Classificacao de Recomendacao

## Objetivo

Evoluir o modelo de recomendacao da Sprint 3 para um artefato consumivel diretamente pela API .NET. O incremento usa ML.NET para classificar pares usuario-espetaculo como recomendaveis e preserva um artefato auxiliar de similaridade item-item.

## Fontes de dados

- PostgreSQL exportado em CSV: `src/dataset/recomendacao_balanceado.csv`
- MongoDB exportado em JSONL: `src/dataset/interacoes_mongo_exemplo.jsonl`
- Registros usados: `40848`
- Eventos MongoDB aproveitados: `5`
- Taxa positiva: `40,00%`

## Incremento aplicado

O Funk SVD original continua sendo a referencia historica para predicao de rating em matriz esparsa, com RMSE aproximado de `1,18` e MAE aproximado de `0,97`, conforme documentado no notebook `src/models/sistema_recomendacao_svd.ipynb`.

A versao incrementada passa a gerar um modelo ML.NET em `.zip`, que pode ser carregado diretamente pelo backend C# na T26. O pipeline combina:

1. sinais relacionais do PostgreSQL;
2. features comportamentais vindas do MongoDB;
3. agregacoes por usuario e item;
4. similaridade item-item exportada em JSON para apoiar reordenacao kNN;
5. classificador binario ML.NET para priorizar recomendacoes.

## Metricas

| Modelo | Accuracy | Precision | Recall | F1 | ROC AUC |
|---|---:|---:|---:|---:|---:|
| ML.NET incrementado | 65,76% | 53,78% | 92,25% | 67,95% | 77,52% |

## Top-K

| Metrica | Valor |
|---|---:|
| Precision@10 | 80,89% |
| Recall@10 | 100,00% |
| Hit Rate@10 | 100,00% |

## Limitacoes

- O treino ainda usa exports offline do PostgreSQL e MongoDB.
- O dataset historico usa usuario numerico; usuarios UUID da API precisam de mapeamento na integracao.
- O `score_implicito` segue altamente correlacionado com `rating`, entao as metricas de classificacao devem ser lidas como validacao funcional do pipeline, nao como comparacao direta com RMSE/MAE do Funk SVD.
- A similaridade item-item exportada e uma aproximacao simples baseada em agregados; pode ser substituida por kNN sobre embeddings em iteracoes futuras.

## Artefatos

- Modelo ML.NET: `src/models/artifacts/recomendacao_incrementada_mlnet.zip`
- Similaridades item-item: `src/models/artifacts/item_similarities.json`
- Metricas JSON: `src/models/reports/recomendacao_mlnet_metrics.json`
- Exemplos CSV: `src/models/reports/recomendacao_mlnet_exemplos.csv`
- Projeto de treino: `src/models/Spotlight.Recomendacao.ModelTraining`

## Como reproduzir

```powershell
dotnet run --project src/models/Spotlight.Recomendacao.ModelTraining/Spotlight.Recomendacao.ModelTraining.csproj
```