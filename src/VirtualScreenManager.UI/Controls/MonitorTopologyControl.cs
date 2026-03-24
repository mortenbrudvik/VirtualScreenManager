using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VirtualDisplayDriver;

namespace VirtualScreenManager.UI.Controls;

public class MonitorTopologyControl : FrameworkElement
{
    private static readonly SolidColorBrush DefaultVirtualFill = CreateFrozen(Color.FromArgb(40, 96, 165, 250));
    private static readonly SolidColorBrush DefaultPhysicalFill = CreateFrozen(Color.FromArgb(40, 156, 163, 175));
    private static readonly SolidColorBrush DefaultVirtualBorder = CreateFrozen(Color.FromArgb(200, 96, 165, 250));
    private static readonly SolidColorBrush DefaultPhysicalBorder = CreateFrozen(Color.FromArgb(200, 156, 163, 175));
    private static readonly SolidColorBrush DefaultPrimaryBorder = CreateFrozen(Color.FromArgb(220, 52, 211, 153));
    private static readonly SolidColorBrush DefaultText = CreateFrozen(Color.FromArgb(220, 255, 255, 255));
    private static readonly SolidColorBrush DefaultSubtext = CreateFrozen(Color.FromArgb(140, 255, 255, 255));

    public static readonly DependencyProperty MonitorsProperty =
        DependencyProperty.Register(
            nameof(Monitors),
            typeof(IEnumerable),
            typeof(MonitorTopologyControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnMonitorsChanged));

    public IEnumerable? Monitors
    {
        get => (IEnumerable?)GetValue(MonitorsProperty);
        set => SetValue(MonitorsProperty, value);
    }

    private static void OnMonitorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MonitorTopologyControl)d;

        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= control.OnCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += control.OnCollectionChanged;
        }

        control.InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var monitors = Monitors?.Cast<SystemMonitor>().ToList();
        if (monitors is null || monitors.Count == 0) return;

        // Calculate bounds of all monitors
        int minX = monitors.Min(m => m.X);
        int minY = monitors.Min(m => m.Y);
        int maxX = monitors.Max(m => m.X + m.Width);
        int maxY = monitors.Max(m => m.Y + m.Height);

        int totalWidth = maxX - minX;
        int totalHeight = maxY - minY;

        if (totalWidth <= 0 || totalHeight <= 0) return;

        // Scale to fit control with padding
        double padding = 24;
        double availableWidth = ActualWidth - padding * 2;
        double availableHeight = ActualHeight - padding * 2;

        if (availableWidth <= 0 || availableHeight <= 0) return;

        double scale = Math.Min(availableWidth / totalWidth, availableHeight / totalHeight);

        // Center the drawing
        double scaledTotalWidth = totalWidth * scale;
        double scaledTotalHeight = totalHeight * scale;
        double offsetX = padding + (availableWidth - scaledTotalWidth) / 2;
        double offsetY = padding + (availableHeight - scaledTotalHeight) / 2;

        var virtualFill = ResolveBrush("TopologyVirtualFillBrush", DefaultVirtualFill);
        var physicalFill = ResolveBrush("TopologyPhysicalFillBrush", DefaultPhysicalFill);
        var virtualBorder = ResolveBrush("TopologyVirtualBorderBrush", DefaultVirtualBorder);
        var physicalBorder = ResolveBrush("TopologyPhysicalBorderBrush", DefaultPhysicalBorder);
        var primaryBorder = ResolveBrush("TopologyPrimaryBorderBrush", DefaultPrimaryBorder);
        var textBrush = ResolveBrush("TopologyTextBrush", DefaultText);
        var subtextBrush = ResolveBrush("TopologySubtextBrush", DefaultSubtext);

        foreach (var monitor in monitors)
        {
            double x = offsetX + (monitor.X - minX) * scale;
            double y = offsetY + (monitor.Y - minY) * scale;
            double w = monitor.Width * scale;
            double h = monitor.Height * scale;

            var rect = new Rect(x, y, w, h);
            double cornerRadius = 6;

            var fill = monitor.IsVirtual ? virtualFill : physicalFill;
            var border = monitor.IsPrimary ? primaryBorder : (monitor.IsVirtual ? virtualBorder : physicalBorder);
            double borderThickness = monitor.IsPrimary ? 2.5 : 1.5;

            var geometry = new RectangleGeometry(rect, cornerRadius, cornerRadius);
            dc.DrawGeometry(fill, new Pen(border, borderThickness), geometry);

            // Display number (large, centered)
            var numberText = new FormattedText(
                monitor.DisplayNumber.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal),
                Math.Max(14, Math.Min(32, h * 0.25)),
                textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            double numX = x + (w - numberText.Width) / 2;
            double numY = y + (h - numberText.Height) / 2 - (h > 80 ? 10 : 0);
            dc.DrawText(numberText, new Point(numX, numY));

            // Resolution text (smaller, below number)
            if (h > 80)
            {
                string resText = $"{monitor.Width}x{monitor.Height}";
                var resFormatted = new FormattedText(
                    resText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    Math.Max(9, Math.Min(14, h * 0.1)),
                    subtextBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                double resX = x + (w - resFormatted.Width) / 2;
                double resY = numY + numberText.Height + 2;
                dc.DrawText(resFormatted, new Point(resX, resY));
            }

            // Virtual badge
            if (monitor.IsVirtual && w > 60)
            {
                string badge = "Virtual";
                var badgeText = new FormattedText(
                    badge,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    9,
                    virtualBorder,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                dc.DrawText(badgeText, new Point(x + 6, y + 4));
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 200 : availableSize.Height;
        return new Size(width, height);
    }

    private static Brush ResolveBrush(string resourceKey, Brush fallback)
    {
        return Application.Current?.TryFindResource(resourceKey) as Brush ?? fallback;
    }

    private static SolidColorBrush CreateFrozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
