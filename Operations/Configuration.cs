using System.IO;
using System.Text;
using SekiroModManager.Models;

namespace SekiroModManager.Operations;

public class Configuration
{
    private const string DirIniPath = "dir.ini";
    private const string SettingsIniPath = "settings.ini";
    private readonly FileLogger _logger;

    public Configuration(FileLogger logger)
    {
        _logger = logger;
    }

    public AppSettings LoadSettings()
    {
        var settings = new AppSettings();

        if (File.Exists(SettingsIniPath))
        {
            var lines = File.ReadAllLines(SettingsIniPath);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "Warnings":
                            settings.WarningsEnabled = value == "1";
                            break;
                        case "Logging":
                            settings.LoggingEnabled = value == "1";
                            break;
                        case "CloseOnLaunch":
                            settings.CloseOnLaunch = value == "1";
                            break;
                        case "ReinstallAfterDirChange":
                            settings.ReinstallAfterDirChange = value == "1";
                            break;
                        case "KeepModengineSettings":
                            settings.KeepModengineSettings = value == "1";
                            break;
                        case "ActiveProfile":
                            settings.ActiveProfile = value;
                            break;
                    }
                }
            }
        }

        settings.SekiroDirectory = GetSekiroDirectory();
        return settings;
    }

    public void SaveSettings(AppSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Warnings={(settings.WarningsEnabled ? 1 : 0)}");
        sb.AppendLine($"Logging={(settings.LoggingEnabled ? 1 : 0)}");
        sb.AppendLine($"CloseOnLaunch={(settings.CloseOnLaunch ? 1 : 0)}");
        sb.AppendLine($"ReinstallAfterDirChange={(settings.ReinstallAfterDirChange ? 1 : 0)}");
        sb.AppendLine($"KeepModengineSettings={(settings.KeepModengineSettings ? 1 : 0)}");
        sb.AppendLine($"ActiveProfile={settings.ActiveProfile}");

        File.WriteAllText(SettingsIniPath, sb.ToString());
    }

    public string GetSekiroDirectory()
    {
        if (File.Exists(DirIniPath))
        {
            var content = File.ReadAllText(DirIniPath).Trim();
            if (!string.IsNullOrEmpty(content) && Directory.Exists(content))
            {
                return content;
            }
        }
        return string.Empty;
    }

    public void SetSekiroDirectory(string directory)
    {
        File.WriteAllText(DirIniPath, directory);
        _logger.Log($"Sekiro directory set to: {directory}");
    }
}
