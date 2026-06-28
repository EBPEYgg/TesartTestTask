using TesartTestTask.Application.DTO;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Presentation.ViewModels;

namespace TesartTestTask.Tests.UnitTests;

public class MeasurementViewModelTests
{
    [Fact]
    public void Constructor_FromMeasurement_ShouldInitializeProperties()
    {
        // Arrange
        var measurement = new Measurement
        {
            Id = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            Value = 12.5,
            Timestamp = DateTime.Now,
            IsSuccess = true,
            ErrorMessage = null
        };

        // Act
        var viewModel = new MeasurementViewModel(measurement);

        // Assert
        viewModel.Id.Should().Be(measurement.Id);
        viewModel.DeviceId.Should().Be(measurement.DeviceId);
        viewModel.Value.Should().Be(measurement.Value);
        viewModel.Timestamp.Should().Be(measurement.Timestamp);
        viewModel.IsSuccess.Should().BeTrue();
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_FromDto_ShouldInitializeProperties()
    {
        // Arrange
        var dto = new MeasurementRecordedDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            15.3,
            DateTime.Now,
            false,
            "Error");

        // Act
        var viewModel = new MeasurementViewModel(dto);

        // Assert
        viewModel.Id.Should().Be(dto.MeasurementId);
        viewModel.DeviceId.Should().Be(dto.DeviceId);
        viewModel.Value.Should().Be(dto.Value);
        viewModel.Timestamp.Should().Be(dto.Timestamp);
        viewModel.IsSuccess.Should().BeFalse();
        viewModel.ErrorMessage.Should().Be("Error");
    }
}