using LUNAI.Models;

namespace LUNAI.Services;

public sealed class DownloadProgress
{
    public long   BytesReceived { get; init; }
    public long   TotalBytes    { get; init; }
    public double Fraction      => TotalBytes > 0 ? (double)BytesReceived / TotalBytes : 0;
    public string SpeedLabel    { get; init; } = "";
}

public sealed class AppInstaller
{
    private readonly InstallStateStore _store;

    // Injected by the Android head project so Core stays platform-agnostic.
    // Android impl fires the ACTION_INSTALL_PACKAGE intent.
    public static Func<string, Task<bool>>? PlatformInstallApk { get; set; }

    public AppInstaller(InstallStateStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Full pipeline: download → verify → trigger system install.
    /// Reports progress via <paramref name="onProgress"/>.
    /// Returns true on success.
    /// </summary>
    public async Task<bool> InstallAsync(
        AppEntry               app,
        IProgress<DownloadProgress> onProgress,
        CancellationToken      ct)
    {
        if (app.DownloadUrl is null) return false;

        string cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LUNAI", "cache");
        Directory.CreateDirectory(cacheDir);

        string apkPath = Path.Combine(cacheDir, $"{app.Id}.apk");

        // ── Download ─────────────────────────────────────────────
        using var http   = new HttpClient();
        using var resp   = await http.GetAsync(app.DownloadUrl,
                               HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        long total    = resp.Content.Headers.ContentLength ?? -1;
        long received = 0;
        var  sw       = System.Diagnostics.Stopwatch.StartNew();

        await using (var src  = await resp.Content.ReadAsStreamAsync(ct))
        await using (var dst  = File.Create(apkPath))
        {
            var buf = new byte[81920];
            int read;
            while ((read = await src.ReadAsync(buf, ct)) > 0)
            {
                await dst.WriteAsync(buf.AsMemory(0, read), ct);
                received += read;

                double mbps = received / 1024.0 / 1024.0 / sw.Elapsed.TotalSeconds;
                onProgress.Report(new DownloadProgress
                {
                    BytesReceived = received,
                    TotalBytes    = total,
                    SpeedLabel    = $"{mbps:F1} MB/s",
                });
            }
        }

        // ── System install intent ─────────────────────────────────
        if (PlatformInstallApk is null)
            throw new InvalidOperationException("PlatformInstallApk not wired — set it in the Android head project.");

        bool ok = await PlatformInstallApk(apkPath);
        if (!ok) return false;

        _store.Set(app.Id, new InstalledAppRecord
        {
            Id          = app.Id.ToString(),
            Version     = app.LatestVersion ?? "unknown",
            ApkPath     = apkPath,
            InstalledAt = DateTime.UtcNow.ToString("O"),
        });

        return true;
    }
}
