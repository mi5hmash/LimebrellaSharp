using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Models;
using LimebrellaSharpCore.Models.DSSS.Lime;
using LimebrellaSharpWinforms.Helpers;
using static LimebrellaSharpWinforms.Helpers.IoHelpers;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;
using static LimebrellaSharpCore.Helpers.ISimpleMediator;

namespace LimebrellaSharpWinforms;

public class Core
{
    #region BUSY_LOCK

    public bool IsBusy { get; private set; }

    #endregion

    #region PATHS

    private const string OutputFolder = "_OUTPUT";
    public static string OutputPath => Path.Combine(AppInfo.RootPath, OutputFolder);

    #endregion

    #region IO

    /// <summary>
    /// Creates necessary directories.
    /// </summary>
    public static void CreateDirectories()
    {
        Directory.CreateDirectory(OutputPath);
    }

    /// <summary>
    /// Checks whether the directory at the given path exists.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    private static bool DoesDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath)) return true;
        _mediator.Inform($"""Directory: "{directoryPath}" does not exists.""", "Error", DialogType.Error);
        return false;
    }

    /// <summary>
    /// Checks if <paramref name="inputPath"/> is a forbidden directory.
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsForbiddenDirectory(string inputPath, string path)
    {
        if (!inputPath.Contains(path)) return false;
        _mediator.Inform($"The entered path:{Environment.NewLine}\"{path}\"{Environment.NewLine}cannot be used as the Input Folder Path.{Environment.NewLine}The path has not been updated.", "Forbidden directory", DialogType.Exclamation);
        return true;
    }

    /// <summary>
    /// Tries to write an array of bytes to a file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private static DialogAnswer WriteBytesToFile(string filePath, ReadOnlySpan<byte> fileData)
    {
        do
        {
            if (WriteBinaryFile(filePath, fileData.ToArray())) return DialogAnswer.Continue;
            // ask the user if they want to try again
            var dialogResult = _mediator.Ask($"Failed to save the file: \"{filePath}\".{Environment.NewLine}It may be currently in use by another program.{Environment.NewLine}Would you like to try again?", "Failed to save the file", QuestionOptions.AbortRetryIgnore, DialogType.Exclamation);
            if (dialogResult == DialogAnswer.Retry) continue;
            return dialogResult;
        } while (true);
    }

    /// <summary>
    /// Opens the Output directory.
    /// </summary>
    public static void OpenOutputDirectory()
        => OpenDirectory(OutputPath);

    #endregion

    #region INPUT_PATH

    public string InputPath { get; set; } = AppInfo.RootPath;

    /// <summary>
    /// Validates a <paramref name="inputPath"/> string and sets the <see cref="InputPath"/> property.
    /// </summary>
    /// <param name="inputPath"></param>
    public void SetInputPath(string inputPath)
    {
        if (IsBusy) return;

        // checking for forbidden directory
        if (IsForbiddenDirectory(inputPath, OutputPath)) return;

        // checking if directory exists
        var result = Directory.Exists(inputPath);
        if (result)
        {
            InputPath = inputPath;
            SteamIdManager.ExtractSteamIdFromPathIfValid(inputPath);
        }
        _progressReporter.Report(result ? "The entered Input Folder Path is correct." : "The entered Input Folder Path was invalid.", 0);
    }

    #endregion

    #region STEAM_ID

    public readonly SteamIdManager SteamIdManager = new(@$"\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}remote\{Path.DirectorySeparatorChar}win64_save\{Path.DirectorySeparatorChar}?$");

    #endregion

    #region CONSTRUCTOR

    private CancellationTokenSource _cts = new();
    private readonly LimeDeencryptor _deencryptor = new();
    private static SimpleLoggerWindows _logger = null!;
    private static SimpleMediatorWinForms _mediator = null!;
    private static ProgressReporter _progressReporter = null!;

    /// <summary>
    /// Constructs new <see cref="Core"/> class.
    /// </summary>
    /// <param name="progressReporter"></param>
    public Core(ProgressReporter progressReporter)
    {
        _mediator = new SimpleMediatorWinForms();
        _logger = new SimpleLoggerWindows(new SimpleLoggerOptions(AppInfo.RootPath)
        {
            AllowDebugMessages = true,
            LoggedAppName = $"{AppInfo.Title} v{AppInfo.Version}",
            MaxLogFiles = 1,
            MinSeverityLevel = LogSeverity.Information
        });
        _logger.NewLog();
        _progressReporter = progressReporter;
        
        // create directories
        CreateDirectories();
    }

    #endregion

    #region FUNCTIONS
    
    /// <summary>
    /// Aborts currently running operation and lifts the Busy Lock.
    /// </summary>
    public void AbortOperation()
    {
        if (!IsBusy) return;

        _cts.Cancel();
        _cts.Dispose();
        IsBusy = false;
    }

    private enum OperationType
    {
        Packing,
        Unpacking,
        Resigning
    }

    public Task UnpackAllAsync()
        => ProcessAsyncOperation(OperationType.Unpacking);

    public Task PackAllAsync()
        => ProcessAsyncOperation(OperationType.Packing);

    public Task ResignAllAsync()
        => ProcessAsyncOperation(OperationType.Resigning);
    
    private async Task ProcessAsyncOperation(OperationType operationType)
    {
        if (IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            _logger.Log(LogSeverity.Information, $"{operationType} has started.");
            _logger.Log(LogSeverity.Information, $"Provided Steam32_IDs: INPUT={SteamIdManager.GetInput()}, OUTPUT={SteamIdManager.GetOutput()}");
            _logger.Log(LogSeverity.Information, "ID | FileName | MD5_Checksum | IsEncrypted");
            await AsyncLimeFileOperation(operationType);
            _logger.Log(LogSeverity.Information, $"{operationType} complete.");
        }
        catch (OperationCanceledException)
        {
            var message = $"{operationType} was interrupted by the user.";
            _progressReporter.Report(message);
            _logger.Log(LogSeverity.Warning, message);
        }
        AbortOperation();
    }

    private Task AsyncLimeFileOperation(OperationType operationType)
    {
        return Task.Run(() =>
        {
            // check if input directory exists
            if (!DoesDirectoryExists(InputPath)) return;

            // get the paths of "*.bin" files located in the input directory
            var files = Directory.GetFiles(InputPath, $"*.{LimeFile.Extension}", SearchOption.TopDirectoryOnly);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var processedFiles = 0;
            var progress = 0;
            _progressReporter.Report($"[{progress}/{files.Length}] Processing files...", progress);

            Parallel.For(0, files.Length, po, ctr =>
            {
                // try to load file
                var fileDataInput = ReadBinaryFile(files[ctr]);
                var dsssFile = new LimeFile(_deencryptor, Path.GetFileNameWithoutExtension(files[ctr]));
                var boolResult = dsssFile.SetFileData(fileDataInput);
                if (!boolResult.Result)
                {
                    _logger.Log(LogSeverity.Error, $"I_{ctr} -> Couldn't load the file. {boolResult.Description}");
                    goto ORDER_66;
                }
                
                // log info about the input file
                _logger.LogDebug(LogSeverity.Information, $"I_{ctr} | {dsssFile.FileName} | {Md5HashFromByteArray(fileDataInput)} | {dsssFile.IsEncrypted}");

                // check operation type and adjust to it
                var result = false;
                switch (operationType)
                {
                    case OperationType.Resigning:
                        if (dsssFile.IsEncrypted)
                        {
                            dsssFile.Key = SteamIdManager.GetInputNumeric();
                            result = dsssFile.DecryptSegments();
                            if (!result) break;
                        }
                        dsssFile.Key = SteamIdManager.GetOutputNumeric();
                        dsssFile.EncryptSegments();
                        break;
                    case OperationType.Unpacking:
                        if (!dsssFile.IsEncrypted)
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already decrypted.");
                            goto ORDER_66;
                        }
                        dsssFile.Key = SteamIdManager.GetInputNumeric();
                        result = dsssFile.DecryptSegments();
                        break;
                    case OperationType.Packing:
                    default:
                        if (dsssFile.IsEncrypted)
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already encrypted.");
                            goto ORDER_66;
                        }
                        dsssFile.Key = SteamIdManager.GetOutputNumeric();
                        result = dsssFile.EncryptSegments();
                        break;
                }
                if (!result)
                {
                    _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The provided SteamID does not work for this file.");
                    goto ORDER_66;
                }

                // save file
                var filePathOutput = Path.Combine(OutputPath, $"{dsssFile.FileName}.{LimeFile.Extension}");
                var fileDataOutput = operationType switch
                {
                    OperationType.Unpacking => dsssFile.GetFileSegments(),
                    _ => dsssFile.GetFileData(),
                };
                var writeResult = WriteBytesToFile(filePathOutput, fileDataOutput);
                switch (writeResult)
                {
                    case DialogAnswer.Continue:
                        processedFiles++;
                        break;
                    case DialogAnswer.Abort:
                        _cts.Cancel();
                        goto ORDER_66;
                    default:
                        goto ORDER_66;
                }

                // log info about the output file
                _logger.LogDebug(LogSeverity.Information, $"O_{ctr} | {dsssFile.FileName} | {Md5HashFromByteArray(fileDataOutput)} | {dsssFile.IsEncrypted}");

            ORDER_66:
                Interlocked.Increment(ref progress);
                _progressReporter.Report($"[{progress}/{files.Length}] Processing files...", (int)((double)progress / files.Length * 100));
            });

            var message = $"{operationType} done. Number of processed files: {processedFiles}.";
            _logger.Log(LogSeverity.Information, message);
            _progressReporter.Report(message, 100);
        });
    }

    public async Task BruteforceSteamIdAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            await BruteforceFirst();
        }
        catch (OperationCanceledException)
        {
            _progressReporter.Report("The operation was interrupted by the user.");
        }
        AbortOperation();
    }

    private Task BruteforceFirst()
    {
        return Task.Run(() =>
        {
            // check if input directory exists
            if (!DoesDirectoryExists(InputPath)) return;

            // get the paths of "*.bin" files located in the input directory
            var files = Directory.GetFiles(InputPath, $"*.{LimeFile.Extension}", SearchOption.TopDirectoryOnly);
            
            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };
            
            // find first compatible file
            var result = false;
            var dsssFile = new LimeFile(_deencryptor);
            foreach (var file in files)
            {
                var fileDataInput = ReadBinaryFile(file);
                result = dsssFile.SetFileData(fileDataInput, true).Result;
                if (result) break;
            }
            if (!result)
            {
                _mediator.Inform($"There is no compatible file in the \"{InputPath}\" directory.", "Error", DialogType.Error);
                return;
            }

            const uint batches = 100;
            const uint maxValue = uint.MaxValue;
            const uint batchSize = maxValue / batches;
            
            BoolResult boolResult = new(false, "Bruteforce attempt has failed!");

            var progress = 0;
            _progressReporter.Report($"[{progress}%] Drilling...", progress);

            Parallel.For(0, batches, po, (ctr, state) =>
            {
                // bruteforce
                var start = (uint)(ctr * batchSize + ctr);
                var end = ctr >= batches - 1 ? maxValue : start + batchSize;
                var bruteforceResult = dsssFile.BruteforceSegment(out var id, _cts, start, end);
                // update progress
                Interlocked.Increment(ref progress);
                _progressReporter.Report($"[{progress}%] Drilling...", progress);
                // check if the correct SteamID has been found
                if (!bruteforceResult) return;
                boolResult.Set(true, "The Correct SteamID has been found!");
                // update the Input SteamID
                SteamIdManager.SetInput(id);
                state.Break();
            });
            var message = $"{boolResult.Description}";
            _logger.Log(LogSeverity.Information, message);
            _progressReporter.Report($"[{progress}%] {message}", progress);
        });
    }

    #endregion
}