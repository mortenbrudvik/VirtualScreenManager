using System.Windows;
using Microsoft.Extensions.Hosting;
using VirtualScreenManager.UI.Views;
using VirtualScreenManager.UI.Views.Pages;
using Wpf.Ui.Appearance;

namespace VirtualScreenManager.UI.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IWindow _mainWindow;

    public ApplicationHostService(IWindow mainWindow)
    {
        _mainWindow = mainWindow;
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

        _mainWindow.Loaded += OnMainWindowLoaded;
        _mainWindow.Show();

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
