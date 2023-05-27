namespace LimebrellaSharp.Helpers;

/// <summary>
/// Container for Global Read-Only Variables.
/// </summary>
public static class AppInfo
{
    #region APP INFO

    public static string Name => "Limebrella Sharp";

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    #endregion


    #region OTHER INFO

    public static string OutputFolder => "_OUTPUT";
    
    #endregion
}