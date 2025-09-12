using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
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