using Spotlight.Recomendacao.Domain.ValueObjects;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Leitura da localização consentida do usuário (coluna <c>usuario.localizacao</c>,
/// JSONB). Retorna as coordenadas quando presentes e parseáveis, ou <c>null</c>
/// quando o usuário não tem localização ou o JSON não contém lat/lon — caso em
/// que o enriquecimento geográfico é simplesmente omitido (NO-OP).
/// </summary>
public interface ILocalizacaoUsuarioRepository
{
    Task<Coordenadas?> ObterCoordenadasAsync(Guid usuarioId, CancellationToken cancellationToken);
}
