"""Incrementa o modelo de recomendacao com abordagem hibrida SVD + k-NN.

Entradas:
- CSV relacional exportado do PostgreSQL: pares usuario-item com rating.
- JSON/JSONL exportado do MongoDB: eventos de interacao/telemetria.

Saidas:
- modelo .pkl com SVD, k-NN, classificador e metadados de features;
- metricas em JSON;
- relatorio Markdown;
- exemplos de recomendacoes em CSV para inspecao.
"""

from __future__ import annotations

import argparse
import json
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Any

import joblib
import numpy as np
import pandas as pd
from scipy.sparse import csr_matrix
from sklearn.decomposition import TruncatedSVD
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, f1_score, precision_score, recall_score, roc_auc_score
from sklearn.model_selection import train_test_split
from sklearn.neighbors import NearestNeighbors


ROOT = Path(__file__).resolve().parent
MODELS_ROOT = ROOT.parent
SRC_ROOT = MODELS_ROOT.parent
REPO_ROOT = SRC_ROOT.parent
DATA_ROOT = SRC_ROOT / "dataset"
DEFAULT_POSTGRES_CSV = DATA_ROOT / "recomendacao_balanceado.csv"
DEFAULT_MONGO_JSONL = DATA_ROOT / "interacoes_mongo_exemplo.jsonl"

ARTIFACTS_ROOT = MODELS_ROOT / "artifacts"
REPORTS_ROOT = MODELS_ROOT / "reports"

MODEL_OUT = ARTIFACTS_ROOT / "recomendacao_hibrida_svd_knn_incrementado.pkl"
METRICS_JSON = REPORTS_ROOT / "recomendacao_hibrida_svd_knn_metrics.json"
REPORT_MD = REPORTS_ROOT / "README_modelo_classificacao_recomendacao.md"
EXAMPLES_CSV = REPORTS_ROOT / "recomendacao_hibrida_exemplos.csv"

TARGET = "recomendar"
FEATURE_COLUMNS = [
    "score_implicito",
    "svd_score",
    "knn_item_score",
    "mongo_interaction_score",
    "mongo_interaction_count",
    "user_rating_mean",
    "user_interactions",
    "item_rating_mean",
    "item_interactions",
]

EVENT_WEIGHTS = {
    "screen_viewed": 0.05,
    "show_card_clicked": 0.35,
    "show_viewed": 0.45,
    "favorite_toggled": 0.75,
    "purchase_clicked": 1.0,
}


@dataclass(frozen=True)
class Metrics:
    accuracy: float
    precision: float
    recall: float
    f1: float
    roc_auc: float


