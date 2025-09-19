using System;
using DeadWrongGames.ZUtils;

namespace DeadWrongGames.ZServices.Time
{
    public abstract class Timer : IDisposable 
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }
        public abstract bool IsFinished { get; }

        protected float _initialTime;
        private readonly TimerManager _timerManager;

        private readonly Action _onTimerStart;
        private readonly Action _onTimerStop;

        protected Timer(float initialTime, Action onTimerStart = default, Action onTimerStop = default) 
        {
            _initialTime = initialTime;
            _onTimerStart = onTimerStart;
            _onTimerStop = onTimerStop;
                
            _timerManager = ServiceLocator.Get<TimerManager>();
        }

        public void Start() 
        {
            CurrentTime = _initialTime;
            if (!IsRunning) 
            {
                IsRunning = true;
                _timerManager.RegisterTimer(this);
                _onTimerStart.Invoke();
            }
        }

        public void Stop() {
            if (IsRunning) {
                IsRunning = false;
                _timerManager.DeregisterTimer(this);
                _onTimerStop.Invoke();
            }
        }

        public abstract void Tick();
        
        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public virtual void Reset() => CurrentTime = _initialTime;

        public virtual void Reset(float newTime) 
        {
            _initialTime = newTime;
            Reset();
        }

        ~Timer() 
        {
            Dispose(false);
        }

        // Call Dispose to ensure de-registration of the timer from the TimerManager
        // when the user is done with the timer or being destroyed
        private bool _disposed;
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) return;
            if (disposing) _timerManager.DeregisterTimer(this);

            _disposed = true;
        }
    }
}