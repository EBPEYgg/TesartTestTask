using TesartTestTask.Domain.Devices;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Application.Interfaces;

public interface IDeviceFactory
{
    IVirtualDevice Create(Device device);
}
