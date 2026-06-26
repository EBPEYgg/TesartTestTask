using TesartTestTask.Application.Interfaces;
using TesartTestTask.Application.Repositories;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Infrastructure.Persistence;

public sealed class DatabaseInitializer : IApplicationDataInitializer
{
    private readonly SqliteTesartDbContextFactory _dbContextFactory;

    private readonly IDeviceRepository _deviceRepository;

    public DatabaseInitializer(SqliteTesartDbContextFactory dbContextFactory,
                               IDeviceRepository deviceRepository)
    {
        _dbContextFactory = dbContextFactory;
        _deviceRepository = deviceRepository;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _deviceRepository.CountAsync(cancellationToken) > 0)
            return;

        var devices = new[]
        {
            CreateDevice("Pt100", DeviceType.TemperatureSensor, 1_000),
            CreateDevice("Wika S-20", DeviceType.PressureSensor, 1_000),
            CreateDevice("ÎÂÅÍ ÑÂ01-220.È", DeviceType.VoltageMeter, 0_500),
            CreateDevice("Ìåàíäð ÀÌ-2", DeviceType.CurrentMeter, 0_500),
        };

        await _deviceRepository.AddRangeAsync(devices, cancellationToken);
    }

    private static Device CreateDevice(string name, DeviceType deviceType, int pollingIntervalMs) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            DeviceType = deviceType,
            Status = DeviceStatus.Offline,
            PollingIntervalMs = pollingIntervalMs
        };
}