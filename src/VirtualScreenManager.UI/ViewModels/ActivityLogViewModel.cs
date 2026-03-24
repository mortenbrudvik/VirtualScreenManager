using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using VirtualScreenManager.Core.Models;
using VirtualScreenManager.Core.Services;

namespace VirtualScreenManager.UI.ViewModels;

public partial class ActivityLogViewModel : ViewModelBase
{
    private readonly IActivityLogger _activityLogger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllSelected))]
    [NotifyPropertyChangedFor(nameof(IsInfoSelected))]
    [NotifyPropertyChangedFor(nameof(IsWarningSelected))]
    [NotifyPropertyChangedFor(nameof(IsErrorSelected))]
    private LogLevel _selectedFilter = LogLevel.Trace;

    public bool IsAllSelected => SelectedFilter == LogLevel.Trace;
    public bool IsInfoSelected => SelectedFilter == LogLevel.Information;
    public bool IsWarningSelected => SelectedFilter == LogLevel.Warning;
    public bool IsErrorSelected => SelectedFilter == LogLevel.Error;

    public ObservableCollection<LogEntry> FilteredEntries { get; } = [];

    public ActivityLogViewModel(IActivityLogger activityLogger)
    {
        _activityLogger = activityLogger;
        _activityLogger.EntryAdded += OnEntryAdded;
        _activityLogger.Cleared += OnCleared;
    }

    public override Task OnNavigatedToAsync()
    {
        RefreshFilteredEntries();
        return Task.CompletedTask;
    }

    private void OnEntryAdded(LogEntry entry)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (SelectedFilter == LogLevel.Trace || entry.Level >= SelectedFilter)
            {
                FilteredEntries.Add(entry);
            }
        });
    }

    private void OnCleared()
    {
        Application.Current.Dispatcher.Invoke(() => FilteredEntries.Clear());
    }

    partial void OnSelectedFilterChanged(LogLevel value)
    {
        RefreshFilteredEntries();
    }

    private void RefreshFilteredEntries()
    {
        FilteredEntries.Clear();
        foreach (var entry in _activityLogger.Entries)
        {
            if (SelectedFilter == LogLevel.Trace || entry.Level >= SelectedFilter)
            {
                FilteredEntries.Add(entry);
            }
        }
    }

    [RelayCommand]
    private void SetFilter(string level)
    {
        SelectedFilter = Enum.TryParse<LogLevel>(level, out var parsed) ? parsed : LogLevel.Trace;
    }

    [RelayCommand]
    private void ClearLog()
    {
        _activityLogger.Clear();
    }

    [RelayCommand]
    private void CopyLog()
    {
        var text = string.Join(Environment.NewLine,
            FilteredEntries.Select(e => $"[{e.Timestamp:HH:mm:ss}] [{e.Level}] [{e.Category}] {e.Message}{(e.Detail is not null ? $" - {e.Detail}" : "")}"));
        Clipboard.SetText(text);
    }
}
