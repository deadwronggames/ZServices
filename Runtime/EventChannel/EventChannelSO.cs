using System.Collections.Generic;
using System.Linq;
using DeadWrongGames.ZServices.Diagnostics;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>
    /// ScriptableObject representing a single event channel.
    /// Can register/unregister listeners and invoke events.
    /// </summary>
    /// <remarks>
    /// To create a new channel:
    /// 1. Create a new "ChannelMarker" class.
    /// 2. Create a new EventChannelSO via the top bar -> Create -> EventChannelSO.
    /// 3. Rename it exactly to match the marker class name.
    /// </remarks>
    public class EventChannelSO : ScriptableObject
    {
        [SerializeField] bool _verbose;
        
        private readonly HashSet<EventListener> _eventListeners = new();

        public void RegisterListener(EventListener eventListener)
        {
            _eventListeners.Add(eventListener);
        }

        public void UnregisterListener(EventListener eventListener)
        {
            _eventListeners.Remove(eventListener);
        }
        
        public void Invoke(                             ) => Invoke(null, null);
        public void Invoke(Component sender             ) => Invoke(sender, null);
        public void Invoke(object data                  ) => Invoke(null, data);
        public void Invoke(Component sender, object data)
        {
            if (_verbose)
            {
                string senderName = (sender != null) ? sender.name : "Unknown Sender";
                LogService.Debug(BuiltInLogCategories.ZSystems, $"<i>{senderName}</i> broadcasted on channel <i>{name}</i> with data {data}").Log();
            }
            
            // Copy to avoid modification during iteration
            foreach (EventListener listener in _eventListeners.ToArray())
                listener.OnEventRaised(sender, data);
        }
        
        // Just for debugging purposes
        public void PrintListeners()
        {
            string listenerNames =  (_eventListeners.Count == 0) ? "None" : string.Join(", ", _eventListeners.Select(listener => listener.ListenerName));
            $"Listeners for channel <i>{name}</i>: {listenerNames}".Print();
        }
    }
}