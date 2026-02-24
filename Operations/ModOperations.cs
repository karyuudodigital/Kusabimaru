using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SekiroModManager.Models;

namespace SekiroModManager.Operations;

public class ModOperations 
{
    private const string ModsDirectory = "mods";
    private const string ConfigsDirectory = "configs";
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;
    private readonly List<Mod> _mods = new();

    public ModOperations(FileOperations fileService, FileLogger logger)
    {
        _fileService = fileService;
        _logger = logger;
        _fileService.EnsureDirectoryExists(ModsDirectory);
        _fileService.EnsureDirectoryExists(ConfigsDirectory);
    }

    public List<Mod> GetAllMods()
    {
        return _mods.ToList();
    }

    public Mod? GetModByName(string name)
    {
        return _mods.FirstOrDefault(m => m.Name == name);
    }

    public void AddMod(Mod mod)
    {
        if (_mods.Any(m => m.Name == mod.Name))
        {
            throw new InvalidOperationException($"Mod with name '{mod.Name}' already exists");
        }

        _mods.Add(mod);
        SaveMod(mod);
        _logger.Log($"Added mod: {mod.Name}");
    }

    public void RemoveMod(string name)
    {
        var mod = GetModByName(name);
        if (mod == null) return;

        _mods.Remove(mod);

        if (File.Exists(mod.ModConfigPath))
        {
            File.Delete(mod.ModConfigPath);
        }

        _logger.Log($"Removed mod: {name}");
    }

    public void InstallMod(string modName)
    {
        var mod = GetModByName(modName);
        if (mod == null)
        {
            throw new InvalidOperationException($"Mod '{modName}' not found");
        }

        // Implementation will be in the main service that has access to Sekiro directory
        mod.IsInstalled = true;
        SaveMod(mod);
        _logger.Log($"Installed mod: {modName}");
    }

    public void UninstallMod(string modName)
    {
        var mod = GetModByName(modName);
        if (mod == null) return;

        mod.IsInstalled = false;
        SaveMod(mod);
        _logger.Log($"Uninstalled mod: {modName}");
    }

    public bool NameExists(string name)
    {
        return _mods.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void SaveMod(Mod mod)
    {
        var configPath = Path.Combine(ConfigsDirectory, $"{mod.Name}.ini");
        mod.ModConfigPath = configPath;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(mod.IsInstalled ? "y" : "n");
        sb.AppendLine(mod.Name);
        sb.AppendLine(mod.Path);
        sb.AppendLine(mod.ModConfigPath);
        sb.AppendLine(mod.Files.Count.ToString());

        foreach (var file in mod.Files)
        {
            sb.AppendLine(file);
        }

        File.WriteAllText(configPath, sb.ToString());
    }

    public Mod? LoadMod(string configPath)
    {
        if (!File.Exists(configPath))
            return null;

        var lines = File.ReadAllLines(configPath);
        if (lines.Length < 5)
            return null;

        var mod = new Mod
        {
            IsInstalled = lines[0] == "y",
            Name = lines[1],
            Path = lines[2],
            ModConfigPath = lines[3]
        };

        if (int.TryParse(lines[4], out var fileCount))
        {
            for (int i = 5; i < 5 + fileCount && i < lines.Length; i++)
            {
                mod.Files.Add(lines[i]);
            }
        }

        return mod;
    }

    public void LoadAllMods()
    {
        _mods.Clear();
        
        if (!Directory.Exists(ConfigsDirectory))
            return;

        foreach (var configFile in Directory.GetFiles(ConfigsDirectory, "*.ini"))
        {
            var mod = LoadMod(configFile);
            if (mod != null)
            {
                _mods.Add(mod);
            }
        }
    }
}
