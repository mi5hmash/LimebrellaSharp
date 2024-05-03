using System.Diagnostics;
using System.Security.Cryptography;

namespace LimebrellaSharpCore.Helpers;

public static class IoHelpers
{
    /// <summary>
    /// Tries to safely delete folder under given <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns>True if folderPath has been successfully deleted.</returns>
    public static bool SafelyDeleteDirectory(string folderPath)
    {
        try { Directory.Delete(folderPath, true); }
        catch { /* ignored */ }
        return !Directory.Exists(folderPath);
    }
    /// <summary>
    /// Tries to safely delete many folders located under the given <paramref name="folderPaths"/>.
    /// </summary>
    /// <param name="folderPaths"></param>
    /// <returns></returns>
    public static bool SafelyDeleteDirectory(string[] folderPaths)
        => folderPaths.Aggregate(true, (current, folder) => SafelyDeleteDirectory(folder) && current);

    /// <summary>
    /// Tries to safely delete file located under the given <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>True if file has been successfully deleted.</returns>
    public static bool SafelyDeleteFile(string filePath)
    {
        try { File.Delete(filePath); }
        catch { /* ignored */ }
        return !Directory.Exists(filePath);
    }
    /// <summary>
    /// Tries to safely delete many files located under the given <paramref name="filePaths"/>.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public static bool SafelyDeleteFile(string[] filePaths)
        => filePaths.Aggregate(true, (current, file) => SafelyDeleteFile(file) && current);

    /// <summary>
    /// Tries to safely recreate <paramref name="folderPath"/> folderPath.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public static bool RecreateDirectory(string folderPath)
    {
        if (!SafelyDeleteDirectory(folderPath)) return false;
        Directory.CreateDirectory(folderPath);
        return true;
    }

    /// <summary>
    /// Reads file using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadFile(string filePath)
    {
        using StreamReader sr = new(filePath);
        return sr.ReadToEnd();
    }
    /// <summary>
    /// Reads file using StreamReader asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<string> ReadFileAsync(string filePath)
    {
        using StreamReader sr = new(filePath);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Writes file using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="append"></param>
    public static void WriteFile(string filePath, string content, bool append = false)
    {
        using StreamWriter sw = new(filePath, append);
        sw.Write(content);
    }

    /// <summary>
    /// Writes file using StreamReader asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="append"></param>
    public static async Task WriteFileAsync(string filePath, string content, bool append = false)
    {
        await using StreamWriter sw = new(filePath, append);
        await sw.WriteAsync(content);
    }

    /// <summary>
    /// Safely appends <paramref name="content"/> to a text file or saves the <paramref name="content"/> to a new file if it does not exist.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool SafelyAppendFile(string filePath, string content)
    {
        try
        {
            if (File.Exists(filePath)) File.AppendAllText(filePath, content);
            else File.WriteAllText(filePath, content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Opens the folder in the new file explorer window.
    /// </summary>
    /// <param name="fPath"></param>
    /// <returns></returns>
    public static void OpenDirectory(string fPath)
    {
        if (File.Exists(fPath))
            Process.Start("explorer.exe", $"/select,\"{fPath}\"");
        else if (Directory.Exists(fPath))
            Process.Start("explorer.exe", fPath);
    }

    /// <summary>
    /// Calculates MD5 hash from the file under the given <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string Md5HashFromFile(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return hash.ToHexString();
    }
}