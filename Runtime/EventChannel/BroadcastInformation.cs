using System;
using DeadWrongGames.ZCommon.Variables;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>Holds info needed to broadcast an event (channel, sender, optional data).</summary>
    [Serializable]
    public struct BroadcastInformation
    {
        public EventChannelSO Channel;
        public Component Sender;
        public BaseReference Data;
    }
}