#!/usr/bin/env python3
import argparse,csv,re
from datetime import datetime
from pathlib import Path

TIMESTAMP_RE=re.compile(r'\d{2}:\d{2}:\d{2}|\d{4}-\d{2}-\d{2}T')
LEVEL_RE=re.compile(r'\b(INF|DBG|WRN|ERR|FTL|Information|Debug|Warning|Error|Fatal)\b',re.I)
SECRET_RE=re.compile(r'(bearer\s+[A-Za-z0-9._-]+|password\s*[=:]|connection[_ ]?string\s*[=:]|supabase[_ ]?(jwt|service)[_ ]?(secret|key)\s*[=:])',re.I)

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--logs',required=True); ap.add_argument('--events',required=True); ap.add_argument('--output',required=True); args=ap.parse_args()
    log_text=Path(args.logs).read_text(encoding='utf-8',errors='replace'); log_lines=[l for l in log_text.splitlines() if l.strip()]
    with Path(args.events).open(encoding='utf-8-sig',newline='') as f: events=list(csv.DictReader(f))
    results=[]
    for e in events:
        cid=e['event_id']; matching=[line for line in log_lines if cid in line]
        complete=[]
        for line in matching:
            fields={
                'timestamp':bool(TIMESTAMP_RE.search(line)),
                'level':bool(LEVEL_RE.search(line)),
                'service':e['service'].lower() in line.lower(),
                'session':'rnf07-session' in line,
                'device':'rnf07-windows-powershell' in line,
            }
            complete.append(all(fields.values()))
        results.append((e,matching,complete))
    total_logs=sum(len(x[1]) for x in results); complete_logs=sum(sum(x[2]) for x in results)
    exposed=SECRET_RE.findall(log_text)
    lines=['# RNF07 — Relatório de logs estruturados','', '| Evento | Serviço | Logs correlacionados | Logs completos | Situação |','|---|---|---:|---:|---|']
    for e,m,c in results:
        status='Atendido' if m and all(c) else 'Não atendido'
        lines.append(f"| {e['name']} | {e['service']} | {len(m)} | {sum(c)} | {status} |")
    pct=complete_logs/total_logs*100 if total_logs else 0
    lines += ['',f'- Logs correlacionados: **{total_logs}**',f'- Logs com cinco campos: **{complete_logs} ({pct:.2f}%)**',f'- Padrões de segredo detectados: **{len(exposed)}**','', '**Critério:** 100% dos registros com os cinco campos e emissão em até 1 segundo.']
    if total_logs==0: lines += ['', '> Nenhum correlation ID foi encontrado nos logs. Isso indica ausência de propagação/enriquecimento dos cabeçalhos de correlação.']
    out=Path(args.output); out.parent.mkdir(parents=True,exist_ok=True); out.write_text('\n'.join(lines),encoding='utf-8'); print(out)
if __name__=='__main__': main()
