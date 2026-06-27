using TesartTestTask.Application.DTO;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Presentation.ViewModels;

public class MeasurementViewModel
{
    public MeasurementViewModel(Measurement measurement)
    {
        Id = measurement.Id;
        DeviceId = measurement.DeviceId;
        Value = measurement.Value;
        Timestamp = measurement.Timestamp;
        IsSuccess = measurement.IsSuccess;
        ErrorMessage = measurement.ErrorMessage;
    }

    public MeasurementViewModel(MeasurementRecordedDto measurement)
    {
        Id = measurement.MeasurementId;
        DeviceId = measurement.DeviceId;
        Value = measurement.Value;
        Timestamp = measurement.Timestamp;
        IsSuccess = measurement.IsSuccess;
        ErrorMessage = measurement.ErrorMessage;
    }

    #region Свойства

    public Guid Id { get; }

    public Guid DeviceId { get; }

    public double? Value { get; }

    public DateTime Timestamp { get; }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    #endregion
}