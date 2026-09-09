namespace Spotlight.Recomendacao.Application.Dtos;

/// <summary>Item retornado por <c>GET /recomendacao/me</c>.</summary>
public class RecomendacaoDto
{
    /// <summary>Espetáculo recomendado (subconjunto de campos do catálogo).</summary>
    public EspetaculoRecomendadoDto Espetaculo { get; init; } = new();

    /// <summary>Score de afinidade no intervalo [0, 1]. Quanto maior, melhor.</summary>
    public decimal Score { get; init; }

    /// <summary>Identificador do algoritmo usado (ex.: <c>ml_net</c>, <c>postgres_preprocessado</c>).</summary>
    public string TipoAlgoritmo { get; init; } = string.Empty;

    /// <summary>Motivo curto/legível para a sugestão, quando disponível (ex.: <c>preferencias_genero</c>).</summary>
    public string? Motivo { get; init; }

    /// <summary>Instante (UTC) em que a recomendação foi gerada.</summary>
    public DateTime DataGeracao { get; init; }
}

/// <summary>Subconjunto de campos do espetáculo expostos junto da recomendação.</summary>
public class EspetaculoRecomendadoDto
{
    /// <summary>Identificador do espetáculo no catálogo.</summary>
    public int Id { get; init; }

    /// <summary>Título do espetáculo.</summary>
    public string Titulo { get; init; } = string.Empty;

    /// <summary>Gênero principal (pode ser nulo).</summary>
    public string? Genero { get; init; }

    /// <summary>Status do espetáculo no catálogo (ex.: <c>ativo</c>, <c>encerrado</c>).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Nota média das avaliações no intervalo [0, 5].</summary>
    public decimal NotaMedia { get; init; }

    /// <summary>Mídias associadas (capas, banners) ordenadas por <c>OrdemExibicao</c>.</summary>
    public IReadOnlyList<EspetaculoMidiaDto> Midias { get; init; } = [];
}

/// <summary>Arquivo associado ao espetáculo no bucket Supabase Storage.</summary>
public class EspetaculoMidiaDto
{
    /// <summary>Tipo da mídia (ex.: <c>capa</c>, <c>banner</c>).</summary>
    public string TipoMidia { get; init; } = string.Empty;

    /// <summary>Nome do bucket no Supabase Storage.</summary>
    public string StorageBucket { get; init; } = string.Empty;

    /// <summary>Caminho do arquivo dentro do bucket.</summary>
    public string StoragePath { get; init; } = string.Empty;

    /// <summary>Texto alternativo para acessibilidade.</summary>
    public string? AltText { get; init; }

    /// <summary>Ordem de exibição (menor aparece primeiro).</summary>
    public int OrdemExibicao { get; init; }
}

/// <summary>Envelope <c>{ "data": [...] }</c> sem paginação.</summary>
public class ListaDto<T>
{
    /// <summary>Itens retornados — pode ser uma lista vazia.</summary>
    public IReadOnlyList<T> Data { get; init; } = [];
}
