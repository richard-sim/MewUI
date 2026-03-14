using System.Diagnostics.CodeAnalysis;

namespace Aprillz.MewUI.Platform;

/// <summary>
/// Represents drag-and-drop or clipboard data in a format-agnostic way.
/// </summary>
public interface IDataObject
{
    /// <summary>
    /// Gets the available format identifiers.
    /// </summary>
    IReadOnlyList<string> Formats { get; }

    /// <summary>
    /// Returns true when the data object contains the specified format.
    /// </summary>
    bool Contains(string format);

    /// <summary>
    /// Attempts to retrieve strongly typed data for the specified format.
    /// </summary>
    bool TryGetData<T>(string format, [NotNullWhen(true)]out T? value);

    /// <summary>
    /// Returns the raw data for the specified format, or null when not present.
    /// </summary>
    object? GetData(string format);
}

/// <summary>
/// Well-known cross-platform data format identifiers.
/// </summary>
public static class StandardDataFormats
{
    /// <summary>
    /// File system items represented as <see cref="IReadOnlyList{T}"/> of absolute paths.
    /// </summary>
    public const string StorageItems = nameof(StorageItems);

    /// <summary>
    /// Plain text represented as <see cref="string"/>.
    /// </summary>
    public const string Text = nameof(Text);
}

internal sealed class DataObject : IDataObject
{
    private readonly Dictionary<string, object> _data;

    public DataObject(Dictionary<string, object> data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        Formats = _data.Keys.ToArray();
    }

    public IReadOnlyList<string> Formats { get; }

    public bool Contains(string format)
        => !string.IsNullOrWhiteSpace(format) && _data.ContainsKey(format);

    public bool TryGetData<T>(string format, out T? value)
    {
        if (!string.IsNullOrWhiteSpace(format) &&
            _data.TryGetValue(format, out var raw) &&
            raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public object? GetData(string format)
        => !string.IsNullOrWhiteSpace(format) && _data.TryGetValue(format, out var raw) ? raw : null;
}
