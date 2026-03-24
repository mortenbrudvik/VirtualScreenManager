using System.Windows.Controls;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace VirtualScreenManager.UI.Views.Pages;

public partial class StatusPage : Page, INavigableView<StatusViewModel>
{
    public StatusViewModel ViewModel { get; }

    public StatusPage(StatusViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
