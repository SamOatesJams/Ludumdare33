using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class MobDamagedPacket : IPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public int MobID { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int NewHealth { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        public MobDamagedPacket(int mobId, int newHealth) : base(typeof(MobDamagedPacket))
        {
            this.MobID = mobId;
            this.NewHealth = newHealth;
        }
    }
}

