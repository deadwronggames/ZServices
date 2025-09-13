using System.Collections.Generic;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventService : MonoBehaviour, IService
    {
        public interface IChannelMarker { }
        
        public const string RESOURCE_FOLDER_PATH = "Assets/Resources";
        public const string EVENT_CHANNEL_ASSET_FOLDER_NAME = "EventChannels";
        
        private readonly Dictionary<string, EventChannel> _eventChannelDict = new();

        protected void Awake()
        {
            // find and cache all channels
            foreach (Object obj in Resources.LoadAll(EVENT_CHANNEL_ASSET_FOLDER_NAME))
            {
                EventChannel eventChannel = obj as EventChannel;
                if (eventChannel == null) continue;

                if (!_eventChannelDict.ContainsKey(eventChannel.name)) _eventChannelDict.Add(eventChannel.name, eventChannel);
                else $"Duplicate EventChannel: {eventChannel.name}. Duplicate not added.".Log(level: ZMethodsDebug.LogLevel.Warning);
            }
            
            // register service
            ServiceLocator.Register(this);
        }

        // API with different signatures that can be called by users
        public void Broadcast<TChannelMarker>() where TChannelMarker : IChannelMarker => Broadcast<TChannelMarker>(null, null);
        public void Broadcast<TChannelMarker>(object data) where TChannelMarker : IChannelMarker => Broadcast<TChannelMarker>(null, data);
        public void Broadcast<TChannelMarker>(Component sender) where TChannelMarker : IChannelMarker => Broadcast<TChannelMarker>(sender, null);
        public void Broadcast<TChannelMarker>(Component sender, object data) where TChannelMarker : IChannelMarker
        {
            _eventChannelDict[typeof(TChannelMarker).Name].Invoke(sender, data);
        }
    }
}