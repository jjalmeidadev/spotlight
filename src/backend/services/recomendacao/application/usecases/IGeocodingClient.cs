using Spotlight.Recomendacao.Domain.ValueObjects;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Cliente da API externa de geocodificação (OpenStreetMap/Nominatim). Faz o
/// reverse geocoding das coordenadas do usuário para obter um rótulo legível de
/// cidade/região (o <c>contextoGeografico</c> anexado às recomendações).
///
/// É uma <b>dependência consumida</b> (terceiro externo, sem controle de
/// deploy/uptime). Por isso a orquestração trata qualquer falha como degradação
/// graciosa — ver <see cref="ListarRecomendacoesMeUseCase"/>.
/// </summary>
public interface IGeocodingClient
{
    /// <summary>
    /// Resolve um rótulo de localização (ex.: "São Paulo") a partir das coordenadas.
    /// Lança exceção em falha de rede/HTTP — o tratamento (degradação) fica a cargo
    /// do caso de uso, mantendo este contrato simples e testável.
    /// </summary>
    Task<string?> ReverseGeocodeAsync(Coordenadas coordenadas, CancellationToken cancellationToken);
}
