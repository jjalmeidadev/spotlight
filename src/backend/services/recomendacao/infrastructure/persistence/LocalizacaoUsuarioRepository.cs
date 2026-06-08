using System.Text.Json;
using Npgsql;
using Spotlight.Recomendacao.Application.UseCases;
using Spotlight.Recomendacao.Domain.ValueObjects;

namespace Spotlight.Recomendacao.Infrastructure.Persistence;

/// <summary>
/// Lê a localização consentida do usuário da coluna JSONB <c>usuario.localizacao</c>
/// via SQL puro (Npgsql), seguindo o padrão dos demais repositórios do serviço.
///
/// A coluna é JSONB e, dependendo da origem do dado, pode conter um objeto
/// estruturado (gravado por <c>PUT /usuario/me/localizacao</c>:
/// <c>{ formattedAddress, latitude, longitude, ... }</c>) ou um valor sem
/// coordenadas (ex.: seed legado). Quando não há lat/lon parseáveis, retorna
/// <c>null</c> — o caso de uso então omite o contexto geográfico (NO-OP).
/// </summary>
public sealed class LocalizacaoUsuarioRepository(NpgsqlDataSource dataSource) : ILocalizacaoUsuarioRepository
{
    public async Task<Coordenadas?> ObterCoordenadasAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT localizacao FROM usuario WHERE id_usuario = @usuario";
        cmd.Parameters.AddWithValue("usuario", usuarioId);

        var resultado = await cmd.ExecuteScalarAsync(cancellationToken);
        if (resultado is null or DBNull)
        {
            return null;
        }

        return ExtrairCoordenadas(resultado.ToString());
    }

    /// <summary>
    /// Extrai latitude/longitude de um JSON de localização. Tolerante: aceita
    /// chaves em qualquer caixa (latitude/Latitude) e retorna <c>null</c> sem
    /// lançar quando o JSON é inválido, não é objeto ou não tem coordenadas.
    /// </summary>
    private static Coordenadas? ExtrairCoordenadas(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var latitude = LerNumero(doc.RootElement, "latitude");
            var longitude = LerNumero(doc.RootElement, "longitude");

            return latitude is not null && longitude is not null
                ? new Coordenadas(latitude.Value, longitude.Value)
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static double? LerNumero(JsonElement objeto, string propriedade)
    {
        foreach (var prop in objeto.EnumerateObject())
        {
            if (!string.Equals(prop.Name, propriedade, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return prop.Value.ValueKind switch
            {
                JsonValueKind.Number when prop.Value.TryGetDouble(out var n) => n,
                JsonValueKind.String when double.TryParse(
                    prop.Value.GetString(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var s) => s,
                _ => null
            };
        }

        return null;
    }
}
