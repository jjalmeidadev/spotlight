# Spotlight — Backend

Estrutura base do backend do sistema de recomendação **Spotlight**, pronta para
receber as features de negócio. Segue a arquitetura **SOA** documentada em
`documentos/Projeto.md` (seção 5.2/5.3): **ASP.NET Core (.NET 9)** + **PostgreSQL
(Supabase)**, com acesso a dados em **SQL puro via Npgsql (sem ORM)**.

O backend é composto por **serviços independentes** (cada um sobe como um processo
próprio) atrás de um **API Gateway**. Cada serviço é autossuficiente: tem seu
próprio logging, health check, tratamento de erros e conexão com o banco — assim,
se for chamado diretamente (sem passar pelo gateway), continua funcionando.

## Projetos

| Projeto | Pasta | Porta | Responsabilidade |
|---|---|---|---|
| **Spotlight.Gateway** | `api-gateway/` | 5000 | Ponto único de entrada; roteia para os serviços (YARP) e validará JWT |
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
│   │   ├── application/{dtos,usecases}/        # DTOs e Services
│   │   ├── domain/{entities,value-objects}/    # Models do domínio
│   │   ├── infrastructure/persistence/         # Repositórios SQL puro
│   │   └── presentation/{controllers,middleware}/
│   ├── usuario/                      # Spotlight.Usuario (porta 5002) — mesma estrutura
│   └── recomendacao/                 # Spotlight.Recomendacao (porta 5003) — mesma estrutura
├── ml-engine/                        # Motor de ML (fora do escopo do backend de APIs)
├── contracts/{dtos,events}/          # Contratos de API/eventos
└── tests/
    ├── Spotlight.Gateway.Tests/
    ├── Spotlight.Catalogo.Tests/
    ├── Spotlight.Usuario.Tests/
    └── Spotlight.Recomendacao.Tests/
```

## Pré-requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- Acesso ao PostgreSQL/Supabase do projeto

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

## Dockerização dos serviços

A base Docker do backend está organizada na raiz do repositório, em `docker-compose.yml`, com os quatro serviços backend em containers. O banco não é containerizado nesta configuração; ele fica no Supabase remoto, conforme o `.env` final.

Os serviços expostos ficam assim:

- `catalogo` em `http://localhost:5001`
- `usuario` em `http://localhost:5002`
- `recomendacao` em `http://localhost:5003`
- `geolocalizacao` em `http://localhost:5004`

Para subir tudo:

```bash
# na raiz do repositório
docker compose up -d --build
```

Se preferir, crie um arquivo `.env` na raiz com pelo menos:

```env
EXPO_PUBLIC_SUPABASE_URL=https://nyaxqgujyyvkjnqzbklf.supabase.co
EXPO_PUBLIC_SUPABASE_ANON_KEY=REDACTED_FOR_PUBLIC_RELEASE
EXPO_PUBLIC_SITE_URL=http://localhost:3000
EXPO_PUBLIC_API_BASE_URL=http://192.168.1.118:5000
EXPO_PUBLIC_AUTH_REDIRECT_URL=exp://192.168.1.118:8081/--/email-confirmado
ASPNETCORE_ENVIRONMENT=Development
NOMINATIM_BASE_URL=https://nominatim.openstreetmap.org
NOMINATIM_USER_AGENT=spotlight-inteli-g04/1.0
DATABASE_CONNECTION_STRING=Host=db.nyaxqgujyyvkjnqzbklf.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=spotlight2026@#S;SSL Mode=Require;Trust Server Certificate=true
SUPABASE_JWT_SECRET=REDACTED_FOR_PUBLIC_RELEASE
```

Se o Docker estiver no Windows, mantenha o Docker Desktop aberto antes de subir os containers.

Para acompanhar a execução:

```bash
docker compose ps
docker compose logs -f catalogo usuario recomendacao geolocalizacao
```

No Docker Desktop ou na extensão Docker do VS Code, confirme que os quatro containers estão com status `running` e que as portas `5001`, `5002`, `5003` e `5004` estão publicadas no host.

Se você for validar o app mobile junto com esses containers, mantenha o API Gateway rodando fora do compose apontando para esses serviços, porque a aplicação ainda usa `EXPO_PUBLIC_API_BASE_URL=http://192.168.1.118:5000`.

## Verificando os critérios de aceite

```bash
# Sobe sem erros (ex.: catálogo)
dotnet run --project services/catalogo

# GET /health retorna 200 (em outro terminal)
curl -i http://localhost:5001/health

# Conecta no banco remoto com sucesso (200 quando a connection string está correta)
curl -i http://localhost:5001/health/ready

# Testes rodam (todos os serviços)
dotnet test
```

Validação rápida no navegador ou no app HTTP do VS Code:

- abrir `http://localhost:5001/swagger`
- abrir `http://localhost:5002/swagger`
- abrir `http://localhost:5003/swagger`
- abrir `http://localhost:5004/swagger`
- testar `http://localhost:5001/health` e `http://localhost:5004/health`

Se quiser validar por terminal:

```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

| Critério de aceite | Como validar |
|---|---|
| `dotnet run` sobe o backend sem erros | passo 3 acima |
| `GET /health` retorna 200 | `curl /health` → `200 OK` |
| Conecta no banco remoto com sucesso | `curl /health/ready` → `200` (`503` se a connection string estiver errada) |
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
  serão implementadas depois, preenchendo as pastas `application`/`domain`/
  `infrastructure`/`presentation` de cada serviço.
- **Contratos de API** (entrada/saída) ficam em `contracts/`.
- **Swagger documentado** propriamente (descrições, exemplos) virá depois; aqui
  fica apenas o setup.
