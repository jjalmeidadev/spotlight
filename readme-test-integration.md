# Ponderada Semana 8 - Teste de integração do fluxo de avaliação

**Aluno:** Leonardo Ramos Vieira  
**Atividade:** Testando um fluxo da aplicação com regras de negócio e RNFs  
**Projeto:** Spotlight  
**Fluxo escolhido:** `POST /catalogo/espetaculos/{id}/avaliacoes`  
**Branch:** `test/integration-avaliacao-espetaculo`

## Coordenação do grupo - Testes de integração

| Aluno | Fluxo escolhido | Dependência possuída (Testcontainers) | Dependência consumida (WireMock.Net) | RF | RNF / ISO 25010 | Link do MR | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Mariana Reis | `PUT /usuario/me/preferencias` | PostgreSQL - tabela `usuario`, campo `perfil_preferencias` | Motor de recomendação/notificação de preferências | RF03 | RNF03 - Confiabilidade | A preencher | Em desenvolvimento |
| Vitor | `PUT /usuario/me/localizacao` | PostgreSQL - tabela `usuario`, campo `localizacao` | OpenStreetMap/Nominatim | RF03 | RNF03 - Confiabilidade | A preencher | A confirmar |
| Rafael | `GET /recomendacao/me` | PostgreSQL - tabelas `recomendacao`, `usuario` e `espetaculo` | OpenStreetMap/Nominatim para contexto geográfico, se usado no fluxo | RF04 | RNF02 - Desempenho ou RNF03 - Confiabilidade | A preencher | Em desenvolvimento |
| Celso | `GET /recomendacao/espetaculos/{id}/similares` | PostgreSQL - tabelas `similaridade_item` e `espetaculo` | Motor externo de similaridade/k-NN simulado | RF14 | RNF02 - Desempenho | A preencher | A confirmar |
| Leonardo | `POST /catalogo/espetaculos/{id}/avaliacoes` | PostgreSQL - tabelas `usuario`, `espetaculo` e `avaliacao` | Serviço de recomendação recebendo evento de avaliação | RF09, RF10, RF11 e RF13 | RNF05 - Segurança; RNF02/RNF09 como impacto indireto | A preencher após abertura do MR | Implementado |
| Mariana Pereira | `POST /catalogo/espetaculos` | PostgreSQL - tabelas `espetaculo`, `categoria` e relacionamentos | API externa simulada de armazenamento/validação de imagem ou ticketeira | RF16 | RNF05 - Segurança | A preencher | A confirmar |
| Jaime | `GET /geolocalizacao/geocoding/reverse` | Cache ou PostgreSQL, caso o fluxo persista/cacheie o resultado | OpenStreetMap/Nominatim | RF03 ou RF12 | RNF03 - Confiabilidade | A preencher | Mais arriscado se não tocar dependência possuída |

## 1. Contexto do fluxo

Este Merge Request implementa testes de integração para o fluxo de criação de avaliação de espetáculo no serviço `Spotlight.Catalogo`.

O endpoint escolhido permite que um usuário autenticado registre uma nota para um espetáculo e, opcionalmente, envie um comentário sobre a experiência. Esse fluxo é importante para o MVP porque as avaliações aparecem como dado transacional do catálogo e também servem como sinal comportamental para melhorar recomendações futuras.

O teste foi estruturado para validar o comportamento real da aplicação em um fluxo que toca dois tipos de dependência exigidos pela atividade:

- uma dependência possuída, representada pelo PostgreSQL;
- uma dependência consumida, representada pelo serviço de recomendação, isolado com WireMock.Net.

Assim, o teste não valida apenas uma regra isolada do use case. Ele exercita a rota HTTP, autenticação JWT, persistência relacional via Npgsql e chamada HTTP para outro serviço.

## 2. O que foi implementado

Foram adicionados testes de integração para o endpoint público:

```http
POST /catalogo/espetaculos/{id}/avaliacoes
```

O serviço de catálogo passou a notificar o serviço de recomendação sempre que uma avaliação é criada com sucesso. Essa notificação ocorre por HTTP no endpoint interno:

```http
POST /eventos/avaliacoes
```

Para manter o ambiente local resiliente, quando `RECOMENDACAO_BASE_URL` não está configurada, o catálogo usa uma implementação `NoOp`. Nos testes de integração, essa variável é apontada para o servidor WireMock, garantindo que a dependência consumida seja controlada e determinística.

Arquivos principais criados ou alterados:

