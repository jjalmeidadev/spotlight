<p align="center">
  <img src="./img/header.png" alt="header">
</p>

<table align="center" style="border: none; border-collapse: collapse;">
  <tr>
    <td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/celsojwnior/">
        <img src="./img/celso.png" width="90" alt="Celso Rodrigues">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/jaimeandradedealmeida/">
        <img src="./img/jaime.png" width="90" alt="Jaime Almeida">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/leonardoramosvieira/">
        <img src="./img/leonardo.png" width="90" alt="Leonardo Ramos">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/marianalacerdareis/">
        <img src="./img/mariana_lacerda.png" width="90" alt="Mariana Lacerda">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/mariana-pereira-de-souza1/">
        <img src="./img/marianap.png" width="90" alt="Mariana Pereira">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/rafael-figueiredo-campos-72131735b/">
        <img src="./img/rafael.png" width="90" alt="Rafael Campos">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/vitor-ribeiro-ms/">
        <img src="./img/vitor.png" width="90" alt="Vitor Ribeiro">
      </a>
    </td>
  </tr>
</table>

<br>

<p align="center">
  <img src="./img/professores.png" alt="professores">
</p>

<table align="center" style="border: none; border-collapse: collapse;">
  <tr>
    <td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/vanunes/">
        <img src="./img/vanessa.png" width="100" alt="Vanessa Nunes">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/afonsolelis/">
        <img src="./img/afonso.png" width="100" alt="Afonso Brandão">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/hermano-peixoto-1091796/">
        <img src="./img/hermano.png" width="100" alt="Hermano Peixoto">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/luciano-galdino-26191b36/">
        <img src="./img/luciano.png" width="100" alt="Luciano Galdino">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/rafael-jacomossi-6135b0a1/">
        <img src="./img/rafaelJ.png" width="100" alt="Rafael Jacomossi">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/francisco-escobar/">
        <img src="./img/francisco.png" width="100" alt="Francisco Escobar">
      </a>
    </td>

<td align="center" style="border: none; padding: 5px;">
      <a href="https://www.linkedin.com/in/filipe-gon%C3%A7alves-08a55015b/">
        <img src="./img/filipe.png" width="100" alt="Filipe Gonçalves">
      </a>
    </td>
  </tr>
</table>

<br>

<p align="center">
  <img src="./img/bottom.png" alt="bottom">
</p>

# Spotlight

## Gestão de Projeto e Configuração

# Sumário

