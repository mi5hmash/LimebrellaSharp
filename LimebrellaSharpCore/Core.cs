using System.Text.RegularExpressions;
using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Models;
using LimebrellaSharpCore.Models.DSSS.Lime;
using static LimebrellaSharpCore.Helpers.IoHelpers;
using static LimebrellaSharpCore.Helpers.ISimpleMediator;
using static LimebrellaSharpCore.Helpers.SimpleLogger;

namespace LimebrellaSharpCore;

public class Core
{

    #region PATHS

    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    private const string OutputFolder = "_OUTPUT";
    public static string OutputPath => Path.Combine(RootPath, OutputFolder);
    
    private static string PathPattern => @$"\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}remote\{Path.DirectorySeparatorChar}win64_save\{Path.DirectorySeparatorChar}?$";

    #endregion
    
    #region BUSY_LOCK

    public bool IsBusy { get; private set; }

    #endregion

    #region LOGGER

    private readonly SimpleLogger _logger;

    public void ActivateLogger() => _logger.NewLogFile();

    #endregion

    #region PROGRESS

    private readonly IProgress<string> _pText;
    private readonly IProgress<int> _pValue;

    /// <summary>
    /// Reports progress.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    private void ReportProgress(string text, int value)
    {
        _pText.Report(text);
        _pValue.Report(value);
    }

    #endregion
    
    #region CONSTRUCTOR

    private readonly LimeDeencryptor _deencryptor = new();
    private CancellationTokenSource _cts = new();
    private static ISimpleMediator _mediator = null!;

    /// <summary>
    /// Constructs new <see cref="Core"/> class.
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="pText"></param>
    /// <param name="pValue"></param>
    /// <param name="logger"></param>
    public Core(ISimpleMediator mediator, IProgress<string> pText, IProgress<int> pValue, SimpleLogger logger)
    {
        _mediator = mediator;
        _pText = pText;
        _pValue = pValue;
        _logger = logger;
        InputDirectory = RootPath;
        InitializeComponent();
    }

    /// <summary>
    /// Initialize component.
    /// </summary>
    private static void InitializeComponent()
    {
        // create directories
        CreateDirectories();
    }

    #endregion

    #region IO

    /// <summary>
    /// Creates necessary directories.
    /// </summary>
    private static void CreateDirectories()
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
    /// Tries to write an array of bytes to a file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private static DialogAnswer WriteBytesToFile(string filePath, ReadOnlySpan<byte> fileData)
    {
        do
        {
            if (TryWriteAllBytes(filePath, fileData)) return DialogAnswer.Continue;
            // ask the user if they want to try again
            var dialogResult = _mediator.Ask($"""Failed to save the file: "{filePath}".{Environment.NewLine}It may be currently in use by another program.{Environment.NewLine}Would you like to try again?""", "Failed to save the file", QuestionOptions.AbortRetryIgnore, DialogType.Exclamation);
            if (dialogResult == DialogAnswer.Retry) continue;
            return dialogResult;
        } while (true);

        static bool TryWriteAllBytes(string fPath, ReadOnlySpan<byte> bytes)
        {
            try
            {
                File.WriteAllBytes(fPath, bytes.ToArray());
            }
            catch { return false; }
            return true;
        }
    }
    
    /// <summary>
    /// Opens the Output directory.
    /// </summary>
    public static void OpenOutputDirectory()
        => OpenDirectory(OutputPath);
    
    #endregion

    #region STEAM_ID

    public uint SteamIdInput { get; private set; }
    public uint SteamIdOutput { get; private set; }

    private enum SteamIdType
    {
        Input,
        Output
    }

    /// <summary>
    /// Validates a <paramref name="steamId"/> string and sets the <see cref="SteamIdInput"/> property.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="verbose"></param>
    public void SetSteamIdInput(string steamId, bool verbose = false)
        => SetSteamId(steamId, SteamIdType.Input, verbose);

    /// <summary>
    /// Validates a <paramref name="steamId"/> string and sets the <see cref="SteamIdOutput"/> property.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="verbose"></param>
    public void SetSteamIdOutput(string steamId, bool verbose = false)
        => SetSteamId(steamId, SteamIdType.Output, verbose);

    /// <summary>
    /// Validates a <paramref name="steamId"/> string and sets the value of SteamId property of a type determined by the <paramref name="steamIdType"/>.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="steamIdType"></param>
    /// <param name="verbose"></param>
    private void SetSteamId(string steamId, SteamIdType steamIdType, bool verbose = false)
    {
        if (IsBusy) return;
        var result = ulong.TryParse(steamId, out var parsed);
        if (result)
        {
            switch (steamIdType)
            {
                case SteamIdType.Input:
                    SteamIdInput = (uint)parsed;
                    break;
                case SteamIdType.Output:
                    SteamIdOutput = (uint)parsed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(steamIdType), steamIdType, null);
            }
        }

