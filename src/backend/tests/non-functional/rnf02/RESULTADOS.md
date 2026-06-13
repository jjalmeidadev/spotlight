# RNF02 — Estado da execução

## Critério

Três perfis, dez execuções por perfil e 100% das recomendações exibidas em até 2.000 ms.

## Implementação entregue

- cenário k6 com três perfis e 30 medições;
- threshold de tempo máximo de 2 segundos;
- validação de status e corpo da resposta;
- planilha para tempo percebido na interface;
- consolidador de mínimo, média, máximo e p95;
- campos para spinner, renderização e observações.

## Situação

Automação pronta. A execução exige três tokens JWT válidos e o serviço de recomendação disponível.
