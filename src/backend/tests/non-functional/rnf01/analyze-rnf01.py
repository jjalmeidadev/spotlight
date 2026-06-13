#!/usr/bin/env python3
import argparse
import csv
import math
from collections import defaultdict
from pathlib import Path


def parse_bool(value: str) -> bool:
    return value.strip().lower() in {"true", "1", "sim", "yes", "s"}


def percentile(values, percentile_value):
    ordered = sorted(values)
    if not ordered:
        return 0.0
    rank = (len(ordered) - 1) * percentile_value
    lower = math.floor(rank)
    upper = math.ceil(rank)
    if lower == upper:
        return ordered[lower]
    return ordered[lower] + (ordered[upper] - ordered[lower]) * (rank - lower)


def main():
    parser = argparse.ArgumentParser(description="Analisa as medições do RNF01")
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--limit-ms", type=float, default=2000)
    args = parser.parse_args()

    input_path = Path(args.input)
    output_path = Path(args.output)
    if not input_path.exists():
        raise SystemExit(f"Arquivo não encontrado: {input_path}")

    grouped = defaultdict(list)
    with input_path.open(encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            if not row.get("screen") or not row.get("duration_ms"):
                continue
            duration = float(row["duration_ms"].replace(",", "."))
            grouped[row["screen"].strip()].append({
                "duration": duration,
                "loaded": parse_bool(row.get("loaded", "false")),
                "feedback": row.get("loading_feedback", "").strip(),
                "platform": row.get("platform", "").strip(),
            })

    lines = [
        "# RNF01 — Relatório de desempenho das telas",
        "",
        f"Critério: 100% das execuções em até {args.limit_ms:.0f} ms.",
        "",
        "| Tela | Amostra | Mínimo | Média | Máximo | p95 | Aprovadas | Situação |",
        "|---|---:|---:|---:|---:|---:|---:|---|",
    ]

    overall_ok = bool(grouped)
    total = approved = 0
    for screen, rows in sorted(grouped.items()):
        values = [item["duration"] for item in rows]
        passed = sum(1 for value in values if value <= args.limit_ms)
        total += len(values)
        approved += passed
        status = "Atendido" if len(values) >= 5 and passed == len(values) else "Não atendido"
        overall_ok = overall_ok and status == "Atendido"
        lines.append(
            f"| {screen} | {len(values)} | {min(values):.2f} ms | "
            f"{sum(values)/len(values):.2f} ms | {max(values):.2f} ms | "
            f"{percentile(values, 0.95):.2f} ms | {passed}/{len(values)} | {status} |"
        )

    lines += [
        "",
        "## Consolidação",
        "",
        f"- Execuções válidas: **{total}**",
        f"- Execuções dentro do limite: **{approved}**",
        f"- Percentual dentro do limite: **{(approved / total * 100 if total else 0):.2f}%**",
        f"- Veredito: **{'Aprovado' if overall_ok else 'Não aprovado ou amostra incompleta'}**",
        "",
    ]

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text("\n".join(lines), encoding="utf-8")
    print(output_path)


if __name__ == "__main__":
    main()
