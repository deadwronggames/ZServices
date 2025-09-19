using System;

namespace DeadWrongGames.ZServices.Time 
{
    /// <summary>
    /// Timer that counts up from zero to infinity.  Great for measuring durations.
    /// </summary>
    public class TimerStopwatch : Timer 
    {
        public TimerStopwatch(Action onTimerStart = default, Action onTimerStop = default) : base(initialTime: 0f, onTimerStart, onTimerStop) { }

        public override void Tick() 
        {
            if (!IsRunning) return;
            CurrentTime += UnityEngine.Time.deltaTime;
        }

        public override bool IsFinished => false;
    }
}