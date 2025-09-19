using System.Collections.Generic;

namespace DeadWrongGames.ZServices.Time 
{
    public class TimerManager : UpdatedMonoBehaviour, IUpdatable
    {
        private readonly List<Timer> _timers = new();
        private readonly List<Timer> _sweep = new();
        
        public void RegisterTimer(Timer timer) => _timers.Add(timer);
        public void DeregisterTimer(Timer timer) => _timers.Remove(timer);

        public void OnUpdate() 
        {
            if (_timers.Count == 0) return;
            
            _sweep.Clear();
            _sweep.AddRange(_timers);
            foreach (Timer timer in _sweep)
                timer.Tick();
        }
        
        public void Clear() {
            _sweep.Clear();
            _sweep.AddRange(_timers);
            foreach (Timer timer in _sweep)
                timer.Dispose();
            
            _timers.Clear();
            _sweep.Clear();
        }
    }
}