# Documentação SOA e Integração de API Externa

## Introdução

O presente documento tem como objetivo descrever a arquitetura orientada a serviços (SOA) adotada no projeto, bem como detalhar a integração de uma API externa de geolocalização utilizada para complementar as funcionalidades do sistema de recomendação.

A arquitetura SOA foi escolhida para promover maior modularização, escalabilidade e desacoplamento entre os componentes da aplicação, permitindo que diferentes serviços sejam desenvolvidos, mantidos e evoluídos de maneira independente.

Além disso, a integração com uma API externa de geolocalização possibilita enriquecer o mecanismo de recomendação, fornecendo sugestões personalizadas baseadas na localização do usuário e na proximidade geográfica das atividades cadastradas.

---

# Arquitetura SOA

## Conceito Utilizado

A arquitetura SOA (Service-Oriented Architecture) organiza o sistema em serviços independentes responsáveis por funcionalidades específicas da aplicação.

Cada serviço possui responsabilidades bem definidas e se comunica por meio de APIs REST, permitindo maior reutilização, flexibilidade e facilidade de manutenção.

No projeto, os serviços foram divididos em módulos especializados, responsáveis pelo gerenciamento de usuários, recomendações, autenticação, geolocalização e processamento de dados.

---

# Objetivos da Arquitetura

A adoção da arquitetura SOA possui os seguintes objetivos:

* desacoplar responsabilidades do sistema;
* facilitar manutenção e evolução;
* permitir escalabilidade individual de serviços;
* melhorar reutilização de funcionalidades;
* simplificar integração com APIs externas;
* facilitar testes e monitoramento;
* reduzir impacto de falhas isoladas.

---

# Estrutura dos Serviços

A arquitetura proposta é composta pelos seguintes serviços:

| Serviço                   | Responsabilidade                       |
| ------------------------- | -------------------------------------- |
| Serviço de Usuários       | Gerenciamento de contas e perfis       |
| Serviço de Recomendação   | Processamento das recomendações        |
| Serviço de Geolocalização | Comunicação com APIs externas de mapas |
| Serviço de Autenticação   | Controle de login e autenticação       |
| Serviço de Catálogo       | Gerenciamento das aventuras/eventos    |
| Serviço de Favoritos      | Controle de interações do usuário      |

---

# Fluxo Geral da Arquitetura

O usuário acessa a aplicação por meio do frontend web ou mobile.

As requisições são encaminhadas ao backend, que distribui as responsabilidades entre os serviços especializados.

O serviço de recomendação consulta:

* preferências do usuário;
* histórico de interações;
* dados geográficos obtidos pela API externa;
* localização atual do usuário.

Com base nessas informações, o sistema gera recomendações personalizadas.

---

# Integração com API Externa de Geolocalização

## Objetivo da Integração

O sistema utilizará uma API externa de geolocalização para permitir funcionalidades relacionadas à localização geográfica dos usuários e das atividades recomendadas.

A integração permitirá:

* recomendação de eventos próximos;
* cálculo de distância;
* busca regional;
* exibição de mapas;
* personalização baseada em localização;
* filtragem por proximidade.

---

# APIs Pesquisadas

Durante o levantamento técnico, foram analisadas diferentes APIs de geolocalização.

| API                     | Vantagens                             | Desvantagens                  |
| ----------------------- | ------------------------------------- | ----------------------------- |
| Google Maps Platform    | Alta precisão e documentação completa | Cobrança após limite gratuito |
| OpenStreetMap/Nominatim | Gratuita e open source                | Menor quantidade de recursos  |
| Mapbox                  | Interface moderna e flexível          | Limitações gratuitas          |

---

# API Escolhida

## Google Maps Platform

Foi escolhida a API da Google: Google Maps Platform.

Documentação oficial:

