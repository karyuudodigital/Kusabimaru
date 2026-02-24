using System.Collections.Generic;
namespace SekiroModManager.Models;

public class Mod
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ModConfigPath { get; set; } = string.Empty;
    public List<string> Files { get; set; } = new();
    public bool IsInstalled { get; set; }
}
