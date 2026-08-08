using System;
using DeadWrongGames.ZCommon;
using UnityEngine;

namespace DeadWrongGames.ZServices.EventChannel
{
    /// <summary>Struct allowing event broadcast via inspector setup.</summary>
    [Serializable]
    public struct Broadcaster : IInvokable
    {
        [SerializeField] BroadcastInformation _broadcastInformation;

        public void Invoke() => Broadcast();
        public void Broadcast() => _broadcastInformation.Channel.Invoke(_broadcastInformation.Sender, _broadcastInformation.Data.ValueAsObject);
    }
}