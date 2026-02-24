using System.Collections.Generic;
namespace SekiroModManager.Models;

public class Profile
{
    public string Name { get; set; } = string.Empty;
    public bool IsInstalled { get; set; }
    public string Path { get; set; } = string.Empty;
    public string ProfileConfigPath { get; set; } = string.Empty;
    public int ModCount { get; set; }
    public List<string> Files { get; set; } = new();
    public string ProfileFolder { get; set; } = string.Empty;
}
