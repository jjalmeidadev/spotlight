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

## Escopo e limitação

O executor deste RNF foi criado para atualização isolada dos serviços de domínio: catálogo, usuário, recomendação e geolocalização. A topologia final de produção da entrega está em `docker-compose.prod.yml` e inclui também frontend e API Gateway.

Para a evidência final de deploy, registrar a execução em AWS EC2 conforme `documentos/manual_deploy.md`, incluindo `docker compose -f docker-compose.prod.yml ps`, `http://<PUBLIC_IP>/api/health` e `http://<PUBLIC_IP>/swagger`.
