namespace Aprillz.MewUI.Platform.Linux;

internal sealed class LinuxMessageBoxService : IMessageBoxService
{
    public bool? Show(nint owner, string text, string caption, NativeMessageBoxButtons buttons, NativeMessageBoxIcon icon)
        => LinuxExternalDialogs.ShowMessageBox(owner, text ?? string.Empty, caption ?? string.Empty, buttons, icon);
}
