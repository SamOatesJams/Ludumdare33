using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Client.Packet
{
    [Serializable]
    public class PlayerConnectPacket : IPacket
    {
        /// <summary>
        /// The name of the player
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="username">the name of the player</param>
        public PlayerConnectPacket(string username) : base(typeof(PlayerConnectPacket))
        {
            this.Username = username;
        }
    }
}

