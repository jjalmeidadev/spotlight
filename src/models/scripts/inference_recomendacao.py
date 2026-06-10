"""Inferencia offline para o modelo hibrido de recomendacao.

Carrega o artefato exportado em `artifacts/` e retorna top-N recomendacoes
para um usuario usando candidatos vindos do CSV relacional e, opcionalmente,
interacoes exportadas do MongoDB em JSON/JSONL.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any

import joblib
import pandas as pd

from incrementar_modelo_classificacao_recomendacao import (
    DEFAULT_MONGO_JSONL,
    DEFAULT_POSTGRES_CSV,
    FEATURE_COLUMNS,
    MODELS_ROOT,
    apply_features,
    normalize_mongo_interactions,
)


DEFAULT_MODEL = MODELS_ROOT / "artifacts" / "recomendacao_hibrida_svd_knn_incrementado.pkl"


def load_candidates(postgres_csv: Path, mongo_json: Path, user_id: int) -> pd.DataFrame:
    base = pd.read_csv(postgres_csv)
    base["user_id"] = base["user_id"].astype(int)
    base["item_id"] = base["item_id"].astype(int)

    user_rows = base[base["user_id"] == user_id].copy()
    if user_rows.empty:
        item_defaults = (
            base.groupby("item_id", as_index=False)
            .agg(
                rating=("rating", "mean"),
                score_implicito=("score_implicito", "mean"),
            )
            .assign(user_id=user_id)
        )
        user_rows = item_defaults[["user_id", "item_id", "rating", "score_implicito"]]

    mongo = normalize_mongo_interactions(mongo_json)
    merged = user_rows.merge(mongo, on=["user_id", "item_id"], how="left")
    merged["mongo_interaction_score"] = merged["mongo_interaction_score"].fillna(0.0)
    merged["mongo_interaction_count"] = merged["mongo_interaction_count"].fillna(0.0)
    return merged


def build_artifact_views(artifact: dict[str, Any]) -> tuple[dict[str, Any], dict[str, Any]]:
    svd_artifact = {
        "user_index": artifact["user_index"],
        "item_index": artifact["item_index"],
        "user_factors": artifact["user_factors"],
        "item_factors": artifact["item_factors"],
        "global_rating_mean": artifact.get("global_rating_mean", 3.0),
    }
    knn_artifact = {"knn": artifact["knn"]}
    return svd_artifact, knn_artifact


def recomendar(
    user_id: int,
    top_n: int,
    model_path: Path = DEFAULT_MODEL,
    postgres_csv: Path = DEFAULT_POSTGRES_CSV,
    mongo_json: Path = DEFAULT_MONGO_JSONL,
) -> list[dict[str, Any]]:
    artifact = joblib.load(model_path)
    candidates = load_candidates(postgres_csv, mongo_json, user_id)
    svd_artifact, knn_artifact = build_artifact_views(artifact)
    features = apply_features(candidates, artifact["feature_stats"], svd_artifact, knn_artifact)

    probabilities = artifact["classifier"].predict_proba(features[FEATURE_COLUMNS])[:, 1]
    output = candidates[["user_id", "item_id", "score_implicito", "mongo_interaction_score"]].copy()
    output["probabilidade_recomendacao"] = probabilities
    output = output.sort_values("probabilidade_recomendacao", ascending=False).head(top_n)
    return output.to_dict(orient="records")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Gera top-N recomendacoes com o modelo hibrido.")
    parser.add_argument("--user-id", type=int, required=True)
    parser.add_argument("--top-n", type=int, default=10)
    parser.add_argument("--model-path", type=Path, default=DEFAULT_MODEL)
    parser.add_argument("--postgres-csv", type=Path, default=DEFAULT_POSTGRES_CSV)
    parser.add_argument("--mongo-json", type=Path, default=DEFAULT_MONGO_JSONL)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    recomendacoes = recomendar(
        user_id=args.user_id,
        top_n=args.top_n,
        model_path=args.model_path,
        postgres_csv=args.postgres_csv,
        mongo_json=args.mongo_json,
    )
    print(json.dumps({"user_id": args.user_id, "data": recomendacoes}, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
