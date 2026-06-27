using Microsoft.EntityFrameworkCore;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence.Repositories;

public sealed class MeasurementRepository(SqliteTesartDbContextFactory dbContextFactory) : IMeasurementRepository
{
    private readonly SqliteTesartDbContextFactory _dbContextFactory = dbContextFactory;

    public async Task<IReadOnlyList<Measurement>> GetByDeviceIdAsync(Guid deviceId, 
                                                                     int? recordCount, 
                                                                     CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        IQueryable<Measurement> query = dbContext.Measurements
            .AsNoTracking()
            .Where(measurement => measurement.DeviceId == deviceId)
            .OrderByDescending(measurement => measurement.Timestamp);

        if (recordCount.HasValue)
            query = query.Take(recordCount.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Measurements.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveMeasurementAsync(Device device, Measurement measurement, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        dbContext.Attach(device);
        dbContext.Entry(device).Property(d => d.Status).IsModified = true;
        dbContext.Entry(device).Property(d => d.LastValue).IsModified = true;
        dbContext.Entry(device).Property(d => d.LastUpdateTime).IsModified = true;

        await dbContext.Measurements.AddAsync(measurement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}