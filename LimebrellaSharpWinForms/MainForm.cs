using System.Media;
using LimebrellaSharpCore.Helpers;
using LimebrellaSharpWinforms.Helpers;
using static LimebrellaSharpWinforms.Core;

namespace LimebrellaSharpWinforms;

public partial class MainForm : Form
{
    #region SOUNDS

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
                SoundPlayer sp = new(LimebrellaSharp.Properties.Resources.typewritter_machine);
                sp.Play();
                break;
            case SoundsEnum.None:
            default:
                break;
        }
    }

    #endregion

    #region SUPER_USER

    // Super User
    private const int SuperUserThreshold = 3;
    private bool _isSuperUser;
    private int _superUserClicks;

    private void SuperUserTrigger_Click(object sender, EventArgs e)
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
        // play sound
        PlaySound(SoundsEnum.System);
    }

    #endregion
    
    #region CONSTRUCTOR

    private readonly Core _core;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainForm()
    {
        var progressReporter = new ProgressReporter(
            new Progress<string>(s => toolStripStatusLabel1.Text = s),
            new Progress<int>(i => toolStripProgressBar1.Value = i)
        );
        _core = new Core(progressReporter);
        
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

        // transparent SuperUserTrigger hack
        versionLabel.Controls.Add(superUserTrigger);
        superUserTrigger.Size = versionLabel.Size;
        superUserTrigger.Location = new Point(0, 0);
    }

    #endregion

    #region OPERATIONS

    private void TBSteamIdInput_Leave(object sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _core.SteamIdManager.SetInput(textBox.Text);
        textBox.Text = _core.SteamIdManager.GetInput();
    }

    private void TBSteamIdOutput_Leave(object sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _core.SteamIdManager.SetOutput(textBox.Text);
        textBox.Text = _core.SteamIdManager.GetOutput();
    }

    private void ButtonChangePlaces_Click(object sender, EventArgs e)
    {
        if (_core.IsBusy) return;

        _core.SteamIdManager.SteamIdInterchange();
        TBSteamIdInput.Text = _core.SteamIdManager.GetInput();
        TBSteamIdOutput.Text = _core.SteamIdManager.GetOutput();
    }
    
    private void ValidatePath(object sender)
    {
        if (sender is not TextBox textBox) return;
        _core.SetInputPath(textBox.Text);
        textBox.Text = _core.InputPath;
        TBSteamIdInput.Text = _core.SteamIdManager.GetInput();
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
        if (_core.IsBusy) return;
        if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
        TBFilepath.Text = folderBrowserDialog1.SelectedPath;
        ValidatePath(TBFilepath);
    }

    private void AuthorLabel_Click(object sender, EventArgs e)
        => AppInfo.VisitAuthorsGithub();

    private void ButtonOpenOutputDir_Click(object sender, EventArgs e)
        => OpenOutputDirectory();

    private void ButtonAbort_Click(object sender, EventArgs e)
        => _core.AbortOperation();

    private delegate Task ClickOperationDelegate();
    private async Task ProcessAsyncClickOperation(ClickOperationDelegate operationDelegate, SoundsEnum sound = SoundsEnum.None, bool isLongOperation = false)
    {
        if (_core.IsBusy) return;

        if (isLongOperation) ButtonAbort.Visible = true;
        await operationDelegate();
        if (isLongOperation) ButtonAbort.Visible = false;

        // play sound
        PlaySound(sound);
    }

    private async void ButtonResignAll_Click(object sender, EventArgs e)
        => await ProcessAsyncClickOperation(_core.ResignAllAsync, SoundsEnum.Typewritter, true);

    private async void ButtonUnpackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncClickOperation(_core.UnpackAllAsync, SoundsEnum.Typewritter, true);

    private async void ButtonPackAll_Click(object sender, EventArgs e)
        => await ProcessAsyncClickOperation(_core.PackAllAsync, SoundsEnum.Typewritter, true);

    private async void ButtonBruteforceSteamId_Click(object sender, EventArgs e)
    {
        await ProcessAsyncClickOperation(_core.BruteforceSteamIdAsync, SoundsEnum.System, true);
        TBSteamIdInput.Text = _core.SteamIdManager.GetInput();
    }

    #endregion

}