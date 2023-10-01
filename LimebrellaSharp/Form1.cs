using LimebrellaSharp.Helpers;
using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Models.DSSS.Lime;
using System.Text.RegularExpressions;

namespace LimebrellaSharp;

public partial class Form1 : Form
{
    private CancellationTokenSource _cts = new();
    private bool _isBusy;

    private static readonly string PathPattern = @$"\{Path.DirectorySeparatorChar}(\d+)\{Path.DirectorySeparatorChar}2050650\{Path.DirectorySeparatorChar}remote\{Path.DirectorySeparatorChar}win64_save\{Path.DirectorySeparatorChar}?$";

    private void ExtractSteamIdFromPathIfValid()
    {
        var match = Regex.Match(TBFilepath.Text, PathPattern);
        if (!match.Success) return;
        TBSteamIdLeft.Text = match.Groups[1].Value;
    }

    private void ValidateSteamId()
    {
        TBSteamIdLeft.Text = SteamIdFixer(TBSteamIdLeft.Text);
        TBSteamIdRight.Text = SteamIdFixer(TBSteamIdRight.Text);
        return;

        static string SteamIdFixer(string textBoxText) => ulong.TryParse(textBoxText, out var result) ? ((uint)result).ToString() : "0";
    }

    private void ResetToolStrip()
    {
        toolStripProgressBar1.Value = 0;
        toolStripStatusLabel1.Text = @"Ready";
    }

