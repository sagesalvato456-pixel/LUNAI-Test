using LUNAI.Models;

namespace LUNAI.Services;

public static class AppRegistry
{
    public static IReadOnlyList<AppEntry> CreateDefault() =>
    [
        new AppEntry(
            Id:            AppId.OpenUtauMobile,
            DisplayName:   "OpenUTAU Mobile",
            Ecosystem:     "UTAU / OpenUtau",
            Description:   "Open-source UTAU engine for Android.",
            AccentKey:     "App.OpenUtau.AccentBrush",
            PackageName:   "com.openutau.mobile",
            IconAsset:     "avares://LUNAI/Assets/Icons/icon_openutau.svg",
            DownloadUrl:   null,          // FILL: GitHub releases URL when ready
            LatestVersion: null
        ),
        new AppEntry(
            Id:            AppId.OpenLoidFlame,
            DisplayName:   "OpenLoid Flame",
            Ecosystem:     "VOCALOID",
            Description:   "VOCALOID V6 / V7 engine for Android.",
            AccentKey:     "App.OpenLoid.AccentBrush",
            PackageName:   "com.openloid.flame",
            IconAsset:     "avares://LUNAI/Assets/Icons/icon_openloid.svg",
            DownloadUrl:   null,          // FILL: GitHub releases URL when ready
            LatestVersion: null
        ),
        new AppEntry(
            Id:            AppId.OpenSVFlat,
            DisplayName:   "OpenSV Flat",
            Ecosystem:     "Synthesizer V",
            Description:   "OpenSV+ neural synthesis engine for Android.",
            AccentKey:     "App.OpenSV.AccentBrush",
            PackageName:   "com.opensv.flat",
            IconAsset:     "avares://LUNAI/Assets/Icons/icon_opensv.svg",
            DownloadUrl:   null,          // FILL: GitHub releases URL when ready
            LatestVersion: null
        ),
        new AppEntry(
            Id:            AppId.OpenSonaAether,
            DisplayName:   "OpenSona Aether",
            Ecosystem:     "VoiSona",
            Description:   "Coming soon.",
            AccentKey:     "App.OpenSona.AccentBrush",
            PackageName:   "com.opensona.aether",
            IconAsset:     "avares://LUNAI/Assets/Icons/icon_opensona.svg",
            DownloadUrl:   null,
            LatestVersion: null
        ),
    ];
}
