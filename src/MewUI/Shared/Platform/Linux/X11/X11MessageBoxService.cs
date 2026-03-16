namespace Aprillz.MewUI.Platform.Linux.X11;

internal sealed class X11MessageBoxService : IMessageBoxService
{
    public bool? Show(nint owner, string text, string caption, NativeMessageBoxButtons buttons, NativeMessageBoxIcon icon)
        => LinuxExternalDialogs.ShowMessageBox(owner, text ?? string.Empty, caption ?? string.Empty, buttons, icon);
}
