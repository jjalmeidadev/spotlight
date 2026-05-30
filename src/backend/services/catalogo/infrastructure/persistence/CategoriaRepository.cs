using Npgsql;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

public class CategoriaRepository(NpgsqlDataSource dataSource) : ICategoriaRepository
{
    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT id_categoria, nome, descricao FROM categoria ORDER BY nome;";

        var itens = new List<Categoria>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(new Categoria
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }
        return itens;
    }
}