- `src/backend/tests/Spotlight.Catalogo.Tests/AvaliacaoIntegrationTests.cs`
- `src/backend/tests/Spotlight.Catalogo.Tests/Spotlight.Catalogo.Tests.csproj`
- `src/backend/services/catalogo/application/usecases/CriarAvaliacaoUseCase.cs`
- `src/backend/services/catalogo/application/usecases/IAvaliacaoRecomendacaoClient.cs`
- `src/backend/services/catalogo/infrastructure/recomendacao/RecomendacaoAvaliacaoHttpClient.cs`
- `src/backend/services/catalogo/infrastructure/recomendacao/NoOpAvaliacaoRecomendacaoClient.cs`
- `src/backend/services/catalogo/domain/entities/Exceptions.cs`
- `src/backend/services/catalogo/presentation/middleware/ErrorHandlingMiddleware.cs`
- `src/backend/services/catalogo/presentation/controllers/EspetaculosController.cs`
- `src/backend/services/catalogo/Program.cs`
- `src/backend/services/recomendacao/presentation/controllers/AvaliacaoEventosController.cs`
- `.env.example`
- `docker-compose.yml`

## 3. Classificação das dependências

### Dependência possuída: PostgreSQL

A dependência possuída do fluxo é o PostgreSQL, usado pelo serviço de catálogo para consultar e persistir dados nas tabelas do domínio da aplicação.

No teste, o banco é criado com `Testcontainers.PostgreSql`, usando a imagem `postgres:16-alpine`. O container é descartável e iniciado durante o ciclo de vida do teste com `IAsyncLifetime`.

O cenário prepara as estruturas reais do projeto executando:

- `src/database/00_supabase_compat.sql`
- `src/database/01_create_database.sql`

Em seguida, o teste insere dados mínimos em:

- `auth.users`
- `usuario`
- `espetaculo`

Depois da chamada HTTP, a tabela `avaliacao` é consultada diretamente com Npgsql para confirmar que a nota foi realmente persistida.

Essa dependência é classificada como possuída porque o banco relacional faz parte da infraestrutura controlada pelo próprio sistema Spotlight. Ele é a fonte de verdade para avaliações, usuários e espetáculos.

### Dependência consumida: serviço de recomendação

A dependência consumida do fluxo é o serviço de recomendação, chamado pelo catálogo após o registro de uma nova avaliação.

Essa chamada representa a comunicação entre serviços SOA: o catálogo produz um evento de avaliação criada, e a recomendação recebe esse sinal para recalcular ou enriquecer recomendações futuras.

Nos testes, essa dependência não é chamada de verdade. Ela é isolada com `WireMock.Net`, que sobe um servidor HTTP local e simula o endpoint:

```http
POST /eventos/avaliacoes
```

Essa abordagem permite validar dois comportamentos relevantes:

- quando a recomendação aceita o evento, o fluxo de avaliação retorna sucesso;
- quando a recomendação está indisponível, o catálogo devolve erro controlado.

Essa dependência é classificada como consumida porque o catálogo depende de uma API HTTP externa ao seu próprio serviço para concluir a etapa de retroalimentação do motor de recomendação.

## 4. Regra de negócio validada

### RF09 - Avaliar espetáculos

O requisito funcional diretamente validado é o `RF09`: o usuário autenticado deve conseguir avaliar um espetáculo atribuindo uma nota.

No cenário de sucesso, o teste confirma que:

- a requisição autenticada é aceita pela rota protegida;
- a nota enviada é válida;
- o espetáculo existe no banco;
- a avaliação é gravada na tabela `avaliacao`;
- a resposta HTTP é `201 Created`;
- o corpo da resposta retorna nota, comentário, identificador e data de criação.

### RF10 - Comentar sobre espetáculos

O fluxo também cobre o `RF10`, porque o comentário opcional enviado no corpo da requisição é persistido junto com a avaliação.

O teste valida esse ponto ao enviar o comentário `"Excelente producao."` e verificar que ele retorna corretamente no corpo da resposta.

### RF11 e RF13 - Interações que alimentam recomendações

Além da avaliação em si, o fluxo se conecta aos requisitos `RF11` e `RF13`, pois uma avaliação é uma interação relevante do usuário com o catálogo.

Ao notificar o serviço de recomendação após a criação da avaliação, o sistema preserva a rastreabilidade entre comportamento do usuário e melhoria futura das recomendações.

## 5. Requisito não funcional relacionado

### RNF05 - Segurança com autenticação JWT