    private bool DoesInputDirectoryExists()
    {
        if (Directory.Exists(TBFilepath.Text)) return true;
        MessageBox.Show($"""Directory: "{TBFilepath.Text}" does not exists.""", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    private bool WriteBytesToFile(string filePath, Span<byte> fileData)
    {
        do
        {
            if (TryWriteAllBytes(filePath, fileData)) return true;
            // Ask the user if they want to try again
            var dialogResult = MessageBox.Show($"""Failed to save the file: "{filePath}".{Environment.NewLine}It may be currently in use by another program.{Environment.NewLine}Would you like to try again?""", @"Failed to save the file", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.No) return false;
        } while (true);

        bool TryWriteAllBytes(string fPath, Span<byte> bytes)
        {
            try
            {
                File.WriteAllBytes(fPath, bytes.ToArray());
            }
            catch { return false; }
            return true;
        }
    }

    private static void CreateDirectories()
    {
        Directory.CreateDirectory(AppInfo.OutputPath);
    }

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // create directories
        CreateDirectories();

        // set controls
        TBFilepath.Text = AppInfo.RootPath;
        versionLabel.Text = $@"v{AppInfo.Version}";
        authorLabel.Text = $@"{AppInfo.Author} 2023";
    }

    private void ButtonChangePlaces_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        if (TBSteamIdLeft.Text == TBSteamIdRight.Text) return;
        ValidateSteamId();
        (TBSteamIdLeft.Text, TBSteamIdRight.Text) = (TBSteamIdRight.Text, TBSteamIdLeft.Text);
    }

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) TBFilepath.Text = folderBrowserDialog1.SelectedPath;
    }

    private void TBFilepath_TextChanged(object sender, EventArgs e)
    {
        ResetToolStrip();
        ExtractSteamIdFromPathIfValid();
    }

    private void TBFilepath_DragDrop(object sender, DragEventArgs e)
    {
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var filePath = filePaths[0];
        if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
            filePath = Path.GetDirectoryName(filePath);
        TBFilepath.Text = filePath;
    }

    private void TBFilepath_DragOver(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.Copy;

    private void ButtonOpenOutputDir_Click(object sender, EventArgs e)
        => IoHelpers.OpenDirectory(AppInfo.OutputPath);

    private void ButtonAbort_Click(object sender, EventArgs e) => AbortOperation();
    private void AbortOperation()
    {
        _cts.Cancel();
        _cts.Dispose();
        ButtonAbort.Visible = false;
        _isBusy = false;
    }

    private async void ButtonUnpackAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        ValidateSteamId();
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await UnpackAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task UnpackAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var limeDeencryptor = new LimeDeencryptor();

            var steamId = ulong.TryParse(TBSteamIdLeft.Text, out var result) ? result : 0;
            var unpackedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, (ctr) =>
            {
                // load file
                var dsssFile = new DsssLimeFile();
                var resultRead = dsssFile.SetFileData(files[ctr]);
                if (!resultRead.Result) goto ORDER_66;

                // decrypt file
                if (!limeDeencryptor.Limetree(dsssFile, steamId)) return;

                // save file
                var filePath = Path.Combine(AppInfo.OutputPath, Path.GetFileName(files[ctr]));
                var fileSegments = dsssFile.GetFileSegments();
                var writeResult = WriteBytesToFile(filePath, fileSegments);

                if (writeResult) unpackedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));

            });
            pText.Report($@"Unpacking done. Number of unpacked files: {unpackedFiles}.");
            pPercentage.Report(100);
        });
    }

    private async void ButtonPackAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        ValidateSteamId();
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await PackAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task PackAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var limeDeencryptor = new LimeDeencryptor();

            var steamIdRight = ulong.TryParse(TBSteamIdRight.Text, out var result) ? result : 0;
            var packedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, ctr =>
            {
                // load file
                var dsssFile = new DsssLimeFile();
                var resultRead = dsssFile.SetFileSegments(files[ctr]);
                if (!resultRead.Result) goto ORDER_66;

                // encrypt
                limeDeencryptor.Limetree(dsssFile, steamIdRight, true);

                // save file
                var filePath = Path.Combine(AppInfo.OutputPath, Path.GetFileName(files[ctr]));
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(filePath, fileData);

                if (writeResult) packedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));

            });
            pText.Report($@"Packing done. Number of packed files: {packedFiles}.");
            pPercentage.Report(100);
        });
    }

    private async void ButtonResignAll_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        ValidateSteamId();
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _cts = new CancellationTokenSource();
        ButtonAbort.Visible = true;

        try
        {
            await ResignAll(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task ResignAll(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var limeDeencryptor = new LimeDeencryptor();

            var steamIdLeft = ulong.TryParse(TBSteamIdLeft.Text, out var result) ? result : 0;
            var steamIdRight = ulong.TryParse(TBSteamIdRight.Text, out result) ? result : 0;
            var resignedFiles = 0;

            var files = Directory.GetFiles(TBFilepath.Text);

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var progress = 0;
            pText.Report($@"[{progress}/{files.Length}] Processing files...");
            pPercentage.Report(progress);
            Parallel.For((long)0, files.Length, po, ctr =>
            {
                // load file
                var dsssFile = new DsssLimeFile();
                var resultRead = dsssFile.SetFileData(files[ctr]);
                if (!resultRead.Result) goto ORDER_66;

                // decrypt
                if (!limeDeencryptor.Limetree(dsssFile, steamIdLeft)) return;

                // encrypt
                limeDeencryptor.Limetree(dsssFile, steamIdRight, true);

                // save file
                var filePath = Path.Combine(AppInfo.OutputPath, Path.GetFileName(files[ctr]));
                var fileData = dsssFile.GetFileData();
                var writeResult = WriteBytesToFile(filePath, fileData);

                if (writeResult) resignedFiles++;
                ORDER_66:
                Interlocked.Increment(ref progress);
                pText.Report($@"[{progress}/{files.Length}] Processing files...");
                pPercentage.Report((int)((double)progress / files.Length * 100));
            });
            pText.Report($@"Resigning done. Number of resigned files: {resignedFiles}.");
            pPercentage.Report(100);
        });
    }

    private async void ButtonBruteforceSteamID_Click(object sender, EventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        ValidateSteamId();
        CreateDirectories();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pPercentage = new Progress<int>(i => toolStripProgressBar1.Value = i);
        ButtonAbort.Visible = true;

        try
        {
            await BruteforceSteamId(pText, pPercentage);
        }
        catch (OperationCanceledException)
        {
            toolStripStatusLabel1.Text = @"The operation was aborted by the user.";
        }
        AbortOperation();
    }
    private Task BruteforceSteamId(IProgress<string> pText, IProgress<int> pPercentage)
    {
        return Task.Run(() =>
        {
            if (!DoesInputDirectoryExists()) return;

            var limeDeencryptor = new LimeDeencryptor();
            var files = Directory.GetFiles(TBFilepath.Text);

            // Try to load first compatible file.
            DsssLimeFile dsssFile = new();
            var test = false;
            foreach (var file in files)
            {
                test = dsssFile.SetFileData(file).Result;
                if (!test) continue;
                break;
            }
            if (!test)
            {
                MessageBox.Show($"""There is no compatible file in the "{TBFilepath.Text}" directory.""", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ParallelOptions po = new()
            {
                CancellationToken = _cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var progress = 0;
            pText.Report($@"[{progress}/100] Drilling...");
            pPercentage.Report(progress);
            var resultText = "Bruteforce attempt has failed!";

            Parallel.For(0, int.MaxValue, po, (ctr, state) =>
            {
                // bruteforce
                if (limeDeencryptor.BruteforceLime(dsssFile.Segments[0], (ulong)ctr))
                {
                    resultText = $"Correct Steam ID has been found! {ctr}";
                    TBSteamIdLeft.Text = ctr.ToString();
                    state.Break();
                }

                Interlocked.Increment(ref progress);
                if (progress % 20000000 != 0) return;
                pText.Report($@"[{(int)((double)progress / int.MaxValue * 100)}%] Drilling...");
                pPercentage.Report((int)((double)progress / int.MaxValue * 100));
            });
            pText.Report(resultText);
            pPercentage.Report(100);
        });
    }
}