using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Infrastructure.Devices;

namespace TesartTestTask.Tests.UnitTests;

public class VirtualDeviceFactoryTests
{
    private readonly VirtualDeviceFactory _factory = new();

    [Theory]
    [InlineData(DeviceType.TemperatureSensor)]
    [InlineData(DeviceType.PressureSensor)]
    [InlineData(DeviceType.VoltageMeter)]
    [InlineData(DeviceType.CurrentMeter)]
    public void Create_ShouldReturnVirtualMeasurementDevice(DeviceType deviceType)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = "Device",
            DeviceType = deviceType,
            Status = DeviceStatus.Offline,
            PollingIntervalMs = 1000
        };

        var result = _factory.Create(device);

        result.Should().NotBeNull();
        result.Should().BeOfType<VirtualMeasurementDevice>();
        result.DeviceId.Should().Be(device.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenDeviceTypeIsUnknown()
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = "Device",
            DeviceType = (DeviceType)999,
            Status = DeviceStatus.Offline,
            PollingIntervalMs = 1000
        };

        var action = () => _factory.Create(device);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}