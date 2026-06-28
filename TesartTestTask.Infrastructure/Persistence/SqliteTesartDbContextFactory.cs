using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TesartTestTask.Infrastructure.Persistence;

public sealed class SqliteTesartDbContextFactory
{
    private readonly DbContextOptions<TesartDbContext> _options;

    public SqliteTesartDbContextFactory(string connectionString)
    {
        _options = new DbContextOptionsBuilder<TesartDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    public SqliteTesartDbContextFactory(SqliteConnection connection)
    {
        _options = new DbContextOptionsBuilder<TesartDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public TesartDbContext CreateDbContext()
    {
        return new TesartDbContext(_options);
    }
}