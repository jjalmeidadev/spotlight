using Npgsql;
using Spotlight.Catalogo.Application.Dtos;
using Spotlight.Catalogo.Application.UseCases;
using Spotlight.Catalogo.Domain.Entities;

namespace Spotlight.Catalogo.Infrastructure.Persistence;

/// <summary>
/// Implementação de <see cref="IEspetaculoRepository"/> com SQL puro via Npgsql
/// (ADO.NET, sem ORM). Usa o <see cref="NpgsqlDataSource"/> registrado no container
/// (pool de conexões) e consultas totalmente parametrizadas.
/// </summary>
public class EspetaculoRepository(NpgsqlDataSource dataSource) : IEspetaculoRepository
{
    public async Task<(IReadOnlyList<Espetaculo> Itens, int Total)> ListarAsync(
        ListarEspetaculosFiltro filtro, CancellationToken cancellationToken)
    {
        var (where, parametros) = MontarFiltro(filtro);

        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);

        // 1) Total de registros que satisfazem o filtro (para a paginação).
        int total;
        await using (var cmdTotal = conexao.CreateCommand())
        {
            cmdTotal.CommandText = $"SELECT COUNT(*) FROM espetaculo e {where};";
            AplicarParametros(cmdTotal, parametros);
            total = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync(cancellationToken));
        }

        // 2) Página de resultados.
        var itens = new List<Espetaculo>();
        await using (var cmd = conexao.CreateCommand())
        {
            cmd.CommandText = $"""
                SELECT e.id_espetaculo, e.titulo, e.genero, e.status::text AS status,
                       e.faixa_etaria, e.classificacao_indicativa, e.duracao,
                       e.nota_media, e.popularidade
                FROM espetaculo e
                {where}
                ORDER BY {MapearOrdenacao(filtro.OrdenarPor)}
                LIMIT @limit OFFSET @offset;
                """;
            AplicarParametros(cmd, parametros);
            cmd.Parameters.AddWithValue("limit", filtro.PageSize);
            cmd.Parameters.AddWithValue("offset", (filtro.Page - 1) * filtro.PageSize);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                itens.Add(Mapear(reader));
            }
        }

        return (itens, total);
    }

    /// <summary>Monta a cláusula WHERE e a lista de parâmetros conforme os filtros ativos.</summary>
    private static (string Where, List<(string Nome, object Valor)> Parametros) MontarFiltro(
        ListarEspetaculosFiltro filtro)
    {
        var condicoes = new List<string>();
        var parametros = new List<(string, object)>();

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            condicoes.Add("e.titulo ILIKE @q");
            parametros.Add(("q", $"%{filtro.Q.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Status))
        {
            condicoes.Add("e.status::text = @status");
            parametros.Add(("status", filtro.Status));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Genero))
        {
            condicoes.Add("e.genero = @genero");
            parametros.Add(("genero", filtro.Genero));
        }

        if (filtro.CategoriaId is int categoriaId)
        {
            condicoes.Add(
                "EXISTS (SELECT 1 FROM espetaculo_categoria ec " +
                "WHERE ec.id_espetaculo = e.id_espetaculo AND ec.id_categoria = @categoriaId)");
            parametros.Add(("categoriaId", categoriaId));
        }

        var where = condicoes.Count > 0 ? "WHERE " + string.Join(" AND ", condicoes) : string.Empty;
        return (where, parametros);
    }

    private static void AplicarParametros(NpgsqlCommand cmd, List<(string Nome, object Valor)> parametros)
    {
        foreach (var (nome, valor) in parametros)
        {
            cmd.Parameters.AddWithValue(nome, valor);
        }
    }

    /// <summary>
    /// Traduz o parâmetro de ordenação para SQL fixo (whitelist) — defesa contra
    /// injeção, já que ORDER BY não pode ser parametrizado.
    /// </summary>
    private static string MapearOrdenacao(string? ordenarPor) => ordenarPor switch
    {
        "nota_media" => "e.nota_media DESC",
        "titulo" => "e.titulo ASC",
        _ => "e.popularidade DESC"
    };

    private static Espetaculo Mapear(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Titulo = reader.GetString(1),
        Genero = reader.IsDBNull(2) ? null : reader.GetString(2),
        Status = reader.GetString(3),
        FaixaEtaria = reader.IsDBNull(4) ? null : reader.GetString(4),
        ClassificacaoIndicativa = reader.IsDBNull(5) ? null : reader.GetString(5),
        Duracao = reader.IsDBNull(6) ? null : reader.GetInt32(6),
        NotaMedia = reader.GetDecimal(7),
        Popularidade = reader.GetDecimal(8)
    };

    // -----------------------------------------------------------------------
    // Detalhe (GET /catalogo/espetaculos/{id}) — usa 4 queries pequenas em vez
    // de um JOIN cartesiano (evita explosão de linhas com N categorias × M atores).
    // -----------------------------------------------------------------------

    public async Task<EspetaculoDetalhe?> ObterDetalheAsync(int id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);

        var detalhe = await CarregarEspetaculoAsync(conexao, id, cancellationToken);
        if (detalhe is null) return null;

        var categorias = await CarregarCategoriasAsync(conexao, id, cancellationToken);
        var atores = await CarregarAtoresAsync(conexao, id, cancellationToken);
        var sessoes = await CarregarSessoesAsync(conexao, id, cancellationToken);

        return new EspetaculoDetalhe
        {
            Id = detalhe.Id,
            Titulo = detalhe.Titulo,
            Descricao = detalhe.Descricao,
            Genero = detalhe.Genero,
            Status = detalhe.Status,
            FaixaEtaria = detalhe.FaixaEtaria,
            ClassificacaoIndicativa = detalhe.ClassificacaoIndicativa,
            Duracao = detalhe.Duracao,
            Produtora = detalhe.Produtora,
            NotaMedia = detalhe.NotaMedia,
            Popularidade = detalhe.Popularidade,
            CreatedAt = detalhe.CreatedAt,
            UpdatedAt = detalhe.UpdatedAt,
            Categorias = categorias,
            Atores = atores,
            Sessoes = sessoes
        };
    }

    private static async Task<EspetaculoDetalhe?> CarregarEspetaculoAsync(
        NpgsqlConnection conexao, int id, CancellationToken cancellationToken)
    {
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT id_espetaculo, titulo, descricao, genero, status::text AS status,
                   faixa_etaria, classificacao_indicativa, duracao, produtora,
                   nota_media, popularidade, created_at, updated_at
            FROM espetaculo
            WHERE id_espetaculo = @id;
            """;
        cmd.Parameters.AddWithValue("id", id);

        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await r.ReadAsync(cancellationToken)) return null;

        return new EspetaculoDetalhe
        {
            Id = r.GetInt32(0),
            Titulo = r.GetString(1),
            Descricao = r.IsDBNull(2) ? null : r.GetString(2),
            Genero = r.IsDBNull(3) ? null : r.GetString(3),
            Status = r.GetString(4),
            FaixaEtaria = r.IsDBNull(5) ? null : r.GetString(5),
            ClassificacaoIndicativa = r.IsDBNull(6) ? null : r.GetString(6),
            Duracao = r.IsDBNull(7) ? null : r.GetInt32(7),
            Produtora = r.IsDBNull(8) ? null : r.GetString(8),
            NotaMedia = r.GetDecimal(9),
            Popularidade = r.GetDecimal(10),
            CreatedAt = r.GetDateTime(11),
            UpdatedAt = r.GetDateTime(12)
        };
    }

    private static async Task<IReadOnlyList<CategoriaResumo>> CarregarCategoriasAsync(
        NpgsqlConnection conexao, int espetaculoId, CancellationToken cancellationToken)
    {
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT c.id_categoria, c.nome
            FROM categoria c
            JOIN espetaculo_categoria ec USING (id_categoria)
            WHERE ec.id_espetaculo = @id
            ORDER BY c.nome;
            """;
        cmd.Parameters.AddWithValue("id", espetaculoId);

        var itens = new List<CategoriaResumo>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            itens.Add(new CategoriaResumo { Id = r.GetInt32(0), Nome = r.GetString(1) });
        }
        return itens;
    }

    private static async Task<IReadOnlyList<AtorElenco>> CarregarAtoresAsync(
        NpgsqlConnection conexao, int espetaculoId, CancellationToken cancellationToken)
    {
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT a.id_ator, a.nome, ea.papel, ea.ordem_exibicao
            FROM ator a
            JOIN espetaculo_ator ea USING (id_ator)
            WHERE ea.id_espetaculo = @id
            ORDER BY ea.ordem_exibicao NULLS LAST, a.nome;
            """;
        cmd.Parameters.AddWithValue("id", espetaculoId);

        var itens = new List<AtorElenco>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            itens.Add(new AtorElenco
            {
                Id = r.GetInt32(0),
                Nome = r.GetString(1),
                Papel = r.IsDBNull(2) ? null : r.GetString(2),
                OrdemExibicao = r.IsDBNull(3) ? null : r.GetInt32(3)
            });
        }
        return itens;
    }

    private static async Task<IReadOnlyList<Sessao>> CarregarSessoesAsync(
        NpgsqlConnection conexao, int espetaculoId, CancellationToken cancellationToken)
    {
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT s.id_sessao, s.data, s.horario, s.preco_minimo, s.preco_maximo,
                   s.disponibilidade, s.link_compra,
                   t.id_teatro, t.nome, t.cidade, t.estado
            FROM sessao s
            JOIN teatro t ON t.id_teatro = s.id_teatro
            WHERE s.id_espetaculo = @id
            ORDER BY s.data, s.horario;
            """;
        cmd.Parameters.AddWithValue("id", espetaculoId);

        var itens = new List<Sessao>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            itens.Add(new Sessao
            {
                Id = r.GetInt32(0),
                Data = r.GetFieldValue<DateOnly>(1),
                Horario = r.GetFieldValue<TimeOnly>(2),
                PrecoMinimo = r.IsDBNull(3) ? null : r.GetDecimal(3),
                PrecoMaximo = r.IsDBNull(4) ? null : r.GetDecimal(4),
                Disponibilidade = r.GetInt32(5),
                LinkCompra = r.IsDBNull(6) ? null : r.GetString(6),
                Teatro = new TeatroResumo
                {
                    Id = r.GetInt32(7),
                    Nome = r.GetString(8),
                    Cidade = r.GetString(9),
                    Estado = r.GetString(10)
                }
            });
        }
        return itens;
    }

    // -----------------------------------------------------------------------
    // Existência (usada por rotas aninhadas para devolver 404 cedo)
    // -----------------------------------------------------------------------

    public async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM espetaculo WHERE id_espetaculo = @id;";
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    // -----------------------------------------------------------------------
    // CRUD admin (transacional)
    // -----------------------------------------------------------------------

    public async Task<int> CriarAsync(EspetaculoCreateDto dados, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await conexao.BeginTransactionAsync(cancellationToken);

        int id;
        await using (var cmd = conexao.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO espetaculo (
                    titulo, descricao, genero, faixa_etaria, duracao,
                    classificacao_indicativa, produtora, status
                )
                VALUES (
                    @titulo, @descricao, @genero, @faixa, @duracao,
                    @classif, @produtora,
                    COALESCE(@status::status_espetaculo_enum, 'em_breve'::status_espetaculo_enum)
                )
                RETURNING id_espetaculo;
                """;
            cmd.Parameters.AddWithValue("titulo", dados.Titulo!);
            cmd.Parameters.AddWithValue("descricao", (object?)dados.Descricao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("genero", (object?)dados.Genero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("faixa", (object?)dados.FaixaEtaria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("duracao", (object?)dados.Duracao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("classif", (object?)dados.ClassificacaoIndicativa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("produtora", (object?)dados.Produtora ?? DBNull.Value);
            cmd.Parameters.AddWithValue("status", (object?)dados.Status ?? DBNull.Value);
            id = (int)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        }

        if (dados.CategoriaIds is { Count: > 0 })
        {
            await SubstituirCategoriasAsync(conexao, tx, id, dados.CategoriaIds, cancellationToken);
        }
        if (dados.Atores is { Count: > 0 })
        {
            await SubstituirAtoresAsync(conexao, tx, id, dados.Atores, cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return id;
    }

    public async Task<bool> AtualizarAsync(int id, EspetaculoUpdateDto dados, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await conexao.BeginTransactionAsync(cancellationToken);

        // Garante que o registro existe antes de aplicar updates parciais.
        await using (var cmdExiste = conexao.CreateCommand())
        {
            cmdExiste.Transaction = tx;
            cmdExiste.CommandText = "SELECT 1 FROM espetaculo WHERE id_espetaculo = @id;";
            cmdExiste.Parameters.AddWithValue("id", id);
            if (await cmdExiste.ExecuteScalarAsync(cancellationToken) is null)
            {
                return false;
            }
        }

        var sets = new List<string>();
        var parametros = new List<(string Nome, object Valor)>();

        void Add(string set, string nome, object? valor)
        {
            if (valor is null) return;
            sets.Add(set);
            parametros.Add((nome, valor));
        }

        Add("titulo = @titulo", "titulo", dados.Titulo);
        Add("descricao = @descricao", "descricao", dados.Descricao);
        Add("genero = @genero", "genero", dados.Genero);
        Add("faixa_etaria = @faixa", "faixa", dados.FaixaEtaria);
        Add("duracao = @duracao", "duracao", dados.Duracao);
        Add("classificacao_indicativa = @classif", "classif", dados.ClassificacaoIndicativa);
        Add("produtora = @produtora", "produtora", dados.Produtora);
        Add("status = @status::status_espetaculo_enum", "status", dados.Status);

        if (sets.Count > 0)
        {
            await using var cmd = conexao.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = $"UPDATE espetaculo SET {string.Join(", ", sets)} WHERE id_espetaculo = @id;";
            cmd.Parameters.AddWithValue("id", id);
            foreach (var (nome, valor) in parametros) cmd.Parameters.AddWithValue(nome, valor);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        if (dados.CategoriaIds is not null)
        {
            await SubstituirCategoriasAsync(conexao, tx, id, dados.CategoriaIds, cancellationToken);
        }
        if (dados.Atores is not null)
        {
            await SubstituirAtoresAsync(conexao, tx, id, dados.Atores, cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken)
    {
        await using var conexao = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = conexao.CreateCommand();
        cmd.CommandText = "DELETE FROM espetaculo WHERE id_espetaculo = @id;";
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    // -----------------------------------------------------------------------
    // Helpers de associação (replace-all dentro da transação)
    // -----------------------------------------------------------------------

    private static async Task SubstituirCategoriasAsync(
        NpgsqlConnection conexao, NpgsqlTransaction tx, int espetaculoId,
        IReadOnlyList<int> categoriaIds, CancellationToken cancellationToken)
    {
        await using (var del = conexao.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM espetaculo_categoria WHERE id_espetaculo = @id;";
            del.Parameters.AddWithValue("id", espetaculoId);
            await del.ExecuteNonQueryAsync(cancellationToken);
        }

        if (categoriaIds.Count == 0) return;

        await using var ins = conexao.CreateCommand();
        ins.Transaction = tx;
        ins.CommandText = """
            INSERT INTO espetaculo_categoria (id_espetaculo, id_categoria)
            SELECT @id, c FROM unnest(@ids) AS c
            ON CONFLICT DO NOTHING;
            """;
        ins.Parameters.AddWithValue("id", espetaculoId);
        ins.Parameters.AddWithValue("ids", categoriaIds.Distinct().ToArray());
        await ins.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SubstituirAtoresAsync(
        NpgsqlConnection conexao, NpgsqlTransaction tx, int espetaculoId,
        IReadOnlyList<AtorInputDto> atores, CancellationToken cancellationToken)
    {
        await using (var del = conexao.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM espetaculo_ator WHERE id_espetaculo = @id;";
            del.Parameters.AddWithValue("id", espetaculoId);
            await del.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var ator in atores.Where(a => a.AtorId.HasValue))
        {
            await using var ins = conexao.CreateCommand();
            ins.Transaction = tx;
            ins.CommandText = """
                INSERT INTO espetaculo_ator (id_espetaculo, id_ator, papel, ordem_exibicao)
                VALUES (@esp, @ator, @papel, @ordem)
                ON CONFLICT (id_espetaculo, id_ator) DO UPDATE
                SET papel = EXCLUDED.papel, ordem_exibicao = EXCLUDED.ordem_exibicao;
                """;
            ins.Parameters.AddWithValue("esp", espetaculoId);
            ins.Parameters.AddWithValue("ator", ator.AtorId!.Value);
            ins.Parameters.AddWithValue("papel", (object?)ator.Papel ?? DBNull.Value);
            ins.Parameters.AddWithValue("ordem", (object?)ator.OrdemExibicao ?? DBNull.Value);
            await ins.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
