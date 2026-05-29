using Npgsql;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

public class AvaliacaoRepository(NpgsqlDataSource dataSource) : IAvaliacaoRepository
{
    /// <summary>SQLSTATE 23505 = unique_violation. Usado para detectar a constraint UNIQUE (id_usuario, id_espetaculo).</summary>
    private const string SqlStateUniqueViolation = "23505";

    public async Task<(IReadOnlyList<Avaliacao> Itens, int Total)> ListarPorEspetaculoAsync(
        int espetaculoId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);

        int total;
        await using (var cmdTotal = conexao.CreateCommand())
        {
            cmdTotal.CommandText = "SELECT COUNT(*) FROM avaliacao WHERE id_espetaculo = @id;";
            cmdTotal.Parameters.AddWithValue("id", espetaculoId);
            total = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync(cancellationToken));
        }

        var itens = new List<Avaliacao>();
        await using (var cmd = conexao.CreateCommand())
        {
            cmd.CommandText = """
                SELECT a.id_avaliacao, a.nota, a.comentario, a.created_at,
                       u.id_usuario, u.nome
                FROM avaliacao a
                JOIN usuario u ON u.id_usuario = a.id_usuario
                WHERE a.id_espetaculo = @id
                ORDER BY a.created_at DESC
                LIMIT @limit OFFSET @offset;
                """;
            cmd.Parameters.AddWithValue("id", espetaculoId);
            cmd.Parameters.AddWithValue("limit", pageSize);
            cmd.Parameters.AddWithValue("offset", (page - 1) * pageSize);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                itens.Add(new Avaliacao
                {
                    Id = reader.GetInt32(0),
                    Nota = reader.GetDecimal(1),
                    Comentario = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    Usuario = new UsuarioResumo
                    {
                        Id = reader.GetGuid(4),
                        Nome = reader.GetString(5)
                    }
                });
            }
        }

        return (itens, total);
    }

    public async Task<Avaliacao> CriarAsync(
        int espetaculoId, Guid usuarioId, decimal nota, string? comentario,
        CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            WITH nova AS (
                INSERT INTO avaliacao (id_usuario, id_espetaculo, nota, comentario)
                VALUES (@usuario, @espetaculo, @nota, @comentario)
                RETURNING id_avaliacao, nota, comentario, created_at, id_usuario
            )
            SELECT n.id_avaliacao, n.nota, n.comentario, n.created_at, u.id_usuario, u.nome
            FROM nova n JOIN usuario u ON u.id_usuario = n.id_usuario;
            """;
        cmd.Parameters.AddWithValue("usuario", usuarioId);
        cmd.Parameters.AddWithValue("espetaculo", espetaculoId);
        cmd.Parameters.AddWithValue("nota", nota);
        cmd.Parameters.AddWithValue("comentario", (object?)comentario ?? DBNull.Value);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            return new Avaliacao
            {
                Id = reader.GetInt32(0),
                Nota = reader.GetDecimal(1),
                Comentario = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = reader.GetDateTime(3),
                Usuario = new UsuarioResumo { Id = reader.GetGuid(4), Nome = reader.GetString(5) }
            };
        }
        catch (PostgresException ex) when (ex.SqlState == SqlStateUniqueViolation)
        {
            throw new ConflictException("Você já avaliou este espetáculo.");
        }
    }
}
