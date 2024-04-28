using static LimebrellaSharpCore.Helpers.IoHelpers;

namespace LimebrellaSharpCore.Helpers;

/// <summary>
/// Constructs new <see cref="SimpleLoggerOptions"/> class that holds options for <see cref="SimpleLogger"/> class.
/// </summary>
/// <param name="logsRootDirectory"></param>
public class SimpleLoggerOptions(string logsRootDirectory)
{
    /// <summary>
    /// Determines if logger logs debug messages.
    /// </summary>
    public bool AllowDebugMessages { get; set; }

    /// <summary>
    /// A prefix used in log file naming.
    /// </summary>
    public string LogFileNamePrefix { get; set; } = "log";

    /// <summary>
    /// Stores the name of the logged app.
    /// </summary>
    public string LoggedAppName { get; set; } = "Program";

    /// <summary>
    /// A path where the log files should be stored.
    /// </summary>
    public string LogsRootDirectory { get; set; } = logsRootDirectory;

    /// <summary>
    /// A max number of log files that can be stored simultaneously.
    /// </summary>
    public int MaxLogFiles { get; set; } = 3;

    /// <summary>
    /// Minimum severity level of the messages to include in the log.
    /// </summary>
    public SimpleLogger.LogSeverity MinSeverityLevel { get; set; } = SimpleLogger.LogSeverity.Information;
}

/// <summary>
/// Constructs new <see cref="SimpleLogger"/> class.
/// </summary>
/// <param name="options"></param>
public class SimpleLogger(SimpleLoggerOptions options)
{
    private const string Version = "1.1";
    private const string LogFileExtension = ".log";

    /// <summary>
    /// Log severity enumerator.
    /// </summary>
    public enum LogSeverity
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Logger options.
    /// </summary>
    private readonly SimpleLoggerOptions _options = options;

    /// <summary>
    /// A path to current log file.
    /// </summary>
    public string CurrentLogFilePath { get; private set; } = string.Empty;

    /// <summary>
    /// Determines if logging is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Combines a path to a new log file. 
    /// </summary>
    /// <returns></returns>
    private string NewCurrentLogFilePath()
        => Path.Combine(_options.LogsRootDirectory, $"{_options.LogFileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}{LogFileExtension}");

    /// <summary>
    /// Creates a folder for a new backup.
    /// </summary>
    public void NewLogFile()
    {
        // get all log files
        var logFiles = Directory.GetFiles(_options.LogsRootDirectory, $"*{LogFileExtension}", SearchOption.TopDirectoryOnly)
            .Where(filePath => Path.GetFileName(filePath).StartsWith(_options.LogFileNamePrefix, StringComparison.OrdinalIgnoreCase)).OrderDescending().ToList();

        // delete the oldest log file(s) if the logs limit is reached
        var limitOverflow = logFiles.Count - _options.MaxLogFiles;
        switch (limitOverflow)
        {
            case 0:
                SafelyDeleteFile(logFiles.Last());
                break;
            case > 0:
                SafelyDeleteFile(logFiles.TakeLast(limitOverflow).ToArray());
                break;
        }

        // update the path to current log
        CurrentLogFilePath = NewCurrentLogFilePath();

        // enable logging
        IsEnabled = true;

        // append header to the log file
        SafelyAppendFile(CurrentLogFilePath, $"~~~ {_options.LoggedAppName} log file created with SimpleLogger v{Version} by Mi5hmasH. ~~~\n\n");
    }

    /// <summary>
    /// Logs a <paramref name="message"/> into a log file.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void Log(LogSeverity logSeverity, string message)
    {
        if (logSeverity >= _options.MinSeverityLevel && IsEnabled)
            SafelyAppendFile(CurrentLogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logSeverity.ToString().Left(3).ToUpper()}] {message}\n");
    }

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log file if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void LogDebug(LogSeverity logSeverity, string message)
    {
        if (_options.AllowDebugMessages) Log(logSeverity, message);
    }
}