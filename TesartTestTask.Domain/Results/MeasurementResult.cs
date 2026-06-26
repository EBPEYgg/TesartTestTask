namespace TesartTestTask.Domain.Results;

public sealed record MeasurementResult(Guid DeviceId, double? Value, DateTime Timestamp, bool IsSuccess, string? ErrorMessage)
{
    public static MeasurementResult Success(Guid deviceId, double value, DateTime timestamp) =>
        new(deviceId, value, timestamp, true, null);

    public static MeasurementResult Failure(Guid deviceId, DateTime timestamp, string errorMessage) =>
        new(deviceId, null, timestamp, false, errorMessage);
}