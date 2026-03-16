using Aprillz.MewUI.Native;

namespace Aprillz.MewUI.Platform.Win32;

internal sealed class Win32MessageBoxService : IMessageBoxService
{
    public bool? Show(nint owner, string text, string caption, NativeMessageBoxButtons buttons, NativeMessageBoxIcon icon)
    {
        var type = (uint)buttons | (uint)icon;
        int result = User32.MessageBox(owner, text ?? string.Empty, caption ?? string.Empty, type);
        return result switch
        {
            1 => true,   // IDOK     → Accept
            6 => true,   // IDYES    → Accept
            4 => true,   // IDRETRY  → Accept
            2 => false,  // IDCANCEL → Reject
            3 => false,  // IDABORT  → Reject
            7 => null,   // IDNO     → Destructive
            5 => null,   // IDIGNORE → Destructive
            _ => true
        };
    }
}
