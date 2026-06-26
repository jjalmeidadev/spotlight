# RNF08 — Deploy independente

## Objetivo

Atualizar um serviço isoladamente, medir sua indisponibilidade e confirmar que os demais permanecem operacionais. O limite é de cinco minutos.

## Execução

O script inicia em modo seguro. Para executar o deploy real em ambiente autorizado, use `-Execute`:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf08/run-rnf08.ps1 `
  -Service catalogo `
  -Execute
```

## Arquivos

- `services.json`: serviços e health checks do Compose;
- `run-rnf08.ps1`: build, atualização isolada, monitoramento e relatório;
- `RESULTADOS.md`: critérios e limitações.

## Critério

- serviço alvo recuperado em até 300 segundos;
- demais serviços disponíveis durante a atualização;
- atualização com `--no-deps`;
- rollback documentado em caso de falha;
- gateway e frontend validados no deploy final pelo `docker-compose.prod.yml` e pelo roteiro de `documentos/manual_deploy.md`.
