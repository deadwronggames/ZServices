using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeadWrongGames.ZServices.EventChannels
{

    [Serializable]
    public class EventListener
    {
        public string ListenerName { get; set; } // just for debugging purposes

        [Tooltip("EventChannel to register with.")]
        public EventChannelSO EventChannel;

        [Tooltip("Response to invoke when EventChannel is raised.")]
        [SerializeField] UnityEvent<Component, object> _response;
    
        public void OnEventRaised(Component sender, object data)
        {
            _response.Invoke(sender, data);
        }
    }
}