using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Client.Packet
{
    [Serializable]
    public class PlayerChatSendPacket : IPacket
    {
        /// <summary>
        /// The players chat message
        /// </summary>
        public string ChatMessage { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="chatMessage">The players chat message</param>
        public PlayerChatSendPacket(string chatMessage) : base(typeof(PlayerChatSendPacket))
        {
            this.ChatMessage = chatMessage;
        }
    }
}

