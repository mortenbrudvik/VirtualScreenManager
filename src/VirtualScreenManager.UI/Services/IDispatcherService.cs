namespace VirtualScreenManager.UI.Services;

public interface IDispatcherService
{
    void Invoke(Action action);
}
