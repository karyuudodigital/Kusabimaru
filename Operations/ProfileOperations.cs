using System.Collections.Generic;
using System.IO;
using SekiroModManager.Models;
using System.Linq;
using System;

namespace SekiroModManager.Operations;

public class ProfileOperations
{
    private const string ProfilesDirectory = "profiles";
    private const string ConfigsPDirectory = "configsP";
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;
    private readonly List<Profile> _profiles = new();

    public ProfileOperations(FileOperations fileService, FileLogger logger)
    {
        _fileService = fileService;
        _logger = logger;
        _fileService.EnsureDirectoryExists(ProfilesDirectory);
        _fileService.EnsureDirectoryExists(ConfigsPDirectory);
    }

    public List<Profile> GetAllProfiles()
    {
        return _profiles;
    }

    public Profile? GetProfileByName(string name)
    {
        return _profiles.FirstOrDefault(p => p.Name == name);
    }

    public void AddProfile(Profile profile)
    {
        if (_profiles.Any(p => p.Name == profile.Name))
        {
            throw new InvalidOperationException($"Profile with name '{profile.Name}' already exists");
        }

        _profiles.Add(profile);
        SaveProfile(profile);
        _logger.Log($"Added profile: {profile.Name}");
    }

    public void RemoveProfile(string name)
    {
        var profile = GetProfileByName(name);
        if (profile == null) return;

        _profiles.Remove(profile);

        if (File.Exists(profile.ProfileConfigPath))
        {
            File.Delete(profile.ProfileConfigPath);
        }

        _logger.Log($"Removed profile: {name}");
    }

    public void InstallProfile(string profileName)
    {
        var profile = GetProfileByName(profileName);
        if (profile == null)
        {
            throw new InvalidOperationException($"Profile '{profileName}' not found");
        }

        profile.IsInstalled = true;
        SaveProfile(profile);
        _logger.Log($"Installed profile: {profileName}");
    }

    public void UninstallProfile(string profileName)
    {
        var profile = GetProfileByName(profileName);
        if (profile == null) return;

        profile.IsInstalled = false;
        SaveProfile(profile);
        _logger.Log($"Uninstalled profile: {profileName}");
    }

    public bool NameExists(string name)
    {
        return _profiles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void SaveProfile(Profile profile)
    {
        var configPath = Path.Combine(ConfigsPDirectory, $"{profile.Name}.ini");
        profile.ProfileConfigPath = configPath;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(profile.IsInstalled ? "y" : "n");
        sb.AppendLine(profile.Name);
        sb.AppendLine(profile.Path);
        sb.AppendLine(profile.ProfileConfigPath);
        sb.AppendLine(profile.ProfileFolder);
        sb.AppendLine(profile.ModCount.ToString());
        sb.AppendLine(profile.Files.Count.ToString());

        foreach (var file in profile.Files)
        {
            sb.AppendLine(file);
        }

        File.WriteAllText(configPath, sb.ToString());
    }

    public Profile? LoadProfile(string configPath)
    {
        if (!File.Exists(configPath))
            return null;

        var lines = File.ReadAllLines(configPath);
        if (lines.Length < 7)
            return null;

        var profile = new Profile
        {
            IsInstalled = lines[0] == "y",
            Name = lines[1],
            Path = lines[2],
            ProfileConfigPath = lines[3],
            ProfileFolder = lines[4]
        };

        if (int.TryParse(lines[5], out var modCount))
        {
            profile.ModCount = modCount;
        }

        if (int.TryParse(lines[6], out var fileCount))
        {
            for (int i = 7; i < 7 + fileCount && i < lines.Length; i++)
            {
                profile.Files.Add(lines[i]);
            }
        }

        return profile;
    }

    public void LoadAllProfiles()
    {
        _profiles.Clear();
        
        if (!Directory.Exists(ConfigsPDirectory))
            return;

        foreach (var configFile in Directory.GetFiles(ConfigsPDirectory, "*.ini"))
        {
            var profile = LoadProfile(configFile);
            if (profile != null)
            {
                _profiles.Add(profile);
            }
        }
    }
}
