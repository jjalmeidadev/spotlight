# Orientações para a finalização do projeto — Semana 8

Este documento reúne orientações para a conclusão do projeto nesta semana, com
ênfase no módulo de recomendação e no motor de recomendação (*engine*). Não se
trata de uma correção nem de atribuição de nota; é um conjunto de recomendações
técnicas derivado do estado atual do repositório em 8 de junho de 2026,
considerando os ramos `develop` (linha de trabalho mais atual) e `main`.

## Síntese

O serviço de recomendação (`src/backend/services/recomendacao`) apresenta uma
arquitetura limpa e bem organizada, com separação clara entre domínio, aplicação,
infraestrutura e apresentação: casos de uso (`ListarRecomendacoesMeUseCase` e
`ListarSimilaresUseCase`), interfaces de repositório, repositórios de persistência
com SQL puro via Npgsql, tratamento de autenticação e controladores REST. O serviço
está inserido em um *backend* de microsserviços (*gateway* e serviços de catálogo,
geolocalização, usuário e recomendação) e dispõe de testes. A leitura das
recomendações é feita diretamente do banco de dados — modelo de *serving* adequado,
em que o serviço apenas consulta resultados previamente computados.

O ponto determinante para a semana decorre justamente disso: **o componente que
produz as recomendações ainda não existe**. O diretório `src/backend/ml-engine`,
que deveria conter as etapas de preparação de dados, treino e inferência, está vazio
(apenas `.gitkeep` em `data-preparation/`, `training/` e `inference/`). Além disso,
as tabelas consultadas pelo serviço — `recomendacao` e `similaridade_item` — não
recebem carga inicial no `02_initial_data.sql`. Em consequência, embora a camada de
*serving* esteja corretamente implementada, ela consulta tabelas vazias e, na
prática, não retorna recomendações reais. Os modelos foram desenvolvidos em
*notebooks* (`src/models/sistema_recomendacao_knn.ipynb` e
`sistema_recomendacao_svd (5).ipynb`), mas não há, ainda, um processo que os
materialize no banco.

A prioridade da semana, portanto, é fechar esse elo: gerar as recomendações e
populá-las no banco, de modo que o caminho completo — geração, persistência e
leitura — funcione de ponta a ponta.

## Geração e população do *store* de recomendações

Recomenda-se implementar, sob `ml-engine`, ao menos um processo executável que leia
as interações da base e produza os registros consumidos pelo serviço, gravando-os
nas tabelas `recomendacao` (recomendações por usuário) e `similaridade_item`
(itens similares). Não é necessário, neste prazo, um *pipeline* sofisticado: um
trabalho *offline* mínimo, porém funcional, que popule essas tabelas a partir de um
algoritmo concreto já encerra o elo que hoje está aberto.

Caso a portabilidade integral dos modelos não seja viável no tempo disponível, é
preferível entregar uma estratégia de base (por exemplo, popularidade para a
`recomendacao` e coocorrência/similaridade para a `similaridade_item`), real e
documentada, a manter as tabelas vazias. O essencial é que os *endpoints* passem a
retornar dados efetivos.

## Aproveitamento dos modelos já desenvolvidos

Os modelos de recomendação (KNN e SVD) já foram explorados nos *notebooks* em
`src/models`, com os conjuntos de dados em `src/dataset/recomendacao.csv` e
`recomendacao_balanceado.csv`. Convém estabelecer a ponte entre esse trabalho e o
serviço: trazer o algoritmo escolhido para uma etapa executável do `ml-engine` e
registrar, no `Projeto.md`, qual modelo foi adotado, com base em quais métricas, e
de que forma seus resultados chegam às tabelas servidas. Essa rastreabilidade entre
a análise e a implementação é um ponto importante da avaliação.

## Verificação de ponta a ponta

Após a população das tabelas, recomenda-se validar o fluxo completo: que
`ListarRecomendacoesMeUseCase` e `ListarSimilaresUseCase` retornem dados reais por
meio dos respectivos *endpoints*, e que esse comportamento seja coberto por testes
de integração do caminho de leitura. A camada de *serving* já está pronta; a
verificação garante que a geração e a leitura estão de fato conectadas.

