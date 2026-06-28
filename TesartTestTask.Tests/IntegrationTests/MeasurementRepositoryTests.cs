using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Tests.IntegrationTests;

public class MeasurementRepositoryTests : SqliteRepositoryTestBase
{
    [Fact]
    public async Task SaveMeasurementAsync_ShouldSaveMeasurement()
    {
        var deviceRepository = CreateDeviceRepository();
        var repository = CreateMeasurementRepository();
        var device = CreateDevice();
        await deviceRepository.AddRangeAsync([device], CancellationToken.None);
        var measurement = CreateMeasurement(device.Id);

        await repository.SaveMeasurementAsync(device, measurement, CancellationToken.None);

        var result = await repository.GetByDeviceIdAsync(device.Id, null, CancellationToken.None);
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByDeviceIdAsync_ShouldReturnOnlyDeviceMeasurements()
    {
        var deviceRepository = CreateDeviceRepository();
        var repository = CreateMeasurementRepository();
        var device1 = CreateDevice();
        var device2 = CreateDevice();
        await deviceRepository.AddRangeAsync([device1, device2], CancellationToken.None);
        await repository.SaveMeasurementAsync(device1, CreateMeasurement(device1.Id), CancellationToken.None);
        await repository.SaveMeasurementAsync(device2, CreateMeasurement(device2.Id), CancellationToken.None);

        var result = await repository.GetByDeviceIdAsync(device1.Id, null, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].DeviceId.Should().Be(device1.Id);
    }

    [Fact]
    public async Task GetByDeviceIdAsync_ShouldLimitRecords()
    {
        var deviceRepository = CreateDeviceRepository();
        var repository = CreateMeasurementRepository();
        var device = CreateDevice();
        await deviceRepository.AddRangeAsync([device], CancellationToken.None);
        for (int i = 0; i < 5; i++)
        {
            await repository.SaveMeasurementAsync(device, CreateMeasurement(device.Id), CancellationToken.None);
        }

        var result = await repository.GetByDeviceIdAsync(device.Id, 2, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ClearAsync_ShouldDeleteAllMeasurements()
    {
        var deviceRepository = CreateDeviceRepository();
        var repository = CreateMeasurementRepository();
        var device = CreateDevice();
        await deviceRepository.AddRangeAsync([device], CancellationToken.None);
        await repository.SaveMeasurementAsync(device, CreateMeasurement(device.Id), CancellationToken.None);

        await repository.ClearAsync(CancellationToken.None);

        var result = await repository.GetByDeviceIdAsync(device.Id, null, CancellationToken.None);
        result.Should().BeEmpty();
    }

    private static Measurement CreateMeasurement(Guid deviceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Value = 25,
            Timestamp = DateTime.UtcNow,
            IsSuccess = true
        };
}