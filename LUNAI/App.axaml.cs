using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LUNAI.Services;
using LUNAI.ViewModels;
using LUNAI.Views;

namespace LUNAI;

public class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var registry = AppRegistry.CreateDefault();
        var store     = new InstallStateStore();
        var installer = new AppInstaller(store);
        var vm        = new MainViewModel(registry, store, installer);

        if (ApplicationLifetime is ISingleViewApplicationLifetime sv)
            sv.MainView = new MainView { DataContext = vm };

        base.OnFrameworkInitializationCompleted();
    }
}
