using Microsoft.EntityFrameworkCore;

namespace TesartTestTask.Infrastructure.Persistence;

public sealed class SqliteTesartDbContextFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public TesartDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TesartDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new TesartDbContext(options);
    }
}