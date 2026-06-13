#!/usr/bin/env python3
import argparse,csv
from collections import defaultdict
from pathlib import Path

def b(v): return v.strip().lower() in {'true','1','sim','yes','s'}

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--devices',required=True); ap.add_argument('--executions',required=True); ap.add_argument('--output',required=True); ap.add_argument('--max-error-rate',type=float,default=1.0); args=ap.parse_args()
    with Path(args.devices).open(encoding='utf-8-sig',newline='') as f: devices={r['device_id']:r for r in csv.DictReader(f)}
    rows=[]
    with Path(args.executions).open(encoding='utf-8-sig',newline='') as f:
        for r in csv.DictReader(f):
            if r.get('success','').strip(): rows.append(r)
    by_platform=defaultdict(list); by_device=defaultdict(list); by_flow=defaultdict(list)
    for r in rows:
        platform=devices.get(r['device_id'],{}).get('platform','desconhecido'); by_platform[platform].append(r); by_device[r['device_id']].append(r); by_flow[r['flow_id']].append(r)
    def failed(r): return (not b(r['success'])) or b(r.get('crash','false')) or b(r.get('visual_failure','false')) or int(r.get('error_count') or 0)>0
    lines=['# RNF11 — Relatório de compatibilidade','', '## Por plataforma','', '| Plataforma | Execuções | Falhas | Taxa de erro | Situação |','|---|---:|---:|---:|---|']
    overall=len(by_platform)>=2
    for key,items in sorted(by_platform.items()):
        errors=sum(failed(r) for r in items); rate=errors/len(items)*100 if items else 0; status='Atendido' if len(items)>=100 and rate<=args.max_error_rate else 'Não atendido'; overall &= status=='Atendido'; lines.append(f'| {key} | {len(items)} | {errors} | {rate:.2f}% | {status} |')
    lines += ['', '## Por dispositivo','', '| Dispositivo | Plataforma | Execuções | Falhas | Taxa |','|---|---|---:|---:|---:|']
    for key,items in sorted(by_device.items()):
        errors=sum(failed(r) for r in items); rate=errors/len(items)*100 if items else 0; lines.append(f"| {key} | {devices.get(key,{}).get('platform','?')} | {len(items)} | {errors} | {rate:.2f}% |")
    lines += ['',f'**Veredito: {"Aprovado" if overall and len(rows)==200 else "Não aprovado ou amostra incompleta"}.**',f' Execuções preenchidas: {len(rows)}/200.']
    out=Path(args.output); out.parent.mkdir(parents=True,exist_ok=True); out.write_text('\n'.join(lines),encoding='utf-8'); print(out)
if __name__=='__main__': main()
