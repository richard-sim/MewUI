namespace Aprillz.MewUI.Platform.Linux.X11;

/// <summary>
/// Result of <see cref="IX11InputMethod.ProcessKeyEvent"/>.
/// </summary>
internal readonly record struct X11ImeProcessResult(
    bool Handled,
    bool ForwardKeyToApp,
    string? CommittedText);
