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
_conteúdo_

## 1.1 Termos e Abreviações
_conteúdo_

## 1.2 Objetivo do Documento
_conteúdo_

# 2. Entendimento do Projeto e do Negócio
_conteúdo_

## 2.1 Contexto da Indústria do Parceiro
_conteúdo_

## 2.2 Problema
_conteúdo_

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
## 2.4.1 Personas

  Considerando a diversidade de públicos que consomem apresentações culturais e utilizam plataformas digitais para descoberta de eventos, foram desenvolvidas diferentes personas para o projeto. Essas personas representam perfis variados de usuários, permitindo compreender comportamentos, necessidades, dificuldades e expectativas relacionadas ao processo de descoberta e escolha de apresentações teatrais.

  A construção das personas teve como objetivo auxiliar nas decisões de experiência do usuário, personalização das recomendações e definição das funcionalidades da plataforma. Além disso, os perfis desenvolvidos ajudam a identificar diferentes níveis de familiaridade tecnológica e padrões de navegação, garantindo que o sistema seja acessível e relevante para públicos distintos.

  Dessa forma, as personas representam usuários reais em potencial da plataforma, contribuindo para o desenvolvimento de uma experiência mais intuitiva, personalizada e alinhada às necessidades do público-alvo do projeto.

---

### 2.4.1.1 Ana Clara Amaral – Estudante e interessada em teatro

  Ana Clara Amaral foi desenvolvida como uma das principais personas do projeto, representando um perfil jovem, conectado digitalmente e interessado em arte e teatro. Estudante do segundo ano do ensino médio e aluna de uma escola de teatro, Ana Clara já teve contato com apresentações culturais anteriormente, mas ainda enfrenta dificuldades para encontrar espetáculos alinhados aos seus gostos pessoais.

  Por estar constantemente conectada às redes sociais, especialmente por meio do celular, Ana Clara possui alta familiaridade com ambientes digitais e espera experiências rápidas, intuitivas e visualmente atrativas. Seu comportamento é fortemente influenciado por tendências culturais, indicações de amigos e conteúdos compartilhados online. Apesar disso, sente frustração ao precisar procurar informações em diferentes plataformas ou descobrir peças interessantes apenas após saírem de cartaz.

  Outro fator importante em seu perfil é a dependência de amigos ou familiares para frequentar apresentações. Dessa forma, Ana Clara valoriza descrições claras, avaliações e informações objetivas sobre os espetáculos, pois frequentemente precisa convencer outras pessoas a acompanhá-la. Isso faz com que a confiança nas recomendações e a facilidade de compartilhamento sejam aspectos fundamentais em sua experiência.

  Assim, a plataforma busca atender diretamente às necessidades dessa persona ao centralizar informações culturais, oferecer recomendações personalizadas com base em seus interesses e facilitar a descoberta de novas apresentações de maneira dinâmica e acessível. Dessa forma, o sistema contribui para aumentar o engajamento cultural da usuária e tornar o processo de escolha de peças mais simples, rápido e alinhado ao seu perfil.

<div align="center">

### Figura X – Persona 1 – Ana Clara Amaral – Estudante e interessada em teatro

![Persona Ana Clara](./img/persona1-AnaAmaral.png)

Fonte: Material produzido pelos autores (2026).

</div>

### 2.4.1.2 Mariana Lacerda Souza – Mãe em busca de experiências culturais infantis

  Mariana Lacerda Souza foi desenvolvida como uma persona que representa responsáveis que desejam proporcionar experiências culturais para seus filhos, mas que possuem pouco hábito de frequentar teatros e apresentações culturais. Com 34 anos, Mariana trabalha em período integral e busca atividades de lazer que possam ser aproveitadas em família durante os finais de semana.

  Apesar de utilizar aplicativos e plataformas digitais no cotidiano, Mariana prefere experiências simples, organizadas e objetivas. Sua principal dificuldade está em encontrar apresentações infantis adequadas para a idade do filho, com informações claras sobre classificação etária, duração, localização e proposta educativa do espetáculo. Muitas vezes, sente insegurança durante o processo de escolha por não conhecer previamente as peças ou não encontrar avaliações confiáveis de outros pais.

  Seu comportamento de navegação é mais analítico e cauteloso, já que costuma comparar opções antes de tomar uma decisão. Mariana valoriza principalmente recomendações confiáveis, avaliações positivas e descrições claras que ajudem a identificar rapidamente se a experiência será apropriada e interessante para a criança.

  Dessa forma, a plataforma busca atender às necessidades dessa persona ao centralizar informações relevantes sobre peças infantis e oferecer recomendações personalizadas com base em faixa etária, preferências familiares e popularidade entre outros responsáveis. Além disso, a organização intuitiva das informações contribui para tornar o processo de descoberta e decisão mais rápido, seguro e confortável para usuários familiares.

<div align="center">

### Figura X – Persona 2 – Mariana Lacerda Souza – Mãe e usuária familiar

![Persona Mariana](./img/persona2-MarianaLSouza.png)

Fonte: Material produzido pelos autores (2026).

</div>

