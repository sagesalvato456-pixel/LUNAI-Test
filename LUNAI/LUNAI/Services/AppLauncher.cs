using LUNAI.Models;

namespace LUNAI.Services;

public static class AppLauncher
{
    // Injected by Android head — fires startActivity with package name
    public static Func<string, Task<bool>>? PlatformLaunch { get; set; }

    public static async Task<bool> LaunchAsync(AppEntry app)
    {
        if (PlatformLaunch is null)
            throw new InvalidOperationException("PlatformLaunch not wired.");
        return await PlatformLaunch(app.PackageName);
    }
}
