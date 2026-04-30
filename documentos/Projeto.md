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

## 2.5 Modelagem do Fluxo de Negócio
_conteúdo_

## 2.5.1 Cadeia de Valor
_conteúdo_

## 2.5.2 Fluxo de Negócio Proposto (AS-IS e TO-BE)
_conteúdo_

## 2.6 Matriz de Risco do Projeto
_conteúdo_

## 2.7 Ideação
_conteúdo_
Obs.: Seção dedicada a discussão sobre ideias e priorização de features.

## 2.8 Business Model Canvas

Esta seção detalha o modelo de negócios desenvolvido para o **Spotlight**, **sistema inteligente de recomendação** no **Portal Aventura**. A solução atua em duas frentes: aprimorar a experiência do usuário final e fornecer inteligência de dados para a equipe interna.

<div align="center">

<sub>Figura X - Business Model Canvas</sub><br>

  <img src="../documentos/img/bmn.png" width="100%" alt="Business Model Canvas"><br>

  <sup>Source: Material produzido pelos autores, 2026.</sup>

</div>


### 2.8.1 Parceiros-Chave 

Os parceiros estratégicos essenciais para a operação, fornecimento de dados e viabilidade do projeto:

- **Aventura Teatros**: A parceira principal e dona do ecossistema. Fornece o portal, o catálogo de espetáculos e o direcionamento de negócios.
- **Ticketeiras parceiras**: Plataformas parceiras de emissão de ingressos.
- **Equipe de Marketing da Aventura**: Parceiro interno vital para a validação das campanhas. Atuam tanto como co-criadores das regras de negócio quanto como validadores da eficácia da segmentação gerada pelo sistema.

---

### 2.8.2 Atividades Principais

As ações técnicas e operacionais contínuas que o sistema e a equipe devem executar para entregar a proposta de valor:

- **Coleta e processamento de dados de comportamento do usuário**: Criação de pipelines de dados para capturar cliques, tempo de tela, buscas e histórico de compras. Envolve processos de ETL e higienização dos dados para garantir a qualidade das entradas.
- **Treinamento e atualização do modelo de recomendação**: Rotinas de MLOps para retreinar os algoritmos de Machine Learning periodicamente, garantindo que as recomendações não fiquem defasadas e se adaptem a novos espetáculos e tendências de consumo.

---

### 2.8.3 Principais Recursos 

Os ativos tecnológicos e de informação indispensáveis para o funcionamento da plataforma:

- **Dados de interação do usuário**: O principal combustível do motor de recomendação. Sem um volume adequado de dados comportamentais (logs, sessões, cliques), o modelo não consegue inferir preferências.
- **Motor de ML (Machine Learning)**: A infraestrutura de algoritmos (como filtragem colaborativa ou baseada em conteúdo) responsável por calcular a similaridade entre usuários e espetáculos, gerando as predições.
- **Catálogo de espetáculos e metadados da Aventura**: O banco de dados estruturado contendo descrições, gêneros, elenco, datas, classificação indicativa e tags de cada peça teatral.

---

### 2.8.4 Propostas Principais 

O valor entregue para os diferentes segmentos de clientes (espectadores e equipe interna):

- **Descoberta personalizada de espetáculos**: Supera o problema da "página em branco", sugerindo peças que o usuário não procuraria ativamente, mas que se alinham perfeitamente ao seu gosto.
- **Aumento do tempo de permanência no Portal Aventura**: Ao oferecer um feed contínuo e relevante, o usuário consome mais conteúdo sobre as produções antes de decidir.
- **Estímulo à compra de ingressos por recomendação contextual**: Inserção de "gatilhos" de compra no momento exato de maior interesse, aumentando a taxa de conversão.
- **Jornada cultural contínua**: Transforma uma compra isolada em um ciclo de consumo recorrente, fidelizando o cliente ao selo Aventura.
- **Segmentação inteligente de público (B2B)**: Permite que o marketing abandone o anúncios genéricos e faça campanhas ultra-segmentadas baseadas nos clusters gerados pelo ML.
- **Explicabilidade das recomendações**: Transparência para o usuário ("Recomendado porque você assistiu X") e para os gestores, aumentando a confiança nas predições do sistema.

---

### 2.8.5 Relacionamento com o Consumidor

Como o sistema interage e cria laços com os usuários para melhorar a coleta de dados e a experiência:

