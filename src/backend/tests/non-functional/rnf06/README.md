# RNF06 — Carga e escalabilidade

Este diretório contém os scripts e as instruções para execução do teste de carga e escalabilidade dos serviços do Spotlight.

## Objetivo

Validar se os serviços de catálogo, busca e recomendação conseguem atender usuários simultâneos sem ultrapassar os critérios definidos de latência e taxa de erro.

## Escopo

O teste contempla as seguintes operações:

* consulta paginada ao catálogo;
* busca de espetáculos por texto;
* obtenção de recomendações personalizadas;
* monitoramento de CPU e memória dos containers;
* coleta dos logs dos serviços;
* análise do comportamento durante sobrecarga;
* verificação da recuperação após redução da carga.

## Endpoints utilizados

| Serviço               | Método | Endpoint                                   |
| --------------------- | ------ | ------------------------------------------ |
| Catálogo              | GET    | `/espetaculos?page=1&pageSize=20`          |
| Busca                 | GET    | `/espetaculos?q=Hamlet&page=1&pageSize=20` |
| Recomendação          | GET    | `/me?limite=10`                            |
| Saúde do catálogo     | GET    | `/health/ready`                            |
| Saúde da recomendação | GET    | `/health/ready`                            |

A operação de recomendação exige um token JWT válido no campo `ACCESS_TOKEN`.

## Cenário principal

O cenário principal utiliza:

* 200 usuários virtuais simultâneos;
* duração de 10 minutos;
* intervalo de 1 segundo entre as iterações;
* consultas aos fluxos de catálogo, busca e recomendação;
* monitoramento dos serviços durante toda a execução.

## Cenário de sobrecarga

O cenário negativo utiliza as seguintes etapas:

1. aumento gradual até 200 usuários virtuais;
2. manutenção da carga de referência;
3. aumento até 300 usuários virtuais;
4. manutenção da sobrecarga durante 5 minutos;
5. redução para 50 usuários virtuais;
6. observação da recuperação do ambiente;
7. encerramento gradual da carga.

Esse cenário permite analisar a degradação do sistema e sua capacidade de retornar ao comportamento estável.

## Critérios de aceitação

| Métrica                                     |                         Critério |
| ------------------------------------------- | -------------------------------: |
| Tempo médio de resposta                     |              Inferior a 3.000 ms |
| Taxa de erro HTTP                           |                    Inferior a 1% |
| Taxa de erro da aplicação                   |                    Inferior a 1% |
| Disponibilidade durante o cenário principal |        Sem interrupções críticas |
| Recuperação após sobrecarga                 | Retorno ao comportamento estável |

Os thresholds são aplicados automaticamente pelo k6 no cenário principal.

## Arquivos

| Arquivo                     | Responsabilidade                                           |
| --------------------------- | ---------------------------------------------------------- |
| `load-test.js`              | Define os cenários, endpoints, métricas e thresholds do k6 |
| `run-rnf06.ps1`             | Executa o teste, monitora containers e coleta evidências   |
| `generate-rnf06-report.ps1` | Converte o resumo JSON do k6 em relatório Markdown         |
| `README.md`                 | Documenta configuração, execução e critérios do RNF06      |

## Configuração

As variáveis versionáveis ficam em:

```text
src/backend/tests/non-functional/.env.rnf.example
```

As configurações locais e informações sensíveis devem ficar em:

```text
src/backend/tests/non-functional/.env.rnf
```

O arquivo local não deve ser versionado.

Antes da execução, preencha um token válido:

```env
ACCESS_TOKEN=TOKEN_JWT_DO_USUARIO
```

Não registre tokens, senhas, connection strings ou chaves privadas no repositório.

## Execução com Docker

Na raiz do projeto, execute o cenário principal:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf06/run-rnf06.ps1 -Scenario positive -ExecutionMode docker
```

Execute o cenário de sobrecarga:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf06/run-rnf06.ps1 -Scenario negative -ExecutionMode docker
```

O executor utiliza a imagem oficial do k6 e converte automaticamente os endereços `localhost` para `host.docker.internal`.

## Execução com k6 instalado localmente

Para executar sem o container do k6:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf06/run-rnf06.ps1 -Scenario positive -ExecutionMode native
```

## Evidências geradas

Os arquivos brutos são armazenados em:

```text
src/backend/tests/non-functional/.results/rnf06/
```

Para cada execução são gerados:

* resumo das métricas em JSON;
* saída completa do console;
* consumo de CPU e memória em CSV;
* logs dos serviços de catálogo e recomendação.

Esses arquivos permanecem fora do versionamento até serem revisados e consolidados na documentação.

## Geração do relatório

Utilize o resumo JSON produzido pelo k6:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf06/generate-rnf06-report.ps1 `
  -SummaryPath "src/backend/tests/non-functional/.results/rnf06/ARQUIVO-summary.json" `
  -Scenario positive `
  -ResourcesPath "src/backend/tests/non-functional/.results/rnf06/ARQUIVO-resources.csv" `
  -LogsPath "src/backend/tests/non-functional/.results/rnf06/ARQUIVO-services.txt"
```

O relatório apresenta:

* total de requisições;
* throughput;
* tempo mínimo, médio, máximo, mediana, p90 e p95;
* taxa de erro HTTP;
* taxa de erro da aplicação;
* tempos médios por operação;
* consumo de CPU e memória;
* avaliação dos critérios;
* veredito final.

## Resultado esperado

No cenário principal, o sistema deve manter tempo médio inferior a 3 segundos e taxa de erro inferior a 1%.

Durante a sobrecarga, pode ocorrer aumento de latência e erros. Após a redução da carga, o ambiente deve recuperar sua estabilidade sem necessidade de reinicialização dos serviços.
