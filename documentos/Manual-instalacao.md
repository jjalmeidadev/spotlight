<p align="center">
  <img src="./img/header.png" alt="header">
</p>

# Spotlight

## Manual de instalacao

Este manual descreve como preparar, configurar, executar e validar o prototipo
Spotlight em ambiente local. O projeto combina um frontend Expo/React Native,
um backend SOA em ASP.NET Core, PostgreSQL/Supabase, MongoDB para telemetria e
artefatos de recomendacao por Machine Learning.

## 1. Visao geral da distribuicao

O repositorio esta organizado como um monorepo:

```text
g04/
|-- App.js
|-- package.json
|-- Dockerfile.frontend
|-- docker-compose.dev.yml
|-- docker-compose.prod.yml
|-- deploy/nginx/default.conf
|-- documentos/
|-- assets/
`-- src/
    |-- frontend/
    |-- backend/
    |-- database/
    |-- dataset/
    `-- models/
```

Componentes principais:

| Componente | Pasta | Porta local | Funcao |
|---|---|---:|---|
| Frontend Expo | raiz + `src/frontend` | 8081 dev / 8080 prod | Interface mobile/web do Spotlight |
| API Gateway | `src/backend/api-gateway` | 5000 | Entrada unica para o frontend e roteamento YARP |
| Catalogo | `src/backend/services/catalog` | 5001 | Espetaculos, sessoes, categorias, teatros e avaliacoes |
| Usuario | `src/backend/services/user` | 5002 | Perfil, preferencias, localizacao e telemetria |
| Recomendacao | `src/backend/services/recommendation` | 5003 | Recomendacoes personalizadas e espetaculos similares |
| Geolocalizacao | `src/backend/services/geolocation` | 5004 | Geocodificacao via OpenStreetMap/Nominatim |
| Banco relacional | `src/database` | externo/Supabase ou local | Catalogo, usuarios, avaliacoes e recomendacoes |
| Telemetria | MongoDB Atlas | externo | Eventos brutos de interacao do usuario |

O gateway publica as rotas:

```text
http://localhost:5000/catalogo/*
http://localhost:5000/usuario/*
http://localhost:5000/recomendacao/*
http://localhost:5000/geolocalizacao/*
```

## 2. Pre-requisitos

Instale os itens abaixo antes de executar o projeto:

| Dependencia | Versao recomendada | Uso |
|---|---:|---|
| Node.js | 22.x | Frontend Expo e build web |
| npm | 10.x ou superior | Instalacao de dependencias JS |
| Expo CLI | via `npx expo` | Execucao mobile/web |
| .NET SDK | 9.0 ou superior | Backend ASP.NET Core |
| Docker Desktop | 24.x ou superior | Execucao containerizada |
| Docker Compose | 2.20 ou superior | Orquestracao local |
| PostgreSQL/psql | 15.x ou superior | Aplicacao opcional de scripts SQL |
| DBeaver | opcional | Administracao visual do banco |
| Expo Go | opcional | Teste em celular fisico |

No Windows, mantenha o Docker Desktop aberto antes de usar Docker Compose. Para
testar o app no celular, o computador e o dispositivo devem estar na mesma rede.

## 3. Configuracao de ambiente

Na raiz do repositorio, copie o arquivo de exemplo:

```powershell
Copy-Item .env.example .env
```

Preencha o `.env` com os valores reais do ambiente. O arquivo `.env` nao deve
ser versionado, pois contem connection strings e chaves de acesso.

Variaveis minimas para execucao integrada:

```env
ASPNETCORE_ENVIRONMENT=Development

DATABASE_CONNECTION_STRING=Host=db.project-ref.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=sua-senha;SSL Mode=Require;Trust Server Certificate=true
SUPABASE_JWT_SECRET=seu_legacy_jwt_secret
SUPABASE_URL=https://project-ref.supabase.co
SUPABASE_ANON_KEY=sua_chave_anon
SUPABASE_SERVICE_ROLE_KEY=sua_service_role_key

EXPO_PUBLIC_SUPABASE_URL=https://project-ref.supabase.co
EXPO_PUBLIC_SUPABASE_ANON_KEY=sua_chave_anon
EXPO_PUBLIC_API_BASE_URL=http://localhost:5000
EXPO_PUBLIC_AUTH_REDIRECT_URL=spotlightmobile://email-confirmado

MONGODB_CONNECTION_STRING=sua_connection_string_mongodb
MONGODB_DATABASE=spotlight_telemetry
MONGODB_TELEMETRY_COLLECTION=interacoes

NOMINATIM_BASE_URL=https://nominatim.openstreetmap.org
NOMINATIM_USER_AGENT=spotlight-inteli-g04/1.0
```

Para testar em celular fisico com Expo Go, substitua `localhost` pelo IPv4 da
maquina que executa o backend:

```powershell
ipconfig
```

