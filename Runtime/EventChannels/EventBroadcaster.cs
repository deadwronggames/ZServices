using System;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    [Serializable]
    public struct Broadcaster
    {
        [SerializeField] BroadcastInformation _broadcastInformation;

        public void Broadcast() => _broadcastInformation.Channel.Invoke(_broadcastInformation.Sender, _broadcastInformation.Data.ValueAsObject);
    }
}