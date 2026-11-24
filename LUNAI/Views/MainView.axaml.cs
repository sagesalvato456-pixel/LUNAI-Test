using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LUNAI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
