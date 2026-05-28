using Npgsql;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

public class TeatroRepository(NpgsqlDataSource dataSource) : ITeatroRepository
{
    private const string ColunasSelect =
        "id_teatro, nome, endereco, cidade, estado, capacidade, acessibilidade, parceiro_naming_rights";

    public async Task<IReadOnlyList<Teatro>> ListarAsync(CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = $"SELECT {ColunasSelect} FROM teatro ORDER BY nome;";

        var itens = new List<Teatro>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(Mapear(reader));
        }
        return itens;
    }

    public async Task<Teatro?> ObterAsync(int id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = $"SELECT {ColunasSelect} FROM teatro WHERE id_teatro = @id;";
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Mapear(reader) : null;
    }

    private static Teatro Mapear(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Nome = r.GetString(1),
        Endereco = r.GetString(2),
        Cidade = r.GetString(3),
        Estado = r.GetString(4),
        Capacidade = r.IsDBNull(5) ? null : r.GetInt32(5),
        Acessibilidade = r.GetBoolean(6),
        ParceiroNamingRights = r.IsDBNull(7) ? null : r.GetString(7)
    };
}
