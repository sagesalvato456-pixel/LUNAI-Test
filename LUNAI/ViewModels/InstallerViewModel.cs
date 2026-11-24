using System.Reactive;
using System.Reactive.Linq;
using LUNAI.Models;
using LUNAI.Services;
using ReactiveUI;

namespace LUNAI.ViewModels;

public enum WizardStep
{
    Welcome   = 0,
    License   = 1,
    Download  = 2,
    Installing = 3,
    Done      = 4,
    Error     = 5,
}

public sealed class InstallerViewModel : ReactiveObject
{
    private readonly AppEntry      _app;
    private readonly AppInstaller  _installer;
    private          CancellationTokenSource? _cts;

    // ── Step ──────────────────────────────────────────────────────
    private WizardStep _step = WizardStep.Welcome;
    public  WizardStep  Step
    {
        get => _step;
        private set => this.RaiseAndSetIfChanged(ref _step, value);
    }

    // ── License ───────────────────────────────────────────────────
    private bool _licenseAccepted;
    public  bool  LicenseAccepted
    {
        get => _licenseAccepted;
        set => this.RaiseAndSetIfChanged(ref _licenseAccepted, value);
    }

    // ── Download progress ─────────────────────────────────────────
    private double _downloadFraction;
    public  double  DownloadFraction
    {
        get => _downloadFraction;
        private set => this.RaiseAndSetIfChanged(ref _downloadFraction, value);
    }

    private string _downloadLabel = "";
    public  string  DownloadLabel
    {
        get => _downloadLabel;
        private set => this.RaiseAndSetIfChanged(ref _downloadLabel, value);
    }

    // ── Error ─────────────────────────────────────────────────────
    private string _errorMessage = "";
    public  string  ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    // ── Exposed app info ─────────────────────────────────────────
    public string AppName      => _app.DisplayName;
    public string Ecosystem    => _app.Ecosystem;
    public string Description  => _app.Description;
    public string AccentKey    => _app.AccentKey;

    // ── Commands ──────────────────────────────────────────────────
    public ReactiveCommand<Unit, Unit> NextCommand   { get; }
    public ReactiveCommand<Unit, Unit> BackCommand   { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchCommand { get; }

    // Raised when the wizard should close (cancel or done-without-launch)
    public event Action? CloseRequested;
    // Raised when the app should be launched after install
    public event Action<AppEntry>? LaunchRequested;

    public InstallerViewModel(AppEntry app, AppInstaller installer)
    {
        _app       = app;
        _installer = installer;

        var canNext = this.WhenAnyValue(
            x => x.Step,
            x => x.LicenseAccepted,
            (step, accepted) => step switch
            {
                WizardStep.Welcome  => true,
                WizardStep.License  => accepted,
                _                   => false,
            });

        NextCommand   = ReactiveCommand.CreateFromTask(OnNextAsync, canNext);
        BackCommand   = ReactiveCommand.Create(OnBack,
                            this.WhenAnyValue(x => x.Step, s => s == WizardStep.License));
        CancelCommand = ReactiveCommand.Create(OnCancel);
        LaunchCommand = ReactiveCommand.Create(OnLaunch);
    }

    private async Task OnNextAsync()
    {
        switch (Step)
        {
            case WizardStep.Welcome:
                Step = WizardStep.License;
                break;

            case WizardStep.License:
                Step = WizardStep.Download;
                await BeginDownloadAsync();
                break;
        }
    }

    private void OnBack()
    {
        if (Step == WizardStep.License)
            Step = WizardStep.Welcome;
    }

    private void OnCancel()
    {
        _cts?.Cancel();
        CloseRequested?.Invoke();
    }

    private void OnLaunch()
    {
        LaunchRequested?.Invoke(_app);
        CloseRequested?.Invoke();
    }

    private async Task BeginDownloadAsync()
    {
        _cts = new CancellationTokenSource();

        var progress = new Progress<DownloadProgress>(p =>
        {
            DownloadFraction = p.Fraction;
            long mb  = p.BytesReceived / 1024 / 1024;
            long tot = p.TotalBytes   / 1024 / 1024;
            DownloadLabel = tot > 0
                ? $"{mb} MB / {tot} MB  —  {p.SpeedLabel}"
                : $"{mb} MB  —  {p.SpeedLabel}";
        });

        try
        {
            bool ok = await _installer.InstallAsync(_app, progress, _cts.Token);

            if (_cts.Token.IsCancellationRequested) return;

            if (ok)
                Step = WizardStep.Done;
            else
            {
                ErrorMessage = "System installation was cancelled or failed.";
                Step         = WizardStep.Error;
            }
        }
        catch (OperationCanceledException) { /* user cancelled */ }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Step         = WizardStep.Error;
        }
    }
}