O requisito não funcional mais diretamente exercitado pelo teste é o `RNF05`, que trata de segurança com autenticação JWT e controle de acesso.

O endpoint `POST /catalogo/espetaculos/{id}/avaliacoes` exige usuário autenticado com perfil `cadastrado`. Por isso, o teste cria um JWT válido com os claims `sub` e `tipo_usuario=cadastrado` e exercita a rota protegida por `[Authorize]`.

Esse teste não substitui uma bateria completa de segurança, porque não mede HTTPS nem cobre todos os cenários de token inválido. Mesmo assim, ele valida que o fluxo de negócio principal roda sob autenticação real do serviço, e não por chamada direta ao use case.

### RNF02 e RNF09 - Impacto indireto em recomendação

O fluxo também impacta `RNF02` e `RNF09`, mas de forma indireta.

O `RNF02` fala sobre geração e exibição rápida de recomendações após interações do usuário. O `RNF09` fala sobre processamento assíncrono de eventos com fila. A implementação deste MR ainda usa uma chamada HTTP entre catálogo e recomendação, não uma fila RabbitMQ. Portanto, o teste não deve ser apresentado como validação completa do RNF09.

O alinhamento correto é: o teste confirma que a avaliação, uma interação relevante para `RF11` e `RF13`, não fica restrita ao banco do catálogo. Ela é encaminhada ao serviço de recomendação, preservando o elo arquitetural necessário para que os RNFs de recomendação possam ser atendidos em uma evolução com processamento assíncrono.

## 6. Cenários testados

### 6.1 Cenário de sucesso

Teste:

```csharp
PostCatalogoEspetaculoAvaliacoes_ComDadosValidos_PersisteNoPostgresENotificaRecomendacao
```

Esse teste valida o caminho feliz do fluxo.

Passos executados:

1. Sobe um PostgreSQL descartável com Testcontainers.
2. Executa os scripts reais de compatibilidade Supabase e criação do banco.
3. Insere dois usuários de teste e um espetáculo disponível.
4. Sobe um servidor WireMock.Net simulando o serviço de recomendação.
5. Configura o WireMock para responder `202 Accepted` em `POST /eventos/avaliacoes`.
6. Cria um JWT válido com claim `sub` e `tipo_usuario=cadastrado`.
7. Chama `POST /catalogo/espetaculos/1/avaliacoes` com nota `4.5` e comentário.
8. Verifica que a resposta HTTP é `201 Created`.
9. Verifica que a avaliação foi persistida na tabela `avaliacao`.
10. Verifica que o WireMock recebeu exatamente uma chamada.
11. Verifica que o payload enviado para recomendação contém `espetaculoId` e `usuarioId`.

Esse cenário protege o fluxo principal de negócio: um usuário avalia um espetáculo e o sistema registra a interação para uso futuro em recomendação.

### 6.2 Cenário alternativo de regra de negócio

Teste:

```csharp
PostCatalogoEspetaculoAvaliacoes_ComNotaForaDaRegra_RetornaBadRequestSemNotificarRecomendacao
```

Esse teste valida o comportamento quando a entrada viola a regra documentada de avaliação.

Passos executados:

1. Sobe o PostgreSQL descartável com os dados mínimos.
2. Sobe o WireMock.Net simulando o serviço de recomendação.
3. Configura o WireMock para responder caso seja chamado.
4. Cria um JWT válido para um usuário cadastrado.
5. Chama `POST /catalogo/espetaculos/1/avaliacoes` com `nota = 0`.
6. Verifica que a resposta HTTP é `400 Bad Request`.
7. Verifica que o erro segue o envelope padrão da API.
8. Verifica que o serviço de recomendação não foi chamado.

Esse cenário está alinhado ao `RF09`, que descreve avaliação com nota válida de 1 a 5. Ele garante que uma nota fora da regra não seja persistida nem propagada como sinal para recomendação.

### 6.3 Cenário alternativo de dependência consumida

Teste:

```csharp
PostCatalogoEspetaculoAvaliacoes_QuandoRecomendacaoFalha_RetornaBadGateway
```

Esse teste valida o comportamento quando a dependência consumida fica indisponível durante o fluxo.

Passos executados:

1. Sobe o PostgreSQL descartável com os dados mínimos.
2. Sobe o WireMock.Net simulando o serviço de recomendação.
3. Configura o WireMock para responder `503 Service Unavailable`.
4. Cria um JWT válido para um usuário cadastrado.
5. Chama `POST /catalogo/espetaculos/1/avaliacoes` com nota e comentário.
6. Verifica que a resposta HTTP é `502 Bad Gateway`.
7. Verifica que o erro segue o envelope padrão da API.
8. Verifica que a chamada ao serviço de recomendação realmente aconteceu.

