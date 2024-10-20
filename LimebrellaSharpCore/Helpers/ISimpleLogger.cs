namespace LimebrellaSharpCore.Helpers;

/// <summary>
/// Constructs new <see cref="SimpleLoggerOptions"/> class that holds options for <see cref="ISimpleLogger"/> class.
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
    /// An extension of a log file.
    /// </summary>
    public string LogFileExtension { get; set; } = ".log";

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
    public ISimpleLogger.LogSeverity MinSeverityLevel { get; set; } = ISimpleLogger.LogSeverity.Information;
}

public interface ISimpleLogger
{
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
    /// Determines if logging is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Creates a new log.
    /// </summary>
    void NewLog();

    /// <summary>
    /// Logs a <paramref name="message"/> into a log.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    void Log(LogSeverity logSeverity, string message);

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    void LogDebug(LogSeverity logSeverity, string message);
}