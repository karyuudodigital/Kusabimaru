using System.Text.RegularExpressions;
using System.Windows;
using SekiroModManager.Operations;

namespace SekiroModManager.Views;

public partial class ModNameDialog : Window
{
    public string? ModName { get; private set; }
    public bool IsModPack { get; private set; }

    private readonly ModOperations _modService;
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;
    private string _modpackName = string.Empty;

    public ModNameDialog(ModOperations modService, FileOperations fileService, FileLogger logger)
    {
        InitializeComponent();
        _modService = modService;
        _fileService = fileService;
        _logger = logger;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ModNameTextBox.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("No name was entered", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Validate name (alphanumeric, dash, space only)
        if (!Regex.IsMatch(name, @"^[A-Za-z0-9-\s]+$"))
        {
            MessageBox.Show("Name can only contain alphanumeric characters, dashes, and spaces", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Check for duplicate names
        if (_modService.NameExists(name))
        {
            MessageBox.Show("Name already matches a previously installed mod", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Check modpack name if applicable
        if (IsModPack && !string.IsNullOrEmpty(_modpackName))
        {
            var fullName = $"{name}({_modpackName})";
            if (_modService.NameExists(fullName))
            {
                MessageBox.Show("Name already matches a previously installed mod", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        ModName = name;
        DialogResult = true;
        Close();
    }

    private void IsModPackCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        IsModPack = true;
        ModNameLabel.Content = "Please enter the name of the modpack";
    }

    private void IsModPackCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        IsModPack = false;
        ModNameLabel.Content = "Please enter the name of the mod";
    }

    public void SetModpackName(string modpackName)
    {
        _modpackName = modpackName;
        IsModPackCheckBox.IsEnabled = false;
        IsModPackCheckBox.Visibility = Visibility.Collapsed;
    }
}
