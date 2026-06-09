namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Body de <c>POST /catalogo/espetaculos/{id}/avaliacoes</c>.</summary>
public class AvaliacaoCreateDto
{
    /// <summary>Obrigatório. Deve estar entre 1 e 5 (validação no use case).</summary>
    public decimal? Nota { get; set; }

    public string? Comentario { get; set; }
}

/// <summary>Resposta 201 da criação de avaliação.</summary>
public class AvaliacaoCriadaDto
{
    public int Id { get; init; }
    public decimal Nota { get; init; }
    public string? Comentario { get; init; }
    public DateTime CreatedAt { get; init; }
}
