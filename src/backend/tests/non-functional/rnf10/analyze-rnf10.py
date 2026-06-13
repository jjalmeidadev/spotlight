#!/usr/bin/env python3
import argparse,csv
from collections import defaultdict
from pathlib import Path

def b(v): return v.strip().lower() in {'true','1','sim','yes','s'}

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--input',required=True); ap.add_argument('--output',required=True); ap.add_argument('--success-target',type=float,default=85); ap.add_argument('--max-touches',type=float,default=3); args=ap.parse_args()
    grouped=defaultdict(list)
    with Path(args.input).open(encoding='utf-8-sig',newline='') as f:
        for r in csv.DictReader(f):
            if r.get('participant_id') and r.get('task_id') and r.get('completed_without_help','').strip(): grouped[r['task_id']].append(r)
    lines=['# RNF10 — Relatório de usabilidade','', '| Tarefa | Participantes | Sucesso sem ajuda | Taxa de sucesso | Média de toques (sucessos) | Situação |','|---|---:|---:|---:|---:|---|']
    overall=True; all_rows=[]
    for task,rows in sorted(grouped.items()):
        successes=[r for r in rows if b(r['completed_without_help'])]; touches=[float(r['touches'].replace(',','.')) for r in successes if r.get('touches')]
        rate=len(successes)/len(rows)*100 if rows else 0; avg=sum(touches)/len(touches) if touches else 0
        status='Atendido' if len(rows)>=5 and rate>=args.success_target and touches and avg<=args.max_touches else 'Não atendido'
        overall &= status=='Atendido'; all_rows.extend(rows)
        lines.append(f'| {task} | {len(rows)} | {len(successes)} | {rate:.2f}% | {avg:.2f} | {status} |')
    participants=len({r['participant_id'] for r in all_rows}); completed=sum(b(r['completed_without_help']) for r in all_rows); total=len(all_rows); overall_rate=completed/total*100 if total else 0
    lines += ['', '## Consolidação','',f'- Participantes: **{participants}**',f'- Execuções registradas: **{total}**',f'- Taxa geral de sucesso: **{overall_rate:.2f}%**',f'- Veredito: **{"Aprovado" if overall and participants>=5 else "Não aprovado ou amostra incompleta"}**']
    out=Path(args.output); out.parent.mkdir(parents=True,exist_ok=True); out.write_text('\n'.join(lines),encoding='utf-8'); print(out)
if __name__=='__main__': main()
