using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Presentation.Interfaces;
using TesartTestTask.Presentation.ViewModels;

namespace TesartTestTask.Tests.UnitTests;

public class MainViewModelTests
{
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock = new();

    private readonly Mock<IMeasurementRepository> _measurementRepositoryMock = new();

    private readonly Mock<IDevicePollingService> _pollingServiceMock = new();

    private readonly Mock<IHistoryExportService> _exportServiceMock = new();

    private readonly Mock<IFileDialogService> _fileDialogServiceMock = new();

    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _viewModel = new MainViewModel(
            _deviceRepositoryMock.Object,
            _measurementRepositoryMock.Object,
            _pollingServiceMock.Object,
            _exportServiceMock.Object,
            _fileDialogServiceMock.Object);
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadDevices()
    {
        var devices = new[]
        {
        CreateDevice("Device1"),
        CreateDevice("Device2")
    };

        _deviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        await _viewModel.LoadAsync(TestContext.Current.CancellationToken);

        _viewModel.Devices.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_ShouldUpdateStatusMessage()
    {
        _deviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateDevice()]);

        await _viewModel.LoadAsync(TestContext.Current.CancellationToken);

        _viewModel.StatusMessage.Should().Be("Загружено устройств: 1.");
    }

    [Fact]
    public async Task StartCommand_ShouldStartPolling()
    {
        _deviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateDevice()]);

        await _viewModel.LoadAsync(TestContext.Current.CancellationToken);

        _viewModel.StartCommand.Execute();

        _pollingServiceMock.Verify(x => x.StartAsync(), Times.Once);
    }

    [Fact]
    public async Task StopCommand_ShouldStopPolling()
    {
        _viewModel.StopCommand.Execute();

        await Task.Yield();

        _pollingServiceMock.Verify(x => x.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task PauseCommand_ShouldPausePolling()
    {
        _viewModel.PauseCommand.Execute();

        await Task.Yield();

        _pollingServiceMock.Verify(x => x.PauseAsync(), Times.Once);
    }

    [Fact]
    public async Task ResumeCommand_ShouldResumePolling()
    {
        _viewModel.ResumeCommand.Execute();

        await Task.Yield();

        _pollingServiceMock.Verify(x => x.ResumeAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearHistoryCommand_ShouldClearMeasurements()
    {
        _viewModel.Measurements.Add(
            new MeasurementViewModel(
                new Measurement
                {
                    Id = Guid.NewGuid(),
                    DeviceId = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    IsSuccess = true
                }));

        _viewModel.ClearHistoryCommand.Execute();

        await Task.Yield();

        _measurementRepositoryMock.Verify(
            x => x.ClearAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _viewModel.Measurements.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportCommand_ShouldExportHistory()
    {
        var device = new DeviceViewModel(CreateDevice());

        _viewModel.Devices.Add(device);
        _viewModel.SelectedDevice = device;

        _fileDialogServiceMock
            .Setup(x => x.ShowSaveCsvDialog(It.IsAny<string>()))
            .Returns("history.csv");

        _viewModel.ExportCommand.Execute();

        await Task.Yield();

        _exportServiceMock.Verify(
            x => x.ExportDeviceHistoryAsync(
                device.Id,
                "history.csv",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShutdownAsync_ShouldStopPolling()
    {
        await _viewModel.ShutdownAsync();

        _pollingServiceMock.Verify(
            x => x.StopAsync(),
            Times.Once);
    }

    [Fact]
    public async Task OnClosingAsync_ShouldStopPolling()
    {
        await _viewModel.OnClosingAsync();

        _pollingServiceMock.Verify(x => x.StopAsync(), Times.Once);
    }

    private static Device CreateDevice(string name = "Device") =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            DeviceType = DeviceType.TemperatureSensor,
            Status = DeviceStatus.Online,
            PollingIntervalMs = 1000
        };
}