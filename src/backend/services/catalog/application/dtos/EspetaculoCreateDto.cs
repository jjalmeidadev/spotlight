namespace Spotlight.Catalogo.Application.Dtos;

/// <summary>Body de <c>POST /catalogo/espetaculos</c> (administrador).</summary>
public class EspetaculoCreateDto
{
    /// <summary>Obrigatório. Tamanho máximo 200.</summary>
    public string? Titulo { get; set; }

    public string? Descricao { get; set; }
    public string? Genero { get; set; }
    public string? FaixaEtaria { get; set; }

    /// <summary>Minutos. Deve ser maior que zero quando informado.</summary>
    public int? Duracao { get; set; }

    public string? ClassificacaoIndicativa { get; set; }
    public string? Produtora { get; set; }

    /// <summary>Default <c>em_breve</c> quando ausente.</summary>
    public string? Status { get; set; }

    public List<int>? CategoriaIds { get; set; }
    public List<AtorInputDto>? Atores { get; set; }
}

/// <summary>Body de <c>PUT /catalogo/espetaculos/{id}</c>. Todos os campos são opcionais
/// — só os enviados (não-nulos) são atualizados. Listas, se enviadas, substituem.</summary>
public class EspetaculoUpdateDto
{
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? Genero { get; set; }
    public string? FaixaEtaria { get; set; }
    public int? Duracao { get; set; }
    public string? ClassificacaoIndicativa { get; set; }
    public string? Produtora { get; set; }
    public string? Status { get; set; }
    public List<int>? CategoriaIds { get; set; }
    public List<AtorInputDto>? Atores { get; set; }
}

/// <summary>Item da lista <c>atores</c> nos requests de criação/atualização de espetáculo.</summary>
public class AtorInputDto
{
    public int? AtorId { get; set; }
    public string? Papel { get; set; }
    public int? OrdemExibicao { get; set; }
}
