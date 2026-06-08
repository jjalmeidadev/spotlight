namespace Spotlight.Recomendacao.Domain.ValueObjects;

/// <summary>
/// Coordenadas geográficas (latitude/longitude) extraídas da localização
/// consentida do usuário (<c>usuario.localizacao</c>, JSONB). Usadas para o
/// enriquecimento geográfico das recomendações via reverse geocoding.
/// </summary>
public sealed record Coordenadas(double Latitude, double Longitude);
