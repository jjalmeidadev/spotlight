# RNF11 — Compatibilidade Android e iOS

## Objetivo

Validar os fluxos principais em Android 10+ e iOS 15+, com dois dispositivos por plataforma e taxa de erro de até 1%.

## Amostra planejada

A matriz contém:

- dois dispositivos Android;
- dois dispositivos iOS;
- dez fluxos;
- cinco repetições por fluxo e dispositivo;
- 200 execuções no total;
- 100 execuções por plataforma.

Essa amostra permite calcular a taxa de erro por plataforma em passos de 1%.

## Arquivos

- `devices.template.csv`: cadastro dos quatro dispositivos;
- `executions.template.csv`: 200 execuções planejadas;
- `analyze-rnf11.py`: taxa de erro por plataforma, dispositivo e fluxo;
- `RESULTADOS.md`: critérios e estado.

## Análise

```powershell
python src/backend/tests/non-functional/rnf11/analyze-rnf11.py `
  --devices src/backend/tests/non-functional/rnf11/devices.template.csv `
  --executions "EXECUCOES_PREENCHIDAS.csv" `
  --output src/backend/tests/non-functional/.results/rnf11/relatorio.md
```
