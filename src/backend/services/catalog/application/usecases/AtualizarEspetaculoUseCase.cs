using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Atualiza parcialmente um espetáculo. Campos não enviados (null) ficam como estão;
/// listas (categoriaIds / atores), se enviadas, substituem as associações.
/// 404 se o id não existe.
/// </summary>
public class AtualizarEspetaculoUseCase(IEspetaculoRepository repositorio)
{
    public async Task<EspetaculoDetalheDto> ExecutarAsync(
        int id, EspetaculoUpdateDto dados, CancellationToken cancellationToken)
    {
        Validacoes.ValidarEspetaculo(dados.Titulo, dados.Duracao, dados.Status, dados.Atores, obrigaTitulo: false);

        var atualizado = await repositorio.AtualizarAsync(id, dados, cancellationToken);
        if (!atualizado) throw new NotFoundException($"Espetáculo {id} não encontrado.");

        var detalhe = await repositorio.ObterDetalheAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Espetáculo {id} não encontrado.");

        return ObterEspetaculoUseCase.MapearDetalhe(detalhe);
    }
}
