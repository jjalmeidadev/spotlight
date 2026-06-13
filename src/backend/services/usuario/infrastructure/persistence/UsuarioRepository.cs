using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Spotlight.Usuario.Application.Dtos;
using Spotlight.Usuario.Application.UseCases;
using Spotlight.Usuario.Domain.Entities;

namespace Spotlight.Usuario.Infrastructure.Persistence;

/// <summary>
/// Implementação de <see cref="IUsuarioRepository"/> com SQL puro via Npgsql.
/// Lê/escreve em <c>usuario</c>, inclusive o JSONB <c>perfil_preferencias</c>.
/// </summary>
public class UsuarioRepository(NpgsqlDataSource dataSource) : IUsuarioRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Domain.Entities.Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT id_usuario, nome, email, data_nascimento, localizacao,
                   tipo_usuario::text, data_cadastro
            FROM public.usuario
            WHERE id_usuario = @id;
            """;
        cmd.Parameters.AddWithValue("id", id);

        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await r.ReadAsync(cancellationToken)) return null;

        return new Domain.Entities.Usuario
        {
            Id = r.GetGuid(0),
            Nome = r.GetString(1),
            Email = r.GetString(2),
            DataNascimento = r.IsDBNull(3) ? null : r.GetFieldValue<DateOnly>(3),
            Localizacao = r.IsDBNull(4) ? null : r.GetString(4),
            TipoUsuario = r.GetString(5),
            DataCadastro = r.GetDateTime(6)
        };
    }

    public async Task GarantirPerfilAsync(
        Guid id,
        string email,
        string? nome,
        DateOnly? dataNascimento,
        CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            INSERT INTO public.usuario (id_usuario, nome, email, data_nascimento)
            SELECT @id, @nome, @email, @data_nascimento
            WHERE EXISTS (
                SELECT 1
                FROM auth.users
                WHERE id = @id
            )
            ON CONFLICT (id_usuario) DO NOTHING;
            """;
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("nome", string.IsNullOrWhiteSpace(nome)
            ? email.Split('@', 2)[0]
            : nome.Trim());
        cmd.Parameters.AddWithValue("email", email.Trim());
        cmd.Parameters.AddWithValue("data_nascimento", dataNascimento is null
            ? DBNull.Value
            : dataNascimento.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> AtualizarAsync(Guid id, UsuarioUpdateDto dados, CancellationToken cancellationToken)
    {
        // Confirma existência antes do update — devolve 404 cedo se o id sumiu.
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using (var existe = conexao.CreateCommand())
        {
            existe.CommandText = "SELECT 1 FROM public.usuario WHERE id_usuario = @id;";
            existe.Parameters.AddWithValue("id", id);
            if (await existe.ExecuteScalarAsync(cancellationToken) is null) return false;
        }

        var sets = new List<string>();
        var parametros = new List<(string Nome, object Valor)>();

        void Add(string set, string nome, object? valor)
        {
            if (valor is null) return;
            sets.Add(set);
            parametros.Add((nome, valor));
        }

        Add("nome = @nome", "nome", dados.Nome);
        Add("data_nascimento = @nasc", "nasc", dados.DataNascimento);

        if (sets.Count == 0) return true; // nada a atualizar — semântica no-op

        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = $"UPDATE public.usuario SET {string.Join(", ", sets)} WHERE id_usuario = @id;";
        cmd.Parameters.AddWithValue("id", id);
        foreach (var (nome, valor) in parametros) cmd.Parameters.AddWithValue(nome, valor);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    public async Task<Preferencias?> ObterPreferenciasAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT perfil_preferencias::text FROM public.usuario WHERE id_usuario = @id;";
        cmd.Parameters.AddWithValue("id", id);

        var raw = await cmd.ExecuteScalarAsync(cancellationToken) as string;
        if (raw is null) return null;

        // perfil_preferencias começa como {} (default) — tratamos como Preferencias vazias.
        var prefs = JsonSerializer.Deserialize<Preferencias>(raw, JsonOpts) ?? new Preferencias();
        return prefs;
    }

    public async Task<bool> AtualizarPreferenciasAsync(
        Guid id, Preferencias dados, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            UPDATE public.usuario SET perfil_preferencias = @prefs
            WHERE id_usuario = @id;
            """;
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.Add("prefs", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(dados, JsonOpts);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