## Integração contínua

Existe a *branch* `feat/ci-pipeline`, ainda não integrada à `develop`. Recomenda-se
concluir sua integração, de modo que o `dotnet build` e o `dotnet test` sejam
executados a cada *merge request*. Para um projeto que adota TDD, a automação
fornece evidência objetiva de não regressão — especialmente relevante na semana em
que o `ml-engine` será introduzido.

## Encerramento do projeto

- **Vídeo demonstrativo.** Não foi localizado, no `Projeto.md`, link para o vídeo de
  demonstração. Convém registrar a demonstração (com a tela dividida entre a
  aplicação e o console), evidenciando o caminho da recomendação já funcionando de
  ponta a ponta, e incluir o link na documentação. A ausência de vídeo configura
  demérito no artefato de desenvolvimento.
- **Documentação Swagger.** Confirmar que as interfaces Swagger dos serviços
  (inclusive o de recomendação) estão acessíveis e referenciadas no `Projeto.md`.
- **Reconciliação de ramos.** Há *feature branches* recentes ainda em aberto
  (integração contínua, ajustes de esquema de banco e correções de localização).
  Recomenda-se concluir sua integração e, ao final, promover `develop` para `main`,
  que se encontra defasada.
- **`Projeto.md`.** Recomenda-se que a documentação reflita o estado efetivo do
  motor ao fim da semana, descrevendo o processo de geração das recomendações e a
  origem dos dados servidos.

## Arquitetura de referência da recomendação

```
        ┌──────────── GERAÇÃO (offline) — A IMPLEMENTAR ────────────┐
        │  ml-engine/  (hoje vazio: data-preparation / training /    │
        │              inference apenas com .gitkeep)                │
        │   dataset (recomendacao.csv) + modelos (KNN / SVD)         │
        │                       │ grava                              │
        └───────────────────────┼───────────────────────────────────┘
                                 ▼
                  ┌────────────────────────────────┐
                  │  banco de dados                 │
                  │  recomendacao + similaridade_item│  (hoje sem carga)
                  └────────────────────────────────┘
                                 ▲ consulta (Npgsql, SQL puro)
        ┌──────────── SERVING (online) — PRONTO ───────────────────┐
        │  api-gateway → serviço de recomendação                    │
        │   RecomendacaoMeController / EspetaculosSimilaresController│
        │   → UseCases → Repositories → banco                       │
        │   (autenticação e normalização de limites já tratadas)    │
        └───────────────────────────────────────────────────────────┘

   Elo a fechar na semana: implementar o ml-engine e popular as tabelas,
   para que a camada de serving (já correta) retorne recomendações reais.
```

## Sequência de atividades sugerida

1. Implementar, sob `ml-engine`, um processo executável que gere recomendações e as
   grave nas tabelas `recomendacao` e `similaridade_item`.
2. Portar o algoritmo escolhido a partir dos *notebooks* de `src/models`, ou, se
   necessário, adotar uma estratégia de base real e documentada.
3. Validar de ponta a ponta os *endpoints* de recomendações e de similares, com
   testes de integração do caminho de leitura.
4. Documentar, no `Projeto.md`, o modelo escolhido, as métricas que sustentaram a
   escolha e o processo de geração e persistência.
5. Integrar a *branch* `feat/ci-pipeline` à `develop`.
6. Registrar o vídeo de demonstração e referenciá-lo no `Projeto.md`.
7. Reconciliar as demais *feature branches* pendentes e promover `develop` para
   `main`.

A orientação central é que o grupo já dispõe de uma camada de *serving* de
recomendação bem construída, integrada a um *backend* de microsserviços e apoiada no
banco de dados. O que determinará a qualidade da entrega nesta semana é fechar o elo
que falta — gerar as recomendações e populá-las no banco — para que o sistema
retorne resultados reais, de ponta a ponta e demonstráveis.
