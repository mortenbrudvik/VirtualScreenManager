using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VirtualScreenManager.UI.Controls;

/// <summary>
/// Workaround for WPF nested ScrollViewer issue where mouse wheel events are consumed
/// by inner scrollable controls, preventing the outer ScrollViewer from scrolling.
/// Attach FixMouseWheel="True" to intercept PreviewMouseWheel and handle scrolling directly.
/// </summary>
public static class ScrollViewerHelper
{
    public static readonly DependencyProperty FixMouseWheelProperty =
        DependencyProperty.RegisterAttached(
            "FixMouseWheel",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnFixMouseWheelChanged));

    public static bool GetFixMouseWheel(DependencyObject obj) => (bool)obj.GetValue(FixMouseWheelProperty);
    public static void SetFixMouseWheel(DependencyObject obj, bool value) => obj.SetValue(FixMouseWheelProperty, value);

    private static void OnFixMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer) return;

        if ((bool)e.NewValue)
        {
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
        }
        else
        {
            scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}
