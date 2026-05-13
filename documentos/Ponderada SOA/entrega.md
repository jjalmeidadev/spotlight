# Documentação da Arquitetura SOA - Spotlight

## Introdução

Este documento apresenta a documentação da arquitetura orientada a serviços (SOA) do projeto **Spotlight**, solução proposta para o **Portal Aventura**. A arquitetura foi estruturada com foco em modularidade, desacoplamento, integração entre serviços, escalabilidade e organização clara das responsabilidades dos componentes.

A proposta arquitetural utiliza uma divisão em camadas, separando apresentação, entrada, serviços de backend, processamento de dados, machine learning, mensageria, banco de dados e autenticação externa. Essa separação permite que os componentes evoluam de forma independente e que partes específicas da solução possam ser escaladas conforme a necessidade do parceiro.

---

# 1. Diagrama de Componentes

![Diagrama de Componentes](./diagrama-componentes.png)

## 1.1 Objetivo do Diagrama

O Diagrama de Componentes tem como objetivo representar a organização lógica da aplicação Spotlight, evidenciando os principais módulos internos, suas responsabilidades e suas integrações.

Ele apresenta a arquitetura em camadas, demonstrando como usuários e gestores interagem com a aplicação, como as requisições chegam ao sistema por meio do API Gateway, como os serviços internos se comunicam com o banco de dados e como o módulo de recomendação utiliza processamento assíncrono e machine learning.

---

## 1.2 Blocos Principais da Arquitetura

| Bloco | Componentes | Responsabilidade |
|---|---|---|
| Camada de Apresentação | Frontend Web / Mobile e Painel Administrativo | Permite a interação de usuários e gestores com a plataforma |
| Camada de Entrada | API Gateway e interface REST `/api/v1` | Centraliza a entrada das requisições e direciona chamadas para os serviços internos |
| Camada de Serviços | Autenticação, Perfil, Catálogo, Recomendação e Coleta de Interações | Implementa as principais regras de negócio da plataforma |
| Camada de Processamento e ML | Motor de Recomendação, RabbitMQ e Pipeline ETL | Processa eventos, consolida dados e gera rankings de recomendação |
| Camada de Dados | Banco de Dados PostgreSQL | Armazena usuários, espetáculos, interações, preferências e dados necessários para recomendação |
| Serviços Externos | Supabase Auth | Serviço externo responsável pelo suporte à autenticação |

---

## 1.3 Componentes e Responsabilidades

| Componente | Responsabilidade | Justificativa Arquitetural |
|---|---|---|
| Frontend Web / Mobile | Disponibilizar a interface de navegação para o usuário final | Separa a experiência visual da lógica de negócio |
| Painel Administrativo | Permitir que gestores cadastrem e atualizem conteúdos | Isola funcionalidades administrativas das funcionalidades do usuário final |
| API Gateway | Centralizar o acesso aos serviços internos | Reduz acoplamento entre frontend e backend, além de controlar o fluxo de requisições |
| Serviço de Autenticação | Validar tokens e delegar autenticação ao Supabase Auth | Mantém a autenticação modular e integrada a um serviço externo especializado |
| Serviço de Perfil do Usuário | Gerenciar preferências, dados e características do usuário | Permite personalização da experiência e suporte ao sistema de recomendação |
| Serviço de Catálogo de Espetáculos | Consultar e organizar informações dos espetáculos | Centraliza regras relacionadas ao catálogo da plataforma |
| Serviço de Recomendação | Solicitar recomendações personalizadas para os usuários | Desacopla a recomendação das demais regras de negócio |
| Serviço de Coleta de Interações | Registrar eventos de navegação, cliques e interações | Gera dados comportamentais utilizados pelo motor de recomendação |
| Motor de Recomendação (ML.NET) | Gerar rankings e recomendações com base nos dados processados | Permite separar processamento de machine learning da aplicação principal |
| RabbitMQ | Gerenciar fila de mensagens e eventos assíncronos | Evita sobrecarga imediata nos serviços principais |
| Pipeline ETL | Consolidar e preparar dados para análise e recomendação | Organiza dados brutos antes do uso pelo motor de recomendação |
| PostgreSQL | Armazenar os dados estruturados da plataforma | Garante persistência e integridade das informações |
| Supabase Auth | Realizar autenticação externa dos usuários | Reduz complexidade interna e melhora segurança da autenticação |

