using Aprillz.MewUI.Platform;

namespace Aprillz.MewUI;

/// <summary>
/// Arguments for drag-and-drop events.
/// </summary>
public sealed class DragEventArgs
{
    /// <summary>
    /// Gets the dropped or dragged data payload.
    /// </summary>
    public IDataObject Data { get; }

    /// <summary>
    /// Gets the position relative to the window in DIPs.
    /// </summary>
    public Point Position { get; }

    /// <summary>
    /// Gets the position in screen coordinates in device pixels.
    /// </summary>
    public Point ScreenPosition { get; }

    /// <summary>
    /// Gets or sets whether the event has been handled.
    /// </summary>
    public bool Handled { get; set; }

    public DragEventArgs(IDataObject data, Point position, Point screenPosition)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Position = position;
        ScreenPosition = screenPosition;
    }
}
