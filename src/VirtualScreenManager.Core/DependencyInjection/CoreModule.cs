using Autofac;
using VirtualScreenManager.Core.Services;

namespace VirtualScreenManager.Core.DependencyInjection;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ActivityLogger>()
            .As<IActivityLogger>()
            .SingleInstance();
    }
}
