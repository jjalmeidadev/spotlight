# RNF06 — Resultados de carga e escalabilidade

## Objetivo

Avaliar a capacidade da solução Spotlight de processar operações concorrentes de catálogo, recomendação e persistência, observando latência, throughput, falhas e estabilidade do ambiente.

## Ambiente avaliado

| Item                     | Configuração                      |
| ------------------------ | --------------------------------- |
| Data da execução         | 09/06/2026 às 16:45:59 — UTC-3    |
| Banco de dados           | PostgreSQL 16 Alpine              |
| Execução                 | Docker local                      |
| Massa de dados           | Aproximadamente 208 mil registros |
| Ferramenta executada     | pgbench 16.13                     |
| Clientes simultâneos     | 20                                |
| Threads                  | 4                                 |
| Duração por cenário      | 30 segundos                       |
| Tentativas por transação | 1                                 |

## Cenário 1 — leitura do catálogo

O cenário realizou consultas relacionadas ao catálogo, sessões, popularidade e recomendação de espetáculos.

| Métrica                  |             Resultado |
| ------------------------ | --------------------: |
| Transações processadas   |                85.108 |
| Transações com falha     |                     0 |
| Taxa de erro             |                0,000% |
| Latência média           |              7,043 ms |
| Throughput               | 2.839,57 transações/s |
| Tempo inicial de conexão |             43,685 ms |

### Tempos médios das consultas

| Operação                                | Latência |
| --------------------------------------- | -------: |
| Consulta de espetáculo por popularidade | 1,369 ms |
| Consulta de catálogo                    | 1,203 ms |
| Consulta complementar de espetáculo     | 1,439 ms |
| Consulta de sessões e preços            | 1,501 ms |
| Consulta de score de recomendação       | 1,442 ms |

## Cenário 2 — escrita de avaliação e auditoria

O cenário realizou inserções concorrentes de avaliações, incluindo o acionamento da trilha de auditoria associada à operação.

| Métrica                    |             Resultado |
| -------------------------- | --------------------: |
| Transações processadas     |               156.903 |
| Transações com falha       |                     0 |
| Taxa de erro               |                0,000% |
| Latência média             |              3,823 ms |
| Throughput                 | 5.232,15 transações/s |
| Tempo inicial de conexão   |             39,921 ms |
| Latência média do `INSERT` |              3,783 ms |

## Auditoria

Após a execução do cenário de escrita, foram identificados:

| Evidência                            | Resultado |
| ------------------------------------ | --------: |
| Registros na auditoria de avaliações |   256.903 |
| Falhas de escrita registradas        |         0 |
| Perda de registros identificada      |         0 |

## Comparação com os critérios do RNF06

| Critério                                      |            Resultado observado | Situação |
| --------------------------------------------- | -----------------------------: | -------- |
| Tempo médio inferior ou igual a 3 segundos    | 7,043 ms no cenário de leitura | Atendido |
| Taxa de erro inferior ou igual a 1%           |                         0,000% | Atendido |
| Ausência de falhas críticas                   |       Nenhuma falha registrada | Atendido |
| Estabilidade da persistência sob concorrência | 242.011 transações processadas | Atendido |
| Preservação da auditoria                      |  256.903 registros disponíveis | Atendido |

## Automação da camada de API

Além da validação da persistência, foi implementada a automação de carga HTTP com k6 para os seguintes fluxos:

* consulta paginada ao catálogo;
* busca de espetáculos;
* recomendação personalizada;
* cenário principal com 200 usuários virtuais durante 10 minutos;
* cenário de sobrecarga progressiva até 300 usuários;
* monitoramento de CPU e memória;
* coleta de logs dos serviços;
* geração automática do relatório em Markdown.

Os arquivos responsáveis pela automação são:

* `load-test.js`;
* `run-rnf06.ps1`;
* `generate-rnf06-report.ps1`;
* `.env.rnf.example`.

A execução HTTP depende da disponibilidade saudável dos serviços e de um token JWT válido para o fluxo autenticado de recomendação.

## Evidências

* `src/database/load_test/resultados_pgbench.txt`;
* `src/backend/tests/non-functional/rnf06/load-test.js`;
* `src/backend/tests/non-functional/rnf06/run-rnf06.ps1`;
* `src/backend/tests/non-functional/rnf06/generate-rnf06-report.ps1`;
* `src/backend/tests/non-functional/rnf06/README.md`.

## Conclusão

A camada de persistência foi aprovada no escopo avaliado. Os dois cenários apresentaram latências muito inferiores ao limite de três segundos, taxa de erro igual a zero e alto volume de transações processadas.

O cenário de leitura processou 85.108 transações com latência média de 7,043 ms, enquanto o cenário de escrita processou 156.903 transações com latência média de 3,823 ms. A trilha de auditoria permaneceu operacional durante a carga.

O teste demonstra que a persistência suporta concorrência com estabilidade e sem falhas observadas. A automação complementar de carga HTTP encontra-se implementada para avaliação integrada dos serviços.
