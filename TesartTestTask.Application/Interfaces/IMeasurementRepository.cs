using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Application.Repositories;

public interface IMeasurementRepository
{
    Task AddAsync(Measurement measurement, CancellationToken cancellationToken);

    Task<IReadOnlyList<Measurement>> GetByDeviceIdAsync(Guid deviceId, int? recordCount, CancellationToken cancellationToken);
    
    Task ClearAsync(CancellationToken cancellationToken);
}
