using Microsoft.Extensions.Logging;
using Spotlight.Recomendacao.Application.Dtos;

namespace Spotlight.Recomendacao.Application.UseCases;

/// <summary>
/// Recomendações pré-processadas do usuário autenticado. Normaliza o
/// <c>limite</c> dentro de 1..50 (default 10) — limites definidos no contrato.
///
/// Enriquecimento geográfico (aditivo): após ler as recomendações do Postgres,
/// tenta resolver um <c>contextoGeografico</c> a partir da localização consentida
/// do usuário, fazendo reverse geocoding numa API externa (Nominatim).
///
/// As dependências de enriquecimento (<see cref="ILocalizacaoUsuarioRepository"/> e
/// <see cref="IGeocodingClient"/>) são <b>opcionais</b>: quando ausentes (ex.: nos
/// testes unitários que constroem o caso de uso só com o repositório), o
/// comportamento é idêntico ao original (NO-OP, sem contexto geográfico).
/// </summary>
public class ListarRecomendacoesMeUseCase(
    IRecomendacaoRepository repositorio,
    ILocalizacaoUsuarioRepository? localizacaoRepositorio = null,
    IGeocodingClient? geocodingClient = null,
    ILogger<ListarRecomendacoesMeUseCase>? logger = null)
{
    private const int LimitePadrao = 10;
    private const int LimiteMaximo = 50;

    public async Task<ListaDto<RecomendacaoDto>> ExecutarAsync(
        Guid usuarioId, int? limite, CancellationToken cancellationToken)
    {
        var limiteNorm = limite switch
        {
            null or < 1 => LimitePadrao,
            > LimiteMaximo => LimiteMaximo,
            _ => limite.Value
        };

        var itens = await repositorio.ListarPorUsuarioAsync(usuarioId, limiteNorm, cancellationToken);

        // Enriquecimento geográfico defensivo: nunca pode quebrar o fluxo principal.
        var contextoGeografico = await ResolverContextoGeograficoAsync(usuarioId, cancellationToken);

        return new ListaDto<RecomendacaoDto>
        {
            Data = itens.Select(r => new RecomendacaoDto
            {
                Score = r.Score,
                TipoAlgoritmo = r.TipoAlgoritmo,
                Motivo = r.Motivo,
                DataGeracao = r.DataGeracao,
                ContextoGeografico = contextoGeografico,
                Espetaculo = new EspetaculoRecomendadoDto
                {
                    Id = r.Espetaculo.Id,
                    Titulo = r.Espetaculo.Titulo,
                    Genero = r.Espetaculo.Genero,
                    Status = r.Espetaculo.Status,
                    NotaMedia = r.Espetaculo.NotaMedia
                }
            }).ToList()
        };
    }

    /// <summary>
    /// Resolve o rótulo de contexto geográfico do usuário, ou <c>null</c>.
    ///
    /// Degradação graciosa (RNF03 — Confiabilidade): qualquer falha — dependência
    /// não configurada, usuário sem localização, ou indisponibilidade/timeout do
    /// geocoder externo — resulta em <c>null</c> (sem contexto), nunca numa exceção
    /// propagada. Assim, GET /recomendacao/me continua retornando 200 com as
    /// recomendações mesmo com o serviço externo fora do ar.
    /// </summary>
    private async Task<string?> ResolverContextoGeograficoAsync(
        Guid usuarioId, CancellationToken cancellationToken)
    {
        // NO-OP quando o enriquecimento não está habilitado (ex.: testes unitários).
        if (localizacaoRepositorio is null || geocodingClient is null)
        {
            return null;
        }

        try
        {
            var coordenadas = await localizacaoRepositorio.ObterCoordenadasAsync(usuarioId, cancellationToken);
            if (coordenadas is null)
            {
                return null; // usuário sem localização consentida — enriquecimento omitido
            }

            return await geocodingClient.ReverseGeocodeAsync(coordenadas, cancellationToken);
        }
        catch (Exception ex)
        {
            // Falha do serviço externo NÃO derruba a recomendação (RNF03).
            logger?.LogWarning(ex,
                "Reverse geocoding indisponível para o usuário {UsuarioId}; " +
                "retornando recomendações sem contexto geográfico.", usuarioId);
            return null;
        }
    }
}
