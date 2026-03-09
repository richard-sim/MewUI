namespace Aprillz.MewUI.Styling;

/// <summary>
/// Container-level type-based style matching. Attached to a container element
/// (Panel, Window) to override default styles for descendant controls.
/// </summary>
public sealed class StyleScope
{
    // (Type targetType, Style? directStyle, string? styleName)
    // If directStyle is set, use it. If styleName is set, resolve from StyleSheet.
    private readonly List<(Type Type, Style? Style, string? StyleName)> _rules = new();

    /// <summary>
    /// Adds a direct style override for controls of type <typeparamref name="T"/>.
    /// </summary>
    public void Add<T>(Style style)
    {
        _rules.Add((typeof(T), style, null));
    }

    /// <summary>
    /// Adds a named style reference for controls of type <typeparamref name="T"/>.
    /// The name is resolved from <see cref="StyleSheet"/> at apply time.
    /// </summary>
    public void Add<T>(string styleName)
    {
        _rules.Add((typeof(T), null, styleName));
    }

    /// <summary>
    /// Gets the matching style or style name for the given control type.
    /// Checks exact type first, then base types.
    /// </summary>
    /// <returns>A tuple of (Style?, StyleName?). Both may be null if no match.</returns>
    public (Style? Style, string? StyleName) GetStyleOrName(Type controlType)
    {
        // Exact match first
        for (int i = _rules.Count - 1; i >= 0; i--)
        {
            if (_rules[i].Type == controlType)
                return (_rules[i].Style, _rules[i].StyleName);
        }

        // Base type match
        for (int i = _rules.Count - 1; i >= 0; i--)
        {
            if (_rules[i].Type.IsAssignableFrom(controlType))
                return (_rules[i].Style, _rules[i].StyleName);
        }

        return (null, null);
    }
}
