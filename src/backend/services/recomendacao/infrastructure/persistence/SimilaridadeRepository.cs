using Npgsql;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Infrastructure.Persistence;

/// <summary>
/// Implementação de <see cref="ISimilaridadeRepository"/> com SQL puro via Npgsql.
/// Lê de <c>similaridade_item</c> com JOIN em <c>espetaculo</c> para enriquecer o item destino.
/// </summary>
public class SimilaridadeRepository(NpgsqlDataSource dataSource) : ISimilaridadeRepository
{
    public async Task<bool> EspetaculoExisteAsync(int espetaculoId, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM espetaculo WHERE id_espetaculo = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("id", espetaculoId);

        var resultado = await cmd.ExecuteScalarAsync(cancellationToken);
        return resultado is not null;
    }

    public async Task<IReadOnlyList<Similar>> ListarPorOrigemAsync(
        int espetaculoOrigemId, int limite, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT s.score_similaridade, s.tipo_algoritmo,
                   e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media
            FROM similaridade_item s
            JOIN espetaculo e ON e.id_espetaculo = s.id_espetaculo_similar
            WHERE s.id_espetaculo_origem = @origem
            ORDER BY s.score_similaridade DESC
            LIMIT @limite;
            """;
        cmd.Parameters.AddWithValue("origem", espetaculoOrigemId);
        cmd.Parameters.AddWithValue("limite", limite);

        var itens = new List<Similar>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(new Similar
            {
                Score = reader.GetDecimal(0),
                TipoAlgoritmo = reader.GetString(1),
                Espetaculo = new EspetaculoRecomendado
                {
                    Id = reader.GetInt32(2),
                    Titulo = reader.GetString(3),
                    Genero = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Status = reader.GetString(5),
                    NotaMedia = reader.GetDecimal(6)
                }
            });
        }
        return itens;
    }
}
