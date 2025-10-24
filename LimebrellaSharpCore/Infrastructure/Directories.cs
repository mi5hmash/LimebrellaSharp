using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LimebrellaSharpCore.Infrastructure;

/// <summary>
/// Provides utility methods and properties for managing application directory paths, including the root and output directories.
/// </summary>
public static partial class Directories
{
    public static readonly string RootPath = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string Output = Path.Combine(RootPath, "_OUTPUT");
    private static readonly string SaveDataDirectorySuffix = Path.Combine("remote", "win64_save");

    public static string ExtractSteamId(this string filePath)
        => SteamIdFromFilePathRegex().Match(filePath).Groups[1].Value;
    [GeneratedRegex("""[\/\\](\d+)(?:[\/\\](?:\d+))?[\/\\]remote[\/\\]win64_save""")]
    private static partial Regex SteamIdFromFilePathRegex();
    
    /// <summary>
    /// Generates a new output directory path using the current date and time, combined with the specified action name.
    /// </summary>
    /// <param name="action">The name of the action to include in the output directory path.</param>
    /// <returns>A string representing the full path of the new output directory, formatted with the current date, time, and the specified action.</returns>
    public static string GetNewOutputDirectory(string action) 
        => Path.Combine(Output, $"{DateTime.Now:yyyy-MM-dd_HHmmssfff}_{action}");

    /// <summary>
    /// Appends the user identifier and a predefined suffix to the specified output directory path.
    /// </summary>
    /// <param name="outputDirectory">The base directory path to which the user identifier and suffix will be appended.</param>
    /// <param name="userId">The user identifier to include in the resulting path.</param>
    /// <returns>A string representing the full path of the new output directory, including the user ID, and the predefined suffix.</returns>
    public static string AddUserIdAndSuffix(this string outputDirectory, string userId)
        => Path.Combine(outputDirectory, userId, SaveDataDirectorySuffix);

    /// <summary>
    /// Opens the specified directory in the system's default file explorer, if the directory exists.
    /// </summary>
    /// <param name="path">The full path of the directory to open.</param>
    public static void OpenDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        string? openCmd = null;
        string? args = null;

        Directory.CreateDirectory(path);
        if (OperatingSystem.IsWindows())
        {
            openCmd = "explorer.exe";
            args = $"\"{path}\"";
        }
        else if (OperatingSystem.IsMacOS())
        {
            openCmd = "open";
            args = $"\"{path}\"";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            openCmd = "xdg-open";
            args = $"\"{path}\"";
        }

        if (openCmd != null && args != null)
            Process.Start(new ProcessStartInfo
            {
                FileName = openCmd,
                Arguments = args,
                UseShellExecute = false
            });
    }
}