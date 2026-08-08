namespace DeadWrongGames.ZServices.Time;

/// <summary>
/// Timer that counts up from zero to infinity. Great for measuring durations.
/// </summary>
public class TimerStopwatch : Timer<TimerStopwatch> 
{
    protected override void Tick() 
    {
        CurrentTime += UnityEngine.Time.deltaTime;
    }
}