---

## 1.4 Fluxo de Comunicação

O usuário interage com o **Frontend Web / Mobile**, enquanto o gestor acessa o **Painel Administrativo** para cadastro e atualização de conteúdos. Ambos utilizam a interface REST `/api/v1`, que encaminha as requisições ao **API Gateway**.

O **API Gateway** centraliza o acesso aos serviços internos e direciona as chamadas para os serviços de autenticação, perfil do usuário, catálogo de espetáculos, recomendação e coleta de interações.

Os serviços que precisam acessar informações persistidas utilizam a interface `IDatabase`, conectada ao banco de dados **PostgreSQL**. Essa estratégia reduz o acoplamento entre os serviços e a camada de persistência.

O **Serviço de Coleta de Interações** publica eventos na fila **RabbitMQ**, permitindo processamento assíncrono. A fila encaminha esses eventos para o **Motor de Recomendação (ML.NET)** e pode acionar o **Pipeline ETL** para consolidação dos dados.

O **Serviço de Recomendação** se comunica com o **Motor de Recomendação** para solicitar rankings personalizados, enquanto o **Serviço de Autenticação** delega a autenticação ao **Supabase Auth**.

---

## 1.5 Justificativa SOA

A arquitetura apresentada segue os princípios de uma arquitetura orientada a serviços porque divide o sistema em componentes especializados, cada um responsável por uma capacidade específica do negócio.

Essa separação permite que autenticação, perfil, catálogo, recomendação, coleta de interações e processamento de machine learning sejam tratados como serviços independentes. Com isso, alterações em um serviço tendem a gerar menor impacto nos demais componentes da solução.

O uso de interfaces como `REST /api/v1`, `IDatabase` e `IFila` reforça o desacoplamento entre os módulos, pois os componentes se comunicam por contratos bem definidos.

Além disso, o uso de RabbitMQ e Pipeline ETL permite que eventos de interação sejam processados de forma assíncrona, melhorando desempenho e evitando que tarefas pesadas prejudiquem a navegação do usuário.

---

# 2. Diagrama de Deployment

![Diagrama de Deployment](./diagrama-deployment.png)

## 2.1 Objetivo do Diagrama

O Diagrama de Deployment tem como objetivo representar como os componentes da aplicação Spotlight são distribuídos em nós físicos ou lógicos de infraestrutura.

Ele demonstra onde cada artefato é executado, quais protocolos são utilizados na comunicação e como os ambientes de frontend, backend, processamento, mensageria, banco de dados e serviços externos se relacionam.

---

## 2.2 Nós de Infraestrutura

| Nó | Artefatos | Responsabilidade |
|---|---|---|
| Dispositivo do Usuário | Browser / App Mobile | Acesso do usuário final à plataforma |
| Dispositivo do Gestor | Browser | Acesso administrativo ao sistema |
| Servidor Web | Frontend Web / Mobile e Painel Administrativo | Hospedagem das interfaces da aplicação |
| Servidor de Aplicação | API Gateway e serviços de backend | Execução das regras de negócio e orquestração dos serviços |
| Servidor de Processamento / ML | Motor de Recomendação e Pipeline ETL | Execução de tarefas de machine learning e tratamento de dados |
| Servidor de Mensageria | RabbitMQ | Processamento assíncrono por meio de fila de mensagens |
| Servidor de Banco de Dados | Banco de Dados Spotlight em PostgreSQL | Persistência dos dados estruturados |
| Serviço Externo | Supabase Auth | Autenticação externa dos usuários |

---

## 2.3 Protocolos e Comunicação

| Origem | Destino | Protocolo | Finalidade |
|---|---|---|---|
| Browser / App Mobile | Frontend Web / Mobile | HTTPS / TLS 1.3 | Acesso seguro do usuário à aplicação |
| Browser do Gestor | Painel Administrativo | HTTPS / TLS 1.3 | Acesso seguro ao painel administrativo |
| Frontend Web / Mobile | API Gateway | REST / HTTPS | Envio de requisições da interface para o backend |
| Painel Administrativo | API Gateway | REST / HTTPS | Envio de operações administrativas |
| API Gateway | Supabase Auth | HTTPS / OAuth 2.0 | Autenticação e autorização |
| API Gateway | Serviços Internos | HTTP em rede interna | Comunicação com serviços de backend |
| Serviços Internos | PostgreSQL | TCP / SQL | Consulta e persistência de dados |
| Serviço de Coleta de Interações | RabbitMQ | AMQP | Publicação de eventos assíncronos |
| RabbitMQ | Motor de Recomendação | AMQP | Consumo de eventos para recomendação |
| RabbitMQ | Pipeline ETL | AMQP | Acionamento de consolidação de dados |
| Serviço de Recomendação | Motor de Recomendação | HTTP em rede interna | Solicitação de rankings personalizados |
| Motor de Recomendação | PostgreSQL | TCP / SQL | Leitura e gravação de dados processados |
| Pipeline ETL | PostgreSQL | TCP / SQL | Consolidação e persistência de dados tratados |

