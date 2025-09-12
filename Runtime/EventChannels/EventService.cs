using System;
using System.Collections.Generic;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventService : MonoBehaviour
    {
        private readonly Dictionary<Type, EventChannel> _eventChannelDict = new();

        protected void Awake()
        {
            foreach (UnityEngine.Object obj in Resources.LoadAll("EventChannels"))
            {
                EventChannel eventChannel = obj as EventChannel;
                if (eventChannel == null) continue;

                Type eventChannelType = obj.GetType();
                if (!_eventChannelDict.ContainsKey(eventChannelType)) _eventChannelDict.Add(eventChannelType, eventChannel);
                else $"Duplicate EventChannel type: {eventChannelType.Name}. Duplicate not added.".Log(level: ZMethodsDebug.LogLevel.Warning);
            }
        }

        public void Broadcast<TEventChannel>() where TEventChannel : EventChannel
        {
            _eventChannelDict[typeof(TEventChannel)].Invoke();
        }
    }
}