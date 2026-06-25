namespace TesartTestTask.Domain.Results;

public sealed record MeasurementResult(Guid DeviceId, double? Value, DateTime Timestamp, bool IsSuccess, string? ErrorMessage) { }