using System;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using SekiroModManager.Models;
using SekiroModManager.Operations;
using SekiroModManager.Views;

namespace SekiroModManager;

public partial class MainWindow : Window
{
    private readonly ModOperations _modService;
    private readonly ProfileOperations _profileService;
    private readonly ModInstallation _installationService;
    private readonly ModEngineOperations _modEngineService;
    private readonly Configuration _configService;
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;
    private AppSettings _settings;

    public MainWindow(
        ModOperations modService,
        ProfileOperations profileService,
        ModInstallation installationService,
        ModEngineOperations modEngineService,
        Configuration configService,
        FileOperations fileService,
        FileLogger logger)
    {
        InitializeComponent();
        
        _modService = modService;
        _profileService = profileService;
        _installationService = installationService;
        _modEngineService = modEngineService;
        _configService = configService;
        _fileService = fileService;
        _logger = logger;

        InitializeApplication();
    }

    private void InitializeApplication()
    {
        // Ensure directories exist
        _fileService.EnsureDirectoryExists("mods");
        _fileService.EnsureDirectoryExists("configs");
        _fileService.EnsureDirectoryExists("profiles");
        _fileService.EnsureDirectoryExists("configsP");

        // Check Sekiro directory
        CheckSekiroDirectory();

        // Load settings
        _settings = _configService.LoadSettings();
        ApplySettings();

        // Load mods and profiles
        _modService.LoadAllMods();
        _profileService.LoadAllProfiles();

        RefreshModsList();
        RefreshProfilesList();

        // Check ModEngine
        CheckModEngine();

        // Get active profile
        GetActiveProfile();
    }

    private void CheckSekiroDirectory()
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (string.IsNullOrEmpty(sekiroDir) || !_fileService.DirectoryContainsSekiroExe(sekiroDir))
        {
            var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Please select your Sekiro installation directory (must contain sekiro.exe)"
            };

            while (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                if (_fileService.DirectoryContainsSekiroExe(dialog.SelectedPath))
                {
                    _configService.SetSekiroDirectory(dialog.SelectedPath);
                    break;
                }
                else
                {
                    MessageBox.Show("The selected directory does not contain sekiro.exe. Please select the correct directory.",
                        "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    private void CheckModEngine()
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (!string.IsNullOrEmpty(sekiroDir) && !_modEngineService.IsModEngineInstalled(sekiroDir))
        {
            var result = MessageBox.Show(
                "ModEngine is not installed. Would you like to install it?",
                "ModEngine Not Found",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _modEngineService.InstallModEngine(sekiroDir);
            }
        }
    }

    private void ApplySettings()
    {
        WarningsCheckBox.IsChecked = _settings.WarningsEnabled;
        LoggingCheckBox.IsChecked = _settings.LoggingEnabled;
        _logger.IsEnabled = _settings.LoggingEnabled;
        CloseOnLaunchCheckBox.IsChecked = _settings.CloseOnLaunch;

        var sekiroDir = _configService.GetSekiroDirectory();
        if (!string.IsNullOrEmpty(sekiroDir))
        {
            var modEngineSettings = _modEngineService.LoadSettings(sekiroDir);
            ChainDllCheckBox.IsChecked = modEngineSettings.ChainDll;
            DebugCheckBox.IsChecked = modEngineSettings.Debug;
            SkipLogosCheckBox.IsChecked = modEngineSettings.SkipLogos;
            CacheFilePathsCheckBox.IsChecked = modEngineSettings.CacheFilePaths;
            LoadUxmFilesCheckBox.IsChecked = modEngineSettings.LoadUxmFiles;
        }
    }

    private void RefreshModsList()
    {
        ModsComboBox.Items.Clear();
        foreach (var mod in _modService.GetAllMods())
        {
            ModsComboBox.Items.Add(mod.Name);
        }
    }

    private void RefreshProfilesList()
    {
        ProfilesComboBox.Items.Clear();
        foreach (var profile in _profileService.GetAllProfiles())
        {
            ProfilesComboBox.Items.Add(profile.Name);
        }
    }

    private void GetActiveProfile()
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (!string.IsNullOrEmpty(sekiroDir))
        {
            var settings = _modEngineService.LoadSettings(sekiroDir);
            var activeProfile = settings.ModOverrideDirectory == "mods" ? "None" : settings.ModOverrideDirectory;
            ActiveProfileLabel.Content = $"Active Profile: {activeProfile}";
        }
    }

    private void AddModButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ModNameDialog(_modService, _fileService, _logger);
        if (dialog.ShowDialog() == true)
        {
            RefreshModsList();
        }
    }

