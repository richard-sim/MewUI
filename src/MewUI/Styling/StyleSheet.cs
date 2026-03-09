namespace Aprillz.MewUI.Styling;

/// <summary>
/// Named style registry. Attached to Application or Window
/// for application-wide or window-scoped named styles.
/// </summary>
public sealed class StyleSheet
{
    private readonly Dictionary<string, Style> _styles = new(StringComparer.Ordinal);

    /// <summary>
    /// Defines a named style.
    /// </summary>
    /// <param name="name">The style name (matched via <c>Control.StyleName</c>).</param>
    /// <param name="style">The style to register.</param>
    public void Define(string name, Style style)
    {
        _styles[name] = style;
    }

    /// <summary>
    /// Gets a named style, or <see langword="null"/> if not found.
    /// </summary>
    public Style? Get(string name)
    {
        return _styles.GetValueOrDefault(name);
    }
}
