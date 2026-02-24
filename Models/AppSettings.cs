namespace SekiroModManager.Models;

public class AppSettings
{
    public string SekiroDirectory { get; set; } = string.Empty;
    public bool WarningsEnabled { get; set; }
    public bool LoggingEnabled { get; set; }
    public bool CloseOnLaunch { get; set; }
    public bool ReinstallAfterDirChange { get; set; }
    public bool KeepModengineSettings { get; set; }
    public string ActiveProfile { get; set; } = string.Empty;
}