---

## 2.4 Justificativa do Deployment

A distribuição dos componentes em servidores distintos contribui para o desacoplamento e para a escalabilidade da arquitetura. O **Servidor Web** concentra as interfaces da aplicação, enquanto o **Servidor de Aplicação** executa a lógica de negócio por meio do API Gateway e dos serviços internos.

O **Servidor de Processamento / ML** foi separado para isolar tarefas de maior custo computacional, como geração de rankings e execução do Pipeline ETL. Essa decisão evita que o processamento de recomendações comprometa o desempenho da API principal.

O **Servidor de Mensageria**, com RabbitMQ, permite que eventos sejam processados de maneira assíncrona. Assim, interações dos usuários podem ser registradas rapidamente e processadas posteriormente pelo motor de recomendação.

O banco de dados PostgreSQL foi mantido em um servidor próprio para centralizar a persistência dos dados e permitir controle independente sobre armazenamento, consultas e escalabilidade da camada de dados.

A integração com o Supabase Auth como serviço externo reduz a responsabilidade interna sobre autenticação, ao mesmo tempo que mantém comunicação segura por HTTPS e OAuth 2.0.

---

# 3. Integração com Serviços Externos

A arquitetura evidencia integração com o **Supabase Auth**, utilizado como serviço externo para autenticação. Essa comunicação ocorre por meio de **HTTPS / OAuth 2.0**, garantindo segurança na troca de informações.

O uso de um serviço externo para autenticação segue os princípios de SOA, pois delega uma responsabilidade específica para um serviço especializado. Isso reduz complexidade no backend principal e permite que o sistema evolua sem depender de uma implementação própria de autenticação.

Além disso, a presença do API Gateway como intermediador evita que o frontend se comunique diretamente com serviços internos sensíveis, aumentando controle, segurança e organização da arquitetura.

---

# 4. Escalabilidade e Desacoplamento

A arquitetura do Spotlight foi projetada para ser desacoplada e escalável. A separação entre frontend, API Gateway, serviços internos, motor de recomendação, fila de mensagens, pipeline ETL e banco de dados permite que cada parte da solução seja mantida, substituída ou escalada de forma independente.

No contexto da Aventura, essa decisão é importante porque o volume de usuários, espetáculos e interações pode crescer conforme a plataforma for utilizada. Com o serviço de recomendação separado, é possível aumentar a capacidade computacional do motor de recomendação sem precisar escalar toda a aplicação.

A utilização do RabbitMQ também contribui para escalabilidade, pois permite que eventos de interação sejam processados de forma assíncrona. Dessa forma, o usuário não precisa esperar o processamento completo do modelo de recomendação para continuar navegando na plataforma.

O Pipeline ETL também reforça o desacoplamento, pois separa a coleta dos dados da etapa de consolidação e preparação para o motor de recomendação.

---

# 5. Requisitos Não Funcionais

