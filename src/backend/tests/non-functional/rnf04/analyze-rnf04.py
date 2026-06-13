#!/usr/bin/env python3
import argparse,csv
from pathlib import Path

def b(v): return v.strip().lower() in {'true','1','sim','yes','s'}
def n(v):
    try:return float(v.replace(',','.'))
    except:return None

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--input',required=True); ap.add_argument('--output',required=True); args=ap.parse_args()
    rows=[]
    with Path(args.input).open(encoding='utf-8-sig',newline='') as f: rows=list(csv.DictReader(f))
    assessed=[]
    for r in rows:
        w,h=n(r.get('width','')),n(r.get('height',''))
        complete=bool(w is not None and h is not None and r.get('android_ok','').strip() and r.get('ios_ok','').strip())
        compliant=complete and w>=44 and h>=44 and b(r['android_ok']) and b(r['ios_ok'])
        assessed.append((r,complete,compliant))
    complete=sum(x[1] for x in assessed); compliant=sum(x[2] for x in assessed)
    lines=['# RNF04 — Relatório de áreas de toque','',f'- Elementos mapeados: **{len(rows)}**',f'- Elementos completamente avaliados: **{complete}**',f'- Elementos conformes: **{compliant}**',f'- Conformidade sobre avaliados: **{(compliant/complete*100 if complete else 0):.2f}%**','', '| Tela | Elemento | Dimensão | Android | iOS | Situação |','|---|---|---:|---|---|---|']
    for r,done,ok in assessed:
        dim=f"{r.get('width') or '?'}×{r.get('height') or '?'}"
        status='Conforme' if ok else ('Não conforme' if done else 'Pendente')
        lines.append(f"| {r['screen']} | {r['element']} | {dim} | {r.get('android_ok') or '-'} | {r.get('ios_ok') or '-'} | {status} |")
    final='Aprovado' if complete==len(rows) and compliant==len(rows) else 'Não aprovado ou avaliação incompleta'
    lines += ['',f'**Veredito: {final}.**']
    out=Path(args.output); out.parent.mkdir(parents=True,exist_ok=True); out.write_text('\n'.join(lines),encoding='utf-8'); print(out)
if __name__=='__main__': main()
