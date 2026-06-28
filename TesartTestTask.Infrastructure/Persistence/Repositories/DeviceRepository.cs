using Microsoft.EntityFrameworkCore;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence.Repositories;

public sealed class DeviceRepository(SqliteTesartDbContextFactory dbContextFactory) : IDeviceRepository
{
    private readonly SqliteTesartDbContextFactory _dbContextFactory = dbContextFactory;

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Devices
            .AsNoTracking()
            .OrderBy(device => device.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(device => device.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Devices.CountAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Device> devices, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        dbContext.Devices.AddRange(devices);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}