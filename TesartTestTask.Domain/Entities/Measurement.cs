namespace TesartTestTask.Domain.Entities;

public class Measurement
{
    /// <summary>
    /// Уникальный идентификатор измерения.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор устройства.
    /// </summary>
    public required Guid DeviceId { get; set; }

    /// <summary>
    /// Значение измерения.
    /// </summary>
    public double? Value { get; set; }

    /// <summary>
    /// Время измерения.
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Признак успешного измерения.
    /// </summary>
    public required bool IsSuccess { get; set; }

    /// <summary>
    /// Текст ошибки при неуспешном измерении.
    /// </summary>
    public string? ErrorMessage { get; set; }
}