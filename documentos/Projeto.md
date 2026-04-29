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

' '
| RF   | Requisito Funcional                                                                                                                                                                                                                         | Pré-condição                                                                                               | Procedimento de Teste                                                                                                                                                                              | Resultado Esperado                                                                                                                                                                                                                      | Pós-condição                                                                                          |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| RF01 | Como usuário, eu quero me cadastrar na plataforma informando nome, e-mail e senha para acessar recomendações personalizadas.                                                                                                                | Usuário não possui cadastro e acessa a tela de cadastro.                                                   | Acessar tela de cadastro, preencher nome, e-mail válido e senha (mínimo 8 caracteres) e clicar em "Criar conta". Repetir com dados inválidos (e-mail duplicado, senha fraca).                      | Conta criada com sucesso, e-mail de confirmação enviado e redirecionamento para tela de login. Em caso de dados inválidos, mensagem de erro específica exibida.                                                                         | Usuário cadastrado e ativo no sistema.                                                                |
| RF02 | Como usuário, eu quero fazer login na plataforma com e-mail e senha para acessar minhas preferências e recomendações salvas.                                                                                                                | Usuário possui cadastro válido e confirmado.                                                               | Inserir e-mail e senha corretos e clicar em "Entrar". Repetir com credenciais incorretas.                                                                                                          | Usuário autenticado e redirecionado para tela principal com suas recomendações. Após 3 tentativas inválidas, exibir alerta de segurança.                                                                                                | Sessão iniciada e token de autenticação gerado.                                                       |
| RF03 | Como usuário, eu quero selecionar minhas preferências iniciais (tipo de espetáculo, faixa de preço, localização e público-alvo) para personalizar minha experiência desde o primeiro acesso.                                                | Usuário logado pela primeira vez, sem preferências configuradas.                                           | Selecionar ao menos uma opção em cada categoria de preferência e clicar em "Salvar preferências". Tentar confirmar sem preencher campos obrigatórios.                                              | Preferências armazenadas no sistema e perfil inicial configurado. Sem seleção obrigatória, mensagem de campo requerido exibida.                                                                                                         | Perfil inicial configurado e algoritmo de recomendação inicializado com os dados.                     |
| RF04 | Como usuário, eu quero visualizar uma tela principal com recomendações personalizadas, ordenadas por relevância, para descobrir espetáculos relevantes ao meu perfil.                                                                       | Usuário logado com perfil e preferências configurados.                                                     | Acessar tela principal e verificar se os espetáculos exibidos correspondem ao perfil do usuário (categorias, localização, histórico).                                                              | Lista de no mínimo 5 recomendações exibida, ordenadas por relevância com base no perfil. Cada card deve exibir título, categoria, data e preço.                                                                                         | Conteúdos disponíveis para interação e dados de visualização registrados.                             |
| RF05 | Como usuário, eu quero navegar por categorias de espetáculos (teatro, dança, música, etc.) para explorar opções fora das minhas recomendações.                                                                                              | Usuário na tela principal com categorias disponíveis no sistema.                                           | Acessar menu de categorias, selecionar uma opção e verificar os conteúdos filtrados. Testar categoria sem conteúdo disponível.                                                                     | Conteúdos filtrados pela categoria selecionada exibidos em lista ou grade. Categoria vazia exibe mensagem "Nenhum espetáculo disponível nesta categoria no momento".                                                                    | Filtro de categoria ativo e navegação registrada no histórico de comportamento.                       |
| RF06 | Como usuário, eu quero visualizar um calendário de espetáculos para filtrar e escolher eventos por data específica ou período.                                                                                                               | Usuário logado com espetáculos cadastrados no sistema.                                                     | Acessar seção de calendário, selecionar uma data com eventos e uma data sem eventos.                                                                                                               | Ao selecionar data com eventos, lista de espetáculos do dia exibida. Ao selecionar data sem eventos, mensagem "Sem espetáculos nesta data" exibida.                                                                                     | Calendário disponível para navegação e data selecionada destacada visualmente.                        |
| RF07 | Como usuário, eu quero visualizar os detalhes completos de um espetáculo (descrição, elenco, data, horário, local, preço e classificação indicativa) para tomar minha decisão de consumo.                                                   | Espetáculo cadastrado e selecionado pelo usuário.                                                          | Clicar em um espetáculo e verificar se todas as informações estão presentes. Testar espetáculo com dados incompletos.                                                                              | Página de detalhes exibida com todas as informações listadas. Campos não preenchidos exibem "Informação não disponível".                                                                                                                | Detalhes disponíveis para análise e acesso registrado no histórico do usuário.                        |
| RF08 | Como usuário, eu quero visualizar sugestões de espetáculos relacionados ao que estou visualizando, com base em categoria e avaliações, para ampliar minhas opções.                                                                          | Usuário visualizando a página de detalhes de um espetáculo.                                                | Acessar seção de "Relacionados" na página de detalhes e verificar se os itens exibidos pertencem à mesma categoria ou têm avaliações similares.                                                    | Seção exibe ao menos 3 sugestões relacionadas com título, categoria e nota média. Caso não haja relacionados, seção é ocultada.                                                                                                         | Sugestões disponíveis para interação e cliques registrados no comportamento do usuário.               |
| RF09 | Como usuário, eu quero visualizar avaliações, notas e comentários de outros usuários sobre um espetáculo para embasar minha decisão.                                                                                                        | Espetáculo selecionado com ao menos uma avaliação cadastrada.                                              | Acessar seção de avaliações, verificar exibição de nota média, quantidade de avaliações e comentários individuais com data e autor.                                                                | Nota média calculada corretamente, lista de comentários exibida em ordem cronológica decrescente. Espetáculo sem avaliações exibe "Seja o primeiro a avaliar".                                                                          | Informações disponíveis para consulta e contagem de visualizações de avaliações registrada.           |
| RF10 | Como usuário, eu quero avaliar um espetáculo com nota (1 a 5 estrelas) e comentário opcional para contribuir com o sistema de recomendação e ajudar outros usuários.                                                                        | Usuário logado que visualizou ou consumiu o espetáculo.                                                    | Inserir nota de 1 a 5 estrelas, adicionar comentário e confirmar. Tentar avaliar o mesmo espetáculo duas vezes.                                                                                    | Avaliação registrada e nota média do espetáculo atualizada imediatamente. Segunda tentativa exibe opção de editar avaliação existente.                                                                                                  | Avaliação armazenada e dados utilizados para atualizar o algoritmo de recomendação.                   |
| RF11 | Como sistema, eu quero registrar automaticamente o comportamento do usuário (cliques, tempo de permanência em cada tela e histórico de visualizações) para enriquecer e melhorar as recomendações.                                          | Usuário autenticado e navegando na plataforma.                                                             | Realizar uma sequência de interações (acessar 3 espetáculos, permanecer por períodos variados, navegar por categorias) e verificar os dados registrados no banco.                                  | Todos os eventos de interação registrados com timestamp, tipo de ação e ID do conteúdo acessado, sem impacto perceptível na performance da interface.                                                                                   | Histórico de comportamento atualizado e disponível para o algoritmo de recomendação.                  |
| RF12 | Como sistema, eu quero gerar recomendações personalizadas com base no histórico de navegação, preferências declaradas e fatores contextuais do usuário (localização, data e faixa de preço) para exibir sugestões altamente relevantes.     | Usuário com histórico de navegação registrado, preferências configuradas e dados contextuais disponíveis.  | Simular usuário com histórico definido e preferências configuradas. Verificar se as recomendações correspondem ao perfil, localização e espetáculos disponíveis na data atual dentro da faixa de preço. | Recomendações exibidas são coerentes com o perfil do usuário, localizadas na região configurada, disponíveis na data atual e dentro da faixa de preço. Atualizadas em até 5 minutos após nova interação.                               | Recomendações contextualizadas e personalizadas disponíveis na tela principal.                        |
| RF13 | Como sistema, eu quero gerar recomendações com base em usuários com perfis semelhantes (filtragem colaborativa) para aumentar a relevância das sugestões para novos usuários ou com histórico limitado.                                     | Base de dados com ao menos 10 usuários com histórico de interações registrado.                             | Criar usuário com preferências similares a um grupo já existente e verificar se as recomendações incluem conteúdos bem avaliados por esse grupo.                                                   | Recomendações incluem ao menos 3 conteúdos avaliados positivamente por usuários com perfil similar. Para usuário sem histórico suficiente, recomendações baseadas em popularidade são exibidas como fallback.                          | Recomendações colaborativas ativas e refinadas conforme novos dados de interação são registrados.     |
| RF14 | Como sistema, eu quero recomendar conteúdos populares e tendências da semana para aumentar o engajamento de usuários com histórico limitado ou novos na plataforma.                                                                         | Dados de popularidade (visualizações, avaliações e cliques) disponíveis e atualizados nos últimos 7 dias.  | Acessar plataforma com usuário novo (sem histórico) e verificar se a seção de tendências exibe os conteúdos com maior engajamento recente. Verificar atualização da lista após 24h.                | Seção "Em alta" exibe os 10 conteúdos com maior engajamento nos últimos 7 dias, ordenados por score de popularidade. Lista atualizada automaticamente a cada 24 horas.                                                                  | Recomendações de tendências disponíveis e diferenciadas visualmente das recomendações personalizadas. |
| RF15 | Como administrador, eu quero acessar um dashboard com métricas de uso, conteúdos cadastrados e desempenho das recomendações para gerenciar a plataforma.                                                                                    | Usuário autenticado com perfil de administrador.                                                           | Acessar área administrativa e verificar exibição de métricas (total de usuários, espetáculos cadastrados, taxa de cliques nas recomendações). Testar acesso com perfil de usuário comum.           | Dashboard exibido com métricas atualizadas. Acesso por usuário comum resulta em mensagem de "Acesso negado" e redirecionamento.                                                                                                         | Ambiente administrativo ativo com dados atualizados em tempo real.                                    |
| RF16 | Como administrador, eu quero priorizar manualmente determinados espetáculos para destacá-los na tela principal e em seções especiais da plataforma.                                                                                         | Administrador logado com espetáculos cadastrados no sistema.                                               | Selecionar um espetáculo no dashboard, definir nível de prioridade (destaque, normal) e salvar. Verificar exibição na tela principal do usuário.                                                   | Espetáculo priorizado aparece na seção de destaques da tela principal. Ao remover prioridade, conteúdo retorna à ordenação padrão do algoritmo.                                                                                        | Priorização aplicada e vigente até ser alterada manualmente pelo administrador.                       |
| RF17 | Como sistema, eu quero processar dados de comportamento e recomendações de forma assíncrona para garantir que a performance da interface não seja impactada durante operações pesadas.                                                       | Sistema em operação com usuários ativos gerando eventos de comportamento.                                  | Simular 50 interações simultâneas e monitorar o tempo de resposta da interface durante o processamento. Verificar se os dados são processados sem bloquear a navegação.                            | Tempo de resposta da interface mantido abaixo de 2 segundos durante o processamento assíncrono. Dados processados e disponíveis em até 5 minutos após geração.                                                                         | Sistema mantém desempenho estável e fila de processamento assíncrono gerenciada sem perda de dados.   |
| RF18 | Como usuário, eu quero encerrar minha sessão na plataforma para garantir a segurança da minha conta, especialmente em dispositivos compartilhados.                                                                                          | Usuário autenticado e com sessão ativa.                                                                    | Clicar no botão "Sair" no menu do usuário e verificar redirecionamento. Tentar acessar página restrita após logout.                                                                                | Sessão encerrada, token de autenticação invalidado e usuário redirecionado para tela de login. Tentativa de acesso a página restrita após logout redireciona para login.                                                                | Usuário deslogado, sessão removida e dados de navegação temporários limpos.                           |
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

