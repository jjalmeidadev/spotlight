<Table>
  <tr>
    <td><a href= "https://www.btgpactual.com/"><img src="img/logo-btg.png" alt="Centro Paula Souza" border="0"></td>
    <td>
      <a href= "https://www.inteli.edu.br/"><img src="img/logo-Inteli.png" alt="Inteli - Instituto de Tecnologia e Liderança" border="0"></a>
    </td>
  </tr>
</table>

# Nome do Projeto: <nome do projeto>

## Nome do Grupo: <nome do grupo>

## Integrantes:

- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>
- <a href="https://www.linkedin.com/in/username/">Nome</a>

# Sumário

- [1. Introdução](#1-introdução)
  - [1.1 Termos e Abreviações](#11-termos-e-abreviações)
  - [1.2 Objetivo do Documento](#12-objetivo-do-documento)
- [2. Entendimento do Projeto e do Negócio](#2-entendimento-do-projeto-e-do-negócio)
  - [2.1 Contexto da Indústria do Parceiro](#21-contexto-da-indústria-do-parceiro)
  - [2.2 Problema](#22-problema)
  - [2.3 Visão do Projeto e do Produto](#23-visão-do-projeto-e-do-produto)
    - [2.3.1 Visão do Produto](#231-visão-do-produto)
    - [2.3.2 Proposta de Valor](#232-proposta-de-valor)
    - [2.3.3 O que o Produto Faz e Não Faz](#233-o-que-o-produto-faz-e-não-faz)
    - [2.3.4 Objetivos Estratégicos e Mensuráveis](#234-objetivos-estratégicos-e-mensuráveis)
    - [2.3.5 Impacto e Abrangência](#235-impacto-e-abrangência)
    - [2.3.6 Ownership e Accountability Estratégico](#236-ownership-e-accountability-estratégico)
    - [2.3.7 Alinhamento com o Problema](#237-alinhamento-com-o-problema)
  - [2.4 Personas e Jornada do Usuário](#24-personas-e-jornada-do-usuário)
  - [2.5 Modelagem do Fluxo de Negócio](#25-modelagem-do-fluxo-de-negócio)
    - [2.5.1 Cadeia de Valor](#251-cadeia-de-valor)
    - [2.5.2 Fluxo de Negócio Proposto (AS-IS e TO-BE)](#252-fluxo-de-negócio-proposto-as-is-e-to-be)
  - [2.6 Matriz de Risco do Projeto](#26-matriz-de-risco-do-projeto)
  - [2.7 Ideação](#27-ideação)
  - [2.8 Canvas do Projeto](#28-canvas-do-projeto)
- [3. Requisitos do Projeto](#3-requisitos-do-projeto)
  - [3.1 Requisitos Funcionais (RFs)](#31-requisitos-funcionais-rfs)
  - [3.2 Requisitos Não Funcionais (RNFs)](#32-requisitos-não-funcionais-rnfs)
  - [3.3 Correlação RFs e RNFs](#33-correlação-rfs-e-rnfs)
  - [3.4 Casos de Uso](#34-casos-de-uso)
  - [3.5 Casos de Uso x Requisitos Funcionais](#35-casos-de-uso-x-requisitos-funcionais)
- [4. Modelagem de Dados](#4-modelagem-de-dados)
  - [4.1 Especificação da Base de Dados para Modelo de Recomendação](#41-especificação-da-base-de-dados-para-modelo-de-recomendação)
  - [4.2 Modelo Conceitual de Dados](#42-modelo-conceitual-de-dados)
  - [4.3 Modelo Lógico de Dados](#43-modelo-lógico-de-dados)
  - [4.4 Modelo Físico de Dados](#44-modelo-físico-de-dados)
- [5. Solução Técnica (Design)](#5-solução-técnica-design)
  - [5.1 Diagrama de Classes](#51-diagrama-de-classes)
  - [5.2 Diagrama de Componentes da UML](#52-diagrama-de-componentes-da-uml)
  - [5.3 Diagramas de Sequência da UML](#53-diagramas-de-sequência-da-uml)
- [6. Mapeamento Técnico de Infraestrutura e Implantação](#6-mapeamento-técnico-de-infraestrutura-e-implantação)
  - [6.1 Diagrama de Implantação da UML](#61-diagrama-de-implantação-da-uml)
  - [6.2 Justificativa das Escolhas de Implantação](#62-justificativa-das-escolhas-de-implantação)
  - [6.3 Considerações sobre Desempenho e Segurança](#63-considerações-sobre-desempenho-e-segurança)
- [7. Projeto Visual da Solução](#7-projeto-visual-da-solução)
  - [7.1 Desenvolvimento de Wireframes](#71-desenvolvimento-de-wireframes)
  - [7.2 Desenvolvimento de Mockups](#72-desenvolvimento-de-mockups)
  - [7.3 Guia Visual](#73-guia-visual)
- [8. Desenvolvimento do Projeto](#8-desenvolvimento-do-projeto)
  - [8.1 Arquitetura de Codificação e Estrutura de Diretórios](#81-arquitetura-de-codificação-e-estrutura-de-diretórios)
  - [8.2 Modelo de Recomendação](#82-modelo-de-recomendação)
  - [8.3 Desenvolvimento de Features](#83-desenvolvimento-de-features)
    - [8.3.1 Sprint 3](#831-sprint-3)
    - [8.3.2 Sprint 4](#832-sprint-4)
    - [8.3.3 Sprint 5](#833-sprint-5)
  - [8.4 Testes Unitários e de Integração](#84-testes-unitários-e-de-integração)
  - [8.5 Documentações automáticas](#85-documentações-automáticas)
- [9. Planejamento e Execução de Testes](#9-planejamento-e-execução-de-testes)
  - [9.1 Testes Funcionais](#91-testes-funcionais)
    - [9.1.1 Planejamento](#911-planejamento)
    - [9.1.2 Resultados](#912-resultados)
  - [9.2 Testes de RNFs](#92-testes-de-rnfs)
    - [9.2.1 Planejamento](#921-planejamento)
    - [9.2.2 Resultados](#922-resultados)
  - [9.3 Testes de Usabilidade](#93-testes-de-usabilidade)
    - [9.3.1 Planejamento](#931-planejamento)
    - [9.3.2 Resultados](#932-resultados)
- [10. Procedimentos de Implantação](#10-procedimentos-de-implantação)
  - [10.1 Implantação e Configuração do Banco de Dados](#101-implantação-e-configuração-do-banco-de-dados)
  - [10.2 Implantação do Protótipo para uso por equipe de desenvolvimento](#102-implantação-do-protótipo-para-uso-por-equipe-de-desenvolvimento)
- [Referências](#referências)

# 1. Introdução

&emsp;Este documento foi elaborado com o objetivo de servir como um guia completo para o leitor, 
fornecendo uma visão ampla de todas as etapas e aspectos do projeto desenvolvido em parceria 
com a Aventura Teatros. Nele, são apresentadas desde a descrição do parceiro envolvido e o 
problema proposto, até as decisões estratégicas tomadas, as análises de negócio realizadas e 
a documentação técnica relacionada à solução desenvolvida.

## 1.1 Termos e Abreviações

Abreviações utilizadas na elaboração desta documentação:

| Abreviação | Significado |
|---|---|
| API | Application Programming Interface (Interface de Programação de Aplicação) |
| MVP | Minimum Viable Product (Produto Mínimo Viável) |
| UI | User Interface (Interface do Usuário) |
| UX | User Experience (Experiência do Usuário) |
| RF | Requisito Funcional |
| RNF | Requisito Não Funcional |
| REST | Representational State Transfer (Transferência de Estado Representacional) |
| RACI | Responsible, Accountable, Consulted, Informed |
| Sprint | Ciclo de desenvolvimento ágil |
| SOA | Service-Oriented Architecture (Arquitetura Orientada a Serviços) |
| Portal | Plataforma digital da Aventura Teatros voltada ao público externo |

## 1.2 Objetivo do Documento

&emsp;Este documento tem como objetivo apresentar o escopo e o propósito do projeto voltado ao 
desenvolvimento de uma solução de recomendação de espetáculos no Portal Aventura. A principal 
função da solução é sugerir, de forma inteligente, espetáculos, conteúdos e experiências 
culturais alinhados ao interesse do usuário, estimulando a navegação dentro do portal e 
aumentando o potencial de conversão em vendas de ingressos. A proposta visa contribuir para 
a melhoria da experiência do usuário, o aumento do engajamento com o ecossistema da produtora 
e o fortalecimento da presença digital da Aventura Teatros como plataforma cultural completa.

# 2. Entendimento do Projeto e do Negócio

&emsp;O projeto consiste no desenvolvimento de uma solução digital de recomendação inteligente 
integrada ao Portal Aventura, com o objetivo de conectar o usuário a espetáculos, conteúdos 
e experiências culturais relevantes ao seu perfil de interesse. A iniciativa responde a uma 
lacuna identificada na jornada digital do usuário, que atualmente não recebe sugestões ao 
navegar pelo portal, o que reduz as oportunidades de descoberta de novos conteúdos e de 
conversão em vendas. A solução proposta permitirá a geração de recomendações contextualizadas 
ao comportamento e às preferências do usuário, criando uma jornada mais fluida e conectada 
entre as produções da Aventura.

## 2.1 Contexto da Indústria do Parceiro

&emsp;A Aventura Teatros, fundada em 2008, é referência na produção de espetáculos de teatro 
musical no Brasil e uma das principais forças da economia criativa nacional. Liderada por 
Aniela Jordan, Luiz Calainho e Giulia Jordan, a empresa evoluiu de produtora de espetáculos 
para uma plataforma cultural completa, atuando na produção de festivais, gestão de 
equipamentos culturais e formação de parcerias estratégicas com marcas de grande expressão, 
como BTG Pactual, YouTube, Nubank, Ri Happy e TotalEnergies.

&emsp;O setor de entretenimento cultural no Brasil tem passado por uma transformação significativa 
nos últimos anos, impulsionada pela retomada do consumo pós-pandemia e pela crescente 
digitalização dos canais de relacionamento com o público. Nesse cenário, plataformas digitais 
tornaram-se protagonistas na descoberta e no consumo de conteúdo cultural, exigindo das 
produtoras uma presença online estruturada e capaz de engajar o usuário além do momento da 
compra do ingresso.


&emsp;A Aventura acumula mais de 50 produções, entre espetáculos inéditos e versões da Broadway, 
além de parcerias com a Disney-Pixar e a responsabilidade pela produção da Cia de Ballet 
Dallal Achcar. Com mais de 5 mil apresentações realizadas, 5 milhões de espectadores e 
aproximadamente 16 mil empregos diretos e indiretos gerados, a empresa se consolidou também 
como a maior gestora de naming rights em equipamentos culturais do Brasil. Esse porte e essa 
diversidade de portfólio tornam a ausência de um sistema de recomendação digital uma lacuna 
estratégica relevante, tanto do ponto de vista da experiência do usuário quanto do potencial 
de geração de receita.

## 2.2 Problema

&emsp;O Portal Aventura, principal canal digital da Aventura Teatros, não conta com um sistema 
estruturado de recomendação de espetáculos e conteúdos. Atualmente, um usuário que acessa 
a página de um espetáculo específico encerra sua jornada naquele ponto, sem receber sugestões 
de outras produções, sem ser conduzido a experiências relacionadas e sem estímulo para 
explorar o ecossistema cultural que a Aventura oferece. Isso representa uma ruptura 
significativa na jornada do usuário e uma oportunidade de conversão desperdiçada.

&emsp;A relevância desse problema se amplifica quando considerado o porte da empresa: com mais de 
50 produções e uma plataforma cultural diversificada, a Aventura possui um catálogo rico e 
variado, porém subaproveitado digitalmente. A ausência de recomendações impede que esse 
catálogo trabalhe de forma ativa na geração de novas descobertas e vendas.

&emsp;Os principais desafios associados ao problema incluem a heterogeneidade do público, que 
abrange desde frequentadores assíduos de teatro até visitantes casuais sem histórico no 
portal, a potencial escassez de dados comportamentais estruturados sobre os usuários e a 
necessidade de que as recomendações sejam relevantes tanto para quem compra para si quanto 
para quem adquire ingressos para terceiros.

&emsp;Um sistema de recomendação bem construído pode aumentar o tempo de permanência no portal, 
fortalecer o engajamento com o ecossistema Aventura, criar uma jornada mais fluida entre 
produções e impulsionar a venda de ingressos, uma prioridade estratégica declarada pela 
empresa. Além disso, o projeto se insere em um momento de rebranding da Aventura, tornando-se 
uma ferramenta com potencial de reforçar a identidade da produtora como plataforma cultural 
completa.

## 2.3 Visão do Projeto e do Produto

Este tópico apresenta a visão estratégica do produto proposto, bem como seus objetivos mensuráveis, alinhando a solução ao problema identificado, às necessidades do negócio da Aventura e ao valor esperado para os usuários. A definição desses elementos é fundamental para orientar o desenvolvimento da solução, garantir coerência nas decisões de produto e estabelecer critérios claros de sucesso com base em resultados mensuráveis.

A partir desse direcionamento, busca-se não apenas descrever o que será desenvolvido, mas explicitar o papel estratégico do produto dentro do ecossistema digital da Aventura, evidenciando seu impacto potencial em engajamento, descoberta de conteúdo e conversão:

### 2.3.1 Visão do Produto

A visão do produto é transformar o Portal Aventura de uma plataforma estática de navegação em um ambiente digital inteligente, personalizado e orientado à descoberta, capaz de conectar usuários a experiências culturais relevantes de forma contínua, contextualizada e escalável.

Atualmente, a experiência do portal é fragmentada e limitada à navegação direta por páginas específicas, sem mecanismos que incentivem a exploração ou ampliem a jornada do usuário. O produto propõe uma reengenharia dessa dinâmica, introduzindo um sistema de recomendação que atua como uma camada estratégica de curadoria, responsável por guiar o usuário ao longo de uma experiência mais fluida, conectada e orientada à decisão.

A solução conceitualmente funciona como uma camada de inteligência sobre o conteúdo existente, estruturando e relacionando espetáculos, conteúdos e experiências com base em critérios de relevância, contexto e comportamento. Ao fazer isso, transforma a navegação em uma jornada progressiva de descoberta, reduzindo fricções e ampliando oportunidades de engajamento.

Mais do que melhorar a navegação, o produto reposiciona o Portal Aventura como um hub digital de experiências culturais, fortalecendo seu papel dentro do ecossistema da empresa e aumentando sua capacidade de gerar valor tanto para usuários quanto para o negócio.

---

### 2.3.2 Proposta de Valor

O produto entrega valor ao:

- Reduzir o esforço do usuário na descoberta de experiências culturais  
- Aumentar a relevância do conteúdo apresentado  
- Expandir a jornada de navegação dentro do portal  
- Potencializar a exposição de todo o portfólio da Aventura  
- Converter engajamento em oportunidades reais de negócio  

A proposta central é transformar atenção em descoberta e descoberta em conversão.

---

### 2.3.3 O que o Produto Faz e Não Faz
## O que o produto faz

O produto implementa um sistema de recomendação focado na descoberta de espetáculos dentro do Portal Aventura, com aplicação direta nas telas em que a recomendação agrega valor à navegação:

- Na `Tela principal de descoberta`, exibe listas e carrosséis de espetáculos recomendados, organizados por relações com o perfil e o comportamento do usuário.
- Na `Sub-tela de cada espetáculo`, apresenta um carrossel de sugestões como `Espetáculos similares` ou `Você também pode gostar`, ampliando a exploração de conteúdos relacionados.
- No `Login/Cadastro` e `Seleção de tags`, evita o cold start com coleta mínima de informações e preferências iniciais, sem sobrecarregar essas etapas com recomendações, porém garantindo que o usuário tenha acesso à peças que se encaixem nos gêneros escolhidos.

As recomendações seguem esta ordem de prioridade no MVP:

1. Filtragem colaborativa, baseada em similaridade entre usuários e regras inspiradas em SVD.
2. Sinais de navegação, considerando tempo de permanência nas páginas e cliques.
3. Histórico de interações, reforçando conteúdos já acessados pelo usuário.

Além disso, o produto disponibiliza um protótipo navegável com fluxo de descoberta entre a tela principal e as páginas de espetáculo, permitindo validar a experiência e o comportamento das recomendações.

## O que o produto não faz

- Não cobre, neste MVP, processos transacionais, operacionais ou administrativos que extrapolem a jornada de descoberta de espetáculos.
- Não entrega, nesta etapa, uma solução completa de gestão, análise ou automação do ecossistema do portal.
- Não pretende substituir integralmente a curadoria humana nem eliminar a necessidade de revisão manual das recomendações quando necessário.

---

### 2.3.4 Objetivos Estratégicos e Mensuráveis

O sucesso do produto será medido por indicadores de impacto real no comportamento do usuário e nos resultados de negócio.

Esses objetivos refletem não apenas melhorias na experiência do usuário, mas também impactos diretos nos resultados de negócio, especialmente em engajamento, retenção e conversão de vendas:

#### Engajamento e Experiência

- Aumentar em **20%** o tempo médio de sessão  
- Aumentar em **25%** o número de páginas por sessão  
- Aumentar a taxa de interação com recomendações  

#### Descoberta de Conteúdo

- Aumentar em **30%** a navegação entre diferentes espetáculos  
- Aumentar a diversidade de conteúdos acessados por usuário  
- Reduzir a dependência de acessos concentrados em poucos conteúdos  

#### Conversão e Valor de Negócio

- Aumentar em **10% a 15%** a taxa de conversão associada a recomendações  
- Aumentar o número de sessões que evoluem para intenção de compra  
- Estimular a venda cruzada entre espetáculos  

#### Retenção e Recorrência

- Aumentar a taxa de retorno de usuários ao portal  
- Melhorar a continuidade da jornada entre sessões  

---

### 2.3.5 Impacto e Abrangência

A solução foi concebida para atender uma base ampla e heterogênea de usuários, com diferentes níveis de familiaridade, objetivos e contextos de uso.

Ao introduzir recomendações contextualizadas, o produto:
- Reduz barreiras de entrada para novos usuários  
- Amplia a relevância para usuários recorrentes  
- Adapta a experiência a diferentes momentos de navegação  
- Garante acessibilidade e clareza na exploração de conteúdos  
- Atende à diversidade de perfis de usuários e suas diferentes expectativas  

Isso amplia significativamente o alcance e o impacto do portal como plataforma digital.

---

### 2.3.6 Ownership e Accountability Estratégico

A responsabilidade estratégica do produto é compartilhada em duas camadas:

**Stakeholders da Aventura:**
- Validam alinhamento com objetivos de negócio  
- Aprovam mudanças de escopo relevantes  
- Avaliam impacto em métricas de negócio  

**Equipe do Projeto:**
- Define e prioriza funcionalidades dentro do MVP  
- Traduz objetivos estratégicos em decisões de produto  
- Documenta trade-offs e decisões técnicas  
- Conduz validação da solução proposta  

**Tomada de decisão:**
- Estratégia e escopo macro -> Stakeholders Aventura  
- Execução e priorização -> Equipe do projeto  
- Validação de valor -> Compartilhada  

Essa estrutura garante alinhamento com o negócio sem comprometer a agilidade de execução.

---

### 2.3.7 Alinhamento com o Problema

O produto responde diretamente à ausência de um sistema estruturado de recomendação no Portal Aventura, que atualmente limita:

- A descoberta de novos conteúdos  
- O engajamento contínuo do usuário  
- O potencial de conversão  

A solução resolve esse cenário ao introduzir inteligência, conexão e orientação na jornada do usuário.

---

O produto posiciona o Portal Aventura como uma plataforma orientada à descoberta, capaz de transformar a forma como usuários exploram experiências culturais.

Ao conectar inteligência de recomendação com objetivos de negócio, a solução não apenas melhora a experiência do usuário, mas também cria impacto direto em engajamento, retenção e conversão, consolidando o portal como um ativo estratégico da Aventura.

---

## 2.4 Personas e Jornada do Usuário
_conteúdo_

## 2.5 Modelagem do Fluxo de Negócio BPMN

Este documento descreve o **fluxo de negócio** modelado em **BPMN 2.0** para o sistema **Spotlight**, solução de **recomendação inteligente de espetáculos** desenvolvida para o **Portal Aventura**. O diagrama é composto por três pools que representam os três atores principais do processo: o **Cliente**, o **Sistema de Recomendação** e os **Gestores de Dados**.
O fluxo modela a jornada completa, desde o primeiro acesso ao portal até a compra do ingresso ou avaliação do evento, integrando o **motor de ML** e o pipeline de dados que sustenta as recomendações.

---

### 2.5.1. Análise As Is / To Be — Antes e Depois do Spotlight

Esta seção descreve como o processo de navegação e descoberta de espetáculos funciona **hoje** no Portal Aventura (As Is) e como ele passará a funcionar **após a implementação do Spotlight** (To Be). A comparação justifica a existência do projeto e fundamenta as decisões de design do fluxo modelado no BPMN.

---

#### 2.5.1.1 As Is — Como o processo funciona hoje

O Portal Aventura, em seu estado atual, não possui qualquer mecanismo estruturado de recomendação ou personalização. A jornada do usuário é inteiramente linear e passiva: o visitante acessa o portal, busca ou encontra um espetáculo específico por conta própria, visualiza sua página e decide — de forma isolada e sem apoio contextual — se compra o ingresso ou abandona o portal.

**Fluxo atual (As Is):**

```
Usuário acessa o portal
        ↓
Busca ou navega manualmente pelo catálogo
        ↓
Encontra (ou não) um espetáculo de interesse
        ↓
Visualiza a página do espetáculo
        ↓
Decide sozinho: compra o ingresso ou sai do portal
```

**Problemas identificados no processo atual:**

| Problema | Impacto para a Aventura |
|---|---|
| Ausência de recomendações personalizadas | O usuário vê apenas o que buscou e não descobre outros espetáculos do catálogo |
| Nenhuma coleta estruturada de preferências | A Aventura não sabe quais gêneros ou faixas de preço interessam a cada usuário |
| Interações não são registradas nem aproveitadas | Cliques, visualizações e compras não geram inteligência sobre o comportamento do público |
| Catálogo sem metadados para recomendação | Espetáculos não possuem tags, similaridades ou scores que permitam comparações automáticas |
| Jornada encerra sem reengajamento | Após ver um espetáculo, o usuário não recebe estímulo para explorar outros e sai do portal |
| Sem segmentação de público | A equipe de marketing não tem visibilidade sobre perfis distintos de espectadores |
| Baixo tempo de permanência | Sem descoberta guiada, as sessões são curtas e a taxa de conversão em vendas é reduzida |

---

#### 2.5.1.2 To Be — Como o processo funcionará com o Spotlight

Com a implementação do Spotlight, o Portal Aventura passa a operar com um sistema inteligente de recomendação que transforma a navegação passiva em uma **jornada de descoberta cultural contínua e personalizada**. O processo deixa de ser linear e passa a ser orientado a dados, com três atores integrados — Cliente, Sistema de Recomendação e Gestores de Dados — operando de forma coordenada.

**Fluxo futuro (To Be) — representado no diagrama BPMN:**

```
Usuário acessa o portal
        ↓
Realiza cadastro (se necessário) e efetua login
        ↓
Declara preferências de gênero e faixa de preço
        ↓
Recebe recomendações personalizadas geradas pelo motor de ML
        ↓
Interage com as sugestões (clique, favorito, avaliação)
        ↓
Cada interação retroalimenta o modelo → recomendações ficam mais precisas
        ↓
Compra ingresso ou avalia o evento
```

**Melhorias entregues pelo Spotlight (To Be):**

| Dimensão | As Is (hoje) | To Be (com Spotlight) |
|---|---|---|
| **Descoberta de espetáculos** | Manual, sem sugestões | Personalizada, baseada em perfil e histórico do usuário |
| **Coleta de preferências** | Inexistente | Onboarding estruturado: gênero favorito e faixa de preço |
| **Uso de dados comportamentais** | Não coletados | Cliques, avaliações e compras alimentam o motor de ML continuamente |
| **Jornada no portal** | Linear e curta | Contínua — o sistema sugere o próximo espetáculo, aumentando o tempo de permanência |
| **Apoio à decisão de compra** | Nenhum | Recomendações contextuais com justificativa ("Porque você gosta de musicais") |
| **Catálogo de espetáculos** | Sem metadados para recomendação | Enriquecido com tags, scores de popularidade e atributos de similaridade |
| **Segmentação de público** | Sem visibilidade | k-means agrupa automaticamente o público em clusters (Famílias, Musical Adulto, etc.) |
| **Reengajamento pós-visita** | Inexistente | Avaliações retroalimentam o modelo e sustentam o ciclo de melhoria contínua |
| **Inteligência de marketing** | Sem dados estruturados | Relatórios de segmentos entregues à equipe de marketing para campanhas direcionadas |

---

#### 2.5.1.3 Síntese da Transformação

O processo **As Is** representa uma navegação passiva onde o usuário é o único responsável pela descoberta de conteúdo. A Aventura não participa ativamente dessa jornada e perde oportunidades de venda a cada sessão encerrada sem conversão.

O processo **To Be**, modelado no diagrama BPMN do Spotlight, inverte essa lógica: o sistema passa a guiar ativamente o usuário, conectando preferências declaradas, comportamento real e catálogo estruturado para entregar recomendações que aumentam o engajamento, o tempo de permanência e a conversão em vendas de ingressos, transformando cada acesso ao portal em uma oportunidade de descoberta cultural.

---

### 2.5.2. Fluxo de Negócios BPMN

Este documento descreve o fluxo de negócio modelado em BPMN 2.0 para o sistema **Spotlight**, solução de recomendação inteligente de espetáculos desenvolvida para o Portal Aventura. O diagrama é composto por **três pools** que representam os três atores principais do processo: o **Cliente**, o **Sistema de Recomendação** e os **Gestores de Dados**.

O fluxo modela a jornada completa, desde o primeiro acesso ao portal até a compra do ingresso ou avaliação do evento, integrando o motor de ML e o pipeline de dados que sustenta as recomendações.

Para visualizar o diagrama completo, basta acessar o [Diagrama BPMN](../documentos/img/BPMN.pdf) no repositório.

---

#### 2.5.2.1 Pools

| Pool | Ator | Responsabilidade |
|---|---|---|
| **Pool 1** | Cliente | Representa toda a jornada do usuário no portal: cadastro, preferências, visualização de recomendações e interação com os espetáculos |
| **Pool 2** | Sistema de Recomendação | Motor de ML responsável por capturar interações, analisar o perfil do usuário, gerar e exibir recomendações personalizadas |
| **Pool 3** | Gestores de Dados | Equipe interna responsável por coletar dados de novos espetáculos, popular o banco de dados e disponibilizar o catálogo atualizado para o motor |

---

#### 2.5.2.2. Descrição Detalhada dos Fluxos

#### Pool 1 — Cliente

Este pool representa a experiência do usuário no Portal Aventura. O fluxo é iniciado quando o usuário acessa o portal e segue até uma das três ações finais possíveis: compra de ingresso, avaliação do evento ou encerramento da sessão sem engajamento.

<div align="center">

<sub>Figura X - Cliente </sub><br>
 
  <img src="../documentos/img/pool1.png" width="100%" alt="Cliente"><br>

  <sup>Fonte: Material produzido pelos autores, 2026.</sup>
</div>

**Passo a passo:**

**1. Acesso ao Portal**
O processo tem início com um evento de start representando o momento em que o usuário acessa o Portal Aventura. Este é o ponto de entrada de toda a jornada.

**2. Gateway — Possui cadastro?**
Imediatamente após o acesso, um gateway XOR verifica se o usuário já possui cadastro na plataforma:
- **Sim** → o usuário segue diretamente para a etapa de login.
- **Não** → o usuário é direcionado para preencher o cadastro, retornando ao fluxo principal após a conclusão.

**3. Efetuar Login**
Usuários já cadastrados realizam o login na plataforma para ter acesso ao ambiente personalizado e às recomendações vinculadas ao seu perfil.

**4. Inserir Preferências**
Após autenticado, o usuário declara suas preferências culturais, como gêneros de espetáculo de interesse. Essa etapa é fundamental para o motor de recomendação, especialmente para usuários que ainda não possuem histórico de interações (cold start).

**5. Inserir Faixa de Preço**
O usuário informa a faixa de preço que está disposto a pagar pelos ingressos. Esse filtro é utilizado pelo sistema para calibrar as recomendações geradas, garantindo que apenas espetáculos dentro do orçamento declarado sejam sugeridos.

**6. Visualizar Recomendações**
Com o perfil declarado completo, o sistema exibe as recomendações personalizadas na tela de descoberta, em formato de carrossel, com a justificativa de cada sugestão para tornar o sistema transparente ao usuário.

**7. Gateway — Houve interação?**
Um segundo gateway XOR avalia se o usuário interagiu com alguma recomendação exibida:
- **Não** → a sessão é encerrada sem engajamento.
- **Sim** → o usuário é direcionado para a tela de detalhes do evento.

**8. Ver Detalhes do Evento**
O usuário acessa a página descritiva do espetáculo selecionado, contendo informações como sinopse, elenco, datas, local e preço do ingresso.

**9. Gateway Inclusivo**
Um terceiro gateway inclusivo captura a decisão do usuário após visualizar os detalhes do evento:
- **Clicar no link da ticketeira** → o usuário é redirecionado para a plataforma de venda de ingressos, onde finaliza a compra. O fluxo termina com o evento de end "Ingresso Comprado".
- **Avaliar evento** → o usuário registra uma avaliação do espetáculo, que retroalimenta o sistema de recomendação com dados de satisfação.

---

#### Pool 2 — Sistema de Recomendação

Este pool representa o motor de ML do Spotlight. Ele é ativado pelas interações do usuário no Pool 1 e é responsável por transformar dados de comportamento e perfil em recomendações personalizadas.

<div align="center">

<sub>Figura X - Sistema de Recomendação</sub><br>

  <img src="../documentos/img/pool2.png" width="100%" alt="Sistema de Recomendação"><br>

  <sup>Fonte: Material produzido pelos autores, 2026.</sup>
</div>

**Passo a passo:**

**1. Motor ML (Start)**
O fluxo do sistema é iniciado pelo evento de start "Motor LM", acionado quando o usuário conclui o preenchimento de suas preferências e dados de perfil no Pool 1.

**2. Captura de Interações**
O sistema captura e registra todos os eventos de comportamento do usuário: visualizações de espetáculos, cliques, tempo de permanência na página, avaliações e compras. Cada tipo de interação contribui para a construção do perfil comportamental do usuário.

**3. Análise de Interações**
Os eventos capturados são processados e classificados por tipo e relevância. Essa etapa transforma os dados brutos de comportamento em sinais estruturados que alimentam a etapa seguinte de análise de perfil.

**4. Análise de Perfil**
Com base nas interações processadas e nas preferências declaradas, o sistema constrói ou atualiza o perfil do usuário. Para novos usuários (sem histórico), a análise utiliza as preferências declaradas. Para usuários recorrentes, o perfil é enriquecido com dados comportamentais acumulados ao longo das sessões.

**5. Gerar Recomendações**
Com o perfil consolidado, o motor gera uma lista rankeada de recomendações personalizadas, acompanhada da justificativa de cada sugestão para tornar o sistema explicável ao usuário.

**6. Exibir Recomendações na Tela de Descoberta**
As recomendações geradas são entregues ao Pool 1 (tela "Visualizar Recomendações"), exibidas na interface do portal. O fluxo retorna ao controle do usuário a partir deste ponto.

---

#### Pool 3 — Gestores de Dados

Este pool representa o processo interno da equipe de gestão de dados da Aventura, responsável por manter o catálogo de espetáculos atualizado e disponível para o motor de recomendação.

<div align="center">

<sub>Figura X - Gestores de Dados</sub><br>

  <img src="../documentos/img/pool3.png" width="100%" alt="Gestores de Dados"><br>

  <sup>Fonte: Material produzido pelos autores, 2026.</sup>
</div>

**Passo a passo:**

**1. Novo Espetáculo**
O fluxo é iniciado pelo evento de start "Novo espetáculo", acionado sempre que um novo espetáculo é produzido ou anunciado pela Aventura Teatros.

**2. Coletar Dados de um Novo Espetáculo**
A equipe realiza a coleta de todas as informações relevantes sobre o espetáculo: título, gênero, sinopse, elenco, teatro, datas de apresentação, faixa de preço e classificação etária.

**3. Popular no Banco de Dados**
Os dados coletados são inseridos e organizados no banco de dados relacional, respeitando o modelo de entidades do sistema.

**4. Inserir Metadados do Espetáculo**
Além dos dados básicos, são inseridos os metadados que alimentam o motor de recomendação: tags de categoria, palavras-chave, score de popularidade inicial e atributos de similaridade com outros espetáculos do catálogo.

**5. Disponibilizar o Novo Espetáculo no Catálogo**
O espetáculo é publicado e disponibilizado no catálogo ativo do portal. A partir deste momento, ele passa a ser elegível para aparecer nas recomendações geradas pelo motor de ML.

**6. Catálogo Disponível (End)**
O fluxo encerra com o evento de end "Catálogo disponível", sinalizando que o novo espetáculo está pronto para ser consumido pelo Pool 2.

---

#### 2.5.2.3. Eventos de Início e Fim

| Pool | Evento de Start | Evento(s) de End |
|---|---|---|
| Cliente | Acesso ao Portal | Ingresso Comprado (fim positivo) / Sem engajamento (fim sem conversão) |
| Sistema de Recomendação | Motor LM | Exibição de recomendações na tela de descoberta (entrega ao Pool 1) |
| Gestores de Dados | Novo evento | Catálogo disponível (fim de processo interno) |

---

#### 2.5.2.4. Conexões Entre Pools 

As conexões entre pools representam a troca de informações entre atores distintos do processo. Na notação BPMN 2.0, essas conexões são representadas por linhas tracejadas (message flows) e nunca por sequence flows, que são exclusivos de cada pool.

| Origem | Destino | Informação Trafegada |
|---|---|---|
| Pool 1 — Cliente | Pool 2 — Sistema | Preferências declaradas e faixa de preço do usuário |
| Pool 1 — Cliente | Pool 2 — Sistema | Eventos de interação (clique, avaliação, tempo de permanência) |
| Pool 2 — Sistema | Pool 1 — Cliente | Lista de recomendações personalizadas com justificativas |
| Pool 3 — Gestores de Dados | Pool 2 — Sistema | Notificação de novo espetáculo disponível no catálogo |

---

#### 2.5.2.5. Dados e Armazenamento

| Dado | Origem | Utilização |
|---|---|---|
| Preferências declaradas | Pool 1 — Usuário | Alimenta a análise de perfil para usuários sem histórico (cold start) |
| Faixa de preço | Pool 1 — Usuário | Filtro aplicado antes da geração das recomendações |
| Interações comportamentais | Pool 1 — Usuário | Alimenta a análise de interações e enriquece o perfil do usuário |
| Avaliações dos eventos | Pool 1 — Usuário | Retroalimenta o modelo de recomendação com dados de satisfação |
| Metadados dos espetáculos | Pool 3 — Gestores de Dados | Base do catálogo consumida pelo motor de ML para gerar recomendações |

---

#### 2.5.2.6. Observações Finais

- O diagrama segue a notação BPMN 2.0 com uso correto de **pools separados por ator**, **gateways XOR para decisões exclusivas** e **eventos de start e end** distintos por pool.
- O Pool 2 apresenta uma separação clara entre **captura**, **análise de interações**, **análise de perfil** e **geração de recomendações**, o que demonstra maturidade no design do fluxo e facilita a implementação modular do motor de ML.
- O **Pool 3 opera de forma assíncrona** em relação aos outros dois: ele é acionado sempre que há um novo espetáculo, independentemente do fluxo de navegação do usuário no portal.
- O diagrama representa o **fluxo principal (happy path)** do sistema. Fluxos de exceção podem ser detalhados em subprocessos futuros.
- A etapa de **Avaliar evento** no Pool 1 cria um ciclo virtuoso de retroalimentação: as avaliações dos usuários enriquecem o modelo de recomendação ao longo do tempo, tornando as sugestões progressivamente mais precisas e relevantes.

---

## 2.6 Matriz de Risco do Projeto
_conteúdo_

## 2.7 Ideação
_conteúdo_
Obs.: Seção dedicada a discussão sobre ideias e priorização de features.

## 2.8 Canvas do Projeto
_conteúdo_

# 3. Requisitos do Projeto
_conteúdo_

## 3.1 Requisitos Funcionais (RFs)
_conteúdo_ 

## 3.2 Requisitos Não Funcionais (RNFs)
_conteúdo_

## 3.3 Correlação RFs e RNFs
_conteúdo_

## 3.4 Casos de Uso
_conteúdo_

## 3.5 Casos de Uso x Requisitos Funcionais
_conteúdo_

# 4. Modelagem de Dados
_conteúdo_

## 4.1 Especificação da Base de Dados para Modelo de Recomendação
Este item reaproveita e formaliza as fontes de dados necessárias para o MVP de recomendações (apenas telas essenciais).

Tabelas mínimas propostas:

- `espetaculos` (catálogo):
  - `id` (PK), `titulo`, `genero`, `regiao`, `tipo_producao`, `descricao_curta`, `datas_exibicao`, `imagens`, `tags` (campo para keywords/metadados).
- `interacoes` (eventos de navegação):
  - `event_id` (PK), `anon_user_id` (quando houver), `session_id`, `espetaculo_id` (FK), `evento` (view/click), `duracao` (segundos), `timestamp`.
- `preferencias_iniciais` (onboarding):
  - `anon_user_id`, `tag`, `peso` — registro das escolhas na tela de seleção de tags.
- `agregados_item` (materialized/denormalized, apoio ao read):
  - `espetaculo_id`, `views_total`, `avg_duracao`, `ultimo_update`.

Notas de pré-processamento e uso (Sprint 1):

- ETL em batch para consolidar logs de navegação e popular `interacoes` e `agregados_item` (rotina diária ou conforme necessidade).
- Normalização de títulos e tags; extração de tokens para matching por similaridade de conteúdo.
- Geração de matriz esparsa usuário×item (ou item×item) a partir de `interacoes` e `preferencias_iniciais` para cálculo de similaridade; aplicar SVD leve ou heurísticas de similaridade conforme escopo.
- Privacidade: usar `anon_user_id` e evitar PII no MVP.

Artefatos entregáveis nesta Sprint:

- Scripts SQL de criação e carga inicial: [database/01_create_database.sql](database/01_create_database.sql) e [database/02_initial_data.sql](database/02_initial_data.sql).
- Script/notebook de pré-processamento para geração de agregados e matrizes de similaridade.

Esta especificação está alinhada com o escopo mínimo definido para as telas essenciais; melhorias (analytics, pipelines ML, integrações transacionais) ficam para fases posteriores.
## 4.2 Modelo Conceitual de Dados
_conteúdo_

## 4.3 Modelo Lógico de Dados
_conteúdo_

## 4.4 Modelo Físico de Dados
_conteúdo_

**Nota:** Insira uma explicação e direcionamento para o readme.md da pasta database.

# 5. Solução Técnica (Design)
_conteúdo_

## 5.1 Diagrama de Classes
_conteúdo_

## 5.2 Diagrama de Componentes da UML
_conteúdo_

## 5.3 Diagramas de Sequência da UML
_conteúdo_

# 6. Mapeamento Técnico de Infraestrutura e Implantação
_conteúdo_

## 6.1 Diagrama de Implantação da UML
_conteúdo_

## 6.2 Justificativa das Escolhas de Implantação
_conteúdo_

## 6.3 Considerações sobre Desempenho e Segurança
_conteúdo_

# 7. Projeto Visual da Solução
_conteúdo_

## 7.1 Desenvolvimento de Wireframes
_conteúdo_

## 7.2 Desenvolvimento de Mockups
_conteúdo_

## 7.3 Guia Visual
_conteúdo_

# 8. Desenvolvimento do Projeto
_conteúdo_

## 8.1 Arquitetura de Codificação e Estrutura de Diretórios
_conteúdo_

## 8.2 Modelo de Recomendação
_conteúdo_

## 8.3 Desenvolvimento de Features
_conteúdo_

**Nota:** Insira uma explicação de entregas em cada Sprint.

## 8.3.1 Sprint 3
_conteúdo_

## 8.3.2 Sprint 4
_conteúdo_

## 8.3.3 Sprint 5
_conteúdo_

## 8.4 Testes Unitários e de Integração
_conteúdo_
Obs.: Insira informações sobre estratégias para realização dos testes

## 8.5 Documentações automáticas
_conteúdo_
Obs.: Insira informações sobre quais frameworks foram usados e como acessar.

# 9. Planejamento e Execução de Testes
_conteúdo_

## 9.1 Testes Funcionais
_conteúdo_

## 9.1.1 Planejamento
_conteúdo_

## 9.1.2 Resultados
_conteúdo_

## 9.2 Testes de RNFs
_conteúdo_

## 9.2.1 Planejamento
_conteúdo_

## 9.2.2 Resultados
_conteúdo_

## 9.3 Testes de Usabilidade
_conteúdo_

## 9.3.1 Planejamento
_conteúdo_

## 9.3.2 Resultados
_conteúdo_

# 10. Procedimentos de Implantação
_conteúdo_

## 10.1 Implantação e Configuração do Banco de Dados
_conteúdo_

## 10.2 Implantação do Protótipo para uso por equipe de desenvolvimento
_conteúdo_

# Referências
_conteúdo_

