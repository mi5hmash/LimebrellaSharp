using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Models;
using LimebrellaSharpCore.Models.DSSS.Lime;
using LimebrellaSharpWinforms.Helpers;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using static LimebrellaSharpCore.Helpers.ISimpleLogger;
using static LimebrellaSharpCore.Helpers.ISimpleMediator;
using static LimebrellaSharpWinforms.Helpers.IoHelpers;

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

    /// <summary>
    /// Calculates MD5 hash from the given <paramref name="data"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string Md5HashFromByteArray(ReadOnlySpan<byte> data)
    {
        var hash = MD5.HashData(data);
        return Convert.ToHexString(hash);
    }

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

    private static readonly int AvailableCpuThreads = Math.Max(Environment.ProcessorCount - 1, 1);
    private CancellationTokenSource _cts = new();
    private readonly LimeDeencryptor _deencryptor = new(AvailableCpuThreads);
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
        _logger = new SimpleLoggerWindows(AppInfo.RootPath)
        {
            AllowDebugMessages = true,
            LoggedAppName = $"{AppInfo.Title} v{AppInfo.Version}",
            MaxLogFiles = 1,
            MinSeverityLevel = LogSeverity.Information,
        };
        _logger.NewLog();
        _progressReporter = progressReporter;

        // Set Aes Encryption Platform
        _aesEncryptionPlatform = GetSupportedAesEncryption();

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
            await _logger.LogAsync(LogSeverity.Information, $"{operationType} has started.");
            await _logger.LogAsync(LogSeverity.Information,
                $"Provided Steam32_IDs: INPUT={SteamIdManager.GetInput()}, OUTPUT={SteamIdManager.GetOutput()}");
            await _logger.LogAsync(LogSeverity.Information, "ID | FileName | MD5_Checksum | IsEncrypted");
            await LimeFileOperationAsync(operationType);
            await _logger.LogAsync(LogSeverity.Information, $"{operationType} complete.");
        }
        catch (OperationCanceledException)
        {
            var message = $"{operationType} was interrupted by the user.";
            _progressReporter.Report(message);
            await _logger.LogAsync(LogSeverity.Warning, message);
        }
        finally
        {
            await _logger.FlushAsync();
        }
        AbortOperation();
    }

    #region AES_ENCRYPTION_PLATFORM

    /// <summary>
    /// Represents the different platforms where AES encryption can be supported.
    /// </summary>
    private readonly AesEncryptionPlatform _aesEncryptionPlatform;

    /// <summary>
    /// Enum for defining the types of platforms supporting AES encryption.
    /// </summary>
    private enum AesEncryptionPlatform
    {
        Hardware,
        Software
    }

    /// <summary>
    /// Determines the supported AES encryption platform. Performs hardware and software checks to identify if AES encryption is supported on the current platform and returns the corresponding platform type.
    /// </summary>
    /// <returns>AesEncryptionPlatform value indicating the supported platform.</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static AesEncryptionPlatform GetSupportedAesEncryption()
    {
        // Hardware check
        if (LimeDeencryptor.IsIntrinsicsSupported())
            return AesEncryptionPlatform.Hardware; 
        if(LimeDeencryptor.IsSoftwareAesSupported())
            return AesEncryptionPlatform.Software;
        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="inputData"/>.
    /// </summary>
    /// <param name="inputData"></param>
    /// <param name="encryptionKey"></param>
    /// <returns>Modified <paramref name="inputData"/></returns>
    private void DeencryptData(Span<byte> inputData, Span<byte> encryptionKey)
    {
        switch (_aesEncryptionPlatform)
        {
            case AesEncryptionPlatform.Hardware:
                var dataAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(inputData);
                var encryptionKeyAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(encryptionKey);
                LimeDeencryptor.DeencryptIntrinsics(dataAsVectors, encryptionKeyAsVectors);
                return;
            default:
            case AesEncryptionPlatform.Software:
                var key = encryptionKey[..16].ToArray();
                var state = encryptionKey[16..];
                LimeDeencryptor.AesDeencryptSoftwareBased(inputData, key, state);
                return;
        }
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="inputData"/> asynchronously.
    /// </summary>
    /// <param name="inputData"></param>
    /// <param name="encryptionKey"></param>
    /// <returns>Modified <paramref name="inputData"/></returns>
    private async Task DeencryptDataAsync(Memory<byte> inputData, Memory<byte> encryptionKey)
    {
        switch (_aesEncryptionPlatform)
        {
            case AesEncryptionPlatform.Hardware:
                await LimeDeencryptor.DeencryptIntrinsicsAsync(inputData, encryptionKey);
                return;
            default:
            case AesEncryptionPlatform.Software:
                await LimeDeencryptor.AesDeencryptSoftwareBasedAsync(inputData, encryptionKey);
                return;
        }
    }

    #endregion

    /// <summary>
    /// Asynchronously performs a Lime file operation.
    /// </summary>
    /// <param name="operationType">The type of operation to perform.</param>
    /// <returns></returns>
    private async Task LimeFileOperationAsync(OperationType operationType)
    {
        // Check if input directory exists
        if (!DoesDirectoryExists(InputPath)) return;

        // Get the paths of "*.bin" files located in the input directory
        var files = Directory.GetFiles(InputPath, $"*.{LimeFile.Extension}", SearchOption.TopDirectoryOnly);
        
        ParallelOptions po = new()
        {
            CancellationToken = _cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1) 
        };

        var processedFiles = 0;
        var progress = 0;
        _progressReporter.Report($"[{progress}/{files.Length}] Processing files...", progress);

        await Parallel.ForAsync(0, files.Length, po, async (i, _) =>
        {
            // try to load file
            var fileDataInput = await ReadBinaryFileAsync(files[i]);
            var fileId = i + 1;
            var dsssFile = new LimeFile(_deencryptor, Path.GetFileNameWithoutExtension(files[i]));
            var boolResult = dsssFile.SetFileData(fileDataInput);
            if (!boolResult.Result)
            {
                await _logger.LogAsync(LogSeverity.Error, $"I_{fileId} -> Couldn't load the file. {boolResult.Description}");
                goto ORDER_66;
            }

            // log info about the input file
            await _logger.LogDebugAsync(LogSeverity.Information, $"I_{fileId} | {dsssFile.FileName} | {Md5HashFromByteArray(fileDataInput)} | {dsssFile.IsEncrypted}");

            // check operation type and adjust to it
            bool result;
            switch (operationType)
            {
                case OperationType.Resigning:
                    if (dsssFile.IsEncrypted)
                    {
                        dsssFile.Key = SteamIdManager.GetInputNumeric();
                        result = await dsssFile.DecryptSegmentsAsync(DeencryptDataAsync);
                        if (!result) break;
                    }
                    dsssFile.Key = SteamIdManager.GetOutputNumeric();
                    result = await dsssFile.EncryptSegmentsAsync(DeencryptDataAsync);
                    break;
                case OperationType.Unpacking:
                    if (!dsssFile.IsEncrypted)
                    {
                        await _logger.LogAsync(LogSeverity.Warning, $"I_{fileId} -> The file is already decrypted.");
                        goto ORDER_66;
                    }
                    dsssFile.Key = SteamIdManager.GetInputNumeric();
                    result = await dsssFile.DecryptSegmentsAsync(DeencryptDataAsync);
                    break;
                case OperationType.Packing:
                default:
                    if (dsssFile.IsEncrypted)
                    {
                        await _logger.LogAsync(LogSeverity.Warning, $"I_{fileId} -> The file is already encrypted.");
                        goto ORDER_66;
                    }
                    dsssFile.Key = SteamIdManager.GetOutputNumeric();
                    result = await dsssFile.EncryptSegmentsAsync(DeencryptDataAsync);
                    break;
            }
            if (!result)
            {
                await _logger.LogAsync(LogSeverity.Warning, $"I_{fileId} -> The provided SteamID does not work for this file.");
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
                    await _cts.CancelAsync();
                    goto ORDER_66;
                default:
                    goto ORDER_66;
            }

            // log info about the output file
            await _logger.LogDebugAsync(LogSeverity.Information, $"O_{fileId} | {dsssFile.FileName} | {Md5HashFromByteArray(fileDataOutput)} | {dsssFile.IsEncrypted}");

        ORDER_66:
            Interlocked.Increment(ref progress);
            _progressReporter.Report($"[{progress}/{files.Length}] Processing files...", (int)((double)progress / files.Length * 100));
        });
        
        var message = $"{operationType} done. Number of processed files: {processedFiles}.";
        await _logger.LogAsync(LogSeverity.Information, message);
        _progressReporter.Report(message, 100);
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
                var bruteforceResult = dsssFile.BruteforceSegment(out var id, DeencryptData, _cts, start, end);
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