using System.Collections.Generic;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventService : MonoBehaviour
    {
        public interface IChannelMarker { }
        
        public const string RESOURCE_FOLDER_PATH = "Assets/Resources";
        public const string EVENT_CHANNEL_ASSET_FOLDER_NAME = "EventChannels";
        
        private readonly Dictionary<string, EventChannel> _eventChannelDict = new();

        protected void Awake()
        {
            foreach (Object obj in Resources.LoadAll(EVENT_CHANNEL_ASSET_FOLDER_NAME))
            {
                EventChannel eventChannel = obj as EventChannel;
                if (eventChannel == null) continue;

                if (!_eventChannelDict.ContainsKey(eventChannel.name)) _eventChannelDict.Add(eventChannel.name, eventChannel);
                else $"Duplicate EventChannel: {eventChannel.name}. Duplicate not added.".Log(level: ZMethodsDebug.LogLevel.Warning);
            }
        }

        public void Broadcast<TChannelMarker>() where TChannelMarker : IChannelMarker
        {
            _eventChannelDict[typeof(TChannelMarker).Name].Invoke();
        }
    }
}