using Microsoft.Data.Sqlite;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Infrastructure.Persistence;
using TesartTestTask.Infrastructure.Persistence.Repositories;

namespace TesartTestTask.Tests;

public abstract class SqliteRepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected readonly SqliteTesartDbContextFactory Factory;

    protected SqliteRepositoryTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        Factory = new SqliteTesartDbContextFactory(_connection);

        using var context = Factory.CreateDbContext();
        context.Database.EnsureCreated();
    }

    protected DeviceRepository CreateDeviceRepository() => new(Factory);

    protected MeasurementRepository CreateMeasurementRepository() => new(Factory);

    protected static Device CreateDevice(string name = "Device") =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            DeviceType = DeviceType.TemperatureSensor,
            Status = DeviceStatus.Online,
            PollingIntervalMs = 1000
        };

    public void Dispose()
    {
        _connection.Dispose();
    }
}