namespace Aprillz.MewUI;

/// <summary>
/// Per-instance storage for <see cref="MewProperty{T}"/> values.
/// Manages local values, animation targets, and an animated-value overlay
/// that an external animation system can set without this class knowing about clocks or interpolation.
/// </summary>
internal sealed class PropertyValueStore
{
    private readonly WeakReference<IPropertyOwner> _ownerRef;
    private readonly Type _ownerType;
    private Entry[]? _entries;

    /// <summary>
    /// Callback invoked when a snap (<see cref="SetTarget"/>) overrides a property that has an animated value.
    /// The animation system registers this to stop the running clock for that property.
    /// </summary>
    internal Action<int>? StopAnimationCallback;

    /// <summary>
    /// Callback invoked when <see cref="Clear"/> is called, so the animation system can stop all clocks.
    /// </summary>
    internal Action? StopAllAnimationsCallback;

    public PropertyValueStore(IPropertyOwner owner)
    {
        _ownerRef = new WeakReference<IPropertyOwner>(owner);
        _ownerType = owner.GetType();
    }

    /// <summary>
    /// Gets the current (possibly interpolated) value of a property.
    /// Resolution: local value > animated value > target value > default.
    /// </summary>
    public T GetValue<T>(MewProperty<T> property)
    {
        ref var entry = ref GetEntry(property.Id);
        if (entry.IsEmpty)
            return property.GetDefaultForType(_ownerType);

        if (entry.HasLocal)
            return (T)entry.LocalValue!;

        if (entry.HasAnimated)
            return (T)entry.AnimatedValue!;

        if (entry.HasTarget)
            return (T)entry.TargetValue!;

        return property.GetDefaultForType(_ownerType);
    }

    /// <summary>
    /// Gets the current value as <see cref="object"/>. Used by non-generic resolution paths.
    /// </summary>
    public object GetBoxedValue(MewProperty property)
    {
        ref var entry = ref GetEntry(property.Id);
        if (entry.IsEmpty)
            return property.GetBoxedDefaultForType(_ownerType);

        if (entry.HasLocal)
            return entry.LocalValue!;

        if (entry.HasAnimated)
            return entry.AnimatedValue!;

        if (entry.HasTarget)
            return entry.TargetValue!;

        return property.GetBoxedDefaultForType(_ownerType);
    }

    /// <summary>
    /// Sets the local (user-defined) value. Highest priority in resolution.
    /// </summary>
    public void SetLocal<T>(MewProperty<T> property, T value)
    {
        ref var entry = ref EnsureEntry(property.Id);
        if (entry.HasLocal && EqualityComparer<T>.Default.Equals((T)entry.LocalValue!, value))
            return;
        var oldValue = CaptureOldEffective(ref entry, property);
        entry.LocalValue = value;
        entry.HasLocal = true;
        NotifyChanged(property, oldValue, value);
    }

    /// <summary>
    /// Sets the animation target for a property. Snaps immediately (no animation).
    /// Stops any running animation on this property via <see cref="StopAnimationCallback"/>.
    /// </summary>
    public void SetTarget(MewProperty property, object value)
    {
        ref var entry = ref EnsureEntry(property.Id);

        if (entry.HasTarget && !entry.HasAnimated && Equals(entry.TargetValue, value))
            return; // no change

        var oldValue = CaptureOldEffective(ref entry, property);

        // Stop any running animation for this property
        if (entry.HasAnimated)
        {
            StopAnimationCallback?.Invoke(property.Id);
            entry.HasAnimated = false;
            entry.AnimatedValue = null;
        }

        entry.TargetValue = value;
        entry.HasTarget = true;
        NotifyChanged(property, oldValue, value);
    }

    /// <summary>
    /// Typed SetTarget convenience.
    /// </summary>
    public void SetTarget<T>(MewProperty<T> property, T value)
    {
        SetTarget(property, (object)value!);
    }

    /// <summary>
    /// Sets the target value without stopping animations or notifying.
    /// Used by <see cref="Animation.PropertyAnimator"/> when updating the underlying target
    /// alongside a new animation (the animated overlay handles the visual update).
    /// </summary>
    internal void SetTargetDirect(MewProperty property, object value)
    {
        ref var entry = ref EnsureEntry(property.Id);
        entry.TargetValue = value;
        entry.HasTarget = true;
    }

    /// <summary>
    /// Sets the animated (interpolated) value for a property.
    /// Called by the animation system on each frame tick.
    /// </summary>
    internal void SetAnimatedValue(int propertyId, object value)
    {
        ref var entry = ref EnsureEntry(propertyId);
        var property = MewPropertyRegistry.GetProperty(propertyId);
        var oldValue = property != null ? CaptureOldEffective(ref entry, property) : null;
        entry.AnimatedValue = value;
        entry.HasAnimated = true;

        if (property != null)
            NotifyChanged(property, oldValue, value);
    }

