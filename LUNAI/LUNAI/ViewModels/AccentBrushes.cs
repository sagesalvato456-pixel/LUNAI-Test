using Avalonia;
using Avalonia.Media;

namespace LUNAI.ViewModels;

/// <summary>Resolves an accent brush by resource key (DynamicResource can't take a bound key).</summary>
internal static class AccentBrushes
{
    public static IBrush Resolve(string key)
    {
        var app = Application.Current;
        if (app is not null &&
            app.TryGetResource(key, app.ActualThemeVariant, out var res) &&
            res is IBrush brush)
            return brush;

        return Brushes.MediumPurple;
    }
}