- [1. Introdução](#1-introdução)
  - [1.1 Objetivo do Documento](#11-objetivo-do-documento)
- [2. Visão Geral do GitFlow](#2-visão-geral-do-gitflow)
- [3. Fluxo Gitflow e Políticas de Código](#3-fluxo-gitflow-e-políticas-de-código)
  - [3.1 Estrutura e Regras de Branches](#31-estrutura-e-regras-de-branches)
  - [3.2 Controle de versão e etiquetagem](#32-controle-de-versão-e-etiquetagem)
  - [3.3 Política de Criação de Branch](#33-política-de-criação-de-branch)
  - [3.4 Política de Merge Request](#34-política-de-merge-request)
  - [3.5 Políticas de Pull Requests (PR)](#35-políticas-de-pull-requests-pr)
  - [3.6 Políticas de Commits](#36-políticas-de-commits)
  - [3.7 Exemplo ponta a ponta de ciclo de feature](#37-exemplo-ponta-a-ponta-de-ciclo-de-feature)
- [4. Políticas de Kanban e Fluxo de Trabalho](#4-políticas-de-kanban-e-fluxo-de-trabalho)
  - [4.1 Regras de Movimentação e Organização](#41-regras-de-movimentação-e-organização)
  - [4.2 Gestão de Impedimentos](#42-gestão-de-impedimentos)
- [5. Estrutura de Cards](#5-estrutura-de-cards)
  - [5.1 A Descrição](#51-a-descrição)
  - [5.2 Definition of Ready (DoR) e Definition of Done (DoD)](#52-definition-of-ready-dor-e-definition-of-done-dod)
- [6. Cerimônias e Melhoria Contínua](#6-cerimônias-e-melhoria-contínua)
- [7. Métricas de Desempenho do Processo](#7-métricas-de-desempenho-do-processo)
- [8. Alinhamento com PMO e Gestão de Riscos](#8-alinhamento-com-pmo-e-gestão-de-riscos)

# Políticas de Versionamento, Gestão de Tarefas e Alinhamento com o PMO

Este documento estabelece as diretrizes para o fluxo de desenvolvimento de software, unindo o **versionamento de código via Gitflow** à **gestão de tarefas no Kanban**. 

O objetivo é garantir um ambiente de trabalho colaborativo, organizado, focado na entrega de valor contínua, com alta rastreabilidade e totalmente alinhado aos padrões de governança do Escritório de Gerenciamento de Projetos.

## 1. Introdução

A gestão de configuração é essencial para garantir organização, rastreabilidade e controle sobre a evolução do código-fonte e da documentação do projeto. 

Este documento define os padrões, políticas e procedimentos adotados pela equipe para o gerenciamento de versionamento utilizando **Git** e **GitHub**, combinados com a gestão visual de tarefas através do **Kanban**.

Ao estabelecer regras claras para branches, commits, pull/merge requests e organização do repositório, a equipe assegura o desenvolvimento colaborativo, minimiza conflitos e mantém a integridade do projeto ao longo de todo o seu ciclo de vida.

### 1.1 Objetivo do Documento

O propósito deste documento é formalizar as práticas de gerenciamento de configuração adotadas no projeto, incluindo:

- Modelo de branching Gitflow
- Políticas de criação, nomenclatura e exclusão de branches
- Padrões de commits 
- Procedimentos de Pull/Merge Requests
- Integração entre o código e o quadro Kanban
- Definições de Ready e Done
- Cerimônias ágeis e métricas de desempenho

Este documento garante que todos os membros da equipe sigam as mesmas práticas, promovendo consistência, qualidade, segurança e alinhamento com as melhores práticas de desenvolvimento de software e com as expectativas do PMO.

## 2. Visão Geral do GitFlow

GitFlow é um modelo de branching projetado para organizar e estruturar o desenvolvimento de software. Seu principal objetivo é fornecer uma estrutura clara para a criação, utilização e merge de branches ao longo do ciclo de vida do projeto.

Esse modelo melhora a colaboração, aumenta a visibilidade das mudanças e garante maior rastreabilidade, permitindo que a equipe trabalhe simultaneamente em novas funcionalidades, correções e releases sem comprometer a estabilidade do código principal.

### Estrutura operacional do Gitflow

| Branch | Finalidade | Origem | Destino |
|--------|----------|--------|--------|
| main | Versão estável em produção | release / hotfix | - |
| develop | Integração das funcionalidades | main | main |
| feature/* | Desenvolvimento de novas funcionalidades | develop | develop |
| release/* | Preparação para entrega | develop | main e develop |
| hotfix/* | Correções urgentes em produção | main | main e develop |

### Regras de uso

- Nenhuma alteração deve ser feita diretamente na branch main
- Todas as funcionalidades devem ser desenvolvidas em branches do tipo feature/*
- Toda integração deve ocorrer via Merge Request (MR)
- Após aprovação, a branch deve ser mesclada na develop
- Após o merge, a branch deve ser excluída
- Correções emergenciais devem ser feitas via hotfix/*

## 3. Fluxo Gitflow e Políticas de Código

### 3.1 Estrutura e Regras de Branches

#### 3.1.1 Branches Principais

- **main**: Representa o código estável e pronto para produção. Recebe apenas merges de branches de release ou hotfix. *Pushes diretos são estritamente proibidos.*
- **develop**: Branch de integração principal. Todas as funcionalidades são integradas aqui. *Pushes diretos são estritamente proibidos.*

#### 3.1.2 Branches de Suporte

- **feature/***: Criadas a partir da `develop` para desenvolver novas funcionalidades, melhorias ou ajustes.
- **release/***: Criadas a partir da `develop` quando o backlog da sprint está completo e validado internamente. Usadas para estabilização final, correções de bugs e ajustes de documentação.  
  **Não são permitidas novas funcionalidades** nesta branch.
- **hotfix/***: Criadas diretamente a partir da `main` para corrigir problemas críticos em produção que não podem aguardar a próxima sprint.

### 3.2 Controle de versão e etiquetagem

O projeto segue **Semantica de versão** (MAJOR.MINOR.PATCH):
- **MAJOR**: Mudanças breaking
- **MINOR**: Novas funcionalidades
- **PATCH**: Correções de bugs

Após aprovação de uma release ou hotfix, deve-se criar uma tag de versão.

### 3.3 Política de Criação de Branch

#### Nomes Convencionais

| Tipo              | Padrão                          | Exemplo                              |
|-------------------|---------------------------------|--------------------------------------|
| Feature           | `feat/<descrição>`              | `feat/integracao-rabbitmq`           |
| Fix / Bugfix      | `fix/<descrição>`               | `fix/correcao-login`                 |
| Documentation     | `docs/<descrição>`              | `docs/atualizacao-readme`            |
| Release           | `release/x.y.z`                 | `release/1.0.0`                      |
| Hotfix            | `hotfix/<descrição>`            | `hotfix/correcao-producao`           |

Todas as branches devem ser escritas em **lowercase**, separando palavras com hífen.

#### Passos para Criar uma Branch

O fluxo de criação e utilização das branches segue as seguintes etapas:

1. Atualizar a branch base (develop):
   git checkout develop
   git pull origin develop

2. Criar uma nova branch de trabalho:
   git checkout -b feature/nome-da-funcionalidade

3. Realizar commits seguindo padrão semântico:
   feat: descrição da funcionalidade

4. Enviar a branch para o repositório remoto:
   git push origin feature/nome-da-funcionalidade

5. Abrir um Merge Request (MR) para a branch develop

6. Após revisão e aprovação:
   - Realizar o merge
   - Excluir a branch utilizada

#### Exemplo de aplicação

A seguir, um exemplo prático da criação de uma funcionalidade no projeto:

git checkout develop  
git checkout -b feature/recomendacao  

git add .  
git commit -m "feat: implementação inicial do sistema de recomendação"  

git push origin feature/recomendacao  

→ abertura de Merge Request  
→ revisão por outro integrante  
→ merge na develop  
→ exclusão da branch  

## 3.4 Política de Merge Request

Todas as integrações de código devem ocorrer **exclusivamente via Pull/Merge Request**. Para garantir a higiene do repositório e a estabilidade das versões, aplicam-se as seguintes regras estruturais:

- **Exclusão Pós-Merge**: Após o merge bem-sucedido de uma feature ou branch de suporte, a branch de origem deve ser imediatamente excluída do repositório remoto.
- **Sincronia Dupla**: Branches de `release` e `hotfix` devem, obrigatoriamente, ser mergeadas tanto na branch `main` quanto na `develop`.
- **Regra de Merge-back**: Todo código validado em branches de release e hotfix deve ser mergeado de volta para a `develop` o mais rápido possível, a fim de evitar divergências futuras no ambiente de integração.

## 3.5 Políticas de Pull Requests (PR)

**Regra de Ouro**: Commits diretos nas branches `main` e `develop` são **estritamente proibidos**. Toda e qualquer alteração no código deve passar por um Pull Request.

### Requisitos de Aprovação 

A quantidade de revisões necessárias varia conforme o tipo de branch:

- **feature/** → Mínimo de **1 aprovação**
- **release/** e **hotfix/** → Mínimo de **2 aprovações**

### Critérios Adicionais para Release e Hotfix

Para que um Pull Request de release ou hotfix seja aceito, todas as condições abaixo devem ser atendidas:

- [ ] Pipeline de CI/CD finalizado com status **verde**
- [ ] Nenhuma discussão ou thread de code review pendente
- [ ] Todos os issues ou cards vinculados foram movidos ou fechados
- [ ] Validação explícita por parte do Scrum Master ou Tech Lead

## 3.6 Políticas de Commits

Os commits devem ser **atômicos**, ou seja, pequenos, frequentes e representando uma única alteração lógica por vez.

Utilizamos o padrão **Conventional Commits**, complementado com a referência ao card do Kanban, seguindo a estrutura:

### Tipos Recomendados

| Tipo | Descrição |
|---|---|
| `feat` | Nova funcionalidade ou recurso de negócio |
| `fix` | Correção de um bug ou falha |
| `docs` | Alterações, atualizações ou criação de documentação |
| `refactor` | Refatoração de código sem alterar o comportamento |
| `test` | Criação, ajuste ou padronização de testes |
| `chore` | Tarefas de manutenção que não alteram comportamento de produto, como ajustes de configuração, limpeza de arquivos e organização de dependências |

### Exemplos

```bash
feat: adiciona endpoint de processamento de fila #23
fix: corrige falha na validação do formulário de login #47
docs: atualiza diagrama de arquitetura do banco de dados #62
refactor: melhora estrutura do serviço de autenticação #19
test: adiciona testes de regras de negócio para avaliação #45
chore: atualiza configuração do pipeline de testes #52
```

### 3.7 Exemplo ponta a ponta de ciclo de feature

O ciclo completo de uma feature deve seguir o fluxo abaixo, garantindo rastreabilidade entre Kanban, branch, commit, Merge Request e documentação:

| Etapa | Ação esperada | Evidência |
|---|---|---|
| 1. Criação do card | Criar issue no Kanban com título no infinitivo, descrição, DoR, DoD, labels de tipo e tamanho. | Issue com labels `CODE`, `DOCUMENTATION`, `DESIGN` ou `MEET`, além de `SIZE_PP`, `SIZE_P`, `SIZE_M` ou `SIZE_G`. |
| 2. Preparação da branch | Atualizar `develop` e criar branch seguindo o padrão definido. | Branch como `feat/recomendacao-home`, `fix/token-avaliacao` ou `docs/gestao-sprint-5`. |
| 3. Desenvolvimento | Implementar a mudança mantendo escopo coerente com o card. | Commits pequenos, atômicos e vinculados ao número da issue. |
| 4. Validação local | Rodar build, testes ou validação manual adequada ao tipo da entrega. | Log de execução, print, resumo no MR ou evidência no documento correspondente. |
| 5. Abertura do MR | Abrir Merge Request para `develop`, associando a issue e descrevendo escopo, validação e riscos. | MR com `Closes #N`, checklist e reviewer designado. |
| 6. Revisão | Reviewer analisa código/documentação, solicita ajustes se necessário e aprova quando os critérios forem atendidos. | Comentários resolvidos, aprovação registrada e pipeline verde quando aplicável. |
| 7. Merge e fechamento | Realizar merge na `develop`, fechar issue e remover branch se aplicável. | Issue em `Closed`, MR merged e histórico rastreável no GitLab. |

Exemplo aplicado:

```text
Issue: #293 - Validar integração final frontend-backend-recomendação
Branch: feat/integra-frontend-recomendacao-fluxo
Commit: fix: estabiliza onboarding e recomendacoes iniciais #293
MR: Closes #293, com validação do fluxo de cadastro, preferências, recomendação e telemetria.
Resultado: funcionalidade integrada na develop após revisão.
```

## 4. Políticas de Kanban e Fluxo de Trabalho

O fluxo de tarefas é gerenciado de forma visual, refletindo a realidade exata do progresso da equipe.

**Fluxo de Status**: `Open → Backlog → Doing → Waiting Review → Review → Closed`

### 4.1 Regras de Movimentação e Organização

- **WIP Limit**: Apenas 1 tarefa por membro da equipe pode estar em "In Progress" simultaneamente.

- **Vínculo Código-Kanban**: É obrigatório que todo card de código contenha o link do Merge Request anexado. Um card não pode avançar para `Review` ou `Closed` sem este link validado.

- **Tamanhos e Tags**: Cards devem ser estimados em **PP**, **P**, **M** ou **G** (cards G devem ser quebrados sempre que possível) e categorizados com tags como `[Code]`, `[Design]`, `[Documentation]`, ou `[Meet]`.

### 4.2 Gestão de Impedimentos

- **Identificação**: Impedimentos devem ser sinalizados imediatamente na Daily e o card deve receber a tag `Blocked`.

- **Prioridade**: Tarefas bloqueadas têm prioridade de resolução sobre novos trabalhos. Se não resolvidas em 24h, escalam para o Scrum Master.

## 5. Estrutura de Cards 

### 5.1 A Descrição

Todo card deve ter uma explicação detalhada sobre o problema a ser resolvido e o valor entregue, compreensível para qualquer desenvolvedor multidisciplinar do time.

### 5.2 Definition of Ready (DoR) e Definition of Done (DoD)

Cada card deve possuir sua própria **Definition of Ready (DoR)** e **Definition of Done (DoD)**, definidas de forma específica para aquela tarefa. 

O objetivo é garantir que os critérios de pronto para iniciar e pronto para entregar sejam claros, contextualizados e acordados por toda a equipe, evitando ambiguidades durante a execução.

#### DoR (Definition of Ready) – Critérios para iniciar o trabalho

A tarefa só pode ser movida para **Ready** quando atender aos seguintes critérios mínimos:

- Requisitos e objetivo da tarefa estão bem descritos e compreensíveis;
- Critérios de aceite estão claros e mensuráveis;
- Dependências técnicas e/ou de negócio foram identificadas e mapeadas;
- Um responsável foi definido;
- Tamanho da tarefa foi estimado (P, M ou G).

> **Exemplo de DoR específica:**
> - Acesso ao banco de dados de homologação liberado;
> - API externa documentada e disponível para testes;
> - Protótipo de UI aprovado pelo Product Owner.

#### DoD (Definition of Done) – Critérios para considerar a tarefa concluída

A tarefa só pode ser movida para **Done** quando todos os itens abaixo forem atendidos:

- Código implementado conforme os requisitos;
- Testes unitários e/ou de integração criados e passando;
- Code Review realizado e Merge Request aprovado;
- Merge efetuado na branch `develop`;
- Documentação técnica e funcional atualizada (quando aplicável);
- Card vinculado ao Merge Request no Kanban.

> **Exemplo de DoD específica:**
> - Endpoint retorna status 200 com payload correto em ambiente de testes;
> - Cobertura de testes da funcionalidade ≥ 80%;
> - Swagger atualizado com novos campos e descrições;
> - Tratamento de erros implementado e logado.

---

**Observação:** Embora existam critérios globais mínimos, a DoR e a DoD devem ser complementadas com itens específicos de cada tarefa durante a Sprint Planning ou refinamento do backlog. Isso garante maior qualidade, transparência e responsabilidade compartilhada.

## 6. Cerimônias e Melhoria Contínua

### 6.1 Daily Meetings

- **Formato**: Segunda a sexta, duração máxima de 15 minutos, presenciais no Inteli.

- **Pauta**: Cada membro responde:

  1. O que concluí desde a última reunião?

  2. O que farei hoje?

  3. Há algum impedimento?

- **Registro**: Obrigatório gerar um resumo escrito diário no chat da equipe ou no quadro Kanban. Membros ausentes devem postar suas respostas antes da reunião.

### 6.2 Sprint Planning

As Plannings garantem que o escopo seja focado no MVP e nos objetivos da Sprint. Inclui revisão de riscos e integração das lições aprendidas da sprint anterior.

### 6.3 Retrospectives

Realizadas ao final de cada sprint para avaliar o processo. Devem identificar:

- O que funcionou

- O que falhou

- Impedimentos

- Oportunidades de otimização

**Ação Prática**: Toda melhoria deve ser documentada, receber um responsável, critério de validação e se transformar em um ticket no Backlog da próxima sprint.

## 7. Métricas de Desempenho do Processo

| KPI                  | Definição                                                                 | Meta                  |
|----------------------|---------------------------------------------------------------------------|-----------------------|
| **Lead time**        | Tempo desde a criação/início da tarefa até sua conclusão                  | ≤ 3 dias úteis       |
| **Cycle time**       | Tempo desde o início prático do trabalho até estar pronto para entrega    | ≤ 2 dias úteis       |
| **Tempo de PR**      | Tempo desde a criação do MR até a aprovação final                         | ≤ 24 horas           |
| **Taxa de Defeito**  | % de bugs encontrados após o merge para a branch `develop`                | < 5%                 |
| **Precisão do Board**| % de tarefas transitadas corretamente e com tags/links exigidos          | ≥ 90%                |

## 8. Alinhamento com PMO e Gestão de Riscos

### 8.1 Governança e Rastreabilidade

Todas as decisões de projeto são documentadas na pasta `docs/`. O Gitflow atua como principal ferramenta de controle de versão, garantindo que features, metas de sprint e MRs estejam perfeitamente amarrados e auditáveis.

### 8.2 Controle de Escopo

O escopo deve ser estritamente aderente ao MVP. Qualquer alteração de escopo deve ser levada à Planning para evitar o aumento do escopo.

### 8.3 Monitoramento de Riscos 

Mantemos um Registro de Riscos atualizado e revisado semanalmente, contendo:

- Descrição do Risco

- Probabilidade e Impacto (Baixo / Médio / Alto)

- Estratégia de Prevenção e Plano de Contingência

- Status Atual

---

Esta estrutura garante a maturidade do nosso ciclo de vida de desenvolvimento, transformando a gestão de riscos de reativa para proativa.
