using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence.Configurations;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.Property(measurement => measurement.ErrorMessage)
               .HasMaxLength(512);

        builder.HasIndex(measurement => new { measurement.DeviceId, measurement.Timestamp });
    }
}