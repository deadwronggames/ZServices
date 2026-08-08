using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>
    /// A listener for a specific <see cref="EventChannelSO"/>.
    /// Handles the event by invoking a UnityEvent.
    /// </summary>
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