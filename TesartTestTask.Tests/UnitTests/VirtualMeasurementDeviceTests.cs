using TesartTestTask.Domain.Results;
using TesartTestTask.Infrastructure.Devices;

namespace TesartTestTask.Tests.UnitTests;

public class VirtualMeasurementDeviceTests
{
    [Fact]
    public void Constructor_ShouldSetDeviceId()
    {
        var id = Guid.NewGuid();

        var device = new VirtualMeasurementDevice(id, 0, 100, 2);

        device.DeviceId.Should().Be(id);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnCorrectDeviceId()
    {
        var id = Guid.NewGuid();

        var device = new VirtualMeasurementDevice(id, 0, 100, 2);

        var result = await device.ReadAsync(CancellationToken.None);

        result.DeviceId.Should().Be(id);
    }

    [Fact]
    public async Task ReadAsync_ShouldGenerateValueInRange()
    {
        var device = new VirtualMeasurementDevice(Guid.NewGuid(), 10, 20, 2);
        MeasurementResult result;

        do
        {
            result = await device.ReadAsync(CancellationToken.None);
        }
        while (!result.IsSuccess);

        result.Value.Should().BeInRange(10, 20);
    }

    [Fact]
    public async Task ReadAsync_ShouldRoundValue()
    {
        var device = new VirtualMeasurementDevice(Guid.NewGuid(), 0, 10, 3);
        MeasurementResult result;

        do
        {
            result = await device.ReadAsync(CancellationToken.None);
        }
        while (!result.IsSuccess);

        result.Value.Should().NotBeNull();
        var value = result.Value!.Value;
        value.Should().Be(Math.Round(value, 3));
    }

    [Fact]
    public async Task ReadAsync_ShouldSetTimestamp()
    {
        var before = DateTime.Now;

        var device = new VirtualMeasurementDevice(Guid.NewGuid(), 0, 10, 2);

        var result = await device.ReadAsync(CancellationToken.None);

        var after = DateTime.Now;

        result.Timestamp.Should().BeOnOrAfter(before);
        result.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task ReadAsync_ShouldThrow_WhenCancelled()
    {
        var device = new VirtualMeasurementDevice(Guid.NewGuid(), 0, 10, 2);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var action = async () => await device.ReadAsync(cts.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }
}