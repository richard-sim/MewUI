using Aprillz.MewUI.Diagnostics;
using Aprillz.MewUI.Platform.Linux.X11.IBus;

namespace Aprillz.MewUI.Platform.Linux.X11;

/// <summary>
/// Creates the best available <see cref="IX11InputMethod"/> for the current environment.
/// Priority: IBus > fcitx5 > XIM fallback.
/// </summary>
internal static class X11InputMethodFactory
{
    private static readonly EnvDebugLogger ImeLogger = new("MEWUI_IME_DEBUG", "[IMFactory]");

    public static IX11InputMethod? Create(nint display, nint window)
    {
        if (display == 0 || window == 0) return null;

        // Check environment hints for user intent
        string? gtkIm = null, qtIm = null;
        try { gtkIm = Environment.GetEnvironmentVariable("GTK_IM_MODULE"); } catch { }
        try { qtIm = Environment.GetEnvironmentVariable("QT_IM_MODULE"); } catch { }

        bool hintFcitx = IsHint(gtkIm, "fcitx") || IsHint(qtIm, "fcitx");

        ImeLogger.Write($"Hints: GTK_IM_MODULE={gtkIm ?? "(null)"} QT_IM_MODULE={qtIm ?? "(null)"}");
        Log($"[IMFactory] GTK_IM_MODULE={gtkIm ?? "(null)"} QT_IM_MODULE={qtIm ?? "(null)"}");

        // Try IBus first (unless fcitx is explicitly preferred)
        if (!hintFcitx)
        {
            Log("[IMFactory] Trying IBus...");
            var ibus = IBusInputMethod.TryCreate();
            if (ibus != null)
            {
                Log("[IMFactory] Selected: IBus");
                return ibus;
            }
            Log("[IMFactory] IBus not available");
        }

        // TODO: Phase 4 — try Fcitx5InputMethod
        if (hintFcitx)
        {
            Log("[IMFactory] fcitx hinted, trying IBus as fallback...");
            var ibus = IBusInputMethod.TryCreate();
            if (ibus != null)
            {
                Log("[IMFactory] Selected: IBus (fcitx hinted but not yet supported)");
                return ibus;
            }
        }

        // Fallback: XIM
        Log("[IMFactory] Trying XIM fallback...");
        var xim = XimInputMethod.TryCreate(display, window);
        if (xim != null)
        {
            Log("[IMFactory] Selected: XIM fallback");
            return xim;
        }

        Log("[IMFactory] No input method available");
        return null;
    }

    private static void Log(string msg) => Console.Error.WriteLine(msg);

    private static bool IsHint(string? value, string prefix)
        => value != null && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