- **Onboarding de preferências**: Um formulário rápido e gamificado no primeiro acesso ou criação de conta para mitigar o problema de Cold Start.
- **Recomendação personalizada em tempo real**: A interface do portal se molda dinamicamente conforme o usuário navega, gerando uma sensação de atendimento exclusivo.
- **Notificações de novos espetáculos por cluster**: Engajamento ativo informando aos usuários que uma peça do seu nicho de interesse entrou em cartaz.

---

### 2.8.6 Canais 

- **Portal Aventura**: O ponto de contato exclusivo e centralizado. Toda a inteligência do motor de recomendação será exposta no front-end deste portal, seja na home page, páginas de detalhes de peças ou no carrinho de compras.

---

### 2.8.7 Segmentos de Clientes 

Os grupos de pessoas que a solução visa atender, divididos em perfis de consumo e usuários internos:

### Consumidores Finais

- Famílias com crianças (buscam classificação livre e horários diurnos).
- Adultos fãs de musical (ticket médio maior, alta recorrência).
- Público de ballet e clássicos (nicho específico e tradicional).
- Jovens e público experimental (abertos a novas linguagens teatrais).
- Visitantes esporádicos do portal (usuários não logados que precisam ser convertidos rapidamente).

### Clientes Internos 

- Gestores e analistas de marketing da Aventura.

---

### 2.8.8 Estrutura de Custos 

Os principais direcionadores de despesas para manter o projeto rodando:

- **Infraestrutura de nuvem**: Custos com servidores para hospedar o portal, bancos de dados para armazenar logs massivos e instâncias de computação otimizadas para treinar os modelos de Machine Learning.
- **Desenvolvimento e manutenção do modelo de ML**: Esforço contínuo para refinar algoritmos, evitar degradação do modelo e realizar testes A/B.
- **Horas de desenvolvimento**: Custo da equipe de engenharia de software envolvida na criação de APIs, integração com o Sympla, estruturação de banco de dados e rotinas de deploy.

---

### 2.8.9 Fluxos de Receita

Como a solução gera valor financeiro ou reduz custos, justificando o investimento:

- **Aumento de conversão em venda de ingressos**: O impacto direto das recomendações personalizadas na jornada, diminuindo a taxa de abandono do carrinho.
- **Maior ticket médio por sessão**: Consequência de cross-sell e up-sell bem-sucedidos.
- **Campanhas pagas eficientes**: Redução do Custo de Aquisição de Clientes e aumento do Retorno sobre Investimento em mídias pagas, utilizando os dados de segmentação para criar públicos personalizados e mais assertivos.

--

# 3. Requisitos do Projeto
_conteúdo_

## 3.1 Requisitos Funcionais (RFs)

 &emsp; Os requisitos funcionais representam as funcionalidades que o sistema deverá oferecer para atender os objetivos e necessidades dos usuários da plataforma de recomendação de espetáculos da Aventura. Cada requisito define uma funcionalidade essencial do sistema, considerando tanto a experiência do usuário quanto os mecanismos de recomendação e gestão administrativa. Os testes descritos incluem pré-condição, procedimento, resultado esperado e pós-condição, garantindo a validação das funcionalidades implementadas.

<div align="center"> <sub>Tabela X - Requisitos funcionais</sub>


## Requisitos Funcionais

### Autenticação e Perfil

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF01 | Como usuário, eu quero me cadastrar na plataforma informando meus dados pessoais (como nome, email e senha) para acessar funcionalidades personalizadas e utilizar o sistema de recomendação. | Usuário não autenticado. | Inserir dados válidos e inválidos e submeter cadastro. | Usuário cadastrado com sucesso quando dados são válidos; mensagens de erro exibidas para campos inválidos. | Usuário registrado no sistema. |
| RF02 | Como usuário, eu quero realizar login informando meu email e senha para acessar minha conta e meus dados personalizados. | Usuário previamente cadastrado. | Inserir credenciais válidas e inválidas. | Acesso permitido com credenciais válidas; erro exibido para dados incorretos. | Sessão autenticada iniciada. |
| RF03 | Como usuário, eu quero definir minhas preferências (como gênero, faixa de preço e localização) para receber recomendações mais relevantes e alinhadas ao meu perfil. | Usuário autenticado. | Selecionar preferências e salvar. | Preferências armazenadas corretamente no sistema. | Perfil do usuário atualizado. |

---

