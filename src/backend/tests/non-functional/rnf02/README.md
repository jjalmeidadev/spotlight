# RNF02 — Latência das recomendações

## Objetivo

Medir o intervalo entre a solicitação e o recebimento das recomendações para três perfis, com dez execuções por perfil, exigindo até 2 segundos em 100% das medições.

## Arquivos

| Arquivo | Função |
|---|---|
| `recommendation-latency.js` | Executa 30 medições HTTP no k6 |
| `run-rnf02.ps1` | Carrega variáveis e executa o k6 via Docker |
| `.env.rnf02.example` | Exemplo de configuração sem segredos |
| `interface-measurements.template.csv` | Registro do tempo percebido na interface |
| `analyze-rnf02.py` | Consolida os tempos percebidos |
| `RESULTADOS.md` | Estado e critérios |

## Configuração

Copie `.env.rnf02.example` para `.env.rnf02` e preencha três tokens válidos. O arquivo local não deve ser versionado.

## Execução

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf02/run-rnf02.ps1
```

O teste usa três VUs, um por perfil, e dez iterações por VU.

## Critérios

- 30 medições válidas;
- 10 por perfil;
- 100% em até 2.000 ms;
- resposta HTTP 2xx;
- conteúdo retornado;
- spinner ou feedback visual registrado quando houver atraso.
