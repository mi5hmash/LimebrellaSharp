// v2024-12-08 21:16:48

namespace LimebrellaSharpBlazorWASM.Helpers;

/// <summary>
/// Helper class containing static extension methods for Bootstrap.
/// </summary>
public static class BootstrapHelper
{

    public enum Color
    {
        Primary, 
        Secondary,
        Success,
        Warning,
        Danger,
        Info,
        Light,
        Dark
    }

    public static string BootstrapIcon(this string iconName)
    {
        const string iconPack = "bi";
        return $"{iconPack} {iconPack}-{iconName}";
    }

    public static string AsString(this Color bsColor)
        => bsColor.ToString().ToLower();
}