namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Contrato base da API de recomendacao exposta pelo gateway em <c>/recomendacao</c>.</summary>
public class RecomendacaoBaseDto
{
    /// <summary>Nome do serviço (fixo: <c>recomendacao</c>).</summary>
    public string Service { get; init; } = "recomendacao";

    /// <summary>Status sumário do serviço (ex.: <c>ok</c>).</summary>
    public string Status { get; init; } = "ok";

    /// <summary>Versão pública do contrato (ex.: <c>v1</c>).</summary>
    public string Version { get; init; } = "v1";

    /// <summary>Lista de endpoints públicos com método, caminho e nível de autenticação.</summary>
    public IReadOnlyList<RecomendacaoEndpointDto> Endpoints { get; init; } = [];
}

/// <summary>Endpoint disponivel na API publica de recomendacao.</summary>
public class RecomendacaoEndpointDto
{
    /// <summary>Verbo HTTP (ex.: <c>GET</c>).</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Caminho público completo (ex.: <c>/recomendacao/me</c>).</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Nível de autenticação (<c>publico</c> ou nome da policy, ex.: <c>cadastrado</c>).</summary>
    public string Auth { get; init; } = string.Empty;

    /// <summary>Descrição curta do que o endpoint faz.</summary>
    public string Description { get; init; } = string.Empty;
}
