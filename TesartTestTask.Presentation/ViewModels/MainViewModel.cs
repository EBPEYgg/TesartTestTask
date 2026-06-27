using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using TesartTestTask.Application.DTO;
using TesartTestTask.Application.Interfaces;
using TesartTestTask.Application.Repositories;
using TesartTestTask.Domain.Entities;
using TesartTestTask.Domain.Enums;
using TesartTestTask.Presentation.Interfaces;
using TesartTestTask.Presentation.Services;

namespace TesartTestTask.Presentation.ViewModels;

public class MainViewModel : BindableBase, IDisposable, IAsyncCloseHandler
{
    #region Поля

    private const int HistoryDisplayLimit = 100;

    private readonly IDeviceRepository _deviceRepository;

    private readonly IMeasurementRepository _measurementRepository;

    private readonly IDevicePollingService _pollingService;

    private readonly IHistoryExportService _historyExportService;

    private readonly IFileDialogService _fileDialogService;

    private DeviceViewModel? _selectedDevice;

    private EnumFilterOption<DeviceType>? _selectedDeviceTypeFilter;

    private EnumFilterOption<DeviceStatus>? _selectedStatusFilter;

    private string _searchText = string.Empty;

    private PollingState _pollingState = PollingState.Stopped;

    private bool _isBusy;

    private bool _isHistoryLoading;

    private string _statusMessage = "Ожидание.";

    private int _historyLoadVersion;

    #endregion

    public MainViewModel(IDeviceRepository deviceRepository,
                         IMeasurementRepository measurementRepository,
                         IDevicePollingService pollingService,
                         IHistoryExportService historyExportService,
                         IFileDialogService fileDialogService)
    {
        _deviceRepository = deviceRepository;
        _measurementRepository = measurementRepository;
        _pollingService = pollingService;
        _historyExportService = historyExportService;
        _fileDialogService = fileDialogService;

        DeviceTypeFilters = new ObservableCollection<EnumFilterOption<DeviceType>>(
            BuildFilterOptions<DeviceType>("Все типы"));
        StatusFilters = new ObservableCollection<EnumFilterOption<DeviceStatus>>(
            BuildFilterOptions<DeviceStatus>("Все статусы"));
        _selectedDeviceTypeFilter = DeviceTypeFilters[0];
        _selectedStatusFilter = StatusFilters[0];

        DeviceView = CollectionViewSource.GetDefaultView(Devices);
        DeviceView.Filter = FilterDevice;
        InitCommands();

        SubscribeToPollingEvents();
    }

    #region Свойства

    public ObservableCollection<DeviceViewModel> Devices { get; } = [];

    public ObservableCollection<MeasurementViewModel> Measurements { get; } = [];

    public ObservableCollection<EnumFilterOption<DeviceType>> DeviceTypeFilters { get; }

    public ObservableCollection<EnumFilterOption<DeviceStatus>> StatusFilters { get; }

    public ICollectionView DeviceView { get; }

