using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Domain.Entities;

public class Device
{
    /// <summary>
    /// Уникальный идентификатор устройства.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Наименование устройства.
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// Тип устройства.
    /// </summary>
    public required DeviceType DeviceType { get; set; }

    /// <summary>
    /// Текущий статус устройства.
    /// </summary>
    public required DeviceStatus Status { get; set; } = DeviceStatus.Offline;

    /// <summary>
    /// Последнее полученное значение.
    /// </summary>
    public double? LastValue { get; set; }

    /// <summary>
    /// Время последнего обновления.
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// Интервал опроса устройства.
    /// </summary>
    public required int PollingIntervalMs { get; set; }

    /// <summary>
    /// Список измерений.
    /// </summary>
    public ICollection<Measurement> Measurements { get; set; } = [];
}