        var steamIdTypeAsString = steamIdType.ToString().ToUpper();
        if (!verbose) ReportProgress(result ? $"The entered SteamID ({steamIdTypeAsString}) is correct." : $"The entered SteamID ({steamIdTypeAsString}) was invalid.", 0);
    }

    /// <summary>
    /// Swaps <see cref="SteamIdInput"/> and <see cref="SteamIdOutput"/>.
    /// </summary>
    public void SteamIdInterchange()
    {
        if (IsBusy) return;
        (SteamIdInput, SteamIdOutput) = (SteamIdOutput, SteamIdInput);
    }

    /// <summary>
    /// Extracts SteamID from a directory path.
    /// </summary>
    /// <param name="directoryPath"></param>
    private void ExtractSteamIdFromPathIfValid(string directoryPath)
    {
        var match = Regex.Match(directoryPath, PathPattern);
        if (match.Success) SetSteamIdInput(match.Groups[1].Value, true);
    }

    #endregion

    #region INPUT_DIRECTORY

    public string InputDirectory { get; private set; }

    /// <summary>
    /// Validates a <paramref name="inputPath"/> string and sets the <see cref="InputDirectory"/> property.
    /// </summary>
    /// <param name="inputPath"></param>
    public void SetInputDirectory(string inputPath)
    {
        if (IsBusy) return;

        // checking for forbidden directory
        if (IsForbiddenDirectory(OutputPath)) return;

        // checking if directory exists
        var result = Directory.Exists(inputPath);
        if (result)
        {
            InputDirectory = inputPath;
            ExtractSteamIdFromPathIfValid(inputPath);
        }
        ReportProgress(result ? "The entered Input Folder Path is correct." : "The entered Input Folder Path was invalid.", 0);
        return;

        bool IsForbiddenDirectory(string path)
        {
            if (!inputPath.Contains(path)) return false;
            _mediator.Inform($"The entered path:\n\"{path}\", \ncannot be used as the Input Folder Path. \nThe path has not been updated.", "Forbidden directory", DialogType.Exclamation);
            return true;
        }
    }

    #endregion

    #region OPERATIONS

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

    private delegate void OperationDelegate(DsssLimeFile dsssFile);

    private enum OperationType
    {
        Packing,
        Unpacking,
        Resigning
    }

    public Task UnpackAllAsync()
        => ProcessAsyncOperation(OperationType.Unpacking, UnpackAll);

    public Task PackAllAsync()
        => ProcessAsyncOperation(OperationType.Packing, PackAll);

    public Task ResignAllAsync()
        => ProcessAsyncOperation(OperationType.Resigning, ResignAll);
    
    private async Task ProcessAsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        if (IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            _logger.Log(LogSeverity.Information, $"{operationType} has started.");
            _logger.Log(LogSeverity.Information, $"Provided Steam32_IDs: INPUT={SteamIdInput}, OUTPUT={SteamIdOutput}");
            _logger.Log(LogSeverity.Information, "ID | FileName | MD5_Checksum | IsEncrypted");
            await AsyncOperation(operationType, operationDelegate);
            _logger.Log(LogSeverity.Information, $"{operationType} complete.");
        }
        catch (OperationCanceledException)
        {
            var message = $"{operationType} was interrupted by the user.";
            _pText.Report(message);
            _logger.Log(LogSeverity.Warning, message);
        }
        AbortOperation();
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
            _pText.Report("The operation was interrupted by the user.");
        }
        AbortOperation();
    }

    private Task BruteforceFirst()
    {
        return Task.Run(() =>
        {
            // check if input directory exists
            if (!DoesDirectoryExists(InputDirectory)) return;

            // get the paths of "*.bin" files located in the input directory
            var files = Directory.GetFiles(InputDirectory, "*.bin", SearchOption.TopDirectoryOnly);

            var dsssFile = new DsssLimeFile(_deencryptor);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            // find first compatible file
            var result = false;
            foreach (var file in files)
            {
                result = dsssFile.SetFileData(file, true).Result;
                if (result) break;
            }
            if (!result)
            {
                _mediator.Inform($"There is no compatible file in the \"{InputDirectory}\" directory.", "Error",
                    DialogType.Error);
                return;
            }

            const uint batches = 100;
            const uint maxValue = uint.MaxValue;
            const uint batchSize = maxValue / batches;

            long start;
            long end = 0;
            BoolResult boolResult = new(false, "Bruteforce attempt has failed!");
            ProcessBatches();
            ReportProgress(boolResult.Description, 100);
            return;
            
            void ProcessBatches()
            {
                for (var i = 0; i < batches; i++)
                {
                    ReportProgress($"[{i}%] Drilling...", i);
                    start = i * batchSize;
                    end = (i + 1) * batchSize - 1;
                    BruteforceBatch();
                    if (boolResult.Result) return;
                }
                // modulo batch
                start = end + 1;
                end = maxValue;
                BruteforceBatch();
            }

            void BruteforceBatch()
            {
                Parallel.For(start, end, po, (ctr, state) =>
                {
                    // bruteforce
                    if (!dsssFile.BruteforceSegment((ulong)ctr)) return;
                    boolResult.Set(true, "The Correct Steam ID has been found!");
                    SteamIdInput = (uint)ctr;
                    state.Break();
                });
            }
        });
    }

    private Task AsyncOperation(OperationType operationType, OperationDelegate operationDelegate)
    {
        return Task.Run(() =>
        {
            // check if input directory exists
            if (!DoesDirectoryExists(InputDirectory)) return;

            // get the paths of "*.bin" files located in the input directory
            var files = Directory.GetFiles(InputDirectory, "*.bin", SearchOption.TopDirectoryOnly);
            
            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var processedFiles = 0;
            var progress = 0;
            ReportProgress($"[{progress}/{files.Length}] Processing files...", progress);

            Parallel.For((long)0, files.Length, po, ctr =>
            {
                // try to load file
                var dsssFile = new DsssLimeFile(_deencryptor);
                var result = dsssFile.SetFileData(files[ctr]);
                if (!result.Result)
                {
                    _logger.Log(LogSeverity.Error, $"I_{ctr} -> Couldn't load the file. {result.Description}");
                    goto ORDER_66;
                }

                // check file compatibility
                var localSteamIdInput = SteamIdInput;
                result = dsssFile.CheckCompatibility(ref localSteamIdInput);
                if (!result.Result)
                {
                    _logger.Log(LogSeverity.Error, $"I_{ctr} -> {result.Description}");
                    goto ORDER_66;
                }

                // log info about the input file
                _logger.LogDebug(LogSeverity.Information, $"I_{ctr} | {Path.GetFileName(files[ctr])} | {Md5HashFromFile(files[ctr])} | {dsssFile.IsEncrypted}");

                // check operation type and adjust to it
                switch (operationType)
                {
                    case OperationType.Resigning:
                        break;
                    case OperationType.Unpacking:
                        if (!dsssFile.IsEncrypted)
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already decrypted.");
                            goto ORDER_66;
                        }
                        break;
                    case OperationType.Packing:
                    default:
                        if (dsssFile.IsEncrypted)
                        {
                            _logger.Log(LogSeverity.Warning, $"I_{ctr} -> The file is already encrypted.");
                            goto ORDER_66;
                        }
                        break;
                }

                // run operation
                if (localSteamIdInput == SteamIdInput)
                {
                    operationDelegate(dsssFile);
                }
                else
                {
                    // temporarily swap SteamIdInput with the known SteamIdInput if it fits the currently processed file
                    var reminder = SteamIdInput;
                    SteamIdInput = localSteamIdInput;
                    operationDelegate(dsssFile);
                    SteamIdInput = reminder;
                }

                // save file
                var filePath = Path.Combine(OutputPath, Path.GetFileName(files[ctr]));
                var fileData = operationType switch
                {
                    OperationType.Unpacking => dsssFile.GetFileSegments(),
                    _ => dsssFile.GetFileData(),
                };
                var writeResult = WriteBytesToFile(filePath, fileData);
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
                _logger.LogDebug(LogSeverity.Information, $"O_{ctr} | {Path.GetFileName(files[ctr])} | {Md5HashFromFile(filePath)} | {dsssFile.IsEncrypted}");

                ORDER_66:
                Interlocked.Increment(ref progress);
                ReportProgress($"[{progress}/{files.Length}] Processing files...", (int)((double)progress / files.Length * 100));
            });

            var message = $"{operationType} done. Number of processed files: {processedFiles}.";
            _logger.Log(LogSeverity.Information, message);
            ReportProgress(message, 100);
        });
    }

    private void UnpackAll(DsssLimeFile dsssFile) 
        => dsssFile.DecryptSegments(SteamIdInput);
    
    private void PackAll(DsssLimeFile dsssFile)
        => dsssFile.EncryptSegments(SteamIdOutput);

    private void ResignAll(DsssLimeFile dsssFile)
    {
        if (dsssFile.IsEncrypted) UnpackAll(dsssFile);
        PackAll(dsssFile);
    }
    
    #endregion
}