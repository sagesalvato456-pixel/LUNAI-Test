using System.Globalization;
using Avalonia.Data.Converters;
using LUNAI.Models;

namespace LUNAI.ViewModels;

public sealed class InstallStateToLabelConverter : IValueConverter
{
    public static readonly InstallStateToLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is InstallState s ? s switch
        {
            InstallState.NotInstalled    => "Install",
            InstallState.Downloading     => "Downloading...",
            InstallState.Installing      => "Installing...",
            InstallState.Installed       => "Launch",
            InstallState.UpdateAvailable => "Update",
            InstallState.Updating        => "Updating...",
            InstallState.Error           => "Retry",
            InstallState.ComingSoon      => "Coming Soon",
            _                            => "—",
        } : "—";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
