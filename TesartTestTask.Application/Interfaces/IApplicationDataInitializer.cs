namespace TesartTestTask.Application.Interfaces;

public interface IApplicationDataInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}