using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

public class ListarTeatrosUseCase(ITeatroRepository repositorio)
{
    public async Task<ListaDto<TeatroDto>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var itens = await repositorio.ListarAsync(cancellationToken);
        return new ListaDto<TeatroDto> { Data = itens.Select(Mapear).ToList() };
    }

    internal static TeatroDto Mapear(Teatro t) => new()
    {
        Id = t.Id,
        Nome = t.Nome,
        Endereco = t.Endereco,
        Cidade = t.Cidade,
        Estado = t.Estado,
        Capacidade = t.Capacidade,
        Acessibilidade = t.Acessibilidade,
        ParceiroNamingRights = t.ParceiroNamingRights
    };
}

public class ObterTeatroUseCase(ITeatroRepository repositorio)
{
    public async Task<TeatroDto> ExecutarAsync(int id, CancellationToken cancellationToken)
    {
        var teatro = await repositorio.ObterAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Teatro {id} não encontrado.");
        return ListarTeatrosUseCase.Mapear(teatro);
    }
}
