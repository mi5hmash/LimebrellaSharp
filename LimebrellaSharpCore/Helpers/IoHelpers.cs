// v2024-10-06 00:16:48

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

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
    public static bool SafelyDeleteDirectories(string[] folderPaths)
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
    public static bool SafelyDeleteFiles(string[] filePaths)
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
    /// Safely reads all bytes of a binary file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static byte[] ReadBinaryFile(string filePath)
        => !File.Exists(filePath) ? [] : File.ReadAllBytes(filePath);

    /// <summary>
    /// Safely reads all bytes of a binary file asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<byte[]> ReadBinaryFileAsync(string filePath)
        => !File.Exists(filePath) ? [] : await File.ReadAllBytesAsync(filePath);

    /// <summary>
    /// Tries to write binary file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool WriteBinaryFile(string filePath, byte[] data)
    {
        try
        {
            File.WriteAllBytes(filePath, data);
        }
        catch { return false; }
        return true;
    }

    /// <summary>
    /// Tries to write binary file asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<bool> WriteBinaryFileAsync(string filePath, byte[] data)
    {
        try
        {
            await File.WriteAllBytesAsync(filePath, data);
        }
        catch { return false; }
        return true;
    }

    /// <summary>
    /// Reads text file using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadFile(string filePath, Encoding? encoding = null)
    {
        // check if fileExists
        if (!File.Exists(filePath)) return string.Empty;
        // read file
        encoding ??= Encoding.Default;
        using StreamReader sr = new(filePath, encoding);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Reads text file using StreamReader asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<string> ReadFileAsync(string filePath, Encoding? encoding = null)
    {
        // check if fileExists
        if (!File.Exists(filePath)) return string.Empty;
        // read file
        encoding ??= Encoding.Default;
        using StreamReader sr = new(filePath, encoding);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Reads text file line by line using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string[] ReadFileLineByLine(string filePath, Encoding? encoding = null)
    {
        // check if fileExists
        if (!File.Exists(filePath)) return [string.Empty];
        // read file
        encoding ??= Encoding.Default;
        using StreamReader sr = new(filePath, encoding);
        var textFromFile = sr.ReadToEnd();
        // split text by NewLine and return lines[]
        return textFromFile.Split(Environment.NewLine);
    }

    /// <summary>
    /// Reads file line by line using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<string[]> ReadFileLineByLineAsync(string filePath, Encoding? encoding = null)
    {
        // check if fileExists
        if (!File.Exists(filePath)) return [string.Empty];
        // read file
        encoding ??= Encoding.Default;
        using StreamReader sr = new(filePath, encoding);
        var textFromFile = await sr.ReadToEndAsync();
        // split text by NewLine and return lines[]
        return textFromFile.Split(Environment.NewLine);
    }

    /// <summary>
    /// Writes text file using StreamReader.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="append"></param>
    /// <param name="encoding"></param>
    public static void WriteFile(string filePath, string content, bool append = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        using StreamWriter sw = new(filePath, append, encoding);
        sw.Write(content);
    }

    /// <summary>
    /// Writes text file using StreamReader asynchronously.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="append"></param>
    /// <param name="encoding"></param>
    public static async Task WriteFileAsync(string filePath, string content, bool append = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        await using StreamWriter sw = new(filePath, append, encoding);
        await sw.WriteAsync(content);
    }

    /// <summary>
    /// Safely appends <paramref name="content"/> to a text file or saves the <paramref name="content"/> to a new file if it does not exist.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static bool SafelyAppendFile(string filePath, string content, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.Default;
            if (File.Exists(filePath)) File.AppendAllText(filePath, content, encoding);
            else File.WriteAllText(filePath, content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Safely appends <paramref name="content"/> to a text file or saves the <paramref name="content"/> to a new file if it does not exist.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<bool> SafelyAppendFileAsync(string filePath, string content, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.Default;
            if (File.Exists(filePath)) await File.AppendAllTextAsync(filePath, content, encoding);
            else await File.WriteAllTextAsync(filePath, content);
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
    /// Opens the website in the default web browser.
    /// </summary>
    /// <param name="url"></param>
    public static void OpenWebsite(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    /// <summary>
    /// Calculates MD5 hash from the file under the given <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string Md5HashFromFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = MD5.HashData(stream);
        return hash.ToHexString();
    }
}