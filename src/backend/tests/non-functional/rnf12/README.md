# RNF12 — Containers e recuperação

## Objetivo

Validar containerização, isolamento e retorno ao health check em até 30 segundos após reinicialização controlada.

## Arquivos

- `audit-containers.ps1`: verifica Dockerfiles, Compose, health checks, restart policy e rede;
- `services.json`: serviços containerizados e URLs;
- `run-rnf12.ps1`: reinicia cada serviço e mede recuperação;
- `RESULTADOS.md`: auditoria estática e limitações.

## Auditoria

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf12/audit-containers.ps1
```

## Recuperação

O script inicia em modo seguro. Para executar:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf12/run-rnf12.ps1 -Execute
```

## Critérios

- recuperação do health check em até 30 segundos;
- demais serviços continuam disponíveis;
- isolamento pela rede Docker;
- 100% dos serviços do escopo com Dockerfile e Compose;
- configuração inválida registrada apenas em ambiente controlado.
