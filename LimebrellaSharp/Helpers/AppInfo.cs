namespace LimebrellaSharp.Helpers;

/// <summary>
/// Container for Global Read-Only Variables.
/// </summary>
public static class AppInfo
{
    #region APP INFO

    public static string Name => "Limebrella Sharp";

    public const string ToolVersion = "1.0.2.0";

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    #endregion


    #region OTHER INFO

    public static string OutputFolder => "_OUTPUT";

    public static string OutputPath => Path.Combine(RootPath, OutputFolder);

    #endregion
}