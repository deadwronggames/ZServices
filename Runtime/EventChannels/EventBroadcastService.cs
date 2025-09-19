using System.Collections.Generic;
using DeadWrongGames.ZCommon;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    public class EventBroadcastService : MonoBehaviour, IService
    {
        public abstract class ChannelMarker { }
        
        [SerializeField] AssetReferenceFolderSO _eventChannelFolderAssetReference;
        
        private readonly Dictionary<string, EventChannelSO> _eventChannelDict = new();

        private void Awake()
        {
            // find and cache all channels
            _eventChannelFolderAssetReference.LoadAssetsAsync<EventChannelSO>(asset => _eventChannelDict.Add(asset.name, asset))
                .Completed += _ => ServiceLocator.Register(this); // register service
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