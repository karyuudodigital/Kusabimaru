using System;
using System.IO;
using SekiroModManager.Models;

namespace SekiroModManager.Operations;

public class ModInstallation
{
    private readonly ModOperations _modService;
    private readonly ProfileOperations _profileService;
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;

    public ModInstallation(
        ModOperations modService,
        ProfileOperations profileService,
        FileOperations fileService,
        FileLogger logger)
    {
        _modService = modService;
        _profileService = profileService;
        _fileService = fileService;
        _logger = logger;
    }

    public void InstallModToSekiro(string modName, string sekiroDirectory)
    {
        var mod = _modService.GetModByName(modName);
        if (mod == null)
            throw new InvalidOperationException($"Mod '{modName}' not found");

        var modsDir = Path.Combine(sekiroDirectory, "mods");
        _fileService.EnsureDirectoryExists(modsDir);

        var modArchivePath = Path.Combine("mods", $"{mod.Name}.zip");
        if (!File.Exists(modArchivePath))
            throw new FileNotFoundException($"Mod archive not found: {modArchivePath}");

        var tempExtractPath = Path.Combine("tmp", mod.Name);
        _fileService.EnsureDirectoryExists(tempExtractPath);

        try
        {
            _fileService.ExtractArchive(modArchivePath, tempExtractPath);

            // Copy files to Sekiro mods directory
            foreach (var file in mod.Files)
            {
                var sourcePath = Path.Combine(tempExtractPath, file);
                var destPath = Path.Combine(modsDir, file);
                
                if (File.Exists(sourcePath))
                {
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        _fileService.EnsureDirectoryExists(destDir);
                    }
                    File.Copy(sourcePath, destPath, true);
                }
            }

            _modService.InstallMod(modName);
            _logger.Log($"Installed mod '{modName}' to Sekiro directory");
        }
        finally
        {
            _fileService.DeleteDirectory(tempExtractPath);
        }
    }

    public void UninstallModFromSekiro(string modName, string sekiroDirectory)
    {
        var mod = _modService.GetModByName(modName);
        if (mod == null)
            return;

        var modsDir = Path.Combine(sekiroDirectory, "mods");

        // Delete files from Sekiro mods directory
        foreach (var file in mod.Files)
        {
            var filePath = Path.Combine(modsDir, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        _modService.UninstallMod(modName);
        _logger.Log($"Uninstalled mod '{modName}' from Sekiro directory");
    }

    public void InstallProfileToSekiro(string profileName, string sekiroDirectory)
    {
        var profile = _profileService.GetProfileByName(profileName);
        if (profile == null)
            throw new InvalidOperationException($"Profile '{profileName}' not found");

        var profileDir = Path.Combine(sekiroDirectory, profile.Name);
        _fileService.EnsureDirectoryExists(profileDir);

        var profileArchivePath = Path.Combine("profiles", $"{profile.Name}.zip");
        if (!File.Exists(profileArchivePath))
            throw new FileNotFoundException($"Profile archive not found: {profileArchivePath}");

        var tempExtractPath = Path.Combine("tmp", profile.Name);
        _fileService.EnsureDirectoryExists(tempExtractPath);

        try
        {
            _fileService.ExtractArchive(profileArchivePath, tempExtractPath);

            // Copy files to profile directory
            foreach (var file in profile.Files)
            {
                var sourcePath = Path.Combine(tempExtractPath, file);
                var destPath = Path.Combine(profileDir, file);
                
                if (File.Exists(sourcePath))
                {
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        _fileService.EnsureDirectoryExists(destDir);
                    }
                    File.Copy(sourcePath, destPath, true);
                }
            }

            _profileService.InstallProfile(profileName);
            _logger.Log($"Installed profile '{profileName}' to Sekiro directory");
        }
        finally
        {
            _fileService.DeleteDirectory(tempExtractPath);
        }
    }

    public void UninstallProfileFromSekiro(string profileName, string sekiroDirectory)
    {
        var profile = _profileService.GetProfileByName(profileName);
        if (profile == null)
            return;

        var profileDir = Path.Combine(sekiroDirectory, profile.Name);
        if (Directory.Exists(profileDir))
        {
            _fileService.DeleteDirectory(profileDir);
        }

        _profileService.UninstallProfile(profileName);
        _logger.Log($"Uninstalled profile '{profileName}' from Sekiro directory");
    }
}
