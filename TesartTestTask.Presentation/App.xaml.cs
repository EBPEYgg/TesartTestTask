using Prism.DryIoc;
using Prism.Ioc;
using System.IO;
using System.Windows;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Application.Services;
using TesartTestTask.Infrastructure.Devices;
using TesartTestTask.Infrastructure.Export;
using TesartTestTask.Infrastructure.Persistence;
using TesartTestTask.Infrastructure.Persistence.Repositories;
using TesartTestTask.Presentation.Interfaces;
using TesartTestTask.Presentation.Services;
using TesartTestTask.Presentation.ViewModels;

namespace TesartTestTask.Presentation;

public partial class App : PrismApplication
{
    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TesartTestTask");
        Directory.CreateDirectory(appDataDirectory);

        var dbPath = Path.Combine(appDataDirectory, "device-monitoring.db");
        var dbContextFactory = new SqliteTesartDbContextFactory($"Data Source={dbPath}");

        containerRegistry.RegisterInstance(dbContextFactory);
        containerRegistry.Register<IDeviceRepository, DeviceRepository>();
        containerRegistry.Register<IMeasurementRepository, MeasurementRepository>();
        containerRegistry.RegisterSingleton<IDeviceFactory, VirtualDeviceFactory>();
        containerRegistry.RegisterSingleton<IDevicePollingService, DevicePollingService>();
        containerRegistry.Register<IHistoryExportService, CsvHistoryExportService>();
        containerRegistry.Register<IFileDialogService, FileDialogService>();
        containerRegistry.Register<IApplicationDataInitializer, DatabaseInitializer>();
        containerRegistry.Register<MainViewModel>();
        containerRegistry.Register<MainWindow>();
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        try
        {
            var initializer = Container.Resolve<IApplicationDataInitializer>();
            await initializer.InitializeAsync(CancellationToken.None);

            if (MainWindow?.DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Application startup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (MainWindow?.DataContext is MainViewModel viewModel)
        {
            await viewModel.ShutdownAsync();
            viewModel.Dispose();
        }

        base.OnExit(e);
    }
}