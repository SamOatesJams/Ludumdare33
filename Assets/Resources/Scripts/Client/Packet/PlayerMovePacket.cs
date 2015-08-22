using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Client.Packet
{
    [Serializable]
    public class PlayerMovePacket : IPacket
    {
        /// <summary>
        /// The target x position of the player
        /// </summary>
        public float PositionX { get; private set; }

        /// <summary>
        /// The target y position of the player
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        /// The target z position of the player
        /// </summary>
        public float PositionZ { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="position">The target position of the player</param>
        public PlayerMovePacket(Vector3 position) : base(typeof(PlayerMovePacket))
        {
            this.PositionX = position.x;
            this.PositionY = position.y;
            this.PositionZ = position.z;
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
