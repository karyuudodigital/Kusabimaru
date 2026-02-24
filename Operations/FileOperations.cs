using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using SekiroModManager.Models;
using System.IO;
using System;

namespace SekiroModManager.Operations;

public class FileOperations 
{

    public enum TraverseMode
    {
        FindModFolders = 0,
        CollectFileNames = 1,
        ValidateModFiles = 2
    }

    private static readonly HashSet<string> ModengineFolders = new()
    {
        "parts", "event", "map", "msg", "param", "script", "chr", "cutscene",
        "facegen", "font", "action", "menu", "mtd", "obj", "other", "sfx",
        "shader", "sound", "movie"
    };

    private readonly FileLogger _logger;

    public FileOperations(FileLogger logger)
    {
        _logger = logger;
    }

    public bool IsModPathEmpty(string modPath)
    {
        return string.IsNullOrWhiteSpace(modPath);
    }

    public void ExtractArchive(string archivePath, string outputPath)
    {
        EnsureDirectoryExists(outputPath);

        var extension = Path.GetExtension(archivePath).ToLower();
        
        try
        {
            if (extension == ".zip" || extension == ".7z")
            {
                ExtractWith7Zip(archivePath, outputPath);
            }
            else if (extension == ".rar")
            {
                ExtractRar(archivePath, outputPath);
            }
            else
            {
                throw new NotSupportedException($"Archive format {extension} is not supported");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to extract archive {archivePath}: {ex.Message}");
            throw;
        }
    }

    public void CreateArchive(string sourcePath, string archivePath)
    {
        EnsureDirectoryExists(Path.GetDirectoryName(archivePath) ?? string.Empty);

        try
        {
            CreateZipWith7Zip(sourcePath, archivePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to create archive {archivePath}: {ex.Message}");
            throw;
        }
    }

    public List<string> TraverseDirectory(string directory, TraverseMode mode)
    {
        var files = new List<string>();
        
        if (!Directory.Exists(directory))
        {
            return files;
        }

        switch (mode)
        {
            case TraverseMode.FindModFolders:
                _foundModPath = null;
                FindModFolders(directory);
                break;
            case TraverseMode.CollectFileNames:
                CollectFileNames(directory, files);
                break;
            case TraverseMode.ValidateModFiles:
                ValidateModFiles(directory);
                break;
        }

        return files;
    }

    private string? _foundModPath;

    public string? GetFoundModPath()
    {
        return _foundModPath;
    }

    private void FindModFolders(string directory)
    {
        var dirs = Directory.GetDirectories(directory);
        
        foreach (var dir in dirs)
        {
            var dirName = Path.GetFileName(dir).ToLower();
            if (ModengineFolders.Contains(dirName))
            {
                _foundModPath = dir;
                return;
            }
        }

        // Check subdirectories
        foreach (var dir in dirs)
        {
            FindModFolders(dir);
            if (_foundModPath != null) return;
        }
    }

    private void CollectFileNames(string directory, List<string> files)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            files.Add(Path.GetRelativePath(directory, file).Replace('\\', '/'));
        }

        foreach (var subDir in Directory.GetDirectories(directory))
        {
            CollectFileNames(subDir, files);
        }
    }

    private void ValidateModFiles(string directory)
    {
        // This would set a flag somewhere - for now just check if modengine folders exist
        var dirs = Directory.GetDirectories(directory);
        foreach (var dir in dirs)
        {
            var dirName = Path.GetFileName(dir).ToLower();
            if (ModengineFolders.Contains(dirName))
            {
                // Mod is valid
                return;
            }
        }
    }

    public bool DirectoryContainsSekiroExe(string directory)
    {
        return File.Exists(Path.Combine(directory, "sekiro.exe"));
    }

    public void CopyDirectory(string sourceDir, string destDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        EnsureDirectoryExists(destDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        if (recursive)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                var newDestDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestDir, true);
            }
        }
    }

    public void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public void EnsureDirectoryExists(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void ExtractWith7Zip(string archivePath, string outputPath)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var command = $"cd \"{currentDir}\" & 7za e -spf -y -o\"{outputPath}\" \"{archivePath}\"";
        ExecuteCommand(command);
    }

    private void ExtractRar(string archivePath, string outputPath)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var command = $"cd \"{currentDir}\" & unrar x -y \"{archivePath}\" * \"{outputPath}\"";
        ExecuteCommand(command);
    }

    private void CreateZipWith7Zip(string sourcePath, string archivePath)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var command = $"cd \"{currentDir}\" & 7za a -y \"{archivePath}\" \"{sourcePath}/*\"";
        ExecuteCommand(command);
    }

    private void ExecuteCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {error}");
        }
    }
}
