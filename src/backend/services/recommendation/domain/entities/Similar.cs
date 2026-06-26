namespace Spotlight.Recomendacao.Domain.Entities;

/// <summary>
/// Item da tabela <c>similaridade_item</c> — similaridade global item-item
/// (igual para todos os usuários), populada em batch pelo k-NN do Motor de ML
/// e lida em rota pública.
/// </summary>
public class Similar
{
    public EspetaculoRecomendado Espetaculo { get; init; } = new();
    public decimal Score { get; init; }
    public string TipoAlgoritmo { get; init; } = string.Empty;
}