Esse cenário evita uma falha crítica: a aplicação não deve esconder indisponibilidade da dependência consumida quando essa dependência faz parte do fluxo validado pela integração.

## 7. Como executar os testes

### Pré-requisitos

Antes de rodar os testes de integração, é necessário ter:

- .NET SDK 9.0 ou superior;
- Docker Desktop em execução;
- acesso à internet para restaurar pacotes NuGet, caso eles ainda não estejam no cache local.

### Restaurar dependências

Na raiz do repositório:

```powershell
dotnet restore src/backend/Spotlight.sln
```

### Rodar apenas os testes de integração do catálogo

```powershell
dotnet test src/backend/tests/Spotlight.Catalogo.Tests/Spotlight.Catalogo.Tests.csproj --filter "Category=Integration" --logger "console;verbosity=detailed"
```

Resultado esperado:

```text
Failed: 0
```

### Rodar os testes do catálogo sem integração

Esse comando é útil quando o Docker não está disponível:

```powershell
dotnet test src/backend/tests/Spotlight.Catalogo.Tests/Spotlight.Catalogo.Tests.csproj --filter "Category!=Integration"
```

Resultado esperado:

```text
Failed: 0
```

### Rodar todos os testes do backend

Com Docker aberto:

```powershell
dotnet test src/backend/Spotlight.sln
```

Resultado esperado:

```text
Failed: 0
```

## 8. Observações sobre execução

Os testes de integração usam Testcontainers. Portanto, se o Docker não estiver aberto ou se o endpoint do Docker estiver mal configurado, os testes de integração falham antes de chamar a API.

O erro esperado nessa situação é semelhante a:

```text
Docker is either not running or misconfigured.
```

Isso não indica erro de regra de negócio. Indica apenas que o ambiente local não consegue iniciar o container PostgreSQL descartável.

O WireMock.Net é iniciado pelo próprio teste e não exige serviço externo real. Nenhuma API de recomendação em produção ou homologação é chamada durante a execução.

## 9. Critérios da ponderada atendidos

- Fluxo único escolhido: `POST /catalogo/espetaculos/{id}/avaliacoes`.
- Branch no padrão solicitado: `test/integration-avaliacao-espetaculo`.
- Implementação usando xUnit, framework já presente no backend.
- Uso de Testcontainers para a dependência possuída PostgreSQL.
- Uso de WireMock.Net para a dependência consumida HTTP.
- Cenário de sucesso implementado.
- Cenário alternativo de regra de negócio implementado.
- Cenário alternativo de falha da dependência consumida implementado.
- Dependências classificadas e justificadas.
- RF09 documentado como regra principal.
- RF10 documentado como regra complementar.
- RF11/RF13 documentados como rastreabilidade da retroalimentação de recomendação.
- RNF05 documentado como RNF diretamente exercitado pela autenticação JWT.
- RNF02 e RNF09 documentados como RNFs impactados indiretamente, sem superprometer validação completa de desempenho ou fila.
- Instruções de execução documentadas.

## 10. Prevenção de falha crítica para o parceiro

Para o parceiro Aventura Teatros, avaliações são mais do que comentários exibidos em tela. Elas representam sinais de satisfação do público e ajudam o sistema a melhorar a descoberta de espetáculos.

Sem esse teste, uma regressão poderia fazer o sistema:

- Aceitar uma avaliação sem gravá-la no banco;
- Gravar a avaliação para o usuário errado;
- Deixar de notificar o serviço de recomendação;
- Esconder indisponibilidade da recomendação;
- Degradar a qualidade das recomendações futuras sem erro visível.

O teste de integração reduz esse risco ao validar o fluxo completo, desde a rota HTTP autenticada até o banco relacional e a chamada ao serviço consumido.

## 11. Conclusão

Esta entrega adiciona cobertura de integração para um fluxo relevante do MVP do Spotlight. O teste valida que uma avaliação de espetáculo é registrada corretamente no PostgreSQL e que a nova interação é enviada ao serviço de recomendação.

Com isso, o MR cobre a regra de negócio de avaliação de espetáculos, a persistência transacional no catálogo e o impacto da avaliação na evolução das recomendações. A implementação também deixa explícito o comportamento esperado quando a dependência consumida está indisponível, tornando o fluxo mais confiável e rastreável para a avaliação da ponderada.
