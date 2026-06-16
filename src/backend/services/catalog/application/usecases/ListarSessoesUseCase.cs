using Spotlight.Catalogo.Application.Dtos;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Lista sessões de um espetáculo. Se o espetáculo não existir, lança
/// <see cref="Domain.Entities.NotFoundException"/> (→ 404 no contrato).
/// </summary>
public class ListarSessoesUseCase(
    ISessaoRepository sessaoRepositorio, IEspetaculoRepository espetaculoRepositorio)
{
    public async Task<ListaDto<SessaoDto>> ExecutarAsync(int espetaculoId, CancellationToken cancellationToken)
    {
        if (!await espetaculoRepositorio.ExisteAsync(espetaculoId, cancellationToken))
        {
            throw new Domain.Entities.NotFoundException($"Espetáculo {espetaculoId} não encontrado.");
        }

        var sessoes = await sessaoRepositorio.ListarPorEspetaculoAsync(espetaculoId, cancellationToken);
        return new ListaDto<SessaoDto> { Data = sessoes.Select(ObterEspetaculoUseCase.MapearSessao).ToList() };
    }
}
