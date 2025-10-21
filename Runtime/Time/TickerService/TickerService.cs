using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZServices.Time
{
    public class TickerService : MonoBehaviour, IService
    {
        private readonly Dictionary<float, Action> _tickerActionsDict = new();
        private readonly Dictionary<float, TimerTicker> _timersDict = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        /// <summary>
        /// Subscribe to a shared tick interval (e.g., 0.5f = every 0.5 seconds).
        /// Automatically creates and manages a TimerTicker if needed.
        /// </summary>
        public void Subscribe(float interval, Action callback)
        {
            if (!_tickerActionsDict.ContainsKey(interval))
            {
                _tickerActionsDict[interval] = null;
                TimerTicker ticker = TimerTicker.Create(
                    gameObject, 
                    interval,
                    onTick: () => _tickerActionsDict[interval]?.Invoke(),
                    name: $"TimerTicker_{interval}s");
                ticker.StartTimer();
                _timersDict[interval] = ticker;
            }

            _tickerActionsDict[interval] += callback;
        }

        /// <summary>
        /// Unsubscribe from a tick interval. 
        /// Automatically stops and cleans up if no subscribers remain.
        /// </summary>
        public void Unsubscribe(float interval, Action callback)
        {
            if (!_tickerActionsDict.ContainsKey(interval)) return;

            _tickerActionsDict[interval] -= callback;

            if (_tickerActionsDict[interval] == null)
            {
                _tickerActionsDict.Remove(interval);
                
                if (_timersDict.TryGetValue(interval, out TimerTicker ticker))
                {
                    _timersDict.Remove(interval);
                    ticker.StopTimer();
                    Destroy(ticker); // TODO test, I hope that really only destroys the component, not the GO
                }
            }
        }
    }
}
