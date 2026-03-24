using System.Windows.Controls;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace VirtualScreenManager.UI.Views.Pages;

public partial class ActivityLogPage : Page, INavigableView<ActivityLogViewModel>
{
    public ActivityLogViewModel ViewModel { get; }

    public ActivityLogPage(ActivityLogViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
