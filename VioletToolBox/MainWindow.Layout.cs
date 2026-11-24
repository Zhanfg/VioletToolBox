using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace WpfApp1;

/// <summary>
/// Main-window layout only. Device operations and flashing workflows are not touched.
/// </summary>
public partial class MainWindow
{
    private void ConfigureResponsiveWindowBounds()
    {
        // WPF uses device-independent pixels: the work area already accounts for
        // display scaling. Keep a margin so the borderless window is reachable.
        Rect workArea = SystemParameters.WorkArea;
        double usableWidth = Math.Max(1, workArea.Width - 24);
        double usableHeight = Math.Max(1, workArea.Height - 24);

        // The former 1000 x 800 Viewbox shrank all text on smaller displays.
        // These are usable window limits, not a scale factor for the content.
        MinWidth = Math.Min(760, usableWidth);
        MinHeight = Math.Min(560, usableHeight);
        Width = Math.Min(1160, usableWidth);
        Height = Math.Min(850, usableHeight);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void WindowResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (WindowState != System.Windows.WindowState.Normal)
        {
            return;
        }

        // The transparent WPF window has no standard non-client resize border.
        // This grip changes only the window bounds; the page scroll viewport
        // makes wide controls reachable instead of uniformly shrinking them.
        Width = Math.Max(MinWidth, ActualWidth + e.HorizontalChange);
        Height = Math.Max(MinHeight, ActualHeight + e.VerticalChange);
    }
}
