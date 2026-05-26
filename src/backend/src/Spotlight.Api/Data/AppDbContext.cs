using Microsoft.EntityFrameworkCore;

namespace Spotlight.Api.Data;

/// <summary>
/// Contexto EF Core para o banco relacional do Spotlight (PostgreSQL/Supabase).
/// As entidades (Usuario, Espetaculo, Sessao, Teatro, ...) e seus DbSet serão
/// adicionados pelas features do T16. Aqui fica apenas o esqueleto de conexão.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Exemplo (T16):
    // public DbSet<Espetaculo> Espetaculos => Set<Espetaculo>();
}
