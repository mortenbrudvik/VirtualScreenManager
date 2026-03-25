using System.Windows;

namespace VirtualScreenManager.UI.Services;

public class DispatcherService : IDispatcherService
{
    public void Invoke(Action action)
    {
        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }
    }
}
