using System.Windows;
using System.Windows.Controls;

namespace SekiroModManager.Views;

public partial class RepeatFileDialog : Window
{
    public string? SelectedFolder { get; private set; }

    public RepeatFileDialog(string fileName, RepeatFileType type)
    {
        InitializeComponent();
        FileNameLabel.Content = $"{fileName} found";
        PopulateFolders(type);
    }

    private void PopulateFolders(RepeatFileType type)
    {
        FolderComboBox.Items.Clear();

        switch (type)
        {
            case RepeatFileType.Font:
                // Add font folders
                var fontFolders = new[] { "dandk_map", "dandk_std", "dandk_texteffect", "deude_map", "deude_std", 
                    "deude_texteffect", "enggb_map", "enggb_std", "enggb_texteffect", "engus_map", "engus_std",
                    "engus_texteffect", "finfi_map", "finfi_std", "finfi_texteffect", "frafr_map", "frafr_std",
                    "frafr_texteffect", "itait_map", "itait_std", "itait_texteffect", "jpnjp_map", "jpnjp_std",
                    "jpnjp_texteffect", "korkr_map", "korkr_std", "korkr_texteffect", "nldnl_map", "nldnl_std",
                    "nldnl_texteffect", "norno_map", "norno_std", "norno_texteffect", "polpl_map", "polpl_std",
                    "polpl_texteffect", "porbr_map", "porbr_std", "porbr_texteffect", "porpt_map", "porpt_std",
                    "porpt_texteffect", "rusru_map", "rusru_std", "rusru_texteffect", "spaar_map", "spaar_std",
                    "spaar_texteffect", "spaes_map", "spaes_std", "spaes_texteffect", "swese_map", "swese_std",
                    "swese_texteffect", "thath_map", "thath_std", "thath_texteffect", "turtr_map", "turtr_std",
                    "turtr_texteffect", "zhocn_map", "zhocn_std", "zhocn_texteffect", "zhotw_map", "zhotw_std",
                    "zhotw_texteffect" };
                foreach (var folder in fontFolders)
                {
                    FolderComboBox.Items.Add(folder);
                }
                break;
            case RepeatFileType.Region:
                FolderComboBox.Items.Add("na");
                FolderComboBox.Items.Add("uk");
                FolderComboBox.Items.Add("jp");
                FolderComboBox.Items.Add("as");
                FolderComboBox.Items.Add("eu");
                break;
            case RepeatFileType.Language:
                var languages = new[] { "deude", "engus", "frafr", "itait", "japanese", "jpnjp", "korkr", 
                    "polpl", "porbr", "rusru", "spaar", "spaes", "thath", "zhocn", "zhotw" };
                foreach (var lang in languages)
                {
                    FolderComboBox.Items.Add(lang);
                }
                break;
            case RepeatFileType.Quality:
                FolderComboBox.Items.Add("hi");
                FolderComboBox.Items.Add("low");
                break;
            case RepeatFileType.MapImage:
                FolderComboBox.Items.Add("mapimage(hi)");
                FolderComboBox.Items.Add("mapimage(low)");
                break;
        }
    }

    private void FolderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FolderComboBox.SelectedItem != null)
        {
            SelectedFolder = FolderComboBox.SelectedItem.ToString();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (FolderComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a folder", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedFolder = FolderComboBox.SelectedItem.ToString();
        DialogResult = true;
        Close();
    }
}

public enum RepeatFileType
{
    Font = 0,
    Region = 1,
    Language = 2,
    Quality = 3,
    MapImage = 4
}
