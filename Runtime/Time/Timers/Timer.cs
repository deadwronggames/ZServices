using System;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.Time;

/// Base class for all timers that automatically updates via <see cref="UpdateCallbackService"/> 
public abstract class Timer<TTimer> : MonoBehaviour, IUpdatable where TTimer : Timer<TTimer>
{
    public float CurrentTime { get; protected set; }
    public bool IsRunning
    {
        get => _isRunningBacking;
        private set {
            // Before setting value, register or unregister from update callback service
            (_updateCallbackService != null).Print();
            if (value && !_isRunningBacking) _updateCallbackService.Register(this);
            if (!value && _isRunningBacking) _updateCallbackService.Unregister(this);
            _isRunningBacking = value;
        }
    }
    private bool _isRunningBacking;

    protected float _initialTime;
    private Action _onTimerStart;
    private Action _onTimerStop;
    private UpdateCallbackService _updateCallbackService => ZMethods.LazyInitialization(ref _updateCallbackServiceBacking, ServiceLocator.Get<UpdateCallbackService>);
    private UpdateCallbackService _updateCallbackServiceBacking;

    /// <summary>
    /// Creates and returns a new Timer Component on a GameObject
    /// </summary>
    public static TTimer Create(GameObject userGO, float initialTime, Action onStart = null, Action onStop = null, string name = nameof(TTimer))
    {
        TTimer timer = userGO.AddComponent<TTimer>();
        timer._initialTime = initialTime;
        timer._onTimerStart = onStart;
        timer._onTimerStop = onStop;
            
        return timer;
    }

    private void OnDestroy()
    {
        if (_updateCallbackServiceBacking != null) _updateCallbackServiceBacking.Unregister(this);
    }
        
    public void OnUpdate() 
    {
        if (IsRunning) Tick();
    }
    protected abstract void Tick();

    public void StartTimer() 
    {
        CurrentTime = _initialTime;
        if (!IsRunning) 
        {
            IsRunning = true;
            _onTimerStart?.Invoke();
        }
    }

    public void StopTimer() {
        if (IsRunning) {
            IsRunning = false;
            _onTimerStop?.Invoke();
        }
    }
        
    public void ResumeTimer() => IsRunning = true;
    public void PauseTimer() => IsRunning = false;

    public virtual void ResetTimer() => CurrentTime = _initialTime;

    public virtual void ResetTimer(float newTime) 
    {
        _initialTime = newTime;
        ResetTimer();
    }
}