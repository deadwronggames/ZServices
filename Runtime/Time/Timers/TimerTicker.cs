using System;

namespace DeadWrongGames.ZServices.Time 
{
    /// <summary>
    /// Timer that ticks at a specific frequency. (N times per second)
    /// </summary>
    public class TimerTicker : Timer 
    {
        private readonly Action _onTick;
        
        private float _interval;

        public TimerTicker(int ticksPerSecond, Action onTick, Action onTimerStart = default, Action onTimerStop = default) : base(initialTime: 0f, onTimerStart, onTimerStop) 
        {
            CalculateTimeThreshold(ticksPerSecond);
            _onTick = onTick;
        }

        public override void Tick() 
        {
            if (!IsRunning) return;
            if (CurrentTime >= _interval)
            {
                CurrentTime -= _interval;
                _onTick.Invoke();
            }
            else CurrentTime += UnityEngine.Time.deltaTime;
        }

        public override bool IsFinished => !IsRunning;

        public override void Reset()
        {
            CurrentTime = 0;
        }
        
        public void Reset(int newTicksPerSecond) 
        {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }
        
        void CalculateTimeThreshold(int ticksPerSecond)
        {
            _interval = 1f / ticksPerSecond;
        }
    }
}