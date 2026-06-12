# Testes de Requisitos Não Funcionais

Esta pasta contém os scripts, configurações e instruções utilizados para executar os testes de requisitos não funcionais do projeto Spotlight.

## Organização

- `common/`: arquivos e utilitários compartilhados entre os testes;
- `scripts/`: scripts de validação e execução geral;
- `rnf01/`: tempo de carregamento das telas;
- `rnf02/`: tempo de geração das recomendações;
- `rnf03/`: disponibilidade do sistema;
- `rnf04/`: abordagem mobile first e áreas de toque;
- `rnf05/`: segurança, autenticação e HTTPS;
- `rnf06/`: carga e usuários simultâneos;
- `rnf07/`: logs estruturados;
- `rnf08/`: deploy independente dos serviços;
- `rnf10/`: usabilidade e quantidade de toques;
- `rnf11/`: compatibilidade Android e iOS;
- `rnf12/`: execução e recuperação dos containers.

## RNF não executado

O RNF09 não será executado nesta versão, pois a infraestrutura de processamento assíncrono com RabbitMQ ou mecanismo equivalente não foi implementada no projeto.

Não serão criados scripts ou resultados simulados para esse requisito.

## Resultados e evidências

Os resultados consolidados serão documentados na seção `9.2.2 — Resultados e Evidências` do arquivo:

`documentos/Projeto.md`

Cada execução deverá registrar:

- data;
- ambiente;
- ferramenta;
- comando utilizado;
- cenário positivo ou negativo;
- resultado esperado;
- resultado obtido;
- critério de aceitação;
- status;
- evidências;
- limitações;
- melhorias identificadas.

## Status possíveis

- Aprovado;
- Reprovado;
- Parcialmente aprovado;
- Bloqueado;
- Não aplicável;
- Não executado.

## Segurança

Tokens, senhas, connection strings, hashes completos e outros dados sensíveis não devem ser adicionados ao repositório.