using VirtualDisplayDriver;

namespace VirtualScreenManager.UI.Services;

public class VirtualDisplayInfo : IVirtualDisplayInfo
{
    public IReadOnlyList<SystemMonitor> GetAllMonitors()
        => VirtualDisplayConfiguration.GetAllMonitors();

    public IReadOnlyList<VirtualMonitor> GetVirtualMonitors()
        => VirtualDisplayConfiguration.GetVirtualMonitors();

    public int GetConfiguredDisplayCount()
        => VirtualDisplayDetection.GetConfiguredDisplayCount();

    public void SetConfiguredDisplayCount(int count)
        => VirtualDisplayDetection.SetConfiguredDisplayCount(count);

    public string? GetInstallPath()
        => VirtualDisplayDetection.GetInstallPath();
}
