using Spotlight.Catalogo.Application.Dtos;

namespace Spotlight.Catalogo.Application.UseCases;

public class ListarCategoriasUseCase(ICategoriaRepository repositorio)
{
    public async Task<ListaDto<CategoriaDto>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var itens = await repositorio.ListarAsync(cancellationToken);
        return new ListaDto<CategoriaDto>
        {
            Data = itens.Select(c => new CategoriaDto
            {
                Id = c.Id, Nome = c.Nome, Descricao = c.Descricao
            }).ToList()
        };
    }
}
