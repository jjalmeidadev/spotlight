# RNF04 — Mobile first e áreas de toque

## Objetivo

Auditar se 100% dos elementos interativos possuem área mínima de 44×44 unidades de layout e funcionam em Android e iOS.

## Arquivos

- `run-static-touch-audit.ps1`: busca dimensões explicitamente menores que 44 no frontend;
- `touch-target-checklist.csv`: checklist de telas e componentes reais;
- `analyze-rnf04.py`: calcula conformidade geral e por plataforma;
- `RESULTADOS.md`: achados estáticos e estado da validação física.

## Auditoria estática

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf04/run-static-touch-audit.ps1
```

## Validação física

Preencha o checklist com largura, altura, funcionamento Android/iOS e evidência. Depois execute:

```powershell
python src/backend/tests/non-functional/rnf04/analyze-rnf04.py `
  --input src/backend/tests/non-functional/rnf04/touch-target-checklist.csv `
  --output src/backend/tests/non-functional/.results/rnf04/relatorio.md
```

## Critério

- largura mínima: 44;
- altura mínima: 44;
- funcionamento em Android e iOS;
- conformidade exigida: 100%.
