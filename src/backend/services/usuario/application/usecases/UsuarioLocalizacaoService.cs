using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;
using Spotlight.Usuario.Infrastructure.Geolocalizacao;

namespace Spotlight.Usuario.Application.UseCases;

public interface IUsuarioLocalizacaoService
{
    Task AtualizarLocalizacaoAsync(
        Guid idUsuario,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken);
}

public sealed class UsuarioLocalizacaoService(
    IEnumerable<NpgsqlDataSource> dataSources,
    IGeocodingClient geocodingClient)
    : IUsuarioLocalizacaoService
{
    private readonly NpgsqlDataSource? dataSource = dataSources.FirstOrDefault();

    public async Task AtualizarLocalizacaoAsync(
        Guid idUsuario,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCoordenadas(request);

        if (dataSource is null)
        {
            throw new ConflictException("DATABASE_CONNECTION_STRING nao configurada.");
        }

        var canonico = await geocodingClient.ReverseGeocodeAsync(
            request.Latitude,
            request.Longitude,
            cancellationToken);

        var localizacaoPersistida = new AtualizarLocalizacaoRequest(
            FormattedAddress: canonico.FormattedAddress,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            PlaceId: canonico.PlaceId,
            Provider: canonico.Provider);

        await using var command = dataSource.CreateCommand("""
            UPDATE public.usuario
            SET localizacao = @localizacao
            WHERE id_usuario = @id_usuario
            """);

        command.Parameters.AddWithValue("id_usuario", idUsuario);
        command.Parameters.Add("localizacao", NpgsqlDbType.Jsonb).Value =
            JsonSerializer.Serialize(localizacaoPersistida);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows == 0)
        {
            throw new NotFoundException("Usuario nao encontrado para atualizar localizacao.");
        }
    }

    private static void ValidateCoordenadas(AtualizarLocalizacaoRequest request)
    {
        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["latitude"] = ["Latitude deve estar entre -90 e 90."],
                ["longitude"] = ["Longitude deve estar entre -180 e 180."]
            });
        }
    }
}
