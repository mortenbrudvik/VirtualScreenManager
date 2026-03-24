using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VirtualScreenManager.UI.Controls;

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
