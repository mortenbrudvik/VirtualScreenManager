using Autofac;
using VirtualScreenManager.Core.DependencyInjection;
using VirtualScreenManager.UI.Services;
using VirtualScreenManager.UI.ViewModels;
using VirtualScreenManager.UI.Views;
using VirtualScreenManager.UI.Views.Pages;

namespace VirtualScreenManager.UI.DependencyInjection;

public class AppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CoreModule>();

        builder.RegisterType<VirtualDisplayInfo>().As<IVirtualDisplayInfo>().SingleInstance();
        builder.RegisterType<DispatcherService>().As<IDispatcherService>().SingleInstance();

        // ViewModels
        builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<StatusViewModel>().AsSelf();
        builder.RegisterType<DisplayManagementViewModel>().AsSelf();
        builder.RegisterType<SettingsViewModel>().AsSelf();
        builder.RegisterType<ActivityLogViewModel>().AsSelf().SingleInstance();

        // MainWindow
        builder.RegisterType<MainWindow>().As<IWindow>().AsSelf().SingleInstance();

        // Pages
        builder.RegisterType<StatusPage>().AsSelf();
        builder.RegisterType<DisplayManagementPage>().AsSelf();
        builder.RegisterType<SettingsPage>().AsSelf();
        builder.RegisterType<ActivityLogPage>().AsSelf();
    }
}
