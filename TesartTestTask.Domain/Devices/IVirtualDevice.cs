using TesartTestTask.Domain.Results;

namespace TesartTestTask.Domain.Devices;

public interface IVirtualDevice
{
    Guid DeviceId { get; }

    Task<MeasurementResult> ReadAsync(CancellationToken cancellationToken);
}
