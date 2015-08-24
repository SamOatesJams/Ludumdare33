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
        /// Spawn X position
        /// </summary>
        public float PositionX { get; private set; }

        /// <summary>
        /// Spawn Y position
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        /// Spawn Z position
        /// </summary>
        public float PositionZ { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="allowConnection"></param>
        /// <param name="errorMessage"></param>
        public PlayerHandshakePacket(bool allowConnection, string errorMessage, Vector3 spawnPosition) : base(typeof(PlayerHandshakePacket))
        {
            this.AllowConnection = allowConnection;
            this.ErrorMessage = errorMessage;
            this.PositionX = spawnPosition.x;
            this.PositionX = spawnPosition.y;
            this.PositionX = spawnPosition.z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return new Vector3(this.PositionX, this.PositionY, this.PositionZ);
        }
    }
}

