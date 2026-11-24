namespace LUNAI.Models;

public enum AppId
{
    OpenUtauMobile,
    OpenLoidFlame,
    OpenSVFlat,
    OpenSonaAether,
}

public enum InstallState
{
    NotInstalled,
    Downloading,
    Installing,
    Installed,
    UpdateAvailable,
    Updating,
    Error,
    ComingSoon,
}

/// <summary>Static metadata about a managed app. Never mutated at runtime.</summary>
public sealed record AppEntry(
    AppId       Id,
    string      DisplayName,
    string      Ecosystem,
    string      Description,
    string      AccentKey,       // resource key e.g. "App.OpenUtau.AccentBrush"
    string      PackageName,     // android package id used to check installation
    string      IconAsset,       // resource key of a DrawingImage in Themes/LunaiIcons.axaml
    string?     DownloadUrl,     // null = coming soon
    string?     LatestVersion
);
