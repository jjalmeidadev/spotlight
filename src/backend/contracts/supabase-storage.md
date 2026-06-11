# Supabase Storage - artefatos do modelo

Este documento descreve o uso de Supabase Storage no Spotlight apenas para
artefatos privados do modelo de recomendacao.

## Bucket

| Bucket | Visibilidade | Uso | Quem acessa |
|---|---|---|---|
| `ml-artifacts` | Privado | Modelos treinados, similaridades, metricas e datasets candidatos | Backend e CI com `SUPABASE_SERVICE_ROLE_KEY` |

O frontend nunca deve acessar `ml-artifacts`. O modelo deve ser carregado pelo
backend ou publicado pelo pipeline de CI usando uma chave segura do ambiente.

## Criacao do bucket

Execute a migration abaixo no SQL Editor do Supabase, depois do schema principal:

```sql
src/database/migrations/10_supabase_storage_buckets.sql
```

Ela cria `ml-artifacts` como bucket privado, com limite de 100 MB por arquivo e
MIME types para `.zip`, `.json`, `.csv` e texto.

O bucket nao recebe policy publica. O acesso operacional deve acontecer com
service role key em ambiente seguro.

## Estrutura recomendada dos objetos

```text
ml-artifacts/
  models/
    recomendacao/
      current/
        recomendacao_incrementada_mlnet.zip
        item_similarities.json
        recomendacao_balanceado.csv
      <commit-sha>/
        recomendacao_incrementada_mlnet.zip
        item_similarities.json
        recomendacao_balanceado.csv
```

`current/` aponta para a versao ativa consumida pelo backend. O prefixo por
commit preserva historico e permite rollback.

## Variaveis de ambiente

Configure estas variaveis no ambiente do backend:

```env
SUPABASE_URL=https://project-ref.supabase.co
SUPABASE_SERVICE_ROLE_KEY=<secret-server-side>
SUPABASE_MODEL_BUCKET=ml-artifacts
SUPABASE_MODEL_ARTIFACTS_ENABLED=true
RECOMMENDATION_MODEL_ENABLED=true
```

No Docker Compose, o servico `recomendacao` baixa os artefatos para:

```text
/app/model-artifacts/
```

e usa estes caminhos:

```env
RecommendationModel__ModelPath=/app/model-artifacts/recomendacao_incrementada_mlnet.zip
RecommendationModel__CandidateCsvPath=/app/model-artifacts/recomendacao_balanceado.csv
RecommendationModel__SimilaritiesPath=/app/model-artifacts/item_similarities.json
```

## Treinamento e publicacao pelo CI

O job `train:model-artifacts` gera os artefatos do modelo dentro da propria CI
com o projeto:

```text
src/models/Spotlight.Recomendacao.ModelTraining/Spotlight.Recomendacao.ModelTraining.csproj
```

Ele usa como entrada:

```text
src/dataset/recomendacao_balanceado.csv
src/dataset/interacoes_mongo_exemplo.jsonl
```

e gera:

- `src/models/artifacts/recomendacao_incrementada_mlnet.zip`
- `src/models/artifacts/item_similarities.json`
- `src/models/reports/recomendacao_mlnet_metrics.json`
- `src/models/reports/README_modelo_classificacao_recomendacao.md`
- `src/models/reports/recomendacao_mlnet_exemplos.csv`

Depois que os testes e o treinamento passam, o job `publish:model-artifacts`
publica automaticamente no bucket quando a pipeline roda em `develop`.

O upload envia:

- `src/models/artifacts/recomendacao_incrementada_mlnet.zip`
- `src/models/artifacts/item_similarities.json`
- `src/dataset/recomendacao_balanceado.csv`

Para permitir a publicacao, cadastre no GitLab CI/CD variables:

| Variavel | Tipo | Protecao recomendada |
|---|---|---|
| `SUPABASE_URL` | variable | masked |
| `SUPABASE_SERVICE_ROLE_KEY` | variable | masked + protected |

O job de publicacao envia os artefatos para dois prefixos:

```text
models/recomendacao/$CI_COMMIT_SHORT_SHA/
models/recomendacao/current/
```

Assim, o backend pode sempre consumir `current/`, enquanto o historico por commit
fica disponivel para auditoria e rollback.

## Fluxo final

1. CI executa os testes e treina o modelo.
2. CI publica os artefatos em `ml-artifacts` usando `SUPABASE_SERVICE_ROLE_KEY`.
3. Servico de recomendacao baixa `current/` no startup quando
   `SUPABASE_MODEL_ARTIFACTS_ENABLED=true`.
4. API `GET /recomendacao/me` usa o modelo carregado ou faz fallback para
   recomendacoes pre-processadas no PostgreSQL.

## Cuidados

- Nunca exponha `SUPABASE_SERVICE_ROLE_KEY` no frontend Expo.
- Mantenha o bucket de modelo privado.
- Nao use Storage como banco de telemetria. Telemetria bruta continua no MongoDB
  Atlas; recomendacoes persistidas continuam no PostgreSQL/Supabase.