def read_json_events(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        return []

    text = path.read_text(encoding="utf-8").strip()
    if not text:
        return []

    if text.startswith("["):
        data = json.loads(text)
        if not isinstance(data, list):
            raise ValueError("Arquivo JSON do MongoDB deve ser uma lista de eventos.")
        return [event for event in data if isinstance(event, dict)]

    events = []
    for line in text.splitlines():
        line = line.strip()
        if line:
            events.append(json.loads(line))
    return events


def parse_int(value: Any) -> int | None:
    if value is None:
        return None
    try:
        return int(str(value))
    except ValueError:
        return None


def event_entity(event: dict[str, Any]) -> dict[str, Any] | None:
    entity = event.get("entity") or event.get("Entity")
    return entity if isinstance(entity, dict) else None


def normalize_mongo_interactions(path: Path) -> pd.DataFrame:
    rows: list[dict[str, Any]] = []
    for event in read_json_events(path):
        entity = event_entity(event)
        if not entity:
            continue

        entity_type = entity.get("type") or entity.get("Type")
        if entity_type != "espetaculo":
            continue

        user_id = parse_int(event.get("userId") or event.get("UserId") or event.get("user_id"))
        item_id = parse_int(entity.get("id") or entity.get("Id"))
        event_type = str(event.get("eventType") or event.get("EventType") or "")
        weight = EVENT_WEIGHTS.get(event_type)

        if user_id is None or item_id is None or weight is None:
            continue

        rows.append(
            {
                "user_id": user_id,
                "item_id": item_id,
                "mongo_interaction_score": weight,
                "mongo_interaction_count": 1,
            }
        )

    if not rows:
        return pd.DataFrame(
            columns=["user_id", "item_id", "mongo_interaction_score", "mongo_interaction_count"]
        )

    return (
        pd.DataFrame(rows)
        .groupby(["user_id", "item_id"], as_index=False)
        .agg(
            mongo_interaction_score=("mongo_interaction_score", "sum"),
            mongo_interaction_count=("mongo_interaction_count", "sum"),
        )
    )


def load_relational_dataset(path: Path) -> pd.DataFrame:
    df = pd.read_csv(path)
    required = {"user_id", "item_id", "rating", "score_implicito"}
    missing = required.difference(df.columns)
    if missing:
        raise ValueError(f"Colunas ausentes no CSV relacional: {sorted(missing)}")

    df = df.dropna(subset=list(required)).copy()
    df["user_id"] = df["user_id"].astype(int)
    df["item_id"] = df["item_id"].astype(int)
    df["rating"] = df["rating"].astype(float)
    df["score_implicito"] = df["score_implicito"].astype(float)
    df[TARGET] = (df["rating"] >= 4).astype(int)
    return df


def merge_sources(postgres_csv: Path, mongo_json: Path) -> pd.DataFrame:
    base = load_relational_dataset(postgres_csv)
    mongo = normalize_mongo_interactions(mongo_json)

    merged = base.merge(mongo, on=["user_id", "item_id"], how="left")
    merged["mongo_interaction_score"] = merged["mongo_interaction_score"].fillna(0.0)
    merged["mongo_interaction_count"] = merged["mongo_interaction_count"].fillna(0.0)
    return merged


def build_svd(train: pd.DataFrame, components: int = 32) -> dict[str, Any]:
    users = sorted(train["user_id"].unique())
    items = sorted(train["item_id"].unique())
    user_index = {user_id: idx for idx, user_id in enumerate(users)}
    item_index = {item_id: idx for idx, item_id in enumerate(items)}

    rows = train["user_id"].map(user_index).to_numpy()
    cols = train["item_id"].map(item_index).to_numpy()
    values = train["rating"].to_numpy()
    matrix = csr_matrix((values, (rows, cols)), shape=(len(users), len(items)))

    n_components = min(components, max(2, min(matrix.shape) - 1))
    svd = TruncatedSVD(n_components=n_components, random_state=42)
    user_factors = svd.fit_transform(matrix)
    item_factors = svd.components_.T
    global_rating_mean = float(train["rating"].mean())

    return {
        "svd": svd,
        "user_index": user_index,
        "item_index": item_index,
        "user_factors": user_factors,
        "item_factors": item_factors,
        "global_rating_mean": global_rating_mean,
    }


def svd_score(row: pd.Series, svd_artifact: dict[str, Any]) -> float:
    user_pos = svd_artifact["user_index"].get(int(row["user_id"]))
    item_pos = svd_artifact["item_index"].get(int(row["item_id"]))
    if user_pos is None or item_pos is None:
        return svd_artifact["global_rating_mean"]

    score = float(np.dot(svd_artifact["user_factors"][user_pos], svd_artifact["item_factors"][item_pos]))
    return max(1.0, min(5.0, score))


def build_knn(svd_artifact: dict[str, Any]) -> dict[str, Any]:
    item_factors = svd_artifact["item_factors"]
    neighbors = min(6, len(item_factors))
    knn = NearestNeighbors(n_neighbors=neighbors, metric="cosine")
    knn.fit(item_factors)
    return {"knn": knn, "neighbors": neighbors}


def knn_item_score(row: pd.Series, svd_artifact: dict[str, Any], knn_artifact: dict[str, Any]) -> float:
    item_pos = svd_artifact["item_index"].get(int(row["item_id"]))
    if item_pos is None:
        return 0.0

    distances, _ = knn_artifact["knn"].kneighbors([svd_artifact["item_factors"][item_pos]])
    similarities = 1.0 - distances[0]
    if len(similarities) <= 1:
        return float(similarities[0])
    return float(np.mean(similarities[1:]))


def build_feature_stats(train: pd.DataFrame) -> dict[str, Any]:
    user_stats = train.groupby("user_id")["rating"].agg(["mean", "count"])
    item_stats = train.groupby("item_id")["rating"].agg(["mean", "count"])

    return {
        "global_rating_mean": float(train["rating"].mean()),
        "global_user_interactions": float(user_stats["count"].median()),
        "global_item_interactions": float(item_stats["count"].median()),
        "user_rating_mean": user_stats["mean"].to_dict(),
        "user_interactions": user_stats["count"].to_dict(),
        "item_rating_mean": item_stats["mean"].to_dict(),
        "item_interactions": item_stats["count"].to_dict(),
    }


def apply_features(
    df: pd.DataFrame,
    stats: dict[str, Any],
    svd_artifact: dict[str, Any],
    knn_artifact: dict[str, Any],
) -> pd.DataFrame:
    features = df[["user_id", "item_id", "score_implicito", "mongo_interaction_score", "mongo_interaction_count"]].copy()
    features["user_rating_mean"] = features["user_id"].map(stats["user_rating_mean"])
    features["user_interactions"] = features["user_id"].map(stats["user_interactions"])
    features["item_rating_mean"] = features["item_id"].map(stats["item_rating_mean"])
    features["item_interactions"] = features["item_id"].map(stats["item_interactions"])
    features["svd_score"] = df.apply(lambda row: svd_score(row, svd_artifact), axis=1)
    features["knn_item_score"] = df.apply(lambda row: knn_item_score(row, svd_artifact, knn_artifact), axis=1)

    features["user_rating_mean"] = features["user_rating_mean"].fillna(stats["global_rating_mean"])
    features["item_rating_mean"] = features["item_rating_mean"].fillna(stats["global_rating_mean"])
    features["user_interactions"] = features["user_interactions"].fillna(stats["global_user_interactions"])
    features["item_interactions"] = features["item_interactions"].fillna(stats["global_item_interactions"])
    return features[FEATURE_COLUMNS]


def evaluate(model: Any, x_test: pd.DataFrame, y_test: pd.Series) -> Metrics:
    predictions = model.predict(x_test)
    probabilities = model.predict_proba(x_test)[:, 1]
    return Metrics(
        accuracy=accuracy_score(y_test, predictions),
        precision=precision_score(y_test, predictions, zero_division=0),
        recall=recall_score(y_test, predictions, zero_division=0),
        f1=f1_score(y_test, predictions, zero_division=0),
        roc_auc=roc_auc_score(y_test, probabilities),
    )


def top_k_metrics(test: pd.DataFrame, probabilities: np.ndarray, k: int = 10) -> dict[str, float]:
    ranked = test[["user_id", TARGET]].copy()
    ranked["probabilidade"] = probabilities

    precisions: list[float] = []
    recalls: list[float] = []
    hits: list[float] = []

    for _, group in ranked.groupby("user_id"):
        relevant_total = int(group[TARGET].sum())
        if relevant_total == 0:
            continue

        top = group.sort_values("probabilidade", ascending=False).head(k)
        relevant_at_k = int(top[TARGET].sum())
        precisions.append(relevant_at_k / len(top))
        recalls.append(relevant_at_k / relevant_total)
        hits.append(1.0 if relevant_at_k > 0 else 0.0)

    if not precisions:
        return {f"precision_at_{k}": 0.0, f"recall_at_{k}": 0.0, f"hit_rate_at_{k}": 0.0}

    return {
        f"precision_at_{k}": float(np.mean(precisions)),
        f"recall_at_{k}": float(np.mean(recalls)),
        f"hit_rate_at_{k}": float(np.mean(hits)),
    }


def as_percent(value: float) -> str:
    return f"{value * 100:.2f}%"


def rel(path: Path) -> str:
    return str(path.relative_to(REPO_ROOT).as_posix())


def write_examples(test: pd.DataFrame, x_test: pd.DataFrame, model: Any) -> None:
    examples = test[["user_id", "item_id", "rating", "score_implicito", "mongo_interaction_score"]].copy()
    examples["probabilidade_recomendacao"] = model.predict_proba(x_test)[:, 1]
    examples["recomendar_previsto"] = model.predict(x_test)
    REPORTS_ROOT.mkdir(parents=True, exist_ok=True)
    examples.sort_values("probabilidade_recomendacao", ascending=False).head(20).to_csv(EXAMPLES_CSV, index=False)


def write_report(metrics: dict[str, Any]) -> None:
    baseline = metrics["baseline_classificacao_svd_features"]
    incrementado = metrics["modelo_hibrido_incrementado"]
    topk = metrics["top_k"]
    delta_f1 = incrementado["f1"] - baseline["f1"]
    delta_auc = incrementado["roc_auc"] - baseline["roc_auc"]

    REPORTS_ROOT.mkdir(parents=True, exist_ok=True)
    REPORT_MD.write_text(
        "\n".join(
            [
                "# Modelo de Classificação de Recomendação",
                "",
                "## Objetivo",
                "",
                "Evoluir o modelo de recomendação da Sprint 3 para uma abordagem híbrida, combinando Funk SVD, k-NN e uma camada de classificação binária.",
                "",
                "A tarefa classifica cada par usuário-espetáculo como:",
                "",
                "- `recomendar = 1`, quando `rating >= 4`.",
                "- `recomendar = 0`, quando `rating < 4`.",
                "",
                "## Fontes de dados",
                "",
                "- PostgreSQL: fonte relacional de usuários, espetáculos, avaliações e recomendações calculadas. Para treino offline, é representado por export CSV.",
                "- MongoDB: fonte de interações comportamentais e telemetria, como visualização, clique, favorito e compra. Para treino offline, é representado por export JSON/JSONL.",
                "",
                f"- CSV relacional: `{metrics['dataset']['postgres_csv']}`",
                f"- JSON/JSONL MongoDB: `{metrics['dataset']['mongo_json']}`",
                f"- Registros usados: `{metrics['dataset']['registros']}`",
                f"- Eventos MongoDB aproveitados no treino: `{metrics['dataset']['eventos_mongo_aproveitados']}`",
                f"- Taxa positiva: `{as_percent(metrics['dataset']['taxa_positiva'])}`",
                "",
                "## Incremento aplicado",
                "",
                "O Funk SVD original continua sendo a referência para previsão de rating em matriz esparsa, com RMSE aproximado de `1,18` e MAE aproximado de `0,97`, conforme documentado no notebook `src/models/sistema_recomendacao_svd.ipynb`.",
                "",
                "A versão anterior usava principalmente sinais explícitos/implícitos de rating. A versão incrementada adiciona um caminho híbrido:",
                "",
                "1. Funk SVD / fatoração matricial para lidar com matriz usuário-item esparsa.",
                "2. k-NN sobre fatores latentes dos itens para capturar similaridade item-item.",
                "3. Features de interação vindas do MongoDB para incorporar comportamento recente.",
                "4. Random Forest como camada de classificação/priorização final.",
                "",
                "Features usadas pelo classificador:",
                "",
                *[f"- `{feature}`" for feature in FEATURE_COLUMNS],
                "",
                "## Resultado",
                "",
                "| Modelo | Accuracy | Precision | Recall | F1 | ROC AUC |",
                "|---|---:|---:|---:|---:|---:|",
                f"| Baseline de classificação com feature SVD | {as_percent(baseline['accuracy'])} | {as_percent(baseline['precision'])} | {as_percent(baseline['recall'])} | {as_percent(baseline['f1'])} | {as_percent(baseline['roc_auc'])} |",
                f"| Híbrido SVD + k-NN + Mongo | {as_percent(incrementado['accuracy'])} | {as_percent(incrementado['precision'])} | {as_percent(incrementado['recall'])} | {as_percent(incrementado['f1'])} | {as_percent(incrementado['roc_auc'])} |",
                "",
                "## Resultado Top-K",
                "",
                "| Métrica | Valor |",
                "|---|---:|",
                f"| Precision@10 | {as_percent(topk['precision_at_10'])} |",
                f"| Recall@10 | {as_percent(topk['recall_at_10'])} |",
                f"| Hit Rate@10 | {as_percent(topk['hit_rate_at_10'])} |",
                "",
                "## Ganho observado",
                "",
                f"- Delta F1: `{delta_f1:+.4f}`",
                f"- Delta ROC AUC: `{delta_auc:+.4f}`",
                "",
                "## Exemplos validados",
                "",
                f"Os 20 pares usuário-item com maior probabilidade de recomendação foram exportados para `{metrics['artefatos']['exemplos']}`.",
                "",
                "A função de inferência offline foi adicionada em `src/models/scripts/inference_recomendacao.py` e carrega o modelo exportado para retornar top-N recomendações por usuário.",
                "",
                "## Limitações",
                "",
                "- O treino ainda usa exports offline; a leitura direta do MongoDB e PostgreSQL em runtime fica para a integração da API.",
                "- Eventos reais do MongoDB com `UserId` em UUID precisam de mapeamento para o identificador numérico usado no dataset histórico.",
                "- O `score_implicito` é muito correlacionado com `rating`, então o baseline já é alto. O principal avanço aqui é arquitetural e funcional: unificar SVD, k-NN e interações MongoDB em um pipeline reproduzível.",
                "- As métricas de classificação não substituem RMSE/MAE do Funk SVD. Elas complementam a avaliação para o problema de priorizar recomendações.",
                "- O modelo gerado ainda deve ser integrado ao serviço de recomendação na T26.",
                "",
                "## Artefatos gerados",
                "",
                f"- Modelo: `{metrics['artefatos']['modelo']}`",
                f"- Métricas JSON: `{metrics['artefatos']['metricas']}`",
                f"- Exemplos CSV: `{metrics['artefatos']['exemplos']}`",
                f"- Inferência: `{metrics['artefatos']['inferencia']}`",
                "",
                "## Como reproduzir",
                "",
                "```bash",
                "python src/models/scripts/incrementar_modelo_classificacao_recomendacao.py",
                "```",
                "",
                "Também é possível informar exports reais:",
                "",
                "```bash",
                "python src/models/scripts/incrementar_modelo_classificacao_recomendacao.py --postgres-csv caminho/recomendacao.csv --mongo-json caminho/interacoes.jsonl",
                "```",
                "",
                "Para executar inferência offline:",
                "",
                "```bash",
                "python src/models/scripts/inference_recomendacao.py --user-id 10 --top-n 5",
                "```",
                "",
            ]
        ),
        encoding="utf-8",
    )


def train(postgres_csv: Path, mongo_json: Path) -> dict[str, Any]:
    df = merge_sources(postgres_csv, mongo_json)
    train_df, test_df = train_test_split(
        df,
        test_size=0.2,
        random_state=42,
        stratify=df[TARGET],
    )

    stats = build_feature_stats(train_df)
    svd_artifact = build_svd(train_df)
    knn_artifact = build_knn(svd_artifact)
    x_train = apply_features(train_df, stats, svd_artifact, knn_artifact)
    x_test = apply_features(test_df, stats, svd_artifact, knn_artifact)
    y_train = train_df[TARGET]
    y_test = test_df[TARGET]

    baseline = RandomForestClassifier(
        n_estimators=80,
        max_depth=6,
        class_weight="balanced",
        random_state=42,
        n_jobs=-1,
    )
    baseline.fit(x_train[["svd_score"]], y_train)
    baseline_metrics = evaluate(baseline, x_test[["svd_score"]], y_test)

    classifier = RandomForestClassifier(
        n_estimators=180,
        max_depth=12,
        min_samples_leaf=5,
        class_weight="balanced",
        random_state=42,
        n_jobs=-1,
    )
    classifier.fit(x_train, y_train)
    incrementado_metrics = evaluate(classifier, x_test, y_test)
    probabilities = classifier.predict_proba(x_test)[:, 1]
    topk = top_k_metrics(test_df, probabilities, k=10)
    write_examples(test_df, x_test, classifier)

    artifact = {
        "classifier": classifier,
        "svd": svd_artifact["svd"],
        "knn": knn_artifact["knn"],
        "user_index": svd_artifact["user_index"],
        "item_index": svd_artifact["item_index"],
        "user_factors": svd_artifact["user_factors"],
        "item_factors": svd_artifact["item_factors"],
        "global_rating_mean": svd_artifact["global_rating_mean"],
        "feature_columns": FEATURE_COLUMNS,
        "feature_stats": stats,
        "target": TARGET,
        "positive_rule": "rating >= 4",
        "threshold": 0.5,
        "event_weights": EVENT_WEIGHTS,
    }
    ARTIFACTS_ROOT.mkdir(parents=True, exist_ok=True)
    joblib.dump(artifact, MODEL_OUT)

    mongo_events = normalize_mongo_interactions(mongo_json)
    metrics = {
        "dataset": {
            "postgres_csv": rel(postgres_csv),
            "mongo_json": rel(mongo_json) if mongo_json.exists() else "",
            "registros": int(len(df)),
            "eventos_mongo_aproveitados": int(mongo_events["mongo_interaction_count"].sum()) if not mongo_events.empty else 0,
            "taxa_positiva": float(df[TARGET].mean()),
            "train": int(len(train_df)),
            "test": int(len(test_df)),
        },
        "referencia_funk_svd_notebook": {
            "rmse_aproximado": 1.18,
            "mae_aproximado": 0.97,
            "fonte": "src/models/sistema_recomendacao_svd.ipynb",
        },
        "baseline_classificacao_svd_features": asdict(baseline_metrics),
        "modelo_hibrido_incrementado": asdict(incrementado_metrics),
        "top_k": topk,
        "features": FEATURE_COLUMNS,
        "artefatos": {
            "modelo": rel(MODEL_OUT),
            "metricas": rel(METRICS_JSON),
            "relatorio": rel(REPORT_MD),
            "exemplos": rel(EXAMPLES_CSV),
            "inferencia": "src/models/scripts/inference_recomendacao.py",
        },
    }
    METRICS_JSON.write_text(json.dumps(metrics, indent=2, ensure_ascii=False), encoding="utf-8")
    write_report(metrics)
    return metrics


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Treina modelo hibrido SVD + k-NN + MongoDB.")
    parser.add_argument("--postgres-csv", type=Path, default=DEFAULT_POSTGRES_CSV)
    parser.add_argument("--mongo-json", type=Path, default=DEFAULT_MONGO_JSONL)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    metrics = train(args.postgres_csv, args.mongo_json)
    print(json.dumps(metrics, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
