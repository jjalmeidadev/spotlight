using Npgsql;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Infrastructure.Persistence;

public class AvaliacaoUsuarioRepository(NpgsqlDataSource dataSource) : IAvaliacaoUsuarioRepository
{
    public async Task<(IReadOnlyList<AvaliacaoUsuario> Itens, int Total)> ListarPorUsuarioAsync(
        Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);

        int total;
        await using (var cmdTotal = conexao.CreateCommand())
        {
            cmdTotal.CommandText = "SELECT COUNT(*) FROM avaliacao WHERE id_usuario = @id;";
            cmdTotal.Parameters.AddWithValue("id", usuarioId);
            total = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync(cancellationToken));
        }

        var itens = new List<AvaliacaoUsuario>();
        await using (var cmd = conexao.CreateCommand())
        {
            cmd.CommandText = """
                SELECT a.id_avaliacao, a.nota, a.comentario, a.created_at,
                       e.id_espetaculo, e.titulo
                FROM avaliacao a
                JOIN espetaculo e ON e.id_espetaculo = a.id_espetaculo
                WHERE a.id_usuario = @id
                ORDER BY a.created_at DESC
                LIMIT @limit OFFSET @offset;
                """;
            cmd.Parameters.AddWithValue("id", usuarioId);
            cmd.Parameters.AddWithValue("limit", pageSize);
            cmd.Parameters.AddWithValue("offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                itens.Add(new AvaliacaoUsuario
                {
                    Id = reader.GetInt32(0),
                    Nota = reader.GetDecimal(1),
                    Comentario = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    Espetaculo = new EspetaculoResumoUsuario
                    {
                        Id = reader.GetInt32(4),
                        Titulo = reader.GetString(5)
                    }
                });
            }
        }

        return (itens, total);
    }
}
