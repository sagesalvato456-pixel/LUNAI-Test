using System.Collections.ObjectModel;
using System.Reactive;
using LUNAI.Models;
using LUNAI.Services;
using ReactiveUI;
using static LUNAI.Services.AppLauncher;

namespace LUNAI.ViewModels;

public sealed class AppCardViewModel : ReactiveObject
{
    private readonly InstallStateStore _store;

    public AppEntry Entry { get; }

    private InstallState _state;
    public  InstallState  State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    public string InstalledVersion
        => _store.Get(Entry.Id)?.Version ?? "—";

    public string LatestVersion
        => Entry.LatestVersion ?? "—";

    public bool IsComingSoon   => Entry.DownloadUrl is null && State == InstallState.ComingSoon;
    public bool CanInstall     => State == InstallState.NotInstalled;
    public bool CanUpdate      => State == InstallState.UpdateAvailable;
    public bool CanLaunch      => State == InstallState.Installed;

    public ReactiveCommand<Unit, Unit> PrimaryActionCommand { get; }

    public event Action<AppCardViewModel>? InstallRequested;
    public event Action<AppCardViewModel>? LaunchRequested;

    public AppCardViewModel(AppEntry entry, InstallStateStore store)
    {
        Entry  = entry;
        _store = store;

        // Determine initial state
        _state = entry.DownloadUrl is null
            ? InstallState.ComingSoon
            : store.Get(entry.Id) is not null
                ? InstallState.Installed
                : InstallState.NotInstalled;

        PrimaryActionCommand = ReactiveCommand.Create(() =>
        {
            if (CanInstall || CanUpdate) InstallRequested?.Invoke(this);
            else if (CanLaunch)         LaunchRequested?.Invoke(this);
        });
    }
}

public sealed class MainViewModel : ReactiveObject
{
    private readonly AppInstaller      _installer;
    private readonly InstallStateStore _store;

    public ObservableCollection<AppCardViewModel> Cards { get; } = new();

    // null = home; non-null = installer wizard is open
    private InstallerViewModel? _activeInstaller;
    public  InstallerViewModel?  ActiveInstaller
    {
        get => _activeInstaller;
        private set => this.RaiseAndSetIfChanged(ref _activeInstaller, value);
    }

    public bool InstallerOpen => ActiveInstaller is not null;

    public MainViewModel(
        IReadOnlyList<AppEntry> registry,
        InstallStateStore       store,
        AppInstaller            installer)
    {
        _store     = store;
        _installer = installer;

        foreach (var entry in registry)
        {
            var card = new AppCardViewModel(entry, store);
            card.InstallRequested += OnInstallRequested;
            card.LaunchRequested  += OnLaunchRequested;
            Cards.Add(card);
        }

        this.WhenAnyValue(x => x.ActiveInstaller)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(InstallerOpen)));
    }

    private void OnInstallRequested(AppCardViewModel card)
    {
        var vm = new InstallerViewModel(card.Entry, _installer);
        vm.CloseRequested   += () => ActiveInstaller = null;
        vm.LaunchRequested  += entry => LaunchApp(entry);
        ActiveInstaller = vm;
    }

    private void OnLaunchRequested(AppCardViewModel card)
        => LaunchApp(card.Entry);

    private void LaunchApp(AppEntry entry)
    {
        _ = AppLauncher.LaunchAsync(entry);
    }
}
