using System.ComponentModel;

namespace TesartTestTask.Domain.Enums;

public enum PollingState
{
    [Description("Завершен")]
    Stopped,

    [Description("Запущен")]
    Running,

    [Description("Остановлен")]
    Paused,

    [Description("Останавливается")]
    Stopping
}
