using System.Windows;
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

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
