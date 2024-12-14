// v2024-12-14 19:34:11

using LimebrellaSharpCore.Helpers;
using Microsoft.JSInterop;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;

namespace LimebrellaSharpBlazorWASM.Services;

/// <summary>
/// A service that manage the message logging.
/// </summary>
public class SimpleLoggerWasmService(IJSRuntime jsRuntime) : VirtualLog, ISimpleLogger
{
    private const string Version = "1.0";
    private const string Platform = "WebAssembly";

    /// <summary>
    /// Creates a new log.
    /// </summary>
    public void NewLog()
    {
        _ = NewLogAsync();
    }

    /// <summary>
    /// Creates a new log asynchronously.
    /// </summary>
    public async Task NewLogAsync()
    {
        // enable logging
        Enable();
        // create a header log and send it
        await LogAsync(LogSeverity.Debug, CreateLogHeader(LoggedAppName, Version, Platform));
    }

    /// <summary>
    /// Logs a <paramref name="message"/> into a log.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void Log(LogSeverity logSeverity, string message)
    {
        _ = LogAsync(logSeverity, message);
    }

    /// <summary>
    /// Logs a <paramref name="message"/> into a log asynchronously.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public async Task LogAsync(LogSeverity logSeverity, string message)
    {
        if (logSeverity < MinSeverityLevel || !IsEnabled) return;
        var type = logSeverity switch
        {
            LogSeverity.Trace => "trace",
            LogSeverity.Debug => "log",
            LogSeverity.Information => "info",
            LogSeverity.Warning => "warn",
            LogSeverity.Error => "error",
            LogSeverity.Critical => "error",
            _ => throw new ArgumentOutOfRangeException(nameof(logSeverity), logSeverity, null)
        };

        var slm = new SimpleLoggerMessage(DateTime.Now, logSeverity, message);
        await jsRuntime.InvokeVoidAsync($"console.{type}", slm.GetAsLine());
    }

    /// <summary>
    /// Logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public void LogDebug(LogSeverity logSeverity, string message)
    {
        if (AllowDebugMessages)
            _ = LogAsync(logSeverity, message);
    }

    /// <summary>
    /// Asynchronously logs a debug message <paramref name="message"/> into a log if it is allowed.
    /// </summary>
    /// <param name="logSeverity"></param>
    /// <param name="message"></param>
    public async Task LogDebugAsync(LogSeverity logSeverity, string message)
    {
        if (AllowDebugMessages)
            await LogAsync(logSeverity, message);
    }

    /// <summary>
    /// Clears the buffer.
    /// </summary>
    public void Flush()
    {
        CurrentBufferSize = 0;
    }

    /// <summary>
    /// Asynchronously clears the buffer.
    /// </summary>
    public async Task FlushAsync()
        => await Task.Run(Flush);
}