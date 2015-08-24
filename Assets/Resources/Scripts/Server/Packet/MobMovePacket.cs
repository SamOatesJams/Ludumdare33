using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class MobMovePacket : IPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public int MobID { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float PositionX { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float PositionZ { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        public MobMovePacket(int mobId, Vector3 target) : base(typeof(MobMovePacket))
        {
            this.MobID = mobId;
            this.PositionX = target.x;
            this.PositionY = target.y;
            this.PositionZ = target.z;
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

