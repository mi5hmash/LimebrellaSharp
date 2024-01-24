using LimebrellaSharp.Helpers;
using System.Media;
using LimebrellaSharpCore;
using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpCore.Helpers.SimpleLogger;

namespace LimebrellaSharp;

public partial class Form1 : Form
{
    // Program Core
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

    /// <summary>
    /// Enumeration of all available Sounds.
    /// </summary>
    private enum SoundsEnum
    {
        None,
        System,
        Typewritter
    }

    private static void PlaySound(SoundsEnum sound)
    {
        switch (sound)
        {
            case SoundsEnum.System:
                SystemSounds.Beep.Play();
                break;
            case SoundsEnum.Typewritter:
                SoundPlayer sp = new(Properties.Resources.typewritter_machine);
                sp.Play();
                break;
            case SoundsEnum.None:
            default:
                break;
        }
    }

    #region SUPER_USER

    // Super User
    private const int SuperUserThreshold = 3;
    private bool _isSuperUser;
    private int _superUserClicks;

    private void VersionLabel_Click(object sender, EventArgs e)
    {
        if (_isSuperUser) return;

        _superUserClicks += 1;

        if (_superUserClicks >= SuperUserThreshold) return;

        // restart superUserTimer
        superUserTimer.Stop();
        superUserTimer.Start();
    }

    private void SuperUserTimer_Tick(object sender, EventArgs e)
    {
        superUserTimer.Stop();
        if (_superUserClicks >= SuperUserThreshold) EnableSuperUser();
        _superUserClicks = 0;
    }

    private void EnableSuperUser()
    {
        _isSuperUser = true;
        // things to unlock
        ButtonPackAll.Visible = true;
        ButtonUnpackAll.Visible = true;
        ButtonBruteforceSteamId.Visible = true;
        // play sound
        PlaySound(SoundsEnum.System);
    }

    #endregion

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

    private async void ButtonBruteforceSteamId_Click(object sender, EventArgs e)
    {
        await ProcessAsyncOperation(_programCore.BruteforceSteamIdAsync, SoundsEnum.System, true);
        TBSteamIdInput.Text = _programCore.SteamIdInput.ToString();
    }

    #endregion

    #region INPUT_PATH

    private void ValidatePath(object sender)
    {
        if (sender is not TextBox textBox) return;
        _programCore.SetInputDirectory(textBox.Text);
        textBox.Text = _programCore.InputDirectory;
        TBSteamIdInput.Text = _programCore.SteamIdInput.ToString();
    }

    private void TBFilepath_Leave(object sender, EventArgs e)
        => ValidatePath(sender);

    private void TBFilepath_DragDrop(object sender, DragEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!e.Data!.GetDataPresent(DataFormats.FileDrop)) return;
        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var filePath = filePaths[0];
        if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
            filePath = Path.GetDirectoryName(filePath);
        textBox.Text = filePath;
        ValidatePath(textBox);
    }

    private void TBFilepath_DragOver(object sender, DragEventArgs e)
        => e.Effect = DragDropEffects.Copy;

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        if (_programCore.IsBusy) return;
        if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
        TBFilepath.Text = folderBrowserDialog1.SelectedPath;
        ValidatePath(TBFilepath);
    }

    #endregion

    #region OPERATIONS

    private void AuthorLabel_Click(object sender, EventArgs e)
        => AppInfo.VisitAuthorsGithub();

    private void ButtonOpenOutputDir_Click(object sender, EventArgs e)
        => Core.OpenOutputDirectory();

    private void ButtonAbort_Click(object sender, EventArgs e)
        => _programCore.AbortOperation();

    private delegate Task OperationDelegate();
    
    private async Task ProcessAsyncOperation(OperationDelegate operationDelegate, SoundsEnum sound = SoundsEnum.None, bool isLongOperation = false)
    {
        if (_programCore.IsBusy) return;

        if (isLongOperation) ButtonAbort.Visible = true;
        await operationDelegate();
        if (isLongOperation) ButtonAbort.Visible = false;

        // play sound
        PlaySound(sound);
    }

    private async void ButtonUnpackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.UnpackAllAsync, SoundsEnum.Typewritter, true);

    private async void ButtonPackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.PackAllAsync, SoundsEnum.Typewritter, true);

    private async void ButtonResignAll_Click(object sender, EventArgs e)
        => await ProcessAsyncOperation(_programCore.ResignAllAsync, SoundsEnum.Typewritter, true);
    
    #endregion

}