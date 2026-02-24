namespace SekiroModManager.Models;

public class ModEngineSettings
{
    public bool ChainDll { get; set; }
    public bool Debug { get; set; }
    public bool SkipLogos { get; set; }
    public bool CacheFilePaths { get; set; }
    public bool LoadUxmFiles { get; set; }
    public string ModOverrideDirectory { get; set; } = "mods";
}
