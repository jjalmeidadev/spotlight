using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Application.UseCases;

/// <summary>
/// Cria um espetáculo. Valida os campos obrigatórios e o enum de status,
/// delega a persistência (transacional) ao repositório e retorna o detalhe
/// já carregado da nova linha (espelha a resposta 201 do contrato).
/// </summary>
public class CriarEspetaculoUseCase(IEspetaculoRepository repositorio)
{
    public async Task<EspetaculoDetalheDto> ExecutarAsync(
        EspetaculoCreateDto dados, CancellationToken cancellationToken)
    {
        Validacoes.ValidarEspetaculo(dados.Titulo, dados.Duracao, dados.Status, dados.Atores, obrigaTitulo: true);

        var id = await repositorio.CriarAsync(dados, cancellationToken);
        var detalhe = await repositorio.ObterDetalheAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Detalhe do espetáculo {id} não encontrado logo após inserção (estado inesperado).");

        return ObterEspetaculoUseCase.MapearDetalhe(detalhe);
    }
}
