namespace Aprillz.MewUI;

/// <summary>
/// Lightweight abstraction over a binding source: get, set, subscribe, unsubscribe.
/// Allows binding to arbitrary getter/setter+event combinations without requiring <see cref="ObservableValue{T}"/>.
/// </summary>
public readonly struct BindingSource<T>
{
    public readonly Func<T> Get;
    public readonly Action<T>? Set;
    private readonly Action<Action> _subscribe;
    private readonly Action<Action> _unsubscribe;

    public BindingSource(
        Func<T> get,
        Action<T>? set,
        Action<Action> subscribe,
        Action<Action> unsubscribe)
    {
        Get = get;
        Set = set;
        _subscribe = subscribe;
        _unsubscribe = unsubscribe;
    }

    public void Subscribe(Action handler) => _subscribe(handler);
    public void Unsubscribe(Action handler) => _unsubscribe(handler);

    /// <summary>
    /// Implicit conversion from <see cref="ObservableValue{T}"/> for backward compatibility.
    /// </summary>
    public static implicit operator BindingSource<T>(ObservableValue<T> source) =>
        new(() => source.Value,
            v => source.Value = v,
            source.Subscribe,
            source.Unsubscribe);
}
