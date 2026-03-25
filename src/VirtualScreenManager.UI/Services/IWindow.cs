using System.Windows;

namespace VirtualScreenManager.UI.Services;

public interface IWindow
{
    event RoutedEventHandler Loaded;
    void Show();
    void NavigateToDefault();
}
