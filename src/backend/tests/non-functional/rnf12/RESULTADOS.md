# RNF12 — Auditoria estática e estado

## Critério

Todos os serviços do escopo devem executar em containers, permanecer isolados e recuperar o health check em até 30 segundos.

## Auditoria do repositório

O Compose analisado contém quatro serviços:

- catálogo;
- usuário;
- recomendação;
- geolocalização.

Todos possuem Dockerfile, `restart: unless-stopped`, health check e participação na rede `spotlight_network`.

O API Gateway possui projeto .NET, porém não possui Dockerfile nem entrada no `docker-compose.yml` analisado. Essa ausência deve aparecer como limitação da cobertura de containerização.

## Implementação entregue

- auditoria automática do Compose e Dockerfiles;
- reinicialização controlada por serviço;
- medição do tempo de recuperação;
- verificação da continuidade dos demais serviços;
- saída CSV consolidada;
- modo seguro antes da execução destrutiva.
