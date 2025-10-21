using UnityEngine;

namespace DeadWrongGames.ZServices.Time 
{
    /// <summary>
    /// Timer that counts down from a specific value to zero.
    /// </summary>
    public class TimerCountdown : Timer<TimerCountdown>
    {
        public bool IsFinished => (CurrentTime <= 0f);
        public float Progress => (_initialTime > 0f) ? 
            Mathf.Clamp01(CurrentTime / _initialTime) : 
            1f;
        
        protected override void Tick() 
        {
            if (CurrentTime > 0f) CurrentTime -= UnityEngine.Time.deltaTime;
            else StopTimer();
        }
    }
}