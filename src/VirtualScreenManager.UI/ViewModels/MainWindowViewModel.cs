using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualScreenManager.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Virtual Screen Manager";
}
