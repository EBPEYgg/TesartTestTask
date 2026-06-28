namespace TesartTestTask.Tests.IntegrationTests;

public class DeviceRepositoryTests : SqliteRepositoryTestBase
{
    [Fact]
    public async Task AddRangeAsync_ShouldAddDevices()
    {
        var repository = CreateDeviceRepository();
        var devices = new[]
        {
            CreateDevice("B"),
            CreateDevice("A")
        };

        await repository.AddRangeAsync(devices, CancellationToken.None);

        (await repository.CountAsync(CancellationToken.None)).Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnDevicesOrderedByName()
    {
        var repository = CreateDeviceRepository();
        await repository.AddRangeAsync(
        [
            CreateDevice("Device B"),
            CreateDevice("Device A")
        ],
        CancellationToken.None);

        var result = await repository.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Device A");
        result[1].Name.Should().Be("Device B");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDevice()
    {
        var repository = CreateDeviceRepository();
        var device = CreateDevice();
        await repository.AddRangeAsync([device], CancellationToken.None);

        var result = await repository.GetByIdAsync(device.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(device.Id);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnDevicesCount()
    {
        var repository = CreateDeviceRepository();
        await repository.AddRangeAsync(
        [
            CreateDevice(),
            CreateDevice(),
            CreateDevice()
        ],
        CancellationToken.None);

        var count = await repository.CountAsync(CancellationToken.None);

        count.Should().Be(3);
    }
}