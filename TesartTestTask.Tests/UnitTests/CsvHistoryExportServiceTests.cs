using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Infrastructure.Export;

namespace TesartTestTask.Tests.UnitTests;

public class CsvHistoryExportServiceTests : IDisposable
{
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock = new();

    private readonly Mock<IMeasurementRepository> _measurementRepositoryMock = new();

    private readonly CsvHistoryExportService _service;

    private readonly string _filePath;

    public CsvHistoryExportServiceTests()
    {
        _service = new CsvHistoryExportService(_deviceRepositoryMock.Object, _measurementRepositoryMock.Object);

        _filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    [Fact]
    public async Task ExportDeviceHistoryAsync_ShouldCreateCsvFile()
    {
        var device = CreateDevice();

        _deviceRepositoryMock
            .Setup(x => x.GetByIdAsync(device.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _measurementRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(device.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.ExportDeviceHistoryAsync(device.Id, _filePath, CancellationToken.None);
        File.Exists(_filePath).Should().BeTrue();
    }

    [Fact]
    public async Task ExportDeviceHistoryAsync_ShouldWriteHeader()
    {
        var device = CreateDevice();

        _deviceRepositoryMock
            .Setup(x => x.GetByIdAsync(device.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _measurementRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(device.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _service.ExportDeviceHistoryAsync(device.Id, _filePath, CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(_filePath, TestContext.Current.CancellationToken);
        lines.Should().HaveCount(1);
        lines[0].Should().Be("DeviceId,DeviceName,DeviceType,Timestamp,Value,IsSuccess,ErrorMessage");
    }

    [Fact]
    public async Task ExportDeviceHistoryAsync_ShouldWriteMeasurements()
    {
        var device = CreateDevice();
        var measurements = new[]
        {
            new Measurement
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                Timestamp = DateTime.Parse("2026-01-01"),
                Value = 23.5,
                IsSuccess = true
            }
        };

        _deviceRepositoryMock
            .Setup(x => x.GetByIdAsync(device.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _measurementRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(device.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        await _service.ExportDeviceHistoryAsync(device.Id, _filePath, CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(_filePath, TestContext.Current.CancellationToken);
        lines.Should().HaveCount(2);
        lines[1].Should().Contain(device.Name);
        lines[1].Should().Contain("23.5");
        lines[1].Should().Contain("True");
    }

    [Fact]
    public async Task ExportDeviceHistoryAsync_ShouldThrow_WhenDeviceNotFound()
    {
        var id = Guid.NewGuid();

        _deviceRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        var action = () => _service.ExportDeviceHistoryAsync(id, _filePath, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Device was not found.");
    }

    private static Device CreateDevice() =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Temperature",
            DeviceType = DeviceType.TemperatureSensor,
            Status = DeviceStatus.Online,
            PollingIntervalMs = 1000
        };
}