    /// <summary>
    /// Clears the animated value for a property, letting the target value show through.
    /// Called by the animation system when an animation completes.
    /// </summary>
    internal void ClearAnimatedValue(int propertyId)
    {
        ref var entry = ref GetEntry(propertyId);
        if (!entry.HasAnimated)
            return;

        var property = MewPropertyRegistry.GetProperty(propertyId);
        var oldValue = property != null ? CaptureOldEffective(ref entry, property) : null;

        entry.HasAnimated = false;
        entry.AnimatedValue = null;

        if (property != null)
        {
            // After clearing animated, effective is now target or default
            var newValue = entry.HasLocal ? entry.LocalValue
                : entry.HasTarget ? entry.TargetValue
                : property.GetBoxedDefaultForType(_ownerType);
            NotifyChanged(property, oldValue, newValue);
        }
    }

    /// <summary>
    /// Returns true if a target value has been set for this property.
    /// </summary>
    internal bool HasTargetValue(int propertyId)
    {
        ref var entry = ref GetEntry(propertyId);
        return entry.HasTarget;
    }

    /// <summary>
    /// Gets the current visual value for a property (animated if running, otherwise target).
    /// Used by the animation system to capture the "from" value when starting a new animation.
    /// </summary>
    internal object? GetCurrentVisualValue(int propertyId)
    {
        ref var entry = ref GetEntry(propertyId);
        if (entry.HasAnimated)
            return entry.AnimatedValue;
        if (entry.HasTarget)
            return entry.TargetValue;
        return null;
    }

    /// <summary>
    /// Returns true if this store has any value (local, target, or animated) for the property.
    /// Used by inheritance resolution to determine when to stop walking the parent chain.
    /// </summary>
    public bool HasOwnValue(int propertyId)
    {
        ref var entry = ref GetEntry(propertyId);
        return !entry.IsEmpty;
    }

    /// <summary>
    /// Clears the local value for a property, allowing style/animation/default to take precedence.
    /// </summary>
    public void ClearLocal(MewProperty property)
    {
        ref var entry = ref GetEntry(property.Id);
        if (entry.IsEmpty || !entry.HasLocal)
            return;

        var oldValue = entry.LocalValue;
        entry.HasLocal = false;
        entry.LocalValue = null;

        // After clearing local, effective is now animated/target/default
        var newValue = entry.HasAnimated ? entry.AnimatedValue
            : entry.HasTarget ? entry.TargetValue
            : property.GetBoxedDefaultForType(_ownerType);
        NotifyChanged(property, oldValue, newValue);
    }

    /// <summary>
    /// Clears all stored values, stops animations, and releases references.
    /// </summary>
    public void Clear()
    {
        StopAllAnimationsCallback?.Invoke();

        if (_entries != null)
            Array.Clear(_entries);
    }

    private void NotifyChanged(MewProperty property, object? oldValue, object? newValue)
    {
        if (_ownerRef.TryGetTarget(out var owner))
            owner.OnPropertyChanged(property, oldValue, newValue);
    }

    /// <summary>
    /// Captures the current effective value before a mutation.
    /// Only performs work when the property has a <see cref="MewProperty.ChangedWithValuesCallback"/>.
    /// </summary>
    private object? CaptureOldEffective(ref Entry entry, MewProperty property)
    {
        if (property.ChangedWithValuesCallback == null)
            return null;

        if (entry.HasLocal) return entry.LocalValue;
        if (entry.HasAnimated) return entry.AnimatedValue;
        if (entry.HasTarget) return entry.TargetValue;
        return property.GetBoxedDefaultForType(_ownerType);
    }

    private ref Entry GetEntry(int id)
    {
        if (_entries == null || id >= _entries.Length)
            return ref Entry.Empty;

        return ref _entries[id];
    }

    private ref Entry EnsureEntry(int id)
    {
        if (_entries == null)
        {
            _entries = new Entry[Math.Max(id + 1, 8)];
        }
        else if (id >= _entries.Length)
        {
            Array.Resize(ref _entries, Math.Max(id + 1, _entries.Length * 2));
        }

        return ref _entries[id];
    }

    private struct Entry
    {
        public static Entry Empty;

        public object? LocalValue;
        public object? TargetValue;
        public object? AnimatedValue;
        public bool HasLocal;
        public bool HasTarget;
        public bool HasAnimated;

        public readonly bool IsEmpty => !HasLocal && !HasTarget;
    }
}

/// <summary>
/// Stores MewProperty references indexed by property Id for fast lookup.
/// </summary>
internal static class MewPropertyRegistry
{
    private static MewProperty?[] s_properties = new MewProperty?[16];
    private static readonly object s_lock = new();

    /// <summary>
    /// Registers a property reference. Called during <see cref="MewProperty{T}"/> construction.
    /// </summary>
    internal static void Register(MewProperty property)
    {
        lock (s_lock)
        {
            int id = property.Id;
            if (id >= s_properties.Length)
            {
                int newSize = Math.Max(id + 1, s_properties.Length * 2);
                Array.Resize(ref s_properties, newSize);
            }
            s_properties[id] = property;
        }
    }

    internal static MewProperty? GetProperty(int id)
    {
        return id < s_properties.Length ? s_properties[id] : null;
    }
}
