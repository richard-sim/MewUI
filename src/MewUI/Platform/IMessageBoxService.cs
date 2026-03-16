namespace Aprillz.MewUI.Platform;

/// <summary>
/// Platform implementation for message box dialogs.
/// </summary>
public interface IMessageBoxService
{
    /// <summary>
    /// Shows a message box and returns the user selection.
    /// </summary>
    bool? Show(nint owner, string text, string caption, NativeMessageBoxButtons buttons, NativeMessageBoxIcon icon);
}
