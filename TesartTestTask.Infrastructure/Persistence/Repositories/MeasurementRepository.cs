using Microsoft.EntityFrameworkCore;
using TesartTestTask.Application.Repositories;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence.Repositories;

public sealed class MeasurementRepository(SqliteTesartDbContextFactory dbContextFactory) : IMeasurementRepository
{
    private readonly SqliteTesartDbContextFactory _dbContextFactory = dbContextFactory;

    public async Task AddAsync(Measurement measurement, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        dbContext.Measurements.Add(new()
            {
                Id = measurement.Id,
                DeviceId = measurement.DeviceId,
                Value = measurement.Value,
                Timestamp = measurement.Timestamp,
                IsSuccess = measurement.IsSuccess,
                ErrorMessage = measurement.ErrorMessage
            });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

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
}