### 2.4.1.3 Ronaldo Benito Soares – Turista cultural interessado em teatro

  Ronaldo Benito Soares foi desenvolvido como uma persona que representa usuários interessados em experiências culturais durante viagens e períodos de turismo. Aos 36 anos, Ronaldo possui grande interesse por arte, teatro e atividades culturais, enxergando essas experiências como uma forma de conhecer melhor os locais que visita e enriquecer sua vivência turística.

  Apesar de seu forte interesse cultural, Ronaldo possui baixa familiaridade com plataformas digitais mais complexas. Utiliza aplicativos básicos no cotidiano, mas demonstra dificuldade e insegurança diante de interfaces com excesso de informações, muitos passos ou navegação pouco intuitiva. Por esse motivo, tende a abandonar rapidamente plataformas que dificultam a busca por informações relevantes.

  Durante suas viagens, Ronaldo procura apresentações alinhadas aos seus gostos pessoais, como peças dramáticas, clássicas e produções culturalmente reconhecidas. Seu processo de decisão é fortemente influenciado por descrições claras, avaliações de outros usuários e recomendações que demonstrem relevância cultural. Além disso, valoriza praticidade ao organizar seus passeios, buscando opções compatíveis com sua localização e disponibilidade de tempo.

  Dessa forma, a plataforma busca atender às necessidades dessa persona ao oferecer recomendações personalizadas com base em preferências culturais, localização e disponibilidade de sessões. A organização simplificada das informações e a facilidade de navegação contribuem para que Ronaldo consiga descobrir apresentações relevantes de maneira rápida e segura, melhorando sua experiência turística e cultural na cidade.

<div align="center">

### Figura X – Persona 3 – Ronaldo Benito Soares – Turista cultural interessado em teatro

![Persona Ronaldo](./img/persona3-RonaldoSoares.png)

Fonte: Material produzido pelos autores (2026).

</div>

### 2.4.1.4 Pedro Henrique Lima – Universitário iniciante no teatro

  Pedro Henrique Lima foi desenvolvido como uma persona que representa jovens universitários com alto nível de familiaridade digital, mas que ainda não possuem experiência prévia com o universo teatral. Com 21 anos e estudante de Engenharia de Software no Inteli, Pedro está constantemente inserido em ambientes tecnológicos e acostumado a consumir conteúdo digital de forma rápida e objetiva.

  Apesar de já ter recebido indicações de amigos sobre peças teatrais como forma de estimular a criatividade, Pedro encontra dificuldades em dar o primeiro passo nesse universo. Sua principal barreira está na ausência de direcionamento claro e na dificuldade em identificar quais espetáculos são mais adequados para iniciantes. O excesso de opções disponíveis, sem uma curadoria eficiente, gera indecisão e insegurança, aumentando o receio de ter uma primeira experiência negativa.

  Seu comportamento de navegação é dinâmico e superficial, priorizando conteúdos visuais, objetivos e de fácil compreensão. Pedro raramente consome textos longos e tende a tomar decisões com base em validação social, como avaliações, लोकप्रियidade e recomendações de outros usuários.

  Dessa forma, a plataforma busca atender às necessidades dessa persona ao oferecer recomendações personalizadas com base em seus interesses — como tecnologia, criatividade e humor — além de apresentar uma curadoria específica para iniciantes. A disponibilização de informações rápidas, objetivas e com forte validação social contribui para aumentar sua confiança na escolha e incentivar sua entrada no universo teatral.

<div align="center">

### Figura X – Persona 4 – Pedro Henrique Lima – Universitário iniciante no teatro

![Persona Pedro](./img/persona4-PedroHLima.png)

Fonte: Material produzido pelos autores (2026).

</div>

### 2.4.1.5 Cintia Araújo Sena – Administradora da plataforma

  Cintia Araújo Sena foi desenvolvida como uma persona que representa o perfil administrativo responsável pela gestão e análise de desempenho da plataforma Aventura. Atuando como administradora do sistema, Cintia tem como principal objetivo avaliar se as recomendações oferecidas estão impactando positivamente na conversão de vendas e na descoberta de novos espetáculos pelos usuários.

  Com nível técnico médio a alto, Cintia possui familiaridade com ferramentas digitais e análise de dados, utilizando métricas e relatórios com frequência para embasar suas decisões. No entanto, enfrenta dificuldades relacionadas à falta de clareza e organização dessas informações, o que dificulta a visualização do impacto real das recomendações e do comportamento dos usuários dentro da plataforma.

  Seu comportamento é orientado por dados, priorizando dashboards objetivos, diretos e de fácil interpretação. Cintia valoriza especialmente a agilidade na obtenção de insights, pois isso influencia diretamente sua capacidade de tomada de decisão estratégica.

  Dessa forma, a plataforma busca atender às necessidades dessa persona ao oferecer dashboards claros, com métricas bem definidas e visualizações intuitivas. A disponibilização de indicadores de conversão, engajamento e ეფექტividade das recomendações permite que Cintia acompanhe o desempenho do sistema de forma eficiente, identifique oportunidades de melhoria e valide o impacto das funcionalidades implementadas.

<div align="center">

### Figura X – Persona 5 – Cintia Araújo Sena – Administradora da plataforma

![Persona Cintia](./img/persona5-CintiaASena.png)

Fonte: Material produzido pelos autores (2026).

</div>

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

