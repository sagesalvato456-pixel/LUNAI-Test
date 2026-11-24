using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using LUNAI.Services;

namespace LUNAI.Android;

[Activity(
    Label = "LUNAI",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@mipmap/ic_launcher",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
                         | ConfigChanges.ScreenSize
                         | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Wire platform services before Avalonia boots.
        AppInstaller.PlatformInstallApk = InstallApkAsync;
        AppLauncher.PlatformLaunch      = LaunchAppAsync;
        base.OnCreate(savedInstanceState);
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
               .WithInterFont()
               .UseSkia()
               .With(new SkiaOptions { MaxGpuResourceSizeBytes = 256 * 1024 * 1024 });

    // ── Android install intent ────────────────────────────────────
    private TaskCompletionSource<bool>? _installTcs;

    private Task<bool> InstallApkAsync(string apkPath)
    {
        _installTcs = new TaskCompletionSource<bool>();

        var file = new Java.IO.File(apkPath);
        var uri  = AndroidX.Core.Content.FileProvider.GetUriForFile(
            this,
            $"{PackageName}.provider",
            file);

        var intent = new Intent(Intent.ActionInstallPackage);
        intent.SetDataAndType(uri, "application/vnd.android.package-archive");
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
        intent.PutExtra(Intent.ExtraReturnResult, true);

        StartActivityForResult(intent, requestCode: 1001);
        return _installTcs.Task;
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == 1001)
            _installTcs?.TrySetResult(resultCode == Result.Ok);
    }

    // ── App launcher ─────────────────────────────────────────────
    private Task<bool> LaunchAppAsync(string packageName)
    {
        try
        {
            var intent = PackageManager!.GetLaunchIntentForPackage(packageName);
            if (intent is null) return Task.FromResult(false);
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
