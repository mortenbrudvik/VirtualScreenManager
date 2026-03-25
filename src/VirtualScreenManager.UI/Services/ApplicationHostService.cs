using System.Windows;
using Microsoft.Extensions.Hosting;
using VirtualScreenManager.UI.Views;
using Wpf.Ui.Appearance;

namespace VirtualScreenManager.UI.Services;

public class ApplicationHostService : IHostedService
{
    private readonly Lazy<IWindow> _mainWindow;

    public ApplicationHostService(Lazy<IWindow> mainWindow)
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

        var mainWindow = _mainWindow.Value;
        mainWindow.Loaded += OnMainWindowLoaded;
        mainWindow.Show();

        return Task.CompletedTask;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not IWindow window)
        {
            return;
        }

        window.NavigateToDefault();
    }
}
