using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public class ExcluirEspetaculoUseCase(IEspetaculoRepository repositorio)
{
    public async Task ExecutarAsync(int id, CancellationToken cancellationToken)
    {
        var excluido = await repositorio.ExcluirAsync(id, cancellationToken);
        if (!excluido) throw new NotFoundException($"Espetáculo {id} não encontrado.");
    }
}
