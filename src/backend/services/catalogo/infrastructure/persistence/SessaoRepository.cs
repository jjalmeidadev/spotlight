using Npgsql;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

public class SessaoRepository(NpgsqlDataSource dataSource) : ISessaoRepository
{
    public async Task<IReadOnlyList<Sessao>> ListarPorEspetaculoAsync(
        int espetaculoId, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT s.id_sessao, s.data, s.horario, s.preco_minimo, s.preco_maximo,
                   s.disponibilidade, s.link_compra,
                   t.id_teatro, t.nome, t.cidade, t.estado
            FROM sessao s
            JOIN teatro t ON t.id_teatro = s.id_teatro
            WHERE s.id_espetaculo = @id
            ORDER BY s.data, s.horario;
            """;
        cmd.Parameters.AddWithValue("id", espetaculoId);

        var itens = new List<Sessao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(new Sessao
            {
                Id = reader.GetInt32(0),
                Data = reader.GetFieldValue<DateOnly>(1),
                Horario = reader.GetFieldValue<TimeOnly>(2),
                PrecoMinimo = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                PrecoMaximo = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                Disponibilidade = reader.GetInt32(5),
                LinkCompra = reader.IsDBNull(6) ? null : reader.GetString(6),
                Teatro = new TeatroResumo
                {
                    Id = reader.GetInt32(7),
                    Nome = reader.GetString(8),
                    Cidade = reader.GetString(9),
                    Estado = reader.GetString(10)
                }
            });
        }
        return itens;
    }
}
