#!/usr/bin/env python3
import argparse, csv, math
from collections import defaultdict
from pathlib import Path


def p95(values):
    values = sorted(values)
    if not values: return 0
    rank = 0.95 * (len(values)-1)
    lo, hi = math.floor(rank), math.ceil(rank)
    return values[lo] if lo == hi else values[lo] + (values[hi]-values[lo])*(rank-lo)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--input', required=True)
    ap.add_argument('--output', required=True)
    ap.add_argument('--limit-ms', type=float, default=2000)
    args = ap.parse_args()
    grouped = defaultdict(list)
    with Path(args.input).open(encoding='utf-8-sig', newline='') as f:
        for row in csv.DictReader(f):
            if row.get('profile') and row.get('perceived_duration_ms'):
                grouped[row['profile']].append(float(row['perceived_duration_ms'].replace(',', '.')))
    lines = [
        '# RNF02 — Relatório de latência percebida', '',
        '| Perfil | Amostra | Mínimo | Média | Máximo | p95 | Dentro do limite | Situação |',
        '|---|---:|---:|---:|---:|---:|---:|---|'
    ]
    overall = len(grouped) == 3
    total = 0
    for profile, values in sorted(grouped.items()):
        passed = sum(v <= args.limit_ms for v in values)
        status = 'Atendido' if len(values) == 10 and passed == len(values) else 'Não atendido'
        overall = overall and status == 'Atendido'
        total += len(values)
        lines.append(f'| {profile} | {len(values)} | {min(values):.2f} ms | {sum(values)/len(values):.2f} ms | {max(values):.2f} ms | {p95(values):.2f} ms | {passed}/{len(values)} | {status} |')
    lines += ['', f'- Medições válidas: **{total}/30**', f'- Veredito: **{"Aprovado" if overall else "Não aprovado ou amostra incompleta"}**']
    out = Path(args.output); out.parent.mkdir(parents=True, exist_ok=True); out.write_text('\n'.join(lines), encoding='utf-8'); print(out)

if __name__ == '__main__': main()
