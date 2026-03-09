namespace Aprillz.MewUI;

/// <summary>
/// Metadata flags for <see cref="MewProperty{T}"/> registration.
/// Controls invalidation behavior and value inheritance.
/// </summary>
[Flags]
public enum MewPropertyOptions
{
    /// <summary>No special behavior.</summary>
    None = 0,

    /// <summary>Value changes trigger InvalidateVisual.</summary>
    AffectsRender = 1 << 0,

    /// <summary>Value changes trigger InvalidateLayout.</summary>
    AffectsLayout = 1 << 1,

    /// <summary>Value is inherited from parent elements when not set locally or by style.</summary>
    Inherits = 1 << 2,

    /// <summary>Bind() defaults to TwoWay mode instead of OneWay for this property.</summary>
    BindsTwoWayByDefault = 1 << 3,
}
