using System.Collections.Generic;
using DeadWrongGames.ZConstants;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventBroadcastService : MonoBehaviour, IService
    {
        public abstract class ChannelMarker { }
        
        private readonly Dictionary<string, EventChannelSO> _eventChannelDict = new();

        protected void Awake()
        {
            // find and cache all channels
            foreach (Object obj in Resources.LoadAll(Constants.SERVICES_EVENT_CHANNEL_SO_FOLDER_NAME))
            {
                EventChannelSO eventChannel = obj as EventChannelSO;
                if (eventChannel == null) continue;

                if (!_eventChannelDict.ContainsKey(eventChannel.name)) _eventChannelDict.Add(eventChannel.name, eventChannel);
                else $"Duplicate EventChannel: {eventChannel.name}. Duplicate not added.".Log(level: ZMethodsDebug.LogLevel.Warning);
            }
            
            ServiceLocator.Register(this);
        }

        // API with different signatures that can be called by users
        public void Broadcast<TChannelMarker>() where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(null, null);
        public void Broadcast<TChannelMarker>(object data) where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(null, data);
        public void Broadcast<TChannelMarker>(Component sender) where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(sender, null);
        public void Broadcast<TChannelMarker>(Component sender, object data) where TChannelMarker : ChannelMarker
        {
            _eventChannelDict[typeof(TChannelMarker).Name].Invoke(sender, data);
        }
    }
}