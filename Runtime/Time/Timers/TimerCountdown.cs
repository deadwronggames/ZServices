using System;
using UnityEngine;

namespace DeadWrongGames.ZServices.Time 
{
    /// <summary>
    /// Timer that counts down from a specific value to zero.
    /// </summary>
    public class TimerCountdown : Timer 
    {
        public float Progress => Mathf.Clamp(CurrentTime / _initialTime, 0, 1);
        
        public TimerCountdown(float initialTime, Action onTimerStart = default, Action onTimerStop = default) : base(initialTime, onTimerStart, onTimerStop) { }

        public override void Tick() 
        {
            if (!IsRunning) return;
            if (CurrentTime > 0) CurrentTime -= UnityEngine.Time.deltaTime;
            else Stop();
        }

        public override bool IsFinished => (CurrentTime <= 0);
    }
}