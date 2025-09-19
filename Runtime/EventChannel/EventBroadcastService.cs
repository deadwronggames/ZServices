using System.Collections.Generic;
using DeadWrongGames.ZConstants;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>
    /// Central service responsible for broadcasting events to <see cref="EventChannelSO"/> instances.
    /// Channels are discovered automatically from the Resources folder and cached.
    /// </summary>
    public class EventBroadcastService : MonoBehaviour, IService
    {
        /// <summary>
        /// Marker base class for defining custom event channels.
        /// Create a new channel by subclassing this:
        /// <code>public class TestEvent : EventBroadcastService.ChannelMarker { }</code>
        /// </summary>
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

        // <summary>Broadcasts an event on the channel specified by marker type.</summary>
        public void Broadcast<TChannelMarker>() where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(null, null);
        /// <summary>Broadcasts an event with custom data.</summary>
        public void Broadcast<TChannelMarker>(object data) where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(null, data);
        /// <summary>Broadcasts an event passing the sender component along.</summary>
        public void Broadcast<TChannelMarker>(Component sender) where TChannelMarker : ChannelMarker => Broadcast<TChannelMarker>(sender, null);
        /// <summary>Broadcasts an event with both sender and data.</summary>
        public void Broadcast<TChannelMarker>(Component sender, object data) where TChannelMarker : ChannelMarker
        {
            _eventChannelDict[typeof(TChannelMarker).Name].Invoke(sender, data);
        }
    }
}