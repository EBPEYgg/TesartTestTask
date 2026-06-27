using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Application.Interfaces;

public interface IMeasurementRepository
{
    Task<IReadOnlyList<Measurement>> GetByDeviceIdAsync(Guid deviceId, int? recordCount, CancellationToken cancellationToken);
    
    Task ClearAsync(CancellationToken cancellationToken);

    Task SaveMeasurementAsync(Device device, Measurement measurement, CancellationToken cancellationToken);
}
