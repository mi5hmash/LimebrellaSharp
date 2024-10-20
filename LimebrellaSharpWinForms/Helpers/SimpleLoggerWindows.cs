// v2024-11-29 12:37:01

using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpWinforms.Helpers.IoHelpers;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;

namespace LimebrellaSharpWinforms.Helpers;

/// <summary>
/// Constructs new <see cref="SimpleLoggerWindows"/> class.
/// </summary>
/// <param name="options"></param>
public class SimpleLoggerWindows(SimpleLoggerOptions options) : ISimpleLogger
{
    private const string Version = "1.2";
    private const string Platform = "Windows";
    
    /// <summary>
    /// Logger options.
    /// </summary>
    private readonly SimpleLoggerOptions _options = options;
    
    /// <summary>
    /// Determines if logging is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// A path to current log file.
    /// </summary>
    public string CurrentLogFilePath { get; private set; } = string.Empty;

    /// <summary>
    /// Combines a path to a new log file. 
    /// </summary>
    /// <returns></returns>
    public string NewCurrentLogFilePath()
        => Path.Combine(_options.LogsRootDirectory, $"{_options.LogFileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}{_options.LogFileExtension}");

    /// <summary>
    /// Creates a new log.
    /// </summary>
    public void NewLog()
    {
        // get all log files
        var logFiles = Directory.GetFiles(_options.LogsRootDirectory, $"*{_options.LogFileExtension}", SearchOption.TopDirectoryOnly)
            .Where(filePath => Path.GetFileName(filePath).StartsWith(_options.LogFileNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();

        // delete the oldest log file(s) if the logs limit is reached
        var limitOverflow = logFiles.Count - _options.MaxLogFiles;
        switch (limitOverflow)
        {
            case 0:
                SafelyDeleteFile(logFiles.Last());
                break;
            case > 0:
                SafelyDeleteFiles(logFiles.TakeLast(limitOverflow).ToArray());
                break;
        }

        // update the path to current log
        CurrentLogFilePath = NewCurrentLogFilePath();

        // enable logging
        IsEnabled = true;

        // append header to the log file
        SafelyAppendFile(CurrentLogFilePath, $"~~~ {_options.LoggedAppName} log file created with SimpleLogger v{Version} ({Platform}) by Mi5hmasH. ~~~\n\n");
    }

    /// <summary>
    /// Logs a <paramref name="message"/> into a log.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void Log(LogSeverity logSeverity, string message)
    {
        if (logSeverity >= _options.MinSeverityLevel && IsEnabled)
            SafelyAppendFile(CurrentLogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logSeverity.ToString().Left(3).ToUpper()}] {message}\n");
    }

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void LogDebug(LogSeverity logSeverity, string message)
    {
        if (_options.AllowDebugMessages) Log(logSeverity, message);
    }
}