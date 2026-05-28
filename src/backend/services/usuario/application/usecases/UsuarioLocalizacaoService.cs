using System.IdentityModel.Tokens.Jwt;
using Npgsql;
using Spotlight.Usuario.Application.Dtos;

namespace Spotlight.Usuario.Application.UseCases;

public interface IUsuarioLocalizacaoService
{
    Task AtualizarLocalizacaoAsync(
        string? authorizationHeader,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken);
}

public sealed class UsuarioLocalizacaoService(IEnumerable<NpgsqlDataSource> dataSources)
    : IUsuarioLocalizacaoService
{
    private readonly NpgsqlDataSource? dataSource = dataSources.FirstOrDefault();

    public async Task AtualizarLocalizacaoAsync(
        string? authorizationHeader,
        AtualizarLocalizacaoRequest request,
        CancellationToken cancellationToken)
    {
        var idUsuario = GetUserIdFromBearerToken(authorizationHeader);
        Validate(request);

        if (dataSource is null)
        {
            throw new UsuarioPersistenceException("DATABASE_CONNECTION_STRING nao configurada.");
        }

        await using var command = dataSource.CreateCommand("""
            UPDATE public.usuario
            SET localizacao = @localizacao
            WHERE id_usuario = @id_usuario
            """);

        command.Parameters.AddWithValue("id_usuario", idUsuario);
        command.Parameters.AddWithValue("localizacao", TrimToColumn(request.FormattedAddress, 150));

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows == 0)
        {
            throw new UsuarioPersistenceException("Usuario nao encontrado para atualizar localizacao.");
        }
    }

    private static Guid GetUserIdFromBearerToken(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UsuarioUnauthorizedException("Token de acesso ausente.");
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var sub = jwt.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(sub, out var idUsuario)
            ? idUsuario
            : throw new UsuarioUnauthorizedException("Token de acesso invalido.");
    }

    private static void Validate(AtualizarLocalizacaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FormattedAddress))
        {
            throw new UsuarioValidationException("Endereco formatado e obrigatorio.");
        }

        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            throw new UsuarioValidationException("Coordenadas invalidas.");
        }
    }

    private static string TrimToColumn(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

public sealed class UsuarioValidationException(string message) : Exception(message);

public sealed class UsuarioUnauthorizedException(string message) : Exception(message);

public sealed class UsuarioPersistenceException(string message) : Exception(message);
