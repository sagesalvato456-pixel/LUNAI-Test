using Avalonia.Media;

namespace LUNAI.ViewModels;

/// <summary>Finds app-level resources by key (a DynamicResource key can't be data-bound).</summary>
internal static class ResourceLookup
{
    public static T? Find<T>(string key) where T : class
    {
        var app = Avalonia.Application.Current;
        if (app is not null &&
            app.TryGetResource(key, app.ActualThemeVariant, out var res) &&
            res is T typed)
            return typed;

        return null;
    }

    public static IBrush Brush(string key)
        => Find<IBrush>(key) ?? Brushes.MediumPurple;
}
