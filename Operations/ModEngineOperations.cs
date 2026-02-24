using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SekiroModManager.Models;

namespace SekiroModManager.Operations;

public class ModEngineOperations
{
    private readonly FileOperations _fileService;
    private readonly FileLogger _logger;

    public ModEngineOperations(FileOperations fileService, FileLogger logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public bool IsModEngineInstalled(string sekiroDirectory)
    {
        var dinput8Path = Path.Combine(sekiroDirectory, "dinput8.dll");
        return File.Exists(dinput8Path);
    }

    public void InstallModEngine(string sekiroDirectory)
    {
        // This would download and install ModEngine
        // For now, just log that it needs to be done
        _logger.Log("ModEngine installation check - user needs to install manually");
    }

    public void SetActiveProfile(string sekiroDirectory, string? profileName)
    {
        var iniPath = Path.Combine(sekiroDirectory, "modengine.ini");
        if (!File.Exists(iniPath))
        {
            _logger.LogError("modengine.ini not found");
            return;
        }

        var content = File.ReadAllText(iniPath);
        var modOverrideDir = string.IsNullOrEmpty(profileName) 
            ? "mods" 
            : profileName;

        content = Regex.Replace(
            content,
            @"modOverrideDirectory\s*=\s*.*",
            $"modOverrideDirectory = {modOverrideDir}",
            RegexOptions.IgnoreCase
        );

        File.WriteAllText(iniPath, content);
        _logger.Log($"Set active profile to: {modOverrideDir}");
    }

    public void SetDefaultProfile(string sekiroDirectory)
    {
        SetActiveProfile(sekiroDirectory, null);
    }

    public ModEngineSettings LoadSettings(string sekiroDirectory)
    {
        var settings = new ModEngineSettings();
        var iniPath = Path.Combine(sekiroDirectory, "modengine.ini");

        if (!File.Exists(iniPath))
            return settings;

        var content = File.ReadAllText(iniPath);

        // Parse settings from ini file
        settings.ChainDll = Regex.IsMatch(content, @"chainDll\s*=\s*true", RegexOptions.IgnoreCase);
        settings.Debug = Regex.IsMatch(content, @"debug\s*=\s*true", RegexOptions.IgnoreCase);
        settings.SkipLogos = Regex.IsMatch(content, @"skipLogos\s*=\s*true", RegexOptions.IgnoreCase);
        settings.CacheFilePaths = Regex.IsMatch(content, @"cacheFilePaths\s*=\s*true", RegexOptions.IgnoreCase);
        settings.LoadUxmFiles = Regex.IsMatch(content, @"loadUXMFiles\s*=\s*true", RegexOptions.IgnoreCase);

        var modOverrideMatch = Regex.Match(content, @"modOverrideDirectory\s*=\s*(.+)", RegexOptions.IgnoreCase);
        if (modOverrideMatch.Success)
        {
            settings.ModOverrideDirectory = modOverrideMatch.Groups[1].Value.Trim();
        }

        return settings;
    }

    public void SaveSettings(string sekiroDirectory, ModEngineSettings settings)
    {
        var iniPath = Path.Combine(sekiroDirectory, "modengine.ini");
        if (!File.Exists(iniPath))
        {
            _logger.LogError("modengine.ini not found");
            return;
        }

        var content = File.ReadAllText(iniPath);

        content = Regex.Replace(content, @"chainDll\s*=\s*.*", 
            $"chainDll = {settings.ChainDll.ToString().ToLower()}", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"debug\s*=\s*.*", 
            $"debug = {settings.Debug.ToString().ToLower()}", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"skipLogos\s*=\s*.*", 
            $"skipLogos = {settings.SkipLogos.ToString().ToLower()}", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"cacheFilePaths\s*=\s*.*", 
            $"cacheFilePaths = {settings.CacheFilePaths.ToString().ToLower()}", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"loadUXMFiles\s*=\s*.*", 
            $"loadUXMFiles = {settings.LoadUxmFiles.ToString().ToLower()}", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"modOverrideDirectory\s*=\s*.*", 
            $"modOverrideDirectory = {settings.ModOverrideDirectory}", RegexOptions.IgnoreCase);

        File.WriteAllText(iniPath, content);
        _logger.Log("Saved ModEngine settings");
    }

    public void EditModEngineIni(string sekiroDirectory, string dll, bool unchain)
    {
        var iniPath = Path.Combine(sekiroDirectory, "modengine.ini");
        if (!File.Exists(iniPath))
        {
            _logger.LogError("modengine.ini not found");
            return;
        }

        var content = File.ReadAllText(iniPath);
        
        if (unchain)
        {
            // Remove DLL from chain
            content = Regex.Replace(content, $@"{Regex.Escape(dll)}\s*,?", string.Empty, RegexOptions.IgnoreCase);
        }
        else
        {
            // Add DLL to chain
            if (!content.Contains(dll, StringComparison.OrdinalIgnoreCase))
            {
                var chainMatch = Regex.Match(content, @"(chainDll\s*=\s*)([^\r\n]+)", RegexOptions.IgnoreCase);
                if (chainMatch.Success)
                {
                    var currentChain = chainMatch.Groups[2].Value.Trim();
                    var newChain = string.IsNullOrEmpty(currentChain) 
                        ? dll 
                        : $"{currentChain}, {dll}";
                    content = content.Replace(chainMatch.Value, $"{chainMatch.Groups[1].Value}{newChain}");
                }
            }
        }

        File.WriteAllText(iniPath, content);
        _logger.Log($"{(unchain ? "Unchained" : "Chained")} DLL: {dll}");
    }

    public void LaunchSekiro(string sekiroDirectory)
    {
        var exePath = Path.Combine(sekiroDirectory, "sekiro.exe");
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Sekiro executable not found at {exePath}");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = sekiroDirectory,
            UseShellExecute = true
        });

        _logger.Log("Launched Sekiro");
    }
}
