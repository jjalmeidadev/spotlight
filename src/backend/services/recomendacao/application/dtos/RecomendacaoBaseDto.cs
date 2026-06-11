namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Contrato base da API de recomendacao exposta pelo gateway em <c>/recomendacao</c>.</summary>
public class RecomendacaoBaseDto
{
    public string Service { get; init; } = "recomendacao";
    public string Status { get; init; } = "ok";
    public string Version { get; init; } = "v1";
    public IReadOnlyList<RecomendacaoEndpointDto> Endpoints { get; init; } = [];
}

/// <summary>Endpoint disponivel na API publica de recomendacao.</summary>
public class RecomendacaoEndpointDto
{
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Auth { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
