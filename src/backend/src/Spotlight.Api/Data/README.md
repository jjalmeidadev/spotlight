# Data — acesso ao banco (sem ORM)

Camada de persistência do Spotlight. **Não usamos ORM** (decisão de projeto):
o acesso ao PostgreSQL é feito com **SQL puro via Npgsql (ADO.NET)**.

## Como funciona

- O `NpgsqlDataSource` é registrado como singleton no `Program.cs` (pool de
  conexões, thread-safe).
- Cada repositório recebe o `NpgsqlDataSource` por injeção de dependência, abre
  uma conexão sob demanda e executa comandos parametrizados.

## Padrão de repositório (T16)

```csharp
using Npgsql;

namespace Spotlight.Api.Data;

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

        return new Espetaculo
        {
            Id = reader.GetInt32(0),
            Titulo = reader.GetString(1)
        };
    }
}
```

Registre cada repositório no `Program.cs` com
`builder.Services.AddScoped<EspetaculoRepository>();` e injete-o no Service.
