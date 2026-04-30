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
_conteúdo_ 

## 3.2 Requisitos Não Funcionais (RNFs)

 &emsp; Os requisitos não funcionais do sistema desempenham um papel fundamental na definição das características de qualidade da plataforma de recomendação de espetáculos da Aventura. Esses requisitos garantem que o sistema apresente desempenho adequado, segurança, escalabilidade e uma experiência de uso consistente para diferentes perfis de usuários. A seguir, são apresentados os requisitos não funcionais identificados:

<div align="center"> <sub>Tabela X - Requisitos não funcionais</sub>

# Requisitos Não Funcionais

| RNF   | Requisito Não Funcional                                                                                                                                                                                                                                                                      | Pré-condição                                                                                                                                                              | Procedimento de Teste                                                                                                                                                                                                                                                                                                          | Resultado Esperado                                                                                                                                                                                                                                                                                         | Pós-condição                                                                                                                              | Aspecto de Qualidade |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | -------------------- |
| RNF01 | O sistema deve carregar todas as telas principais (home, detalhes de espetáculo, categorias e calendário) em no máximo **2 segundos** em dispositivos mobile com conexão 4G (mínimo 20 Mbps de download), sendo este o cenário de referência prioritário. Em desktop, o limite se mantém.    | Sistema disponível em produção, testado prioritariamente em dispositivo mobile físico (Android ou iOS) com conexão 4G ativa.                                              | Utilizar ferramenta de medição de performance (ex: Lighthouse no modo mobile) para acessar cada tela principal e registrar FCP (First Contentful Paint) e TTI (Time to Interactive). Repetir o teste 5 vezes por tela em horários distintos, priorizando dispositivo físico mobile.                                           | Todas as telas carregam completamente em até 2 segundos em 100% das execuções no cenário mobile 4G. Relatório aponta gargalos caso o limite seja ultrapassado.                                                                                                                                             | Métricas de performance mobile registradas no sistema de monitoramento. Usuário mobile navega sem lentidão perceptível.                   | Desempenho           |
| RNF02 | O sistema deve gerar e exibir recomendações personalizadas em no máximo **3 segundos** após interação do usuário em dispositivo mobile com conexão 4G, cobrindo os cenários: acesso à home, visualização de espetáculo e atualização de preferências.                                        | Usuário logado com histórico e preferências configurados, acessando via dispositivo mobile com conexão 4G e pipeline de recomendação ativo.                               | Registrar timestamp no momento da interação e no momento em que as recomendações aparecem na tela, em dispositivo mobile físico. Executar com 3 perfis: usuário com histórico extenso, usuário novo e usuário com preferências recém-alteradas. Repetir 10 vezes por perfil.                                                   | Diferença entre timestamps inferior a 3 segundos em todas as execuções no cenário mobile 4G. Em caso de lentidão, o sistema exibe indicador de carregamento (spinner) para não deixar a tela em branco.                                                                                                   | Tempo de geração de recomendações registrado nos logs. Usuário mobile recebe sugestões dentro do tempo esperado.                          | Desempenho           |
| RNF03 | O sistema deve estar disponível para acesso no mínimo **99% do tempo** em qualquer mês calendário, equivalendo a no máximo 7h18min de indisponibilidade mensal, incluindo manutenções programadas, garantindo acesso contínuo para usuários mobile.                                          | Sistema implantado em produção com monitoramento de uptime ativo (ex: UptimeRobot ou Datadog), verificando disponibilidade tanto via mobile quanto via desktop.           | Configurar monitoramento automático com ping a cada 5 minutos durante 30 dias consecutivos. Registrar todos os períodos de indisponibilidade com horário de início, fim e causa. Calcular percentual de disponibilidade total ao final do mês.                                                                                 | Disponibilidade mensal ≥ 99% (máximo 7h18min de downtime acumulado). Incidentes registrados automaticamente com causa identificada. Manutenções programadas notificadas com antecedência mínima de 24h.                                                                                                    | Relatório de disponibilidade mensal gerado e armazenado. Incidentes documentados para análise e melhoria.                                 | Disponibilidade      |
| RNF04 | O sistema deve ser desenvolvido com abordagem **mobile first**, sendo a experiência em dispositivos mobile o cenário principal de design, desenvolvimento e validação. A interface deve ser projetada prioritariamente para telas pequenas, com elementos dimensionados para interação por toque (touch targets mínimos de 44x44px), independentemente da tecnologia de frontend adotada. O suporte a desktop é complementar. | Sistema acessado prioritariamente em dispositivos mobile físicos (Android e iOS). Tecnologia de frontend ainda a ser definida pelo grupo, mas com abordagem mobile first garantida desde o início do desenvolvimento. | (1) Validar todas as telas prioritariamente em dispositivos mobile físicos Android e iOS. (2) Verificar se botões, menus e campos respeitam touch targets mínimos de 44x44px. (3) Confirmar que todas as funcionalidades do sistema operam corretamente em mobile. (4) Validar complementarmente em desktop.                   | Interface funcional e intuitiva em dispositivos mobile físicos como cenário principal. Touch targets com no mínimo 44x44px. Todas as funcionalidades disponíveis e operacionais em mobile, sem dependência de desktop para completar qualquer fluxo.                                                        | Experiência mobile validada como prioritária. Suporte desktop funcional como extensão complementar.                                       | Usabilidade          |
| RNF05 | O sistema deve garantir a segurança dos dados dos usuários por meio de autenticação via token JWT, criptografia de senhas com bcrypt (custo mínimo 10), tráfego exclusivamente via HTTPS com certificado SSL válido, e armazenamento seguro de credenciais no lado do cliente mobile, conforme as boas práticas da plataforma adotada. | Usuário cadastrado, sistema em execução com HTTPS configurado, banco de dados com senhas armazenadas e mecanismo seguro de armazenamento de token implementado no cliente mobile. | (1) Verificar se todas as requisições utilizam HTTPS, rejeitando HTTP. (2) Confirmar que senhas no banco estão em formato hash bcrypt, nunca em texto plano. (3) Testar acesso a rotas protegidas sem token válido em dispositivo mobile. (4) Verificar que tokens não ficam expostos de forma insegura no cliente. (5) Verificar expiração e invalidação de tokens após logout. | (1) Requisições HTTP redirecionadas para HTTPS. (2) Nenhuma senha em texto plano. (3) Acesso sem token retorna HTTP 401. (4) Token armazenado de forma segura no cliente, não acessível por terceiros. (5) Token expirado após logout resulta em redirecionamento para login em qualquer dispositivo.       | Dados sensíveis protegidos em ambiente mobile e desktop. Acessos não autorizados bloqueados. Conformidade de segurança documentada.       | Segurança            |
| RNF06 | O sistema deve suportar no mínimo **200 usuários simultâneos**, com maioria de acessos via dispositivos mobile, sem que o tempo de resposta médio ultrapasse **3 segundos** e sem erros de disponibilidade (HTTP 500 ou timeouts). Os serviços SOA devem escalar de forma independente conforme a demanda. | Ambiente de produção ou homologação configurado, sistema SOA em execução com serviços independentes ativos e ferramenta de teste de carga disponível (ex: k6, JMeter ou Locust). | Simular 200 usuários acessando simultaneamente as rotas principais (home, busca, detalhes), com ao menos 70% dos acessos simulados via perfil mobile. Monitorar tempo de resposta médio, taxa de erros e uso de recursos por serviço SOA durante no mínimo 10 minutos contínuos.                                               | Tempo de resposta médio ≤ 3 segundos com 200 usuários simultâneos. Taxa de erros inferior a 1%. Cada serviço SOA opera dentro dos limites de CPU e memória definidos. Sistema estável sem necessidade de reinicialização.                                                                                   | Sistema estável após o teste. Relatório de carga gerado com métricas por serviço SOA e breakdown mobile vs desktop.                       | Escalabilidade       |
| RNF07 | O sistema deve registrar logs estruturados (formato JSON) em cada serviço SOA, contendo nível de severidade (INFO, WARN, ERROR), timestamp, identificador de sessão, nome do serviço de origem e tipo de dispositivo do cliente (mobile ou desktop), para monitoramento e rastreabilidade distribuída. | Sistema SOA em execução com solução de logging configurada em cada serviço (ex: Winston, Log4j ou similar) e campo de device type e service name implementados.           | (1) Realizar ações rastreáveis via mobile e desktop e verificar geração de logs em cada serviço envolvido. (2) Confirmar que cada log contém todos os campos obrigatórios, incluindo nome do serviço e tipo de dispositivo. (3) Verificar que erros críticos disparam alerta imediato para o time responsável.                 | Todos os eventos geram logs em até 1 segundo com todos os campos obrigatórios. Logs de diferentes serviços SOA correlacionáveis por identificador de sessão. Erros de nível ERROR disparam notificação via sistema de alertas configurado.                                                                  | Logs armazenados e acessíveis por serviço. Rastreabilidade distribuída garantida entre os serviços SOA.                                   | Manutenibilidade     |
| RNF08 | O sistema deve permitir que cada serviço SOA seja atualizado e deployado de forma **independente**, sem causar indisponibilidade nos demais serviços, com no máximo **5 minutos** de instabilidade por serviço durante o processo de atualização.                                            | Sistema SOA em produção com pipeline de CI/CD configurado por serviço, monitoramento de uptime ativo e análise de janelas de menor tráfego mobile disponível.             | Executar deploy de atualização em um serviço isolado e verificar: (1) se os demais serviços permanecem operacionais durante o processo; (2) se funcionalidades críticas (login, home) permanecem acessíveis em mobile durante a atualização; (3) tempo total de instabilidade do serviço atualizado.                           | Serviços não atualizados permanecem 100% operacionais durante o deploy. Instabilidade máxima de 5 minutos no serviço atualizado. Funcionalidades críticas acessíveis em mobile durante todo o processo.                                                                                                    | Serviço atualizado e funcional de forma independente. Downtime por serviço registrado e documentado. Demais serviços SOA sem impacto.     | Manutenibilidade     |
| RNF09 | O sistema deve processar dados de comportamento e recomendações de forma **assíncrona** entre os serviços SOA, utilizando fila de mensagens (ex: RabbitMQ ou similar), sem bloquear a interface mobile do usuário nem aumentar o tempo de resposta das telas em mais de **500ms** em dispositivos com conexão 4G. | Sistema SOA operando com pipeline de dados e fila de mensagens configurada entre os serviços, testado prioritariamente em dispositivo mobile com conexão 4G.              | Simular 100 eventos de comportamento simultâneos via dispositivos mobile e verificar: (1) se o processamento ocorre em background entre os serviços SOA sem bloquear a UI mobile; (2) se o tempo de resposta não aumenta mais de 500ms em 4G; (3) se nenhum evento é perdido na fila entre serviços.                           | Nenhuma tela mobile apresenta aumento de tempo de resposta superior a 500ms durante o processamento assíncrono entre serviços SOA em conexão 4G. 100% dos eventos enfileirados processados corretamente. Fila sem backlog superior a 500 itens em operação normal.                                         | Sistema mantém desempenho estável em mobile durante comunicação assíncrona entre serviços SOA. Métricas de throughput registradas nos logs. | Desempenho           |
| RNF10 | O sistema deve seguir os princípios de usabilidade mobile (Nielsen e diretrizes de acessibilidade WCAG), garantindo que qualquer usuário consiga realizar as ações principais (buscar espetáculo, visualizar detalhes e acessar calendário) em no máximo **3 toques** a partir da tela inicial em dispositivo mobile. | Sistema acessível ao usuário final em dispositivo mobile físico (Android e iOS) em ambiente de homologação ou produção, independentemente da tecnologia de frontend adotada. | Conduzir sessões de teste de usabilidade com ao menos 5 usuários sem experiência prévia, utilizando dispositivos mobile físicos Android e iOS. Solicitar que realizem as tarefas principais e registrar: número de toques, erros cometidos e tempo de conclusão por tarefa.                                                     | Todas as tarefas principais concluídas em no máximo 3 toques a partir da home em dispositivo mobile. Taxa de sucesso ≥ 85% entre os usuários testados. Nenhum usuário relata dificuldade em encontrar funcionalidades básicas no mobile.                                                                   | Relatório de usabilidade mobile gerado com pontos de melhoria identificados. Experiência mobile validada como prioritária e documentada.  | Usabilidade          |
| RNF11 | O sistema deve funcionar corretamente em dispositivos mobile físicos com **Android 10 ou superior** e **iOS 15 ou superior** como cenário prioritário de compatibilidade, e complementarmente nos navegadores desktop Chrome, Edge e Safari em suas versões mais recentes, sem erros de layout ou funcionalidade. | Sistema acessado prioritariamente em dispositivos físicos Android (versão 10+) e iOS (versão 15+), e complementarmente em navegadores desktop nas versões mais recentes.  | (1) Executar o fluxo completo (cadastro, login, busca, visualização, avaliação) prioritariamente em dispositivos físicos Android 10+ e iOS 15+. (2) Testar em ao menos dois modelos distintos de cada plataforma. (3) Complementarmente, validar nos navegadores desktop Chrome, Edge e Safari. (4) Verificar ausência de erros em todos os ambientes testados. | Todas as funcionalidades operam corretamente em Android 10+ e iOS 15+ sem erros. Layout visualmente consistente entre os dispositivos mobile testados. Funcionalidades também validadas nos navegadores desktop complementarmente, sem inconsistências críticas.                                             | Sistema validado prioritariamente para mobile (Android e iOS). Relatório de compatibilidade com evidências (capturas de tela) de dispositivos físicos. | Compatibilidade      |
| RNF12 | Cada serviço da arquitetura SOA deve ser implantado e executado em **containers Docker** independentes, com imagens versionadas no repositório, permitindo deploy, atualização e escalabilidade de cada serviço de forma isolada, sem impacto nos demais serviços da plataforma.             | Ambiente com Docker e Docker Compose instalados. Repositório de imagens configurado (ex: Docker Hub ou registry privado). Cada serviço SOA com seu próprio Dockerfile.   | (1) Realizar build independente da imagem Docker de cada serviço. (2) Derrubar um serviço e verificar que os demais continuam operacionais. (3) Promover as imagens de desenvolvimento para produção sem rebuild. (4) Simular falha de container de um serviço e verificar reinicialização automática sem afetar os outros.    | Cada serviço SOA executa em container isolado. Falha ou atualização de um serviço não afeta os demais. Reinicialização automática de container em menos de 30 segundos após falha simulada. Sistema portável entre ambientes sem alteração de código.                                                       | Infraestrutura SOA documentada via docker-compose.yml versionado no repositório, com um serviço por container.                            | Manutenibilidade     |
</div>

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

