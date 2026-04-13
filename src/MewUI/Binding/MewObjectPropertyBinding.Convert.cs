using Aprillz.MewUI.Controls;

namespace Aprillz.MewUI;

/// <summary>
/// Bridges a <see cref="MewProperty{TProp}"/> on a target <see cref="MewObject"/>
/// to a <see cref="MewProperty{TSource}"/> on a source <see cref="MewObject"/>
/// with type conversion via convert/convertBack functions.
/// </summary>
internal sealed class MewObjectPropertyBinding<TProp, TSource> : IDisposable
{
    private readonly MewObject _target;
    private readonly MewProperty<TProp> _targetProperty;
    private readonly MewObject _source;
    private readonly MewProperty<TSource> _sourceProperty;
    private readonly Func<TSource, TProp> _convert;
    private readonly Func<TProp, TSource>? _convertBack;
    private readonly BindingMode _mode;
    private readonly Action _onSourceChanged;
    private readonly Action? _onTargetChanged;
    private bool _updating;

    public MewObjectPropertyBinding(
        MewObject target,
        MewProperty<TProp> targetProperty,
        MewObject source,
        MewProperty<TSource> sourceProperty,
        Func<TSource, TProp> convert,
        Func<TProp, TSource>? convertBack,
        BindingMode mode)
    {
        _target = target;
        _targetProperty = targetProperty;
        _source = source;
        _sourceProperty = sourceProperty;
        _convert = convert;
        _convertBack = convertBack;
        _mode = mode;
        _onSourceChanged = OnSourceChanged;

        // Source → Target
        source.AddPropertyBindingCallback(sourceProperty.Id, _onSourceChanged);

        // Target → Source (TwoWay)
        if (mode == BindingMode.TwoWay && convertBack != null)
        {
            _onTargetChanged = OnTargetChanged;
            target.AddPropertyBindingCallback(targetProperty.Id, _onTargetChanged);
        }

        // Initial sync
        OnSourceChanged();
    }

    private void OnSourceChanged()
    {
        if (_updating) return;
        _updating = true;
        try
        {
            var sourceValue = _source.PropertyStore.GetValue(_sourceProperty);
            var converted = _convert(sourceValue);
            if (!EqualityComparer<TProp>.Default.Equals(
                    _target.PropertyStore.GetValue(_targetProperty), converted))
            {
                _target.PropertyStore.SetLocal(_targetProperty, converted);
            }
        }
        finally { _updating = false; }
    }

    private void OnTargetChanged()
    {
        if (_updating || _convertBack == null) return;
        _updating = true;
        try
        {
            var targetValue = _target.PropertyStore.GetValue(_targetProperty);
            var convertedBack = _convertBack(targetValue);
            if (!EqualityComparer<TSource>.Default.Equals(
                    _source.PropertyStore.GetValue(_sourceProperty), convertedBack))
            {
                _source.PropertyStore.SetLocal(_sourceProperty, convertedBack);
            }
        }
        finally { _updating = false; }
    }

    public void Dispose()
    {
        _source.RemovePropertyBindingCallback(_sourceProperty.Id, _onSourceChanged);
        if (_mode == BindingMode.TwoWay && _onTargetChanged != null)
        {
            _target.RemovePropertyBindingCallback(_targetProperty.Id, _onTargetChanged);
        }
    }
}
