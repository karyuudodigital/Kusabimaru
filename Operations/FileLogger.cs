using System.IO;
using System;

namespace SekiroModManager.Operations;

public class FileLogger 
{
    private readonly string _logPath;
    public bool IsEnabled { get; set; }

    public FileLogger()
    {
        _logPath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
    }

    public void Log(string message)
    {
        if (!IsEnabled) return;
        
        try
        {
            File.AppendAllText(_logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
        }
        catch
        {
            // Silently fail if logging fails
        }
    }

    public void LogError(string message)
    {
        Log($"ERROR: {message}");
    }
}