    public DeviceViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                _ = LoadSelectedHistoryAsync();
                RaiseCommandStates();
            }
        }
    }

    public EnumFilterOption<DeviceType>? SelectedDeviceTypeFilter
    {
        get => _selectedDeviceTypeFilter;
        set
        {
            if (SetProperty(ref _selectedDeviceTypeFilter, value))
            {
                DeviceView.Refresh();
            }
        }
    }

    public EnumFilterOption<DeviceStatus>? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                DeviceView.Refresh();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public PollingState PollingState
    {
        get => _pollingState;
        private set
        {
            if (SetProperty(ref _pollingState, value))
            {
                RaisePropertyChanged(nameof(PollingStateText));
                RaiseCommandStates();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IsHistoryLoading
    {
        get => _isHistoryLoading;
        private set => SetProperty(ref _isHistoryLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private bool CanStart =>
        !IsBusy && PollingState == PollingState.Stopped && Devices.Count > 0;

    private bool CanStop =>
        !IsBusy && (PollingState == PollingState.Running || PollingState == PollingState.Paused);

    private bool CanPause =>
        !IsBusy && PollingState == PollingState.Running;

    private bool CanResume =>
        !IsBusy && PollingState == PollingState.Paused;

    private bool CanClearHistory =>
        !IsBusy;

    private bool CanExport =>
        !IsBusy && SelectedDevice is not null;

    public string PollingStateText => PollingState.GetDescription().ToLowerInvariant();

    #endregion

    #region Команды

    public DelegateCommand StartCommand { get; private set; } = null!;

    public DelegateCommand StopCommand { get; private set; } = null!;

    public DelegateCommand PauseCommand { get; private set; } = null!;

    public DelegateCommand ResumeCommand { get; private set; } = null!;

    public DelegateCommand ClearHistoryCommand { get; private set; } = null!;

    public DelegateCommand ExportCommand { get; private set; } = null!;

    public DelegateCommand FindCommand { get; private set; } = null!;

    public DelegateCommand ClearSearchTextCommand { get; private set; } = null!;

    #endregion

    #region Методы

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        try
        {
            var devices = await _deviceRepository.GetAllAsync(cancellationToken);
            ReplaceDevices(devices);
            DeviceView.Refresh();
            StatusMessage = $"Загружено устройств: {Devices.Count}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Не удалось загрузить устройства: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    public async Task OnClosingAsync() => await StopPollingAsync();

    public async Task ShutdownAsync() => await StopPollingAsync();

    public void Dispose() => UnsubscribeToPollingEvents();
    
    private void InitCommands()
    {
        StartCommand = new DelegateCommand(async () => await ExecuteCommandAsync(StartPollingAsync), () => CanStart);
        StopCommand = new DelegateCommand(async () => await ExecuteCommandAsync(StopPollingAsync), () => CanStop);
        PauseCommand = new DelegateCommand(async () => await ExecuteCommandAsync(PausePollingAsync), () => CanPause);
        ResumeCommand = new DelegateCommand(async () => await ExecuteCommandAsync(ResumePollingAsync), () => CanResume);
        ClearHistoryCommand = new DelegateCommand(async () => await ExecuteCommandAsync(ClearHistoryAsync), () => CanClearHistory);
        ExportCommand = new DelegateCommand(async () => await ExecuteCommandAsync(ExportHistoryAsync), () => CanExport);
        FindCommand = new DelegateCommand(DeviceView.Refresh);
        ClearSearchTextCommand = new DelegateCommand(ClearSearchText);
    }

    private void ClearSearchText()
    {
        SearchText = string.Empty;
        DeviceView.Refresh();
    }

    private void ReplaceDevices(IEnumerable<Device> devices)
    {
        Devices.Clear();

        foreach (var device in devices)
            Devices.Add(new DeviceViewModel(device));
    }

    private void ReplaceMeasurements(IEnumerable<Measurement> measurements)
    {
        Measurements.Clear();

        foreach (var measurement in measurements)
            Measurements.Add(new MeasurementViewModel(measurement));
    }

    private async Task StartPollingAsync()
    {
        await _pollingService.StartAsync();
        StatusMessage = "Опрос начался.";
    }

    private async Task ExecuteCommandAsync(Func<Task> command)
    {
        try
        {
            await command();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Команда завершилась с ошибкой: {ex.Message}";
        }
        finally
        {
            RaiseCommandStates();
        }
    }

    private async Task StopPollingAsync()
    {
        await _pollingService.StopAsync();
        StatusMessage = "Опрос завершен.";
    }

    private async Task PausePollingAsync()
    {
        await _pollingService.PauseAsync();
        StatusMessage = "Опрос остановлен.";
    }

    private async Task ResumePollingAsync()
    {
        await _pollingService.ResumeAsync();
        StatusMessage = "Опрос возобновлен.";
    }

    private async Task ClearHistoryAsync()
    {
        IsBusy = true;

        try
        {
            await _measurementRepository.ClearAsync(CancellationToken.None);
            Measurements.Clear();
            StatusMessage = "История измерений очищена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Не удалось очистить историю измерений: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task ExportHistoryAsync()
    {
        if (SelectedDevice is null)
            return;

        var filePath = _fileDialogService.ShowSaveCsvDialog(BuildExportFileName(SelectedDevice));
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        IsBusy = true;

        try
        {
            await _historyExportService.ExportDeviceHistoryAsync(
                SelectedDevice.Id,
                filePath,
                CancellationToken.None);
            StatusMessage = $"История измерений экспортирована в файл {filePath}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Экспорт завершился с ошибкой: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task LoadSelectedHistoryAsync()
    {
        var version = Interlocked.Increment(ref _historyLoadVersion);
        var selectedDevice = SelectedDevice;

        if (selectedDevice is null)
        {
            Measurements.Clear();
            RaiseCommandStates();
            return;
        }

        IsHistoryLoading = true;

        try
        {
            var measurements = await _measurementRepository
                .GetByDeviceIdAsync(selectedDevice.Id, HistoryDisplayLimit, CancellationToken.None);

            if (version != _historyLoadVersion)
                return;

            ReplaceMeasurements(measurements);
            StatusMessage = $"Загружено измерений: {Measurements.Count}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Не удалось загрузить измерения: {ex.Message}";
        }
        finally
        {
            if (version == _historyLoadVersion)
            {
                IsHistoryLoading = false;
                RaiseCommandStates();
            }
        }
    }

    private void OnDeviceUpdated(object? sender, DeviceUpdatedDto device)
    {
        RunOnUiThread(() =>
        {
            var viewModel = Devices.FirstOrDefault(item => item.Id == device.DeviceId);
            viewModel?.UpdateState(device.Status, device.LastValue, device.LastUpdateTime);
            DeviceView.Refresh();
        });
    }

    private void OnMeasurementRecorded(object? sender, MeasurementRecordedDto measurement)
    {
        RunOnUiThread(() =>
        {
            if (SelectedDevice?.Id == measurement.DeviceId)
            {
                Measurements.Insert(0, new MeasurementViewModel(measurement));

                while (Measurements.Count > HistoryDisplayLimit)
                    Measurements.RemoveAt(Measurements.Count - 1);
            }

            if (!measurement.IsSuccess && !string.IsNullOrWhiteSpace(measurement.ErrorMessage))
                StatusMessage = measurement.ErrorMessage;

            RaiseCommandStates();
        });
    }

    private void OnPollingStateChanged(object? sender, PollingState state)
    {
        RunOnUiThread(() => PollingState = state);
    }

    private bool FilterDevice(object item)
    {
        if (item is not DeviceViewModel device)
            return false;

        if (SelectedDeviceTypeFilter?.Value is { } deviceType && device.DeviceType != deviceType)
            return false;

        if (SelectedStatusFilter?.Value is { } status && device.Status != status)
            return false;

        return string.IsNullOrWhiteSpace(SearchText) || device.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<EnumFilterOption<TEnum>> BuildFilterOptions<TEnum>(string allText) where TEnum : struct, Enum
    {
        yield return new EnumFilterOption<TEnum>(allText, null);

        foreach (var value in Enum.GetValues<TEnum>())
            yield return new EnumFilterOption<TEnum>(value.GetDescription(), value);
    }

    private static string BuildExportFileName(DeviceViewModel device)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(device.Name
            .Select(character => invalidChars.Contains(character) ? '_' : character)
            .ToArray());

        return $"{safeName}_history.csv";
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;

        if (dispatcher?.CheckAccess() is not false)
        {
            action();
            return;
        }

        _ = dispatcher.InvokeAsync(action);
    }

    private void RaiseCommandStates()
    {
        StartCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
        ResumeCommand.RaiseCanExecuteChanged();
        ClearHistoryCommand.RaiseCanExecuteChanged();
        ExportCommand.RaiseCanExecuteChanged();
    }

    private void SubscribeToPollingEvents()
    {
        _pollingService.DeviceUpdated += OnDeviceUpdated;
        _pollingService.MeasurementRecorded += OnMeasurementRecorded;
        _pollingService.StateChanged += OnPollingStateChanged;
    }

    private void UnsubscribeToPollingEvents()
    {
        _pollingService.DeviceUpdated -= OnDeviceUpdated;
        _pollingService.MeasurementRecorded -= OnMeasurementRecorded;
        _pollingService.StateChanged -= OnPollingStateChanged;
    }

    #endregion
}