[Google Maps Platform Documentation](https://developers.google.com/maps/documentation)

---

# Justificativa da Escolha

A escolha da Google Maps Platform ocorreu devido aos seguintes fatores:

* ampla documentação oficial;
* alta precisão geográfica;
* facilidade de integração REST;
* suporte a múltiplos serviços de localização;
* grande quantidade de exemplos e bibliotecas;
* confiabilidade e disponibilidade elevada;
* facilidade de integração com aplicações web e mobile.

---

# Funcionalidades Dependentes da API

As seguintes funcionalidades dependem diretamente da integração com a API de geolocalização:

* identificação da localização do usuário;
* cálculo de distância entre usuários e eventos;
* busca de atividades próximas;
* recomendação regional;
* exibição de localização em mapas;
* filtragem de aventuras por proximidade.

---

# Endpoints Utilizados

## 1. Geocoding API

Responsável por converter endereços em coordenadas geográficas.

### Endpoint

```text
GET https://maps.googleapis.com/maps/api/geocode/json
```

### Método

```text
GET
```

### Exemplo de Requisição

```text
GET /maps/api/geocode/json?address=Campinas+SP&key=API_KEY
```

### Exemplo de Resposta

```json
{
  "results": [
    {
      "geometry": {
        "location": {
          "lat": -22.9099,
          "lng": -47.0626
        }
      }
    }
  ],
  "status": "OK"
}
```

---

## 2. Places API

Responsável pela busca de locais próximos.

### Endpoint

```text
GET https://maps.googleapis.com/maps/api/place/nearbysearch/json
```

### Método

```text
GET
```

### Exemplo de Requisição

```text
GET /maps/api/place/nearbysearch/json?location=-22.9099,-47.0626&radius=5000&type=tourist_attraction&key=API_KEY
```

### Exemplo de Resposta

```json
{
  "results": [
    {
      "name": "Parque Portugal",
      "vicinity": "Campinas - SP"
    }
  ]
}
```

---

## 3. Distance Matrix API

Responsável pelo cálculo de distância e tempo estimado.

### Endpoint

```text
GET https://maps.googleapis.com/maps/api/distancematrix/json
```

### Método

```text
GET
```

### Exemplo de Requisição

```text
GET /maps/api/distancematrix/json?origins=Campinas&destinations=São+Paulo&key=API_KEY
```

### Exemplo de Resposta

```json
{
  "rows": [
    {
      "elements": [
        {
          "distance": {
            "text": "98 km"
          },
          "duration": {
            "text": "1 hour 20 mins"
          }
        }
      ]
    }
  ]
}
```

---

# Variáveis de Ambiente

Para utilização da API, será necessário configurar as seguintes variáveis no ambiente da aplicação:

```env
GOOGLE_MAPS_API_KEY=xxxxxxxx
GOOGLE_MAPS_BASE_URL=https://maps.googleapis.com/maps/api
```

---

# Estrutura de Implementação no Backend

A integração será organizada em um serviço específico responsável pela comunicação com APIs externas.

Exemplo de estrutura:

```text
src/
 ├── services/
 │    ├── GeolocationService.js
 │
 ├── controllers/
 │    ├── GeolocationController.js
 │
 ├── routes/
 │    ├── geolocationRoutes.js
```

---

# Responsabilidades do Serviço de Geolocalização

O serviço de geolocalização será responsável por:

* autenticação via API Key;
* consumo dos endpoints externos;
* tratamento de erros;
* padronização de respostas;
* cálculo de distância;
* cache de coordenadas;
* integração com o serviço de recomendação.

---

# Impacto na Arquitetura

A integração com a API externa introduz novos requisitos arquiteturais no sistema.

## Impactos Positivos

* maior personalização das recomendações;
* enriquecimento dos dados processados;
* melhor experiência do usuário;
* possibilidade de recomendações regionais.

## Impactos Técnicos

* dependência de conexão com internet;
* necessidade de gerenciamento de credenciais;
* controle de limites de requisição;
* necessidade de tratamento de falhas externas;
* possível aumento de latência.

---

# Restrições e Custos

## Restrições

* dependência de chave de API;
* limitação de requisições por minuto;
* necessidade de internet;
* algumas funcionalidades exigem faturamento ativado.

## Custos

A Google Maps Platform possui modelo de cobrança baseado em uso.

Para ambiente acadêmico e prototipação, a cota gratuita é suficiente para desenvolvimento e testes iniciais.

---

# Segurança

As credenciais da API não serão armazenadas diretamente no código-fonte.

As chaves serão mantidas em variáveis de ambiente e protegidas por arquivos `.env` não versionados no repositório.

Além disso, o backend será responsável por centralizar o acesso à API externa, evitando exposição direta da chave no frontend.

---

# Escalabilidade

A arquitetura SOA facilita futuras expansões do sistema.

Novos serviços poderão ser adicionados sem necessidade de alterar toda a aplicação, como:

* serviços de recomendação avançada;
* integração com APIs de clima;
* APIs de eventos;
* sistemas de analytics;
* sistemas de machine learning.

---

# Conclusão

A utilização da arquitetura SOA proporciona maior organização, desacoplamento e escalabilidade ao sistema.

A integração com a API de geolocalização da Google amplia significativamente as capacidades do mecanismo de recomendação, permitindo personalização baseada em localização e proximidade geográfica.

A solução proposta também prepara o sistema para futuras integrações externas e crescimento modular da aplicação.
