using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Presentation.ViewModels;

namespace TesartTestTask.Tests.UnitTests;

public class DeviceViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var device = CreateDevice();

        // Act
        var viewModel = new DeviceViewModel(device);

        // Assert
        viewModel.Id.Should().Be(device.Id);
        viewModel.Name.Should().Be(device.Name);
        viewModel.DeviceType.Should().Be(device.DeviceType);
        viewModel.Status.Should().Be(device.Status);
        viewModel.LastValue.Should().Be(device.LastValue);
        viewModel.LastUpdateTime.Should().Be(device.LastUpdateTime);
        viewModel.PollingIntervalMs.Should().Be(device.PollingIntervalMs);
    }

    [Fact]
    public void UpdateState_ShouldUpdateProperties()
    {
        // Arrange
        var viewModel = new DeviceViewModel(CreateDevice());

        var time = DateTime.Now;

        // Act
        viewModel.UpdateState(DeviceStatus.Error, 100.5, time);

        // Assert
        viewModel.Status.Should().Be(DeviceStatus.Error);
        viewModel.LastValue.Should().Be(100.5);
        viewModel.LastUpdateTime.Should().Be(time);
    }

    [Fact]
    public void UpdateState_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new DeviceViewModel(CreateDevice());

        var changed = new List<string>();

        viewModel.PropertyChanged += (_, e) =>
            changed.Add(e.PropertyName!);

        // Act
        viewModel.UpdateState(DeviceStatus.Error, 15, DateTime.Now);

        // Assert
        changed.Should().Contain(nameof(DeviceViewModel.Status));
        changed.Should().Contain(nameof(DeviceViewModel.LastValue));
        changed.Should().Contain(nameof(DeviceViewModel.LastUpdateTime));
    }



    private static Device CreateDevice() =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Temperature",
            DeviceType = DeviceType.TemperatureSensor,
            Status = DeviceStatus.Online,
            LastValue = 24.6,
            LastUpdateTime = new DateTime(2024, 1, 1, 12, 0, 0),
            PollingIntervalMs = 1000
        };
}