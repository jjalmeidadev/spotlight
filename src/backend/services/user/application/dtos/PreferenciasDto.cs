namespace Spotlight.Usuario.Application.Dtos;

/// <summary>
/// Preferências do usuário — usadas tanto como resposta de GET quanto como body
/// de PUT em <c>/usuario/me/preferencias</c>.
/// </summary>
public class PreferenciasDto
{
    public List<string> GenerosFavoritos { get; set; } = [];
    public List<int> CategoriasFavoritas { get; set; } = [];
    public decimal? FaixaPrecoMax { get; set; }
}
