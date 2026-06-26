# RNF12 — Auditoria estática e estado

## Critério

Todos os serviços do escopo devem executar em containers, permanecer isolados e recuperar o health check em até 30 segundos.

## Auditoria do repositório

O escopo inicial do executor contém quatro serviços de domínio:

- catálogo;
- usuário;
- recomendação;
- geolocalização.

Todos possuem Dockerfile, `restart: unless-stopped`, health check e participação na rede `spotlight_network`.

A topologia final de produção está em `docker-compose.prod.yml` e inclui também frontend e API Gateway. A validação final da recuperação/containerização deve ser registrada no ambiente AWS EC2 conforme `documentos/manual_deploy.md`, usando a saída de `docker compose -f docker-compose.prod.yml ps` e os health checks públicos.

## Implementação entregue

- auditoria automática do Compose e Dockerfiles;
- reinicialização controlada por serviço;
- medição do tempo de recuperação;
- verificação da continuidade dos demais serviços;
- saída CSV consolidada;
- modo seguro antes da execução destrutiva.
