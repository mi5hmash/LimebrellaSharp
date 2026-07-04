using LimebrellaSharpCore.Infrastructure;
using LimebrellaSharpCore.Models.DSSS.Lime;
using Mi5hmasH.GameLaunchers.Steam.Types;
using Mi5hmasH.Logger;
using Mi5hmasH.Progress;
using static LimebrellaSharpCore.Helpers.LimeDeencryptor;

namespace LimebrellaSharpCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    /// <summary>
    /// Marks the progress reporting as complete by reporting 100% progress.
    /// </summary>
    /// <param name="progressTracker">The progress tracker used to report progress.</param>
    /// <param name="errorCounter">The error counter used to report errors.</param>
    private void LogAllTasksCompleted(ProgressTracker progressTracker, ErrorCounter errorCounter)
        => logger.LogInfo($"{progressTracker} All tasks completed. {errorCounter}");

    /// <summary>
    /// Asynchronously unpacks and decrypts all encrypted Lime files from the specified input directory for the given user, saving the decrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the encrypted Lime files to be unpacked.</param>
    /// <param name="userId">The user identifier used to derive the Steam account ID for decryption.</param>
    /// <param name="cts">A CancellationTokenSource used to observe cancellation requests and cancel the unpacking operation if needed.</param>
    /// <returns>A task that represents the asynchronous unpacking operation. The task completes when all eligible files have been processed or the operation is canceled.</returns>
    public async Task UnpackFilesAsync(string inputDir, ulong userId, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // UNPACK
        logger.LogInfo($"Unpacking [{progressTracker.Total}] files...");
        // Get Steam Account ID from user ID
        var steamId = new SteamId(userId).AccountId;
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("unpacked").AddUserIdAndSuffix(steamId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progressTracker.Increment();
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"{progressTracker} Trying to unpack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}");
                    continue;
                }
                // Process file data
                var limeFile = new LimeFile();
                await limeFile.SetFileDataAsync(data, true);
                if (!limeFile.IsEncrypted)
                {
                    errorCounter.AddWarning($"{progressTracker} The [{fileName}] file is not encrypted, skipping...");
                    continue;
                }
                // Try to decrypt file data
                try { await limeFile.DecryptSegmentsAsync(steamId, po); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to decrypt the file: {ex.Message}");
                    continue;
                }
                // Check for cancellation
                cts.Token.ThrowIfCancellationRequested();
                // Try to save the decrypted file data
                try
                {
                    var outputFilePath = Path.Combine(outputDir, fileName);
                    var outputData = await limeFile.GetFileSegmentsAsync();
                    await File.WriteAllBytesAsync(outputFilePath, outputData);
                }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker}  Failed to save the file: {ex}");
                    continue;
                }
                logger.LogInfo($"{progressTracker} Decrypted the [{fileName}] file.");
                progressReporter.Report(progressTracker.Percentage);
            }
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }

    /// <summary>
    /// Encrypts and packs all eligible Lime files from the specified input directory for a given user, saving the processed files to a new output directory asynchronously.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the Lime files to be processed.</param>
    /// <param name="userId">The unique identifier of the user whose Steam account ID will be used for encryption.</param>
    /// <param name="cts">A CancellationTokenSource used to observe cancellation requests and abort the operation if cancellation is requested.</param>
    /// <returns>A task that represents the asynchronous packing operation. The task completes when all eligible files have been processed or the operation is canceled.</returns>
    public async Task PackFilesAsync(string inputDir, ulong userId, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // PACK
        logger.LogInfo($"Packing [{progressTracker.Total}] files...");
        // Get Steam Account ID from user ID
        var steamId = new SteamId(userId).AccountId;
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("packed").AddUserIdAndSuffix(steamId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progressTracker.Increment();
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"{progressTracker} Trying to pack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}");
                    continue;
                }
                // Process file data
                var limeFile = new LimeFile();
                await limeFile.SetFileDataAsync(data);
                if (limeFile.IsEncrypted)
                {
                    errorCounter.AddWarning($"{progressTracker} The [{fileName}] file is already encrypted, skipping...");
                    continue;
                }
                // Try to encrypt file data
                try { await limeFile.EncryptSegmentsAsync(steamId, po); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to encrypt the file: {ex.Message}");
                    continue;
                }
                // Check for cancellation
                cts.Token.ThrowIfCancellationRequested();
                // Try to save the encrypted file data
                try
                {
                    var outputFilePath = Path.Combine(outputDir, fileName);
                    var outputData = await limeFile.GetFileDataAsync();
                    await File.WriteAllBytesAsync(outputFilePath, outputData);
                }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to save the file: {ex}");
                    continue;
                }
                logger.LogInfo($"{progressTracker} Encrypted the [{fileName}] file.");
                progressReporter.Report(progressTracker.Percentage);
            }
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }
    
    /// <summary>
    /// Re-signs all encrypted Lime files in the specified directory by decrypting them with the input user ID and re-encrypting them with the output user ID.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the Lime files to be processed.</param>
    /// <param name="userIdInput">The user ID used to decrypt the encrypted segments of each file. Must correspond to the original encryption user.</param>
    /// <param name="userIdOutput">The user ID used to re-encrypt the file segments after decryption. Determines the new ownership of the re-signed files.</param>
    /// <param name="cts">A CancellationTokenSource used to observe cancellation requests during the re-signing process. If cancellation is requested, the operation will terminate early.</param>
    /// <returns>A task that represents the asynchronous re-signing operation. The task completes when all eligible files have been processed or the operation is canceled.</returns>
    public async Task ResignFilesAsync(string inputDir, ulong userIdInput, ulong userIdOutput, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        string[] filesToProcess;
        try { filesToProcess = SaveDataFileIo.GetFiles(inputDir); }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return;
        }
        // INITIALIZE PROGRESS TRACKER
        var progressTracker = new ProgressTracker(filesToProcess.Length);
        var errorCounter = new ErrorCounter(logger);
        // RE-SIGN
        logger.LogInfo($"Resigning [{progressTracker.Total}] files...");
        // Get Steam Account ID from user ID
        var steamIdInput = new SteamId(userIdInput).AccountId;
        var steamIdOutput = new SteamId(userIdOutput).AccountId;
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserIdAndSuffix(steamIdOutput.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progressTracker.Increment();
                // DECRYPT
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"{progressTracker} Trying to unpack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to read the [{fileName}] file: {ex}");
                    continue;
                }
                // Process file data
                var limeFile = new LimeFile();
                await limeFile.SetFileDataAsync(data);
                if (limeFile.IsEncrypted)
                {
                    // Try to decrypt file data
                    try { await limeFile.DecryptSegmentsAsync(steamIdInput, po); }
                    catch (Exception ex)
                    {
                        errorCounter.AddError($"{progressTracker} Failed to decrypt the file: {ex.Message}");
                        continue;
                    }
                    // Check for cancellation
                    cts.Token.ThrowIfCancellationRequested();
                }
                // ENCRYPT
                // Try to encrypt file data
                try { await limeFile.EncryptSegmentsAsync(steamIdOutput, po); }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to encrypt the file: {ex.Message}");
                    continue;
                }
                // Check for cancellation
                cts.Token.ThrowIfCancellationRequested();
                // Try to save the encrypted file data
                try
                {
                    var outputFilePath = Path.Combine(outputDir, fileName);
                    var outputData = await limeFile.GetFileDataAsync();
                    await File.WriteAllBytesAsync(outputFilePath, outputData);
                }
                catch (Exception ex)
                {
                    errorCounter.AddError($"{progressTracker} Failed to save the file: {ex}");
                    continue;
                }
                logger.LogInfo($"{progressTracker} Re-signed the [{fileName}] file.");
                progressReporter.Report(progressTracker.Percentage);
            }
            LogAllTasksCompleted(progressTracker, errorCounter);
        }
        catch (OperationCanceledException ex)
        {
            errorCounter.AddWarning(ex.Message);
        }
        finally
        {
            progressReporter.Complete();
        }
    }
}