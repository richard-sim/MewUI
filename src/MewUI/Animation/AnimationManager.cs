using System.Diagnostics;

namespace Aprillz.MewUI.Animation;

/// <summary>
/// Drives all active <see cref="AnimationClock"/> instances synchronized with the render loop.
/// When animations are active, switches to <see cref="RenderLoopMode.Continuous"/> so the
/// platform host renders every frame. Reverts to <see cref="RenderLoopMode.OnRequest"/> when idle.
/// </summary>
public sealed class AnimationManager
{
    private static AnimationManager? _instance;

    private readonly List<AnimationClock> _active = new();
    private readonly List<AnimationClock> _pendingAdd = new();
    private readonly List<AnimationClock> _pendingRemove = new();
    private bool _isUpdating;

    internal AnimationManager() { }

    /// <summary>
    /// Gets the singleton animation manager instance.
    /// </summary>
    internal static AnimationManager Instance => _instance ??= new AnimationManager();

    /// <summary>
    /// Gets the number of currently active animations.
    /// </summary>
    public int ActiveCount => _active.Count;

    internal void Register(AnimationClock clock)
    {
        if (_isUpdating)
        {
            _pendingAdd.Add(clock);
        }
        else
        {
            _active.Add(clock);
        }

        EnableContinuousMode();
    }

    internal void Unregister(AnimationClock clock)
    {
        if (_isUpdating)
        {
            _pendingRemove.Add(clock);
        }
        else
        {
            _active.Remove(clock);
        }

        if (!_isUpdating)
        {
            DisableContinuousModeIfIdle();
        }
    }

    /// <summary>
    /// Updates all active animation clocks. Called by the rendering pipeline
    /// before each frame (e.g. from <c>Window.RenderFrameCore</c>).
    /// </summary>
    public void Update()
    {
        if (_active.Count == 0 && _pendingAdd.Count == 0)
        {
            return;
        }

        long now = Stopwatch.GetTimestamp();

        _isUpdating = true;
        try
        {
            for (int i = 0; i < _active.Count; i++)
            {
                _active[i].Update(now);
            }
        }
        finally
        {
            _isUpdating = false;
        }

        // Apply deferred additions/removals
        if (_pendingAdd.Count > 0)
        {
            _active.AddRange(_pendingAdd);
            _pendingAdd.Clear();
        }

        if (_pendingRemove.Count > 0)
        {
            for (int i = 0; i < _pendingRemove.Count; i++)
            {
                _active.Remove(_pendingRemove[i]);
            }

            _pendingRemove.Clear();
        }

        DisableContinuousModeIfIdle();
    }

    private void EnableContinuousMode()
    {
        if (!Application.IsRunning)
        {
            return;
        }

        var settings = Application.Current.RenderLoopSettings;

        if (!settings.VSyncEnabled)
        {
            return;
        }

        settings.Mode = RenderLoopMode.Continuous;
    }

    private void DisableContinuousModeIfIdle()
    {
        if (_active.Count > 0 || _pendingAdd.Count > 0)
        {
            return;
        }

        if (!Application.IsRunning)
        {
            return;
        }
        var settings = Application.Current.RenderLoopSettings;
        if (settings.VSyncEnabled)
        {
            Application.Current.RenderLoopSettings.Mode = RenderLoopMode.OnRequest;
        }
    }

    /// <summary>
    /// Resets the singleton instance. For testing purposes only.
    /// </summary>
    internal static void Reset()
    {
        _instance?.DisableContinuousModeIfIdle();
        _instance?._active.Clear();
        _instance?._pendingAdd.Clear();
        _instance?._pendingRemove.Clear();
        _instance = null;
    }
}
