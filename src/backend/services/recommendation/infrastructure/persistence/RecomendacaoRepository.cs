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
                   e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media,
                   m.storage_bucket, m.storage_path, m.alt_text, m.ordem_exibicao
            FROM recomendacao r
            JOIN espetaculo e ON e.id_espetaculo = r.id_espetaculo
            LEFT JOIN LATERAL (
                SELECT storage_bucket, storage_path, alt_text, ordem_exibicao
                FROM espetaculo_midia
                WHERE id_espetaculo = e.id_espetaculo
                  AND tipo_midia = 'catalog_card'
                ORDER BY ordem_exibicao
                LIMIT 1
            ) m ON true
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
                    NotaMedia = reader.GetDecimal(8),
                    Midias = reader.IsDBNull(9)
                        ? []
                        : [
                            new EspetaculoMidia
                            {
                                TipoMidia = "catalog_card",
                                StorageBucket = reader.GetString(9),
                                StoragePath = reader.GetString(10),
                                AltText = reader.IsDBNull(11) ? null : reader.GetString(11),
                                OrdemExibicao = reader.GetInt32(12)
                            }
                        ]
                }
            });
        }
        return itens;
    }

    public async Task<IReadOnlyList<Domain.Entities.Recomendacao>> ListarUserBasedPorAvaliacoesAsync(
        Guid usuarioId, int limite, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            WITH avaliacoes_usuario AS (
                SELECT id_espetaculo, nota
                FROM avaliacao
                WHERE id_usuario = @usuario
            ),
            vizinhos AS (
                SELECT
                    a.id_usuario,
                    COUNT(*) AS itens_em_comum,
                    SUM(1 - (ABS(a.nota - au.nota) / 5.0)) AS similaridade
                FROM avaliacao a
                JOIN avaliacoes_usuario au ON au.id_espetaculo = a.id_espetaculo
                WHERE a.id_usuario <> @usuario
                GROUP BY a.id_usuario
                HAVING COUNT(*) > 0
            ),
            candidatos AS (
                SELECT
                    a.id_espetaculo,
                    SUM(v.similaridade * (a.nota / 5.0)) AS score_bruto,
                    SUM(v.similaridade) AS peso_total,
                    COUNT(DISTINCT v.id_usuario) AS usuarios_similares
                FROM vizinhos v
                JOIN avaliacao a ON a.id_usuario = v.id_usuario
                WHERE a.nota >= 4
                  AND NOT EXISTS (
                      SELECT 1
                      FROM avaliacoes_usuario au
                      WHERE au.id_espetaculo = a.id_espetaculo
                  )
                GROUP BY a.id_espetaculo
            )
            SELECT
                LEAST(1.0, COALESCE(c.score_bruto / NULLIF(c.peso_total, 0), 0))::numeric(8, 6) AS score,
                'k-NN' AS tipo_algoritmo,
                CASE
                    WHEN c.usuarios_similares = 1
                        THEN 'Usuario com gosto parecido avaliou positivamente este espetaculo.'
                    ELSE c.usuarios_similares || ' usuarios com gosto parecido avaliaram positivamente este espetaculo.'
                END AS motivo,
                NOW() AS data_geracao,
                e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media,
                m.storage_bucket, m.storage_path, m.alt_text, m.ordem_exibicao
            FROM candidatos c
            JOIN espetaculo e ON e.id_espetaculo = c.id_espetaculo
            LEFT JOIN LATERAL (
                SELECT storage_bucket, storage_path, alt_text, ordem_exibicao
                FROM espetaculo_midia
                WHERE id_espetaculo = e.id_espetaculo
                  AND tipo_midia = 'catalog_card'
                ORDER BY ordem_exibicao
                LIMIT 1
            ) m ON true
            ORDER BY score DESC, e.nota_media DESC, e.popularidade DESC
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
                Motivo = reader.GetString(2),
                DataGeracao = reader.GetDateTime(3),
                Espetaculo = new EspetaculoRecomendado
                {
                    Id = reader.GetInt32(4),
                    Titulo = reader.GetString(5),
                    Genero = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Status = reader.GetString(7),
                    NotaMedia = reader.GetDecimal(8),
                    Midias = reader.IsDBNull(9)
                        ? []
                        : [
                            new EspetaculoMidia
                            {
                                TipoMidia = "catalog_card",
                                StorageBucket = reader.GetString(9),
                                StoragePath = reader.GetString(10),
                                AltText = reader.IsDBNull(11) ? null : reader.GetString(11),
                                OrdemExibicao = reader.GetInt32(12)
                            }
                        ]
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
            SELECT e.id_espetaculo, e.titulo, e.genero, e.status::text AS status, e.nota_media,
                   m.storage_bucket, m.storage_path, m.alt_text, m.ordem_exibicao
            FROM espetaculo e
            LEFT JOIN LATERAL (
                SELECT storage_bucket, storage_path, alt_text, ordem_exibicao
                FROM espetaculo_midia
                WHERE id_espetaculo = e.id_espetaculo
                  AND tipo_midia = 'catalog_card'
                ORDER BY ordem_exibicao
                LIMIT 1
            ) m ON true
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
                NotaMedia = reader.GetDecimal(4),
                Midias = reader.IsDBNull(5)
                    ? []
                    : [
                        new EspetaculoMidia
                        {
                            TipoMidia = "catalog_card",
                            StorageBucket = reader.GetString(5),
                            StoragePath = reader.GetString(6),
                            AltText = reader.IsDBNull(7) ? null : reader.GetString(7),
                            OrdemExibicao = reader.GetInt32(8)
                        }
                    ]
            };
        }

        return itens;
    }
}
