using System.Globalization;
using System.Text;
using TesartTestTask.Application.Interfaces;

namespace TesartTestTask.Infrastructure.Export;

public class CsvHistoryExportService : IHistoryExportService
{
    private readonly IDeviceRepository _deviceRepository;

    private readonly IMeasurementRepository _measurementRepository;

    public CsvHistoryExportService(IDeviceRepository deviceRepository,
                                   IMeasurementRepository measurementRepository)
    {
        _deviceRepository = deviceRepository;
        _measurementRepository = measurementRepository;
    }

    public async Task ExportDeviceHistoryAsync(Guid deviceId, string filePath, CancellationToken cancellationToken)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId, cancellationToken)
                     ?? throw new InvalidOperationException("Device was not found.");
        var measurements = await _measurementRepository.GetByDeviceIdAsync(deviceId, recordCount: null, cancellationToken);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var writer = new StreamWriter(filePath,
                                                  append: false,
                                                  new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await writer.WriteLineAsync("DeviceId,DeviceName,DeviceType,Timestamp,Value,IsSuccess,ErrorMessage".AsMemory(),
                                    cancellationToken);

        foreach (var measurement in measurements.OrderBy(item => item.Timestamp))
        {
            var line = string.Join(
                ',',
                Escape(device.Id.ToString()),
                Escape(device.Name),
                Escape(device.DeviceType.ToString()),
                Escape(measurement.Timestamp.ToString("O", CultureInfo.InvariantCulture)),
                Escape(measurement.Value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                Escape(measurement.IsSuccess ? "True" : "False"),
                Escape(measurement.ErrorMessage ?? string.Empty));

            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
            return value;

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}