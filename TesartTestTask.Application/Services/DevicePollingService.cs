using TesartTestTask.Application.DTO;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Devices;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Application.Services;

public class DevicePollingService : IDevicePollingService
{
    #region Поля

    private readonly IDeviceFactory _deviceFactory;

    private readonly IDeviceRepository _deviceRepository;

    private readonly IMeasurementRepository _measurementRepository;

    private readonly AsyncManualResetEvent _pauseGate = new(true);

    private readonly object _syncRoot = new();

    private CancellationTokenSource? _pollingCancellation;

    private List<Task> _pollingTasks = [];

    private PollingState _state = PollingState.Stopped;

    #endregion

    #region События

    public event EventHandler<DeviceUpdatedDto>? DeviceUpdated;

    public event EventHandler<MeasurementRecordedDto>? MeasurementRecorded;

    public event EventHandler<PollingState>? StateChanged;

    #endregion

    public DevicePollingService(IDeviceFactory deviceFactory,
                                IDeviceRepository deviceRepository,
                                IMeasurementRepository measurementRepository)
    {
        _deviceFactory = deviceFactory;
        _deviceRepository = deviceRepository;
        _measurementRepository = measurementRepository;
    }

    #region Свойства

    public PollingState State
    {
        get
        {
            lock (_syncRoot)
                return _state;
        }
    }

    #endregion

    #region Методы

    public async Task StartAsync()
    {
        lock (_syncRoot)
        {
            if (_state != PollingState.Stopped)
                return;

            _pollingCancellation = new CancellationTokenSource();
            _pauseGate.Set();
        }

        var devices = await _deviceRepository.GetAllAsync(_pollingCancellation.Token);

        if (devices.Count == 0)
        {
            SetState(PollingState.Stopped);
            return;
        }

        lock (_syncRoot)
            _pollingTasks = devices
                .Select(device => PollDeviceAsync(device, _pollingCancellation.Token))
                .ToList();

        SetState(PollingState.Running);
    }

    public Task PauseAsync()
    {
        if (State != PollingState.Running)
            return Task.CompletedTask;

        _pauseGate.Reset();
        SetState(PollingState.Paused);
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        if (State != PollingState.Paused)
            return Task.CompletedTask;

        _pauseGate.Set();
        SetState(PollingState.Running);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        CancellationTokenSource? pollingCancellation;
        Task[] tasks;

        lock (_syncRoot)
        {
            if (_state == PollingState.Stopped)
                return;

            pollingCancellation = _pollingCancellation;
            tasks = _pollingTasks.ToArray();
        }

        SetState(PollingState.Stopping);
        _pauseGate.Set();
        pollingCancellation?.Cancel();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            pollingCancellation?.Dispose();

            lock (_syncRoot)
            {
                _pollingCancellation = null;
                _pollingTasks = [];
            }

            SetState(PollingState.Stopped);
        }
    }

    private async Task PollDeviceAsync(Device device, CancellationToken cancellationToken)
    {
        var virtualDevice = _deviceFactory.Create(device);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _pauseGate.WaitAsync(cancellationToken);
                var measurement = await ReadMeasurementAsync(device, virtualDevice, cancellationToken);
                await SaveMeasurementAsync(device, measurement, cancellationToken);

                DeviceUpdated?.Invoke(this, new DeviceUpdatedDto(device.Id, device.Status, device.LastValue, device.LastUpdateTime));
                MeasurementRecorded?.Invoke(this, new MeasurementRecordedDto(measurement.DeviceId, measurement.Id, measurement.Value, 
                    measurement.Timestamp, measurement.IsSuccess, measurement.ErrorMessage));

                await Task.Delay(device.PollingIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                var measurement = BuildFailureMeasurement(device, $"Ошибка опроса: {ex.Message}");
                ApplyFailure(device, measurement);

                try
                {
                    await SaveMeasurementAsync(device, measurement, cancellationToken);
                }
                catch
                {
                }

                DeviceUpdated?.Invoke(this, new DeviceUpdatedDto(device.Id, device.Status, device.LastValue, device.LastUpdateTime));
                MeasurementRecorded?.Invoke(this, new MeasurementRecordedDto(measurement.DeviceId, measurement.Id, measurement.Value, 
                    measurement.Timestamp, measurement.IsSuccess, measurement.ErrorMessage));

                await DelayAfterErrorAsync(device, cancellationToken);
            }
        }
    }

    private async Task<Measurement> ReadMeasurementAsync(Device device,
                                                         IVirtualDevice virtualDevice,
                                                         CancellationToken cancellationToken)
    {
        try
        {
            var result = await virtualDevice.ReadAsync(cancellationToken);
            var measurement = new Measurement
            {
                Id = Guid.NewGuid(),
                DeviceId = result.DeviceId,
                Value = result.Value,
                Timestamp = result.Timestamp,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage
            };

            ApplyResult(device, measurement);
            return measurement;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var measurement = BuildFailureMeasurement(device, $"Ошибка чтения измерений: {ex.Message}");
            ApplyFailure(device, measurement);
            return measurement;
        }
    }

    private async Task SaveMeasurementAsync(Device device,
                                            Measurement measurement,
                                            CancellationToken cancellationToken)
    {
        await _measurementRepository.SaveMeasurementAsync(device, measurement, cancellationToken);
    }

    private static Measurement BuildFailureMeasurement(Device device, string errorMessage) =>
        new()
        {
            Id = Guid.NewGuid(),
            DeviceId = device.Id,
            Timestamp = DateTime.Now,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

    private static void ApplyResult(Device device, Measurement measurement)
    {
        if (measurement.IsSuccess)
        {
            device.Status = DeviceStatus.Online;
            device.LastValue = measurement.Value;
            device.LastUpdateTime = measurement.Timestamp;
            return;
        }

        ApplyFailure(device, measurement);
    }

    private static void ApplyFailure(Device device, Measurement measurement)
    {
        device.Status = measurement.ErrorMessage?.Contains("offline", StringComparison.OrdinalIgnoreCase) == true
            ? DeviceStatus.Offline
            : DeviceStatus.Error;
        device.LastUpdateTime = measurement.Timestamp;
    }

    private static async Task DelayAfterErrorAsync(Device device, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Math.Max(500, device.PollingIntervalMs), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetState(PollingState state)
    {
        var changed = false;

        lock (_syncRoot)
        {
            if (_state != state)
            {
                _state = state;
                changed = true;
            }
        }

        if (changed)
        {
            StateChanged?.Invoke(this, state);
        }
    }

    #endregion
}