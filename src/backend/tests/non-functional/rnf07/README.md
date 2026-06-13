# RNF07 — Logs estruturados

## Objetivo

Validar que os eventos geram logs em até 1 segundo e que cada registro contém timestamp, nível, serviço, sessão/correlation ID e dispositivo.

## Arquivos

- `run-rnf07.ps1`: dispara eventos e coleta logs do Docker;
- `validate-logs.py`: valida campos, latência e exposição de dados sensíveis;
- `events.example.json`: eventos HTTP de teste;
- `RESULTADOS.md`: auditoria estática e estado.

## Execução

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf07/run-rnf07.ps1
```

Depois:

```powershell
python src/backend/tests/non-functional/rnf07/validate-logs.py `
  --logs "CAMINHO_DO_ARQUIVO_LOGS" `
  --events "CAMINHO_DO_ARQUIVO_EVENTOS" `
  --output "CAMINHO_DO_RELATORIO.md"
```

## Critérios

- emissão em até 1.000 ms;
- 100% dos logs com cinco campos;
- ausência de token, senha, connection string ou segredo;
- registro de eventos válidos, inválidos e falhas controladas.
