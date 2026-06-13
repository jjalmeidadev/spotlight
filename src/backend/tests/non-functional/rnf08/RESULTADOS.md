# RNF08 — Estado da execução

## Critério

Atualização independente com indisponibilidade máxima de cinco minutos e 100% dos demais serviços operacionais.

## Implementação entregue

- build isolado do serviço alvo;
- atualização com `docker compose up -d --no-deps`;
- monitoramento do alvo e dos demais serviços;
- cálculo do tempo até recuperação;
- relatório com falhas externas ao serviço alvo;
- instrução de rollback;
- modo seguro antes de qualquer operação real.

## Limitação arquitetural identificada

O `docker-compose.yml` contém catálogo, usuário, recomendação e geolocalização. O API Gateway possui projeto próprio, mas não está incluído no Compose analisado; portanto, não integra o escopo de deploy containerizado deste teste.
