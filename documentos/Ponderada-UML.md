# Documentação Arquitetural: Spotlight - Aventura Teatros

Este documento apresenta a proposta arquitetural técnica para a nova solução de recomendação inteligente do **Portal Aventura**. O objetivo principal do projeto é criar um sistema capaz de sugerir espetáculos e conteúdos relacionados aos interesses do usuário, visando aumentar o tempo de permanência na plataforma e incentivar a compra de ingressos.

Para suportar esse nível de personalização e integração com o ecossistema da produtora, a solução foi desenhada utilizando o paradigma de **Arquitetura Orientada a Serviços (SOA)**. Essa abordagem garante o desacoplamento necessário entre a interface do usuário, a inteligência do motor de recomendação e os sistemas de bilheteria parceiros.

---

## Estrutura da Documentação

Em conformidade com os requisitos da atividade, esta documentação está estruturada nas seguintes frentes:

### 1. Arquitetura SOA (Componentes)

Apresentação do diagrama UML em código (**PlantUML**), detalhando os principais blocos arquiteturais:

- Frontend
- API Gateway
- Serviços de Negócio
- Banco de Dados
- Integração com APIs externas

---

### 2. Requisitos Não Funcionais (RNFs)

Mapeamento e justificativa de 8 requisitos não funcionais, categorizados estritamente pelos eixos de qualidade da norma **ISO/IEC 25010**, estabelecendo:

- SLAs mensuráveis;
- critérios técnicos de validação;
- e o componente arquitetural responsável por cada entrega.

---

# Requisitos Não Funcionais (RNFs) — ISO/IEC 25010

Abaixo estão os 8 requisitos exigidos, com SLAs mensuráveis, justificativas alinhadas às dores da parceira e o mapeamento direto aos diagramas.

---

## 1. Adequação Funcional (Functional Suitability)

### Requisito
O motor de recomendação deve cobrir ≥ 95% das requisições válidas de usuários logados (com histórico mínimo de 3 interações de telemetria), retornando ao menos 3 espetáculos ou conteúdos relacionados por chamada.

### Justificativa
Garantir a eficácia da ferramenta em sugerir espetáculos, conteúdos e experiências relacionadas ao interesse do usuário, evitando que encontrem “seções vazias”, o que frustraria o objetivo de maior descoberta de espetáculos e conteúdo da Aventura.

### Mapeamento
**Motor de Recomendação (C# + ML.NET)** (Diagrama de Componentes) atuando em conjunto com a **Base de Telemetria (MongoDB Atlas)**, que garante o volume de dados de interações necessário para o match do algoritmo.

---

## 2. Eficiência de Desempenho (Performance Efficiency)

### Requisito
O tempo de resposta (latência p99) da rota `/recomendacoes` deve ser ≤ 300 ms sob uma carga concorrente de 200 requisições por segundo (req/s).

### Justificativa
O benefício esperado de melhor experiência de navegação para o público só ocorre se o carregamento dinâmico da tela no aplicativo for imperceptível para o usuário, aumentando o tempo de permanência no Portal Aventura.

### Mapeamento
**API Gateway (.NET 9)** realizando roteamento eficiente e o **Cluster de Orquestração (K8s / ECS)** (Diagrama de Deployment), responsável pelo auto-scaling dos pods de SOA e ML sob picos de carga.

---

## 3. Compatibilidade (Compatibility)

### Requisito
Todos os endpoints expostos pelo Gateway para o App Mobile devem interoperar consumindo e produzindo dados no formato `application/json` (OpenAPI 3.0), enquanto a comunicação interna (microsserviços) deve suportar gRPC para otimização.

### Justificativa
A Aventura almeja a criação de uma jornada mais conectada entre os espetáculos. Contratos padronizados garantem que o novo backend em .NET seja consumível por diferentes plataformas futuras sem refatoração profunda.

### Mapeamento
**Componente API Gateway** (ponto de contato padronizado no Diagrama de Componentes) e as conexões de roteamento interno REST/gRPC (Diagrama de Deployment).

---

## 4. Usabilidade (Usability)

### Requisito
A interface do aplicativo deve alcançar índice de acessibilidade em conformidade com as diretrizes WCAG 2.1 nível AA para mobile, garantindo compatibilidade com leitores de tela nativos (VoiceOver/TalkBack) e contraste adequado.

### Justificativa
O teatro é uma experiência inclusiva, e o ambiente digital deve refletir esse compromisso, permitindo que qualquer pessoa interessada navegue e compre ingressos sem barreiras.

### Mapeamento
**Componente App Aventura (React Native + TS)** na Camada de Apresentação (Diagrama de Componentes).

---

## 5. Confiabilidade (Reliability)

### Requisito
A arquitetura backend deve prover uma disponibilidade (uptime) ≥ 99,9%.

### Justificativa
A indisponibilidade da plataforma significa perda direta do estímulo à venda de ingressos por meio de recomendações, o que é especialmente crítico durante aberturas de lotes.

### Mapeamento
**Application Load Balancer** operando em conjunto com o **Cluster de Orquestração** e os serviços gerenciados de alta disponibilidade do **Supabase Cluster** e **MongoDB Atlas** (Diagrama de Deployment).

---

## 6. Segurança (Security)

### Requisito
100% das requisições externas devem ser trafegadas via HTTPS (TLS 1.3), e as rotas protegidas devem exigir autenticação via token JWT válido expedido pelo Supabase.

### Justificativa
Proteger o histórico de consumo, telemetria e os dados de perfil é vital para garantir a privacidade dos cerca de 5 milhões de espectadores que interagem com o ecossistema.

### Mapeamento
**Componente Supabase Auth** (Identity no Diagrama de Componentes) atuando em conjunto com o nó do **Application Load Balancer** (SSL Termination) (Diagrama de Deployment).

---

## 7. Manutenibilidade (Maintainability)

### Requisito
O código fonte dos serviços em C# (Core Services e ML Engine) deve manter uma cobertura de testes automatizados (unitários e integração) ≥ 80%, rastreado via pipeline de CI/CD.

### Justificativa
O mercado em que a empresa atua é extremamente dinâmico (são mais de 50 produções entre espetáculos inéditos e versões da Broadway). Uma alta manutenibilidade previne regressões técnicas quando novas integrações forem introduzidas.

### Mapeamento
Reflete na separação lógica rigorosa entre os pods de **Serviços SOA** e **Inteligência** (Diagrama de Deployment), permitindo testes isolados para as APIs e para os modelos de ML.NET.

---

## 8. Portabilidade (Portability)

### Requisito
100% dos serviços do backend desenvolvidos (.NET 9) devem ser empacotados em imagens de container OCI (Docker) baseadas em Linux, aderindo aos princípios do 12-factor app (ex.: strings de conexão injetadas via variáveis de ambiente).

### Justificativa
Garante que a Aventura não sofra de vendor lock-in, podendo migrar ou espelhar a infraestrutura da solução entre provedores de nuvem conforme suas parcerias estratégicas (naming rights) com empresas financeiras e de tecnologia evoluam.

### Mapeamento
**Nó de Cluster de Orquestração (K8s / ECS)** hospedando os containers da aplicação (Diagrama de Deployment).