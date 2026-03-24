using System.IO;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualDisplayDriver.DependencyInjection;
using VirtualScreenManager.UI.DependencyInjection;
using VirtualScreenManager.UI.Services;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace VirtualScreenManager.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule<AppModule>();
                })
                .ConfigureServices(services =>
                {
                    // Virtual Display Driver API
                    services.AddVirtualDisplayDriver();

                    // WPF-UI services
                    services.AddHostedService<ApplicationHostService>();
                    services.AddNavigationViewPageProvider();
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<ISnackbarService, SnackbarService>();
                    services.AddSingleton<IContentDialogService, ContentDialogService>();
                })
                .Build();

            _host.Start();
        }
        catch (Exception ex)
        {
            LogFatalError("Application failed to start.", ex);
            MessageBox.Show(
                $"Application failed to start.\n\n{ex.Message}",
                "Virtual Screen Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host is not null)
            {
                await _host.StopAsync();
            }
        }
        catch (Exception ex)
        {
            LogFatalError("Error during application shutdown.", ex);
        }
        finally
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogFatalError("An unhandled UI exception occurred.", e.Exception);
        MessageBox.Show(
            $"An unexpected error occurred.\n\n{e.Exception.Message}",
            "Virtual Screen Manager",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogFatalError("A fatal application error occurred.", ex);
            MessageBox.Show(
                $"A fatal error occurred. The application will close.\n\n{ex.Message}",
                "Virtual Screen Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogFatalError("An unobserved task exception occurred.", e.Exception);
        e.SetObserved();
    }

    private static void LogFatalError(string message, Exception ex)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VirtualScreenManager",
                "Logs");
            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, "crash.log");
            var entry = $"[{DateTime.UtcNow:O}] {message}\n{ex}\n\n";
            File.AppendAllText(logFile, entry);
        }
        catch
        {
            // Last resort: nothing we can do if logging itself fails
        }
    }
}
