using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class PlayerChatPacket : IPacket
    {
        /// <summary>
        /// The connectionId of the joining player
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// The chat message from the player
        /// </summary>
        public string ChatMessage { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="connectionId"></param>
        public PlayerChatPacket(int connectionId, string message) : base(typeof(PlayerChatPacket))
        {
            this.ConnectionId = connectionId;
            this.ChatMessage = message;
        }
    }
}

