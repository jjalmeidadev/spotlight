using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Caso de uso de <c>GET /catalogo/espetaculos/{id}</c>: busca detalhe completo
/// ou lança <see cref="NotFoundException"/> (mapeada para 404 pelo middleware).
/// </summary>
public class ObterEspetaculoUseCase(IEspetaculoRepository repositorio)
{
    public async Task<EspetaculoDetalheDto> ExecutarAsync(int id, CancellationToken cancellationToken)
    {
        var detalhe = await repositorio.ObterDetalheAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Espetáculo {id} não encontrado.");

        return MapearDetalhe(detalhe);
    }

    /// <summary>Reutilizado por <c>CriarEspetaculoUseCase</c> e <c>AtualizarEspetaculoUseCase</c>.</summary>
    internal static EspetaculoDetalheDto MapearDetalhe(EspetaculoDetalhe e) => new()
    {
        Id = e.Id,
        Titulo = e.Titulo,
        Descricao = e.Descricao,
        Genero = e.Genero,
        Status = e.Status,
        FaixaEtaria = e.FaixaEtaria,
        ClassificacaoIndicativa = e.ClassificacaoIndicativa,
        Duracao = e.Duracao,
        Produtora = e.Produtora,
        NotaMedia = e.NotaMedia,
        Popularidade = e.Popularidade,
        Categorias = e.Categorias.Select(c => new CategoriaResumoDto { Id = c.Id, Nome = c.Nome }).ToList(),
        Atores = e.Atores.Select(a => new AtorResumoDto
        {
            Id = a.Id, Nome = a.Nome, Papel = a.Papel, OrdemExibicao = a.OrdemExibicao
        }).ToList(),
        Sessoes = e.Sessoes.Select(MapearSessao).ToList(),
        Midias = e.Midias.Select(ListarEspetaculosUseCase.MapearMidia).ToList(),
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    internal static SessaoDto MapearSessao(Sessao s) => new()
    {
        Id = s.Id,
        Data = s.Data.ToString("yyyy-MM-dd"),
        Horario = s.Horario.ToString("HH:mm"),
        PrecoMinimo = s.PrecoMinimo,
        PrecoMaximo = s.PrecoMaximo,
        Disponibilidade = s.Disponibilidade,
        LinkCompra = s.LinkCompra,
        Teatro = new TeatroResumoDto
        {
            Id = s.Teatro.Id, Nome = s.Teatro.Nome, Cidade = s.Teatro.Cidade, Estado = s.Teatro.Estado
        }
    };
}
