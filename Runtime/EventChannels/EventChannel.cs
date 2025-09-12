using System.Collections.Generic;
using System.Linq;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    // Instances can be created via top bar
    public class EventChannel : ScriptableObject
    {
        [SerializeField] bool _verbose;
        
        private readonly List<EventListener> _eventListeners = new();
        
        public void RegisterListener(EventListener eventListener)
        {
            if (!_eventListeners.Contains(eventListener)) _eventListeners.Add(eventListener);
        }

        public void UnregisterListener(EventListener eventListener)
        {
            if (_eventListeners.Contains(eventListener)) _eventListeners.Remove(eventListener);
        }
        
        public void Invoke(                             ) => Invoke(null, null);
        public void Invoke(Component sender             ) => Invoke(sender, null);
        public void Invoke(object data                  ) => Invoke(null, data);
        public void Invoke(Component sender, object data)
        {
            if (_verbose)
            {
                string senderName = (sender != null) ? sender.name : "Unknown Sender";
                $"<i>{senderName}</i> broadcasted on channel <i>{name}</i> with data {data}".Print();
            }
            
            for (int i = _eventListeners.Count - 1; i >= 0; i--) { _eventListeners[i].OnEventRaised(sender, data); }
        }
        
        // Just for debugging purposes
        public void PrintListeners()
        {
            string listenerNames =  (_eventListeners.Count == 0) ? "None" : string.Join(", ", _eventListeners.Select(listener => listener.ListenerName));
            $"Listeners for channel <i>{name}</i>: {listenerNames}".Print();
        }
    }
}