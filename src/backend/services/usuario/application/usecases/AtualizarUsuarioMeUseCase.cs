using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Application.UseCases;

/// <summary>
/// Atualiza parcialmente o perfil do usuário autenticado (nome, dataNascimento,
/// localizacao). Valida os limites de tamanho do contrato.
/// </summary>
public class AtualizarUsuarioMeUseCase(IUsuarioRepository repositorio)
{
    private const int TamanhoMaximoNome = 150;
    private const int TamanhoMaximoLocalizacao = 150;

    public async Task<UsuarioDto> ExecutarAsync(
        Guid usuarioId, UsuarioUpdateDto dados, CancellationToken cancellationToken)
    {
        Validar(dados);

        var atualizado = await repositorio.AtualizarAsync(usuarioId, dados, cancellationToken);
        if (!atualizado) throw new NotFoundException("Perfil do usuário autenticado não encontrado.");

        var perfil = await repositorio.ObterAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Perfil do usuário autenticado não encontrado.");

        return ObterUsuarioMeUseCase.Mapear(perfil);
    }

    private static void Validar(UsuarioUpdateDto dados)
    {
        var erros = new Dictionary<string, string[]>();

        if (dados.Nome is { Length: > TamanhoMaximoNome })
        {
            erros["nome"] = [$"O nome deve ter no máximo {TamanhoMaximoNome} caracteres."];
        }
        if (dados.Nome is not null && string.IsNullOrWhiteSpace(dados.Nome))
        {
            erros["nome"] = ["O nome não pode ser vazio."];
        }
        if (dados.Localizacao is { Length: > TamanhoMaximoLocalizacao })
        {
            erros["localizacao"] = [$"A localização deve ter no máximo {TamanhoMaximoLocalizacao} caracteres."];
        }

        if (erros.Count > 0) throw new ValidationException(erros);
    }
}
