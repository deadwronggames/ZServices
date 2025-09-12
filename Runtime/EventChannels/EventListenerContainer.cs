using System.Collections.Generic;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventListenerContainer : MonoBehaviour
    {
        [SerializeField] List<EventListener> _eventListeners;
        
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