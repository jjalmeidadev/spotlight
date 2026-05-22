# Spotlight API — Backend (T15)

Esqueleto do backend do sistema de recomendação **Spotlight**, pronto para
receber as features do T16. Stack conforme a arquitetura documentada em
`documentos/Projeto.md` (seção 5.2): **ASP.NET Core (.NET 9)** + **PostgreSQL
(Supabase)**.

## O que já vem montado

| Item | Onde |
|---|---|
| Inicialização ASP.NET Core (.NET 9) | `src/Spotlight.Api/Program.cs` |
| Variáveis de ambiente (`.env`) | `.env.example` + `DotNetEnv` |
| Conexão com o banco (EF Core + Npgsql) | `src/Spotlight.Api/Data/AppDbContext.cs` |
| Logger (Serilog) | `Program.cs` (`UseSerilog`) |
| Middlewares: CORS, error handler, request logging | `Program.cs` + `Middleware/ErrorHandlingMiddleware.cs` |
| Health check `GET /health` | `Program.cs` |
| Health check do banco `GET /health/ready` | `Program.cs` |
| Setup de testes (xUnit) | `tests/Spotlight.Api.Tests/` |
| Swagger / OpenAPI | `Program.cs` (`AddSwaggerGen` / `UseSwaggerUI`) |

## Estrutura

```text
src/backend/
├── Spotlight.sln                       # solution com Api + Tests vinculados
├── .env.example
├── src/
│   └── Spotlight.Api/
│       ├── Spotlight.Api.csproj
│       ├── Program.cs                  # bootstrap, middlewares, health, swagger
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Controllers/PingController.cs
│       ├── Data/AppDbContext.cs
│       ├── Middleware/ErrorHandlingMiddleware.cs
│       └── Properties/launchSettings.json
└── tests/
    └── Spotlight.Api.Tests/
        ├── Spotlight.Api.Tests.csproj
        └── HealthEndpointsTests.cs
```

## Pré-requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- Acesso ao PostgreSQL/Supabase do projeto (T1/T6)

## Como rodar

A partir desta pasta (`src/backend`):

```bash
# 1. Configurar variáveis de ambiente
cp .env.example .env
# edite o .env e preencha DATABASE_CONNECTION_STRING

# 2. Restaurar dependências (a solution Spotlight.sln já vem versionada)
dotnet restore

# 3. Subir a API
dotnet run --project src/Spotlight.Api
```

A API sobe em `http://localhost:5000`. Swagger em `http://localhost:5000/swagger`.

> A solution `Spotlight.sln` já está no repositório com os dois projetos
> (`Spotlight.Api` e `Spotlight.Api.Tests`) vinculados. Para abrir no Visual
> Studio/Rider, basta abrir esse arquivo.

> Se algum pacote acusar versão inexistente no `restore`, rode
> `dotnet add src/Spotlight.Api package <Pacote>` para fixar a versão estável
> mais recente.

## Verificando os critérios de aceite

```bash
# Sobe sem erros
dotnet run --project src/Spotlight.Api

# GET /health retorna 200 (em outro terminal)
curl -i http://localhost:5000/health

# Conecta no banco com sucesso (200 quando a connection string está correta)
curl -i http://localhost:5000/health/ready

# Testes rodam
dotnet test
```

| Critério de aceite | Como validar |
|---|---|
| `dotnet run` sobe o backend sem erros | passo 4 acima |
| `GET /health` retorna 200 | `curl /health` → `200 OK` |
| Conecta no banco com sucesso | `curl /health/ready` → `200` (`503` se a connection string estiver errada) |
| Testes rodam (mesmo que vazios) | `dotnet test` |

## Notas de projeto

- **`/health` x `/health/ready`** — `/health` é *liveness* (a API está de pé) e
  não toca no banco; `/health/ready` é *readiness* e só retorna 200 quando o
  Postgres responde. Isso garante que a API suba mesmo com o banco indisponível.
- **Entidades e rotas de negócio** (catálogo, usuário, recomendação) e as
  migrations do EF Core entram no **T16**. O `AppDbContext` está vazio de
  propósito.
- **Swagger documentado** propriamente (descrições, exemplos) é o **T18**; aqui
  fica apenas o setup.
