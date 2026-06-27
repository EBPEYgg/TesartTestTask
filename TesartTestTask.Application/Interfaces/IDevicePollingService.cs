using TesartTestTask.Application.DTO;
using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Application.Interfaces;

public interface IDevicePollingService
{
    event EventHandler<DeviceUpdatedDto>? DeviceUpdated;

    event EventHandler<MeasurementRecordedDto>? MeasurementRecorded;

    event EventHandler<PollingState>? StateChanged;

    PollingState State { get; }

    Task StartAsync();

    Task PauseAsync();

    Task ResumeAsync();

    Task StopAsync();
}