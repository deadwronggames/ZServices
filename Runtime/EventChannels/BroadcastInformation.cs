using System;
using DeadWrongGames.ZCommon.Variables;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannels
{
    [Serializable]
    public struct BroadcastInformation
    {
        public EventChannel Channel;
        public Component Sender;
        public BaseReference Data;
    }
}