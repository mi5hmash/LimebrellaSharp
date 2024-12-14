// v2024-12-14 23:37:01

using System.Text;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;

namespace LimebrellaSharpCore.Helpers;

/// <summary>
/// A model of a <see cref="SimpleLoggerMessage"/>.
/// </summary>
/// <param name="timestamp"></param>
/// <param name="severity"></param>
/// <param name="message"></param>
public class SimpleLoggerMessage(DateTime timestamp, LogSeverity severity, string message)
{
    public DateTime Timestamp { get; set; } = timestamp;

    public LogSeverity Severity { get; set; } = severity;

    public string Message { get; set; } = message;

    public int GetSize()
        => 8 + 4 + Encoding.Default.GetByteCount(Message);

    public string GetAsLine() =>
        $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Severity.ToString()[..3].ToUpper()}] {Message}\n";
}

/// <summary>
/// A model of a <see cref="VirtualLog"/>.
/// </summary>
public class VirtualLog
{
    protected Queue<SimpleLoggerMessage> LogBuffer = new();
    protected int CurrentBufferSize;

    /// <summary>
    /// Determines if logging is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

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
    /// Minimum severity level of the messages to include in the log.
    /// </summary>
    public LogSeverity MinSeverityLevel { get; set; } = LogSeverity.Information;

    /// <summary>
    /// Maximum buffer size in bytes.
    /// </summary>
    public int MaxBufferSize { get; set; } = int.MaxValue;

    /// <summary>
    /// Disables logger. 
    /// </summary>
    public void Disable() => IsEnabled = false;

    /// <summary>
    /// Enables logger.
    /// </summary>
    public void Enable() => IsEnabled = true;

    /// <summary>
    /// Gets Log File Name with its extension.
    /// </summary>
    /// <returns></returns>
    public string GetLogFileNameWithExtension()
        => $"{LogFileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}{LogFileExtension}";

    /// <summary>
    /// Creates a Log Header.
    /// </summary>
    /// <param name="loggedAppName"></param>
    /// <param name="version"></param>
    /// <param name="platform"></param>
    /// <returns></returns>
    public static string CreateLogHeader(string loggedAppName, string version, string platform)
        => $"~~~ {loggedAppName} log created with SimpleLogger v{version} ({platform}) by Mi5hmasH. ~~~\n";
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
    /// Creates a new log.
    /// </summary>
    void NewLog();

    /// <summary>
    /// Creates a new log asynchronously.
    /// </summary>
    Task NewLogAsync();

    /// <summary>
    /// Logs a <paramref name="message"/> into a log.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    void Log(LogSeverity logSeverity, string message);

    /// <summary>
    /// Logs a <paramref name="message"/> into a log asynchronously.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    Task LogAsync(LogSeverity logSeverity, string message);

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    void LogDebug(LogSeverity logSeverity, string message);

    /// <summary>
    /// Asynchronously logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    Task LogDebugAsync(LogSeverity logSeverity, string message);

    /// <summary>
    /// Writes all log messages from the buffer to the current log file and clears the buffer.
    /// </summary>
    void Flush();

    /// <summary>
    /// Writes all log messages from the buffer to the current log file asynchronously and clears the buffer.
    /// </summary>
    Task FlushAsync();
}