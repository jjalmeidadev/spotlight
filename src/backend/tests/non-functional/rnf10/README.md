# RNF10 — Usabilidade

## Objetivo

Validar taxa de sucesso mínima de 85% e até três toques nas tarefas concluídas, com pelo menos cinco participantes.

## Arquivos

- `tasks.csv`: roteiro T01–T09;
- `sessions.template.csv`: planilha de execução;
- `analyze-rnf10.py`: consolidação por tarefa e geral;
- `RESULTADOS.md`: critérios e estado.

## Uso

1. Aplique as tarefas a pelo menos cinco participantes externos.
2. Registre uma linha por participante e tarefa.
3. Informe conclusão sem ajuda, número de toques, tempo e observações.
4. Execute o analisador.

```powershell
python src/backend/tests/non-functional/rnf10/analyze-rnf10.py `
  --input "PLANILHA_PREENCHIDA.csv" `
  --output "src/backend/tests/non-functional/.results/rnf10/relatorio.md"
```

## Privacidade

Utilize identificadores anônimos como `P01`. Não registre nome, e-mail, telefone ou outro dado pessoal.
