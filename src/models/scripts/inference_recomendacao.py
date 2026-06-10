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


def attach_mongo_features(candidates: pd.DataFrame, mongo_json: Path) -> pd.DataFrame:
    mongo = normalize_mongo_interactions(mongo_json)
    merged = candidates.merge(mongo, on=["user_id", "item_id"], how="left")
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


def expandir_candidatos_por_knn(
    candidatos: pd.DataFrame,
    catalogo_itens: pd.DataFrame,
    artifact: dict[str, Any],
    user_id: int,
    quantidade_base: int,
    vizinhos_por_item: int,
) -> pd.DataFrame:
    if candidatos.empty or vizinhos_por_item <= 0:
        return candidatos

    item_index = artifact["item_index"]
    index_item = {idx: item_id for item_id, idx in item_index.items()}
    item_factors = artifact["item_factors"]
    knn = artifact["knn"]
    candidatos_base = candidatos.sort_values("score_implicito", ascending=False).head(quantidade_base)
    item_defaults = catalogo_itens.groupby("item_id", as_index=False).agg(
        rating=("rating", "mean"),
        score_implicito=("score_implicito", "mean"),
    )

    novos: list[dict[str, Any]] = []
    for item_id in candidatos_base["item_id"].astype(int):
        item_pos = item_index.get(item_id)
        if item_pos is None:
            continue

        neighbors = min(vizinhos_por_item + 1, len(item_factors))
        distances, indices = knn.kneighbors([item_factors[item_pos]], n_neighbors=neighbors)
        for distance, neighbor_pos in zip(distances[0], indices[0]):
            similar_id = index_item.get(int(neighbor_pos))
            if similar_id is None or similar_id == item_id:
                continue

            similar_default = item_defaults[item_defaults["item_id"] == similar_id]
            if similar_default.empty:
                continue

            novos.append(
                {
                    "user_id": user_id,
                    "item_id": int(similar_id),
                    "rating": float(similar_default.iloc[0]["rating"]),
                    "score_implicito": float(similar_default.iloc[0]["score_implicito"]),
                    "origem_candidato": "knn",
                    "similaridade_knn": float(1.0 - distance),
                }
            )

    enriquecidos = candidatos.copy()
    enriquecidos["origem_candidato"] = "historico"
    enriquecidos["similaridade_knn"] = 1.0
    if novos:
        enriquecidos = pd.concat([enriquecidos, pd.DataFrame(novos)], ignore_index=True)

    return (
        enriquecidos.sort_values(["origem_candidato", "score_implicito"], ascending=[True, False])
        .drop_duplicates(subset=["user_id", "item_id"], keep="first")
        .reset_index(drop=True)
    )


def recomendar(
    user_id: int,
    top_n: int,
    expandir_knn: bool = True,
    quantidade_base_knn: int = 5,
    vizinhos_por_item: int = 5,
    model_path: Path = DEFAULT_MODEL,
    postgres_csv: Path = DEFAULT_POSTGRES_CSV,
    mongo_json: Path = DEFAULT_MONGO_JSONL,
) -> list[dict[str, Any]]:
    artifact = joblib.load(model_path)
    candidates = load_candidates(postgres_csv, mongo_json, user_id)
    if expandir_knn:
        catalogo_itens = pd.read_csv(postgres_csv)
        catalogo_itens["item_id"] = catalogo_itens["item_id"].astype(int)
        candidates = expandir_candidatos_por_knn(
            candidates,
            catalogo_itens,
            artifact,
            user_id=user_id,
            quantidade_base=quantidade_base_knn,
            vizinhos_por_item=vizinhos_por_item,
        )
        candidates = attach_mongo_features(
            candidates.drop(columns=["mongo_interaction_score", "mongo_interaction_count"], errors="ignore"),
            mongo_json,
        )
    svd_artifact, knn_artifact = build_artifact_views(artifact)
    features = apply_features(candidates, artifact["feature_stats"], svd_artifact, knn_artifact)

    probabilities = artifact["classifier"].predict_proba(features[FEATURE_COLUMNS])[:, 1]
    output_columns = ["user_id", "item_id", "score_implicito", "mongo_interaction_score"]
    if "origem_candidato" in candidates.columns:
        output_columns.extend(["origem_candidato", "similaridade_knn"])
    output = candidates[output_columns].copy()
    output["probabilidade_recomendacao"] = probabilities
    output = output.sort_values("probabilidade_recomendacao", ascending=False).head(top_n)
    return output.to_dict(orient="records")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Gera top-N recomendacoes com o modelo hibrido.")
    parser.add_argument("--user-id", type=int, required=True)
    parser.add_argument("--top-n", type=int, default=10)
    parser.add_argument("--sem-expansao-knn", action="store_true")
    parser.add_argument("--base-knn", type=int, default=5)
    parser.add_argument("--vizinhos-por-item", type=int, default=5)
    parser.add_argument("--model-path", type=Path, default=DEFAULT_MODEL)
    parser.add_argument("--postgres-csv", type=Path, default=DEFAULT_POSTGRES_CSV)
    parser.add_argument("--mongo-json", type=Path, default=DEFAULT_MONGO_JSONL)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    recomendacoes = recomendar(
        user_id=args.user_id,
        top_n=args.top_n,
        expandir_knn=not args.sem_expansao_knn,
        quantidade_base_knn=args.base_knn,
        vizinhos_por_item=args.vizinhos_por_item,
        model_path=args.model_path,
        postgres_csv=args.postgres_csv,
        mongo_json=args.mongo_json,
    )
    print(json.dumps({"user_id": args.user_id, "data": recomendacoes}, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
