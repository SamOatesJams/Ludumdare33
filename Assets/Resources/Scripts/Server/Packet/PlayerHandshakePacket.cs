using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class PlayerHandshakePacket : IPacket
    {
        /// <summary>
        /// Is the connection to the server allowed
        /// </summary>
        public bool AllowConnection { get; private set; }

        /// <summary>
        /// If connection is refused, the reason why
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="allowConnection"></param>
        /// <param name="errorMessage"></param>
        public PlayerHandshakePacket(bool allowConnection, string errorMessage) : base(typeof(PlayerHandshakePacket))
        {
            this.AllowConnection = allowConnection;
            this.ErrorMessage = errorMessage;
        }
    }
}

