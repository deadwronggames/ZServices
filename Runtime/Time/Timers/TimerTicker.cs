using System;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.Time 
{
    /// <summary>
    /// Timer that ticks every X seconds. 
    /// </summary>
    public class TimerTicker : Timer<TimerTicker> 
    {
        private Action _onTick;
        private float _tickIntervalSeconds = -1f;
        
        
        /// <summary>
        /// Factory method for creating a TimerTicker
        /// </summary>
        public static TimerTicker Create(GameObject userGO, float tickIntervalSeconds, Action onTick, Action onTimerStart = null, Action onTimerStop = null, string name = nameof(TimerTicker))
        {
            TimerTicker timer = Timer<TimerTicker>.Create(userGO, initialTime: 0f, onStart: onTimerStart, onStop: onTimerStop, name: name);
            timer._tickIntervalSeconds = tickIntervalSeconds;
            timer._onTick = onTick;
            return timer;
        }

        private void Awake()
        {
            // Validate that ticker was created correctly
            if (_onTick == null || _tickIntervalSeconds < 0f)
                $"{nameof(TimerTicker)} {name} was created without a tick interval or tick action!".Log(level: ZMethodsDebug.LogLevel.Error);
        }

        protected override void Tick() 
        {
            if (CurrentTime >= _tickIntervalSeconds)
            {
                CurrentTime -= _tickIntervalSeconds;
                _onTick.Invoke();
            }
            else CurrentTime += UnityEngine.Time.deltaTime;
        }
        

        public override void ResetTimer()
        {
            CurrentTime = 0f;
        }
        
        public void ResetTimer(int newTickIntervalSeconds)
        {
            _tickIntervalSeconds = newTickIntervalSeconds;
            ResetTimer();
        }
    }
}