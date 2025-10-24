using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimebrellaSharpCore;
using LimebrellaSharpCore.Helpers;
using LimebrellaSharpCore.Infrastructure;
using LimebrellaSharpWpf.Fonts;
using LimebrellaSharpWpf.Helpers;
using LimebrellaSharpWpf.Settings;
using Mi5hmasH.AppInfo;
using Mi5hmasH.AppSettings;
using Mi5hmasH.AppSettings.Flavors;
using Mi5hmasH.Logger;
using Mi5hmasH.Logger.Models;
using Mi5hmasH.Logger.Providers;
using Microsoft.Win32;
using System.Collections.Specialized;
using System.IO;
using System.Media;

namespace LimebrellaSharpWpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region APP_INFO
    public readonly MyAppInfo AppInfo = new("Limebrella Sharp");
    public string AppTitle => AppInfo.Name;
    public static string AppAuthor => MyAppInfo.Author;
    public static string AppVersion => $"v{MyAppInfo.Version}";

    [RelayCommand] private static void VisitAuthorsGithub() => Urls.OpenAuthorsGithub();
    [RelayCommand] private static void VisitProjectsRepo() => Urls.OpenProjectsRepo();
    #endregion

    #region ICONS
    public static string PackIcon => IconFont.Import;
    public static string UnpackIcon => IconFont.Export;
    public static string FolderIcon => IconFont.Folder;
    public static string FolderSymlinkIcon => IconFont.FolderSymlink;
    public static string GithubIcon => IconFont.Github;
    public static string InterchangeIcon => IconFont.Interchange;
    public static string ResignIcon => IconFont.Resign;
    public static string XCircleIcon => IconFont.XCircle;
    #endregion

    #region UI_STATE
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isAbortAllowed;
    #endregion

    #region PROGRESS_REPORTER
    [ObservableProperty] private int _progressValue;
    [ObservableProperty] private string _progressText = "Loading...";
    private readonly ProgressReporter _progressReporter;
    #endregion

    #region LOGGER
    private readonly SimpleLogger _logger;
    private void InitializeLogger()
    {
        // Configure StatusBarLogProvider
        var statusBarLogProvider = new StatusBarLogProvider(_progressReporter.Report);
        _logger.AddProvider(statusBarLogProvider);
        // Configure FileLogProvider
        var fileLogProvider = new FileLogProvider(MyAppInfo.RootPath, 2);
        fileLogProvider.CreateLogFile();
        _logger.AddProvider(fileLogProvider);
        // Add event handler for unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is not Exception exception) return;
            var logEntry = new LogEntry(SimpleLogger.LogSeverity.Critical, $"Unhandled Exception: {exception}");
            fileLogProvider.Log(logEntry);
            fileLogProvider.Flush();
        };
        // Flush log providers on process exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) => _logger.Flush();
    }
    #endregion

    #region STEAM_ID
    [ObservableProperty] private string _steamIdInput = string.Empty;
    [ObservableProperty] private string _steamIdOutput = string.Empty;

    [RelayCommand]
    private void SwapSteamIds()
    {
        (SteamIdInput, SteamIdOutput) = (SteamIdOutput, SteamIdInput);
        _progressReporter.Report("Steam IDs has been swapped.");
    }

    // TODO: Use this method when implementing drag-and-drop for InputFolderPath
    private void ExtractSteamIdFromFilePath()
    {
        var userId = InputFolderPath.ExtractSteamId();
        if (userId != string.Empty) SteamIdInput = userId;
    }
    #endregion

    #region INPUT_FOLDER_PATH
    [ObservableProperty] private string _inputFolderPath = MyAppInfo.RootPath;

    partial void OnInputFolderPathChanged(string value)
    {
        if (Directory.Exists(value)) return;
        if (File.Exists(value))
        {
            _inputFolderPath = Path.GetDirectoryName(value) ?? string.Empty;
            _progressReporter.Report("Input Folder Path is valid.");
            return;
        }
        _progressReporter.Report("Invalid Input Folder Path!");
        _inputFolderPath = string.Empty;
    }

    [RelayCommand]
    private void SelectInputFolderPath()
    {
        OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = InputFolderPath,
            Filter = "Binary Files (*.bin)|*.bin",
            FilterIndex = 1
        };
        if (openFileDialog.ShowDialog() == true) InputFolderPath = openFileDialog.FileName;
    }
    #endregion

    #region OUTPUT_FOLDER_PATH
    [RelayCommand]
    private static void OpenOutputDirectory()
        => Directories.OpenDirectory(Directories.Output);
    #endregion

    #region APP_SETTINGS
    private readonly AppSettingsManager<MyAppSettings, Json> _appSettingsManager;
    private const string SettingsMagic = "2VprD3uJ6vA0TmAn3AYEHzuWlrDuySjIU0eNTnUPKfA=";
    private void InitializeSettings()
    {
        _appSettingsManager.SetEncryptor(SettingsMagic);
        try { _appSettingsManager.Load(); }
        catch
        {
            // ignore
        }
        // Apply loaded settings
        LoadAppSettings();
        // Save settings on exit
        AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveAppSettings();
    }
    private void LoadAppSettings()
    {
        SteamIdInput = _appSettingsManager.Settings.SteamIdInput;
        SteamIdOutput = _appSettingsManager.Settings.SteamIdOutput;
        SuperUserManager.IsSuperUser = _appSettingsManager.Settings.IsSu;
    }
    private void SaveAppSettings()
    {
        _appSettingsManager.Settings.SteamIdInput = SteamIdInput;
        _appSettingsManager.Settings.SteamIdOutput = SteamIdOutput;
        _appSettingsManager.Settings.IsSu = SuperUserManager.IsSuperUser;
        _appSettingsManager.Save();
    }
    #endregion

    #region FILE_DROP
    public void OnFileDrop(string operationType, StringCollection filePaths)
    {
        if (filePaths.Count < 1) return;
        if (operationType == "GetInputPath") InputFolderPath = filePaths[0] ?? string.Empty;
    }
    #endregion

    private CancellationTokenSource _cts = new();
    private readonly Core _core;
    [ObservableProperty] private SuperUserManager _superUserManager;

    public MainWindowViewModel()
    {
        // Initialize ProgressReporter
        _progressReporter = new ProgressReporter(
            new Progress<string>(s => ProgressText = s),
            new Progress<int>(i => ProgressValue = i)
        );
        // Initialize Logger
        _logger = new SimpleLogger
        {
            LoggedAppName = AppInfo.Name
        };
        InitializeLogger();
        // Initialize Core
        _core = new Core(_logger, _progressReporter);
        // Initialize SuperUserManager
        SuperUserManager = new SuperUserManager(_progressReporter);
        // Initialize AppSettings
        _appSettingsManager = new AppSettingsManager<MyAppSettings, Json>(null, MyAppInfo.RootPath);
        InitializeSettings();
        // Finalize setup
        _progressReporter.Report("Ready", 100);
    }

    #region ACTIONS

    [RelayCommand]
    public void AbortAction()
    {
        if (!IsAbortAllowed || !IsBusy) return;
        _cts.Cancel();
    }

    private async Task PerformAction(Func<Task> function, bool canBeAborted = false)
    {
        if (IsBusy) return;
        IsBusy = true;
        if (canBeAborted) IsAbortAllowed = true;
        try
        {
            await function();
        }
        finally
        {
            // play sound
            if (_cts.IsCancellationRequested)
                SystemSounds.Beep.Play();
            else
            {
                using var sp = new SoundPlayer(Properties.Resources.typewriter_machine);
                sp.Play();
            }
            // reset flags
            if (canBeAborted) IsAbortAllowed = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UnpackAllAsync()
    {
        _cts = new CancellationTokenSource();
        await PerformAction(() => _core.UnpackFilesAsync(InputFolderPath, SteamIdInput, _cts), true);
        _cts.Dispose();
    }

    [RelayCommand]
    private async Task PackAllAsync()
    {
        _cts = new CancellationTokenSource();
        await PerformAction(() => _core.PackFilesAsync(InputFolderPath, SteamIdOutput, _cts), true);
        _cts.Dispose();
    }

    [RelayCommand]
    private async Task ResignAllAsync()
    {
        _cts = new CancellationTokenSource();
        await PerformAction(() => _core.ResignFilesAsync(InputFolderPath, SteamIdInput, SteamIdOutput, _cts), true);
        _cts.Dispose();
    }

    #endregion
}