using System.Windows;
using Microsoft.Extensions.Hosting;
using VirtualScreenManager.UI.Views;
using VirtualScreenManager.UI.Views.Pages;
using Wpf.Ui.Appearance;

namespace VirtualScreenManager.UI.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return HandleActivationAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task HandleActivationAsync()
    {
        if (Application.Current.Windows.OfType<MainWindow>().Any())
        {
            return Task.CompletedTask;
        }

        ApplicationThemeManager.ApplySystemTheme();

        var mainWindow = (IWindow)_serviceProvider.GetService(typeof(IWindow))!;
        mainWindow.Loaded += OnMainWindowLoaded;
        mainWindow.Show();

        return Task.CompletedTask;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not MainWindow mainWindow)
        {
            return;
        }

        _ = mainWindow.NavigationView.Navigate(typeof(StatusPage));
    }
}
