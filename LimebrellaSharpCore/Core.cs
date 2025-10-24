using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Infrastructure;
using LimebrellaSharpCore.Models.DSSS.Lime;
using Mi5hmasH.GameLaunchers.Steam.Types;
using Mi5hmasH.Logger;
using static LimebrellaSharpCore.Helpers.LimeDeencryptor;

namespace LimebrellaSharpCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{LimeFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) return;
        // Get Steam Account ID from user ID
        var steamId = new SteamId(userId).AccountId;
        // UNPACK
        logger.LogInfo($"Unpacking [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("unpacked").AddUserIdAndSuffix(steamId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        var progress = 0;
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progress++;
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Trying to unpack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}");
                    continue; // Skip to the next file
                }
                // Process file data
                var limeFile = new LimeFile();
                await limeFile.SetFileDataAsync(data, true);
                if (!limeFile.IsEncrypted)
                {
                    logger.LogWarning($"[{progress}/{filesToProcess.Length}] The [{fileName}] file is not encrypted, skipping...");
                    continue; // Skip to the next file
                }
                // Try to decrypt file data
                try { await limeFile.DecryptSegmentsAsync(steamId, po); }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to decrypt the file: {ex.Message}");
                    continue; // Skip to the next file
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
                    logger.LogError($"Failed to save the file: {ex}");
                    continue; // Skip to the next file
                }
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypted the [{fileName}] file.");
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            }
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{LimeFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) return;
        // Get Steam Account ID from user ID
        var steamId = new SteamId(userId).AccountId;
        // PACK
        logger.LogInfo($"Packing [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("packed").AddUserIdAndSuffix(steamId.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        var progress = 0;
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progress++;
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Trying to pack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}");
                    continue; // Skip to the next file
                }
                // Process file data
                var limeFile = new LimeFile();
                await limeFile.SetFileDataAsync(data);
                if (limeFile.IsEncrypted)
                {
                    logger.LogWarning($"[{progress}/{filesToProcess.Length}] The [{fileName}] file is already encrypted, skipping...");
                    continue; // Skip to the next file
                }
                // Try to encrypt file data
                try { await limeFile.EncryptSegmentsAsync(steamId, po); }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to encrypt the file: {ex.Message}");
                    continue; // Skip to the next file
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
                    logger.LogError($"Failed to save the file: {ex}");
                    continue; // Skip to the next file
                }
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypted the [{fileName}] file.");
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            }
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
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
        var filesToProcess = Directory.GetFiles(inputDir, $"*{LimeFile.FileExtension}", SearchOption.TopDirectoryOnly);
        if (filesToProcess.Length == 0) return;
        // Get Steam Account ID from user ID
        var steamIdInput = new SteamId(userIdInput).AccountId;
        var steamIdOutput = new SteamId(userIdOutput).AccountId;
        // RE-SIGN
        logger.LogInfo($"Resigning [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserIdAndSuffix(steamIdOutput.ToString());
        Directory.CreateDirectory(outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts.Token);
        // Process files
        var progress = 0;
        try
        {
            foreach (var file in filesToProcess)
            {
                // Update progress
                progress++;
                // DECRYPT
                // Try to read file data
                var fileName = Path.GetFileName(file);
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Trying to unpack the [{fileName}] file...");
                byte[] data;
                try { data = await File.ReadAllBytesAsync(file); }
                catch (Exception ex)
                {
                    logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}");
                    continue; // Skip to the next file
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
                        logger.LogError($"Failed to decrypt the file: {ex.Message}");
                        continue; // Skip to the next file
                    }
                    // Check for cancellation
                    cts.Token.ThrowIfCancellationRequested();
                }
                // ENCRYPT
                // Try to encrypt file data
                try { await limeFile.EncryptSegmentsAsync(steamIdOutput, po); }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to encrypt the file: {ex.Message}");
                    continue; // Skip to the next file
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
                    logger.LogError($"Failed to save the file: {ex}");
                    continue; // Skip to the next file
                }
                logger.LogInfo($"[{progress}/{filesToProcess.Length}] Re-signed the [{fileName}] file.");
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            }
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }
}