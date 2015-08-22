using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class PlayerJoinPacket : IPacket
    {
        /// <summary>
        /// The username of the player who connected
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The connectionId of the joining player
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// The initial x position of the player
        /// </summary>
        public float PositionX { get; private set; }

        /// <summary>
        /// The initial y position of the player
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        /// The initial z position of the player
        /// </summary>
        public float PositionZ { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="username"></param>
        public PlayerJoinPacket(int connectionId, string username, Vector3 position) : base(typeof(PlayerJoinPacket))
        {
            this.Username = username;
            this.ConnectionId = connectionId;
            this.PositionX = position.x;
            this.PositionY = position.y;
            this.PositionZ = position.z;
        }

        /// <summary>
        /// Return the position as a unity vector3
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return new Vector3(this.PositionX, this.PositionY, this.PositionZ);
        }
    }
}

