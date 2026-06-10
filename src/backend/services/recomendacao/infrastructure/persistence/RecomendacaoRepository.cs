using Npgsql;
using NpgsqlTypes;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.Entities;

namespace Spotlight.Recomendacao.Infrastructure.Persistence;

/// <summary>
/// Implementação de <see cref="IRecomendacaoRepository"/> com SQL puro via Npgsql.
/// Lê de <c>recomendacao</c> com JOIN em <c>espetaculo</c> para enriquecer cada item.
/// </summary>
public class RecomendacaoRepository(NpgsqlDataSource dataSource) : IRecomendacaoRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Recomendacao>> ListarPorUsuarioAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT r.score_recomendacao, r.tipo_algoritmo, r.motivo_recomendacao, r.data_geracao,
                   e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media
            FROM recomendacao r
            JOIN espetaculo e ON e.id_espetaculo = r.id_espetaculo
            WHERE r.id_usuario = @usuario
            ORDER BY r.score_recomendacao DESC
            LIMIT @limite;
            """;
        cmd.Parameters.AddWithValue("usuario", usuarioId);
        cmd.Parameters.AddWithValue("limite", limite);

        var itens = new List<Domain.Entities.Recomendacao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            itens.Add(new Domain.Entities.Recomendacao
            {
                Score = reader.GetDecimal(0),
                TipoAlgoritmo = reader.GetString(1),
                Motivo = reader.IsDBNull(2) ? null : reader.GetString(2),
                DataGeracao = reader.GetDateTime(3),
                Espetaculo = new EspetaculoRecomendado
                {
                    Id = reader.GetInt32(4),
                    Titulo = reader.GetString(5),
                    Genero = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Status = reader.GetString(7),
                    NotaMedia = reader.GetDecimal(8)
                }
            });
        }
        return itens;
    }

    public async Task<IReadOnlyDictionary<int, EspetaculoRecomendado>> ObterEspetaculosPorIdsAsync(
        IEnumerable<int> espetaculoIds,
        CancellationToken cancellationToken)
    {
        var ids = espetaculoIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<int, EspetaculoRecomendado>();
        }

        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media
            FROM espetaculo e
            WHERE e.id_espetaculo = ANY(@ids);
            """;
        cmd.Parameters.AddWithValue("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer, ids);

        var itens = new Dictionary<int, EspetaculoRecomendado>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetInt32(0);
            itens[id] = new EspetaculoRecomendado
            {
                Id = id,
                Titulo = reader.GetString(1),
                Genero = reader.IsDBNull(2) ? null : reader.GetString(2),
                Status = reader.GetString(3),
                NotaMedia = reader.GetDecimal(4)
            };
        }

        return itens;
    }
}
