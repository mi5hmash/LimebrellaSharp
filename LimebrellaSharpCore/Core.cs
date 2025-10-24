using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Infrastructure;
using Mi5hmasH.Logger;

namespace LimebrellaSharpCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts) 
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    public async Task UnpackFilesAsync(string inputDir, string userId, CancellationTokenSource cts)
        => await Task.Run(() => UnpackFiles(inputDir, userId, cts));

    public void UnpackFiles(string inputDir, string userId, CancellationTokenSource cts)
    {
        // TODO: Implementation goes here
    }

    public async Task PackFilesAsync(string inputDir, string userId, CancellationTokenSource cts)
        => await Task.Run(() => PackFiles(inputDir, userId, cts));

    public void PackFiles(string inputDir, string userId, CancellationTokenSource cts)
    {
        // TODO: Implementation goes here
    }

    public async Task ResignFilesAsync(string inputDir, string userIdInput, string userIdOutput, CancellationTokenSource cts)
        => await Task.Run(() => ResignFiles(inputDir, userIdInput, userIdOutput, cts));

    public void ResignFiles(string inputDir, string userIdInput, string userIdOutput, CancellationTokenSource cts)
    {
        // TODO: Implementation goes here
    }
}