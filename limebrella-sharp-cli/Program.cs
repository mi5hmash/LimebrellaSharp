using LimebrellaSharpCore;
using LimebrellaSharpCore.Helpers;
using Mi5hmasH.AppInfo;
using Mi5hmasH.ConsoleHelper;
using Mi5hmasH.Logger;
using Mi5hmasH.Logger.Models;
using Mi5hmasH.Logger.Providers;

#region SETUP

// CONSTANTS
const string breakLine = "---";

// Initialize APP_INFO
var appInfo = new MyAppInfo("limebrella-sharp-cli");

// Initialize LOGGER
var logger = new SimpleLogger
{
    LoggedAppName = appInfo.Name
};
// Configure ConsoleLogProvider
var consoleLogProvider = new ConsoleLogProvider();
logger.AddProvider(consoleLogProvider);
// Configure FileLogProvider
var fileLogProvider = new FileLogProvider(MyAppInfo.RootPath, 2);
fileLogProvider.CreateLogFile();
logger.AddProvider(fileLogProvider);
// Add event handler for unhandled exceptions
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    if (e.ExceptionObject is not Exception exception) return;
    var logEntry = new LogEntry(SimpleLogger.LogSeverity.Critical, $"Unhandled Exception: {exception}");
    fileLogProvider.Log(logEntry);
    fileLogProvider.Flush();
};
// Flush log providers on process exit
AppDomain.CurrentDomain.ProcessExit += (_, _) => logger.Flush();

//Initialize ProgressReporter
var progressReporter = new ProgressReporter(new Progress<string>(Console.WriteLine), null);

// Initialize CORE
var core = new Core(logger, progressReporter);

// Print HEADER
ConsoleHelper.PrintHeader(appInfo, breakLine);

// Say HELLO
ConsoleHelper.SayHello(breakLine);

// Get ARGUMENTS from command line
#if DEBUG
// For debugging purposes, you can manually set the arguments...
if (args.Length < 1)
{
    // ...below
    const string localArgs = "-m TEST";
    args = ConsoleHelper.GetArgs(localArgs);
}
#endif
var arguments = ConsoleHelper.ReadArguments(args);
#if DEBUG
// Write the arguments to the console for debugging purposes
ConsoleHelper.WriteArguments(arguments);
Console.WriteLine(breakLine);
#endif

#endregion

#region MAIN

// Show HELP if no arguments are provided or if -h is provided
if (arguments.Count == 0 || arguments.ContainsKey("-h"))
{
    PrintHelp();
    return;
}

// Optional argument: isVerbose
var isVerbose = arguments.ContainsKey("-v");

// Get MODE
arguments.TryGetValue("-m", out var mode);
switch (mode)
{
    case "unpack" or "u":
        await UnpackAll();
        break;
    case "pack" or "p":
        await PackAll();
        break;
    case "resign" or "r":
        await ResignAll();
        break;
    default:
        throw new ArgumentException($"Unknown mode: '{mode}'.");
}

// EXIT the application
Console.WriteLine(breakLine); // print a break line
ConsoleHelper.SayGoodbye(breakLine);
#if DEBUG
ConsoleHelper.PressAnyKeyToExit();
#else
if (isVerbose) ConsoleHelper.PressAnyKeyToExit();
#endif

return;

#endregion

#region HELPERS

static void PrintHelp()
{
    const string steamIdInput = "1";
    const string steamIdOutput = "2";
    var inputPath = Path.Combine(".", "InputDirectory");
    var exeName = Path.Combine(".", Path.GetFileName(Environment.ProcessPath) ?? "ThisExecutableFileName.exe");
    var helpMessage = $"""
                       Usage: {exeName} -m <mode> [options]

                       Modes:
                         -m u  Unpack SaveData files
                         -m p  Pack SaveData files
                         -m r  Re-sign SaveData files

                       Options:
                         -p <path>      Path to folder containing SaveData files
                         -s <steam_id>  Steam ID (used in unpack/pack modes)
                         -sI <old_id>   Original Steam ID (used in re-sign mode)
                         -sO <new_id>   New Steam ID (used in re-sign mode)
                         -v             Verbose output
                         -h             Show this help message

                       Examples:
                         Unpack:  {exeName} -m u -p "{inputPath}" -s {steamIdInput}
                         Pack:    {exeName} -m p -p "{inputPath}" -s {steamIdOutput}
                         Re-sign: {exeName} -m r -p "{inputPath}" -sI {steamIdInput} -sO {steamIdOutput}
                       """;
    Console.WriteLine(helpMessage);
}

string GetValidatedInputRootPath()
{
    arguments.TryGetValue("-p", out var inputRootPath);
    if (File.Exists(inputRootPath)) inputRootPath = Path.GetDirectoryName(inputRootPath);
    return !Directory.Exists(inputRootPath)
        ? throw new DirectoryNotFoundException($"The provided path '{inputRootPath}' is not a valid directory or does not exist.")
        : inputRootPath;
}

#endregion

#region MODES

async Task UnpackAll()
{
    var cts = new CancellationTokenSource();
    arguments.TryGetValue("-s", out var steamId);
    if (string.IsNullOrEmpty(steamId))
        throw new ArgumentException("Input Steam ID is missing.");
    var inputRootPath = GetValidatedInputRootPath();
    await core.UnpackFilesAsync(inputRootPath, Convert.ToUInt64(steamId), cts);
    cts.Dispose();
}

async Task PackAll()
{
    var cts = new CancellationTokenSource();
    arguments.TryGetValue("-s", out var steamId);
    if (string.IsNullOrEmpty(steamId))
        throw new ArgumentException("Output Steam ID is missing.");
    var inputRootPath = GetValidatedInputRootPath();
    await core.PackFilesAsync(inputRootPath, Convert.ToUInt64(steamId), cts);
    cts.Dispose();
}

async Task ResignAll()
{
    var cts = new CancellationTokenSource();
    arguments.TryGetValue("-sI", out var steamIdInput);
    if (string.IsNullOrEmpty(steamIdInput))
        throw new ArgumentException("Input Steam ID is missing.");
    arguments.TryGetValue("-sO", out var steamIdOutput);
    if (string.IsNullOrEmpty(steamIdOutput))
        throw new ArgumentException("Output Steam ID is missing.");
    var inputRootPath = GetValidatedInputRootPath();
    await core.ResignFilesAsync(inputRootPath, Convert.ToUInt64(steamIdInput), Convert.ToUInt64(steamIdOutput), cts);
    cts.Dispose();
}

#endregion