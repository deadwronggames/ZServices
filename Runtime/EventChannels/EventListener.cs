using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeadWrongGames.ZServices.EventChannels
{

    [Serializable]
    public class EventListener
    {
        [Tooltip("EventChannel to register with.")]
        public EventChannel EventChannel;

        [Tooltip("Response to invoke when EventChannel is raised.")]
        [SerializeField] UnityEvent _response;
    
        public void OnEventRaised()
        {
            _response.Invoke();
        }
    }
}