using System.Diagnostics;

namespace LimebrellaSharpCore.Helpers;

public static class IoHelpers
{
    /// <summary>
    /// Tries to safely delete directory under given <paramref name="path"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>True if directory has been successfully deleted.</returns>
    public static bool SafelyDeleteDirectory(string path)
    {
        try { Directory.Delete(path, true); }
        catch { /* ignored */ }
        return !Directory.Exists(path);
    }

    /// <summary>
    /// Tries to safely delete file located under the given <paramref name="path"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>True if file has been successfully deleted.</returns>
    public static bool SafelyDeleteFile(string path)
    {
        try { File.Delete(path); }
        catch { /* ignored */ }
        return !Directory.Exists(path);
    }

    /// <summary>
    /// Tries to safely recreate <paramref name="dir"/> directory.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool RecreateDirectory(string dir)
    {
        if (!SafelyDeleteDirectory(dir)) return false;
        Directory.CreateDirectory(dir);
        return true;
    }

    /// <summary>
    /// Reads file using FileStream.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadFile(string filePath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using StreamReader sr = new(fs);
        return sr.ReadToEnd();
    }
    /// <summary>
    /// Reads file using FileStream (async variant).
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<string> ReadFileAsync(string filePath)
    {
        await using FileStream fs = File.OpenRead(filePath);
        using StreamReader sr = new(fs);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Writes file using FileStream.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileContent"></param>
    public static void WriteFile(string filePath, string fileContent)
    {
        using FileStream fs = File.OpenWrite(filePath);
        using StreamWriter sw = new(fs);
        sw.Write(fileContent);
    }
    /// <summary>
    /// Writes file using FileStream (async variant).
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileContent"></param>
    public static async Task WriteFileAsync(string filePath, string fileContent)
    {
        await using FileStream fs = File.OpenWrite(filePath);
        await using StreamWriter sw = new(fs);
        await sw.WriteAsync(fileContent);
    }
    /// <summary>
    /// Opens the folder in explorer.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static void OpenDirectory(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start("explorer.exe", path);
    }

    /// <summary>
    /// Removes all digits from the end of a filename.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string RemoveSuffixNumbers(this string str)
    {
        while (int.TryParse(str[^1..], out _)) str = str[..^1];
        return str;
    }
}