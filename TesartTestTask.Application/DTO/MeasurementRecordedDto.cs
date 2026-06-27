namespace TesartTestTask.Application.DTO;

public sealed record MeasurementRecordedDto(Guid DeviceId, Guid MeasurementId, double? Value, DateTime Timestamp, bool IsSuccess, string? ErrorMessage);