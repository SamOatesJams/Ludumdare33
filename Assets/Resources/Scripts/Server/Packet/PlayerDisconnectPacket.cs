using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class PlayerDisconnectPacket : IPacket
    {
        /// <summary>
        /// The connectionId of the joining player
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="connectionId"></param>
        public PlayerDisconnectPacket(int connectionId) : base(typeof(PlayerDisconnectPacket))
        {
            this.ConnectionId = connectionId;
        }
    }
}

