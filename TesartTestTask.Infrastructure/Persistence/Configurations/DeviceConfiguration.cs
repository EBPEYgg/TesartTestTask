using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesartTestTask.Domain.Entities;

namespace TesartTestTask.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.Property(device => device.Name)
               .HasMaxLength(160);

        builder.Property(device => device.DeviceType)
               .HasConversion<string>()
               .HasMaxLength(64);

        builder.Property(device => device.Status)
               .HasConversion<string>()
               .HasMaxLength(32);

        builder.HasMany(device => device.Measurements)
               .WithOne()
               .HasForeignKey(measurement => measurement.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(device => device.Name);
    }
}