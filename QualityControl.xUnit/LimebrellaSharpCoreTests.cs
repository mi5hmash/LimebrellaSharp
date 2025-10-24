using LimebrellaSharpCore;
using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Models.DSSS.Lime;
using Mi5hmasH.GameLaunchers.Steam.Types;
using Mi5hmasH.Logger;

namespace QualityControl.xUnit;

public sealed class LimebrellaSharpCoreTests : IDisposable
{
    private readonly Core _core;
    private readonly ITestOutputHelper _output;
    
    public LimebrellaSharpCoreTests(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("SETUP");

        // Setup
        var logger = new SimpleLogger();
        var progressReporter = new ProgressReporter(null, null);
        _core = new Core(logger, progressReporter);
    }

    public void Dispose()
    {
        _output.WriteLine("CLEANUP");
    }
    
    [Fact]
    public async Task DecryptFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.UnpackFilesAsync(tempDir, 0, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task EncryptFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.PackFilesAsync(tempDir, 0, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task ResignFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.ResignFilesAsync(tempDir, 0, 1, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task DecryptFiles_DoesDecrypt()
    {
        // Arrange
        const string userId = "76561197960265729";
        var steamId = new SteamId(userId).AccountId;
        var limeFile = new LimeFile();
        await limeFile.SetFileDataAsync(Properties.Resources.encryptedFile);

        // Act
        await limeFile.DecryptSegmentsAsync(steamId);
        var resultData = await limeFile.GetFileSegmentsAsync();

        // Assert
        Assert.Equal((ReadOnlySpan<byte>)resultData, Properties.Resources.decryptedFile);
    }

    [Fact]
    public async Task EncryptFiles_DoesEncrypt()
    {
        // Arrange
        const string userId = "76561197960265729";
        var steamId = new SteamId(userId).AccountId;
        var limeFile = new LimeFile();
        await limeFile.SetFileDataAsync(Properties.Resources.decryptedFile);

        // Act
        await limeFile.EncryptSegmentsAsync(steamId);
        await limeFile.DecryptSegmentsAsync(steamId);
        var resultData = await limeFile.GetFileSegmentsAsync();

        // Assert
        Assert.Equal((ReadOnlySpan<byte>)resultData, Properties.Resources.decryptedFile);
    }
}