// v2024-12-14 23:37:01

using System.Text;
using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;

namespace LimebrellaSharpWinforms.Helpers;

/// <summary>
/// Constructs new <see cref="SimpleLoggerWindows"/> class.
/// </summary>
public class SimpleLoggerWindows(string logsRootDirectory, int maxLogFiles = 3) : VirtualLog, ISimpleLogger
{
    private const string Version = "1.4";
    private const string Platform = "Windows";

    /// <summary>
    /// A max number of log files that can be stored simultaneously.
    /// </summary>
    public int MaxLogFiles { get; set; } = maxLogFiles;

    /// <summary>
    /// A path where the log files should be stored.
    /// </summary>
    public string LogsRootDirectory { get; set; } = logsRootDirectory;

    /// <summary>
    /// A path to current log file.
    /// </summary>
    public string CurrentLogFilePath { get; private set; } = null!;

    /// <summary>
    /// Combines a path to a new log file. 
    /// </summary>
    /// <returns></returns>
    public string NewCurrentLogFilePath()
        => Path.Combine(LogsRootDirectory, GetLogFileNameWithExtension());

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
    private static bool SafelyDeleteFiles(string[] filePaths)
        => filePaths.Aggregate(true, (current, file) => SafelyDeleteFile(file) && current);

    /// <summary>
    /// Safely appends <paramref name="content"/> to a text file or saves the <paramref name="content"/> to a new file if it does not exist.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    private static void SafelyAppendFile(string filePath, string content, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        if (File.Exists(filePath)) File.AppendAllText(filePath, content, encoding);
        else File.WriteAllText(filePath, content, encoding);
    }

    /// <summary>
    /// Creates a new log.
    /// </summary>
    public void NewLog()
    {
        // get all log files
        var logFiles = Directory.GetFiles(LogsRootDirectory, $"*{LogFileExtension}", SearchOption.TopDirectoryOnly)
            .Where(filePath => Path.GetFileName(filePath).StartsWith(LogFileNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();

        // delete the oldest log file(s) if the logs limit is reached
        var limitOverflow = logFiles.Count - MaxLogFiles + 1;
        _ = limitOverflow switch
        {
            1 => SafelyDeleteFile(logFiles.Last()),
            > 1 => SafelyDeleteFiles(logFiles.TakeLast(limitOverflow).ToArray()),
            _ => false
        };

        // update the path to current log
        CurrentLogFilePath = NewCurrentLogFilePath();

        // enable logging
        Enable();

        // append header to the log file
        SafelyAppendFile(CurrentLogFilePath, CreateLogHeader(LoggedAppName, Version, Platform));
    }

    /// <summary>
    /// Creates a new log asynchronously.
    /// </summary>
    public async Task NewLogAsync()
        => await Task.Run(NewLog);

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void LogDebug(LogSeverity logSeverity, string message)
    {
        if (AllowDebugMessages) Log(logSeverity, message);
    }

    /// <summary>
    /// Asynchronously logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public async Task LogDebugAsync(LogSeverity logSeverity, string message)
        => await Task.Run(() => LogDebug(logSeverity, message));

    /// <summary>
    /// Logs a <paramref name="message"/> into a log.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void Log(LogSeverity logSeverity, string message)
    {
        if (logSeverity >= MinSeverityLevel && IsEnabled)
            LogMessage(new SimpleLoggerMessage(DateTime.Now, logSeverity, message));
    }

    /// <summary>
    /// Logs a <paramref name="message"/> into a log asynchronously.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public async Task LogAsync(LogSeverity logSeverity, string message)
        => await Task.Run(() => Log(logSeverity, message));

    /// <summary>
    /// A lock that is being used in <see cref="LogMessage"/> to hold multiple threads in line.
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="slm"></param>
    private void LogMessage(SimpleLoggerMessage slm)
    {
        if (slm is null) throw new Exception();
        var slmSize = slm.GetSize();
        if (CurrentBufferSize + slmSize <= MaxBufferSize)
        {
            // Add the message to the buffer.
            lock (_lock)
            {
                LogBuffer.Enqueue(slm);
                CurrentBufferSize += slmSize;
            }
            return;
        }
        lock (_lock)
        {
            Flush();
            SafelyAppendFile(CurrentLogFilePath, slm.GetAsLine());
        }
    }

    /// <summary>
    /// Writes all log messages from the buffer to the current log file and clears the buffer.
    /// </summary>
    public void Flush()
    {
        if (LogBuffer.Count == 0) return;
        using (var streamWriter = new StreamWriter(CurrentLogFilePath, append: true, Encoding.Default))
        {
            while (LogBuffer.Count > 0)
            {
                var logMessage = LogBuffer.Dequeue();
                streamWriter.Write(logMessage.GetAsLine());
            }
        }
        CurrentBufferSize = 0;
    }

    /// <summary>
    /// Writes all log messages from the buffer to the current log file asynchronously and clears the buffer.
    /// </summary>
    public async Task FlushAsync()
    {
        if (LogBuffer.Count == 0) return;
        await using (var streamWriter = new StreamWriter(CurrentLogFilePath, append: true, Encoding.Default))
        {
            while (LogBuffer.Count > 0)
            {
                var logMessage = LogBuffer.Dequeue();
                await streamWriter.WriteAsync(logMessage.GetAsLine());
            }
        }
        CurrentBufferSize = 0;
    }
}