Use o valor de `Endereco IPv4` da rede ativa:

```env
EXPO_PUBLIC_API_BASE_URL=http://SEU_IPV4_LOCAL:5000
```

Sempre que o IP mudar, atualize o `.env` e reinicie o Expo.

## 4. Banco de dados

O banco principal do Spotlight e PostgreSQL provisionado via Supabase. Ele
armazena catalogo, usuarios, avaliacoes, sessoes, recomendacoes, similaridades
e auditoria. A autenticacao usa Supabase Auth; senhas nao sao armazenadas no
schema publico.

### 4.1 Aplicacao inicial do schema

Execute os scripts em ordem pelo SQL Editor do Supabase, DBeaver ou `psql`:

```text
src/database/01_create_database.sql
src/database/02_initial_data.sql
```

O script `01_create_database.sql` cria tabelas, tipos, funcoes, triggers e
politicas iniciais. O script `02_initial_data.sql` popula categorias, teatros,
atores, espetaculos e sessoes iniciais.

Validacao recomendada:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

E tambem:

```sql
SELECT
  (SELECT COUNT(*) FROM public.usuario) AS usuarios,
  (SELECT COUNT(*) FROM public.espetaculo) AS espetaculos,
  (SELECT COUNT(*) FROM public.sessao) AS sessoes;
```

### 4.2 Migrations incrementais

Depois do setup inicial, aplique as migrations de `src/database/migrations/` em
ordem numerica. Cada migration possui um arquivo `_rollback.sql` correspondente.

Exemplo:

```powershell
psql "<DATABASE_URL>" -f src/database/migrations/04_trigger_updated_at_espetaculo.sql
psql "<DATABASE_URL>" -f src/database/migrations/05_timestamptz.sql
psql "<DATABASE_URL>" -f src/database/migrations/06_indexes.sql
```

Nao execute `03_rollback.sql` em ambiente com dados uteis. Esse rollback apaga
estruturas e dados do schema publico.

### 4.3 Supabase Storage

Para imagens de espetaculos e artefatos do modelo, aplique quando necessario:

```text
src/database/migrations/10_supabase_storage_buckets.sql
```

O bucket privado `ml-artifacts` deve ser acessado somente pelo backend ou CI com
`SUPABASE_SERVICE_ROLE_KEY`. Essa chave nunca deve ser exposta no frontend.

## 5. Execucao manual do backend

Restaure os pacotes da solution:

```powershell
dotnet restore src/backend/Spotlight.sln
```

Em terminais separados, execute:

```powershell
dotnet run --project src/backend/services/catalog
dotnet run --project src/backend/services/user
dotnet run --project src/backend/services/recommendation
dotnet run --project src/backend/services/geolocation
dotnet run --project src/backend/api-gateway
```

Tambem e possivel usar o script auxiliar:

```powershell
Set-Location src/backend
.\run-all.ps1
```

Validacao basica:

```powershell
Invoke-WebRequest http://localhost:5000/catalogo/health
Invoke-WebRequest http://localhost:5000/usuario/health
Invoke-WebRequest http://localhost:5000/recomendacao/health
Invoke-WebRequest http://localhost:5000/geolocalizacao/health
```

O retorno esperado para servicos saudaveis e `Healthy`.

Swagger local dos servicos:

```text
http://localhost:5001/swagger
http://localhost:5002/swagger
http://localhost:5003/swagger
http://localhost:5004/swagger
```

## 6. Execucao do frontend

Instale as dependencias:

```powershell
npm install
```

Execute o Expo:

```powershell
npx expo start --lan --clear
```

Opcoes de teste:

| Alvo | Comando ou acao |
|---|---|
| Celular fisico | Abrir o QR Code no Expo Go |
| Navegador | Pressionar `w` no terminal do Expo |
| Android | `npm run android` |
| iOS | `npm run ios` |
| Web direto | `npm run web` |

Fluxo esperado para validacao funcional:

1. Abrir o aplicativo.
2. Fazer login ou cadastro.
3. Selecionar preferencias quando solicitado.
4. Acessar o catalogo.
5. Buscar ou filtrar espetaculos.
6. Abrir detalhes de um espetaculo.
7. Ver sessoes, similares e acao de compra quando houver link disponivel.

A tela de login possui um modo de desenvolvimento para demonstracao. Ele permite
navegar pelo prototipo quando alguma dependencia local estiver indisponivel, mas
nao substitui a validacao de login real, banco, persistencia ou integracao.

## 7. Execucao com Docker Compose em desenvolvimento

O arquivo `docker-compose.dev.yml` sobe frontend, gateway e os quatro servicos
backend com volumes e hot reload.

```powershell
docker compose -f docker-compose.dev.yml up --build
```

Portas esperadas:

