
namespace Aprillz.MewUI;

/// <summary>
/// Common button configurations for <see cref="NativeMessageBox"/>.
/// </summary>
public enum NativeMessageBoxButtons : uint
{
    Ok = 0x00000000,
    OkCancel = 0x00000001,
    YesNo = 0x00000004,
    YesNoCancel = 0x00000003
}

/// <summary>
/// Common icon configurations for <see cref="NativeMessageBox"/>.
/// </summary>
public enum NativeMessageBoxIcon : uint
{
    None = 0x00000000,
    Information = 0x00000040,
    Warning = 0x00000030,
    Error = 0x00000010,
    Question = 0x00000020
}

/// <summary>
/// Native platform message box (synchronous only).
/// </summary>
public static class NativeMessageBox
{
    private static nint ResolveOwnerHandle(nint owner)
    {
        if (owner != 0 || !Application.IsRunning)
        {
            return owner;
        }

        var windows = Application.Current.AllWindows;
        for (int i = 0; i < windows.Count; i++)
        {
            var w = windows[i];
            if (w.IsActive && w.Handle != 0)
                return w.Handle;
        }

        for (int i = 0; i < windows.Count; i++)
        {
            var w = windows[i];
            if (w.Handle != 0)
                return w.Handle;
        }

        return 0;
    }

    public static bool? Show(string text, string caption = "Aprillz.MewUI", NativeMessageBoxButtons buttons = NativeMessageBoxButtons.Ok, NativeMessageBoxIcon icon = NativeMessageBoxIcon.None)
        => Show(0, text, caption, buttons, icon);

    public static bool? Show(nint owner, string text, string caption = "Aprillz.MewUI", NativeMessageBoxButtons buttons = NativeMessageBoxButtons.Ok, NativeMessageBoxIcon icon = NativeMessageBoxIcon.None)
    {
        var host = Application.IsRunning ? Application.Current.PlatformHost : Application.DefaultPlatformHost;
        owner = ResolveOwnerHandle(owner);
        return host.MessageBox.Show(owner, text ?? string.Empty, caption ?? string.Empty, buttons, icon);
    }
}
