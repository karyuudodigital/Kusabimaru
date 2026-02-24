using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SekiroModManager.Operations;

namespace SekiroModManager;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<FileLogger, FileLogger>();
        services.AddSingleton<FileOperations, FileOperations>();
        services.AddSingleton<Configuration, Configuration>();
        services.AddSingleton<ModOperations, ModOperations>();
        services.AddSingleton<ProfileOperations, ProfileOperations>();
        services.AddSingleton<ModEngineOperations, ModEngineOperations>();
        services.AddSingleton<ModInstallation, ModInstallation>();
        
        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
