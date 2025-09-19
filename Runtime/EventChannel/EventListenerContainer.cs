using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>
    /// Component container for multiple <see cref="EventListener"/>s on a GameObject.
    /// Registers/unregisters them automatically on enable/disable.
    /// </summary>
    public class EventListenerContainer : MonoBehaviour
    {
        [SerializeField] List<EventListener> _eventListeners;
        
        private void Awake()
        {
            // assign GO name to listeners, just so that it can be printed for debugging purposes
            foreach (EventListener eventListener in _eventListeners) eventListener.ListenerName = name;
        }
        
        private void OnEnable()
        {
            foreach (EventListener eventListener in _eventListeners)
            {
                eventListener.EventChannel.RegisterListener(eventListener);
            }
        }

        private void OnDisable()
        {
            foreach (EventListener eventListener in _eventListeners)
            {
                eventListener.EventChannel.UnregisterListener(eventListener);
            }
        }
    }
}