using TesartTestTask.Application.Interfaces;
using TesartTestTask.Domain.Devices;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Infrastructure.Devices;

public class VirtualDeviceFactory : IDeviceFactory
{
    public IVirtualDevice Create(Device device) =>
        device.DeviceType switch
        {
            DeviceType.TemperatureSensor => new VirtualMeasurementDevice(device.Id, -20, 120, 2),
            DeviceType.PressureSensor => new VirtualMeasurementDevice(device.Id, 0.5, 16, 3),
            DeviceType.VoltageMeter => new VirtualMeasurementDevice(device.Id, 0, 240, 2),
            DeviceType.CurrentMeter => new VirtualMeasurementDevice(device.Id, 0, 50, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(device), device.DeviceType, "Не поддерживаемый тип устройства.")
        };
}