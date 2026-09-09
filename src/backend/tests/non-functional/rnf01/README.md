# RNF01 — Desempenho das telas

## Objetivo

Verificar se as telas principais carregam em até 2 segundos no cenário mobile de referência, com cinco execuções por tela.

## Estratégia

A validação combina:

1. Lighthouse em modo mobile para a versão Expo Web;
2. planilha de medições percebidas em Android/iOS;
3. análise automática de média, mínimo, máximo, p95 e percentual aprovado.

O aplicativo atual utiliza navegação por estado em `App.js`, portanto o Lighthouse mede a entrada web disponível. As demais telas devem ser registradas na planilha durante a navegação real.

## Arquivos

| Arquivo | Função |
|---|---|
| `run-rnf01.ps1` | Executa cinco rodadas do Lighthouse em modo mobile |
| `analyze-rnf01.py` | Consolida a planilha de medições e gera relatório |
| `measurements.template.csv` | Modelo de registro das execuções |
| `RESULTADOS.md` | Estado, critérios e evidências do RNF |

## Execução do Lighthouse

Com o Expo Web em execução:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf01/run-rnf01.ps1 `
  -BaseUrl "http://localhost:8081" `
  -Runs 5
```

O script utiliza `npx lighthouse` e grava os relatórios em `.results/rnf01`.

## Medições das telas

Copie `measurements.template.csv` para um arquivo local e preencha uma linha por execução. Cada tela deve possuir cinco execuções.

Colunas principais:

- `screen`: tela avaliada;
- `run`: número da execução;
- `platform`: `web`, `android` ou `ios`;
- `network`: condição de rede;
- `duration_ms`: tempo total percebido;
- `loaded`: `true` ou `false`;
- `loading_feedback`: presença de spinner ou feedback;
- `notes`: observações.

## Análise

```powershell
python src/backend/tests/non-functional/rnf01/analyze-rnf01.py `
  --input "CAMINHO_DAS_MEDICOES.csv" `
  --output "src/backend/tests/non-functional/.results/rnf01/relatorio.md"
```

## Critério

- cinco execuções por tela;
- 100% das execuções concluídas em até 2.000 ms;
- feedback visual registrado no cenário degradado.
