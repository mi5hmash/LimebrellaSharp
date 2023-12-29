using LimebrellaSharp.Helpers;
using LimebrellaSharpCore;
using LimebrellaSharpCore.Helpers;
using System.Media;
using static LimebrellaSharpCore.Helpers.SimpleLogger;

namespace LimebrellaSharp;

public partial class Form1 : Form
{
    private readonly Core _programCore;

    public Form1()
    {
        var mediator = new SimpleMediatorWinForms();
        var pText = new Progress<string>(s => toolStripStatusLabel1.Text = s);
        var pValue = new Progress<int>(i => toolStripProgressBar1.Value = i);
        _programCore = new Core(mediator, pText, pValue, new SimpleLogger(new SimpleLoggerOptions(AppInfo.RootPath)
        {
            MaxLogFiles = 1,
            MinSeverityLevel = LogSeverity.Information,
            LoggedAppName = $"{AppInfo.Title} v{AppInfo.Version}"
        }));
        _programCore.ActivateLogger();

        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // set controls
        versionLabel.Text = $@"v{AppInfo.Version}";
        authorLabel.Text = $@"{AppInfo.Author} 2024";
        TBFilepath.Text = AppInfo.RootPath;
        TBSteamIdInput.Text = @"0";
        TBSteamIdOutput.Text = @"0";
    }

    #region STEAM_ID

    private void TBSteamIdInput_Leave(object sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetSteamIdInput(textBox.Text);
        textBox.Text = _programCore.SteamIdInput.ToString();
    }
    private void TBSteamIdOutput_Leave(object sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetSteamIdOutput(textBox.Text);
        textBox.Text = _programCore.SteamIdOutput.ToString();
    }

    private void ButtonChangePlaces_Click(object sender, EventArgs e)
    {
        _programCore.SteamIdInterchange();
        TBSteamIdInput.Text = _programCore.SteamIdInput.ToString();
        TBSteamIdOutput.Text = _programCore.SteamIdOutput.ToString();
    }

    #endregion

    #region INPUT_PATH

    private void ValidateFilepath(object sender)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetInputDirectory(textBox.Text);
        textBox.Text = _programCore.InputDirectory;
        TBSteamIdInput.Text = _programCore.SteamIdInput.ToString();
    }

    private void TBFilepath_Leave(object sender, EventArgs e)
        => ValidateFilepath(sender);

    private void TBFilepath_DragDrop(object sender, DragEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var filePath = filePaths[0];
        if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
            filePath = Path.GetDirectoryName(filePath);
        textBox.Text = filePath;
        ValidateFilepath(textBox);
    }

    private void TBFilepath_DragOver(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.Copy;

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        if (_programCore.IsBusy) return;
        if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
        TBFilepath.Text = folderBrowserDialog1.SelectedPath;
        ValidateFilepath(TBFilepath);
    }

    #endregion

    #region OPERATIONS

    private void ButtonOpenOutputDir_Click(object sender, EventArgs e)
        => Core.OpenOutputDirectory();

    private void ButtonAbort_Click(object sender, EventArgs e)
        => _programCore.AbortOperation();

    private delegate Task OperationDelegate();

    private async Task ProcessAsyncOperation(OperationDelegate operationDelegate)
    {
        if (_programCore.IsBusy) return;

        ButtonAbort.Visible = true;
        await operationDelegate();
        ButtonAbort.Visible = false;

        // play sound
        SoundPlayer sp = new(Properties.Resources.typewritter_machine);
        sp.Play();
    }

    private async void ButtonUnpackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.UnpackAllAsync);

    private async void ButtonPackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.PackAllAsync);

    private async void ButtonResignAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.ResignAllAsync);

    private async void ButtonBruteforceSteamId_Click(object sender, EventArgs e)
    {
        await ProcessAsyncOperation(_programCore.BruteforceSteamIdAsync);
        TBSteamIdInput.Text = _programCore.SteamIdInput.ToString();
    }

    #endregion
}