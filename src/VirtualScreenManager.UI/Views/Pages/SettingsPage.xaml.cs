using System.Windows.Controls;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace VirtualScreenManager.UI.Views.Pages;

public partial class SettingsPage : Page, INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
