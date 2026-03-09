namespace Aprillz.MewUI.Styling;

/// <summary>
/// Base class for property value setters used in <see cref="Style"/> definitions.
/// </summary>
public abstract class SetterBase
{
    /// <summary>Gets the target property.</summary>
    public MewProperty Property { get; }

    /// <summary>Gets the boxed value to set.</summary>
    public object Value { get; }

    private protected SetterBase(MewProperty property, object value)
    {
        Property = property;
        Value = value;
    }
}

/// <summary>
/// Sets a property value on the control itself.
/// </summary>
public sealed class Setter : SetterBase
{
    private Setter(MewProperty property, object value) : base(property, value) { }

    /// <summary>
    /// Creates a type-safe setter.
    /// </summary>
    public static Setter Create<T>(MewProperty<T> property, T value)
        => new(property, value!);
}

/// <summary>
/// Sets a property value on a named child part (resolved via <c>RegisterPart</c>/<c>GetPart</c>).
/// </summary>
public sealed class TargetSetter : SetterBase
{
    /// <summary>Gets the name of the target part.</summary>
    public string TargetName { get; }

    private TargetSetter(string targetName, MewProperty property, object value) : base(property, value)
    {
        TargetName = targetName;
    }

    /// <summary>
    /// Creates a type-safe target setter.
    /// </summary>
    public static TargetSetter Create<T>(string targetName, MewProperty<T> property, T value)
        => new(targetName, property, value!);
}
