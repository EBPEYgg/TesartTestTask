using Prism.Mvvm;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Presentation.ViewModels;

public class DeviceViewModel : BindableBase
{
    #region Поля

    private DeviceStatus _status;

    private double? _lastValue;

    private DateTime? _lastUpdateTime;

    #endregion

    public DeviceViewModel(Device device)
    {
        Id = device.Id;
        Name = device.Name;
        DeviceType = device.DeviceType;
        PollingIntervalMs = device.PollingIntervalMs;
        UpdateState(device.Status, device.LastValue, device.LastUpdateTime);
    }

    #region Свойства

    public Guid Id { get; }

    public string Name { get; }

    public DeviceType DeviceType { get; }

    public DeviceStatus Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public double? LastValue
    {
        get => _lastValue;
        private set => SetProperty(ref _lastValue, value);
    }

    public DateTime? LastUpdateTime
    {
        get => _lastUpdateTime;
        private set => SetProperty(ref _lastUpdateTime, value);
    }

    public int PollingIntervalMs { get; }

    #endregion Свойства

    #region Методы

    public void UpdateState(DeviceStatus status, double? lastValue, DateTime? lastUpdateTime)
    {
        Status = status;
        LastValue = lastValue;
        LastUpdateTime = lastUpdateTime;
    }

    #endregion
}