    private void RemoveModButton_Click(object sender, RoutedEventArgs e)
    {
        if (ModsComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a mod to remove.", "No Mod Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var modName = ModsComboBox.SelectedItem.ToString();
        if (MessageBox.Show($"Are you sure you want to remove '{modName}'?", "Confirm Removal", 
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _modService.RemoveMod(modName!);
            RefreshModsList();
        }
    }

    private void InstallModButton_Click(object sender, RoutedEventArgs e)
    {
        if (ModsComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a mod to install.", "No Mod Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var modName = ModsComboBox.SelectedItem.ToString();
        var sekiroDir = _configService.GetSekiroDirectory();

        if (string.IsNullOrEmpty(sekiroDir))
        {
            MessageBox.Show("Sekiro directory is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            if (!_settings.WarningsEnabled)
            {
                var result = MessageBox.Show(
                    "Installing a mod might overwrite a previously installed mod. Do you still want to install this mod?",
                    "Overwrite Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            _installationService.InstallModToSekiro(modName!, sekiroDir);
            RefreshModsList();
            MessageBox.Show($"Mod '{modName}' installed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error installing mod: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UninstallModButton_Click(object sender, RoutedEventArgs e)
    {
        if (ModsComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a mod to uninstall.", "No Mod Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var modName = ModsComboBox.SelectedItem.ToString();
        var mod = _modService.GetModByName(modName!);
        
        if (mod == null || !mod.IsInstalled)
        {
            MessageBox.Show("This mod is not installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var sekiroDir = _configService.GetSekiroDirectory();
            _installationService.UninstallModFromSekiro(modName!, sekiroDir);
            RefreshModsList();
            MessageBox.Show($"Mod '{modName}' uninstalled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error uninstalling mod: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddProfileDialog(_profileService, _modService, _fileService, _logger, _configService);
        if (dialog.ShowDialog() == true)
        {
            RefreshProfilesList();
        }
    }

    private void RemoveProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a profile to remove.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profileName = ProfilesComboBox.SelectedItem.ToString();
        if (MessageBox.Show($"Are you sure you want to remove '{profileName}'?", "Confirm Removal",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _profileService.RemoveProfile(profileName!);
            RefreshProfilesList();
        }
    }

    private void InstallProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a profile to install.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profileName = ProfilesComboBox.SelectedItem.ToString();
        var sekiroDir = _configService.GetSekiroDirectory();

        if (string.IsNullOrEmpty(sekiroDir))
        {
            MessageBox.Show("Sekiro directory is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _installationService.InstallProfileToSekiro(profileName!, sekiroDir);
            RefreshProfilesList();
            MessageBox.Show($"Profile '{profileName}' installed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error installing profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UninstallProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a profile to uninstall.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profileName = ProfilesComboBox.SelectedItem.ToString();
        var profile = _profileService.GetProfileByName(profileName!);

        if (profile == null || !profile.IsInstalled)
        {
            MessageBox.Show("This profile is not installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var sekiroDir = _configService.GetSekiroDirectory();
            _installationService.UninstallProfileFromSekiro(profileName!, sekiroDir);
            RefreshProfilesList();
            MessageBox.Show($"Profile '{profileName}' uninstalled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error uninstalling profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetActiveProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a profile to set as active.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var profileName = ProfilesComboBox.SelectedItem.ToString();
        var sekiroDir = _configService.GetSekiroDirectory();

        if (string.IsNullOrEmpty(sekiroDir))
        {
            MessageBox.Show("Sekiro directory is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _modEngineService.SetActiveProfile(sekiroDir, profileName);
            GetActiveProfile();
            MessageBox.Show($"Profile '{profileName}' set as active.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error setting active profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DefaultProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (string.IsNullOrEmpty(sekiroDir))
        {
            MessageBox.Show("Sekiro directory is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _modEngineService.SetDefaultProfile(sekiroDir);
            GetActiveProfile();
            MessageBox.Show("Default profile set.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error setting default profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ChangeSekiroDirButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WinForms.FolderBrowserDialog
        {
            Description = "Please select your Sekiro installation directory (must contain sekiro.exe)"
        };

        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            if (_fileService.DirectoryContainsSekiroExe(dialog.SelectedPath))
            {
                _configService.SetSekiroDirectory(dialog.SelectedPath);
                MessageBox.Show("Sekiro directory updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("The selected directory does not contain sekiro.exe.", "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void LaunchSekiroButton_Click(object sender, RoutedEventArgs e)
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (string.IsNullOrEmpty(sekiroDir))
        {
            MessageBox.Show("Sekiro directory is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _modEngineService.LaunchSekiro(sekiroDir);
            if (_settings.CloseOnLaunch)
            {
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error launching Sekiro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void WarningsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _settings.WarningsEnabled = true;
        _configService.SaveSettings(_settings);
    }

    private void WarningsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        _settings.WarningsEnabled = false;
        _configService.SaveSettings(_settings);
    }

    private void LoggingCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _settings.LoggingEnabled = true;
        _logger.IsEnabled = true;
        _configService.SaveSettings(_settings);
    }

    private void LoggingCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        _settings.LoggingEnabled = false;
        _logger.IsEnabled = false;
        _configService.SaveSettings(_settings);
    }

    private void CloseOnLaunchCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _settings.CloseOnLaunch = true;
        _configService.SaveSettings(_settings);
    }

    private void CloseOnLaunchCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        _settings.CloseOnLaunch = false;
        _configService.SaveSettings(_settings);
    }

    private void ChainDllCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void ChainDllCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void DebugCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void DebugCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void SkipLogosCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void SkipLogosCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void CacheFilePathsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void CacheFilePathsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void LoadUxmFilesCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void LoadUxmFilesCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SaveModEngineSettings();
    }

    private void SaveModEngineSettings()
    {
        var sekiroDir = _configService.GetSekiroDirectory();
        if (string.IsNullOrEmpty(sekiroDir))
            return;

        var settings = new ModEngineSettings
        {
            ChainDll = ChainDllCheckBox.IsChecked ?? false,
            Debug = DebugCheckBox.IsChecked ?? false,
            SkipLogos = SkipLogosCheckBox.IsChecked ?? false,
            CacheFilePaths = CacheFilePathsCheckBox.IsChecked ?? false,
            LoadUxmFiles = LoadUxmFilesCheckBox.IsChecked ?? false,
            ModOverrideDirectory = _modEngineService.LoadSettings(sekiroDir).ModOverrideDirectory
        };

        _modEngineService.SaveSettings(sekiroDir, settings);
    }

   
}
