using System.Windows.Controls;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace VirtualScreenManager.UI.Views.Pages;

public partial class DisplayManagementPage : Page, INavigableView<DisplayManagementViewModel>
{
    public DisplayManagementViewModel ViewModel { get; }

    public DisplayManagementPage(DisplayManagementViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
