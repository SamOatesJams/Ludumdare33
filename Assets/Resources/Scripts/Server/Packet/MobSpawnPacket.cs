using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class MobSpawnPacket : IPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string MobType { get; private set; }

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
        public MobSpawnPacket(Realms.Common.Mob mob) : base(typeof(MobSpawnPacket))
        {
            this.ID = mob.ID;
            this.MobType = mob.MobType;
            this.PositionX = mob.transform.position.x;
            this.PositionY = mob.transform.position.y;
            this.PositionZ = mob.transform.position.z;
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

