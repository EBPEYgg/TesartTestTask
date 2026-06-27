using TesartTestTask.Domain.Enums;

namespace TesartTestTask.Application.DTO;

public sealed record DeviceUpdatedDto(Guid DeviceId, DeviceStatus Status, double? LastValue, DateTime? LastUpdateTime);