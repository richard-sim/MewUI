using System.Runtime.InteropServices;

using Aprillz.MewUI.Native;

namespace Aprillz.MewUI.Platform.Win32;

/// <summary>
/// Provides DPI API fallbacks for Windows 7/8 compatibility.
/// Probes for modern DPI APIs at startup and falls back to legacy equivalents.
/// </summary>
internal static partial class Win32DpiApiResolver
{
    private const int LOGPIXELSX = 88;

    // Win10 1703+
    private static readonly bool _hasSetProcessDpiAwarenessContext;
    // Win10 1607+
    private static readonly bool _hasGetDpiForWindow;
    private static readonly bool _hasGetDpiForSystem;
    private static readonly bool _hasGetSystemMetricsForDpi;
    // Win8.1+ (shcore.dll)
    private static readonly nint _shcoreLib;
    private static readonly nint _setProcessDpiAwarenessPtr;

    static Win32DpiApiResolver()
    {
        nint user32 = NativeLibrary.Load("user32.dll");
        _hasSetProcessDpiAwarenessContext = NativeLibrary.TryGetExport(user32, "SetProcessDpiAwarenessContext", out _);
        _hasGetDpiForWindow = NativeLibrary.TryGetExport(user32, "GetDpiForWindow", out _);
        _hasGetDpiForSystem = NativeLibrary.TryGetExport(user32, "GetDpiForSystem", out _);
        _hasGetSystemMetricsForDpi = NativeLibrary.TryGetExport(user32, "GetSystemMetricsForDpi", out _);

        // Probe shcore.dll for Win8.1 API (may not exist on Win7).
        if (NativeLibrary.TryLoad("shcore.dll", out _shcoreLib) && _shcoreLib != 0)
        {
            NativeLibrary.TryGetExport(_shcoreLib, "SetProcessDpiAwareness", out _setProcessDpiAwarenessPtr);
        }
    }

    /// <summary>
    /// Enables the best available DPI awareness for the process.
    /// Fallback chain: SetProcessDpiAwarenessContext (Win10 1703+)
    ///              → SetProcessDpiAwareness (Win8.1+)
    ///              → SetProcessDPIAware (Vista+)
    /// </summary>
    public static bool EnablePerMonitorDpiAwareness()
    {
        // Win10 1703+: Per-Monitor V2
        if (_hasSetProcessDpiAwarenessContext)
        {
            const nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
            if (User32.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
            {
                return true;
            }
        }

        // Win8.1+: Per-Monitor V1 via shcore.dll
        if (_setProcessDpiAwarenessPtr != 0)
        {
            const int PROCESS_PER_MONITOR_DPI_AWARE = 2;
            int hr = CallSetProcessDpiAwareness(PROCESS_PER_MONITOR_DPI_AWARE);
            if (hr == 0) // S_OK
            {
                return true;
            }
        }

        // Vista+: System DPI aware (no per-monitor support)
        return SetProcessDPIAware();
    }

    /// <summary>
    /// Gets the DPI for a specific window, falling back to system DPI via GetDeviceCaps.
    /// </summary>
    public static uint GetDpiForWindow(nint hwnd)
    {
        if (_hasGetDpiForWindow && hwnd != 0)
        {
            return User32.GetDpiForWindow(hwnd);
        }

        return GetSystemDpi();
    }

    /// <summary>
    /// Gets the system DPI, falling back to GetDeviceCaps.
    /// </summary>
    public static uint GetSystemDpi()
    {
        if (_hasGetDpiForSystem)
        {
            return User32.GetDpiForSystem();
        }

        // Legacy fallback: GetDeviceCaps on the screen DC.
        var hdc = User32.GetDC(0);
        if (hdc != 0)
        {
            uint dpi = (uint)GetDeviceCaps(hdc, LOGPIXELSX);
            User32.ReleaseDC(0, hdc);
            return dpi > 0 ? dpi : 96;
        }

        return 96;
    }

    /// <summary>
    /// Gets DPI-scaled system metrics, falling back to unscaled GetSystemMetrics.
    /// </summary>
    public static int GetSystemMetricsForDpi(int nIndex, uint dpi)
    {
        if (_hasGetSystemMetricsForDpi)
        {
            return User32.GetSystemMetricsForDpi(nIndex, dpi);
        }

        // Fallback: unscaled metrics (best effort on Win7/8).
        return User32.GetSystemMetrics(nIndex);
    }

    [LibraryImport("gdi32.dll")]
    private static partial int GetDeviceCaps(nint hdc, int index);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessDPIAware();

    private static unsafe int CallSetProcessDpiAwareness(int value)
    {
        // SetProcessDpiAwareness has signature: HRESULT __stdcall(PROCESS_DPI_AWARENESS)
        var fn = (delegate* unmanaged[Stdcall]<int, int>)_setProcessDpiAwarenessPtr;
        return fn(value);
    }
}
