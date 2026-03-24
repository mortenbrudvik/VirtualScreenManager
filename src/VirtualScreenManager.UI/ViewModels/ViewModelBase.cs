using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace VirtualScreenManager.UI.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, INavigationAware
{
    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;

    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}
