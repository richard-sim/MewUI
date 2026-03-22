using Aprillz.MewUI.Controls;

namespace Aprillz.MewUI;

/// <summary>
/// Extension methods for <see cref="Window"/> that use overlay services.
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// Shows a toast notification at 4/5 of the window height.
    /// Auto-dismisses after a duration based on text length.
    /// </summary>
    public static void ShowToast(this Window window, string text)
    {
        var toast = window.OverlayLayer.GetOrCreateService<ToastService>(
            layer => new ToastService(layer));
        toast.Show(text);
    }

    /// <summary>
    /// Creates and shows a busy indicator overlay.
    /// Dispose the returned <see cref="IBusyIndicator"/> to dismiss it.
    /// </summary>
    public static IBusyIndicator CreateBusyIndicator(this Window window, string? message = null, bool cancellable = false)
    {
        var service = window.OverlayLayer.GetOrCreateService<BusyIndicatorService>(
            layer => new BusyIndicatorService(layer));
        return service.Create(message, cancellable);
    }
}
