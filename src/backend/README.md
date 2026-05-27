# Spotlight — Backend (T15)

Estrutura base do backend do sistema de recomendação **Spotlight**, pronta para
receber as features do T16. Segue a arquitetura **SOA** documentada em
`documentos/Projeto.md` (seção 5.2/5.3): **ASP.NET Core (.NET 9)** + **PostgreSQL
(Supabase)**, com acesso a dados em **SQL puro via Npgsql (sem ORM)**.

O backend é composto por **serviços independentes** (cada um sobe como um processo
próprio) atrás de um **API Gateway**. Cada serviço é autossuficiente: tem seu
próprio logging, health check, tratamento de erros e conexão com o banco — assim,
se for chamado diretamente (sem passar pelo gateway), continua funcionando.

## Projetos

| Projeto | Pasta | Porta | Responsabilidade |
|---|---|---|---|
| **Spotlight.Gateway** | `api-gateway/` | 5000 | Ponto único de entrada; roteia para os serviços (YARP) e validará JWT (T16/T18) |
| **Spotlight.Catalogo** | `services/catalogo/` | 5001 | Espetáculos, teatros, gêneros e precificação |
| **Spotlight.Usuario** | `services/usuario/` | 5002 | Perfis, preferências declaradas e histórico |
| **Spotlight.Recomendacao** | `services/recomendacao/` | 5003 | Orquestra sugestões consultando o Motor de ML |

Cada serviço tem um projeto de teste correspondente em `tests/`.

## O que já vem montado (em cada serviço)

| Item | Onde |
|---|---|
| Inicialização ASP.NET Core (.NET 9) | `Program.cs` |
| Variáveis de ambiente (`.env`) | `.env.example` + `DotNetEnv` |
| Conexão com o banco (Npgsql / ADO.NET, sem ORM) | `Program.cs` (`NpgsqlDataSource`) |
| Logger (Serilog) | `Program.cs` (`UseSerilog`) |
| Middlewares: CORS, error handler, request logging | `Program.cs` + `presentation/middleware/ErrorHandlingMiddleware.cs` |
| Health check `GET /health` (liveness) | `Program.cs` |
| Health check do banco `GET /health/ready` (readiness) | `Program.cs` |
| Swagger / OpenAPI | `Program.cs` (`AddSwaggerGen` / `UseSwaggerUI`) |
| Setup de testes (xUnit) | `tests/Spotlight.<Servico>.Tests/` |

> O gateway não acessa banco (só roteia), então não tem `/health/ready` nem Npgsql.

## Estrutura

```text
src/backend/
├── Spotlight.sln                     # solution: 4 apps + 4 projetos de teste
├── api-gateway/                      # Spotlight.Gateway (porta 5000)
│   ├── Spotlight.Gateway.csproj
│   ├── Program.cs
│   ├── Controllers/PingController.cs
│   ├── Middleware/ErrorHandlingMiddleware.cs
│   ├── appsettings.json / appsettings.Development.json   # rotas/clusters do YARP
│   └── Properties/launchSettings.json
├── services/
│   ├── catalogo/                     # Spotlight.Catalogo (porta 5001)
│   │   ├── Spotlight.Catalogo.csproj
│   │   ├── Program.cs
│   │   ├── application/{dtos,usecases}/        # DTOs e Services (T16)
│   │   ├── domain/{entities,value-objects}/    # Models do domínio (T16)
│   │   ├── infrastructure/persistence/         # Repositórios SQL puro (T16)
│   │   └── presentation/{controllers,middleware}/
│   ├── usuario/                      # Spotlight.Usuario (porta 5002) — mesma estrutura
│   └── recomendacao/                 # Spotlight.Recomendacao (porta 5003) — mesma estrutura
├── ml-engine/                        # Motor de ML (fora do escopo do backend de APIs)
├── contracts/{dtos,events}/          # Contratos de API/eventos (T17)
└── tests/
    ├── Spotlight.Gateway.Tests/
    ├── Spotlight.Catalogo.Tests/
    ├── Spotlight.Usuario.Tests/
    └── Spotlight.Recomendacao.Tests/
```

## Pré-requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- Acesso ao PostgreSQL/Supabase do projeto (T1/T6)

> **No WSL/Windows:** o repositório fica no sistema de arquivos do Windows
> (`/mnt/c/...`). Use o `dotnet` do Windows — no WSL chame `dotnet.exe`.

## Como rodar

A partir desta pasta (`src/backend`):

```bash
# 1. Configurar variáveis de ambiente do serviço que vai rodar
cp services/catalogo/.env.example services/catalogo/.env
# edite o .env e preencha DATABASE_CONNECTION_STRING

# 2. Restaurar dependências (a solution Spotlight.sln já vem versionada)
dotnet restore

# 3. Subir um serviço (cada um numa porta — ver tabela de Projetos)
dotnet run --project services/catalogo     # http://localhost:5001/swagger
dotnet run --project services/usuario       # http://localhost:5002/swagger
dotnet run --project services/recomendacao   # http://localhost:5003/swagger
dotnet run --project api-gateway             # http://localhost:5000  (roteia p/ os serviços)
```

> Para abrir no Visual Studio/Rider, abra o `Spotlight.sln` (já referencia os 8 projetos).

## Verificando os critérios de aceite

```bash
# Sobe sem erros (ex.: catálogo)
dotnet run --project services/catalogo

# GET /health retorna 200 (em outro terminal)
curl -i http://localhost:5001/health

# Conecta no banco com sucesso (200 quando a connection string está correta)
curl -i http://localhost:5001/health/ready

# Testes rodam (todos os serviços)
dotnet test
```

| Critério de aceite | Como validar |
|---|---|
| `dotnet run` sobe o backend sem erros | passo 3 acima |
| `GET /health` retorna 200 | `curl /health` → `200 OK` |
| Conecta no banco com sucesso | `curl /health/ready` → `200` (`503` se a connection string estiver errada) |
| Testes rodam | `dotnet test` → 8 testes passando |

## Acesso a dados — padrão de repositório (sem ORM)

O acesso ao PostgreSQL é feito com **SQL puro via Npgsql**. O `NpgsqlDataSource`
é registrado como singleton no `Program.cs`; cada repositório (em
`infrastructure/persistence/`) o recebe por injeção de dependência:

```csharp
using Npgsql;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

public class EspetaculoRepository(NpgsqlDataSource dataSource)
{
    public async Task<Espetaculo?> GetByIdAsync(int id)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, titulo FROM espetaculos WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);   // SEMPRE parametrizado (anti SQL injection)

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new Espetaculo { Id = reader.GetInt32(0), Titulo = reader.GetString(1) };
    }
}
```

Registre cada repositório no `Program.cs` com
`builder.Services.AddScoped<EspetaculoRepository>();` e injete-o no Service.

## Notas de projeto

- **`/health` x `/health/ready`** — `/health` é *liveness* (o serviço está de pé) e
  não toca no banco; `/health/ready` é *readiness* e só retorna 200 quando o
  Postgres responde. Isso garante que o serviço suba mesmo com o banco indisponível.
- **Entidades, repositórios e rotas de negócio** (catálogo, usuário, recomendação)
  entram no **T16**, preenchendo as pastas `application`/`domain`/`infrastructure`/
  `presentation` de cada serviço.
- **Contratos de API** (entrada/saída) são o **T17** e ficam em `contracts/`.
- **Swagger documentado** propriamente (descrições, exemplos) é o **T18**; aqui
  fica apenas o setup.
