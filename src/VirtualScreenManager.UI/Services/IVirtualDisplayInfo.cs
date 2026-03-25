using VirtualDisplayDriver;

namespace VirtualScreenManager.UI.Services;

public interface IVirtualDisplayInfo
{
    IReadOnlyList<SystemMonitor> GetAllMonitors();
    IReadOnlyList<VirtualMonitor> GetVirtualMonitors();
    int GetConfiguredDisplayCount();
    void SetConfiguredDisplayCount(int count);
    string? GetInstallPath();
}
