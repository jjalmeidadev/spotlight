# RNF03 — Disponibilidade

## Objetivo

Monitorar endpoints críticos, calcular disponibilidade e registrar incidentes. O critério mensal é disponibilidade mínima de 99%.

## Arquivos

- `endpoints.example.json`: lista dos endpoints locais do Compose;
- `run-rnf03.ps1`: monitor com intervalo configurável;
- `simulate-outage.ps1`: indisponibilidade controlada de um serviço;
- `analyze-rnf03.py`: cálculo de uptime e incidentes;
- `RESULTADOS.md`: estado e critérios.

## Execução local curta

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf03/run-rnf03.ps1 `
  -DurationMinutes 30 `
  -IntervalSeconds 300
```

Para acompanhamento mensal, mantenha o monitor em ambiente de homologação pelo período efetivo e informe as datas reais.

## Indisponibilidade controlada

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf03/simulate-outage.ps1 `
  -Service geolocalizacao `
  -HealthUrl "http://localhost:5004/health" `
  -Execute
```

## Análise

```powershell
python src/backend/tests/non-functional/rnf03/analyze-rnf03.py `
  --input "CAMINHO_DO_CSV" `
  --output "CAMINHO_DO_RELATORIO.md"
```
