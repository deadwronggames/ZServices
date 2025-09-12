using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public abstract class EventChannel : ScriptableObject
    {
        private readonly List<EventListener> _eventListeners = new();
        
        public void RegisterListener(EventListener eventListener)
        {
            if (!_eventListeners.Contains(eventListener)) _eventListeners.Add(eventListener);
        }

        public void UnregisterListener(EventListener eventListener)
        {
            if (_eventListeners.Contains(eventListener)) _eventListeners.Remove(eventListener);
        }
        
        public void Invoke()
        {
            for (int i = _eventListeners.Count - 1; i >= 0; i--) { _eventListeners[i].OnEventRaised(); }
        }
    }
}