using TesartTestTask.Application.DTO;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Application.Services;
using TesartTestTask.Domain.Devices;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Domain.Results;

namespace TesartTestTask.Tests.UnitTests;

public class DevicePollingServiceTests
{
    private readonly Mock<IDeviceFactory> _deviceFactoryMock = new();

    private readonly Mock<IDeviceRepository> _deviceRepositoryMock = new();

    private readonly Mock<IMeasurementRepository> _measurementRepositoryMock = new();

    private readonly Mock<IVirtualDevice> _virtualDeviceMock = new();

    private readonly DevicePollingService _service;

    private readonly Device _device;

    public DevicePollingServiceTests()
    {
        _service = new DevicePollingService(_deviceFactoryMock.Object, _deviceRepositoryMock.Object, _measurementRepositoryMock.Object);
        _device = new Device
        {
            Id = Guid.NewGuid(),
            Name = "Temperature",
            DeviceType = DeviceType.TemperatureSensor,
            Status = DeviceStatus.Offline,
            PollingIntervalMs = 10
        };
    }

    [Fact]
    public async Task StartAsync_ShouldChangeStateToRunning()
    {
        _deviceRepositoryMock
        .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync([_device]);

        _deviceFactoryMock
            .Setup(x => x.Create(_device))
            .Returns(_virtualDeviceMock.Object);

        _virtualDeviceMock
            .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                return MeasurementResult.Success(_device.Id, 25, DateTime.Now);
            });

        await _service.StartAsync();
        _service.State.Should().Be(PollingState.Running);
        await _service.StopAsync();
    }

    [Fact]
    public async Task StartAsync_WithNoDevices_ShouldRemainStopped()
    {
        _deviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.StartAsync();

        _service.State.Should().Be(PollingState.Stopped);
    }

    [Fact]
    public async Task StopAsync_ShouldChangeStateToStopped()
    {
        PreparePolling();
        await _service.StartAsync();

        await _service.StopAsync();

        _service.State.Should().Be(PollingState.Stopped);
    }

    [Fact]
    public async Task PauseAsync_ShouldChangeStateToPaused()
    {
        PreparePolling();

        await _service.StartAsync();

        await _service.PauseAsync();
        _service.State.Should().Be(PollingState.Paused);
        await _service.StopAsync();
    }

    [Fact]
    public async Task ResumeAsync_ShouldChangeStateToRunning()
    {
        PreparePolling();
        await _service.StartAsync();
        await _service.PauseAsync();

        await _service.ResumeAsync();

        _service.State.Should().Be(PollingState.Running);
        await _service.StopAsync();
    }

    [Fact]
    public async Task Polling_ShouldRaiseDeviceUpdatedEvent()
    {
        PreparePolling();
        DeviceUpdatedDto? dto = null;
        _service.DeviceUpdated += (_, e) => dto = e;

        await _service.StartAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        dto.Should().NotBeNull();
        await _service.StopAsync();
    }

    [Fact]
    public async Task Polling_ShouldRaiseMeasurementRecordedEvent()
    {
        PreparePolling();
        MeasurementRecordedDto? dto = null;
        _service.MeasurementRecorded += (_, e) => dto = e;

        await _service.StartAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        dto.Should().NotBeNull();
        await _service.StopAsync();
    }

    [Fact]
    public async Task Polling_ShouldSaveMeasurement()
    {
        PreparePolling();
        await _service.StartAsync();

        await Task.Delay(100, TestContext.Current.CancellationToken);

        _measurementRepositoryMock.Verify(
            x => x.SaveMeasurementAsync(
                It.IsAny<Device>(),
                It.IsAny<Measurement>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        await _service.StopAsync();
    }

    [Fact]
    public async Task StateChanged_ShouldBeRaised()
    {
        PreparePolling();
        PollingState? state = null;
        _service.StateChanged += (_, e) => state = e;

        await _service.StartAsync();

        state.Should().Be(PollingState.Running);
        await _service.StopAsync();
    }

    private void PreparePolling()
    {
        _deviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([_device]);

        _deviceFactoryMock
            .Setup(x => x.Create(_device))
            .Returns(_virtualDeviceMock.Object);

        _virtualDeviceMock
            .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
                MeasurementResult.Success(
                    _device.Id,
                    25,
                    DateTime.Now));

        _measurementRepositoryMock
            .Setup(x => x.SaveMeasurementAsync(
                It.IsAny<Device>(),
                It.IsAny<Measurement>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}