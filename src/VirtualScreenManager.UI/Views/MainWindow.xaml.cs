using VirtualScreenManager.UI.Services;
using VirtualScreenManager.UI.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.Views;

public partial class MainWindow : FluentWindow, IWindow
{
    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
    {
        DataContext = viewModel;
        InitializeComponent();

        navigationService.SetNavigationControl(NavigationView);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        contentDialogService.SetDialogHost(RootContentDialogHost);

        SystemThemeWatcher.Watch(this);
    }
}