| RNF | Categoria ISO/IEC 25010 | Descrição | SLA/Métrica | Componente Responsável | Justificativa |
|---|---|---|---|---|---|
| RNF01 | Performance Efficiency | A plataforma deve responder rapidamente às requisições dos usuários | 95% das requisições do frontend devem responder em até 2 segundos | API Gateway e Serviços Internos | Garante boa experiência de navegação e evita lentidão no portal |
| RNF02 | Performance Efficiency | O sistema deve processar recomendações sem bloquear a navegação do usuário | Eventos de interação devem ser publicados na fila em até 500 ms | Serviço de Coleta de Interações e RabbitMQ | O uso de mensageria evita que tarefas pesadas impactem a experiência do usuário |
| RNF03 | Reliability | A plataforma deve permanecer disponível para usuários e gestores | Disponibilidade mínima de 99,5% ao mês | Servidor Web, API Gateway e Backend | Reduz indisponibilidade do portal e do painel administrativo |
| RNF04 | Security | A autenticação deve proteger o acesso às funcionalidades da aplicação | Todas as rotas protegidas devem exigir token válido via Supabase Auth | Serviço de Autenticação e Supabase Auth | Garante controle de acesso e proteção dos dados dos usuários |
| RNF05 | Maintainability | Os serviços devem possuir responsabilidades separadas e bem definidas | Alterações em um serviço não devem exigir mudanças diretas nos demais serviços | API Gateway, Serviços Internos, Motor ML e Pipeline ETL | Facilita manutenção, testes e evolução da arquitetura |
| RNF06 | Scalability | O sistema deve suportar crescimento no volume de usuários e interações | Serviço de recomendação e workers devem permitir escalabilidade horizontal | Motor de Recomendação, RabbitMQ e Pipeline ETL | Permite aumentar capacidade de processamento sem escalar toda a aplicação |
| RNF07 | Compatibility | Os componentes devem se comunicar por protocolos padronizados | Comunicação via REST/HTTPS, HTTP interno, AMQP e TCP/SQL | API Gateway, RabbitMQ, Serviços Internos e PostgreSQL | Facilita integração entre componentes e reduz dependência de implementação |
| RNF08 | Modifiability | A arquitetura deve permitir substituição de componentes com baixo impacto | Serviços externos e internos devem ser acessados por interfaces bem definidas | Supabase Auth, API Gateway, IDatabase e IFila | Permite trocar tecnologias ou evoluir serviços sem reestruturar todo o sistema |

---

# 6. Relação dos RNFs com os Componentes do Diagrama

| Componente | RNFs Relacionados | Motivo |
|---|---|---|
| Frontend Web / Mobile | RNF01, RNF03 | Precisa oferecer acesso rápido e disponível para o usuário |
| Painel Administrativo | RNF01, RNF03, RNF04 | Precisa ser seguro, disponível e performático para gestores |
| API Gateway | RNF01, RNF03, RNF05, RNF07 | Centraliza a entrada da aplicação e organiza a comunicação entre serviços |
| Serviço de Autenticação | RNF04, RNF08 | Garante acesso seguro e integração com Supabase Auth |
| Serviço de Perfil | RNF01, RNF05, RNF07 | Gerencia dados do usuário de forma modular |
| Serviço de Catálogo | RNF01, RNF05, RNF07 | Consulta informações dos espetáculos com responsabilidade isolada |
| Serviço de Recomendação | RNF01, RNF05, RNF06 | Solicita rankings personalizados sem acoplar ML diretamente ao backend |
| Serviço de Coleta de Interações | RNF02, RNF05, RNF06 | Publica eventos para processamento assíncrono |
| RabbitMQ | RNF02, RNF06, RNF07 | Permite mensageria assíncrona e escalabilidade |
| Motor de Recomendação | RNF02, RNF06 | Processa recomendações em infraestrutura separada |
| Pipeline ETL | RNF02, RNF06, RNF08 | Consolida dados sem bloquear a aplicação principal |
| PostgreSQL | RNF03, RNF07 | Centraliza persistência e comunicação via TCP/SQL |
| Supabase Auth | RNF04, RNF08 | Serviço externo especializado em autenticação |

---

# 7. Conclusão

A arquitetura SOA proposta para o Spotlight atende aos principais critérios da entrega, pois identifica os blocos principais da solução, representa integrações com serviços externos, utiliza notação compreensível em PlantUML e justifica tecnicamente os elementos da arquitetura.

Os diagramas demonstram uma solução modular, baseada em serviços independentes e comunicação padronizada. A presença do API Gateway, dos serviços internos, do RabbitMQ, do Motor de Recomendação, do Pipeline ETL, do PostgreSQL e do Supabase Auth evidencia uma arquitetura preparada para evolução e crescimento.

A separação do motor de recomendação e do processamento assíncrono é especialmente importante para a escalabilidade da Aventura, pois permite que o sistema lide com maior volume de interações e recomendações sem comprometer a experiência do usuário.

Dessa forma, a documentação conecta diretamente a arquitetura desacoplada à escalabilidade do parceiro, atendendo também aos critérios de excelência indicados no barema.