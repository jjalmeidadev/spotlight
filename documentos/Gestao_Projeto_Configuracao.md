# Sumário

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

## 3. Fluxo Gitflow e Políticas de Código

### 3.1 Estrutura e Regras de Branches

#### 3.1.1 Branche Principais

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

1. Atualize o repositório local:
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. Crie a nova branch:
   ```bash
   git checkout -b feature/T12-integracao-rabbitmq
   ```

3. Desenvolva, faça commits frequentes e faça push:
   ```bash
   git push origin feature/T12-integracao-rabbitmq
   ```

4. Abra um Pull Request para a branch develop.

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

| Tipo       | | Descrição                                              |
|------------|-------|--------------------------------------------------------|
| `feat`     |  | Nova funcionalidade ou recurso de negócio              |
| `fix`      |  | Correção de um bug ou falha                            |
| `docs`     | | Alterações, atualizações ou criação de documentação    |
| `refactor` | | Refatoração de código sem alterar o comportamento      |

### Exemplos

```bash
feat: adiciona endpoint de processamento de fila #23
fix: corrige falha na validação do formulário de login #47
docs: atualiza diagrama de arquitetura do banco de dados #62
refactor: melhora estrutura do serviço de autenticação #19
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