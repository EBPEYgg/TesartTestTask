namespace TesartTestTask.Application.Interfaces;

public interface IHistoryExportService
{
    Task ExportDeviceHistoryAsync(Guid deviceId, string filePath, CancellationToken cancellationToken);
}