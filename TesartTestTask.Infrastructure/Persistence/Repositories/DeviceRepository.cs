using Microsoft.EntityFrameworkCore;
using TesartTestTask.Application.Repositories;
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

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var existing = await dbContext.Devices
            .FirstOrDefaultAsync(item => item.Id == device.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.Devices.Add(new()
            {
                Id = device.Id,
                Name = device.Name,
                DeviceType = device.DeviceType,
                Status = device.Status,
                LastValue = device.LastValue,
                LastUpdateTime = device.LastUpdateTime,
                PollingIntervalMs = device.PollingIntervalMs
            });
        }
        else
        {
            existing.Name = device.Name;
            existing.DeviceType = device.DeviceType;
            existing.Status = device.Status;
            existing.LastValue = device.LastValue;
            existing.LastUpdateTime = device.LastUpdateTime;
            existing.PollingIntervalMs = device.PollingIntervalMs;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}