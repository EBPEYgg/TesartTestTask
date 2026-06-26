using System.ComponentModel;

namespace TesartTestTask.Domain.Enums;

public enum DeviceStatus
{
    [Description("Активен")]
    Online,

    [Description("Не активен")]
    Offline,

    [Description("Ошибка")]
    Error
}