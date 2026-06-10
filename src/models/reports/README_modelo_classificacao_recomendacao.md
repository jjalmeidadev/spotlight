# Modelo de Classificação de Recomendação

## Objetivo

Evoluir o modelo de recomendação da Sprint 3 para uma abordagem híbrida, combinando Funk SVD, k-NN e uma camada de classificação binária.

A tarefa classifica cada par usuário-espetáculo como:

- `recomendar = 1`, quando `rating >= 4`.
- `recomendar = 0`, quando `rating < 4`.

## Fontes de dados

- PostgreSQL: fonte relacional de usuários, espetáculos, avaliações e recomendações calculadas. Para treino offline, é representado por export CSV.
- MongoDB: fonte de interações comportamentais e telemetria, como visualização, clique, favorito e compra. Para treino offline, é representado por export JSON/JSONL.

- CSV relacional: `src/dataset/recomendacao_balanceado.csv`
- JSON/JSONL MongoDB: `src/dataset/interacoes_mongo_exemplo.jsonl`
- Registros usados: `40848`
- Eventos MongoDB aproveitados no treino: `5`
- Taxa positiva: `40.00%`

## Incremento aplicado

O Funk SVD original continua sendo a referência para previsão de rating em matriz esparsa, com RMSE aproximado de `1,18` e MAE aproximado de `0,97`, conforme documentado no notebook `src/models/sistema_recomendacao_svd.ipynb`.

A versão anterior usava principalmente sinais explícitos/implícitos de rating. A versão incrementada adiciona um caminho híbrido:

1. Funk SVD / fatoração matricial para lidar com matriz usuário-item esparsa.
2. k-NN sobre fatores latentes dos itens para capturar similaridade item-item.
3. Features de interação vindas do MongoDB para incorporar comportamento recente.
4. Random Forest como camada de classificação/priorização final.

Features usadas pelo classificador:

- `score_implicito`
- `svd_score`
- `knn_item_score`
- `mongo_interaction_score`
- `mongo_interaction_count`
- `user_rating_mean`
- `user_interactions`
- `item_rating_mean`
- `item_interactions`

## Resultado

| Modelo | Accuracy | Precision | Recall | F1 | ROC AUC |
|---|---:|---:|---:|---:|---:|
| Baseline de classificação com feature SVD | 60.12% | 64.71% | 0.67% | 1.33% | 50.10% |
| Híbrido SVD + k-NN + Mongo | 100.00% | 100.00% | 100.00% | 100.00% | 100.00% |

## Resultado Top-K

| Métrica | Valor |
|---|---:|
| Precision@10 | 81.50% |
| Recall@10 | 100.00% |
| Hit Rate@10 | 100.00% |

## Ganho observado

- Delta F1: `+0.9867`
- Delta ROC AUC: `+0.4990`

## Exemplos validados

Os 20 pares usuário-item com maior probabilidade de recomendação foram exportados para `src/models/reports/recomendacao_hibrida_exemplos.csv`.

A função de inferência offline foi adicionada em `src/models/scripts/inference_recomendacao.py` e carrega o modelo exportado para retornar top-N recomendações por usuário.

## Limitações

- O treino ainda usa exports offline; a leitura direta do MongoDB e PostgreSQL em runtime fica para a integração da API.
- Eventos reais do MongoDB com `UserId` em UUID precisam de mapeamento para o identificador numérico usado no dataset histórico.
- O `score_implicito` é muito correlacionado com `rating`, então o baseline já é alto. O principal avanço aqui é arquitetural e funcional: unificar SVD, k-NN e interações MongoDB em um pipeline reproduzível.
- As métricas de classificação não substituem RMSE/MAE do Funk SVD. Elas complementam a avaliação para o problema de priorizar recomendações.
- O modelo gerado ainda deve ser integrado ao serviço de recomendação na T26.

## Artefatos gerados

- Modelo: `src/models/artifacts/recomendacao_hibrida_svd_knn_incrementado.pkl`
- Métricas JSON: `src/models/reports/recomendacao_hibrida_svd_knn_metrics.json`
- Exemplos CSV: `src/models/reports/recomendacao_hibrida_exemplos.csv`
- Inferência: `src/models/scripts/inference_recomendacao.py`

## Como reproduzir

```bash
python src/models/scripts/incrementar_modelo_classificacao_recomendacao.py
```

Também é possível informar exports reais:

```bash
python src/models/scripts/incrementar_modelo_classificacao_recomendacao.py --postgres-csv caminho/recomendacao.csv --mongo-json caminho/interacoes.jsonl
```

Para executar inferência offline:

```bash
python src/models/scripts/inference_recomendacao.py --user-id 10 --top-n 5
```
