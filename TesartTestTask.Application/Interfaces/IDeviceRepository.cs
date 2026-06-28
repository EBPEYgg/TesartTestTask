using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Application.Interfaces;

public interface IDeviceRepository
{
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken);

    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<Device> devices, CancellationToken cancellationToken);
}
