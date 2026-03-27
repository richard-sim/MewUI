namespace Aprillz.MewUI;

/// <summary>
/// Style registry supporting both named styles and type-based style rules.
/// Attach to any <see cref="Controls.FrameworkElement"/> (typically a Window) to provide
/// scoped styles for descendant controls.
/// </summary>
public sealed class StyleSheet
{
    private readonly Dictionary<string, Style> _namedStyles = new(StringComparer.Ordinal);
    private List<(Type Type, Style Style)>? _typeRules;

    /// <summary>
    /// Defines a named style.
    /// </summary>
    /// <param name="name">The style name (matched via <c>Control.StyleName</c>).</param>
    /// <param name="style">The style to register.</param>
    public void Define(string name, Style style)
    {
        _namedStyles[name] = style;
    }

    /// <summary>
    /// Defines a type-based style rule. All descendant controls of type <typeparamref name="T"/>
    /// (without an explicit <c>StyleName</c>) will receive this style.
    /// </summary>
    public void Define<T>(Style style)
    {
        _typeRules ??= new();
        _typeRules.Add((typeof(T), style));
    }

    /// <summary>
    /// Gets a named style, or <see langword="null"/> if not found.
    /// </summary>
    public Style? Get(string name)
    {
        return _namedStyles.GetValueOrDefault(name);
    }

    /// <summary>
    /// Gets the matching style for the given control type.
    /// Checks exact type first, then base types.
    /// </summary>
    public Style? GetByType(Type controlType)
    {
        if (_typeRules == null) return null;

        // Exact match first
        for (int i = _typeRules.Count - 1; i >= 0; i--)
        {
            if (_typeRules[i].Type == controlType)
                return _typeRules[i].Style;
        }

        // Base type match
        for (int i = _typeRules.Count - 1; i >= 0; i--)
        {
            if (_typeRules[i].Type.IsAssignableFrom(controlType))
                return _typeRules[i].Style;
        }

        return null;
    }
}