| Servico | URL |
|---|---|
| Frontend Expo Web | `http://localhost:8081` |
| API Gateway | `http://localhost:5000` |
| Catalogo | `http://localhost:5001` |
| Usuario | `http://localhost:5002` |
| Recomendacao | `http://localhost:5003` |
| Geolocalizacao | `http://localhost:5004` |

Verificacao:

```powershell
docker compose -f docker-compose.dev.yml ps
docker compose -f docker-compose.dev.yml logs -f gateway
```

## 8. Execucao com Docker Compose em producao local

O arquivo `docker-compose.prod.yml` publica apenas o frontend. O Nginx serve o
build web e encaminha `/api/*` para o gateway interno. Gateway e servicos ficam
na rede interna do Compose.

```powershell
docker compose -f docker-compose.prod.yml up --build -d
```

A aplicacao fica disponivel em:

```text
http://localhost:8080
```

A documentacao Swagger agregada (Gateway + 4 servicos) fica em
`http://localhost:8080/api/swagger`. No ambiente publicado, trocar
`localhost:8080` pelo host real.

Comandos uteis:

```powershell
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f frontend
docker compose -f docker-compose.prod.yml logs -f gateway
```

Para um deploy simples em VPS, recomenda-se:

1. Proxy reverso externo nas portas 80/443, como Caddy, Traefik ou Nginx.
2. Frontend acessivel pelo proxy.
3. Gateway e microsservicos sem portas publicas.
4. Supabase e MongoDB Atlas com allowlist do IP do servidor.
5. Segredos configurados no provedor, nunca no repositorio.

## 9. Builds manuais de imagens

Quando necessario, as imagens podem ser construidas individualmente:

```powershell
docker build -f Dockerfile.frontend --target runtime -t spotlight-frontend:local .
docker build -t spotlight-gateway:local src/backend/api-gateway
docker build -t spotlight-catalog:local src/backend/services/catalog
docker build -t spotlight-user:local src/backend/services/user
docker build -t spotlight-recommendation:local src/backend/services/recommendation
docker build -t spotlight-geolocation:local src/backend/services/geolocation
```

## 10. Testes e validacao

Execute a suite .NET:

```powershell
dotnet test src/backend/Spotlight.sln
```

Valide tambem os endpoints principais:

```powershell
Invoke-WebRequest http://localhost:5000/catalogo/espetaculos
Invoke-WebRequest http://localhost:5000/catalogo/categorias
Invoke-WebRequest http://localhost:5000/recomendacao
Invoke-WebRequest http://localhost:5000/geolocalizacao/health
```

Para validar readiness com banco nos servicos diretos:

```powershell
Invoke-WebRequest http://localhost:5001/health/ready
Invoke-WebRequest http://localhost:5002/health/ready
Invoke-WebRequest http://localhost:5003/health/ready
```

`/health` valida se o servico esta de pe. `/health/ready` valida tambem
dependencias como PostgreSQL.

## 11. Problemas comuns

| Sintoma | Causa provavel | Acao recomendada |
|---|---|---|
| `Network request failed` no app | `EXPO_PUBLIC_API_BASE_URL` incorreta, celular em outra rede ou gateway desligado | Conferir IP local, reiniciar Expo e validar `http://IP:5000` |
| HTTP 502 pelo gateway | Servico interno fora do ar | Conferir portas 5001 a 5004 ou logs do Compose |
| `/health/ready` retorna 503 | Banco indisponivel ou connection string invalida | Conferir `DATABASE_CONNECTION_STRING` e acesso ao Supabase |
| Login falha | Chave anon, URL do Supabase ou redirect incorretos | Conferir `EXPO_PUBLIC_SUPABASE_URL`, `EXPO_PUBLIC_SUPABASE_ANON_KEY` e redirect |
| Recomendacoes vazias | Usuario sem token, sem preferencias ou sem dados processados | Validar login, preferencias e tabela `recomendacao` |
| Telemetria nao grava | MongoDB nao configurado | Conferir `MONGODB_CONNECTION_STRING` e allowlist do Atlas |
| Geolocalizacao falha | Nominatim indisponivel ou bloqueio de rede | Validar `NOMINATIM_USER_AGENT`; o app deve continuar sem bloquear o fluxo |
| Imagens nao aparecem | Supabase Storage nao configurado ou paths divergentes | Verificar bucket `espetaculos` e URLs publicas |

## 12. Checklist final

Antes de considerar a instalacao concluida, confirme:

- `.env` criado e preenchido com valores reais.
- Scripts `01_create_database.sql` e `02_initial_data.sql` aplicados.
- Migrations necessarias aplicadas em ordem.
- Backend sobe manualmente ou via Docker Compose.
- Gateway responde em `http://localhost:5000`.
- Frontend abre pelo Expo ou por `http://localhost:8080` em producao local.
- Fluxo de login, catalogo e detalhes funciona.
- Testes .NET executados com sucesso.
- Segredos permanecem fora do Git.


