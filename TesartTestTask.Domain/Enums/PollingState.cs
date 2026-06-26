using System.ComponentModel;

namespace TesartTestTask.Domain.Enums;

public enum PollingState
{
    [Description("Остановлен")]
    Stopped,

    [Description("Запущен")]
    Running,

    [Description("Приостановлен")]
    Paused,

    [Description("Останавливается")]
    Stopping
}
