using TesartTestTask.Domain.Devices;
using TesartTestTask.Domain.Results;

namespace TesartTestTask.Infrastructure.Devices;

public class VirtualMeasurementDevice(Guid deviceId, double minValue, double maxValue, int roundDigits) : IVirtualDevice
{
    private bool _isOffline;

    private readonly double _minValue = minValue;

    private readonly double _maxValue = maxValue;

    private readonly int _roundDigits = roundDigits;

    public Guid DeviceId { get; } = deviceId;

    public async Task<MeasurementResult> ReadAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Random.Shared.Next(100, 450), cancellationToken);
        var timestamp = DateTime.Now;

        if (_isOffline)
        {
            if (Random.Shared.NextDouble() < 0.35)
                _isOffline = false;

            else
                return MeasurementResult.Failure(DeviceId, timestamp, "Устройство неактивно.");
        }

        if (Random.Shared.NextDouble() < 0.04)
        {
            _isOffline = true;
            return MeasurementResult.Failure(DeviceId, timestamp, "Устройство стало неактивным.");
        }

        if (Random.Shared.NextDouble() < 0.08)
        {
            return MeasurementResult.Failure(DeviceId, timestamp, "Ошибка чтения измерения.");
        }

        var value = _minValue + Random.Shared.NextDouble() * (_maxValue - _minValue);
        return MeasurementResult.Success(DeviceId, Math.Round(value, _roundDigits), timestamp);
    }
}