namespace TesartTestTask.Presentation.Services;

public class EnumFilterOption<TEnum>(string displayName, TEnum? value) where TEnum : struct, Enum
{
    public string DisplayName { get; } = displayName;

    public TEnum? Value { get; } = value;
}