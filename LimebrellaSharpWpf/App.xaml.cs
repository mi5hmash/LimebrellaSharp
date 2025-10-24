using Mi5hmasH.WpfHelper;
using System.Windows;
using System.Windows.Media;

namespace LimebrellaSharpWpf;
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Set the theme accent
        var colorAccent = new ColorAccentModel(
            Color.FromRgb(100, 130, 0),
            Color.FromRgb(134, 168, 0),
            Color.FromRgb(220, 255, 14),
            Color.FromRgb(234, 255, 71),
            Color.FromRgb(134, 168, 0),
            Color.FromRgb(79, 105, 0),
            Color.FromRgb(23, 36, 0));
        WpfThemeAccent.SetThemeAccent(colorAccent);
    }
}