### Exploração e Navegação

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF04 | Como usuário, eu quero visualizar recomendações personalizadas com base no meu perfil e comportamento na plataforma para descobrir espetáculos alinhados aos meus interesses. | Usuário autenticado com ou sem histórico de interações. | Acessar tela inicial do sistema. | Lista com no mínimo 5 recomendações exibida, ordenada por relevância, contendo título, categoria, data e indicação de personalização (ex: “baseado nas suas preferências”). | Recomendações exibidas ao usuário. |
| RF05 | Como usuário, eu quero pesquisar espetáculos por nome ou palavra-chave para encontrar conteúdos específicos. | Base de espetáculos disponível. | Inserir termo de busca válido e inválido. | Lista de resultados correspondente ao termo pesquisado ou mensagem de “nenhum resultado encontrado”. | Lista filtrada exibida. |
| RF06 | Como usuário, eu quero filtrar espetáculos por categoria, data e faixa de preço para refinar minha busca. | Lista de espetáculos carregada. | Aplicar diferentes combinações de filtros. | Lista atualizada conforme filtros selecionados. | Lista refinada exibida ao usuário. |

---

### Conteúdo

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF07 | Como usuário, eu quero visualizar detalhes completos de um espetáculo para avaliar se tenho interesse em assisti-lo. | Espetáculo disponível no sistema. | Acessar página de detalhes do espetáculo. | Exibição de informações completas como descrição, categoria, data, horário, preço e local. | Dados apresentados ao usuário. |
| RF08 | Como usuário, eu quero visualizar avaliações de outros usuários para apoiar minha decisão sobre assistir ou não a um espetáculo. | Avaliações cadastradas para o espetáculo. | Acessar seção de avaliações. | Lista de avaliações exibida corretamente, contendo nota e comentário. | Avaliações visíveis ao usuário. |

---

### Engajamento

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF09 | Como usuário, eu quero avaliar espetáculos atribuindo uma nota para expressar minha opinião e contribuir com o sistema. | Usuário autenticado. | Inserir nota válida e submeter. | Avaliação registrada corretamente no sistema. | Avaliação armazenada. |
| RF10 | Como usuário, eu quero comentar sobre espetáculos para compartilhar minha experiência com outros usuários. | Usuário autenticado. | Inserir comentário válido e enviar. | Comentário exibido corretamente na página do espetáculo. | Comentário registrado no sistema. |

---

### Coleta de Dados

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF11 | Como sistema, eu quero registrar interações dos usuários (como cliques, buscas, visualizações e avaliações) para alimentar e melhorar continuamente o algoritmo de recomendação. | Usuário interagindo com a plataforma. | Realizar diferentes tipos de interação no sistema. | Registro contendo ID do usuário, ID do espetáculo, tipo de ação e timestamp, sem impacto perceptível na navegação. | Dados armazenados para uso no algoritmo. |

---

### Recomendação

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF12 | Como sistema, eu quero gerar recomendações personalizadas com base nas preferências declaradas e no histórico de interações do usuário para oferecer sugestões relevantes e contextualizadas. | Dados de preferências e histórico disponíveis. | Gerar recomendações para usuário com histórico. | Lista de recomendações personalizada exibida corretamente. | Recomendações geradas. |
| RF13 | Como sistema, eu quero atualizar as recomendações após novas interações do usuário para tornar as sugestões progressivamente mais precisas ao longo do uso. | Novas interações registradas. | Realizar nova interação e atualizar recomendações. | Recomendações ajustadas conforme comportamento recente do usuário. | Recomendações atualizadas. |
| RF14 | Como sistema, eu quero recomendar conteúdos com base em usuários com perfis e comportamentos semelhantes (filtragem colaborativa). | Base de usuários com interações registrada. | Comparar perfis e gerar recomendações. | Itens recomendados com base na similaridade entre usuários. | Recomendações exibidas. |
| RF15 | Como sistema, eu quero recomendar conteúdos populares para usuários com pouco ou nenhum histórico de interação. | Usuário novo ou com baixa interação. | Acessar sistema com usuário sem histórico. | Lista de recomendações baseada em popularidade exibida. | Recomendações exibidas ao usuário. |

---

### Administração

| RF | Requisito Funcional | Pré-condição | Procedimento de Teste | Resultado Esperado | Pós-condição |
|----|---------------------|--------------|------------------------|---------------------|---------------|
| RF16 | Como administrador, eu quero gerenciar conteúdos da plataforma (espetáculos, categorias e avaliações) para garantir a atualização, consistência e qualidade das informações exibidas aos usuários. | Administrador autenticado. | Criar, editar e remover conteúdos no sistema. | Alterações refletidas corretamente na plataforma. | Sistema atualizado. |
</div>

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

