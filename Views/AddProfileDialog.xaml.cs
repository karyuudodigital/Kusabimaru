using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Collections.Generic;
using SekiroModManager.Models;
using SekiroModManager.Operations;

namespace SekiroModManager.Views;

public partial class AddProfileDialog : Window
{
    public Profile? Profile { get; private set; }

    private readonly ProfileOperations _profileService;
    private readonly ModOperations _modService;
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;
    private readonly Configuration _configService;

    public AddProfileDialog(
        ProfileOperations profileService,
        ModOperations modService,
        FileOperations fileService,
        FileLogger logger,
        Configuration configService)
    {
        InitializeComponent();
        _profileService = profileService;
        _modService = modService;
        _fileService = fileService;
        _logger = logger;
        _configService = configService;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ProfileNameTextBox.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("No name was entered", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Validate name
        if (!Regex.IsMatch(name, @"^[A-Za-z0-9-\s]+$"))
        {
            MessageBox.Show("Name can only contain alphanumeric characters, dashes, and spaces", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Check for duplicate names
        if (_profileService.NameExists(name))
        {
            MessageBox.Show("Name already matches a previously installed profile", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!int.TryParse(ModCountTextBox.Text, out var modCount) || modCount < 1)
        {
            MessageBox.Show("Please enter a valid number of mods (1 or more)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Create profile - the actual mod selection and archive creation will be handled by the caller
        var sekiroDir = _configService.GetSekiroDirectory();
        Profile = new Profile
        {
            Name = name,
            ModCount = modCount,
            Path = Path.Combine("profiles", name),
            ProfileFolder = Path.Combine(sekiroDir, name),
            Files = new List<string>()
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
