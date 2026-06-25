using Microsoft.EntityFrameworkCore;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence;

public sealed class TesartDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();

    public DbSet<Measurement> Measurements => Set<Measurement>();

    public TesartDbContext(DbContextOptions<TesartDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TesartDbContext).Assembly);
    }
}