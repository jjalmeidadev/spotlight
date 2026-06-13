#!/usr/bin/env python3
import argparse, csv
from collections import defaultdict
from datetime import datetime
from pathlib import Path


def parse_bool(v): return v.strip().lower() in {'true','1','sim','yes'}
def parse_dt(v): return datetime.fromisoformat(v.replace('Z', '+00:00'))


def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--input',required=True); ap.add_argument('--output',required=True); ap.add_argument('--target',type=float,default=99.0); args=ap.parse_args()
    grouped=defaultdict(list)
    with Path(args.input).open(encoding='utf-8-sig',newline='') as f:
        for row in csv.DictReader(f): grouped[row['endpoint']].append(row)
    lines=['# RNF03 — Relatório de disponibilidade','', '| Endpoint | Verificações | Sucessos | Falhas | Disponibilidade | Situação |','|---|---:|---:|---:|---:|---|']
    overall=True
    for name, rows in sorted(grouped.items()):
        ok=sum(parse_bool(r['success']) for r in rows); total=len(rows); pct=ok/total*100 if total else 0; status='Atendido' if pct>=args.target else 'Não atendido'; overall &= status=='Atendido'
        lines.append(f'| {name} | {total} | {ok} | {total-ok} | {pct:.4f}% | {status} |')
    lines += ['', '## Incidentes', '']
    incident_count=0
    for name, rows in sorted(grouped.items()):
        rows=sorted(rows,key=lambda r:r['timestamp_utc']); start=None; last=None
        for row in rows:
            ts=parse_dt(row['timestamp_utc']); success=parse_bool(row['success'])
            if not success and start is None: start=ts
            if success and start is not None:
                duration=(ts-start).total_seconds(); incident_count+=1; lines.append(f'- **{name}**: {start.isoformat()} até {ts.isoformat()} — {duration:.0f} s.'); start=None
            last=ts
        if start is not None and last is not None: incident_count+=1; lines.append(f'- **{name}**: incidente aberto desde {start.isoformat()}.')
    if incident_count==0: lines.append('- Nenhum incidente identificado na amostra.')
    lines += ['', f'**Veredito da amostra: {"Aprovado" if overall else "Não aprovado"}.**', '', '> O período efetivamente monitorado deve ser informado na documentação final.']
    out=Path(args.output); out.parent.mkdir(parents=True,exist_ok=True); out.write_text('\n'.join(lines),encoding='utf-8'); print(out)
if __name__=='__main__': main()
