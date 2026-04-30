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
  _conteúdo_

  **Nota**: _Insira aqui informações sobre o que se trata o projeto e que valor ele vai entregar, Objetivos do Produto e O que o produto faz e não faz._

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

## 2.8 Canvas do Projeto
_conteúdo_

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
_conteúdo_
Obs.: Insira aqui informações sobre a construção do artefato da Sprint 1. 

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

