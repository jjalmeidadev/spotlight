using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

public interface IUsuarioLocalizacaoService
{
    Task AtualizarLocalizacaoAsync(
        Guid idUsuario,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken);
}

public sealed class UsuarioLocalizacaoService(IEnumerable<NpgsqlDataSource> dataSources)
    : IUsuarioLocalizacaoService
{
    private readonly NpgsqlDataSource? dataSource = dataSources.FirstOrDefault();

    public async Task AtualizarLocalizacaoAsync(
        Guid idUsuario,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        if (dataSource is null)
        {
            throw new ConflictException("DATABASE_CONNECTION_STRING nao configurada.");
        }

        await using var command = dataSource.CreateCommand("""
            UPDATE public.usuario
            SET localizacao = @localizacao
            WHERE id_usuario = @id_usuario
            """);

        command.Parameters.AddWithValue("id_usuario", idUsuario);
        command.Parameters.Add("localizacao", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(request);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows == 0)
        {
            throw new NotFoundException("Usuario nao encontrado para atualizar localizacao.");
        }
    }

    private static void Validate(AtualizarLocalizacaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FormattedAddress))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["formattedAddress"] = ["Endereco formatado e obrigatorio."]
            });
        }

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
