using System.ComponentModel;

namespace TesartTestTask.Domain.Enums;

public enum DeviceType
{
    [Description("Датчик температуры")]
    TemperatureSensor,

    [Description("Датчик давления")]
    PressureSensor,

    [Description("Вольтметр")]
    VoltageMeter,

    [Description("Амперметр")]
    CurrentMeter
}